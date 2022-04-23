using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LobbyInfoText : MonoBehaviour
{
    #region Variables
    private Text textComponent;
    #endregion

    public void Init()
    {
        textComponent = GetComponent<Text>();
        Assert.IsNotNull(textComponent, "[LobbyInfoText at Init]: The textComponent is null");
    }

    public void SetText(string text)
    {
        textComponent.text = text;
    }
}
