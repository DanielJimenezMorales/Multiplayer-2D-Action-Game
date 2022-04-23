using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LobbyCountdown : MonoBehaviour
{
    #region Variables
    public event Action OnLobbyCountdownFinished;
    private Text countdownTextComponent;
    private int currentTime = 0;
    private bool isStopped = true;
    #endregion

    #region Getters
    public bool GetIsStopped() { return isStopped; }
    #endregion

    public void Init()
    {
        countdownTextComponent = GetComponent<Text>();
    }

    public void StartCountdown(int initialTime)
    {
        isStopped = false;
        currentTime = initialTime;
        SetCountdownText(currentTime.ToString());
        StartCoroutine(CountCycleCorroutine());
    }

    public void StopCountdown()
    {
        isStopped = true;
        StopCoroutine(CountCycleCorroutine());
    }

    private IEnumerator CountCycleCorroutine()
    {
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            SetCountdownText(currentTime.ToString());
        }

        if(currentTime == 0)
        {
            OnLobbyCountdownFinished?.Invoke();
        }
    }

    private void SetCountdownText(string text)
    {
        countdownTextComponent.text = text;
    }
}
