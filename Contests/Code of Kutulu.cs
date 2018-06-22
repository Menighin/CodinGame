using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Survive the wrath of Kutulu
 * Coded fearlessly by JohnnyYuge & nmahoude (ok we might have been a bit scared by the old god...but don't say anything)
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        Game.Width = int.Parse(Console.ReadLine());
        Game.Height = int.Parse(Console.ReadLine());
        for (int i = 0; i < Game.Height; i++)
        {
            Game.Map.Add(Console.ReadLine());
        }

        inputs = Console.ReadLine().Split(' ');
        int sanityLossLonely = int.Parse(inputs[0]); // how much sanity you lose every turn when alone, always 3 until wood 1
        int sanityLossGroup = int.Parse(inputs[1]); // how much sanity you lose every turn when near another player, always 1 until wood 1
        int wandererSpawnTime = int.Parse(inputs[2]); // how many turns the wanderer take to spawn, always 3 until wood 1
        int wandererLifeTime = int.Parse(inputs[3]); // how many turns the wanderer is on map after spawning, always 40 until wood 1

        // game loop
        while (true)
        {

            Game.Explorers = new List<Explorer>();
            Game.Wanderers = new List<Wanderer>();

            int entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var entityType = EntityTypeEnum.GetEnumFrom(inputs[0]);
                int id = int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int param0 = int.Parse(inputs[4]);
                int param1 = int.Parse(inputs[5]);
                int param2 = int.Parse(inputs[6]);

                if (i == 0) // That's the player
                {
                    Game.Player = new Explorer()
                    {
                        Id = id,
                        Position = new Point() { X = x, Y = y },
                        EntityType = EntityTypeEnum.Explorer,
                        Sanity = param0
                    };
                }
                else if (entityType == EntityTypeEnum.Explorer)
                {
                    Game.Explorers.Add(new Explorer() {
                        Id = id,
                        Position = new Point() { X = x, Y = y },
                        EntityType = EntityTypeEnum.Explorer,
                        Sanity = param0
                    });
                }
                else if (entityType == EntityTypeEnum.Wanderer)
                {
                    Game.Wanderers.Add(new Wanderer() {
                        Id = id,
                        Position = new Point() { X = x, Y = y },
                        EntityType = EntityTypeEnum.Wanderer,
                        Time = param0,
                        Status = (WandererStatusEnum) param1,
                        Target = param2
                    });
                }
                else if (entityType == EntityTypeEnum.Slasher)
                {
                    Game.Slashers.Add(new Slasher() {
                        Id = id,
                        Position = new Point() { X = x, Y = y },
                        EntityType = EntityTypeEnum.Wanderer,
                        Time = param0,
                        Status = (WandererStatusEnum) param1,
                        Target = param2
                    });
                }
            }

            // Get the closest wanderer to the player
            var closestWanderer = Game.Player.GetClosestWanderer(Game.Wanderers);
            var distToClosest = Double.MaxValue;
            
            if (closestWanderer != null) {
                distToClosest = Game.GetDistanceBetween(Game.Player.Position, closestWanderer.Position);
            }

            if (distToClosest > 2) // If no wanderer is close, wait for death
            {
                Console.WriteLine("WAIT WAITING FOR FEAR TO FIND ME");
            }
            else // If there is a wanderer close enough, move away from it
            {
                // Get the valid positions
                var validMovePositions = Game.GetValidMovePositionsAround(Game.Player.Position);

                // Move to the one that gets the player farther from the wanderer
                var position = Game.Player.Position;

                foreach (var p in validMovePositions)
                {
                    var dist = Game.GetDistanceBetween(closestWanderer.Position, p);
                    if (dist > distToClosest)
                        position = p;
                }

                Console.WriteLine($"MOVE {position.X} {position.Y} GET AWAY FROM ME!");
            }

        }
    }

    static class Game
    {
        public static int Width;
        public static int Height;
        public static List<string> Map = new List<string>();

        public static List<Explorer> Explorers;
        public static Explorer Player;
        public static List<Wanderer> Wanderers;
        public static List<Slasher> Slashers;

        public static double GetDistanceBetween(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public static List<Point> GetValidMovePositionsAround(Point p)
        {
            var result = new List<Point>();

            // Check up
            if (p.Y > 0 && Map[p.Y - 1][p.X] != '#')
                result.Add(new Point(){ X = p.X, Y = p.Y - 1});
            
            // Check down
            if (p.Y < Height && Map[p.Y + 1][p.X] != '#')
                result.Add(new Point(){ X = p.X, Y = p.Y + 1});

            // Check left
            if (p.X > 0 && Map[p.Y][p.X - 1] != '#')
                result.Add(new Point(){ X = p.X - 1, Y = p.Y});

            // Check right
            if (p.X < Width && Map[p.Y][p.X + 1] != '#')
                result.Add(new Point(){ X = p.X + 1, Y = p.Y});

            return result;
        }
    }

    class Point 
    {
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString() 
        {
            return $"({X}, {Y})";
        }
    }

    abstract class Entity 
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public EntityTypeEnum EntityType { get; set; }
    }

    class Explorer : Entity
    {
        public int Sanity { get; set; }

        public Wanderer GetClosestWanderer(List<Wanderer> wanderers)
        {
            var minDist = Double.MaxValue;
            Wanderer wanderer = null;
            foreach (var w in wanderers)
            {
                var dist = Game.GetDistanceBetween(this.Position, w.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    wanderer = w;
                }
            }

            return wanderer;
        }
    }

    class Wanderer : Entity
    {
        public WandererStatusEnum Status { get; set; }
        public int Time { get; set; }
        public int Target { get; set; }
    }

    class Slasher : Entity
    {
        public WandererStatusEnum Status { get; set; }
        public int Time { get; set; }
        public int Target { get; set; }
    }


    class EntityTypeEnum 
    {
        private static List<EntityTypeEnum> EnumList = new List<EntityTypeEnum>();

        public static EntityTypeEnum Explorer = new EntityTypeEnum("Explorer");
        public static EntityTypeEnum Wanderer = new EntityTypeEnum("Wanderer");
        public static EntityTypeEnum Slasher = new EntityTypeEnum("Slasher");
        public static EntityTypeEnum EffectPlan  = new EntityTypeEnum("Effect_Plan");
        public static EntityTypeEnum EffectLight = new EntityTypeEnum("Effect_Light");
        public static EntityTypeEnum EffectShelter = new EntityTypeEnum("Effect_Shelter");
        public static EntityTypeEnum EffectYell = new EntityTypeEnum("Effect_Yell");

        public string Value { get; set; }

        private EntityTypeEnum(string value) 
        {
            Value = value;
            EnumList.Add(this);
        }

        public static EntityTypeEnum GetEnumFrom(string value)
        {
            return EnumList.FirstOrDefault(o => o?.Value?.ToLower() == value?.ToLower());
        }

        public static bool operator== (EntityTypeEnum obj1, EntityTypeEnum obj2)
        {
            return obj1?.Value == obj2?.Value;
        }

        public static bool operator!= (EntityTypeEnum obj1, EntityTypeEnum obj2)
        {
            return obj1?.Value != obj2?.Value;
        }

        public override bool Equals(object _)
        {
            var obj = (EntityTypeEnum) _;
            return this.Value == obj?.Value;
        }

        public override int GetHashCode() 
        {
            return this.Value.GetHashCode();
        }

    }

    enum WandererStatusEnum
    {
        Spawning = 0,
        Wandering = 1,
        Stalking = 2,
        Rushing = 3,
        Sunned = 4
    }

}