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

    private List<Transform> spawns = new List<Transform>();

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

    public void AddSpawnPoint(Transform transform) => spawns.Add(transform);

    public void RemoveSpawnPoint(Transform transform) => spawns.Remove(transform);

    /// <summary>
    /// Spawns one player per clientId at a random sapwn point the first time (When they come from the lobby)
    /// </summary>
    /// <param name="clientsId"></param>
    public void SpawnPlayersFromLobbyAtRandomSpawnPoint(IReadOnlyList<PlayerLobbyData> playersLobbyData)
    {
        for (int i = 0; i < playersLobbyData.Count; i++)
        {
            int spawnPointIndex = GetRandomSpawnPoint();
            GameObject player = SpawnPlayer(playersLobbyData[i].playerId, spawns[spawnPointIndex].position);
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
    /// Pick a random spawn within the list
    /// </summary>
    /// <returns>The spawn point index</returns>
    private int GetRandomSpawnPoint()
    {
        return Random.Range(0, spawns.Count);
    }
}
