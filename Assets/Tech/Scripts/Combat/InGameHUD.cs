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

    [SerializeField]
    private float speedSpellCdImageFill;

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
        float smoothProgress = Mathf.Lerp(spellCdImg.fillAmount, spellCdTimer/spellCd, Time.deltaTime * speedSpellCdImageFill);

        if(smoothProgress < spellCdImg.fillAmount)
        {
            spellCdImg.fillAmount = smoothProgress;
        }
        else
        {
            spellCdImg.fillAmount = spellCdTimer / spellCd;
        }

        //float newProgress = spellCdTimer / spellCd;
        //spellCdImg.fillAmount = newProgress;

        //if (newProgress < spellCdImg.fillAmount)
        //{
        //    spellCdImg.fillAmount = Mathf.SmoothDamp(spellCdImg.fillAmount, newProgress, ref progressImageVelocity, 0.13f);
        //}
        //else
        //{
            
        //}
    }
}
