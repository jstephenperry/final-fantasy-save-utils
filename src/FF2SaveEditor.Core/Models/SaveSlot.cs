namespace FF2SaveEditor.Core.Models;

/// <summary>
/// Represents a 768-byte save slot from an FF2 SRAM file.
/// Preserves full raw data for round-trip fidelity.
/// </summary>
public class SaveSlot
{
    public const int Size = 768;
    public const int CharacterCount = 4;
    public const int CharacterBlockAOffset = 0x100;
    public const int CharacterBlockBOffset = 0x200;
    public const int InventoryOffset = 0x60;
    public const int InventoryCount = 32;
    public const int KeywordOffset = 0x80;
    public const int KeywordCount = 16;
    public const int GilOffset = 0x1C;
    public const int KeyItemsOffset = 0x1A;
    public const int ValidityOffset = 0xFE;
    public const int ChecksumOffset = 0xFF;

    private readonly byte[] _raw = new byte[Size];

    public CharacterData[] Characters { get; } = new CharacterData[CharacterCount];
    public byte[] Inventory { get; } = new byte[InventoryCount]; // Item IDs only (no qty in FF2)

    public bool IsValid { get; private set; }

    /// <summary>Gil (24-bit LE at offset $1C, max 16,777,215).</summary>
    public uint Gil
    {
        get => (uint)(_raw[GilOffset] | (_raw[GilOffset + 1] << 8) | (_raw[GilOffset + 2] << 16));
        set
        {
            var clamped = Math.Min(value, 16_777_215u);
            _raw[GilOffset] = (byte)(clamped & 0xFF);
            _raw[GilOffset + 1] = (byte)((clamped >> 8) & 0xFF);
            _raw[GilOffset + 2] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    /// <summary>Key items bitmask (16-bit at $1A-$1B).</summary>
    public ushort KeyItems
    {
        get => (ushort)(_raw[KeyItemsOffset] | (_raw[KeyItemsOffset + 1] << 8));
        set
        {
            _raw[KeyItemsOffset] = (byte)(value & 0xFF);
            _raw[KeyItemsOffset + 1] = (byte)((value >> 8) & 0xFF);
        }
    }

    public byte MessageSpeed { get => _raw[0x1F]; set => _raw[0x1F] = value; }

    public ReadOnlySpan<byte> RawData => _raw;

    public static SaveSlot FromBytes(ReadOnlySpan<byte> data)
    {
        var slot = new SaveSlot();
        data[..Size].CopyTo(slot._raw);

        slot.IsValid = IO.Checksum.Verify(data[..Size]);

        // Parse characters (split across two blocks)
        for (int i = 0; i < CharacterCount; i++)
        {
            int offsetA = CharacterBlockAOffset + (i * CharacterData.BlockSize);
            int offsetB = CharacterBlockBOffset + (i * CharacterData.BlockSize);
            slot.Characters[i] = CharacterData.FromBytes(
                data.Slice(offsetA, CharacterData.BlockSize),
                data.Slice(offsetB, CharacterData.BlockSize));
        }

        // Parse inventory (item IDs only, no quantities in FF2)
        for (int i = 0; i < InventoryCount; i++)
            slot.Inventory[i] = slot._raw[InventoryOffset + i];

        return slot;
    }

    public byte[] ToBytes()
    {
        var result = new byte[Size];
        Array.Copy(_raw, result, Size);

        // Write characters back (both blocks)
        for (int i = 0; i < CharacterCount; i++)
        {
            int offsetA = CharacterBlockAOffset + (i * CharacterData.BlockSize);
            int offsetB = CharacterBlockBOffset + (i * CharacterData.BlockSize);
            Characters[i].WriteATo(result.AsSpan(offsetA, CharacterData.BlockSize));
            Characters[i].WriteBTo(result.AsSpan(offsetB, CharacterData.BlockSize));
        }

        // Write inventory back
        for (int i = 0; i < InventoryCount; i++)
            result[InventoryOffset + i] = Inventory[i];

        // Write gil
        var gil = Gil;
        result[GilOffset] = (byte)(gil & 0xFF);
        result[GilOffset + 1] = (byte)((gil >> 8) & 0xFF);
        result[GilOffset + 2] = (byte)((gil >> 16) & 0xFF);

        // Set validity and compute checksum
        result[ValidityOffset] = IO.Checksum.ValidityMarker;
        var span = result.AsSpan();
        result[ChecksumOffset] = IO.Checksum.ComputeChecksumByte(span);

        return result;
    }

    public static SaveSlot CreateEmpty()
    {
        var slot = new SaveSlot();
        slot.IsValid = false;
        for (int i = 0; i < CharacterCount; i++)
            slot.Characters[i] = CharacterData.FromBytes(new byte[CharacterData.BlockSize], new byte[CharacterData.BlockSize]);
        return slot;
    }
}
