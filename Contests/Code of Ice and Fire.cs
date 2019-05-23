using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Position 
{
	public int X { get ; set; }
	public int Y { get ; set; }

	public int ManhattanDistance(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);

	public override string ToString() => $"({X}, {Y})";

    public static bool operator ==(Position obj1, Position obj2) => obj1?.Equals(obj2) ?? ReferenceEquals(obj2, null);

    public static bool operator !=(Position obj1, Position obj2) => !(obj1 == obj2);

    public override bool Equals(object obj) => Equals((Position) obj);

    protected bool Equals(Position other)
    {
        if (other == null) return false;
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = (hash * 17) + X.GetHashCode();
            hash = (hash * 17) + Y.GetHashCode();
            return hash;
        }
    }
}

public class Unit
{
    public int UnitId { get; set; }
    public int Level { get; set; }
    public Position Position { get; set; }
}

public enum BuildingType
{
    Hq
}

public class Building
{
    public BuildingType BuildingType { get; set; }
    public Position Position { get; set; }
}

public class Player
{
    public int Gold { get; set; }
    public int Income { get; set; }
    public List<Unit> Units { get; set; } = new List<Unit>();
    public List<Building> Buildings { get; set; } = new List<Building>();
}

public class Game
{
    public char[,] Map { get; set; } = new char[12, 12];
    public Player Me { get; set; } = new Player();
    public Player Opponent { get; set; } = new Player();


    public List<string> GetCommands()
    {
        // PrintMap();
        var commands = new List<string>();
        SpawnUnits(commands);
        MoveUnits(commands);
        return commands;
    }

    private Dictionary<Position, int> CalculateValueMapFor(Position position)
    {
        var mapValue = new Dictionary<Position, int>();

        for (int i = 0; i < Map.GetLength(0); i++)
        {
            for (int j = 0; j < Map.GetLength(1); j++)
            {
                var cell = Map[i, j];
                var cellPosition = new Position { X = j, Y = i };

                switch(cell)
                {
                    case '#':
                        mapValue[cellPosition] = 0;
                        break;
                    case '.':
                        mapValue[cellPosition] = 1000 - position.ManhattanDistance(cellPosition);
                        break;
                    case 'o':
                    case 'O':
                        mapValue[cellPosition] = 1;
                        break;
                    case 'x':
                    case 'X':
                        mapValue[cellPosition] = 1000 - position.ManhattanDistance(cellPosition);
                        break;
                }
            }
        }
        return mapValue;
    }

    private HashSet<Position> GetPossibleSpawnPositions() 
    {
        var result = new HashSet<Position>();

        for (int i = 0; i < Map.GetLength(0); i++)
        for (int j = 0; j < Map.GetLength(1); j++)
        {
            var cell = Map[i, j];
            if (cell == 'o' || cell == 'O')
            {
                result.Add(new Position {X = j, Y = i});

                var neighbors = new List<(char? Cell, Position Pos)>();
                neighbors.Add((i > 0 ? Map[i - 1, j] : (char?) null, new Position { X = j, Y = i - 1 } ));
                neighbors.Add((j > 0 ? Map[i, j - 1] : (char?) null, new Position { X = j - 1, Y = i } ));
                neighbors.Add((i < Map.GetLength(0) - 1 ? Map[i + 1, j] : (char?) null, new Position { X = j, Y = i + 1 } ));
                neighbors.Add((j < Map.GetLength(1) - 1 ? Map[i, j + 1] : (char?) null, new Position { X = j + 1, Y = i } ));

                foreach (var n in neighbors)
                {
                    if (n.Cell != null && n.Cell != '#')
                        result.Add(n.Pos);
                }
            }
        }

        return result;
    }

    private void SpawnUnits(List<string> commands)
    {
        var mapValue = CalculateValueMapFor(Me.Buildings.First(o => o.BuildingType == BuildingType.Hq).Position);

        var canSpawnPositions = GetPossibleSpawnPositions();

        var enemiesToDefeat = canSpawnPositions.Intersect(Opponent.Units.Select(o => o.Position)).ToHashSet();

        if (enemiesToDefeat.Count > 0) 
        {
            foreach (var enemyPos in enemiesToDefeat)
            {
                var enemyAtPos = Opponent.Units.First(o => o.Position == enemyPos);
                if (enemyAtPos.Level == 1)
                {
                    commands.Add($"TRAIN 2 {enemyPos.X} {enemyPos.Y}");
                    Me.Gold -= 20;
                }
                else 
                {
                    commands.Add($"TRAIN 3 {enemyPos.X} {enemyPos.Y}");
                    Me.Gold -= 30;
                }

                if (Me.Gold < 20) break;
            }
        }
        else if (Me.Gold > 10)
        {
            var occupiedPositions = Me.Units.Select(o => o.Position).ToHashSet();

            var highestAvailableValue = mapValue
                .OrderByDescending(o => o.Value)
                .First(o => !occupiedPositions.Contains(o.Key));
            commands.Add($"TRAIN 1 {highestAvailableValue.Key.X} {highestAvailableValue.Key.Y}");
        }
    }

    private void MoveUnits(List<string> commands)
    {
        var usedPosition = new HashSet<Position>();
        foreach (var u in Me.Units)
        {
            var mapValue = CalculateValueMapFor(u.Position);

            // PrintMapValue($"Unit {u.UnitId}", mapValue);

            var occupiedPositions = Me.Units.Select(o => o.Position).ToHashSet();

            var highestAvailableValue = mapValue
                .OrderByDescending(o => o.Value)
                .First(o => !occupiedPositions.Contains(o.Key));

            commands.Add($"MOVE {u.UnitId} {highestAvailableValue.Key.X} {highestAvailableValue.Key.Y}");
        }
    }

    private void PrintMapValue(string id, Dictionary<Position, int> mapValue)
    {
        Console.Error.WriteLine($"\nMAP OF {id} - {mapValue.Values.Count}");
        var listMapValue = mapValue.Values.ToList();
        for (int i = 0, k = 0; i < 12; i++)
        {
            var line = "";
            for (var j = 0; j < 12; j++, k++)
            {
                line += $"{listMapValue[k]}".PadLeft(5) + " ";
            }
            Console.Error.WriteLine(line);
        }
    }

    private void PrintMap()
    {
        Console.Error.WriteLine($"\nMAP");
        for (int i = 0; i < Map.GetLength(0); i++)
        {
            var line = "";
            for (int j = 0; j < Map.GetLength(1); j++)
            {
                line += $"{Map[i, j]}".PadLeft(5) + " ";
            }
            Console.Error.WriteLine(line);
        }
    }
}

public class CodeOfIceAndFire
{

    static void Main(string[] args)
    {
        string[] inputs;
        int numberMineSpots = int.Parse(Console.ReadLine());
        for (int i = 0; i < numberMineSpots; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
        }

        // game loop
        while (true)
        {
            var turn = new Game();

            #region Reading input
            
            turn.Me.Gold = int.Parse(Console.ReadLine());
            turn.Me.Income = int.Parse(Console.ReadLine());
            turn.Opponent.Gold = int.Parse(Console.ReadLine());
            turn.Opponent.Income = int.Parse(Console.ReadLine());

            for (int i = 0; i < 12; i++)
            {
                string line = Console.ReadLine();
                for (var j = 0; j < line.Length; j++)
                {
                    turn.Map[i, j] = line[j];
                }
            }

            int buildingCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < buildingCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var owner = int.Parse(inputs[0]) == 0 ? turn.Me : turn.Opponent;

                owner.Buildings.Add(new Building()
                {
                    BuildingType = (BuildingType) int.Parse(inputs[1]),
                    Position = new Position()
                    {
                        X = int.Parse(inputs[2]),
                        Y = int.Parse(inputs[3])
                    }
                });
            }

            int unitCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < unitCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var owner = int.Parse(inputs[0]) == 0 ? turn.Me : turn.Opponent;

                owner.Units.Add(new Unit()
                {
                    UnitId = int.Parse(inputs[1]),
                    Level = int.Parse(inputs[2]),
                    Position = new Position()
                    {
                        X = int.Parse(inputs[3]),
                        Y = int.Parse(inputs[4])
                    }
                });
            }
            #endregion


            var commands = turn.GetCommands();
            commands.Add("MSG Boi, that was a bad ending...");
            Console.WriteLine(string.Join("; ", commands));
        }
    }
}