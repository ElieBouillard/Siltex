using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private PlayerAnim playerAnim;
    [SerializeField] private NavMeshAgent m_agent = null;
    [SerializeField] private float dodgeDistance = 0f;
    [SerializeField] private float dodgeSpeed = 0f;

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

    [Command]
    public void CmdDodge(Vector3 dir)
    {
        m_agent.ResetPath();
        playerAnim.CastDodgeAnim();
        Vector3 targetPos = transform.position + dir * dodgeDistance;
        m_agent.SetDestination(targetPos);
        m_agent.speed = dodgeSpeed;
    }

    [Server]
    private void ServerMove(Vector3 pos)
    {
        m_agent.speed = 5;
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
