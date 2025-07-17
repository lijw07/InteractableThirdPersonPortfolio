# Jump Animation Setup - No Blend Tree Needed

## Why No Blend Tree for Jump?

**Blend trees** are for smoothly mixing between similar animations based on parameters (like walk → run based on speed).

**Jumping** is a sequence of distinct phases that play one after another:
1. Jump Start (takeoff)
2. Jump Loop (in air)
3. Jump Land (landing)

## Correct Jump Setup:

### Option 1: Simple Jump (Using One Animation)
If using a single jump animation like `Humanoid@RunJumpToApexLeftFootUnarmed_RM.fbx`:

```
Ground Movement (blend tree)
     ↓
Jump Animation (single state)
     ↓
Ground Movement (return)
```

- Just one jump state
- The animation contains all phases
- Transitions based on IsGrounded

### Option 2: Three-Phase Jump (Recommended)
For more control, split into three states:

```
Ground Movement → Jump Start → Jump Loop → Jump Land → Ground Movement
```

**State Setup:**
- **Jump Start**: First part of jump animation (0-30%)
- **Jump Loop**: Middle part looped (30-70%) 
- **Jump Land**: Final part (70-100%)

**Transitions:**
- Ground Movement → Jump Start: `Jump trigger`
- Jump Start → Jump Loop: `Exit Time 0.9`
- Jump Loop → Jump Land: `IsGrounded = true`
- Jump Land → Ground Movement: `Exit Time 0.8`

## No Blend Tree Because:
- You're not blending between different jump types
- It's a sequential animation, not a parametric blend
- Each phase plays completely before the next

## Visual Example:
```
BLEND TREE (for movement):
Speed 0 ← Idle
Speed 1 ← Walk  } Blended smoothly
Speed 2 ← Run

JUMP STATES (sequential):
State 1: Jump Start → State 2: Loop → State 3: Land
         Sequential, not blended
```

The script sets IsGrounded and triggers Jump - perfect for state transitions, not blending!