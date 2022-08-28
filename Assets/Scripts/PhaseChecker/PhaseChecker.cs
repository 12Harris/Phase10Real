using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhaseChecker
{
    public List<Card> cards;

    public abstract bool Evaluate();

    public PhaseChecker nextPhaseChecker;

    public int minCount, maxCount;

    public PhaseChecker(int maxCount)
    {
        minCount = 0;
        this.maxCount = maxCount;
        cards = new List<Card>();
    } 
}
