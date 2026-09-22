using IESEViewModel.InazumaEleven.Games;
using IESEViewModel.Tools;
using IESEViewModel.InazumaEleven.Games.IEGO;
using IESEViewModel.InazumaEleven.Games.IEGOCS;
using IESEViewModel.InazumaEleven.Games.IEGOGalaxy;

namespace IESEViewModel;

public static class DataHandler
{
    public static async Task<IGame> Open(Stream stream)
    {
        BinaryDataReader reader = new(stream);
        reader.Skip(4);
        ushort header = reader.ReadValue<ushort>();

        if (header != 0x2CF1 & header != 0x4CF1 & header != 0x40F1 & header != 0x6CF1 & header != 0xC4F1)
        {
            reader = new BinaryDataReader(
                new MemoryStream(
                    await Level5.DecryptAsync(reader.GetSection(0, reader.Length)))
            );
            reader.Skip(4);
            header = reader.ReadValue<ushort>();
        }

        reader.Seek(0);

        return await Task.Run<IGame>(() => header switch
        {
            0x2CF1 => new Go(reader, false),
            0x6CF1 => new Go(reader, true),
            0x4CF1 => new Cs(reader, false),
            0xC4F1 => new Cs(reader, true),
            0x40F1 => new Galaxy(reader),
            _ => throw new FormatException("Save Game not supported")
        });
    }

    public static async Task<byte[]> SaveAsync(IGame game, bool encrypted = true)
    {
        byte[] saveData = game.Save();
        return encrypted ? await Level5.EncryptAsync(saveData) : saveData;
    }
}
