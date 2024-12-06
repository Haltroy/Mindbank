using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DialogHostAvalonia;
using Mindbank.Backend;

namespace Mindbank.Views;

public partial class MainView : UserControl
{
    public static readonly StyledProperty<bool?> IsOnDesktopProperty =
        AvaloniaProperty.Register<MainView, bool?>(nameof(IsOnDesktop));

    public static readonly StyledProperty<MainWindow?> DesktopContainerProperty =
        AvaloniaProperty.Register<MainView, MainWindow?>(nameof(DesktopContainer));

    public static readonly StyledProperty<Settings> SettingsProperty =
        AvaloniaProperty.Register<MainView, Settings>(nameof(Settings), new Settings());

    internal static readonly StyledProperty<bool> CheckingForUpdatesProperty =
        AvaloniaProperty.Register<MainView, bool>(nameof(CheckingForUpdates), true);

    internal static readonly StyledProperty<bool> UpToDateProperty =
        AvaloniaProperty.Register<MainView, bool>(nameof(UpToDate));

    internal static readonly StyledProperty<bool> UpdateAvailableProperty =
        AvaloniaProperty.Register<MainView, bool>(nameof(UpdateAvailable));

    public MainView()
    {
        InitializeComponent();
        Settings.Load();
        CheckForUpdates();
    }

    public static bool IsDebugMode
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    public bool CheckingForUpdates
    {
        get => GetValue(CheckingForUpdatesProperty);
        set => SetValue(CheckingForUpdatesProperty, value);
    }

    public bool UpToDate
    {
        get => GetValue(UpToDateProperty);
        set => SetValue(UpToDateProperty, value);
    }

    public bool UpdateAvailable
    {
        get => GetValue(UpdateAvailableProperty);
        set => SetValue(UpdateAvailableProperty, value);
    }

    public Settings Settings
    {
        get => GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public bool? IsOnDesktop
    {
        get => GetValue(IsOnDesktopProperty);
        set => SetValue(IsOnDesktopProperty, value);
    }

    public MainWindow? DesktopContainer
    {
        get => GetValue(DesktopContainerProperty);
        set => SetValue(DesktopContainerProperty, value);
    }

    private static Version GetVersion()
    {
        return Assembly.GetExecutingAssembly() is { } ass
               && ass.GetName() is { } name
               && name.Version != null
            ? name.Version
            : new Version();
    }

    private async void CheckForUpdates()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Design.IsDesignMode)
                {
                    CheckingForUpdates = false;
                    UpToDate = true;
                    UpdateAvailable = false;
                    return;
                }

                const string updateCheckLocation =
                    "https://raw.githubusercontent.com/Haltroy/Mindbank/refs/heads/main/version";
                using var httpClient = new HttpClient();
                var response = await Task.Run(async () => await httpClient.GetAsync(updateCheckLocation));
                response.EnsureSuccessStatusCode();
                var versionText = await response.Content.ReadAsStringAsync();
                var version = new Version(versionText);
                if (version.CompareTo(GetVersion()) > 0)
                {
                    CheckingForUpdates = false;
                    UpToDate = false;
                    UpdateAvailable = true;
                    return;
                }

                CheckingForUpdates = false;
                UpToDate = true;
                UpdateAvailable = false;
            });
        }
        catch (Exception)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CheckingForUpdates = false;
                UpToDate = true;
                UpdateAvailable = false;
            });
        }
    }

    private void NewNotesClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: Control c }) return;
        DialogHost.Show(c);
    }

    private void DialogDismiss(object? sender, RoutedEventArgs e)
    {
        DialogHost.Close(null);
    }

    internal void ShowVersionMismatchError(int v)
    {
        StackPanel stackPanel = new() { Orientation = Orientation.Vertical, Spacing = 5 };
        DockPanel infoPanel = new() { Margin = new Thickness(10) };
        stackPanel.Children.Add(infoPanel);
        var errorPath = new Path
        {
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 1,
            Stretch = Stretch.Uniform,
            Height = 25,
            Margin = new Thickness(10),
            Data = new PathGeometry
            {
                Figures = PathFigures.Parse(
                    "M12,2 C17.523,2 22,6.478 22,12 C22,17.522 17.523,22 12,22 C6.477,22 2,17.522 2,12 C2,6.478 6.477,2 12,2 Z M12,3.667 C7.405,3.667 3.667,7.405 3.667,12 C3.667,16.595 7.405,20.333 12,20.333 C16.595,20.333 20.333,16.595 20.333,12 C20.333,7.405 16.595,3.667 12,3.667 Z M11.9986626,14.5022358 C12.5502088,14.5022358 12.9973253,14.9493523 12.9973253,15.5008984 C12.9973253,16.0524446 12.5502088,16.4995611 11.9986626,16.4995611 C11.4471165,16.4995611 11,16.0524446 11,15.5008984 C11,14.9493523 11.4471165,14.5022358 11.9986626,14.5022358 Z M11.9944624,7 C12.3741581,6.99969679 12.6881788,7.28159963 12.7381342,7.64763535 L12.745062,7.7494004 L12.7486629,12.2509944 C12.7489937,12.6652079 12.4134759,13.0012627 11.9992625,13.0015945 C11.6195668,13.0018977 11.3055461,12.7199949 11.2555909,12.3539592 L11.2486629,12.2521941 L11.245062,7.7506001 C11.2447312,7.33638667 11.580249,7.00033178 11.9944624,7 Z")
            }
        };
        DockPanel.SetDock(errorPath, Dock.Left);
        infoPanel.Children.Add(errorPath);
        infoPanel.Children.Add(new TextBlock
            { Text = Lang.Lang.MainView_CanntoLoad_VersionMismatch.Replace("%v%", $"{v}") });
        Button okButton = new() { Content = Lang.Lang.MainView_OK, HorizontalAlignment = HorizontalAlignment.Right };
        okButton.Click += DialogDismiss;
        stackPanel.Children.Add(okButton);

        DialogHost.Show(stackPanel);
    }

    private void NoteGroupButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: Control c } || !MainCarousel.Items.Contains(c)) return;
        MainCarousel.SelectedItem = c;
    }

    private void Init(object? sender, EventArgs e)
    {
        Settings.Load();
    }


    private void VisualReady(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Settings.Load();
        if (Application.Current is { } app) app.RequestedThemeVariant = Settings.Theme;

        if (IsOnDesktop is not true || DesktopContainer == null) return;
        DesktopContainer.SetOpacity(Settings.UseBlur ? Settings.BlurLevel : 100);
        if (Settings.HideToSysTray) DesktopContainer.AllowClosing = false;
        if (Settings.StartInTray) DesktopContainer.Hide();
    }

    private void BankClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bank bank }) return;
        var ns = new NoteScreen { Bank = bank, Main = this };
        MainCarousel.Items.Add(ns);
        MainCarousel.SelectedItem = ns;
    }

    private void DeleteNoteSourceClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bank bank }) return;
        DockPanel mainPanel = new() { Margin = new Thickness(10) };
        var warningIcon = new Path
        {
            Margin = new Thickness(10),
            Stroke = new SolidColorBrush(Colors.Yellow),
            StrokeThickness = 0,
            Data = new PathGeometry
            {
                Figures = PathFigures.Parse(
                    "M10.9093922,2.78216375 C11.9491636,2.20625071 13.2471955,2.54089334 13.8850247,3.52240345 L13.9678229,3.66023048 L21.7267791,17.6684928 C21.9115773,18.0021332 22.0085303,18.3772743 22.0085303,18.7586748 C22.0085303,19.9495388 21.0833687,20.9243197 19.9125791,21.003484 L19.7585303,21.0086748 L4.24277801,21.0086748 C3.86146742,21.0086748 3.48641186,20.9117674 3.15282824,20.7270522 C2.11298886,20.1512618 1.7079483,18.8734454 2.20150311,17.8120352 L2.27440063,17.668725 L10.0311968,3.66046274 C10.2357246,3.291099 10.5400526,2.98673515 10.9093922,2.78216375 Z M20.4146132,18.3952808 L12.6556571,4.3870185 C12.4549601,4.02467391 11.9985248,3.89363262 11.6361802,4.09432959 C11.5438453,4.14547244 11.4637001,4.21532637 11.4006367,4.29899869 L11.3434484,4.38709592 L3.58665221,18.3953582 C3.385998,18.7577265 3.51709315,19.2141464 3.87946142,19.4148006 C3.96285732,19.4609794 4.05402922,19.4906942 4.14802472,19.5026655 L4.24277801,19.5086748 L19.7585303,19.5086748 C20.1727439,19.5086748 20.5085303,19.1728883 20.5085303,18.7586748 C20.5085303,18.6633247 20.4903516,18.5691482 20.455275,18.4811011 L20.4146132,18.3952808 L12.6556571,4.3870185 L20.4146132,18.3952808 Z M12.0004478,16.0017852 C12.5519939,16.0017852 12.9991104,16.4489016 12.9991104,17.0004478 C12.9991104,17.5519939 12.5519939,17.9991104 12.0004478,17.9991104 C11.4489016,17.9991104 11.0017852,17.5519939 11.0017852,17.0004478 C11.0017852,16.4489016 11.4489016,16.0017852 12.0004478,16.0017852 Z M11.9962476,8.49954934 C12.3759432,8.49924613 12.689964,8.78114897 12.7399193,9.14718469 L12.7468472,9.24894974 L12.750448,13.7505438 C12.7507788,14.1647572 12.4152611,14.5008121 12.0010476,14.5011439 C11.621352,14.5014471 11.3073312,14.2195442 11.257376,13.8535085 L11.250448,13.7517435 L11.2468472,9.25014944 C11.2465164,8.83593601 11.5820341,8.49988112 11.9962476,8.49954934 Z")
            }
        };
        DockPanel.SetDock(warningIcon, Dock.Left);
        mainPanel.Children.Add(warningIcon);
        var sidePanel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
        sidePanel.Children.Add(new TextBlock
        {
            Text = Lang.Lang.MainView_DeleteBank,
            FontWeight = FontWeight.Bold
        });
        sidePanel.Children.Add(new TextBlock
        {
            Margin = new Thickness(10),
            Text = bank.Name,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        var buttonPanel = new Grid { ColumnDefinitions = new ColumnDefinitions("*,5,*") };
        var yesButton = new Button
        {
            Content = Lang.Lang.MainView_Yes, HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(yesButton, 0);
        yesButton.Click += (_, _) =>
        {
            Settings.Remove(bank);
            DialogHost.Close(null);
        };
        var noButton = new Button
        {
            Content = Lang.Lang.MainView_No, HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(noButton, 2);
        noButton.Click += (_, _) => DialogHost.Close(null);
        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        sidePanel.Children.Add(buttonPanel);
        mainPanel.Children.Add(sidePanel);
        DialogHost.Show(mainPanel);
    }

    internal void GoBack()
    {
        if (MainCarousel is null || MainPanel is null || !MainCarousel.Items.Contains(MainPanel)) return;
        MainCarousel.SelectedItem = MainPanel;
        var l = new List<object?>();
        foreach (var control in MainCarousel.Items)
        {
            if (Equals(control, MainPanel) || Equals(control, SettingsView) || Equals(control, AboutView)) continue;
            l.Add(control);
            if (control is NoteScreen ns) ns.Main = null;
        }

        foreach (var obj in l) MainCarousel.Items.Remove(obj);
    }

    private void CreateNewNoteClicked(object? sender, RoutedEventArgs e)
    {
        if (NewNoteGroupName is not { Text: var t } || string.IsNullOrWhiteSpace(t)) return;
        var bank = Settings.NewNote(t);
        var ns = new NoteScreen { Bank = bank, Main = this };
        MainCarousel.Items.Add(ns);
        MainCarousel.SelectedItem = ns;
        NewNoteGroupName.Text = string.Empty;
        DialogHost.Close(null);
    }

    private async void ImportFromFileClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Parent is not TopLevel { StorageProvider.CanOpen: true } top) return;
                var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = Lang.Lang.ImportNoteTitle,
                    AllowMultiple = true,
                    FileTypeFilter =
                    [
                        new FilePickerFileType(Lang.Lang.ImportExportFileType)
                        {
                            Patterns = "*.mbf".Split('|'),
                            MimeTypes = "application/haltroy-mindbank".Split('|'),
                            AppleUniformTypeIdentifiers = "haltroy.mindbank".Split('|')
                        },
                        FilePickerFileTypes.All
                    ]
                });
                if (files.Count < 0) return;
                foreach (var file in files) Settings.ImportNote(await file.OpenReadAsync());
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void UpdateApp(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://haltroy.com/en/fluxion#fluxion-viewer") { UseShellExecute = true });
    }

    private void UpdateButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (UpdateAvailable)
        {
            if (sender is not Button { Tag: Control c }) return;
            DialogHost.Show(c);
            return;
        }

        if (!UpToDate) return;
        CheckingForUpdates = true;
        UpToDate = false;
        UpdateAvailable = false;
        CheckForUpdates();
    } // ReSharper disable UnusedParameter.Local
    private void CloseDialogHost(object? s, RoutedEventArgs e)
    {
        MainDialogHost.CurrentSession?.Close();
    }
}