using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float directionSmoothTime = 0.2f;
    [SerializeField] private float animationDeadzone = 0.1f;
    
    private PlayerController playerController;
    private Animator currentAnimator;
    private GameObject currentCharacter;
    
    private float smoothedSpeed = 0f;
    private float speedSmoothVelocity = 0f;
    private float smoothedHorizontal = 0f;
    private float smoothedVertical = 0f;
    private float horizontalVelocity = 0f;
    private float verticalVelocity = 0f;
    
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError($"No PlayerController found on {gameObject.name}");
        }
    }
    
    public void SetCurrentCharacter(GameObject character)
    {
        currentCharacter = character;
        currentAnimator = character.GetComponentInChildren<Animator>();
        
        if (currentAnimator == null)
        {
            Debug.LogWarning($"No Animator found on character {character.name} or its children");
        }
    }
    
    void Update()
    {
        if (currentAnimator == null || playerController == null) return;
        
        UpdateAnimationParameters();
    }
    
    
    void UpdateAnimationParameters()
    {
        Vector2 movementVector = playerController.GetMovementVector();
        bool isGrounded = playerController.IsGrounded();
        float verticalVel = playerController.GetVerticalVelocity();
        bool isSprinting = playerController.IsSprinting();
        bool isWalking = playerController.IsWalking();
        float currentSpeed = playerController.GetCurrentSpeed();
        float walkSpeed = playerController.GetWalkSpeed();
        float runSpeed = playerController.GetRunSpeed();
        
        // Calculate normalized animation speed based on actual movement speed
        float normalizedSpeed = 0f;
        
        if (currentSpeed > 0.1f)
        {
            if (isWalking)
            {
                // Map current speed relative to walk speed: 0.5 = full walk speed
                normalizedSpeed = Mathf.Clamp(currentSpeed / walkSpeed, 0f, 1f) * 0.5f;
            }
            else if (isSprinting)
            {
                // Map current speed relative to run speed: 1.5-2.0 = sprint range
                float sprintRatio = Mathf.Clamp(currentSpeed / runSpeed, 0f, 1f);
                normalizedSpeed = 1.5f + (sprintRatio * 0.5f);
            }
            else
            {
                // Map current speed relative to run speed: 0.5-1.5 = run range
                float runRatio = Mathf.Clamp(currentSpeed / runSpeed, 0f, 1f);
                normalizedSpeed = 0.5f + runRatio;
            }
        }
        
        // Handle instant start from PlayerController
        bool isInstantStart = currentSpeed > 0.1f && smoothedSpeed < 0.1f;
        
        if (isInstantStart)
        {
            // Match PlayerController's instant start behavior
            smoothedSpeed = normalizedSpeed;
            speedSmoothVelocity = 0f;
        }
        else
        {
            // Use smooth transitions for normal speed changes
            float smoothTime = 0.15f; // Slightly faster than controller acceleration
            smoothedSpeed = Mathf.SmoothDamp(smoothedSpeed, normalizedSpeed, ref speedSmoothVelocity, smoothTime);
        }
        
        smoothedHorizontal = Mathf.SmoothDamp(smoothedHorizontal, movementVector.x, ref horizontalVelocity, directionSmoothTime);
        smoothedVertical = Mathf.SmoothDamp(smoothedVertical, movementVector.y, ref verticalVelocity, directionSmoothTime);
        
        if (Mathf.Abs(smoothedHorizontal) < animationDeadzone)
        {
            smoothedHorizontal = 0f;
            horizontalVelocity = 0f;
        }
        
        if (Mathf.Abs(smoothedVertical) < animationDeadzone)
        {
            smoothedVertical = 0f;
            verticalVelocity = 0f;
        }
        
        // Snap to exact state values when very close to prevent flickering
        if (Mathf.Abs(smoothedSpeed - 0f) < 0.1f)
            smoothedSpeed = 0f;
        else if (Mathf.Abs(smoothedSpeed - 0.5f) < 0.1f)
            smoothedSpeed = 0.5f;
        else if (Mathf.Abs(smoothedSpeed - 1f) < 0.1f)
            smoothedSpeed = 1f;
        else if (Mathf.Abs(smoothedSpeed - 2f) < 0.1f)
            smoothedSpeed = 2f;
        
        currentAnimator.SetFloat("MovementSpeed", smoothedSpeed);
        currentAnimator.SetFloat("Horizontal", smoothedHorizontal);
        currentAnimator.SetFloat("Vertical", smoothedVertical);
        currentAnimator.SetBool("IsGrounded", isGrounded);
        currentAnimator.SetFloat("VerticalVelocity", verticalVel);
    }
    
    public void TriggerJump()
    {
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("Jump");
        }
    }
    
    // Debug method to check animation sync
    public float GetAnimationSpeed() => smoothedSpeed;
}