using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDeadzone = 0.1f;
    
    private PlayerController playerController;
    private EnhancedPlayerController enhancedController;
    private Animator currentAnimator;
    private GameObject currentCharacter;
    
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        enhancedController = GetComponent<EnhancedPlayerController>();
        
        if (playerController == null && enhancedController == null)
        {
            Debug.LogError($"No PlayerController or EnhancedPlayerController found on {gameObject.name}");
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
        // Use enhanced controller if available, otherwise fall back to regular controller
        Vector2 movementVector;
        bool isGrounded;
        float verticalVel;
        bool isSprinting;
        bool isWalking;
        float currentSpeed;
        float walkSpeed;
        float runSpeed;
        
        if (enhancedController != null)
        {
            movementVector = enhancedController.GetMovementVector();
            isGrounded = enhancedController.IsGrounded();
            verticalVel = enhancedController.GetVerticalVelocity();
            isSprinting = enhancedController.IsSprinting();
            isWalking = enhancedController.IsWalking();
            currentSpeed = enhancedController.GetCurrentSpeed();
            walkSpeed = 5f; // Default values - could be exposed as properties
            runSpeed = 10f;
        }
        else if (playerController != null)
        {
            movementVector = playerController.GetMovementVector();
            isGrounded = playerController.IsGrounded();
            verticalVel = playerController.GetVerticalVelocity();
            isSprinting = playerController.IsSprinting();
            isWalking = playerController.IsWalking();
            currentSpeed = playerController.GetCurrentSpeed();
            walkSpeed = playerController.GetWalkSpeed();
            runSpeed = playerController.GetRunSpeed();
        }
        else
        {
            return; // No controller found
        }
        
        // Set discrete state values for MovementSpeed
        // 0 = idle, 0.5 = walk, 1 = run, 1.5 = sprint
        float movementSpeed = 0f;
        
        if (currentSpeed > 0.1f)
        {
            if (isWalking)
            {
                movementSpeed = 0.5f;  // Walk state
            }
            else if (isSprinting)
            {
                movementSpeed = 1.5f;  // Sprint state
            }
            else
            {
                movementSpeed = 1f;    // Run state
            }
        }
        // else movementSpeed stays 0 for idle
        
        // Apply deadzone to movement vector
        float horizontal = movementVector.x;
        float vertical = movementVector.y;
        
        if (Mathf.Abs(horizontal) < animationDeadzone)
        {
            horizontal = 0f;
        }
        
        if (Mathf.Abs(vertical) < animationDeadzone)
        {
            vertical = 0f;
        }
        
        // Set animator parameters directly without smoothing
        currentAnimator.SetFloat("MovementSpeed", movementSpeed);
        currentAnimator.SetFloat("Horizontal", horizontal);
        currentAnimator.SetFloat("Vertical", vertical);
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
    public float GetAnimationSpeed()
    {
        if (currentAnimator != null)
        {
            return currentAnimator.GetFloat("MovementSpeed");
        }
        return 0f;
    }
    
    // Handle movement state changes from EnhancedPlayerController
    public void OnMovementStateChanged(EnhancedPlayerController.MovementState newState)
    {
        if (currentAnimator == null) return;
        
        // Trigger state-specific animations
        switch (newState)
        {
            case EnhancedPlayerController.MovementState.Starting:
                currentAnimator.SetTrigger("StartMoving");
                break;
            case EnhancedPlayerController.MovementState.Stopping:
                currentAnimator.SetTrigger("Stopping");
                break;
            case EnhancedPlayerController.MovementState.TurningInPlace:
                currentAnimator.SetBool("TurningInPlace", true);
                break;
            default:
                currentAnimator.SetBool("TurningInPlace", false);
                break;
        }
        
        // Set current state for blend tree
        currentAnimator.SetInteger("MovementState", (int)newState);
    }
}