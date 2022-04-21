using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

public class Lobby : NetworkBehaviour
{
    private NetworkList<PlayerLobbyData> playersInLobby;
    public event Action<PlayerLobbyData> OnPlayerConnectedToLobby;

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// This method initializes the lobby
    /// </summary>
    private void Init()
    {
        playersInLobby = new NetworkList<PlayerLobbyData>();

        //If we are the client we check if there are already players in lobby (in case we are not the first one going into the lobby).
        if (IsClient)
        {
            foreach (PlayerLobbyData pld in playersInLobby)
            {
                OnPlayerConnectedToLobby?.Invoke(pld);
            }
        }

        //Subscribe to the network variable event
        playersInLobby.OnListChanged += ModifyLobbyPlayers;
    }

    /// <summary>
    /// This method is called when NetworkObject.Spawn() is called (This should be call only from the server as it is the one who instantiate the lobby)
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //If we are the server subscribe to the methods when a player connect or disconnect to the lobby
        if (IsServer)
        {
            Debug.Log("Inicializo");
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToLobby;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromLobby;
        }
    }

    private void OnDisable()
    {
        playersInLobby.OnListChanged -= ModifyLobbyPlayers;

        if(IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayerToLobby;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerFromLobby;
        }
    }

    private void AddPlayerToLobby(ulong playerID)
    {
        if (IsServer)
        {
            PlayerLobbyData newPlayerLobbyData = new PlayerLobbyData("Juan", playerID);
            Debug.Log("Se conecta Juan");
            playersInLobby.Add(newPlayerLobbyData);
        }
    }

    public void AddPlayerToLobby(PlayerLobbyData newPlayerLobbyData)
    {
        if (IsServer)
        {
            Debug.Log("Se conecta Juan");
            playersInLobby.Add(newPlayerLobbyData);
        }
    }

    private void RemovePlayerFromLobby(ulong playerID)
    {
        if(IsServer)
        {
            PlayerLobbyData playerDisconnected = new PlayerLobbyData();
            foreach(PlayerLobbyData pld in playersInLobby)
            {
                if(pld.playerId == playerID)
                {
                    playerDisconnected = pld;
                    break;
                }
            }

            bool succesfullyRemoved = playersInLobby.Remove(playerDisconnected);

            Assert.IsFalse(succesfullyRemoved, $"[Lobby at RemovePlayerFromLobby]: The player [{playerDisconnected.playerName}] is not in the playersLobbyList.");
        }
    }

    private void ModifyLobbyPlayers(NetworkListEvent<PlayerLobbyData> change)
    {
        Debug.Log("MMM");
        if (IsClient)
        {
            Debug.Log("MMM2");
            if (change.Type.CompareTo(NetworkListEvent<PlayerLobbyData>.EventType.Add) == 0)
            {
                Debug.Log($"{playersInLobby[playersInLobby.Count - 1]} has joint to the lobby!");
                OnPlayerConnectedToLobby?.Invoke(playersInLobby[playersInLobby.Count - 1]);
            }
            else if(change.Type.CompareTo(NetworkListEvent<PlayerLobbyData>.EventType.Remove) == 0)
            {
                Debug.Log($"Someone has left the lobby!");
            }
        }
    }
}
