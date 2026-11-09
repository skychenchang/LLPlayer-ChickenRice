using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using LLPlayer.Extensions;
using LLPlayer.Services;

namespace LLPlayer.Controls.Settings;

public partial class SettingsAbout : UserControl
{
    public SettingsAbout()
    {
        InitializeComponent();

        DataContext = ((App)Application.Current).Container.Resolve<SettingsAboutVM>();

        // A pending network request must not call back into an unloaded view.
        Unloaded += (_, _) => ((SettingsAboutVM)DataContext).CancelPendingCheck();
    }
}

public class SettingsAboutVM : Bindable
{
    public SettingsAboutVM()
    {
        Libraries =
        [
            new LibraryInfo
            {
                Name = "SuRGeoNix/Flyleaf",
                Description = "Media Player .NET Library for WinUI 3/WPF/WinForms (based on FFmpeg/DirectX)",
                Url = "https://github.com/SuRGeoNix/Flyleaf"
            },

            new LibraryInfo
            {
                Name = "SuRGeoNix/Flyleaf.FFmpeg",
                Description = "FFmpeg Bindings for C#/.NET",
                Url = "https://github.com/SuRGeoNix/Flyleaf.FFmpeg"
            },

            new LibraryInfo
            {
                Name = "sandrohanea/whisper.net",
                Description = "Dotnet bindings for OpenAI Whisper (whisper.cpp)",
                Url = "https://github.com/sandrohanea/whisper.net"
            },

            new LibraryInfo
            {
                Name = "Purfview/whisper-standalone-win",
                Description = "Faster-Whisper standalone executables",
                Url = "https://github.com/Purfview/whisper-standalone-win"
            },

            new LibraryInfo
            {
                Name = "Sicos1977/TesseractOCR",
                Description = ".NET wrapper for Tesseract OCR",
                Url = "https://github.com/Sicos1977/TesseractOCR"
            },

            new LibraryInfo
            {
                Name = "MaterialDesignInXamlToolkit ",
                Description = "Google's Material Design in XAML & WPF",
                Url = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit"
            },

            new LibraryInfo
            {
                Name = "searchpioneer/lingua-dotnet",
                Description = "Natural language detection library for .NET",
                Url = "https://github.com/searchpioneer/lingua-dotnet"
            },

            new LibraryInfo
            {
                Name = "CharsetDetector/UTF-unknowns",
                Description = "Character set detector build in C#",
                Url = "https://github.com/CharsetDetector/UTF-unknown"
            },

            new LibraryInfo
            {
                Name = "komutan/NMeCab",
                Description = "Japanese morphological analyzer on .NET",
                Url = "https://github.com/komutan/NMeCab"
            },

            new LibraryInfo
            {
                Name = "PrismLibrary/Prism",
                Description = "A framework for building MVVM application",
                Url = "https://github.com/PrismLibrary/Prism"
            },

            new LibraryInfo
            {
                Name = "amerkoleci/Vortice.Windows",
                Description = ".NET bindings for Direct3D11, XAudio, etc.",
                Url = "https://github.com/amerkoleci/Vortice.Windows"
            },

            new LibraryInfo
            {
                Name = "sskodje/WpfColorFont",
                Description = " A WPF font and color dialog",
                Url = "https://github.com/sskodje/WpfColorFont"
            },
        ];

        CmdCheckUpdate!.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CmdCheckUpdate.IsExecuting))
                OnPropertyChanged(nameof(CanCheckUpdate));
        };

        MenuStrings.LanguageChanged += () =>
        {
            foreach (LibraryInfo library in Libraries)
                library.RaiseLocalizedDescription();

            // Bound text is not rewritten by AutoLocalize; recompute it here.
            ApplyResult();
        };
    }
    public ObservableCollection<LibraryInfo> Libraries { get; }

    #region Software update

    private UpdateCheckResult? _lastResult;
    private CancellationTokenSource? _checkCts;

    public string UpdateStatusText { get; private set => Set(ref field, value); } = "";
    public string VersionSummaryText { get; private set => Set(ref field, value); } = "";
    public bool HasUpdate { get; private set => Set(ref field, value); }

    public bool CanCheckUpdate => !CmdCheckUpdate.IsExecuting;

    public AsyncDelegateCommand CmdCheckUpdate => field ??= new AsyncDelegateCommand(async () =>
    {
        _checkCts?.Cancel();
        _checkCts = new CancellationTokenSource();
        CancellationToken token = _checkCts.Token;

        HasUpdate = false;
        VersionSummaryText = "";
        UpdateStatusText = MenuStrings.Translate("Checking for updates...");

        try
        {
            _lastResult = await UpdateChecker.CheckAsync(token);
        }
        catch (OperationCanceledException)
        {
            UpdateStatusText = MenuStrings.Translate("Update check canceled.");
            return;
        }
        finally
        {
            _checkCts?.Dispose();
            _checkCts = null;
        }

        ApplyResult();
    }).ObservesCanExecute(() => CanCheckUpdate);

    public DelegateCommand CmdOpenReleases => field ??= new(() =>
    {
        string url = _lastResult?.ReleaseUrl ?? UpdateUrls.ReleasesPage;
        try
        {
            HyperlinkHelper.OpenUrlInBrowser(url);
        }
        catch (Exception)
        {
            UpdateStatusText = MenuStrings.Translate("Cannot open the download page.");
        }
    });

    public void CancelPendingCheck() => _checkCts?.Cancel();

    private void ApplyResult()
    {
        if (_lastResult is null)
            return;

        UpdateCheckResult result = _lastResult;
        HasUpdate = result.Kind == UpdateCheckKind.UpdateAvailable;
        string current = App.Version;

        switch (result.Kind)
        {
            case UpdateCheckKind.UpdateAvailable:
                VersionSummaryText = MenuStrings.Format(
                    "Current version: {0} | Latest version: {1}", current,
                    result.LatestVersion ?? result.RawTag ?? "-");
                UpdateStatusText = MenuStrings.Translate(
                    "A new version is available. Open the download page to get it.");
                break;

            case UpdateCheckKind.UpToDate:
                VersionSummaryText = MenuStrings.Format(
                    "Current version: {0} | Latest version: {1}", current,
                    result.LatestVersion ?? result.RawTag ?? "-");
                UpdateStatusText = MenuStrings.Translate("You are already on the latest version.");
                break;

            case UpdateCheckKind.NoPublishedRelease:
                VersionSummaryText = MenuStrings.Format("Current version: {0}", current);
                UpdateStatusText = MenuStrings.Translate("No published release is available yet.");
                break;

            default: // Failed
                VersionSummaryText = MenuStrings.Format("Current version: {0}", current);
                UpdateStatusText = MenuStrings.Translate(
                    "Update check failed. Please check your network or try again later.");
                break;
        }
    }
    #endregion

    public DelegateCommand CmdCopyVersion => field ??= new(() =>
    {
        Clipboard.SetText($"""
                           Version: {App.Version}, CommitHash: {App.CommitHash}
                           OS Architecture: {App.OSArchitecture}, Process Architecture: {App.ProcessArchitecture}
                           """);
    });
}

public class LibraryInfo : INotifyPropertyChanged
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Url { get; init; }

    /// <summary>Third-party library descriptions are UI copy, not data.</summary>
    public string LocalizedDescription => MenuStrings.Translate(Description);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RaiseLocalizedDescription()
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalizedDescription)));
}
