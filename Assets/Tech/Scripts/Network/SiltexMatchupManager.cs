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

    private List<SiltexPlayer> playersToMatch = new List<SiltexPlayer>();
    private List<SiltexPlayer> Match1 = new List<SiltexPlayer>();
    private List<SiltexPlayer> Match2 = new List<SiltexPlayer>();

    [SyncVar]
    public float couldownMatchmaking;
    private bool canCouldownMatchmaking;
    #region Server

    public override void OnStartServer()
    {
        canCouldownMatchmaking = true;
        couldownMatchmaking = 5f;
    }

    [ServerCallback]
    private void Update()
    {
        if (canCouldownMatchmaking)
        {
            if (couldownMatchmaking > 0)
            {
                couldownMatchmaking -= Time.deltaTime;
            }
            else
            {
                SetMatchmaking();
                canCouldownMatchmaking = false;
            }
        }
    }

    [Server]
    private void SetMatchmaking()
    {
        playersToMatch = ((SiltexNetworkManager)NetworkManager.singleton).Players;
        int playersCount = playersToMatch.Count;

        if (playersCount <= 2)
        {
            for (int i = 0; i < playersToMatch.Count; i++)
            {
                Match1.Add(playersToMatch[i]);
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

        ClientSetMatckmakingHud(Match1, Match2);
    }

    [Server]
    public void StartMatch()
    {
        matchupHud.SetActive(false);
    }

    #endregion

    #region Client
    [ClientRpc]
    private void ClientSetMatckmakingHud(List<SiltexPlayer>match1, List<SiltexPlayer> match2)
    {
        List<TMP_Text> currNames = semiFinalsPlayerNameTxts;
        for (int i = 0; i < match1.Count; i++)
        {
            currNames[i].text = match1[i].GetDisplayName();
        }

        if (match2 == null) { return; }

        for (int i = 0; i < match2.Count; i++)
        {
            currNames[i + match2.Count].text = match2[i].GetDisplayName();
        }
    }


    #endregion
}
