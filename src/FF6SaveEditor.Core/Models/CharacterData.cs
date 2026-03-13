namespace FF6SaveEditor.Core.Models;

/// <summary>
/// Represents a 37-byte character record from an FF6 save slot.
/// Preserves raw bytes for round-trip fidelity.
///
/// Layout (verified against real save data):
///   0x00:       Actor ID
///   0x01:       Graphic index
///   0x02-0x07:  Name (6 bytes, FF6 text encoding)
///   0x08:       Level
///   0x09-0x0A:  Current HP (16-bit LE)
///   0x0B-0x0C:  Max HP (16-bit LE, top 2 bits = flags)
///   0x0D-0x0E:  Current MP (16-bit LE)
///   0x0F-0x10:  Max MP (16-bit LE)
///   0x11-0x13:  Experience (24-bit LE)
///   0x14-0x15:  Status bytes (preserved)
///   0x16-0x19:  Battle commands (4 bytes)
///   0x1A:       Vigor
///   0x1B:       Speed
///   0x1C:       Stamina
///   0x1D:       Magic Power
///   0x1E:       Equipped Esper (0xFF = none)
///   0x1F:       Weapon
///   0x20:       Shield
///   0x21:       Helmet
///   0x22:       Armor
///   0x23:       Relic 1
///   0x24:       Relic 2
/// </summary>
public class CharacterData
{
    public const int Size = 37;

    private readonly byte[] _raw = new byte[Size];

    public ReadOnlySpan<byte> RawData => _raw;

    // Offset 0x00: Actor ID
    public ActorId ActorId
    {
        get => (ActorId)_raw[0x00];
        set => _raw[0x00] = (byte)value;
    }

    // Offset 0x01: Graphic index (preserved)
    public byte GraphicIndex
    {
        get => _raw[0x01];
        set => _raw[0x01] = value;
    }

    // Offsets 0x02-0x07: Name (6 bytes, FF6 text encoding)
    public string Name
    {
        get => DecodeFF6Text(_raw.AsSpan(0x02, 6));
        // Display-only in Phase 1
    }

    // Offset 0x08: Level
    public byte Level
    {
        get => _raw[0x08];
        set => _raw[0x08] = Math.Clamp(value, (byte)1, (byte)99);
    }

    // Offsets 0x09-0x0A: Current HP (16-bit LE)
    public ushort CurrentHp
    {
        get => ReadUInt16LE(0x09);
        set => WriteUInt16LE(0x09, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x0B-0x0C: Max HP (16-bit LE, bits 15-14 are flags, value masked with 0x3FFF)
    public ushort MaxHp
    {
        get => (ushort)(ReadUInt16LE(0x0B) & 0x3FFF);
        set
        {
            ushort flags = (ushort)(ReadUInt16LE(0x0B) & 0xC000);
            WriteUInt16LE(0x0B, (ushort)(flags | Math.Min(value, (ushort)9999)));
        }
    }

    // Offsets 0x0D-0x0E: Current MP (16-bit LE)
    public ushort CurrentMp
    {
        get => ReadUInt16LE(0x0D);
        set => WriteUInt16LE(0x0D, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x0F-0x10: Max MP (16-bit LE)
    public ushort MaxMp
    {
        get => ReadUInt16LE(0x0F);
        set => WriteUInt16LE(0x0F, Math.Min(value, (ushort)9999));
    }

    // Offsets 0x11-0x13: Experience (24-bit LE)
    public uint Experience
    {
        get => (uint)(_raw[0x11] | (_raw[0x12] << 8) | (_raw[0x13] << 16));
        set
        {
            var clamped = Math.Min(value, 16_777_215u);
            _raw[0x11] = (byte)(clamped & 0xFF);
            _raw[0x12] = (byte)((clamped >> 8) & 0xFF);
            _raw[0x13] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    // Offsets 0x14-0x15: Status bytes (preserved)
    public byte Status1 { get => _raw[0x14]; set => _raw[0x14] = value; }
    public byte Status2 { get => _raw[0x15]; set => _raw[0x15] = value; }

    // Offsets 0x16-0x19: Battle commands (4 bytes)
    public byte Command1 { get => _raw[0x16]; set => _raw[0x16] = value; }
    public byte Command2 { get => _raw[0x17]; set => _raw[0x17] = value; }
    public byte Command3 { get => _raw[0x18]; set => _raw[0x18] = value; }
    public byte Command4 { get => _raw[0x19]; set => _raw[0x19] = value; }

    // Offset 0x1A: Vigor
    public byte Vigor
    {
        get => _raw[0x1A];
        set => _raw[0x1A] = Math.Min(value, (byte)128);
    }

    // Offset 0x1B: Speed
    public byte Speed
    {
        get => _raw[0x1B];
        set => _raw[0x1B] = Math.Min(value, (byte)128);
    }

    // Offset 0x1C: Stamina
    public byte Stamina
    {
        get => _raw[0x1C];
        set => _raw[0x1C] = Math.Min(value, (byte)128);
    }

    // Offset 0x1D: Magic Power
    public byte MagicPower
    {
        get => _raw[0x1D];
        set => _raw[0x1D] = Math.Min(value, (byte)128);
    }

    // Offset 0x1E: Equipped Esper (0xFF = none)
    public byte EquippedEsper
    {
        get => _raw[0x1E];
        set => _raw[0x1E] = value;
    }

    // Offset 0x1F: Weapon item ID
    public byte WeaponId { get => _raw[0x1F]; set => _raw[0x1F] = value; }

    // Offset 0x20: Shield item ID
    public byte ShieldId { get => _raw[0x20]; set => _raw[0x20] = value; }

    // Offset 0x21: Helmet item ID
    public byte HelmetId { get => _raw[0x21]; set => _raw[0x21] = value; }

    // Offset 0x22: Armor item ID
    public byte ArmorId { get => _raw[0x22]; set => _raw[0x22] = value; }

    // Offset 0x23: Relic 1 item ID
    public byte Relic1Id { get => _raw[0x23]; set => _raw[0x23] = value; }

    // Offset 0x24: Relic 2 item ID
    public byte Relic2Id { get => _raw[0x24]; set => _raw[0x24] = value; }

    public bool IsEmpty => ActorId == ActorId.Empty;

    public string DisplayName
    {
        get
        {
            var name = Name;
            return string.IsNullOrWhiteSpace(name) ? ActorId.GetDisplayName() : name;
        }
    }

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

    /// <summary>
    /// Decodes FF6 text encoding to a .NET string.
    /// 0x80=A..0x99=Z, 0x9A=a..0xB3=z, 0xB4=0..0xBD=9, 0xFF=terminator
    /// </summary>
    private static string DecodeFF6Text(ReadOnlySpan<byte> data)
    {
        var chars = new List<char>();
        foreach (byte b in data)
        {
            if (b == 0xFF) break;
            char c = b switch
            {
                >= 0x80 and <= 0x99 => (char)('A' + (b - 0x80)),
                >= 0x9A and <= 0xB3 => (char)('a' + (b - 0x9A)),
                >= 0xB4 and <= 0xBD => (char)('0' + (b - 0xB4)),
                0xBE => '!',
                0xBF => '?',
                0xC0 => '/',
                0xC1 => ':',
                0xC2 => '"',
                0xC3 => '\'',
                0xC4 => '-',
                0xC5 => '.',
                0xC7 => ' ',
                _ => ' ',
            };
            chars.Add(c);
        }
        return new string(chars.ToArray());
    }
}
