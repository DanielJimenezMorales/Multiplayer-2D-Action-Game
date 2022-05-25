using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Victory Conditions/Maximum Kills", fileName = "MaximumKillsVictoryCondition")]
public class MaximumKillsVictoryConditionSO : VictoryConditionSO
{
    [SerializeField] private int maxNumberOfKills = 10;
    public override bool CheckCondition(int matchSecondsLeft)
    {
        IReadOnlyList<PlayerMatchStatisticsData> statisticsData = MatchStatistics.GetInstance().GetStatistics();

        foreach(PlayerMatchStatisticsData playerData in statisticsData)
        {
            if(playerData.kills >= maxNumberOfKills)
            {
                return true;
            }
        }

        return false;
    }
}
