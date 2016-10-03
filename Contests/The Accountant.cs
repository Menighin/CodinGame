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
        string[] inputs;

        // Game loop
        while (true)
        {

			var dataPoints = new List<DataPoint>();
			var enemies = new List<Enemy>();

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

				var enemy = new Enemy(enemyId, enemyX, enemyY, enemyLife); 
				enemies.Add(enemy);
            }

            Console.WriteLine(String.Format("SHOOT {0}", enemies.OrderBy(e => e.GetLifeLeftIfShootFrom(x, y)).First().Id)); // MOVE x y or SHOOT id
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

		public Enemy(int id, int x, int y, int life)
		{
			this.Id = id;
			this.X = x;
			this.Y = y;
			this.Life = life;
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