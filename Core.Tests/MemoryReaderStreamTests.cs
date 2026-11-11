using System.IO;
using ndsSharp.Core.Data.Reader;
using Xunit;

namespace IECODE.Core.Tests;

public sealed class MemoryReaderStreamTests
{
    [Fact]
    public void ReadsAndSeeksAcrossTheMemoryReader()
    {
        using var stream = new MemoryReaderStream(new MemoryReader([1, 2, 3, 4]));
        var buffer = new byte[2];

        Assert.Equal(2, stream.Read(buffer, 0, buffer.Length));
        Assert.Equal([1, 2], buffer);
        Assert.Equal(2, stream.Position);

        Assert.Equal(3, stream.Seek(-1, SeekOrigin.End));
        Assert.Equal(4, stream.ReadByte());
        Assert.Equal(4, stream.Position);
        Assert.Equal(-1, stream.ReadByte());
    }

    [Fact]
    public void ExposesReadOnlyStreamContract()
    {
        using var stream = new MemoryReaderStream(new MemoryReader([1]));

        stream.Flush();
        Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
        Assert.Throws<NotSupportedException>(() => stream.Write([1], 0, 1));
    }
}
