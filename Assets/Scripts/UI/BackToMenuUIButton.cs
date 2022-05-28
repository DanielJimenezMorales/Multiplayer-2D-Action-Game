using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// This class handles the shutdown process when the BackToMenu button is clicked by the server
/// (a client cannot start the shutdown process)
/// </summary>
[RequireComponent(typeof(Button))]
public class BackToMenuUIButton : MonoBehaviour
{
    #region Variables
    private Button buttonComponent;
    private UIManager uiManager;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        Assert.IsNotNull(buttonComponent, "[BackToMenuUIButton at Awake]: The buttonComponent is null");
    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>(true);
        Assert.IsNotNull(buttonComponent, "[BackToMenuUIButton at Start]: The UIManager is null");
    }

    private void OnEnable()
    {
        buttonComponent.onClick.AddListener(OnClickButton);
        NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnect;
    }

    private void OnDisable()
    {
        buttonComponent.onClick.RemoveListener(OnClickButton);
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnect;
    }
    #endregion

    /// <summary>
    /// When the button is clicked by the server, the shutdown process starts
    /// </summary>
    private void OnClickButton()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown(); // disconnects all clients and the server
            uiManager.ActivateMainMenu(); // goes back to the main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene"); // reloads the scene
        }
    }

    /// <summary>
    /// Whenever a client is disconnected by the server, the shutdown process on client starts
    /// </summary>
    /// <param name="clientId">id of the client who is being disconnected</param>
    private void HandlePlayerDisconnect(ulong clientId)
    {
        Debug.Log($"Disconnecting {clientId}...");
        NetworkManager.Singleton.Shutdown(); // disconnect the client 
        uiManager.ActivateMainMenu(); // goes back to the menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene"); // reloads the scene
    }
}
