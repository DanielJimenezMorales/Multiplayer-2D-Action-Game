using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;
using Unity.Collections;

/// <summary>
/// Main class of the lobby. It manages all the logic of the lobby and communicates with every UI object of the lobby screen.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class Lobby : NetworkBehaviour
{
    #region Variables
    private const int MINIMUM_PLAYERS_IN_LOBBY = 1; // Minimum number of players required to start
    private const int LOBBY_COUNTDOWN_TIME = 10; // Whenever the players reach MINIMUM_PLAYERS_IN_LOBBY, how many seconds has the countdown before starting the match?

    [SerializeField] private int lobbyCapacity = 5; // Which is the maximum players that the lobby is able to handle?
    private NetworkList<PlayerLobbyData> playersInLobby;
    private NetworkVariable<int> lobbyCountdownSeconds;

    //UI Components
    private UIManager uiManager;
    private PlayerLobbyUIController playerLobbyUIController;
    private LobbyCountdown lobbyCountdown;
    private LobbyInfoText lobbyInfoText;

    //GameManager
    private GameManager gameManager;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        //Init network variables
        playersInLobby = new NetworkList<PlayerLobbyData>();
        lobbyCountdownSeconds = new NetworkVariable<int>();

        // Init the UI Players Lobby.
        playerLobbyUIController = FindObjectOfType<PlayerLobbyUIController>(true);
        Assert.IsNotNull(playerLobbyUIController, "[Lobby at Init]: The playerLobbyUIController component is null");
        playerLobbyUIController.Init(lobbyCapacity);

        // Init the lobby countdown
        lobbyCountdown = FindObjectOfType<LobbyCountdown>(true);
        Assert.IsNotNull(playerLobbyUIController, "[Lobby at Init]: The lobbyCountdown component is null");
        lobbyCountdown.Init();
        lobbyCountdown.gameObject.SetActive(false);

        // Init the lobby info text
        lobbyInfoText = FindObjectOfType<LobbyInfoText>(true);
        Assert.IsNotNull(lobbyInfoText, "[Lobby at Init]: The lobbyInfoText component is null");
        lobbyInfoText.Init();
        lobbyInfoText.SetText("Waiting for opponents...");
        lobbyInfoText.gameObject.SetActive(true);

        // Init UIManager
        uiManager = FindObjectOfType<UIManager>();
        Assert.IsNotNull(uiManager, "[Lobby at Init]: The UIManager is null");

        //Init GameManager
        gameManager = FindObjectOfType<GameManager>();
        Assert.IsNotNull(gameManager, "[Lobby at Init]: The GameManager is null");

        if (IsClient)
        {
            ClientUpdateLobbyCountdown(lobbyCountdownSeconds.Value);
        }
    }

    private void OnEnable()
    {
        // These events should be tracked both by the server and its clients.
        // Init is only for the server
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

    private void StartGameServer()
    {
        if (!IsServer) return;

        // Spawn players
        ulong[] clientsId = new ulong[playersInLobby.Count];

        for (int i = 0; i < clientsId.Length; i++)
        {
            clientsId[i] = playersInLobby[i].playerId;
        }

        SpawnSystem.Instance.SpawnPlayersFromLobbyAtRandomSpawnPoint(GetPlayersLobbyData());

        for (int i = 0; i < playersInLobby.Count; i++)
        {
            PlayerMatchStatisticsData initialPlayerStatisticsData = new PlayerMatchStatisticsData(playersInLobby[i].playerName, playersInLobby[i].playerId);
            MatchStatistics.GetInstance().AddPlayerStatistics(initialPlayerStatisticsData);
        }

        // Start game in Game Manager
        gameManager.StartGame_Server();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Dispose();
    }

    private void Init()
    {
        Debug.Log("Initializing lobby...");

        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToLobby;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromLobby;

        // If you are the host, add yourself as a player to the lobby
        if (IsHost)
        {
            AddPlayerToLobby(OwnerClientId);
        }
    }

    private void Dispose()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayerToLobby;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerFromLobby;
    }

    /// <summary>
    /// This method adds a player to the lobby when it gets connected.
    /// </summary>
    /// <param name="clientId"></param>
    private void AddPlayerToLobby(ulong clientId)
    {
        if(playersInLobby.Count == lobbyCapacity)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
            return;
        }

        if(IsServer)
        {
            PlayerLobbyData playerLobbyData = new PlayerLobbyData("Jugador " + clientId.ToString(), clientId);
            playersInLobby.Add(playerLobbyData);
            CheckForCountdownVisibility();
        }
    }

    /// <summary>
    /// This method removes a player from the lobby when it disconnects.
    /// </summary>
    /// <param name="clientId"></param>
    private void RemovePlayerFromLobby(ulong clientId)
    {
        if(IsServer)
        {
            PlayerLobbyData disconnectedPlayer = SearchPlayerWithID(clientId);
            Assert.IsFalse(disconnectedPlayer.playerId == ulong.MaxValue 
                && disconnectedPlayer.playerName == System.String.Empty, 
                $"[Lobby at RemovePlayerFromLobby]: Couldn't find the player with {clientId} ID");
            
            playersInLobby.Remove(disconnectedPlayer);
            CheckForCountdownVisibility();
        }
        else if(IsClient)
        {
            if(playersInLobby.Count == lobbyCapacity)
            {
                uiManager.ActivateLobbyFull();
            }
            else
            {
                uiManager.ActivateMainMenu();
            }
        }
    }

    /// <summary>
    /// This method searches for a player inside the lobby.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>Either returns the player's data or an invalid player</returns>
    private PlayerLobbyData SearchPlayerWithID(ulong clientId)
    {
        foreach(PlayerLobbyData playerLobbyData in playersInLobby)
        {
            if(playerLobbyData.playerId.Equals(clientId))
            {
                return playerLobbyData;
            }
        }

        return new PlayerLobbyData(System.String.Empty, ulong.MaxValue);
    }

    /// <summary>
    /// Updates the lobby player list UI when a player connects or disconnects.
    /// </summary>
    /// <param name="changeEvent"></param>
    private void UpdateLobbyPlayerList(NetworkListEvent<PlayerLobbyData> changeEvent)
    {
        UpdateLobbyPlayerList();
    }

    /// <summary>
    /// Get the current player lobby data list. (because we can't return the content of a NetworkList easier)
    /// </summary>
    /// <returns></returns>
    private IReadOnlyList<PlayerLobbyData> GetPlayersLobbyData()
    {
        List<PlayerLobbyData> players = new List<PlayerLobbyData>();
        foreach (PlayerLobbyData playerData in playersInLobby)
        {
            players.Add(playerData);
        }

        return players;
    }

    /// <summary>
    /// Updates the lobby player list UI whenever there is a change.
    /// </summary>
    /// <param name="changeEvent"></param>
    private void UpdateLobbyPlayerList()
    {
        playerLobbyUIController.UpdatePlayers(GetPlayersLobbyData());
    }

    /// <summary>
    /// This method manages the visibility of the lobby's countdown and info 
    /// text depending on the current number of players in lobby.
    /// </summary>
    /// <param name="changeEvent"></param>
    private void CheckForCountdownVisibility()
    {
        if (!IsServer) return;

        if(playersInLobby.Count >= MINIMUM_PLAYERS_IN_LOBBY)
        {
            // Show countdown and hide info text
            if(!lobbyCountdown.gameObject.activeInHierarchy)
            {
                Debug.Log("[Server] Show lobby countdown");
                StartLobbyCountdown();
            }

            lobbyInfoText.gameObject.SetActive(false);
        }
        else
        {
            // Show info text and hide countdown
            if(lobbyCountdown.gameObject.activeInHierarchy)
            {
                Debug.Log("[Server] Stop lobby countdown");
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
            StartGameServer();
        }
    }

    [ServerRpc(RequireOwnership = false)] //Adding (RequireOwnership = false) because the lobby is owned by the server, not by any players
    public void SetPlayerNameServerRpc(ulong clientID, string newName)
    {
        int index = 0;
        foreach(PlayerLobbyData playerLobbyData in playersInLobby)
        {
            if(playerLobbyData.playerId == clientID)
            {
                break;
            }
            index++;
        }

        PlayerLobbyData modifiedPlayer = playersInLobby[index];
        modifiedPlayer.playerName = newName;
        playersInLobby[index] = modifiedPlayer;
        UpdateLobbyPlayerList();
    }

    [ServerRpc(RequireOwnership = false)] //Adding (RequireOwnership = false) because the lobby is owned by the server, not by any players
    public void SetPlayerClassTypeServerRpc(ulong clientID, PlayerClassType type)
    {
        Debug.Log(type);
        int index = 0;
        foreach (PlayerLobbyData playerLobbyData in playersInLobby)
        {
            if (playerLobbyData.playerId == clientID)
            {
                break;
            }
            index++;
        }

        PlayerLobbyData modifiedPlayer = playersInLobby[index];
        modifiedPlayer.classType = type;
        playersInLobby[index] = modifiedPlayer;
    }
}
