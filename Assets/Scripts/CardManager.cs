using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
{

    public List<Card> cards;
    public Stack<Card> discardPile = new Stack<Card>();
    
    //public readonly SyncList<Card> cards = new SyncList<Card>();

    private List<int> cardIndices;

    public static CardManager instance;

    public List<Player> Players;

    private bool cardsDelivered = false;

    public static event Action OnCardsDelivered;
    public static event Action OnSpawnCards;
    
    public GameObject Number;
    public GameObject Color;

    public GameObject cardArea1;
    public GameObject cardArea2;
    public GameObject UICard;

    public Transform deckPosition;

    public List<GameObject> CardAreas;

    public Player currentPlayer;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        cards = new List<Card>();
        cardIndices = new List<int>();

        if(isServer)
        {
            InitializeCards();
            ShuffleCards();
            SortDeck();
            //DisplayDeck();
        }
    }

    public bool PlayersAreValid()
    {
        foreach(Player player in Players)
        {
            if(player.name == "")
                return false;
        }
        return true;
    }

    public void Update()
    {
        if(Players.Count == 2 && PlayersAreValid())
        {
            if(cardsDelivered == false)
            {
                cardsDelivered = true;

                if(isServer)
                {
                    UIManager.instance.SpawnButton(new Vector3 (0,0,0),0, "Draw Card From Discard Pile");
                    UIManager.instance.SpawnButton(new Vector3 (500,0,0),1,"Draw Card from Deck");
                    UIManager.instance.SpawnButton(new Vector3 (1000,0,0),2,"Reset Move");
                }

                if(isServer)
                    DeliverCards();

                //OnCardsDelivered?.Invoke();

                if(isServer)
                {
                    OnSpawnCards?.Invoke();
                }

                //Assign Authority of all the player hands to the player(put this somewhere else later on)
                if(Players[0].hasAuthority)
                    Players[0].AddedCardsToHand();

                //Assign Authority of all the player hands to the player(put this somewhere else later on)
                if(Players[1].hasAuthority)   
                    Players[1].AddedCardsToHand();

                if(isServer)
                {
                    //put the top card of the deck on the table
                    UIManager.instance.SpawnMiddleCanvas();
                    Card card = cards[0];
                    cards.Remove(card);//Only the server removes cards
                    UIManager.instance.SpawnDiscardCard(card);
                    discardPile.Push(card);
                    
                    //spawn the ui deck next to the discard pile
                    UIManager.instance.SpawnUIDeck();

                }

                    //currentPlayer.Play();
                
                //The current player draws a card from the deck or from the middle canvas

                //Players[0].ShowHand();
                //Players[1].ShowHand(); 
                currentPlayer = Players[1];          
            } 
        }
    }

    public void swapPlayers()
    {
        //rpcSwapPlayers();
    }

    [ClientRpc]
    public void rpcSwapPlayers()
    {
        if(currentPlayer == Players[0])
            currentPlayer = Players[1];
        else
            currentPlayer = Players[0];
    }

    public void PushCardToDiscardPile(Card card)
    {
        discardPile.Push(card);
    }

    //Deliver 10 cards to the player and remove them from the deck
    //[Server]
    public void DeliverCards()
    {
        rpcDeliverCards();
    }

    //[ClientRpc]
    public void rpcDeliverCards()
    {
        Debug.Log("There are " + cards.Count  + " cards left");
        for(int i = 0; i < Players.Count;i++)
        {
            for(int j = 0; j < 10; j++)
            {
                Players[i].Hand.Add(cards[0]);
                cards.Remove(cards[0]);
            }
        }
    }    

    private void DisplayDeck()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            if(cards[i].IsJoker) Debug.Log(cards[i].DeckIndex +" JOKER");
            else if (cards[i].IsSkipCard) Debug.Log(cards[i].DeckIndex + " SKIP CARD");
            else Debug.Log(cards[i].DeckIndex + " " + cards[i].Number + " " + cards[i].Color);
        }
    }

    //Sort the cards on the deck so that the card with deckindex 1 is followed by the card with deckindex 2 and so on
    //Algorithm: bubble sort
    private void SortDeck()
    {
        Card temp;

        for (int i= 0; i < cards.Count; i++) {
            for (int j = 0; j < cards.Count - 1; j++) {
                if (cards[j].DeckIndex > cards[j + 1].DeckIndex) {
                    temp = cards[j + 1];
                    cards[j + 1] = cards[j];
                    cards[j] = temp;
                }
            }
        }
    }

    //shuffle the cards whenever a player won his phase
    private void ShuffleCards()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            int deckIndex = UnityEngine.Random.Range(0, cards.Count);

            while(cardIndices.Contains(deckIndex))
            {
                deckIndex = UnityEngine.Random.Range(0, cards.Count);
            }
            cards[i].DeckIndex = deckIndex;
            cardIndices.Add(deckIndex);
        }
        
    }

    //The game consists of 108 cards
    private void InitializeCards()
    {

        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"red"));
        }
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"red"));
        }

        
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"blue"));
        }
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"blue"));
        }

        //Add all cards of red color
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"green"));
        }
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"green"));
        }

        //Add all cards of red color
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"yellow"));
        }
        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"yellow"));
        } 

        //Add all jokers
        for(int i = 0; i < 8; i++)
        {   
            Card card = new Card();
            card.IsJoker = true;
            cards.Add(card);
        }

        //Add all skip cards
        for(int i = 0; i < 4; i++)
        {
            Card card = new Card();
            card.IsSkipCard = true;
            cards.Add(card);
        }
    }
}
