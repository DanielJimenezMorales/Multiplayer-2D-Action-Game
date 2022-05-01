using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Bullet : NetworkBehaviour
{
    Rigidbody2D _rb;
    NetworkObject _networkObject;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _rb = GetComponent<Rigidbody2D>();
        _networkObject = GetComponent<NetworkObject>();

    }

    [ServerRpc]
    private void DespawnBulletServerRpc()
    {
        Debug.Log("Despawn bullet");
        _networkObject.Despawn();
    }
}
