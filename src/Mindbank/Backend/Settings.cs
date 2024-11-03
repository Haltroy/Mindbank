using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Styling;

namespace Mindbank.Backend;

public class Settings : INotifyPropertyChanged
{
    private readonly List<Bank> _banks = [];
    private bool _alreadyLoaded;
    private byte _blurLevel = 75;
    private bool _hideToSysTray = true;
    private bool _isLoading;
    private bool _keepText;
    private bool _startInTray;
    private ThemeVariant _theme = ThemeVariant.Default;
    private bool _useBlur = true;

    public double BlurLevel
    {
        get => _blurLevel;
        set
        {
            if (value is < 0 or > byte.MaxValue) _blurLevel = 75;
            _blurLevel = (byte)value;
            if (!_isLoading)
                Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlurLevel)));
        }
    }

    public bool HideToSysTray
    {
        get => _hideToSysTray;
        set
        {
            _hideToSysTray = value;
            if (!_isLoading)
                Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideToSysTray)));
        }
    }

    public Bank[] Banks
    {
        get
        {
            var list = new Bank[Count];
            for (var i = 0; i < Count; i++) list[i] = this[i];
            return list;
        }
    }

    public int Count => _banks.Count;

    public Bank this[int index] => _banks[index];

    public Bank this[string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase]
    {
        get
        {
            foreach (var bank in _banks)
                if (bank.Name.Equals(name, stringComparison))
                    return bank;

            throw new Exception($"Can't find bank with name '{name}'");
        }
    }

    public bool KeepText
    {
        get => _keepText;
        set
        {
            _keepText = value;
            if (!_isLoading)
                Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KeepText)));
        }
    }

    public bool StartInTray
    {
        get => _startInTray;
        set
        {
            _startInTray = value;
            if (!_isLoading)
                Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartInTray)));
        }
    }

    public ThemeVariant Theme
    {
        get => _theme;
        set
        {
            _theme = value;

            if (!_isLoading)
                Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
        }
    }

    public bool UseBlur
    {
        get => _useBlur;
        set
        {
            _useBlur = value;
            if (!_isLoading)
                Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseBlur)));
        }
    }

    private static string AppFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "haltroy", "mindbank");

    private static string SettingsFile => Path.Combine(AppFolder, "settings");
    private static string SourcesFolder => Path.Combine(AppFolder, "sources");

    public static byte Version => 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Load(bool force = false)
    {
        if (_alreadyLoaded && !force) return;
        if (!Directory.Exists(AppFolder) || !File.Exists(SettingsFile)) return;
        using var stream = new FileStream(SettingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _isLoading = true;
        var version = stream.ReadByte();
        if (version < 0 || version > Version) return;
        var theme = stream.ReadByte();
        UseBlur = Tools.isBitSet((byte)theme, 2);
        theme -= UseBlur ? 4 : 0;
        HideToSysTray = Tools.isBitSet((byte)theme, 3);
        theme -= HideToSysTray ? 8 : 0;
        KeepText = Tools.isBitSet((byte)theme, 4);
        theme -= KeepText ? 16 : 0;
        StartInTray = Tools.isBitSet((byte)theme, 4);
        theme -= StartInTray ? 32 : 0;
        switch (theme)
        {
            case 0:
                Theme = ThemeVariant.Default;
                break;
            case 1:
                Theme = ThemeVariant.Dark;
                break;
            case 2:
                Theme = ThemeVariant.Light;
                break;
            default:
                return;
        }

        BlurLevel = stream.ReadByte();
        var sourcesCount = Tools.DecodeVarInt(stream);
        for (var i = 0; i < sourcesCount; i++)
        {
            var bank = new Bank(this);
            bank.GetInfo(stream);
            _banks.Add(bank);
        }

        _isLoading = false;
        _alreadyLoaded = true;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Settings"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void Save()
    {
        if (_isLoading) return;
        if (!Directory.Exists(AppFolder)) Directory.CreateDirectory(AppFolder);
        using var stream = File.Exists(SettingsFile)
            ? new FileStream(SettingsFile, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite)
            : File.Create(SettingsFile);
        stream.WriteByte(Version);
        byte booleans = 0;
        if (Theme == ThemeVariant.Default)
            booleans = 0;
        else if (Theme == ThemeVariant.Dark)
            booleans = 1;
        else if (Theme == ThemeVariant.Light)
            booleans = 2;
        booleans += (byte)(UseBlur ? 4 : 0);
        booleans += (byte)(HideToSysTray ? 8 : 0);
        booleans += (byte)(KeepText ? 16 : 0);
        booleans += (byte)(StartInTray ? 32 : 0);
        stream.WriteByte(booleans);
        stream.WriteByte(_blurLevel);

        Tools.WriteVarInt(stream, Count);
        foreach (var ns in _banks)
            ns.SetInfo(stream);
    }

    public void Add(Bank bank)
    {
        _banks.Add(bank);
        Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void Remove(Bank bank)
    {
        _banks.Remove(bank);
        if (!_isLoading)
            Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void Remove(string name, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        RemoveAll(bank => bank.Name.Equals(name, stringComparison));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void Insert(int index, Bank bank)
    {
        _banks.Insert(index, bank);
        if (!_isLoading)
            Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void AddRange(IEnumerable<Bank> banks)
    {
        _banks.AddRange(banks);
        if (!_isLoading)
            Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void RemoveAll()
    {
        _banks.Clear();
        if (!_isLoading)
            Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void RemoveAll(Predicate<Bank> match)
    {
        _banks.RemoveAll(match);
        if (!_isLoading)
            Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public void RemoveAt(int index)
    {
        _banks.RemoveAt(index);
        if (!_isLoading)
            Save();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Banks)));
    }

    public Bank NewNote(string name)
    {
        if (!Directory.Exists(SourcesFolder)) Directory.CreateDirectory(SourcesFolder);
        Bank ns = new(this)
        {
            Name = name,
            FilePath = GenerateNewFileName()
        };
        ns.Write();
        Add(ns);
        return ns;
    }

    public void ImportNote(Stream file)
    {
        if (!Directory.Exists(SourcesFolder)) Directory.CreateDirectory(SourcesFolder);
        var generatedName = GenerateNewFileName();
        using var stream = File.Create(generatedName);
        file.CopyTo(stream);
        file.Close();
        Bank ns = new(this)
        {
            FilePath = generatedName
        };
        Add(ns);
    }

    private static string GenerateNewFileName()
    {
        if (!Directory.Exists(SourcesFolder)) Directory.CreateDirectory(SourcesFolder);
        while (true)
        {
            var generated = Tools.GenerateRandomText();
            var path = Path.Combine(SourcesFolder, generated);
            if (!File.Exists(path)) return path;
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