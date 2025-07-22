using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6f;
    
    [Header("Acceleration Settings")]
    [SerializeField] private float accelerationTime = 0.3f;
    [SerializeField] private float decelerationTime = 0.2f;
    [SerializeField] private float inputSmoothTime = 0.1f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private bool useMouseRotation = true;
    
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float groundCheckRadius = 0.3f;
    
    private const float ZERO_THRESHOLD = 0.001f;
    private const float INPUT_THRESHOLD = 0.1f;
    private const float SPRINT_ANGLE_THRESHOLD = 60f;
    private const float SIDEWAYS_MIN_ANGLE = 60f;
    private const float SIDEWAYS_MAX_ANGLE = 120f;
    
    public CharacterController characterController;
    public CharacterManageController characterManager;
    public PlayerInputActions inputActions;
    
    private float currentSpeed;
    private float targetSpeed;
    private bool isWalking;
    private bool isSprinting;
    private Vector3 currentMoveDirection;
    
    private float currentHorizontal;
    private float currentVertical;
    private float horizontalVelocity;
    private float verticalVelocity;
    
    private Vector3 calculatedVelocity;
    private Vector3 previousPosition;
    
    private Vector3 smoothMoveDirection;
    private Vector3 moveDirectionVelocity;
    
    private float verticalVelocityY;
    private bool isGrounded;
    private bool hasLandedOnce = false;
    
    private bool canMove = true;
    
    private void Awake()
    {
        InitializeInputComponents();
    }
    
    private void Start()
    {
        InitializeCharacterComponents();
        SubscribeToInputEvents();
        previousPosition = transform.position;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }
    
    private void Update()
    {
        CheckGroundStatus();
        UpdateGravity();
        if (!canMove)
        {
            ApplyMovement(Vector3.zero);
            UpdateVelocityTracking();
            return;
        }

        UpdateInputState();
        UpdateMovementState();
        ProcessMovementAndRotation();
        UpdateVelocityTracking();
    }

    private void CheckGroundStatus()
    {
        bool controllerGrounded = characterController != null && characterController.isGrounded;
        
        bool raycastGrounded = false;
        if (characterController != null)
        {
            Vector3 spherePosition = transform.position + (Vector3.up * groundCheckRadius);
            raycastGrounded = Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, 
                out RaycastHit hit, groundCheckDistance + groundCheckRadius, groundLayerMask);
        }
        
        bool wasGrounded = isGrounded;
        isGrounded = controllerGrounded || raycastGrounded;
        
        if (!wasGrounded && isGrounded && !hasLandedOnce)
        {
            hasLandedOnce = true;
        }
    }
    
    private void UpdateGravity()
    {
        if (isGrounded && verticalVelocityY < 0)
        {
            verticalVelocityY = -2f;
        }
        else
        {
            verticalVelocityY += gravity * Time.deltaTime;
        }
    }
    
    private void InitializeInputComponents()
    {
        inputActions = GetComponent<PlayerInputActions>();
        
        if (inputActions == null)
            inputActions = gameObject.AddComponent<PlayerInputActions>();
    }
    
    private void InitializeCharacterComponents()
    {
        characterManager = GetComponent<CharacterManageController>();
        
        if (characterManager == null)
            Debug.LogError("PlayerController: CharacterManageController component missing!");
        else
            UpdateCharacterController();
    }
    
    private void UpdateCharacterController()
    {
        if (characterManager != null)
        {
            characterController = characterManager.GetCurrentCharacterController();
            if (characterController == null)
                Debug.LogError("PlayerController: No CharacterController found on active character!");
        }
    }
    
    private void SubscribeToInputEvents()
    {
        inputActions.OnSprintStart += () => isSprinting = true;
        inputActions.OnSprintEnd += () => isSprinting = false;
        inputActions.OnCrouchToggle += () => isWalking = !isWalking;
    }
    
    private void UnsubscribeFromInputEvents()
    {
        if (inputActions == null) return;
        
        inputActions.OnSprintStart -= () => isSprinting = true;
        inputActions.OnSprintEnd -= () => isSprinting = false;
        inputActions.OnCrouchToggle -= () => isWalking = !isWalking;
    }
    
    private void UpdateInputState()
    {
        Vector2 rawInput = inputActions.MoveValue;
        SmoothInputValues(rawInput);
    }
    
    private void UpdateMovementState()
    {
        CalculateTargetSpeed();
        currentSpeed = targetSpeed;
    }
    
    private void ProcessMovementAndRotation()
    {
        bool isMoving = ShouldProcessMovement();
        
        if (isMoving)
        {
            ProcessMovement();
        }
        else
        {
            ClearMovementIfStopped();
            ApplyMovement(Vector3.zero);
        }
        
        HandleRotation();
    }
    
    private void UpdateVelocityTracking()
    {
        calculatedVelocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }
    
    private void SmoothInputValues(Vector2 rawInput)
    {
        currentHorizontal = Mathf.SmoothDamp(currentHorizontal, rawInput.x, ref horizontalVelocity, inputSmoothTime);
        currentVertical = Mathf.SmoothDamp(currentVertical, rawInput.y, ref verticalVelocity, inputSmoothTime);
        
        ClampToZero(ref currentHorizontal, ref horizontalVelocity);
        ClampToZero(ref currentVertical, ref verticalVelocity);
    }
    
    private bool ShouldProcessMovement()
    {
        return HasSignificantInput() && HasSignificantSpeed();
    }
    
    private void ProcessMovement()
    {
        Vector3 inputDirection = GetWorldSpaceInputDirection();
        Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
        
        ApplyMovement(moveDirection);
        smoothMoveDirection = Vector3.SmoothDamp(smoothMoveDirection, moveDirection, ref moveDirectionVelocity, 0.1f);
        currentMoveDirection = smoothMoveDirection;
    }
    
    private void ClearMovementIfStopped()
    {
        if (!HasSignificantSpeed())
        {
            currentMoveDirection = Vector3.zero;
        }
    }
    
    private void HandleRotation()
    {
        if (!useMouseRotation)
        {
            if (ShouldProcessMovement())
            {
                Vector3 inputDirection = GetWorldSpaceInputDirection();
                Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
                RotateTowardsMovement(moveDirection);
            }
            return;
        }
        
        if (ShouldProcessMovement())
        {
            if (isSprinting)
            {
                Vector3 inputDirection = GetWorldSpaceInputDirection();
                Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
                RotateTowardsMovement(moveDirection);
            }
            else
            {
                RotateWithCamera();
            }
        }
    }
    
    private Vector3 CalculateCameraRelativeDirection(Vector3 inputDirection)
    {
        if (ThirdPersonCamera.Instance == null)
        {
            Debug.LogWarning("PlayerController: ThirdPersonCamera instance not found");
            return inputDirection;
        }
        
        return ThirdPersonCamera.Instance.GetCameraRelativeDirection(inputDirection);
    }
    
    private void ApplyMovement(Vector3 direction)
    {
        if (characterController == null)
        {
            UpdateCharacterController();
            if (characterController == null) return;
        }
        
        if (!characterController.enabled || !characterController.gameObject.activeInHierarchy)
        {
            UpdateCharacterController();
            return;
        }
        
        Vector3 horizontalMovement = direction * currentSpeed * Time.deltaTime;
        
        Vector3 verticalMovement = new Vector3(0, verticalVelocityY * Time.deltaTime, 0);
        
        Vector3 totalMovement = horizontalMovement + verticalMovement;
        
        Vector3 childPosBefore = characterController.transform.position;
        
        characterController.Move(totalMovement);
        
        Vector3 childPosAfter = characterController.transform.position;
        Vector3 deltaMovement = childPosAfter - childPosBefore;
        
        transform.position += deltaMovement;
        
        characterController.transform.localPosition = Vector3.zero;
    }
    
    private void RotateWithCamera()
    {
        if (ThirdPersonCamera.Instance == null) return;
        
        Vector3 cameraForward = ThirdPersonCamera.Instance.GetForwardDirection();
        if (cameraForward.sqrMagnitude < ZERO_THRESHOLD) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
    
    private void RotateTowardsMovement(Vector3 direction)
    {
        if (direction.sqrMagnitude < ZERO_THRESHOLD) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
    
    private void CalculateTargetSpeed()
    {
        if (!HasSignificantInput())
        {
            targetSpeed = 0f;
            return;
        }
        
        targetSpeed = GetSpeedForCurrentState();
    }
    
    private float GetSpeedForCurrentState()
    {
        if (isWalking || IsMovingSideways()) return walkSpeed;
        if (isSprinting && CanSprintInCurrentDirection()) return sprintSpeed;
        return runSpeed;
    }
    
    
    private bool CanSprintInCurrentDirection()
    {
        return HasSignificantInput() && GetAngleToMovementDirection() <= SPRINT_ANGLE_THRESHOLD;
    }
    
    private bool IsMovingSideways()
    {
        if (!HasSignificantInput()) return false;
        
        float angle = GetAngleToMovementDirection();
        return angle > SIDEWAYS_MIN_ANGLE && angle < SIDEWAYS_MAX_ANGLE;
    }
    
    private float GetAngleToMovementDirection()
    {
        Vector3 inputDirection = GetWorldSpaceInputDirection();
        Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
        
        if (moveDirection.magnitude < INPUT_THRESHOLD) return 0f;
        
        return Vector3.Angle(transform.forward, moveDirection);
    }
    
    private bool HasSignificantInput()
    {
        return GetInputMagnitude() > INPUT_THRESHOLD;
    }
    
    private float GetInputMagnitude()
    {
        return new Vector2(currentHorizontal, currentVertical).magnitude;
    }
    
    private Vector3 GetWorldSpaceInputDirection()
    {
        return new Vector3(currentHorizontal, 0f, currentVertical).normalized;
    }
    
    private bool HasSignificantSpeed()
    {
        return currentSpeed > INPUT_THRESHOLD;
    }
    
    private void ClampToZero(ref float value, ref float velocity)
    {
        if (Mathf.Abs(value) < ZERO_THRESHOLD)
        {
            value = 0f;
            velocity = 0f;
        }
    }
    
    public void OnCharacterSwitched()
    {
        UpdateCharacterController();
    }
    
    public float GetCurrentSpeed() => currentSpeed;
    public float GetTargetSpeed() => targetSpeed;
    public float GetCurrentHorizontal() => currentHorizontal;
    public float GetCurrentVertical() => currentVertical;
    public bool IsWalking() => isWalking;
    public bool IsSprinting() => isSprinting;
    public Vector3 GetCurrentMoveDirection() => currentMoveDirection;
    public Vector3 GetCalculatedVelocity() => calculatedVelocity;
    public PlayerInputActions GetInputActions() => inputActions;
    public Vector3 GetRawMoveDirection() => currentMoveDirection;
    public Vector3 GetSmoothedMoveDirection() => smoothMoveDirection;
    public CharacterController GetCharacterController() => characterController;
   public bool IsGrounded() => isGrounded;
   public bool HasLandedOnce() => hasLandedOnce;
   public void EnableMovement() => canMove = true;
   public void DisableMovement() => canMove = false;
    public float GetVerticalVelocity() => verticalVelocityY;
}