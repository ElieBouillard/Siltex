using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private int InitialHeatlh = 0;
    [SerializeField] private SiltexPlayer m_siltexPlayer;

    public event Action<int, int> ClientOnHealthUpdated;

    [SyncVar(hook =nameof(HookClientOnHealthUpdated))]
    private int currHealth = 0;

    private void Start()
    {
        if (isClient) 
        {
            currHealth = InitialHeatlh;
        }

        if (isServer)
        {
            m_siltexPlayer = this.gameObject.GetComponent<SiltexPlayer>();
        }

    }

    [Server]
    public void DealDamage(int value)
    {
        currHealth -= value;

        if(currHealth > 0) { return; }

        m_siltexPlayer.ServerSetPlayerDeath(true);
    }

    private void HookClientOnHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(currHealth, InitialHeatlh);
    }
}
