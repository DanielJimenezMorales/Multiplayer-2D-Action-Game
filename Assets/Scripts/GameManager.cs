using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject playerPrefab = null;

    private static List<Transform> spawns = new List<Transform>();

    private void Start()
    {
        networkManager.OnServerStarted += OnServerReady;
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerReady;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    public static void AddSpawnPoint(Transform transform) => spawns.Add(transform);

    public static void RemoveSpawnPoint(Transform transform) => spawns.Remove(transform);

    private void OnServerReady()
    {
        print("Server ready");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager.IsServer)
        {
            int index = Random.Range(0, spawns.Count);
            var player = Instantiate(playerPrefab, spawns[index].position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}
