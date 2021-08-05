using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private int InitialHeatlh = 0;

    public event Action<int, int> ClientOnHealthUpdated;

    [SyncVar(hook =nameof(HookClientOnHealthUpdated))]
    private int currHealth = 0;

    private void Start()
    {
        currHealth = InitialHeatlh;
    }

    [Server]
    public void DealDamage(int value)
    {
        currHealth -= value;
    }

    private void HookClientOnHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(currHealth, InitialHeatlh);
    }
}
