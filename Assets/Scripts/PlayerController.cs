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
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = false;
    
    private const float ZERO_THRESHOLD = 0.001f;
    private const float INPUT_THRESHOLD = 0.1f;
    private const float GIZMO_HEIGHT_OFFSET = 1f;
    private const float GIZMO_LINE_LENGTH = 3f;
    private const float ARROW_SIZE = 0.5f;
    private const float COORDINATE_AXIS_LENGTH = 2f;
    
    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity;
    private bool isWalking = false;
    private bool isSprinting = false;
    
    private float currentHorizontal = 0f;
    private float currentVertical = 0f;
    private float horizontalVelocity = 0f;
    private float verticalVelocity = 0f;
    
    private Vector3 currentMoveDirection = Vector3.zero;
    private Vector3 calculatedVelocity = Vector3.zero;
    private Vector3 previousPosition = Vector3.zero;
    
    private float currentTurnAngle = 0f;
    private float targetTurnAngle = 0f;
    private float turnAngleVelocity = 0f;
    
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
        HandleMovement();
        CalculateActualVelocity();
        UpdatePreviousPosition();
    }
    
    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = GetComponent<PlayerInputActions>();
        
        if (characterController == null)
        {
            Debug.LogError("CharacterController component missing!");
        }
        
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
    }
    
    private void SubscribeToInputEvents()
    {
        inputActions.OnSprintStart += SetSprintingTrue;
        inputActions.OnSprintEnd += SetSprintingFalse;
        inputActions.OnCrouchToggle += ToggleWalking;
    }
    
    private void UnsubscribeFromInputEvents()
    {
        if (inputActions != null)
        {
            inputActions.OnSprintStart -= SetSprintingTrue;
            inputActions.OnSprintEnd -= SetSprintingFalse;
            inputActions.OnCrouchToggle -= ToggleWalking;
        }
    }
    
    private void SetSprintingTrue()
    {
        isSprinting = true;
    }
    
    private void SetSprintingFalse()
    {
        isSprinting = false;
    }
    
    private void ToggleWalking()
    {
        isWalking = !isWalking;
    }
    
    private void InitializePreviousPosition()
    {
        previousPosition = transform.position;
    }
    
    private void CalculateActualVelocity()
    {
        calculatedVelocity = (transform.position - previousPosition) / Time.deltaTime;
    }
    
    private void UpdatePreviousPosition()
    {
        previousPosition = transform.position;
    }
    
    private void HandleMovement()
    {
        Vector2 input = GetInput();
        SmoothInputValues(input);
        CalculateTargetSpeed(input);
        SmoothCurrentSpeed();
        ApplyMovement(input);
    }
    
    private Vector2 GetInput()
    {
        return inputActions.MoveValue;
    }
    
    private void SmoothInputValues(Vector2 input)
    {
        currentHorizontal = SmoothValue(currentHorizontal, input.x, ref horizontalVelocity);
        currentVertical = SmoothValue(currentVertical, input.y, ref verticalVelocity);
        ClampInputValuesToZero();
    }
    
    private float SmoothValue(float current, float target, ref float velocity)
    {
        return Mathf.SmoothDamp(current, target, ref velocity, inputSmoothTime);
    }
    
    private void ClampInputValuesToZero()
    {
        ClampToZero(ref currentHorizontal, ref horizontalVelocity);
        ClampToZero(ref currentVertical, ref verticalVelocity);
    }
    
    private void CalculateTargetSpeed(Vector2 input)
    {
        if (HasInput(input))
        {
            targetSpeed = GetSpeedForCurrentState();
        }
        else
        {
            targetSpeed = 0f;
        }
    }
    
    private bool HasInput(Vector2 input)
    {
        return input.magnitude > INPUT_THRESHOLD;
    }
    
    private float GetSpeedForCurrentState()
    {
        if (isWalking)
        {
            return walkSpeed;
        }
        else if (isSprinting)
        {
            return sprintSpeed;
        }
        else
        {
            return runSpeed;
        }
    }
    
    private void SmoothCurrentSpeed()
    {
        float smoothTime = GetSpeedSmoothTime();
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);
        ClampToZero(ref currentSpeed, ref speedVelocity);
    }
    
    private void ClampToZero(ref float value, ref float velocity)
    {
        if (IsValueVerySmall(value))
        {
            value = 0f;
            velocity = 0f;
        }
    }
    
    private bool IsValueVerySmall(float value)
    {
        return Mathf.Abs(value) < ZERO_THRESHOLD;
    }
    
    private float GetSpeedSmoothTime()
    {
        return targetSpeed > currentSpeed ? accelerationTime : decelerationTime;
    }
    
    private void ApplyMovement(Vector2 input)
    {
        if (ShouldMove(input))
        {
            ProcessMovement(input);
        }
        else
        {
            HandleIdleRotation();
            if (ShouldClearMovementDirection())
            {
                ClearMovementDirection();
            }
        }
    }
    
    private bool ShouldMove(Vector2 input)
    {
        return HasInput(input) && currentSpeed > INPUT_THRESHOLD;
    }
    
    private bool ShouldClearMovementDirection()
    {
        return currentSpeed <= INPUT_THRESHOLD;
    }
    
    private void ClearMovementDirection()
    {
        currentMoveDirection = Vector3.zero;
    }
    
    private void ProcessMovement(Vector2 input)
    {
        Vector3 direction = CalculateInputDirection(input);
        Vector3 moveDirection = CalculateCameraRelativeDirection(direction);
        UpdateMoveDirection(moveDirection);
        RotatePlayerToMovementDirection(moveDirection);
        MoveCharacter(moveDirection);
    }
    
    private void RotatePlayerToMovementDirection(Vector3 moveDirection)
    {
        if (moveDirection.magnitude > INPUT_THRESHOLD)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
    
    private void HandleIdleRotation()
    {
        if (useTurnInPlaceAnimations)
        {
            HandleTurnInPlaceAnimation();
        }
        else
        {
            HandleSmoothIdleRotation();
        }
    }
    
    private void HandleTurnInPlaceAnimation()
    {
        Vector3 cameraForward = GetCameraForwardDirection();
        float angleDifference = CalculateSignedAngleDifference(cameraForward);
        
        targetTurnAngle = CalculateTurnAngleThreshold(angleDifference);
        SmoothTurnAngle();
    }
    
    private void HandleSmoothIdleRotation()
    {
        Vector3 cameraForward = GetCameraForwardDirection();
        float angleDifference = CalculateAngleDifference(cameraForward);
        
        if (ShouldRotateTowardsCamera(angleDifference))
        {
            RotateTowardsCamera(cameraForward, angleDifference);
        }
        
        targetTurnAngle = 0f;
        SmoothTurnAngle();
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
    
    private float CalculateTurnAngleThreshold(float angleDifference)
    {
        if (angleDifference >= -45f && angleDifference <= 45f)
        {
            return 0f;
        }
        else if (angleDifference > 45f && angleDifference <= 135f)
        {
            return 90f;
        }
        else if (angleDifference > 135f)
        {
            return 180f;
        }
        else if (angleDifference < -45f && angleDifference >= -135f)
        {
            return -90f;
        }
        else
        {
            return -180f;
        }
    }
    
    private void SmoothTurnAngle()
    {
        currentTurnAngle = Mathf.SmoothDamp(
            currentTurnAngle, 
            targetTurnAngle, 
            ref turnAngleVelocity, 
            turnAngleSmoothTime
        );
        
        ClampToZero(ref currentTurnAngle, ref turnAngleVelocity);
    }
    
    private Vector3 GetCameraForwardDirection()
    {
        Transform cameraTransform = GetCameraTransform();
        return GetFlattenedDirection(cameraTransform.forward);
    }
    
    private float CalculateAngleDifference(Vector3 cameraForward)
    {
        Vector3 currentForward = transform.forward;
        return Vector3.Angle(currentForward, cameraForward);
    }
    
    private bool ShouldRotateTowardsCamera(float angleDifference)
    {
        return angleDifference > cameraFacingThresholdMin;
    }
    
    private void RotateTowardsCamera(Vector3 cameraForward, float angleDifference)
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
        float normalizedAngle = Mathf.InverseLerp(
            cameraFacingThresholdMin, 
            cameraFacingThresholdMax, 
            angleDifference
        );
        
        return Mathf.Lerp(minIdleTurnSpeed, maxIdleTurnSpeed, normalizedAngle);
    }
    
    private Vector3 CalculateInputDirection(Vector2 input)
    {
        return new Vector3(input.x, 0f, input.y).normalized;
    }
    
    private Vector3 CalculateCameraRelativeDirection(Vector3 direction)
    {
        Transform cameraTransform = GetCameraTransform();
        Vector3 cameraForward = GetFlattenedDirection(cameraTransform.forward);
        Vector3 cameraRight = GetFlattenedDirection(cameraTransform.right);
        
        return (cameraForward * direction.z + cameraRight * direction.x).normalized;
    }
    
    private Transform GetCameraTransform()
    {
        return Camera.main.transform;
    }
    
    private Vector3 GetFlattenedDirection(Vector3 direction)
    {
        direction.y = 0f;
        return direction.normalized;
    }
    
    private void UpdateMoveDirection(Vector3 moveDirection)
    {
        currentMoveDirection = moveDirection;
    }
    
    private void MoveCharacter(Vector3 moveDirection)
    {
        Vector3 movement = CalculateMovement(moveDirection);
        characterController.Move(movement);
    }
    
    private Vector3 CalculateMovement(Vector3 moveDirection)
    {
        return moveDirection * currentSpeed * Time.deltaTime;
    }
    
    public float GetCurrentSpeed() => currentSpeed;
    public float GetTargetSpeed() => targetSpeed;
    public float GetCurrentHorizontal() => currentHorizontal;
    public float GetCurrentVertical() => currentVertical;
    public float GetCurrentTurnAngle() => currentTurnAngle;
    public bool IsWalking() => isWalking;
    public bool IsSprinting() => isSprinting;
    
    private void OnGUI()
    {
        if (!showDebugGUI) return;
        
        DrawDebugWindow();
    }
    
    private void DrawDebugWindow()
    {
        GUI.Box(new Rect(10, 10, 300, 260), "Player Controller Debug");
        
        GUILayout.BeginArea(new Rect(20, 30, 280, 230));
        
        DrawMovementSpeeds();
        DrawCurrentState();
        DrawMovementMode();
        DrawInputInfo();
        DrawMovementDirection();
        DrawInstructions();
        
        GUILayout.EndArea();
    }
    
    private void DrawMovementSpeeds()
    {
        GUIStyle headerStyle = CreateHeaderStyle();
        GUIStyle labelStyle = CreateLabelStyle();
        
        GUILayout.Label("Movement Speeds:", headerStyle);
        GUILayout.Label($"Walk Speed: {walkSpeed}", labelStyle);
        GUILayout.Label($"Run Speed: {runSpeed}", labelStyle);
        GUILayout.Label($"Sprint Speed: {sprintSpeed}", labelStyle);
        GUILayout.Space(5);
    }
    
    private void DrawCurrentState()
    {
        GUIStyle headerStyle = CreateHeaderStyle();
        GUIStyle labelStyle = CreateLabelStyle();
        
        GUILayout.Label("Current State:", headerStyle);
        GUILayout.Label($"Current Speed: {currentSpeed}", labelStyle);
        GUILayout.Label($"Target Speed: {targetSpeed}", labelStyle);
        GUILayout.Label($"Speed Velocity: {speedVelocity}", labelStyle);
        GUILayout.Label($"Is Walking: {isWalking}", labelStyle);
        GUILayout.Label($"Is Sprinting: {isSprinting}", labelStyle);
    }
    
    private void DrawMovementMode()
    {
        GUIStyle labelStyle = CreateLabelStyle();
        string movementMode = GetMovementModeString();
        GUILayout.Label($"Movement Mode: {movementMode}", labelStyle);
        GUILayout.Space(5);
    }
    
    private string GetMovementModeString()
    {
        if (currentSpeed <= INPUT_THRESHOLD) return "Idle";
        if (isWalking) return "Walking";
        if (isSprinting) return "Sprinting";
        return "Running";
    }
    
    private void DrawInputInfo()
    {
        if (inputActions == null) return;
        
        GUIStyle headerStyle = CreateHeaderStyle();
        GUIStyle labelStyle = CreateLabelStyle();
        
        Vector2 moveInput = inputActions.MoveValue;
        GUILayout.Label("Input:", headerStyle);
        GUILayout.Label($"Raw Input: ({moveInput.x}, {moveInput.y})", labelStyle);
        GUILayout.Label($"Input Magnitude: {moveInput.magnitude}", labelStyle);
        GUILayout.Label($"Smoothed Horizontal: {currentHorizontal}", labelStyle);
        GUILayout.Label($"Smoothed Vertical: {currentVertical}", labelStyle);
        GUILayout.Space(5);
    }
    
    private void DrawMovementDirection()
    {
        GUIStyle headerStyle = CreateHeaderStyle();
        GUIStyle labelStyle = CreateLabelStyle();
        
        GUILayout.Label("Movement Direction:", headerStyle);
        GUILayout.Label($"X: {currentMoveDirection.x}", labelStyle);
        GUILayout.Label($"Y: {currentMoveDirection.y}", labelStyle);
        GUILayout.Label($"Z: {currentMoveDirection.z}", labelStyle);
        GUILayout.Space(5);
        
        GUILayout.Label("Actual Velocity:", headerStyle);
        GUILayout.Label($"X: {calculatedVelocity.x}", labelStyle);
        GUILayout.Label($"Y: {calculatedVelocity.y}", labelStyle);
        GUILayout.Label($"Z: {calculatedVelocity.z}", labelStyle);
        GUILayout.Label($"Magnitude: {calculatedVelocity.magnitude}", labelStyle);
        GUILayout.Space(5);
    }
    
    private void DrawInstructions()
    {
        GUIStyle labelStyle = CreateLabelStyle();
        GUILayout.Label("Toggle in Inspector to show/hide", labelStyle);
    }
    
    private GUIStyle CreateLabelStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = Color.white }
        };
    }
    
    private GUIStyle CreateHeaderStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow }
        };
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGUI) return;
        
        DrawInputDirectionGizmo();
        DrawActualVelocityGizmo();
        DrawSpeedIndicatorGizmo();
    }
    
    private void DrawInputDirectionGizmo()
    {
        if (currentMoveDirection == Vector3.zero) return;
        
        Gizmos.color = Color.yellow;
        
        Vector3 startPos = GetGizmoStartPosition();
        Vector3 endPos = startPos + currentMoveDirection * 3f;
        
        Gizmos.DrawLine(startPos, endPos);
        DrawArrowHead(endPos, currentMoveDirection);
    }
    
    private void DrawActualVelocityGizmo()
    {
        if (calculatedVelocity == Vector3.zero) return;
        
        Gizmos.color = Color.cyan;
        
        Vector3 startPos = GetGizmoStartPosition();
        Vector3 normalizedVelocity = calculatedVelocity.normalized;
        Vector3 endPos = startPos + normalizedVelocity * GIZMO_LINE_LENGTH;
        
        Gizmos.DrawLine(startPos, endPos);
        DrawArrowHead(endPos, normalizedVelocity);
    }
    
    private Vector3 GetGizmoStartPosition()
    {
        return transform.position + Vector3.up * GIZMO_HEIGHT_OFFSET;
    }
    
    private Vector3 GetGizmoEndPosition(Vector3 startPos)
    {
        return startPos + currentMoveDirection * GIZMO_LINE_LENGTH;
    }
    
    private void DrawArrowHead(Vector3 endPos, Vector3 direction)
    {
        Vector3 arrowRight = CalculateArrowDirection(direction, 160f);
        Vector3 arrowLeft = CalculateArrowDirection(direction, -160f);
        
        Gizmos.DrawLine(endPos, endPos + arrowRight * ARROW_SIZE);
        Gizmos.DrawLine(endPos, endPos + arrowLeft * ARROW_SIZE);
    }
    
    private Vector3 CalculateArrowDirection(Vector3 direction, float angle)
    {
        return Quaternion.LookRotation(direction) * Quaternion.Euler(0, angle, 0) * Vector3.forward;
    }
    
    private void DrawSpeedIndicatorGizmo()
    {
        Gizmos.color = Color.white;
        Vector3 position = transform.position + Vector3.up * 0.1f;
        float radius = currentSpeed * 0.2f;
        Gizmos.DrawWireSphere(position, radius);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGUI) return;
        
        DrawCoordinateSystem();
    }
    
    private void DrawCoordinateSystem()
    {
        Vector3 pos = GetCoordinateSystemPosition();
        
        DrawXAxis(pos);
        DrawYAxis(pos);
        DrawZAxis(pos);
    }
    
    private Vector3 GetCoordinateSystemPosition()
    {
        return transform.position + Vector3.up * COORDINATE_AXIS_LENGTH;
    }
    
    private void DrawXAxis(Vector3 pos)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * COORDINATE_AXIS_LENGTH);
    }
    
    private void DrawYAxis(Vector3 pos)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + Vector3.up * COORDINATE_AXIS_LENGTH);
    }
    
    private void DrawZAxis(Vector3 pos)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + Vector3.forward * COORDINATE_AXIS_LENGTH);
    }
}