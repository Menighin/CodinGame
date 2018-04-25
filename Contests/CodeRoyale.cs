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
            var site = new Site() 
            {
                Id     = int.Parse(inputs[0]),
                X      = int.Parse(inputs[1]),
                Y      = int.Parse(inputs[2]),
                Radius = int.Parse(inputs[3])
            };
            Table.Sites[site.Id] = site;
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            Player.Gold = int.Parse(inputs[0]);
            Player.TouchSite = int.Parse(inputs[1]); // -1 if none

            // Reading the sites
            Table.EmptySites = new List<Site>();
            Table.EnemySites = new List<Site>();
            Player.Sites = new List<Site>();
            for (int i = 0; i < Table.NumSites; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var site = Table.Sites[int.Parse(inputs[0])];

                int ignore1 = int.Parse(inputs[1]); // used in future leagues
                int ignore2 = int.Parse(inputs[2]); // used in future leagues

                site.StructureType = (StructureTypeEnum) int.Parse(inputs[3]);
                site.IsEnemy       = int.Parse(inputs[4]) == -1 ? null : Convert.ToBoolean(int.Parse(inputs[4]));
                site.NextSpawn     = int.Parse(inputs[5]) == -1 ? null : Convert.ToBoolean(int.Parse(inputs[5]));
                site.CreepType     = (CreepTypeEnum) int.Parse(inputs[6]);

                if (site.IsEnemy == null) Table.EmptySites.Add(site);
                else if (site.IsEnemy) Table.EnemySites.Add(site);
                else Player.Sites.Add(site);
            }

            // Reading the units
            Table.NumUnits = int.Parse(Console.ReadLine());
            Table.EnemyUnits = new List<Unit>();
            Player.Units = new List<Unit>();
            for (int i = 0; i < Table.NumUnits; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int owner = int.Parse(inputs[2]); // 0 Friendly, 1 Foe
                int unitType = (UnitTypeEnum) int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
                int health = int.Parse(inputs[4]);

                var unit = new Unit() 
                {
                    X = x,
                    Y = y,
                    HealthPoints = health,
                    UnitType = unitType
                };

                if (unit.UnitType == UnitTypeEnum.Queen) 
                {
                    if (owner == 0)
                        Player.Queen = unit;
                    else
                        Table.EnemyQueen = unit;
                }
                else
                {
                    if (owner == 0)
                        Player.Units.Add(unit);
                    else
                        Table.EnemyUnits.Add(unit);
                }
            }

            Console.WriteLine("WAIT");
            Console.WriteLine("TRAIN");
        }
    }
}

static class Player {
    public static int Gold;
    public static int TouchSite;
    public static Unit Queen = null;
    public static List<Unit> Units = null;
    public static List<Unit> Sites = null;
}

static class Table {
    public static int NumSites;
    public static int NumUnits;
    public static Dictionary<int, Site> Sites = new Dictionary<int, Site>();
    public static List<Site> EmptySites = null;
    public static List<Unit> EnemyUnits = null;
    public static Unit EnemyQueen = null;
    public static List<Site> EnemySites = null;
}

class Site {
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Radius { get; set; } 
    public StructureTypeEnum StructureType { get; set; }
    public int NextSpawn { get; set; }
    public bool? IsEnemy { get; set; }
    public CreepTypeEnum CreepType { get; set; }
}

class Unit {
    public int X { get; set; }
    public int Y { get; set; }
    public int HealthPoints { get; set; }
    public UnitTypeEnum UnitType { get; set; }
}

enum StructureTypeEnum {
    None = -1,
    Barracks = 2
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