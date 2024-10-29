using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Avalonia.Media;
using Mindbank.Backend.Exceptions;

namespace Mindbank.Backend;

public class Bank : INotifyPropertyChanged
{
    private readonly List<Note> _notes = [];
    private readonly List<Tag> _tags = [];

    private bool _init;

    private bool _isSaving;
    private string _name = string.Empty;
    private string _path = string.Empty;

    private bool _save;

    public Bank(Settings? settings)
    {
        Settings = settings;
    }

    public Settings? Settings { get; }
    public Bank Self => this;

    /// <summary>
    ///     The name of the bank.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            Settings?.Save();
        }
    }

    /// <summary>
    ///     The file path of the bank.
    /// </summary>
    public string FilePath
    {
        get => _path;
        set
        {
            _path = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
            Settings?.Save();
        }
    }

    public bool NeedsSaving
    {
        get => _save;
        set
        {
            _save = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NeedsSaving)));
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            _isSaving = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSaving)));
        }
    }

    private int Version { get; set; }
    public Note[] Notes => _notes.ToArray();

    public Note this[int index] => _notes[index];
    public Tag this[int index, bool isTag = true] => _tags[index];

    public int Count => _notes.Count;

    public int VisibleCount
    {
        get { return _notes.Count(n => n.Visible); }
    }

    public int TagCount => _tags.Count;

    public Tag[] Tags => _tags.ToArray();

    public bool VersionMatch => Version == Settings.Version;
    public bool LimitNotReached => Count < int.MaxValue;
    public bool CanWrite => VersionMatch && LimitNotReached;

    public bool IsExample { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static Bank GenerateExampleBank()
    {
        var bank = new Bank(null)
        {
            IsExample = true,
            Name = "Example",
            FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "example")
        };
        bank.Add(new Note("This is a test.", [], DateTime.Now, bank));
        var tag1 = new Tag("Test1", Colors.Gold, bank);
        bank.Add(tag1);
        bank.Add(new Note("This is a test with a tag.", [tag1], DateTime.Now, bank));
        return bank;
    }

    public void Unload()
    {
        _init = true;
        foreach (var note in _notes) note.Bank = null;
        _notes.Clear();
        foreach (var tag in _tags) tag.Bank = null;
        _tags.Clear();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        _init = false;
    }

    public void Add(Note note)
    {
        if (!CanWrite) return;
        _notes.Add(note);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        if (!_init) NeedsSaving = true;
    }

    public void Add(Tag tag)
    {
        if (!CanWrite) return;
        _tags.Add(tag);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        if (!_init) NeedsSaving = true;
    }

    public void AddRange(IEnumerable<Note> note)
    {
        if (!CanWrite) return;
        _notes.AddRange(note);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        if (!_init) NeedsSaving = true;
    }

    public void AddRange(IEnumerable<Tag> tags)
    {
        if (!CanWrite) return;
        _tags.AddRange(tags);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        if (!_init) NeedsSaving = true;
    }

    public void Insert(int index, Note note)
    {
        if (!CanWrite) return;
        _notes.Insert(index, note);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        if (!_init) NeedsSaving = true;
    }

    public void InsertRange(int index, IEnumerable<Note> notes)
    {
        if (!CanWrite) return;
        _notes.InsertRange(index, notes);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        if (!_init) NeedsSaving = true;
    }

    public void Insert(int index, Tag tag)
    {
        if (!CanWrite) return;
        _tags.Insert(index, tag);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        if (!_init) NeedsSaving = true;
    }

    public void InsertRange(int index, IEnumerable<Tag> tags)
    {
        if (!CanWrite) return;
        _tags.InsertRange(index, tags);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        if (!_init) NeedsSaving = true;
    }

    public void Remove(Note note)
    {
        if (!CanWrite) return;
        _notes.Remove(note);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        if (!_init) NeedsSaving = true;
    }

    public void Remove(Tag tag)
    {
        if (!CanWrite) return;
        _tags.Remove(tag);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        if (!_init) NeedsSaving = true;
    }

    public void RemoveAt(int index, bool isTag = false)
    {
        if (!CanWrite) return;
        if (isTag)
        {
            _tags.RemoveAt(index);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
            foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        }
        else
        {
            _notes.RemoveAt(index);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        }

        if (!_init) NeedsSaving = true;
    }

    public void RemoveRange(int index, int count, bool isTag = false)
    {
        if (!CanWrite) return;
        if (isTag)
        {
            _tags.RemoveRange(index, count);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
            foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        }
        else
        {
            _notes.RemoveRange(index, count);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        }

        if (!_init) NeedsSaving = true;
    }

    public void RemoveAll(Predicate<Note> predicate)
    {
        if (!CanWrite) return;
        _notes.RemoveAll(predicate);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
        if (!_init) NeedsSaving = true;
    }

    public void RemoveAll(Predicate<Tag> predicate)
    {
        if (CanWrite)
            _tags.RemoveAll(predicate);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        foreach (var note in _notes) note.RequestPropertyChangeInvoke();
        if (!_init) NeedsSaving = true;
    }

    public int IndexOf(Note note)
    {
        return _notes.IndexOf(note);
    }

    public int IndexOf(Tag tag, bool isTag = true)
    {
        return _tags.IndexOf(tag);
    }

    public void GetInfo(Stream stream)
    {
        Name = Encoding.Unicode.GetString(Tools.DecodeByteArrWithVarInt(stream));
        FilePath = Encoding.Unicode.GetString(Tools.DecodeByteArrWithVarInt(stream));
    }

    public void SetInfo(Stream stream)
    {
        Tools.WriteByteArrWithVarInt(stream, Encoding.Unicode.GetBytes(Name));
        Tools.WriteByteArrWithVarInt(stream, Encoding.Unicode.GetBytes(FilePath));
    }

    public async void Read()
    {
        if (IsExample)
        {
            Thread.Sleep(5000);
            return;
        }

        await using var fileStream =
            new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        LoadFromStream(fileStream);
    }

    public async void Write()
    {
        IsSaving = true;
        if (!CanWrite) return;
        await using var fileStream = !File.Exists(FilePath)
            ? File.Create(FilePath)
            : File.Open(FilePath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
        SaveToStream(fileStream);
        NeedsSaving = false;
        IsSaving = false;
    }

    private void LoadFromStream(Stream stream)
    {
        _init = true;
        var version = stream.ReadByte();
        if (version < 0) throw new MindbankEndOfFileException(0, nameof(stream));
        var r_hasTags = Tools.isBitSet((byte)version, 7);
        if (r_hasTags) version -= 128;
        var r_hasItems = Tools.isBitSet((byte)version, 6);
        if (r_hasItems) version -= 64;
        Version = version;
        if (r_hasTags)
        {
            var tags_count = Tools.DecodeVarInt(stream);
            for (var ignored = 0; ignored < tags_count; ignored++)
            {
                var color = Tools.DecodeVarUInt(stream);
                var text = Encoding.Unicode.GetString(Tools.DecodeByteArrWithVarInt(stream));
                _tags.Add(new Tag(text, Color.FromUInt32(color), this));
            }
        }

        if (!r_hasItems) return;

        var count = Tools.DecodeVarInt(stream);
        for (var ignored = 0; ignored < count; ignored++)
        {
            Tag[] tags = [];
            var type = stream.ReadByte();
            if (type < 0) throw new MindbankEndOfFileException(7, "" + ignored);
            var hasTags = Tools.isBitSet((byte)type, 7);
            if (hasTags)
            {
                type -= 128;
                var tagCount = Tools.DecodeVarInt(stream);
                tags = new Tag[tagCount];
                for (var ignored2 = 0; ignored2 < tagCount; ignored2++)
                {
                    var index = Tools.DecodeVarInt(stream);
                    tags[ignored2] = Tags[index];
                }
            }

            switch (type)
            {
                case 0:
                    var text = Tools.DecodeByteArrWithVarInt(stream);
                    var date = Tools.DecodeVarLong(stream);
                    _notes.Add(new Note(Encoding.Unicode.GetString(text), tags, DateTime.FromBinary(date),
                        this));
                    break;

                // no other type is implemented yet, so we don't have to deal with this here for now.
            }
        }

        _init = false;
    }

    private void SaveToStream(Stream stream)
    {
        var version = Settings.Version;
        if (Count > 0) version += 64;
        if (TagCount > 0) version += 128;
        stream.WriteByte(version);
        if (TagCount > 0)
        {
            Tools.WriteVarInt(stream, TagCount);
            foreach (var tag in Tags)
            {
                Tools.WriteVarUInt(stream, tag.Color.ToUInt32());
                Tools.WriteByteArrWithVarInt(stream, Encoding.Unicode.GetBytes(tag.Text));
            }
        }

        if (Count <= 0) return;
        Tools.WriteVarInt(stream, Count);
        foreach (var note in Notes)
        {
            // no other type is implemented yet
            var type = 0 +
                       (note.Tags.Length > 0 ? 128 : 0);
            stream.WriteByte((byte)type);
            if (note.Tags.Length > 0)
            {
                Tools.WriteVarInt(stream, note.Tags.Length);
                foreach (var tag in note.Tags) Tools.WriteVarInt(stream, tag.Index);
            }

            Tools.WriteByteArrWithVarInt(stream, Encoding.Unicode.GetBytes(note.Text));
            Tools.WriteVarLong(stream, note.Date.ToBinary());
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public class Tag : INotifyPropertyChanged
{
    private readonly bool _init;
    private Color _color;
    private bool _newNoteChecked;
    private string _text = string.Empty;

    public Tag(string text, Color color, Bank? bank)
    {
        _init = true;
        Bank = bank;
        Color = color;
        Text = text;
        _init = false;
    }

    public Tag Self => this;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (!_init && Bank != null) Bank.NeedsSaving = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            if (!_init && Bank != null) Bank.NeedsSaving = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
        }
    }

    public Bank? Bank { get; internal set; }
    public int Index => Bank?.IndexOf(this) ?? -1;

    public bool NewNoteChecked
    {
        get => _newNoteChecked;
        set
        {
            _newNoteChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewNoteChecked)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public record NoteTag(Note Note, Tag Tag)
{
    public NoteTag Self => this;
    public bool NoteHasTag => Note.Tags.Contains(Tag);
}

public class Note : INotifyPropertyChanged
{
    private readonly bool _init;

    private bool _checked;
    private DateTime _date = DateTime.MinValue;
    private Tag[] _tags = [];
    private string _text = string.Empty;

    private bool _visible = true;

    public Note(string text, Tag[] tags, DateTime date, Bank bank)
    {
        _init = true;
        Bank = bank;
        Text = text;
        Tags = tags;
        Date = date;
        _init = false;
    }

    public Note Self => this;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (!_init && Bank != null) Bank.NeedsSaving = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }

    public Tag[] Tags
    {
        get => _tags;
        set
        {
            _tags = value;
            if (!_init && Bank != null) Bank.NeedsSaving = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
        }
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            _date = value;
            if (!_init && Bank != null) Bank.NeedsSaving = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
        }
    }

    public NoteTag[] NoteTagCombination
    {
        get
        {
            if (Bank is null) return [];
            var tagCombo = new NoteTag[Bank.Tags.Length];
            for (var i = 0; i < tagCombo.Length; i++) tagCombo[i] = new NoteTag(this, Bank.Tags[i]);
            return tagCombo;
        }
    }

    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
        }
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Checked)));
        }
    }


    public string DateAsString => Date.ToString("f");

    public Bank? Bank { get; internal set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RequestPropertyChangeInvoke()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NoteTagCombination)));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}