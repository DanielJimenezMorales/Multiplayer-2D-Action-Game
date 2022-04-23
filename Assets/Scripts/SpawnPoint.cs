using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake() => GameManager.AddSpawnPoint(transform);

    private void OnDestroy() => GameManager.RemoveSpawnPoint(transform);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
