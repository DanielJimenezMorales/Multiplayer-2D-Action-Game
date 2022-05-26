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
    }

    private void OnDisable()
    {
        buttonComponent.onClick.RemoveListener(OnClickButton);
    }
    #endregion

    private void OnClickButton()
    {
        
    }
}
