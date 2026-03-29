namespace FF3SaveEditor.Core.Models;

/// <summary>
/// Represents a complete 8192-byte FF3 SRAM file containing 3 save slots + working area.
/// Working area: $0000-$03FF, Slot 1: $0400-$07FF, Slot 2: $0800-$0BFF, Slot 3: $0C00-$0FFF.
/// </summary>
public class SaveFile
{
    public const int FileSize = 8192;
    public const int SlotCount = 3;
    public const int WorkingAreaOffset = 0x0000;
    public const int FirstSlotOffset = 0x0400;

    public SaveSlot[] Slots { get; } = new SaveSlot[SlotCount];

    public static SaveFile FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != FileSize)
            throw new ArgumentException($"SAV file must be exactly {FileSize} bytes, got {data.Length}.");

        var file = new SaveFile();
        for (int i = 0; i < SlotCount; i++)
        {
            int offset = FirstSlotOffset + (i * SaveSlot.Size);
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
            Array.Copy(slotBytes, 0, result, FirstSlotOffset + (i * SaveSlot.Size), SaveSlot.Size);
        }
        // Copy slot 1 to working area as well
        if (Slots[0].IsValid)
            Array.Copy(result, FirstSlotOffset, result, WorkingAreaOffset, SaveSlot.Size);
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
