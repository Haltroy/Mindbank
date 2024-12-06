using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace Mindbank.Views;

public partial class SettingsScreen : NUC
{
    private bool _initializingSettings = true;

    public SettingsScreen()
    {
        InitializeComponent();
    }

    private void SystemThemeChecked(object? sender, RoutedEventArgs e)
    {
        if (_initializingSettings || sender is not RadioButton { IsChecked: true } ||
            Application.Current is not { } app) return;
        app.RequestedThemeVariant = ThemeVariant.Default;
        Settings.Theme = ThemeVariant.Default;
        if (BlurLevel is { Value: var v } && DesktopContainer is not null && UseBlur is { IsChecked: var useBlur })
            DesktopContainer.SetOpacity(useBlur is true ? v : 100);
        if (!Design.IsDesignMode) Settings.Save();
    }

    private void LightThemeChecked(object? sender, RoutedEventArgs e)
    {
        if (_initializingSettings || sender is not RadioButton { IsChecked: true } ||
            Application.Current is not { } app) return;
        app.RequestedThemeVariant = ThemeVariant.Light;
        Settings.Theme = ThemeVariant.Light;
        if (BlurLevel is { Value: var v } && DesktopContainer is not null && UseBlur is { IsChecked: var useBlur })
            DesktopContainer.SetOpacity(useBlur is true ? v : 100);
        if (!Design.IsDesignMode) Settings.Save();
    }

    private void DarkThemeChecked(object? sender, RoutedEventArgs e)
    {
        if (_initializingSettings || sender is not RadioButton { IsChecked: true } ||
            Application.Current is not { } app) return;
        app.RequestedThemeVariant = ThemeVariant.Dark;
        Settings.Theme = ThemeVariant.Dark;
        if (BlurLevel is { Value: var v } && DesktopContainer is not null && UseBlur is { IsChecked: var useBlur })
            DesktopContainer.SetOpacity(useBlur is true ? v : 100);
        if (!Design.IsDesignMode) Settings.Save();
    }

    private void BlurLevelValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_initializingSettings || sender is not Slider { Value: var v } || DesktopContainer is null ||
            UseBlur is not { IsChecked: var useBlur }) return;
        DesktopContainer.SetOpacity(useBlur is true ? v : 100);
        if (!Design.IsDesignMode) Settings.Save();
    }

    private void UseBlurCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (_initializingSettings || BlurLevel is not { Value: var v } || DesktopContainer is null ||
            UseBlur is not { IsChecked: var useBlur }) return;
        DesktopContainer.SetOpacity(useBlur is true ? v : 100);
        if (!Design.IsDesignMode) Settings.Save();
    }

    private void GoBack(object? sender, RoutedEventArgs e)
    {
        Main?.GoBack();
    }

    private void Init(object? sender, EventArgs e)
    {
        _initializingSettings = true;
        if (Settings.Theme == ThemeVariant.Default && DefaultTheme != null) DefaultTheme.IsChecked = true;
        if (Settings.Theme == ThemeVariant.Dark && DarkTheme != null) DarkTheme.IsChecked = true;
        if (Settings.Theme == ThemeVariant.Light && LightTheme != null) LightTheme.IsChecked = true;
        if (UseBlur != null) UseBlur.IsChecked = Settings.UseBlur;
        if (BlurLevel != null) BlurLevel.Value = Settings.BlurLevel;
        _initializingSettings = false;
    }
}