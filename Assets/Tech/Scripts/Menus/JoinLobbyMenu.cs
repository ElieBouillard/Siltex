using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField adressInput = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        SiltexNetworkManager.ClientOnConnected += HandleClientConnected;
        SiltexNetworkManager.ClientOnDisconnected += HandeClientDisconnected;
    }

    private void OnDisable()
    {
        SiltexNetworkManager.ClientOnConnected -= HandleClientConnected;
        SiltexNetworkManager.ClientOnDisconnected -= HandeClientDisconnected;
    }

    public void Join()
    {
        string adress = adressInput.text;

        NetworkManager.singleton.networkAddress = adress;
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandeClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
