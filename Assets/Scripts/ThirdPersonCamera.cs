using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float zoomSensitivity = 0.5f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 20f;
    [SerializeField] private float targetHeightOffset = 1.2f; // Height offset to focus on character's upper body
    
    private float currentX = 0f;
    private float currentY = 20f;
    private PlayerInputActions inputActions;
    
    void Awake()
    {
        inputActions = GetComponent<PlayerInputActions>() ?? gameObject.AddComponent<PlayerInputActions>();
        
        // Lock cursor to game window and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Handle escape key to unlock cursor (for testing in editor)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Re-lock cursor on click
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Handle mouse rotation
        Vector2 mouseInput = inputActions.LookValue;
        currentX += mouseInput.x * mouseSensitivity * Time.deltaTime;
        currentY -= mouseInput.y * mouseSensitivity * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, -30f, 60f);
        
        // Handle zoom
        float scrollInput = inputActions.ScrollValue;
        distance -= scrollInput * zoomSensitivity;
        distance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
        
        // Apply camera position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        
        // Add height offset to focus on character's head/upper body
        Vector3 focusPoint = target.position + Vector3.up * targetHeightOffset;
        
        transform.position = focusPoint + offset;
        transform.LookAt(focusPoint);
    }
    
    public void SetTarget(Transform newTarget) => target = newTarget;
    public Transform GetTarget() => target;
}