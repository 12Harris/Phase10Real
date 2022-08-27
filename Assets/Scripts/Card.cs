using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Card
{
    //[field: SerializeField] public InputReader InputReader {get; private set;}
    public int Number;
    public string Color;
    public bool IsJoker;
    public bool IsSkipCard;
    public int DeckIndex;

    public Card(int number, string color)
    {
        this.Number = number;
        this.Color = color;
        IsJoker = false;
        IsSkipCard = false;
        DeckIndex = 0;
    }

    public Card()
    {

    }
}
