using FF1SaveEditor.Core.IO;

namespace FF1SaveEditor.Tests;

public class ChecksumTests
{
    [Fact]
    public void Calculate_AllZeros_ReturnsZero()
    {
        var data = new byte[1024];
        var checksum = Checksum.Calculate(data);
        Assert.Equal(0, checksum);
    }

    [Fact]
    public void ComputeChecksumByte_MakesSumFF()
    {
        var data = new byte[1024];
        data[0] = 0x42;
        data[1] = 0x10;

        var span = data.AsSpan();
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);

        Assert.True(Checksum.Verify(data));
        Assert.Equal(0xFF, Checksum.Calculate(data));
    }

    [Fact]
    public void Verify_CorrectChecksum_ReturnsTrue()
    {
        var data = new byte[1024];
        data[0] = 0x42;
        var span = data.AsSpan();
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);
        Assert.True(Checksum.Verify(data));
    }

    [Fact]
    public void Verify_WrongChecksum_ReturnsFalse()
    {
        var data = new byte[1024];
        data[0] = 0x42;
        // Leave checksum as zero — sum won't be $FF
        Assert.False(Checksum.Verify(data));
    }

    [Fact]
    public void Verify_AllFF_SumsCorrectly()
    {
        var data = new byte[1024];
        Array.Fill(data, (byte)0xFF);
        // 1024 * 0xFF = 0x3FC01; low byte = 0x01, not 0xFF
        Assert.False(Checksum.Verify(data));
    }

    [Fact]
    public void RoundTrip_AfterModification_ChecksumStillValid()
    {
        var data = new byte[1024];
        for (int i = 0; i < 100; i++)
            data[i] = (byte)(i * 3);

        var span = data.AsSpan();
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);
        Assert.True(Checksum.Verify(data));

        // Modify and recompute
        data[50] = 0xAB;
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);
        Assert.True(Checksum.Verify(data));
    }
}
