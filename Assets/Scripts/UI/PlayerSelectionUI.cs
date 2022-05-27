using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;

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

    public void SelectType(PlayerClassType newType)
    {
        selectedType = newType;
    }

    public void SendPlayerClassTypeInformation()
    {
        Lobby lobby = FindObjectOfType<Lobby>();
        Assert.IsNotNull(lobby, "[PlayerSelectionUI at SendPlayerClassTypeInformation]: The lobby component is null");
        uiManager.ActivateLobby();
        
        lobby.SetPlayerClassTypeServerRpc(NetworkManager.Singleton.LocalClientId, selectedType);
    }
}
