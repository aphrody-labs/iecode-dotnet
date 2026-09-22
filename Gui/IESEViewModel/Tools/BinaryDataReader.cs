using System.Text;
using System.Runtime.InteropServices;

namespace IESEViewModel.Tools;

public class BinaryDataReader(Stream stream) : IDisposable
{
    private static bool BigEndian => false;

    public long Length => BaseStream.Length;

    private Stream BaseStream { get; } = stream;

    public long Position => BaseStream.Position;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        BaseStream.Dispose();
    }

    public T ReadValue<T>()
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        BaseStream.ReadExactly(bytes, 0, size);

        if (typeof(T) == typeof(byte))
        {
            return (T)(object)bytes[0];
        }

        if (BigEndian)
        {
            Array.Reverse(bytes);
        }

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject())!;
        handle.Free();

        return value;
    }

    public T[] ReadMultipleValue<T>(int count)
        => Enumerable.Range(0, count)
            .Select(_ => ReadValue<T>())
            .ToArray();

    public string ReadString(Encoding encoding)
    {
        List<byte> bytes = [];
        int b;

        while ((b = BaseStream.ReadByte()) != 0x0 && BaseStream.Position < BaseStream.Length)
        {
            bytes.Add((byte)b);
        }

        return encoding.GetString(bytes.ToArray());
    }

    public void Skip(uint size)
    {
        BaseStream.Seek(size, SeekOrigin.Current);
    }

    public void Seek(uint position)
    {
        BaseStream.Seek(position, SeekOrigin.Begin);
    }

    public byte[] GetSection(int size)
    {
        //long temp = _stream.Position;
        byte[] data = new byte[size];
        BaseStream.ReadExactly(data, 0, data.Length);
        return data;
    }

    public byte[] GetSection(uint offset, long size)
    {
        long temp = BaseStream.Position;
        Seek(offset);
        byte[] data = new byte[size];
        BaseStream.ReadExactly(data, 0, data.Length);
        Seek((uint)temp);
        return data;
    }

    private long Find<T>(T search, uint start) where T : struct, IEquatable<T>
    {
        int count = (int)(BaseStream.Length - start) / Marshal.SizeOf<T>();

        long temp = BaseStream.Position;
        Seek(start);

        T[] tableSearch = ReadMultipleStruct<T>(count);
        int foundIndex = Array.IndexOf(tableSearch, search);

        Seek((uint)temp);

        if (foundIndex != -1)
        {
            return start + foundIndex * Marshal.SizeOf<T>();
        }

        return -1;
    }

    public void SeekOf<T>(T search, uint start) where T : struct, IEquatable<T>
    {
        long pos = Find(search, start);

        if (pos != -1)
        {
            Seek((uint)pos);
        }
        else
        {
            throw new IndexOutOfRangeException();
        }
    }

    public void PrintPosition()
    {
        Console.WriteLine(BaseStream.Position.ToString("X"));
    }

    public T ReadStruct<T>()
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        BaseStream.ReadExactly(bytes, 0, size);

        if (BigEndian)
            Array.Reverse(bytes);

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject())!;
        handle.Free();

        return theStructure;
    }

    public T[] ReadMultipleStruct<T>(int count)
        => Enumerable.Range(0, count)
            .Select(_ => ReadStruct<T>())
            .ToArray();
}
