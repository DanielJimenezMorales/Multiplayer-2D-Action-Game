using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

/// <summary>
/// This class handles the player and object spawning in the game.
/// Contains a list of transforms corresponding to the different spawn points
/// located in the scene.
/// </summary>
public class SpawnSystem : MonoBehaviour
{
    public static SpawnSystem Instance;

    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private PlayerClassSO[] possiblePlayerClasses = null;

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
            GameObject player = SpawnPlayer(playersLobbyData[i].playerId, selectedSpawn.transform.position, playersLobbyData[i].classType);
            // set player name
            player.GetComponentInChildren<PlayerNameUI>().SetNameServer(playersLobbyData[i].playerName.ToString());
        }
    }

    /// <summary>
    /// Respawn a given player at a random spawn point
    /// </summary>
    /// <param name="player"></param>
    public void RespawnPlayer(Player player)
    {
        int spawnPointIndex = GetRandomSpawnPointIndex(); // pick a random index
        SpawnPoint selectedSpawn = spawns[spawnPointIndex];
        player.transform.position = selectedSpawn.transform.position; // teleport player to new spawn position
    }

    /// <summary>
    /// Spawns a player owned by clientId at position
    /// </summary>
    /// <param name="clientId">The client who owns the player</param>
    /// <param name="position">The spawning position</param>
    private GameObject SpawnPlayer(ulong clientId, Vector3 position, PlayerClassType classType)
    {
        var player = Instantiate(playerPrefab, position, Quaternion.identity);

        Player playerComponent = player.GetComponent<Player>();
        Assert.IsNotNull(playerComponent, "[SpawnSystem at SpawnPlayer]: The Player component is null");

        PlayerClassSO playerClass = GetPlayerClassFromType(classType);
        Assert.IsNotNull(playerClass, "[SpawnSystem at SpawnPlayer]: The playerClass could not be found");

        playerComponent.SetPlayerClass(playerClass);

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        return player;
    }

    private PlayerClassSO GetPlayerClassFromType(PlayerClassType type)
    {
        for (int i = 0; i < possiblePlayerClasses.Length; i++)
        {
            if(possiblePlayerClasses[i].GetClassType().CompareTo(type) == 0)
            {
                return possiblePlayerClasses[i];
            }
        }

        return null;
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
