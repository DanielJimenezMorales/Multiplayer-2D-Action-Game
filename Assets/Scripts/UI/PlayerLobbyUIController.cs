using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobbyUIController : MonoBehaviour
{
    [SerializeField] private int maxPlayersInLobby = 3;
    [SerializeField] private int activePlayers = 0;
    [SerializeField] private IList<PlayerLobbyContainer> playerLobbyContainers;
    [SerializeField] private GameObject playerLobbyContainerPrefab;

    private void Awake()
    {
        playerLobbyContainers = new List<PlayerLobbyContainer>();
    }

    public void AddPlayer(PlayerLobbyData playerLobbyData)
    {
        if(activePlayers == maxPlayersInLobby)
        {
            return;
        }

        activePlayers++;
        GameObject container = GameObject.Instantiate(playerLobbyContainerPrefab, this.transform);
        container.SetActive(true);
        playerLobbyContainers.Add(container.GetComponent<PlayerLobbyContainer>());
        playerLobbyContainers[activePlayers - 1].SetText(playerLobbyData.playerName.ToString());
    }

    public void RemovePlayer()
    {

    }
}
