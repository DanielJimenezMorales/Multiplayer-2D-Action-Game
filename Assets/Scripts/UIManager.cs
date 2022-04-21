using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{

    #region Variables

    UnityTransport transport;

    [SerializeField] Sprite[] hearts = new Sprite[3];

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private InputField inputFieldIP;

    [Header("Lobby")]
    [SerializeField] private GameObject lobby;
    public GameObject lobbyPrefab;
    [SerializeField] private PlayerLobbyUIController playerLobbyUIController;

    [Header("In-Game HUD")]
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] RawImage[] heartsUI = new RawImage[3];

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        //transport = NetworkManager.Singleton.GetComponent<UnityTransport>(); Puesto en el Start para que funcione
    }

    private void Start()
    {
        transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        ActivateMainMenu();
    }

    #endregion

    #region UI Related Methods

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        lobby.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        lobby.SetActive(false);

        // for test purposes
        UpdateLifeUI(Random.Range(1, 6));
    }

    private void ActivateLobby()
    {
        lobby.SetActive(true);
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
    }

    public void UpdateLifeUI(int hitpoints)
    {
        switch (hitpoints)
        {
            case 6:
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 5:
                heartsUI[0].texture = hearts[1].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 4:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 3:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[1].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 2:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 1:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[1].texture;
                break;
        }
    }

    #endregion

    #region Netcode Related Methods

    /// <summary>
    /// This method is called when there are enough players to start a match.
    /// </summary>
    private void StartGame()
    {
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        ActivateLobby();
    }

    private void StartClient()
    {
        var ip = inputFieldIP.text;
        if (!string.IsNullOrEmpty(ip))
        {
            transport.ConnectionData.Address = ip;
        }
        NetworkManager.Singleton.StartClient();

        //If we are the client we wait until everything has loaded from the server and then we subscribe to lobby delegates (with LoadLobby method)
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += LoadLobby;
        
        ActivateLobby();
    }

    private void LoadLobby(ulong clienId)
    {
        Lobby lobbyr = FindObjectOfType<Lobby>();
        lobbyr.OnPlayerConnectedToLobby += playerLobbyUIController.AddPlayer;
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();

        //If we are the server, we instantiate and initialize the lobby
        GameObject lobbygo = GameObject.Instantiate(lobbyPrefab);
        Lobby lobbyComponent = lobbygo.GetComponent<Lobby>();
        lobbyComponent.OnPlayerConnectedToLobby += playerLobbyUIController.AddPlayer;
        lobbygo.GetComponent<NetworkObject>().Spawn();

        //This is for testing purposes
        PlayerLobbyData dd = new PlayerLobbyData("Paquito", 38);
        lobbyComponent.AddPlayerToLobby(dd);

        ActivateLobby();
    }

    #endregion

}
