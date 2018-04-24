using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Game
{
    static void Main(string[] args)
    {
        string[] inputs;
        Table.NumSites = int.Parse(Console.ReadLine());
        for (int i = 0; i < Table.NumSites; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int siteId = int.Parse(inputs[0]);
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int gold = int.Parse(inputs[0]);
            int touchedSite = int.Parse(inputs[1]); // -1 if none
            for (int i = 0; i < Table.NumSites; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int siteId = int.Parse(inputs[0]);
                int ignore1 = int.Parse(inputs[1]); // used in future leagues
                int ignore2 = int.Parse(inputs[2]); // used in future leagues
                int structureType = int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
                int owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                int param1 = int.Parse(inputs[5]);
                int param2 = int.Parse(inputs[6]);
            }
            int numUnits = int.Parse(Console.ReadLine());
            for (int i = 0; i < numUnits; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int owner = int.Parse(inputs[2]);
                int unitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
                int health = int.Parse(inputs[4]);
            }

            Console.WriteLine("WAIT");
            Console.WriteLine("TRAIN");
        }
    }
}

class Player {
    public int Gold { get; set; }
    public int TouchSite { get; set; }
    public Unit Queen { get; set; }
}

static class Table {
    public static int NumSites;
    public static int NumUnits;
}

class Site {
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Radius { get; set; } 
    public StructureTypeEnum StructureType { get; set; }
    public int NextSpawn { get; set; }
    public int MyProperty { get; set; }
}

class Unit {
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsEnemy { get; set; }
    public int HealthPoints { get; set; }
    public UnitTypeEnum UnitType { get; set; }
}

enum StructureTypeEnum {
    None = -1,
    Barracs = 2
}

enum OwnerEnum {
    None = -1,
    Friendly = 0,
    Enemy = 1
}

enum CreepTypeEnum {
    None = -1,
    Knight = 0,
    Archer = 1
}

enum UnitTypeEnum {
    Queen = -1,
    Knight = 0,
    Archer = 1
}