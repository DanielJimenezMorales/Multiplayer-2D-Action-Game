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
                ulong otherId = other.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                if (otherId != ShooterId.Value)
                {
                    HitPlayer(other.gameObject.GetComponent<Player>(), ShooterId.Value);
                    DespawnBullet();
                }
                break;
            case 6: // despawn bullet
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

    /// <summary>
    /// This method hits a player with a bullet from a particular shooter
    /// </summary>
    /// <param name="player">A reference to the player who has been hit</param>
    /// <param name="shooter">The player who shot the bullet</param>
    void HitPlayer(Player playerHit, ulong shooter)
    {
        playerHit.ReceiveHitFrom(shooter);
    }

    #endregion

    #region Netcode Related Methods

    void OnShooterIdValueChanged(ulong previous, ulong current)
    {
        ShooterId.Value = current;
    }

    #endregion
}
