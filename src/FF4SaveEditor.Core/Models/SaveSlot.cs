namespace FF4SaveEditor.Core.Models;

/// <summary>
/// Represents a 2048-byte save slot from an FF4 SRAM file.
/// Preserves full raw data for round-trip fidelity.
/// </summary>
public class SaveSlot
{
    public const int Size = 2048;
    public const int CharacterCount = 5;
    public const int CharacterDataOffset = 0x000;
    public const int BackupCharacterOffset = 0x140;
    public const int InventoryOffset = 0x440;
    public const int InventoryCount = 48;
    public const int GilOffset = 0x6A0;
    public const int LoadFlagOffset = 0x7FB;
    public const int ChecksumOffset = 0x7FC;
    public const int ValidationOffset = 0x7FE;
    public const ushort ValidationValue = 0x1BE4;

    private readonly byte[] _raw = new byte[Size];

    public CharacterData[] Characters { get; } = new CharacterData[CharacterCount];
    public InventorySlot[] Inventory { get; } = new InventorySlot[InventoryCount];

    /// <summary>Whether this slot has valid save data.</summary>
    public bool IsValid { get; private set; }

    /// <summary>Gil (0-16,777,215).</summary>
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

    /// <summary>Read-only access to the raw slot data.</summary>
    public ReadOnlySpan<byte> RawData => _raw;

    public static SaveSlot FromBytes(ReadOnlySpan<byte> data)
    {
        var slot = new SaveSlot();
        data[..Size].CopyTo(slot._raw);

        // Validate
        ushort validation = (ushort)(slot._raw[ValidationOffset] | (slot._raw[ValidationOffset + 1] << 8));
        byte loadFlag = slot._raw[LoadFlagOffset];
        slot.IsValid = validation == ValidationValue && loadFlag == 0x01;

        // Parse characters
        for (int i = 0; i < CharacterCount; i++)
        {
            int offset = CharacterDataOffset + (i * CharacterData.Size);
            slot.Characters[i] = CharacterData.FromBytes(data.Slice(offset, CharacterData.Size));
        }

        // Parse inventory
        for (int i = 0; i < InventoryCount; i++)
        {
            int offset = InventoryOffset + (i * 2);
            slot.Inventory[i] = InventorySlot.FromBytes(slot._raw[offset], slot._raw[offset + 1]);
        }

        return slot;
    }

    /// <summary>
    /// Serializes this slot back to a 2048-byte array.
    /// Overlays known fields onto the raw buffer, recalculates checksum, and sets validation.
    /// </summary>
    public byte[] ToBytes()
    {
        var result = new byte[Size];
        Array.Copy(_raw, result, Size);

        // Write characters back into raw buffer
        for (int i = 0; i < CharacterCount; i++)
        {
            int offset = CharacterDataOffset + (i * CharacterData.Size);
            Characters[i].WriteTo(result.AsSpan(offset, CharacterData.Size));
        }

        // Write inventory back
        for (int i = 0; i < InventoryCount; i++)
        {
            int offset = InventoryOffset + (i * 2);
            result[offset] = Inventory[i].ItemId;
            result[offset + 1] = Inventory[i].Quantity;
        }

        // Write gil
        var gil = Gil;
        result[GilOffset] = (byte)(gil & 0xFF);
        result[GilOffset + 1] = (byte)((gil >> 8) & 0xFF);
        result[GilOffset + 2] = (byte)((gil >> 16) & 0xFF);

        // Set load flag and validation
        result[LoadFlagOffset] = 0x01;
        result[ValidationOffset] = (byte)(ValidationValue & 0xFF);
        result[ValidationOffset + 1] = (byte)((ValidationValue >> 8) & 0xFF);

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
