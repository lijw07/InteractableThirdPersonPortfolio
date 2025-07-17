# Animation Setup Guide

## Required Animation Parameters

Based on the scripts, your Animator Controller needs these parameters:

### Float Parameters
- **Speed** (Float) - Controls movement animation blending
  - 0 = Idle
  - 1 = Walk
  - 2 = Run (when sprinting)

- **VerticalVelocity** (Float) - For fall/jump blend states
  - Positive = Jumping up
  - Negative = Falling down

### Bool Parameters
- **IsMoving** (Bool) - True when character is moving
- **IsGrounded** (Bool) - True when on ground
- **Block** (Bool) - True when blocking (for CombatController)

### Trigger Parameters
- **Jump** (Trigger) - Fires when jumping
- **Attack** (Trigger) - Fires when attacking

## Animator Setup Steps

### 1. Create Base Layer States
```
Ground Movement (blend tree - default state)
Jump (single state - recommended for simplicity)
Attack 01
Block Idle
```

Note: Idle, Walk, and Run are handled inside the Ground Movement blend tree, not as separate states.

**Jump Options:**
- **Simple (Recommended)**: One "Jump" state with complete animation
- **Advanced**: Split into Jump Start → Jump Loop → Jump Land (only if you need air control)

**State Machine Structure:**
```
Entry → Ground Movement (contains Idle/Walk/Run blend)
         ↑
         ├── Jump Land returns here
         ├── Attack 01 returns here
         └── Block Idle returns here
```

### 2. Set Up Blend Tree for Movement
1. Right-click in Animator > Create State > From New Blend Tree
2. Name it "Ground Movement"
3. Double-click to edit
4. Set Blend Type to "1D"
5. Set Parameter to "Speed"
6. Add Motion fields:
   - Idle animation (threshold: 0)
   - Walk animation (threshold: 1)
   - Run animation (threshold: 2)

### 3. Configure Transitions

#### Entry Setup
- **Entry → Ground Movement** (default state)
  - This is your main state that handles idle/walk/run

#### Movement Transitions
- The Ground Movement blend tree handles transitions internally based on Speed parameter
- No additional transitions needed for movement states

#### Jump Transitions

**Simple Jump (Recommended):**
- **Any State → Jump**
  - Condition: Jump (trigger)
  - Transition Duration: 0.1
  - Can Transition To Self: false

- **Jump → Ground Movement**
  - Exit Time: 0.9
  - Transition Duration: 0.2
  - Has Exit Time: true

**Advanced 3-Phase Jump (Optional):**
Only use if you need hovering or special air control:
- Any State → Jump Start (Condition: Jump trigger)
- Jump Start → Jump Loop (Exit Time: 0.9)
- Jump Loop → Jump Land (Condition: IsGrounded = true)
- Jump Land → Ground Movement (Exit Time: 0.8)

#### Combat Transitions
- **Any State → Attack 01**
  - Condition: Attack (trigger)
  - Transition Duration: 0.05
  - Can Transition To Self: false

- **Attack 01 → Ground Movement**
  - Exit Time: 0.9
  - Transition Duration: 0.1
  - Has Exit Time: true

- **Any State → Block Idle**
  - Condition: Block = true
  - Transition Duration: 0.1

- **Block Idle → Ground Movement**
  - Condition: Block = false
  - Transition Duration: 0.2

### 4. Animation Import Settings

For each animation clip:
1. Select the animation in Project
2. In Inspector, go to Animation tab
3. Configure:
   - **Loop Time**: 
     - ✓ for: Idle, Walk, Run, Block Idle
     - ✗ for: Jump, Attack animations
   - **Root Transform Rotation**: Bake Into Pose
   - **Root Transform Position (Y)**: Bake Into Pose
   - **Root Transform Position (XZ)**: Bake Into Pose

### 5. Layer Settings

#### Base Layer (for movement)
- Weight: 1
- Mask: None
- Blending: Override

#### Combat Layer (optional, for upper body)
- Weight: 1
- Mask: Upper Body (create Avatar Mask)
- Blending: Override
- For attacking while moving

### 6. Script Integration Tips

The scripts expect:
- Smooth transitions (0.1-0.2s)
- Attack animations about 0.7s long
- Jump trigger responds immediately
- Block is a hold state (bool, not trigger)

### 7. Testing Checklist

- [ ] Idle plays when standing still
- [ ] Walk/Run blend smoothly with Speed parameter
- [ ] Jump animation plays fully when spacebar pressed
- [ ] Attack plays fully before returning
- [ ] Block holds while right-click held
- [ ] Can attack while moving (if using layers)
- [ ] Transitions are smooth, not snappy

## Common Issues & Solutions

**Character slides during animations**
- Enable "Bake Into Pose" for Root Motion

**Attack animation cuts off early**
- Increase Exit Time on Attack → Idle transition
- Adjust attackAnimationDuration in CombatController

**Jump feels unresponsive**
- Reduce transition time to Jump Start
- Check Jump trigger is firing in script

**Movement looks robotic**
- Increase transition durations
- Add more in-between animations to blend tree

## Advanced Setup (Optional)

### Combat Combo System
```
Attack 01 → Attack 02 → Attack 03
```
Add transitions with:
- Exit Time: 0.6-0.7
- Condition: Attack (trigger)

### Directional Attacks
Use Sub-State Machines with blend trees based on movement input

### Weapon-Specific Animations
Use Animator Override Controllers for different weapon types