using System.Diagnostics;
using System.Text.Json;
using FlyleafLib.MediaPlayer;

namespace FlyleafLib;

/// <summary>
/// Integration tests for the ChickenRice (JA -> ZH) ASR engine.
/// The heavy upstream tool is replaced by a small CLI-compatible stand-in so the
/// whole LLPlayer-side chain (config -> validation -> process -> SRT -> subtitles)
/// can be verified without a 3 GB model / CUDA runtime.
/// </summary>
public class ChickenRiceIntegrationTests
{
    private static string VerifyRoot =>
        Environment.GetEnvironmentVariable("LLPLAYER_CR_VERIFY")
        ?? @"D:\codex\wbuddy\_integration\verify";

    private static string FakeEngineDir => Path.Combine(VerifyRoot, "fake_engine");
    private static string FakeBat => Path.Combine(FakeEngineDir, "infer.bat");
    private static string MediaPath => Path.Combine(VerifyRoot, "test.mp4");

    private static ChickenRiceConfig ReadyConfig() => new()
    {
        UseManualEngine = true,
        ManualEnginePath = FakeBat,
        ModelDirectory = "models",
        Device = "cpu",
        ComputeType = "auto",
        Task = "translate",
        TimeoutMinutes = 0,
    };

    private static void WithMode(string mode, Action action)
    {
        RequireFakeEngine();
        string? previous = Environment.GetEnvironmentVariable("FAKE_CR_MODE");
        Environment.SetEnvironmentVariable("FAKE_CR_MODE", mode);
        try { action(); }
        finally
        {
            if (previous == null)
                Environment.SetEnvironmentVariable("FAKE_CR_MODE", null);
            else
                Environment.SetEnvironmentVariable("FAKE_CR_MODE", previous);
        }
    }

    private static int TempDirCount() =>
        Directory.GetDirectories(Path.GetTempPath(), "LLPlayer-ChickenRice-*").Length;

    /// <summary>
    /// Process-level tests need the CLI-compatible stand-in and a media file. They live
    /// outside the source tree (integration workspace), so skip cleanly when absent
    /// instead of making a plain `dotnet test` fail on a fresh checkout.
    /// </summary>
    private static void RequireFakeEngine()
    {
        Assert.SkipUnless(File.Exists(FakeBat), "ChickenRice verification engine stand-in is not present.");
        Assert.SkipUnless(File.Exists(MediaPath), "Verification media file is not present.");
    }

    [Fact]
    public void Validate_MissingEngine_ReturnsActionableConfigurationError()
    {
        var config = new ChickenRiceConfig
        {
            UseManualEngine = true,
            ManualEnginePath = Path.Combine(VerifyRoot, "does-not-exist.exe"),
        };

        var (ok, error) = ChickenRiceASRService.Validate(config);

        Assert.False(ok);
        Assert.Contains("was not found", error);
        Assert.Contains("Subtitles > ASR", error);
    }

    [Fact]
    public void Validate_MissingModelFiles_ReturnsActionableError()
    {
        RequireFakeEngine();
        var config = ReadyConfig();
        config.ModelDirectory = Path.Combine(VerifyRoot, "empty-model-dir");
        Directory.CreateDirectory(config.ModelDirectory);

        var (ok, error) = ChickenRiceASRService.Validate(config);

        Assert.False(ok);
        Assert.Contains("CTranslate2 model files", error);
    }

    [Fact]
    public void Validate_ReadyEngine_Passes()
    {
        RequireFakeEngine();
        var (ok, error) = ChickenRiceASRService.Validate(ReadyConfig());
        Assert.True(ok, error);
    }

    [Fact]
    public void BuildArguments_PutsAllOptionsBeforePositionalMediaPath()
    {
        string args = ChickenRiceASRService.BuildArguments(
            ReadyConfig(), @"D:\media\my episode.mp4", @"D:\out dir");

        Assert.Contains("--overwrite", args);
        Assert.Contains("--sub_formats", args);
        Assert.Contains("srt", args);
        Assert.Contains("--output_dir", args);
        Assert.Contains("--task translate", args);
        Assert.Contains("--model_name_or_path", args);
        Assert.Contains("my episode.mp4", args);

        // argparse.REMAINDER: the media path must be the very last token group.
        int mediaIndex = args.LastIndexOf("my episode.mp4", StringComparison.Ordinal);
        int lastOptionIndex = args.LastIndexOf("--", StringComparison.Ordinal);
        Assert.True(mediaIndex > lastOptionIndex,
            $"media path must be last, got: {args}");
    }

    [Fact]
    public void TryResolveLocalFile_AcceptsLocalFile_RejectsOnlineStream()
    {
        RequireFakeEngine();
        Assert.True(ChickenRiceASRService.TryResolveLocalFile(MediaPath, out string path, out _));
        Assert.Equal(Path.GetFullPath(MediaPath), path);

        Assert.False(ChickenRiceASRService.TryResolveLocalFile(
            "https://example.com/live.m3u8", out _, out string error));
        Assert.Contains("online", error);
    }

    [Fact]
    public void ParseSrt_HandlesBomCrlfChineseMultilineAndTimestamps()
    {
        const string srt = "\uFEFF1\r\n" +
                           "00:00:00,500 --> 00:00:02,500\r\n" +
                           "你好，世界\r\n\r\n" +
                           "2\r\n" +
                           "00:01:03,250 --> 00:01:06,000\r\n" +
                           "这是第二行\r\n继续第二行\r\n\r\n";

        var subs = ChickenRiceASRService.ParseSrt(srt, "zh");

        Assert.Equal(2, subs.Count);
        Assert.Equal("你好，世界", subs[0].Text);
        Assert.Equal("这是第二行\n继续第二行", subs[1].Text);
        Assert.Equal(TimeSpan.FromMilliseconds(500), subs[0].StartTime);
        Assert.Equal(TimeSpan.FromMilliseconds(2500), subs[0].EndTime);
        Assert.Equal(TimeSpan.FromSeconds(63.25), subs[1].StartTime);
        Assert.All(subs, s => Assert.Equal("zh", s.Language));
    }

    [Fact]
    public void ParseSrt_EmptyInput_ReturnsEmptyList()
    {
        Assert.Empty(ChickenRiceASRService.ParseSrt("", "zh"));
        Assert.Empty(ChickenRiceASRService.ParseSrt("not a subtitle", "zh"));
    }

    [Fact]
    public void Generate_EndToEnd_ReturnsChineseSubtitlesFromUpstreamCli()
    {
        WithMode("ok", () =>
        {
            int before = TempDirCount();

            var subs = ChickenRiceASRService.Generate(
                ReadyConfig(), MediaPath, TimeSpan.Zero, CancellationToken.None);

            Assert.Equal(3, subs.Count);
            Assert.Contains(subs, s => s.Text.Contains("中文字幕"));
            Assert.All(subs, s => Assert.Equal("zh", s.Language));
            Assert.Equal(TimeSpan.FromMilliseconds(500), subs[0].StartTime);
            Assert.Equal(TimeSpan.FromSeconds(9), subs[2].EndTime);
            Assert.Equal(before, TempDirCount()); // temp output always cleaned
        });
    }

    [Fact]
    public void BuildArguments_RequestsDebugLevelForProgressiveSegments()
    {
        string args = ChickenRiceASRService.BuildArguments(ReadyConfig(), MediaPath, @"D:\out");

        // Upstream logs each decoded segment at DEBUG while it is still running;
        // that is the live subtitle channel used by progressive playback.
        Assert.Contains("--log_level DEBUG", args);
    }

    [Fact]
    public void TryParseLiveSegment_ParsesUpstreamDebugLine()
    {
        Assert.True(ChickenRiceASRService.TryParseLiveSegment(
            "[01:03.25 --> 01:06.00] 中文字幕", "zh", out SubtitleASRData? data));
        Assert.NotNull(data);
        Assert.Equal("中文字幕", data!.Text);
        Assert.Equal(TimeSpan.FromSeconds(63.25), data.StartTime);
        Assert.Equal(TimeSpan.FromSeconds(66), data.EndTime);
        Assert.Equal("zh", data.Language);

        Assert.False(ChickenRiceASRService.TryParseLiveSegment(
            "Smart VAD chunk 1/3: 00:00:00,000 --> 00:00:30,000", "zh", out _));
    }

    [Fact]
    public void ProgressiveBuffer_ReleasesSubtitlesByPlaybackProgress()
    {
        var subs = ChickenRiceASRService.ParseSrt(
            "1\n00:00:02,000 --> 00:00:03,000\n早\n\n" +
            "2\n00:00:40,000 --> 00:00:41,000\n晚\n\n", "zh");

        TimeSpan now = TimeSpan.Zero;
        List<SubtitleASRData> delivered = [];
        ChickenRiceProgressiveBuffer buffer = new(
            () => now, TimeSpan.FromSeconds(10), delivered.Add);

        // Deliberately add out of order: delivery must still be chronological.
        buffer.Add(subs[1]);
        buffer.Add(subs[0]);
        buffer.Pump();

        Assert.Single(delivered);
        Assert.Equal(TimeSpan.FromSeconds(2), delivered[0].StartTime);
        Assert.Equal(1, buffer.PendingCount); // future subtitle is held back

        now = TimeSpan.FromSeconds(35);
        buffer.Pump();

        Assert.Equal(2, delivered.Count);
        Assert.Equal(TimeSpan.FromSeconds(40), delivered[1].StartTime);
        Assert.Equal(0, buffer.PendingCount);
    }

    [Fact]
    public void GenerateProgressive_DeliversLiveSubtitlesBeforeTheEngineExits()
    {
        WithMode("progressive", () =>
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<long> liveAt = [];
            int completions = 0;
            IReadOnlyList<SubtitleASRData> final = [];

            IReadOnlyList<SubtitleASRData> result = ChickenRiceASRService.GenerateProgressive(
                ReadyConfig(), MediaPath, TimeSpan.Zero, CancellationToken.None,
                _ => liveAt.Add(sw.ElapsedMilliseconds),
                null,
                subs => { completions++; final = subs; });
            long total = sw.ElapsedMilliseconds;

            Assert.True(liveAt.Count >= 2, $"expected incremental segments, got {liveAt.Count}");
            Assert.True(liveAt[0] < total - 150,
                $"first subtitle must be available while the engine is still running (first={liveAt[0]}ms, total={total}ms)");
            Assert.Equal(1, completions);
            Assert.Equal(3, result.Count);
            Assert.Equal(3, final.Count);
            Assert.All(result, s => Assert.Equal("zh", s.Language));
        });
    }

    [Fact]
    public void GenerateProgressive_StreamsFromIncrementallyWrittenSrt()
    {
        WithMode("progressive_srt", () =>
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<long> liveAt = [];

            IReadOnlyList<SubtitleASRData> result = ChickenRiceASRService.GenerateProgressive(
                ReadyConfig(), MediaPath, TimeSpan.Zero, CancellationToken.None,
                _ => liveAt.Add(sw.ElapsedMilliseconds));
            long total = sw.ElapsedMilliseconds;

            Assert.True(liveAt.Count >= 2, $"expected incremental SRT blocks, got {liveAt.Count}");
            Assert.True(liveAt[0] < total - 150,
                $"first SRT block must be read during processing (first={liveAt[0]}ms, total={total}ms)");
            Assert.Equal(3, result.Count);
        });
    }

    [Fact]
    public void Generate_TranscribeTask_UsesJapaneseLanguageTag()
    {
        var config = ReadyConfig();
        config.Task = "transcribe";
        Assert.Equal("ja", config.OutputLanguage);
        Assert.Equal("zh", ReadyConfig().OutputLanguage);
    }

    [Fact]
    public void Generate_NonZeroExit_ThrowsReadableErrorAndCleansUp()
    {
        WithMode("exit1", () =>
        {
            int before = TempDirCount();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                ChickenRiceASRService.Generate(ReadyConfig(), MediaPath, TimeSpan.Zero, CancellationToken.None));

            Assert.Contains("code 7", ex.Message);
            Assert.Contains("simulated upstream failure", ex.Message);
            Assert.Equal(before, TempDirCount());
        });
    }

    [Fact]
    public void Generate_MissingSrt_ThrowsReadableErrorAndCleansUp()
    {
        WithMode("nosrt", () =>
        {
            int before = TempDirCount();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                ChickenRiceASRService.Generate(ReadyConfig(), MediaPath, TimeSpan.Zero, CancellationToken.None));

            Assert.Contains("did not produce an SRT file", ex.Message);
            Assert.Equal(before, TempDirCount());
        });
    }

    [Fact]
    public void Generate_Cancellation_KillsEngineAndCleansUp()
    {
        WithMode("sleep", () =>
        {
            int before = TempDirCount();
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(900));

            Assert.ThrowsAny<OperationCanceledException>(() =>
                ChickenRiceASRService.Generate(ReadyConfig(), MediaPath, TimeSpan.Zero, cts.Token));

            Assert.Equal(before, TempDirCount());
        });
    }

    [Fact]
    public void EngineSelection_ExistingEnumValuesUnchanged_SavedConfigsRemainValid()
    {
        // The enum is persisted as an integer in LLPlayer.PlayerConfig.json.
        // Appending ChickenRice must not renumber the two pre-existing engines.
        Assert.Equal(0, (int)SubASREngineType.WhisperCpp);
        Assert.Equal(1, (int)SubASREngineType.FasterWhisper);
        Assert.Equal(2, (int)SubASREngineType.ChickenRice);
        Assert.Equal(3, Enum.GetValues<SubASREngineType>().Length);

        var fw = new FasterWhisperConfig();
        Assert.Equal("tiny", fw.Model);
        Assert.Equal("--device cpu", fw.ExtraArguments);

        var cr = new ChickenRiceConfig();
        Assert.Equal("translate", cr.Task);
        Assert.Equal("cuda", cr.Device);
        Assert.False(cr.UseManualEngine);
        Assert.Equal("zh", cr.OutputLanguage);
    }

    [Fact]
    public void Validate_ChickenRiceError_IsReportedAsConfigurationNotCrash()
    {
        var config = new ChickenRiceConfig
        {
            UseManualEngine = true,
            ManualEnginePath = Path.Combine(VerifyRoot, "missing-infer.exe"),
        };

        var (ok, error) = ChickenRiceASRService.Validate(config);

        Assert.False(ok);
        Assert.Contains("ChickenRice", error);
        Assert.DoesNotContain("Exception", error);
    }

    [Fact]
    public void Config_JsonRoundTrip_PersistsChickenRiceSettings()
    {
        var config = ReadyConfig();
        config.ExtraArguments = "--vad_threshold 0.45";
        config.TimeoutMinutes = 45;
        config.ProgressiveEnabled = false;
        config.ProgressiveLeadSeconds = 15;

        string json = JsonSerializer.Serialize(config);
        var restored = JsonSerializer.Deserialize<ChickenRiceConfig>(json)!;

        Assert.True(restored.UseManualEngine);
        Assert.Equal(FakeBat, restored.ManualEnginePath);
        Assert.Equal("translate", restored.Task);
        Assert.Equal("cpu", restored.Device);
        Assert.Equal("models", restored.ModelDirectory);
        Assert.Equal("--vad_threshold 0.45", restored.ExtraArguments);
        Assert.Equal(45, restored.TimeoutMinutes);
        Assert.False(restored.ProgressiveEnabled);
        Assert.Equal(15, restored.ProgressiveLeadSeconds);

        var defaults = new ChickenRiceConfig();
        Assert.True(defaults.ProgressiveEnabled);
        Assert.Equal(30, defaults.ProgressiveLeadSeconds);
    }
}
