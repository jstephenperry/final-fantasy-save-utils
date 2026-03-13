namespace FF4SaveEditor.Core.Models;

/// <summary>
/// Represents a 64-byte character record from an FF4 save slot.
/// Preserves raw bytes for round-trip fidelity - only known fields are exposed as properties.
/// </summary>
public class CharacterData
{
    public const int Size = 64;

    private readonly byte[] _raw = new byte[Size];

    /// <summary>Raw 64-byte backing store for round-trip fidelity.</summary>
    public ReadOnlySpan<byte> RawData => _raw;

    // Offset 0x00: rlcccccc (r=right-handed, l=left-handed, c=character ID)
    // Handedness affects bow damage: 20% penalty when bow is in the off-hand.
    public CharacterId CharacterId
    {
        get => (CharacterId)(_raw[0x00] & 0x3F);
        set => _raw[0x00] = (byte)((_raw[0x00] & 0xC0) | ((byte)value & 0x3F));
    }

    public bool RightHanded
    {
        get => (_raw[0x00] & 0x80) != 0;
        set => _raw[0x00] = (byte)(value ? _raw[0x00] | 0x80 : _raw[0x00] & ~0x80);
    }

    public bool LeftHanded
    {
        get => (_raw[0x00] & 0x40) != 0;
        set => _raw[0x00] = (byte)(value ? _raw[0x00] | 0x40 : _raw[0x00] & ~0x40);
    }

    // Offset 0x01: r?spgggg (r=back row, s=?, p=?, g=job graphics)
    public bool BackRow
    {
        get => (_raw[0x01] & 0x80) != 0;
        set => _raw[0x01] = (byte)(value ? _raw[0x01] | 0x80 : _raw[0x01] & ~0x80);
    }

    // Offset 0x02: Level
    public byte Level
    {
        get => _raw[0x02];
        set => _raw[0x02] = Math.Clamp(value, (byte)1, (byte)99);
    }

    // Offsets 0x03-0x06: Status bytes (preserved but not individually exposed)
    public byte Status1 { get => _raw[0x03]; set => _raw[0x03] = value; }
    public byte Status2 { get => _raw[0x04]; set => _raw[0x04] = value; }
    public byte Status3 { get => _raw[0x05]; set => _raw[0x05] = value; }
    public byte Status4 { get => _raw[0x06]; set => _raw[0x06] = value; }

    // Offsets 0x07-0x08: Current HP (16-bit LE)
    public ushort CurrentHp
    {
        get => ReadUInt16LE(0x07);
        set => WriteUInt16LE(0x07, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x09-0x0A: Max HP
    public ushort MaxHp
    {
        get => ReadUInt16LE(0x09);
        set => WriteUInt16LE(0x09, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x0B-0x0C: Current MP
    public ushort CurrentMp
    {
        get => ReadUInt16LE(0x0B);
        set => WriteUInt16LE(0x0B, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x0D-0x0E: Max MP
    public ushort MaxMp
    {
        get => ReadUInt16LE(0x0D);
        set => WriteUInt16LE(0x0D, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x0F-0x13: Base stats
    public byte Strength { get => _raw[0x0F]; set => _raw[0x0F] = Math.Min(value, (byte)99); }
    public byte Agility { get => _raw[0x10]; set => _raw[0x10] = Math.Min(value, (byte)99); }
    public byte Stamina { get => _raw[0x11]; set => _raw[0x11] = Math.Min(value, (byte)99); }
    public byte Intellect { get => _raw[0x12]; set => _raw[0x12] = Math.Min(value, (byte)99); }
    public byte Spirit { get => _raw[0x13]; set => _raw[0x13] = Math.Min(value, (byte)99); }

    // Offsets 0x14-0x18: Modified stats (computed from base + equipment)
    public byte ModifiedStrength { get => _raw[0x14]; set => _raw[0x14] = value; }
    public byte ModifiedAgility { get => _raw[0x15]; set => _raw[0x15] = value; }
    public byte ModifiedStamina { get => _raw[0x16]; set => _raw[0x16] = value; }
    public byte ModifiedIntellect { get => _raw[0x17]; set => _raw[0x17] = value; }
    public byte ModifiedSpirit { get => _raw[0x18]; set => _raw[0x18] = value; }

    // Offset 0x30: Helmet item ID
    public byte HelmetId { get => _raw[0x30]; set => _raw[0x30] = value; }

    // Offset 0x31: Armor item ID
    public byte ArmorId { get => _raw[0x31]; set => _raw[0x31] = value; }

    // Offset 0x32: Accessory item ID
    public byte AccessoryId { get => _raw[0x32]; set => _raw[0x32] = value; }

    // Offsets 0x33-0x34: Right hand (item ID + quantity/properties)
    public byte RightHandItemId { get => _raw[0x33]; set => _raw[0x33] = value; }
    public byte RightHandProperties { get => _raw[0x34]; set => _raw[0x34] = value; }

    // Offsets 0x35-0x36: Left hand (item ID + quantity/properties)
    public byte LeftHandItemId { get => _raw[0x35]; set => _raw[0x35] = value; }
    public byte LeftHandProperties { get => _raw[0x36]; set => _raw[0x36] = value; }

    // Offsets 0x37-0x39: Experience (24-bit LE)
    public uint Experience
    {
        get => (uint)(_raw[0x37] | (_raw[0x38] << 8) | (_raw[0x39] << 16));
        set
        {
            var clamped = Math.Min(value, 16_777_215u);
            _raw[0x37] = (byte)(clamped & 0xFF);
            _raw[0x38] = (byte)((clamped >> 8) & 0xFF);
            _raw[0x39] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    public bool IsEmpty => _raw[0x00] == 0 && _raw[0x02] == 0;

    public string DisplayName => CharacterId.GetDisplayName();

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
