using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2Checker : PhaseChecker
{
    public Phase2Checker(int maxCount) : base(maxCount) {}

    public override bool CheckCards()
    {
        return false;
    }

    public override bool Evaluate()
    {
        return false;
    }
}
