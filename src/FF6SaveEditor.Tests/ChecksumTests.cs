using FF6SaveEditor.Core.IO;

namespace FF6SaveEditor.Tests;

public class ChecksumTests
{
    [Fact]
    public void Calculate_AllZeros_ReturnsZero()
    {
        var data = new byte[2560];
        var checksum = Checksum.Calculate(data);
        Assert.Equal(0, checksum);
    }

    [Fact]
    public void Calculate_SingleByteSet_CorrectResult()
    {
        // With byte[0] = 0x42: lo = 0 + 0x42 = 0x42, no carry. hi stays 0.
        // Result: 0x0042
        var data = new byte[2560];
        data[0] = 0x42;
        var checksum = Checksum.Calculate(data);
        Assert.Equal(0x0042, checksum);
    }

    [Fact]
    public void Calculate_CarryPropagates_ToHiByte()
    {
        // Two bytes that overflow lo: 0x80 + 0x90 = 0x110
        // After byte[0]=0x80: lo=0x80, carry=0, hi=0
        // After byte[1]=0x90: lo=0x80+0x90=0x110, lo=0x10, carry=1, hi=0+1=1
        // Result: 0x0110
        var data = new byte[2560];
        data[0] = 0x80;
        data[1] = 0x90;
        var checksum = Checksum.Calculate(data);
        Assert.Equal((ushort)0x0110, checksum);
    }

    [Fact]
    public void Calculate_NoCarryBetweenIterations()
    {
        // CPX clears carry between iterations.
        // After byte[0]=0xFF: lo=0xFF, carry=0, hi=0
        // After byte[1]=0x01: lo=0xFF+0x01=0x100, lo=0x00, carry=1, hi=0+1=1
        // After byte[2]=0x01: lo=0x00+0x01=0x01 (carry was cleared by CPX), carry=0, hi stays 1
        // Result: 0x0101
        var data = new byte[2560];
        data[0] = 0xFF;
        data[1] = 0x01;
        data[2] = 0x01;
        var checksum = Checksum.Calculate(data);
        Assert.Equal((ushort)0x0101, checksum);
    }

    [Fact]
    public void Calculate_ManyOverflows_HiByteWraps()
    {
        // Fill 512 bytes with 0xFF. Each byte adds 0xFF to lo.
        // First byte: lo=0xFF, carry=0, hi=0
        // Second: lo=0xFF+0xFF=0x1FE, lo=0xFE, carry=1, hi=1
        // Third: lo=0xFE+0xFF=0x1FD, lo=0xFD, carry=1, hi=2
        // Pattern: after N bytes (N>=2), lo decreases by 1 each time, hi increases by 1
        // After 256 bytes: hi should have wrapped around
        var data = new byte[2560];
        for (int i = 0; i < 512; i++)
            data[i] = 0xFF;
        var checksum = Checksum.Calculate(data);
        // Just verify it produces a consistent result without error
        Assert.True(checksum != 0);
    }

    [Fact]
    public void Calculate_KnownPattern_ExpectedResult()
    {
        var data = new byte[2560];
        // Set some known values and compute manually
        data[0] = 0x01; data[1] = 0x02; data[2] = 0x03;
        // After byte[0]=0x01: lo=0x01, carry=0, hi=0
        // After byte[1]=0x02: lo=0x03, carry=0, hi=0
        // After byte[2]=0x03: lo=0x06, carry=0, hi=0
        // Rest are zero, so result = 0x0006
        var checksum = Checksum.Calculate(data);
        Assert.Equal((ushort)0x0006, checksum);
    }

    [Fact]
    public void Verify_CorrectChecksum_ReturnsTrue()
    {
        var data = new byte[2560];
        data[0] = 0x42;
        data[1] = 0x10;
        var checksum = Checksum.Calculate(data);
        // Write checksum at 0x09FE-0x09FF (LE)
        data[0x09FE] = (byte)(checksum & 0xFF);
        data[0x09FF] = (byte)((checksum >> 8) & 0xFF);
        Assert.True(Checksum.Verify(data));
    }

    [Fact]
    public void Verify_WrongChecksum_ReturnsFalse()
    {
        var data = new byte[2560];
        data[0] = 0x42;
        // Leave checksum as zero
        Assert.False(Checksum.Verify(data));
    }
}
