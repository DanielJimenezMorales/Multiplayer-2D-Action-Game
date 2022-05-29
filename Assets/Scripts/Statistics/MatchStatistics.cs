using System.Collections.Generic;
using Unity.Netcode;
using System;
using UnityEngine.Assertions;
using UnityEngine;

/// <summary>
/// This class contains the players statistics data of a single match. It is responsible of updating this data.
/// </summary>
public class MatchStatistics : NetworkBehaviour
{
    private NetworkList<PlayerMatchStatisticsData> playersStatisticsData;
    private static MatchStatistics singletonInstance = null;

    private void Awake()
    {
        if (singletonInstance == null)
        {
            singletonInstance = this;
            playersStatisticsData = new NetworkList<PlayerMatchStatisticsData>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static MatchStatistics GetInstance() { return singletonInstance; }

    public void AddPlayerStatistics(PlayerMatchStatisticsData newPlayerData)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            playersStatisticsData.Add(newPlayerData);
        }
    }

    public IReadOnlyList<PlayerMatchStatisticsData> GetStatistics()
    {
        List<PlayerMatchStatisticsData> resultList = new List<PlayerMatchStatisticsData>();
        foreach (PlayerMatchStatisticsData data in playersStatisticsData)
        {
            resultList.Add(data);
        }

        return resultList;
    }

    /// <summary>
    /// Search for the player with id clientID and add it a kill to its statistics
    /// </summary>
    /// <param name="clientID"></param>
    public void AddAKill(ulong clientID)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int index = GetPlayerDataIndex(clientID);

            Assert.IsTrue(index >= 0, "[MatchStatistics at AddAKill]: Client not found");

            PlayerMatchStatisticsData currentPlayerData = playersStatisticsData[index];
            currentPlayerData.kills++;
            playersStatisticsData[index] = currentPlayerData;
        }
    }

    /// <summary>
    /// Search for the player with id clientID and add it a death to its statistics
    /// </summary>
    /// <param name="clientID"></param>
    public void AddADeath(ulong clientID)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int index = GetPlayerDataIndex(clientID);

            Assert.IsTrue(index >= 0, "[MatchStatistics at AddADeath]: Client not found");

            PlayerMatchStatisticsData currentPlayerData = playersStatisticsData[index];
            currentPlayerData.deaths++;
            playersStatisticsData[index] = currentPlayerData;
        }
    }

    /// <summary>
    /// Search for the player with id clientID and updates the ping to its statistics
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="newPing"></param>
    public void UpdatePing(ulong clientID, int newPing)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int index = GetPlayerDataIndex(clientID);

            Assert.IsTrue(index >= 0, "[MatchStatistics at UpdatePing]: Client not found");

            PlayerMatchStatisticsData currentPlayerData = playersStatisticsData[index];
            currentPlayerData.ping = newPing;
            playersStatisticsData[index] = currentPlayerData;
        }
    }

    /// <summary>
    /// Search for the index of the statistics data from the player with id clientID
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns></returns>
    private int GetPlayerDataIndex(ulong clientID)
    {
        for (int i = 0; i < playersStatisticsData.Count; i++)
        {
            if(playersStatisticsData[i].playerId == clientID)
            {
                return i;
            }
        }

        return -1;
    }
}
