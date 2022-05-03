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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _networkObject = GetComponent<NetworkObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == 6)
            // Despawn bullet if it collides with an obstacle
        {
            DespawnBulletServerRpc();
        }
    }

    [ServerRpc]
    private void DespawnBulletServerRpc()
    {
        _networkObject.Despawn();
    }
}
