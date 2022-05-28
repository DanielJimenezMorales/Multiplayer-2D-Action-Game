using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class manages one PlayerClass container box inside of the PlayerClass selection screen.
/// </summary>
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

    /// <summary>
    /// This method is called when the player clicks the select button of the container box. 
    /// It will notify the PlayerClass selection screen that the selection has been made.
    /// </summary>
    private void OnSelectedPlayerClass()
    {
        Assert.IsNotNull(playerSelectionUI, "[PlayerClassUIContainer at OnSelectedPlayerClass]: The player selection UI component is null");
        playerSelectionUI.SelectType(type);
        playerSelectionUI.SendPlayerClassTypeInformation();
    }
}
