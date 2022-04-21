using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This struct will contains the necessary info for displaying a player in the lobby screen. It needs to implement INetworkSerializable and IEquatable<T> since it is made for a NetworkList<T>
/// https://github.com/Unity-Technologies/com.unity.multiplayer.docs/issues/409
/// </summary>
public struct PlayerLobbyData : INetworkSerializable, IEquatable<PlayerLobbyData>
{
    public FixedString32Bytes playerName; //Using this type instead of string because with the normal string the NetworkList declaration of this struct fails.
    public ulong playerId;

    public PlayerLobbyData(FixedString32Bytes playerName, ulong playerId)
    {
        this.playerName = playerName;
        this.playerId = playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
    }

    public bool Equals(PlayerLobbyData other)
    {
        if (playerName == other.playerName && playerId == other.playerId) return true;

        return false;
    }
}