using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerClassUIContainer : MonoBehaviour
{
    #region Variables
    [SerializeField] private PlayerClassType type = PlayerClassType.AgilePlayer;
    [SerializeField] private Button selectButton = null;
    [SerializeField] private PlayerSelectionUI playerSelectionUI = null;
    #endregion

    private void OnEnable()
    {
        selectButton.onClick.AddListener(OnSelectedPlayerClass);
    }

    private void OnDisable()
    {
        selectButton.onClick.RemoveListener(OnSelectedPlayerClass);
    }

    private void OnSelectedPlayerClass()
    {
        Assert.IsNotNull(playerSelectionUI, "[PlayerClassUIContainer at OnSelectedPlayerClass]: The player selection UI component is null");
        playerSelectionUI.SelectType(type);
        playerSelectionUI.SendPlayerClassTypeInformation();
    }
}
