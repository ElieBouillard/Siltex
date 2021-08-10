using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpell : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private Transform launchAt = null;
    [SerializeField] private GameObject qSpellObj = null;
    [SerializeField] private GameObject qSpellOnFloorImage = null;
    [SerializeField] private SpriteRenderer qSpellOnFloorSpriteRenderer = null;
    [SerializeField] private float qSpellCD = 0;
    [SerializeField] private float qSpellFreezeTime = 0.5f;
    
    [SyncVar]
    private bool canQSpell = true;
    [SyncVar]
    private float currQSpellCD = 0f;

    private void Start()
    {
        currQSpellCD = qSpellCD;
        if (!hasAuthority) { qSpellOnFloorImage.SetActive(false); }
    }

    private void Update()
    {
        if (isServer && !canQSpell)
        {
            if (currQSpellCD > 0)
            {
                currQSpellCD -= Time.deltaTime;
            }
            else
            {
                canQSpell = true;
                currQSpellCD = qSpellCD;
            }
        }

        //Show SpellOnFloor
        if (!hasAuthority) { return; }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask);
        Vector3 mousePos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        qSpellOnFloorImage.transform.forward = (mousePos - transform.position).normalized;
        qSpellOnFloorSpriteRenderer.color = (canQSpell) ? Color.green : Color.red;
    }

    [Command]
    public void CmdTryShoot(Vector3 dir)
    {
        if (!canQSpell) { return; }
        GameObject projectileInstance = Instantiate(qSpellObj, launchAt.position, Quaternion.LookRotation(dir));
        NetworkServer.Spawn(projectileInstance, connectionToClient);
        projectileInstance.GetComponent<Projectile>().SetStartPos(launchAt.position);
        playerMovement.FreezePlayer(qSpellFreezeTime);
        canQSpell = false;
    }
}
