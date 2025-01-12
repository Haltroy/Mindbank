using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Mindbank.Backend;

namespace Mindbank.Views;

public partial class NoteScreen : NUC
{
    public static readonly StyledProperty<Bank> BankProperty =
        AvaloniaProperty.Register<NoteScreen, Bank>(nameof(Bank), Bank.GenerateExampleBank());

    public static readonly StyledProperty<bool> EditModeProperty =
        AvaloniaProperty.Register<NoteScreen, bool>(nameof(EditMode));

    public static readonly StyledProperty<Note?> EditNoteProperty =
        AvaloniaProperty.Register<NoteScreen, Note?>(nameof(EditNote));

    public static readonly StyledProperty<Color> NewTagColorProperty =
        AvaloniaProperty.Register<NoteScreen, Color>(nameof(NewTagColor), Colors.CornflowerBlue);

    public static readonly StyledProperty<string> NewTagTextProperty =
        AvaloniaProperty.Register<NoteScreen, string>(nameof(NewTagColor), string.Empty);

    private readonly List<Tag> _newNoteTags = [];

    private string _newText = string.Empty;

    private Note? _randomNote;

    public NoteScreen()
    {
        InitializeComponent();
        Task.Run(LoadNote);
    }

    public Bank Bank
    {
        get => GetValue(BankProperty);
        set => SetValue(BankProperty, value);
    }

    public Note? EditNote
    {
        get => GetValue(EditNoteProperty);
        set => SetValue(EditNoteProperty, value);
    }

    public bool EditMode
    {
        get => GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    private Predicate<Note> GetPredicate => SearchConditionIsMet;

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

    private async void LoadNote()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    await Task.Run(Bank.Read);
                    MainCarousel.SelectedIndex = 1;
                }
                catch (Exception ex)
                {
                    ErrorPanel.IsVisible = true;
                    LogText.Text = ex.ToString();
                }
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async void SaveLogToFileClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                IStorageProvider? storageProvider = null;
                switch (IsOnDesktop)
                {
                    case true:
                    {
                        if (DesktopContainer is { StorageProvider: { CanSave: true } sp })
                            storageProvider = sp;
                        break;
                    }
                    default:
                    {
                        if (TopLevel.GetTopLevel(this) is { StorageProvider: { CanSave: true } sp })
                            storageProvider = sp;
                        break;
                    }
                }

                if (storageProvider is null) return;
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = Lang.Lang.NoteScreen_SaveLogToFileTitle,
                    FileTypeChoices = [FilePickerFileTypes.TextPlain, FilePickerFileTypes.All]
                });
                if (file is not null)
                {
                    await using var fileStream = await file.OpenWriteAsync();
                    await using var streamWriter = new StreamWriter(fileStream);
                    await streamWriter.WriteLineAsync(LogText.Text ?? string.Empty);
                }
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    internal void AllTagsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not TagControl { TagObject: { } tag }) return;
        if (tag.Checked) _newNoteTags.Add(tag);
        else _newNoteTags.Remove(tag);
        DoSearch(GetPredicate);

        if (!EditMode || EditNote is not { } note) return;

        if (note.Tags.Contains(tag))
        {
            note.Tags = note.Tags.Where(x => x != tag).ToArray();
        }
        else
        {
            var tags = note.Tags;
            Array.Resize(ref tags, tags.Length + 1);
            tags[^1] = tag;
            note.Tags = tags;
        }

        note.RequestPropertyChangeInvoke();

        Dispatcher.UIThread.InvokeAsync(() => Bank.Write());
    }

    private void SearchCaseSensitivityCheckedChanged(object? sender, RoutedEventArgs e)
    {
        DoSearch(GetPredicate);
    }

    private void SearchUseRegexCheckedChanged(object? sender, RoutedEventArgs e)
    {
        DoSearch(GetPredicate);
    }

    private async void ExportSelected(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Run(async () =>
            {
                IStorageProvider? storageProvider = null;
                switch (IsOnDesktop)
                {
                    case true:
                    {
                        if (DesktopContainer is { StorageProvider: { CanSave: true } sp })
                            storageProvider = sp;
                        break;
                    }
                    default:
                    {
                        if (TopLevel.GetTopLevel(this) is { StorageProvider: { CanSave: true } sp })
                            storageProvider = sp;
                        break;
                    }
                }

                if (storageProvider is null) return;
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = null,
                    SuggestedStartLocation = null,
                    SuggestedFileName = null,
                    DefaultExtension = null,
                    FileTypeChoices = null,
                    ShowOverwritePrompt = null
                });
                if (file is null) return;
                var exported = Bank.Notes.Where(it => it is { Visible: true }).ToArray();
                await using var fs = await file.OpenWriteAsync();
                Bank.SaveToStream(fs, exported);
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async void ImportFromFile(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Run(async () =>
            {
                IStorageProvider? storageProvider = null;
                switch (IsOnDesktop)
                {
                    case true:
                    {
                        if (DesktopContainer is { StorageProvider: { CanSave: true } sp })
                            storageProvider = sp;
                        break;
                    }
                    default:
                    {
                        if (TopLevel.GetTopLevel(this) is { StorageProvider: { CanSave: true } sp })
                            storageProvider = sp;
                        break;
                    }
                }

                if (storageProvider is null) return;
                var file = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = null,
                    SuggestedStartLocation = null,
                    SuggestedFileName = null,
                    AllowMultiple = false,
                    FileTypeFilter = null
                });
                if (file.Count <= 0) return;
                await using var fs = await file[0].OpenReadAsync();
                var loaded = Bank.LoadFromStream(fs, Bank, out var v);
                if (v > Settings.Version)
                    Main?.ShowVersionMismatchError(v);
                else
                    Bank.AddRange(loaded);
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void DoSearch(Predicate<Note> predicate)
    {
        foreach (var child in NotesIc.Items)
        {
            if (child is not Note note) continue;
            note.Visible = predicate(note);
        }
    }

    private bool SearchConditionIsMet(Note note)
    {
        if (note == EditNote) return false;
        if (_randomNote is not null) return note == _randomNote;

        var condition1 = true;
        if (SearchBox is { Text: var searchText } && !string.IsNullOrEmpty(searchText))
        {
            var caseSensitive = SearchCaseSensitivity.IsChecked is true;
            var useRegex = SearchUseRegex.IsChecked is true;
            condition1 = useRegex
                ? new Regex(searchText, !caseSensitive ? RegexOptions.IgnoreCase : RegexOptions.None).IsMatch(note.Text)
                : note.Text.Contains(searchText,
                    caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
        }

        if (_newNoteTags.Count <= 0) return condition1;
        foreach (var tag in _newNoteTags)
            if (note.Tags.Contains(tag))
                return condition1;

        return false;
    }

    private void PickRandomCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { IsChecked: var b } || RandomPickSlider is not
                { UpperSelectedValue: var max, LowerSelectedValue: var min }) return;
        if (b is true)
        {
            var rnd = new Random();
            var pick = rnd.Next((int)min, (int)max);
            _randomNote = Bank.Notes.Where(it => it is { Visible: true }).ToArray()[pick];
        }
        else
        {
            _randomNote = null;
        }

        DoSearch(GetPredicate);
    }

    private void CheckAll_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { IsChecked: var c }) return;
        foreach (var item in NotesIc.Items)
        {
            if (item is not Note { Visible: true } note) return;
            note.Checked = c is true;
        }
    }

    private void InvertSelection(object? sender, RoutedEventArgs e)
    {
        foreach (var item in NotesIc.Items)
        {
            if (item is not Note { Visible: true } note) return;
            note.Checked = !note.Checked;
        }
    }

    private void RemoveSelected(object? sender, RoutedEventArgs e)
    {
        foreach (var n in Bank.Notes)
        {
            if (!n.Visible || !n.Checked) continue;
            Bank.Remove(n);
        }
    }

    private void SearchBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        DoSearch(GetPredicate);
    }

    private void AddNewNoteClicked(object? sender, RoutedEventArgs e)
    {
        if (NewText is not { Text: var text } || string.IsNullOrWhiteSpace(text)) return;
        Bank.Add(new Note(text, _newNoteTags.ToArray(), DateTime.Now, Bank));
        if (Settings.PowerMode) return;
        NewText.Text = string.Empty;
        _newNoteTags.Clear();
    }

    private void GoBack(object? sender, RoutedEventArgs e)
    {
        Bank.Write();
        Bank.Unload();
        Main?.GoBack();
    }

    private void MoveItemUp(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: Note note }) return;
        if (note.IsOnTop) return;
        var index = Bank.IndexOf(note) - 1;
        Bank.Remove(note);
        Bank.Insert(index, note);
    }

    private void MoveItemDown(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: Note note }) return;
        if (note.IsOnBottom) return;
        var index = Bank.IndexOf(note) + 1;
        Bank.Remove(note);
        Bank.Insert(index, note);
    }

    private void NoteItemEditMode_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: Note note }) return;
        note.Visible = false;
        note.Checked = false;
        EditMode = true;
        EditNote = note;
        if (NewText is not null)
        {
            _newText = NewText.Text ?? string.Empty;
            NewText.Text = note.Text;
        }

        foreach (var tag in note.Tags) tag.Checked = true;

        _newNoteTags.Clear();
        _newNoteTags.AddRange(note.Tags);
        DoSearch(GetPredicate);
    }

    private void NewText_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (NewText is null) return;
        if (EditMode && EditNote is { } note) note.Text = NewText.Text ?? string.Empty;
    }

    private void NoteItemEditOffOnClick(object? sender, RoutedEventArgs e)
    {
        EditMode = false;
        if (EditNote is not null) EditNote.Visible = true;
        EditNote = null;
        if (NewText is not null) NewText.Text = _newText;
        if (EditNote is not null)
            foreach (var tag in EditNote.Tags)
                tag.Checked = false;
        _newNoteTags.Clear();
        DoSearch(GetPredicate);
    }

    private void TagControl_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { ContextFlyout: { } flyout } c) return;
        var point = e.GetCurrentPoint(c);
        if (point.Properties.IsRightButtonPressed) flyout.ShowAt(c);
    }

    private void TagControl_OnHolding(object? sender, HoldingRoutedEventArgs e)
    {
        if (sender is not Control { ContextFlyout: { } flyout } c) return;
        flyout.ShowAt(c);
    }

    private void TagControl_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { ContextFlyout: { } flyout } c) return;
        flyout.ShowAt(c);
    }

    private void DeleteSelected(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: Tag tag }) return;
        Bank.Remove(tag);
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
}