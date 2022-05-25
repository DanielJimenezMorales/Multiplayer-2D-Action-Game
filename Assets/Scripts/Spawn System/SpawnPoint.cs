using System.Collections;
using UnityEngine;

/// <summary>
/// This class represents a spawn point in the scene
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    private bool available = true; // whether the spawn point is available
    private bool restartingAvailability = false; // whether the availability of the spawn is being restarted

    public bool Available 
    { 
        get { return available; }
        set { available = value; }
    }

    const int RESET_TIME = 6; // how long it takes for the spawn to be available after use

    // At Start(), add the spawn point to the SpawnSystem singleton instance list
    private void Start() => SpawnSystem.Instance.AddSpawnPoint(this);

    // At OnDestroy(), remove the spawn point from the SpawnSystem singleton instance list
    private void OnDestroy() => SpawnSystem.Instance.RemoveSpawnPoint(this);

    private void Update()
    {
        if (available || restartingAvailability) return;

        restartingAvailability = true;
        StartCoroutine(RestartAvailability());
    }

    // Draw gizmos in order to make spawn points visible in the editor
    private void OnDrawGizmos()
    {
        if (transform.position.x > 0.0f)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.blue;

        if (!available) // unavailable spawn points display as green spheres
            Gizmos.color = Color.green;

        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    /// <summary>
    /// This coroutine restarts the availability of the spawn point
    /// </summary>
    /// <returns></returns>
    private IEnumerator RestartAvailability()
    {
        int timer = RESET_TIME;
        while (timer-- > 0)
            yield return new WaitForSeconds(1f);
        available = true;
        restartingAvailability = false;
    }
}
