using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 6f;
    
    [Header("Acceleration Settings")]
    [SerializeField] private float accelerationTime = 0.3f;
    [SerializeField] private float decelerationTime = 0.2f;
    [SerializeField] private float inputSmoothTime = 0.1f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private bool useMouseRotation = true; // New setting to control rotation behavior
    
    private const float ZERO_THRESHOLD = 0.001f;
    private const float INPUT_THRESHOLD = 0.1f;
    
    private CharacterController characterController;
    private PlayerInputActions inputActions;
    
    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity;
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
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void Start()
    {
        SubscribeToInputEvents();
        InitializePreviousPosition();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }
    
    private void Update()
    {
        UpdateInputState();
        UpdateMovementState();
        ProcessMovementOrRotation();
        UpdateVelocityTracking();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = GetComponent<PlayerInputActions>();
        
        ValidateComponents();
        EnsureInputActionsExists();
    }
    
    private void ValidateComponents()
    {
        if (characterController == null)
            Debug.LogError("PlayerController: CharacterController component missing!");
    }
    
    private void EnsureInputActionsExists()
    {
        if (inputActions == null)
            inputActions = gameObject.AddComponent<PlayerInputActions>();
    }
    
    private void InitializePreviousPosition()
    {
        previousPosition = transform.position;
    }
    
    #endregion
    
    #region Input Events
    
    private void SubscribeToInputEvents()
    {
        inputActions.OnSprintStart += SetSprintingTrue;
        inputActions.OnSprintEnd += SetSprintingFalse;
        inputActions.OnCrouchToggle += ToggleWalking;
    }
    
    private void UnsubscribeFromInputEvents()
    {
        if (inputActions == null) return;
        
        inputActions.OnSprintStart -= SetSprintingTrue;
        inputActions.OnSprintEnd -= SetSprintingFalse;
        inputActions.OnCrouchToggle -= ToggleWalking;
    }
    
    #endregion
    
    #region Core Update Loop
    
    private void UpdateInputState()
    {
        Vector2 rawInput = inputActions.MoveValue;
        SmoothInputValues(rawInput);
    }
    
    private void UpdateMovementState()
    {
        CalculateTargetSpeed();
        SmoothCurrentSpeed();
    }
    
    private void ProcessMovementOrRotation()
    {
        bool isMoving = ShouldProcessMovement();
        
        if (isMoving)
        {
            ProcessMovement();
        }
        else
        {
            ClearMovementIfStopped();
        }
        
        HandleRotation();
    }
    
    private void UpdateVelocityTracking()
    {
        CalculateActualVelocity();
        UpdatePreviousPosition();
    }
    
    #endregion
    
    #region Input Processing
    
    private void SmoothInputValues(Vector2 rawInput)
    {
        currentHorizontal = Mathf.SmoothDamp(currentHorizontal, rawInput.x, ref horizontalVelocity, inputSmoothTime);
        currentVertical = Mathf.SmoothDamp(currentVertical, rawInput.y, ref verticalVelocity, inputSmoothTime);
        
        ClampInputValuesToZero();
    }
    
    private void ClampInputValuesToZero()
    {
        ClampToZero(ref currentHorizontal, ref horizontalVelocity);
        ClampToZero(ref currentVertical, ref verticalVelocity);
    }
    
    private bool HasSignificantInput() => GetInputMagnitude() > INPUT_THRESHOLD;
    
    private float GetInputMagnitude()
    {
        return new Vector2(currentHorizontal, currentVertical).magnitude;
    }
    
    private Vector3 GetWorldSpaceInputDirection()
    {
        return new Vector3(currentHorizontal, 0f, currentVertical).normalized;
    }
    
    #endregion
    
    #region Movement Processing
    
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
        SetCurrentMoveDirection(smoothMoveDirection);
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
        Vector3 movement = direction * currentSpeed * Time.deltaTime;
        characterController.Move(movement);
    }
    
    private void ClearMovementIfStopped()
    {
        if (!HasSignificantSpeed())
        {
            ClearMoveDirection();
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
        
        if (isSprinting && ShouldProcessMovement())
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
    
    private void RotateWithCamera()
    {
        if (ThirdPersonCamera.Instance == null) return;
        
        Vector3 cameraForward = ThirdPersonCamera.Instance.GetForwardDirection();
        if (cameraForward.sqrMagnitude < 0.001f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
    
    private void RotateTowardsMovement(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
    
    private void SetCurrentMoveDirection(Vector3 direction) => currentMoveDirection = direction;
    private void ClearMoveDirection() => currentMoveDirection = Vector3.zero;
    
    #endregion
    
    #region Speed Calculation
    
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
        if (isWalking) return walkSpeed;
        
        if (IsMovingSideways())
        {
            return walkSpeed;
        }
        
        if (isSprinting && CanSprintInCurrentDirection()) return sprintSpeed;
        return runSpeed;
    }
    
    private void SmoothCurrentSpeed()
    {
        float smoothTime = GetSpeedSmoothTime();
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);
        ClampToZero(ref currentSpeed, ref speedVelocity);
    }
    
    private float GetSpeedSmoothTime()
    {
        return targetSpeed > currentSpeed ? accelerationTime : decelerationTime;
    }
    
    private bool HasSignificantSpeed() => currentSpeed > INPUT_THRESHOLD;
    
    #endregion
    
    #region Rotation State Management
    
    private bool CanSprintInCurrentDirection()
    {
        if (!HasSignificantInput()) return false;
        
        Vector3 inputDirection = GetWorldSpaceInputDirection();
        Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
        
        if (moveDirection.magnitude < INPUT_THRESHOLD) return false;
        
        float angle = Vector3.Angle(transform.forward, moveDirection);
        return angle <= 60f;
    }
    
    private bool IsMovingSideways()
    {
        if (!HasSignificantInput()) return false;
        
        Vector3 inputDirection = GetWorldSpaceInputDirection();
        Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
        
        if (moveDirection.magnitude < INPUT_THRESHOLD) return false;
        
        float angle = Vector3.Angle(transform.forward, moveDirection);
        return angle > 60f && angle < 120f;
    }

    private float GetMovementDirectionDot()
    {
        if (!HasSignificantInput()) return 0f;
        
        Vector3 inputDirection = GetWorldSpaceInputDirection();
        Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
        
        if (moveDirection.magnitude < INPUT_THRESHOLD) return 0f;
        
        return Vector3.Dot(transform.forward, moveDirection.normalized);
    }
    
    public bool IsMovingBackward()
    {
        return GetMovementDirectionDot() < -0.5f;
    }
    
    public bool IsMovingForward()
    {
        return GetMovementDirectionDot() > 0.5f;
    }
    
    #endregion
    
    #region Velocity Tracking
    
    private void CalculateActualVelocity()
    {
        calculatedVelocity = (transform.position - previousPosition) / Time.deltaTime;
    }
    
    private void UpdatePreviousPosition()
    {
        previousPosition = transform.position;
    }
    
    #endregion
    
    #region Utility Methods
    
    private void ClampToZero(ref float value, ref float velocity)
    {
        if (Mathf.Abs(value) < ZERO_THRESHOLD)
        {
            value = 0f;
            velocity = 0f;
        }
    }
    
    #endregion
    
    #region Public Interface for Animation
    
    public float GetCurrentSpeed() => currentSpeed;
    public float GetTargetSpeed() => targetSpeed;
    public float GetCurrentHorizontal() => currentHorizontal;
    public float GetCurrentVertical() => currentVertical;
    public bool IsWalking() => isWalking;
    public bool IsSprinting() => isSprinting;
    
    #endregion
    
    #region Public Interface for Debug
    
    public float GetSpeedVelocity() => speedVelocity;
    public Vector3 GetCurrentMoveDirection() => currentMoveDirection;
    public Vector3 GetCalculatedVelocity() => calculatedVelocity;
    public PlayerInputActions GetInputActions() => inputActions;
    private void SetSprintingTrue() => isSprinting = true;
    private void SetSprintingFalse() => isSprinting = false;
    private void ToggleWalking() => isWalking = !isWalking;
    
    public Vector3 GetRawMoveDirection() => currentMoveDirection;
    public Vector3 GetSmoothedMoveDirection() => smoothMoveDirection;
    #endregion
}