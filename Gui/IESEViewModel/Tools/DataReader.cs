namespace IESEViewModel.Tools;

public class DataReader(byte[] data) : BinaryReader(new MemoryStream(data))
{
    private static bool BigEndian => false;

    public long Length => BaseStream.Length;

    public override short ReadInt16()
    {
        return BitConverter.ToInt16(Reverse(base.ReadBytes(2)), 0);
    }

    public override ushort ReadUInt16()
    {
        return BitConverter.ToUInt16(Reverse(base.ReadBytes(2)), 0);
    }

    public override int ReadInt32()
    {
        return BitConverter.ToInt32(Reverse(base.ReadBytes(4)), 0);
    }

    public override uint ReadUInt32()
    {
        return BitConverter.ToUInt32(Reverse(base.ReadBytes(4)), 0);
    }

    public override long ReadInt64()
    {
        return BitConverter.ToInt64(Reverse(base.ReadBytes(8)), 0);
    }

    public override ulong ReadUInt64()
    {
        return BitConverter.ToUInt64(Reverse(base.ReadBytes(8)), 0);
    }

    public override float ReadSingle()
    {
        return BitConverter.ToSingle(Reverse(base.ReadBytes(4)), 0);
    }

    public void Skip(uint size)
    {
        BaseStream.Seek(size, SeekOrigin.Current);
    }

    private void Seek(uint position)
    {
        BaseStream.Seek(position, SeekOrigin.Begin);
    }

    private byte[] Reverse(byte[] b)
    {
        if (BitConverter.IsLittleEndian && BigEndian)
            Array.Reverse(b);
        return b;
    }

    public static uint Reverse(uint u)
    {
        byte[] b = BitConverter.GetBytes(u);
        Array.Reverse(b);
        return BitConverter.ToUInt32(b, 0);
    }

    public byte[] GetSection(uint offset, int size)
    {
        long temp = (uint)BaseStream.Position;
        Seek(offset);
        byte[] data = ReadBytes(size);
        Seek((uint)temp);
        return data;
    }

    public void PrintPosition()
    {
        Console.WriteLine(BaseStream.Position.ToString("X"));
    }
}