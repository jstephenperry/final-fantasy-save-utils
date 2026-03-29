namespace FF1SaveEditor.Core.Models;

/// <summary>
/// Represents a 1024-byte save slot from an FF1 SRAM file.
/// The validated save region is at file offset $0400-$07FF.
/// Preserves full raw data for round-trip fidelity.
/// </summary>
public class SaveSlot
{
    public const int Size = 1024;
    public const int CharacterCount = 4;
    public const int CharacterOffset = 0x100;
    public const int GilOffset = 0x1C;
    public const int MagicAssert55Offset = 0x0FC;
    public const int MagicAssertAAOffset = 0x0FD;
    public const int ChecksumOffset = 0x0FD;
    public const byte MagicValue55 = 0x55;
    public const byte MagicValueAA = 0xAA;

    // Key item flag offsets (relative to slot start at $6400 → mapped to $20-$31)
    public const int KeyItemsOffset = 0x20;

    // Magic data
    public const int MagicDataOffset = 0x300;
    public const int MagicDataPerCharacter = 0x40;

    private readonly byte[] _raw = new byte[Size];

    public CharacterData[] Characters { get; } = new CharacterData[CharacterCount];

    public bool IsValid { get; private set; }

    /// <summary>Gil (24-bit LE at offset $1C, max 999999).</summary>
    public uint Gil
    {
        get => (uint)(_raw[GilOffset] | (_raw[GilOffset + 1] << 8) | (_raw[GilOffset + 2] << 16));
        set
        {
            var clamped = Math.Min(value, 999_999u);
            _raw[GilOffset] = (byte)(clamped & 0xFF);
            _raw[GilOffset + 1] = (byte)((clamped >> 8) & 0xFF);
            _raw[GilOffset + 2] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    // Key items (individual byte flags)
    public byte Lute { get => _raw[0x20]; set => _raw[0x20] = value; }
    public byte Crown { get => _raw[0x21]; set => _raw[0x21] = value; }
    public byte Crystal { get => _raw[0x22]; set => _raw[0x22] = value; }
    public byte Herb { get => _raw[0x23]; set => _raw[0x23] = value; }
    public byte MysticKey { get => _raw[0x24]; set => _raw[0x24] = value; }
    public byte Tnt { get => _raw[0x25]; set => _raw[0x25] = value; }
    public byte Adamant { get => _raw[0x26]; set => _raw[0x26] = value; }
    public byte Slab { get => _raw[0x27]; set => _raw[0x27] = value; }
    public byte Ruby { get => _raw[0x28]; set => _raw[0x28] = value; }
    public byte Rod { get => _raw[0x29]; set => _raw[0x29] = value; }
    public byte Floater { get => _raw[0x2A]; set => _raw[0x2A] = value; }
    public byte Chime { get => _raw[0x2B]; set => _raw[0x2B] = value; }
    public byte Tail { get => _raw[0x2C]; set => _raw[0x2C] = value; }
    public byte Cube { get => _raw[0x2D]; set => _raw[0x2D] = value; }
    public byte Bottle { get => _raw[0x2E]; set => _raw[0x2E] = value; }
    public byte Oxyale { get => _raw[0x2F]; set => _raw[0x2F] = value; }

    // Orbs
    public byte FireOrb { get => _raw[0x30]; set => _raw[0x30] = value; }
    public byte WaterOrb { get => _raw[0x31]; set => _raw[0x31] = value; }

    // World state
    public byte ShipVisible { get => _raw[0x00]; set => _raw[0x00] = value; }
    public byte AirshipVisible { get => _raw[0x04]; set => _raw[0x04] = value; }
    public byte HasCanoe { get => _raw[0x12]; set => _raw[0x12] = value; }

    public ReadOnlySpan<byte> RawData => _raw;

    public static SaveSlot FromBytes(ReadOnlySpan<byte> data)
    {
        var slot = new SaveSlot();
        data[..Size].CopyTo(slot._raw);

        // Validate: check magic bytes at $FC and $FD within the working region
        // Note: In the actual SRAM, $6400+$FC = assert_55, $6400+$FD = checksum byte
        // The ffse source uses $FC for $55 and $FE for $AA validation
        // For simplicity, we check the overall checksum validity
        slot.IsValid = IO.Checksum.Verify(data[..Size]);

        // Parse characters (4 x 64 bytes at offset $100)
        for (int i = 0; i < CharacterCount; i++)
        {
            int offset = CharacterOffset + (i * CharacterData.Size);
            slot.Characters[i] = CharacterData.FromBytes(data.Slice(offset, CharacterData.Size));
        }

        return slot;
    }

    public byte[] ToBytes()
    {
        var result = new byte[Size];
        Array.Copy(_raw, result, Size);

        // Write characters back
        for (int i = 0; i < CharacterCount; i++)
        {
            int offset = CharacterOffset + (i * CharacterData.Size);
            Characters[i].WriteTo(result.AsSpan(offset, CharacterData.Size));
        }

        // Write gil
        var gil = Gil;
        result[GilOffset] = (byte)(gil & 0xFF);
        result[GilOffset + 1] = (byte)((gil >> 8) & 0xFF);
        result[GilOffset + 2] = (byte)((gil >> 16) & 0xFF);

        // Compute and write checksum
        var span = result.AsSpan();
        result[IO.Checksum.ChecksumOffset] = IO.Checksum.ComputeChecksumByte(span);

        return result;
    }

    public static SaveSlot CreateEmpty()
    {
        var slot = new SaveSlot();
        slot.IsValid = false;
        for (int i = 0; i < CharacterCount; i++)
            slot.Characters[i] = CharacterData.FromBytes(new byte[CharacterData.Size]);
        return slot;
    }
}
