using UnityEngine;

public class PlayerControllerBridge : MonoBehaviour
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }
    
    public void EnableMovement() 
    {
        if (playerController != null)
            playerController.EnableMovement();
    }
    
    public void DisableMovement() 
    {
        if (playerController != null)
            playerController.DisableMovement();
    }
}
