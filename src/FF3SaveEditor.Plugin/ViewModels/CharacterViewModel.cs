using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FF3SaveEditor.Core.GameData;
using FF3SaveEditor.Core.Models;

namespace FF3SaveEditor.Plugin.ViewModels;

public partial class CharacterViewModel : ObservableObject
{
    private readonly CharacterData _character;
    private readonly Action _markDirty;

    public string Name => _character.DisplayName;
    public string JobName => _character.JobName;
    public bool IsEmpty => _character.IsEmpty;

    [ObservableProperty] private byte _level;
    [ObservableProperty] private ushort _currentHp;
    [ObservableProperty] private ushort _maxHp;
    [ObservableProperty] private byte _strength;
    [ObservableProperty] private byte _agility;
    [ObservableProperty] private byte _vitality;
    [ObservableProperty] private byte _intelligence;
    [ObservableProperty] private byte _spirit;
    [ObservableProperty] private uint _experience;
    [ObservableProperty] private byte _jobId;

    public CharacterViewModel(CharacterData character, Action markDirty)
    {
        _character = character;
        _markDirty = markDirty;

        _level = character.Level;
        _currentHp = character.CurrentHp;
        _maxHp = character.MaxHp;
        _strength = character.Strength;
        _agility = character.Agility;
        _vitality = character.Vitality;
        _intelligence = character.Intelligence;
        _spirit = character.Spirit;
        _experience = character.Experience;
        _jobId = character.JobId;
    }

    partial void OnLevelChanged(byte value) { _character.Level = value; _markDirty(); }
    partial void OnCurrentHpChanged(ushort value) { _character.CurrentHp = value; _markDirty(); }
    partial void OnMaxHpChanged(ushort value) { _character.MaxHp = value; _markDirty(); }
    partial void OnStrengthChanged(byte value) { _character.Strength = value; _markDirty(); }
    partial void OnAgilityChanged(byte value) { _character.Agility = value; _markDirty(); }
    partial void OnVitalityChanged(byte value) { _character.Vitality = value; _markDirty(); }
    partial void OnIntelligenceChanged(byte value) { _character.Intelligence = value; _markDirty(); }
    partial void OnSpiritChanged(byte value) { _character.Spirit = value; _markDirty(); }
    partial void OnExperienceChanged(uint value) { _character.Experience = value; _markDirty(); }
    partial void OnJobIdChanged(byte value) { _character.JobId = value; _markDirty(); OnPropertyChanged(nameof(JobName)); }

    [RelayCommand]
    public void MaxStats()
    {
        if (IsEmpty) return;

        Level = 99;
        MaxHp = 9999;
        CurrentHp = 9999;
        Strength = 99;
        Agility = 99;
        Vitality = 99;
        Intelligence = 99;
        Spirit = 99;
        Experience = 16_777_215;
    }

    public override string ToString() => Name;
}
