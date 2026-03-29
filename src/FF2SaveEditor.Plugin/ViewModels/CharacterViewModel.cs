using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FF2SaveEditor.Core.Models;

namespace FF2SaveEditor.Plugin.ViewModels;

public partial class CharacterViewModel : ObservableObject
{
    private readonly CharacterData _character;
    private readonly Action _markDirty;

    public string Name => _character.DisplayName;
    public bool IsEmpty => _character.IsEmpty;

    [ObservableProperty] private ushort _currentHp;
    [ObservableProperty] private ushort _maxHp;
    [ObservableProperty] private ushort _currentMp;
    [ObservableProperty] private ushort _maxMp;
    [ObservableProperty] private byte _strength;
    [ObservableProperty] private byte _agility;
    [ObservableProperty] private byte _stamina;
    [ObservableProperty] private byte _intelligence;
    [ObservableProperty] private byte _spirit;
    [ObservableProperty] private byte _magicPower;

    public CharacterViewModel(CharacterData character, Action markDirty)
    {
        _character = character;
        _markDirty = markDirty;

        _currentHp = character.CurrentHp;
        _maxHp = character.MaxHp;
        _currentMp = character.CurrentMp;
        _maxMp = character.MaxMp;
        _strength = character.Strength;
        _agility = character.Agility;
        _stamina = character.Stamina;
        _intelligence = character.Intelligence;
        _spirit = character.Spirit;
        _magicPower = character.MagicPower;
    }

    partial void OnCurrentHpChanged(ushort value) { _character.CurrentHp = value; _markDirty(); }
    partial void OnMaxHpChanged(ushort value) { _character.MaxHp = value; _markDirty(); }
    partial void OnCurrentMpChanged(ushort value) { _character.CurrentMp = value; _markDirty(); }
    partial void OnMaxMpChanged(ushort value) { _character.MaxMp = value; _markDirty(); }
    partial void OnStrengthChanged(byte value) { _character.Strength = value; _markDirty(); }
    partial void OnAgilityChanged(byte value) { _character.Agility = value; _markDirty(); }
    partial void OnStaminaChanged(byte value) { _character.Stamina = value; _markDirty(); }
    partial void OnIntelligenceChanged(byte value) { _character.Intelligence = value; _markDirty(); }
    partial void OnSpiritChanged(byte value) { _character.Spirit = value; _markDirty(); }
    partial void OnMagicPowerChanged(byte value) { _character.MagicPower = value; _markDirty(); }

    [RelayCommand]
    public void MaxStats()
    {
        if (IsEmpty) return;

        MaxHp = 9999;
        CurrentHp = 9999;
        MaxMp = 9999;
        CurrentMp = 9999;
        Strength = 99;
        Agility = 99;
        Stamina = 99;
        Intelligence = 99;
        Spirit = 99;
        MagicPower = 99;
    }

    public override string ToString() => Name;
}
