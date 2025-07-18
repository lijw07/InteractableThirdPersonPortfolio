using UnityEngine;

public class CameraDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enablePeriodicLogs = true;
    [SerializeField] private float logInterval = 2f;
    [SerializeField] private bool checkForAnomalies = true;
    [SerializeField] private bool trackAngleChanges = true;
    [SerializeField] private float angleChangeThreshold = 45f;
    
    private ThirdPersonCamera cameraScript;
    private Transform lastKnownTarget;
    private Vector3 lastCameraPosition;
    private Vector3 lastCameraRotation;
    private float nextLogTime;
    
    // Angle tracking
    private float lastXAngle;
    private float lastYAngle;
    private float cumulativeXRotation;
    private System.Collections.Generic.List<float> recentXAngles;
    
    void Start()
    {
        cameraScript = GetComponent<ThirdPersonCamera>();
        if (cameraScript == null)
        {
            Debug.LogError("[CameraDebugger] No ThirdPersonCamera component found!");
            enabled = false;
            return;
        }
        
        lastCameraPosition = transform.position;
        lastCameraRotation = transform.eulerAngles;
        lastXAngle = transform.eulerAngles.y;
        lastYAngle = transform.eulerAngles.x;
        nextLogTime = Time.time + logInterval;
        recentXAngles = new System.Collections.Generic.List<float>();
        
        Debug.Log("[CameraDebugger] Camera debugger initialized");
    }
    
    void LateUpdate()
    {
        if (!enablePeriodicLogs) return;
        
        // Check for anomalies
        if (checkForAnomalies)
        {
            CheckCameraAnomalies();
        }
        
        // Periodic status log
        if (Time.time >= nextLogTime)
        {
            LogCameraStatus();
            nextLogTime = Time.time + logInterval;
        }
        
        // Update last known values
        lastCameraPosition = transform.position;
        lastCameraRotation = transform.eulerAngles;
        
        Transform currentTarget = cameraScript.GetTarget();
        if (currentTarget != lastKnownTarget)
        {
            Debug.Log($"[CameraDebugger] Target changed from {(lastKnownTarget != null ? lastKnownTarget.name : "null")} to {(currentTarget != null ? currentTarget.name : "null")}");
            lastKnownTarget = currentTarget;
        }
    }
    
    void CheckCameraAnomalies()
    {
        // Check for sudden position jumps
        float positionDelta = Vector3.Distance(transform.position, lastCameraPosition);
        if (positionDelta > 10f)
        {
            Debug.LogError($"[CameraDebugger] ANOMALY: Camera jumped {positionDelta:F2} units in one frame!");
            Debug.LogError($"[CameraDebugger] From: {lastCameraPosition} To: {transform.position}");
        }
        
        // Check for rotation flips with proper angle difference calculation
        float xAngleDiff = Mathf.DeltaAngle(lastCameraRotation.y, transform.eulerAngles.y);
        float yAngleDiff = Mathf.DeltaAngle(lastCameraRotation.x, transform.eulerAngles.x);
        
        if (Mathf.Abs(xAngleDiff) > angleChangeThreshold || Mathf.Abs(yAngleDiff) > angleChangeThreshold)
        {
            Debug.LogError($"[CameraDebugger] ANOMALY: Large rotation detected! X-axis: {xAngleDiff:F1}°, Y-axis: {yAngleDiff:F1}°");
            Debug.LogError($"[CameraDebugger] From: {lastCameraRotation} To: {transform.eulerAngles}");
            
            // Log recent angle history
            if (recentXAngles.Count > 0)
            {
                Debug.LogError("[CameraDebugger] Recent X angle history:");
                for (int i = 0; i < recentXAngles.Count; i++)
                {
                    Debug.LogError($"  [{i}] {recentXAngles[i]:F1}°");
                }
            }
        }
        
        // Track angle changes
        if (trackAngleChanges)
        {
            float currentXAngle = transform.eulerAngles.y;
            cumulativeXRotation += xAngleDiff;
            
            // Keep history of recent angles
            recentXAngles.Add(currentXAngle);
            if (recentXAngles.Count > 10)
                recentXAngles.RemoveAt(0);
            
            // Check for cumulative rotation issues
            if (Mathf.Abs(cumulativeXRotation) > 720f)
            {
                Debug.LogWarning($"[CameraDebugger] Cumulative X rotation exceeds 720°: {cumulativeXRotation:F1}°");
                cumulativeXRotation = 0f; // Reset to prevent overflow
            }
            
            lastXAngle = currentXAngle;
            lastYAngle = transform.eulerAngles.x;
        }
        
        // Check if camera is at origin (common bug)
        if (transform.position.magnitude < 0.1f)
        {
            Debug.LogError("[CameraDebugger] ANOMALY: Camera is at or near origin!");
        }
        
        // Check if camera is too far from target
        Transform target = cameraScript.GetTarget();
        if (target != null)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);
            if (distToTarget > 50f)
            {
                Debug.LogError($"[CameraDebugger] ANOMALY: Camera is {distToTarget:F2} units from target!");
            }
        }
    }
    
    void LogCameraStatus()
    {
        Transform target = cameraScript.GetTarget();
        string targetInfo = target != null ? $"{target.name} at {target.position}" : "null";
        
        Debug.Log($"[CameraDebugger] === STATUS at Time: {Time.time:F1} ===");
        Debug.Log($"[CameraDebugger] Position: {transform.position}");
        Debug.Log($"[CameraDebugger] Rotation: {transform.eulerAngles} (X={transform.eulerAngles.y:F1}°, Y={transform.eulerAngles.x:F1}°)");
        Debug.Log($"[CameraDebugger] Target: {targetInfo}");
        
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            Vector3 direction = (target.position - transform.position).normalized;
            Debug.Log($"[CameraDebugger] Distance to target: {dist:F2}, Direction: {direction}");
        }
        
        if (trackAngleChanges)
        {
            Debug.Log($"[CameraDebugger] Cumulative X rotation: {cumulativeXRotation:F1}°");
        }
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying && cameraScript != null)
        {
            Transform target = cameraScript.GetTarget();
            if (target != null)
            {
                // Draw line from camera to target
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);
                
                // Draw sphere at target
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target.position, 0.5f);
                
                // Draw camera frustum
                Gizmos.color = Color.cyan;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, 0.5f, 5f, Camera.main.aspect);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}