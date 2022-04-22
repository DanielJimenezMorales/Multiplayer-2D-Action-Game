using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This class will manage the Lobby UI. It will display one row per player inside the lobby with relevant information like their names.
/// </summary>
public class PlayerLobbyUIController : MonoBehaviour
{
    [Tooltip("The lobby row to be instantiated")]
    [SerializeField] private GameObject playerLobbyContainerPrefab;

    private PlayerLobbyContainer[] playerLobbyContainers;

    public void Init(int lobbyCapacity)
    {
        playerLobbyContainers = new PlayerLobbyContainer[lobbyCapacity];

        //Instantiate the maximum possible rows and switch off the visibility.
        for (int i = 0; i < lobbyCapacity; i++)
        {
            GameObject playerRow = Instantiate(playerLobbyContainerPrefab, this.transform);
            playerRow.SetActive(false);

            PlayerLobbyContainer playerLobbyContainerComponent = playerRow.GetComponent<PlayerLobbyContainer>();
            playerLobbyContainerComponent.Init();

            playerLobbyContainers[i] = playerLobbyContainerComponent;
        }
    }

    /// <summary>
    /// This method will update the UI of the players in lobby whenever a new change is made
    /// </summary>
    /// <param name="playersInLobby"></param>
    public void UpdatePlayers(IList<PlayerLobbyData> playersInLobby)
    {
        Assert.IsFalse(playersInLobby.Count > playerLobbyContainers.Length, $"[PlayerLobbyUIController at UpdatePlayers]: The new players list [{playersInLobby.Count}]" +
            $" is bigger than the actual lobbyCapacity [{playerLobbyContainers.Length}]");

        for (int i = 0; i < playerLobbyContainers.Length; i++)
        {
            //If the new players list has this index activate the i row with the i player data.
            if(i < playersInLobby.Count)
            {
                ActivateLobbyRow(playerLobbyContainers[i], playersInLobby[i]);
            }
            else
            {
                DeactivateLobbyRow(playerLobbyContainers[i]);
            }
        }
    }

    /// <summary>
    /// Activates a certain lobby row to display the player data.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="data"></param>
    private void ActivateLobbyRow(PlayerLobbyContainer container, PlayerLobbyData data)
    {
        container.SetText(data.playerName.ToString());
        container.gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates a certain lobby row
    /// </summary>
    /// <param name="container"></param>
    private void DeactivateLobbyRow(PlayerLobbyContainer container)
    {

        container.gameObject.SetActive(false);
        container.SetText("None");
    }
}
