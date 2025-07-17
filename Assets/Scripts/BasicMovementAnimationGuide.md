# Basic Movement Animation Setup

## Simplified Setup - Movement Only

### Required Animation Parameters

**Float Parameters:**
- **Speed** (Float) - Controls movement animation blending
  - 0 = Idle
  - 1 = Walk
  - 2 = Run

**Bool Parameters:**
- **IsMoving** (Bool) - True when character is moving

### Animator States

You only need ONE state:
```
Entry → Ground Movement (blend tree)
```

### Setting Up the Movement Blend Tree

1. **Create Blend Tree**
   - Right-click in Animator > Create State > From New Blend Tree
   - Name it "Ground Movement"
   - Set as default state (orange)

2. **Configure Blend Tree**
   - Double-click to edit
   - Blend Type: **1D**
   - Parameter: **Speed**

3. **Add Animations**
   - Click (+) to add 3 motion fields:
   ```
   Idle:  Humanoid@IdleUnarmed.fbx         (threshold: 0)
   Walk:  Humanoid@WalkForwardUnarmed2_RM.fbx  (threshold: 1)
   Run:   Humanoid@RunForward2Unarmed.fbx      (threshold: 2)
   ```

### That's It!

No transitions needed - the blend tree handles everything based on Speed.

## Character Setup

1. Add these components to each character:
   - **Animator** (assign your movement controller)
   - **Character Controller**
   - **BasicThirdPersonController** (new simplified script)
   - **BasicPlayerInput** (simplified input)

2. For the camera:
   - **BasicThirdPersonCamera** (on camera object)
   - Tag as "MainCamera"

## Controls
- **WASD** - Move
- **Mouse** - Look around
- **Left Shift** - Sprint (hold)

## Testing
- Character should idle when still
- Smoothly blend to walk when moving
- Smoothly blend to run when sprinting
- No jumping, no combat - just smooth movement!