# Third Person Character Controller System

A modular third-person character controller system for Unity with smooth movement, animation synchronization, and character switching capabilities.

## Table of Contents
- [Features](#features)
- [Setup Instructions](#setup-instructions)
- [Controls](#controls)
- [Component Overview](#component-overview)
- [Animation Setup](#animation-setup)
- [Character Setup](#character-setup)
- [Customization](#customization)
- [Troubleshooting](#troubleshooting)

## Features

- **Smooth Character Movement**: Walk, run, and sprint with acceleration/deceleration
- **Dynamic Camera System**: Third-person camera with mouse control and zoom
- **Character Switching**: Switch between multiple characters seamlessly
- **Animation Synchronization**: Animations perfectly sync with movement speed
- **Input System**: Modern Unity Input System integration
- **Modular Design**: Easy to extend and customize

### New Enhanced Features
- **Movement States**: Idle, Starting, Walking, Running, Sprinting, Stopping, TurningInPlace
- **Advanced Physics**: Ground detection with slope handling and step-up capability
- **Momentum System**: Preserves momentum during direction changes for realistic movement
- **Input Buffering**: Captures rapid inputs for more responsive controls
- **Acceleration Curves**: Customizable ease-in/ease-out curves for natural movement
- **Debug Tools**: Real-time parameter tuning and visual debugging
- **Sticky Sprint**: Optional toggle mode for sprint in addition to hold-to-sprint

## Setup Instructions

### 1. Scene Hierarchy Setup

Create the following GameObject hierarchy:

```
PlayerController (Empty GameObject)
├── PlayerController.cs
├── PlayerAnimationController.cs
├── CharacterManageController.cs
├── PlayerInputActions.cs
└── Characters (Children)
    ├── Character1 (with CharacterController & Animator)
    ├── Character2 (with CharacterController & Animator)
    └── Character3 (with CharacterController & Animator)
```

### 2. Component Configuration

#### PlayerController GameObject:
1. Add all four controller scripts:
   - `PlayerController.cs`
   - `PlayerAnimationController.cs`
   - `CharacterManageController.cs`
   - `PlayerInputActions.cs`

2. Configure PlayerController settings:
   - **Walk Speed**: 5 (recommended)
   - **Run Speed**: 10 (recommended)
   - **Turn Smooth Time**: 0.1
   - **Acceleration Time**: 0.25
   - **Deceleration Time**: 0.15

#### Camera Setup:
1. Create a Camera GameObject with `ThirdPersonCamera.cs`
2. Set the target to the PlayerController GameObject
3. Configure camera settings:
   - **Distance**: 10
   - **Mouse Sensitivity**: 100
   - **Min Zoom Distance**: 5
   - **Max Zoom Distance**: 20
   - **Target Height Offset**: 1.2

### 3. Character Model Setup

For each character model:

1. **Add Components**:
   - CharacterController component
   - Animator component
   - Character model mesh

2. **CharacterController Settings**:
   - **Height**: Match character height
   - **Radius**: 0.5 (typical)
   - **Center**: Adjust to character center

3. **Animator Setup**:
   - Create an Animator Controller
   - Set up animation parameters (see Animation Setup section)

## Controls

| Action | Key/Input |
|--------|-----------|
| Move | WASD |
| Look Around | Mouse |
| Sprint | Left Shift (Hold) |
| Walk Toggle | C |
| Switch Character Next | T |
| Switch Character Previous | Alt + T |
| Zoom Camera | Mouse Scroll |
| Unlock Cursor | ESC |

## Component Overview

### PlayerController.cs
Handles character movement physics and input processing.

**Key Parameters**:
- `walkSpeed`: Base walking speed
- `runSpeed`: Maximum running speed
- `turnSmoothTime`: Rotation smoothing
- `accelerationTime`: Time to reach max speed
- `inputSmoothTime`: Input dampening

### EnhancedPlayerController.cs (New)
Advanced movement controller with state management and enhanced physics.

**Key Parameters**:
- `idleToWalkDelay`: Delay before starting movement (realism)
- `momentumPreservation`: How much momentum to keep when changing directions
- `accelerationCurve`: Custom curve for acceleration feel
- `decelerationCurve`: Custom curve for deceleration feel
- `groundCheckDistance`: Distance for ground detection
- `slopeLimit`: Maximum walkable slope angle
- `enableStickySpring`: Toggle between hold and toggle sprint

### PlayerAnimationController.cs
Synchronizes animations with movement states.

**Key Parameters**:
- `directionSmoothTime`: Directional blend smoothing
- `animationDeadzone`: Minimum input threshold

### CharacterManageController.cs
Manages character switching and initialization.

**Key Parameters**:
- `startingCharacterIndex`: Initial character (0-based)
- `switchCooldown`: Time between character switches

### ThirdPersonCamera.cs
Controls camera movement and positioning.

**Key Parameters**:
- `distance`: Camera distance from character
- `mouseSensitivity`: Look sensitivity
- `targetHeightOffset`: Vertical offset for camera focus

## Animation Setup

### Required Animator Parameters

Create these parameters in your Animator Controller:

| Parameter | Type | Description |
|-----------|------|-------------|
| MovementSpeed | Float | 0=Idle, 0.5=Walk, 1=Run, 2=Sprint |
| Horizontal | Float | -1 to 1 strafe input |
| Vertical | Float | -1 to 1 forward/back input |
| IsGrounded | Bool | Ground detection |
| VerticalVelocity | Float | For jump/fall detection |
| Jump | Trigger | Jump animation trigger |
| MovementState | Integer | Current movement state (0-6) |
| TurningInPlace | Bool | Character turning without moving |
| StartMoving | Trigger | Idle to movement transition |
| Stopping | Trigger | Movement to idle transition |

### Animation State Setup

1. **Create Blend Tree** for movement:
   - Idle (0 speed)
   - Walk (0.5 speed)
   - Run (1.0 speed)
   - Sprint (2.0 speed)

2. **Directional Blending** (optional):
   - Use 2D Freeform Directional blend tree
   - Map Horizontal/Vertical parameters
   - Add directional walk/run animations

3. **Transition Settings**:
   - Has Exit Time: Off
   - Transition Duration: 0.1-0.25
   - Use MovementSpeed conditions

## Character Setup

### Adding New Characters

1. Create character model as child of PlayerController
2. Add CharacterController component
3. Add Animator component with configured controller
4. Ensure character starts inactive (will be managed by system)
5. Position at local origin (0,0,0)

### Character Requirements

- Must have CharacterController component
- Must have Animator component (optional but recommended)
- Should be direct children of PlayerController GameObject
- Models should face forward (+Z direction)

## Customization

### Movement Feel Adjustments

**For Responsive Movement**:
- Decrease `accelerationTime` (0.15-0.2)
- Decrease `turnSmoothTime` (0.05-0.1)
- Increase `stationaryTurnSpeed` (200-360)

**For Realistic Movement**:
- Increase `accelerationTime` (0.3-0.5)
- Increase `turnSmoothTime` (0.15-0.25)
- Add momentum curves to `turnSpeedCurve`

### Animation Timing

Adjust in PlayerAnimationController:
- `directionSmoothTime`: Controls animation direction blending
- Smooth time in line 100: Controls speed transition smoothing

### Camera Feel

**For Tighter Camera**:
- Increase `mouseSensitivity` (150-200)
- Decrease camera position smoothing

**For Cinematic Camera**:
- Add `rotationSmoothTime` (0.15-0.3)
- Add `positionSmoothTime` (0.1-0.2)

## Troubleshooting

### Common Issues

**Characters Not Moving**
- Check CharacterController is properly configured
- Verify Input System is enabled in Project Settings
- Ensure character is active and at correct position

**Animation Not Playing**
- Verify Animator Controller is assigned
- Check animation parameter names match exactly
- Ensure animation clips are properly imported

**Camera Jitter**
- Camera should update in LateUpdate()
- Check for conflicting camera scripts
- Verify smooth rotation values

**Character Switching Issues**
- All characters must be children of PlayerController
- Characters must have CharacterController component
- Check console for error messages

### Performance Optimization

1. **Reduce Update Calls**:
   - Cache component references
   - Use object pooling for effects

2. **Animation Optimization**:
   - Use Avatar Masks for partial animations
   - Optimize animation compression settings

3. **LOD System**:
   - Disable animations on distant characters
   - Reduce update frequency for non-active characters

## Debug Mode

### Debug Overlay (F1)
Press F1 to toggle the debug overlay showing:
- Current movement state and speed
- Animation parameters
- Input values
- Ground detection status

### Runtime Parameter Tuner (F2)
Press F2 to open the parameter tuner for real-time adjustments:
- Movement speeds and acceleration
- Camera sensitivity and distance
- Animation timing
- Save/Load presets

### Visual Debug Gizmos
Enable in EnhancedPlayerController inspector:
- Movement direction visualization
- Ground detection sphere
- Momentum vectors
- State information

### Testing
Add MovementSystemTest.cs component and enable "Run Tests" to:
- Test all movement states
- Verify animation synchronization
- Check input buffering
- Validate physics systems

## Extensions

This system is designed to be extended. Consider adding:
- Jump/Crouch mechanics
- Combat system
- Interaction system
- Inventory management
- Save/Load functionality

For questions or issues, check the console for error messages and ensure all components are properly configured.