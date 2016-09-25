using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Game
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);
		Map map = new Map(height, width);

        // Game loop
        while (true)
        {
            // Read the map state
            map.ReadMap();
			
			var players = new Dictionary<int, Player>();
			var bombs = new Dictionary<int, Bomb>();
            
            // Read the entities
            int entities = int.Parse(Console.ReadLine());
            for (int i = 0; i < entities; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityType = int.Parse(inputs[0]);
                int owner = int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int param1 = int.Parse(inputs[4]);
                int param2 = int.Parse(inputs[5]);

				if (entityType == (int)EntityType.Player) 
					players[owner] = new Player(owner, param1, param2, x, y);
				else {
					bombs[owner] = new Bomb(owner, param1, param2, x, y);
					map.SetCell(x, y, 'b');
				}

            }
            
            // Process map
			//map.PrintMap();

			int maxScore = 0;
			int targetX = 0, targetY = 0;
			for (var i = 0; i < map.Height; i++)
			{
				for (var j = 0; j < map.Width; j++) {

					int cellScore = map.CalculateScore(j, i, players[myId].BombRadius);
					if (cellScore > maxScore) 
					{
						targetX = j;
						targetY = i;
						maxScore = cellScore;
					}
				}
			}
			
			if (players[myId].X == targetX && players[myId].Y == targetY)
            	Console.WriteLine("BOMB " + targetX + " " + targetY + " KABOOM!");
			else
            	Console.WriteLine("MOVE " + targetX + " " + targetY + " Going to: (" + targetX + ", " + targetY + ")");
				
        }
    }

	// Enum of EntityTypes
	public enum EntityType 
	{
		Player,
		Bomb
	};
    
    // Class to define and control the map state
    public class Map 
	{
        
        public List<StringBuilder> Grid {get; set;}
        public int Height {get; set;}
		public int Width {get; set;}

		public Map(int height, int width) 
		{
			this.Height = height;
			this.Width = width;
		}
        
        public void ReadMap() 
		{
            this.Grid = new List<StringBuilder>();

            for (int i = 0; i < this.Height; i++)
            {
				Grid.Add(new StringBuilder());
                Grid[i].Append(Console.ReadLine());
            }
        }
        
		public char GetCell(int x, int y) 
		{
			return Grid[y][x];
		}
		
		public void SetCell(int x, int y, char value) 
		{
		    Grid[y][x] = value;
		}

		public void PrintMap() 
		{
			Console.Error.WriteLine("\n");
			
			foreach(var l in this.Grid)
			{
				Console.Error.WriteLine(l);
			}

			Console.Error.WriteLine("\n");
		}

		public int CalculateScore (int x, int y, int radius) 
		{
			// If can't place a bomb, then it has no score
			if (Grid[y][x] != '.') return 0;

			int score = 0;
			bool checkUp = true, checkDown = true, checkRight = true, checkLeft = true;

			for (var i = 1; i <= radius; i++)
			{

				if (checkUp && y - i >= 0 && Grid[y - 1][x] == '0')
				{
					score++;
					checkUp = false;
				}

				if (checkDown && y + i < this.Height && Grid[y + 1][x] == '0')
				{
					score++;
					checkDown = false;
				}

				if (checkLeft && x - i >= 0 && Grid[y][x - i] == '0')
				{
					score++;
					checkLeft = false;
				}

				if (checkRight && x + i < this.Width && Grid[y][x + i] == '0')
				{
					score++;
					checkRight = false;
				}
			}

			return score; 

		}
    }

	// Class to represent a player
	public class Player 
	{

		public int Id {get; set;}
		public int BombsLeft {get; set;}
		public int BombRadius {get;set;}
		public int X {get; set;}
		public int Y {get; set;}

		public Player (int id, int bombs, int range, int x, int y) 
		{
			Id = id;
			BombsLeft = bombs;
			BombRadius = range;
			X = x;
			Y = y;
		}
	}

	// Class to represent a bomb
	public class Bomb {

		public int OwnerId {get; set;}
		public int RoundsLeft {get; set;}
		public int BombRadius {get;set;}
		public int X {get; set;}
		public int Y {get; set;}

		public Bomb (int ownerId, int roundsLeft, int range, int x, int y) 
		{
			OwnerId = ownerId;
			RoundsLeft = roundsLeft;
			BombRadius = range;
			X = x;
			Y = y;
		}
	}
}