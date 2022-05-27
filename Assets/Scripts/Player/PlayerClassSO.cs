using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Player/Player Class Type", fileName = "XXXPlayerType")]
public class PlayerClassSO : ScriptableObject, INetworkSerializable
{
    #region Variables
    [SerializeField] private PlayerClassType classType = PlayerClassType.AgilePlayer;
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private float movementSpeed = 3.4f;
    [SerializeField] private float bulletsSpeed = 3f;
    #endregion

    #region Getters
    public PlayerClassType GetClassType() { return classType; }
    public int GetMaxHealth() { return maxHealth; }
    public float GetMovementSpeed() { return movementSpeed; }
    public float GetBulletsSpeed() { return bulletsSpeed; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref classType);
        serializer.SerializeValue(ref maxHealth);
        serializer.SerializeValue(ref movementSpeed);
    }
    #endregion
}
