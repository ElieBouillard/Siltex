using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class SiltexNetworkManager : NetworkManager
{
    public static event Action ClientOnConnected; 
    public static event Action ClientOnDisconnected;
    public List<SiltexPlayer> Players = new List<SiltexPlayer>();

    private bool isGameInProgress = false;

    public bool GetIsGameInProgress()
    {
        return isGameInProgress;
    }

    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!isGameInProgress) { return; }
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        SiltexPlayer player = conn.identity.GetComponent<SiltexPlayer>();

        Players.Remove(player);
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }
               
    public void StartGame()
    {
        //if(Players.Count < 2) { return; }
        isGameInProgress = true;
        ServerChangeScene("Scene_Map01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map")) { return; }
        base.OnServerAddPlayer(conn);

        SiltexPlayer player = conn.identity.GetComponent<SiltexPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetPartyOwner(Players.Count == 1);

        player.ServerJoinedByPlayer();
    }

    [Server]
    public void ServerReloadScene()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            NetworkServer.Destroy(Players[i].gameObject);
        }
        Players.Clear();
        ServerChangeScene("MainMenu");
    }

    [Server]
    public void ServerSetPlayerNameDisplay(NetworkConnection conn, string newPlayerName)
    {
        conn.identity.GetComponent<SiltexPlayer>().SetDisplayName(newPlayerName);
    }
    #endregion

    #region Client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }
    #endregion
}
