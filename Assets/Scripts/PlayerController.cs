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
    
    [Header("Camera Facing Settings")]
    [SerializeField] private float cameraFacingThresholdMin = 45f;
    [SerializeField] private float cameraFacingThresholdMax = 90f;
    [SerializeField] private float minIdleTurnSpeed = 1f;
    [SerializeField] private float maxIdleTurnSpeed = 5f;
    [SerializeField] private bool useTurnInPlaceAnimations = true;
    [SerializeField] private float turnAngleSmoothTime = 0.2f;
    
    [Header("Turn In Place Settings")]
    [SerializeField] private float turnInPlaceRotationSpeed = 90f;
    [SerializeField] private float turnAngleDeadzone = 15f;
    [SerializeField] private float turnCompleteThreshold = 10f;
    [SerializeField] private float turn180SpeedMultiplier = 1.2f;
    
    [Header("Turn Threshold Settings")]
    [SerializeField] private float turnStartThreshold = 60f;
    [SerializeField] private float turnStopThreshold = 55f;
    [SerializeField] private float turn180StartThreshold = 135f;
    [SerializeField] private float turn180StopThreshold = 130f;
    
    [Header("Turn Safety Settings")]
    [SerializeField] private float maxRotationPerFrame = 360f; // degrees
    [SerializeField] private float turnCompletionThreshold = 30f; // degrees - allow turn to finish if close
    
    private const float ZERO_THRESHOLD = 0.001f;
    private const float INPUT_THRESHOLD = 0.1f;
    
    // Core components
    private CharacterController characterController;
    private PlayerInputActions inputActions;
    
    // Movement state
    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity;
    private bool isWalking;
    private bool isSprinting;
    private Vector3 currentMoveDirection;
    
    // Input state
    private float currentHorizontal;
    private float currentVertical;
    private float horizontalVelocity;
    private float verticalVelocity;
    
    // Rotation state
    private float currentTurnAngle;
    private float targetTurnAngle;
    private float turnAngleVelocity;
    private bool isPerformingTurnInPlace;
    
    // Debug rotation tracking
    private Vector3 rotationBeforeTurn;
    private Vector3 rotationAfterTurn;
    private float totalRotationApplied;
    private bool trackingRotation;
    private string lastTurnDirection = "";
    private float turnStartTime;
    private float expectedRotationAmount;
    
    // Turn target tracking
    private Vector3 lockedTurnTargetDirection;
    private bool hasLockedTurnTarget;
    
    // Velocity tracking
    private Vector3 calculatedVelocity;
    private Vector3 previousPosition;
    
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
        ProcessFrame();
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
    
    private void SetSprintingTrue() => isSprinting = true;
    private void SetSprintingFalse() => isSprinting = false;
    private void ToggleWalking() => isWalking = !isWalking;
    
    #endregion
    
    #region Core Update Loop
    
    private void ProcessFrame()
    {
        UpdateInputState();
        UpdateMovementState();
        ProcessMovementOrRotation();
        UpdateVelocityTracking();
    }
    
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
        if (ShouldProcessMovement())
        {
            ProcessMovement();
        }
        else
        {
            ProcessIdleRotation();
            ClearMovementIfStopped();
        }
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
        ApplyMovementRotation(moveDirection);
        SetCurrentMoveDirection(moveDirection);
        ResetTurnInPlace();
    }
    
    private Vector3 CalculateCameraRelativeDirection(Vector3 inputDirection)
    {
        Transform cameraTransform = GetCameraTransform();
        Vector3 cameraForward = GetFlattenedDirection(cameraTransform.forward);
        Vector3 cameraRight = GetFlattenedDirection(cameraTransform.right);
        
        return (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;
    }
    
    private void ApplyMovement(Vector3 direction)
    {
        Vector3 movement = direction * currentSpeed * Time.deltaTime;
        characterController.Move(movement);
    }
    
    private void ApplyMovementRotation(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < INPUT_THRESHOLD) return;
        transform.rotation = Quaternion.LookRotation(moveDirection);
    }
    
    private void ClearMovementIfStopped()
    {
        if (!HasSignificantSpeed())
        {
            ClearMoveDirection();
        }
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
        if (isSprinting) return sprintSpeed;
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
    
    #region Rotation Processing
    
    private void ProcessIdleRotation()
    {
        if (useTurnInPlaceAnimations)
        {
            ProcessTurnInPlaceRotation();
        }
        else
        {
            ProcessSmoothIdleRotation();
        }
    }
    
    private void ProcessTurnInPlaceRotation()
    {
        Vector3 cameraForward = GetCameraForwardDirection();
        float angleDifference = CalculateSignedAngleDifference(cameraForward);
        
        if (Mathf.Abs(angleDifference) < turnCompleteThreshold)
        {
            CompleteTurn();
        }
        else
        {
            float calculatedTurnAngle = CalculateTurnAngle(angleDifference);
            SetTargetTurnAngle(calculatedTurnAngle);
            
            if (ShouldPerformTurn())
            {
                PerformTurnRotation(cameraForward, angleDifference);
            }
        }
        
        UpdateCurrentTurnAngle();
    }
    
    private void ProcessSmoothIdleRotation()
    {
        Vector3 cameraForward = GetCameraForwardDirection();
        float angleDifference = CalculateUnsignedAngleDifference(cameraForward);
        
        if (ShouldRotateTowardsCamera(angleDifference))
        {
            PerformSmoothRotationTowardsCamera(cameraForward, angleDifference);
        }
        
        SetTargetTurnAngle(0f);
        UpdateCurrentTurnAngle();
    }
    
    private void PerformTurnRotation(Vector3 cameraForward, float angleDifference)
    {
        if (!IsWithinTurnDeadzone(angleDifference))
        {
            if (IsTargetAngle180())
            {
                PerformLockedDirectionTurn();
            }
            else
            {
                PerformLockedDirectionTurn();
            }
        }
        
        MarkAsPerformingTurn();
    }
    
    private void PerformLockedDirectionTurn()
    {
        if (!hasLockedTurnTarget)
        {
            Debug.LogWarning("Trying to perform turn without locked target!");
            return;
        }
        
        // Rotate toward the locked target direction (not the camera)
        Quaternion targetRotation = Quaternion.LookRotation(lockedTurnTargetDirection);
        float rotationSpeed = IsTargetAngle180() ? 
            turnInPlaceRotationSpeed * turn180SpeedMultiplier : 
            turnInPlaceRotationSpeed;
        
        // Clamp rotation to prevent large jumps on low framerates
        rotationSpeed = Mathf.Min(rotationSpeed, maxRotationPerFrame);
        
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
        
        // Check if we've reached the target direction
        float angleToTarget = Vector3.Angle(transform.forward, lockedTurnTargetDirection);
        if (angleToTarget < turnCompleteThreshold)
        {
            // Snap to exact target for precision
            transform.rotation = targetRotation;
            Debug.Log($"Turn reached target direction. Angle to target: {angleToTarget:F2}° - SNAPPING TO TARGET");
        }
    }
    
    private void PerformSmoothRotationTowardsCamera(Vector3 cameraForward, float angleDifference)
    {
        float rotationSpeed = CalculateRotationSpeed(angleDifference);
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
    
    private float CalculateRotationSpeed(float angleDifference)
    {
        float normalizedAngle = Mathf.InverseLerp(cameraFacingThresholdMin, cameraFacingThresholdMax, angleDifference);
        return Mathf.Lerp(minIdleTurnSpeed, maxIdleTurnSpeed, normalizedAngle);
    }
    
    #endregion
    
    #region Angle Calculations
    
    private Vector3 GetCameraForwardDirection()
    {
        Transform cameraTransform = GetCameraTransform();
        return GetFlattenedDirection(cameraTransform.forward);
    }
    
    private float CalculateSignedAngleDifference(Vector3 cameraForward)
    {
        Vector3 currentForward = transform.forward;
        float angle = Vector3.Angle(currentForward, cameraForward);
        
        Vector3 cross = Vector3.Cross(currentForward, cameraForward);
        if (cross.y < 0)
        {
            angle = -angle;
        }
        
        return angle;
    }
    
    private float CalculateUnsignedAngleDifference(Vector3 cameraForward)
    {
        Vector3 currentForward = transform.forward;
        return Vector3.Angle(currentForward, cameraForward);
    }
    
    private float CalculateTurnAngle(float angleDifference)
    {
        float absAngle = Mathf.Abs(angleDifference);
        float currentAbsTurnAngle = Mathf.Abs(targetTurnAngle);
        
        // Use hysteresis: different thresholds for starting vs stopping turns
        float deadZoneThreshold = currentAbsTurnAngle > 0 ? turnStopThreshold : turnStartThreshold;
        float turn180Threshold = currentAbsTurnAngle >= 180f ? turn180StopThreshold : turn180StartThreshold;
        
        // Dead zone: No turn within threshold
        if (absAngle < deadZoneThreshold)
        {
            return 0f;
        }
        // 180° turn zone: Above 180 threshold
        else if (absAngle >= turn180Threshold)
        {
            return angleDifference > 0 ? 180f : -180f;
        }
        // 90° turn zone: Between dead zone and 180 threshold
        else
        {
            return angleDifference > 0 ? 90f : -90f;
        }
    }
    
    #endregion
    
    #region Rotation State Management
    
    private void SetTargetTurnAngle(float angle) 
    {
        // Start rotation tracking when a new turn begins
        if (targetTurnAngle == 0f && angle != 0f)
        {
            StartRotationTracking(angle);
        }
        
        targetTurnAngle = angle;
    }
    
    private void StartRotationTracking(float turnAngle)
    {
        rotationBeforeTurn = transform.eulerAngles;
        totalRotationApplied = 0f;
        trackingRotation = true;
        turnStartTime = Time.time;
        expectedRotationAmount = turnAngle;
        
        // Lock the target direction when turn starts
        CalculateAndLockTurnTarget(turnAngle);
        
        if (turnAngle > 0)
            lastTurnDirection = turnAngle >= 180f ? "Right 180°" : "Right 90°";
        else
            lastTurnDirection = turnAngle <= -180f ? "Left 180°" : "Left 90°";
        
        Debug.Log($"Turn Started: {lastTurnDirection} | Before: {rotationBeforeTurn.y:F1}° | Target: {lockedTurnTargetDirection}");
    }
    
    private void CalculateAndLockTurnTarget(float turnAngle)
    {
        // Calculate the exact direction we should face after the turn
        Vector3 currentForward = transform.forward;
        Quaternion turnRotation = Quaternion.AngleAxis(turnAngle, Vector3.up);
        lockedTurnTargetDirection = turnRotation * currentForward;
        hasLockedTurnTarget = true;
        
        Debug.Log($"Locked Turn Target - Angle: {turnAngle}°, Direction: {lockedTurnTargetDirection}");
    }
    private void MarkAsPerformingTurn() => isPerformingTurnInPlace = true;
    
    private void CompleteTurn()
    {
        // End rotation tracking when turn completes
        if (trackingRotation)
        {
            EndRotationTracking();
        }
        
        targetTurnAngle = 0f;
        isPerformingTurnInPlace = false;
    }
    
    private void EndRotationTracking()
    {
        rotationAfterTurn = transform.eulerAngles;
        float actualRotation = CalculateActualRotationDifference();
        float turnDuration = Time.time - turnStartTime;
        
        trackingRotation = false;
        hasLockedTurnTarget = false; // Clear the locked target
        
        Debug.Log($"Turn Completed: {lastTurnDirection} | After: {rotationAfterTurn.y:F1}° | " +
                 $"Expected: {expectedRotationAmount:F1}° | Actual: {actualRotation:F1}° | " +
                 $"Duration: {turnDuration:F2}s");
    }
    
    private float CalculateActualRotationDifference()
    {
        float beforeY = rotationBeforeTurn.y;
        float afterY = rotationAfterTurn.y;
        
        // Handle 360° wraparound
        float diff = afterY - beforeY;
        
        if (diff > 180f)
            diff -= 360f;
        else if (diff < -180f)
            diff += 360f;
            
        return diff;
    }
    
    private void ResetTurnInPlace()
    {
        if (!isPerformingTurnInPlace) return;
        
        // Allow turn to complete if it's close to finishing (smoother interruption)
        float remainingTurnAngle = Mathf.Abs(currentTurnAngle - targetTurnAngle);
        if (remainingTurnAngle < turnCompletionThreshold)
        {
            // Let the turn finish - don't interrupt it
            return;
        }
        
        // Otherwise, reset the turn normally
        targetTurnAngle = 0f;
        isPerformingTurnInPlace = false;
        hasLockedTurnTarget = false; // Clear locked target when resetting
    }
    
    private void UpdateCurrentTurnAngle()
    {
        currentTurnAngle = Mathf.SmoothDamp(
            currentTurnAngle,
            targetTurnAngle,
            ref turnAngleVelocity,
            turnAngleSmoothTime
        );
        
        ClampToZero(ref currentTurnAngle, ref turnAngleVelocity);
    }
    
    private bool ShouldPerformTurn() => Mathf.Abs(targetTurnAngle) > 0f;
    private bool IsTargetAngle180() => Mathf.Abs(targetTurnAngle) >= 180f;
    private bool IsWithinTurnDeadzone(float angle) => Mathf.Abs(angle) < turnAngleDeadzone;
    private bool ShouldRotateTowardsCamera(float angle) => angle > cameraFacingThresholdMin;
    
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
    
    private Transform GetCameraTransform()
    {
        // Primary: Use Camera.main if available
        if (Camera.main != null) 
            return Camera.main.transform;
        
        // Fallback 1: Find any active camera in scene
        Camera fallbackCamera = FindObjectOfType<Camera>();
        if (fallbackCamera != null)
        {
            Debug.LogWarning("PlayerController: Camera.main not found, using fallback camera: " + fallbackCamera.name);
            return fallbackCamera.transform;
        }
        
        // Fallback 2: Use player transform as last resort
        Debug.LogError("PlayerController: No camera found! Using player transform as camera reference.");
        return transform;
    }
    
    private Vector3 GetFlattenedDirection(Vector3 direction)
    {
        direction.y = 0f;
        return direction.normalized;
    }
    
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
    public float GetCurrentTurnAngle() => currentTurnAngle;
    public bool IsWalking() => isWalking;
    public bool IsSprinting() => isSprinting;
    
    #endregion
    
    #region Public Interface for Debug
    
    public float GetSpeedVelocity() => speedVelocity;
    public Vector3 GetCurrentMoveDirection() => currentMoveDirection;
    public Vector3 GetCalculatedVelocity() => calculatedVelocity;
    public float GetTargetTurnAngle() => targetTurnAngle;
    public bool IsPerformingTurnInPlace() => isPerformingTurnInPlace;
    public PlayerInputActions GetInputActions() => inputActions;
    
    // Debug rotation tracking getters
    public Vector3 GetRotationBeforeTurn() => rotationBeforeTurn;
    public Vector3 GetRotationAfterTurn() => rotationAfterTurn;
    public float GetTotalRotationApplied() => totalRotationApplied;
    public bool IsTrackingRotation() => trackingRotation;
    public string GetLastTurnDirection() => lastTurnDirection;
    public float GetExpectedRotationAmount() => expectedRotationAmount;
    public float GetTurnStartTime() => turnStartTime;
    public Vector3 GetLockedTurnTarget() => lockedTurnTargetDirection;
    public bool HasLockedTurnTarget() => hasLockedTurnTarget;
    public Vector2 GetLookInput() => inputActions.LookValue;

    #endregion
}