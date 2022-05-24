using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// This struct contains the statistics data of one single player
/// </summary>
public struct PlayerMatchStatisticsData : INetworkSerializable, IEquatable<PlayerMatchStatisticsData>
{
    #region Variables
    public FixedString32Bytes playerName; // Using this type instead of string because with the normal string the NetworkList declaration of this struct fails.
    public ulong playerId;
    public int kills;
    public int deaths;
    public int ping;
    #endregion

    public PlayerMatchStatisticsData(FixedString32Bytes playerName, ulong playerId, int kills = 0, int deaths = 0, int ping = 0)
    {
        this.playerId = playerId;
        this.playerName = playerName;
        this.kills = kills;
        this.deaths = deaths;
        this.ping = ping;
    }

    public float GetKD()
    {
        if(deaths == 0)
        {
            return kills;
        }
        else
        {
            return kills / deaths;
        }
    }

    public bool Equals(PlayerMatchStatisticsData other)
    {
        if (playerId == other.playerId && playerName == other.playerName && kills == other.kills && deaths == other.deaths && ping == other.ping) return true;
        return false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref kills);
        serializer.SerializeValue(ref deaths);
        serializer.SerializeValue(ref ping);
    }
}
