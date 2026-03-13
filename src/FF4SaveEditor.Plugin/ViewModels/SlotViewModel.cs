using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FF4SaveEditor.Core.Models;
using FF4SaveEditor.Core.Services;
using SaveEditor.Shell.Abstractions;

namespace FF4SaveEditor.Plugin.ViewModels;

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

    public ObservableCollection<CharacterViewModel> Characters { get; } = new();
    public InventoryViewModel Inventory { get; }
    public ObservableCollection<StoryMilestone> StoryProgress { get; } = new();

    public SlotViewModel(SaveSlot slot, int slotNumber, Action markDirty)
    {
        _slot = slot;
        _markDirty = markDirty;
        _isValid = slot.IsValid;
        _header = $"Slot {slotNumber}" + (slot.IsValid ? "" : " (Empty)");
        _gil = slot.Gil;

        for (int i = 0; i < SaveSlot.CharacterCount; i++)
        {
            var charVm = new CharacterViewModel(slot.Characters[i], markDirty);
            Characters.Add(charVm);
        }

        Inventory = new InventoryViewModel(slot.Inventory, markDirty);

        if (slot.IsValid)
        {
            foreach (var m in Core.Services.StoryProgress.Analyze(slot))
                StoryProgress.Add(m);
        }

        if (Characters.Count > 0)
            SelectedCharacter = Characters[0];
    }

    partial void OnGilChanged(uint value)
    {
        _slot.Gil = Math.Min(value, 16_777_215u);
        _markDirty();
    }

    [RelayCommand]
    private void MaxAllStats()
    {
        foreach (var c in Characters)
            c.MaxStats();
    }

    [RelayCommand]
    private void BestAllLoadouts()
    {
        foreach (var c in Characters)
            c.ApplyBestLoadout();
    }
}
