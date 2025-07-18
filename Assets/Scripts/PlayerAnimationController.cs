using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDeadzone = 0.1f;
    [SerializeField] private float movementSpeedTransitionRate = 2f;
    
    private PlayerController playerController;
    private Animator currentAnimator;
    private GameObject currentCharacter;
    private float currentMovementSpeed = 0f;
    
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        if (playerController == null)
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
        
        if (playerController != null)
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
        float targetMovementSpeed = 0f;
        
        if (currentSpeed > 0.1f)
        {
            if (isWalking)
            {
                targetMovementSpeed = 0.5f;  // Walk state
            }
            else if (isSprinting)
            {
                targetMovementSpeed = 1.5f;  // Sprint state
            }
            else
            {
                targetMovementSpeed = 1f;    // Run state
            }
        }
        // else targetMovementSpeed stays 0 for idle
        
        // Smoothly transition to the target movement speed
        currentMovementSpeed = Mathf.MoveTowards(currentMovementSpeed, targetMovementSpeed, movementSpeedTransitionRate * Time.deltaTime);
        
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
        
        // Set animator parameters with smoothed movement speed
        currentAnimator.SetFloat("MovementSpeed", currentMovementSpeed);
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
}