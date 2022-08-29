using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhaseChecker
{
    public List<Card> cards;

    public abstract bool Evaluate();

    public abstract bool CheckCards(GameObject card);

    public void DeleteCards()
    {
        while(cards.Count > 0)
            cards.Remove(cards[0]);
    }

    public PhaseChecker nextPhaseChecker;

    public int checkIndex, maxCount;

    public PhaseChecker(int maxCount)
    {
        checkIndex = 0;
        this.maxCount = maxCount;
        cards = new List<Card>();
    } 
}
