using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhaseChecker
{
    public List<Card> cards;

    public abstract bool Evaluate();

    public abstract bool CheckCards();

    public void DeleteCards(int checkIndex)
    {
        for(int i = checkIndex; i < cards.Count; i++)
            cards.Remove(cards[checkIndex]);
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
