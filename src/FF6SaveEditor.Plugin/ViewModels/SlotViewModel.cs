using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FF6SaveEditor.Core.Models;
using SaveEditor.Shell.Abstractions;

namespace FF6SaveEditor.Plugin.ViewModels;

public partial class SlotViewModel : ObservableObject, ISlotViewModel
{
    private readonly SaveSlot _slot;
    private readonly Action _markDirty;

    [ObservableProperty]
    private string _header;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private CharacterViewModel? _selectedCharacter;

    [ObservableProperty]
    private uint _gil;

    [ObservableProperty]
    private byte _hours;

    [ObservableProperty]
    private byte _minutes;

    [ObservableProperty]
    private byte _seconds;

    [ObservableProperty]
    private uint _steps;

    public ObservableCollection<CharacterViewModel> Characters { get; } = new();
    public InventoryViewModel Inventory { get; }

    public string GameTimeDisplay => $"{Hours}:{Minutes:D2}:{Seconds:D2}";

    public SlotViewModel(SaveSlot slot, int slotNumber, Action markDirty)
    {
        _slot = slot;
        _markDirty = markDirty;
        _isValid = slot.IsValid;
        _header = $"Slot {slotNumber}" + (slot.IsValid ? "" : " (Empty)");
        _gil = slot.Gil;
        _hours = slot.Hours;
        _minutes = slot.Minutes;
        _seconds = slot.Seconds;
        _steps = slot.Steps;

        for (int i = 0; i < SaveSlot.CharacterCount; i++)
        {
            var charVm = new CharacterViewModel(slot.Characters[i], markDirty);
            if (!charVm.IsEmpty)
                Characters.Add(charVm);
        }

        Inventory = new InventoryViewModel(slot.Inventory, markDirty);

        if (Characters.Count > 0)
            SelectedCharacter = Characters[0];
    }

    partial void OnGilChanged(uint value)
    {
        _slot.Gil = Math.Min(value, 9_999_999u);
        _markDirty();
    }

    partial void OnHoursChanged(byte value)
    {
        _slot.Hours = value;
        OnPropertyChanged(nameof(GameTimeDisplay));
        _markDirty();
    }

    partial void OnMinutesChanged(byte value)
    {
        _slot.Minutes = Math.Min(value, (byte)59);
        OnPropertyChanged(nameof(GameTimeDisplay));
        _markDirty();
    }

    partial void OnSecondsChanged(byte value)
    {
        _slot.Seconds = Math.Min(value, (byte)59);
        OnPropertyChanged(nameof(GameTimeDisplay));
        _markDirty();
    }

    partial void OnStepsChanged(uint value)
    {
        _slot.Steps = value;
        _markDirty();
    }
}
