using System.Text;
using System.Collections;
using IESEViewModel.InazumaEleven.Common.GO;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Tools;

namespace IESEViewModel.InazumaEleven.Games.IEGOGalaxy;

public class Galaxy(BinaryDataReader data) : Galaxy<Common.GO.Galaxy>(data);

public class Galaxy<TCommon> : GameMiximax<TCommon> where TCommon : ICommonMixiMaxGame
{
    protected Galaxy(BinaryDataReader data) : base(data, GameVersion.Galaxy, "Inazuma Eleven Go Galaxy", 336)
    {
        Data = data;

        Inventory = GetInventory();
        Reserve = GetPlayers();
    }

    private Dictionary<int, Item> GetInventory()
    {
        // Group Item 1
        Data.Seek((uint)IeGoGalaxyHelper.ItemBlockGroup1Offset);
        Dictionary<int, Item> itemBlockGroup1 = Data.ReadMultipleStruct<IeGoGalaxyHelper.ItemBlockGroup1>(608).Where(x => Items.ContainsKey(x.ID) && x.ID != 0x00).ToDictionary(x => x.Index, y => new Item(y.ID, TCommon.Items[y.ID], y.Quantity));

        // Group Item 2
        Data.Seek((uint)IeGoGalaxyHelper.ItemBlockGroup2Offset);
        Dictionary<int, Item> itemBlockGroup2 = Data.ReadMultipleStruct<IeGoGalaxyHelper.ItemBlockGroup2>(452).Where(x => Items.ContainsKey(x.ID) && x.ID != 0x00).ToDictionary(x => x.Index, y => new Item(y.ID, TCommon.Items[y.ID], y.Quantity, y.QuantityEquiped));

        // Group Item 3
        Data.Seek((uint)IeGoGalaxyHelper.ItemBlockGroup3Offset);
        Dictionary<int, Item> itemBlockGroup3 = Data.ReadMultipleStruct<IeGoGalaxyHelper.ItemBlockGroup3>(888).Where(x => Items.ContainsKey(x.ID) && x.ID != 0x00).ToDictionary(x => x.Index, y => new Item(y.ID, TCommon.Items[y.ID]));

        // Find Equipment
        Equipments = itemBlockGroup2.Where(x => Common.GO.Galaxy.Equipments.ContainsKey(x.Value.Id)).ToDictionary(x => x.Key, y => Common.GO.Galaxy.Equipments[y.Value.Id]);
        Equipments.Add(0, Common.GO.Galaxy.Equipments[0x0]);

        // Merge
        // ReSharper disable twice UsageOfDefaultStructEquality
        return itemBlockGroup1.Union(itemBlockGroup2).Union(itemBlockGroup3).ToDictionary(x => x.Key, x => x.Value);
    }

    public override void SaveInventory(BinaryDataWriter writer)
    {
        // Group Item 1
        writer.Seek((uint)IeGoGalaxyHelper.ItemBlockGroup1Offset);
        Dictionary<int, Item> itemGroup1 = Inventory.Where(x => x.Value.Category == 1).ToDictionary(x => x.Key, y => y.Value);
        for (int i = 0; i < 608; i++)
        {
            if (i < itemGroup1.Count)
            {
                KeyValuePair<int, Item> item = itemGroup1.ElementAt(i);
                writer.Write(item.Key);
                writer.Write(item.Value.Id);
                writer.Write(item.Value.Quantity);
            }
            else
            {
                writer.Write(0x00);
                writer.Write(0x00);
                writer.Write(0x00);
            }
        }

        // Calculates the number of Fighting Spirits used
        foreach (KeyValuePair<int, Item> item in Inventory.Where(x => x.Value.SubCategory == 21))
        {
            int count = Reserve.Count(x => x.Avatar != null && Avatars.FirstOrDefault(y => y.Value.Name == x.Avatar.Name).Key == item.Value.Id);

            if (count > item.Value.Quantity)
            {
                item.Value.Quantity = count;
            }

            item.Value.QuantityEquiped = count;
        }

        // Calculates the number of Fighting Spirits used
        foreach (KeyValuePair<int, Item> item in Inventory.Where(x => x.Value.SubCategory is > 0 and < 5))
        {
            int count = Reserve.Count(x => x.Equipments?.FirstOrDefault(y => y.Name == item.Value.Name) != null);

            if (count > item.Value.Quantity)
            {
                item.Value.Quantity = count;
            }

            item.Value.QuantityEquiped = count;
        }

        // Group Item 2
        writer.Seek((uint)IeGoGalaxyHelper.ItemBlockGroup2Offset);
        Dictionary<int, Item> itemGroup2 = Inventory.Where(x => x.Value.Category == 2).ToDictionary(x => x.Key, y => y.Value);
        for (int i = 0; i < 452; i++)
        {
            if (i < itemGroup2.Count)
            {
                KeyValuePair<int, Item> item = itemGroup2.ElementAt(i);
                writer.Write(item.Key);
                writer.Write(item.Value.Id);
                writer.Write(item.Value.Quantity);
                writer.Write(item.Value.QuantityEquiped);
            }
            else
            {
                writer.Write(0x00);
                writer.Write(0x00);
                writer.Write(0x00);
                writer.Write(0x00);
            }
        }

        // Group Item 3
        writer.Seek((uint)IeGoGalaxyHelper.ItemBlockGroup3Offset);
        Dictionary<int, Item> itemGroup3 = Inventory.Where(x => x.Value.Category == 3).ToDictionary(x => x.Key, y => y.Value);
        for (int i = 0; i < 888; i++)
        {
            if (i < itemGroup3.Count)
            {
                KeyValuePair<int, Item> item = itemGroup3.ElementAt(i);
                writer.Write(item.Key);
                writer.Write(item.Value.Id);
            }
            else
            {
                writer.Write(0x00);
                writer.Write(0x00);
            }
        }
    }

    private List<Player> GetPlayers()
    {
        // Get player index
        Data.Seek((uint)IeGoGalaxyHelper.PlayerIndexOffset);
        int[] index = Data.ReadMultipleStruct<int>(IeGoGalaxyHelper.MaximumPlayer)
            .Where(x => x != 0x0)
            .ToArray();

        // Get player data according to their index
        Data.Seek((uint)IeGoGalaxyHelper.PlayerDataOffset);
        List<IeGoGalaxyHelper.PlayerBlock> playerBlocks = Data.ReadMultipleStruct<IeGoGalaxyHelper.PlayerBlock>(index.Length)
            .ToList();

        // Create player objects and apply transformations
        List<Player> players = playerBlocks
            .Where(playerBlock => Players.ContainsKey(playerBlock.ID))
            .Select(playerBlock =>
            {
                Player player = new Player(Players[playerBlock.ID])
                {
                    Id = playerBlock.ID,
                    Index = playerBlock.Index,
                    Level = playerBlock.Level,
                    Fp = playerBlock.GP,
                    Tp = playerBlock.TP,
                    InvestedPoint = playerBlock.InvestedPoint.Select(Convert.ToInt32).ToList(),
                    Moves = playerBlock.Moves
                        .Select(moveBlock => new Move(Moves.TryGetValue(moveBlock.MoveID, out var move) ? move : Moves[0x0], moveBlock.MoveLevel, moveBlock.MoveLearned, moveBlock.MoveUsage))
                        .ToList(),
                    Avatar = playerBlock.TotemID != 0x00 ?
                        new Avatar(Avatars[playerBlock.TotemID], 1, 1) :
                        new Avatar(Avatars[playerBlock.AvatarID], playerBlock.AvatarLevel, playerBlock.AvatarUsage),
                    Invoke = (playerBlock.InvokeStatus & 8) != 0,
                    Armed = (playerBlock.InvokeStatus & 16) != 0,
                    Equipments =
                    [
                        Equipments[playerBlock.BootsIndex],
                        Equipments[playerBlock.BraceletIndex],
                        Equipments[playerBlock.PendantIndex],
                        Equipments[playerBlock.GlovesIndex]
                    ],
                    Style = (playerBlock.Style & 0xF0) >> 4,
                    Participation = playerBlock.ParticipationPoint,
                    Score = playerBlock.ScorePoint
                };

                if (playerBlock.MiximaxIndex != 0x00)
                {
                    player.MixiMax = new MixiMax();
                }

                player.IsTrained = player.InvestedPoint.Any(value => value > 0);

                return player;
            })
            .ToList();

        // Link Miximax
        foreach (Player player in players.Where(p => p.MixiMax != null))
        {
            int miximaxIndex = playerBlocks.First(x => x.Index == player.Index).MiximaxIndex;
            int miximaxMove1 = playerBlocks.First(x => x.Index == player.Index).MixiMaxMove1Index;
            int miximaxMove2 = playerBlocks.First(x => x.Index == player.Index).MixiMaxMove2Index;

            Player? searchPlayer = players.Find(x => x.Index == miximaxIndex);
            if (searchPlayer is not null)
            {
                // The aura is a Player
                BestMatch? isBestMatch = IsBestMatch(player.Id, searchPlayer.Id);

                player.MixiMax = isBestMatch != null ?
                    new MixiMax(searchPlayer, (miximaxMove1, miximaxMove2), isBestMatch) :
                    new MixiMax(searchPlayer, (miximaxMove1, miximaxMove2));

                searchPlayer.IsAura = true;
            }
            else
            {
                // Miximax not found -> remove miximax status
                player.MixiMax = null;
            }
        }

        // Sort
        return players.OrderBy(player => Array.IndexOf(index, player.Index)).ToList();
    }

    public override void SavePlayers(BinaryDataWriter writer)
    {
        // Save player index
        writer.Seek((uint)IeGoGalaxyHelper.PlayerIndexOffset);
        for (int i = 0; i < IeGoGalaxyHelper.MaximumPlayer; i++)
        {
            writer.Write(i < Reserve.Count ? Reserve[i].Index : 0x0);
        }

        // Save player data
        writer.Seek((uint)IeGoGalaxyHelper.PlayerDataOffset);
        List<Player> sortedReserve = Reserve.OrderBy(player => player.Index).ToList();
        for (int i = 0; i < IeGoGalaxyHelper.MaximumPlayer; i++)
        {
            if (i < sortedReserve.Count)
            {
                Player player = sortedReserve[i];

                if (player.Avatar is not null)
                {
                    writer.Write(player.Avatar.IsFightingSpirit ? 0x0 : Avatars.FirstOrDefault(x => x.Value.Name == player.Avatar.Name).Key);
                }
                else
                {
                    Console.Error.WriteLine("Player avatar is null");
                }

                writer.Write(player.Index);
                writer.Write(player.Id);

                if (player.MixiMax?.AuraPlayer is not null)
                {
                    writer.Write(player.MixiMax != null ? player.MixiMax.AuraPlayer.Index : 0x00);
                }
                else
                {
                    Console.Error.WriteLine("Player miximax aura player is null");
                }

                writer.Skip(0x0C);
                writer.Write((short)player.Fp);
                writer.Write((short)player.Tp);
                writer.Write((short)player.Freedom);
                writer.Write((byte)player.Level);
                writer.Skip(0x01);
                writer.Write((byte)(player.MixiMax != null ? player.MixiMax.MixiMaxMoveNumber[0] : 0x00));
                writer.Write((byte)(player.MixiMax != null ? player.MixiMax.MixiMaxMoveNumber[1] : 0x00));

                // Determines the Invoker value and saves it
                int canInvokeArmed = Convert.ToInt32(player.Invoke) * 8 + Convert.ToInt32(player.Armed) * 16;
                if (player.MixiMax is { AuraData: true }) canInvokeArmed += 1;
                var playerIsAura = Reserve.FirstOrDefault(x => x.MixiMax != null && x.MixiMax.AuraPlayer == player);
                if (playerIsAura != null) canInvokeArmed += 2;
                writer.Write((byte)canInvokeArmed);

                writer.Write((byte)(player.Style << 4 & 0xF0));
                writer.Write((short)player.Participation);
                writer.Write((short)player.Score);
                writer.Skip(0x04);
                writer.Skip(0x04);

                if (player.InvestedPoint is not null)
                {
                    writer.Write(player.InvestedPoint.SelectMany(x => BitConverter.GetBytes((short)x)).ToArray());
                }

                if (player.Avatar is not null)
                {
                    writer.Write(player.Avatar.IsFightingSpirit ? Avatars.FirstOrDefault(x => x.Value.Name == player.Avatar.Name).Key : 0x00);

                    if (player.Avatar.Level is not null)
                    {
                        writer.Write(player.Avatar.IsFightingSpirit ? (byte)player.Avatar.Level : (byte)1);
                    }
                    else
                    {
                        Console.Error.WriteLine("Player avatar level is null");
                    }

                    if (player.Avatar.Usage is not null)
                    {
                        writer.Write(player.Avatar.IsFightingSpirit ? (byte)player.Avatar.Usage : (byte)0);
                    }
                    else
                    {
                        Console.Error.WriteLine("Player avatar usage is null");
                    }
                }
                else
                {
                    Console.Error.WriteLine("Player avatar is null");
                }

                writer.Skip(0x01);
                writer.Skip(0x01);

                if (player.Equipments is not null)
                {
                    writer.Write(Equipments.FirstOrDefault(x => x.Value == player.Equipments[0]).Key);
                    writer.Write(Equipments.FirstOrDefault(x => x.Value == player.Equipments[1]).Key);
                    writer.Write(Equipments.FirstOrDefault(x => x.Value == player.Equipments[2]).Key);
                    writer.Write(Equipments.FirstOrDefault(x => x.Value == player.Equipments[3]).Key);
                }
                else
                {
                    Console.Error.WriteLine("Player equipments is null");
                }

                writer.Skip(0x04);

                for (int j = 0; j < 6; j++)
                {
                    Move? move = player.Moves?[j];

                    if (move is null)
                    {
                        Console.Error.WriteLine("move was null");
                        continue;
                    }

                    KeyValuePair<uint, Move> moveKeyValue = Moves.FirstOrDefault(x => x.Value.Name == move.Name);
                    writer.Write(moveKeyValue.Key);
                    writer.Write((byte)move.Level);
                    writer.Write((byte)move.UsedCount);
                    writer.Write(Convert.ToInt32(move.Unlock));
                    writer.Skip(2);
                }

                writer.Skip(0x64);
            }
            else
            {
                int[] emptyPlayerData = Enumerable.Repeat(0, 67).ToArray();
                writer.Write(emptyPlayerData.SelectMany(BitConverter.GetBytes).ToArray());
            }
        }
    }

    public override Dictionary<int, List<PlayRecord>> GetPlayRecords()
    {
        Dictionary<int, List<PlayRecord>> playRecords = TCommon.PlayRecords;

        Data.Seek((uint)IeGoGalaxyHelper.PlayRecordsOffset);

        foreach (KeyValuePair<int, List<PlayRecord>> playRecord in playRecords)
        {
            BitArray bits = new BitArray(new int[] { Data.ReadValue<byte>() });

            for (int i = 0; i < playRecord.Value.Count; i++)
            {
                playRecord.Value[playRecord.Value.Count - 1 - i].Unlocked = bits[i];
            }
        }

        return playRecords;
    }

    public override void SavePlayRecords(BinaryDataWriter writer)
    {
        if (!PlayRecordsInitialized) return;

        writer.Seek((uint)IeGoGalaxyHelper.PlayRecordsOffset);

        foreach (var listBool in PlayRecords.Select(playRecord => playRecord.Value.Select(x => x.Unlocked).ToList()))
        {
            while (listBool.Count < 8)
            {
                listBool.Add(true);
            }

            BitArray bits = new BitArray(listBool.ToArray());
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            writer.Write(bytes[0]);
        }
    }

    public override SaveInfo GetSaveInfo()
    {
        // Name
        Data.Seek((uint)IeGoGalaxyHelper.NameOffset);
        string name = Data.ReadString(Encoding.UTF8);
        Data.Seek((uint)IeGoGalaxyHelper.TeamNameOffset);
        string teamName = Data.ReadString(Encoding.UTF8);

        // Time Hours
        Data.Seek((uint)IeGoGalaxyHelper.TimeOffset);
        int seconds = Data.ReadValue<int>();
        int hours = seconds / 3600;
        int minutes = seconds / 60 % 60;
        if (hours > 999)
        {
            hours = 999;
        }
        if (minutes > 59)
        {
            minutes = 59;
        }

        // Link Level
        Data.Seek((uint)IeGoGalaxyHelper.LinkOffset);
        byte secretLinkLevel = Data.ReadValue<byte>();

        // Current Chapter
        Data.Seek((uint)IeGoGalaxyHelper.ChapterOffset);
        byte chapter = Data.ReadValue<byte>();

        // Money
        Data.Seek((uint)IeGoGalaxyHelper.MoneyOffset);
        int prestige = Data.ReadValue<int>();
        int friendship = Data.ReadValue<int>();

        // Coins
        Data.Seek((uint)IeGoGalaxyHelper.CoinOffset);

        int[] coins = new int[5].Select(_ => Data.ReadValue<int>()).ToArray();

        return new SaveInfo(name, teamName, hours, minutes, prestige, friendship, secretLinkLevel, chapter, coins);
    }

    public override void SaveSaveInfo(BinaryDataWriter writer)
    {
        if (!SaveInfoInitialized) return;

        // Save Name
        writer.Seek((uint)IeGoGalaxyHelper.NameOffset);
        writer.Write(Encoding.UTF8.GetBytes(SaveInfo.Name));
        writer.Write((byte)0x0);
        writer.Write((byte)0x88);

        // Team Name
        writer.Seek((uint)IeGoGalaxyHelper.TeamNameOffset);
        writer.Write(Encoding.UTF8.GetBytes(SaveInfo.TeamName));
        writer.Write((byte)0x0);
        writer.Write((byte)0x88);

        // Time Hours
        writer.Seek((uint)IeGoGalaxyHelper.TimeOffset);
        int time = SaveInfo.Hours * 3600 + SaveInfo.Minutes * 60;
        writer.Write(time);

        // Link Level
        writer.Seek((uint)IeGoGalaxyHelper.LinkOffset);
        if (SaveInfo.SecretLinkLevel is null) throw new ArgumentException(nameof(SaveInfo.SecretLinkLevel));
        writer.Write((byte)SaveInfo.SecretLinkLevel);

        // Money
        writer.Seek((uint)IeGoGalaxyHelper.MoneyOffset);
        writer.Write(SaveInfo.Prestige);

        // Coins
        writer.Seek((uint)IeGoGalaxyHelper.CoinOffset);
        if (SaveInfo.Coins is null) throw new ArgumentException(nameof(SaveInfo.Coins));

        foreach (int coin in SaveInfo.Coins)
        {
            writer.Write((short)coin);
        }

        if (!SaveInfo.UnlockAllData) return;
        writer.Seek(0x8F62);
        writer.Write([0xB9, 0x08]);
        writer.Skip(6);
        writer.Write([0x46, 0xC9]);

        writer.Seek(0x8FFD);
        writer.Write([0x1C, 0x0E, 0xFE]);

        writer.Seek(0x9000);
        writer.Write([0x1F, 0x00, 0x00, 0x00, 0x00, 0xFC, 0xF9, 0xFF, 0xFF, 0xFF, 0x7F]);

        writer.Seek(0x902F);
        writer.Write([0xF8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0x00, 0x0E]);

        // Ugly Write
        writer.Seek(0x2ECB0);
        writer.Write([0x76, 0x00, 0x00, 0x00, 0x04, 0x44, 0x9D, 0x00, 0xC4, 0xB4, 0x8A, 0x81, 0x04, 0x44, 0x9D, 0x00, 0x52, 0x84, 0x8D, 0xF6, 0x04, 0x44, 0x9D, 0x00, 0x7E, 0xE5, 0x83, 0x18, 0x0D, 0x44, 0x9D, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x02, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x03, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x05, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x06, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x07, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x08, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x09, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x10, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x11, 0x00, 0x00, 0x00, 0x0D, 0x44, 0x9D, 0x00, 0x12, 0x00, 0x00, 0x00, 0x14, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x44, 0x9D, 0x00, 0x99, 0xCA, 0xBD, 0x07, 0x16, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x44, 0x9D, 0x00, 0x62, 0x10, 0x4C, 0x3B, 0x00, 0x44, 0x9D, 0x00, 0xCA, 0x40, 0x78, 0xBB, 0x00, 0x44, 0x9D, 0x00, 0xEC, 0x16, 0xC5, 0x8D, 0x00, 0x44, 0x9D, 0x00, 0xBA, 0xF2, 0x4E, 0x43, 0x00, 0x44, 0x9D, 0x00, 0x3B, 0x7F, 0x3F, 0xC9, 0x04, 0x44, 0x9D, 0x00, 0xA8, 0x92, 0x66, 0x99, 0x10, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x44, 0x9D, 0x00, 0x20, 0x74, 0xE7, 0xF2, 0x04, 0x44, 0x9D, 0x00, 0xEC, 0xC0, 0x9C, 0xB2, 0x00, 0x44, 0x9D, 0x00, 0x76, 0x01, 0x91, 0x7C, 0x0E, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x44, 0x9D, 0x00, 0xC1, 0xF3, 0x40, 0x00, 0x00, 0x44, 0x9D, 0x00, 0x74, 0x37, 0x3F, 0x2A, 0x00, 0x44, 0x9D, 0x00, 0xC9, 0x4B, 0x8E, 0x54, 0x00, 0x44, 0x9D, 0x00, 0xCE, 0x66, 0x36, 0xB3, 0x00, 0x44, 0x9D, 0x00, 0x17, 0x1E, 0x31, 0x27, 0x00, 0x44, 0x9D, 0x00, 0x81, 0x2E, 0x36, 0x50, 0x00, 0x44, 0x9D, 0x00, 0x14, 0xDD, 0xEF, 0x8C, 0x00, 0x44, 0x9D, 0x00, 0x58, 0x56, 0x31, 0xC4, 0x00, 0x44, 0x9D, 0x00, 0x06, 0x85, 0x13, 0x45, 0x00, 0x44, 0x9D, 0x00, 0xC8, 0x40, 0x62, 0x2C, 0x00, 0x44, 0x9D, 0x00, 0xE4, 0x21, 0x6C, 0xC2, 0x0E, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x44, 0x9D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0xEB, 0x00, 0x57, 0xC3, 0x47, 0x77, 0x00, 0x28, 0xEB, 0x00, 0xEB, 0x53, 0x9B, 0x40, 0x00, 0x28, 0xEB, 0x00, 0xB4, 0x8B, 0x55, 0xB9, 0x00, 0x28, 0xEB, 0x00, 0x7D, 0x63, 0x9C, 0x37, 0x00, 0x28, 0xEB, 0x00, 0xC0, 0x1F, 0x2D, 0x49, 0x00, 0x28, 0xEB, 0x00, 0x41, 0xBE, 0xC5, 0xD0, 0x00, 0x28, 0xEB, 0x00, 0xFB, 0xEF, 0xCC, 0x49, 0x0E, 0x28, 0xEB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0xEB, 0x00, 0x55, 0xEC, 0xF4, 0x95, 0x00, 0x28, 0xEB, 0x00, 0xC3, 0xDC, 0xF3, 0xE2, 0x00, 0x28, 0xEB, 0x00, 0x79, 0x8D, 0xFA, 0x7B, 0x00, 0x28, 0xEB, 0x00, 0xB7, 0x48, 0x8B, 0x12, 0x00, 0xBC, 0x0E, 0x03, 0x8F, 0x7B, 0xB4, 0xB9, 0x00, 0xBC, 0x0E, 0x03, 0x1E, 0x66, 0x0B, 0x29, 0x00, 0xBC, 0x0E, 0x03, 0x0F, 0xD6, 0x23, 0x63, 0x00, 0xBC, 0x0E, 0x03, 0x20, 0x1C, 0x15, 0x0C, 0x10, 0xBC, 0x0E, 0x03, 0x41, 0xB6, 0xFE, 0xFE, 0x0D, 0xBC, 0x0E, 0x03, 0x13, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x14, 0x00, 0x00, 0x00, 0x06, 0xBC, 0x0E, 0x03, 0x00, 0x00, 0x00, 0x00, 0x08, 0xBC, 0x0E, 0x03, 0x00, 0x00, 0x00, 0x00, 0x0A, 0xBC, 0x0E, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBC, 0x0E, 0x03, 0xEE, 0xD9, 0x64, 0x65, 0x00, 0xBC, 0x0E, 0x03, 0x0E, 0xDA, 0x5C, 0x20, 0x00, 0xBC, 0x0E, 0x03, 0x98, 0xEA, 0x5B, 0x57, 0x00, 0xBC, 0x0E, 0x03, 0x22, 0xBB, 0x52, 0xCE, 0x10, 0xBC, 0x0E, 0x03, 0x4F, 0xC8, 0x76, 0x3F, 0x10, 0xBC, 0x0E, 0x03, 0xF5, 0x99, 0x7F, 0xA6, 0x0D, 0xBC, 0x0E, 0x03, 0x15, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x16, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x17, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x18, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x19, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x1A, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x1B, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x1C, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x1D, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x1E, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x1F, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x20, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x21, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x22, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x23, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x24, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x25, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x26, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x27, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x28, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x29, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x2A, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x2B, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x2C, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x2D, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x2E, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x2F, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x30, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x31, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x32, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x33, 0x00, 0x00, 0x00, 0x0D, 0xBC, 0x0E, 0x03, 0x34, 0x00, 0x00, 0x00, 0x00, 0xBC, 0x0E, 0x03, 0x06, 0x50, 0x85, 0xE8, 0x00, 0xBC, 0x0E, 0x03, 0xE6, 0x3B, 0x5B, 0x87, 0x00, 0xBC, 0x0E, 0x03, 0xC8, 0x95, 0xF4, 0x81, 0x00, 0xBC, 0x0E, 0x03, 0x8D, 0x0D, 0xC6, 0x8B, 0x00, 0xBC, 0x0E, 0x03, 0xEA, 0xE6, 0x37, 0xD9, 0x00, 0xBC, 0x0E, 0x03, 0xC7, 0xCC, 0x09, 0x2B, 0x00, 0xBC, 0x0E, 0x03, 0xBA, 0xDE, 0xD7, 0x50, 0x00, 0xBC, 0x0E, 0x03, 0x34, 0xD8, 0x5E, 0xE6, 0x00, 0xBC, 0x0E, 0x03, 0x0B, 0xC6, 0xD9, 0xAA
        ]);
    }

    public override List<Team> GetTactics()
    {
        List<Team> teams =
        [
            LoadTeam(0x268E0, (uint)IeGoGalaxyHelper.TeamNameOffset, 0x26E28)
            // Custom Teams
        ];

        // Selection Team

        // Custom Teams
        uint teamInfo = 0x26914;
        uint teamName = 0x26B50;
        uint teamPlayers = 0x273A8;
        for (int i = 0; i < 10; i++)
        {
            teams.Add(LoadTeam(teamInfo, teamName, teamPlayers));
            teamInfo += 0x34;
            teamName += 0x20;
            teamPlayers += 0x40;
        }

        return teams;
    }

    public override void SaveTactics(BinaryDataWriter writer)
    {
        if (!TeamsInitialized) return;

        // Chrono Storm Team
        SaveTeam(writer, Teams[0], 0x268E0, (uint)IeGoGalaxyHelper.TeamNameOffset, 0x26E28);

        // Custom Teams
        uint teamInfo = 0x26914;
        uint teamName = 0x26B50;
        uint teamPlayers = 0x273A8;
        for (int i = 0; i < 10; i++)
        {
            SaveTeam(writer, Teams[i + 1], teamInfo, teamName, teamPlayers);
            teamInfo += 0x34;
            teamName += 0x20;
            teamPlayers += 0x40;
        }
    }


    private Team LoadTeam(uint teamInfo, uint teamName, uint teamPlayers)
    {
        Data.Seek(teamInfo);

        IeGoGalaxyHelper.Team team = Data.ReadStruct<IeGoGalaxyHelper.Team>();
        Item coach = Inventory[team.CoachIndex];
        Item formation = Inventory[team.FormationIndex];
        Item kit = Inventory[team.KitIndex];
        Item emblem = Inventory[team.EmblemIndex];

        Data.Seek(teamName);
        string name = Data.ReadString(Encoding.UTF8);

        Data.Seek(teamPlayers);
        IeGoGalaxyHelper.TeamPlayer teamPlayer = Data.ReadStruct<IeGoGalaxyHelper.TeamPlayer>();
        List<Player?> players = teamPlayer.PlayersIndex.Where(x => x != 0x00).Select(x => Reserve.FirstOrDefault(y => y.Index == x)).ToList();
        List<int> playersFormationIndex = team.PlayersFormationIndex.Select(Convert.ToInt32).ToList();
        List<int> playersKitNumber = team.PlayerKitNumber.Select(Convert.ToInt32).ToList();

        return new Team(name, emblem, kit, formation, coach, players, playersFormationIndex, playersKitNumber);
    }

    private void SaveTeam(BinaryDataWriter writer, Team team, uint teamInfo, uint teamName, uint teamPlayers)
    {
        writer.Seek(teamInfo);
        writer.Write(team.Coach == null ? 0x00 : Inventory.FirstOrDefault(x => x.Value == team.Coach).Key);
        writer.Write(team.Formation == null ? 0x00 : Inventory.FirstOrDefault(x => x.Value == team.Formation).Key);
        writer.Write(team.Kit == null ? 0x00 : Inventory.FirstOrDefault(x => x.Value == team.Kit).Key);
        writer.Write(team.Emblem == null ? 0x00 : Inventory.FirstOrDefault(x => x.Value == team.Emblem).Key);
        writer.Write(team.PlayersFormationIndex.Select(x => (byte)x).ToArray());
        writer.Write(team.PlayersKitNumber.Select(x => (byte)x).ToArray());

        writer.Seek(teamName);
        writer.Write(Encoding.UTF8.GetBytes(team.Name));
        writer.Write((byte)0x0);
        writer.Write((byte)0x88);

        writer.Seek(teamPlayers);
        writer.Write(team.Players
            .Where(player =>
                {
                    if (player is not null) return true;
                    Console.Error.WriteLine("Player is null");
                    return false;
                }
            ).OfType<Player>()
            .SelectMany(x => BitConverter.GetBytes(x.Index)).ToArray());
    }

    /*public void NewMixiMax(Player player, int miximaxIndex, int miximaxMove1, int miximaxMove2)
    {
        BestMatch? isBestMatch = base.IsBestMatch(player.Id, Reserve.Find(x => x.Index == miximaxIndex).Id);

        player.MixiMax = isBestMatch != null ? 
            new MixiMax(Reserve.Find(x => x.Index == miximaxIndex), (miximaxMove1, miximaxMove2), isBestMatch) : 
            new MixiMax(Reserve.Find(x => x.Index == miximaxIndex), (miximaxMove1, miximaxMove2));

        Reserve.Find(x => x.Index == miximaxIndex).IsAura = true;
    }*/

    public override (int, int, string, bool) Training(Player player, int newStat, int statIndex)
    {
        if (player.InvestedFreedom is null || player.InvestedPoint is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        player.InvestedFreedom[statIndex] = newStat;

        if (player.InvestedFreedom.Sum() > player.Freedom)
            return (0, player.InvestedPoint[statIndex], "investedNumericUpDown" + (statIndex + 3), true);

        player.InvestedPoint[statIndex] = newStat;
        return (0, player.Freedom, "investedNumericUpDown" + (statIndex + 3), true);
    }
}