using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class InGameHUD : NetworkBehaviour
{
    [SerializeField]
    private Image qSpellCdImg;
    [SerializeField]
    private Image dodgeCdImg;

    private GameObject playerObj;
    private PlayerMovement playerMovement;
    private PlayerSpell playerSpell;

    private float progressImageVelocity;

    [ClientCallback]
    private void Start()
    {
        playerObj = NetworkClient.localPlayer.gameObject;
        playerMovement = playerObj.GetComponent<PlayerMovement>();
        playerSpell = playerObj.GetComponent<PlayerSpell>();
    }

    [ClientCallback]
    private void Update()
    {
        UpdateSpellsCouldown(dodgeCdImg, playerMovement.dodgeCouldownTimer, playerMovement.GetDodgeCouldown());
        UpdateSpellsCouldown(qSpellCdImg, playerSpell.currQSpellCD, playerSpell.GetQSpellCouldown());
    }

    [ClientCallback]
    private void UpdateSpellsCouldown(Image spellCdImg, float spellCdTimer, float spellCd) 
    {
        float newProgress = spellCdTimer / spellCd;

        if (newProgress < spellCdImg.fillAmount)
        {
            spellCdImg.fillAmount = Mathf.SmoothDamp(spellCdImg.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
        else
        {
            spellCdImg.fillAmount = newProgress;

        }
    }
}
