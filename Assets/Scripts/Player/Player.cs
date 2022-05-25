using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using UnityEngine.Assertions;
using System;

public class Player : NetworkBehaviour
{
    #region Network Variables

    public NetworkVariable<PlayerState> State;
    public NetworkVariable<bool> Alive;
    public NetworkVariable<int> Health;

    #endregion

    #region Private Variables

    PlayerHealthIndicator healthIndicator;

    #endregion

    #region Constants

    const int MAX_HEALTH = 6;

    #endregion

    #region Unity Event Functions

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ConfigurePlayer(OwnerClientId);
        State = new NetworkVariable<PlayerState>();
        Health = new NetworkVariable<int>(MAX_HEALTH);
        Alive = new NetworkVariable<bool>(true);

        State.OnValueChanged += OnPlayerStateValueChanged;
        Health.OnValueChanged += OnPlayerHealthValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        State.OnValueChanged -= OnPlayerStateValueChanged;
        Health.OnValueChanged -= OnPlayerHealthValueChanged;
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

        /*
        // update player ping
        int ping = NetworkTransport.GetCurrentRtt(OwnerClientId);
        StatisticsManager.Instance.UpdatePing(OwnerClientId, ping);
        */

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
        Health.Value--;

        if (Health.Value <= 0 && Alive.Value)
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
        Debug.Log("Respawning player " + OwnerClientId + "...");
        Alive.Value = false;
        SpawnSystem.Instance.RespawnPlayer(this); // teleport the player to a new location
        Health.Value = MAX_HEALTH; // restore health
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

    #endregion

    #region ClientRPC

    [ClientRpc]
    public void UpdatePlayerHealthClientRpc(int health, ClientRpcParams clientRpcParams = default)
    {
        healthIndicator.UpdateLifeUI(health);
    }

    #endregion

    #endregion

    #region Netcode Related Methods

    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current)
    {
        State.Value = current;
        //Debug.Log("Player state: " + previous + " -> " + current);
    }

    void OnPlayerHealthValueChanged(int previous, int current)
    {
        Health.Value = current;
        //Debug.Log("Player " + OwnerClientId + 
            //" health dropped from " + previous + " to " + current);

        // only execute the client RPC on the owner of this object
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
