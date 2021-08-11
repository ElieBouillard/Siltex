using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;

public class PlayerNameMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput = null;

    public static event Action<String> OnClientTryChangePlayerName;

    public void SetPlayerDisplayName()
    {
        OnClientTryChangePlayerName?.Invoke(playerNameInput.text);
    }
}
