using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static void Main(string[] args)
    {
        int myTeamId = int.Parse(Console.ReadLine()); // if 0 you need to score on the right of the map, if 1 you need to score on the left
		bool firstLoop = true;
		Point teamGoal = null;
		Point enemyGoal = null;

        // game loop
        while (true)
        {

			List<Entity> snaffles = new List<Entity>();
			List<Entity> players = new List<Entity>();
			List<Entity> enemies = new List<Entity>();

            int entities = int.Parse(Console.ReadLine()); // number of entities still in game
            // Reading entities
			for (int i = 0; i < entities; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]); // entity identifier
                string entityType = inputs[1]; // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" (or "BLUDGER" after first league)
                int x = int.Parse(inputs[2]); // position
                int y = int.Parse(inputs[3]); // position
                int vx = int.Parse(inputs[4]); // velocity
                int vy = int.Parse(inputs[5]); // velocity
                int state = int.Parse(inputs[6]); // 1 if the wizard is holding a Snaffle, 0 otherwise

				if (entityType == "WIZARD")
					players.Add(new Wizard(entityId, x, y, vx, vy, state));
				else if (entityType == "OPPONENT_WIZARD")
					enemies.Add(new Wizard(entityId, x, y, vx, vy, state));
				else if (entityType == "SNAFFLE")
					snaffles.Add(new Snaffle(entityId, x, y, vx, vy, state));

				// Getting the goals positions
				if (firstLoop && entityType == "WIZARD")
				{
					firstLoop = false;
					if (x < 8000) 
					{
						teamGoal = new Point(0, 3750);
						enemyGoal = new Point(16000, 3750);
					} else 
					{
						enemyGoal = new Point(0, 3750);
						teamGoal = new Point(16000, 3750);
					}
				}
            }

			foreach (Wizard w in players)
			{

				// Find the closest snaffle
				Snaffle closestSnaffle = null;
				double minDist = 99999999.9;
				foreach (Entity s in snaffles)
				{
					double dist = w.CalculateDistance(s);
					if (dist < minDist)
					{
						minDist = dist;
						closestSnaffle = (Snaffle) s;
					}
				}

				if (w.State == 0)
					Console.WriteLine($"MOVE {closestSnaffle.X} {closestSnaffle.Y} 150 PEGA");
				else
					Console.WriteLine($"THROW {enemyGoal.X} {enemyGoal.Y} 500 OLHA O GOL!");
			}
        }
    }


	class Entity {
		public int Id {get; set;}
		public int X {get; set;}
		public int Y {get; set;}
		public int VX {get; set;}
		public int VY {get; set;}
		public int State {get; set;}

		public Entity (int id, int x, int y, int vx, int vy, int state) {
			Id = id;
			X = x;
			Y = y;
			VX = vx;
			VY = vy;
			State = state;
		}

		public double CalculateDistance(Entity e)
		{
			return Math.Sqrt( Math.Pow(this.X - e.X, 2) + Math.Pow(this.Y - e.Y, 2) );
		}

	}

	class Snaffle : Entity {
		public Snaffle (int id, int x, int y, int vx, int vy, int state) : base(id, x, y, vx, vy, state) {

		}
	}

	class Wizard : Entity {
		public Wizard (int id, int x, int y, int vx, int vy, int state) : base(id, x, y, vx, vy, state) {

		}
	}

	class Point {
		public int X {get; set;}
		public int Y {get; set;}

		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

	}


}