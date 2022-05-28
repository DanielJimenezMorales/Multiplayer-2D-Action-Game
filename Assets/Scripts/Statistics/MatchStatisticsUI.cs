using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// This class manages how the players statistics table will display in the UI.
/// </summary>
public class MatchStatisticsUI : MonoBehaviour
{
    private IList<PlayerStatisticsRowUI> rows;

    private void Awake()
    {
        //Init rows
        PlayerStatisticsRowUI[] rowsComponents = GetComponentsInChildren<PlayerStatisticsRowUI>(true);
        rows = new List<PlayerStatisticsRowUI>(rowsComponents);

        foreach(PlayerStatisticsRowUI row in rows)
        {
            row.gameObject.SetActive(false);
        }
    }

    public void UpdateMatchStatistics(IReadOnlyList<PlayerMatchStatisticsData> playersStatistics)
    {
        playersStatistics = SortPlayersStatisticsByNumberOfKills(playersStatistics);

        for (int i = 0; i < rows.Count; i++)
        {
            if(i < playersStatistics.Count)
            {
                //Check if this row is from our local player
                bool isThisClientOwnerOfThisStatisticsRow = NetworkManager.Singleton.LocalClientId == playersStatistics[i].playerId;
                rows[i].UpdateStatisticsRow(i + 1, playersStatistics[i].playerName.ToString(), playersStatistics[i].kills, playersStatistics[i].deaths, 
                    playersStatistics[i].GetKD(), playersStatistics[i].ping, isThisClientOwnerOfThisStatisticsRow);

                rows[i].gameObject.SetActive(true);
            }
            else
            {
                rows[i].gameObject.SetActive(false);
            }
        }
    }

    private IReadOnlyList<PlayerMatchStatisticsData> SortPlayersStatisticsByNumberOfKills(IReadOnlyList<PlayerMatchStatisticsData> playersStatistics)
    {
        List<PlayerMatchStatisticsData> sortedPlayerStatistics = new List<PlayerMatchStatisticsData>();
        bool[] visitedPositions = new bool[playersStatistics.Count];

        int numberOfKills = -1;
        int index = 0;

        for (int i = 0; i < playersStatistics.Count; i++)
        {
            for (int j = 0; j < playersStatistics.Count; j++)
            {
                if(numberOfKills < playersStatistics[j].kills && !visitedPositions[j])
                {
                    numberOfKills = playersStatistics[j].kills;
                    index = j;
                    visitedPositions[j] = true;
                }
            }

            sortedPlayerStatistics.Add(playersStatistics[index]);
            numberOfKills = -1;
            index = 0;
        }

        return sortedPlayerStatistics;
    }
}
