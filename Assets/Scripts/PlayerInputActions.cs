using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActions : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;
    
    // Input values accessible by other components
    public Vector2 MoveValue { get; private set; }
    public Vector2 LookValue { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool CrouchPressed { get; private set; }
    public float ScrollValue { get; private set; }
    
    // Events for actions that need to be handled by other components
    public System.Action OnJumpAction;
    public System.Action OnInteractAction;
    public System.Action OnCrouchToggle;
    public System.Action OnSprintStart;
    public System.Action OnSprintEnd;
    public System.Action OnNextAction;

    void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        inputActions.Player.Enable();
        inputActions.Player.SetCallbacks(this);
        
        // Set up manual scroll handling since it's not in the Player action map
        InputSystem.onAfterUpdate += UpdateScrollValue;
    }
    
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.SetCallbacks(null);
        InputSystem.onAfterUpdate -= UpdateScrollValue;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveValue = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookValue = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractPressed = true;
            OnInteractAction?.Invoke();
        }
        else if (context.canceled)
        {
            InteractPressed = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CrouchPressed = !CrouchPressed;
            OnCrouchToggle?.Invoke();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpPressed = true;
            OnJumpAction?.Invoke();
        }
        else if (context.canceled)
        {
            JumpPressed = false;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintHeld = true;
            OnSprintStart?.Invoke();
        }
        else if (context.canceled)
        {
            SprintHeld = false;
            OnSprintEnd?.Invoke();
        }
    }
    
    // Helper method to get scroll input (if needed)
    public void OnScroll(InputAction.CallbackContext context)
    {
        ScrollValue = context.ReadValue<float>();
    }

    public void OnSwitchCharacter(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnNextAction?.Invoke();
        }
    }
    
    void UpdateScrollValue()
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            ScrollValue = mouse.scroll.ReadValue().y;
        }
    }
}