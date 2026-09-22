namespace IESEViewModel.Tools;

public class DataWriter(byte[] data) : BinaryWriter(new MemoryStream(data))
{
    public bool BigEndian { get; set; } = false;

    public long Length => BaseStream.Length;

    public override void Write(byte[] data)
    {
        BaseStream.Write(data, 0, data.Length);
    }

    private void WriteByte(int number)
    {
        BaseStream.WriteByte(Convert.ToByte(number));
    }

    public void WriteInt16(int number)
    {
        byte[] data = BitConverter.GetBytes(number);

        for (int i = 0; i < 2; i++)
        {
            WriteByte(data[i]);
        }
    }

    public void WriteUInt16(int number)
    {
        byte[] data = BitConverter.GetBytes(number);
        Array.Reverse(data);

        for (int i = 0; i < 2; i++)
        {
            WriteByte(data[i]);
        }
    }

    public void WriteUInt32(uint number)
    {
        byte[] data = BitConverter.GetBytes(number);
        Array.Reverse(data);
        Write(data, 0, data.Length);
    }

    public void WriteByteArray(byte[] data)
    {
        foreach (var num in data)
            WriteByte(num);
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
}
