using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class MatchBehaviour
{
    private int matchIndex = 0;
    private bool matchEnded = false;
    private List<SiltexPlayer> playersInMatch;

    public MatchBehaviour()
    {
        matchIndex = 0;
        playersInMatch = new List<SiltexPlayer>();
    }

    public void SetMatchEnded(bool value)
    {
        matchEnded = value;
    }

    public bool GetMatchEnded()
    {
        return matchEnded;
    }

    public void SetMatchIndex(int index)
    {
        matchIndex = index;
    }

    public int GetMatchIndex()
    {
        return matchIndex;
    }

    public List<SiltexPlayer> GetPlayersInMatch()
    {
        return playersInMatch;
    }

    public void AddPlayerToMatch(SiltexPlayer playerToAdd)
    {
        playersInMatch.Add(playerToAdd);
    }

    public void ClearMatch()
    {
        playersInMatch.Clear();
    }
}
