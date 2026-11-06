using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace LLPlayer.Services;

/// <summary>
/// UI language for the player menus and the settings interface, selected from
/// Settings &gt; Player &gt; UI Language.
/// Persisted in LLPlayer.Config.json through <see cref="AppConfig.MenuUILanguage"/>.
/// Chinese is the default for new installations; a saved English value is respected.
/// </summary>
public enum MenuLanguage
{
    English,
    Chinese
}

/// <summary>
/// Minimal, allocation-free string table for the menu copy. Keys are the original
/// English strings, which keeps every existing XAML Command / CommandParameter /
/// binding untouched: the localizer rewrites only literal text at runtime.
/// </summary>
public static class MenuStrings
{
    private static MenuLanguage _language = MenuLanguage.Chinese;

    public static MenuLanguage Language
    {
        get => _language;
        set
        {
            if (_language == value)
                return;

            _language = value;
            LanguageChanged?.Invoke();
        }
    }

    /// <summary>Raised on language changes so data-backed UI can re-evaluate.</summary>
    public static event Action? LanguageChanged;

    public static bool IsChinese => Language == MenuLanguage.Chinese;

    /// <summary>
    /// Data source for the UI language ComboBox. Display values are DATA, not
    /// dictionary keys: "English" / "中文" stay identical in both languages.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<MenuLanguage, string>> UILanguageOptions { get; } =
    [
        new(MenuLanguage.English, "English"),
        new(MenuLanguage.Chinese, "中文"),
    ];

    private static readonly Dictionary<string, string> Chinese = new(StringComparer.Ordinal)
    {
        // Main popup menu
        ["Open File"] = "打开文件",
        ["Paste URL"] = "粘贴网址",
        ["Audio"] = "音频",
        ["Subtitles"] = "字幕",
        ["Video"] = "视频",
        ["App"] = "应用",
        ["Settings"] = "设置",
        ["Interface"] = "界面",
        ["UI Language"] = "界面语言",
        ["Playlist ({0}/{1})"] = "播放列表 ({0}/{1})",
        ["Show Debug"] = "显示调试信息",
        ["Cheat Sheet"] = "快捷键说明",
        ["Always On Top"] = "窗口置顶",
        ["Launch App"] = "启动新窗口",
        ["Clone App"] = "复制窗口",
        ["Restart App"] = "重启应用",
        ["Exit App"] = "退出应用",

        // Shared menu items
        ["Enabled"] = "启用",
        ["Embedded"] = "内置",
        ["External"] = "外部",
        ["Streams"] = "流",
        ["Devices"] = "设备",
        ["Off"] = "关闭",
        ["Reset..."] = "重置...",
        ["Reset ..."] = "重置 ...",
        ["Increase"] = "增大",
        ["Decrease"] = "减小",
        ["Top"] = "顶部",
        ["Center"] = "居中",
        ["Bottom"] = "底部",
        ["Up"] = "上移",
        ["Down"] = "下移",
        ["ASR ({0})"] = "语音识别 ({0})",
        ["Auto-Translate"] = "自动翻译",
        ["If you run it again when it is already enabled, subtitles will be regenerated from the playback position."] =
            "再次启用时，将从当前播放位置重新生成字幕。",

        // Audio / subtitles
        ["Delay ({0})"] = "延迟 ({0})",
        ["Delay 1st ({0})"] = "主字幕延迟 ({0})",
        ["Delay 2nd ({0})"] = "副字幕延迟 ({0})",
        ["From 1st"] = "与主字幕相同",
        ["From 2nd"] = "与副字幕相同",
        ["Fonts..."] = "字体...",
        ["Position ({0})"] = "位置 ({0})",
        ["Size ({0})"] = "字号 ({0})",
        ["Size 1st ({0})"] = "主字幕字号 ({0})",
        ["Size 2nd ({0})"] = "副字幕字号 ({0})",
        ["Distance ({0})"] = "间距 ({0})",
        ["Alignment"] = "对齐",
        ["When Dual"] = "双字幕时",
        ["Visibility"] = "显示",
        ["1st Visible"] = "显示主字幕",
        ["2nd Visible"] = "显示副字幕",
        ["Subs Downloader"] = "字幕下载",
        ["Subs Exporter"] = "字幕导出",

        // Video
        ["Aspect Ratio"] = "宽高比",
        ["Chapters"] = "章节",
        ["HW Acceleration"] = "硬件加速",
        ["Record"] = "录制",
        ["Rotation ({0}°)"] = "旋转 ({0}°)",
        ["Flip"] = "翻转",
        ["Horizontal Flip"] = "水平翻转",
        ["Vertical Flip"] = "垂直翻转",
        ["Reverse Playback"] = "倒放",
        ["Loop Playback"] = "循环播放",
        ["Speed (x{0})"] = "倍速 (x{0})",
        ["Take a Snapshot"] = "截图",
        ["V.Sync"] = "垂直同步",
        ["Zoom ({0}%)"] = "缩放 ({0}%)",
        ["Zoom In"] = "放大",
        ["Zoom out"] = "缩小",

        // Bottom bar tooltips / title context menu
        ["Play / Pause"] = "播放 / 暂停",
        ["Primary Subtitles"] = "主字幕",
        ["Secondary Subtitles"] = "副字幕",
        ["Audio Streams"] = "音频流",
        ["Video Streams"] = "视频流",
        ["Mute / Unmute"] = "静音 / 取消静音",
        ["Previous Playlist Item"] = "上一个播放项",
        ["Next Playlist Item"] = "下一个播放项",
        ["Open Settings"] = "打开设置",
        ["Toggle Sub Sidebar"] = "显示 / 隐藏字幕侧栏",
        ["Toggle Fullscreen"] = "全屏切换",
        ["Open next"] = "打开下一个",
        ["Open prev"] = "打开上一个",
        ["Open path"] = "打开所在路径",
        ["Copy path"] = "复制路径",
        ["Left Click: Open File Dialog\nRight Click: Open Context Menu"] = "左键：打开文件\n右键：打开菜单",
        ["Left Click: Open File Dialog\r\nRight Click: Open Context Menu"] = "左键：打开文件\r\n右键：打开菜单",

        // ChickenRice settings hint
        ["Whole-file offline Japanese-to-Chinese subtitles (TransWithAI ChickenRice). Select ASR (AUTO) from the CC menu to generate subtitles for the entire local file. Turn off auto-translate, because this engine already outputs Chinese."] =
            "离线整文件日译中字幕（TransWithAI ChickenRice）。在字幕菜单中选择「语音识别」后，会为整个本地文件生成字幕；播放开始后即可显示已生成的字幕，其余字幕会在播放过程中继续生成并按进度同步显示。该引擎已直接输出中文，请关闭自动翻译。",

        // ---- Settings / dialogs (generated by verify/localization/apply_translations.py) ----
        ["(Defaults to 1) An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered. We generally recommend altering this or temperature but not both."] = "（默认 1）温度采样的替代方案，称为核采样：模型只考虑累计概率达到 top_p 的词元。例如 0.1 表示只考虑累计概率前 10% 的词元。通常建议只调整本项或 temperature，不要同时调整。",
        ["(Defaults to 1) What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic. We generally recommend altering this or top_p but not both."] = "（默认 1）采样温度，取值 0 到 2。数值越高（如 0.8）输出越随机，数值越低（如 0.2）输出越集中、确定。通常建议只调整本项或 top_p，不要同时调整。",
        ["(maybe deprecated) The maximum number of tokens that can be generated in the chat completion. This value can be used to control costs for text generated via API. This value is now deprecated in favor of max_completion_tokens, and is not compatible with o-series models."] = "（可能已弃用）对话补全可生成的最大词元数，可用于控制 API 文本生成成本。该参数现已由 max_completion_tokens 取代，且与 o 系列模型不兼容。",
        ["- video.en.srt (language is set from the lang code)"] = "- video.en.srt（语言由语言代码决定）",
        ["- video.eng.srt (same as above)"] = "- video.eng.srt（同上）",
        ["- video.foo.srt (language is auto-detected)"] = "- video.foo.srt（自动检测语言）",
        ["0 for Auto"] = "0 表示自动",
        ["About"] = "关于",
        ["Accurate"] = "精确",
        ["Action"] = "动作",
        ["Add"] = "添加",
        ["Add Clipboard"] = "添加复制",
        ["Add ClipboardAll"] = "添加复制全部",
        ["Add Search"] = "添加搜索",
        ["Always Seek Accurate"] = "始终精确定位",
        ["An upper bound for the number of tokens that can be generated for a completion, including visible output tokens and reasoning tokens."] = "一次补全可生成词元数的上限，包含可见输出词元与推理词元。",
        ["Apply"] = "应用",
        ["ASR"] = "语音识别",
        ["ASR settings"] = "语音识别设置",
        ["Audio Frames Max"] = "音频帧数上限",
        ["Audio Language (Priority to the above)"] = "音频语言（优先于上方设置）",
        ["Audio language can be set manually. If automatic detection is used, it will take precedence."] = "可手动指定音频语言；启用自动检测时，以检测结果为准。",
        ["Auto Detect"] = "自动检测",
        ["Auto Set"] = "自动设置",
        ["Auto Text Copy Target"] = "自动复制文本目标",
        ["Automatically detects language from text subtitle. Accurate language identification allows for the translation function. Memory usage will increase slightly."] = "从文本字幕自动识别语言。准确识别语言后才能使用翻译功能，内存占用会略有增加。",
        ["Available Languages"] = "可用语言",
        ["Available Options"] = "可用选项",
        ["Background Color"] = "背景颜色",
        ["Background Opacity"] = "背景不透明度",
        ["Background Opacity (2nd)"] = "背景不透明度（副字幕）",
        ["Bitmap Position (Primary) (0-150%)"] = "位图位置（主字幕）（0-150%）",
        ["Bitmap Scale (Primary)"] = "位图缩放（主字幕）",
        ["Bitmap Scale (Secondary)"] = "位图缩放（副字幕）",
        ["Bitmap Scale Offset (%)"] = "位图缩放步进（%）",
        ["Buffer Duration Max (ms)"] = "最大缓冲时长（毫秒）",
        ["Buffer Duration Min (ms)"] = "最小缓冲时长（毫秒）",
        ["Check"] = "检查",
        ["Check Engine"] = "检查引擎",
        ["ChickenRice config"] = "ChickenRice 配置",
        ["Clone Row"] = "复制行",
        ["Close"] = "关闭",
        ["Close the dialog without saving to the config file. The changes are reflected until restart."] = "不写入配置文件直接关闭对话框。改动在重启前仍然有效。",
        ["Commit:"] = "提交版本：",
        ["Common:"] = "通用：",
        ["Compute Type"] = "计算精度",
        ["Configure"] = "配置",
        ["Copy Debug Command"] = "复制调试命令",
        ["Copy Help Command"] = "复制帮助命令",
        ["Copy the text subtitle to the clipboard when it changes. Note that this does not work for bitmap subtitles. In that case, please use OCR to convert them to text."] = "文本字幕变化时自动复制到剪贴板。位图字幕不适用，需先用 OCR 转为文本。",
        ["Copy version to clipboard"] = "复制版本号到剪贴板",
        ["Copy words on selected"] = "选中时复制单词",
        ["CTranslate2 model directory passed as --model_name_or_path. Relative paths resolve against the engine folder (upstream default: models)."] = "作为 --model_name_or_path 传入的 CTranslate2 模型目录。相对路径以引擎目录为基准（上游默认：models）。",
        ["Current Color"] = "当前颜色",
        ["Debug"] = "调试",
        ["Decoder Threads"] = "解码线程数",
        ["Default Device"] = "默认设备",
        ["Default Volume (%)"] = "默认音量（%）",
        ["Default: Whisper\\Faster-Whisper-XXL\\faster-whisper-xxl.exe"] = "默认：Whisper\\Faster-Whisper-XXL\\faster-whisper-xxl.exe",
        ["Default: whispermodels\\"] = "默认：whispermodels\\",
        ["Deinterlace"] = "去隔行",
        ["Delay (ms)"] = "延迟（毫秒）",
        ["Delay Offset1 (ms)"] = "延迟步进 1（毫秒）",
        ["Delay Offset2 (ms)"] = "延迟步进 2（毫秒）",
        ["Delay Primary (ms)"] = "主字幕延迟（毫秒）",
        ["Delay Secondary (ms)"] = "副字幕延迟（毫秒）",
        ["Delete"] = "删除",
        ["Delete Row"] = "删除行",
        ["Details"] = "详情",
        ["Device"] = "设备",
        ["Distance Offset"] = "间距步进",
        ["Do last search on selected"] = "选中时重复上次搜索",
        ["Double Click to FullScreen"] = "双击进入全屏",
        ["Double Rate"] = "双倍速率",
        ["Download Engine"] = "下载引擎",
        ["Download Model"] = "下载模型",
        ["Downloaded Languages"] = "已下载语言",
        ["e.g. --device cpu"] = "例如 --device cpu",
        ["Error"] = "错误",
        ["Export UTF8 with BOM"] = "导出带 BOM 的 UTF-8",
        ["Extra Arguments"] = "附加参数",
        ["Fallback Source Language (Primary)"] = "备用源语言（主字幕）",
        ["Fallback Source Language (Secondary)"] = "备用源语言（副字幕）",
        ["faster-whisper config"] = "faster-whisper 配置",
        ["FFmpeg Load Profile"] = "FFmpeg 加载配置",
        ["Filters"] = "滤镜",
        ["Fix Overflow Bottom Margin (0-100%)"] = "底部溢出修正边距（0-100%）",
        ["Font Color"] = "字体颜色",
        ["Font Color (2nd)"] = "字体颜色（副字幕）",
        ["Font Size (1st)"] = "字号（主字幕）",
        ["Font Size (2nd)"] = "字号（副字幕）",
        ["Font Size Offset"] = "字号步进",
        ["Fonts"] = "字体",
        ["Fonts (2nd)"] = "字体（副字幕）",
        ["Get Models"] = "获取模型",
        ["HDR to SDR Method"] = "HDR 转 SDR 方式",
        ["Hello API"] = "测试接口",
        ["If enabled, automatic subtitles such as those on YouTube are automatically opened. If disabled, they will be added but not auto-open."] = "启用后会自动打开 YouTube 之类的自动字幕；关闭时仍会加入列表，但不会自动打开。",
        ["If enabled, internal bitmap subtitles can be displayed in the sidebar. and internal bitmap subtitles are always displayed during seek, However, memory usage will increase."] = "启用后，内置位图字幕可显示在侧栏中，且在跳转时始终显示；内存占用会增加。",
        ["If enabled, subtitle files in the same path as the media file are automatically opened. The following subtitle files will be opened."] = "启用后会自动打开与媒体文件同目录下的字幕文件。将打开以下字幕文件：",
        ["If this is switched on, the seek bar is only displayed on mouse-over; if it is switched off, it is displayed when the cursor is moved."] = "开启时进度条仅在鼠标悬停时显示；关闭时只要移动光标即显示。",
        ["Ignore Line Break"] = "忽略换行",
        ["Input Enter"] = "输入回车",
        ["Key"] = "按键",
        ["Keys"] = "快捷键",
        ["Keys > Offset"] = "快捷键 > 偏移",
        ["Language"] = "语言",
        ["Larger models increase load and accuracy. Please choose the appropriate model for your hardware. Models ending in .en are only available in English."] = "模型越大负载越高、准确率也越高，请根据硬件选择合适的模型。以 .en 结尾的模型仅支持英语。",
        ["LLM Parameters"] = "大模型参数",
        ["LLPlayer cannot exist without the following libraries!"] = "LLPlayer 依赖以下程序库！",
        ["Load"] = "加载",
        ["Loaded Option:"] = "已加载选项：",
        ["Log Cached Lines"] = "日志缓存行数",
        ["Log File"] = "日志文件",
        ["Log Level"] = "日志级别",
        ["Log Level (ffmpeg)"] = "日志级别（FFmpeg）",
        ["Logging"] = "日志",
        ["Manual"] = "手动",
        ["Manual Engine Path"] = "手动指定引擎路径",
        ["Manual Model Directory"] = "手动指定模型目录",
        ["Mask subtitles after current (to prevent spoiler)"] = "遮挡当前字幕之后的字幕（防剧透）",
        ["Max Latency (ms)"] = "最大延迟（毫秒）",
        ["Max Translate Concurrency"] = "最大翻译并发数",
        ["Max Translate Count (Backward)"] = "向后翻译条数上限",
        ["Max Translate Count (Forward)"] = "向前翻译条数上限",
        ["Max Volume (1-1000%)"] = "最大音量（1-1000%）",
        ["Modifier:"] = "修饰键：",
        ["Mouse"] = "鼠标",
        ["Move Down (right selected libraries)"] = "下移（右侧选中项）",
        ["Move Left"] = "左移",
        ["Move Right"] = "右移",
        ["Move Up (right selected libraries)"] = "上移（右侧选中项）",
        ["No Supported Service"] = "无可用服务",
        ["OCR Engine (Primary)"] = "OCR 引擎（主字幕）",
        ["OCR Engine (Secondary)"] = "OCR 引擎（副字幕）",
        ["Offset"] = "偏移",
        ["Open Engine Folder"] = "打开引擎目录",
        ["Open Folder"] = "打开目录",
        ["Open Timeout (ms)"] = "打开超时（毫秒）",
        ["Order"] = "顺序",
        ["OS Architecture:"] = "操作系统架构：",
        ["Parameter for how many MB of audio to accumulate and pass to ASR. Increasing it increase memory usage and may cause natural breaks in sentences."] = "累计多少 MB 音频后送入语音识别。数值越大内存占用越高，也可能在句子自然断句处被切断。",
        ["Parameter for how many seconds to wait to accumulate and pass to ASR. It is the real time, not the audio time. This parameter is ORed with Audio Chunk Size, and is used for when the size of the video is small, such as in the case of live video."] = "累计多少秒后送入语音识别，按实际时间而非音频时间计算。该参数与「音频块大小」取或关系，用于直播等较小的视频。",
        ["Path"] = "路径",
        ["PDIC Executable Path"] = "PDIC 可执行文件路径",
        ["Player Timeout (For Live Stream)"] = "播放器超时（直播流）",
        ["Player UI"] = "播放器界面",
        ["Please configure the settings in advance in the Translate tab."] = "请先在「翻译」页签中完成设置。",
        ["Plugins"] = "插件",
        ["Position / Size"] = "位置 / 大小",
        ["Position Alignment"] = "位置对齐",
        ["Position Alignment (When Dual)"] = "位置对齐（双字幕时）",
        ["Position Offset (%)"] = "位置步进（%）",
        ["Primary Color"] = "主色",
        ["Primary/Secondary Distance"] = "主/副字幕间距",
        ["Process Architecture:"] = "进程架构：",
        ["Profile Mismatch"] = "配置不匹配",
        ["Progress Lead (sec)"] = "进度提前量（秒）",
        ["Prompt to send to chat. Certain strings will be replaced by the following."] = "发送给对话模型的提示词，其中部分字符串会按下列规则替换。",
        ["Read Live Timeout (ms)"] = "直播读取超时（毫秒）",
        ["Read Timeout (ms)"] = "读取超时（毫秒）",
        ["Region (optional)"] = "区域（可选）",
        ["Required to restart to apply changes"] = "需要重启后才能生效",
        ["Reset"] = "重置",
        ["Reset all"] = "全部重置",
        ["Reset from 1st"] = "按主字幕重置",
        ["Reset to default"] = "恢复默认",
        ["Same as Primary"] = "与主字幕相同",
        ["Save & Close"] = "保存并关闭",
        ["SDR Display Nits"] = "SDR 显示亮度",
        ["Search Local Paths"] = "搜索本地路径",
        ["Secondary Color"] = "辅色",
        ["Seek"] = "定位",
        ["Seek Accurate Fix Margin (ms)"] = "精确定位修正余量（毫秒）",
        ["Seek Offset1 (ms)"] = "定位步进 1（毫秒）",
        ["Seek Offset2 (ms)"] = "定位步进 2（毫秒）",
        ["Seek Offset3 (ms)"] = "定位步进 3（毫秒）",
        ["Seek Offset4 (ms)"] = "定位步进 4（毫秒）",
        ["Seek Timeout (ms)"] = "定位超时（毫秒）",
        ["SeekBar FadeIn Time (ms)"] = "进度条淡入时间（毫秒）",
        ["SeekBar FadeOut Time (ms)"] = "进度条淡出时间（毫秒）",
        ["SeekBar Height"] = "进度条高度",
        ["SeekBar Thumb Height"] = "进度条滑块高度",
        ["SeekBar Track Height"] = "进度条轨道高度",
        ["Select OCR Engine"] = "选择 OCR 引擎",
        ["Select specific region"] = "选择具体区域",
        ["Selected Options (Priority to the above)"] = "已选选项（优先于上方设置）",
        ["Separator Max Width"] = "分隔线最大宽度",
        ["Separator to be displayed when dual subtitles are used. If you do not want to display it, set it to 0."] = "双字幕时显示的分隔线。设为 0 则不显示。",
        ["Set Default"] = "设为默认",
        ["Set your native language. It will be translated into that language. If it is in the same language as the video, it will not be translated. Some engines do not support certain languages."] = "设置你的母语，字幕将翻译为该语言。若与视频语言相同则不翻译；部分引擎不支持某些语言。",
        ["Sets the maximum width of the subtitle; if set to 0, no limit is set."] = "设置字幕最大宽度；设为 0 表示不限制。",
        ["Show already generated subtitles as soon as playback starts. Remaining subtitles keep being generated in the background and are released in sync with the playback position. Turn off to wait for the whole file (legacy behaviour)."] = "播放开始后即显示已生成的字幕，其余字幕在后台继续生成，并按播放进度同步显示。关闭后恢复为等待整份文件处理完成（旧行为）。",
        ["Show Original Text when translating enabled"] = "启用翻译时显示原文",
        ["Show Sidebar"] = "显示侧栏",
        ["Sidebar Left"] = "侧栏靠左",
        ["Sidebar Width"] = "侧栏宽度",
        ["Single Click to Play/Pause"] = "单击播放 / 暂停",
        ["Size"] = "大小",
        ["Source Language Region"] = "源语言区域",
        ["Specifies the GPU device number to be used, index starting from 0. The first device is used by default."] = "指定使用的 GPU 设备编号，从 0 开始，默认使用第一块设备。",
        ["Specify additional folder paths to open subtitles, separated by semicolons. If left blank, subtitles will be opened only from the current directory. Folder case is ignored. Both relative and absolute paths can be specified."] = "指定额外搜索字幕的目录，多个路径用分号分隔。留空时仅搜索当前目录。目录名不区分大小写，可使用相对或绝对路径。",
        ["Specify idle time in milliseconds to hide seek bar and mouse cursor"] = "指定隐藏进度条与鼠标指针的空闲时间（毫秒）",
        ["Specify the full path to faster-whisper-xxl.exe or whisper-faster.exe."] = "指定 faster-whisper-xxl.exe 或 whisper-faster.exe 的完整路径。",
        ["Specify the number of threads. This will increase the load, but will speed up subtitle generation. Do not specify more than CPU threads."] = "指定线程数。数值越大负载越高，但字幕生成更快；请勿超过 CPU 线程数。",
        ["Speed Offset1"] = "倍速步进 1",
        ["Speed Offset2"] = "倍速步进 2",
        ["Stroke Color"] = "描边颜色",
        ["Stroke Thickness"] = "描边粗细",
        ["Subtitle Padding"] = "字幕内边距",
        ["Subtitles (Position/Size)"] = "字幕（位置 / 大小）",
        ["Subtitles > ASR"] = "字幕 > 语音识别",
        ["Subtitles > OCR"] = "字幕 > OCR",
        ["Subtitles > Position / Size"] = "字幕 > 位置 / 大小",
        ["Subtitles > Translate"] = "字幕 > 翻译",
        ["Subtitles > Word Action"] = "字幕 > 取词动作",
        ["Subtitles Auto Copy"] = "字幕自动复制",
        ["Subtitles Fonts"] = "字幕字体",
        ["Subtitles Language"] = "字幕语言",
        ["Subtitles Language Auto-Open (Priority to the above)"] = "字幕语言自动打开（优先于上方设置）",
        ["Subtitles Sidebar"] = "字幕侧栏",
        ["Super Resolution"] = "超分辨率",
        ["The model is automatically downloaded at the first run. Note that if the size is large, it will take longer. Models starting with distil- and ending in .en are only available in English."] = "首次运行时自动下载模型；模型越大耗时越长。以 distil- 开头或以 .en 结尾的模型仅支持英语。",
        ["The number of subtitles to be sent together. Increasing the number increases the accuracy of the translation, but also the load and tokens."] = "一次一并发送的字幕条数。条数越多翻译越准确，但负载与词元消耗也越大。",
        ["Themes"] = "主题",
        ["Timeout (minutes)"] = "超时（分钟）",
        ["Timeout (ms)"] = "超时（毫秒）",
        ["Timeout Health (ms)"] = "健康检查超时（毫秒）",
        ["Title"] = "标题",
        ["Translate"] = "翻译",
        ["translate = Japanese speech to Chinese subtitles (fine-tuned model). transcribe = Japanese original text (use the whisper-ja model in the model directory)."] = "translate = 日语语音转中文字幕（微调模型）；transcribe = 输出日文原文（需在模型目录使用 whisper-ja 模型）。",
        ["Translated into English using Whisper's built-in translation engine. Only English is supported. If you want to translate into another language, you can use the LLPlayer's translation engine."] = "使用 Whisper 内置翻译引擎译为英文，仅支持英文。如需翻译为其他语言，请使用 LLPlayer 的翻译引擎。",
        ["Translation Chat Config (for LLM API such as Ollama, OpenAI, etc...)"] = "对话翻译配置（适用于 Ollama、OpenAI 等大模型接口）",
        ["Translation Engine"] = "翻译引擎",
        ["Translation Language"] = "翻译语言",
        ["Translation Parameters"] = "翻译参数",
        ["True for English-only models, in which case the Audio Language setting above is ignored."] = "仅支持英语的模型请开启此项；开启后上方的「音频语言」设置将被忽略。",
        ["Use Filters"] = "使用滤镜",
        ["Use Manual Engine"] = "手动指定引擎",
        ["Use Separate 2nd Fonts"] = "副字幕使用独立字体",
        ["Using libraries"] = "使用的程序库",
        ["Version:"] = "版本：",
        ["Vertical Position (-25%-150%)"] = "垂直位置（-25%-150%）",
        ["Vertical Resolution Max"] = "最大垂直分辨率",
        ["Video Acceleration"] = "硬件加速",
        ["Video Frames Max"] = "视频帧数上限",
        ["Video Processor"] = "视频处理器",
        ["Volume Offset"] = "音量步进",
        ["Wheel to Volume Up/Down"] = "滚轮调节音量",
        ["Whether to include regional information in the {target_lang} of the prompt as follows."] = "是否在提示词的 {target_lang} 中包含如下区域信息。",
        ["whisper.cpp config"] = "whisper.cpp 配置",
        ["whisper.cpp hardware options (Required to restart to apply changes)"] = "whisper.cpp 硬件选项（需重启后生效）",
        ["Width (1-100%)"] = "宽度（1-100%）",
        ["Word Action"] = "取词动作",
        ["Word Actions"] = "取词动作",
        ["Word Click Action:"] = "单词点击动作：",
        ["Word Context Menu"] = "取词右键菜单",
        ["Word Special Actions for Japanese"] = "日语取词专用动作",
        ["Word Translation Engine"] = "取词翻译引擎",
        ["Writes to the config file and closes the dialog."] = "写入配置文件并关闭对话框。",
        ["Yes"] = "是",
        ["You can change the priority of the whisper process. If playback is affected, you can lower the priority."] = "可调整 Whisper 进程优先级；若影响播放，可适当调低。",
        ["You can copy debugging commands to the clipboard, which can then be executed in PowerShell by pasting."] = "可将调试命令复制到剪贴板，粘贴到 PowerShell 中执行。",
        ["You can limit the number of characters per subtitle. If the default subtitle is too long, you can adjust it by specifying the number of characters."] = "可限制每条字幕的字符数；若默认字幕过长，可指定字符数进行调整。",
        ["You may be able to customize the output. For example, you can control whether the output is in Simplified or Traditional Chinese."] = "可用于自定义输出，例如控制输出为简体中文还是繁体中文。",
        ["Zero Latency"] = "零延迟",
        ["Zoom Unit"] = "缩放步进",
        ["Zoom Unit (%)"] = "缩放步进（%）",
        ["Progressive Subtitles"] = "渐进式字幕",
        ["Model Directory"] = "模型目录",
        ["Task"] = "任务",
        ["Process Priority"] = "进程优先级",

        // ---- Settings / dialogs (generated by verify/localization/apply_translations.py) ----
        ["(For Japanese-speaking learners of English)"] = "（适用于以日语为母语学习英语的用户）",
        ["[%lw] selected words (lower case)"] = "[%lw] 选中的单词（小写）",
        ["[%w] selected words"] = "[%w] 选中的单词",
        ["Additional arguments to faster-whisper-xxl.exe in string format. For details, execute the --help command from the Copy Help Command below."] = "以字符串形式附加到 faster-whisper-xxl.exe 的参数。详情可通过下方「复制帮助命令」执行 --help 查看。",
        ["as a reference."] = "作为参考。",
        ["Audio Chunk Seconds (sec)"] = "音频块时长（秒）",
        ["Audio Chunk Size (MB)"] = "音频块大小（MB）",
        ["Audio Language"] = "音频语言",
        ["Audios via YoutubeDL are also affected."] = "通过 YoutubeDL 获取的音频同样受影响。",
        ["Auto Language Detection"] = "自动语言检测",
        ["Auto Text Copy"] = "自动复制文本",
        ["Chat Prompt (KeepContext)"] = "对话提示词（KeepContext）",
        ["Chat Prompt (OneByOne)"] = "对话提示词（OneByOne）",
        ["Chinese may have multiple engines for their regions. Please specify what you want to use."] = "中文可能对应多个区域引擎，请指定要使用的引擎。",
        ["Concurrent requests are not allowed."] = "不允许并发请求。",
        ["Context Retain Policy"] = "上下文保留策略",
        ["Dark Title Bar"] = "深色标题栏",
        ["Enabled Cached"] = "启用缓存",
        ["English, Chinese, and other languages may have multiple engines for their regions. Please specify what you want to use."] = "英语、中文等语言可能对应多个区域引擎，请指定要使用的引擎。",
        ["for details."] = "了解详情。",
        ["for more information."] = "了解更多信息。",
        ["Idle Timeout (ms)"] = "空闲超时（毫秒）",
        ["Immediately after reset, context is not taken into account, but the translation result is stable."] = "刚重置时不考虑上下文，但翻译结果更稳定。",
        ["Include Target Language Region"] = "包含目标语言区域",
        ["It always adjusts the number of messages according to the Subtitle Context Count."] = "始终按「字幕上下文条数」调整消息数量。",
        ["It can be linked to the PDIC dictionary tool."] = "可与 PDIC 词典工具联动。",
        ["Max Width"] = "最大宽度",
        ["Open Automatic Subs"] = "打开自动字幕",
        ["OpenAI Reference"] = "OpenAI 参考文档",
        ["Prompt"] = "提示词",
        ["Reset the message count to 0 when Subtitle Context Count is reached."] = "达到「字幕上下文条数」时将消息数重置为 0。",
        ["Search Local"] = "搜索本地",
        ["Separator Opacity"] = "分隔线不透明度",
        ["Show SeekBar only when MouseOver"] = "仅在鼠标悬停时显示进度条",
        ["Some runtimes require the installation of a toolkit or others beforehand. Please check"] = "部分运行时需要预先安装工具包，请查看",
        ["Subtitle Context Count"] = "字幕上下文条数",
        ["Subtitles are sent to the chat one at a time and translated. Translation accuracy will be lower, but it will be more stable. Concurrent requests are possible."] = "字幕逐条发送给对话模型翻译，准确率较低但更稳定，允许并发请求。",
        ["Subtitles via YoutubeDL are also affected."] = "通过 YoutubeDL 获取的字幕同样受影响。",
        ["Target Language"] = "目标语言",
        ["The audio stream is automatically opened in the order of the languages set here."] = "音频流将按此处设置的语言顺序自动打开。",
        ["The previous subtitle and the translation result are sent together to the chat."] = "上一条字幕及其译文会一并发送给对话模型。",
        ["The subtitle stream is automatically opened in the order of the languages set here."] = "字幕流将按此处设置的语言顺序自动打开。",
        ["This increases accuracy because the translation is always context-sensitive, but once an incorrect result is generated, it tends to remain incorrect."] = "翻译始终结合上下文，准确率更高；但一旦出现错误结果，错误往往会延续。",
        ["this link"] = "此链接",
        ["Threads"] = "线程数",
        ["Translate Chat Method"] = "对话翻译方式",
        ["Translate to English"] = "翻译为英文",
        ["Translation accuracy is improved because context is taken into account."] = "因结合上下文，翻译准确率更高。",
        ["Use Manual Model"] = "手动指定模型",
        ["Whisper Engine"] = "Whisper 引擎",
        ["Whisper Model"] = "Whisper 模型",
        ["You can send subtitle text to your browser via the clipboard and work with browser extensions such as Yomitan and 10ten."] = "可通过剪贴板把字幕文本发送到浏览器，配合 Yomitan、10ten 等扩展使用。",
        ["You will need to install a language pack on your OS or install an OCR engine using"] = "需要在操作系统中安装语言包，或使用以下方式安装 OCR 引擎：",

        // ---- Settings / dialogs (generated by verify/localization/apply_translations.py) ----
        ["Player"] = "播放器",

        // ---- Settings / dialogs (generated by verify/localization/apply_translations.py) ----
        ["You can send subtitle text to your browser via the clipboard and work with browser extensions such as Yomitan and 10ten. See"] = "可通过剪贴板把字幕文本发送到浏览器，配合 Yomitan、10ten 等扩展使用。参见",
        ["Media Player .NET Library for WinUI 3/WPF/WinForms (based on FFmpeg/DirectX)"] = "基于 FFmpeg/DirectX 的 WinUI 3 / WPF / WinForms .NET 媒体播放器库",
        ["FFmpeg Bindings for C#/.NET"] = "C#/.NET 的 FFmpeg 绑定",
        ["Dotnet bindings for OpenAI Whisper (whisper.cpp)"] = "OpenAI Whisper（whisper.cpp）的 .NET 绑定",
        ["Faster-Whisper standalone executables"] = "Faster-Whisper 独立可执行程序",
        [".NET wrapper for Tesseract OCR"] = "Tesseract OCR 的 .NET 封装",
        ["Google's Material Design in XAML & WPF"] = "Google Material Design 的 XAML 与 WPF 实现",
        ["Natural language detection library for .NET"] = ".NET 自然语言检测库",
        ["Character set detector build in C#"] = "使用 C# 编写的字符集检测库",
        ["Japanese morphological analyzer on .NET"] = ".NET 日语形态分析器",
        ["A framework for building MVVM application"] = "用于构建 MVVM 应用的框架",
        [".NET bindings for Direct3D11, XAudio, etc."] = "Direct3D11、XAudio 等的 .NET 绑定",
        ["A WPF font and color dialog"] = "WPF 字体与颜色对话框",
    };

    public static string Translate(string? original)
    {
        if (string.IsNullOrEmpty(original))
            return original ?? "";

        if (!IsChinese)
            return original;

        if (Chinese.TryGetValue(original, out string? localized))
            return localized;

        // Text inside a XAML element often carries formatting whitespace
        // (e.g. "<TextBlock>\n  Idle Timeout (ms)\n  <InlineUIContainer/>").
        // Look up the trimmed key but keep the original padding so the layout
        // and the help icon stay intact.
        string trimmed = original.Trim();
        if (trimmed.Length != original.Length && trimmed.Length > 0 &&
            Chinese.TryGetValue(trimmed, out localized))
        {
            int lead = original.Length - original.TrimStart().Length;
            int trail = original.Length - original.TrimEnd().Length;
            string prefix = original[..lead];
            string suffix = trail > 0 ? original[^trail..] : "";
            return prefix + localized + suffix;
        }

        // Numeric / symbol-only entries (delay steps, speed values, rotation
        // angles) and unknown dynamic values are intentionally left untouched.
        return original;
    }

    /// <summary>Localizes a whole format string; keep {n} placeholders intact.</summary>
    public static string Format(string formatKey, params object?[] args)
        => string.Format(Translate(formatKey), args);
}

/// <summary>
/// Runtime localization for LLPlayer's menus and related hints.
///
/// The existing menus are plain literal strings spread over resource dictionaries,
/// data templates and the bottom bar. Instead of replacing every one of them with
/// a binding (which would risk breaking RelativeSource / AncestorLevel bindings),
/// the localizer walks the live menu/control tree, remembers the original English
/// text in an attached property, and rewrites it. Switching back to English simply
/// restores the remembered originals, so repeated switches are idempotent.
/// </summary>
public static class MenuLocalizer
{
    private static readonly List<WeakReference<FrameworkElement>> Tracked = [];

    public static readonly DependencyProperty AutoLocalizeProperty =
        DependencyProperty.RegisterAttached(
            "AutoLocalize", typeof(bool), typeof(MenuLocalizer),
            new PropertyMetadata(false, OnAutoLocalizeChanged));

    public static void SetAutoLocalize(DependencyObject element, bool value) => element.SetValue(AutoLocalizeProperty, value);
    public static bool GetAutoLocalize(DependencyObject element) => (bool)element.GetValue(AutoLocalizeProperty);

    /// <summary>
    /// Kept for compatibility with existing opt-in call sites. Static TextBlock
    /// text is now translated by default (through its Inlines); bound/dynamic
    /// values are skipped automatically.
    /// </summary>
    public static readonly DependencyProperty LocalizeTextProperty =
        DependencyProperty.RegisterAttached(
            "LocalizeText", typeof(bool), typeof(MenuLocalizer),
            new PropertyMetadata(false));

    public static void SetLocalizeText(DependencyObject element, bool value) => element.SetValue(LocalizeTextProperty, value);
    public static bool GetLocalizeText(DependencyObject element) => (bool)element.GetValue(LocalizeTextProperty);

    /// <summary>Explicit opt-out for a dynamic / technical value.</summary>
    public static readonly DependencyProperty SkipProperty =
        DependencyProperty.RegisterAttached(
            "Skip", typeof(bool), typeof(MenuLocalizer),
            new PropertyMetadata(false));

    public static void SetSkip(DependencyObject element, bool value) => element.SetValue(SkipProperty, value);
    public static bool GetSkip(DependencyObject element) => (bool)element.GetValue(SkipProperty);

    private static readonly DependencyProperty OriginalsProperty =
        DependencyProperty.RegisterAttached(
            "Originals", typeof(Dictionary<string, string>), typeof(MenuLocalizer),
            new PropertyMetadata(null));

    private static Dictionary<string, string> GetOriginals(DependencyObject element)
    {
        var map = (Dictionary<string, string>?)element.GetValue(OriginalsProperty);
        if (map == null)
        {
            map = new Dictionary<string, string>(StringComparer.Ordinal);
            element.SetValue(OriginalsProperty, map);
        }

        return map;
    }

    private static void OnAutoLocalizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement fe || e.NewValue is not true)
            return;

        lock (Tracked)
        {
            Tracked.RemoveAll(w => !w.TryGetTarget(out _));
            Tracked.Add(new WeakReference<FrameworkElement>(fe));
        }

        if (d is ContextMenu menu)
        {
            // Items from ItemsSource only exist once the menu has been opened.
            menu.Opened += (_, _) => Localize(menu);
        }
        else
        {
            fe.Loaded += (_, _) => Localize(fe);
        }

        if (fe.IsLoaded || d is ContextMenu)
            Localize(fe);
    }

    /// <summary>Re-applies the current language to every live menu and control.</summary>
    public static void Refresh()
    {
        Application? app = Application.Current;
        if (app == null)
            return;

        if (!app.Dispatcher.CheckAccess())
        {
            app.Dispatcher.Invoke(Refresh);
            return;
        }

        FrameworkElement[] roots;
        lock (Tracked)
        {
            Tracked.RemoveAll(w => !w.TryGetTarget(out _));
            roots = Tracked
                .Select(w => w.TryGetTarget(out FrameworkElement? t) ? t : null)
                .Where(t => t != null)
                .Select(t => t!)
                .ToArray();
        }

        foreach (FrameworkElement root in roots)
            Localize(root);

        foreach (Window window in app.Windows)
            Localize(window);
    }

    public static void Localize(DependencyObject root)
    {
        Localize(root, new HashSet<DependencyObject>(ReferenceEqualityComparer.Instance));
    }

    private static void Localize(DependencyObject node, HashSet<DependencyObject> visited)
    {
        if (!visited.Add(node))
            return;

        if (node is FrameworkElement fe)
            LocalizeElement(fe, visited);

        // Run / Span / Hyperlink / InlineUIContainer are FrameworkContentElement,
        // not FrameworkElement, and are not reliably part of the visual tree.
        if (node is FrameworkContentElement fce)
            LocalizeContentElement(fce, visited);

        // ItemsControl.Items also contains MenuItem objects supplied through a
        // resource ArrayList (MenuSubtitlesStreams / MenuAudioStreams / ...).
        if (node is ItemsControl itemsControl)
        {
            foreach (object item in itemsControl.Items)
            {
                if (item is DependencyObject dep)
                    Localize(dep, visited);
            }
        }

        // Static TextBlock labels. A TextBlock whose Text was set as a property
        // creates an implicit Run bound to that property, so translate Text
        // directly in that case. A TextBlock with real inlines (Run + help icon
        // InlineUIContainer) must NOT have Text reassigned (it would drop the
        // icon and its tooltip); translate each Run instead.
        if (node is TextBlock textBlock &&
            !BindingOperations.IsDataBound(textBlock, TextBlock.TextProperty) &&
            !GetSkip(textBlock))
        {
            bool hasComplexInlines = textBlock.Inlines.Any(i => i is not Run);
            if (hasComplexInlines)
            {
                // Snapshot: assigning Run.Text can rebuild the Inlines collection.
                foreach (Inline inline in textBlock.Inlines.ToArray())
                    Localize(inline, visited);
            }
            else
            {
                Apply(textBlock, "Text", textBlock.Text, v => textBlock.Text = v);
            }
        }

        // DataGridColumn / GridViewColumn are not FrameworkElements.
        if (node is DataGrid dataGrid)
        {
            foreach (DataGridColumn column in dataGrid.Columns)
                LocalizeColumnHeader(column, DataGridColumn.HeaderProperty, visited);
        }

        if (node is ListView listView && listView.View is GridView gridView)
        {
            foreach (GridViewColumn column in gridView.Columns)
                LocalizeColumnHeader(column, GridViewColumn.HeaderProperty, visited);
        }

        // Logical tree (declared MenuItems, resource ArrayList items, ...)
        try
        {
            foreach (object child in LogicalTreeHelper.GetChildren(node))
            {
                if (child is DependencyObject dep)
                    Localize(dep, visited);
            }
        }
        catch
        {
            // Some elements do not expose a logical tree.
        }

        // Visual tree (ItemsSource-generated containers, control templates, ...)
        if (node is Visual visual)
        {
            int count = VisualTreeHelper.GetChildrenCount(visual);
            for (int i = 0; i < count; i++)
            {
                if (VisualTreeHelper.GetChild(visual, i) is DependencyObject dep)
                    Localize(dep, visited);
            }
        }
    }

    private static void LocalizeElement(FrameworkElement fe, HashSet<DependencyObject> visited)
    {
        if (GetSkip(fe))
            return;

        LocalizeHeader(fe, visited);

        // Button / CheckBox / Label / ToggleButton / ToolTip literal content.
        // Skip generated item containers (ComboBoxItem, ListBoxItem, ...) because
        // their content is data (language names, technical values, ...).
        if (fe is ContentControl contentControl &&
            !BindingOperations.IsDataBound(contentControl, ContentControl.ContentProperty) &&
            ItemsControl.ItemsControlFromItemContainer(fe) is null &&
            contentControl.Content is string content)
        {
            Apply(fe, "Content", content, v => contentControl.Content = v);
        }

        LocalizeToolTip(fe, visited);

        if (fe.ContextMenu is ContextMenu contextMenu)
            Localize(contextMenu, visited);
    }

    private static void LocalizeContentElement(FrameworkContentElement fce, HashSet<DependencyObject> visited)
    {
        if (GetSkip(fce))
            return;

        if (fce is Run run)
        {
            // A Run generated for TextBlock.Text is handled here; an InlineUIContainer
            // sibling must survive, which is why TextBlock.Text is never assigned.
            if (!BindingOperations.IsDataBound(run, Run.TextProperty))
                Apply(run, "RunText", run.Text, v => run.Text = v);
        }
        else if (fce is Span span)
        {
            foreach (Inline child in span.Inlines.ToArray())
                Localize(child, visited);
        }
        else if (fce is InlineUIContainer container && container.Child is DependencyObject childElement)
        {
            Localize(childElement, visited);
        }

        LocalizeToolTip(fce, visited);
    }

    /// <summary>
    /// FrameworkElement.ToolTipProperty is an AddOwner of ToolTipService.ToolTipProperty,
    /// so a single handler covers string tooltips, ToolTip objects and the
    /// &lt;ToolTipService.ToolTip&gt; property elements used by the settings pages.
    /// </summary>
    private static void LocalizeToolTip(DependencyObject node, HashSet<DependencyObject> visited)
    {
        if (BindingOperations.IsDataBound(node, ToolTipService.ToolTipProperty))
            return;

        object? tip = ToolTipService.GetToolTip(node);
        if (tip is string text)
            Apply(node, "ToolTip", text, v => ToolTipService.SetToolTip(node, v));
        else if (tip is DependencyObject tipObject)
            Localize(tipObject, visited);
    }

    private static void LocalizeHeader(FrameworkElement fe, HashSet<DependencyObject> visited)
    {
        object? header;
        DependencyProperty headerProperty;
        Action<string> setHeader;
        DependencyProperty formatProperty;
        Func<string?> getFormat;
        Action<string> setFormat;

        if (fe is HeaderedItemsControl hic)
        {
            header = hic.Header;
            headerProperty = HeaderedItemsControl.HeaderProperty;
            setHeader = v => hic.Header = v;
            formatProperty = HeaderedItemsControl.HeaderStringFormatProperty;
            getFormat = () => hic.HeaderStringFormat;
            setFormat = v => hic.HeaderStringFormat = v;
        }
        else if (fe is HeaderedContentControl hcc)
        {
            header = hcc.Header;
            headerProperty = HeaderedContentControl.HeaderProperty;
            setHeader = v => hcc.Header = v;
            formatProperty = HeaderedContentControl.HeaderStringFormatProperty;
            getFormat = () => hcc.HeaderStringFormat;
            setFormat = v => hcc.HeaderStringFormat = v;
        }
        else
        {
            return;
        }

        if (!BindingOperations.IsDataBound(fe, headerProperty))
        {
            if (header is string text)
                Apply(fe, "Header", text, setHeader);
            else if (header is DependencyObject headerObject)
                Localize(headerObject, visited);
        }

        // Whole format string is the key ("ASR ({0})", "Delay 1st ({0})", ...).
        if (!BindingOperations.IsDataBound(fe, formatProperty))
            Apply(fe, "HeaderStringFormat", getFormat(), setFormat);
    }

    private static void LocalizeColumnHeader(
        DependencyObject column,
        DependencyProperty headerProperty,
        HashSet<DependencyObject> visited)
    {
        if (BindingOperations.IsDataBound(column, headerProperty))
            return;

        object? header = column.GetValue(headerProperty);
        if (header is string text)
            Apply(column, "ColumnHeader", text, v => column.SetValue(headerProperty, v));
        else if (header is DependencyObject headerObject)
            Localize(headerObject, visited);
    }

    private static readonly DependencyProperty AppliedProperty =
        DependencyProperty.RegisterAttached(
            "Applied", typeof(Dictionary<string, string>), typeof(MenuLocalizer),
            new PropertyMetadata(null));

    private static Dictionary<string, string> GetApplied(DependencyObject element)
    {
        var map = (Dictionary<string, string>?)element.GetValue(AppliedProperty);
        if (map == null)
        {
            map = new Dictionary<string, string>(StringComparer.Ordinal);
            element.SetValue(AppliedProperty, map);
        }

        return map;
    }

    private static void Apply(DependencyObject element, string key, string? value, Action<string> setter)
    {
        if (value == null)
            return;

        Dictionary<string, string> originals = GetOriginals(element);
        Dictionary<string, string> applied = GetApplied(element);

        if (!originals.TryGetValue(key, out string? original))
        {
            original = value;
            originals[key] = original;
        }
        else if (applied.TryGetValue(key, out string? last) &&
                 !string.Equals(value, original, StringComparison.Ordinal) &&
                 !string.Equals(value, last, StringComparison.Ordinal))
        {
            // The value was changed externally (e.g. a converter refreshed it).
            // Re-baseline instead of overwriting it with a stale translation.
            original = value;
            originals[key] = original;
        }

        string wanted = MenuStrings.Translate(original);
        if (!string.Equals(value, wanted, StringComparison.Ordinal))
            setter(wanted);

        applied[key] = wanted;
    }
}
