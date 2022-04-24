using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LobbyCountdown : MonoBehaviour
{
    #region Variables
    private Text countdownTextComponent;
    #endregion

    public void Init()
    {
        countdownTextComponent = GetComponent<Text>();
    }

    public void SetCountdownText(string text)
    {
        countdownTextComponent.text = text;
    }
}
