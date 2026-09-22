using System.Runtime.InteropServices;

namespace IESEViewModel.Tools;

public class BinaryDataWriter : IDisposable
{
    public bool BigEndian { get; set; } = false;

    public long Length => BaseStream.Length;

    private Stream BaseStream { get; }

    public long Position => BaseStream.Position;

    public BinaryDataWriter(byte[] data)
    {
        BaseStream = new MemoryStream(data);
    }

    public BinaryDataWriter(Stream stream)
    {
        BaseStream = stream;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        BaseStream.Dispose();
    }

    public void Skip(uint size)
    {
        BaseStream.Seek(size, SeekOrigin.Current);
    }

    public void Seek(uint position)
    {
        BaseStream.Seek(position, SeekOrigin.Begin);
    }

    public void PrintPosition()
    {
        Console.WriteLine(BaseStream.Position.ToString("X"));
    }

    public void Write(byte[] data)
    {
        BaseStream.Write(data, 0, data.Length);
    }

    public void Write(byte value)
    {
        BaseStream.WriteByte(value);
    }

    public void Write(short value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void Write(int value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void Write(long value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void Write(ushort value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void Write(uint value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void Write(ulong value)
    {
        Write(BitConverter.GetBytes(value));
    }
    public void Write(float value)
    {
        Write(BitConverter.GetBytes(value));
    }

    private void WriteAlignment(int alignment, byte alignmentByte)
    {
        var remainder = BaseStream.Position % alignment;
        if (remainder <= 0) return;
        for (var i = 0; i < alignment - remainder; i++)
            Write(alignmentByte);
    }

    public void WriteAlignment()
    {
        Write((byte)0x00);
        WriteAlignment(16, 0xFF);
    }

    private void WriteStruct<T>(T structure)
    {
        byte[] bytes = new byte[Marshal.SizeOf<T>()];

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        Marshal.StructureToPtr(structure!, handle.AddrOfPinnedObject(), false);
        handle.Free();

        Write(bytes);
    }

    public void WriteMultipleStruct<T>(IEnumerable<T> structures)
    {
        foreach (T structure in structures)
        {
            WriteStruct(structure);
        }
    }
}