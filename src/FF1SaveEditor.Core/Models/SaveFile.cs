namespace FF1SaveEditor.Core.Models;

/// <summary>
/// Represents a complete 8192-byte FF1 SRAM file.
/// FF1 has 1 save slot. The SRAM contains a working copy at $0000-$03FF
/// and a validated copy at $0400-$07FF. We operate on the validated copy.
/// </summary>
public class SaveFile
{
    public const int FileSize = 8192;
    public const int SlotCount = 1;
    public const int ValidatedRegionOffset = 0x0400;

    public SaveSlot Slot { get; private set; } = null!;

    /// <summary>Expose as array for IGamePlugin compatibility.</summary>
    public SaveSlot[] Slots => new[] { Slot };

    public static SaveFile FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != FileSize)
            throw new ArgumentException($"SAV file must be exactly {FileSize} bytes, got {data.Length}.");

        var file = new SaveFile();
        file.Slot = SaveSlot.FromBytes(data.Slice(ValidatedRegionOffset, SaveSlot.Size));
        return file;
    }

    public byte[] ToBytes()
    {
        var result = new byte[FileSize];
        var slotBytes = Slot.ToBytes();

        // Write to validated region ($0400-$07FF)
        Array.Copy(slotBytes, 0, result, ValidatedRegionOffset, SaveSlot.Size);
        // Also copy to working region ($0000-$03FF) for consistency
        Array.Copy(slotBytes, 0, result, 0, SaveSlot.Size);

        return result;
    }

    public static SaveFile CreateEmpty()
    {
        var file = new SaveFile();
        file.Slot = SaveSlot.CreateEmpty();
        return file;
    }
}
