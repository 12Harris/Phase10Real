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
    public List<CardSlot> phaseSlots;
    public List<GameObject> phaseCards;
    public float timer = 0.5f;
    bool updateCards = false;
    bool updatePhaseSlotCards = false;

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

        if(card == null)
            Debug.Log("card is null");

        if(cardSlot.slotType == CardSlot.SlotType.PHASESLOT)
        {

            if(cardSlot.slotCard != null)
            {
                Debug.Log("cardslot already has a card!");
                card.transform.position = card.GetComponent<UICardData>().InitialPosition;
                return;
            }

            if(cardSlot.index > currentPhase.cards.Count)
            {
                Debug.Log("cant put card into this slot!");
               card.transform.position = card.GetComponent<UICardData>().InitialPosition;
                return;
            }

            cardSlot.SetCard(card);

            //if(!phaseSlots.Contains(cardSlot))
            {
                phaseSlots.Add(cardSlot);
            }

            currentPhase.cards.Add(selectedCard);

            cmdRemoveSelectedCardFromUIHand();
            cmdRemoveSelectedCardFromHand();
            cmdSetSelectedCardIndex(0);


            /*if(currentPhase.cards.Count > currentPhase.checkIndex+1)
            {
                if(currentPhase.CheckCards() == false)
                {
                    Debug.Log("the cards dont match!");
                    Debug.Log("ui hand has " + UIHand.Count + " cards");
                    Debug.Log("hand has " + Hand.Count + " cards");

                    //if(currentPhase.cards.Count == 0)
                    //StartCoroutine(UpdatePhaseSlotCards());
  
                    for(int i = currentPhase.checkIndex; i < phaseSlots.Count; i++)
                    {
                        AddCardToHand(currentPhase.cards[i],phaseSlots[i].slotCard) ; //HERE I AM.............................................................//
                        //UIHand[Hand.Count-3].transform.Find("Highlight").gameObject.SetActive(true);
                    }
                    updatePhaseSlotCards = true;
                    //Add the cards back to the ui hand

                    //card.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;

                    return;
                }
            }*/

            //currentPhase.cards.Add(selectedCard);
            Debug.Log("Removing card " + selectedCardIndex + " from hand");

            //Reposition the ui hand cards
            //RepositionUIHandCards();
        }


        else if(cardSlot.slotType == CardSlot.SlotType.DISCARDPILESLOT)
        {         
            //We have to remove all cards from the phase slots before we can make the final move
            if(currentPhase.cards.Count > 0)
            {   
                card.transform.position = card.GetComponent<UICardData>().InitialPosition;
                return;
            }
            pushCardToDiscardPile = true;
            cmdPushCardToDiscardPile(selectedCard);
            cmdUpdateDiscardPile();         
        }

        
    }

    private void RepositionUIHandCards()
    {
        Debug.Log("I have " + UIHand.Count + "cards on my hand.");//hand should exclude these cards at this point
        
        for(int i = selectedCardIndex; i < UIHand.Count; i++)
        {
            UIHand[i].transform.position = new Vector3(UIHand[i].transform.position.x-200, UIHand[i].transform.position.y, UIHand[i].transform.position.z);
            UIHand[i].GetComponent<UICardData>().InitialPosition = UIHand[i].transform.position;
        }
    }

    private void UpdatePhaseSlotCards()
    {
        //yield return new WaitForSeconds(.2f);

        UIHand[Hand.Count-1].transform.Find("Highlight").gameObject.SetActive(true);
      

        UIHand[Hand.Count-1].transform.position = UIHand[Hand.Count-2].GetComponent<UICardData>().InitialPosition +  new Vector3(200,0,0);
        UIHand[Hand.Count-1].GetComponent<UICardData>().InitialPosition =  UIHand[Hand.Count-1].transform.position;
        
        //updatePhaseSlotCards = false;

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

        //attempt to reset the current players move
        else if(button.index == 2)
        {
            cmdResetMove();
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

    public GameObject getHandCardAtIndex(int index)
    {
        if(index < 0  || index >= UIHand.Count)
            return null;

        return UIHand[index];
    }

    [Command]
    public void cmdResetMove()
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

    [Command]
    private void AddCardToHand(Card card, GameObject uiCard)
    {
        AddCardToUIHand(uiCard);
        Hand.Add(card);
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

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
         worldPos.z = card.transform.position.z;

        if(card.GetComponent<DragDrop>().dragging)
        {

            Debug.Log("dragging card");
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

                        //card.transform.position = leftCard.transform.position;
                        //leftCard.transform.position = card.GetComponent<UICardData>().InitialPosition;

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

                        //Hand[i].transform.position = rightCard.transform.position;
                        //rightCard.transform.position = UIHand[i].GetComponent<UICardData>().InitialPosition;


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
                    //card.transform.position = card.GetComponent<UICardData>().InitialPosition;
                }
                else
                {
                    if(card.GetComponent<DragDrop>().dropped == false)
                    {
                        Debug.Log("GRRRMPF!!");
                        card.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;
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
            
            /*if(updatePhaseSlotCards)
            {
                for(int i = currentPhase.checkIndex; i < phaseSlots.Count; i++)
                {
                    
                    phaseSlots[i].slotCard.transform.position = UIHand[Hand.Count-3].GetComponent<UICardData>().InitialPosition +  new Vector3(200 * i+1,0,0);
                    //phaseSlots[i].ResetCard();
                }

                for(int i = currentPhase.checkIndex; i < phaseSlots.Count; i++)
                    phaseSlots.Remove(phaseSlots[0]);
                updatePhaseSlotCards = false;
            }*/

            timer -= Time.deltaTime;
            if(timer < 0.0)
            {
                timer = 0.5f;
                //Debug.Log("I have " + UIHand.Count + " cards on my hand");
            }

            if(selectedCard != null && pushCardToDiscardPile == true)
            {
                cmdRemoveSelectedCardFromHand();
                cmdRemoveSelectedCardFromUIHand();
                CmdDestroyObject(selectedUICard);
                cmdSetSelectedUICard(UIHand[0]);
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
        
        Debug.Log("Hand changed:" + op);
        Debug.Log("There are "+  UIHand.Count + " cards on my hand");

        switch (op)
        {
            case SyncList<Card>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                Debug.Log("Card was added to list");
                if(updatePhaseSlotCards)
                    UpdatePhaseSlotCards();
                    //StartCoroutine(UpdatePhaseSlotCards());
                break;

            case SyncList<Card>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<Card>.Operation.OP_REMOVEAT:
                Debug.Log("Card was removed at an index");
                RepositionUIHandCards();


                if(currentPhase.cards.Count > currentPhase.checkIndex+1)
                {
                    if(currentPhase.CheckCards() == false)
                    {
                        Debug.Log("the cards dont match!");
                        Debug.Log("ui hand has " + UIHand.Count + " cards");
                        Debug.Log("hand has " + Hand.Count + " cards");

                        //if(currentPhase.cards.Count == 0)
                        //StartCoroutine(UpdatePhaseSlotCards());
    
                        for(int i = currentPhase.checkIndex; i < phaseSlots.Count; i++)
                        {
                            AddCardToHand(currentPhase.cards[i],phaseSlots[i].slotCard) ; //HERE I AM.............................................................//
                            //UIHand[Hand.Count-3].transform.Find("Highlight").gameObject.SetActive(true);
                        }
                        updatePhaseSlotCards = true;
                        //Add the cards back to the ui hand

                        //card.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;

                        return;
                    }
                }
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<Card>.Operation.OP_SET:
                Debug.Log("Cards have been swapped");
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<Card>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }

}