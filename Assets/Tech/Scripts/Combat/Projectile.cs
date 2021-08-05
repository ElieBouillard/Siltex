using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody m_rb = null;
    [SerializeField] private int damage = 0;
    [SerializeField] private float speedTravel = 0;
    [SerializeField] private float spellRange = 0;

    private Vector3 startPos;
    private float distToPlayer;

    private void Start()
    {
        m_rb.velocity = transform.forward * speedTravel; 
    }

    [ServerCallback]
    private void Update()
    {
        distToPlayer = (transform.position - startPos).magnitude;
        if(distToPlayer > spellRange)
        {
            SelfDestroy();
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            if(networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damage);
        }

        SelfDestroy();
    }

    [Server]
    private void SelfDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }


    public void SetStartPos(Vector3 pos)
    {
        startPos = pos;
    }
}
