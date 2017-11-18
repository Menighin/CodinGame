using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Game
{

    public enum UnitType
    {
        Reaper = 0,
        Destroyer = 1,
        Tanker = 3,
        Wreck = 4
    }

    public static Dictionary<int, Player> players = new Dictionary<int, Player>();
    public static List<Wreck> wrecks;
    public static List<Tanker> tankers;

    public static void Main(string[] args)
    {

        players[0] = new Player() { PlayerId = 0 };
        players[1] = new Player() { PlayerId = 1 };
        players[2] = new Player() { PlayerId = 2 };
        
        // game loop
        while (true)
        {
            players[0].Score = int.Parse(Console.ReadLine());
            players[1].Score = int.Parse(Console.ReadLine());
            players[2].Score = int.Parse(Console.ReadLine());
            
            players[0].Rage  = int.Parse(Console.ReadLine());
            players[1].Rage  = int.Parse(Console.ReadLine());
            players[2].Rage  = int.Parse(Console.ReadLine());
            
            int unitCount = int.Parse(Console.ReadLine());
            
            wrecks = new List<Wreck>();
            tankers = new List<Tanker>();

            for (int i = 0; i < unitCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int player = int.Parse(inputs[2]);
                int type = int.Parse(inputs[1]);
                int id = int.Parse(inputs[0]);
                float mass = float.Parse(inputs[3]);
                int radius = int.Parse(inputs[4]);
                int x = int.Parse(inputs[5]);
                int y = int.Parse(inputs[6]);
                int vx = int.Parse(inputs[7]);
                int vy = int.Parse(inputs[8]);
                int extra1 = int.Parse(inputs[9]);
                int extra2 = int.Parse(inputs[10]);

                if (type == (int) UnitType.Reaper) {

                    players[player].Reaper.Id = id;
                    players[player].Reaper.Mass = mass;
                    players[player].Reaper.Radius = radius;
                    players[player].Reaper.Position.X = x;
                    players[player].Reaper.Position.Y = y;
                    players[player].Reaper.Speed.X = vx;
                    players[player].Reaper.Speed.Y = vy;

                } else if (type == (int) UnitType.Destroyer) {

                    players[player].Destroyer.Id = id;
                    players[player].Destroyer.Mass = mass;
                    players[player].Destroyer.Radius = radius;
                    players[player].Destroyer.Position.X = x;
                    players[player].Destroyer.Position.Y = y;
                    players[player].Destroyer.Speed.X = vx;
                    players[player].Destroyer.Speed.Y = vy;

                } else if (type == (int) UnitType.Tanker) {

                    var tanker = new Tanker();
                    tanker.Id = id;
                    tanker.Position.X = x;
                    tanker.Position.Y = y;
                    tanker.Speed.X = vx;
                    tanker.Speed.Y = vy;
                    tanker.Radius = radius;
                    tanker.Water = extra1;
                    tanker.Capacity = extra2;
                    tanker.IsInCircle = IsInsideArea(tanker.Position);

                    tankers.Add(tanker);

                } else if (type == (int) UnitType.Wreck) {

                    var wreck = new Wreck();
                    wreck.Id = id;
                    wreck.Position.X = x;
                    wreck.Position.Y = y;
                    wreck.Radius = radius;
                    wreck.Water = extra1;

                    wrecks.Add(wreck);
                }
            }

            Console.Error.WriteLine($"Wreckes: {wrecks.Count()}");

            // Find closest wreck for reaper
            var closestWreck = FindClosestWreck();

            // Find the closest tanker with water
            var closestTanker = FindClosestTanker(1);

            // To debug: Console.Error.WriteLine("Debug messages...");
            if (closestWreck == null) {
                Console.WriteLine("WAIT");
            } else {
                Console.WriteLine($"{closestWreck.X} {closestWreck.Y} 300");
            }

            if (closestTanker == null) {
                Console.WriteLine("WAIT");
            } else {
                Console.WriteLine($"{closestTanker.X} {closestTanker.Y} 300");
            }

            Console.WriteLine("WAIT");
        }
    }
    

    public static Point FindClosestWreck() {
        Point closestWreck = null;
        var distance = 99999999.0;
        var playerPos = players[0].Reaper.Position;
        var wreckId = -1;
        foreach(var w in wrecks) {
            var distToWreck = CalculateDistance(w.Position, playerPos);
            if (distToWreck < distance) {
                distance = distToWreck;
                closestWreck = w.Position;
                wreckId = w.Id;
            }
        }
        
        Console.Error.WriteLine($"Reaper going to {wreckId}");
        return closestWreck;
    }

    public static Point FindClosestTanker(int water = 0) {
        Point closestTanker = null;
        double distance = 99999999.0;
        var playerPos = players[0].Destroyer.Position;
        var tankerId = -1;
        foreach(var t in tankers.Where(t => t.IsInCircle)) {
            var distToTanker = CalculateDistance(t.Position, playerPos);
            if (distToTanker < distance) {
                distance = distToTanker;
                closestTanker = t.Position;
                tankerId = t.Id;
            }
        }
        
        Console.Error.WriteLine($"Destroyer going to {tankerId}");
        return closestTanker;
    }

    public static bool IsInsideArea(Point p) {
        var center = new Point() { X = 0, Y = 0};
        return CalculateDistance(p, center) < 6000;
    }

    public static double CalculateDistance(Point p1, Point p2) {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    public class Reaper {
        public int Id { get; set; }
        public float Mass { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public Point Speed { get; set; }

        public Reaper() {
            this.Position = new Point();
            this.Speed = new Point();
        }
    }

    public class Destroyer {
        public int Id { get; set; }
        public float Mass { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public Point Speed { get; set; }

        public Destroyer() {
            this.Position = new Point();
            this.Speed = new Point();
        }
    }

    public class Player {
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int Rage { get; set; }
        public int Extra2 { get; set; }
        public Reaper Reaper { get; set; }
        public Destroyer Destroyer {get; set;}

        public Player() {
            this.Reaper = new Reaper();
            this.Destroyer = new Destroyer();
        }
    }

    public class Wreck {
        public int Id { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public int Water { get; set; }

        public Wreck() {
            this.Position = new Point();
        }
    }

    public class Tanker {
        public int Id { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public Point Speed { get; set; }
        public int Water { get; set; }
        public int Capacity { get; set; }
        public bool IsInCircle { get; set; }

        public Tanker() {
            this.Position = new Point();
            this.Speed = new Point();
        }
    }
    
    public class Point {
        public int X { get; set; }
        public int Y { get; set; }
    }
}