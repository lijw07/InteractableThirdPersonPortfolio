using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    
    private PlayerController playerController;
    private CharacterManageController characterManager;
    private Animator animator;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterManager = GetComponent<CharacterManageController>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController component missing!");
        }
        
        if (characterManager == null)
        {
            Debug.LogError("CharacterManageController component missing!");
        }
    }
    
    private void Start()
    {
        UpdateAnimatorReference();
    }
    
    private void UpdateAnimatorReference()
    {
        if (characterManager != null)
        {
            animator = characterManager.GetCurrentAnimator();
            if (animator == null)
            {
                Debug.LogError("No Animator found on active character!");
            }
        }
    }
    
    private void Update()
    {
        if (!playerController || !animator) return;
        
        UpdateAnimation();
    }
    
    private void UpdateAnimation()
    {
        if (animator == null)
        {
            UpdateAnimatorReference();
            if (animator == null) return;
        }
        
        float currentSpeed = playerController.GetCurrentSpeed();
        float horizontal = playerController.GetCurrentHorizontal();
        float vertical = playerController.GetCurrentVertical();
        bool isGrounded = playerController.IsGrounded();
        bool hasLandedOnce = playerController.HasLandedOnce();

        animator.SetFloat(MovementSpeed, currentSpeed);
        animator.SetFloat(Horizontal, horizontal);
        animator.SetFloat(Vertical, vertical);

        animator.SetBool(IsGrounded, isGrounded);
        
        if (hasLandedOnce && !animator.GetBool("HasLandedOnce"))
        {
            animator.SetBool("HasLandedOnce", true);
        }
    }
    
    public void OnCharacterSwitched()
    {
        UpdateAnimatorReference();
    }
}