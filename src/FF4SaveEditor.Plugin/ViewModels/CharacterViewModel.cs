using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FF4SaveEditor.Core.GameData;
using FF4SaveEditor.Core.Models;
using FF4SaveEditor.Core.Services;

namespace FF4SaveEditor.Plugin.ViewModels;

public partial class CharacterViewModel : ObservableObject
{
    private readonly CharacterData _character;
    private readonly Action _markDirty;

    public string Name => _character.DisplayName;
    public bool IsEmpty => _character.IsEmpty;

    [ObservableProperty] private byte _level;
    [ObservableProperty] private ushort _currentHp;
    [ObservableProperty] private ushort _maxHp;
    [ObservableProperty] private ushort _currentMp;
    [ObservableProperty] private ushort _maxMp;
    [ObservableProperty] private byte _strength;
    [ObservableProperty] private byte _agility;
    [ObservableProperty] private byte _stamina;
    [ObservableProperty] private byte _intellect;
    [ObservableProperty] private byte _spirit;
    [ObservableProperty] private uint _experience;
    [ObservableProperty] private bool _backRow;

    [ObservableProperty] private ItemDef? _selectedHelmet;
    [ObservableProperty] private ItemDef? _selectedArmor;
    [ObservableProperty] private ItemDef? _selectedAccessory;
    [ObservableProperty] private ItemDef? _selectedRightHand;
    [ObservableProperty] private ItemDef? _selectedLeftHand;

    public ObservableCollection<ItemDef> AvailableHelmets { get; } = new();
    public ObservableCollection<ItemDef> AvailableArmors { get; } = new();
    public ObservableCollection<ItemDef> AvailableAccessories { get; } = new();
    public ObservableCollection<ItemDef> AvailableRightHand { get; } = new();
    public ObservableCollection<ItemDef> AvailableLeftHand { get; } = new();

    public CharacterViewModel(CharacterData character, Action markDirty)
    {
        _character = character;
        _markDirty = markDirty;

        _level = character.Level;
        _currentHp = character.CurrentHp;
        _maxHp = character.MaxHp;
        _currentMp = character.CurrentMp;
        _maxMp = character.MaxMp;
        _strength = character.Strength;
        _agility = character.Agility;
        _stamina = character.Stamina;
        _intellect = character.Intellect;
        _spirit = character.Spirit;
        _experience = character.Experience;
        _backRow = character.BackRow;

        if (!IsEmpty)
        {
            PopulateEquipmentLists();
        }
    }

    private void PopulateEquipmentLists()
    {
        var db = ItemDb.Instance;
        var charId = _character.CharacterId;
        var noneItem = db.GetById(0);

        PopulateList(AvailableHelmets, db.GetEquippableBy(charId, ItemCategory.Helmet), noneItem);
        PopulateList(AvailableArmors, db.GetEquippableBy(charId, ItemCategory.BodyArmor), noneItem);
        PopulateList(AvailableAccessories, db.GetEquippableBy(charId, ItemCategory.Accessory), noneItem);
        PopulateList(AvailableRightHand, db.GetEquippableBy(charId, EquipSlot.RightHand), noneItem);
        PopulateList(AvailableLeftHand, db.GetEquippableBy(charId, EquipSlot.LeftHand), noneItem);

        SelectedHelmet = FindItem(AvailableHelmets, _character.HelmetId);
        SelectedArmor = FindItem(AvailableArmors, _character.ArmorId);
        SelectedAccessory = FindItem(AvailableAccessories, _character.AccessoryId);
        SelectedRightHand = FindItem(AvailableRightHand, _character.RightHandItemId);
        SelectedLeftHand = FindItem(AvailableLeftHand, _character.LeftHandItemId);
    }

    private static void PopulateList(ObservableCollection<ItemDef> list, IEnumerable<ItemDef> items, ItemDef noneItem)
    {
        list.Clear();
        list.Add(noneItem);
        foreach (var item in items.Where(i => i.Id != 0))
            list.Add(item);
    }

    private static ItemDef? FindItem(ObservableCollection<ItemDef> list, byte id)
        => list.FirstOrDefault(i => i.Id == id) ?? list.FirstOrDefault();

    partial void OnLevelChanged(byte value) { _character.Level = value; _markDirty(); }
    partial void OnCurrentHpChanged(ushort value) { _character.CurrentHp = value; _markDirty(); }
    partial void OnMaxHpChanged(ushort value) { _character.MaxHp = value; _markDirty(); }
    partial void OnCurrentMpChanged(ushort value) { _character.CurrentMp = value; _markDirty(); }
    partial void OnMaxMpChanged(ushort value) { _character.MaxMp = value; _markDirty(); }
    partial void OnStrengthChanged(byte value) { _character.Strength = value; _markDirty(); }
    partial void OnAgilityChanged(byte value) { _character.Agility = value; _markDirty(); }
    partial void OnStaminaChanged(byte value) { _character.Stamina = value; _markDirty(); }
    partial void OnIntellectChanged(byte value) { _character.Intellect = value; _markDirty(); }
    partial void OnSpiritChanged(byte value) { _character.Spirit = value; _markDirty(); }
    partial void OnExperienceChanged(uint value) { _character.Experience = value; _markDirty(); }
    partial void OnBackRowChanged(bool value) { _character.BackRow = value; _markDirty(); }

    partial void OnSelectedHelmetChanged(ItemDef? value)
    {
        if (value != null) { _character.HelmetId = value.Id; _markDirty(); }
    }
    partial void OnSelectedArmorChanged(ItemDef? value)
    {
        if (value != null) { _character.ArmorId = value.Id; _markDirty(); }
    }
    partial void OnSelectedAccessoryChanged(ItemDef? value)
    {
        if (value != null) { _character.AccessoryId = value.Id; _markDirty(); }
    }
    partial void OnSelectedRightHandChanged(ItemDef? value)
    {
        if (value != null) { _character.RightHandItemId = value.Id; _markDirty(); }
    }
    partial void OnSelectedLeftHandChanged(ItemDef? value)
    {
        if (value != null) { _character.LeftHandItemId = value.Id; _markDirty(); }
    }

    [RelayCommand]
    public void MaxStats()
    {
        if (IsEmpty) return;

        Level = 99;
        MaxHp = 9999;
        CurrentHp = 9999;
        MaxMp = 9999;
        CurrentMp = 9999;
        Strength = 99;
        Agility = 99;
        Stamina = 99;
        Intellect = 99;
        Spirit = 99;
        Experience = 9_999_999;
    }

    [RelayCommand]
    public void ApplyBestLoadout()
    {
        if (IsEmpty) return;

        var best = BestLoadout.Get(_character.CharacterId);

        SelectedRightHand = FindItem(AvailableRightHand, best.RightHand);
        SelectedLeftHand = FindItem(AvailableLeftHand, best.LeftHand);
        SelectedHelmet = FindItem(AvailableHelmets, best.Helmet);
        SelectedArmor = FindItem(AvailableArmors, best.Armor);
        SelectedAccessory = FindItem(AvailableAccessories, best.Accessory);
    }

    public override string ToString() => Name;
}
