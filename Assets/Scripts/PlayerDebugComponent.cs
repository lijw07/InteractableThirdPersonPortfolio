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
        
        // Draw axis label
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
}

public class DebugGUI
{
    private readonly PlayerController controller;
    private bool isEnabled;
    
    private const int WINDOW_WIDTH = 320;
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
        DrawDebugWindow();
    }
    
    private void DrawDebugWindow()
    {
        GUI.Box(new Rect(MARGIN, MARGIN, WINDOW_WIDTH, WINDOW_HEIGHT), "Player Controller Debug");
        
        GUILayout.BeginArea(new Rect(CONTENT_MARGIN, MARGIN + 20, WINDOW_WIDTH - 20, WINDOW_HEIGHT - 40));
        
        DrawMovementInfo();
        DrawInputInfo();
        DrawRotationInfo();
        DrawRotationTrackingInfo();
        DrawVelocityInfo();
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
        var inputActions = controller.GetInputActions();
        
        if (inputActions == null) return;
        
        DrawSection("Input", () =>
        {
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
            DrawLabelValue("Current Turn Angle", controller.GetCurrentTurnAngle().ToString("F2") + "°");
            DrawLabelValue("Target Turn Angle", controller.GetTargetTurnAngle().ToString("F2") + "°");
            DrawLabelValue("Is Performing Turn", controller.IsPerformingTurnInPlace().ToString());
            
            // Calculate angle to camera
            Vector3 cameraForward = GetCameraForwardDirection();
            float angleDifference = CalculateSignedAngleDifference(cameraForward);
            DrawLabelValue("Angle to Camera", angleDifference.ToString("F2") + "°");
            
            // Current player rotation
            DrawLabelValue("Player Y Rotation", controller.transform.eulerAngles.y.ToString("F1") + "°");
            DrawLabelValue("Camera Y Rotation", Camera.main.transform.eulerAngles.y.ToString("F1") + "°");
        });
    }
    
    private void DrawRotationTrackingInfo()
    {
        DrawSection("Turn Tracking", () =>
        {
            DrawLabelValue("Tracking Active", controller.IsTrackingRotation().ToString());
            DrawLabelValue("Last Turn Type", controller.GetLastTurnDirection());
            DrawLabelValue("Has Locked Target", controller.HasLockedTurnTarget().ToString());
            
            if (controller.HasLockedTurnTarget())
            {
                Vector3 lockedTarget = controller.GetLockedTurnTarget();
                DrawLabelValue("Locked Target Dir", VectorToString(lockedTarget));
                
                // Calculate angle between current forward and locked target
                float angleToTarget = Vector3.Angle(controller.transform.forward, lockedTarget);
                DrawLabelValue("Angle to Target", angleToTarget.ToString("F1") + "°");
            }
            
            if (controller.IsTrackingRotation())
            {
                DrawLabelValue("Turn Start Time", (Time.time - controller.GetTurnStartTime()).ToString("F2") + "s ago");
                DrawLabelValue("Expected Rotation", controller.GetExpectedRotationAmount().ToString("F1") + "°");
                DrawLabelValue("Before Turn Y", controller.GetRotationBeforeTurn().y.ToString("F1") + "°");
                DrawLabelValue("Current Y", controller.transform.eulerAngles.y.ToString("F1") + "°");
                
                // Calculate current rotation progress
                float currentDiff = CalculateRotationProgress();
                DrawLabelValue("Rotation Progress", currentDiff.ToString("F1") + "°");
            }
            else if (controller.GetLastTurnDirection() != "")
            {
                DrawLabelValue("Last Before Y", controller.GetRotationBeforeTurn().y.ToString("F1") + "°");
                DrawLabelValue("Last After Y", controller.GetRotationAfterTurn().y.ToString("F1") + "°");
                
                float actualRotation = CalculateActualRotationFromLast();
                DrawLabelValue("Last Actual Rotation", actualRotation.ToString("F1") + "°");
            }
        });
    }
    
    private float CalculateRotationProgress()
    {
        Vector3 before = controller.GetRotationBeforeTurn();
        Vector3 current = controller.transform.eulerAngles;
        
        float diff = current.y - before.y;
        
        // Handle 360° wraparound
        if (diff > 180f)
            diff -= 360f;
        else if (diff < -180f)
            diff += 360f;
            
        return diff;
    }
    
    private float CalculateActualRotationFromLast()
    {
        Vector3 before = controller.GetRotationBeforeTurn();
        Vector3 after = controller.GetRotationAfterTurn();
        
        float diff = after.y - before.y;
        
        // Handle 360° wraparound
        if (diff > 180f)
            diff -= 360f;
        else if (diff < -180f)
            diff += 360f;
            
        return diff;
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
    
    private void DrawInstructions()
    {
        DrawSection("Instructions", () =>
        {
            DrawLabel("Toggle debug in Inspector");
            DrawLabel("WASD: Move");
            DrawLabel("Shift: Sprint");
            DrawLabel("C: Toggle Walk");
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
    
    // Helper methods for angle calculation (copied from PlayerController pattern)
    private Vector3 GetCameraForwardDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        return forward.normalized;
    }
    
    private float CalculateSignedAngleDifference(Vector3 cameraForward)
    {
        Vector3 currentForward = controller.transform.forward;
        float angle = Vector3.Angle(currentForward, cameraForward);
        
        Vector3 cross = Vector3.Cross(currentForward, cameraForward);
        if (cross.y < 0)
        {
            angle = -angle;
        }
        
        return angle;
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