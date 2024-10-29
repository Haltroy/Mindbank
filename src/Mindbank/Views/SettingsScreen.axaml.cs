using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace Mindbank.Views;

public partial class SettingsScreen : NUC
{
    private bool initializing_settings = true;

    public SettingsScreen()
    {
        InitializeComponent();
    }

    private void SystemThemeChecked(object? sender, RoutedEventArgs e)
    {
        if (initializing_settings || sender is not RadioButton { IsChecked: true } ||
            Application.Current is not { } app) return;
        app.RequestedThemeVariant = ThemeVariant.Default;
        Settings.Theme = ThemeVariant.Default;
        if (BlurLevel is { Value: var v } && DesktopContainer is not null && UseBlur is { IsChecked: var useBlur })
            DesktopContainer.SetOpacity(useBlur is true ? v : 100);
    }

    private void LightThemeChecked(object? sender, RoutedEventArgs e)
    {
        if (initializing_settings || sender is not RadioButton { IsChecked: true } ||
            Application.Current is not { } app) return;
        app.RequestedThemeVariant = ThemeVariant.Light;
        Settings.Theme = ThemeVariant.Light;
        if (BlurLevel is { Value: var v } && DesktopContainer is not null && UseBlur is { IsChecked: var useBlur })
            DesktopContainer.SetOpacity(useBlur is true ? v : 100);
    }

    private void DarkThemeChecked(object? sender, RoutedEventArgs e)
    {
        if (initializing_settings || sender is not RadioButton { IsChecked: true } ||
            Application.Current is not { } app) return;
        app.RequestedThemeVariant = ThemeVariant.Dark;
        Settings.Theme = ThemeVariant.Dark;
        if (BlurLevel is { Value: var v } && DesktopContainer is not null && UseBlur is { IsChecked: var useBlur })
            DesktopContainer.SetOpacity(useBlur is true ? v : 100);
    }

    private void BlurLevelValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (initializing_settings || sender is not Slider { Value: var v } || DesktopContainer is null ||
            UseBlur is not { IsChecked: var useBlur }) return;
        DesktopContainer.SetOpacity(useBlur is true ? v : 100);
    }

    private void UseBlurCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (initializing_settings || BlurLevel is not { Value: var v } || DesktopContainer is null ||
            UseBlur is not { IsChecked: var useBlur }) return;
        DesktopContainer.SetOpacity(useBlur is true ? v : 100);
        Settings.Save();
    }

    private void GoBack(object? sender, RoutedEventArgs e)
    {
        Main?.GoBack();
    }

    private void Init(object? sender, EventArgs e)
    {
        initializing_settings = true;
        if (Settings.Theme == ThemeVariant.Default && DefaultTheme != null) DefaultTheme.IsChecked = true;
        if (Settings.Theme == ThemeVariant.Dark && DarkTheme != null) DarkTheme.IsChecked = true;
        if (Settings.Theme == ThemeVariant.Light && LightTheme != null) LightTheme.IsChecked = true;
        if (UseBlur != null) UseBlur.IsChecked = Settings.UseBlur;
        if (BlurLevel != null) BlurLevel.Value = Settings.BlurLevel;
        initializing_settings = false;
    }
}