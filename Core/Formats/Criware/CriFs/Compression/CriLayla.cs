using System.Buffers.Binary;
using IECODE.Core.Formats.Criware.CriFs.Definitions.Utilities;

namespace IECODE.Core.Formats.Criware.CriFs.Compression;

/// <summary>Bounded CRILAYLA compression reader.</summary>
public static class CriLayla
{
    public const int UncompressedDataSize = 0x100;
    public const int MinCopyLength = 3;
    private static readonly int[] CopyLengthWidths = [2, 3, 5, 8];

    public static bool IsCompressed(ReadOnlySpan<byte> data, out int compressedDataSize)
    {
        compressedDataSize = 0;
        if (data.Length < 16 || !data[..8].SequenceEqual("CRILAYLA"u8)) return false;
        uint offset = BinaryPrimitives.ReadUInt32LittleEndian(data[12..]);
        if (offset > int.MaxValue - 16 - UncompressedDataSize)
            throw new InvalidDataException("CRILAYLA header size exceeds the input limit.");
        compressedDataSize = (int)offset + 16 + UncompressedDataSize;
        return true;
    }

    public static ArrayRental DecompressToArrayPool(ReadOnlySpan<byte> input)
    {
        var rental = new ArrayRental(GetOutputSize(input));
        try
        {
            Decompress(input, rental.Span);
            return rental;
        }
        catch
        {
            rental.Dispose();
            throw;
        }
    }

    /// <summary>Maximum decoded allocation accepted for untrusted input (256 MiB).</summary>
    public const int MaximumOutputSize = 256 * 1024 * 1024;

    /// <summary>Decodes a complete, bounded CRILAYLA buffer.</summary>
    public static byte[] DecompressToArray(ReadOnlySpan<byte> input)
    {
        var output = new byte[GetOutputSize(input)];
        Decompress(input, output);
        return output;
    }

    private static int GetOutputSize(ReadOnlySpan<byte> input)
    {
        if (input.Length < 16 || !input[..8].SequenceEqual("CRILAYLA"u8))
            throw new InvalidDataException("Invalid CRILAYLA header.");
        uint decodedSize = BinaryPrimitives.ReadUInt32LittleEndian(input[8..]);
        uint headerOffset = BinaryPrimitives.ReadUInt32LittleEndian(input[12..]);
        if (decodedSize > MaximumOutputSize - UncompressedDataSize ||
            headerOffset > (uint)(input.Length - 16) ||
            input.Length - 16 - (long)headerOffset < UncompressedDataSize)
            throw new InvalidDataException("CRILAYLA size exceeds the input or output limit.");

        return (int)decodedSize + UncompressedDataSize;
    }

    private static void Decompress(ReadOnlySpan<byte> input, Span<byte> output)
    {
        int headerOffset = (int)BinaryPrimitives.ReadUInt32LittleEndian(input[12..]);
        input.Slice(16 + (int)headerOffset, UncompressedDataSize).CopyTo(output);
        var bits = new ReverseBitReader(input.Slice(16, (int)headerOffset));
        int position = output.Length - 1;
        while (position >= UncompressedDataSize)
        {
            if (bits.Read(1) == 0)
            {
                output[position--] = (byte)bits.Read(8);
                continue;
            }
            int offset = bits.Read(13) + MinCopyLength;
            int length = MinCopyLength;
            foreach (int width in CopyLengthWidths)
            {
                int value;
                do
                {
                    value = bits.Read(width);
                    length = checked(length + value);
                    if (length > position - UncompressedDataSize + 1)
                        throw new InvalidDataException("CRILAYLA copy exceeds the decoded payload.");
                } while (width == 8 && value == 255);
                if (value != (1 << width) - 1) break;
            }
            if (length > position - UncompressedDataSize + 1 || offset >= output.Length - position)
                throw new InvalidDataException("Invalid CRILAYLA back-reference.");
            for (int i = 0; i < length; i++, position--)
                output[position] = output[position + offset];
        }
    }

    private ref struct ReverseBitReader
    {
        private readonly ReadOnlySpan<byte> _data;
        private int _position;
        private int _remaining;
        public ReverseBitReader(ReadOnlySpan<byte> data)
        {
            _data = data;
            _position = data.Length;
            _remaining = 0;
        }
        public int Read(int count)
        {
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                if (_remaining == 0)
                {
                    if (_position == 0) throw new InvalidDataException("Truncated CRILAYLA bitstream.");
                    _position--;
                    _remaining = 8;
                }
                value = (value << 1) | ((_data[_position] >> --_remaining) & 1);
            }
            return value;
        }
    }

}
