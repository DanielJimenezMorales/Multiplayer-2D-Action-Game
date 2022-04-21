using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerLobbyContainer : MonoBehaviour
{
    [SerializeField] private Text textComponent;

    private void Awake()
    {
        textComponent = GetComponentInChildren<Text>();
    }

    public void SetText(string text)
    {
        Assert.IsNotNull(textComponent, "[PlayerLobbyContainer at SetText]: The text component is null");

        textComponent.text = text;
    }
}
