namespace FF4SaveEditor.Core.IO;

/// <summary>
/// Implements the FF4 SNES save checksum algorithm.
/// Based on save.asm:121-140 - the 65816 CLCs once before the loop, then uses
/// ADC (add with carry) for 0x7FA iterations over overlapping 16-bit LE words.
/// The carry flag propagates between iterations.
/// </summary>
public static class Checksum
{
    private const int Iterations = 0x7FA;

    /// <summary>
    /// Calculate the 16-bit checksum for a 2048-byte save slot.
    /// </summary>
    public static ushort Calculate(ReadOnlySpan<byte> slotData)
    {
        if (slotData.Length < Iterations + 1)
            throw new ArgumentException($"Slot data must be at least {Iterations + 1} bytes.");

        ushort sum = 0;
        int carry = 0;
        for (int i = 0; i < Iterations; i++)
        {
            int word = slotData[i] | (slotData[i + 1] << 8);
            int total = sum + word + carry;
            sum = (ushort)(total & 0xFFFF);
            carry = total > 0xFFFF ? 1 : 0;
        }
        return sum;
    }

    /// <summary>
    /// Verify a slot's stored checksum matches the calculated value.
    /// </summary>
    public static bool Verify(ReadOnlySpan<byte> slotData)
    {
        if (slotData.Length < 0x7FE)
            return false;

        ushort stored = (ushort)(slotData[0x7FC] | (slotData[0x7FD] << 8));
        ushort calculated = Calculate(slotData);
        return stored == calculated;
    }
}
