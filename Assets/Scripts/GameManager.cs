using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    #region Variables

    public event Action OnMatchFinished;
    private const int MATCH_SECONDS = 60;
    private InGameCountDown inGameCountdown = null;
    private NetworkVariable<int> matchSecondsLeft;
    private VictoryChecker victoryChecker = null;
    [SerializeField] private List<VictoryConditionSO> victoryConditions = null;
    private NetworkVariable<GameState> currentGameState;
    private UIManager uiManager;
    private MatchStatisticsUI matchStatisticsUI = null;

    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        matchSecondsLeft = new NetworkVariable<int>();
        currentGameState = new NetworkVariable<GameState>(readPerm: NetworkVariableReadPermission.Everyone);

        inGameCountdown = FindObjectOfType<InGameCountDown>(true);
        Assert.IsNotNull(inGameCountdown, "[GameManager at Awake]: The inGameCountdown component is null");

        Assert.IsFalse(victoryConditions.Count == 0, "[GameManager at Awake]: The Victory conditions list is empty");
        victoryChecker = new VictoryChecker(victoryConditions);

        // Init UIManager
        uiManager = FindObjectOfType<UIManager>();
        Assert.IsNotNull(uiManager, "[GameManager at Awake]: The UIManager is null");

        matchStatisticsUI = FindObjectOfType<MatchStatisticsUI>(true);
        Assert.IsNotNull(uiManager, "[GameManager at Awake]: The MatchStatisticsUI is null");
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer && currentGameState.Value.CompareTo(GameState.Match) == 0)
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
    /// This method starts the match in the server and orders all clients to start the game
    /// as well
    /// </summary>
    public void StartGame_Server()
    {
        currentGameState.Value = GameState.Match;
        uiManager.ActivateInGameHUD();
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;

        // All clients have to start the game before the countdown starts or an exception
        // will be thrown because some UI elements are not active.
        StartGameClientRpc();

        StartInGameCountdown_Server();
        Debug.Log("[Server] Game started");
    }

    /// <summary>
    /// This method starts the match in the client.
    /// </summary>
    [ClientRpc]
    private void StartGameClientRpc()
    {
        uiManager.ActivateInGameHUD();
        matchSecondsLeft.OnValueChanged += OnInGameCounterChanged;
        Debug.Log($"[Client] Game started");
    }

    /// <summary>
    /// This method ends the match in the server and orders all clients to end the match
    /// as well
    /// </summary>
    private void FinishGame_Server()
    {
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        StopInGameCountdown_Server();
        currentGameState.Value = GameState.MatchEnd;
        uiManager.ActivateEndMatch();
        Debug.Log($"[Server] Game ended");
        //Update statistics
        matchStatisticsUI.UpdateMatchStatistics(MatchStatistics.GetInstance().GetStatistics());

        FinishGameClientRpc();
    }

    /// <summary>
    /// This method ends the match in the client
    /// </summary>
    [ClientRpc]
    private void FinishGameClientRpc()
    {
        OnMatchFinished?.Invoke();
        matchSecondsLeft.OnValueChanged -= OnInGameCounterChanged;
        uiManager.ActivateEndMatch();
        Debug.Log("[Client] Match ended");
        //Update statistics
        Debug.Log("2");
        matchStatisticsUI.UpdateMatchStatistics(MatchStatistics.GetInstance().GetStatistics());
    }

    /// <summary>
    /// This method starts the countdown coroutine in the server
    /// </summary>
    private void StartInGameCountdown_Server()
    {
        matchSecondsLeft.Value = MATCH_SECONDS;
        StartCoroutine(InGameCountdownCycle_Server());
    }

    /// <summary>
    /// This method stops the countdown coroutine in the server
    /// </summary>
    private void StopInGameCountdown_Server()
    {
        StopCoroutine(InGameCountdownCycle_Server());
    }

    /// <summary>
    /// This coroutine decrements the countdown every second until someone stops it.
    /// </summary>
    private IEnumerator InGameCountdownCycle_Server()
    {
        while (matchSecondsLeft.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            matchSecondsLeft.Value--;
        }
    }

    private void SetCountdown(int timeLeft)
    {
        inGameCountdown.SetCountdownText(Utilities.ConvertSecondsToMinutesAndSecondsString(timeLeft));
    }
}
