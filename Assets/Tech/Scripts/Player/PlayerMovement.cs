using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent m_agent = null;

    [Command]
    public void CmdTryMove(Vector3 pos)
    {
        ServerMove(pos);
    }

    [Command]
    public void CmdStopMove()
    {
        m_agent.ResetPath();
    }

    [Server]
    private void ServerMove(Vector3 pos)
    {
        m_agent.SetDestination(pos);
    }

    [Server]
    public void FreezePlayer(float time)
    {
        m_agent.isStopped = true;
        Invoke("UnFreezePlayer", time);
    }

    [Server]
    private void UnFreezePlayer()
    {
        m_agent.isStopped = false;
    }
}
