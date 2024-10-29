using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Mindbank.Views;

public partial class MainWindow : Window
{
    private readonly double _opacity = 0.75;

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
                    SetOpacity(_opacity);
            });
            Thread.Sleep(5000);
            AvaloniaWontSetMyOpacity();
        });
    }

    public void SetOpacity(double value)
    {
        if (Background is Brush b) b.Opacity = value / 100;
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (AllowClosing) return;
        e.Cancel = true;
        Hide();
    }
}