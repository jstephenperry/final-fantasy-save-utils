namespace FF3SaveEditor.Core.Models;

/// <summary>
/// Represents a 1024-byte save slot from an FF3 SRAM file.
/// Preserves full raw data for round-trip fidelity.
/// </summary>
public class SaveSlot
{
    public const int Size = 1024;
    public const int CharacterCount = 4;
    public const int CharacterBlockAOffset = 0x100;
    public const int CharacterBlockBOffset = 0x200;
    public const int InventoryIdOffset = 0xC0;
    public const int InventoryQtyOffset = 0xE0;
    public const int InventoryCount = 32;
    public const int GilOffset = 0x1C;
    public const int CapacityPointsOffset = 0x1B;
    public const int CrystalLevelOffset = 0x21;
    public const int SaveCountOffset = 0x14;
    public const int WorldIdOffset = 0x08;
    public const int ValidityOffset = 0x19;
    public const int ChecksumOffset = 0x1A;

    private readonly byte[] _raw = new byte[Size];

    public CharacterData[] Characters { get; } = new CharacterData[CharacterCount];
    public InventorySlot[] Inventory { get; } = new InventorySlot[InventoryCount];

    public bool IsValid { get; private set; }

    /// <summary>Gil (24-bit LE at offset $1C, max 9,999,999).</summary>
    public uint Gil
    {
        get => (uint)(_raw[GilOffset] | (_raw[GilOffset + 1] << 8) | (_raw[GilOffset + 2] << 16));
        set
        {
            var clamped = Math.Min(value, 9_999_999u);
            _raw[GilOffset] = (byte)(clamped & 0xFF);
            _raw[GilOffset + 1] = (byte)((clamped >> 8) & 0xFF);
            _raw[GilOffset + 2] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    public byte CapacityPoints { get => _raw[CapacityPointsOffset]; set => _raw[CapacityPointsOffset] = value; }
    public byte CrystalLevel { get => _raw[CrystalLevelOffset]; set => _raw[CrystalLevelOffset] = value; }
    public byte SaveCount { get => _raw[SaveCountOffset]; set => _raw[SaveCountOffset] = value; }
    public byte WorldId { get => _raw[WorldIdOffset]; set => _raw[WorldIdOffset] = value; }

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

        // Parse inventory (separate ID and quantity arrays)
        for (int i = 0; i < InventoryCount; i++)
        {
            slot.Inventory[i] = InventorySlot.FromBytes(
                slot._raw[InventoryIdOffset + i],
                slot._raw[InventoryQtyOffset + i]);
        }

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
        {
            result[InventoryIdOffset + i] = Inventory[i].ItemId;
            result[InventoryQtyOffset + i] = Inventory[i].Quantity;
        }

        // Write gil
        var gil = Gil;
        result[GilOffset] = (byte)(gil & 0xFF);
        result[GilOffset + 1] = (byte)((gil >> 8) & 0xFF);
        result[GilOffset + 2] = (byte)((gil >> 16) & 0xFF);

        // Set validity marker and compute checksum
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
        for (int i = 0; i < InventoryCount; i++)
            slot.Inventory[i] = new InventorySlot();
        return slot;
    }
}
