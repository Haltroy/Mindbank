using Avalonia;
using Avalonia.Controls;
using Mindbank.Backend;

namespace Mindbank.Views;

public class NUC : UserControl
{
    public static readonly StyledProperty<MainView?> MainProperty =
        AvaloniaProperty.Register<NUC, MainView?>(nameof(Main));

    public static readonly StyledProperty<bool?> IsOnDesktopProperty =
        AvaloniaProperty.Register<NUC, bool?>(nameof(IsOnDesktop));

    public static readonly StyledProperty<MainWindow?> DesktopContainerProperty =
        AvaloniaProperty.Register<NUC, MainWindow?>(nameof(DesktopContainer));


    public static readonly StyledProperty<Settings> SettingsProperty =
        AvaloniaProperty.Register<MainView, Settings>(nameof(Settings), new Settings());

    public MainView? Main
    {
        get => GetValue(MainProperty);
        set => SetValue(MainProperty, value);
    }

    public Settings Settings
    {
        get => GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public MainWindow? DesktopContainer
    {
        get => GetValue(DesktopContainerProperty);
        set => SetValue(DesktopContainerProperty, value);
    }

    public bool? IsOnDesktop
    {
        get => GetValue(IsOnDesktopProperty);
        set => SetValue(IsOnDesktopProperty, value);
    }
}
