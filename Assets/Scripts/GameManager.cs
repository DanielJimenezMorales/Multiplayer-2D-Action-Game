using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;
using System;

public class GameManager : NetworkBehaviour
{
    #region Variables
    private const int MATCH_SECONDS = 5;
    private InGameCountDown inGameCountdown = null;
    private NetworkVariable<int> matchSecondsLeft;
    private VictoryChecker victoryChecker = null;
    [SerializeField] private List<VictoryConditionSO> victoryConditions = null;
    private NetworkVariable<GameState> currentGameState;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        matchSecondsLeft = new NetworkVariable<int>();
        currentGameState = new NetworkVariable<GameState>(readPerm: NetworkVariableReadPermission.Everyone);

        inGameCountdown = FindObjectOfType<InGameCountDown>();
        Assert.IsNotNull(inGameCountdown, "[GameManager at Awake]: The inGameCountdown component is null");
        inGameCountdown.gameObject.SetActive(false);

        Assert.IsFalse(victoryConditions.Count == 0, "[GameManager at Awake]: The Victory conditions list is empty");
        victoryChecker = new VictoryChecker(victoryConditions);
    }

    private void Update()
    {
        if(NetworkManager.Singleton.IsServer && currentGameState.Value.CompareTo(GameState.Match) == 0)
        {
            bool isVictory = victoryChecker.CheckConditions(matchSecondsLeft.Value);
            if(isVictory)
            {
                FinishGame_Server();
            }
        }
    }
    #endregion

    private void OnInGameCounterChanged(int previousValue, int newValue)
    {
        SetCountdown(matchSecondsLeft.Value);
    }

    /// <summary>
    /// This method will start the match in the server and will share it with the rest of clients.
    /// </summary>
    public void StartGame_Server()
    {
        inGameCountdown.gameObject.SetActive(true);
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;
        StartInGameCountdown_Server();
        currentGameState.Value = GameState.Match;
        Debug.Log("El juego ha empezado");

        StartGameClientRpc();
    }

    /// <summary>
    /// This method will start the match in the client.
    /// </summary>
    [ClientRpc]
    private void StartGameClientRpc()
    {
        inGameCountdown.gameObject.SetActive(true);
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;
        Debug.Log("El juego ha empezado");
    }

    /// <summary>
    /// This method will end the match in the server and will share it with the rest of clients.
    /// </summary>
    private void FinishGame_Server()
    {
        inGameCountdown.gameObject.SetActive(false);
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        StopInGameCountdown_Server();
        currentGameState.Value = GameState.MatchEnd;
        Debug.Log("El juego ha terminado");

        FinishGameClientRpc();
    }

    /// <summary>
    /// This method will end the match in the client
    /// </summary>
    [ClientRpc]
    private void FinishGameClientRpc()
    {
        inGameCountdown.gameObject.SetActive(false);
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        Debug.Log("PARTIDA FINALIZADA");
    }

    /// <summary>
    /// This method will start the countdown corroutine in the server
    /// </summary>
    private void StartInGameCountdown_Server()
    {
        matchSecondsLeft.Value = MATCH_SECONDS;
        StartCoroutine(InGameCountdownCycle_Server());
    }

    /// <summary>
    /// This method will stop the countdown corroutine in the server
    /// </summary>
    private void StopInGameCountdown_Server()
    {
        StopCoroutine(InGameCountdownCycle_Server());
    }

    /// <summary>
    /// This method will substract 1 to the countdown every second until someone stop it.
    /// </summary>
    private IEnumerator InGameCountdownCycle_Server()
    {
        while (matchSecondsLeft.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            matchSecondsLeft.Value--;
        }

        FinishGame_Server();
    }

    private void SetCountdown(int timeLeft)
    {
        inGameCountdown.SetCountdownText(Utilities.ConvertSecondsToMinutesAndSecondsString(timeLeft));
    }
}
