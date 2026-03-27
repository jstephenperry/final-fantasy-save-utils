namespace FF3SaveEditor.Core.IO;

/// <summary>
/// Implements the FF3 NES save checksum algorithm.
/// Based on everything8215/ff3 disassembly (3D/AE5C):
/// CLC before each ADC (no carry propagation between iterations).
/// Sums 4 pages of 256 bytes, then EOR $FF (ones complement).
/// Total of all bytes including checksum must equal $FF.
/// Checksum modifier byte is at slot offset $1A.
/// </summary>
public static class Checksum
{
    public const int SlotSize = 1024;
    public const int ChecksumOffset = 0x1A;
    public const int ValidityOffset = 0x19;
    public const byte ValidityMarker = 0x5A;

    /// <summary>
    /// Calculate the raw 8-bit sum of all bytes in a 1024-byte slot.
    /// CLC before each ADC means no carry propagation — simple byte sum mod 256.
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
    /// Compute the checksum byte that makes the total sum of the slot equal $FF.
    /// </summary>
    public static byte ComputeChecksumByte(Span<byte> data)
    {
        byte original = data[ChecksumOffset];
        data[ChecksumOffset] = 0;
        data[ValidityOffset] = ValidityMarker;
        byte sum = Calculate(data);
        data[ChecksumOffset] = original;
        // EOR $FF: we need sum_without_checksum + checksumByte ≡ $FF (mod 256)
        return (byte)((0xFF - sum) & 0xFF);
    }

    /// <summary>
    /// Verify that the total sum of all 1024 bytes equals $FF.
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
