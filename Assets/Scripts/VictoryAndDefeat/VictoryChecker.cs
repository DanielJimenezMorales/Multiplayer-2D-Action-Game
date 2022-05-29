using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for checking if any of the victory conditions has returned true.
/// </summary>
public class VictoryChecker
{
    #region Variables
    private readonly IList<VictoryConditionSO> victoryConditions = null;
    #endregion

    public VictoryChecker(IList<VictoryConditionSO> victoryConditions)
    {
        this.victoryConditions = victoryConditions;
    }

    /// <summary>
    /// This method will return true if any of the victory conditions has become true.
    /// </summary>
    /// <param name="matchSecondsLeft">A parameter needed for the game conditions</param>
    /// <returns></returns>
    public bool CheckConditions(int matchSecondsLeft)
    {
        foreach (VictoryConditionSO condition in victoryConditions)
        {
            bool conditionAchieved = condition.CheckCondition(matchSecondsLeft);
            if(conditionAchieved)
            {
                return true;
            }
        }

        return false;
    }
}
