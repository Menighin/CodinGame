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
                site.IsEnemy       = int.Parse(inputs[4]) == -1 ? (bool?)null : Convert.ToBoolean(int.Parse(inputs[4]));
                site.NextSpawn     = int.Parse(inputs[5]);
                site.CreepType     = (CreepTypeEnum) int.Parse(inputs[6]);

                if (site.IsEnemy == null) Table.EmptySites.Add(site);
                else if (site.IsEnemy == true) Table.EnemySites.Add(site);
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
                UnitTypeEnum unitType = (UnitTypeEnum) int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
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
                    if (owner == 0) {
                        Player.Queen = unit;
                        if (Player.StartingSide == null) {
                            if (unit.X < 1920 / 2) Player.StartingSide = false;
                            else Player.StartingSide = true;
                        }
                    }
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

            Player.Play();
            
        }
    }
}

static class Player {
    public static int Gold;
    public static int TouchSite;
    public static Unit Queen = null;
    public static List<Unit> Units = null;
    public static List<Site> Sites = null;
    public static bool? StartingSide = null; // True = right, False = left
    public static int NumKnights => Sites.Where(s => s.StructureType == StructureTypeEnum.Barracks && s.CreepType == CreepTypeEnum.Knight).Count();
    public static int NumArchers => Sites.Where(s => s.StructureType == StructureTypeEnum.Barracks && s.CreepType == CreepTypeEnum.Archer).Count();
    public static int TrainedKnights = 0;

    public static int MustHaveArchers = 0;
    public static int MustHaveKnights = 1;

    public static bool HasEnoughGoldToTrain(CreepTypeEnum c) {
        return (c == CreepTypeEnum.Archer && Player.Gold >= 100) ||
               (c == CreepTypeEnum.Knight && Player.Gold >= 80 );
    }

    public static void Play() {
        Player.MoveOrBuild();

        Player.Train();
    }

    public static void MoveOrBuild() {
        var closestEmptySite = Table.GetClosestPieceFrom(Player.Queen, Table.EmptySites);
        var closestEnemyCreep = Table.GetClosestPieceFrom(Player.Queen, Table.EnemyUnits);

        var closestEnemyCreepDistance = closestEnemyCreep == null ? (double?) null : Table.GetDistanceBetween(Player.Queen, closestEnemyCreep);
        var isEnemyClose = closestEnemyCreepDistance < 80;
        var haveEnoughBarracks = Player.NumArchers > 0 && Player.NumKnights > 0;

        if (closestEmptySite != null) 
        {
            if (isEnemyClose || haveEnoughBarracks)
            {
                Console.WriteLine($"BUILD {closestEmptySite.Id} TOWER");
            }
            else 
            {
                var type = Player.NumKnights == 0 ? "KNIGHT" : "ARCHER";
                Console.WriteLine($"BUILD {closestEmptySite.Id} BARRACKS-{type}");
            }
        }
        else 
        {

            var closestTower = Table.GetFarthestPieceFrom(Player.Queen, Player.Sites.Where(s => s.StructureType == StructureTypeEnum.Tower).ToList());
            if (closestTower != null) {
                Console.WriteLine($"MOVE {closestTower.X} {closestTower.Y}");
            }
            else if (Player.StartingSide == false)
                Console.WriteLine($"MOVE 0 0");
            else
                Console.WriteLine($"MOVE 1920 1000");
        }
    }

    public static void Train() {
        var possibleTraining = Player.Sites.Where(s => s.NextSpawn == 0).ToList();
        var creepType = default(CreepTypeEnum);
        if (Player.TrainedKnights >= 3) {
            possibleTraining = possibleTraining.Where(s => s.CreepType == CreepTypeEnum.Archer).ToList();
            creepType = CreepTypeEnum.Archer;
        } else {
            possibleTraining = possibleTraining.Where(s => s.CreepType == CreepTypeEnum.Knight).ToList();
            creepType = CreepTypeEnum.Knight;
        }

        var trainingSite = Table.GetClosestPieceFrom(Table.EnemyQueen, possibleTraining);

        if (Player.HasEnoughGoldToTrain(creepType) && trainingSite != null) {
            Console.WriteLine($"TRAIN {trainingSite.Id}".Trim());
            if (Player.TrainedKnights >= 3) Player.TrainedKnights = 0;
            if (creepType == CreepTypeEnum.Knight) Player.TrainedKnights++;
        }
        else
            Console.WriteLine($"TRAIN");
    }
}

static class Table {
    public static int NumSites;
    public static int NumUnits;
    public static Dictionary<int, Site> Sites = new Dictionary<int, Site>();
    public static List<Site> EmptySites = null;
    public static List<Unit> EnemyUnits = null;
    public static Unit EnemyQueen = null;
    public static List<Site> EnemySites = null;

    public static double GetDistanceBetween(Piece p1, Piece p2) {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    public static Piece GetClosestPieceFrom(Piece piece, IEnumerable<Piece> pieces) {
        var min = Double.MaxValue;
        var result = (Piece) null;
        foreach (var p in pieces) {
            var d = GetDistanceBetween(piece, p);
            if (d < min) {
                min = d;
                result = p;
            }
        }
        return result;
    }

    public static Piece GetFarthestPieceFrom(Piece piece, IEnumerable<Piece> pieces) {
        var max = Double.MinValue;
        var result = (Piece) null;
        foreach (var p in pieces) {
            var d = GetDistanceBetween(piece, p);
            if (d > max) {
                max = d;
                result = p;
            }
        }
        return result;
    }
}

abstract class Piece {
    public int Id { get; set; }
    public abstract int X {get; set; }
    public abstract int Y {get; set; }
}

class Site : Piece {
    public override int X { get; set; }
    public override int Y { get; set; }
    public int Radius { get; set; } 
    public StructureTypeEnum StructureType { get; set; }
    public int NextSpawn { get; set; }
    public bool? IsEnemy { get; set; }
    public CreepTypeEnum CreepType { get; set; }
}

class Unit : Piece {
    public override int X { get; set; }
    public override int Y { get; set; }
    public int HealthPoints { get; set; }
    public UnitTypeEnum UnitType { get; set; }
}

enum StructureTypeEnum {
    None = -1,
    Tower = 1,
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
    Archer = 1,
    Giant = 2
}