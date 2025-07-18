using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Player Movement Settings

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    
    [Header("Acceleration Settings")]
    [SerializeField] private float accelerationTime = 0.25f;
    [SerializeField] private float decelerationTime = 0.15f;
    [SerializeField] private float sprintAccelerationTime = 0.4f;
    [SerializeField] private float sprintDecelerationTime = 0.3f;
    
    [Header("Input Smoothing")]
    [SerializeField] private float inputSmoothTime = 0.1f;
    [SerializeField] private float deadZone = 0.1f;
    
    [Header("Turn Enhancement")]
    [SerializeField] private float turnSpeedMultiplier = 1.5f;
    [SerializeField] private float stationaryTurnSpeed = 180f;
    [SerializeField] private AnimationCurve turnSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Physics")]
    [SerializeField] private float gravity = -9.81f;
    
    private CharacterController currentController;
    private GameObject currentCharacter;
    private Transform cameraTransform;
    private float turnSmoothVelocity;
    private PlayerInputActions inputActions;
    
    private Vector2 moveInput;
    private Vector2 smoothedMoveInput;
    private Vector2 inputVelocity;
    private bool isSprinting;
    private bool isWalking = false;
    
    private float currentSpeed;
    private float speedVelocity;
    private float verticalVelocity;

    #endregion
    
    void Awake()
    {
        inputActions = GetComponent<PlayerInputActions>();
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
    }
    
    void Start()
    {
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        
        // Subscribe to input events
        if (inputActions != null)
        {
            inputActions.OnSprintStart += () => isSprinting = true;
            inputActions.OnSprintEnd += () => isSprinting = false;
            inputActions.OnCrouchToggle += () => isWalking = !isWalking;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inputActions != null)
        {
            inputActions.OnSprintStart -= () => isSprinting = true;
            inputActions.OnSprintEnd -= () => isSprinting = false;
            inputActions.OnCrouchToggle -= () => isWalking = !isWalking;
        }
    }
    
    public void SetCurrentCharacter(GameObject character)
    {
        currentCharacter = character;
        currentController = character.GetComponent<CharacterController>();
        
        if (currentController == null)
        {
            Debug.LogError($"Character {character.name} is missing CharacterController!");
        }
    }
    
    void UpdateInput()
    {
        // Get move input from PlayerInputActions
        Vector2 input = inputActions.MoveValue;
        
        if (input.magnitude < deadZone)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = input.normalized * ((input.magnitude - deadZone) / (1 - deadZone));
        }
    }
    
    void Update()
    {
        if (currentController == null || currentCharacter == null) return;
        
        UpdateInput();
        UpdateInputSmoothing();
        HandleMovement();
        
        verticalVelocity += gravity * Time.deltaTime;
        
        if (currentController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
    }
    
    void UpdateInputSmoothing()
    {
        // Instant response when starting from idle
        if (smoothedMoveInput.magnitude < 0.1f && moveInput.magnitude > deadZone)
        {
            smoothedMoveInput = moveInput;
            inputVelocity = Vector2.zero;
        }
        else
        {
            smoothedMoveInput = Vector2.SmoothDamp(smoothedMoveInput, moveInput, ref inputVelocity, inputSmoothTime);
        }
        
        if (smoothedMoveInput.magnitude < deadZone)
        {
            smoothedMoveInput = Vector2.zero;
            inputVelocity = Vector2.zero;
        }
    }
    
    void HandleMovement()
    {
        Vector3 direction = new Vector3(smoothedMoveInput.x, 0f, smoothedMoveInput.y).normalized;

        if (direction.magnitude >= 0.1f && cameraTransform != null)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);

            bool allowRotation = isSprinting || smoothedMoveInput.y > 0.1f;

            float turnSpeed = turnSmoothTime;
            if (currentSpeed < 0.1f)
            {
                turnSpeed = stationaryTurnSpeed * Time.deltaTime;

                if (allowRotation)
                {
                    transform.rotation = Quaternion.Euler(
                        0f,
                        transform.eulerAngles.y + Mathf.Sign(angleDifference) * Mathf.Min(Mathf.Abs(angleDifference), turnSpeed),
                        0f
                    );
                }
            }
            else
            {
                if (allowRotation)
                {
                    float speedFactor = turnSpeedCurve.Evaluate(currentSpeed / runSpeed);
                    turnSpeed = turnSmoothTime / (speedFactor * turnSpeedMultiplier);
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSpeed);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
            }

            // Calculate target speed
            float targetSpeed;
            if (isSprinting && !isWalking)
            {
                targetSpeed = runSpeed;
            }
            else if (isWalking)
            {
                targetSpeed = walkSpeed;
            }
            else
            {
                targetSpeed = runSpeed;
            }

            // Instant start from idle, smooth acceleration otherwise
            if (currentSpeed < 0.1f && smoothedMoveInput.magnitude > deadZone)
            {
                currentSpeed = walkSpeed * smoothedMoveInput.magnitude;
                speedVelocity = 0f;
            }
            else
            {
                float accelTime = isSprinting ? sprintAccelerationTime : accelerationTime;
                currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed * smoothedMoveInput.magnitude, ref speedVelocity, accelTime);
            }

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 movement = moveDir.normalized * currentSpeed * Time.deltaTime;
            movement.y = verticalVelocity * Time.deltaTime;

            Vector3 childPosBefore = currentCharacter.transform.position;
            currentController.Move(movement);
            Vector3 childPosAfter = currentCharacter.transform.position;
            Vector3 deltaMovement = childPosAfter - childPosBefore;
            transform.position += deltaMovement;

            currentCharacter.transform.localPosition = Vector3.zero;
            currentCharacter.transform.rotation = transform.rotation;
        }
        else
        {
            float decelTime = isSprinting ? sprintDecelerationTime : decelerationTime;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref speedVelocity, decelTime);

            Vector3 movement = Vector3.zero;
            if (currentSpeed > 0.1f)
            {
                movement = transform.forward * currentSpeed * Time.deltaTime;
            }
            movement.y = verticalVelocity * Time.deltaTime;

            Vector3 childPosBefore = currentCharacter.transform.position;
            currentController.Move(movement);
            Vector3 childPosAfter = currentCharacter.transform.position;
            Vector3 deltaMovement = childPosAfter - childPosBefore;
            transform.position += deltaMovement;

            currentCharacter.transform.localPosition = Vector3.zero;
            currentCharacter.transform.rotation = transform.rotation;
        }
    }

    
    
    public float GetCurrentSpeed() => currentSpeed;
    public float GetWalkSpeed() => walkSpeed;
    public float GetRunSpeed() => runSpeed;
    public bool IsSprinting() => isSprinting;
    public bool IsWalking() => isWalking;
    public bool IsMoving() => currentSpeed > 0.1f;
    public bool IsGrounded() => currentController != null && currentController.isGrounded;
    public float GetVerticalVelocity() => verticalVelocity;
    public Vector2 GetMovementVector() => smoothedMoveInput;
    public float GetNormalizedSpeed() => currentSpeed / walkSpeed;
}