using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;


/// <summary>
/// This class represents a bullet from the player's weapon
/// </summary>
public class Bullet : NetworkBehaviour
{
    NetworkObject _networkObject;

    public NetworkVariable<ulong> ShooterId;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _networkObject = GetComponent<NetworkObject>();

        ShooterId = new NetworkVariable<ulong>();

        ShooterId.OnValueChanged += OnShooterIdValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        ShooterId.OnValueChanged -= OnShooterIdValueChanged;
    }

    void OnShooterIdValueChanged(ulong previous, ulong current)
    {
        ShooterId.Value = current;
    }

    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        int objectLayer = other.gameObject.layer;

        switch (objectLayer)
        {
            case 3: // damage player and despawn bullet
                ulong otherId = other.GetComponent<NetworkObject>().NetworkObjectId;
                if (otherId != ShooterId.Value)
                {
                    HitPlayerServerRpc(otherId);
                    DespawnBulletServerRpc();
                }
                break;
            case 6: // despawn bullet
                DespawnBulletServerRpc();
                break;
            default:
                break;
        }
    }
    */

    [ServerRpc(RequireOwnership = false)]
    private void DespawnBulletServerRpc()
    {
        _networkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitPlayerServerRpc(ulong playerId)
    {
        Debug.Log("Hit player " + playerId);
    }
}
