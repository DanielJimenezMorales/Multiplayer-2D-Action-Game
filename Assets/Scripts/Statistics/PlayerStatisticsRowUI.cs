using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manage how a single row with a player statistics shows in the UI
/// </summary>
public class PlayerStatisticsRowUI : MonoBehaviour
{
    [SerializeField] private Text positionTextComponent = null;
    [SerializeField] private Text nameTextComponent = null;
    [SerializeField] private Text killsTextComponent = null;
    [SerializeField] private Text deathsTextComponent = null;
    [SerializeField] private Text kdTextComponent = null;
    [SerializeField] private Text pingTextComponent = null;
    [SerializeField] private GameObject localPlayerIndicator = null;

    public void UpdateStatisticsRow(int position, string name, int kills, int deaths, float kd, int ping, bool isLocalPlayer)
    {
        positionTextComponent.text = position.ToString();
        nameTextComponent.text = name;
        killsTextComponent.text = kills.ToString();
        deathsTextComponent.text = deaths.ToString();
        kdTextComponent.text = kd.ToString("0.00");
        pingTextComponent.text = ping.ToString();
        localPlayerIndicator.SetActive(isLocalPlayer);
    }
}
