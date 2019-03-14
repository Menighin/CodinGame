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
	BLUEBERRIES = 8,
	STRAWBERRIES = 16,
	CHOPPED_STRAWBERRIES = 32,
	DOUGH = 64,
	CROISSANT = 128
}

[Flags]
public enum StateEnum
{
	WAITING,
	PICKING_DISH,
	PICKING_ICE_CREAM,
	PICKING_BLUEBERRIES,
	PICKING_STRAWBERRIES,
	CHOPPING_STRAWBERRIES,
	LEAVING_CHOPPED_STRAWBERRIES,
	GETTING_DOUGH,
	BAKING_DOUGH,
	PICKING_FROM_OVEN,
	LEAVING_CROISSANT,
	PICKING_CROISSANT,
	DELIVERING
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

public class ClientOrder
{
	public ItemEnum Order { get; set; }
	public int Reward { get; set; }

	public void SetOrder(string itemsStr) 
	{
		var items = itemsStr.Split('-').ToList().Select(o => (ItemEnum) Enum.Parse(typeof(ItemEnum), o, true));
		Order = 0;
		foreach (var item in items)
			Order = Order | item;
	}
}

public class Game
{
	public StateEnum CurrentState { get; set; }
	public Player Me { get; set; }
	public Player Partner { get; set; }
	public Table Dishwasher { get; set; }
	public Table ChoppingTable { get; set; }
	public Table Oven { get; set; }
	public Table Window { get; set; }
	public Table IceCream { get; set; }
	public Table Blueberry { get; set; }
	public Table Strawberries { get; set; }
	public Table Dough { get; set; }
	public List<Table> Tables { get; set; }
	public List<ClientOrder> ClientOrders { get; set; }
	public ClientOrder PreparingOrder { get; set; }
	public int TurnsRemaining { get; set; }
	public Stack<StateEnum> StatesStack { get; set; }

	public Game()
	{
		Me = new Player("ME");
		Partner = new Player("PARTNER");
		Tables = new List<Table>();
	}

	public void ClearTables() 
	{
		foreach (var table in Tables)
			table.Content = ItemEnum.EMPTY;
	}

	public string DoSomething()
	{
		if (CurrentState == StateEnum.PICKING_DISH)
		{
			if (Me.CanUseTable(Dishwasher))
				return $"USE {Dishwasher.Position}; {CurrentState}";
			else 
				return $"MOVE {Dishwasher.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.PICKING_BLUEBERRIES)
		{
			if (Me.CanUseTable(Blueberry))
				return $"USE {Blueberry.Position}; {CurrentState}";
			else 
				return $"MOVE {Blueberry.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.PICKING_ICE_CREAM)
		{
			if (Me.CanUseTable(IceCream))
				return $"USE {IceCream.Position}; {CurrentState}";
			else 
				return $"MOVE {IceCream.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.PICKING_STRAWBERRIES)
		{
			if (Me.CanUseTable(Strawberries))
				return $"USE {Strawberries.Pistion}; {CurrentState}";
			else 
				return $"MOVE {Strawberries.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.CHOPPING_STRAWBERRIES)
		{
			if (Me.CanUseTable(ChoppingTable))
				return $"USE {ChoppingTable.Position}; {CurrentState}";
			else 
				return $"MOVE {ChoppingTable.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.LEAVING_CHOPPED_STRAWBERRIES)
		{
			var nearestDishEmptyTable = Tables
				.Where(o => o.Content == ItemEnum.EMPTY)
				.OrderBy(o => o.Position.ManhattanDistance(Dishwasher.Position))
				.First();

			if (Me.CanUseTable(nearestDishEmptyTable))
				return $"USE {nearestDishEmptyTable.Position}; {CurrentState}";
			else 
				return $"MOVE {nearestDishEmptyTable.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.GETTING_CHOPPED_STRAWBERRIES)
		{
			var tableWithChoppedStrawberries = Tables.FirstOrDefault(o => o.Content.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES));
			if (tableWithChoppedStrawberries != null)
			{
				if (Me.CanUseTable(tableWithChoppedStrawberries))
					return $"USE {tableWithChoppedStrawberries.Position}; {CurrentState}";
				else 
					return $"MOVE {tableWithChoppedStrawberries.Position}; {CurrentState}";
			}
		}
		else if (CurrentState == StateEnum.DELIVERING)
		{
			if (Me.CanUseTable(Window)) 
			{
				PreparingOrder = null;
				return $"USE {Window.Position}; {CurrentState}";
			}
			else
				return $"MOVE {Window.Position}; {CurrentState}";
		}

		return "WAIT";
	}

	private void DefineStatesSequence() 
	{
		// Defining equipments to use
		var equipments = new List<string>() {"D", "W"};

		if (PreparingOrder.Order.HasFlag(ItemEnum.BLUEBERRIES))
			equipments.Add("B");

		if (PreparingOrder.Order.HasFlag(ItemEnum.ICE_CREAM))
			equipments.Add("I");

		if (PreparingOrder.Order.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES))
		{
			equipments.Add("S");
			equipments.Add("C");
		}

		if (PreparingOrder.Order.HasFlag(ItemEnum.CROISSANT))
		{
			equipments.Add("H");
			equipments.Add("O");
		}
	}

	private void PickClientOrder() 
	{
		PreparingOrder = ClientOrders.OrderByDescending(o => o.Reward).First();
	}

	public void UpdateState() 
	{
		if (this.PreparingOrder == null)
		{
			PickClientOrder();
			DefineStatesSequence();
			MyStateIndex = 0;
			Console.Error.WriteLine(string.Join(" - ", StatesSequence.ToArray()));
		}

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
                if (kitchenLine[j] == 'S') game.Strawberries = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'C') game.ChoppingTable = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'H') game.Dough = new Table { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'O') game.Oven = new Table { Position = new Position { X = j, Y = i} };
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

			// Read client orders
            int numClients = int.Parse(Console.ReadLine()); // the number of customers currently waiting for food
			game.ClientOrders = new List<ClientOrder>();
            for (int i = 0; i < numClients; i++)
            {
                inputs = Console.ReadLine().Split(' ');
				var clientOrder = new ClientOrder();
				clientOrder.SetOrder(inputs[0]);
				clientOrder.Reward = int.Parse(inputs[1]);
				game.ClientOrders.Add(clientOrder);
            }

            // MOVE x y
            // USE x y
            // WAIT
			game.UpdateState();
            Console.WriteLine(game.DoSomething());

        }
    }
}