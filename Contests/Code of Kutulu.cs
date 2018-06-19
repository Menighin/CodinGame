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
                        X = x,
                        Y = y,
                        EntityType = EntityTypeEnum.Explorer,
                        Sanity = param0
                    };
                }
                else if (entityType == EntityTypeEnum.Explorer)
                {
                    Game.Explorers.Add(new Explorer() {
                        Id = id,
                        X = x,
                        Y = y,
                        EntityType = EntityTypeEnum.Explorer,
                        Sanity = param0
                    });
                }
                else if (entityType == EntityTypeEnum.Wanderer)
                {
                    Game.Wanderers.Add(new Wanderer() {
                        Id = id,
                        X = x,
                        Y = y,
                        EntityType = EntityTypeEnum.Wanderer,
                        Time = param0,
                        Status = (WandererStatusEnum) param1,
                        Target = param2
                    });
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine("WAIT"); // MOVE <x> <y> | WAIT
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
    }

    abstract class Entity 
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public EntityTypeEnum EntityType { get; set; }
    }

    class Explorer : Entity
    {
        public int Sanity { get; set; }
    }

    class Wanderer : Entity
    {
        public int Sanity { get; set; }
        public WandererStatusEnum Status { get; set; }
        public int Time { get; set; }
        public int Target { get; set; }
    }

    class EntityTypeEnum 
    {
        private static List<EntityTypeEnum> EnumList = new List<EntityTypeEnum>();

        public static EntityTypeEnum Explorer = new EntityTypeEnum("Explorer");
        public static EntityTypeEnum Wanderer = new EntityTypeEnum("Wanderer");

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
        Wandering = 1
    }

}