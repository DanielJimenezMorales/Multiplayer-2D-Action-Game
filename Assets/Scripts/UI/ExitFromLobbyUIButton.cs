using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(Button))]
public class ExitFromLobbyUIButton : MonoBehaviour
{
    private Button buttonComponent;
    private UIManager uiManager;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        Assert.IsNotNull(buttonComponent, "[ExitFromLobbyUIButton at Awake]: The buttonComponent is null");
    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>(true);
        Assert.IsNotNull(buttonComponent, "[ExitFromLobbyUIButton at Awake]: The UIManager is null");
    }

    private void OnEnable()
    {
        buttonComponent.onClick.AddListener(OnClickButton);
    }

    private void OnDisable()
    {
        buttonComponent.onClick.RemoveListener(OnClickButton);
    }

    private void OnClickButton()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            uiManager.ActivateMainMenu();
        }
    }
}
