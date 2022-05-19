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
    private UIManager uiManager;
    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        matchSecondsLeft = new NetworkVariable<int>();
        currentGameState = new NetworkVariable<GameState>(readPerm: NetworkVariableReadPermission.Everyone);

        inGameCountdown = FindObjectOfType<InGameCountDown>();
        Assert.IsNotNull(inGameCountdown, "[GameManager at Awake]: The inGameCountdown component is null");

        Assert.IsFalse(victoryConditions.Count == 0, "[GameManager at Awake]: The Victory conditions list is empty");
        victoryChecker = new VictoryChecker(victoryConditions);

        // Init UIManager
        uiManager = FindObjectOfType<UIManager>();
        Assert.IsNotNull(uiManager, "[GameManager at Awake]: The UIManager is null");
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
        currentGameState.Value = GameState.Match;
        uiManager.ActivateInGameHUD();
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;

        //Clients has to start the game before the countdown starts or it will through an error because some UI elements are not active.
        StartGameClientRpc();

        StartInGameCountdown_Server();
        Debug.Log("El juego ha empezado");
    }

    /// <summary>
    /// This method will start the match in the client.
    /// </summary>
    [ClientRpc]
    private void StartGameClientRpc()
    {
        uiManager.ActivateInGameHUD();
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;
        Debug.Log("El juego ha empezado");
    }

    /// <summary>
    /// This method will end the match in the server and will share it with the rest of clients.
    /// </summary>
    private void FinishGame_Server()
    {
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        StopInGameCountdown_Server();
        currentGameState.Value = GameState.MatchEnd;
        uiManager.ActivateEndMatch();
        Debug.Log("El juego ha terminado");

        FinishGameClientRpc();
    }

    /// <summary>
    /// This method will end the match in the client
    /// </summary>
    [ClientRpc]
    private void FinishGameClientRpc()
    {
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        uiManager.ActivateEndMatch();
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
