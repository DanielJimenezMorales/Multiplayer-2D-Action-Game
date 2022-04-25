using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// This class handles the player and object spawning in the game.
/// Contains a list of transforms corresponding to the different spawn points
/// located in the scene.
/// </summary>
public class SpawnSystem : Singleton<SpawnSystem>
{
    [SerializeField] 
    private GameObject playerPrefab = null;

    private static List<Transform> spawns = new List<Transform>();

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public void AddSpawnPoint(Transform transform) => spawns.Add(transform);

    public void RemoveSpawnPoint(Transform transform) => spawns.Remove(transform);

    /// <summary>
    /// Whenever a client is connected, the server must spawn a Player prefab as a player object
    /// </summary>
    /// <param name="clientId">Id of the player whose prefab must be spawned</param>
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int index = Random.Range(0, spawns.Count); // pick a random spawn within the list
            var player = Instantiate(playerPrefab, spawns[index].position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}
