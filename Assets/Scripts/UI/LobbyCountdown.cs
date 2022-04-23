using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LobbyCountdown : MonoBehaviour
{
    private Text countdownTextComponent;
    private int currentTime = 0;
    private bool isStopped = true;

    public bool GetIsStopped() { return isStopped; }

    public void Init()
    {
        countdownTextComponent = GetComponent<Text>();
    }

    public void StartCountdown(int initialTime)
    {
        isStopped = false;
        currentTime = initialTime;
        StartCoroutine(CountCycleCorroutine());
    }

    public void StopCountdown()
    {
        isStopped = true;
        StopCoroutine(CountCycleCorroutine());
    }

    private IEnumerator CountCycleCorroutine()
    {
        Debug.Log("Corrutina");
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            countdownTextComponent.text = currentTime.ToString();
        }

    }
}
