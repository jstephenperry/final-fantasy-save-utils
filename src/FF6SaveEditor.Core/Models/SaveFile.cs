namespace FF6SaveEditor.Core.Models;

/// <summary>
/// Represents a complete 8192-byte FF6 SRAM file containing 3 save slots.
/// SRAM validity is indicated by 4x 0xE41B at file offsets $1FF8-$1FFE.
/// </summary>
public class SaveFile
{
    public const int FileSize = 8192;
    public const int SlotCount = 3;
    public const ushort SramValidityValue = 0xE41B;
    public const int SramValidityOffset = 0x1FF8;

    public SaveSlot[] Slots { get; } = new SaveSlot[SlotCount];

    /// <summary>Slot offsets: $0000, $0A00, $1400.</summary>
    private static int GetSlotOffset(int index) => index * 0x0A00;

    public static SaveFile FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != FileSize)
            throw new ArgumentException($"SRM file must be exactly {FileSize} bytes, got {data.Length}.");

        var file = new SaveFile();
        for (int i = 0; i < SlotCount; i++)
        {
            int offset = GetSlotOffset(i);
            file.Slots[i] = SaveSlot.FromBytes(data.Slice(offset, SaveSlot.Size));
        }
        return file;
    }

    public byte[] ToBytes()
    {
        var result = new byte[FileSize];
        for (int i = 0; i < SlotCount; i++)
        {
            var slotBytes = Slots[i].ToBytes();
            Array.Copy(slotBytes, 0, result, GetSlotOffset(i), SaveSlot.Size);
        }

        // Write SRAM validity signature (4x 0xE41B at $1FF8-$1FFE)
        for (int i = 0; i < 4; i++)
        {
            int offset = SramValidityOffset + (i * 2);
            result[offset] = (byte)(SramValidityValue & 0xFF);
            result[offset + 1] = (byte)((SramValidityValue >> 8) & 0xFF);
        }

        return result;
    }

    public static SaveFile CreateEmpty()
    {
        var file = new SaveFile();
        for (int i = 0; i < SlotCount; i++)
            file.Slots[i] = SaveSlot.CreateEmpty();
        return file;
    }
}
