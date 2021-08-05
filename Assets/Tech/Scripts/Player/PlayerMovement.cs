using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent m_agent = null;

    [Command]
    public void CmdTryMove(Vector3 pos)
    {
        ServerMove(pos);
    }

    [Server]
    private void ServerMove(Vector3 pos)
    {
        m_agent.SetDestination(pos);
    }
}
