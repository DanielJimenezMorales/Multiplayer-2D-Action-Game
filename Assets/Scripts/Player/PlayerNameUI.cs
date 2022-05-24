using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.Assertions;
using Unity.Collections;
using System;

/// <summary>
/// This class manages the player name UI
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class PlayerNameUI : NetworkBehaviour
{
    private TextMeshProUGUI textComponent = null;
    private NetworkVariable<FixedString32Bytes> currentName;

    private void Awake()
    {
        currentName = new NetworkVariable<FixedString32Bytes>();
        textComponent = GetComponent<TextMeshProUGUI>();
        Assert.IsNotNull(textComponent, "[PlayerNameUI at Awake]: The text component is null");
        SetNameServer("UNKNOWN");
    }

    private void OnEnable()
    {
        currentName.OnValueChanged += UpdateTextComponent;
    }

    private void OnDisable()
    {
        currentName.OnValueChanged -= UpdateTextComponent;
    }

    private void UpdateTextComponent(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        textComponent.text = currentName.Value.ToString();
    }

    public void SetNameServer(string newName)
    {
        if (!IsServer) return;
        currentName.Value = newName;
    }
}
