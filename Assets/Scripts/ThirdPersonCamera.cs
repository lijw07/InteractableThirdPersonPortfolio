using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float heightOffset = 1.5f;
    
    private float currentX = 0f;
    private float currentY = 20f;
    private PlayerController playerController;
    
    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError("ThirdPersonCamera: PlayerController not found!");
        }
        
        if (target == null)
        {
            GameObject player = GameObject.Find("LookAt");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleCameraRotation();
        UpdateCameraPosition();
    }
    
    private void HandleCameraRotation()
    {
        if (playerController == null) return;
        
        Vector2 mouseInput = playerController.GetLookInput();
        
        currentX += mouseInput.x * mouseSensitivity * Time.deltaTime;
        currentY -= mouseInput.y * mouseSensitivity * Time.deltaTime;
        
        currentY = Mathf.Clamp(currentY, -30f, 60f);
    }
    
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        transform.position = targetPosition + offset;
        transform.rotation = rotation;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}