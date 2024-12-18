using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Mindbank.Backend;
using Mindbank.Views;

namespace Mindbank;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Startup += DesktopOnStartup;
                desktop.MainWindow = new MainWindow { ThisIsTheSingleton = !Settings.IsInstanceRunning };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DesktopOnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        if (Settings.IsInstanceRunning)
        {
            Environment.Exit(0);
            return;
        }

        Settings.SetupSingleton();
    }
}