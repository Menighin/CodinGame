using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class LegendsOfCodeAndMagic
{
    static void Main(string[] args)
    {
        string[] inputs;

        // Creating game
        Game.Me = new Player();
        Game.Opponent = new Player();
        Game.GamePhase = GamePhaseEnum.DraftPhase;
        Game.Turn = 0;

        // Game loop
        while (true)
        {
            for (var i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                if (i == 0)
                    Game.Me.Update(inputs);
                else
                    Game.Opponent.Update(inputs);
            }

            var opponentHand = int.Parse(Console.ReadLine());

            var cardCount = int.Parse(Console.ReadLine());

            var cards = new List<Card>();
            for (var i = 0; i < cardCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                cards.Add(new Card() 
                {
                    CardNumber = int.Parse(inputs[0]),
                    InstanceId = int.Parse(inputs[1]),
                    Location = (LocationEnum) int.Parse(inputs[2]),
                    CardType = (CardTypeEnum) int.Parse(inputs[3]),
                    Cost = int.Parse(inputs[4]),
                    Attack = int.Parse(inputs[5]),
                    Defense = int.Parse(inputs[6]),
                    Abilities = new HashSet<AbilitiesEnum>(inputs[7].Where(o => o != '-').Select(o => (AbilitiesEnum) o).ToList()),
                    MyHealthChange = int.Parse(inputs[8]),
                    OpponentHealthChange = int.Parse(inputs[9]),
                    CardDraw = int.Parse(inputs[10])
                });
            }

            if (Game.GamePhase == GamePhaseEnum.DraftPhase)
            {
                Game.PickDraft(cards);
            }
            else // Battle time!
            {
                Game.MakeMove(cards);
            }

            Console.Error.WriteLine($"GamePhase: {Game.GamePhase}");

            if (++Game.Turn >= 30)
                Game.GamePhase = GamePhaseEnum.BattlePhase;
        }
    }

    static class Game
    {
        public static Player Me;
        public static Player Opponent;
        public static GamePhaseEnum GamePhase;
        public static int Turn;

        private static PickDraftStateEnum PickState {
            get {
                if (Turn < CardsByState[PickDraftStateEnum.PickingCheap])
                    return PickDraftStateEnum.PickingCheap;
                if (Turn < CardsByState[PickDraftStateEnum.PickingGuard])
                    return PickDraftStateEnum.PickingGuard;
                if (Turn < CardsByState[PickDraftStateEnum.PickingStrong])
                    return PickDraftStateEnum.PickingStrong;
                return PickDraftStateEnum.PickingEquipment;
            }
        } 

        private static Dictionary<PickDraftStateEnum, int> CardsByState = new Dictionary<PickDraftStateEnum, int>() 
        {
            {PickDraftStateEnum.PickingCheap, 12},
            {PickDraftStateEnum.PickingGuard, 20},
            {PickDraftStateEnum.PickingStrong, 27},
            {PickDraftStateEnum.PickingEquipment, 30},
        };

        public static void PickDraft(List<Card> cards) 
        {
            for(var i = 0; i < cards.Count; i++)
                cards[i].InstanceId = i;

            var creatures = cards.Where(o => o.CardType == CardTypeEnum.Creature).ToList();
            var items = cards.Where(o => o.CardType != CardTypeEnum.Creature).ToList();
            
            switch(PickState) {
                case PickDraftStateEnum.PickingCheap:
                    
                    var bestCheapestCard = creatures.OrderBy(o => o.Cost).ThenByDescending(o => o.Attack).FirstOrDefault()
                        ?? items.FirstOrDefault();
                    Console.WriteLine($"PICK {bestCheapestCard.InstanceId}");

                    break;
                case PickDraftStateEnum.PickingGuard:

                    var guards = creatures.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard)).FirstOrDefault() ??
                                 creatures.OrderBy(o => o.Cost).ThenByDescending(o => o.Attack).FirstOrDefault() ??
                                 items.FirstOrDefault();
                    Console.WriteLine($"PICK {guards.InstanceId}");

                    break;
                case PickDraftStateEnum.PickingStrong:

                    var bestStrongestCard = creatures.OrderByDescending(o => o.Attack).ThenBy(o => o.Cost).FirstOrDefault() ?? items.First();
                    Console.WriteLine($"PICK {bestStrongestCard.InstanceId}");

                    break;
                case PickDraftStateEnum.PickingEquipment:
                    
                    var itemCard = items.FirstOrDefault() ?? creatures.OrderBy(o => o.Cost).ThenByDescending(o => o.Attack).FirstOrDefault();
                    Console.WriteLine($"PICK {itemCard.InstanceId}");

                    break;
            }

        }

        public static void MakeMove(List<Card> cards)
        {
            var hand = cards.Where(o => o.Location == LocationEnum.PlayersHand).ToList();
            var myField = cards.Where(o => o.Location == LocationEnum.PlayersSide).OrderByDescending(o => o.Attack).ToList();
            var opponentField = cards.Where(o => o.Location == LocationEnum.OpponentsSide).ToList();
            var opponentGuards = opponentField.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard));

            var isGuarded = myField.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard)).Any();

            var moves = new List<string>();
            
            // Define which card to summon
            if (myField.Count < 5 && hand.Count > 0) 
            {
                Card cardToDrawn = null;
                if (!isGuarded)
                    cardToDrawn = hand.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard) && o.Cost <= Me.Mana).OrderByDescending(o => o.Defense).FirstOrDefault()
                    
                if (cardToDrawn == null)
                    cardToDrawn = hand.Where(o => o.Cost <= Me.Mana).OrderByDescending(o => o.Attack).FirstOrDefault();

                if (cardToDrawn != null)
                    moves.Add($"SUMMON {cardToDrawn.InstanceId}");
            }

            // Define one attack per card in field
            foreach(var card in myField) 
            {
                if (!opponentGuards.Any())
                {
                    moves.Add($"ATTACK {card.InstanceId} -1");
                }
                else 
                {
                    moves.Add($"ATTACK {card.InstanceId} {opponentGuards.First().InstanceId}");
                }
            }

            Console.WriteLine(string.Join(";", moves));
        }
    }

    class Player  
    {
        public int Health { get; set; }
        public int Mana { get; set; }
        public int CardsInDeck { get; set; }
        public int Rune { get; set; }
        public List<Card> Deck { get; set;}

        public Player ()
        {
            Deck = new List<Card>();
        }

        public void Update(string[] inputs)
        {
            Health = int.Parse(inputs[0]);
            Mana = int.Parse(inputs[1]);
            CardsInDeck = int.Parse(inputs[2]);
            Rune = int.Parse(inputs[3]);
        }
    }

    class Card 
    {
        public int CardNumber { get; set; }
        public int InstanceId { get; set; }
        public LocationEnum Location { get; set; }
        public CardTypeEnum CardType { get; set; }
        public int Cost { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public HashSet<AbilitiesEnum> Abilities { get; set; }
        public int MyHealthChange { get; set; }
        public int OpponentHealthChange { get; set; }
        public int CardDraw { get; set; }
    }


    enum CardTypeEnum
    {
        Creature = 0,
        GreenItem = 1,
        RedItem = 2,
        BlueItem = 3
    }

    enum LocationEnum
    {
        PlayersHand = 0,
        PlayersSide = 1,
        OpponentsSide = -1
    }

    enum GamePhaseEnum
    {
        DraftPhase = 0,
        BattlePhase = 1
    }

    enum AbilitiesEnum
    {
        Breakthrough = 'B',
        Guard = 'G',
        Charge = 'C',
        Drain = 'D',
        Lethal = 'L',
        Ward = 'W'
    }

    enum PickDraftStateEnum
    {
        PickingCheap = 0,
        PickingGuard = 1,
        PickingStrong = 2,
        PickingEquipment = 3
    }
}