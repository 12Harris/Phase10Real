using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class checks whether we completed the first phase (hand contains 2 sets of 3 same cards)

public class Phase1Checker : PhaseChecker
{   
    public Phase1Checker(int maxCount) : base(maxCount) {nextPhaseChecker = new Phase2Checker(maxCount);}

    public override bool CheckCards(GameObject card)
    {
        if(cards.Count > 3) checkIndex = 3;

        int number;

        int.TryParse(card.GetComponent<UICardData>().text, out number);

        if(number != cards[checkIndex].Number)
        {
            DeleteCards();
            return false;
        }

        return true;
    }

    public override bool Evaluate()
    {
        Debug.Log("cards[0] = " + cards[0].Color  + ", " + cards[0].Number);
        Debug.Log("cards[1] = " + cards[1].Color  + ", " + cards[1].Number);
        Debug.Log("cards[2] = " + cards[2].Color  + ", " + cards[2].Number);

        if(cards.Count < 3)
        {
            return false;
        }

        //Check if the hand contains a set of 3 same cards that were put on the table in order
        int value = cards[0].Number;

        if(!(cards[1].Number == value && cards[2].Number == value))
        {   
            Debug.Log("cards are not a set!");
            //if not then remove all cards and return false
            cards.Remove(cards[0]);cards.Remove(cards[0]);cards.Remove(cards[0]);
            return false;
        }
        
        checkIndex = 3;

        if(cards.Count < 6) return false;
        
        //Check if the hand contains a second set of 3 same cards that were put on the table in order
        //if so then return success
        value = cards[3].Number;
        if(!(cards[4].Number == value && cards[5].Number == value))
        {
            cards.Remove(cards[3]);cards.Remove(cards[3]);cards.Remove(cards[3]);
            return false;
        }

        return true;
    }
}
