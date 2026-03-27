using FF2SaveEditor.Core.IO;

namespace FF2SaveEditor.Tests;

public class ChecksumTests
{
    [Fact]
    public void Calculate_AllZeros_ReturnsZero()
    {
        var data = new byte[768];
        Assert.Equal(0, Checksum.Calculate(data));
    }

    [Fact]
    public void ComputeChecksumByte_MakesSumFF()
    {
        var data = new byte[768];
        data[0] = 0x42;
        data[1] = 0x10;
        data[Checksum.ValidityOffset] = Checksum.ValidityMarker;

        var span = data.AsSpan();
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);

        Assert.True(Checksum.Verify(data));
        Assert.Equal(0xFF, Checksum.Calculate(data));
    }

    [Fact]
    public void Verify_WithoutValidityMarker_ReturnsFalse()
    {
        var data = new byte[768];
        var span = data.AsSpan();
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);
        data[Checksum.ValidityOffset] = 0;
        Assert.False(Checksum.Verify(data));
    }

    [Fact]
    public void RoundTrip_ModifyAndRecompute()
    {
        var data = new byte[768];
        for (int i = 0; i < 100; i++)
            data[i] = (byte)(i * 5);
        data[Checksum.ValidityOffset] = Checksum.ValidityMarker;

        var span = data.AsSpan();
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);
        Assert.True(Checksum.Verify(data));

        data[50] = 0xEF;
        data[Checksum.ChecksumOffset] = Checksum.ComputeChecksumByte(span);
        Assert.True(Checksum.Verify(data));
    }
}
