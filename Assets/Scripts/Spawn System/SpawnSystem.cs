using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// This class handles the player and object spawning in the game.
/// Contains a list of transforms corresponding to the different spawn points
/// located in the scene.
/// </summary>
public class SpawnSystem : MonoBehaviour
{
    public static SpawnSystem Instance;

    [SerializeField]
    private GameObject playerPrefab = null;

    // list of spawn points available on the map
    private List<SpawnPoint> spawns = new List<SpawnPoint>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddSpawnPoint(SpawnPoint spawnPoint) => spawns.Add(spawnPoint);

    public void RemoveSpawnPoint(SpawnPoint spawnPoint) => spawns.Remove(spawnPoint);

    /// <summary>
    /// Spawns one player per clientId at a random spawn point the first time (when they come from the lobby)
    /// </summary>
    /// <param name="clientsId"></param>
    public void SpawnPlayersFromLobbyAtRandomSpawnPoint(IReadOnlyList<PlayerLobbyData> playersLobbyData)
    {
        for (int i = 0; i < playersLobbyData.Count; i++)
        {
            int spawnPointIndex = GetRandomSpawnPointIndex(); // pick a random index
            SpawnPoint selectedSpawn = spawns[spawnPointIndex];
            selectedSpawn.Available = false; // set the selected spawn to unavailable
            // spawn player at spawn point
            GameObject player = SpawnPlayer(playersLobbyData[i].playerId, selectedSpawn.transform.position);
            // set player name
            player.GetComponentInChildren<PlayerNameUI>().SetNameServer(playersLobbyData[i].playerName.ToString());
        }
    }

    /// <summary>
    /// Spawns a player owned by clientId at position
    /// </summary>
    /// <param name="clientId">The client who owns the player</param>
    /// <param name="position">The spawning position</param>
    private GameObject SpawnPlayer(ulong clientId, Vector3 position)
    {
        var player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        return player;
    }

    /// <summary>
    /// Pick a random avalaible spawn point within the list
    /// </summary>
    /// <returns>A random index</returns>
    private int GetRandomSpawnPointIndex()
    {
        int spawnRndIdx = Random.Range(0, spawns.Count);
        while (!spawns[spawnRndIdx].Available)
            spawnRndIdx = Random.Range(0, spawns.Count);
        return spawnRndIdx;
    }
}
