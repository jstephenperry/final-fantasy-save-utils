using FF4SaveEditor.Core.IO;

namespace FF4SaveEditor.Tests;

public class ChecksumTests
{
    [Fact]
    public void Calculate_AllZeros_ReturnsZero()
    {
        var data = new byte[2048];
        var checksum = Checksum.Calculate(data);
        Assert.Equal(0, checksum);
    }

    [Fact]
    public void Calculate_SingleByteSet_CorrectOverlappingSum()
    {
        // With byte[0] = 0x42 and all others 0:
        // Iteration 0: data[0] | (data[1] << 8) = 0x42 | 0 = 0x42
        // No other iterations contribute because all other bytes are 0
        // But data[0] also appears as high byte in... no, iteration starts at i=0
        // Actually: i=0 reads [0]|[1]=0x42, all others are 0. BUT wait -
        // data[0] = 0x42 means it gets read in iteration 0 as low byte.
        // It doesn't get read as high byte because there's no iteration at i=-1.
        // Result: 0x42
        var data = new byte[2048];
        data[0] = 0x42;
        var checksum = Checksum.Calculate(data);
        Assert.Equal(0x42, checksum);
    }

    [Fact]
    public void Calculate_OverlappingWordsSumCorrectly()
    {
        // Set byte[5] = 0xFF. This byte participates in two overlapping reads:
        // Iteration 4: data[4] | (data[5] << 8) = 0 | (0xFF << 8) = 0xFF00
        // Iteration 5: data[5] | (data[6] << 8) = 0xFF | 0 = 0x00FF
        // Total: 0xFF00 + 0x00FF = 0xFFFF
        var data = new byte[2048];
        data[5] = 0xFF;
        var checksum = Checksum.Calculate(data);
        Assert.Equal(0xFFFF, checksum);
    }

    [Fact]
    public void Calculate_KnownPattern_ExpectedResult()
    {
        var data = new byte[2048];
        // Fill first 10 bytes with known values
        data[0] = 0x01; data[1] = 0x02; data[2] = 0x03; data[3] = 0x04; data[4] = 0x05;
        data[5] = 0x06; data[6] = 0x07; data[7] = 0x08; data[8] = 0x09; data[9] = 0x0A;

        // Manual calculation of overlapping 16-bit LE words for first 10 bytes:
        // i=0: 0x01 | (0x02 << 8) = 0x0201
        // i=1: 0x02 | (0x03 << 8) = 0x0302
        // i=2: 0x03 | (0x04 << 8) = 0x0403
        // i=3: 0x04 | (0x05 << 8) = 0x0504
        // i=4: 0x05 | (0x06 << 8) = 0x0605
        // i=5: 0x06 | (0x07 << 8) = 0x0706
        // i=6: 0x07 | (0x08 << 8) = 0x0807
        // i=7: 0x08 | (0x09 << 8) = 0x0908
        // i=8: 0x09 | (0x0A << 8) = 0x0A09
        // i=9: 0x0A | (0x00 << 8) = 0x000A
        // Sum: 0x0201+0x0302+0x0403+0x0504+0x0605+0x0706+0x0807+0x0908+0x0A09+0x000A
        // = 0x2D2D + 0x000A = 0x2D37 (let me compute precisely)
        // 0x0201 = 513
        // +0x0302 = 770 -> 1283
        // +0x0403 = 1027 -> 2310
        // +0x0504 = 1284 -> 3594
        // +0x0605 = 1541 -> 5135
        // +0x0706 = 1798 -> 6933
        // +0x0807 = 2055 -> 8988
        // +0x0908 = 2312 -> 11300
        // +0x0A09 = 2569 -> 13869
        // +0x000A = 10 -> 13879
        // 13879 = 0x3637 -- wait, let me recompute
        // 513+770=1283, +1027=2310, +1284=3594, +1541=5135, +1798=6933, +2055=8988, +2312=11300, +2569=13869, +10=13879
        // 13879 in hex: 13879 / 16 = 867 r 7 -> 867/16=54 r 3 -> 54/16=3 r 6 -> 0x3637
        var checksum = Checksum.Calculate(data);
        Assert.Equal((ushort)0x3637, checksum);
    }

    [Fact]
    public void Calculate_Wraps16Bit()
    {
        var data = new byte[2048];
        // Fill with 0xFF to force wrapping
        for (int i = 0; i < 100; i++)
            data[i] = 0xFF;

        var checksum = Checksum.Calculate(data);
        // With 65816-style carry propagation: 99 words of 0xFFFF + 1 word of 0x00FF
        // Carries accumulate across additions, resulting in 0x00FF
        Assert.Equal((ushort)0x00FF, checksum);
    }

    [Fact]
    public void Verify_CorrectChecksum_ReturnsTrue()
    {
        var data = new byte[2048];
        data[0] = 0x42;
        data[1] = 0x10;
        var checksum = Checksum.Calculate(data);
        // Write checksum at 0x7FC-0x7FD (LE)
        data[0x7FC] = (byte)(checksum & 0xFF);
        data[0x7FD] = (byte)((checksum >> 8) & 0xFF);
        Assert.True(Checksum.Verify(data));
    }

    [Fact]
    public void Verify_WrongChecksum_ReturnsFalse()
    {
        var data = new byte[2048];
        data[0] = 0x42;
        // Leave checksum as zero
        Assert.False(Checksum.Verify(data));
    }
}
