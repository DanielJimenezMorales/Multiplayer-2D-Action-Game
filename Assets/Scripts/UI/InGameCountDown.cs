using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class InGameCountDown : MonoBehaviour
{
    #region Variables
    private Text countdownTextComponent;
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        countdownTextComponent = GetComponent<Text>();
    }

    public void SetCountdownText(string text)
    {
        countdownTextComponent.text = text;
    }
}
