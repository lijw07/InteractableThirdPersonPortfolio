using System.Collections.Generic;
using UnityEngine;

public class PlayerDebugComponent : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugGUI = false;
    [SerializeField] private bool showGizmos = true;
    
    private PlayerController playerController;
    private DebugRenderer debugRenderer;
    private DebugGUI debugGUI;
    
    private void Awake()
    {
        InitializeDebugComponent();
    }
    
    private void InitializeDebugComponent()
    {
        playerController = GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerDebugComponent: PlayerController component missing!");
            enabled = false;
            return;
        }
        
        debugRenderer = new DebugRenderer(playerController, showGizmos);
        debugGUI = new DebugGUI(playerController, showDebugGUI);
    }
    
    private void OnGUI()
    {
        debugGUI?.DrawIfEnabled();
    }
    
    private void OnDrawGizmos()
    {
        debugRenderer?.DrawGizmos();
    }
    
    private void OnDrawGizmosSelected()
    {
        debugRenderer?.DrawGizmosSelected();
    }
    
    public void ToggleDebugGUI()
    {
        showDebugGUI = !showDebugGUI;
        debugGUI?.SetEnabled(showDebugGUI);
    }
    
    public void ToggleGizmos()
    {
        showGizmos = !showGizmos;
        debugRenderer?.SetEnabled(showGizmos);
    }
}

#region Debug Classes

public class DebugRenderer
{
    private readonly PlayerController controller;
    private bool isEnabled;
    
    private const float GIZMO_HEIGHT_OFFSET = 1f;
    private const float GIZMO_LINE_LENGTH = 3f;
    private const float ARROW_SIZE = 0.5f;
    private const float COORDINATE_AXIS_LENGTH = 2f;
    
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float lastCameraDistance;
    private bool cameraSnappingDetected;
    
    public DebugRenderer(PlayerController controller, bool isEnabled)
    {
        this.controller = controller;
        this.isEnabled = isEnabled;
    }
    
    public void SetEnabled(bool enabled) => isEnabled = enabled;
    
    public void DrawGizmos()
    {
        if (!isEnabled || controller == null) return;
        
        DrawMovementDirection();
        DrawActualVelocity();
        DrawSpeedIndicator();
        DrawCameraDebugInfo();
        UpdateCameraTracking();
    }
    
    public void DrawGizmosSelected()
    {
        if (!isEnabled || controller == null) return;
        DrawCoordinateSystem();
    }
    
    private void DrawMovementDirection()
    {
        Vector3 currentDirection = controller.GetCurrentMoveDirection();
        if (currentDirection == Vector3.zero) return;
        
        Gizmos.color = Color.yellow;
        
        Vector3 startPos = GetGizmoStartPosition();
        Vector3 endPos = startPos + currentDirection * GIZMO_LINE_LENGTH;
        
        Gizmos.DrawLine(startPos, endPos);
        DrawArrowHead(endPos, currentDirection);
    }
    
    private void DrawActualVelocity()
    {
        Vector3 calculatedVelocity = controller.GetCalculatedVelocity();
        if (calculatedVelocity == Vector3.zero) return;
        
        Gizmos.color = Color.cyan;
        
        Vector3 startPos = GetGizmoStartPosition();
        Vector3 normalizedVelocity = calculatedVelocity.normalized;
        Vector3 endPos = startPos + normalizedVelocity * GIZMO_LINE_LENGTH;
        
        Gizmos.DrawLine(startPos, endPos);
        DrawArrowHead(endPos, normalizedVelocity);
    }
    
    private void DrawSpeedIndicator()
    {
        Gizmos.color = Color.white;
        Vector3 position = controller.transform.position + Vector3.up * 0.1f;
        float radius = controller.GetCurrentSpeed() * 0.2f;
        Gizmos.DrawWireSphere(position, radius);
    }
    
    private void DrawCoordinateSystem()
    {
        Vector3 pos = GetCoordinateSystemPosition();
        
        DrawAxis(pos, Vector3.right, Color.red, "X");
        DrawAxis(pos, Vector3.up, Color.green, "Y");
        DrawAxis(pos, Vector3.forward, Color.blue, "Z");
    }
    
    private void DrawAxis(Vector3 position, Vector3 direction, Color color, string label)
    {
        Gizmos.color = color;
        Vector3 endPos = position + direction * COORDINATE_AXIS_LENGTH;
        Gizmos.DrawLine(position, endPos);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(endPos, label);
        #endif
    }
    
    private Vector3 GetGizmoStartPosition()
    {
        return controller.transform.position + Vector3.up * GIZMO_HEIGHT_OFFSET;
    }
    
    private Vector3 GetCoordinateSystemPosition()
    {
        return controller.transform.position + Vector3.up * COORDINATE_AXIS_LENGTH;
    }
    
    private void DrawArrowHead(Vector3 endPos, Vector3 direction)
    {
        Vector3 arrowRight = CalculateArrowDirection(direction, 160f);
        Vector3 arrowLeft = CalculateArrowDirection(direction, -160f);
        
        Gizmos.DrawLine(endPos, endPos + arrowRight * ARROW_SIZE);
        Gizmos.DrawLine(endPos, endPos + arrowLeft * ARROW_SIZE);
    }
    
    private Vector3 CalculateArrowDirection(Vector3 direction, float angle)
    {
        return Quaternion.LookRotation(direction) * Quaternion.Euler(0, angle, 0) * Vector3.forward;
    }
    
    private void DrawCameraDebugInfo()
    {
        if (ThirdPersonCamera.Instance == null) return;
        
        Transform cameraTransform = ThirdPersonCamera.Instance.GetCameraTransform();
        if (cameraTransform == null) return;
        
        Gizmos.color = Color.magenta;
        Vector3 cameraPos = cameraTransform.position;
        Vector3 cameraForward = ThirdPersonCamera.Instance.GetForwardDirection();
        Gizmos.DrawLine(cameraPos, cameraPos + cameraForward * 2f);
        
        Gizmos.color = Color.yellow;
        Vector3 targetPos = controller.transform.position + Vector3.up * 1.5f;
        Gizmos.DrawLine(cameraPos, targetPos);
        
        if (cameraSnappingDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraPos, 0.5f);
        }
    }
    
    private void UpdateCameraTracking()
    {
        if (ThirdPersonCamera.Instance == null) return;
        
        Transform cameraTransform = ThirdPersonCamera.Instance.GetCameraTransform();
        if (cameraTransform == null) return;
        
        Vector3 currentPos = cameraTransform.position;
        Quaternion currentRot = cameraTransform.rotation;
        
        if (lastCameraPosition != Vector3.zero)
        {
            float positionDelta = Vector3.Distance(currentPos, lastCameraPosition);
            float rotationDelta = Quaternion.Angle(currentRot, lastCameraRotation);
            
            const float SNAP_THRESHOLD = 0.5f;
            const float ROTATION_SNAP_THRESHOLD = 10f;
            
            cameraSnappingDetected = (positionDelta > SNAP_THRESHOLD) || (rotationDelta > ROTATION_SNAP_THRESHOLD);
        }
        
        lastCameraPosition = currentPos;
        lastCameraRotation = currentRot;
    }
}

public class DebugGUI
{
    private readonly PlayerController controller;
    private bool isEnabled;
    
    private Vector3 lastCameraPosition;
    private float lastCameraDistance;
    private Queue<float> cameraPositionDeltas = new Queue<float>();
    private Queue<float> cameraDistanceDeltas = new Queue<float>();
    private const int DELTA_HISTORY_SIZE = 60;
    
    private Vector2 scrollPosition;
    
    private const int WINDOW_WIDTH = 300;
    private const int WINDOW_HEIGHT = 500;
    private const int MARGIN = 10;
    private const int CONTENT_MARGIN = 20;
    
    public DebugGUI(PlayerController controller, bool isEnabled)
    {
        this.controller = controller;
        this.isEnabled = isEnabled;
    }
    
    public void SetEnabled(bool enabled) => isEnabled = enabled;
    
    public void DrawIfEnabled()
    {
        if (!isEnabled || controller == null) return;
        
        try
        {
            DrawDebugWindow();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerDebugComponent GUI Error: {e.Message}");
        }
    }
    
    private void DrawDebugWindow()
    {
        GUI.Box(new Rect(MARGIN, MARGIN, WINDOW_WIDTH, WINDOW_HEIGHT), "Player Controller Debug");
        
        GUILayout.BeginArea(new Rect(CONTENT_MARGIN, MARGIN + 20, WINDOW_WIDTH - 20, WINDOW_HEIGHT - 40));
        
        DrawMovementInfo();
        DrawInputInfo();
        DrawRotationInfo();
        DrawVelocityInfo();
        DrawCameraDebugInfo();
        DrawInstructions();
        
        GUILayout.EndArea();
    }
    
    private void DrawMovementInfo()
    {
        DrawSection("Movement", () =>
        {
            DrawLabelValue("Current Speed", controller.GetCurrentSpeed().ToString("F2"));
            DrawLabelValue("Target Speed", controller.GetTargetSpeed().ToString("F2"));
            DrawLabelValue("Speed Velocity", controller.GetSpeedVelocity().ToString("F2"));
            DrawLabelValue("Movement Mode", GetMovementModeString());
            DrawLabelValue("Direction", VectorToString(controller.GetCurrentMoveDirection()));
        });
    }
    
    private void DrawInputInfo()
    {
        DrawSection("Input", () =>
        {
            var inputActions = controller.GetInputActions();
            
            if (inputActions == null)
            {
                DrawLabel("Input actions null!");
                DrawLabel("--");
                DrawLabel("--");
                DrawLabel("--");
                return;
            }
            Vector2 rawInput = inputActions.MoveValue;
            DrawLabelValue("Raw Input", VectorToString(rawInput));
            DrawLabelValue("Input Magnitude", rawInput.magnitude.ToString("F2"));
            DrawLabelValue("Smoothed H", controller.GetCurrentHorizontal().ToString("F2"));
            DrawLabelValue("Smoothed V", controller.GetCurrentVertical().ToString("F2"));
        });
    }
    
    private void DrawRotationInfo()
    {
        DrawSection("Rotation", () =>
        {
            float angleDifference = GetAngleDifferenceFromCamera();
            DrawLabelValue("Angle to Camera", angleDifference.ToString("F2") + "°");
            
            DrawLabelValue("Player Y Rotation", controller.transform.eulerAngles.y.ToString("F1") + "°");
            
            if (ThirdPersonCamera.Instance != null)
            {
                Transform cameraTransform = ThirdPersonCamera.Instance.GetCameraTransform();
                DrawLabelValue("Camera Y Rotation", cameraTransform.eulerAngles.y.ToString("F1") + "°");
            }
        });
    }
    
    
    
    private void DrawVelocityInfo()
    {
        DrawSection("Velocity", () =>
        {
            Vector3 velocity = controller.GetCalculatedVelocity();
            DrawLabelValue("Actual Velocity", VectorToString(velocity));
            DrawLabelValue("Velocity Magnitude", velocity.magnitude.ToString("F2"));
        });
    }
    
    private void DrawCameraDebugInfo()
    {
        DrawSection("Camera Debug", () =>
        {
            bool hasCamera = ThirdPersonCamera.Instance != null;
            
            if (!hasCamera)
            {
                DrawLabel("ThirdPersonCamera not found!");
                DrawLabel("--");
                DrawLabel("--");
                DrawLabel("--");
                return;
            }
            
            Transform cameraTransform = ThirdPersonCamera.Instance.GetCameraTransform();
            if (cameraTransform == null)
            {
                DrawLabel("Camera transform null!");
                DrawLabel("--");
                DrawLabel("--");
                DrawLabel("--");
                return;
            }
            
            Vector3 currentCameraPos = cameraTransform.position;
            Vector3 targetPos = controller.transform.position;
            float currentDistance = Vector3.Distance(currentCameraPos, targetPos);
            
            DrawLabelValue("Distance", currentDistance.ToString("F2"));
            DrawLabelValue("Target", ThirdPersonCamera.Instance.GetCurrentTarget()?.name ?? "null");
            
            if (lastCameraPosition != Vector3.zero)
            {
                float positionDelta = Vector3.Distance(currentCameraPos, lastCameraPosition);
                float distanceDelta = Mathf.Abs(currentDistance - lastCameraDistance);
                
                DrawLabelValue("Pos Delta", positionDelta.ToString("F4"));
                DrawLabelValue("Dist Delta", distanceDelta.ToString("F4"));
                
                const float SNAP_THRESHOLD = 0.1f;
                if (positionDelta > SNAP_THRESHOLD || distanceDelta > SNAP_THRESHOLD)
                {
                    DrawLabel($"SNAP! Δ{positionDelta:F3}");
                }
                else
                {
                    DrawLabel("Camera OK");
                }
            }
            else
            {
                DrawLabel("--");
                DrawLabel("--");
                DrawLabel("Initializing...");
            }
            
            lastCameraPosition = currentCameraPos;
            lastCameraDistance = currentDistance;
        });
    }
    
    private void DrawInstructions()
    {
        DrawSection("Controls", () =>
        {
            DrawLabel("WASD: Move | Shift: Sprint | C: Walk");
        });
    }
    
    private void DrawSection(string title, System.Action drawContent)
    {
        GUILayout.Label(title, CreateHeaderStyle());
        drawContent();
        GUILayout.Space(8);
    }
    
    private void DrawLabelValue(string label, string value)
    {
        GUILayout.Label($"{label}: {value}", CreateLabelStyle());
    }
    
    private void DrawLabel(string text)
    {
        GUILayout.Label(text, CreateLabelStyle());
    }
    
    private string GetMovementModeString()
    {
        if (controller.GetCurrentSpeed() <= 0.1f) return "Idle";
        if (controller.IsWalking()) return "Walking";
        if (controller.IsSprinting()) return "Sprinting";
        return "Running";
    }
    
    private string VectorToString(Vector3 vector)
    {
        return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
    }
    
    private string VectorToString(Vector2 vector)
    {
        return $"({vector.x:F2}, {vector.y:F2})";
    }
    
    private float GetAngleDifferenceFromCamera()
    {
        if (ThirdPersonCamera.Instance == null) return 0f;
        return ThirdPersonCamera.Instance.GetSignedAngleFromPlayer(controller.transform.forward);
    }
    
    private GUIStyle CreateLabelStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal = { textColor = Color.white }
        };
    }
    
    private GUIStyle CreateHeaderStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow }
        };
    }
}

#endregion