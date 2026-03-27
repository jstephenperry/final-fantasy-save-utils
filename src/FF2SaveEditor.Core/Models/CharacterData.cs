using FF2SaveEditor.Core.GameData;

namespace FF2SaveEditor.Core.Models;

/// <summary>
/// Represents a character from an FF2 save slot.
/// FF2 splits character data into two 64-byte blocks:
/// Properties 1 at $100+n*$40 (stats, equipment, spells)
/// Properties 2 at $200+n*$40 (proficiency levels/exp)
/// </summary>
public class CharacterData
{
    public const int BlockSize = 64;

    private readonly byte[] _rawA = new byte[BlockSize]; // Props 1: stats, equip
    private readonly byte[] _rawB = new byte[BlockSize]; // Props 2: proficiency

    // --- Block A: Stats and Equipment ($100+n*$40) ---

    // +$00: Guest flag / Character ID (bit 7: guest)
    public byte CharacterId
    {
        get => (byte)(_rawA[0x00] & 0x0F);
        set => _rawA[0x00] = (byte)((_rawA[0x00] & 0xF0) | (value & 0x0F));
    }

    // +$01: Status
    public byte Status
    {
        get => _rawA[0x01];
        set => _rawA[0x01] = value;
    }

    // +$02-$07: Name (6 bytes)
    public string Name
    {
        get => TextEncoding.Decode(_rawA.AsSpan(0x02, 6));
        set
        {
            var encoded = TextEncoding.Encode(value, 6);
            Array.Copy(encoded, 0, _rawA, 0x02, 6);
        }
    }

    // +$08-$09: Current HP (16-bit LE)
    public ushort CurrentHp
    {
        get => ReadUInt16LE(_rawA, 0x08);
        set => WriteUInt16LE(_rawA, 0x08, Math.Min(value, (ushort)9999));
    }

    // +$0A-$0B: Max HP
    public ushort MaxHp
    {
        get => ReadUInt16LE(_rawA, 0x0A);
        set => WriteUInt16LE(_rawA, 0x0A, Math.Min(value, (ushort)9999));
    }

    // +$0C-$0D: Current MP
    public ushort CurrentMp
    {
        get => ReadUInt16LE(_rawA, 0x0C);
        set => WriteUInt16LE(_rawA, 0x0C, Math.Min(value, (ushort)9999));
    }

    // +$0E-$0F: Max MP
    public ushort MaxMp
    {
        get => ReadUInt16LE(_rawA, 0x0E);
        set => WriteUInt16LE(_rawA, 0x0E, Math.Min(value, (ushort)9999));
    }

    // +$10-$15: Base stats
    public byte Strength { get => _rawA[0x10]; set => _rawA[0x10] = Math.Min(value, (byte)99); }
    public byte Agility { get => _rawA[0x11]; set => _rawA[0x11] = Math.Min(value, (byte)99); }
    public byte Stamina { get => _rawA[0x12]; set => _rawA[0x12] = Math.Min(value, (byte)99); }
    public byte Intelligence { get => _rawA[0x13]; set => _rawA[0x13] = Math.Min(value, (byte)99); }
    public byte Spirit { get => _rawA[0x14]; set => _rawA[0x14] = Math.Min(value, (byte)99); }
    public byte MagicPower { get => _rawA[0x15]; set => _rawA[0x15] = Math.Min(value, (byte)99); }

    // Equipment
    public byte HelmetId { get => _rawA[0x19]; set => _rawA[0x19] = value; }
    public byte ArmorId { get => _rawA[0x1A]; set => _rawA[0x1A] = value; }
    public byte GlovesId { get => _rawA[0x1B]; set => _rawA[0x1B] = value; }
    public byte RightHandId { get => _rawA[0x1C]; set => _rawA[0x1C] = value; }
    public byte LeftHandId { get => _rawA[0x1D]; set => _rawA[0x1D] = value; }

    // --- Block B: Proficiency ($200+n*$40) ---

    // +$00-$0F: Equipment proficiency (8 types x 2 bytes: level, exp)
    public byte GetWeaponSkillLevel(int type) => _rawB[type * 2];
    public void SetWeaponSkillLevel(int type, byte value) => _rawB[type * 2] = value;
    public byte GetWeaponSkillExp(int type) => _rawB[type * 2 + 1];
    public void SetWeaponSkillExp(int type, byte value) => _rawB[type * 2 + 1] = value;

    // +$10-$2F: Spell proficiency (16 spells x 2 bytes: level, exp)
    public byte GetSpellLevel(int spell) => _rawB[0x10 + spell * 2];
    public void SetSpellLevel(int spell, byte value) => _rawB[0x10 + spell * 2] = value;
    public byte GetSpellExp(int spell) => _rawB[0x11 + spell * 2];
    public void SetSpellExp(int spell, byte value) => _rawB[0x11 + spell * 2] = value;

    // +$35: Presence & row (bit 0: front row, bit 7: guest)
    public bool FrontRow
    {
        get => (_rawB[0x35] & 0x01) != 0;
        set => _rawB[0x35] = (byte)(value ? _rawB[0x35] | 0x01 : _rawB[0x35] & ~0x01);
    }

    public bool IsEmpty => _rawA[0x02] == 0xFF && _rawA[0x03] == 0xFF;

    public string DisplayName => IsEmpty ? "(Empty)" : Name;

    public ReadOnlySpan<byte> RawDataA => _rawA;
    public ReadOnlySpan<byte> RawDataB => _rawB;

    public static CharacterData FromBytes(ReadOnlySpan<byte> blockA, ReadOnlySpan<byte> blockB)
    {
        var character = new CharacterData();
        blockA[..BlockSize].CopyTo(character._rawA);
        blockB[..BlockSize].CopyTo(character._rawB);
        return character;
    }

    public void WriteATo(Span<byte> destination) => _rawA.CopyTo(destination);
    public void WriteBTo(Span<byte> destination) => _rawB.CopyTo(destination);

    private static ushort ReadUInt16LE(byte[] data, int offset)
        => (ushort)(data[offset] | (data[offset + 1] << 8));

    private static void WriteUInt16LE(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)(value & 0xFF);
        data[offset + 1] = (byte)((value >> 8) & 0xFF);
    }
}
