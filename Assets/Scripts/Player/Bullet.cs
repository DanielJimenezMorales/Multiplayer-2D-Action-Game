using UnityEngine;
using Unity.Netcode;

/// <summary>
/// This class represents a bullet from the player's weapon
/// </summary>
public class Bullet : NetworkBehaviour
{
    #region Network Variables

    public NetworkVariable<ulong> ShooterId; // id of the player who shot the bullet

    #endregion

    #region Unity Event Functions

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ShooterId = new NetworkVariable<ulong>();

        ShooterId.OnValueChanged += OnShooterIdValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        ShooterId.OnValueChanged -= OnShooterIdValueChanged;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer)
        {
            return;
        }

        // this code can only be executed by the server

        int objectLayer = other.gameObject.layer;

        switch (objectLayer)
        {
            case 3: // damage player and despawn bullet
                ulong otherId = other.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                if (otherId != ShooterId.Value)
                {
                    Debug.Log("Player " + ShooterId.Value + " hit player " + otherId);
                    HitPlayer(other.gameObject.GetComponent<Player>());
                    DespawnBullet();
                }
                break;
            case 6: // despawn bullet
                //Debug.Log("Bullet hit an obstacle");
                DespawnBullet();
                break;
            default:
                break;
        }
    }

    #endregion

    #region Private Methods

    void DespawnBullet()
    {
        //Debug.Log("Despawning bullet...");
        GetComponent<NetworkObject>().Despawn();
    }

    void HitPlayer(Player player)
    {
        player.Health.Value--;
    }

    #endregion

    #region Netcode Related Methods

    void OnShooterIdValueChanged(ulong previous, ulong current)
    {
        ShooterId.Value = current;
    }

    #endregion
}
