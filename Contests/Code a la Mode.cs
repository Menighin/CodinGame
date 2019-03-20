using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#region Permutation
public static class IEnumerableExtensions
{
	public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence)
	{
		if (sequence == null)
		{
			yield break;
		}

		var list = sequence.ToList();

		if (!list.Any())
		{
			yield return Enumerable.Empty<T>();
		}
		else
		{
			var startingElementIndex = 0;

			foreach (var startingElement in list)
			{
				var remainingItems = list.AllExcept(startingElementIndex);

				foreach (var permutationOfRemainder in remainingItems.Permute())
				{
					yield return startingElement.Concat(permutationOfRemainder);
				}

				startingElementIndex++;
			}
		}
	}

	private static IEnumerable<T> Concat<T>(this T firstElement, IEnumerable<T> secondSequence)
	{
		yield return firstElement;
		if (secondSequence == null)
		{
			yield break;
		}

		foreach (var item in secondSequence)
		{
			yield return item;
		}
	}

	private static IEnumerable<T> AllExcept<T>(this IEnumerable<T> sequence, int indexToSkip)
	{
		if (sequence == null)
		{
			yield break;
		}

		var index = 0;

		foreach (var item in sequence.Where(item => index++ != indexToSkip))
		{
			yield return item;
		}
	}
}
#endregion

[Flags]
public enum ItemEnum
{
	EMPTY = 1,
	DISH = 2,
	ICE_CREAM = 4,
	BLUEBERRIES = 8,
	STRAWBERRIES = 16,
	CHOPPED_STRAWBERRIES = 32,
	CHOPPED_DOUGH = 64,
	DOUGH = 128,
	CROISSANT = 256,
	RAW_TART = 512,
	TART = 1024
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
	PICKING_CHOPPED_STRAWBERRIES,
	PICKING_SEMI_FINISHED_ORDER,
	PICKING_DOUGH,
	BAKING_DOUGH,
	PICKING_FROM_OVEN,
	LEAVING_CROISSANT,
	PICKING_CROISSANT,
	EMPTYING_HANDS,
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
			Items = ItemEnum.EMPTY;
			foreach (var item in items)
				Items = Items | item;
			Items &= ~ItemEnum.EMPTY;

			if (this.id == "ME") 
			{
				Console.Error.WriteLine($"Setting items {itemsStr} = {Items}");
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
	public string Alias { get; set; }
	public Position Position { get; set; }
	public ItemEnum Content { get; set; }

	public Table(string alias = null)
	{
		Alias = alias;
		Content = ItemEnum.EMPTY;
	}

	public void SetContent(string itemsStr) 
	{
		if (itemsStr == "NONE") Content = ItemEnum.EMPTY;
		else
		{
			var items = itemsStr.Split('-').ToList().Select(o => (ItemEnum) Enum.Parse(typeof(ItemEnum), o, true));
			Content = 0;
			foreach (var item in items)
				Content = Content | item;
		}
	}

	public override string ToString() => Alias;
}

public class ClientOrder
{
	public ItemEnum Order { get; set; }
	public int Reward { get; set; }
	public string ItemStr { get; set; }

	public void SetOrder(string itemsStr) 
	{
		ItemStr = itemsStr;
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

		Console.Error.WriteLine($"Preparing: {PreparingOrder.ItemStr} | {CurrentState}");


		// Checking delivery condition
		if (CurrentState == StateEnum.DELIVERING)
		{
			if (PreparingOrder.Order.HasFlag(ItemEnum.ICE_CREAM) && !Me.Items.HasFlag(ItemEnum.ICE_CREAM))
			{
				CurrentState = StateEnum.EMPTYING_HANDS;
				StatesStack.Push(StateEnum.DELIVERING);
				StatesStack.Push(StateEnum.PICKING_ICE_CREAM);
				StatesStack.Push(StateEnum.PICKING_DISH);
			}
			else if (PreparingOrder.Order.HasFlag(ItemEnum.BLUEBERRIES) && !Me.Items.HasFlag(ItemEnum.BLUEBERRIES))
			{
				CurrentState = StateEnum.EMPTYING_HANDS;
				StatesStack.Push(StateEnum.DELIVERING);
				StatesStack.Push(StateEnum.PICKING_BLUEBERRIES);
				StatesStack.Push(StateEnum.PICKING_DISH);
			}
			else if (PreparingOrder.Order.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES) && !Me.Items.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES))
			{
				StatesStack.Push(StateEnum.DELIVERING);
				if (Tables.FirstOrDefault(o => o.Content.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES)) != null)
					CurrentState = StateEnum.PICKING_CHOPPED_STRAWBERRIES;
				else
				{
					CurrentState = StateEnum.EMPTYING_HANDS;
					StatesStack.Push(StateEnum.PICKING_CHOPPED_STRAWBERRIES);
					StatesStack.Push(StateEnum.PICKING_SEMI_FINISHED_ORDER);
					StatesStack.Push(StateEnum.CHOPPING_STRAWBERRIES);
					StatesStack.Push(StateEnum.PICKING_STRAWBERRIES);
				}
			}
			else if (PreparingOrder.Order.HasFlag(ItemEnum.CROISSANT) && !Me.Items.HasFlag(ItemEnum.CROISSANT))
			{
				StatesStack.Push(StateEnum.DELIVERING);
				if (Tables.FirstOrDefault(o => o.Content.HasFlag(ItemEnum.CROISSANT)) != null)
					CurrentState = StateEnum.PICKING_CROISSANT;
				else
				{
					CurrentState = StateEnum.EMPTYING_HANDS;
					StatesStack.Push(StateEnum.PICKING_CROISSANT);
					StatesStack.Push(StateEnum.PICKING_SEMI_FINISHED_ORDER);
					StatesStack.Push(StateEnum.BAKING_DOUGH);
					StatesStack.Push(StateEnum.PICKING_DOUGH);
				}
			}
		}

		if ((CurrentState == StateEnum.PICKING_BLUEBERRIES || CurrentState == StateEnum.PICKING_ICE_CREAM) && !Me.Items.HasFlag(ItemEnum.DISH))
		{
			StatesStack.Push(CurrentState);
			CurrentState = StateEnum.PICKING_DISH;
		}


		if (CurrentState == StateEnum.EMPTYING_HANDS)
		{
			var nearestDishEmptyTable = Tables
				.Where(o => o.Content == ItemEnum.EMPTY)
				.OrderBy(o => o.Position.ManhattanDistance(Me.Position))
				.First();
			if (Me.CanUseTable(nearestDishEmptyTable))
				return $"USE {nearestDishEmptyTable.Position}; {CurrentState}";
			else 
				return $"MOVE {nearestDishEmptyTable.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.PICKING_DISH || CurrentState == StateEnum.PICKING_SEMI_FINISHED_ORDER)
		{
			var dishToPick = Tables.FirstOrDefault(o => o.Content.HasFlag(ItemEnum.DISH)) ?? Dishwasher;
			if (Me.CanUseTable(dishToPick))
				return $"USE {dishToPick.Position}; {CurrentState}";
			else 
				return $"MOVE {dishToPick.Position}; {CurrentState}";
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
				return $"USE {Strawberries.Position}; {CurrentState}";
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
		else if (CurrentState == StateEnum.PICKING_CHOPPED_STRAWBERRIES)
		{
			var tableWithChoppedStrawberries = Tables.FirstOrDefault(o => o.Content.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES));
			if (tableWithChoppedStrawberries != null)
			{
				if (Me.CanUseTable(tableWithChoppedStrawberries))
					return $"USE {tableWithChoppedStrawberries.Position}; {CurrentState}";
				else 
					return $"MOVE {tableWithChoppedStrawberries.Position}; {CurrentState}";
			}
			else 
			{
				CurrentState = StateEnum.CHOPPING_STRAWBERRIES;
			}
		}
		else if (CurrentState == StateEnum.PICKING_CROISSANT)
		{
			var tableWithCroissant = Tables.FirstOrDefault(o => o.Content.HasFlag(ItemEnum.CROISSANT));
			if (tableWithCroissant != null)
			{
				if (Me.CanUseTable(tableWithCroissant))
					return $"USE {tableWithCroissant.Position}; {CurrentState}";
				else 
					return $"MOVE {tableWithCroissant.Position}; {CurrentState}";
			}
			else
			{
				CurrentState = StateEnum.BAKING_DOUGH;
			}
		}
		else if (CurrentState == StateEnum.PICKING_DOUGH)
		{
			if (Me.CanUseTable(Dough))
				return $"USE {Dough.Position}; {CurrentState}";
			else 
				return $"MOVE {Dough.Position}; {CurrentState}";
		}
		else if (CurrentState == StateEnum.BAKING_DOUGH)
		{
			if (Me.Items.HasFlag(ItemEnum.DOUGH)) 
			{
				if (Me.CanUseTable(Oven))
					return $"USE {Oven.Position}; {CurrentState}";
				else 
					return $"MOVE {Oven.Position}; {CurrentState}";
			}
			else 
			{
				if (!Me.CanUseTable(Oven))
					return $"MOVE {Oven.Position}; {CurrentState}";
				else
				{
					if (Oven.Content.HasFlag(ItemEnum.CROISSANT))
						return $"USE {Oven.Position}; {CurrentState}";
					else
						return $"WAIT; {CurrentState}";
				}
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

		return $"WAIT; {CurrentState}";
	}

	private void DefineStatesSequence() 
	{
		// Defining equipments to use
		var equipments = new List<Table>();

		if (PreparingOrder.Order.HasFlag(ItemEnum.BLUEBERRIES))
			equipments.Add(Blueberry);

		if (PreparingOrder.Order.HasFlag(ItemEnum.ICE_CREAM))
			equipments.Add(IceCream);

		if (PreparingOrder.Order.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES))
			equipments.Add(Strawberries);

		if (PreparingOrder.Order.HasFlag(ItemEnum.CROISSANT))
			equipments.Add(Dough);

		// Define the best order to use equipments
		var bestOrder = new List<Table>();
		var minCost = int.MaxValue;
		foreach(var p in equipments.Permute())
		{
			var cost = 0;
			var equipmentOrder = new List<Table>();
			var lastPosition = Me.Position;
			var gotDishwasher = false;
			foreach (var t in p)
			{

				if (!gotDishwasher && (t.Alias == IceCream.Alias || t.Alias == Blueberry.Alias))
				{
					cost += lastPosition.ManhattanDistance(Dishwasher.Position);
					equipmentOrder.Add(Dishwasher);
					lastPosition = Dishwasher.Position;
					gotDishwasher = true;
				}

				cost += lastPosition.ManhattanDistance(t.Position);
				equipmentOrder.Add(t);

				if (t.Alias == Strawberries.Alias) 
				{
					cost += t.Position.ManhattanDistance(ChoppingTable.Position);
					equipmentOrder.Add(ChoppingTable);
					lastPosition = ChoppingTable.Position;
				}
				else if (t.Alias == Dough.Alias)
				{
					cost += t.Position.ManhattanDistance(Oven.Position);
					equipmentOrder.Add(Oven);
					lastPosition = Oven.Position;
				}
				else
				{
					lastPosition = t.Position;
				}
			}
			cost += lastPosition.ManhattanDistance(Window.Position);
			equipmentOrder.Add(Window);

			if (cost < minCost)
			{
				minCost = cost;
				bestOrder = equipmentOrder;
			}
		}

		Console.Error.WriteLine(string.Join(" -> ", bestOrder));

		// Defining sequence of states
		var statesSequence = new List<StateEnum>();
		foreach (var t in bestOrder)
		{
			if (t.Alias == Strawberries.Alias)
			{
				statesSequence.Add(StateEnum.EMPTYING_HANDS);
				statesSequence.Add(StateEnum.PICKING_STRAWBERRIES);
			}
			if (t.Alias == ChoppingTable.Alias)
			{
				statesSequence.Add(StateEnum.PICKING_STRAWBERRIES);
				statesSequence.Add(StateEnum.CHOPPING_STRAWBERRIES);
				statesSequence.Add(StateEnum.EMPTYING_HANDS);
			}
			if (t.Alias == Dough.Alias)
			{
				statesSequence.Add(StateEnum.EMPTYING_HANDS);
				statesSequence.Add(StateEnum.PICKING_DOUGH);
			}
			if (t.Alias == Oven.Alias)
			{
				statesSequence.Add(StateEnum.PICKING_DOUGH);
				statesSequence.Add(StateEnum.BAKING_DOUGH);
				statesSequence.Add(StateEnum.EMPTYING_HANDS);
			}
			if (t.Alias == Dishwasher.Alias)
			{
				statesSequence.Add(StateEnum.EMPTYING_HANDS);
				statesSequence.Add(StateEnum.PICKING_DISH);
			}
			if (t.Alias == IceCream.Alias)
			{
				statesSequence.Add(StateEnum.PICKING_ICE_CREAM);
			}
			if (t.Alias == Blueberry.Alias)
			{
				statesSequence.Add(StateEnum.PICKING_BLUEBERRIES);
			}
			if (t.Alias == Window.Alias)
			{
				statesSequence.Add(StateEnum.DELIVERING);
			}
		}

		Console.Error.WriteLine(string.Join(" -> ", statesSequence));
		StatesStack = new Stack<StateEnum>();
		statesSequence.Reverse();
		foreach(var s in statesSequence)
		{
			StatesStack.Push(s);
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
			CurrentState = StatesStack.Pop();
		}

		var nextState = StatesStack.Any() ? StatesStack.Peek() : StateEnum.WAITING;

		if (CurrentState == StateEnum.EMPTYING_HANDS && Me.Items == ItemEnum.EMPTY) 
		{
			CurrentState = StatesStack.Pop();
		} 
		else if (CurrentState == StateEnum.PICKING_DISH && Me.Items.HasFlag(ItemEnum.DISH))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.PICKING_BLUEBERRIES)
		{
			if (Me.Items.HasFlag(ItemEnum.BLUEBERRIES))
			{
				CurrentState = StatesStack.Pop();
			}
			else if (!Me.Items.HasFlag(ItemEnum.DISH)) 
			{
				StatesStack.Push(StateEnum.PICKING_BLUEBERRIES);
				CurrentState = StateEnum.PICKING_DISH;
			}
		}
		else if (CurrentState == StateEnum.PICKING_ICE_CREAM && Me.Items.HasFlag(ItemEnum.ICE_CREAM))
		{
			if (Me.Items.HasFlag(ItemEnum.ICE_CREAM))
			{
				CurrentState = StatesStack.Pop();
			}
			else if (!Me.Items.HasFlag(ItemEnum.DISH)) 
			{
				StatesStack.Push(StateEnum.PICKING_ICE_CREAM);
				CurrentState = StateEnum.PICKING_DISH;
			}
		}
		else if (CurrentState == StateEnum.PICKING_STRAWBERRIES && Me.Items.HasFlag(ItemEnum.STRAWBERRIES))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.PICKING_CHOPPED_STRAWBERRIES && Me.Items.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.PICKING_DOUGH && Me.Items.HasFlag(ItemEnum.DOUGH))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.PICKING_CROISSANT && Me.Items.HasFlag(ItemEnum.CROISSANT))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.CHOPPING_STRAWBERRIES && Me.Items.HasFlag(ItemEnum.CHOPPED_STRAWBERRIES))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.BAKING_DOUGH && Me.Items.HasFlag(ItemEnum.CROISSANT))
		{
			CurrentState = StatesStack.Pop();
		}
		else if (CurrentState == StateEnum.PICKING_SEMI_FINISHED_ORDER && Me.Items.HasFlag(ItemEnum.DISH))
		{
			CurrentState = StatesStack.Pop();
		}

		Console.Error.WriteLine(string.Join(" -> ", StatesStack));
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
                if (kitchenLine[j] == 'W') game.Window = new Table("Window") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'D') game.Dishwasher = new Table("Dishwasher") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'I') game.IceCream = new Table("IceCream") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'B') game.Blueberry = new Table("Blueberry") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'S') game.Strawberries = new Table("Strawberry") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'C') game.ChoppingTable = new Table("ChoppingTable") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'H') game.Dough = new Table("Dough") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == 'O') game.Oven = new Table("Oven") { Position = new Position { X = j, Y = i} };
                if (kitchenLine[j] == '#') game.Tables.Add(new Table("EmptyTable") { Position = new Position { X = j, Y = i} });
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
			game.Oven.SetContent(ovenContents);

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