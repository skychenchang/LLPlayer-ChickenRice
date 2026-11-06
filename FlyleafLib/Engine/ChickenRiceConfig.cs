using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

namespace FlyleafLib;

#nullable enable

/// <summary>
/// Configuration for the TransWithAI ChickenRice ASR engine.
///
/// ChickenRice is a whole-file, offline batch tool (not a streaming ASR engine):
/// it takes a media file, runs a fine-tuned Whisper model with a custom VAD and
/// subtitle post-processing, and writes an SRT next to the output directory.
/// See https://github.com/TransWithAI/Faster-Whisper-TransWithAI-ChickenRice
///
/// The default layout matches the upstream release package:
///   Whisper/ChickenRice/infer.exe
///   Whisper/ChickenRice/models/...
///   Whisper/ChickenRice/generation_config.json5
/// </summary>
public class ChickenRiceConfig : NotifyPropertyChanged
{
    public static string DefaultEngineDirectory { get; } = Path.Combine(WhisperConfig.EnginesDirectory, "ChickenRice");
    public static string DefaultEnginePath { get; } = Path.Combine(DefaultEngineDirectory, "infer.exe");
    public static string DefaultGenerationConfigPath { get; } = Path.Combine(DefaultEngineDirectory, "generation_config.json5");

    /// <summary>Use <see cref="ManualEnginePath"/> instead of the bundled engine.</summary>
    public bool UseManualEngine { get; set => Set(ref field, value); }

    /// <summary>Absolute path to infer.exe (or a .bat / .cmd launcher).</summary>
    public string? ManualEnginePath { get; set => Set(ref field, value); }

    /// <summary>
    /// CTranslate2 model directory passed as --model_name_or_path.
    /// Relative paths are resolved against the engine directory (upstream default: "models").
    /// </summary>
    public string ModelDirectory { get; set => Set(ref field, value); } = "models";

    /// <summary>--device (cuda / cpu / amd).</summary>
    public string Device { get; set => Set(ref field, value); } = "cuda";

    /// <summary>--compute_type (auto / float16 / int8_float16 / ...).</summary>
    public string ComputeType { get; set => Set(ref field, value); } = "auto";

    /// <summary>--task (translate = Japanese speech to Chinese text, transcribe = Japanese text).</summary>
    public string Task { get; set => Set(ref field, value); } = "translate";

    /// <summary>Additional raw CLI arguments appended before the positional media path.</summary>
    public string ExtraArguments { get; set => Set(ref field, value); } = "";

    /// <summary>Maximum runtime for a single whole-file transcription, in minutes (0 = no limit).</summary>
    public int TimeoutMinutes { get; set => Set(ref field, value); } = 60;

    /// <summary>
    /// Deliver subtitles incrementally while the upstream process is still running
    /// instead of waiting for the whole file. When disabled the legacy "wait for the
    /// complete SRT, then inject everything" behaviour is kept as a fallback.
    /// </summary>
    public bool ProgressiveEnabled { get; set => Set(ref field, value); } = true;

    /// <summary>
    /// How far ahead of the playhead (in seconds) generated subtitles may be
    /// released into the subtitle list. Larger values preload more; smaller values
    /// keep delivery tighter to the current playback position.
    /// </summary>
    public int ProgressiveLeadSeconds { get; set => Set(ref field, value); } = 30;

    [JsonIgnore]
    public string EnginePath => UseManualEngine && !string.IsNullOrWhiteSpace(ManualEnginePath)
        ? ManualEnginePath!
        : DefaultEnginePath;

    [JsonIgnore]
    public string EngineDirectory => Path.GetDirectoryName(EnginePath) ?? DefaultEngineDirectory;

    [JsonIgnore]
    public string GenerationConfigPath => Path.Combine(EngineDirectory, "generation_config.json5");

    [JsonIgnore]
    public string ResolvedModelDirectory => Path.IsPathRooted(ModelDirectory)
        ? ModelDirectory
        : Path.Combine(EngineDirectory, ModelDirectory);

    /// <summary>ISO 639-1 language of the generated subtitles.</summary>
    [JsonIgnore]
    public string OutputLanguage => string.Equals(Task, "transcribe", System.StringComparison.OrdinalIgnoreCase)
        ? "ja"
        : "zh";

    [JsonIgnore]
    public ProcessPriorityClass ProcessPriority { get; set => Set(ref field, value); } = ProcessPriorityClass.Normal;
}
