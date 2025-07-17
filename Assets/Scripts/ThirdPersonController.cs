using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(CombatController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -19.62f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask = -1;
    
    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 velocity;
    private bool isGrounded;
    private float turnSmoothVelocity;
    private Animator animator;
    private PlayerInputActions inputActions;
    
    private Vector2 moveInput;
    private bool isJumping;
    private bool isSprinting;
    
    private CombatController combatController;
    private WeaponController weaponController;
    
    void Awake()
    {
        inputActions = GetComponentInParent<PlayerInputActions>();
        if (inputActions == null)
        {
            inputActions = GetComponent<PlayerInputActions>();
        }
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
    }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        combatController = GetComponent<CombatController>();
        weaponController = GetComponent<WeaponController>();
        
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -0.95f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        inputActions.MoveAction.performed += OnMove;
        inputActions.MoveAction.canceled += OnMove;
        inputActions.JumpAction.performed += OnJump;
        inputActions.SprintAction.performed += OnSprint;
        inputActions.SprintAction.canceled += OnSprint;
    }
    
    void OnDestroy()
    {
        inputActions.MoveAction.performed -= OnMove;
        inputActions.MoveAction.canceled -= OnMove;
        inputActions.JumpAction.performed -= OnJump;
        inputActions.SprintAction.performed -= OnSprint;
        inputActions.SprintAction.canceled -= OnSprint;
    }
    
    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isJumping = true;
        }
    }
    
    void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }
    
    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }
    
    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }
    
    void HandleMovement()
    {
        if (combatController != null && combatController.IsAttacking())
        {
            return;
        }
        
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        if (direction.magnitude >= 0.1f && cameraTransform != null)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isSprinting ? runSpeed : walkSpeed;
            
            if (combatController != null && combatController.IsBlocking())
            {
                currentSpeed *= 0.5f;
            }
            
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            
            if (animator != null)
            {
                animator.SetFloat("Speed", currentSpeed / walkSpeed);
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsMoving", false);
            }
        }
    }
    
    void HandleJump()
    {
        if (isJumping && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
            
            isJumping = false;
        }
    }
    
    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", velocity.y);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}