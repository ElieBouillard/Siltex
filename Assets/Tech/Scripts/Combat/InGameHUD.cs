using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class InGameHUD : NetworkBehaviour
{
    [SerializeField]
    private Image qSpellImg;
    [SerializeField]
    private Image qSpellCdImg;
    [SerializeField]
    private TMP_Text qSpellCdTxt;
    [SerializeField]
    private Image dodgeImg;
    [SerializeField]
    private Image dodgeCdImg;
    [SerializeField]
    private TMP_Text dodgeCdTxt;

    private GameObject playerObj;
    private PlayerMovement playerMovement;
    private PlayerSpell playerSpell;

    [SerializeField]
    private float speedSpellCdImageFill;
    private Color canSpellColor = new Color(1f,1f,1f,1f);
    private Color cantSpellColor = new Color(1f,1f,1f,0.35f);

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
        UpdateSpellsCouldown(dodgeImg, dodgeCdImg, dodgeCdTxt, playerMovement.dodgeCouldownTimer, playerMovement.GetDodgeCouldown());
        UpdateSpellsCouldown(qSpellImg, qSpellCdImg, qSpellCdTxt, playerSpell.currQSpellCD, playerSpell.GetQSpellCouldown());
    }

    [ClientCallback]
    private void UpdateSpellsCouldown(Image spellImg, Image spellCdImg, TMP_Text spellCdTxt, float spellCdTimer, float spellCd) 
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

        if(spellCdTimer < spellCd && spellCdTimer > 0)
        {
            spellImg.GetComponent<Image>().color = cantSpellColor;
            spellCdTxt.text = spellCdTimer.ToString("0.0");
        }
        
        if(spellCdTimer <= 0)
        {
            spellImg.GetComponent<Image>().color = canSpellColor;
            spellCdTxt.text = "";
        }
    }
}
