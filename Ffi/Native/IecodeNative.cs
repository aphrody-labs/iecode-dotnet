using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using IECODE.Core.Formats;
using IECODE.Core.Formats.Criware.CriFs.Compression;
using IECODE.Core.Formats.Level5.CfgBin.Rdbn;
using IECODE.Core.Native;
using IECODE.Core.Util;

namespace IECODE.Ffi.Native;

/// <summary>
/// Authoritative C-ABI exports for IECODE.Core via .NET NativeAOT.
/// Callable at zero overhead via bun:ffi or any C foreign function interface.
/// </summary>
public static unsafe class IecodeNative
{
    private static readonly byte[] VersionBytes = "2.0.4-dotnet10\0"u8.ToArray();
    private static readonly byte[] SchemaBytes = "iecode.ffi/v1\0"u8.ToArray();

    [UnmanagedCallersOnly(EntryPoint = "iecode_core_version")]
    public static byte* CoreVersion()
    {
        fixed (byte* p = VersionBytes) return p;
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_ffi_schema")]
    public static byte* Schema()
    {
        fixed (byte* p = SchemaBytes) return p;
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_status")]
    public static int Status() => 1;

    [UnmanagedCallersOnly(EntryPoint = "iecode_level5_crc32")]
    public static uint Level5Crc32(byte* input, int inputLen)
    {
        if (input == null || inputLen <= 0) return 0;
        return Level5Hash.Hash(new ReadOnlySpan<byte>(input, inputLen));
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_format_detect")]
    public static int FormatDetect(byte* input, int inputLen)
    {
        if (input == null || inputLen <= 0) return 0;
        var info = FormatDetector.Detect(new ReadOnlySpan<byte>(input, inputLen));
        return (int)info.Format;
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_cpk_decrypt_block")]
    public static int CpkDecryptBlock(byte* data, int len, ulong key, ulong offset)
    {
        if (data == null || len <= 0) return 0;
        uint k = key == 0 ? NativeCrypto.IEVRCriKey : (uint)key;
        NativeCrypto.DecryptBlock(new Span<byte>(data, len), (long)offset, k);
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_crilayla_decompress")]
    public static int CrilaylaDecompress(byte* input, int inputLen, byte** output, int* outputLen)
    {
        if (input == null || output == null || outputLen == null || inputLen < 16)
            return -1;

        if (!CriLayla.IsCompressed(input, out int compressedDataSize))
            return -2;

        int uncompSizeOfCompData = *(int*)(input + 8);
        int uncompHeaderOffset = *(int*)(input + 12);
        int totalUncompressed = uncompSizeOfCompData + CriLayla.UncompressedDataSize;

        byte* outBuf = (byte*)NativeMemory.Alloc((nuint)totalUncompressed);
        if (outBuf == null)
            return -3;

        try
        {
            CriLayla.Decompress(input, outBuf, uncompSizeOfCompData, uncompHeaderOffset);
            *output = outBuf;
            *outputLen = totalUncompressed;
            return 0;
        }
        catch
        {
            NativeMemory.Free(outBuf);
            *output = null;
            *outputLen = 0;
            return -4;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_crilayla_compress")]
    public static int CrilaylaCompress(byte* input, int inputLen, byte** output, int* outputLen)
    {
        if (input == null || output == null || outputLen == null || inputLen < 0)
            return -1;

        try
        {
            // CRILAYLA compressed format:
            // 0x10 Header (magic "CRILAYLA", uncomp_size, header_offset)
            // Bitstream (encoded literal bytes)
            // 0x100 Raw header block
            int rawBlockSize = Math.Min(inputLen, CriLayla.UncompressedDataSize);
            int uncompressedPayloadSize = inputLen > CriLayla.UncompressedDataSize
                ? (inputLen - CriLayla.UncompressedDataSize)
                : 0;

            byte[] bitstream;
            if (uncompressedPayloadSize > 0)
            {
                var bw = new BitStreamWriter();
                // Encode bytes from LAST to FIRST as literals (flag 0 + 8-bit byte)
                for (int i = inputLen - 1; i >= CriLayla.UncompressedDataSize; i--)
                {
                    bw.PushBit(0); // 0 = literal byte
                    bw.PushBits(input[i], 8);
                }
                bitstream = bw.Finish();
                Array.Reverse(bitstream);
            }
            else
            {
                bitstream = Array.Empty<byte>();
            }

            int headerOffset = bitstream.Length;
            int totalCompressedSize = 16 + headerOffset + CriLayla.UncompressedDataSize;

            byte* outBuf = (byte*)NativeMemory.Alloc((nuint)totalCompressedSize);
            if (outBuf == null)
                return -2;

            // Magic "CRILAYLA"
            *(ulong*)outBuf = 0x414C59414C495243;
            // uncompressed size of compressed payload
            *(int*)(outBuf + 8) = uncompressedPayloadSize;
            // header offset (offset to raw 0x100 block)
            *(int*)(outBuf + 12) = headerOffset;

            // Write bitstream
            if (headerOffset > 0)
            {
                fixed (byte* bsPtr = bitstream)
                {
                    Buffer.MemoryCopy(bsPtr, outBuf + 16, headerOffset, headerOffset);
                }
            }

            // Write raw block
            byte* rawDst = outBuf + 16 + headerOffset;
            NativeMemory.Clear(rawDst, (nuint)CriLayla.UncompressedDataSize);
            if (rawBlockSize > 0)
            {
                Buffer.MemoryCopy(input, rawDst, CriLayla.UncompressedDataSize, rawBlockSize);
            }

            *output = outBuf;
            *outputLen = totalCompressedSize;
            return 0;
        }
        catch
        {
            *output = null;
            *outputLen = 0;
            return -3;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_rdbn_is_valid")]
    public static int RdbnIsValid(byte* input, int inputLen)
    {
        if (input == null || inputLen < RdbnReader.MinimumSize) return 0;
        return RdbnReader.IsRdbn(new ReadOnlySpan<byte>(input, inputLen)) ? 1 : 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "iecode_bytes_free")]
    public static void BytesFree(byte* ptr)
    {
        if (ptr != null)
        {
            NativeMemory.Free(ptr);
        }
    }

    private sealed class BitStreamWriter
    {
        private readonly List<byte> _bytes = new(1024);
        private byte _current;
        private int _bitsUsed;

        public void PushBit(byte bit)
        {
            _current = (byte)((_current << 1) | (bit & 1));
            _bitsUsed++;
            if (_bitsUsed == 8)
            {
                _bytes.Add(_current);
                _current = 0;
                _bitsUsed = 0;
            }
        }

        public void PushBits(uint value, int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                PushBit((byte)((value >> i) & 1));
            }
        }

        public byte[] Finish()
        {
            if (_bitsUsed > 0)
            {
                _current = (byte)(_current << (8 - _bitsUsed));
                _bytes.Add(_current);
                _current = 0;
                _bitsUsed = 0;
            }
            return _bytes.ToArray();
        }
    }
}
