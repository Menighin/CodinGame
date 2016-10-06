using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Game
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
		List<DataPoint> dataPoints = null;
		List<Enemy> enemies = null;
		Dictionary<int, Enemy> enemiesPastState = null;

        // Game loop
        while (true)
        {

			// Getting the past state of enemies
			enemiesPastState = new Dictionary<int, Enemy>();
			if (enemies != null)
			{
				foreach (var e in enemies)
				{
					enemiesPastState[e.Id] = new Enemy(e);
				}
			}

			// Reinitializing enemies
			dataPoints = new List<DataPoint>();
			enemies = new List<Enemy>();
			var isWolffInDanger = false;

            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);

			var player = new Player("Wulff", x, y);

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

				Enemy pastEnemy = null;
				if (enemiesPastState != null) enemiesPastState.TryGetValue(enemyId, out pastEnemy);
				var enemy = new Enemy(enemyId, enemyX, enemyY, enemyLife, dataPoints, pastEnemy, player);
				
				if (enemy.GetDistanceFrom(x, y) <= 2500) {
					isWolffInDanger = true;
					if (_debugLevel >= DebugLevel.Macro) Console.Error.WriteLine($"Wolff is in danger by enemy {enemy.Id}");
				}

				enemies.Add(enemy);
            }

			// If Wolff is in danger, move away from enemies
			if (isWolffInDanger)
			{
				int minX = (x - 1000 < 0 ? 0 : x - 1000);
				int minY = (y - 1000 < 0 ? 0 : y - 1000);
				int maxX = (x + 1000 > 16000 ? 16000 : x + 1000);
				int maxY = (y + 1000 > 9000 ? 9000 : y + 1000);

				var validMoves = new List<Tuple<int, int>>() {
					new Tuple<int, int>(maxX, y),
					new Tuple<int, int>(maxX, maxY),
					new Tuple<int, int>(x, maxY),
					new Tuple<int, int>(minX, maxY),
					new Tuple<int, int>(minX, y),
					new Tuple<int, int>(minX, minY),
					new Tuple<int, int>(x, minY),
					new Tuple<int, int>(maxX, minY)
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
            		Console.WriteLine($"SHOOT {enemies.OrderBy(e => e.GetDistanceFrom(player.X, player.Y)).First().Id} Get away from me!");
				}
				else
				{
					Console.WriteLine($"MOVE {moveWolff.Item1} {moveWolff.Item2} Leave me alone!");
				}

			}
			else
			{
            	Console.WriteLine($"SHOOT {enemies.OrderBy(e => e.GetLifeLeftIfShootFrom(player.X, player.Y)).First().Id} Shoot that moth****cker!");
			}
        }
    }

	public class Player
	{
		public String Name {get; set; }
		public int X {get; set; }
		public int Y {get; set; }

		public Player(String name, int x, int y)
		{
			Name = name;
			X = x;
			Y = y;
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

		public DataPoint(DataPoint dp)
		{
			this.Id = dp.Id;
			this.X = dp.X;
			this.Y = dp.Y;
		}
	}

	public class Enemy 
	{
		public int Id {get; set;}
		public int X {get; set; }
		public int Y {get; set; }
		public int Life {get; set; }
		public DataPoint ClosestDatapoint {get; set; }
		public bool IsGettingCloser{get; set;}

		public Enemy(int id, int x, int y, int life, IEnumerable<DataPoint> dataPoints, Enemy pastState, Player p)
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

			IsGettingCloser = pastState != null && this.GetDistanceFrom(p.X, p.Y) < pastState.GetDistanceFrom(p.X, p.Y);
		}

		public Enemy(Enemy e)
		{
			this.Id = e.Id;
			this.X = e.X;
			this.Y = e.Y;
			this.Life = e.Life;
			this.IsGettingCloser = e.IsGettingCloser;
			this.ClosestDatapoint = new DataPoint(e.ClosestDatapoint);
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