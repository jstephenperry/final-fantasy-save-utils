using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FF6SaveEditor.Core.GameData;
using FF6SaveEditor.Core.Models;

namespace FF6SaveEditor.Plugin.ViewModels;

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
    [ObservableProperty] private byte _vigor;
    [ObservableProperty] private byte _speed;
    [ObservableProperty] private byte _stamina;
    [ObservableProperty] private byte _magicPower;
    [ObservableProperty] private uint _experience;

    [ObservableProperty] private ItemDef? _selectedWeapon;
    [ObservableProperty] private ItemDef? _selectedShield;
    [ObservableProperty] private ItemDef? _selectedHelmet;
    [ObservableProperty] private ItemDef? _selectedArmor;
    [ObservableProperty] private ItemDef? _selectedRelic1;
    [ObservableProperty] private ItemDef? _selectedRelic2;

    public ObservableCollection<ItemDef> AvailableWeapons { get; } = new();
    public ObservableCollection<ItemDef> AvailableShields { get; } = new();
    public ObservableCollection<ItemDef> AvailableHelmets { get; } = new();
    public ObservableCollection<ItemDef> AvailableArmors { get; } = new();
    public ObservableCollection<ItemDef> AvailableRelics { get; } = new();

    public CharacterViewModel(CharacterData character, Action markDirty)
    {
        _character = character;
        _markDirty = markDirty;

        _level = character.Level;
        _currentHp = character.CurrentHp;
        _maxHp = character.MaxHp;
        _currentMp = character.CurrentMp;
        _maxMp = character.MaxMp;
        _vigor = character.Vigor;
        _speed = character.Speed;
        _stamina = character.Stamina;
        _magicPower = character.MagicPower;
        _experience = character.Experience;

        if (!IsEmpty)
        {
            PopulateEquipmentLists();
        }
    }

    private void PopulateEquipmentLists()
    {
        var db = ItemDb.Instance;
        var actorId = _character.ActorId;
        var emptyItem = new ItemDef { Id = 0xFF, Name = "(Empty)", Category = ItemCategory.Consumable };

        PopulateList(AvailableWeapons, db.GetEquippableBy(actorId, ItemCategory.Weapon), emptyItem);
        PopulateList(AvailableShields, db.GetEquippableBy(actorId, ItemCategory.Shield), emptyItem);
        PopulateList(AvailableHelmets, db.GetEquippableBy(actorId, ItemCategory.Helmet), emptyItem);
        PopulateList(AvailableArmors, db.GetEquippableBy(actorId, ItemCategory.Armor), emptyItem);
        PopulateList(AvailableRelics, db.GetEquippableBy(actorId, ItemCategory.Relic), emptyItem);

        SelectedWeapon = FindItem(AvailableWeapons, _character.WeaponId);
        SelectedShield = FindItem(AvailableShields, _character.ShieldId);
        SelectedHelmet = FindItem(AvailableHelmets, _character.HelmetId);
        SelectedArmor = FindItem(AvailableArmors, _character.ArmorId);
        SelectedRelic1 = FindItem(AvailableRelics, _character.Relic1Id);
        SelectedRelic2 = FindItem(AvailableRelics, _character.Relic2Id);
    }

    private static void PopulateList(ObservableCollection<ItemDef> list, IEnumerable<ItemDef> items, ItemDef emptyItem)
    {
        list.Clear();
        list.Add(emptyItem);
        foreach (var item in items.Where(i => i.Id != 0xFF))
            list.Add(item);
    }

    private static ItemDef? FindItem(ObservableCollection<ItemDef> list, byte id)
        => list.FirstOrDefault(i => i.Id == id) ?? list.FirstOrDefault();

    partial void OnLevelChanged(byte value) { _character.Level = value; _markDirty(); }
    partial void OnCurrentHpChanged(ushort value) { _character.CurrentHp = value; _markDirty(); }
    partial void OnMaxHpChanged(ushort value) { _character.MaxHp = value; _markDirty(); }
    partial void OnCurrentMpChanged(ushort value) { _character.CurrentMp = value; _markDirty(); }
    partial void OnMaxMpChanged(ushort value) { _character.MaxMp = value; _markDirty(); }
    partial void OnVigorChanged(byte value) { _character.Vigor = value; _markDirty(); }
    partial void OnSpeedChanged(byte value) { _character.Speed = value; _markDirty(); }
    partial void OnStaminaChanged(byte value) { _character.Stamina = value; _markDirty(); }
    partial void OnMagicPowerChanged(byte value) { _character.MagicPower = value; _markDirty(); }
    partial void OnExperienceChanged(uint value) { _character.Experience = value; _markDirty(); }

    partial void OnSelectedWeaponChanged(ItemDef? value)
    {
        if (value != null) { _character.WeaponId = value.Id; _markDirty(); }
    }
    partial void OnSelectedShieldChanged(ItemDef? value)
    {
        if (value != null) { _character.ShieldId = value.Id; _markDirty(); }
    }
    partial void OnSelectedHelmetChanged(ItemDef? value)
    {
        if (value != null) { _character.HelmetId = value.Id; _markDirty(); }
    }
    partial void OnSelectedArmorChanged(ItemDef? value)
    {
        if (value != null) { _character.ArmorId = value.Id; _markDirty(); }
    }
    partial void OnSelectedRelic1Changed(ItemDef? value)
    {
        if (value != null) { _character.Relic1Id = value.Id; _markDirty(); }
    }
    partial void OnSelectedRelic2Changed(ItemDef? value)
    {
        if (value != null) { _character.Relic2Id = value.Id; _markDirty(); }
    }

    public override string ToString() => Name;
}
