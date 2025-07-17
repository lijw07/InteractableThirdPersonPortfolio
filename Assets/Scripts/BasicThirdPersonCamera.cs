using UnityEngine;
using UnityEngine.InputSystem;

public class BasicThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
    
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    
    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    private float currentX = 0f;
    private float currentY = 0f;
    private Vector2 lookInput;
    private BasicPlayerInput inputActions;
    
    void Awake()
    {
        inputActions = GetComponentInParent<BasicPlayerInput>();
        if (inputActions == null)
        {
            inputActions = GetComponent<BasicPlayerInput>();
        }
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<BasicPlayerInput>();
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
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position + rotation * offset;
        
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
}