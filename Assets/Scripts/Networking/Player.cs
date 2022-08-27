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

    UICardData card;

    public GameObject handDisplayPrefab;

    GameObject handDisplay;

    int i;

    public GameObject cardArea;
    public GameObject cardAreaPrefab;


    public void Awake()
    {
        Hand.Callback +=  ShowHand;
    }

    public void Start()
    {
        CardManager.instance.Players.Add(this);

        handDisplay = Instantiate(handDisplayPrefab,transform.position, transform.rotation);
        
        //CardManager.OnCardsDelivered += ShowHand;
        //UICardData.OnSwapCards += onSwapCards;
        i = 5;
    }

    public void Play()
    {
        if(UIManager.instance.discardCard.GetComponent<DragDrop>().clicked)
        {
            Card discardCard = CardManager.instance.discardPile.Pop();

            AddCardToHand(discardCard);

            //If the pile has conains more than 1 discard card then update it
            if(CardManager.instance.discardPile.Count > 0)
            {
                UIManager.instance.UpdateDiscardCard(CardManager.instance.discardPile.Peek());
            }

            UIManager.instance.discardCard.GetComponent<DragDrop>().clicked = false;
        }
    }

    public void AddCardToHand(Card card)
    {
        //Hand.Add(card);
        GameObject uiCard = UIManager.instance.SpawnCardAtPosition(this, card, new Vector3(UIHand[Hand.Count-1].transform.position.x+200,0,0));
        UIHand.Add(uiCard);
        Hand.Add(card);
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


    //[Command]
    void cmdMoveCard(GameObject card , int i)
    {
        rpcMoveCard(card, i);
    }

    //[ClientRpc]
    void rpcMoveCard(GameObject card, int i)
    {
        if(i > 0) leftCard = UIHand[i-1];
        if(i < 9) rightCard = UIHand[i+1];

        int moveDirection = 0;

        if(card.GetComponent<DragDrop>().dragging)
        {
            
            if(leftCard != null)
                leftCard.transform.Find("Highlight").gameObject.SetActive(true);
            if(rightCard != null)
                rightCard.transform.Find("Highlight").gameObject.SetActive(true);
            
            if(leftCard != null)
            {
                moveDirection = -1;
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
                    }

                    return;
                }
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = card.transform.position.z;
            card.transform.position = new Vector3(worldPos.x,card.transform.position.y,worldPos.z);
        }

        else
        {
            //Once we stopped dragging the active card we snap it to the position of the card that we swapped with
            //check if we stopped dragging the active card
            if(card.GetComponent<UICardData>().active)
            {
                if(leftCard != null) leftCard.transform.Find("Highlight").gameObject.SetActive(false);
                if(rightCard != null) rightCard.transform.Find("Highlight").gameObject.SetActive(false);

                card.transform.localPosition = card.GetComponent<UICardData>().InitialPosition;

                card.GetComponent<UICardData>().active = false;
            }
        }
    }
    
    public void Update()
    {
        if(hasAuthority)
        {
            for(int i = 0; i < UIHand.Count; i++)
            {
                cmdMoveCard(UIHand[i] , i);
            }
        }
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
    //public void ShowHand()
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
