[README.md](https://github.com/user-attachments/files/34482765/README.md)
<p align="center"><img height="96" src="./LLPlayer.png"></p>

<h1 align="center">LLPlayer-ChickenRice 0.3.5</h1>

<h3 align="center">边播放边翻译的播放器 · 日语视频一键出中文字幕</h3>

<p align="center">
在 <a href="https://github.com/umlx5h/LLPlayer">LLPlayer</a> 基础上集成了
<a href="https://github.com/TransWithAI/Faster-Whisper-TransWithAI-ChickenRice">TransWithAI ChickenRice</a>
日译中引擎，并把界面完整中文化。
</p>

<p align="center">
<a href="https://github.com/skychenchang/LLPlayer-ChickenRice/releases">下载发布版</a> ·
<a href="./README.en.md">English README</a> ·
<a href="#五chickenrice-日语音频转简体中文字幕重点">ChickenRice 使用指南</a>
</p>

![主界面](./LLPlayer-screenshot.jpg)

---

## 目录

- [一、软件介绍](#一软件介绍)
- [二、核心功能一览](#二核心功能一览)
- [三、系统要求](#三系统要求)
- [四、快速上手](#四快速上手)
- [五、ChickenRice 日语音频转简体中文字幕（重点）](#五chickenrice-日语音频转简体中文字幕重点)
  - [5.1 这项功能能做什么](#51-这项功能能做什么)
  - [5.2 输入与输出格式](#52-输入与输出格式)
  - [5.3 工作原理](#53-工作原理)
  - [5.4 一次性准备（首次使用）](#54-一次性准备首次使用)
  - [5.5 操作步骤](#55-操作步骤)
  - [5.6 参数说明](#56-参数说明)
  - [5.7 注意事项](#57-注意事项)
  - [5.8 故障排查](#58-故障排查)
- [六、其他常用功能](#六其他常用功能)
  - [检查更新（0.3.5 新增）](#检查更新035-新增)
- [七、界面语言](#七界面语言)
- [八、从源码构建](#八从源码构建)
- [九、目录结构](#九目录结构)
- [十、常见问题](#十常见问题)
- [十一、致谢与许可](#十一致谢与许可)

---

## 一、软件介绍

**LLPlayer-ChickenRice** 是一个面向语言学习者的本地视频播放器。它在普通播放器的「播放」之上，加了一整套**字幕流水线**：生成字幕、翻译字幕、双字幕对照、点词查义、导出字幕。

本项目相对上游 [LLPlayer](https://github.com/umlx5h/LLPlayer) 有两点主要变化：

1. **新增 ChickenRice 日译中引擎**：针对「日语视频 → 简体中文字幕」这一场景，直接调用 TransWithAI 微调模型，**一次生成中文成品字幕**，不需要「先日文 ASR、再机器翻译」的两段式流程，也不消耗任何在线翻译额度。
2. **界面完整中文化**：右键菜单、底部控制栏、全部设置页与设置窗口标题均为中文，默认语言即中文，并可在设置中一键切回 English。

适合谁用：

- 看日语生肉视频（动画、日剧、访谈、网课、会议录像）想直接得到中文字幕的人；
- 需要把日语视频整理成 SRT 字幕文件的人；
- 想对照「原文 + 译文」学习语言的人。

---

## 二、核心功能一览

| 功能 | 说明 |
|---|---|
| **ChickenRice 日译中** | 整文件离线处理，日语语音直接输出简体中文字幕；播放启动即可看到已生成字幕，其余随播放进度出现（[详见第五章](#五chickenrice-日语音频转简体中文字幕重点)） |
| **检查更新（0.3.5 新增）** | `Ctrl+,` → 关于 → `检查更新`，显示当前版本与线上最新正式发布版本，四种结果可区分（[详见第六章](#检查更新035-新增)） |
| 实时语音识别 | `whisper.cpp` / `faster-whisper` 两套引擎，按音频块流式出字幕，支持直播与在线视频 |
| 字幕翻译 | Google / DeepL / OpenAI / Claude / Ollama / LM Studio 等多种翻译后端，可把任意语言字幕译为中文 |
| 双字幕对照 | 主字幕 + 副字幕同时显示，可一份原文一份译文，可设置分隔线与位置 |
| 位图字幕 OCR | 内嵌图形字幕（PGS/VobSub）实时转文本 |
| 点词查义 | 点击字幕中的单词弹出释义，可对接 Yomitan、10ten、PDIC 等外部词典 |
| 在线视频 | 内置 yt-dlp 插件，可直接粘贴 URL 播放网络视频 |
| 字幕导出 | 把生成/翻译结果导出为 SRT（UTF-8，可选 BOM） |

---

## 三、系统要求

| 项 | 要求 |
|---|---|
| 操作系统 | Windows 10 x64 1903+ / Windows 11 |
| 运行时 | .NET 10 Desktop Runtime（发布包内已附带所需程序集，仍建议系统安装） |
| 运行库 | Microsoft Visual C++ Redistributable 2022+ |
| 显卡（推荐） | NVIDIA 显卡 + 驱动；ChickenRice 默认 `cuda` |
| 磁盘 | 播放器本体约 500 MB；ChickenRice 引擎 + 模型另需约 3.5 GB |
| 无 N 卡 | 可用 ChickenRice 的 CPU 版（速度明显慢），或改用 `faster-whisper` |

---

## 四、快速上手

1. **解压即用**：把发布包解压到任意目录（路径不要含特殊字符），双击 `LLPlayer.exe`。
2. **打开视频**：把视频拖进窗口，或右键 → `打开文件`（`Ctrl+O`），也可 `粘贴网址` 播放在线视频。
3. **打开设置**：`Ctrl+,` 或点右下角齿轮（`打开设置`）。左侧分类为 播放器 / 视频 / 音频 / 字幕 / 快捷键 / 偏移 / 鼠标 / 主题 / 插件 / 关于。
4. **想要中文字幕**：按[第五章](#五chickenrice-日语音频转简体中文字幕重点)配置 ChickenRice 即可；想要实时字幕则用 `faster-whisper`。

> 界面默认就是中文。若显示为英文，见[第七章](#七界面语言)切换。

---

## 五、ChickenRice 日语音频转简体中文字幕（重点）

### 5.1 这项功能能做什么

给一段**日语语音**的本地视频/音频，播放器会调用本机的 ChickenRice 引擎（TransWithAI 基于 Whisper large-v2 微调的日译中模型），**整段处理**后产出**简体中文字幕**，并直接挂到播放器字幕轨上：

- **不需要**先出日文字幕再翻译：模型本身做的是「日语语音 → 中文文本」，一步到位，译文比「ASR + 机翻」更连贯，专有名词与人称更稳；
- **完全离线**：不联网、不调用任何翻译 API，不产生费用，也不会上传你的视频；
- **边处理边看**：不必等整部片子跑完——播放一开始就能看到已经生成的部分，后续字幕随播放进度陆续出现；
- **可导出**：处理完可一键导出 SRT，用于剪辑、压制或做字幕文件。

**适用场景**：日语动画 / 日剧 / 电影 / 访谈 / 网课 / 会议录像 / 播客（音频文件同样支持）。

**不适用场景**：

| 场景 | 建议 |
|---|---|
| 直播流、在线视频 URL | 改用 `faster-whisper`（实时流式） |
| 非日语语音（英语、韩语等） | 改用 `faster-whisper` + 字幕翻译 |
| 需要「日文原文 + 中文」双语对照 | 主字幕用 ChickenRice 出中文，副字幕另挂日文字幕 |
| 只有集成显卡 / 显存 < 4 GB | 用 CPU 版引擎，或在设置里把计算精度改为 `int8_float16` |

### 5.2 输入与输出格式

#### 输入

| 项 | 支持情况 |
|---|---|
| 载体 | **本地文件路径**（拖入、打开文件、播放列表均可）。在线 URL 会被拒绝并提示改用实时引擎 |
| 容器 | `mp4` / `mkv` / `mov` / `avi` / `ts` / `m2ts` / `webm` / `flv` / `wmv` 等（由内置 FFmpeg 决定） |
| 音频编码 | AAC / AC3 / EAC3 / DTS / Opus / MP3 / FLAC / PCM 等，采样率与声道数不限（引擎内部按 Whisper 要求重采样，无需手动转码） |
| 纯音频 | `mp3` / `wav` / `flac` / `m4a` / `aac` / `ogg` 可直接当媒体打开并生成字幕 |
| 语音语言 | **日语**效果最佳（模型为日→中微调）；`任务` 改成 `transcribe` 并换用转录模型才输出日文原文 |
| 视频规格 | 分辨率/帧率不限；4K 也能处理，但解码与推理更吃资源 |

#### 输出

| 项 | 说明 |
|---|---|
| 播放器字幕轨 | 简体中文文本字幕，作为**主字幕**挂在当前播放项上；可与副字幕叠加显示 |
| 时间轴 | 与音频对齐（`mm:ss.cc --> mm:ss.cc`），处理结束后用上游合并/去重/校正后的最终结果替换预览 |
| 字幕导出 | 字幕菜单 → `字幕导出`，可存为 SRT（默认 UTF-8，可勾选 `导出带 BOM 的 UTF-8`） |
| 中间产物 | 运行期在系统临时目录生成 `GUID` 命名的输出文件夹（内含 SRT），**结束或取消后自动清理** |
| 实时通道 | 引擎以 `--log_level DEBUG` 逐条输出 `[00:12.34 --> 00:15.67] 中文文本`，播放器解析后按进度释放（这也是「边处理边显示」的数据来源） |

### 5.3 工作原理

```
播放开始（不阻塞播放，后台启动一次引擎进程）
   │
   ├─ 引擎逐段输出 DEBUG 行 ──► 解析 ──► 按「播放进度 + 提前量」释放 ──► 挂到字幕轨
   ├─ 增量读取 SRT 文件（若引擎边写边落盘）──────────────────────────┘
   │
   └─ 进程结束 ──► 读取最终 SRT（合并 / 去重 / 校正时间轴）
                 ──► 原子替换预览字幕，并重新对齐当前播放位置
```

三点关键设计：

1. **整文件处理，不切块**：ChickenRice 是离线批处理程序（自带 VAD 切分与字幕合并），一次加载模型处理整个文件。相比「按音频块反复调起进程」，不会反复加载 3 GB 模型。
2. **渐进式释放**：`ChickenRiceProgressiveBuffer` 按时间戳排序缓存结果，只有当播放进度进入「当前位置 + 提前量」窗口时才释放，保证字幕始终有序，拖动进度条也不会乱序。
3. **最终结果覆盖预览**：预览字幕用于「先看着」，引擎退出后用权威 SRT 整体替换，避免重复条目与微小时间偏差。

### 5.4 一次性准备（首次使用）

> 引擎与模型**不随发布包提供**（体积与授权原因），需要手动放置一次。放好后长期有效。

**第 1 步：下载 ChickenRice 引擎（翻译版）**

到 [TransWithAI Release 页](https://github.com/TransWithAI/Faster-Whisper-TransWithAI-ChickenRice/releases) 下载与你的显卡匹配的 **`-translate` 翻译版**（文件名通常含 `translate`）。

> 也可以在播放器里直接打开该页面：`Ctrl+,` → 字幕 → 语音识别 → 选中 `ChickenRice (JA > ZH, whole file)` 后，引擎选择框右侧会出现与 `faster-whisper` 同款的 **`下载引擎`** 按钮，点击即跳转；按钮图标按 `Whisper\ChickenRice\infer.exe` 是否存在显示「已下载 / 未下载」。

- RTX 50 系 → CUDA 12.8 版
- 其他 NVIDIA 显卡 → 对应 CUDA 版本
- 无 N 卡 → CPU 版

解压后应包含：`infer.exe`、`_internal\`、VAD 模型（`whisper_vad.onnx`）、`generation_config.json5`、`locales\`。

**第 2 步：放到约定位置**

把上述文件整体放到播放器目录下的：

```
<播放器目录>\Whisper\ChickenRice\
    ├─ infer.exe
    ├─ _internal\
    ├─ generation_config.json5
    ├─ locales\
    └─ models\
```

**第 3 步：准备模型（约 3.09 GB）**

- 若下载的是**翻译版（内置模型）**：跳过此步；
- 若是**无主模型版**：另下载 CTranslate2 模型 `chickenrice0721/whisper-large-v2-translate-zh-v0.2-st-ct2`（7 个文件，约 3.09 GB），把文件**平铺**放进 `models\`（不要多套一层目录）。

国内下载可用镜像：

```bat
set HF_ENDPOINT=https://hf-mirror.com
hf download chickenrice0721/whisper-large-v2-translate-zh-v0.2-st-ct2 --local-dir models
```

**第 4 步：确认目录结构**

```
Whisper\ChickenRice\models\
    ├─ config.json
    ├─ model.bin
    ├─ vocabulary.json
    ├─ tokenizer.json
    └─ ...
```

### 5.5 操作步骤

1. **打开设置**：`Ctrl+,`（或右下角齿轮）。
2. 左侧进入 **字幕 → 语音识别**。
3. 顶部 **`Whisper 引擎`** 选择 **`ChickenRice (JA > ZH, whole file)`**。
4. 切到下方的 **`ChickenRice (JA > ZH)`** 页签，逐项确认：

   | 设置项（界面中文） | 建议值 | 说明 |
   |---|---|---|
   | `引擎路径` | 默认 `Whisper\ChickenRice\infer.exe` | 放在别处时打开 `手动指定引擎` 再填绝对路径 |
   | `模型目录` | `models` | 相对引擎目录；也可填绝对路径 |
   | `设备` | `cuda` | 无 N 卡改 `cpu` |
   | `计算精度` | `auto` | 4 GB 显存建议 `int8_float16` |
   | `任务` | `translate` | 日→中必须用 `translate`；改 `transcribe` 需换转录模型 |
   | `渐进式字幕` | 开（默认） | 关闭则等整份 SRT 完成后一次性显示 |
   | `进度提前量（秒）` | `30` | 允许字幕超前播放位置多少秒释放 |
   | `超时（分钟）` | `60` | 超时后任务中止并提示 |
   | `附加参数` | 留空 | 原样追加给引擎，位置参数（媒体路径）始终排在最后 |

5. 点 **`检查引擎`**，确认引擎文件、模型、生成配置与 VAD 文件齐全（缺什么会直接提示）。
6. **关闭自动翻译**：`字幕 → 自动翻译` 取消勾选（或 `Ctrl+,` → 字幕 → 翻译 中关闭）。**这一步必须做**，否则中文会被再翻译一次。
7. **确认是本地文件**（不是 URL），然后开始播放。
8. 打开字幕菜单二选一：
   - 点底栏 **CC 按钮（`主字幕`）** → 勾选 **`语音识别 (...)`**；
   - 或右键 → `字幕` → `语音识别 (...)`。
9. 观察画面：已生成的字幕会**立刻出现**，其余随播放进度陆续出现。
10. **需要文件时再导出**：右键 → `字幕` → `字幕导出`，保存为 SRT。

> 想中途停止？再次点开字幕菜单，取消勾选 `语音识别 (...)` 即可；后台进程会被终止、临时目录被清理，已显示的字幕保留。

### 5.6 参数说明

| 参数 | 默认 | 取值 / 影响 |
|---|---|---|
| `设备` | `cuda` | `cpu` 兼容性最好但慢数倍；`cuda` 需要 NVIDIA 驱动与 CUDA 运行库 |
| `计算精度` | `auto` | `int8_float16` 省显存（约 2-3 GB 可跑）；`float16` 更准更吃显存；`int8` 更快略降质 |
| `任务` | `translate` | `translate` = 日语语音直接出中文；`transcribe` = 输出原文（需配转录模型） |
| `模型目录` | `models` | 必须是含 `model.bin` 与 `config.json` 的目录 |
| `渐进式字幕` | 开 | 关 = 等整份完成再显示（旧行为） |
| `进度提前量（秒）` | `30` | 越大越「提前」出字幕，但跳转回退时可能短暂看到后文 |
| `超时（分钟）` | `60` | 长片建议调大（如 180） |
| `附加参数` | 空 | 例如 `--beam_size 5`；**不要**覆盖 `--log_level`，否则会失去渐进式能力 |

### 5.7 注意事项

1. **必须关闭自动翻译**。ChickenRice 输出的已经是中文；开着自动翻译会把中文再翻成目标语言，出现「中文被翻成英文/日文」的怪现象。
2. **只支持本地文件**。直播、`http(s)`/`m3u8` 在线地址会被拒绝并提示，此时请改用 `faster-whisper`。
3. **首次启动较慢**。第一次推理要加载约 3 GB 模型，视磁盘与显存可能需要十几秒到一分钟；期间播放不受影响，只是字幕出现得晚一些。
4. **渐进式依赖引擎日志**。若引擎版本不输出 DEBUG 段日志、或你用 `附加参数` 覆盖了 `--log_level`，会退化为「处理结束后一次性显示」——仍能出字幕，只是没有「边看边出」。
5. **中途跳转**。跳转后字幕按时间戳匹配，不会错位；但若跳到尚未处理到的位置，需要等后台追上。
6. **显卡与显存**。显存不足会直接报引擎错误：把 `计算精度` 改为 `int8_float16`，或把 `设备` 改为 `cpu`。
7. **磁盘空间**。模型约 3.09 GB，运行期临时目录还会短暂占用与输出 SRT 相当的体积。
8. **单文件串行**。同一时刻只处理一个文件的字幕任务；切歌/换片时会自动取消上一个任务。
9. **不要移动引擎目录**。移动后请同步更新 `引擎路径`（或用 `手动指定引擎` 填新路径），否则 `检查引擎` 会失败。
10. **授权**。ChickenRice 引擎与模型遵循其上游许可（见 [TransWithAI 仓库](https://github.com/TransWithAI/Faster-Whisper-TransWithAI-ChickenRice)），商用前请自行确认。

### 5.8 故障排查

| 现象 | 原因 | 处理 |
|---|---|---|
| 点 `检查引擎` 报缺少文件 | 引擎/模型/VAD 未放齐 | 对照 5.4 的目录结构补齐；确认 `models\` 里是文件而不是又一层文件夹 |
| 提示「无法处理在线流」 | 打开的是 URL | 改成本地文件，或切换到 `faster-whisper` |
| 字幕一直是英文/日文 | 自动翻译仍开着，或 `任务` 不是 `translate` | 关闭 `自动翻译`；把 `任务` 改回 `translate` |
| 有进度但没有字幕 | 引擎未输出 DEBUG 行（日志被覆盖） | 清空 `附加参数`；或关闭再打开 `渐进式字幕` |
| 报 CUDA / 显存错误 | 驱动或 CUDA 版本不匹配、显存不足 | 换对应 CUDA 版本的引擎；精度改 `int8_float16`；或 `设备=cpu` |
| 进程超时 | 片长超过 `超时（分钟）` | 调大超时后重试 |
| 中文夹杂日文汉字 | 模型输出特性 | 属模型能力边界；可改用 `faster-whisper` + 翻译引擎对比 |
| 字幕重复出现两遍 | 预览与最终 SRT 交替显示 | 正常现象，处理结束后会自动去重替换；若持续存在请反馈 |

---

## 六、其他常用功能

| 操作 | 入口 |
|---|---|
| 打开文件 / 粘贴网址 | 右键菜单；`Ctrl+O` |
| 主 / 副字幕切换 | 底栏两个 **CC** 按钮（`主字幕` / `副字幕`） |
| 音轨 / 视频轨切换 | 底栏音符按钮（`音频流`）、摄像机按钮（`视频流`） |
| 字幕位置与大小 | `Ctrl+,` → 字幕 → 位置 / 大小 |
| 双字幕分隔线、防剧透遮罩 | `Ctrl+,` → 字幕 |
| 字幕侧栏（原文/译文对照列表） | 底栏侧栏按钮（`显示 / 隐藏字幕侧栏`） |
| 点词查义 | 点击字幕中的单词弹出释义；对接方式见 字幕 → 取词动作 |
| 字幕导出 / 下载 | 右键 → `字幕` → `字幕导出` / `字幕下载` |
| 快捷键一览 | 右键 → `快捷键说明` |
| 显示调试信息（帧率、丢帧等） | 右键 → `显示调试信息` |
| 检查软件更新 | `Ctrl+,` → 关于 → `检查更新` |

### 检查更新（0.3.5 新增）

入口在 `Ctrl+,` → **关于**，与版本号显示在同一区域。当前版本与「关于」页共用 `App.Version`（0.3.5），依据
`https://api.github.com/repos/skychenchang/LLPlayer-ChickenRice/releases/latest` 列出的**最新正式发布版本**比较，
结果直接显示在关于页，四种状态清晰可区分：

| 状态 | 页面显示 |
|---|---|
| 发现新版本 | `当前版本：0.3.5 \| 最新版本：x.y.z` + `发现新版本，可打开下载页获取。`，并出现 **`打开下载页`** 按钮 |
| 无更新 | `当前版本：… \| 最新版本：…` + `当前已是最新版本。` |
| 暂无已发布版本 | `当前版本：…` + `暂无已发布版本。` |
| 检查失败 | `当前版本：…` + `检查更新失败，请检查网络后重试。` |

`打开下载页` 始终指向 `https://github.com/skychenchang/LLPlayer-ChickenRice/releases`。请求异步执行、12 秒超时、
带 `User-Agent`，检查期间按钮禁用，关闭页面自动取消；不影响播放状态与配置。

---

## 七、界面语言

- 默认语言：**中文**。
- 切换位置：`Ctrl+,` → **播放器 → 界面 → 界面语言**，可选 `English` / `中文`。
- **选中即生效并立即保存**（写入 `LLPlayer.Config.json` 的 `MenuUILanguage`），不需要点「保存并关闭」。
- 右键菜单中**不再有**语言子菜单，避免与设置项重复。
- 切换只影响界面文案，不影响字幕、翻译、快捷键等其他配置。

---

## 八、从源码构建

环境：.NET SDK 10 + Windows。

```bash
git clone https://github.com/skychenchang/LLPlayer-ChickenRice.git
cd LLPlayer-ChickenRice

dotnet publish LLPlayer/LLPlayer.csproj -c Release -o ./publish \
  -p:SourceRevisionId=local-build

# FFmpeg 运行库需要随包拷贝
cp -r FFmpeg ./publish/FFmpeg
```

> **建议带 `-p:SourceRevisionId=...`**：程序启动时从 `ProductVersion` 读取版本，发布版形如 `0.3.5+chickenrice-integration`。
> 本地不带该参数仍可启动（0.3.5 已加入容错），只是「关于」页的提交哈希为空。参数值可任意（如 `local-build`）。

运行测试：

```bash
dotnet test FlyleafLibTests/FlyleafLibTests.csproj -c Release
```

---

## 九、目录结构

```
LLPlayer-ChickenRice/
├─ LLPlayer/                 # 播放器主程序（WPF）
│  ├─ Controls/Settings/     # 全部设置页
│  ├─ Services/              # 配置、动作、界面本地化
│  └─ ViewModels/ Views/     # MVVM 层
├─ FlyleafLib/               # 播放内核（含 ChickenRice 集成与字幕引擎）
│  └─ MediaPlayer/
│     ├─ ChickenRiceASRService.cs        # 整文件调用 + DEBUG 行/SRT 双通道解析
│     └─ ChickenRiceProgressiveBuffer.cs # 按播放进度释放字幕
├─ FlyleafLibTests/          # 单元与集成测试
├─ Plugins/YoutubeDL/        # 在线视频插件
├─ FFmpeg/                   # FFmpeg 7 个运行库
├─ Whisper/ChickenRice/      # 【需自行放置】ChickenRice 引擎与模型
└─ LLPlayer.slnx
```

---

## 十、常见问题

**Q：需要联网吗？**
运行 ChickenRice 不需要。只有在线视频（yt-dlp）、在线翻译引擎、下载模型时才需要网络。

**Q：能用来给直播做实时字幕吗？**
不能。ChickenRice 是整文件离线处理。实时场景请用 `faster-whisper`。

**Q：字幕能导出成 SRT 给别人用吗？**
可以。右键 → `字幕` → `字幕导出`，或等处理完成后在导出对话框选择 UTF-8（可按需带 BOM）。

**Q：能同时显示日中对照吗？**
可以。主字幕用 ChickenRice 出中文；副字幕另挂一份日文字幕（或用 `faster-whisper` 生成日文）。两份字幕可在 字幕 → 位置 / 大小 里分别定位。

**Q：视频很长，处理到一半我关了播放器怎么办？**
任务会随进程结束而终止，临时文件自动清理；下次播放会重新处理。

**Q：界面变成英文了怎么改回中文？**
`Ctrl+,` → 播放器 → 界面 → 界面语言 → `中文`。

**Q：为什么关于页显示的仓库地址是 skychenchang/LLPlayer-ChickenRice？**
本项目是上游 LLPlayer 的衍生版本，代码与问题反馈都在本仓库；上游 wiki 链接仍指向原项目以保证文档可用。

---

## 十一、致谢与许可

本项目的界面、播放与字幕框架来自 [umlx5h/LLPlayer](https://github.com/umlx5h/LLPlayer)；日译中能力来自
[TransWithAI/Faster-Whisper-TransWithAI-ChickenRice](https://github.com/TransWithAI/Faster-Whisper-TransWithAI-ChickenRice)；
播放内核为 [SuRGeoNix/Flyleaf](https://github.com/SuRGeoNix/Flyleaf)。

其他依赖与致谢详见 [README.en.md](./README.en.md) 的 Special Thanks 一节。

许可证：[GPL-3.0](./LICENSE)。使用 ChickenRice 引擎与模型时，请一并遵守其上游许可。
