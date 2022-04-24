using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

/// <summary>
/// This class is the main one of the lobby. It manage all the logic of the lobby and communicates with every UI object of the lobby screen.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class Lobby : NetworkBehaviour
{
    #region Variables
    private const int MINIMUM_PLAYERS_IN_LOBBY = 1; //How many players does the lobby need to start a match?
    private const int LOBBY_COUNTDOWN_TIME = 10; //Whenever the players reach MINIMUM_PLAYERS_IN_LOBBY, How many seconds has the countdown before starting the match?

    [SerializeField] private int lobbyCapacity = 5; //Which is the maximum players that the lobby is able to handle?
    private NetworkList<PlayerLobbyData> playersInLobby;
    private NetworkVariable<int> lobbyCountdownSeconds;

    //UI Components
    private UIManager uiManager;
    private PlayerLobbyUIController playerLobbyUIController;
    private LobbyCountdown lobbyCountdown;
    private LobbyInfoText lobbyInfoText;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        //Init network variables
        playersInLobby = new NetworkList<PlayerLobbyData>();
        lobbyCountdownSeconds = new NetworkVariable<int>();

        //Init the UI Players Lobby.
        playerLobbyUIController = FindObjectOfType<PlayerLobbyUIController>(true);
        Assert.IsNotNull(playerLobbyUIController, "[Lobby at Init]: The playerLobbyUIController component is null");
        playerLobbyUIController.Init(lobbyCapacity);

        //Init the lobby countdown
        lobbyCountdown = FindObjectOfType<LobbyCountdown>(true);
        Assert.IsNotNull(playerLobbyUIController, "[Lobby at Init]: The lobbyCountdown component is null");
        lobbyCountdown.Init();
        lobbyCountdown.gameObject.SetActive(false);

        //Init the lobby info text
        lobbyInfoText = FindObjectOfType<LobbyInfoText>(true);
        Assert.IsNotNull(lobbyInfoText, "[Lobby at Init]: The lobbyInfoText component is null");
        lobbyInfoText.Init();
        lobbyInfoText.SetText("Waiting for opponents...");
        lobbyInfoText.gameObject.SetActive(true);

        //Init UIManager
        uiManager = FindObjectOfType<UIManager>();
        Assert.IsNotNull(lobbyInfoText, "[Lobby at Init]: The UIManager is null");

        if(IsClient)
        {
            ClientUpdateLobbyCountdown(lobbyCountdownSeconds.Value);
        }
    }

    private void OnEnable()
    {
        //This events should be tracked by server and clients. That is the reason why we put them here and not in Init (Init is only for Server)
        playersInLobby.OnListChanged += UpdateLobbyPlayerList;

        lobbyCountdownSeconds.OnValueChanged += UpdateLobbyCountdownSeconds;
    }

    private void UpdateLobbyCountdownSeconds(int previousValue, int newValue)
    {
        ClientUpdateLobbyCountdown(newValue);
    }

    private void ClientUpdateLobbyCountdown(int newValue)
    {
        if (!IsClient) return;

        if (newValue == -1)
        {
            lobbyCountdown.gameObject.SetActive(false);
            lobbyInfoText.gameObject.SetActive(true);
        }
        else
        {
            lobbyInfoText.gameObject.SetActive(false);
            lobbyCountdown.SetCountdownText(newValue.ToString());
            lobbyCountdown.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        playersInLobby.OnListChanged -= UpdateLobbyPlayerList;

        lobbyCountdownSeconds.OnValueChanged -= UpdateLobbyCountdownSeconds;
    }
    #endregion

    #region Netcode Functions
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Init();
    }


    #endregion
    /// <summary>
    /// This method will start the lobby whenever the start condition turns to valid. In this case, when the lobby countdown reaches 0.
    /// </summary>
    [ClientRpc]
    private void StartGameClientRpc()
    {
        StartGame();
    }

    private void StartGame()
    {
        //Activar InGameUI
        uiManager.ActivateInGameHUD();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Dispose();
    }
    private void Init()
    {
        //Init the Lobby (Only being a server)
        if (IsServer)
        {
            Debug.Log("[SERVER] Initializing Lobby...");
            
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToLobby;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromLobby;
        }
    }

    private void Dispose()
    {
        //Dispose the Lobby (only being a server)
        if (IsServer)
        {
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
            CheckForCountdownVisibility();
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
            Assert.IsFalse(disconnectedPlayer.playerId == ulong.MaxValue && disconnectedPlayer.playerName == System.String.Empty, $"[Lobby at RemovePlayerFromLobby]: Couldn't find the player with {clientId} ID");
            
            playersInLobby.Remove(disconnectedPlayer);
            CheckForCountdownVisibility();
        }
    }

    /// <summary>
    /// This method searches for a player inside the lobby.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>If the player exist, it returns it. If it doesn't, it returns an invalid player</returns>
    private PlayerLobbyData SearchPlayerWithID(ulong clientId)
    {
        foreach(PlayerLobbyData playerLobbyData in playersInLobby)
        {
            if(playerLobbyData.playerId.Equals(clientId))
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

    /// <summary>
    /// This method will manage the visibility of the lobby countdown and lobby info text depending on how many players are in lobby.
    /// </summary>
    /// <param name="changeEvent"></param>
    private void CheckForCountdownVisibility()
    {
        if (!IsServer) return;

        if(playersInLobby.Count >= MINIMUM_PLAYERS_IN_LOBBY)
        {
            //Show countdown and hide info text
            if(!lobbyCountdown.gameObject.activeInHierarchy)
            {
                Debug.Log("Show lobby countdown");
                StartLobbyCountdown();
            }

            lobbyInfoText.gameObject.SetActive(false);
        }
        else
        {
            //Show info text and hide countdown
            if(lobbyCountdown.gameObject.activeInHierarchy)
            {
                Debug.Log("Start lobby countdown");
                StopLobbyCountdown();
            }

            lobbyInfoText.gameObject.SetActive(true);
        }
    }

    private void StartLobbyCountdown()
    {
        lobbyCountdown.gameObject.SetActive(true);
        lobbyCountdownSeconds.Value = LOBBY_COUNTDOWN_TIME;
        lobbyCountdown.SetCountdownText(lobbyCountdownSeconds.Value.ToString());
        StartCoroutine(LobbyCountdownCycle());
    }

    private void StopLobbyCountdown()
    {
        StopCoroutine(LobbyCountdownCycle());
        lobbyCountdown.gameObject.SetActive(false);
        lobbyCountdownSeconds.Value = -1;
    }

    private IEnumerator LobbyCountdownCycle()
    {
        while (lobbyCountdownSeconds.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            lobbyCountdownSeconds.Value--;
            lobbyCountdown.SetCountdownText(lobbyCountdownSeconds.Value.ToString());
        }

        if(lobbyCountdownSeconds.Value == 0)
        {
            StartGame();
            StartGameClientRpc();
        }
    }
}
