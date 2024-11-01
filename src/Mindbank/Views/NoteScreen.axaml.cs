using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

    public static readonly StyledProperty<Color> NewTagColorProperty =
        AvaloniaProperty.Register<NoteScreen, Color>(nameof(NewTagColor), Colors.CornflowerBlue);

    public static readonly StyledProperty<string> NewTagTextProperty =
        AvaloniaProperty.Register<NoteScreen, string>(nameof(NewTagColor), string.Empty);

    private readonly List<Tag> _newNoteTags = [];

    private readonly List<Tag> _selectedTags = [];

    private Note? _randomNote;

    public NoteScreen()
    {
        InitializeComponent();
        Task.Run(LoadNote);
    }

    public Bank Bank
    {
        get => GetValue(BankProperty);
        init => SetValue(BankProperty, value);
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

    private Predicate<Note> GetPredicate => SearchConditionIsMet;

    private async void LoadNote()
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

    private async void SaveLogToFileClick(object? sender, RoutedEventArgs e)
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
                    if (Main is { Parent: TopLevel { StorageProvider: { CanSave: true } sp } })
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

    private void NoteTagClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: NoteTag { Note: var note, Tag: var tag } }) return;
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
    }

    private void AllTagsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: Tag tag, IsChecked: var b }) return;
        if (b is true) _selectedTags.Add(tag);
        else _selectedTags.Remove(tag);
        DoSearch(GetPredicate);
    }

    private void SearchCaseSensitivityCheckedChanged(object? sender, RoutedEventArgs e)
    {
        DoSearch(GetPredicate);
    }

    private void SearchUseRegexCheckedChanged(object? sender, RoutedEventArgs e)
    {
        DoSearch(GetPredicate);
    }

    private void DoSearch(Predicate<Note> predicate)
    {
        foreach (var child in NotesIC.Items)
        {
            if (child is not Note note) continue;
            note.Visible = predicate(note);
        }
    }

    private bool SearchConditionIsMet(Note note)
    {
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

        if (_selectedTags.Count <= 0) return condition1;
        foreach (var tag in _selectedTags)
            if (note.Tags.Contains(tag))
                return condition1;

        return false;
    }

    private void PickRandomCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { IsChecked: var b } || RandomPickMax is not { Value: var max } ||
            RandomPickMin is not { Value: var min }) return;
        if (b is true)
        {
            var rnd = new Random();
            var pick = rnd.Next((int)min, (int)max);
            _randomNote = Bank.Notes.Where(n => n.Visible).ToArray()[pick];
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
        foreach (var item in NotesIC.Items)
        {
            if (item is not Note { Visible: true } note) return;
            note.Checked = c is true;
        }
    }

    private void InvertSelection(object? sender, RoutedEventArgs e)
    {
        foreach (var item in NotesIC.Items)
        {
            if (item is not Note { Visible: true } note) return;
            note.Checked = !note.Checked;
        }
    }

    private void RemoveSelected(object? sender, RoutedEventArgs e)
    {
        foreach (var note in Bank.Notes)
        {
            if (!note.Visible || !note.Checked) continue;
            Bank.Remove(note);
        }
    }

    private void SearchBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        DoSearch(GetPredicate);
    }

    private void NewNoteTagChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { IsChecked: var c, Tag: Tag tag }) return;
        if (c is true) _newNoteTags.Add(tag);
        else _newNoteTags.Remove(tag);
        tag.NewNoteChecked = c is true;
    }

    private void RandomColorClick(object? sender, RoutedEventArgs e)
    {
        var rnd = new Random();
        NewTagColor = Color.FromRgb((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256));
    }

    private void NewTagClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewTagText)) return;
        Bank.Add(new Tag(NewTagText, NewTagColor, Bank));
    }

    private void AddNewNoteClicked(object? sender, RoutedEventArgs e)
    {
        if (NewText is not { Text: var text } || string.IsNullOrWhiteSpace(text)) return;
        Bank.Add(new Note(text, _newNoteTags.ToArray(), DateTime.Now, Bank));
        if (!Settings.KeepText)
        {
            NewText.Text = string.Empty;
            _selectedTags.Clear();
            foreach (var item in NewNoteTagsIC.Items)
            {
                if (item is not Tag tag) continue;
                tag.NewNoteChecked = false;
            }
        }
    }

    private void GoBack(object? sender, RoutedEventArgs e)
    {
        Bank.Unload();
        Main?.GoBack();
    }

    private async void SaveClicked(object? sender, RoutedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() => Bank.Write());
    }
}