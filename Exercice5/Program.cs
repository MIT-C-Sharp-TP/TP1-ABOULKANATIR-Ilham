using System;

int Heal(int health, int amount, int maxHealth)
{
return Math.Min(health + amount, maxHealth);
}

int result = Heal(50, 30, 100);
Console.WriteLine($"Health after healing: {result} HP");
