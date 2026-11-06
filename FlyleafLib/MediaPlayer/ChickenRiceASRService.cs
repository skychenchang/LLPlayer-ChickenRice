using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.Builders;
using static FlyleafLib.Logger;

namespace FlyleafLib.MediaPlayer;

#nullable enable

/// <summary>
/// Integration with TransWithAI/Faster-Whisper-TransWithAI-ChickenRice.
///
/// Unlike the other ASR services this is NOT a chunk/streaming engine. Upstream is a
/// whole-file batch CLI: it loads the model once, applies its own VAD-based smart
/// splitting and subtitle de-duplication, and writes an SRT. Calling it once per audio
/// chunk would reload a ~3 GB model for every chunk and destroy its timeline logic,
/// so LLPlayer runs it once for the whole local media file.
///
/// Progressive delivery: upstream logs every decoded segment at DEBUG level while it
/// is still processing ("[mm:ss.cc --> mm:ss.cc] text") and writes the final,
/// post-processed SRT when the process exits. LLPlayer consumes both sources:
///   * live DEBUG lines / an incrementally written SRT -> injected into the player
///     while playback is already running, paced by the playback position;
///   * the final SRT -> an atomic replacement once the run completes, so the
///     authoritative merged/de-duplicated result is what remains.
///
/// Upstream CLI contract (src/faster_whisper_transwithai_chickenrice/infer.py):
///   * all options must come BEFORE the positional paths (argparse.REMAINDER)
///   * --overwrite is required (existing <stem>.srt is otherwise skipped)
///   * output is <output_dir>/<stem>.srt for a single input file
///   * the process chdir()s to its own directory, so WorkingDirectory must be set
/// </summary>
public static class ChickenRiceASRService
{
    private static readonly LogHandler Log = new(("[#1]").PadRight(8, ' ') + " [ChickenRice  ] ");

    private const int ErrorTailChars = 2000;
    private const int MaxCapturedOutputChars = 200_000;
    private const int PumpIntervalMs = 40;
    private const int SrtPollIntervalMs = 200;

    /// <summary>
    /// Upstream emits one line per decoded segment at DEBUG level, both in the
    /// standard and in the smart-VAD path. Timestamps are absolute media time.
    /// </summary>
    private static readonly Regex LiveSegmentReg = new(
        @"^\s*\[(?<start>\d+:\d{2}\.\d{2})\s+-->\s+(?<end>\d+:\d{2}\.\d{2})\]\s?(?<text>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Logging must never break ASR. FlyleafLib's Logger static initializer hooks
    /// WPF Application.Current.Exit, which is unavailable outside the player host
    /// (unit tests, tooling). Guard it so engine runs are host-independent.
    /// </summary>
    private static void Debug(string message)
    {
        try
        {
            if (CanDebug)
                Log.Debug(message);
        }
        catch
        {
            // best-effort diagnostics only
        }
    }

    // Same media suffixes as the upstream .bat launchers.
    private static readonly string[] DefaultSuffixes =
    [
        "mp3", "wav", "flac", "m4a", "aac", "ogg", "wma",
        "mp4", "mkv", "avi", "mov", "webm", "flv", "wmv", "ts", "m2ts", "mpg", "mpeg"
    ];

    /// <summary>
    /// Verifies everything required before starting a lengthy whole-file run.
    /// Returns a user-facing error instead of throwing so it can be surfaced as a
    /// configuration error (and never as an unhandled player crash).
    /// </summary>
    public static (bool ok, string error) Validate(ChickenRiceConfig config)
    {
        if (config == null)
            return (false, "ChickenRice configuration is missing.");

        string engine = config.EnginePath;
        if (string.IsNullOrWhiteSpace(engine) || !File.Exists(engine))
        {
            return (false,
                $"ChickenRice engine was not found at '{engine}'. " +
                $"Extract the upstream release package into '{ChickenRiceConfig.DefaultEngineDirectory}' " +
                "or set a manual engine path in Subtitles > ASR.");
        }

        string modelDir = config.ResolvedModelDirectory;
        if (!Directory.Exists(modelDir))
            return (false, $"ChickenRice model directory '{modelDir}' does not exist.");

        if (!File.Exists(Path.Combine(modelDir, "config.json")) ||
            (!File.Exists(Path.Combine(modelDir, "model.bin")) &&
             !File.Exists(Path.Combine(modelDir, "model.safetensors"))))
        {
            return (false,
                $"ChickenRice CTranslate2 model files were not found in '{modelDir}' " +
                "(expected config.json and model.bin / model.safetensors).");
        }

        if (config.Task is not ("translate" or "transcribe"))
            return (false, "ChickenRice task must be 'translate' or 'transcribe'.");

        return (true, "");
    }

    /// <summary>ChickenRice needs a local file path; online/yt-dlp streams are not supported.</summary>
    public static bool TryResolveLocalFile(string url, out string path, out string error)
    {
        path = "";
        error = "";

        if (string.IsNullOrWhiteSpace(url))
        {
            error = "No media file is open.";
            return false;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && !uri.IsFile)
        {
            error = "ChickenRice processes a whole local media file and cannot be used with online/live streams. " +
                    "Play a local file or switch the ASR engine to faster-whisper.";
            return false;
        }

        string candidate = uri is { IsFile: true } ? uri.LocalPath : url;
        if (!File.Exists(candidate))
        {
            error = $"Media file '{candidate}' was not found. ChickenRice requires a local media file.";
            return false;
        }

        path = Path.GetFullPath(candidate);
        return true;
    }

    /// <summary>Builds the upstream-compatible argument string (options first, media path last).</summary>
    public static string BuildArguments(ChickenRiceConfig config, string mediaPath, string outputDir)
    {
        var suffixes = DefaultSuffixes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        string ext = Path.GetExtension(mediaPath).TrimStart('.');
        if (!string.IsNullOrEmpty(ext))
            suffixes.Add(ext);

        ArgumentsBuilder args = new();
        args.Add("--audio_suffixes").Add(string.Join(",", suffixes));
        args.Add("--sub_formats").Add("srt");
        args.Add("--output_dir").Add(outputDir);
        args.Add("--overwrite");
        args.Add("--model_name_or_path").Add(config.ResolvedModelDirectory);
        args.Add("--device").Add(string.IsNullOrWhiteSpace(config.Device) ? "cuda" : config.Device);
        args.Add("--compute_type").Add(string.IsNullOrWhiteSpace(config.ComputeType) ? "auto" : config.ComputeType);
        args.Add("--task").Add(config.Task);
        // Needed for progressive delivery: upstream logs each decoded segment at
        // DEBUG level while processing. It is upstream's default; set it explicitly
        // so a user-supplied --log_level in ExtraArguments cannot silently disable it.
        args.Add("--log_level").Add("DEBUG");

        if (File.Exists(config.GenerationConfigPath))
            args.Add("--generation_config").Add(config.GenerationConfigPath);

        string result = args.Build();

        if (!string.IsNullOrWhiteSpace(config.ExtraArguments))
            result += " " + config.ExtraArguments.Trim();

        // Positional argument MUST be last (argparse.REMAINDER swallows everything after it).
        ArgumentsBuilder positional = new();
        positional.Add(mediaPath);
        result += " " + positional.Build();

        return result;
    }

    /// <summary>Command line for the "Copy Debug Command" button in settings.</summary>
    public static string BuildDebugCommand(ChickenRiceConfig config, string? mediaPath = null)
    {
        string sampleMedia = mediaPath ?? @"C:\media\episode01.mp4";
        string outputDir = Path.Combine(Path.GetTempPath(), "LLPlayer-ChickenRice-debug");
        var (exe, arguments) = NormalizeLaunch(config.EnginePath, BuildArguments(config, sampleMedia, outputDir));
        return $"\"{exe}\" {arguments}";
    }

    private static (string executable, string arguments) NormalizeLaunch(string enginePath, string arguments)
    {
        string ext = Path.GetExtension(enginePath).ToLowerInvariant();
        if (ext is ".bat" or ".cmd")
        {
            // cmd.exe needs the whole command wrapped in an extra pair of quotes.
            return ("cmd.exe", $"/c \"\"{enginePath}\" {arguments}\"");
        }

        return (enginePath, arguments);
    }

    /// <summary>
    /// Whole-file run that returns the final SRT only. Kept as the simple/fallback
    /// API (and for the legacy non-progressive mode); it does not stream anything.
    /// </summary>
    public static IReadOnlyList<SubtitleASRData> Generate(
        ChickenRiceConfig config,
        string mediaPath,
        TimeSpan startTime,
        CancellationToken token)
        => GenerateProgressive(config, mediaPath, startTime, token, static _ => { });

    /// <summary>
    /// Runs the upstream tool once for the whole media file and delivers subtitles
    /// progressively. Playback is never blocked: <paramref name="onSubtitle"/> is
    /// called as soon as the engine produces a segment, <paramref name="onPump"/> is
    /// called regularly so the caller can release buffered subtitles according to the
    /// current playback position, and <paramref name="onComplete"/> receives the
    /// authoritative post-processed SRT when the process exits.
    ///
    /// Any failure (missing dependency, non-zero exit, timeout, cancellation, no SRT)
    /// is converted into a clear exception; the temporary output directory is always
    /// removed so a failed run never leaves state behind.
    /// </summary>
    public static IReadOnlyList<SubtitleASRData> GenerateProgressive(
        ChickenRiceConfig config,
        string mediaPath,
        TimeSpan startTime,
        CancellationToken token,
        Action<SubtitleASRData> onSubtitle,
        Action? onPump = null,
        Action<IReadOnlyList<SubtitleASRData>>? onComplete = null)
    {
        ArgumentNullException.ThrowIfNull(onSubtitle);

        var (ok, error) = Validate(config);
        if (!ok)
            throw new InvalidOperationException(error);

        if (!TryResolveLocalFile(mediaPath, out string localPath, out string fileError))
            throw new InvalidOperationException(fileError);

        token.ThrowIfCancellationRequested();

        string outputDir = Path.Combine(Path.GetTempPath(), "LLPlayer-ChickenRice-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDir);

        try
        {
            string language = config.OutputLanguage;
            string arguments = BuildArguments(config, localPath, outputDir);
            var (executable, finalArguments) = NormalizeLaunch(config.EnginePath, arguments);

            ProcessStartInfo psi = new()
            {
                FileName = executable,
                Arguments = finalArguments,
                WorkingDirectory = config.EngineDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            // Upstream reconfigures stdout to UTF-8 and uses OpenMP + ONNX Runtime.
            psi.Environment["PYTHONIOENCODING"] = "utf-8";
            psi.Environment["KMP_DUPLICATE_LIB_OK"] = "TRUE";

            Debug($"Running: \"{executable}\" {finalArguments} (cwd: {psi.WorkingDirectory})");

            ConcurrentQueue<SubtitleASRData> liveQueue = new();
            List<SubtitleASRData> seen = [];
            StringBuilder stdOut = new();
            StringBuilder stdErr = new();
            object outputLock = new();

            using Process process = new() { StartInfo = psi };
            if (!process.Start())
                throw new InvalidOperationException($"Failed to start ChickenRice engine '{config.EnginePath}'.");

            if (config.ProcessPriority != ProcessPriorityClass.Normal)
            {
                try { process.PriorityClass = config.ProcessPriority; }
                catch { /* priority is best-effort only */ }
            }

            // Read both pipes line-by-line (upstream logs segment lines at DEBUG on
            // stderr) instead of waiting for the process to exit.
            Task stdOutTask = ReadStream(process.StandardOutput, stdOut, outputLock, liveQueue, language);
            Task stdErrTask = ReadStream(process.StandardError, stdErr, outputLock, liveQueue, language);

            using CancellationTokenRegistration _ = token.Register(() => SafeKill(process));

            int timeoutMs = config.TimeoutMinutes > 0
                ? checked(config.TimeoutMinutes * 60 * 1000)
                : Timeout.Infinite;

            SrtTailReader tail = new(outputDir, language);
            Stopwatch sw = Stopwatch.StartNew();
            DateTime lastSrtPoll = DateTime.MinValue;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                DrainLive(liveQueue, seen, onSubtitle);

                if ((DateTime.UtcNow - lastSrtPoll).TotalMilliseconds >= SrtPollIntervalMs)
                {
                    lastSrtPoll = DateTime.UtcNow;
                    DrainNew(tail.Poll(), seen, onSubtitle);
                }

                // Let the caller release buffered subtitles according to CurTime.
                onPump?.Invoke();

                if (process.WaitForExit(PumpIntervalMs))
                    break;

                if (timeoutMs != Timeout.Infinite && sw.ElapsedMilliseconds > timeoutMs)
                {
                    SafeKill(process);
                    process.WaitForExit(5000);
                    throw new TimeoutException(
                        $"ChickenRice did not finish within {config.TimeoutMinutes} minute(s). " +
                        "Increase the timeout in Subtitles > ASR, or use a smaller file / GPU device.");
                }
            }

            process.WaitForExit();
            try { Task.WaitAll([stdOutTask, stdErrTask], 5000); }
            catch { /* readers swallow stream errors */ }

            DrainLive(liveQueue, seen, onSubtitle);
            DrainNew(tail.Poll(), seen, onSubtitle);
            onPump?.Invoke();

            string stdOutText = stdOut.ToString();
            string stdErrText = stdErr.ToString();

            Debug($"Exit code: {process.ExitCode}");
            Debug($"stdout: {Tail(stdOutText)}");
            if (!string.IsNullOrWhiteSpace(stdErrText))
                Debug($"stderr: {Tail(stdErrText)}");

            if (token.IsCancellationRequested)
                throw new OperationCanceledException(token);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"ChickenRice exited with code {process.ExitCode}. {Tail(stdErrText, emptyFallback: stdOutText)}");
            }

            string srtPath = FindSrt(outputDir, localPath, stdErrText, stdOutText);
            string srtText = File.ReadAllText(srtPath, new UTF8Encoding(false));

            IReadOnlyList<SubtitleASRData> subtitles = ParseSrt(srtText, language);
            if (subtitles.Count == 0)
                throw new InvalidOperationException("ChickenRice produced an empty SRT file.");

            Debug($"Loaded {subtitles.Count} subtitles (start offset {startTime})");

            // Whole-file subtitles use absolute media timestamps already; no chunk offset needed.
            onComplete?.Invoke(subtitles);
            return subtitles;
        }
        finally
        {
            try { Directory.Delete(outputDir, recursive: true); }
            catch { /* best effort cleanup */ }
        }
    }

    private static async Task ReadStream(
        StreamReader reader,
        StringBuilder output,
        object outputLock,
        ConcurrentQueue<SubtitleASRData> liveQueue,
        string language)
    {
        try
        {
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                lock (outputLock)
                {
                    AppendCapped(output, line);
                }

                if (TryParseLiveSegment(line, language, out SubtitleASRData? data) && data != null)
                    liveQueue.Enqueue(data);
            }
        }
        catch
        {
            // Stream can be closed by an explicit kill / process exit.
        }
    }

    private static void AppendCapped(StringBuilder output, string line)
    {
        output.AppendLine(line);
        if (output.Length > MaxCapturedOutputChars)
            output.Remove(0, output.Length - MaxCapturedOutputChars / 2);
    }

    private static void DrainLive(
        ConcurrentQueue<SubtitleASRData> queue,
        List<SubtitleASRData> seen,
        Action<SubtitleASRData> onSubtitle)
    {
        while (queue.TryDequeue(out SubtitleASRData? data))
            DrainNew([data], seen, onSubtitle);
    }

    private static void DrainNew(
        IEnumerable<SubtitleASRData> incoming,
        List<SubtitleASRData> seen,
        Action<SubtitleASRData> onSubtitle)
    {
        foreach (SubtitleASRData data in incoming)
        {
            if (IsDuplicate(seen, data))
                continue;

            seen.Add(data);
            onSubtitle(data);
        }
    }

    /// <summary>
    /// A DEBUG line and the final/partial SRT can describe the same segment with
    /// slightly different timestamp precision. Only the recent window is checked,
    /// because upstream produces segments in chronological order.
    /// </summary>
    private static bool IsDuplicate(List<SubtitleASRData> seen, SubtitleASRData data)
    {
        int from = Math.Max(0, seen.Count - 200);
        for (int i = seen.Count - 1; i >= from; i--)
        {
            if (Math.Abs((seen[i].StartTime - data.StartTime).TotalMilliseconds) <= 150 &&
                string.Equals(seen[i].Text, data.Text, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reads newly appended, complete SRT blocks from the output file while the
    /// engine is still running. Supports upstream builds (and verification engines)
    /// that flush subtitles incrementally instead of writing the file in one shot.
    /// </summary>
    private sealed class SrtTailReader
    {
        private readonly string _outputDir;
        private readonly string _language;
        private long _offset;
        private string _carry = "";

        public SrtTailReader(string outputDir, string language)
        {
            _outputDir = outputDir;
            _language = language;
        }

        public IReadOnlyList<SubtitleASRData> Poll()
        {
            string[] files;
            try { files = Directory.GetFiles(_outputDir, "*.srt", SearchOption.AllDirectories); }
            catch { return []; }

            if (files.Length == 0)
                return [];

            try
            {
                using FileStream fs = new(files[0], FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                if (fs.Length <= _offset)
                    return [];

                fs.Position = _offset;
                using StreamReader reader = new(fs, new UTF8Encoding(false), true);
                string chunk = reader.ReadToEnd();
                _offset = fs.Position;

                _carry = (_carry + chunk)
                    .TrimStart('\uFEFF')
                    .Replace("\r\n", "\n")
                    .Replace('\r', '\n');
            }
            catch
            {
                return [];
            }

            int lastSeparator = _carry.LastIndexOf("\n\n", StringComparison.Ordinal);
            if (lastSeparator < 0)
                return [];

            string complete = _carry[..lastSeparator];
            _carry = _carry[(lastSeparator + 2)..];

            return ParseSrt(complete + "\n\n", _language);
        }
    }

    /// <summary>Parses one upstream DEBUG segment line, e.g. "[00:03.00 --> 00:06.50] 中文字幕".</summary>
    public static bool TryParseLiveSegment(string? line, string language, out SubtitleASRData? data)
    {
        data = null;
        if (string.IsNullOrEmpty(line))
            return false;

        Match match = LiveSegmentReg.Match(line);
        if (!match.Success)
            return false;

        if (!TryParseLrcTimestamp(match.Groups["start"].Value, out TimeSpan start) ||
            !TryParseLrcTimestamp(match.Groups["end"].Value, out TimeSpan end))
            return false;

        string text = match.Groups["text"].Value.Trim();
        if (text.Length == 0)
            return false;

        if (end <= start)
            end = start + TimeSpan.FromMilliseconds(20);

        data = MakeSub(text, start, end, language);
        return true;
    }

    private static bool TryParseLrcTimestamp(string value, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        string[] parts = value.Split(':');
        if (parts.Length != 2)
            return false;

        string[] secondsPart = parts[1].Split('.');
        if (secondsPart.Length != 2)
            return false;

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes) ||
            !int.TryParse(secondsPart[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seconds) ||
            !int.TryParse(secondsPart[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int centiseconds))
            return false;

        result = TimeSpan.FromMinutes(minutes) +
                 TimeSpan.FromSeconds(seconds) +
                 TimeSpan.FromMilliseconds(centiseconds * 10L);
        return true;
    }

    private static SubtitleASRData MakeSub(string text, TimeSpan start, TimeSpan end, string language) => new()
    {
        Text = text,
        StartTime = start,
        EndTime = end,
        Language = language,
#if DEBUG
        ChunkNo = 0,
        StartTimeChunk = TimeSpan.Zero,
        EndTimeChunk = end,
#endif
    };

    private static string FindSrt(string outputDir, string mediaPath, string stdErr, string stdOut)
    {
        string expected = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(mediaPath) + ".srt");
        if (File.Exists(expected))
            return expected;

        // Be tolerant of upstream path-rebuild differences between releases.
        string[] found = Directory.GetFiles(outputDir, "*.srt", SearchOption.AllDirectories);
        if (found.Length > 0)
            return found[0];

        throw new InvalidOperationException(
            $"ChickenRice did not produce an SRT file. {Tail(stdErr, emptyFallback: stdOut)}");
    }

    private static void SafeKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch { /* process may already be gone */ }
    }

    private static string Tail(string text, string? emptyFallback = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            text = emptyFallback ?? "";
        if (string.IsNullOrEmpty(text))
            return "(no engine output)";

        text = text.Trim();
        return text.Length <= ErrorTailChars ? text : "... " + text[^ErrorTailChars..];
    }

    /// <summary>Parses a standard UTF-8 SRT into LLPlayer ASR subtitle data.</summary>
    public static IReadOnlyList<SubtitleASRData> ParseSrt(string text, string language)
    {
        List<SubtitleASRData> result = [];

        if (string.IsNullOrWhiteSpace(text))
            return result;

        string[] lines = text
            .TrimStart('\uFEFF')
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n');

        int i = 0;
        while (i < lines.Length)
        {
            while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
                i++;
            if (i >= lines.Length)
                break;

            // Optional numeric index line.
            if (int.TryParse(lines[i].Trim(), out _))
                i++;
            if (i >= lines.Length)
                break;

            string timeLine = lines[i].Trim();
            int arrow = timeLine.IndexOf("-->", StringComparison.Ordinal);
            if (arrow < 0)
            {
                i++;
                continue;
            }

            string startText = timeLine[..arrow].Trim();
            string endText = timeLine[(arrow + 3)..].Trim();

            // SRT timing lines may carry trailing position coordinates.
            int space = endText.IndexOf(' ');
            if (space > 0)
                endText = endText[..space];

            if (!TryParseTimestamp(startText, out TimeSpan start) ||
                !TryParseTimestamp(endText, out TimeSpan end))
            {
                i++;
                continue;
            }

            i++;
            StringBuilder sb = new();
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(lines[i].Trim());
                i++;
            }

            string subtitleText = sb.ToString();
            if (subtitleText.Length == 0)
                continue;

            if (end <= start)
                end = start + TimeSpan.FromMilliseconds(20);

            result.Add(MakeSub(subtitleText, start, end, language));
        }

        return result;
    }

    private static bool TryParseTimestamp(string value, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string[] parts = value.Replace(',', '.').Split(':');
        if (parts.Length is < 2 or > 3)
            return false;

        try
        {
            int hours = 0;
            int minutes;
            string secondsPart;

            if (parts.Length == 3)
            {
                hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
                minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
                secondsPart = parts[2];
            }
            else
            {
                minutes = int.Parse(parts[0], CultureInfo.InvariantCulture);
                secondsPart = parts[1];
            }

            double seconds = double.Parse(secondsPart, NumberStyles.Float, CultureInfo.InvariantCulture);
            result = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            return result >= TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }
}
