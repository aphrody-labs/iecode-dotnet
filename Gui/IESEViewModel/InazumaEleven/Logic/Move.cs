namespace IESEViewModel.InazumaEleven.Logic;

public class Move
{
    public string? Name;

    public Element? Element;

    public Position? Position;

    public int Power;

    public int Tp;

    public int Difficulty;

    public int Damage;

    public int EvolutionCount;

    public EvolutionSpeed? EvolutionSpeed;

    public UltimateEvolution? UltimateEvolution;

    public List<int> PowerLevel = [];

    public int Level;

    public bool Unlock;

    public int UsedCount;

    public Move()
    {

    }

    public Move(Move move, int level, bool unlock, int timeLevel)
    {
        Name = move.Name;
        Element = move.Element;
        Position = move.Position;
        Power = move.Power;
        Tp = move.Tp;
        Difficulty = move.Difficulty;
        Damage = move.Damage;
        EvolutionCount = move.EvolutionCount;
        EvolutionSpeed = move.EvolutionSpeed;
        UltimateEvolution = move.UltimateEvolution;

        Level = level;
        Unlock = unlock;
        UsedCount = timeLevel;
    }

    public Move(string name, Element? element, Position? position, int power, int tp, int difficulty, int damage, int evolutionCount, EvolutionSpeed? evolutionSpeed, UltimateEvolution? ultimateEvolution)
    {
        Name = name;
        Element = element;
        Position = position;
        Power = power;
        Tp = tp;
        Difficulty = difficulty;
        Damage = damage;
        EvolutionCount = evolutionCount;
        EvolutionSpeed = evolutionSpeed;
        Level = 1;

        if (EvolutionSpeed != null)
        {
            PowerLevel.Add(power);

            // true =  IE1/IE2/IE Game & False = Go/Cs/Galaxy
            if (EvolutionSpeed.PowerLevel.Count != 0)
            {
                for (int i = 0; i < EvolutionCount - 1; i++)
                {
                    PowerLevel.Add(power + EvolutionSpeed.PowerLevel[i]);
                }
            }
            else
            {
                if (evolutionCount < 6)
                {
                    for (int i = 0; i < EvolutionCount - 1; i++)
                    {
                        PowerLevel.Add(PowerLevel[i] + 10 / PowerLevel[i] * 100);
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        PowerLevel.Add(PowerLevel[i] + 10 / PowerLevel[i] * 100);
                    }
                    UltimateEvolution = ultimateEvolution;
                    // Missing Ultimate Evolution
                }
            }

            EvolutionSpeed.TimeLevel.Insert(0, 0);
        }
        else
        {
            EvolutionSpeed = new EvolutionSpeed("Skill", [0], [0]);
        }
    }

    public override string? ToString()
    {
        return Name;
    }

    public int? PowerAtLevel(int level)
    {
        if (level > PowerLevel.Count)
        {
            return null;
        }

        if (level == 0 | level == 1)
        {
            return PowerLevel[0];
        }

        return level == 6 ? UltimateEvolution!.PowerM : PowerLevel[level - 1];
    }
}

public class EvolutionSpeed(string name, List<int> powerUp, List<int> timeLevel)
{
    public string Name = name;

    public List<int> PowerLevel = powerUp;

    public List<int> TimeLevel = timeLevel;

    public static EvolutionSpeed Slow(int evolutionCount, string game)
    {
        return evolutionCount switch
        {
            3 => new EvolutionSpeed("Slow", [5, 12], [15, 33, 0]),
            4 => new EvolutionSpeed("Slow", [], [20, 30, 35, 0]),
            5 when game == "ie" => new EvolutionSpeed("Slow", [4, 10, 14, 18],
            [
                20,
                45,
                70,
                100,
                0
            ]),
            5 when game == "go" => new EvolutionSpeed("Slow", [], [
                20,
                30,
                35,
                40,
                0
            ]),
            _ => new EvolutionSpeed("Slow", [], [
                15,
                20,
                20,
                25,
                0
            ])
        };
    }

    public static EvolutionSpeed Medium(int evolutionCount, string game)
    {
        return evolutionCount switch
        {
            3 => new EvolutionSpeed("Medium", [3, 10], [9, 27, 0]),
            4 => new EvolutionSpeed("Medium", [], [15, 20, 30, 0]),
            5 when game == "ie" => new EvolutionSpeed("Medium", [3, 8, 12, 16], [20, 40, 60, 90, 0]),
            5 when game == "go" => new EvolutionSpeed("Medium", [], [15, 20, 30, 40, 0]),
            _ => new EvolutionSpeed("Medium", [], [10, 15, 20, 25, 0])
        };
    }

    public static EvolutionSpeed Fast(int evolutionCount, string game)
    {
        return evolutionCount switch
        {
            3 => new EvolutionSpeed("Fast", [2, 8], [6, 21, 0]),
            4 => new EvolutionSpeed("Fast", [], [10, 15, 25, 0]),
            5 when game == "ie" => new EvolutionSpeed("Fast", [2, 6, 10, 14], [15, 35, 55, 80, 0]),
            5 when game == "go" => new EvolutionSpeed("Fast", [], [10, 15, 25, 40, 0]),
            _ => new EvolutionSpeed("Fast", [], [10, 10, 15, 20, 0])
        };
    }

    public static EvolutionSpeed Turbo(int evolutionCount, string game)
    {
        return new EvolutionSpeed("Turbo", [], [5, 10, 10, 15, 0]);
    }
}

public class UltimateEvolution(string name)
{
    public string? Name = name;

    public int PowerM;

    public int Tp;

    public int Difficulty;

    public int Damage;

    public UltimateEvolution Power(string position, int difficulty)
    {
        UltimateEvolution power = new UltimateEvolution("Power");

        switch (position)
        {
            case "Shoot":
                PowerM = 320;
                Tp = 99;
                Damage = 10;
                break;
            case "Dribble":
                PowerM = 320;
                Tp = 99;
                Damage = 5;
                break;
            case "Block":
                PowerM = 320;
                Tp = 85;
                Damage = 5;
                break;
            case "Catch":
                PowerM = 320;
                Tp = 85;
                Damage = -10;
                break;
        }

        Difficulty = Difficulty < 100 ? 100 : difficulty;

        return power;
    }

    public UltimateEvolution Balanced(string position, int difficulty)
    {
        UltimateEvolution balanced = new UltimateEvolution("Balanced");

        switch (position)
        {
            case "Shoot":
                PowerM = 300;
                Tp = 99;
                Damage = 50;
                break;
            case "Dribble":
                PowerM = 300;
                Tp = 99;
                Damage = 80;
                break;
            case "Block":
                PowerM = 300;
                Tp = 85;
                Damage = 80;
                break;
            case "Catch":
                PowerM = 300;
                Tp = 85;
                Damage = -40;
                break;
        }

        Difficulty = Difficulty < 100 ? 100 : difficulty;

        return balanced;
    }

    public UltimateEvolution Stunner(string position, int difficulty)
    {
        UltimateEvolution stunner = new UltimateEvolution("Balanced");

        switch (position)
        {
            case "Shoot":
                PowerM = 290;
                Tp = 99;
                Damage = 100;
                break;
            case "Dribble":
                PowerM = 290;
                Tp = 99;
                Damage = 150;
                break;
            case "Block":
                PowerM = 290;
                Tp = 85;
                Damage = 150;
                break;
            case "Catch":
                PowerM = 290;
                Tp = 85;
                Damage = -80;
                break;
        }

        Difficulty = Difficulty < 100 ? 100 : difficulty;

        return stunner;
    }

    public UltimateEvolution ChainBlockLongShot(int difficulty)
    {
        UltimateEvolution power = new UltimateEvolution("Power");

        PowerM = 300;
        Tp = 99;
        Damage = 5;

        Difficulty = Difficulty < 100 ? 100 : difficulty;

        return power;
    }

    public UltimateEvolution PunchingCatchDamageDribble(string position, int difficulty)
    {
        UltimateEvolution stunner = new UltimateEvolution("Stunner");

        PowerM = 280;

        switch (position)
        {
            case "Dribble":
                Tp = 99;
                Damage = 150;
                break;
            case "Catch":
                Tp = 85;
                Damage = -80;
                break;
        }

        Difficulty = Difficulty < 100 ? 100 : difficulty;

        return stunner;
    }

    public UltimateEvolution DefenseBlock(int difficulty)
    {
        UltimateEvolution balanced = new UltimateEvolution("Balanced");

        PowerM = 280;
        Tp = 85;
        Damage = 50;

        Difficulty = Difficulty < 100 ? 100 : difficulty;

        return balanced;
    }
}
