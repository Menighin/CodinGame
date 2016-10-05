using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
	public enum DebugLevel {
		None,
		Macro,
		Micro,
		Detailed
	};

	private static DebugLevel _debugLevel = DebugLevel.None;

    static void Main(string[] args)
    {
        string[] inputs;

        // Game loop
        while (true)
        {

			var dataPoints = new List<DataPoint>();
			var enemies = new List<Enemy>();
			var isWolffInDanger = false;

            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int dataCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < dataCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int dataId = int.Parse(inputs[0]);
                int dataX = int.Parse(inputs[1]);
                int dataY = int.Parse(inputs[2]);

				dataPoints.Add(new DataPoint(dataId, dataX, dataY));
            }
            int enemyCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < enemyCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int enemyId = int.Parse(inputs[0]);
                int enemyX = int.Parse(inputs[1]);
                int enemyY = int.Parse(inputs[2]);
                int enemyLife = int.Parse(inputs[3]);

				var enemy = new Enemy(enemyId, enemyX, enemyY, enemyLife, dataPoints);
				
				if (enemy.GetDistanceFrom(x, y) <= 2500) {
					isWolffInDanger = true;
					if (_debugLevel >= DebugLevel.Macro) Console.Error.WriteLine($"Wolff is in danger by enemy {enemy.Id}");
				}

				enemies.Add(enemy);
            }

			// If Wolff is in danger, move away from enemies
			if (isWolffInDanger)
			{
				var validMoves = new List<Tuple<int, int>>() {
					new Tuple<int, int>(x + 1000, y),
					new Tuple<int, int>(x + 1000, y + 1000),
					new Tuple<int, int>(x, y + 1000),
					new Tuple<int, int>(x - 1000, y + 1000),
					new Tuple<int, int>(x - 1000, y),
					new Tuple<int, int>(x - 1000, y - 1000),
					new Tuple<int, int>(x, y - 1000),
					new Tuple<int, int>(x + 1000, y - 1000)
				};

				// Finding the best move
				double maxScore = int.MinValue;
				Tuple<int, int> moveWolff = null;

				foreach (var move in validMoves)
				{
					var score = 0.0;
					var suicideMove = false;
					foreach (var enemy in enemies)
					{
						var dist = enemy.GetDistanceFrom(move.Item1, move.Item2);
						if (dist < 2500) { suicideMove = true; break; }
						score += dist;
					}

					if (_debugLevel >= DebugLevel.Micro) Console.Error.WriteLine($"Move: ({move.Item1}, {move.Item2}) | Score: {score}");

					if (score > maxScore && !suicideMove) 
					{
						maxScore = score;
						moveWolff = move;
					}
				}

				// If there isnt a valid move to get away, try to kill the closest enemy
				if (moveWolff == null)
				{
            		Console.WriteLine($"SHOOT {enemies.OrderBy(e => e.GetDistanceFrom(x, y)).First().Id} Get away from me!");
				}
				else
				{
					Console.WriteLine($"MOVE {moveWolff.Item1} {moveWolff.Item2} Leave me alone!");
				}

			}
			else
			{
            	Console.WriteLine($"SHOOT {enemies.OrderBy(e => e.GetLifeLeftIfShootFrom(x, y)).First().Id} Shoot that moth****cker!");
			}
        }
    }

	public class DataPoint 
	{
		public int Id {get; set; }
		public int X {get; set; }
		public int Y {get; set; }

		public DataPoint(int id, int x, int y)
		{
			this.Id = id;
			this.X = x;
			this.Y = y;
		}
	}

	public class Enemy 
	{
		public int Id {get; set;}
		public int X {get; set; }
		public int Y {get; set; }
		public int Life {get; set; }
		public DataPoint ClosestDatapoint {get; set; }

		public Enemy(int id, int x, int y, int life, IEnumerable<DataPoint> dataPoints)
		{
			this.Id = id;
			this.X = x;
			this.Y = y;
			this.Life = life;

			var minDist = int.MaxValue + 0.0;
			foreach (var d in dataPoints)
			{
				var dist = this.GetDistanceFrom(d.X, d.Y);
				if (dist < minDist)
				{
					minDist = dist;
					this.ClosestDatapoint = d;
				}
			}
		}

		public double GetDistanceFrom(int x, int y)
		{
			return Math.Sqrt(Math.Pow(x - this.X, 2) + Math.Pow(y - this.Y, 2));
		}

		public int GetLifeLeftIfShootFrom(int x, int y)
		{
			double dist = this.GetDistanceFrom(x, y);

			int damageDealt = Convert.ToInt32(Math.Round(125000/Math.Pow(dist, 1.2)));

			return this.Life - damageDealt;
		}
	}
}