namespace RollTheDiceRace;

public class Player
{
    public string Name { get; }
    public int Score { get; private set; }
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
    public bool IsAi { get; set; }

    public Player(string name) => Name = name;

    public void AddPoints(int amount) => Score += amount;

    public void RemovePoints(int amount)
    {
        Score -= amount;
        if (Score < 0) Score = 0;
    }
    public void SwapScoreWith(Player other)
    {
        (Score, other.Score) = (other.Score, Score);
    }
}

public class Dice
{
    private readonly Random _random;
    public Dice(Random random) => _random = random;
    public int Roll() => _random.Next(1, 7);
}

public interface IPowerUp
{
    string Name { get; }
    string Description { get; }

    // roll = Punkte, die der aktive Spieler durch den Wurf bekommen würde.
    // Die Methode darf roll verändern und/oder direkt Punkte zwischen den Spielern verschieben.
    void Apply(Player self, Player opponent, ref int roll);
}

public class DoublePointsPowerUp : IPowerUp
{
    public string Name => "Giant Growth";
    public string Description => "You grow huge! Your points for this roll are doubled.";

    public void Apply(Player self, Player opponent, ref int roll)
    {
        PlayGrowthAnimation(self);
        roll *= 2;
    }

    private static void PlayGrowthAnimation(Player self)
    {
        for (int size = 1; size <= 10; size++)
        {
            string bar = new string('█', size).PadRight(10, '·');
            Console.Write("\r");
            WriteColored(self.Name, self.Color);
            Console.Write($" is growing! [{bar}]");
            Thread.Sleep(200);
        }

        Console.Write("\r");
        WriteColored(self.Name, self.Color);
        Console.WriteLine(" is now GIANT! Points for this roll are doubled!          ");
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}

public class HalvePointsPowerUp : IPowerUp
{
    public string Name => "Shrink Ray";
    public string Description => "You shrink! Your points for this roll are halved.";

    public void Apply(Player self, Player opponent, ref int roll)
    {
        PlayShrinkAnimation(self);
        roll = Math.Max(1, roll / 2);
    }

    private static void PlayShrinkAnimation(Player self)
    {
        for (int size = 10; size >= 1; size--)
        {
            string bar = new string('█', size).PadRight(10, '·');
            Console.Write("\r");
            WriteColored(self.Name, self.Color);
            Console.Write($" is shrinking! [{bar}]");
            Thread.Sleep(200);
        }

        Console.Write("\r");
        WriteColored(self.Name, self.Color);
        Console.WriteLine(" is now TINY! Points for this roll are halved!            ");
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}

public class StealPointsPowerUp : IPowerUp
{
    private const int StealAmount = 5;
    public string Name => "Pickpocket";
    public string Description => $"You steal {StealAmount} points from your opponent!";

    public void Apply(Player self, Player opponent, ref int roll)
    {
        PlayStealAnimation(self, opponent);
        opponent.RemovePoints(StealAmount);
        self.AddPoints(StealAmount);
    }

    private static void PlayStealAnimation(Player self, Player opponent)
    {
        const int trackWidth = 10;

        for (int position = 0; position <= trackWidth; position++)
        {
            string track = new string('-', position) + "💰" + new string('-', trackWidth - position);

            Console.Write("\r");
            WriteColored(opponent.Name, opponent.Color);
            Console.Write($" [{track}] ");
            WriteColored(self.Name, self.Color);
            Console.Write("   ");

            Thread.Sleep(120);
        }

        Console.Write("\r");
        WriteColored(self.Name, self.Color);
        Console.Write(" pickpocketed ");
        WriteColored(opponent.Name, opponent.Color);
        Console.WriteLine($" for {StealAmount} points!                          ");
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}

public class BonusRollPowerUp : IPowerUp
{
    private const int BonusAmount = 3;
    public string Name => "Lucky Star";
    public string Description => $"Lucky! You get +{BonusAmount} bonus points on top of your roll.";

    public void Apply(Player self, Player opponent, ref int roll)
    {
        PlaySparkleAnimation(self);
        roll += BonusAmount;
    }

    private static void PlaySparkleAnimation(Player self)
    {
        string[] sparkles = { "✨", "🌟", "⭐", "💫" };
        var random = new Random();

        for (int i = 0; i < 10; i++)
        {
            string frame = "";
            for (int j = 0; j < 5; j++)
                frame += sparkles[random.Next(sparkles.Length)];

            Console.Write("\r");
            WriteColored(self.Name, self.Color);
            Console.Write($" feels lucky... {frame}   ");
            Thread.Sleep(200);
        }

        Console.Write("\r");
        WriteColored(self.Name, self.Color);
        Console.WriteLine($" got lucky! +{BonusAmount} bonus points! ✨✨✨              ");
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}

public class SwapScoresPowerUp : IPowerUp
{
    public string Name => "Score Swap";
    public string Description => "Chaos! Total scores are swapped with your opponent.";

    public void Apply(Player self, Player opponent, ref int roll)
    {
        PlaySwapAnimation(self, opponent);
        self.SwapScoreWith(opponent);
    }

    private static void PlaySwapAnimation(Player self, Player opponent)
    {
        int selfScore = self.Score;
        int opponentScore = opponent.Score;

        for (int i = 0; i < 6; i++)
        {
            int shownSelf = i % 2 == 0 ? selfScore : opponentScore;
            int shownOpponent = i % 2 == 0 ? opponentScore : selfScore;

            Console.Write("\r🔄 ");
            WriteColored($"{self.Name}: {shownSelf,3}", self.Color);
            Console.Write("   \u21c4   ");
            WriteColored($"{opponent.Name}: {shownOpponent,3}", opponent.Color);
            Console.Write("   ");

            Thread.Sleep(400);
        }

        Console.WriteLine();
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}

public class PowerUpPool
{
    private readonly List<IPowerUp> _powerUps;
    private readonly Random _random;
    private readonly double _triggerChance;

    public PowerUpPool(Random random, double triggerChance = 0.3)
    {
        _random = random;
        _triggerChance = triggerChance;
        _powerUps = new List<IPowerUp>
        {
            new DoublePointsPowerUp(),
            new HalvePointsPowerUp(),
            new StealPointsPowerUp(),
            new BonusRollPowerUp(),
            new SwapScoresPowerUp(),
        };
    }

    // Gibt null zurück, wenn kein Power-up ausgelöst wird.
    public IPowerUp? TryTrigger()
    {
        if (_random.NextDouble() > _triggerChance)
            return null;

        int index = _random.Next(_powerUps.Count);
        return _powerUps[index];
    }
}

public class Game
{
    private static readonly ConsoleColor[] PlayerColors =
    {
        ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Blue,
        ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.White,
        ConsoleColor.DarkYellow, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan,
        ConsoleColor.DarkMagenta, ConsoleColor.DarkBlue
    };

    private readonly Dice _dice;
    private readonly Random _random;
    private Player _player;
    private Player _enemy;
    private readonly PowerUpPool _powerUps;
    private readonly int _targetScore;

    public Game(int targetScore = 50)
    {
        _random = new Random();
        _dice = new Dice(_random);
        _powerUps = new PowerUpPool(_random);
        _targetScore = targetScore;
        _player = new Player("Player");
        _enemy = new Player("Enemy");
    }

    public void Play()
    {
        ShowWelcomeMessage();
        (_player, _enemy) = ChooseGameMode();
        AssignPlayerColors(_player, _enemy);

        int round = 1;
        while (_player.Score < _targetScore && _enemy.Score < _targetScore)
        {
            PlayRound(round);
            round++;
        }

        AnnounceWinner();
    }

    private void ShowWelcomeMessage()
    {
        Console.WriteLine("Welcome to Roll the Dice: Race to " + _targetScore + "!");
        Console.WriteLine("Roll the dice each round — points add up. First to reach the target wins.");
        Console.WriteLine("Watch out for power-ups along the way!\n");
    }

    private (Player, Player) ChooseGameMode()
    {
        string[] modeLabels = { "Player 1 vs Player 2", "You vs Enemy" };
        int selectedIndex = 0;

        Console.WriteLine("Choose a game mode (use \u2190 \u2192 to switch, Enter to confirm):");
        DrawModeSelection(modeLabels, selectedIndex);

        ConsoleKey key;
        do
        {
            key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
            {
                selectedIndex = selectedIndex == 0 ? 1 : 0;
                DrawModeSelection(modeLabels, selectedIndex);
            }
        } while (key != ConsoleKey.Enter);

        Console.WriteLine("\n");

        if (selectedIndex == 0)
        {
            Console.Write("Enter name for Player 1: ");
            string name1 = ReadNameOrFallback("Player 1");

            Console.Write("Enter name for Player 2: ");
            string name2 = ReadNameOrFallback("Player 2");

            return (new Player(name1), new Player(name2));
        }

        return (new Player("Player"), new Player("Enemy") { IsAi = true });
    }

    private static void DrawModeSelection(string[] labels, int selectedIndex)
    {
        Console.Write($"\r> {labels[selectedIndex],-25}");
    }

    private static string ReadNameOrFallback(string fallback)
    {
        string? input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? fallback : input;
    }

    private void AssignPlayerColors(Player player1, Player player2)
    {
        int index1 = _random.Next(PlayerColors.Length);
        int index2;
        do
        {
            index2 = _random.Next(PlayerColors.Length);
        } while (index2 == index1);

        player1.Color = PlayerColors[index1];
        player2.Color = PlayerColors[index2];
    }

    private void PlayRound(int round)
    {
        Console.WriteLine($"\n--- Round {round} ---");

        PlayerTurn(_player, _enemy);
        if (_player.Score >= _targetScore) return;

        PlayerTurn(_enemy, _player);
    }

    private void PlayerTurn(Player active, Player opponent)
    {
        Console.WriteLine();
        WriteColored(active.Name, active.Color);

        if (active.IsAi)
        {
            Console.WriteLine(" is rolling...");
            Thread.Sleep(700);
        }
        else
        {
            Console.WriteLine("'s turn — press any key to roll...");
            Console.ReadKey(true);
        }

        int roll = _dice.Roll();
        WriteColored(active.Name, active.Color);
        Console.WriteLine($" rolled a {roll}.");

        IPowerUp? powerUp = _powerUps.TryTrigger();
        if (powerUp != null)
        {
            PrintColored($"POWER-UP: {powerUp.Name}! {powerUp.Description}", ConsoleColor.Magenta);
            powerUp.Apply(active, opponent, ref roll);
        }

        active.AddPoints(roll);

        WriteColored(active.Name, active.Color);
        Console.Write($" now has {active.Score} points. (");
        WriteColored(opponent.Name, opponent.Color);
        Console.WriteLine($": {opponent.Score})");
    }

    private void AnnounceWinner()
    {
        Console.WriteLine("\n=== GAME OVER ===");
        Player winner = _player.Score >= _targetScore ? _player : _enemy;
        Console.Write("🏆 ");
        WriteColored(winner.Name, winner.Color);
        Console.WriteLine($" reached {_targetScore} points and wins!");
    }

    private static void PrintColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        new Game(targetScore: 50).Play();
    }
}