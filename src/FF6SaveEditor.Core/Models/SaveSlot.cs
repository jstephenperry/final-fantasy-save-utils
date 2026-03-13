namespace FF6SaveEditor.Core.Models;

/// <summary>
/// Represents a 2560-byte save slot from an FF6 SRAM file.
/// Preserves full raw data for round-trip fidelity.
/// </summary>
public class SaveSlot
{
    public const int Size = 2560;
    public const int CharacterCount = 16;
    public const int CharacterDataOffset = 0x000;
    public const int GilOffset = 0x0260;
    public const int GameTimeOffset = 0x0263;
    public const int StepsOffset = 0x0266;
    public const int InventoryIdOffset = 0x0269;
    public const int InventoryQtyOffset = 0x0369;
    public const int InventoryCount = 256;
    public const int EsperOffset = 0x0469;
    public const int ChecksumOffset = 0x09FE;

    private readonly byte[] _raw = new byte[Size];

    public CharacterData[] Characters { get; } = new CharacterData[CharacterCount];
    public InventorySlot[] Inventory { get; } = new InventorySlot[InventoryCount];

    /// <summary>Whether this slot has valid save data (checksum matches).</summary>
    public bool IsValid { get; private set; }

    /// <summary>Gil (0-9,999,999), 24-bit LE.</summary>
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

    /// <summary>Game time hours.</summary>
    public byte Hours
    {
        get => _raw[GameTimeOffset];
        set => _raw[GameTimeOffset] = value;
    }

    /// <summary>Game time minutes.</summary>
    public byte Minutes
    {
        get => _raw[GameTimeOffset + 1];
        set => _raw[GameTimeOffset + 1] = Math.Min(value, (byte)59);
    }

    /// <summary>Game time seconds.</summary>
    public byte Seconds
    {
        get => _raw[GameTimeOffset + 2];
        set => _raw[GameTimeOffset + 2] = Math.Min(value, (byte)59);
    }

    /// <summary>Steps (24-bit LE).</summary>
    public uint Steps
    {
        get => (uint)(_raw[StepsOffset] | (_raw[StepsOffset + 1] << 8) | (_raw[StepsOffset + 2] << 16));
        set
        {
            var clamped = Math.Min(value, 16_777_215u);
            _raw[StepsOffset] = (byte)(clamped & 0xFF);
            _raw[StepsOffset + 1] = (byte)((clamped >> 8) & 0xFF);
            _raw[StepsOffset + 2] = (byte)((clamped >> 16) & 0xFF);
        }
    }

    /// <summary>Read-only access to the raw slot data.</summary>
    public ReadOnlySpan<byte> RawData => _raw;

    public static SaveSlot FromBytes(ReadOnlySpan<byte> data)
    {
        var slot = new SaveSlot();
        data[..Size].CopyTo(slot._raw);

        // Validate via checksum
        ushort stored = (ushort)(slot._raw[ChecksumOffset] | (slot._raw[ChecksumOffset + 1] << 8));
        ushort calculated = IO.Checksum.Calculate(slot._raw);
        slot.IsValid = stored == calculated && stored != 0;

        // Parse 16 characters (37 bytes each)
        for (int i = 0; i < CharacterCount; i++)
        {
            int offset = CharacterDataOffset + (i * CharacterData.Size);
            slot.Characters[i] = CharacterData.FromBytes(data.Slice(offset, CharacterData.Size));
        }

        // Parse inventory (separate ID and quantity arrays)
        for (int i = 0; i < InventoryCount; i++)
        {
            byte itemId = slot._raw[InventoryIdOffset + i];
            byte qty = slot._raw[InventoryQtyOffset + i];
            slot.Inventory[i] = InventorySlot.FromBytes(itemId, qty);
        }

        return slot;
    }

    /// <summary>
    /// Serializes this slot back to a 2560-byte array.
    /// Overlays known fields onto the raw buffer and recalculates checksum.
    /// </summary>
    public byte[] ToBytes()
    {
        var result = new byte[Size];
        Array.Copy(_raw, result, Size);

        // Write characters back
        for (int i = 0; i < CharacterCount; i++)
        {
            int offset = CharacterDataOffset + (i * CharacterData.Size);
            Characters[i].WriteTo(result.AsSpan(offset, CharacterData.Size));
        }

        // Write inventory back (separate arrays)
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

        // Calculate and write checksum
        ushort checksum = IO.Checksum.Calculate(result);
        result[ChecksumOffset] = (byte)(checksum & 0xFF);
        result[ChecksumOffset + 1] = (byte)((checksum >> 8) & 0xFF);

        return result;
    }

    /// <summary>Creates an empty/invalid slot.</summary>
    public static SaveSlot CreateEmpty()
    {
        var slot = new SaveSlot();
        slot.IsValid = false;
        for (int i = 0; i < CharacterCount; i++)
            slot.Characters[i] = CharacterData.FromBytes(new byte[CharacterData.Size]);
        for (int i = 0; i < InventoryCount; i++)
            slot.Inventory[i] = new InventorySlot();
        return slot;
    }
}
