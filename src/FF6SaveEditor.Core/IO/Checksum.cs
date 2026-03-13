namespace FF6SaveEditor.Core.IO;

/// <summary>
/// Implements the FF6 SNES save checksum algorithm.
/// Byte-by-byte ADC into a 16-bit accumulator (lo/hi bytes).
/// CPX #$09FE clears carry each iteration — no cross-iteration carry propagation.
/// Checksum stored at slot+$09FE.
/// </summary>
public static class Checksum
{
    private const int Iterations = 0x09FE;

    /// <summary>
    /// Calculate the 16-bit checksum for a 2560-byte save slot.
    /// </summary>
    public static ushort Calculate(ReadOnlySpan<byte> slotData)
    {
        if (slotData.Length < Iterations)
            throw new ArgumentException($"Slot data must be at least {Iterations} bytes.");

        byte lo = 0;
        byte hi = 0;
        for (int i = 0; i < Iterations; i++)
        {
            int total = lo + slotData[i]; // carry always 0 from CPX
            lo = (byte)(total & 0xFF);
            int carry = total > 0xFF ? 1 : 0;
            hi = (byte)(hi + carry); // carry from hi is overwritten by CPX
        }
        return (ushort)(lo | (hi << 8));
    }

    /// <summary>
    /// Verify a slot's stored checksum matches the calculated value.
    /// </summary>
    public static bool Verify(ReadOnlySpan<byte> slotData)
    {
        if (slotData.Length < Iterations + 2)
            return false;

        ushort stored = (ushort)(slotData[Iterations] | (slotData[Iterations + 1] << 8));
        ushort calculated = Calculate(slotData);
        return stored == calculated;
    }
}
