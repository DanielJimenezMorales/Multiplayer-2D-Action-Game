using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// This class handles the events that follow a BackToMenu button press by a client
/// when the lobby is active. The client goes back to the menu after disconnecting
/// (the server cannot disconnect)
/// </summary>
[RequireComponent(typeof(Button))]
public class ExitFromLobbyUIButton : MonoBehaviour
{
    #region Variables
    private Button buttonComponent;
    private UIManager uiManager;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        Assert.IsNotNull(buttonComponent, "[ExitFromLobbyUIButton at Awake]: The buttonComponent is null");
    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>(true);
        Assert.IsNotNull(buttonComponent, "[ExitFromLobbyUIButton at Start]: The UIManager is null");
    }

    private void OnEnable()
    {
        buttonComponent.onClick.AddListener(OnClickButton);
    }

    private void OnDisable()
    {
        buttonComponent.onClick.RemoveListener(OnClickButton);
    }
    #endregion

    /// <summary>
    /// Whenever the button is clicked by a client, the shutdown process starts
    /// </summary>
    private void OnClickButton()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // disconnect the client
            uiManager.ActivateMainMenu(); // goes back to the menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene"); // reloads the scene
        }
    }
}
