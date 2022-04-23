using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class represents a row of the lobby. It displays important information on screen such as the player's name.
/// </summary>
public class PlayerLobbyContainer : MonoBehaviour
{
    private Text textComponent;

    public void Init()
    {
        textComponent = GetComponentInChildren<Text>(true);
    }

    /// <summary>
    /// Updates the player's name.
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        Assert.IsNotNull(textComponent, "[PlayerLobbyContainer at SetText]: The text component is null");

        textComponent.text = text;
    }
}
