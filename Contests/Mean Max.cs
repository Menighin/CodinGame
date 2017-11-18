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
class Player
{

    public enum UnitType
    {
        Reaper = 0,
        Wreck = 4
    }

    static void Main(string[] args)
    {

        var players = new Dictionary<int, Unit>();
        players[0] = new Unit() { PlayerId = 0 };
        players[1] = new Unit() { PlayerId = 1 };
        players[2] = new Unit() { PlayerId = 2 };

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
            
            var wrecks = new List<Unit>();

            for (int i = 0; i < unitCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int player = int.Parse(inputs[2]);
                int type = int.Parse(inputs[1]);

                if (type == (int) UnitType.Reaper) {
                    players[player].Id = int.Parse(inputs[0]);
                    players[player].Mass = float.Parse(inputs[3]);
                    players[player].Radius = int.Parse(inputs[4]);
                    players[player].Position.X = int.Parse(inputs[5]);
                    players[player].Position.Y = int.Parse(inputs[6]);
                    players[player].Speed.X = int.Parse(inputs[7]);
                    players[player].Speed.Y = int.Parse(inputs[8]);
                    players[player].Water = int.Parse(inputs[9]);
                    players[player].Extra2 = int.Parse(inputs[10]);
                } else {
                    var wreck = new Unit();
                    wreck.Position.X = int.Parse(inputs[5]);
                    wreck.Position.Y = int.Parse(inputs[6]);
                    wreck.Water = int.Parse(inputs[9]);

                    wrecks.Add(wreck);
                }
            }

            // Find closest wreck
            var closestWreck = new Point();
            var distance = Int32.MaxValue;
            var playerPos = players[0].Position;
            foreach(var w in wrecks) {
                var distToWreck = (int)Math.Sqrt(Math.Pow(w.Position.X - playerPos.X, 2) + Math.Pow(w.Position.Y - playerPos.Y, 2));
                Console.Error.WriteLine(distToWreck);
                if (distToWreck < distance) {
                    distance = distToWreck;
                    closestWreck = w.Position;
                }
            }


            // To debug: Console.Error.WriteLine("Debug messages...");
            if (distance == Int32.MaxValue)
                Console.WriteLine("WAIT");
            else
                Console.WriteLine($"{closestWreck.X} {closestWreck.Y} 200");
                
            Console.WriteLine("WAIT");
            Console.WriteLine("WAIT");
        }
    }
    
    class Unit {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int Rage { get; set; }
        public float Mass { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }
        public Point Speed { get; set; }
        public int Water { get; set; }
        public int Extra2 { get; set; }
        
        public Unit() {
            this.Position = new Point();
            this.Speed = new Point();
        }
    }
    
    class Point {
        public int X { get; set; }
        public int Y { get; set; }
    }
        
    
}