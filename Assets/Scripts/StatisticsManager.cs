using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class manages all k/d data related to players
/// </summary>
public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Adds a kill and its corresponding death to the MatchStatistics
    /// </summary>
    /// <param name="clientId1"></param>
    /// <param name="clientId2"></param>
    public void AddKillDeathToStatistics(ulong clientId1, ulong clientId2)
    {
        MatchStatistics stats = MatchStatistics.GetInstance();
        stats.AddAKill(clientId1);
        stats.AddADeath(clientId2);
    }


    /// <summary>
    /// Updates the ping for an specific player
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="ping"></param>
    public void UpdatePing(ulong clientId, int ping)
    {
        MatchStatistics.GetInstance().UpdatePing(clientId, ping);
    }
}
