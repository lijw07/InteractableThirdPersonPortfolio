using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActions : MonoBehaviour
{
    private InputActionAsset inputActions;
    
    public InputAction MoveAction { get; private set; }
    public InputAction LookAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction SprintAction { get; private set; }
    public InputAction SwitchCharacterAction { get; private set; }
    public InputAction AttackAction { get; private set; }
    public InputAction BlockAimAction { get; private set; }
    
    void Awake()
    {
        inputActions = new InputActionAsset();
        
        var gameplayMap = inputActions.AddActionMap("Gameplay");
        
        MoveAction = gameplayMap.AddAction("Move", InputActionType.Value);
        MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        LookAction = gameplayMap.AddAction("Look", InputActionType.Value);
        LookAction.AddBinding("<Mouse>/delta");
        
        JumpAction = gameplayMap.AddAction("Jump", InputActionType.Button);
        JumpAction.AddBinding("<Keyboard>/space");
        
        SprintAction = gameplayMap.AddAction("Sprint", InputActionType.Button);
        SprintAction.AddBinding("<Keyboard>/leftShift");
        
        SwitchCharacterAction = gameplayMap.AddAction("SwitchCharacter", InputActionType.Button);
        SwitchCharacterAction.AddBinding("<Keyboard>/t");
        
        AttackAction = gameplayMap.AddAction("Attack", InputActionType.Button);
        AttackAction.AddBinding("<Mouse>/leftButton");
        
        BlockAimAction = gameplayMap.AddAction("BlockAim", InputActionType.Button);
        BlockAimAction.AddBinding("<Mouse>/rightButton");
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