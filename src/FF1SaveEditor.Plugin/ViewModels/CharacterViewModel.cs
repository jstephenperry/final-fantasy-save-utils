using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FF1SaveEditor.Core.GameData;
using FF1SaveEditor.Core.Models;

namespace FF1SaveEditor.Plugin.ViewModels;

public partial class CharacterViewModel : ObservableObject
{
    private readonly CharacterData _character;
    private readonly Action _markDirty;

    public string Name => _character.IsEmpty ? "(Empty)" : _character.Name;
    public string ClassName => _character.DisplayName;
    public bool IsEmpty => _character.IsEmpty;

    [ObservableProperty] private byte _level;
    [ObservableProperty] private ushort _currentHp;
    [ObservableProperty] private ushort _maxHp;
    [ObservableProperty] private byte _strength;
    [ObservableProperty] private byte _agility;
    [ObservableProperty] private byte _intelligence;
    [ObservableProperty] private byte _vitality;
    [ObservableProperty] private byte _luck;
    [ObservableProperty] private uint _experience;
    [ObservableProperty] private byte _classId;

    public CharacterViewModel(CharacterData character, Action markDirty)
    {
        _character = character;
        _markDirty = markDirty;

        _level = character.Level;
        _currentHp = character.CurrentHp;
        _maxHp = character.MaxHp;
        _strength = character.Strength;
        _agility = character.Agility;
        _intelligence = character.Intelligence;
        _vitality = character.Vitality;
        _luck = character.Luck;
        _experience = character.Experience;
        _classId = character.ClassId;
    }

    partial void OnLevelChanged(byte value) { _character.Level = value; _markDirty(); }
    partial void OnCurrentHpChanged(ushort value) { _character.CurrentHp = value; _markDirty(); }
    partial void OnMaxHpChanged(ushort value) { _character.MaxHp = value; _markDirty(); }
    partial void OnStrengthChanged(byte value) { _character.Strength = value; _markDirty(); }
    partial void OnAgilityChanged(byte value) { _character.Agility = value; _markDirty(); }
    partial void OnIntelligenceChanged(byte value) { _character.Intelligence = value; _markDirty(); }
    partial void OnVitalityChanged(byte value) { _character.Vitality = value; _markDirty(); }
    partial void OnLuckChanged(byte value) { _character.Luck = value; _markDirty(); }
    partial void OnExperienceChanged(uint value) { _character.Experience = value; _markDirty(); }
    partial void OnClassIdChanged(byte value) { _character.ClassId = value; _markDirty(); OnPropertyChanged(nameof(ClassName)); }

    [RelayCommand]
    public void MaxStats()
    {
        if (IsEmpty) return;

        Level = 50;
        MaxHp = 999;
        CurrentHp = 999;
        Strength = 99;
        Agility = 99;
        Intelligence = 99;
        Vitality = 99;
        Luck = 99;
        Experience = 16_777_215;
    }

    public override string ToString() => Name;
}
