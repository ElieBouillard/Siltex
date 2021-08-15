using UnityEngine;
using Mirror;

public class CameraController : MonoBehaviour
{
    //[SerializeField] private Transform cameraPlayer = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 0f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    [SerializeField] private bool cameraLock = false;

    private void Start()
    {
        SiltexPlayer.ClientSetCamera += SetPlayerCameraPosition;
    }

    private void OnDestroy()
    {
        SiltexPlayer.ClientSetCamera -= SetPlayerCameraPosition;        
    }

    private void Update()
    {
        if (!Application.isFocused) { return; }

        if(cameraLock) 
        {  
            return; 
        }

        Vector3 pos = transform.position;
        Vector3 cursorMovement = Vector3.zero;
        Vector2 cursorPosition = Input.mousePosition;

        if (cursorPosition.y >= Screen.height - screenBorderThickness)
        {
            cursorMovement.z += 1;
        }
        else if (cursorPosition.y <= screenBorderThickness)
        {
            cursorMovement.z -= 1;
        }
        if (cursorPosition.x >= Screen.width - screenBorderThickness)
        {
            cursorMovement.x += 1;
        }
        else if (cursorPosition.x <= screenBorderThickness)
        {
            cursorMovement.x -= 1;
        }

        pos += cursorMovement.normalized * speed * Time.deltaTime;

        //pos.x = Mathf.Clamp(pos.x, screenXLimits.x + transform.position.x, screenXLimits.y + transform.position.x);
        //pos.z = Mathf.Clamp(pos.z, screenZLimits.x + transform.position.z, screenZLimits.y + transform.position.z);

        transform.position = pos;
    }

    private void SetPlayerCameraPosition(int matchIndex)
    {
        if(matchIndex == 0)
        {
            gameObject.transform.position = new Vector3(-12.5f, 13f, -22.5f);
        }
        else if( matchIndex == 1)
        {
            gameObject.transform.position = new Vector3(12.5f, 13f, 2.5f);
        }
    }
}
