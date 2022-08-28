using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;
using TMPro;

public class Player : NetworkBehaviour
{
    [SerializeField] [SyncVar] public string name = "";
    public string Name {get {return name;} set {name = value;}}

    //[field: SerializeField] public CharacterController Controller {get; private set;}

    //public List<Card> Hand;

    //public readonly SyncList<Card> Hand = new SyncList<Card>();
    public SyncList<Card> Hand = new SyncList<Card>();

    //public readonly SyncList<GameObject> UIHand = new SyncList<GameObject>();

    public List<GameObject> UIHand = new List<GameObject>();

    GameObject leftCard = null;

    GameObject rightCard = null;

    [SyncVar (hook=nameof(OnNewCardChanged))]Card newCard;

    public GameObject handDisplayPrefab;

    GameObject handDisplay;

    public GameObject cardArea;
    public GameObject cardAreaPrefab;

    [SyncVar]public Card selectedCard;
    [SyncVar] private int selectedCardIndex;
    public GameObject selectedUICard;

    bool popcardFromDiscardPile = false;
    bool drawCardFromDeck = false;

    public bool playing = false;
    //[SyncVar] public bool playing = false;

    public bool pushCardToDiscardPile = false;
    //[SyncVar (hook = nameof(ShowHand))] public bool HandCardsChanged = false;
    [SyncVar] public bool HandCardsChanged = false;
    PhaseChecker currentPhase;
    public List<CardSlot> cardSlots;
    public List<GameObject> phaseCards;

    public void Awake()
    {
        Button.OnClicked += OnButtonClicked;
        CardSlot.OnDropEvent += OnCardSlotDropped;
        currentPhase = new Phase1Checker(6);
    }

    public void OnCardSlotDropped(CardSlot cardSlot, GameObject card)
    {
        if(!hasAuthority) return;
  
        Debug.Log("dropped card to slot");

        card.GetComponent<DragDrop>().dropped = true;

        if(cardSlot.slotType == CardSlot.SlotType.PHASESLOT)
        {
            if(!cardSlots.Contains(cardSlot))
            {

                cardSlots.Add(cardSlot);
            }

            if(currentPhase.cards.Count < currentPhase.maxCount)
            {
                if(selectedCard == null) Debug.Log("selected card is null");
                currentPhase.cards.Add(selectedCard);
                Debug.Log("current phase has: " + currentPhase.cards.Count);
                phaseCards.Add(card);

                if(currentPhase.cards.Count > 2)
                {
                    bool eval = currentPhase.Evaluate();
                    if(eval == false)
                    {
                        Debug.Log("After eval: current phase has: " + currentPhase.cards.Count);
                        UpdateCardSlots();
                    }
                }
            }

            //card.transform.position = cardSlot.transform.position;
        }
        else if(cardSlot.slotType == CardSlot.SlotType.DISCARDPILESLOT)
        {         
            pushCardToDiscardPile = true;
            cmdPushCardToDiscardPile(selectedCard);
            cmdUpdateDiscardPile();         
        }
    }

    private void UpdateCardSlots()
    {
        if(currentPhase.cards.Count == 0)
        {
            Debug.Log("current phase has 0 cards");
            Debug.Log("There are " + phaseCards.Count + " phase cards");
            for(int i = 0; i < cardSlots.Count; i++)
            {
                //cardSlots[i].ResetCard();
                GameObject card = phaseCards[i];
                phaseCards.Remove(phaseCards[i]);
                CmdDestroyObject(card);
            }
            return;
        }

        for(int i = 0; i < currentPhase.minCount; i++)
        {
            cardSlots[i].SetCard(phaseCards[i]);
        }
        for(int i = currentPhase.minCount; i < currentPhase.cards.Count; i++)
        {
            phaseCards.Remove(phaseCards[i]);
            CmdDestroyObject(phaseCards[i]);
        }
    }

    [Command]
    private void CmdDestroyObject(GameObject obj)
    {
        if(!obj) return;
        
        NetworkServer.Destroy(obj);
    }

    [Command]
    public void cmdPushCardToDiscardPile(Card card)
    {
        CardManager.instance.PushCardToDiscardPile(selectedCard);
    }

    public void OnNewCardChanged(Card oldCard, Card newCard)
    {
        Debug.Log("new card changed");

        if(hasAuthority)
        {
            AddCardToHand(newCard);
        }

        if(popcardFromDiscardPile)
        {
            //If the discard pile has conains more than 1 card then update it
            cmdUpdateDiscardPile();

            popcardFromDiscardPile = false;
        }

        else if(drawCardFromDeck)
        {
            cmdUpdateDeck();

            drawCardFromDeck = false;
        }

    }

    [Command]
    public void cmdUpdateDeck()
    {
        if(CardManager.instance.cards.Count > 0)
        {
            UIManager.instance.UpdateUIDeck();
        }
    }

    [Command]
    public void cmdUpdateDiscardPile()
    {
        if(CardManager.instance.discardPile.Count > 0)
        {
            rpcSetDiscardCardActive(true);
            Card card = CardManager.instance.discardPile.Peek();
            if(card == null) Debug.Log("card is null im cmdupdatedp");
            UIManager.instance.UpdateDiscardPile();
        }
         else
            rpcSetDiscardCardActive(false);  
    }

    public void OnButtonClicked(Button button)
    {
        if(!hasAuthority) return;
        if(CardManager.instance.currentPlayer != this) return;

        playing = true;
        
        //attempt to draw a card from the discard pile and add it to the players hand
        if(button.index == 0)
        {

            popcardFromDiscardPile = true;
            cmdPopCardFromDiscardPile();

            if(newCard == null)
                Debug.Log("new card is null in onbuttonclicked");
        }

        //attempt to draw a card from the deck and add it to the players hand
        else if(button.index == 1)
        {
            drawCardFromDeck = true;

            cmdDrawCardFromDeck();
        }
    }

    public void Start()
    {
        CardManager.instance.Players.Add(this);

        handDisplay = Instantiate(handDisplayPrefab,transform.position, transform.rotation);

        Hand.Callback +=  ShowHand;
        
        //CardManager.OnCardsDelivered += ShowHand;
        //UICardData.OnSwapCards += onSwapCards;
    }

    public void Play()
    {
    }

    [Command]
    public void cmdDrawCardFromDeck()
    {
        newCard = CardManager.instance.cards[0];
        CardManager.instance.cards.Remove(CardManager.instance.cards[0]);
    }

    [Command]
    public void cmdPopCardFromDiscardPile()
    {
        newCard = CardManager.instance.discardPile.Pop();
    }

    //NOT NEEDED

    /*[ClientRpc]
    public void rpcSetDiscardCard(Card card)
    {
        newCard = card;
    }*/

    [Command]
    public void cmdSetDiscardCardActive(bool active)
    {
        UIManager.instance.discardCard.SetActive(active);
       //rpcDeactivateDiscardCard();
    }

    [Command]
    public void cmdDeactivateUIDeck()
    {
       rpcDeactivateUIDeck();
    }

    [ClientRpc]
    public void rpcSetDiscardCardActive(bool active)
    {
        UIManager.instance.discardCard.SetActive(active);
    }

    [ClientRpc]
    public void rpcDeactivateUIDeck()
    {
        UIManager.instance.UIDeck.SetActive(false);
    }

    [Command]
    public void AddCardToHand(Card card)
    {
        GameObject uiCard = UIManager.instance.SpawnCardAtPosition(this, card, new Vector3(UIHand[Hand.Count-1].transform.position.x+200,0,0));
        AddCardToUIHand(uiCard);
        Hand.Add(card);
        HandCardsChanged = true;
    }

    [ClientRpc]
    void AddCardToUIHand(GameObject card)
    {
        UIHand.Add(card);
    }

    [Command]
    public void cmdRemoveSelectedCardFromUIHand()
    {
        rpcRemoveSelectedCardFromUIHand();
    }

    [ClientRpc]
    public void rpcRemoveSelectedCardFromUIHand()
    {
        Debug.Log("removing selected ui card");
        //if(selectedUICard == null) Debug.Log("card to remove is null");
        UIHand.Remove(selectedUICard);

        if(hasAuthority)
        {
            //selectedUICard.GetComponent<DragDrop>().enabled = false;
            //selectedUICard.GetComponent<UICardData>().enabled = false;
            CmdDestroyObject(selectedUICard);
            cmdSetSelectedUICard(UIHand[0]);
            //selectedUICard = UIHand[0];
        }


    }

    [Command]
    public void cmdRemoveCardFromHand(Card card)
    {
        if(Hand.Remove(selectedCard)) Debug.Log("remove op succeded");
    }

     [Command]
    public void cmdRemoveSelectedCardFromHand()
    {
        Hand.Remove(Hand[selectedCardIndex]);
    }

    //[Command]
    void cmdSwapUIHandCards(int firstCardIndex, int secondCardIndex)
    {
        rpcSwapUIHandCards(firstCardIndex, secondCardIndex);
    }

    //[ClientRpc]
    private void rpcSwapUIHandCards(int firstCardIndex, int secondCardIndex)
    {
        GameObject temp2 = UIHand[firstCardIndex];
        UIHand[firstCardIndex] = UIHand[secondCardIndex];
        UIHand[secondCardIndex] = temp2;
    }

    [Command]
    private void cmdSwapHandCards(int firstCardIndex, int secondCardIndex)
    {
        Card temp = Hand[firstCardIndex];
        Hand[firstCardIndex] = Hand[secondCardIndex];
        Hand[secondCardIndex] = temp;
        //ShowHand();
    }

    [Command]
    private void cmdSetSelectedUICard(GameObject obj)
    {
        rpcSetSelectedUICard(obj);
    }

    [ClientRpc]
    private void rpcSetSelectedUICard(GameObject obj)
    {
        if(obj == null)
            Debug.Log("obj is null");
        selectedUICard = obj;
    }

    [Command]
    private void cmdSetSelectedCard(Card card)
    {
        selectedCard = card;
    }

    [Command]
    private void cmdSetSelectedCardIndex(int index)
    {
        selectedCardIndex = index;
    }

    //[Command]
    void cmdMoveCard(GameObject card , int i)
    {
        rpcMoveCard(card, i);
    }

    //[ClientRpc]
    void rpcMoveCard(GameObject card, int i)
    {
        if(i > 0) leftCard = UIHand[i-1];
        if(i < UIHand.Count-1) rightCard = UIHand[i+1];

        if(card.GetComponent<DragDrop>().dragging)
        {
            //selectedCard = Hand[i];
            cmdSetSelectedCard(Hand[i]);
            cmdSetSelectedCardIndex(i);
            cmdSetSelectedUICard(UIHand[i]);
            
            if(playing == false)
            {
                if(leftCard != null)
                    leftCard.transform.Find("Highlight").gameObject.SetActive(true);
                if(rightCard != null)
                    rightCard.transform.Find("Highlight").gameObject.SetActive(true);
                
                if(leftCard != null)
                {
                    //leftCard.transform.Find("Highlight").gameObject.SetActive(true);
                    if(card.transform.position.x < leftCard.transform.position.x)
                    { 
                        leftCard.transform.Find("Highlight").gameObject.SetActive(false);
                        if(rightCard != null) rightCard.transform.Find("Highlight").gameObject.SetActive(false);
                        
                        card.transform.localPosition = leftCard.transform.localPosition;
                        leftCard.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;

                        Vector3 temp = leftCard.GetComponent<UICardData>().InitialPosition;
                        leftCard.GetComponent<UICardData>().InitialPosition = card.GetComponent<UICardData>().InitialPosition;
                        card.GetComponent<UICardData>().InitialPosition = temp;

                        if(hasAuthority)
                        {
                            cmdSwapHandCards(i,i-1);
                            cmdSwapUIHandCards(i,i-1);
                            HandCardsChanged = true;
                        }
                        
        
                        return; 
                    }
                }

                if(rightCard != null)
                {
                    //rightCard.transform.Find("Highlight").gameObject.SetActive(true);
                    if(UIHand[i].transform.position.x > rightCard.transform.position.x)
                    {   
                        rightCard.transform.Find("Highlight").gameObject.SetActive(false);
                        if(leftCard != null) leftCard.transform.Find("Highlight").gameObject.SetActive(false);
                        
                        UIHand[i].transform.localPosition = rightCard.transform.localPosition;
                        rightCard.transform.localPosition = UIHand[i].GetComponent<UICardData>().InitialPosition;

                        Vector3 temp = rightCard.GetComponent<UICardData>().InitialPosition;
                        rightCard.GetComponent<UICardData>().InitialPosition = card.GetComponent<UICardData>().InitialPosition;
                        card.GetComponent<UICardData>().InitialPosition = temp;

                        if(hasAuthority)
                        {
                            cmdSwapHandCards(i,i+1);
                            cmdSwapUIHandCards(i,i+1); 
                            HandCardsChanged = true; 
                        }

                        return;
                    }
                }
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = card.transform.position.z;
            //card.transform.position = new Vector3(worldPos.x,card.transform.position.y,worldPos.z);
            if(playing == false)
                card.transform.position = new Vector3(worldPos.x,card.transform.position.y,worldPos.z);
            else
               card.transform.position = new Vector3(worldPos.x,worldPos.y,worldPos.z);
        }

        else
        {
            //Once we stopped dragging the active card we snap it to the position of the card that we swapped with
            //check if we stopped dragging the active card
            if(card.GetComponent<UICardData>().active)//replace with : "if(selectedCard != null)"
            {
                if(playing == false)
                {
                    if(leftCard != null) leftCard.transform.Find("Highlight").gameObject.SetActive(false);
                    if(rightCard != null) rightCard.transform.Find("Highlight").gameObject.SetActive(false);

                    card.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;
                }
                else
                {
                    if(card.GetComponent<DragDrop>().dropped == false)
                    {
                        card.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;
                    }
                    else
                    {
                       
                    }
                }

                //selectedCard = null;
                card.GetComponent<UICardData>().active = false;
            }
        }
    }
    
    public void Update()
    {
        if(hasAuthority)
        {

            if(selectedCard != null && pushCardToDiscardPile == true)
            {
                cmdRemoveSelectedCardFromHand();
                cmdRemoveSelectedCardFromUIHand();
                //CmdDestroyObject(selectedUICard);
                pushCardToDiscardPile = false;

                cmdSwapPlayers();
                return;
            }

            for(int i = 0; i < UIHand.Count; i++)
            {
                cmdMoveCard(UIHand[i] , i);
            }
            
        }
    }

    [Command]
    void cmdSwapPlayers()
    {
        CardManager.instance.swapPlayers();
    }

    //Assign authority to our hand
    [Command]
    public void AddedCardsToHand()
    {
        foreach(GameObject card in UIHand)
        {
            card.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient); 
        }
    }

    public void ShowHand(SyncList<Card>.Operation op, int index, Card oldItem, Card newItem)
    //public void ShowHand(bool oldHandCardsChanged, bool newHandCardsChanged)
    {
        handDisplay.GetComponent<TextMeshPro>().text = "";

        //Debug.Log("Hand of " + name + ":");
        string text;

        for(int i = 0; i < Hand.Count; i++)
        {
            if(Hand[i].IsJoker) text = i + " JOKER";
            else if (Hand[i].IsSkipCard) text = i + " SKIP CARD";
            else text = i + " " + Hand[i].Number + " " + Hand[i].Color;

            handDisplay.GetComponent<TextMeshPro>().text += text + '\n';
        }
    }
}