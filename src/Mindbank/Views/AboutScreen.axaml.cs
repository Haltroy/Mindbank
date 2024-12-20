using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Mindbank.Views;

public partial class AboutScreen : NUC
{
    private static readonly AvaloniaProperty TechnologyListProperty =
        AvaloniaProperty.Register<AboutScreen, TechnologyLink[]?>(nameof(TechnologyList), GenTechList());

    public AboutScreen()
    {
        InitializeComponent();
        Version.Text = "v" + Tools.GetVersion();
        LicenseBox.Text = ReadResource("LICENSE");
    }

    internal TechnologyLink[]? TechnologyList
    {
        get => GetValue(TechnologyListProperty) as TechnologyLink[];
        set => SetValue(TechnologyListProperty, value);
    }

    private static TechnologyLink[] GenTechList()
    {
        return
        [
            new TechnologyLink("AvaloniaUI", "https://avaloniaui.net/"),
            new TechnologyLink("Avalonia Fluent Icons", "https://avaloniaui.github.io/icons.html"),
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