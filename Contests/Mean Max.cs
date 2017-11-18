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
        Doof = 2,
        Tanker = 3,
        Wreck = 4
    }

    public static Dictionary<int, Player> players = new Dictionary<int, Player>();
    public static Dictionary<int, Wreck> wrecks;
    public static Dictionary<int, Tanker> tankers;

    public static int _SKILL_INTERVAL = 5;

    public static void Main(string[] args)
    {

        players[0] = new Player() { PlayerId = 0 };
        players[1] = new Player() { PlayerId = 1 };
        players[2] = new Player() { PlayerId = 2 };
        
        // game loop
        while (true)
        {
            _SKILL_INTERVAL--;
            players[0].Score = int.Parse(Console.ReadLine());
            players[1].Score = int.Parse(Console.ReadLine());
            players[2].Score = int.Parse(Console.ReadLine());
            
            players[0].Rage  = int.Parse(Console.ReadLine());
            players[1].Rage  = int.Parse(Console.ReadLine());
            players[2].Rage  = int.Parse(Console.ReadLine());
            
            int unitCount = int.Parse(Console.ReadLine());
            
            wrecks = new Dictionary<int, Wreck>();
            tankers = new Dictionary<int, Tanker>();

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

                } else if (type == (int) UnitType.Doof) {

                    players[player].Doof.Id = id;
                    players[player].Doof.Mass = mass;
                    players[player].Doof.Radius = radius;
                    players[player].Doof.Position.X = x;
                    players[player].Doof.Position.Y = y;
                    players[player].Doof.Speed.X = vx;
                    players[player].Doof.Speed.Y = vy;

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

                    tankers[id] = tanker;

                } else if (type == (int) UnitType.Wreck) {

                    var wreck = new Wreck();
                    wreck.Id = id;
                    wreck.Position.X = x;
                    wreck.Position.Y = y;
                    wreck.Radius = radius;
                    wreck.Water = extra1;

                    wrecks[id] = wreck;
                }
            }

            Console.Error.WriteLine($"Wreckes: {wrecks.Count()}");

            // Find closest wreck for reaper
            var bestWreck = FindBestWreck();

            // Find the closest tanker with water
            var closestTanker = FindClosestTanker(1);

            // Find a wreck to bomb
            var wreckerToBomb = FindWreckToBomb();

            // Find a good point to oil
            var pointToOil = FindPointToOil();

            // To debug: Console.Error.WriteLine("Debug messages...");
            if (bestWreck == null) {
                Console.WriteLine("WAIT");
            } else {
                var dist = (int) (CalculateDistance(bestWreck.Position, players[0].Reaper.Position));
                var speed = dist / 4;
                if (dist < bestWreck.Radius) speed = 10;
                Console.WriteLine($"{bestWreck.Position.X} {bestWreck.Position.Y} {speed} {speed}");
            }

            if (wreckerToBomb != null && players[0].Rage > 60 && _SKILL_INTERVAL <= 0) {
                Console.WriteLine($"SKILL {wreckerToBomb.X} {wreckerToBomb.Y} BOMB");
                _SKILL_INTERVAL = 5;
            }
            else if (closestTanker == null) {
                Console.WriteLine("WAIT");
            } 
            else {
                Console.WriteLine($"{closestTanker.Position.X} {closestTanker.Position.Y} 500");
            }

            // Doof follows enemy because why not
            if (pointToOil != null && players[0].Rage >= 30 && _SKILL_INTERVAL <= 0) {
                Console.WriteLine($"SKILL {pointToOil.X} {pointToOil.Y} DRIFT");
                _SKILL_INTERVAL = 5;
            }
            else
                Console.WriteLine($"{players[1].Reaper.Position.X} {players[1].Reaper.Position.Y} 500 GET BACK HERE YOU");
                
        }
    }

    public static Wreck FindBestWreck() {
        var reaper = players[0].Reaper;
        var bestWreck = IsAtWreck();
        if (bestWreck != null) return bestWreck;
        
        foreach (var w1 in wrecks.Values) {
            var waterBank = w1.Water;

            foreach (var w2 in wrecks.Values) {
                if (w2.Id == w1.Id) continue;

                if (CalculateDistance(w1.Position, w2.Position) < 700)
                    waterBank += w2.Water;
            }

            var distance = CalculateDistance(reaper.Position, w1.Position);
            w1.Points = waterBank / distance;
        }

        bestWreck = wrecks.Values.OrderByDescending(w => w.Points).FirstOrDefault();

        Console.Error.WriteLine($"Reaper going to {bestWreck?.Id}");
        return bestWreck;
    }

    public static Wreck IsAtWreck() {
        var reaper = players[0].Reaper;

        foreach (var w in wrecks.Values) {
            var distance = CalculateDistance(reaper.Position, w.Position);
            if (distance < w.Radius)
                return w;
        }

        return null;
    }

    public static Tanker FindClosestTanker(int water = 0) {
        double distance = 99999999.0;
        var playerPos = players[0].Destroyer.Position;
        int? tankerId = null;
        foreach(var t in tankers.Values.Where(t => t.IsInCircle)) {
            var distToTanker = CalculateDistance(t.Position, playerPos);
            if (distToTanker < distance) {
                distance = distToTanker;
                tankerId = t.Id;
            }
        }
        
        Console.Error.WriteLine($"Destroyer going to {tankerId}");
        return tankerId == null ? null : tankers[tankerId.Value];
    }

    public static bool IsInsideArea(Point p) {
        var center = new Point() { X = 0, Y = 0};
        return CalculateDistance(p, center) < 6000 - 500;
    }

    public static double CalculateDistance(Point p1, Point p2) {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    public static Point FindWreckToBomb() {
        Point wreckToBomb = null;
        var enemies = players.Values.Where(p => p.PlayerId != 0);
        var player = players[0];
        var closestToWreck = 9999999.0;

        // Bomb if enemy is close to win
        var winningEnemy = enemies.FirstOrDefault(e => e.Score > player.Score && e.Score > 40);
        if (winningEnemy != null && CalculateDistance(player.Destroyer.Position, winningEnemy.Destroyer.Position) < 2000)
            return winningEnemy.Reaper.Position;

        foreach (var w in wrecks.Values) {
            var distance = 0.0;
            foreach (var e in enemies) {
                distance = CalculateDistance(e.Reaper.Position, w.Position);
                if (distance < closestToWreck && CalculateDistance(player.Reaper.Position, w.Position) > 1000 && CalculateDistance(player.Destroyer.Position, e.Reaper.Position) < 2000) {
                    closestToWreck = distance;
                    wreckToBomb = e.Reaper.Position;
                }
            }
            
        }

        return closestToWreck < 700 ? wreckToBomb : null;
    }

    public static Point FindPointToOil() {
        var enemies = players.Values.Where(p => p.PlayerId != 0);
        var player = players[0];
        // Oil if enemy is close to win
        var winningEnemy = enemies.FirstOrDefault(e => e.Score > player.Score && e.Score > 35);
        
        var distance = 9000.0;
        if (winningEnemy != null) {
            distance = CalculateDistance(winningEnemy.Reaper.Position, player.Doof.Position);
        }
        
        if (winningEnemy != null && distance < 2000)
            return winningEnemy.Reaper.Position;
        return null;
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

    public class Doof {
        public int Id { get; set; }
        public float Mass { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public Point Speed { get; set; }

        public Doof() {
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
        public Destroyer Destroyer { get; set; }
        public Doof Doof { get; set; }

        public Player() {
            this.Reaper = new Reaper();
            this.Destroyer = new Destroyer();
            this.Doof = new Doof();
        }
    }

    public class Wreck {
        public int Id { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public int Water { get; set; }
        public double Points { get; set; }

        public Wreck() {
            this.Position = new Point();
            this.Points = 0.0;
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