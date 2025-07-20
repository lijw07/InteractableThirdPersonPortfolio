using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    #region Singleton
    
    private static ThirdPersonCamera instance;
    public static ThirdPersonCamera Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ThirdPersonCamera>();
            }
            return instance;
        }
    }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float heightOffset = 1.5f;
    
    [Header("Collision Settings")]
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float minCollisionDistance = 1f;
    
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 60f;
    
    #endregion
    
    #region Private Fields
    
    private float currentX = 0f;
    private float currentY = 20f;
    private PlayerController playerController;
    private CharacterManageController characterManager;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
        InitializeComponents();
        InitializeTarget();
        InitializeCursor();
    }
    
    private void LateUpdate()
    {
        UpdateDynamicTarget();
        
        if (!IsTargetValid()) return;
        HandleCameraRotation();
        UpdatePosition();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Multiple ThirdPersonCamera instances found. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    
    private void InitializeComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        characterManager = FindFirstObjectByType<CharacterManageController>();
        
        if (playerController == null)
        {
            Debug.LogError("ThirdPersonCamera: PlayerController not found!");
        }
        
        if (collisionLayers == -1)
        {
            collisionLayers = ~(1 << LayerMask.NameToLayer("UI"));
        }
    }
    
    private void InitializeTarget()
    {
        if (target == null)
        {
            GameObject playerController = GameObject.Find("PlayerController");
            if (playerController != null)
            {
                Transform lookAtChild = playerController.transform.Find("LookAt");
                if (lookAtChild != null)
                {
                    target = lookAtChild;
                }
                else
                {
                    target = playerController.transform;
                }
            }
            else
            {
                GameObject lookAtObject = GameObject.Find("LookAt");
                if (lookAtObject != null)
                {
                    target = lookAtObject.transform;
                }
            }
        }
    }
    
    private void InitializeCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    #endregion
    
    #region Core Camera Updates
    
    private void UpdateDynamicTarget()
    {
        if (!characterManager) return;
        
        GameObject activeCharacter = characterManager.GetCurrentCharacter();
        if (activeCharacter != null)
        {
            Transform lookAtChild = activeCharacter.transform.Find("LookAt");
            if (lookAtChild != null && lookAtChild != target)
            {
                target = lookAtChild;
            }
            else if (lookAtChild == null && activeCharacter.transform != target)
            {
                target = activeCharacter.transform;
            }
        }
    }
    
    private void UpdatePosition()
    {
        Quaternion desiredRotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = CalculateDesiredPosition(desiredRotation);
        Vector3 collisionCheckedPosition = ApplyCollisionDetection(desiredPosition);
        
        transform.position = collisionCheckedPosition;
        transform.rotation = desiredRotation;
    }
    
    private void HandleCameraRotation()
    {
        if (!IsTargetValid()) return;

        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));

        currentX += mouseInput.x * mouseSensitivity;
        currentY += mouseInput.y * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, minY, maxY);
    }
    
    #endregion
    
    #region Position Calculation
    
    private Vector3 CalculateDesiredPosition(Quaternion rotation)
    {
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        return targetPosition + offset;
    }
    
    private Vector3 ApplyCollisionDetection(Vector3 desiredPosition)
    {
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        Vector3 direction = desiredPosition - targetPosition;
        float desiredDistance = direction.magnitude;
        
        RaycastHit hit;
        if (Physics.SphereCast(targetPosition, collisionOffset, direction.normalized, out hit, desiredDistance, collisionLayers))
        {
            float hitDistance = hit.distance - collisionOffset;
            hitDistance = Mathf.Clamp(hitDistance, minCollisionDistance, desiredDistance);
            return targetPosition + direction.normalized * hitDistance;
        }
        
        return desiredPosition;
    }
    
    #endregion
    
    #region Public Camera Utilities
    
    /// <summary>
    /// Gets the camera's forward direction on the XZ plane (Y=0)
    /// </summary>
    public Vector3 GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }
    
    /// <summary>
    /// Gets the camera's right direction on the XZ plane (Y=0)
    /// </summary>
    public Vector3 GetRightDirection()
    {
        Vector3 right = transform.right;
        right.y = 0f;
        return right.normalized;
    }
    
    /// <summary>
    /// Transforms input direction to camera-relative direction
    /// </summary>
    public Vector3 GetCameraRelativeDirection(Vector3 inputDirection)
    {
        Vector3 forward = GetForwardDirection();
        Vector3 right = GetRightDirection();
        
        return (forward * inputDirection.z + right * inputDirection.x).normalized;
    }
    
    /// <summary>
    /// Calculates the signed angle between player forward and camera forward
    /// </summary>
    public float GetSignedAngleFromPlayer(Vector3 playerForward)
    {
        Vector3 cameraForward = GetForwardDirection();
        float angle = Vector3.Angle(playerForward, cameraForward);
        
        Vector3 cross = Vector3.Cross(playerForward, cameraForward);
        if (cross.y < 0) angle = -angle;
        
        return angle;
    }
    
    /// <summary>
    /// Gets the current camera transform
    /// </summary>
    public Transform GetCameraTransform()
    {
        return transform;
    }
    
    /// <summary>
    /// Gets the current target transform
    /// </summary>
    public Transform GetCurrentTarget()
    {
        return target;
    }
    
    #endregion
    
    #region Validation Helpers
    
    private bool IsTargetValid()
    {
        return target != null;
    }
    
    #endregion
}