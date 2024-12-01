using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;

namespace Mindbank.Views;

public partial class Splash : Window
{
    public Splash()
    {
        InitializeComponent();
        TransparencyLevelHint =
        [
            WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur, WindowTransparencyLevel.Transparent,
            WindowTransparencyLevel.None
        ];
        Version.Text = "v"
                       + (
                           Assembly.GetExecutingAssembly() is { } ass
                           && ass.GetName() is { } name
                           && name.Version != null
                               ? "" + (name.Version.Major > 0 ? name.Version.Major : "") +
                                 (name.Version.Minor > 0 ? "." + name.Version.Minor : "") +
                                 (name.Version.Build > 0 ? "." + name.Version.Build : "") +
                                 (name.Version.Revision > 0 ? "." + name.Version.Revision : "")
                               : "?"
                       );
        DoSplash();
    }

    private async void DoSplash()
    {
        try
        {
            await Task.Run(async () =>
            {
                Thread.Sleep(5000);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (Application.Current is not
                        { ApplicationLifetime: ClassicDesktopStyleApplicationLifetime desktop }) return;
                    MainWindow mw = new();
                    mw.Show();
                    desktop.MainWindow = mw;
                    Close();
                });
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}