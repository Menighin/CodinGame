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

	public override string ToString() => $"{X} {Y}";
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

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine("WAIT");
        }
    }
}