using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab = null;

    private static List<Transform> spawns = new List<Transform>();

    public static UnityEvent<ulong> OnPlayerSpawned;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerReady;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public static void AddSpawnPoint(Transform transform) => spawns.Add(transform);

    public static void RemoveSpawnPoint(Transform transform) => spawns.Remove(transform);

    private void OnServerReady()
    {
        print("Server ready");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            int index = Random.Range(0, spawns.Count);
            var player = Instantiate(playerPrefab, spawns[index].position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            OnPlayerSpawned?.Invoke(clientId);
        }
    }
}
