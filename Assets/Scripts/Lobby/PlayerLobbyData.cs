using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This struct contains player information to be displayed on the lobby screen. It needs to implement 
/// INetworkSerializable and IEquatable<T> since it is made for a NetworkList<T>
/// https://github.com/Unity-Technologies/com.unity.multiplayer.docs/issues/409
/// </summary>
public struct PlayerLobbyData : INetworkSerializable, IEquatable<PlayerLobbyData>
{
    #region Variables
    public FixedString32Bytes playerName; // Using this type instead of string because with the normal string the NetworkList declaration of this struct fails.
    public ulong playerId;
    public PlayerClassType classType;
    #endregion

    public PlayerLobbyData(FixedString32Bytes playerName, ulong playerId)
    {
        this.playerName = playerName;
        this.playerId = playerId;
        classType = PlayerClassType.AgilePlayer;
    }

    /// <summary>
    /// In order to use this struct as T value of the NetworkList we need to serialize it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializer"></param>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref classType);
    }

    public bool Equals(PlayerLobbyData other)
    {
        if (playerName == other.playerName && playerId == other.playerId && classType == other.classType) return true;

        return false;
    }
}