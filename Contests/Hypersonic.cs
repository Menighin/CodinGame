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
                int param1 = int.Parse(inputs[4]); // RoundsLeft
                int param2 = int.Parse(inputs[5]); // Range

				if (entityType == (int)EntityType.Player) 
					players[owner] = new Player(owner, param1, param2, x, y);
				else if (entityType == (int)EntityType.Bomb)
				{
					Bomb b = new Bomb(owner, param1, param2, x, y);
					map.MarkDeadBoxes(b);
					map.MarkDangerousPaths(b);
					bombs[owner] = b;
					map.SetCell(x, y, 'b');
				}

            }
            
            // Process map
			//map.PrintMap();
			int maxScore = 0;
			int targetX = 0, targetY = 0;

			// Using a BFS search from players position to find the better place he can put the bomb
			Queue queue = new Queue();
			queue.Enqueue(new Tuple<int, int>(players[myId].X, players[myId].Y)); // Enqueue the player position

			while (queue.Count > 0) {

				Tuple<int, int> pos = (Tuple<int, int>) queue.Dequeue();
				int cellScore = map.CalculateScore(pos.Item1, pos.Item2, players[myId].BombRadius);
				if (cellScore > maxScore) 
				{
					targetX = pos.Item1;
					targetY = pos.Item2;
					maxScore = cellScore;
				}

				// Queue next valid positions that hasn't be queued before
				foreach (var p in map.GetValidAdjacentPositions(pos))
				{
					queue.Enqueue(p);
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
		Bomb,
		Item
	};
    
    // Class to define and control the map state
    public class Map 
	{
        public List<StringBuilder> Grid {get; set;}
        public int[,] ProcessedGrid {get; set;}
        public int Height {get; set;}
		public int Width {get; set;}
		private String _boxesLabels = "012"; 

		public Map(int height, int width) 
		{
			this.Height = height;
			this.Width = width;
		}
        
        public void ReadMap() 
		{
            this.Grid = new List<StringBuilder>();
            this.ProcessedGrid = new int[Height, Width];

			for (var i = 0; i < Height; i++)
				for (var j = 0; j < Width; j++)
					this.ProcessedGrid[i, j] = -1;

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
			if (Grid[y][x] != '.') {
                ProcessedGrid[y, x] = 0;
                return 0;
            }

			int score = 0;
			bool checkUp = true, checkDown = true, checkRight = true, checkLeft = true;
			
			for (var i = 1; i < radius; i++)
			{
				if (checkUp && y - i >= 0 && Grid[y - i][x] != '.')
				{
					if (_boxesLabels.Contains(Grid[y - i][x])) score++;
					checkUp = false;
				}

				if (checkDown && y + i < this.Height && Grid[y + i][x] != '.')
				{
					if (_boxesLabels.Contains(Grid[y + i][x])) score++;
					checkDown = false;
				}

				if (checkLeft && x - i >= 0 && Grid[y][x - i] != '.')
				{
					if (_boxesLabels.Contains(Grid[y][x - i])) score++;
					checkLeft = false;
				}

				if (checkRight && x + i < this.Width && Grid[y][x + i] != '.')
				{
					if (_boxesLabels.Contains(Grid[y][x + i])) score++;
					checkRight = false;
				}
			}

			// Make a copy of actual grid state
			var gridCopy = new List<StringBuilder>(this.Grid.Select(l => new StringBuilder(l.ToString())));
			
			// Mark the dangerous paths if the bomb is put in this position
			this.MarkDangerousPaths(new Bomb(0, 8, radius, x, y), gridCopy);

			if (!this.IsSafeToBombHere(x, y, gridCopy)) // Check wether is safe to put a bomb here
				score = -1;
			
            ProcessedGrid[y, x] = score;

			return score; 

		}

		public void MarkDeadBoxes(Bomb b)
		{
			bool checkUp = true, checkDown = true, checkRight = true, checkLeft = true;
			for (var i = 0; i < b.BombRadius; i++) 
			{
				if (checkUp && b.Y - i >= 0 && _boxesLabels.Contains(Grid[b.Y - i][b.X]))
				{
					Grid[b.Y - i][b.X] = '#';
					checkUp = false;
				}

				if (checkDown && b.Y + i < this.Height && _boxesLabels.Contains(Grid[b.Y + i][b.X]))
				{
					Grid[b.Y + i][b.X] = '#';
					checkDown = false;
				}

				if (checkLeft && b.X - i >= 0 && _boxesLabels.Contains(Grid[b.Y][b.X - i]))
				{
					Grid[b.Y][b.X - i] = '#';
					checkLeft = false;
				}

				if (checkRight && b.X + i < this.Width && _boxesLabels.Contains(Grid[b.Y][b.X + i]))
				{
					Grid[b.Y][b.X + i] = '#';
					checkRight = false;
				}
			}
		}

		public void MarkDangerousPaths(Bomb b, List<StringBuilder> grid = this.Grid)
		{
			bool checkUp = true, checkDown = true, checkRight = true, checkLeft = true;
			if (b.RoundsLeft == 1)
			{
				for (var i = 1; i < b.BombRadius; i++) 
				{
					if (checkUp && b.Y - i >= 0 && grid[b.Y - i][b.X] == '.')
						grid[b.Y - i][b.X] = '@';
					else
						checkUp = false;

					if (checkDown && b.Y + i < this.Height && grid[b.Y + i][b.X] == '.')
						grid[b.Y + i][b.X] = '@';
					else
						checkDown = false;

					if (checkLeft && b.X - i >= 0 && grid[b.Y][b.X - i] == '.')
						grid[b.Y][b.X - i] = '@';
					else
						checkLeft = false;

					if (checkRight && b.X + i < this.Width && grid[b.Y][b.X + i] == '.')
						grid[b.Y][b.X + i] = '@';
					else
						checkRight = false;
				}
			}			
		}

		public bool IsSafeToBombHere(int x, int y, List<StringBuilder> grid = this.Grid)
		{
			// Using a BFS search from bomb's position to find wether you can be safe if you bomb (x, y)
			Queue queue = new Queue();
			queue.Enqueue(new Tuple<int, int>(players[myId].X, players[myId].Y)); // Enqueue the player position

			while (queue.Count > 0) {

				Tuple<int, int> pos = (Tuple<int, int>) queue.Dequeue();
				int cellScore = map.CalculateScore(pos.Item1, pos.Item2, players[myId].BombRadius);
				if (cellScore > maxScore) 
				{
					targetX = pos.Item1;
					targetY = pos.Item2;
					maxScore = cellScore;
				}

				// Queue next valid positions that hasn't be queued before
				foreach (var p in map.GetValidAdjacentPositions(pos))
				{
					queue.Enqueue(p);
				}

			}
		}

		public List<Tuple<int, int>> GetValidAdjacentPositions (Tuple<int, int> p)
		{
			var list = new List<Tuple<int, int>>();
			if (p.Item1 - 1 >= 0 && Grid[p.Item2][p.Item1 - 1] == '.' && ProcessedGrid[p.Item2, p.Item1 - 1] == -1)
				list.Add(new Tuple<int, int>(p.Item1 - 1, p.Item2));

			if (p.Item2 - 1 >= 0 && Grid[p.Item2 - 1][p.Item1] == '.' && ProcessedGrid[p.Item2 - 1, p.Item1] == -1)
				list.Add(new Tuple<int, int>(p.Item1, p.Item2 - 1));

			if (p.Item1 + 1 < Width && Grid[p.Item2][p.Item1 + 1] == '.' && ProcessedGrid[p.Item2, p.Item1 + 1] == -1)
				list.Add(new Tuple<int, int>(p.Item1 + 1, p.Item2));

			if (p.Item2 + 1 < Height && Grid[p.Item2 + 1][p.Item1] == '.' && ProcessedGrid[p.Item2 + 1, p.Item1] == -1)
				list.Add(new Tuple<int, int>(p.Item1, p.Item2 + 1));

			return list;
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