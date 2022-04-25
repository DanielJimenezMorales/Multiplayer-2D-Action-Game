using UnityEngine;

/// <summary>
/// This class represents a spawn point in the scene
/// </summary>
public class SpawnPoint : MonoBehaviour
{

    // At Awake(), add the spawn point to the SpawnSystem singleton instance list
    private void Awake() => SpawnSystem.Instance.AddSpawnPoint(transform);

    private void OnDestroy() => SpawnSystem.Instance.RemoveSpawnPoint(transform);

    // Draw gizmos in order to make spawn points visible in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
