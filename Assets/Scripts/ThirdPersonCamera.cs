using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private static ThirdPersonCamera instance;
    public static ThirdPersonCamera Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<ThirdPersonCamera>();
            return instance;
        }
    }
    
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float heightOffset = 1.5f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 60f;
    
    [Header("Collision Settings")]
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float minCollisionDistance = 1f;
    
    private float currentX = 0f;
    private float currentY = 20f;
    private PlayerController playerController;
    private CharacterManageController characterManager;
    
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 10f;
    [SerializeField] private float zoomSpeed = 5f;

    private float targetDistance;

    
    private void Awake()
    {
        InitializeSingleton();
        InitializeComponents();
        InitializeTarget();
        SetCursorState();
        
        targetDistance = distance;
    }
    
    private void LateUpdate()
    {
        UpdateDynamicTarget();

        if (!IsTargetValid()) return;

        HandleCameraRotation();

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetDistance -= scrollInput * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
        }
        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * zoomSpeed);

        UpdatePosition();
    }

    
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
            Debug.LogError("ThirdPersonCamera: PlayerController not found!");
        
        int playerLayer = LayerMask.NameToLayer("Player");
        collisionLayers = collisionLayers & ~(1 << playerLayer);
        
        if (collisionLayers == -1)
            collisionLayers = ~(1 << LayerMask.NameToLayer("UI"));
    }
    
    private void InitializeTarget()
    {
        if (target != null) return;
        
        target = FindTarget("PlayerController") ?? FindTarget("LookAt");
    }
    
    private Transform FindTarget(string objectName)
    {
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject == null) return null;
        
        if (objectName == "PlayerController")
        {
            Transform lookAtChild = targetObject.transform.Find("LookAt");
            return lookAtChild != null ? lookAtChild : targetObject.transform;
        }
        
        return targetObject.transform;
    }
    
    private void SetCursorState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void UpdateDynamicTarget()
    {
        if (characterManager == null) return;
        
        GameObject activeCharacter = characterManager.GetCurrentCharacter();
        if (activeCharacter == null) return;
        
        Transform newTarget = GetCharacterTarget(activeCharacter);
        if (newTarget != target)
            target = newTarget;
    }
    
    private Transform GetCharacterTarget(GameObject character)
    {
        Transform lookAtChild = character.transform.Find("LookAt");
        return lookAtChild != null ? lookAtChild : character.transform;
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
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));

        currentX += mouseInput.x * mouseSensitivity;
        currentY += mouseInput.y * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, minY, maxY);
    }
    
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
        
        if (Physics.SphereCast(targetPosition, collisionOffset, direction.normalized, out RaycastHit hit, desiredDistance, collisionLayers))
        {
            float hitDistance = Mathf.Clamp(hit.distance - collisionOffset, minCollisionDistance, desiredDistance);
            return targetPosition + direction.normalized * hitDistance;
        }
        
        return desiredPosition;
    }
    
    public Vector3 GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }
    
    public Vector3 GetRightDirection()
    {
        Vector3 right = transform.right;
        right.y = 0f;
        return right.normalized;
    }
    
    public Vector3 GetCameraRelativeDirection(Vector3 inputDirection)
    {
        Vector3 forward = GetForwardDirection();
        Vector3 right = GetRightDirection();
        
        return (forward * inputDirection.z + right * inputDirection.x).normalized;
    }
    
    public float GetSignedAngleFromPlayer(Vector3 playerForward)
    {
        Vector3 cameraForward = GetForwardDirection();
        float angle = Vector3.Angle(playerForward, cameraForward);
        
        Vector3 cross = Vector3.Cross(playerForward, cameraForward);
        if (cross.y < 0) angle = -angle;
        
        return angle;
    }
    
    public Transform GetCameraTransform() => transform;
    public Transform GetCurrentTarget() => target;
    
    private bool IsTargetValid() => target != null;
}