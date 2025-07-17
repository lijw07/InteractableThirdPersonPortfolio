using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(WeaponController))]
public class CombatController : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackAnimationDuration = 0.7f;
    
    [Header("Animation")]
    [SerializeField] private string attackAnimationName = "Attack";
    [SerializeField] private string blockAnimationName = "Block";
    
    private WeaponController weaponController;
    private Animator animator;
    private PlayerInputActions inputActions;
    private ThirdPersonCamera cameraController;
    
    private bool isAttacking = false;
    private bool isBlocking = false;
    private float lastAttackTime = 0f;
    
    [Header("Aim Mode Settings")]
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float aimTransitionSpeed = 5f;
    
    void Awake()
    {
        weaponController = GetComponent<WeaponController>();
        animator = GetComponent<Animator>();
        
        inputActions = GetComponentInParent<PlayerInputActions>();
        if (inputActions == null)
        {
            inputActions = GetComponent<PlayerInputActions>();
        }
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
        
        GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObj != null)
        {
            cameraController = cameraObj.GetComponent<ThirdPersonCamera>();
        }
    }
    
    void Start()
    {
        inputActions.AttackAction.performed += OnAttack;
        inputActions.BlockAimAction.performed += OnBlockAimStart;
        inputActions.BlockAimAction.canceled += OnBlockAimEnd;
    }
    
    void OnDestroy()
    {
        inputActions.AttackAction.performed -= OnAttack;
        inputActions.BlockAimAction.performed -= OnBlockAimStart;
        inputActions.BlockAimAction.canceled -= OnBlockAimEnd;
    }
    
    void OnAttack(InputAction.CallbackContext context)
    {
        if (!isAttacking && Time.time - lastAttackTime > attackCooldown && !isBlocking)
        {
            StartCoroutine(PerformAttack());
        }
    }
    
    void OnBlockAimStart(InputAction.CallbackContext context)
    {
        if (weaponController.HasWeaponEquipped())
        {
            StartBlocking();
        }
        else
        {
            StartAiming();
        }
    }
    
    void OnBlockAimEnd(InputAction.CallbackContext context)
    {
        if (isBlocking)
        {
            StopBlocking();
        }
        else
        {
            StopAiming();
        }
    }
    
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetTrigger(attackAnimationName);
        }
        
        if (weaponController.HasWeaponEquipped())
        {
            WeaponDamage weaponDamage = weaponController.GetEquippedWeapon().GetComponentInChildren<WeaponDamage>();
            if (weaponDamage != null)
            {
                yield return new WaitForSeconds(0.2f);
                weaponDamage.EnableDamage(true);
                weaponController.EnableWeaponCollider(true);
                
                yield return new WaitForSeconds(attackAnimationDuration - 0.4f);
                
                weaponDamage.EnableDamage(false);
                weaponController.EnableWeaponCollider(false);
            }
        }
        else
        {
            PerformUnarmedAttack();
            yield return new WaitForSeconds(attackAnimationDuration);
        }
        
        isAttacking = false;
    }
    
    void PerformUnarmedAttack()
    {
        RaycastHit hit;
        Vector3 attackOrigin = transform.position + transform.up * 1f;
        Vector3 attackDirection = transform.forward;
        
        if (Physics.Raycast(attackOrigin, attackDirection, out hit, 2f))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(5f);
            }
            
            Debug.Log($"Punched {hit.collider.name}!");
        }
    }
    
    void StartBlocking()
    {
        isBlocking = true;
        
        if (animator != null)
        {
            animator.SetBool(blockAnimationName, true);
        }
        
        Debug.Log("Blocking!");
    }
    
    void StopBlocking()
    {
        isBlocking = false;
        
        if (animator != null)
        {
            animator.SetBool(blockAnimationName, false);
        }
    }
    
    void StartAiming()
    {
        if (cameraController != null)
        {
            cameraController.SetAimMode(true);
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartCoroutine(TransitionFOV(mainCamera, aimFOV));
        }
    }
    
    void StopAiming()
    {
        if (cameraController != null)
        {
            cameraController.SetAimMode(false);
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartCoroutine(TransitionFOV(mainCamera, normalFOV));
        }
    }
    
    IEnumerator TransitionFOV(Camera camera, float targetFOV)
    {
        float startFOV = camera.fieldOfView;
        float elapsed = 0f;
        
        while (elapsed < 1f / aimTransitionSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * aimTransitionSpeed;
            camera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
        
        camera.fieldOfView = targetFOV;
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public bool IsBlocking()
    {
        return isBlocking;
    }
    
    public void TakeDamage(float damage)
    {
        if (isBlocking)
        {
            damage *= 0.2f;
            Debug.Log($"Blocked! Reduced damage to {damage}");
        }
        
        Debug.Log($"Took {damage} damage!");
    }
}