namespace FF4SaveEditor.Core.Models;

/// <summary>
/// Represents a complete 8192-byte FF4 SRAM file containing 4 save slots.
/// </summary>
public class SaveFile
{
    public const int FileSize = 8192;
    public const int SlotCount = 4;

    public SaveSlot[] Slots { get; } = new SaveSlot[SlotCount];

    public static SaveFile FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != FileSize)
            throw new ArgumentException($"SRM file must be exactly {FileSize} bytes, got {data.Length}.");

        var file = new SaveFile();
        for (int i = 0; i < SlotCount; i++)
        {
            int offset = i * SaveSlot.Size;
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
            Array.Copy(slotBytes, 0, result, i * SaveSlot.Size, SaveSlot.Size);
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
