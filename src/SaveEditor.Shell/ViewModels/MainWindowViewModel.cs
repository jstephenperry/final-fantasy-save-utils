using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SaveEditor.Shell.Abstractions;

namespace SaveEditor.Shell.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IGamePlugin[] _plugins;

    [ObservableProperty]
    private string _title = "Final Fantasy Save Editor";

    [ObservableProperty]
    private string _statusMessage = "Select a game and open a file";

    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private int _selectedSlotIndex;

    [ObservableProperty]
    private ISlotViewModel? _selectedSlot;

    [ObservableProperty]
    private IGamePlugin? _selectedGame;

    public ObservableCollection<ISlotViewModel> Slots { get; } = new();
    public IReadOnlyList<IGamePlugin> Games { get; }

    private IGamePlugin? _activePlugin;
    private string? _currentFilePath;
    private bool _isDirty;

    public MainWindowViewModel(IGamePlugin[] plugins)
    {
        _plugins = plugins;
        Games = plugins;
        if (plugins.Length > 0)
            SelectedGame = plugins[0];
    }

    partial void OnSelectedSlotIndexChanged(int value)
    {
        if (value >= 0 && value < Slots.Count)
            SelectedSlot = Slots[value];
    }

    partial void OnSelectedGameChanged(IGamePlugin? value)
    {
        if (value != null && _activePlugin != value)
        {
            // Switching games clears current file
            _activePlugin = value;
            Slots.Clear();
            IsFileLoaded = false;
            _currentFilePath = null;
            _isDirty = false;
            Title = $"{value.GameName} Save Editor";
            StatusMessage = $"Selected {value.GameName} - Open a file to begin editing";
        }
    }

    public async Task OpenFile(Window window)
    {
        if (_activePlugin == null) return;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _activePlugin.OpenDialogTitle,
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("SNES Save RAM") { Patterns = new[] { "*.srm", "*.sav" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } },
            }
        });

        if (files.Count == 0) return;

        var path = files[0].TryGetLocalPath();
        if (path == null) return;

        LoadFile(path);
    }

    public void LoadFile(string path)
    {
        if (_activePlugin == null) return;

        try
        {
            var data = File.ReadAllBytes(path);
            var slots = _activePlugin.Load(data, MarkDirty);
            _currentFilePath = path;

            Slots.Clear();
            foreach (var slot in slots)
                Slots.Add(slot);

            IsFileLoaded = true;
            _isDirty = false;
            SelectedSlotIndex = 0;
            if (Slots.Count > 0)
                SelectedSlot = Slots[0];

            Title = $"{_activePlugin.GameName} Save Editor - {Path.GetFileName(path)}";
            StatusMessage = $"Loaded: {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    private void MarkDirty()
    {
        _isDirty = true;
        if (!Title.EndsWith('*'))
            Title += "*";
    }

    public async Task SaveFile(Window window)
    {
        if (_activePlugin == null) return;

        if (_currentFilePath != null)
        {
            SaveToPath(_currentFilePath);
        }
        else
        {
            await SaveFileAs(window);
        }
    }

    public async Task SaveFileAs(Window window)
    {
        if (_activePlugin == null) return;

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = _activePlugin.SaveDialogTitle,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("SNES Save RAM") { Patterns = new[] { "*.srm", "*.sav" } },
            }
        });

        if (file == null) return;

        var path = file.TryGetLocalPath();
        if (path == null) return;

        SaveToPath(path);
    }

    private void SaveToPath(string path)
    {
        if (_activePlugin == null) return;

        try
        {
            var data = _activePlugin.Save();
            File.WriteAllBytes(path, data);
            _currentFilePath = path;
            _isDirty = false;
            Title = $"{_activePlugin.GameName} Save Editor - {Path.GetFileName(path)}";
            StatusMessage = $"Saved: {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }
}
