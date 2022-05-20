using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    #region Network Variables

    public NetworkVariable<PlayerState> State;
    public NetworkVariable<int> Health;

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

        State.OnValueChanged += OnPlayerStateValueChanged;
        Health.OnValueChanged += OnPlayerHealthValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        State.OnValueChanged -= OnPlayerStateValueChanged;
        Health.OnValueChanged -= OnPlayerHealthValueChanged;
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

    #region RPC

    #region ServerRPC

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        State.Value = state;
    }

    [ServerRpc]
    public void UpdatePlayerHealthServerRpc(int health)
    {
        Health.Value = health;
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
        Debug.Log("Player health dropped from " + previous + " to " + current);
    }

    #endregion
}

public enum PlayerState
{
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}
