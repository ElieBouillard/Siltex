using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerAnim : NetworkBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private NavMeshAgent playerAgent;
    [Range(0f, 10f)]
    [SerializeField] private float speedRun;

    [ServerCallback]
    private void Update()
    {
        if (playerAgent.velocity.magnitude < speedRun) { SetRun(false); return; }

        if (playerAnimator.GetBool("Run") == true) { return; }

        SetRun(true);
    }

    [ClientRpc]
    private void SetRun(bool state)
    {
        playerAnimator.SetBool("Run", state);
    }

    [ClientRpc]
    public void CastSpellAnim()
    {
        playerAnimator.SetTrigger("CastSpell");
    }
}
