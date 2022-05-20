using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Victory Conditions/ Match Countdown Zero", fileName = "MatchCountdownZeroVictoryCondition")]
public class MatchCountdownZeroVictoryConditionSO : VictoryConditionSO
{
    public override bool CheckCondition(int matchSecondsLeft)
    {
        return matchSecondsLeft == 0;
    }
}