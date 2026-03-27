namespace FF2SaveEditor.Core.Models;

/// <summary>
/// Represents a complete 8192-byte FF2 SRAM file.
/// Working area: $0000-$02FF (slot 0), Slots 1-4: $0300/$0600/$0900/$0C00 (768 bytes each).
/// </summary>
public class SaveFile
{
    public const int FileSize = 8192;
    public const int SlotCount = 4;
    public const int FirstSlotOffset = 0x0300;

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
