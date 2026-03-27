namespace FF2SaveEditor.Core.IO;

/// <summary>
/// Implements the FF2 NES save checksum algorithm.
/// Based on everything8215/ff2 disassembly (0F/DA8F):
/// CLC before each ADC (no carry propagation). Sums 3 pages (768 bytes).
/// Result EOR $FF stored at offset $FF (last byte).
/// Validation: $FE must be $5A, and sum of all 768 bytes + 1 == 0 (total == $FF).
/// </summary>
public static class Checksum
{
    public const int SlotSize = 768;
    public const int ChecksumOffset = 0xFF;
    public const int ValidityOffset = 0xFE;
    public const byte ValidityMarker = 0x5A;

    /// <summary>
    /// Calculate the raw 8-bit sum of all bytes in a 768-byte slot.
    /// </summary>
    public static byte Calculate(ReadOnlySpan<byte> data)
    {
        if (data.Length < SlotSize)
            throw new ArgumentException($"Data must be at least {SlotSize} bytes.");

        int sum = 0;
        for (int i = 0; i < SlotSize; i++)
            sum += data[i];
        return (byte)(sum & 0xFF);
    }

    /// <summary>
    /// Compute the checksum byte that makes the total sum equal $FF.
    /// </summary>
    public static byte ComputeChecksumByte(Span<byte> data)
    {
        byte original = data[ChecksumOffset];
        data[ChecksumOffset] = 0;
        data[ValidityOffset] = ValidityMarker;
        byte sum = Calculate(data);
        data[ChecksumOffset] = original;
        return (byte)((0xFF - sum) & 0xFF);
    }

    /// <summary>
    /// Verify that validity marker is $5A and total sum equals $FF.
    /// </summary>
    public static bool Verify(ReadOnlySpan<byte> data)
    {
        if (data.Length < SlotSize)
            return false;
        if (data[ValidityOffset] != ValidityMarker)
            return false;
        return Calculate(data) == 0xFF;
    }
}
