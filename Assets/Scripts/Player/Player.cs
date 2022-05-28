using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using UnityEngine.Assertions;
using System;
using Unity.Netcode.Transports.UTP;

public class Player : NetworkBehaviour
{
    #region Network Variables

    public NetworkVariable<PlayerState> State;
    public NetworkVariable<PlayerClassType> ClassType;
    public NetworkVariable<bool> Alive;
    public NetworkVariable<int> Health;
    public NetworkVariable<int> MaxHealth;

    #endregion

    #region Variables

    [SerializeField] private PlayerController playerController = null;
    private PlayerClassSO currentPlayerClass = null;
    PlayerHealthIndicator healthIndicator;

    #endregion

    #region Unity Event Functions

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ConfigurePlayer(OwnerClientId);
        ClassType = new NetworkVariable<PlayerClassType>();
        State = new NetworkVariable<PlayerState>();
        MaxHealth = new NetworkVariable<int>();
        Health = new NetworkVariable<int>();
        Alive = new NetworkVariable<bool>(true);

        State.OnValueChanged += OnPlayerStateValueChanged;
        Health.OnValueChanged += OnPlayerHealthValueChanged;

        if(IsLocalPlayer)
        {
            ConfigurePlayerClassVariablesServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        State.OnValueChanged -= OnPlayerStateValueChanged;
        Health.OnValueChanged -= OnPlayerHealthValueChanged;
    }

    public void SetPlayerClass(PlayerClassSO newClass)
    {
        currentPlayerClass = newClass;
    }

    private void Start()
    {
        if (!IsClient)
            return;

        healthIndicator = FindObjectOfType<PlayerHealthIndicator>();
        Assert.IsNotNull(healthIndicator, "[Player at NetworkSpawn]: The PlayerHealthIndicator component is null");
    }

    private void Update()
    {
        if (!IsServer)
            return;

        // update player ping
        UnityTransport utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        int ping = (int) utp.GetCurrentRtt(OwnerClientId);
        StatisticsManager.Instance.UpdatePing(OwnerClientId, ping);

        if (Health.Value <= 0 && Alive.Value) // if health drops to zero the player should be respawned
            RespawnPlayer();
    }

    #endregion

    #region Config Methods

    void ConfigurePlayer(ulong clientID)
    {
        if (IsLocalPlayer)
        {
            ConfigurePlayer();
            ConfigureCamera();
            ConfigureControls();
        }
    }

    void ConfigurePlayer()
    {
        UpdatePlayerStateServerRpc(PlayerState.Grounded);
    }

    void ConfigureCamera()
    {
        var virtualCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        virtualCam.LookAt = transform;
        virtualCam.Follow = transform;
    }

    void ConfigureControls()
    {
        GetComponent<InputHandler>().enabled = true;
    }

    #endregion

    #region Methods


    /// <summary>
    /// This method decrements player health and changes statistics if health drops to zero
    /// </summary>
    /// <param name="shooter">Player who shot this player</param>
    public void ReceiveHitFrom(ulong shooter)
    {
        Debug.Log("Player " + shooter + " hit " + OwnerClientId);
        Health.Value--; // decrement player health

        if (Health.Value <= 0 && Alive.Value) // the player has been killed
        {
            Debug.Log("Player " + shooter + " killed " + OwnerClientId);
            // update statistics if player dies
            StatisticsManager.Instance.AddKillDeathToStatistics(shooter, OwnerClientId);
        } 
    }

    /// <summary>
    /// This method respawns a player at a new position with maximum health
    /// </summary>
    private void RespawnPlayer()
    {
        Alive.Value = false;
        SpawnSystem.Instance.RespawnPlayer(this); // teleport the player to a new location
        Health.Value = MaxHealth.Value; // restore health
        Alive.Value = true;
    }

    #endregion

    #region RPC

    #region ServerRPC

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        State.Value = state;
    }

    /// <summary>
    /// This method updates the PlayerClass variables whenever a new class is selected.
    /// </summary>
    [ServerRpc]
    private void ConfigurePlayerClassVariablesServerRpc()
    {
        Assert.IsNotNull(currentPlayerClass, "[Player at ConfigurePlayerClassVariables]: The currentPlayerClass is null");
        ClassType.Value = currentPlayerClass.GetClassType();
        MaxHealth.Value = currentPlayerClass.GetMaxHealth();
        Health.Value = MaxHealth.Value;

        playerController.ConfigurePlayerClassVariables(currentPlayerClass);
    }

    #endregion

    #region ClientRPC

    [ClientRpc]
    public void UpdatePlayerHealthClientRpc(int health, ClientRpcParams clientRpcParams = default)
    {
        healthIndicator.UpdateLifeUI(health); // update the health indicator in client
    }

    #endregion

    #endregion

    #region Netcode Related Methods

    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current)
    {
        State.Value = current;
    }

    void OnPlayerHealthValueChanged(int previous, int current)
    {
        Health.Value = current;

        // only execute the client Rpc in the owner of this object
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        UpdatePlayerHealthClientRpc(current, clientRpcParams);
    }

    #endregion
}

public enum PlayerState
{
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}
