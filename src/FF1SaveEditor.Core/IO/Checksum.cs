namespace FF1SaveEditor.Core.IO;

/// <summary>
/// Implements the FF1 NES save checksum algorithm.
/// Based on Disch's disassembly (bank_0F.asm, VerifyChecksum at $C888):
/// CLC once, then for X=0..255: ADC $6400,X; ADC $6500,X; ADC $6600,X; ADC $6700,X.
/// The 8-bit sum (with carry propagation across iterations) must equal $FF.
/// Checksum modifier byte is at offset $FD within the validated save region.
/// </summary>
public static class Checksum
{
    public const int SaveRegionSize = 1024;
    public const int ChecksumOffset = 0xFD;

    /// <summary>
    /// Calculate the 8-bit checksum over 1024 bytes (4 pages of 256 bytes).
    /// Simulates the 6502 ADC with carry propagation.
    /// </summary>
    public static byte Calculate(ReadOnlySpan<byte> data)
    {
        if (data.Length < SaveRegionSize)
            throw new ArgumentException($"Data must be at least {SaveRegionSize} bytes.");

        int sum = 0; // includes carry in bits > 7
        for (int x = 0; x < 256; x++)
        {
            sum += data[x];           // page 0: $6400+X
            sum += data[256 + x];     // page 1: $6500+X
            sum += data[512 + x];     // page 2: $6600+X
            sum += data[768 + x];     // page 3: $6700+X
        }
        return (byte)(sum & 0xFF);
    }

    /// <summary>
    /// Compute the checksum byte value that makes the total sum equal $FF.
    /// Temporarily zeroes the checksum position, calculates the sum, then returns the needed value.
    /// </summary>
    public static byte ComputeChecksumByte(Span<byte> data)
    {
        byte original = data[ChecksumOffset];
        data[ChecksumOffset] = 0;
        byte sum = Calculate(data);
        data[ChecksumOffset] = original;
        // We need: (sum + checksumByte) & 0xFF == 0xFF
        return (byte)((0xFF - sum) & 0xFF);
    }

    /// <summary>
    /// Verify that the 8-bit sum of all 1024 bytes equals $FF.
    /// </summary>
    public static bool Verify(ReadOnlySpan<byte> data)
    {
        if (data.Length < SaveRegionSize)
            return false;
        return Calculate(data) == 0xFF;
    }
}
