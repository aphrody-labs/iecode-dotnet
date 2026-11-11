using IESEViewModel.InazumaEleven.Common.GO;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Tools;

namespace IESEViewModel.InazumaEleven.Games;

public abstract class Game<TCommon>(
    BinaryDataReader data,
    GameVersion version,
    string name,
    int maximumPlayer) : IGame
    where TCommon : ICommonGame
{
    private List<Team>? _teams;
    private SaveInfo? _saveInfo;

    public GameVersion Version { get; } = version;

    public string Name { get; } = name;

    public int MaximumPlayer { get; } = maximumPlayer;

    public SaveInfo SaveInfo
    {
        get => _saveInfo ??= GetSaveInfo();
        set => _saveInfo = value;
    }

    protected bool SaveInfoInitialized => _saveInfo != null;

    public List<Team> Teams
    {
        get => _teams ??= GetTactics();
        set => _teams = value;
    }

    protected bool TeamsInitialized => _teams != null;

    public Dictionary<int, Equipment> Equipments { get; set; } = null!; // must be initialized in inheritors

    public Dictionary<int, Item> Inventory { get; set; } = null!; // must be initialized in inheritors

    protected BinaryDataReader Data { get; init; } = data;

    public Dictionary<uint, Player> Players { get; } = TCommon.Players;

    public Dictionary<uint, Move> Moves { get; } = TCommon.Moves;

    public Dictionary<uint, Avatar> Avatars { get; } = TCommon.Avatars;

    public Dictionary<uint, Item> Items { get; } = TCommon.Items;

    public List<Player> Reserve { get; set; } = null!; // must be initialized in inheritors

    public void RecruitPlayer(Player player, bool configure)
    {
        player = new Player(player);

        if (configure)
        {
            player.Style = 0;
            player.Level = 1;
            player.InvestedPoint = new List<int>(new int[8]);
            player.Avatar = Avatars[0x0];
            player.Invoke = false;
            player.Armed = false;

            player.Moves = [];
            for (int i = 0; i < 4; i++)
            {
                if (player.UInt32Moves is not null)
                {
                    player.Moves.Add(Moves[player.UInt32Moves[i]]);
                }
                else
                {
                    Console.Error.WriteLine("Player moves is null");
                }
            }

            for (int i = 0; i < 2; i++)
            {
                Move newMove = Moves[0x00];
                newMove.Level = 1;
                newMove.UsedCount = 0;
                player.Moves.Add(newMove);
            }

            player.Equipments = [];
            for (int i = 0; i < 4; i++)
            {
                player.Equipments.Add(Equipments[0x0]);
            }
        }

        int newIndex = Reserve.Last().Index;

        while (Reserve.Any(x => x.Index == newIndex))
        {
            short lowInt16 = (short)(newIndex & 0xFFFF);
            short highInt16 = (short)(newIndex >> 16 & 0xFFFF);
            lowInt16++;
            highInt16++;

            newIndex = (ushort)lowInt16 | highInt16 << 16;
        }

        player.Index = newIndex;
        Reserve.Add(player);
    }

    public Player ChangePlayer(Player selectedPlayer, Player newPlayer, bool keepOldPlayerInformation)
    {
        newPlayer = new Player(newPlayer);

        if (!keepOldPlayerInformation) return newPlayer;

        newPlayer.Index = selectedPlayer.Index;
        newPlayer.Level = selectedPlayer.Level;
        newPlayer.Invoke = selectedPlayer.Invoke;
        newPlayer.Armed = selectedPlayer.Armed;
        newPlayer.Style = selectedPlayer.Style;
        newPlayer.Avatar = selectedPlayer.Avatar;
        newPlayer.Equipments = selectedPlayer.Equipments;
        newPlayer.MixiMax = selectedPlayer.MixiMax;
        newPlayer.InvestedPoint = selectedPlayer.InvestedPoint;
        newPlayer.InvestedFreedom = selectedPlayer.InvestedFreedom;
        newPlayer.IsAura = selectedPlayer.IsAura;
        newPlayer.IsTrained = selectedPlayer.IsTrained;
        newPlayer.Score = selectedPlayer.Score;
        newPlayer.Participation = selectedPlayer.Participation;
        newPlayer.Moves = [];

        if (newPlayer.UInt32Moves is not null)
        {
            foreach (Move newMove in newPlayer.UInt32Moves.Select(moveId => Moves[moveId]))
            {
                newMove.Level = 1;

                if (newMove.EvolutionSpeed is not null)
                {
                    newMove.UsedCount = newMove.EvolutionSpeed.TimeLevel[0];
                }
                else
                {
                    Console.Error.WriteLine("NewMove evolution speed is null");
                }

                newMove.Unlock = selectedPlayer.Level >= 99;

                newPlayer.Moves.Add(newMove);
            }
        }
        else
        {
            Console.Error.WriteLine("NewPlayer Moves is null");
        }

        for (int i = 0; i < 2; i++)
        {
            Move newMove = Moves[0x00];
            newMove.Level = 1;
            newMove.UsedCount = 0;
            newPlayer.Moves.Add(newMove);
        }

        return newPlayer;
    }

    public virtual (int, int, string, bool) Training(Player player, int newStat, int statIndex)
    {
        if (player.InvestedFreedom is null || player.InvestedPoint is null || player.Position?.Name is null || player.Stat is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (player.InvestedFreedom.Sum() < player.Freedom)
        {
            player.InvestedFreedom[statIndex] = newStat;
            player.InvestedPoint[statIndex] = newStat;
            return (0, player.Freedom + 1, "investedNumericUpDown" + (statIndex + 3), true);
        }

        int statRemoveIndex = 0;

        statRemoveIndex = player.Position.Name switch
        {
            "Forward" => statIndex switch
            {
                0 => // Kick
                    2,
                1 => // Dribble
                    3,
                2 => // Technique
                    0,
                3 => // Block
                    1,
                4 => // Speed
                    5,
                5 => // Stamina
                    4,
                6 => // Catch
                    7,
                7 => // Luck
                    6,
                _ => statRemoveIndex
            },
            "Midfielder" => statIndex switch
            {
                0 => // Kick
                    6,
                1 => // Dribble
                    2,
                2 => // Technique
                    1,
                3 => // Block
                    7,
                4 => // Speed
                    5,
                5 => // Stamina
                    4,
                6 => // Catch
                    0,
                7 => // Luck
                    3,
                _ => statRemoveIndex
            },
            "Defender" => statIndex switch
            {
                0 => // Kick
                    6,
                1 => // Dribble
                    7,
                2 => // Technique
                    3,
                3 => // Block
                    2,
                4 => // Speed
                    5,
                5 => // Stamina
                    4,
                6 => // Catch
                    0,
                7 => // Luck
                    1,
                _ => statRemoveIndex
            },
            _ => statIndex switch
            {
                0 => // Kick
                    7,
                1 => // Dribble
                    3,
                2 => // Technique
                    6,
                3 => // Block
                    1,
                4 => // Speed
                    5,
                5 => // Stamina
                    4,
                6 => // Catch
                    2,
                7 => // Luck
                    0,
                _ => statRemoveIndex
            }
        };

        if (newStat > player.InvestedPoint[statIndex])
        {
            player.InvestedPoint[statRemoveIndex] -= 1;
        }
        else
        {
            player.InvestedPoint[statRemoveIndex] += 1;
        }

        player.InvestedPoint[statIndex] = newStat;

        return (player.InvestedFreedom[statIndex], player.InvestedFreedom[statIndex] + player.Stat[statRemoveIndex + 2] - 1, "investedNumericUpDown" + (statRemoveIndex + 3), false);
    }

    public virtual void UpdateInventory()
    {
        // Find Equipment
        Equipments = Inventory.Where(x => TCommon.Equipments.ContainsKey(x.Value.Id)).ToDictionary(x => x.Key, y => TCommon.Equipments[y.Value.Id]);
        Equipments.Add(0, TCommon.Equipments[0x0]);
    }

    public abstract SaveInfo GetSaveInfo();
    public abstract List<Team> GetTactics();

    public virtual byte[] Save()
    {
        byte[] saveData = Data.GetSection(0, (int)Data.Length);
        BinaryDataWriter dataWriter = new BinaryDataWriter(saveData);

        SaveSaveInfo(dataWriter);
        SaveTactics(dataWriter);
        SaveInventory(dataWriter);
        SavePlayers(dataWriter);

        return saveData;
    }

    public abstract void SaveSaveInfo(BinaryDataWriter writer);

    public abstract void SaveTactics(BinaryDataWriter writer);

    public abstract void SaveInventory(BinaryDataWriter writer);

    public abstract void SavePlayers(BinaryDataWriter writer);
}