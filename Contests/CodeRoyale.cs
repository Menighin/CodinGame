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

                site.Gold          = int.Parse(inputs[1]); // used in future leagues
                site.MaxRate       = int.Parse(inputs[2]); // used in future leagues
                site.StructureType = (StructureTypeEnum) int.Parse(inputs[3]);
                site.IsEnemy       = int.Parse(inputs[4]) == -1 ? (bool?)null : Convert.ToBoolean(int.Parse(inputs[4]));
                site.NextSpawn     = int.Parse(inputs[5]);
                site.CreepType     = (CreepTypeEnum) int.Parse(inputs[6]);
                site.Param1        = int.Parse(inputs[5]);
                site.Param2        = int.Parse(inputs[6]);

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

static class Player 
{
    public static int Gold;
    public static int TouchSite;
    public static Unit Queen = null;
    public static List<Unit> Units = null;
    public static List<Site> Sites = null;
    public static bool? StartingSide = null; // True = right, False = left
    public static int NumKnights => Sites.Where(s => s.StructureType == StructureTypeEnum.Barracks && s.CreepType == CreepTypeEnum.Knight).Count();
    public static int NumArchers => Sites.Where(s => s.StructureType == StructureTypeEnum.Barracks && s.CreepType == CreepTypeEnum.Archer).Count();
    public static int NumMines   => Sites.Where(s => s.StructureType == StructureTypeEnum.Mine).Count();
    public static int NumTowers  => Sites.Where(s => s.StructureType == StructureTypeEnum.Tower).Count();
    public static int TrainedKnights = 0;

    public static int MustHaveArchers = 0;
    public static int MustHaveKnights = 1;
    public static int MustHaveMines = 1;
    public static int MustHaveTowers = 4;

    public static bool HaveEnoughKnights    => NumKnights >= MustHaveKnights;
    public static bool HaveEnoughArchers    => NumArchers >= MustHaveArchers;
    public static bool HaveEnoughBarracks   => HaveEnoughArchers && HaveEnoughKnights;
    public static bool HaveEnoughMines      => Sites.Where(s => s.StructureType == StructureTypeEnum.Mine).Sum(s => s.Param1) >= TotalMineRate;
    public static bool HaveEnoughTowers     => NumTowers  >= MustHaveTowers;
    public static bool HaveEnoughStructures => HaveEnoughBarracks && HaveEnoughMines;

    public static bool HaveEmptySites => Table.EmptySites.Any();

    public static PlayerStateEnum PlayerState = PlayerStateEnum.BuildingBarrack;

    public static int TotalMineRate  = 5;
    public static int MaxTowerRadius = 500;

    // <Id, value>
    public static Pair<int, int> IncrementalBuilding = null;

    public static HashSet<int> HadMines = new HashSet<int>();

    public static bool HasEnoughGoldToTrain(CreepTypeEnum c) 
    {
        return (c == CreepTypeEnum.Archer && Player.Gold >= 100) ||
               (c == CreepTypeEnum.Knight && Player.Gold >= 80 );
    }

    public static void Play() 
    {

        Player.UpdateState();

        Player.MoveOrBuild();

        Player.Train();
    }

    public static void UpdateState() 
    {

        // Update all mines
        foreach(var m in Player.Sites.Where(s => s.StructureType == StructureTypeEnum.Mine).ToList())
            Player.HadMines.Add(m.Id);

        switch(Player.PlayerState)
        {
            case PlayerStateEnum.BuildingBarrack:
                if (Player.HaveEnoughBarracks)
                    Player.PlayerState = PlayerStateEnum.BuildingMine;
                break;
            case PlayerStateEnum.BuildingMine:
                if (Player.IncrementalBuilding?.Value >= Table.Sites[Player.IncrementalBuilding.Key].MaxRate) 
                {
                    if (Player.HaveEnoughMines) Player.PlayerState = PlayerStateEnum.BuildingTower;
                    Player.IncrementalBuilding = null;
                }
                break;
            case PlayerStateEnum.BuildingTower:
                var tower = Table.Sites[Player.IncrementalBuilding.Key];
                if(tower.StructureType == StructureTypeEnum.Tower && tower.Param2 >= MaxTowerRadius)
                {
                    if (Player.HaveEnoughTowers) Player.PlayerState = PlayerStateEnum.Running;
                    Player.IncrementalBuilding = null;
                }
                break;
            case PlayerStateEnum.Running:
                if (!Player.HaveEnoughMines) Player.PlayerState = PlayerStateEnum.BuildingMine;
                else if (!Player.HaveEnoughTowers) Player.PlayerState = PlayerStateEnum.BuildingTower;
                break;
        }

        if (!HaveEmptySites) Player.PlayerState = PlayerStateEnum.Running;

        // Console.Error.WriteLine($"State {Player.PlayerState}\n{Player.IncrementalBuilding}\n{Player.IncrementalBuilding != null ? Table.Sites[Player.IncrementalBuilding.Key].ToString() : ""}");
    }

    public static void MoveOrBuild() 
    {
        var closestEmptySite = Table.GetClosestPieceFrom(Player.Queen, Table.EmptySites);
        var closestEnemyCreep = Table.GetClosestPieceFrom(Player.Queen, Table.EnemyUnits);

        var closestEnemyCreepDistance = closestEnemyCreep == null ? (double?) null : Table.GetDistanceBetween(Player.Queen, closestEnemyCreep);
        var isEnemyClose = closestEnemyCreepDistance < 100;

        try {
            switch (Player.PlayerState)
            {
                case PlayerStateEnum.BuildingBarrack:
                    if (!HaveEnoughKnights) Console.WriteLine($"BUILD {closestEmptySite.Id} BARRACKS-KNIGHT");
                    else if (!HaveEnoughArchers) Console.WriteLine($"BUILD {closestEmptySite.Id} BARRACKS-ARCHER");
                    break;
                
                case PlayerStateEnum.BuildingMine:
                    var closestEmptySiteThatWasntAMine = Table.GetClosestPieceFrom(Player.Queen, Table.EmptySites.Where(s => !HadMines.Contains(s.Id)));
                    
                    if (Player.IncrementalBuilding == null) 
                    {
                        Player.IncrementalBuilding = new Pair<int, int>(closestEmptySiteThatWasntAMine.Id, 0);
                    }
                    Console.WriteLine($"BUILD {Player.IncrementalBuilding.Key} MINE");
                    Player.IncrementalBuilding.Value++;
                    break;
                
                case PlayerStateEnum.BuildingTower:
                    if (Player.IncrementalBuilding == null) Player.IncrementalBuilding = new Pair<int, int>(closestEmptySite.Id, 0);
                    Console.WriteLine($"BUILD {Player.IncrementalBuilding.Key} TOWER");
                    Player.IncrementalBuilding.Value++;
                    break;

                case PlayerStateEnum.Running:

                    // Calculate safest spot
                    var towers = Player.Sites.Where(s => s.StructureType == StructureTypeEnum.Tower).ToList();
                    int x = 0, y = 0;
                    foreach (var t in towers) {
                        x += t.X;
                        y += t.Y;
                    }
                    var pos = towers.Count > 0 ? $"{x / towers.Count} {y / towers.Count}" : "0 0";

                    Console.WriteLine($"MOVE {pos}");
                    break;
                default:
                    throw new Exception ("wat");
            }
        } catch {
            Console.Error.WriteLine("ERRO");
            Console.WriteLine($"MOVE 0 0");
        }
        
    }

    public static void Train() 
    {
        var possibleTraining = Player.Sites.Where(s => s.NextSpawn == 0).ToList();
        var creepType = CreepTypeEnum.Knight;
        // if (Player.TrainedKnights >= 3) {
        //     possibleTraining = possibleTraining.Where(s => s.CreepType == CreepTypeEnum.Archer).ToList();
        //     creepType = CreepTypeEnum.Archer;
        // } else {
        //     possibleTraining = possibleTraining.Where(s => s.CreepType == CreepTypeEnum.Knight).ToList();
        //     creepType = CreepTypeEnum.Knight;
        // }

        var trainingSite = Table.GetClosestPieceFrom(Table.EnemyQueen, possibleTraining);

        if (Player.HasEnoughGoldToTrain(creepType) && trainingSite != null) {
            Console.WriteLine($"TRAIN {trainingSite.Id}".Trim());
        }
        else
            Console.WriteLine($"TRAIN");
    }
}

static class Table 
{
    public static int NumSites;
    public static int NumUnits;
    public static Dictionary<int, Site> Sites = new Dictionary<int, Site>();
    public static List<Site> EmptySites = null;
    public static List<Unit> EnemyUnits = null;
    public static Unit EnemyQueen = null;
    public static List<Site> EnemySites = null;

    public static double GetDistanceBetween(Piece p1, Piece p2) 
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    public static Piece GetClosestPieceFrom(Piece piece, IEnumerable<Piece> pieces) 
    {
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

    public static Piece GetFarthestPieceFrom(Piece piece, IEnumerable<Piece> pieces) 
    {
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

abstract class Piece 
{
    public int Id { get; set; }
    public abstract int X {get; set; }
    public abstract int Y {get; set; }
}

class Site : Piece 
{
    public override int X { get; set; }
    public override int Y { get; set; }
    public int Radius { get; set; } 
    public StructureTypeEnum StructureType { get; set; }
    public int NextSpawn { get; set; }
    public bool? IsEnemy { get; set; }
    public CreepTypeEnum CreepType { get; set; }
    public int Gold { get; set; }
    public int MaxRate { get; set; }
    public int Param1 {get;set;}
    public int Param2 {get;set;}

    public override string ToString() {
        return $"[ Id = {Id} | Type = {StructureType} | Param1 = {Param1} | Param2 = {Param2}";
    }
}

class Unit : Piece 
{
    public override int X { get; set; }
    public override int Y { get; set; }
    public int HealthPoints { get; set; }
    public UnitTypeEnum UnitType { get; set; }
}

class Pair<T1, T2>
{
    public T1 Key { get; set; }
    public T2 Value { get; set; }

    public Pair(T1 key, T2 value) {
        Key = key; Value = value;
    }

    public override string ToString() {
        return $"[ Key = {Key} | Value = {Value} ]";
    }
}

enum StructureTypeEnum 
{
    None = -1,
    Mine = 0,
    Tower = 1,
    Barracks = 2
}

enum OwnerEnum 
{
    None = -1,
    Friendly = 0,
    Enemy = 1
}

enum CreepTypeEnum 
{
    None = -1,
    Knight = 0,
    Archer = 1
}

enum UnitTypeEnum 
{
    Queen = -1,
    Knight = 0,
    Archer = 1,
    Giant = 2
}

enum PlayerStateEnum
{
    BuildingBarrack,
    BuildingMine,
    BuildingTower,
    Running
}