using FF1SaveEditor.Core.GameData;

namespace FF1SaveEditor.Core.Models;

/// <summary>
/// Represents a 64-byte character record from an FF1 save slot.
/// Preserves raw bytes for round-trip fidelity.
/// </summary>
public class CharacterData
{
    public const int Size = 64;

    private readonly byte[] _raw = new byte[Size];

    public ReadOnlySpan<byte> RawData => _raw;

    // Offset 0x00: Class ID
    public byte ClassId
    {
        get => _raw[0x00];
        set => _raw[0x00] = value;
    }

    // Offset 0x01: Condition/status
    public byte Status
    {
        get => _raw[0x01];
        set => _raw[0x01] = value;
    }

    // Offset 0x02-0x05: Name (4 bytes, FF1 text encoding)
    public string Name
    {
        get => TextEncoding.Decode(_raw.AsSpan(0x02, 4));
        set
        {
            var encoded = TextEncoding.Encode(value, 4);
            Array.Copy(encoded, 0, _raw, 0x02, 4);
        }
    }

    // Offset 0x07-0x09: Experience (24-bit LE)
    public uint Experience
    {
        get => (uint)(_raw[0x07] | (_raw[0x08] << 8) | (_raw[0x09] << 16));
        set
        {
            var clamped = Math.Min(value, 16_777_215u);
            _raw[0x07] = (byte)(clamped & 0xFF);
            _raw[0x08] = (byte)((clamped >> 8) & 0xFF);
            _raw[0x09] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    // Offset 0x0A-0x0B: Current HP (16-bit LE)
    public ushort CurrentHp
    {
        get => ReadUInt16LE(0x0A);
        set => WriteUInt16LE(0x0A, Math.Min(value, (ushort)999));
    }

    // Offset 0x0C-0x0D: Max HP (16-bit LE)
    public ushort MaxHp
    {
        get => ReadUInt16LE(0x0C);
        set => WriteUInt16LE(0x0C, Math.Min(value, (ushort)999));
    }

    // Offset 0x10: Strength
    public byte Strength { get => _raw[0x10]; set => _raw[0x10] = Math.Min(value, (byte)99); }
    // Offset 0x11: Agility
    public byte Agility { get => _raw[0x11]; set => _raw[0x11] = Math.Min(value, (byte)99); }
    // Offset 0x12: Intelligence
    public byte Intelligence { get => _raw[0x12]; set => _raw[0x12] = Math.Min(value, (byte)99); }
    // Offset 0x13: Vitality
    public byte Vitality { get => _raw[0x13]; set => _raw[0x13] = Math.Min(value, (byte)99); }
    // Offset 0x14: Luck
    public byte Luck { get => _raw[0x14]; set => _raw[0x14] = Math.Min(value, (byte)99); }

    // Offsets 0x18-0x1B: Weapons (4 slots, high bit = equipped flag)
    public byte Weapon1 { get => _raw[0x18]; set => _raw[0x18] = value; }
    public byte Weapon2 { get => _raw[0x19]; set => _raw[0x19] = value; }
    public byte Weapon3 { get => _raw[0x1A]; set => _raw[0x1A] = value; }
    public byte Weapon4 { get => _raw[0x1B]; set => _raw[0x1B] = value; }

    // Offsets 0x1C-0x1F: Armor (4 slots, high bit = equipped flag)
    public byte Armor1 { get => _raw[0x1C]; set => _raw[0x1C] = value; }
    public byte Armor2 { get => _raw[0x1D]; set => _raw[0x1D] = value; }
    public byte Armor3 { get => _raw[0x1E]; set => _raw[0x1E] = value; }
    public byte Armor4 { get => _raw[0x1F]; set => _raw[0x1F] = value; }

    // Offset 0x20: Damage
    public byte Damage { get => _raw[0x20]; set => _raw[0x20] = value; }
    // Offset 0x21: Hit%
    public byte HitPercent { get => _raw[0x21]; set => _raw[0x21] = value; }
    // Offset 0x22: Absorb
    public byte Absorb { get => _raw[0x22]; set => _raw[0x22] = value; }
    // Offset 0x23: Evade%
    public byte EvadePercent { get => _raw[0x23]; set => _raw[0x23] = value; }

    // Offset 0x26: Level (stored as level-1 in the ROM, but ffse shows it directly)
    public byte Level
    {
        get => _raw[0x26];
        set => _raw[0x26] = Math.Clamp(value, (byte)1, (byte)50);
    }

    public bool IsEmpty => _raw[0x00] == 0 && _raw[0x26] == 0;

    public string DisplayName => ClassId < ClassDb.ClassNames.Length
        ? ClassDb.ClassNames[ClassId]
        : $"Unknown ({ClassId})";

    public static CharacterData FromBytes(ReadOnlySpan<byte> data)
    {
        var character = new CharacterData();
        data[..Size].CopyTo(character._raw);
        return character;
    }

    public void WriteTo(Span<byte> destination)
    {
        _raw.CopyTo(destination);
    }

    private ushort ReadUInt16LE(int offset)
        => (ushort)(_raw[offset] | (_raw[offset + 1] << 8));

    private void WriteUInt16LE(int offset, ushort value)
    {
        _raw[offset] = (byte)(value & 0xFF);
        _raw[offset + 1] = (byte)((value >> 8) & 0xFF);
    }
}
