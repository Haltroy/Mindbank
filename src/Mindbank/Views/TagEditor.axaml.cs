using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Mindbank.Backend;

namespace Mindbank.Views;

public partial class TagEditor : NoteEditUserControl
{
    private static readonly RoutedEvent<RoutedEventArgs> OkClickEvent =
        RoutedEvent.Register<TagControl, RoutedEventArgs>(nameof(OkClick), RoutingStrategies.Direct);

    public static readonly StyledProperty<Color> NewTagColorProperty =
        AvaloniaProperty.Register<NoteScreen, Color>(nameof(NewTagColor), Colors.CornflowerBlue);

    public static readonly StyledProperty<string> NewTagTextProperty =
        AvaloniaProperty.Register<NoteScreen, string>(nameof(NewTagColor), string.Empty);

    public TagEditor()
    {
        InitializeComponent();
    }

    public Color NewTagColor
    {
        get => GetValue(NewTagColorProperty);
        set => SetValue(NewTagColorProperty, value);
    }

    public string NewTagText
    {
        get => GetValue(NewTagTextProperty);
        set => SetValue(NewTagTextProperty, value);
    }

    private void TagControl_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectTags is { IsChecked: true }) return;
        if (sender is Control { ContextFlyout: var flyout } c)
            flyout?.ShowAt(c);
    }

    public event EventHandler<RoutedEventArgs> OkClick
    {
        add => AddHandler(OkClickEvent, value);
        remove => RemoveHandler(OkClickEvent, value);
    }

    private void OkClicked(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(OkClickEvent));
    }

    private void RandomColorClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control) return;
        switch (sender)
        {
            case Control { Tag: "NewTag" }:
            {
                var rnd = new Random();
                NewTagColor = Color.FromRgb((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256));
                break;
            }
            case Control { Tag: Tag tag }:
            {
                var rnd = new Random();
                tag.Color = Color.FromRgb((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256));
                break;
            }
        }
    }

    private void NewTagClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewTagText)) return;
        Bank.Add(new Tag(NewTagText, NewTagColor, Bank));
    }

    private void InvertSelection(object? sender, RoutedEventArgs e)
    {
        foreach (var obj in TagsItemsControl.Items)
        {
            if (obj is Tag tag) tag.Checked = !tag.Checked;
        }
    }

    private void DeleteSelected(object? sender, RoutedEventArgs e)
    {
        Bank.RemoveAll(it => it.Checked);
    }

}