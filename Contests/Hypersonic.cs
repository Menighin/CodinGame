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
			var bombs = new Dictionary<int, List<Bomb>>();
            
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

					if (!bombs.ContainsKey(owner))
						bombs[owner] = new List<Bomb>();

					bombs[owner].Add(b);
					map.SetCell(x, y, 'b');
				}
            }
			map.Bombs = bombs.Values.SelectMany(x => x).ToList();
			map.MarkDangerousPaths();

            // Process map
			//map.PrintMap();
			
			int maxScore = -100;
			int targetX = 0, targetY = 0;

			// Using a BFS search from players position to find the better place he can put the bomb
			Queue queue = new Queue();
			queue.Enqueue(new Tuple<int, int, int>(players[myId].X, players[myId].Y, 0)); // Enqueue the player position

			// If the player is one step away from killing himself, stay!
			if (players[myId].CanKillMyselfOnNextRound(map))
			{
				Console.WriteLine(String.Format("MOVE {0} {1} CAREFUL NOW", players[myId].X, players[myId].Y));
			}
			else if (players[myId].IsInDanger(map))  // If player is in danger, then it should get safe before thinking about bombing
			{
				var processedPositions = new HashSet<Tuple<int, int, int>>();

				while (queue.Count > 0) {

					Tuple<int, int, int> p = (Tuple<int, int, int>) queue.Dequeue();
					
					// If there is a empty not dangerous position, then it's safe to bomb (x, y)
					if (map.GetCell(p.Item1, p.Item2) == '.') {
						targetX = p.Item1;
						targetY = p.Item2;
						break;
					}

					// Queue next valid positions that hasn't be queued before
					if (p.Item1 - 1 >= 0 && (map.GetCell(p.Item1 - 1, p.Item2) == '.' || map.GetCell(p.Item1 - 1, p.Item2) == '@') && !processedPositions.Contains(new Tuple<int, int, int>(p.Item1 - 1, p.Item2, 0)))
						queue.Enqueue(new Tuple<int, int, int>(p.Item1 - 1, p.Item2, 0));

					if (p.Item2 - 1 >= 0 && (map.GetCell(p.Item1, p.Item2 - 1) == '.' || map.GetCell(p.Item1, p.Item2 - 1) == '@') && !processedPositions.Contains(new Tuple<int, int, int>(p.Item1, p.Item2 - 1, 0)))
						queue.Enqueue(new Tuple<int, int, int>(p.Item1, p.Item2 - 1, 0));

					if (p.Item1 + 1 < map.Width && (map.GetCell(p.Item1 + 1, p.Item2) == '.' || map.GetCell(p.Item1 + 1, p.Item2) == '@') && !processedPositions.Contains(new Tuple<int, int, int>(p.Item1 + 1, p.Item2, 0)))
						queue.Enqueue(new Tuple<int, int, int>(p.Item1 + 1, p.Item2, 0));

					if (p.Item2 + 1 < map.Height && (map.GetCell(p.Item1, p.Item2 + 1) == '.' || map.GetCell(p.Item1, p.Item2 + 1) == '@') && !processedPositions.Contains(new Tuple<int, int, int>(p.Item1, p.Item2 + 1, 0)))
						queue.Enqueue(new Tuple<int, int, int>(p.Item1, p.Item2 + 1, 0));

					processedPositions.Add(p);

				}

				Console.WriteLine("MOVE " + targetX + " " + targetY + " DANGER! Going to: (" + targetX + ", " + targetY + ")");
				
			}
			else 
			{
				#region Find best place for bomb
				while (queue.Count > 0) {

					Tuple<int, int, int> pos = (Tuple<int, int, int>) queue.Dequeue();
					int cellScore = map.CalculateScore(pos.Item1, pos.Item2, players[myId].BombRadius, pos.Item3);
					if (cellScore > maxScore) 
					{
						targetX = pos.Item1;
						targetY = pos.Item2;
						maxScore = cellScore;
					}

					// Queue next valid positions that hasn't be queued before
					foreach (var p in map.GetValidAdjacentPositions(pos))
					{
						queue.Enqueue(new Tuple<int, int, int>(p.Item1, p.Item2, pos.Item3 + 1));
					}

				}

				if (players[myId].X == targetX && players[myId].Y == targetY && maxScore > 0)
					Console.WriteLine("BOMB " + targetX + " " + targetY + " KABOOM!");
				else
					Console.WriteLine("MOVE " + targetX + " " + targetY + " Going to: (" + targetX + ", " + targetY + ")");
				#endregion
			}
			
			 //map.PrintMap();
			// map.PrintScoreMatrix();
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
		public List<Bomb> Bombs {get;set;}

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
					this.ProcessedGrid[i, j] = -2;

            for (int i = 0; i < this.Height; i++)
            {
				Grid.Add(new StringBuilder());
                Grid[i].Append(Console.ReadLine());
            }
        }
        
		public char GetCell(int x, int y) 
		{
			if (x < 0 || y < 0 || x >= Width || y >= Height) return '?';

			return Grid[y][x];
		}
		
		public void SetCell(int x, int y, char value) 
		{
		    Grid[y][x] = value;
		}

		public void PrintMap(List<StringBuilder> grid = null) 
		{
			grid = grid ?? this.Grid;
			Console.Error.WriteLine("\n");
			
			foreach(var l in grid)
			{
				Console.Error.WriteLine(l);
			}

			Console.Error.WriteLine("\n");
		}

		public void PrintScoreMatrix(int[,] matrix = null) {
			matrix = matrix ?? this.ProcessedGrid;
			
			Console.Error.WriteLine("\n");
			
			for(var h = 0; h < this.Height; h++)
			{
				string s = "";
				for (var w = 0; w < this.Width; w++)
					s += matrix[h, w] + " ";
				Console.Error.WriteLine(s);
			}

			Console.Error.WriteLine("\n");
		}

		public int CalculateScore (int x, int y, int radius, int distance) 
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
					if (_boxesLabels.Contains(Grid[y - i][x])) score += 1000;
					checkUp = false;
				}

				if (checkDown && y + i < this.Height && Grid[y + i][x] != '.')
				{
					if (_boxesLabels.Contains(Grid[y + i][x])) score += 1000;
					checkDown = false;
				}

				if (checkLeft && x - i >= 0 && Grid[y][x - i] != '.')
				{
					if (_boxesLabels.Contains(Grid[y][x - i])) score += 1000;
					checkLeft = false;
				}

				if (checkRight && x + i < this.Width && Grid[y][x + i] != '.')
				{
					if (_boxesLabels.Contains(Grid[y][x + i])) score += 1000;
					checkRight = false;
				}
			}
			
			if (distance > 5) // Penalize far bomb spots
				score /= distance;

			// Make a copy of actual grid state
			var gridCopy = new List<StringBuilder>(this.Grid.Select(l => new StringBuilder(l.ToString())));
			
			// Mark the dangerous paths if the bomb is put in this position
			this.MarkDangerousPaths(new List<Bomb>(){new Bomb(0, 8, radius, x, y)}, gridCopy);

			gridCopy[y][x] = 'p';

			// this.PrintMap(gridCopy);

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

		public void MarkDangerousPaths(List<Bomb> bombs = null, List<StringBuilder> grid = null)
		{
			grid = grid ?? this.Grid;
			if (bombs == null)
				bombs = Bombs;
			else
				bombs.AddRange(Bombs);

			var checkedBombs = new HashSet<Bomb>();
			var bombsChained = new Queue();
			var bombsByPosition = new Dictionary<Tuple<int, int>, Bomb>();
			var explosionExpansion = ".@&";

			// Preprocess into dictionary to check for bomb chains
			foreach (Bomb b in bombs)
			{
				bombsByPosition[new Tuple<int, int>(b.X, b.Y)] = b;
			}

			foreach (Bomb b in bombs)
			{
				bool checkUp = true, checkDown = true, checkRight = true, checkLeft = true;

				char EXPLOSION_PATH = (b.RoundsLeft <= 2 ? '&' : '@');

				checkedBombs.Add(b);
				for (var i = 1; i < b.BombRadius; i++) 
				{
					if (checkUp && b.Y - i >= 0 && explosionExpansion.Contains(grid[b.Y - i][b.X]))
					{
						if (grid[b.Y - i][b.X] != '&') grid[b.Y - i][b.X] = EXPLOSION_PATH;
					}
					else 
					{
						checkUp = false;
						if (b.Y - i >= 0 && grid[b.Y - i][b.X] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X, b.Y - i)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X, b.Y - i)]);
					}

					if (checkDown && b.Y + i < this.Height && explosionExpansion.Contains(grid[b.Y + i][b.X]))
					{
						if ( grid[b.Y + i][b.X] != '&') grid[b.Y + i][b.X] = EXPLOSION_PATH;
					}
					else
					{
						checkDown = false;
						if ( b.Y + i < this.Height && grid[b.Y + i][b.X] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X, b.Y + i)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X, b.Y + i)]);
					}

					if (checkLeft && b.X - i >= 0 && explosionExpansion.Contains(grid[b.Y][b.X - i]))
					{
						if (grid[b.Y][b.X - i] != '&') grid[b.Y][b.X - i] = EXPLOSION_PATH;
					}
					else
					{
						checkLeft = false;
						if (b.X - i >= 0 && grid[b.Y][b.X - i] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X - i, b.Y)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X - i, b.Y)]);
					}

					if (checkRight && b.X + i < this.Width && explosionExpansion.Contains(grid[b.Y][b.X + i]))
					{
						if (grid[b.Y][b.X + i] != '&') grid[b.Y][b.X + i] = EXPLOSION_PATH;
					}
					else
					{
						checkRight = false;
						if (b.X + i < this.Width && grid[b.Y][b.X + i] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X + i, b.Y)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X + i, b.Y)]);
					}
					
				}
			}

			// Processing chained bombs
			while (bombsChained.Count > 0)
			{
				Bomb b = (Bomb) bombsChained.Dequeue();
				char EXPLOSION_PATH = (b.RoundsLeft <= 2 ? '&' : '@');

				checkedBombs.Add(b);
				bool checkUp = true, checkDown = true, checkRight = true, checkLeft = true;
				for (var i = 1; i < b.BombRadius; i++) 
				{
					if (checkUp && b.Y - i >= 0 && explosionExpansion.Contains(grid[b.Y - i][b.X]))
					{
						if (grid[b.Y - i][b.X] != '&') grid[b.Y - i][b.X] = EXPLOSION_PATH;
					}
					else 
					{
						checkUp = false;
						if (b.Y - i >= 0 && grid[b.Y - i][b.X] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X, b.Y - i)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X, b.Y - i)]);
					}

					if (checkDown && b.Y + i < this.Height && explosionExpansion.Contains(grid[b.Y + i][b.X]))
					{
						if ( grid[b.Y + i][b.X] != '&') grid[b.Y + i][b.X] = EXPLOSION_PATH;
					}
					else
					{
						checkDown = false;
						if ( b.Y + i < this.Height && grid[b.Y + i][b.X] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X, b.Y + i)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X, b.Y + i)]);
					}

					if (checkLeft && b.X - i >= 0 && explosionExpansion.Contains(grid[b.Y][b.X - i]))
					{
						if (grid[b.Y][b.X - i] != '&') grid[b.Y][b.X - i] = EXPLOSION_PATH;
					}
					else
					{
						checkLeft = false;
						if (b.X - i >= 0 && grid[b.Y][b.X - i] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X - i, b.Y)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X - i, b.Y)]);
					}

					if (checkRight && b.X + i < this.Width && explosionExpansion.Contains(grid[b.Y][b.X + i]))
					{
						if (grid[b.Y][b.X + i] != '&') grid[b.Y][b.X + i] = EXPLOSION_PATH;
					}
					else
					{
						checkRight = false;
						if (b.X + i < this.Width && grid[b.Y][b.X + i] == 'b' && !checkedBombs.Contains(bombsByPosition[new Tuple<int, int>(b.X + i, b.Y)]))
							bombsChained.Enqueue(bombsByPosition[new Tuple<int, int>(b.X + i, b.Y)]);
					}
				}
			}

		}

		public bool IsSafeToBombHere(int x, int y, List<StringBuilder> grid = null)
		{
			grid = grid ?? this.Grid;

			// Using a BFS search from bomb's position to find wether you can be safe if you bomb (x, y)
			Queue queue = new Queue();
			queue.Enqueue(new Tuple<int, int>(x, y)); // Enqueue the player position

			var processedPositions = new HashSet<Tuple<int, int>>();

			while (queue.Count > 0) {

				Tuple<int, int> p = (Tuple<int, int>) queue.Dequeue();
				
				// If there is a empty not dangerous position that the player can reach, then it's safe to bomb (x, y)
				if (grid[p.Item2][p.Item1] == '.') 
				{
					return true;
				}

				// Queue next valid positions that hasn't be queued before
				if (p.Item1 - 1 >= 0 && (grid[p.Item2][p.Item1 - 1] == '.' || grid[p.Item2][p.Item1 - 1] == '@') && !processedPositions.Contains(new Tuple<int, int>(p.Item1 - 1, p.Item2)))
					queue.Enqueue(new Tuple<int, int>(p.Item1 - 1, p.Item2));

				if (p.Item2 - 1 >= 0 && (grid[p.Item2 - 1][p.Item1] == '.' || grid[p.Item2 - 1][p.Item1] == '@') && !processedPositions.Contains(new Tuple<int, int>(p.Item1, p.Item2 - 1)))
					queue.Enqueue(new Tuple<int, int>(p.Item1, p.Item2 - 1));

				if (p.Item1 + 1 < Width && (grid[p.Item2][p.Item1 + 1] == '.' || grid[p.Item2][p.Item1 + 1] == '@') && !processedPositions.Contains(new Tuple<int, int>(p.Item1 + 1, p.Item2)))
					queue.Enqueue(new Tuple<int, int>(p.Item1 + 1, p.Item2));

				if (p.Item2 + 1 < Height && (grid[p.Item2 + 1][p.Item1] == '.' || grid[p.Item2 + 1][p.Item1] == '@') && !processedPositions.Contains(new Tuple<int, int>(p.Item1, p.Item2 + 1)))
					queue.Enqueue(new Tuple<int, int>(p.Item1, p.Item2 + 1));

				processedPositions.Add(p);

			}

			return false;
		}

		public List<Tuple<int, int>> GetValidAdjacentPositions (Tuple<int, int, int> p)
		{
			var list = new List<Tuple<int, int>>();
			if (p.Item1 - 1 >= 0 && Grid[p.Item2][p.Item1 - 1] == '.' && ProcessedGrid[p.Item2, p.Item1 - 1] == -2)
				list.Add(new Tuple<int, int>(p.Item1 - 1, p.Item2));

			if (p.Item2 - 1 >= 0 && Grid[p.Item2 - 1][p.Item1] == '.' && ProcessedGrid[p.Item2 - 1, p.Item1] == -2)
				list.Add(new Tuple<int, int>(p.Item1, p.Item2 - 1));

			if (p.Item1 + 1 < Width && Grid[p.Item2][p.Item1 + 1] == '.' && ProcessedGrid[p.Item2, p.Item1 + 1] == -2)
				list.Add(new Tuple<int, int>(p.Item1 + 1, p.Item2));

			if (p.Item2 + 1 < Height && Grid[p.Item2 + 1][p.Item1] == '.' && ProcessedGrid[p.Item2 + 1, p.Item1] == -2)
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

		public bool IsInDanger(Map map)
		{
			return map.GetCell(X, Y) == '@' || map.GetCell(X, Y) == 'b' || map.GetCell(X, Y) == '&';
		}

		public bool CanKillMyselfOnNextRound(Map map)
		{
			return (map.GetCell(X + 1, Y) == '&' || map.GetCell(X - 1, Y) == '&' || map.GetCell(X, Y + 1) == '&' || map.GetCell(X, Y - 1) == '&') && map.GetCell(X, Y) != '&';
		}

		public override bool Equals(object obj)
		{
			var other = obj as Player;
			return this.X == other.X && this.Y == other.Y && this.Id == other.Id;
		}

		public override int GetHashCode()
		{
			int hash = 13;
			hash = (hash * 7) + this.X.GetHashCode();
			hash = (hash * 7) + this.Y.GetHashCode();
			hash = (hash * 7) + this.Id.GetHashCode();

			return hash;
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

		public override bool Equals(object obj)
		{
			var other = obj as Bomb;
			return this.X == other.X && this.Y == other.Y;
		}

		public override int GetHashCode()
		{
			int hash = 13;
			hash = (hash * 7) + this.X.GetHashCode();
			hash = (hash * 7) + this.Y.GetHashCode();

			return hash;
		}
	}
}