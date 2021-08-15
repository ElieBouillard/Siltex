using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ReloadSceneButton : MonoBehaviour
{
    [SerializeField] private GameObject ReloadGameButton = null;

    private void Start()
    {
        if (((SiltexNetworkManager)NetworkManager.singleton).GetIsGameInProgress())
        {
            ReloadGameButton.SetActive(true);
        }
    }

    public void AskServerToReloadScene()
    {
        NetworkClient.connection.identity.GetComponent<SiltexPlayer>().CmdReloadGame();
    }
}
