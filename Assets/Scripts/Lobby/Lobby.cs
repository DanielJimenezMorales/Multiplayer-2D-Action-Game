using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

[RequireComponent(typeof(NetworkObject))]
public class Lobby : NetworkBehaviour
{
    private NetworkList<PlayerLobbyData> playersInLobby;
    [SerializeField] private int lobbyCapacity = 5;
    private PlayerLobbyUIController playerLobbyUIController;

    private void Awake()
    {
        //Init networkList
        playersInLobby = new NetworkList<PlayerLobbyData>();

        //Init the UI Lobby.
        playerLobbyUIController = FindObjectOfType<PlayerLobbyUIController>(true);
        Assert.IsNotNull(playerLobbyUIController, "[Lobby at Init]: The playerLobbyUIController component is null");
        playerLobbyUIController.Init(lobbyCapacity);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Init();
    }

    private void Init()
    {
        //Init the Lobby (Only being a server)
        if (IsServer)
        {
            Debug.Log("[SERVER] Initializing Lobby...");
            playersInLobby.OnListChanged += UpdateLobbyPlayerList;
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToLobby;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromLobby;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Dispose();
    }

    private void Dispose()
    {
        //Dispose the Lobby (only being a server)
        if (IsServer)
        {
            playersInLobby.OnListChanged -= UpdateLobbyPlayerList;
            NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayerToLobby;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerFromLobby;
        }
    }

    /// <summary>
    /// This method adds a player to the lobby when it gets connected.
    /// </summary>
    /// <param name="clientId"></param>
    private void AddPlayerToLobby(ulong clientId)
    {
        if(IsServer)
        {
            PlayerLobbyData playerLobbyData = new PlayerLobbyData("Jugador" + clientId.ToString(), clientId);
            playersInLobby.Add(playerLobbyData);
        }
    }

    /// <summary>
    /// This method removes a player from the lobby when it gets disconnected.
    /// </summary>
    /// <param name="clientId"></param>
    private void RemovePlayerFromLobby(ulong clientId)
    {
        if(IsServer)
        {
            PlayerLobbyData disconnectedPlayer = SearchPlayerWithID(clientId);
            Assert.IsTrue(disconnectedPlayer.playerId == ulong.MaxValue && disconnectedPlayer.playerName == System.String.Empty, $"[Lobby at RemovePlayerFromLobby]: Couldn't find the player with {clientId} ID");
            
            playersInLobby.Remove(disconnectedPlayer);
        }
    }

    /// <summary>
    /// This method searches for a player inside the lobby
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>If the player exist, it returns it. If it doesn't, it returns an invalid player</returns>
    private PlayerLobbyData SearchPlayerWithID(ulong clientId)
    {
        foreach(PlayerLobbyData playerLobbyData in playersInLobby)
        {
            if(playerLobbyData.playerId == clientId)
            {
                return playerLobbyData;
            }
        }

        //If doesn't found it returns an invalid PlayerLobbyData
        return new PlayerLobbyData(System.String.Empty, ulong.MaxValue);
    }

    /// <summary>
    /// Updates the lobby player list UI whenever there is a change.
    /// </summary>
    /// <param name="changeEvent"></param>
    private void UpdateLobbyPlayerList(NetworkListEvent<PlayerLobbyData> changeEvent)
    {
        List<PlayerLobbyData> players = new List<PlayerLobbyData>();
        foreach(PlayerLobbyData playerData in playersInLobby)
        {
            players.Add(playerData);
        }

        playerLobbyUIController.UpdatePlayers(players);
    }
}
