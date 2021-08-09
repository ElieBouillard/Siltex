using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAnim : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private NavMeshAgent playerAgent;
    [Range(0f,10f)]
    [SerializeField] private float speedRun;

    private void Update()
    {
        if(playerAgent.velocity.magnitude < speedRun) { playerAnimator.SetBool("Run", false); return; }
        
        if(playerAnimator.GetBool("Run") == true) { return; }
        playerAnimator.SetBool("Run", true);
    }
}
