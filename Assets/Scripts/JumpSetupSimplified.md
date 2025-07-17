# Jump Animation - Simplified Setup

## The Confusion:
The original guide shows 3 jump states (Start, Loop, Land) which might be overkill for your needs.

## Two Options - Choose One:

### Option A: Simple Jump (Easiest - Start Here!)
Use just ONE jump animation state:

```
1. Create one state called "Jump"
2. Drag your jump animation into it: Humanoid@RunJumpToApexLeftFootUnarmed_RM.fbx
3. Set transitions:
   - Any State → Jump (Condition: Jump trigger)
   - Jump → Ground Movement (Exit Time: 0.9)
```

That's it! The whole jump plays as one animation.

### Option B: Advanced 3-Phase Jump (Only if Needed)
This is what the original guide described - use ONLY if you want:
- Character to hover in air longer
- Different fall speeds
- More control

```
Jump Start (takeoff) → Jump Loop (floating) → Jump Land (landing)
```

## For PROTOFACTOR Animations:

Since your jump animation (RunJumpToApexLeftFootUnarmed_RM) is one complete animation:
- **Use Option A** - Much simpler!
- The animation already has takeoff and landing built in
- No need to split it up

## Quick Setup:
1. In Animator, create state "Jump"
2. Put the PROTOFACTOR jump animation in it
3. Don't check "Loop" on this animation
4. Two transitions:
   - IN: From Any State when Jump triggers
   - OUT: To Ground Movement when animation ends

## Why the Guide Had 3 States:
- Some games split jump for more control
- You don't need this complexity
- One jump state works perfectly fine!

Start simple. You can always add complexity later if needed.