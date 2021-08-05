using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    [SerializeField] private PlayerSpell playerSpell = null;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
            playerMovement.CmdTryMove(hit.point);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
            Vector3 dir = (hit.point - transform.position).normalized;
            dir.y = 0;
            playerSpell.CmdTryShoot(dir);
        }
    }
}
