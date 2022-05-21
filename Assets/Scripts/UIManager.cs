using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{

    #region Variables
    [SerializeField] NetworkManager networkManager;
    UnityTransport transport;

    readonly ushort port = 7777;

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

    [Header("End match")]
    [SerializeField] private GameObject endMatch;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        ActivateMainMenu();
    }
    #endregion

    #region UI Related Methods

    public void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        lobby.SetActive(false);
        endMatch.SetActive(false);
    }

    public void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        lobby.SetActive(false);
        endMatch.SetActive(false);
    }

    private void ActivateLobby()
    {
        lobby.SetActive(true);
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        endMatch.SetActive(false);
    }

    public void ActivateEndMatch()
    {
        endMatch.SetActive(true);
        lobby.SetActive(false);
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
    }

 

    #endregion

    #region Netcode Related Methods
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
            transport.SetConnectionData(ip, port);
        }
        NetworkManager.Singleton.StartClient();

        ActivateLobby();
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        ActivateLobby();
    }

    #endregion

}
