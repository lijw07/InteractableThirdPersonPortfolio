using UnityEngine;
using UnityEngine.InputSystem;

public class BasicPlayerInput : MonoBehaviour
{
    private InputActionAsset inputActions;
    
    public InputAction MoveAction { get; private set; }
    public InputAction LookAction { get; private set; }
    public InputAction SprintAction { get; private set; }
    
    void Awake()
    {
        inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
        
        var gameplayMap = inputActions.AddActionMap("Gameplay");
        
        MoveAction = gameplayMap.AddAction("Move", InputActionType.Value);
        MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        LookAction = gameplayMap.AddAction("Look", InputActionType.Value);
        LookAction.AddBinding("<Mouse>/delta");
        
        SprintAction = gameplayMap.AddAction("Sprint", InputActionType.Button);
        SprintAction.AddBinding("<Keyboard>/leftShift");
    }
    
    void OnEnable()
    {
        inputActions.Enable();
    }
    
    void OnDisable()
    {
        inputActions.Disable();
    }
}