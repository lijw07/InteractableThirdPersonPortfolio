using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class BasicThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    
    [Header("Physics")]
    [SerializeField] private float gravity = -9.8f;
    
    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 velocity;
    private float turnSmoothVelocity;
    private Animator animator;
    private BasicPlayerInput inputActions;
    
    private Vector2 moveInput;
    private bool isSprinting;
    
    void Awake()
    {
        inputActions = GetComponent<BasicPlayerInput>();
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<BasicPlayerInput>();
        }
    }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        
        inputActions.MoveAction.performed += OnMove;
        inputActions.MoveAction.canceled += OnMove;
        inputActions.SprintAction.performed += OnSprint;
        inputActions.SprintAction.canceled += OnSprint;
    }
    
    void OnDestroy()
    {
        inputActions.MoveAction.performed -= OnMove;
        inputActions.MoveAction.canceled -= OnMove;
        inputActions.SprintAction.performed -= OnSprint;
        inputActions.SprintAction.canceled -= OnSprint;
    }
    
    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }
    
    void Update()
    {
        HandleMovement();
        ApplyGravity();
    }
    
    void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        if (direction.magnitude >= 0.1f && cameraTransform != null)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isSprinting ? runSpeed : walkSpeed;
            
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
    
    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}