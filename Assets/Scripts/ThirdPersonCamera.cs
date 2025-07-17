using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
    
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    private float currentX = 0f;
    private float currentY = 0f;
    private Vector2 lookInput;
    private PlayerInputActions inputActions;
    
    [Header("Aim Mode")]
    [SerializeField] private Vector3 aimOffset = new Vector3(2f, 1.5f, -4f);
    [SerializeField] private float aimSmoothSpeed = 0.2f;
    private bool isAiming = false;
    private Vector3 currentOffset;
    
    void Awake()
    {
        inputActions = GetComponentInParent<PlayerInputActions>();
        if (inputActions == null)
        {
            inputActions = GetComponent<PlayerInputActions>();
        }
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
    }
    
    void Start()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
        
        currentOffset = offset;
        
        inputActions.LookAction.performed += OnLook;
        inputActions.LookAction.canceled += OnLook;
    }
    
    void OnDestroy()
    {
        inputActions.LookAction.performed -= OnLook;
        inputActions.LookAction.canceled -= OnLook;
    }
    
    void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleMouseInput();
        UpdateCameraPosition();
    }
    
    void HandleMouseInput()
    {
        currentX += lookInput.x * mouseSensitivity * Time.deltaTime;
        currentY -= lookInput.y * mouseSensitivity * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
    }
    
    void UpdateCameraPosition()
    {
        Vector3 targetOffset = isAiming ? aimOffset : offset;
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, isAiming ? aimSmoothSpeed : smoothSpeed);
        
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position + rotation * currentOffset;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
    
    public Transform GetTarget()
    {
        return target;
    }
    
    public void SetAimMode(bool aiming)
    {
        isAiming = aiming;
    }
    
    public bool IsAiming()
    {
        return isAiming;
    }
}