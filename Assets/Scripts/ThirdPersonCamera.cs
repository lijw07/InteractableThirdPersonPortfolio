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
    private PlayerInputActions inputActions;
    
    private void Awake()
    {
        inputActions = GetComponent<PlayerInputActions>();
        
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
        
        // Auto-find target if not assigned
        if (target == null)
        {
            GameObject player = GameObject.Find("LookAt");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Lock cursor
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
        // Get mouse input
        Vector2 mouseInput = inputActions.LookValue;
        
        // Update rotation angles
        currentX += mouseInput.x * mouseSensitivity * Time.deltaTime;
        currentY -= mouseInput.y * mouseSensitivity * Time.deltaTime;
        
        // Clamp vertical rotation
        currentY = Mathf.Clamp(currentY, -30f, 60f);
    }
    
    private void UpdateCameraPosition()
    {
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        
        // Calculate position offset
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        
        // Set camera position and rotation
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        transform.position = targetPosition + offset;
        transform.rotation = rotation;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}