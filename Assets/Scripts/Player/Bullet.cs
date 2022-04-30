using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Bullet : NetworkBehaviour
{
    private float _speed = 3f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


    }

    private void FixedUpdate()
    {
        MoveBulletServerRpc();
    }

    [ServerRpc]
    public void MoveBulletServerRpc()
    {
        transform.Translate(_speed * transform.right * Time.deltaTime);
    }
}
