using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class HealthDisplay : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private Image healthBarImage = null;
    [SerializeField] private bool onPlayerObject = false;

    private void Start()
    {
        if (!onPlayerObject) 
        {
            health = NetworkClient.localPlayer.gameObject.GetComponent<Health>();
        }
        health.ClientOnHealthUpdated += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
    }
}
