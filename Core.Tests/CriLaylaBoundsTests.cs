using System.Buffers.Binary;
using IECODE.Core.Formats.Criware.CriFs.Compression;
using Xunit;

namespace IECODE.Core.Tests;

public class CriLaylaBoundsTests
{
    private static byte[] Container(uint size, uint offset, byte[]? compressed = null)
    {
        var data = new byte[272 + (compressed?.Length ?? 0)];
        "CRILAYLA"u8.CopyTo(data);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(8), size);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(12), offset);
        compressed?.CopyTo(data, 16);
        return data;
    }

    [Fact]
    public void RawPrefixRoundTrip()
    {
        byte[] data = Container(0, 0);
        for (int i = 0; i < 256; i++) data[16 + i] = (byte)i;
        Assert.Equal(data[16..], CriLayla.DecompressToArray(data));
    }

    [Fact]
    public void LiteralRoundTrip()
    {
        // Reverse-order stream: literal flag zero followed by byte 0x42.
        byte[] data = Container(1, 2, [0, 0x21]);
        byte[] decoded = CriLayla.DecompressToArray(data);
        Assert.Equal(257, decoded.Length);
        Assert.Equal(0x42, decoded[256]);
    }

    [Fact]
    public void ValidBackReferenceRoundTrips()
    {
        string bits = "0" + Convert.ToString((byte)'z', 2).PadLeft(8, '0')
            + "0" + Convert.ToString((byte)'y', 2).PadLeft(8, '0')
            + "0" + Convert.ToString((byte)'x', 2).PadLeft(8, '0')
            + "1" + new string('0', 15);
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '0');
        byte[] stream = Enumerable.Range(0, bits.Length / 8)
            .Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2)).Reverse().ToArray();
        byte[] decoded = CriLayla.DecompressToArray(Container(6, (uint)stream.Length, stream));
        Assert.Equal("xyzxyz"u8.ToArray(), decoded[256..]);
        using var rental = CriLayla.DecompressToArrayPool(Container(6, (uint)stream.Length, stream));
        Assert.Equal(decoded, rental.Span.ToArray());
    }

    [Fact]
    public void RejectsIncompletePrefix()
    {
        Assert.Throws<InvalidDataException>(() => CriLayla.DecompressToArray(Container(0, 0)[..16]));
    }

    [Theory]
    [InlineData(uint.MaxValue, 0u)]
    [InlineData(0u, uint.MaxValue)]
    public void RejectsUntrustedSizes(uint size, uint offset)
    {
        Assert.Throws<InvalidDataException>(() => CriLayla.DecompressToArray(Container(size, offset)));
    }

    [Fact]
    public void RejectsExhaustedBitstream()
    {
        Assert.Throws<InvalidDataException>(() => CriLayla.DecompressToArray(Container(32, 1, [0])));
    }

    [Fact]
    public void RejectsBackReferenceBeforeDecodedBytes()
    {
        Assert.Throws<InvalidDataException>(() => CriLayla.DecompressToArray(Container(3, 2, [0, 0x80])));
    }
}
