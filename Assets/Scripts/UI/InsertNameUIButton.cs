using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// This class manages the behaviour when pressing the Insert Name button
/// </summary>
public class InsertNameUIButton : MonoBehaviour
{
    [SerializeField] private InputField inputField = null;
    private UIManager uiManager = null;
    private Lobby lobby = null;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        Assert.IsNotNull(uiManager, "[InsertNameUIButton at Awake]: The UIManager component is null");
    }

    /// <summary>
    /// This methods tells the server that the name has been changed.
    /// </summary>
    public void SendNameInformation()
    {
        string name = inputField.text;
        lobby = FindObjectOfType<Lobby>();
        Assert.IsNotNull(lobby, "[InsertNameUIButton at SendNameInformation]: The Lobby component is null");
        uiManager.ActivatePlayerClassSelection();
        lobby.SetPlayerNameServerRpc(NetworkManager.Singleton.LocalClientId, name);
    }
}
