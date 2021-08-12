using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpell : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnim playerAnim;
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private Transform launchAt = null;
    [SerializeField] private GameObject qSpellObj = null;
    [SerializeField] private GameObject qSpellOnFloorImage = null;
    [SerializeField] private SpriteRenderer qSpellOnFloorSpriteRenderer = null;
    [SerializeField] private float qSpellCD = 0f;
    [SerializeField] private float qSpellCanalisationTime = 0.5f;

    [SyncVar]
    private bool canQSpell = true;
    [SyncVar]
    public float currQSpellCD = 0f;

    private float currQSpellCanalisation = 0f;
    private Vector3? currQSpellDir = null;

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
            }

            if(currQSpellCanalisation > 0)
            {
                currQSpellCanalisation -= Time.deltaTime;
            }
            else
            {
                if(currQSpellDir != null)
                {
                    Shoot();
                }
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

        playerMovement.FreezePlayer(qSpellCanalisationTime);
        playerAnim.CastSpellAnim();
        currQSpellCanalisation = qSpellCanalisationTime - 0.75f;
        currQSpellDir = dir;
        transform.forward = currQSpellDir.Value;
        currQSpellCD = qSpellCD;
        canQSpell = false;
    }

    [Server]
    public void Shoot()
    {
        GameObject projectileInstance = Instantiate(qSpellObj, launchAt.position, Quaternion.LookRotation(currQSpellDir.Value));
        NetworkServer.Spawn(projectileInstance, connectionToClient);
        projectileInstance.GetComponent<Projectile>().SetStartPos(launchAt.position);
        currQSpellDir = null;
    }

    #region Client
    public override void OnStartClient()
    {
        if (!hasAuthority) { qSpellOnFloorImage.SetActive(false); }
    }

    public float GetQSpellCouldown()
    {
        return qSpellCD;
    }
    #endregion
}
