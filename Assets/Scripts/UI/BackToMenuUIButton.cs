using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(Button))]
public class BackToMenuUIButton : MonoBehaviour
{
    #region Variables
    private Button buttonComponent;
    private UIManager uiManager;
    private GameManager gameManager;

    private bool matchEnded = false;
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

        gameManager = FindObjectOfType<GameManager>(true);
        Assert.IsNotNull(buttonComponent, "[BackToMenuUIButton at Start]: The GameManager is null");
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

    private void OnClickButton()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            uiManager.ActivateMainMenu();
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
    }

    private void HandlePlayerDisconnect(ulong clientId)
    {
        Debug.Log($"Disconnecting {clientId}...");
        NetworkManager.Singleton.Shutdown();
        uiManager.ActivateMainMenu();
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
