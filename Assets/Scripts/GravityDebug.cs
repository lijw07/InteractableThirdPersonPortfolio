using UnityEngine;

public class GravityDebug : MonoBehaviour
{
    private PlayerController playerController;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }
    
    void OnGUI()
    {
        if (playerController == null) return;
        
        GUI.color = Color.black;
        GUI.backgroundColor = Color.white;
        
        int y = 10;
        int lineHeight = 25;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"IsGrounded: {playerController.IsGrounded()}");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Vertical Velocity: {playerController.GetVerticalVelocity():F2}");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Current Speed: {playerController.GetCurrentSpeed():F2}");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Target Speed: {playerController.GetTargetSpeed():F2}");
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 400, lineHeight), $"Position Y: {transform.position.y:F2}");
    }
}