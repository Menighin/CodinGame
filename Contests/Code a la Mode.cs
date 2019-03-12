using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

[Flags]
public enum ItemEnum
{
	EMPTY = 1,
	DISH = 2,
	ICE_CREAM = 4,
	BLUEBERRIES = 8
}

[Flags]
public enum StateEnum
{
	WAITING = 0,
	PREPARING = 1,
	DELIVERING = 2
}

public class Position 
{
	public int X { get ; set; }
	public int Y { get ; set; }

	public int ManhattanDistance(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);

	public bool IsTangentTo(Position p2) => ManhattanDistance(p2) <= 1 || (Math.Abs(X - p2.X) <= 1 && Math.Abs(Y - p2.Y) <= 1); 

	public override string ToString() => $"{X} {Y}";
}

public class Player 
{
	private string id;

	public Position Position { get; set; }
	public ItemEnum Items { get; set; }

	public Player(string id)
	{
		this.id = id;
		Position = new Position();
		Items = 0;
	}

	public void SetItems(string itemsStr) 
	{
		if (itemsStr == "NONE") Items = ItemEnum.EMPTY;
		else 
		{
			var items = itemsStr.Split('-').ToList().Select(o => (ItemEnum) Enum.Parse(typeof(ItemEnum), o, true));
			foreach (var item in items)
				Items = Items | item;
			Items &= ~ItemEnum.EMPTY;

			if (this.id == "ME") 
			{
				Console.Error.WriteLine($"Setting items {itemsStr}");
				Console.Error.WriteLine($"Has DISH? {Items.HasFlag(ItemEnum.DISH)}");
				Console.Error.WriteLine($"Has BLUEBERRIES? {Items.HasFlag(ItemEnum.BLUEBERRIES)}");
				Console.Error.WriteLine($"Has ICE_CREAM? {Items.HasFlag(ItemEnum.ICE_CREAM)}");
				Console.Error.WriteLine($"Has EMPTY? {Items.HasFlag(ItemEnum.EMPTY)}");
			}
			
		}
	}

	public bool CanUseTable(Table t) {
		return Position.IsTangentTo(t.Position);
	}

}

public class Table 
{
	public Position Position { get; set; }
	public ItemEnum Content { get; set; }

	public Table()
	{
		Content = ItemEnum.EMPTY;
	}

	public void SetContent(string itemsStr) 
	{
		var items = itemsStr.Split('-').ToList().Select(o => (ItemEnum) Enum.Parse(typeof(ItemEnum), o, true));
		Content = 0;
		foreach (var item in items)
			Content = Content | item;
		
	}
}

public class Game
{
	public StateEnum MyState { get; set; }
	public Player Me { get; set; }
	public Player Partner { get; set; }
	public Table Dishwasher { get; set; }
	public Table Window { get; set; }
	public Table IceCream { get; set; }
	public Table Blueberry { get; set; }
	public List<Table> Tables { get; set; }
	public int TurnsRemaining { get; set; }

	public Game()
	{
		Me = new Player("ME");
		Partner = new Player("PARTNER");
		Tables = new List<Table>();
		MyState = StateEnum.PREPARING;
	}

	public void ClearTables() 
	{
		foreach (var table in Tables)
			table.Content = ItemEnum.EMPTY;
	}

	public string DoSomething()
	{
		if (MyState == StateEnum.PREPARING)
		{
			if (!Me.Items.HasFlag(ItemEnum.DISH))
			{
				if (Me.CanUseTable(Dishwasher))
					return $"USE {Dishwasher.Position}; gotta get some plates - {MyState}";
				else 
					return $"MOVE {Dishwasher.Position}; gotta get some plates - {MyState}";
			}
			
			if (!Me.Items.HasFlag(ItemEnum.BLUEBERRIES))
			{
				if (Me.CanUseTable(Blueberry))
					return $"USE {Blueberry.Position}; gotta get some berries - {MyState}";
				else 
					return $"MOVE {Blueberry.Position}; gotta get some berries - {MyState}";
			}
			
			if (!Me.Items.HasFlag(ItemEnum.ICE_CREAM))
			{
				if (Me.CanUseTable(IceCream))
					return $"USE {IceCream.Position}; gotta get some gelatto - {MyState}";
				else 
					return $"MOVE {IceCream.Position}; gotta get some gelatto - {MyState}";
			}
		}
		else if (MyState == StateEnum.DELIVERING)
		{
			if (Me.CanUseTable(Window)) 
			{
				Me.Items = ItemEnum.EMPTY;
				return $"USE {Window.Position}; gotta delivery this rubish - {MyState}";
			}
			else
				return $"MOVE {Window.Position}; gotta delivery this rubish - {MyState}";
		}

		return "WAIT";
	}

	public void UpdateState() 
	{
		if (Me.Items.HasFlag(ItemEnum.EMPTY))
			MyState = StateEnum.PREPARING;
		else if (Me.Items.HasFlag(ItemEnum.DISH) && Me.Items.HasFlag(ItemEnum.BLUEBERRIES) && Me.Items.HasFlag(ItemEnum.ICE_CREAM))
			MyState = StateEnum.DELIVERING;
	}
} 

public class CodeALaMode
{

	private static Game game = new Game();

    static void Main(string[] args)
    {
        string[] inputs;
        int numAllCustomers = int.Parse(Console.ReadLine());
        for (int i = 0; i < numAllCustomers; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            string customerItem = inputs[0]; // the food the customer is waiting for
            int customerAward = int.Parse(inputs[1]); // the number of points awarded for delivering the food
        }
        for (var i = 0; i < 7; i++)
        {
            var kitchenLine = Console.ReadLine();
			for (var j = 0; j < kitchenLine.Length; j++){
                if (kitchenLine[j] == 'W') game.Window = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'D') game.Dishwasher = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'I') game.IceCream = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'B') game.Blueberry = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == '#') game.Tables.Add(new Table { Position = new Position { X = j, Y = i} });
            }
        }

        // game loop
        while (true)
        {
            game.TurnsRemaining = int.Parse(Console.ReadLine());

			// Read player info
            inputs = Console.ReadLine().Split(' ');
			game.Me.Position = new Position { X = int.Parse(inputs[0]), Y = int.Parse(inputs[1]) };
			game.Me.SetItems(inputs[2]);

			// Read partner info
            inputs = Console.ReadLine().Split(' ');
            game.Partner.Position = new Position { X = int.Parse(inputs[0]), Y = int.Parse(inputs[1]) };
			game.Partner.SetItems(inputs[2]);

			// Read tables
			game.ClearTables();
            var numTablesWithItems = int.Parse(Console.ReadLine()); // the number of tables in the kitchen that currently hold an item
            for (int i = 0; i < numTablesWithItems; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var tableX = int.Parse(inputs[0]);
                var tableY = int.Parse(inputs[1]);
				game.Tables.First(o => o.Position.X == tableX && o.Position.Y == tableY).SetContent(inputs[2]);
            }

			// Read oven properties
            inputs = Console.ReadLine().Split(' ');
            string ovenContents = inputs[0]; // ignore until wood 1 league
            int ovenTimer = int.Parse(inputs[1]);
            int numCustomers = int.Parse(Console.ReadLine()); // the number of customers currently waiting for food
            for (int i = 0; i < numCustomers; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string customerItem = inputs[0];
                int customerAward = int.Parse(inputs[1]);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // MOVE x y
            // USE x y
            // WAIT
            Console.WriteLine(game.DoSomething());

			game.UpdateState();
        }
    }
}