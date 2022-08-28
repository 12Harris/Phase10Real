using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

public class UIManager : NetworkBehaviour
{
    public GameObject cardAreaPrefab;
    public GameObject UICard;
    GameObject cardArea;
    GameObject middleCanvas;
    public GameObject discardCard;
    public GameObject UIDeck;
    public GameObject ButtonPrefab;

    public static UIManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CardManager.OnSpawnCards += SpawnCards;
    }

    public void SpawnButton(Vector3 position,int index,string text)
    {   

        foreach(Player player in CardManager.instance.Players)
        {
            GameObject Button = Instantiate(ButtonPrefab) as GameObject;

            //uiCard.transform.position = position

            Button.transform.localPosition = position;

            Button.GetComponent<Button>().index = index;
            Button.GetComponent<Button>().text = text;

            NetworkServer.Spawn(Button, player.connectionToClient);

            RpcDeactivateButton(player, Button);
        }
    }

    [ClientRpc] 
    void RpcDeactivateButton(Player player, GameObject button)
    {
        if(!player.hasAuthority)
            button.SetActive(false);
    }

    public void SpawnDiscardCard(Card card)
    {
        
        discardCard = InstantiateUICardfromCard(card);

        //uiCard.transform.position = position

        discardCard.transform.position = new Vector3(350,0f,0f);

        NetworkServer.Spawn(discardCard);

        RpcSetDiscardCard(discardCard);

        RpcParentCardToMiddleCanvas(discardCard);
    }


    public void SpawnUIDeck()
    {
        UIDeck = InstantiateUICardfromCard(CardManager.instance.cards[0]);

        //the deck cards are not visible to the player
        UIDeck.GetComponent<UICardData>().text = "";
        UIDeck.GetComponent<UICardData>().color = new Color(0,127,127);
        UIDeck.GetComponent<UICardData>().color.a = 0.5f;

        UIDeck.transform.position = new Vector3(700,0f,0f);

        NetworkServer.Spawn(UIDeck);

        //RpcSetUIDeck(UIDeck);

        RpcParentCardToMiddleCanvas(UIDeck);
    }

    [ClientRpc]
    void RpcSetUIDeck(GameObject UIDeck)
    {
        this.UIDeck = UIDeck;
    }

    [ClientRpc]
    void RpcSetDiscardCard(GameObject discardCard)
    {
        this.discardCard = discardCard;
    }

    [Server]
    public GameObject SpawnCardAtPosition(Card card, Vector3 position)
    {
        GameObject uiCard = InstantiateUICardfromCard(card);

        //uiCard.transform.position = position

        uiCard.transform.position = position;

        NetworkServer.Spawn(uiCard);

        rpcSetCardInitialPosition(uiCard);

        return uiCard;
        
    }

    [Server]
    public GameObject SpawnCardAtPosition(Player player, Card card, Vector3 position)
    {
        GameObject uiCard = InstantiateUICardfromCard(card);

        //uiCard.transform.position = position

        uiCard.transform.localPosition = position;

        NetworkServer.Spawn(uiCard,player.connectionToClient);

        rpcSetCardInitialPosition(uiCard);

        RpcParentCardToCanvas(player,uiCard);

        return uiCard;
        
    }

    public void UpdateUIDeck()
    {
        SetUICardDetails(UIDeck, CardManager.instance.cards[0]);
    }

    public void UpdateDiscardPile()
    {
       rpcUpdateDiscardPile();
    }

    public void rpcUpdateDiscardPile()
    {
        Card card = CardManager.instance.discardPile.Peek();
        SetUICardDetails(discardCard, card);
    }

    public void SetUICardDetails(GameObject cardUi, Card card)
    {

        if(card.IsJoker)
            cardUi.GetComponent<UICardData>().text = "JOKER";
        
        else if(card.IsSkipCard)
            cardUi.GetComponent<UICardData>().text = "SKIPCARD";

        else
            cardUi.GetComponent<UICardData>().text = card.Number.ToString();

        switch(card.Color)
        {
            case "red":
                cardUi.GetComponent<UICardData>().color = new Color(255,0,0);
                break;
            case "green":
                cardUi.GetComponent<UICardData>().color = new Color(0,255,0);
                break;
            case "blue":
                cardUi.GetComponent<UICardData>().color = new Color(0,0,255);
                break;
            case "yellow":
                cardUi.GetComponent<UICardData>().color = new Color(255,255,0);
                break;
            default:
                cardUi.GetComponent<UICardData>().color = new Color(127,127,127);
                break;
        }
    }

    public GameObject InstantiateUICardfromCard(Card card)
    {
        GameObject uiCard = Instantiate(UICard) as GameObject;

        SetUICardDetails(uiCard, card);
        return uiCard;
    }

    /*public void SpawnCardAreas()
    {
        foreach(Player player in CardManager.instance.Players)
        {
           
            if(player.Name == "Player 1") 
            {
                cardArea = Instantiate(cardAreaPrefab, new Vector3(0,-100,0), Quaternion.identity) as GameObject;
            }
            else
            {
                cardArea = Instantiate(cardAreaPrefab, new Vector3(0,1000,0), Quaternion.identity) as GameObject;
            }

            NetworkServer.Spawn(cardArea);
        }
    }*/

    public void SpawnMiddleCanvas()
    {
        middleCanvas = Instantiate(cardAreaPrefab, new Vector3(0,500,0), Quaternion.identity) as GameObject;
        NetworkServer.Spawn(middleCanvas);
        rpcSetMiddleCanvas(middleCanvas);
    }

    [ClientRpc]
    public void rpcSetMiddleCanvas(GameObject middleCanvas)
    {
        this.middleCanvas = middleCanvas;
    }

    [ClientRpc]
    void RpcParentCardToMiddleCanvas(GameObject card)
    {
        card.transform.SetParent(middleCanvas.transform.Find("CardArea").transform, false);
    }

    public void SpawnCards()
    {//Vector3 position = Vector3.zero;
        //GameObject cardArea;
        foreach(Player player in CardManager.instance.Players)
        {
            ///cardArea.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);

            if(player.name == "Player 1")
                cardArea = Instantiate(cardAreaPrefab, new Vector3(0,-100,0), Quaternion.identity) as GameObject;
            else
                cardArea = Instantiate(cardAreaPrefab, new Vector3(0,1000,0), Quaternion.identity) as GameObject;
            
            NetworkServer.Spawn(cardArea);

            rpcSetCardArea(player, cardArea);
            
            float xOffset = - 200f;
            
            for(int i = 0; i < player.Hand.Count; i++)
            {
                GameObject card = InstantiateUICardfromCard(player.Hand[i]);
               
                card.GetComponent<UICardData>().deckIndex= player.Hand[i].DeckIndex;
                card.GetComponent<UICardData>().HandIndex= i;

                card.transform.position = new Vector3(xOffset-200,0f,0f);
               
                NetworkServer.Spawn(card);
                rpcSetCardInitialPosition(card);
                RpcParentCardToCanvas(player, card);
                xOffset += 200;
                //card.transform.SetParent(cardArea.transform.Find("CardArea").transform, false);

                player.UIHand.Add(card);
                rpcPlayerUIHandAdd(player, card);
            }
        }
    }

    [ClientRpc]
    void rpcSetCardArea(Player player, GameObject cardArea)
    {
        player.cardArea = cardArea;
    }

    [ClientRpc]
    void rpcSetCardInitialPosition(GameObject card)
    {
        card.GetComponent<UICardData>().InitialPosition = card.transform.position;
    }

    [ClientRpc]
    void rpcPlayerUIHandAdd(Player player, GameObject card)
    {
        if(isClientOnly)
            player.UIHand.Add(card);
    }

    [ClientRpc]
    void RpcParentCardToCanvas(Player player, GameObject card)
    {

        if(player.cardArea == null) Debug.Log("player is null");
            //card.transform.SetParent(cardArea.transform.Find("CardArea").transform, false);
            card.transform.SetParent(player.cardArea.transform.Find("CardArea").transform, false);
    }
}
