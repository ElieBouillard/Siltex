using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class SiltexMatchupManager : NetworkBehaviour
{
    [Header("HUD")]
    [SerializeField] private GameObject matchupHud = null;
    [SerializeField] private List<TMP_Text> semiFinalsPlayerNameTxts = new List<TMP_Text>();
    [SerializeField] private List<TMP_Text> finalsPlayerNameTxts = new List<TMP_Text>();
    [SerializeField] private TMP_Text WinnerPlayerNameTxts = null;

    private List<SiltexPlayer> playersInGame = new List<SiltexPlayer>();
    private List<SiltexPlayer> playersToMatch = new List<SiltexPlayer>();
    private List<SiltexPlayer> Match1 = new List<SiltexPlayer>();
    private List<SiltexPlayer> Match2 = new List<SiltexPlayer>();

    [SyncVar]
    public float couldownMatchmaking = -1f;

    [SyncVar]
    public float couldownStartMatch = -1f;

    [SyncVar]
    public float couldownShowMatchmakingHud = -1f;
    #region Server

    public override void OnStartServer()
    {
        playersInGame = ((SiltexNetworkManager)NetworkManager.singleton).Players;
        couldownMatchmaking = 2f;
    }

    [ServerCallback]
    private void Update()
    {
        if (couldownMatchmaking != -1)
        {
            if (couldownMatchmaking > 0)
            {
                couldownMatchmaking -= Time.deltaTime;
            }
            else
            {
                SetMatchmaking();
                couldownMatchmaking = -1;
            }
        }

        if(couldownShowMatchmakingHud != -1)
        {
            if(couldownShowMatchmakingHud > 0)
            {
                couldownShowMatchmakingHud -= Time.deltaTime;
            }
            else
            {
                SetClientMatchmakingHud(false);
                couldownStartMatch = 3f;
                couldownShowMatchmakingHud = -1;
            }
        }

        if (couldownStartMatch != -1)
        {
            if(couldownStartMatch > 0)
            {
                couldownStartMatch -= Time.deltaTime;
            }
            else
            {
                ServerStartMatch();
                ClientStartMatch();
                couldownStartMatch = -1;
            }
        }
    }

    [Server]
    private void SetMatchmaking()
    {
        for (int i = 0; i < playersInGame.Count; i++)
        {
            playersToMatch.Add(playersInGame[i]);
        }

        if (playersToMatch.Count <= 2)
        {
            for (int i = 0; i < playersToMatch.Count; i++)
            {
                Match1.Add(playersToMatch[i]);
                playersToMatch[i].ClientSetCameraPosition(1);
            }
        }
        else
        {
            Match1.Add(playersToMatch[0]);
            playersToMatch.Remove(playersToMatch[0]);

            while (playersToMatch.Count > 0)
            {
                int randomIndex = Random.Range(0, playersToMatch.Count);
                if (Match1.Count < 2)
                {
                    Match1.Add(playersToMatch[randomIndex]);
                }
                else
                {
                    Match2.Add(playersToMatch[randomIndex]);
                }
                playersToMatch.Remove(playersToMatch[randomIndex]);
            }
        }


        for (int i = 0; i < Match1.Count; i++)
        {
            Match1[i].ServerSetPlayerPos(((SiltexNetworkManager)NetworkManager.singleton).GetStartPosition().position);
            SetPlayerCamera(Match1[i].connectionToClient, 1);
        }

        for (int i = 0; i < Match2.Count; i++)
        {
            Match2[i].ServerSetPlayerPos(((SiltexNetworkManager)NetworkManager.singleton).GetStartPosition().position);
            SetPlayerCamera(Match2[i].connectionToClient, 2);
        }

        ClientSetMatckmakingHud(Match1, Match2);
        couldownShowMatchmakingHud = 5f;
    }

    [Server]
    private void ServerStartMatch()
    {
        for (int i = 0; i < playersInGame.Count; i++)
        {
            playersInGame[i].ServerStartMatch();
        }
    }
    #endregion

    #region Client

    [TargetRpc]
    private void SetPlayerCamera(NetworkConnection conn, int matchIndex)
    {
        conn.identity.GetComponent<SiltexPlayer>().ClientSetCameraPosition(matchIndex);
    }

    [ClientRpc]
    private void ClientStartMatch()
    {
        NetworkClient.localPlayer.gameObject.GetComponent<SiltexPlayer>().ClientOnStartMatch();
    }

    [ClientRpc]
    private void ClientSetMatckmakingHud(List<SiltexPlayer> match1, List<SiltexPlayer> match2)
    {
        int playerCount = match1.Count + match2.Count;
        
        if(playerCount == 1)
        {
            TMP_Text currName = WinnerPlayerNameTxts;
            currName.text = match1[0].GetDisplayName();
        }
        else if(playerCount <= 2)
        {
            List<TMP_Text> currNames = finalsPlayerNameTxts;
            for (int i = 0; i < match1.Count; i++)
            {
                currNames[i].text = match1[i].GetDisplayName();
            }
        }
        else
        {
            List<TMP_Text> currNames = semiFinalsPlayerNameTxts;
            for (int i = 0; i < match1.Count; i++)
            {
                currNames[i].text = match1[i].GetDisplayName();
            }

            if (match2.Count <= 0) { return; }

            for (int i = 0; i < match2.Count; i++)
            {
                currNames[i + match2.Count].text = match2[i].GetDisplayName();
            }
        }
    }

    [ClientRpc]
    private void SetClientMatchmakingHud(bool value)
    {
        matchupHud.SetActive(false);
    }
    #endregion
}
