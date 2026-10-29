using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

List<int> wins = new List<int>();
int game = 0;

while (true)
{
    game++;

    int resultCode = PlayGame();

    string result;

    if (resultCode > 0)
    {
        result = $"Game {game}: victory in {resultCode} turns";
        wins.Add(resultCode);
    }
    else if (resultCode == 0)
    {
        result = $"Game {game}: defeat";
    }
    else
    {
        result = $"Game {game}: ran away";
    }

    SaveResult(result);

    Console.Write("Play again? (y/n) > ");

    string? answer = Console.ReadLine();

    if (answer == null ||
        !answer.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
}

Console.WriteLine("\n=== Statistics ===");
Console.WriteLine($"Victories: {wins.Count} of {game}");

Console.WriteLine("Best victories:");

foreach (int turns in wins.OrderBy(t => t).Take(3))
{
    Console.WriteLine($"- {turns} turns");
}

int PlayGame()
{
    Player player = new Player("Aya", 100, 2);

    player.Died += name =>
        Console.WriteLine($"Game over for {name}.");

    List<Enemy> enemies = new List<Enemy>
    {
        new Goblin(),
        new Orc(),
        new Goblin(),
        new Boss()
    };

    Random rng = new Random();
    int turn = 0;

    Console.WriteLine("\n=== Arena ===");

    Console.WriteLine($"{player.Name} faces {enemies.Count} enemies:");

    foreach (Enemy enemy in enemies)
    {
        Console.WriteLine($"- {enemy.Name}, {enemy.Health} HP");
    }

    while (player.IsAlive && enemies.Count > 0)
    {
        Enemy enemy = enemies[0];

        Console.WriteLine(
            $"\nTurn {turn + 1}: {player.Name} {player.Health} HP, " +
            $"{player.Potions} potions | {enemy.Name} {enemy.Health} HP");

        Console.Write("1 attack, 2 drink a potion, 3 run away > ");

        string? text = Console.ReadLine();

        if (text == null)
        {
            Console.WriteLine("Input ended. Leaving the arena.");
            return -1;
        }

        if (!int.TryParse(text, out int choice))
        {
            Console.WriteLine("Type 1, 2 or 3.");
            continue;
        }

        if (choice < 1 || choice > 3)
        {
            Console.WriteLine("Type 1, 2 or 3.");
            continue;
        }

        if (choice == 3)
        {
            Console.WriteLine("You run away.");
            return -1;
        }

        turn++;

        if (choice == 1)
        {
            int damage = rng.Next(12, 25);
            enemy.TakeDamage(damage);

            if (!enemy.IsAlive)
            {
                Console.WriteLine($"{enemy.Name} is defeated!");
                enemies.RemoveAt(0);
                continue;
            }
        }
        else
        {
            player.DrinkPotion();
        }

        if (player.IsAlive)
        {
            enemy.Attack(player, rng);
        }
    }

    if (player.IsAlive)
    {
        Console.WriteLine($"Victory in {turn} turns!");
        return turn;
    }

    Console.WriteLine("Defeat...");
    return 0;
}

void SaveResult(string line)
{
    try
    {
        File.AppendAllText(
            "results.txt",
            line + Environment.NewLine);

        Console.WriteLine($"Saved: {line}");
    }
    catch (IOException e)
    {
        Console.WriteLine($"Could not save the result: {e.Message}");
    }
    catch (UnauthorizedAccessException e)
    {
        Console.WriteLine($"Could not save the result: {e.Message}");
    }
}

public class Player
{
    public event Action<string>? Died;

    private int health;

    public string Name { get; }
    public int MaxHealth { get; }
    public int Potions { get; private set; }

    public int Health
    {
        get { return health; }
        private set { health = Math.Clamp(value, 0, MaxHealth); }
    }

    public bool IsAlive => Health > 0;

    public Player(string name, int maxHealth, int potions)
    {
        Name = name;
        MaxHealth = maxHealth;
        Potions = potions;
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        bool wasAlive = IsAlive;

        Health -= amount;

        Console.WriteLine(
            $"{Name} takes {amount} damage ({Health} HP)");

        if (wasAlive && !IsAlive)
        {
            Died?.Invoke(Name);
        }
    }

    public void DrinkPotion()
    {
        if (Potions <= 0)
        {
            Console.WriteLine("No potion left!");
            return;
        }

        Potions--;
        Health += 30;

        Console.WriteLine(
            $"{Name} drinks a potion ({Health} HP)");
    }
}

public abstract class Enemy
{
    public string Name { get; }
    public int Health { get; private set; }

    public bool IsAlive => Health > 0;

    protected Enemy(string name, int health)
    {
        Name = name;
        Health = health;
    }

    public virtual void TakeDamage(int amount)
    {
        Health = Math.Max(Health - amount, 0);

        Console.WriteLine(
            $"{Name} takes {amount} damage ({Health} HP)");
    }

    public abstract void Attack(Player target, Random rng);
}

public class Goblin : Enemy
{
    public Goblin() : base("Goblin", 30)
    {
    }

    public override void Attack(Player target, Random rng)
    {
        Console.WriteLine("Goblin stabs!");
        target.TakeDamage(rng.Next(4, 9));
    }
}

public class Orc : Enemy
{
    public Orc() : base("Orc", 60)
    {
    }

    public override void Attack(Player target, Random rng)
    {
        Console.WriteLine("Orc swings its club!");
        target.TakeDamage(rng.Next(8, 15));
    }
}

public class Boss : Enemy
{
    public int Armour { get; } = 6;

    public Boss() : base("Boss", 60)
    {
    }

    public override void TakeDamage(int amount)
    {
        int reducedDamage = Math.Max(amount - Armour, 1);
        int absorbed = amount - reducedDamage;

        Console.WriteLine($"Boss absorbs {absorbed} damage.");

        base.TakeDamage(reducedDamage);
    }

    public override void Attack(Player target, Random rng)
    {
        Console.WriteLine("Boss slams the ground!");
        target.TakeDamage(rng.Next(9, 17));
    }
}