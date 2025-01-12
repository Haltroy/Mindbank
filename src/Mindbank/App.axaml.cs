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
        Settings.SetupSingleton();
        if (Settings.IsInstanceRunning)
        {
            Environment.Exit(0);
            return;
        }
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += (_, _) => { Settings.RemoveSingleton(); };
                desktop.MainWindow = new MainWindow { ThisIsTheSingleton = true };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView { IsOnDesktop = false };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}