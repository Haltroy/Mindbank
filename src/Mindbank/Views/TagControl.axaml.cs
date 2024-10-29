using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Mindbank.Backend;

namespace Mindbank.Views;

[PseudoClasses(":checked", ":unchecked")]
public partial class TagControl : UserControl
{
    public static readonly StyledProperty<Tag> TagObjectProperty =
        AvaloniaProperty.Register<TagControl, Tag>(nameof(TagObject), new Tag("Sample", Colors.Gold, null));

    public static readonly RoutedEvent<RoutedEventArgs> IsCheckedChangedEvent =
        RoutedEvent.Register<TagControl, RoutedEventArgs>(nameof(IsCheckedChanged), RoutingStrategies.Direct);

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<TagControl, RoutedEventArgs>(nameof(Click), RoutingStrategies.Direct);

    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<TagControl, bool>(nameof(IsChecked));

    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<TagControl, Color>(nameof(Color), Colors.DeepSkyBlue);

    private bool _mouseCaptured;

    static TagControl()
    {
        AffectsRender<TagControl>(IsCheckedProperty);
        AffectsRender<TagControl>(ColorProperty);
        AffectsRender<TagControl>(TagObjectProperty);
        AffectsRender<TagControl>(IsPointerOverProperty);
    }

    public TagControl()
    {
        InitializeComponent();
    }

    public IBrush ColorBrush => new SolidColorBrush(Color.FromArgb(45, Color.R, Color.G, Color.B));

    public IBrush ColorBrushIsChecked => new SolidColorBrush(Color);

    public IBrush ColorBrushIsPointerOver => new SolidColorBrush(Color.FromArgb(60, Color.R, Color.G, Color.B));

    public IBrush ColorBrushIsPointerOverIsChecked =>
        new SolidColorBrush(Color.FromArgb(30, Color.R, Color.G, Color.B));

    public IBrush TextColorBrush => new SolidColorBrush(Color);

    public IBrush TextColorBrushIsPointerOver => new SolidColorBrush(Tools.ShiftBrightness(Color, 128));
    public IBrush TextColorBrushIsChecked => new SolidColorBrush(Tools.ShiftBrightness(Color, 64));
    public IBrush TextColorBrushIsCheckedIsPointerOver => new SolidColorBrush(Tools.ShiftBrightness(Color, 96));

    public Tag TagObject
    {
        get => GetValue(TagObjectProperty);
        set => SetValue(TagObjectProperty, value);
    }

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set
        {
            OnIsCheckedChanged();
            SetValue(IsCheckedProperty, value);
        }
    }

    public event EventHandler<RoutedEventArgs> IsCheckedChanged
    {
        add => AddHandler(IsCheckedChangedEvent, value);
        remove => RemoveHandler(IsCheckedChangedEvent, value);
    }

    protected virtual void OnIsCheckedChanged()
    {
        UpdatePseudoClasses(IsChecked);
        var args = new RoutedEventArgs(IsCheckedChangedEvent);
        RaiseEvent(args);
    }


    public event EventHandler<RoutedEventArgs> Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected virtual void OnClick()
    {
        IsChecked = !IsChecked;
        var args = new RoutedEventArgs(ClickEvent);
        RaiseEvent(args);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (IsPointerOver) _mouseCaptured = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!IsPointerOver || !_mouseCaptured) return;
        _mouseCaptured = false;

        OnClick();

        var clickEventArgs = new RoutedEventArgs(ClickEvent);
        RaiseEvent(clickEventArgs);
    }

    private void UpdatePseudoClasses(bool isChecked)
    {
        PseudoClasses.Set(":checked", isChecked);
        PseudoClasses.Set(":unchecked", isChecked == false);
    }
}