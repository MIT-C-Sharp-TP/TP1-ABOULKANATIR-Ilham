using System;

int Heal(int health, int amount, int maxHealth)
{
return Math.Min(health + amount, maxHealth);
}

int result1 = Heal(50, 30, 100);
Console.WriteLine($"Health after healing: {result1} HP");

int result2 = Heal(90, 30, 100);
Console.WriteLine($"Health after healing: {result2} HP");

int result3 = Heal(0, 25, 100);
Console.WriteLine($"Health after healing: {result3} HP");
