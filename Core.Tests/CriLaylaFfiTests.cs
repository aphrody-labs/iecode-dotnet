using System.Runtime.InteropServices;
using IECODE.Core.Formats.Criware.CriFs.Compression;
using IECODE.Ffi.Native;
using Xunit;

namespace IECODE.Core.Tests;

public unsafe class CriLaylaFfiTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(255)]
    public void RejectsInputsThatCannotBeRepresented(int size)
    {
        byte[] input = new byte[Math.Max(size, 1)];
        byte* output = (byte*)1;
        int outputLength = 123;
        delegate* unmanaged<byte*, int, byte**, int*, int> compress = &IecodeNative.CrilaylaCompress;
        fixed (byte* inputPtr = input)
            Assert.Equal(-1, compress(inputPtr, size, &output, &outputLength));
        Assert.True(output == null);
        Assert.Equal(0, outputLength);
    }

    [Theory]
    [InlineData(256)]
    [InlineData(257)]
    [InlineData(512)]
    public void CompressionRoundTripsExactBytes(int size)
    {
        byte[] input = Enumerable.Range(0, size).Select(i => (byte)(i * 37)).ToArray();
        byte* output = null;
        int outputLength = 0;
        delegate* unmanaged<byte*, int, byte**, int*, int> compress = &IecodeNative.CrilaylaCompress;
        try
        {
            fixed (byte* inputPtr = input)
                Assert.Equal(0, compress(inputPtr, size, &output, &outputLength));
            Assert.True(output != null);
            Assert.Equal(input, CriLayla.DecompressToArray(new ReadOnlySpan<byte>(output, outputLength)));
        }
        finally { NativeMemory.Free(output); }
    }
}
