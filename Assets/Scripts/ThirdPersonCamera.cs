using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float zoomSensitivity = 0.5f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 20f;
    [SerializeField] private float targetHeightOffset = 1.2f; // Height offset to focus on character's upper body
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool logMouseInput = false;
    [SerializeField] private bool logCameraPosition = false;
    [SerializeField] private bool logTargetInfo = true;
    [SerializeField] private bool logAngleHistory = true;
    [SerializeField] private int angleHistorySize = 10;
    
    private float currentX = 0f;
    private float currentY = 20f;
    private PlayerInputActions inputActions;
    
    // Debug tracking
    private Vector3 lastTargetPosition;
    private float lastUpdateTime;
    private System.Collections.Generic.Queue<string> angleHistory;
    private int frameCounter = 0;
    
    void Awake()
    {
        inputActions = GetComponent<PlayerInputActions>() ?? gameObject.AddComponent<PlayerInputActions>();
        
        // Lock cursor to game window and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Auto-assign target if not set
        if (target == null)
        {
            GameObject playerController = GameObject.Find("PlayerController");
            if (playerController != null)
            {
                target = playerController.transform;
                Debug.Log($"[ThirdPersonCamera] Auto-assigned target to: {target.name}");
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[ThirdPersonCamera] Initialized - Initial angles: X={currentX}, Y={currentY}, Distance={distance}");
        }
        
        // Initialize angle history
        angleHistory = new System.Collections.Generic.Queue<string>(angleHistorySize);
    }
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("[ThirdPersonCamera] No target assigned! Camera will not function properly.");
        }
        else
        {
            lastTargetPosition = target.position;
            if (enableDebugLogs)
            {
                Debug.Log($"[ThirdPersonCamera] Target confirmed: {target.name} at {target.position}");
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null)
        {
            if (Time.frameCount % 60 == 0 && enableDebugLogs) // Log every second
            {
                Debug.LogWarning("[ThirdPersonCamera] No target assigned!");
            }
            return;
        }
        
        // Track frame timing
        float deltaTime = Time.time - lastUpdateTime;
        lastUpdateTime = Time.time;
        
        // Detect target jumps
        if (enableDebugLogs && logTargetInfo)
        {
            float targetMoveDist = Vector3.Distance(target.position, lastTargetPosition);
            if (targetMoveDist > 5f) // Large movement in one frame
            {
                Debug.LogWarning($"[ThirdPersonCamera] Target jumped {targetMoveDist:F2} units in one frame! From {lastTargetPosition} to {target.position}");
            }
            lastTargetPosition = target.position;
        }
        
        // Handle escape key to unlock cursor (for testing in editor)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (enableDebugLogs) Debug.Log("[ThirdPersonCamera] Cursor unlocked");
        }
        
        // Debug: Reset camera rotation with R key
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
        {
            currentX = 0f;
            currentY = 20f;
            if (enableDebugLogs) Debug.Log($"[ThirdPersonCamera] Camera rotation reset to X={currentX}, Y={currentY}");
        }
        
        // Re-lock cursor on click
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (enableDebugLogs) Debug.Log("[ThirdPersonCamera] Cursor locked");
        }
        
        // Handle mouse rotation
        Vector2 mouseInput = inputActions.LookValue;
        
        float prevX = currentX;
        float prevY = currentY;
        
        // Store raw angle before normalization for debugging
        float rawX = currentX + mouseInput.x * mouseSensitivity * Time.deltaTime;
        
        currentX += mouseInput.x * mouseSensitivity * Time.deltaTime;
        currentY -= mouseInput.y * mouseSensitivity * Time.deltaTime;
        
        // Track angle history before normalization
        if (enableDebugLogs && logAngleHistory && frameCounter % 5 == 0) // Log every 5 frames
        {
            string historyEntry = $"Frame {frameCounter}: PreNorm X={currentX:F1}, Raw={rawX:F1}, Input={mouseInput.x:F3}, dt={Time.deltaTime:F4}";
            angleHistory.Enqueue(historyEntry);
            if (angleHistory.Count > angleHistorySize)
                angleHistory.Dequeue();
        }
        
        // NO NORMALIZATION - Let angles accumulate freely
        // Unity's Quaternion.Euler handles any angle values correctly
        
        // Clamp Y rotation
        currentY = Mathf.Clamp(currentY, -30f, 60f);
        
        if (enableDebugLogs && logMouseInput && mouseInput.magnitude > 0.1f)
        {
            Debug.Log($"[ThirdPersonCamera] Mouse input: {mouseInput}, Angles: X={currentX:F1} (Δ{currentX-prevX:F2}), Y={currentY:F1} (Δ{currentY-prevY:F2})");
        }
        
        // Detect actual angle snapping issues
        float actualDelta = mouseInput.x * mouseSensitivity * Time.deltaTime;
        float measuredDelta = currentX - prevX;
        
        // Without normalization, any large discrepancy between expected and actual delta is a real issue
        if (Mathf.Abs(measuredDelta - actualDelta) > 0.1f && Mathf.Abs(actualDelta) > 0.01f && enableDebugLogs)
        {
            Debug.LogError($"[ThirdPersonCamera] Angle snap detected! X went from {prevX:F1} to {currentX:F1}");
            Debug.LogError($"[ThirdPersonCamera] Details - Raw input: {mouseInput.x}, Expected delta: {actualDelta:F3}, Actual delta: {measuredDelta:F1}");
            Debug.LogError($"[ThirdPersonCamera] Time.deltaTime: {Time.deltaTime}");
            
            // Print angle history
            if (logAngleHistory && angleHistory.Count > 0)
            {
                Debug.LogError("[ThirdPersonCamera] === Angle History ===");
                foreach (string entry in angleHistory)
                {
                    Debug.LogError($"[ThirdPersonCamera] {entry}");
                }
            }
        }
        
        frameCounter++;
        
        // Handle zoom
        float scrollInput = inputActions.ScrollValue;
        if (scrollInput != 0 && enableDebugLogs)
        {
            float prevDist = distance;
            distance -= scrollInput * zoomSensitivity;
            distance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
            Debug.Log($"[ThirdPersonCamera] Zoom: {prevDist:F1} -> {distance:F1} (scroll: {scrollInput})");
        }
        
        // Apply camera position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        
        // Add height offset to focus on character's head/upper body
        Vector3 focusPoint = target.position + Vector3.up * targetHeightOffset;
        Vector3 desiredPosition = focusPoint + offset;
        
        // Log position changes
        if (enableDebugLogs && logCameraPosition)
        {
            float posChange = Vector3.Distance(transform.position, desiredPosition);
            if (posChange > 0.1f)
            {
                Debug.Log($"[ThirdPersonCamera] Position change: {posChange:F2} units, Target: {target.position}, Focus: {focusPoint}");
            }
        }
        
        transform.position = desiredPosition;
        transform.rotation = rotation;
    }
    
    public void SetTarget(Transform newTarget)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ThirdPersonCamera] SetTarget called - Old: {(target != null ? target.name : "null")}, New: {(newTarget != null ? newTarget.name : "null")}");
        }
        
        target = newTarget;
        
        if (newTarget != null)
        {
            lastTargetPosition = newTarget.position;
        }
    }
    
    public Transform GetTarget() => target;
    
    void OnValidate()
    {
        if (enableDebugLogs && Application.isPlaying)
        {
            Debug.Log($"[ThirdPersonCamera] Settings changed - Distance: {distance}, MouseSens: {mouseSensitivity}, HeightOffset: {targetHeightOffset}");
        }
    }
    
    // Normalize angle to be between -180 and 180
    float NormalizeAngle(float angle)
    {
        // Track normalization for debugging
        float originalAngle = angle;
        int iterations = 0;
        
        while (angle > 180f) 
        {
            angle -= 360f;
            iterations++;
            if (iterations > 10) // Safety check
            {
                Debug.LogError($"[ThirdPersonCamera] NormalizeAngle stuck in loop! Original: {originalAngle}, Current: {angle}");
                break;
            }
        }
        
        iterations = 0;
        while (angle < -180f) 
        {
            angle += 360f;
            iterations++;
            if (iterations > 10) // Safety check
            {
                Debug.LogError($"[ThirdPersonCamera] NormalizeAngle stuck in loop! Original: {originalAngle}, Current: {angle}");
                break;
            }
        }
        
        if (enableDebugLogs && Mathf.Abs(originalAngle - angle) > 180f)
        {
            Debug.LogWarning($"[ThirdPersonCamera] Large normalization: {originalAngle:F1} -> {angle:F1}");
        }
        
        return angle;
    }
}