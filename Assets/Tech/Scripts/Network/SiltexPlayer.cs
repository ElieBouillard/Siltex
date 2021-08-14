using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System;

public class SiltexPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerParent = null;
    [SerializeField] private PlayerAnim playerAnim = null;
    [SerializeField] private NavMeshAgent playerAgent = null;
    [SerializeField] private PlayerInput playerInput = null;
    [SerializeField] private PlayerSpell playerSpell = null;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName = null;

    private bool isDead = false;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;
    public static event Action<int> ClientSetCamera;

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    public bool GetIsPlayerDead()
    {
        return isDead;
    }

    #region Server

    public override void OnStartServer()
    {
        DontDestroyOnLoad(this);
    }

    [Server]
    public void ServerSetPlayerPos(Vector3 targetPos)
    {
        if(targetPos == null) { return; }
        transform.position = targetPos;
    }

    [Server]
    public void ServerSetPlayerDeath(bool state)
    {
        isDead = state;
        if(state == true)
        {
            playerAnim.CastDeath();
            ClientOnDie(connectionToClient);
            Invoke(nameof(HideCharacterWhenDead), 2f);
        }
    }

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

    [Server]
    public void ServerStartMatch()
    {
        playerAgent.enabled = true;
        playerInput.enabled = true;
        playerSpell.enabled = true;
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
        DontDestroyOnLoad(this);
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

    [Client]
    public void ClientOnStartMatch()
    {
        playerAgent.enabled = true;
        playerInput.enabled = true;
        playerSpell.enabled = true;
    }

    [Client]
    public void ClientSetCameraPosition(int matchIndex)
    {
        ClientSetCamera?.Invoke(matchIndex);
    }

    [ClientRpc]
    private void HideCharacterWhenDead()
    {
        playerParent.SetActive(false);
    }

    [TargetRpc]
    private void ClientOnDie(NetworkConnection conn)
    {
        playerAgent.enabled = false;
        playerInput.enabled = false;
        playerSpell.enabled = false;
    }
    #endregion
}
