using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private GameObject lobbyUiPanel = null;
    [SerializeField] private bool useSteam = false;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    public static CSteamID LobbyId { get; private set; }

    private void Start()
    {
        SiltexPlayer.ClientOnPlayerConnectedToServer += EnableLobbyUi;
        if (!useSteam) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    private void OnDisable()
    {
        SiltexPlayer.ClientOnPlayerConnectedToServer -= EnableLobbyUi;
    }

    public void EnableLobbyUi()
    {
        landingPagePanel.SetActive(false);
        lobbyUiPanel.SetActive(true);
    }

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        if (useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
            return;
        }

        NetworkManager.singleton.StartHost();
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            landingPagePanel.SetActive(true);
            return;
        }

        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        NetworkManager.singleton.StartHost();

        SteamMatchmaking.SetLobbyData(LobbyId, "HostAdress", SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "HostAdress");
        NetworkManager.singleton.networkAddress = hostAdress;
        NetworkManager.singleton.StartClient();
        landingPagePanel.SetActive(false);
    }
}
