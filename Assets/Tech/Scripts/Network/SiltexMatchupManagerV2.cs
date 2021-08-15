using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class SiltexMatchupManagerV2 : NetworkBehaviour
{
    [SerializeField] private bool DisableMatchMaking = false;

    [Header("HUD")]
    [SerializeField] private GameObject matchupHud = null;
    [SerializeField] private List<TMP_Text> semiFinalsPlayerNameTxts = new List<TMP_Text>();
    [SerializeField] private List<TMP_Text> finalsPlayerNameTxts = new List<TMP_Text>();
    [SerializeField] private List<TMP_Text> winnerPlayerNameTxts = null;
    [SerializeField] private TMP_Text startGameCounterTxt = null;

    public enum MatchmakingState { SetMatchmaking, SemiFinal, Final, Winner }
    [SerializeField]
    private MatchmakingState m_matchmakingState;

    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private List<SiltexPlayer> playersInGame = new List<SiltexPlayer>();
    private List<SiltexPlayer> playersDead = new List<SiltexPlayer>();
    private List<SiltexPlayer> playersToMatch = new List<SiltexPlayer>();

    [SerializeField]
    public List<MatchBehaviour> matchs = new List<MatchBehaviour>();

    [SyncVar]
    public float couldownMatchmaking = -1f;

    [SyncVar]
    public float couldownHideMatchmakingHud = -1f;

    [SyncVar]
    public float couldownStartMatch = -1f;

    #region Server

    public override void OnStartServer()
    {
        for (int i = 0; i < ((SiltexNetworkManager)NetworkManager.singleton).Players.Count; i++)
        {
            playersInGame.Add(((SiltexNetworkManager)NetworkManager.singleton).Players[i]);
        }

        for (int i = 0; i < playersInGame.Count; i++)
        {
            playersInGame[i].ServerSetPlayerDeath(false);
        }

        SiltexPlayer.ServerOnPlayerDeath += ServerOnPlayerDeath;

        if (!DisableMatchMaking)
        {
            couldownMatchmaking = 3f;
        }
        else
        {
            playersInGame[0].ServerSetPlayerPos(new Vector3(12.5f,0f,12.5f));
            ServerStartMatch(true);
            ClientStartMatch(true);
        }
    }

    public override void OnStopServer()
    {
        SiltexPlayer.ServerOnPlayerDeath -= ServerOnPlayerDeath;
    }

    private void Update()
    {
        if (isServer)
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

            if (couldownHideMatchmakingHud != -1)
            {
                if (couldownHideMatchmakingHud > 0)
                {
                    couldownHideMatchmakingHud -= Time.deltaTime;
                }
                else
                {
                    ClientShowMatchmakingHud(false);
                    couldownStartMatch = 3f;
                    couldownHideMatchmakingHud = -1;
                }
            }

            if (couldownStartMatch != -1)
            {
                if (couldownStartMatch > 0)
                {
                    couldownStartMatch -= Time.deltaTime;
                }
                else
                {
                    ServerStartMatch(true);
                    ClientStartMatch(true);
                    couldownStartMatch = -1;
                }
            }

            if (m_matchmakingState == MatchmakingState.SemiFinal)
            {
                int MatchEnded = 0;
                for (int i = 0; i < matchs.Count; i++)
                {
                    if (matchs[i].GetMatchEnded())
                    {
                        MatchEnded++;
                    }
                }
                if (MatchEnded == 2)
                {
                    m_matchmakingState = MatchmakingState.SetMatchmaking;
                    SetMatchmaking();
                }
            }
            else if (m_matchmakingState == MatchmakingState.Final)
            {
                if (matchs[0].GetMatchEnded())
                {
                    matchs.Clear();
                    MatchBehaviour match = new MatchBehaviour();
                    matchs.Add(match);
                    match.AddPlayerToMatch(playersInGame[0]);
                    ClientShowMatchmakingHud(true);
                    ServerSendMatchMakingHudToClient();
                }
            }
        }

        if (isClient)
        {
            if(couldownStartMatch > 0)
            {
                if (startGameCounterTxt.enabled == false)
                {
                    startGameCounterTxt.enabled = true;
                }
                startGameCounterTxt.text = couldownStartMatch.ToString("0");
            }
            else
            {
                if(startGameCounterTxt.enabled == true)
                {
                    startGameCounterTxt.enabled = false;
                }
            }

        }
        
    }

    [Server]
    private void SetMatchmaking()
    {
        ServerStartMatch(false);
        ClientStartMatch(false);
        matchs.Clear();
        playersToMatch.Clear();

        for (int i = 0; i < playersInGame.Count; i++)
        {
            playersToMatch.Add(playersInGame[i]);
        }

        if (playersInGame.Count == 2)
        {
            MatchBehaviour newMatch = new MatchBehaviour();
            matchs.Add(newMatch);

            for (int i = 0; i < playersToMatch.Count; i++)
            {
                newMatch.AddPlayerToMatch(playersToMatch[i]);
            }
            playersToMatch.Clear();
            m_matchmakingState = MatchmakingState.Final;

        }
        else if (playersInGame.Count == 4)
        {
            for (int i = 0; i < playersInGame.Count / 2; i++)
            {
                MatchBehaviour newMatch = new MatchBehaviour();
                matchs.Add(newMatch);
            }

            while (playersToMatch.Count > 0) 
            {
                int randomPlayerIndex = Random.Range(0, playersToMatch.Count);

                if(matchs[0].GetPlayersInMatch().Count < 2)
                {
                    matchs[0].AddPlayerToMatch(playersToMatch[randomPlayerIndex]);
                }
                else
                {
                    matchs[1].AddPlayerToMatch(playersToMatch[randomPlayerIndex]);
                }
                playersToMatch.RemoveAt(randomPlayerIndex);
            }
            matchs[1].SetMatchIndex(1);
            m_matchmakingState = MatchmakingState.SemiFinal;
        }

        playersInGame.Clear();
        for (int i = 0; i < matchs.Count; i++)
        {
            for (int u = 0; u < matchs[i].GetPlayersInMatch().Count; u++)
            {
                playersInGame.Add(matchs[i].GetPlayersInMatch()[u]);
            }
        }

        ClientShowMatchmakingHud(true);
        ServerSetPlayerCameraAndPosOnStartMatch();
        ServerSendMatchMakingHudToClient();
        couldownHideMatchmakingHud = 5f;
    }

    [Server]
    private void ServerStartMatch(bool value)
    {
        for (int i = 0; i < playersInGame.Count; i++)
        {
            playersInGame[i].ServerStartMatch(value);
        }
    }

    [Server]
    private void ServerOnPlayerDeath(SiltexPlayer currDeadPlayer)
    {
        currDeadPlayer.ClientOnStartMatch(false);
        playersInGame.Remove(currDeadPlayer);
        playersDead.Add(currDeadPlayer);
        for (int i = 0; i < matchs.Count; i++)
        {
            if (matchs[i].GetPlayersInMatch().Contains(currDeadPlayer))
            {
                ServerSetCameraPosForEndedMatch(matchs[i]);
                matchs[i].GetPlayersInMatch().Remove(currDeadPlayer);
                matchs[i].SetMatchEnded(true);
            }
        }
    }

    [Server]
    private void ServerSetCameraPosForEndedMatch(MatchBehaviour currMatch)
    {
        for (int i = 0; i < currMatch.GetPlayersInMatch().Count; i++)
        {
            NetworkConnection conn = currMatch.GetPlayersInMatch()[i].connectionToClient;
            if (currMatch.GetMatchIndex() == 0)
            {
                SetPlayerCamera(conn, 1);
            }
            else if(currMatch.GetMatchIndex() == 1)
            {
                SetPlayerCamera(conn, 0);
            }
        }
    }

    [Server]
    private void ServerSetPlayerCameraAndPosOnStartMatch()
    {
        int indexSpawnPoint = 0;
        for (int i = 0; i < matchs.Count; i++)
        {
            for (int u = 0; u < matchs[i].GetPlayersInMatch().Count; u++)
            {
                NetworkConnection playerConn = matchs[i].GetPlayersInMatch()[u].connectionToClient;
                SetPlayerCamera(playerConn, i);
                matchs[i].GetPlayersInMatch()[u].ServerSetPlayerPos(spawnPoints[indexSpawnPoint].position);
                indexSpawnPoint++;
            }
        }
        for (int i = 0; i < playersDead.Count; i++)
        {
            NetworkConnection connn = playersDead[i].connectionToClient;
            SetPlayerCamera(connn, 0);
        }
    }

    [Server]
    private void ServerSendMatchMakingHudToClient()
    {
        List<string> playerNameInOrder = new List<string>();
        for (int i = 0; i < matchs.Count; i++)
        {
            for (int u = 0; u < matchs[i].GetPlayersInMatch().Count; u++)
            {
                playerNameInOrder.Add(matchs[i].GetPlayersInMatch()[u].GetDisplayName());
            }
        }
        ClientSetMatckmakingHud(this.playersInGame, playerNameInOrder);
    }
    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (DisableMatchMaking)
        {
            ClientShowMatchmakingHud(false);
        }
    }

    [TargetRpc]
    private void SetPlayerCamera(NetworkConnection conn, int matchIndex)
    {
        conn.identity.GetComponent<SiltexPlayer>().ClientSetCameraPosition(matchIndex);
    }

    [ClientRpc]
    private void ClientStartMatch(bool value)
    {
        NetworkClient.localPlayer.gameObject.GetComponent<SiltexPlayer>().ClientOnStartMatch(value);
    }

    [ClientRpc]
    private void ClientSetMatckmakingHud(List<SiltexPlayer> playerInGameTemp, List<string> playerNameInOrder)
    {
        List<TMP_Text> currTxt = new List<TMP_Text>();

        if(playerInGameTemp.Count == 4)
        {
            currTxt = semiFinalsPlayerNameTxts;
        }
        else if(playerInGameTemp.Count == 2)
        {
            currTxt = finalsPlayerNameTxts;
        }
        else if(playerInGameTemp.Count == 1)
        {
            currTxt = winnerPlayerNameTxts;
            winnerPlayerNameTxts[0].text = playerInGameTemp[0].GetDisplayName();
        }

        for (int i = 0; i < currTxt.Count; i++)
        {
            currTxt[i].text = playerNameInOrder[i];
        }
    }

    [ClientRpc]
    private void ClientShowMatchmakingHud(bool value)
    {
        matchupHud.SetActive(value);
    }
    #endregion
}
