using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private int InitialHeatlh = 0;
    [SerializeField] private Renderer playerRenderer;

    private SiltexPlayer m_siltexPlayer;
    
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
    public void SetCurrHealth(int health)
    {
        currHealth = health;
    }

    [Server]
    public void DealDamage(int value)
    {
        currHealth -= value;
        ClientTakeDamageFeedBack();

        if(currHealth > 0) { return; }

        m_siltexPlayer.ServerSetPlayerDeath(true);
    }

    [ClientRpc]
    private void ClientTakeDamageFeedBack()
    {
        playerRenderer.material.SetColor("_BaseColor", Color.red);
        Invoke(nameof(ReinitiatePlayerColor), 0.2f);
    }

    [ClientRpc]
    private void ReinitiatePlayerColor()
    {
        playerRenderer.material.SetColor("_BaseColor", Color.white);
    }

    private void HookClientOnHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(currHealth, InitialHeatlh);
    }
}
