using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Mindbank.Views;

public partial class AboutScreen : NUC
{
    internal static AvaloniaProperty TechnologyListProperty =
        AvaloniaProperty.Register<AboutScreen, TechnologyLink[]?>(nameof(TechnologyList), GenTechList());

    public AboutScreen()
    {
        InitializeComponent();
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
        LicenseBox.Text = ReadResource("LICENSE");
    }

    internal TechnologyLink[]? TechnologyList
    {
        get => GetValue(TechnologyListProperty) as TechnologyLink[];
        set => SetValue(TechnologyListProperty, value);
    }

    internal static TechnologyLink[] GenTechList()
    {
        return
        [
            new TechnologyLink("AvaloniaUI", "https://avaloniaui.net/"),
            new TechnologyLink("Avalonia Fluent Icons", "http://avaloniaui.github.io/icons.html"),
            new TechnologyLink(".NET", "https://dotnet.microsoft.com/"),
            new TechnologyLink("RangeSlider.Avalonia", "https://github.com/DmitryNizhebovsky/Avalonia.RangeSlider"),
            new TechnologyLink("FluentAvalonia.ProgressRing", "https://github.com/ymg2006/FluentAvalonia.ProgressRing")
        ];
    }

    private static string ReadResource(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        if (stream == null) return string.Empty;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    // ReSharper disable once InconsistentNaming
    private void CarouselButton_Click(object? S, RoutedEventArgs e)
    {
        if (S is not Button { Tag: Control panel }) return;
        CarouselMenu.SelectedItem = panel;
    }

    private void Navigate(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: string link }) return;
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = link
        });
    }

    private void GoBack(object? sender, RoutedEventArgs e)
    {
        Main?.GoBack();
    }
}

public record TechnologyLink(string Name, string Link);