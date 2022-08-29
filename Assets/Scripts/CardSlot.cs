using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System;


public class CardSlot : NetworkBehaviour, IDropHandler
{
    [Serializable]
    public enum SlotType {DECKSLOT, DISCARDPILESLOT, PHASESLOT};

    //[Serializable]
    public SlotType slotType; 

    public static event Action<CardSlot, GameObject> OnDropEvent;

    public GameObject slotCard; //should be card object

    public int index = 0;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if(eventData.pointerDrag != null)
        {
            Debug.Log("dropped go is: " + eventData.pointerDrag.gameObject);
            OnDropEvent?.Invoke(this,eventData.pointerDrag.gameObject);
            //eventData.pointerDrag.transform.position = transform.position;
        }
    }

    public void SetCard(GameObject card)
    {
        slotCard = card;
        slotCard.transform.position = transform.position;
    }

    public void SetCard(Card card)
    {
        UIManager.instance.SetUICardDetails(slotCard,card);
    }

    public void ResetCard()
    {
        slotCard = null;
    }
    
}
