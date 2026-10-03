using System.Runtime.InteropServices;
using IECODE.Ffi;
using IECODE.Ffi.Native;
using Xunit;

namespace IECODE.Core.Tests;

public unsafe class NativeFfiBoundsTests
{
    [Fact]
    public void MetadataPointersSurviveCompactingCollections()
    {
        delegate* unmanaged<byte*> version = &IecodeNative.CoreVersion;
        delegate* unmanaged<byte*> schema = &IecodeNative.Schema;
        byte* beforeVersion = version();
        byte* beforeSchema = schema();
        string? text = Marshal.PtrToStringUTF8((nint)beforeVersion);
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        Assert.Equal((nint)beforeVersion, (nint)version());
        Assert.Equal((nint)beforeSchema, (nint)schema());
        Assert.Equal(text, Marshal.PtrToStringUTF8((nint)version()));
        Assert.Equal("iecode.ffi/v1", Marshal.PtrToStringUTF8((nint)schema()));
    }

    [Fact]
    public void DeclaredLengthLimitsNativeDecompressionAndClearsOutputs()
    {
        byte[] data = new byte[272];
        "CRILAYLA"u8.CopyTo(data);
        delegate* unmanaged<byte*, int, byte**, int*, int> decompress = &IecodeNative.CrilaylaDecompress;
        fixed (byte* input = data)
        {
            byte* output = (byte*)1;
            int outputLength = 42;
            Assert.Equal(-4, decompress(input, 16, &output, &outputLength));
            Assert.Equal(0, (nint)output);
            Assert.Equal(0, outputLength);
            Assert.Equal(0, decompress(input, data.Length, &output, &outputLength));
            Assert.Equal(256, outputLength);
            NativeMemory.Free(output);
            output = (byte*)1;
            outputLength = 42;
            Assert.Equal(-1, decompress(input, 0, &output, &outputLength));
            Assert.Equal(0, (nint)output);
            Assert.Equal(0, outputLength);
        }
        Assert.Null(NodeApiBridge.DecompressCrilayla(data[..16]));
    }

    [Fact]
    public void NativeCompressionRoundTripsLiteralPayload()
    {
        byte[] data = Enumerable.Range(0, 700).Select(i => (byte)i).ToArray();
        delegate* unmanaged<byte*, int, byte**, int*, int> compress = &IecodeNative.CrilaylaCompress;
        delegate* unmanaged<byte*, int, byte**, int*, int> decompress = &IecodeNative.CrilaylaDecompress;
        fixed (byte* input = data)
        {
            byte* compressed = null;
            byte* decoded = null;
            int compressedLength = 0;
            int decodedLength = 0;
            try
            {
                Assert.Equal(0, compress(input, data.Length, &compressed, &compressedLength));
                Assert.Equal(0, decompress(compressed, compressedLength, &decoded, &decodedLength));
                Assert.Equal(data, new ReadOnlySpan<byte>(decoded, decodedLength).ToArray());
            }
            finally
            {
                NativeMemory.Free(compressed);
                NativeMemory.Free(decoded);
            }
        }
    }
}
