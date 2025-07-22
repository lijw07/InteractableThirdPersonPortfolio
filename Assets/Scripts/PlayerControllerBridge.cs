using System;
using UnityEngine;

public class PlayerControllerBridge : MonoBehaviour
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }
    
    public void SetCanMove() 
    {
        if (playerController != null)
            playerController.SetCanMove();
    }
}
