using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int TurnAngle = Animator.StringToHash("TurnAngle");
    
    private PlayerController playerController;
    private Animator animator;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController component missing!");
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator component missing in children!");
        }
    }
    
    private void Update()
    {
        if (playerController == null || animator == null) return;
        
        UpdateAnimation();
    }
    
    private void UpdateAnimation()
    {
        float currentSpeed = playerController.GetCurrentSpeed();
        float horizontal = playerController.GetCurrentHorizontal();
        float vertical = playerController.GetCurrentVertical();
        float turnAngle = playerController.GetCurrentTurnAngle();

        animator.SetFloat(MovementSpeed, currentSpeed);
        animator.SetFloat(Horizontal, horizontal);
        animator.SetFloat(Vertical, vertical);
        animator.SetFloat(TurnAngle, turnAngle);
    }
}