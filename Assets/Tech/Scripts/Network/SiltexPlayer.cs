using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System;

public class SiltexPlayer : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent playerAgent = null;
    [SerializeField] private PlayerInput playerInput = null;
    [SerializeField] private PlayerSpell playerSpell = null;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName = null;

    [SyncVar]
    private bool isGameStarted = false;
    private bool startGameCheck = false;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    private void Update()
    {
        if (isClient)
        {
            if (!isGameStarted)
            {
                isGameStarted = ((SiltexNetworkManager)NetworkManager.singleton).GetIsGameInProgress();
            }
            else if(!startGameCheck)
            {
                ClienOnStartGame();
                startGameCheck = true;
            }
        }
    }

    #region Server
    [Server]
    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner) { return;}
        ((SiltexNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryChangePlayerName(string newPlayerName)
    {
        ((SiltexNetworkManager)NetworkManager.singleton).ServerSetPlayerNameDisplay(connectionToClient, newPlayerName);
    }

    #endregion

    #region Client
    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    public override void OnStartClient()
    {
        PlayerNameMenu.OnClientTryChangePlayerName += CmdTryChangePlayerName;
        if (NetworkServer.active) { return; }
        ((SiltexNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        PlayerNameMenu.OnClientTryChangePlayerName -= CmdTryChangePlayerName;
        ClientOnInfoUpdated?.Invoke();
        if (NetworkServer.active) { return; }
        ((SiltexNetworkManager)NetworkManager.singleton).Players.Remove(this);
    }

    private void ClienOnStartGame()
    {
        playerAgent.enabled = true;
        playerInput.enabled = true;
        playerSpell.enabled = true;
    }

    #endregion
}
