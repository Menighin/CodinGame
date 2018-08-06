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
            {PickDraftStateEnum.PickingCheap, 5},
            {PickDraftStateEnum.PickingGuard, 15},
            {PickDraftStateEnum.PickingStrong, 25},
            {PickDraftStateEnum.PickingEquipment, 30},
        };

        private static HashSet<int> BestCardsToPick = new HashSet<int>()
        {
            // Green Item
            118, // Give +3 Armor, costs 0

            // Red Item
            141, // Give stats -1/-1, costs 0
            142, // Remove all abilities, cost 0
            143, // Remove guard, cost 0
        };

        public static void PickDraft(List<Card> cards) 
        {
            for(var i = 0; i < cards.Count; i++)
                cards[i].InstanceId = i;
            
            Func<Card, bool> allTimeBestItem = (c) => BestCardsToPick.Contains(c.CardNumber);
            Func<Card, bool> alltimeBestCard = (c) => (c.Cost <= 5 && (c.Abilities.Contains(AbilitiesEnum.Guard) || c.Abilities.Contains(AbilitiesEnum.Lethal)));

            var creatures = cards.Where(o => o.CardType == CardTypeEnum.Creature).ToList();
            var items = cards.Where(o => o.CardType != CardTypeEnum.Creature).ToList();
            
            switch(PickState) {
                case PickDraftStateEnum.PickingCheap:
                    
                    var bestCheapestCard = items.Where(allTimeBestItem).FirstOrDefault()
                        ?? creatures.Where(alltimeBestCard).FirstOrDefault()
                        ?? creatures.OrderBy(o => o.Cost).ThenByDescending(o => o.Attack).FirstOrDefault()
                        ?? items.FirstOrDefault();
                    Console.WriteLine($"PICK {bestCheapestCard.InstanceId}");

                    break;
                case PickDraftStateEnum.PickingGuard:

                    var guards = items.Where(allTimeBestItem).FirstOrDefault()
                        ?? creatures.Where(alltimeBestCard).FirstOrDefault() 
                        ?? creatures.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard)).OrderByDescending(o => o.Defense).FirstOrDefault()
                        ?? creatures.OrderBy(o => o.Cost).ThenByDescending(o => o.Attack).FirstOrDefault()
                        ?? items.FirstOrDefault();

                    Console.WriteLine($"PICK {guards.InstanceId}");

                    break;
                case PickDraftStateEnum.PickingStrong:

                    var bestStrongestCard = items.Where(allTimeBestItem).FirstOrDefault()
                        ?? creatures.Where(alltimeBestCard).FirstOrDefault() 
                        ?? creatures.OrderByDescending(o => o.Attack).ThenBy(o => o.Cost).FirstOrDefault() ?? items.First();

                    Console.WriteLine($"PICK {bestStrongestCard.InstanceId}");

                    break;
                case PickDraftStateEnum.PickingEquipment:
                    
                    var itemCard = items.Where(allTimeBestItem).FirstOrDefault()
                        ?? creatures.Where(alltimeBestCard).FirstOrDefault()
                        ?? items.FirstOrDefault() ?? creatures.OrderBy(o => o.Cost).ThenByDescending(o => o.Attack).FirstOrDefault();
                    Console.WriteLine($"PICK {itemCard.InstanceId}");

                    break;
            }

        }

        public static void MakeMove(List<Card> cards)
        {
            var handCreatures = cards.Where(o => o.Location == LocationEnum.PlayersHand && o.CardType == CardTypeEnum.Creature && o.Cost <= Me.Mana).ToList();
            var handItems = cards.Where(o => o.Location == LocationEnum.PlayersHand && o.CardType != CardTypeEnum.Creature && o.Cost <= Me.Mana).ToList();
            var myField = cards.Where(o => o.Location == LocationEnum.PlayersSide).OrderByDescending(o => o.Attack).ToList();
            var opponentField = cards.Where(o => o.Location == LocationEnum.OpponentsSide).ToList();
            var opponentGuards = opponentField.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard)).ToList();

            var myGuards = myField.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard)).OrderByDescending(o => o.Defense).ToList();

            var isGuarded = myGuards.Count() >= 2;

            var moves = new List<string>();
            
            // Define item to use
            if (handItems.Any()) 
            {
                var handItemsNos = new HashSet<int>(handItems.Select(o => o.CardNumber).ToList());
                var usedItems = new HashSet<int>();

                // Buff my guard
                if (handItemsNos.Contains(118) && isGuarded) 
                {
                    var itemCard = handItems.Where(o => o.CardNumber == 118).First();
                    moves.Add($"USE {itemCard.InstanceId} {myGuards.First().InstanceId}");
                    usedItems.Add(itemCard.InstanceId);
                }

                // Remove guard abilities from enemy
                if (opponentGuards.Any() && (handItemsNos.Contains(142) || handItemsNos.Contains(143)))
                {
                    var itemCard = handItems.Where(o => o.CardNumber == 142 || o.CardNumber == 143).FirstOrDefault();
                    var enemyGuard = opponentGuards.FirstOrDefault();
                    moves.Add($"USE {itemCard.InstanceId} {enemyGuard.InstanceId}");

                    opponentGuards = opponentGuards.Where(o => o.InstanceId != enemyGuard.InstanceId).ToList();
                    usedItems.Add(itemCard.InstanceId);
                }

                var itemToUse = handItems.Where(o => !usedItems.Contains(o.InstanceId)).FirstOrDefault();
                if (itemToUse != null) {
                    if (itemToUse.CardType == CardTypeEnum.BlueItem)
                        moves.Add($"USE {itemToUse.InstanceId} -1");

                    else if (itemToUse.CardType == CardTypeEnum.RedItem && opponentField.Any())
                        moves.Add($"USE {itemToUse.InstanceId} {opponentField.First().InstanceId}");

                    else if (itemToUse.CardType == CardTypeEnum.RedItem && myField.Any())
                        moves.Add($"USE {itemToUse.InstanceId} {myField.First().InstanceId}");
                }

                

            }

            // Define which card to summon
            if (myField.Count < 5 && handCreatures.Count > 0) 
            {
                Card cardToDrawn = null;
                if (!isGuarded)
                    cardToDrawn = handCreatures.Where(o => o.Abilities.Contains(AbilitiesEnum.Guard)).OrderByDescending(o => o.Defense).FirstOrDefault();

                if (cardToDrawn == null)
                    cardToDrawn = handCreatures.OrderByDescending(o => o.Attack).FirstOrDefault();

                if (cardToDrawn != null)
                    moves.Add($"SUMMON {cardToDrawn.InstanceId}");
            }

            // Define one attack per card in field
            foreach(var myCard in myField) 
            {
                if (myCard.Attack == 0) continue;

                // Attack player directly
                if (!opponentGuards.Any())
                {
                    moves.Add($"ATTACK {myCard.InstanceId} -1");
                }
                // Attack the guardian it can
                else 
                {
                    Card bestGuardToAttack = null;
                    
                    // Guards it can kill
                    bestGuardToAttack = opponentGuards.Where(o => o.Defense <= myCard.Attack).FirstOrDefault();

                    // Or guards that don't kill me back
                    if (bestGuardToAttack == null)
                        bestGuardToAttack = opponentGuards.Where(o => o.Attack < myCard.Defense).FirstOrDefault();

                    if (bestGuardToAttack != null)
                        moves.Add($"ATTACK {myCard.InstanceId} {opponentGuards.First().InstanceId}");
                    else 
                        moves.Add($"ATTACK {myCard.InstanceId} -1");
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

    enum PlayerStateEnum
    {
        PlayAnyCard = 0,
        PlayStrongCard = 1,
    }
}