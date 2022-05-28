using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

/// <summary>
/// This class manages the player selection UI screen.
/// </summary>
public class PlayerSelectionUI : MonoBehaviour
{
    #region Variables
    private PlayerClassType selectedType = PlayerClassType.AgilePlayer;
    private UIManager uiManager = null;
    #endregion

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        Assert.IsNotNull(uiManager, "[PlayerSelectionUI at Awake]: The UIManager component is null");
    }

    /// <summary>
    /// This method selects the type of PlayerClass that the player has chosen before sending the information to the server.
    /// </summary>
    /// <param name="newType"></param>
    public void SelectType(PlayerClassType newType)
    {
        selectedType = newType;
    }

    /// <summary>
    /// This method sends the information related to the player selection to the server when the player chooses a PlayerClass (Agile or Heavy)
    /// </summary>
    public void SendPlayerClassTypeInformation()
    {
        Lobby lobby = FindObjectOfType<Lobby>();
        Assert.IsNotNull(lobby, "[PlayerSelectionUI at SendPlayerClassTypeInformation]: The lobby component is null");
        uiManager.ActivateLobby();
        
        lobby.SetPlayerClassTypeServerRpc(NetworkManager.Singleton.LocalClientId, selectedType);
    }
}
