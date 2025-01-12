using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Avalonia.Styling;

namespace Mindbank.Backend;

public sealed class Settings : INotifyPropertyChanged
{
    private static Stream? _settingsFileStream;
    private readonly List<Bank> _banks = [];
    private bool _alreadyLoaded;
    private byte _blurLevel = 75;
    private bool _isLoading;
    private bool _powerMode;
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

    public Bank[] Banks
    {
        get
        {
            var list = new Bank[Count];
            for (var i = 0; i < Count; i++) list[i] = this[i];
            return list;
        }
    }

    private int Count => _banks.Count;

    private Bank this[int index] => _banks[index];

    public bool PowerMode
    {
        get => _powerMode;
        set
        {
            _powerMode = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerMode)));
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

    public static bool IsInstanceRunning { get; private set; }

    public static byte Version => 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static void SetupSingleton()
    {
        try
        {
            _settingsFileStream =
                new FileStream(SettingsFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (Exception)
        {
            IsInstanceRunning = true;
        }
    }

    public static void RemoveSingleton()
    {
        if (_settingsFileStream is null) return;
        _settingsFileStream.Close();
        _settingsFileStream.Dispose();
        _settingsFileStream = null;
    }

    public void Load(bool force = false)
    {
        if (_alreadyLoaded && !force) return;
        if (!Directory.Exists(AppFolder) || !File.Exists(SettingsFile)) return;
        if (_settingsFileStream is not { } stream) return;
        _isLoading = true;
        var version = stream.ReadByte();
        if (version < 0 || version > Version) return;
        var theme = stream.ReadByte();
        theme -= Tools.IsBitSet(theme, 3) ? 8 : 0;
        theme -= Tools.IsBitSet(theme, 4) ? 16 : 0;
        UseBlur = Tools.IsBitSet(theme, 2);
        theme -= UseBlur ? 4 : 0;
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
        if (_settingsFileStream is not { } stream) return;
        stream.SetLength(0);
        stream.WriteByte(Version);
        byte booleans = 0;
        if (Theme == ThemeVariant.Default)
            booleans = 0;
        else if (Theme == ThemeVariant.Dark)
            booleans = 1;
        else if (Theme == ThemeVariant.Light)
            booleans = 2;
        booleans += (byte)(UseBlur ? 4 : 0);
        stream.WriteByte(booleans);
        stream.WriteByte(_blurLevel);

        Tools.WriteVarInt(stream, Count);
        foreach (var ns in _banks)
            ns.SetInfo(stream);
    }

    private void Add(Bank bank)
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
}