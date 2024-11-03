using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Mindbank.Views;

public partial class MainWindow : Window
{
    private double _opacity = 0.75;

    public bool AllowClosing = false;

    public MainWindow()
    {
        InitializeComponent();
        AvaloniaWontSetMyOpacity();
    }

    private async void AvaloniaWontSetMyOpacity()
    {
        await Task.Run(async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Background is Brush brush && Math.Abs(brush.Opacity - _opacity) > 0.001)
                    SetOpacity(_opacity * 100);
            });
            Thread.Sleep(5000);
            AvaloniaWontSetMyOpacity();
        });
    }

    public void SetOpacity(double value)
    {
        _opacity = value / 100;
        if (value < 100)
            TransparencyLevelHint =
            [
                WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur, WindowTransparencyLevel.Transparent,
                WindowTransparencyLevel.None
            ];
        else TransparencyLevelHint = [WindowTransparencyLevel.None];
        if (Background is Brush b) b.Opacity = _opacity;
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (AllowClosing) return;
        e.Cancel = true;
        Hide();
    }
}