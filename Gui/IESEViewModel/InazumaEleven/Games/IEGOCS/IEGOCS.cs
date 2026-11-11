using System.Text;
using System.Collections;
using IESEViewModel.InazumaEleven.Common.GO.Formations;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Tools;

namespace IESEViewModel.InazumaEleven.Games.IEGOCS;

public class Cs(BinaryDataReader data, bool isJp) : Cs<Common.GO.Cs>(data, isJp);

public class Cs<TCommon> : GameMiximax<TCommon> where TCommon : ICommonAuraGame
{
    private readonly IeGoCsHelper _helper;

    protected Cs(BinaryDataReader data, bool isJp) : base(data, GameVersion.Cs, "Inazuma Eleven Go Chrono Stones", 336)
    {
        _helper = IeGoCsHelper.GetInstance(isJp);

        // Set in GetInventory
        Equipments = null!;
        Auras = null!;
        Inventory = GetInventory();

        // Configure Aura
        foreach (KeyValuePair<int, Player> aura in Auras)
        {
            if (aura.Value.UInt32Moves is null) continue;

            aura.Value.Index = aura.Key;
            aura.Value.Moves = aura.Value.UInt32Moves
                    .Select(moveId => new Move(Moves[moveId], 1, true, 0))
                    .ToList();
        }

        Reserve = GetPlayers();
    }

    private Dictionary<int, Item> GetInventory()
    {
        // Group Item 1
        Data.Seek((uint)_helper.ItemBlockGroup1Offset);
        Dictionary<int, Item> itemBlockGroup1 = Data.ReadMultipleStruct<IeGoCsHelper.ItemBlockGroup1>(512).Where(x => Items.ContainsKey(x.ID) && x.ID != 0x00).ToDictionary(x => x.Index, y => new Item(y.ID, Items[y.ID], y.Quantity));

        // Group Item 2
        Data.Seek((uint)_helper.ItemBlockGroup2Offset);
        Dictionary<int, Item> itemBlockGroup2 = Data.ReadMultipleStruct<IeGoCsHelper.ItemBlockGroup2>(336).Where(x => Items.ContainsKey(x.ID) && x.ID != 0x00).ToDictionary(x => x.Index, y => new Item(y.ID, Items[y.ID], y.Quantity, y.QuantityEquiped));

        // Group Item 3
        Data.Seek((uint)_helper.ItemBlockGroup3Offset);
        Dictionary<int, Item> itemBlockGroup3 = Data.ReadMultipleStruct<IeGoCsHelper.ItemBlockGroup3>(800).Where(x => Items.ContainsKey(x.ID) && x.ID != 0x00).ToDictionary(x => x.Index, y => new Item(y.ID, Items[y.ID]));

        // Find Equipment
        Equipments = itemBlockGroup2.Where(x => TCommon.Equipments.ContainsKey(x.Value.Id)).ToDictionary(x => x.Key, y => TCommon.Equipments[y.Value.Id]);
        Equipments.Add(0, TCommon.Equipments[0x0]);

        // Find Aura
        Auras = itemBlockGroup3.Where(x => TCommon.Auras.ContainsKey(x.Value.Id)).ToDictionary(x => x.Key, y => TCommon.Auras[y.Value.Id]);

        // Merge
        // ReSharper disable twice UsageOfDefaultStructEquality
        return itemBlockGroup1.Union(itemBlockGroup2).Union(itemBlockGroup3).ToDictionary(x => x.Key, x => x.Value);
    }

    public override void SaveInventory(BinaryDataWriter writer)
    {
        // Group Item 1
        writer.Seek((uint)_helper.ItemBlockGroup1Offset);
        Dictionary<int, Item> itemGroup1 = Inventory.Where(x => x.Value.Category == 1).ToDictionary(x => x.Key, y => y.Value);
        for (int i = 0; i < 512; i++)
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
            int count = Reserve.Count(x => x.Avatar is not null && Avatars.FirstOrDefault(y => y.Value.Name == x.Avatar.Name).Key == item.Value.Id);

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
        writer.Seek((uint)_helper.ItemBlockGroup2Offset);
        Dictionary<int, Item> itemGroup2 = Inventory.Where(x => x.Value.Category == 2).ToDictionary(x => x.Key, y => y.Value);
        for (int i = 0; i < 336; i++)
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
        writer.Seek((uint)_helper.ItemBlockGroup3Offset);
        Dictionary<int, Item> itemGroup3 = Inventory.Where(x => x.Value.Category == 3).ToDictionary(x => x.Key, y => y.Value);
        for (int i = 0; i < 800; i++)
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
        Data.Seek((uint)_helper.PlayerIndexOffset);
        int[] index = Data.ReadMultipleStruct<int>(IeGoCsHelper.MaximumPlayer)
            .Where(x => x != 0x0)
            .ToArray();

        // Get player data according to their index
        Data.Seek((uint)_helper.PlayerDataOffset);
        List<IeGoCsHelper.PlayerBlock> playerBlocks = Data.ReadMultipleStruct<IeGoCsHelper.PlayerBlock>(index.Length)
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
                    Avatar = new Avatar(Avatars[playerBlock.AvatarID], playerBlock.AvatarLevel, playerBlock.AvatarUsage),
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
                // The aura is an Item
                if (Auras.Any(x => x.Key == miximaxIndex))
                {
                    BestMatch? isBestMatch = IsBestMatch(player.Id, Auras[miximaxIndex].Id);

                    var newMixiMax = isBestMatch != null ?
                        new MixiMax(Auras[miximaxIndex], (miximaxMove1, miximaxMove2), isBestMatch) :
                        new MixiMax(Auras[miximaxIndex], (miximaxMove1, miximaxMove2));

                    newMixiMax.AuraData = true;
                    player.MixiMax = newMixiMax;
                    Auras[miximaxIndex].IsAura = true;
                }
                else
                {
                    player.MixiMax = null;
                }
            }
        }

        // Sort
        return players.OrderBy(player => Array.IndexOf(index, player.Index)).ToList();
    }

    public override void SavePlayers(BinaryDataWriter writer)
    {
        // Save player index
        writer.Seek((uint)_helper.PlayerIndexOffset);
        for (int i = 0; i < IeGoCsHelper.MaximumPlayer; i++)
        {
            writer.Write(i < Reserve.Count ? Reserve[i].Index : 0x0);
        }

        // Save player data
        writer.Seek((uint)_helper.PlayerDataOffset);
        List<Player> sortedReserve = Reserve.OrderBy(player => player.Index).ToList();
        for (int i = 0; i < IeGoCsHelper.MaximumPlayer; i++)
        {
            if (i < sortedReserve.Count)
            {
                Player player = sortedReserve[i];

                writer.Write(player.Index);
                writer.Write(player.Id);
                writer.Skip(0x04);
                writer.Write((short)player.Fp);
                writer.Write((short)player.Tp);
                writer.Write((short)player.Freedom);
                writer.Write((byte)player.Level);
                writer.Skip(0x01);

                if (player.MixiMax?.AuraPlayer != null)
                {
                    writer.Write(player.MixiMax.AuraPlayer.Index);
                    writer.Skip(0x08);
                    writer.Write((byte)player.MixiMax.MixiMaxMoveNumber[0]);
                    writer.Write((byte)player.MixiMax.MixiMaxMoveNumber[1]);
                }
                else
                {
                    writer.Write(0x00);
                    writer.Skip(0x08);
                    writer.Write((byte)0x00);
                    writer.Write((byte)0x00);
                }

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

                if (player.InvestedPoint is not null)
                {
                    writer.Write(player.InvestedPoint.SelectMany(x => BitConverter.GetBytes((short)x)).ToArray());
                }
                else
                {
                    Console.Error.WriteLine("Player investedPoint is null");
                }

                if (player.Avatar is not null)
                {
                    writer.Write(Avatars.FirstOrDefault(x => x.Value.Name == player.Avatar.Name).Key);

                    if (player.Avatar.Level is not null)
                    {
                        writer.Write((byte)player.Avatar.Level);
                    }
                    else
                    {
                        Console.Error.WriteLine("Player avatar level is null");
                    }

                    if (player.Avatar.Usage is not null)
                    {
                        writer.Write((byte)player.Avatar.Usage);
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
                int[] emptyPlayerData = Enumerable.Repeat(0, 65).ToArray();
                writer.Write(emptyPlayerData.SelectMany(BitConverter.GetBytes).ToArray());
            }
        }
    }

    public override Dictionary<int, List<PlayRecord>> GetPlayRecords()
    {
        Dictionary<int, List<PlayRecord>> playRecords = TCommon.PlayRecords;

        Data.Seek((uint)_helper.PlayRecordsOffset);

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

        writer.Seek((uint)_helper.PlayRecordsOffset);

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
        Data.Seek((uint)IeGoCsHelper.NameOffset);
        string name = Data.ReadString(Encoding.UTF8);
        Data.Seek((uint)_helper.TeamNameOffset);
        string teamName = Data.ReadString(Encoding.UTF8);

        // Time Hours
        Data.Seek((uint)IeGoCsHelper.TimeOffset);
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
        Data.Seek((uint)_helper.LinkOffset);
        byte secretLinkLevel = Data.ReadValue<byte>();

        // Current Chapter
        Data.Seek((uint)_helper.ChapterOffset);
        byte chapter = Data.ReadValue<byte>();

        // Money
        Data.Seek((uint)_helper.MoneyOffset);
        int prestige = Data.ReadValue<int>();
        int friendship = Data.ReadValue<int>();

        return new SaveInfo(name, teamName, hours, minutes, prestige, friendship, secretLinkLevel, chapter);
    }

    public override void SaveSaveInfo(BinaryDataWriter writer)
    {
        if (!SaveInfoInitialized) return;

        // Save Name
        writer.Seek((uint)IeGoCsHelper.NameOffset);
        writer.Write(Encoding.UTF8.GetBytes(SaveInfo.Name));
        writer.Write((byte)0x0);
        writer.Write((byte)0x88);

        // Team Name
        writer.Seek((uint)_helper.TeamNameOffset);
        writer.Write(Encoding.UTF8.GetBytes(SaveInfo.TeamName));
        writer.Write((byte)0x0);
        writer.Write((byte)0x88);

        // Time Hours
        writer.Seek((uint)IeGoCsHelper.TimeOffset);
        int time = SaveInfo.Hours * 3600 + SaveInfo.Minutes * 60;
        writer.Write(time);

        // Link Level
        writer.Seek((uint)_helper.LinkOffset);
        if (SaveInfo.SecretLinkLevel is null) throw new ArgumentException(nameof(SaveInfo.SecretLinkLevel));
        writer.Write((byte)SaveInfo.SecretLinkLevel);

        // Money
        writer.Seek((uint)_helper.MoneyOffset);
        writer.Write(SaveInfo.Prestige);
        writer.Write(SaveInfo.Friendship);

        if (!SaveInfo.UnlockAllData) return;

        if (_helper.IsJp)
        {
            writer.Seek(0x2AB); //Special Download
            writer.Write([0xFF, 0xFF, 0x01, 0x00, 0xFE, 0x3F]);

            writer.Seek(0x2B5); //go link
            writer.Write([0xE0, 0xF8, 0xFF, 0xFF, 0x0F, 0xFF]);

            writer.Seek(0x2D2); //phone new notification jp:0x2D2
            writer.Write((byte)0x18);

            writer.Seek(0x2DF); //wifi
            writer.Write([0xF8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0x50, 0x02]);

            writer.Seek(0x2E9);
            writer.Write((byte)0x78);

            // Ugly Write
            writer.Seek(0x242B4);
            writer.Write([0xFF, 0xFE, 0x73, 0x08, 0xFE, 0xFF, 0x73, 0x08, 0x0D, 0x04, 0x04, 0x00, 0x5E, 0x00, 0x00, 0x00, 0xE0, 0xFF, 0xFF, 0xFF, 0xB2, 0xF0, 0x63, 0xAB, 0xE0, 0xFF, 0xFF, 0xFF, 0xCA, 0x40, 0x78, 0xBB, 0xE2, 0xFF, 0xFF, 0xFF, 0xC4, 0xB4, 0x8A, 0x81, 0xE2, 0xFF, 0xFF, 0xFF, 0x7E, 0xE5, 0x83, 0x18, 0xE4, 0xFF, 0xFF, 0xFF, 0x0A, 0x1F, 0x24, 0x90, 0xE4, 0xFF, 0xFF, 0xFF, 0x91, 0x07, 0xD4, 0x64, 0xE4, 0xFF, 0xFF, 0xFF, 0x22, 0xBB, 0x46, 0x1A, 0xE4, 0xFF, 0xFF, 0xFF, 0x79, 0xA6, 0xE4, 0x87, 0xE4, 0xFF, 0xFF, 0xFF, 0x1F, 0x55, 0x33, 0x42, 0xE4, 0xFF, 0xFF, 0xFF, 0x33, 0x34, 0x3D, 0xAC, 0xE4, 0xFF, 0xFF, 0xFF, 0x5F, 0x7C, 0x0E, 0xCC, 0xE0, 0xFF, 0xFF, 0xFF, 0x58, 0x7A, 0xA8, 0xD7, 0xE0, 0xFF, 0xFF, 0xFF, 0xBD, 0x09, 0x50, 0x6F, 0xE4, 0xFF, 0xFF, 0xFF, 0xCE, 0x9E, 0x22, 0xAA, 0xE0, 0xFF, 0xFF, 0xFF, 0xAF, 0x3D, 0xE9, 0xD1, 0xE4, 0xFF, 0xFF, 0xFF, 0xA3, 0xCE, 0x37, 0x5D, 0xE0, 0xFF, 0xFF, 0xFF, 0x74, 0x1B, 0xA6, 0x39, 0xE0, 0xFF, 0xFF, 0xFF, 0xE2, 0x2B, 0xA1, 0x4E, 0xE1, 0xFF, 0xFF, 0xFF, 0x63, 0x37, 0x39, 0x50, 0xE1, 0xFF, 0xFF, 0xFF, 0x7D, 0xB2, 0xA9, 0xC7, 0xE1, 0xFF, 0xFF, 0xFF, 0x83, 0xBF, 0xC5, 0x53, 0xE1, 0xFF, 0xFF, 0xFF, 0xD7, 0x18, 0xB8, 0x08, 0xE1, 0xFF, 0xFF, 0xFF, 0x86, 0xD2, 0xBB, 0x47, 0xE1, 0xFF, 0xFF, 0xFF, 0x53, 0xCA, 0xA0, 0x0A, 0xE1, 0xFF, 0xFF, 0xFF, 0x0A, 0x74, 0xE6, 0x08, 0xE1, 0xFF, 0xFF, 0xFF, 0xF6, 0x39, 0x4C, 0x6F, 0xE1, 0xFF, 0xFF, 0xFF, 0x0C, 0xA8, 0x8D, 0x4F, 0xE1, 0xFF, 0xFF, 0xFF, 0x40, 0x80, 0xAD, 0x0F, 0xE1, 0xFF, 0xFF, 0xFF, 0x0C, 0x4B, 0xAF, 0x23, 0xE1, 0xFF, 0xFF, 0xFF, 0x79, 0x94, 0xE7, 0xCF, 0xE1, 0xFF, 0xFF, 0xFF, 0xE5, 0x90, 0x63, 0x06, 0xE1, 0xFF, 0xFF, 0xFF, 0x6E, 0xCD, 0x20, 0x65, 0xE1, 0xFF, 0xFF, 0xFF, 0x1A, 0xE3, 0xBB, 0x29, 0xE1, 0xFF, 0xFF, 0xFF, 0x60, 0x50, 0x8A, 0xC8, 0xE1, 0xFF, 0xFF, 0xFF, 0xE9, 0xAE, 0x2D, 0x34, 0xE1, 0xFF, 0xFF, 0xFF, 0xA0, 0x15, 0xEF, 0xE5, 0xE1, 0xFF, 0xFF, 0xFF, 0xD0, 0x35, 0x00, 0xEF, 0xE1, 0xFF, 0xFF, 0xFF, 0x15, 0x6C, 0xE0, 0x48, 0xE1, 0xFF, 0xFF, 0xFF, 0x35, 0x6A, 0x61, 0x44, 0xE1, 0xFF, 0xFF, 0xFF, 0x77, 0x65, 0x08, 0x15, 0xE1, 0xFF, 0xFF, 0xFF, 0xE5, 0x46, 0xC5, 0xCD, 0xE1, 0xFF, 0xFF, 0xFF, 0x38, 0x46, 0xDE, 0xBA, 0xE1, 0xFF, 0xFF, 0xFF, 0x6D, 0x25, 0xF4, 0xE6, 0xE1, 0xFF, 0xFF, 0xFF, 0x1D, 0x97, 0xC2, 0x1E, 0xE1, 0xFF, 0xFF, 0xFF, 0xE4, 0x49, 0x8A, 0xE8, 0xE1, 0xFF, 0xFF, 0xFF, 0x6E, 0x42, 0x47, 0x7E, 0xE1, 0xFF, 0xFF, 0xFF, 0x44, 0x45, 0xC1, 0x6B, 0xE1, 0xFF, 0xFF, 0xFF, 0xE8, 0x6A, 0x7A, 0x33, 0xE1, 0xFF, 0xFF, 0xFF, 0x6D, 0x54, 0x0F, 0x78, 0xE1, 0xFF, 0xFF, 0xFF, 0xE4, 0x38, 0x71, 0x76, 0xE1, 0xFF, 0xFF, 0xFF, 0x35, 0xD0, 0x82, 0xF8, 0xE1, 0xFF, 0xFF, 0xFF, 0x63, 0xB8, 0x5E, 0x4B, 0xE1, 0xFF, 0xFF, 0xFF, 0x77, 0xEA, 0x6F, 0x0E, 0xE1, 0xFF, 0xFF, 0xFF, 0x0E, 0x84, 0x0E, 0xCB, 0xE1, 0xFF, 0xFF, 0xFF, 0x22, 0xE5, 0x00, 0x25, 0xE1, 0xFF, 0xFF, 0xFF, 0x41, 0x44, 0xFA, 0x08, 0xE1, 0xFF, 0xFF, 0xFF, 0x6E, 0x77, 0xC3, 0xD9, 0xE1, 0xFF, 0xFF, 0xFF, 0xA4, 0x42, 0x5A, 0x73, 0xE1, 0xFF, 0xFF, 0xFF, 0xFB, 0x64, 0x08, 0x0F, 0xE1, 0xFF, 0xFF, 0xFF, 0x92, 0x63, 0xA8, 0x6E, 0xE1, 0xFF, 0xFF, 0xFF, 0x07, 0xD7, 0x3E, 0xED, 0xE1, 0xFF, 0xFF, 0xFF, 0xED, 0x07, 0x04, 0x27, 0xE1, 0xFF, 0xFF, 0xFF, 0x7B, 0x37, 0x03, 0x50, 0xE1, 0xFF, 0xFF, 0xFF, 0xE1, 0x83, 0xA9, 0xA9, 0xE1, 0xFF, 0xFF, 0xFF, 0xFB, 0xF6, 0xD1, 0xFD, 0xE1, 0xFF, 0xFF, 0xFF, 0x3A, 0x06, 0x18, 0x49, 0xE1, 0xFF, 0xFF, 0xFF, 0xD4, 0x13, 0x4E, 0xE7, 0xE1, 0xFF, 0xFF, 0xFF, 0x48, 0x98, 0xAD, 0x35, 0xE1, 0xFF, 0xFF, 0xFF, 0xFD, 0xFC, 0x1C, 0x71, 0xE1, 0xFF, 0xFF, 0xFF, 0x84, 0xFE, 0x38, 0xC3, 0xE6, 0xFF, 0xFF, 0xFF, 0xFB, 0xEF, 0xCC, 0x49, 0xEA, 0xFF, 0xFF, 0xFF, 0x90, 0xA1, 0x59, 0x32, 0xE6, 0xFF, 0xFF, 0xFF, 0xCE, 0x4A, 0xAF, 0xA0, 0xE6, 0xFF, 0xFF, 0xFF, 0x82, 0xED, 0xE8, 0xFB, 0xE6, 0xFF, 0xFF, 0xFF, 0xBA, 0xF2, 0x4E, 0x43, 0xE6, 0xFF, 0xFF, 0xFF, 0xC9, 0x67, 0x17, 0x47, 0xE6, 0xFF, 0xFF, 0xFF, 0x5F, 0x57, 0x10, 0x30, 0xE6, 0xFF, 0xFF, 0xFF, 0xCE, 0x66, 0x36, 0xB3, 0xE6, 0xFF, 0xFF, 0xFF, 0x58, 0x56, 0x31, 0xC4, 0xE6, 0xFF, 0xFF, 0xFF, 0x14, 0xDD, 0xEF, 0x8C, 0xE6, 0xFF, 0xFF, 0xFF, 0x74, 0x37, 0x3F, 0x2A, 0xE6, 0xFF, 0xFF, 0xFF, 0xB7, 0x48, 0x8B, 0x12, 0xE6, 0xFF, 0xFF, 0xFF, 0xC9, 0x4B, 0x8E, 0x54, 0xE6, 0xFF, 0xFF, 0xFF, 0xD7, 0x8E, 0xC2, 0xA7, 0xE6, 0xFF, 0xFF, 0xFF, 0xAE, 0x8C, 0xE6, 0x15, 0xEC, 0xFF, 0xFF, 0xFF, 0x3F, 0xF2, 0xF4, 0x4F, 0xE6, 0xFF, 0xFF, 0xFF, 0x88, 0x52, 0xAF, 0x03, 0xE6, 0xFF, 0xFF, 0xFF, 0x75, 0xAA, 0xA9, 0xFD, 0xEA, 0xFF, 0xFF, 0xFF, 0xB4, 0x35, 0xEA, 0xAC, 0xEA, 0xFF, 0xFF, 0xFF, 0x0C, 0x14, 0xE6, 0x6F, 0xE6, 0xFF, 0xFF, 0xFF, 0xF3, 0x0D, 0xF3, 0xAB, 0xE6, 0xFF, 0xFF, 0xFF, 0x65, 0x3D, 0xF4, 0xDC, 0xE6, 0xFF, 0xFF, 0xFF, 0x60, 0x7F, 0x23, 0xBD, 0xE8, 0xFF, 0xFF, 0xFF, 0x52, 0x84, 0x8D, 0xF6
            ]);
        }
        else
        {
            writer.Seek(0x2FB);
            writer.Write([0xFF, 0xFF, 0x01, 0x00, 0xFE, 0x3F]);

            writer.Seek(0x305);
            writer.Write([0xE0, 0xF8, 0xFF, 0xFF, 0x0F, 0xFF]);

            writer.Seek(0x322);
            writer.Write((byte)0x18);

            writer.Seek(0x32F);
            writer.Write([0xF8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0x50, 0x02]);

            writer.Seek(0x339);
            writer.Write((byte)0x78);

            // Ugly Write
            writer.Seek(0x24A40);
            writer.Write([0xFE, 0xFF, 0x76, 0x08, 0x0D, 0x04, 0x04, 0x00, 0x6B, 0x00, 0x00, 0x00, 0xE4, 0xFF, 0xFF, 0xFF, 0x91, 0x07, 0xD4, 0x64, 0xE4, 0xFF, 0xFF, 0xFF, 0x1F, 0x55, 0x33, 0x42, 0xE0, 0xFF, 0xFF, 0xFF, 0xB2, 0xF0, 0x63, 0xAB, 0xE0, 0xFF, 0xFF, 0xFF, 0xCA, 0x40, 0x78, 0xBB, 0xE0, 0xFF, 0xFF, 0xFF, 0x60, 0x7F, 0x23, 0xBD, 0xE1, 0xFF, 0xFF, 0xFF, 0x7D, 0xB2, 0xA9, 0xC7, 0xE1, 0xFF, 0xFF, 0xFF, 0x83, 0xBF, 0xC5, 0x53, 0xE1, 0xFF, 0xFF, 0xFF, 0x1D, 0x97, 0xC2, 0x1E, 0xE1, 0xFF, 0xFF, 0xFF, 0x1A, 0xE3, 0xBB, 0x29, 0xE1, 0xFF, 0xFF, 0xFF, 0x53, 0xCA, 0xA0, 0x0A, 0xE1, 0xFF, 0xFF, 0xFF, 0x0E, 0x84, 0x0E, 0xCB, 0xE2, 0xFF, 0xFF, 0xFF, 0xC4, 0xB4, 0x8A, 0x81, 0xE0, 0xFF, 0xFF, 0xFF, 0x75, 0xAA, 0xA9, 0xFD, 0xE0, 0xFF, 0xFF, 0xFF, 0x74, 0x1B, 0xA6, 0x39, 0xE0, 0xFF, 0xFF, 0xFF, 0xE2, 0x2B, 0xA1, 0x4E, 0xE1, 0xFF, 0xFF, 0xFF, 0xD7, 0x18, 0xB8, 0x08, 0xE1, 0xFF, 0xFF, 0xFF, 0xF6, 0x39, 0x4C, 0x6F, 0xE1, 0xFF, 0xFF, 0xFF, 0x6D, 0x54, 0x0F, 0x78, 0xE1, 0xFF, 0xFF, 0xFF, 0x3A, 0x06, 0x18, 0x49, 0xE1, 0xFF, 0xFF, 0xFF, 0xE5, 0x90, 0x63, 0x06, 0xE1, 0xFF, 0xFF, 0xFF, 0x35, 0xD0, 0x82, 0xF8, 0xE4, 0xFF, 0xFF, 0xFF, 0x0A, 0x1F, 0x24, 0x90, 0xE0, 0xFF, 0xFF, 0xFF, 0x58, 0x7A, 0xA8, 0xD7, 0xE0, 0xFF, 0xFF, 0xFF, 0xBD, 0x09, 0x50, 0x6F, 0xE0, 0xFF, 0xFF, 0xFF, 0xAF, 0x3D, 0xE9, 0xD1, 0xE2, 0xFF, 0xFF, 0xFF, 0x7E, 0xE5, 0x83, 0x18, 0xE0, 0xFF, 0xFF, 0xFF, 0x3D, 0xC8, 0x82, 0xC2, 0xE0, 0xFF, 0xFF, 0xFF, 0xE4, 0xB7, 0x16, 0x6D, 0xE1, 0xFF, 0xFF, 0xFF, 0x0A, 0x74, 0xE6, 0x08, 0xE1, 0xFF, 0xFF, 0xFF, 0x0C, 0xA8, 0x8D, 0x4F, 0xE1, 0xFF, 0xFF, 0xFF, 0x63, 0x37, 0x39, 0x50, 0xE1, 0xFF, 0xFF, 0xFF, 0x79, 0x94, 0xE7, 0xCF, 0xE1, 0xFF, 0xFF, 0xFF, 0x40, 0x80, 0xAD, 0x0F, 0xE1, 0xFF, 0xFF, 0xFF, 0x60, 0x50, 0x8A, 0xC8, 0xE0, 0xFF, 0xFF, 0xFF, 0x91, 0x04, 0x1B, 0xF6, 0xE0, 0xFF, 0xFF, 0xFF, 0x7A, 0x9F, 0x11, 0x20, 0xE0, 0xFF, 0xFF, 0xFF, 0x37, 0xE1, 0xBF, 0x95, 0xE0, 0xFF, 0xFF, 0xFF, 0xCE, 0x4A, 0xAF, 0xA0, 0xE1, 0xFF, 0xFF, 0xFF, 0xE9, 0xAE, 0x2D, 0x34, 0xE1, 0xFF, 0xFF, 0xFF, 0x44, 0x45, 0xC1, 0x6B, 0xE1, 0xFF, 0xFF, 0xFF, 0x86, 0xD2, 0xBB, 0x47, 0xE1, 0xFF, 0xFF, 0xFF, 0xD0, 0x35, 0x00, 0xEF, 0xE1, 0xFF, 0xFF, 0xFF, 0x15, 0x6C, 0xE0, 0x48, 0xE1, 0xFF, 0xFF, 0xFF, 0x77, 0x65, 0x08, 0x15, 0xE4, 0xFF, 0xFF, 0xFF, 0x22, 0xBB, 0x46, 0x1A, 0xE0, 0xFF, 0xFF, 0xFF, 0xD7, 0x74, 0xFD, 0x7F, 0xE0, 0xFF, 0xFF, 0xFF, 0xBA, 0xF2, 0x4E, 0x43, 0xE0, 0xFF, 0xFF, 0xFF, 0xD7, 0x8E, 0xC2, 0xA7, 0xE4, 0xFF, 0xFF, 0xFF, 0x79, 0xA6, 0xE4, 0x87, 0xE4, 0xFF, 0xFF, 0xFF, 0x33, 0x34, 0x3D, 0xAC, 0xE4, 0xFF, 0xFF, 0xFF, 0xCE, 0x9E, 0x22, 0xAA, 0xE1, 0xFF, 0xFF, 0xFF, 0xE5, 0x46, 0xC5, 0xCD, 0xE1, 0xFF, 0xFF, 0xFF, 0x38, 0x46, 0xDE, 0xBA, 0xE1, 0xFF, 0xFF, 0xFF, 0x84, 0xFE, 0x38, 0xC3, 0xE1, 0xFF, 0xFF, 0xFF, 0x6E, 0x77, 0xC3, 0xD9, 0xE1, 0xFF, 0xFF, 0xFF, 0xE4, 0x49, 0x8A, 0xE8, 0xE1, 0xFF, 0xFF, 0xFF, 0x6E, 0x42, 0x47, 0x7E, 0xE0, 0xFF, 0xFF, 0xFF, 0xAE, 0x8C, 0xE6, 0x15, 0xE0, 0xFF, 0xFF, 0xFF, 0x5F, 0x57, 0x10, 0x30, 0xE0, 0xFF, 0xFF, 0xFF, 0xC9, 0x67, 0x17, 0x47, 0xE1, 0xFF, 0xFF, 0xFF, 0xE8, 0x6A, 0x7A, 0x33, 0xE1, 0xFF, 0xFF, 0xFF, 0xE4, 0x38, 0x71, 0x76, 0xE1, 0xFF, 0xFF, 0xFF, 0xA0, 0x15, 0xEF, 0xE5, 0xE1, 0xFF, 0xFF, 0xFF, 0x63, 0xB8, 0x5E, 0x4B, 0xE1, 0xFF, 0xFF, 0xFF, 0x77, 0xEA, 0x6F, 0x0E, 0xE1, 0xFF, 0xFF, 0xFF, 0x70, 0x24, 0xF5, 0x85, 0xE4, 0xFF, 0xFF, 0xFF, 0x0C, 0x14, 0xE6, 0x6F, 0xE4, 0xFF, 0xFF, 0xFF, 0xB4, 0x35, 0xEA, 0xAC, 0xE4, 0xFF, 0xFF, 0xFF, 0x90, 0xA1, 0x59, 0x32, 0xE1, 0xFF, 0xFF, 0xFF, 0x22, 0xE5, 0x00, 0x25, 0xE1, 0xFF, 0xFF, 0xFF, 0x41, 0x44, 0xFA, 0x08, 0xE1, 0xFF, 0xFF, 0xFF, 0x6E, 0xCD, 0x20, 0x65, 0xE1, 0xFF, 0xFF, 0xFF, 0xA4, 0x42, 0x5A, 0x73, 0xE1, 0xFF, 0xFF, 0xFF, 0xFB, 0x64, 0x08, 0x0F, 0xE1, 0xFF, 0xFF, 0xFF, 0x92, 0x63, 0xA8, 0x6E, 0xE0, 0xFF, 0xFF, 0xFF, 0x74, 0x37, 0x3F, 0x2A, 0xE0, 0xFF, 0xFF, 0xFF, 0xCE, 0x66, 0x36, 0xB3, 0xE0, 0xFF, 0xFF, 0xFF, 0x58, 0x56, 0x31, 0xC4, 0xE0, 0xFF, 0xFF, 0xFF, 0xC9, 0x4B, 0x8E, 0x54, 0xE0, 0xFF, 0xFF, 0xFF, 0x14, 0xDD, 0xEF, 0x8C, 0xE0, 0xFF, 0xFF, 0xFF, 0xB7, 0x48, 0x8B, 0x12, 0xE1, 0xFF, 0xFF, 0xFF, 0x07, 0xD7, 0x3E, 0xED, 0xE1, 0xFF, 0xFF, 0xFF, 0x7B, 0x37, 0x03, 0x50, 0xE1, 0xFF, 0xFF, 0xFF, 0x35, 0x6A, 0x61, 0x44, 0xE1, 0xFF, 0xFF, 0xFF, 0xE1, 0x83, 0xA9, 0xA9, 0xE1, 0xFF, 0xFF, 0xFF, 0xFB, 0xF6, 0xD1, 0xFD, 0xE1, 0xFF, 0xFF, 0xFF, 0xD4, 0x13, 0x4E, 0xE7, 0xE4, 0xFF, 0xFF, 0xFF, 0x26, 0x7E, 0x2A, 0x7E, 0xE4, 0xFF, 0xFF, 0xFF, 0x73, 0x1D, 0x00, 0x22, 0xE4, 0xFF, 0xFF, 0xFF, 0xF1, 0x7F, 0x36, 0x10, 0xE4, 0xFF, 0xFF, 0xFF, 0x88, 0x7D, 0x12, 0xA2, 0xE1, 0xFF, 0xFF, 0xFF, 0x48, 0x98, 0xAD, 0x35, 0xE1, 0xFF, 0xFF, 0xFF, 0xFD, 0xFC, 0x1C, 0x71, 0xE1, 0xFF, 0xFF, 0xFF, 0xED, 0x07, 0x04, 0x27, 0xE1, 0xFF, 0xFF, 0xFF, 0x0C, 0x4B, 0xAF, 0x23, 0xE1, 0xFF, 0xFF, 0xFF, 0x6D, 0x25, 0xF4, 0xE6, 0xE1, 0xFF, 0xFF, 0xFF, 0x6A, 0x64, 0x09, 0x76, 0xE4, 0xFF, 0xFF, 0xFF, 0x5F, 0x7C, 0x0E, 0xCC, 0xE4, 0xFF, 0xFF, 0xFF, 0xA3, 0xCE, 0x37, 0x5D, 0xE0, 0xFF, 0xFF, 0xFF, 0x88, 0x52, 0xAF, 0x03, 0xE0, 0xFF, 0xFF, 0xFF, 0xF3, 0x0D, 0xF3, 0xAB, 0xE0, 0xFF, 0xFF, 0xFF, 0x65, 0x3D, 0xF4, 0xDC, 0xE8, 0xFF, 0xFF, 0xFF, 0x52, 0x84, 0x8D, 0xF6, 0xEC, 0xFF, 0xFF, 0xFF, 0x3F, 0xF2, 0xF4, 0x4F, 0xE6, 0xFF, 0xFF, 0xFF, 0xFB, 0xEF, 0xCC, 0x49, 0xE6, 0xFF, 0xFF, 0xFF, 0x41, 0xBE, 0xC5, 0xD0, 0xE6, 0xFF, 0xFF, 0xFF
            ]);
        }
    }

    public override List<Team> GetTactics()
    {
        List<Team> teams =
        [
            LoadTeam(_helper.MainTeamInfoOffset, _helper.MainTeamNameOffset, _helper.MainTeamPlayersOffset)
            // Custom Teams
        ];

        // Chrono Storm Team

        // Custom Teams
        uint teamInfo = _helper.CustomTeamInfoOffset;
        uint teamName = _helper.CustomTeamNameOffset;
        uint teamPlayers = _helper.CustomTeamPlayersOffset;
        for (int i = 0; i < 10; i++)
        {
            teams.Add(LoadTeam(teamInfo, teamName, teamPlayers));
            teamInfo += 0x30;
            teamName += !_helper.IsJp ? (uint)0x28 : 0x20;
            teamPlayers += 0x40;
        }

        return teams;
    }

    public override void SaveTactics(BinaryDataWriter writer)
    {
        if (!TeamsInitialized) return;

        // Chrono Storm Team
        SaveTeam(writer, Teams[0], _helper.MainTeamInfoOffset, _helper.MainTeamNameOffset, _helper.MainTeamPlayersOffset);

        // Custom Teams
        uint teamInfo = _helper.CustomTeamInfoOffset;
        uint teamName = _helper.CustomTeamNameOffset;
        uint teamPlayers = _helper.CustomTeamPlayersOffset;
        for (int i = 0; i < 10; i++)
        {
            SaveTeam(writer, Teams[i + 1], teamInfo, teamName, teamPlayers);
            teamInfo += 0x30;
            teamName += !_helper.IsJp ? (uint)0x28 : 0x20;
            teamPlayers += 0x40;
        }
    }

    private Team LoadTeam(uint teamInfo, uint teamName, uint teamPlayers)
    {
        Data.Seek(teamInfo);

        IeGoCsHelper.Team team = Data.ReadStruct<IeGoCsHelper.Team>();
        Item coach = Inventory[team.CoachIndex];
        Item formation = Inventory[team.FormationIndex];
        Item kit = Inventory[team.KitIndex];
        Item emblem = Inventory[team.EmblemIndex];

        Data.Seek(teamName);
        string name = Data.ReadString(Encoding.UTF8);

        Data.Seek(teamPlayers);
        IeGoCsHelper.TeamPlayer teamPlayer = Data.ReadStruct<IeGoCsHelper.TeamPlayer>();
        List<Player?> players = teamPlayer.PlayersIndex.Where(x => x != 0x00).Select(x => Reserve.FirstOrDefault(y => y.Index == x)).ToList();
        List<int> playersFormationIndex = team.PlayersFormationIndex.Select(Convert.ToInt32).ToList();
        List<int> playersKitNumber = team.PlayerKitNumber.Select(Convert.ToInt32).ToList();

        return new Team(name, emblem, kit, formation, coach, players, playersFormationIndex, playersKitNumber);
    }

    private void SaveTeam(BinaryDataWriter writer, Team team, uint teamInfo, uint teamName, uint teamPlayers)
    {
        writer.Seek(teamInfo);
        writer.Write(Inventory.FirstOrDefault(x => x.Value == team.Coach).Key);
        writer.Write(Inventory.FirstOrDefault(x => x.Value == team.Formation).Key);
        writer.Write(Inventory.FirstOrDefault(x => x.Value == team.Kit).Key);
        writer.Write(Inventory.FirstOrDefault(x => x.Value == team.Emblem).Key);
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
        Player? auraPlayer = Reserve.Find(x => x.Index == miximaxIndex);
        if (auraPlayer is not null)
        {
            // The aura is a Player
            BestMatch? isBestMatch = IsBestMatch(player.Id, auraPlayer.Id);

            player.MixiMax = isBestMatch != null ? 
                new MixiMax(auraPlayer, (miximaxMove1, miximaxMove2), isBestMatch) : 
                new MixiMax(auraPlayer, (miximaxMove1, miximaxMove2));

            Reserve.Find(x => x.Index == miximaxIndex).IsAura = true;
        }
        else
        {
            // The aura is an Item
            BestMatch? isBestMatch = IsBestMatch(player.Id, Auras[miximaxIndex].Id);

            var newMixiMax = isBestMatch != null ? 
                new MixiMax(Auras[miximaxIndex], (miximaxMove1, miximaxMove2), isBestMatch) : 
                new MixiMax(Auras[miximaxIndex], (miximaxMove1, miximaxMove2));

            newMixiMax.AuraData = true;
            player.MixiMax = newMixiMax;
            Auras[miximaxIndex].IsAura = true;
        }
    }*/

    public override void UpdateInventory()
    {
        // Find Equipment
        base.UpdateInventory();

        // Find Aura
        Auras = Inventory.Where(x => TCommon.Auras.ContainsKey(x.Value.Id)).ToDictionary(x => x.Key, y => TCommon.Auras[y.Value.Id]);

        // Configure Aura
        foreach (KeyValuePair<int, Player> aura in Auras)
        {
            aura.Value.Index = aura.Key;
            aura.Value.Moves = [];

            if (aura.Value.UInt32Moves is null)
            {
                Console.Error.WriteLine("Player Uintmoves is null");
                continue;
            }

            foreach (uint moveId in aura.Value.UInt32Moves)
            {
                aura.Value.Moves.Add(new Move(Moves[moveId], 1, true, 0));
            }
        }
    }
}