using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    private const int MATCH_SECONDS = 5;
    private InGameCountDown inGameCountdown = null;
    private NetworkVariable<int> matchSecondsLeft;

    private void Awake()
    {
        matchSecondsLeft = new NetworkVariable<int>();

        inGameCountdown = FindObjectOfType<InGameCountDown>();
        Assert.IsNotNull(inGameCountdown, "[GameManager at Awake]: The inGameCountdown component is null");
    }

    public void StartGame()
    {
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;
        StartInGameCountdown();
    }

    private void FinishGame()
    {
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        Debug.Log("El juego ha terminado");
    }

    private void OnInGameCounterChanged(int previousValue, int newValue)
    {
        inGameCountdown.SetCountdownText(Utilities.ConvertSecondsToMinutesAndSecondsString(matchSecondsLeft.Value));
    }

    private void StartInGameCountdown()
    {
        matchSecondsLeft.Value = MATCH_SECONDS;
        StartCoroutine(InGameCountdownCycle());
    }

    private IEnumerator InGameCountdownCycle()
    {
        while (matchSecondsLeft.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            matchSecondsLeft.Value--;
        }

        FinishGame();
    }
}
