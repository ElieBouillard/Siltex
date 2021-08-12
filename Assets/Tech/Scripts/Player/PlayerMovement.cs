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
    [SerializeField] private float dodgeCouldown = 3f;
    [SerializeField] private float dodgeDistance = 0f;
    [SerializeField] private float dodgeSpeed = 0f;

    [HideInInspector]
    [SyncVar]
    public float dodgeCouldownTimer = 0f;
    [SyncVar]
    private bool canDodge = true;

    private void Update()
    {
        if (!isServer) { return; }

        if(dodgeCouldownTimer > 0)
        {
            dodgeCouldownTimer -= Time.deltaTime;
        }
        else if(dodgeCouldownTimer != -1)
        {
            canDodge = true;
            dodgeCouldownTimer = -1;
        }        
    }

    #region Server

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
        if (canDodge)
        {
            m_agent.ResetPath();
            playerAnim.CastDodgeAnim();

            Vector3 targetPos = transform.position + dir * dodgeDistance;
            m_agent.SetDestination(targetPos);
            m_agent.speed = dodgeSpeed;
            canDodge = false;
            dodgeCouldownTimer = dodgeCouldown;
        }
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
    #endregion

    #region Client
    public float GetDodgeCouldown()
    {
        return dodgeCouldown;
    }
    #endregion
}
