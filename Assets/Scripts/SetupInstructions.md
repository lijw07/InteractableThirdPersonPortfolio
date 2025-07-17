# Character Switching System Setup

## Required Setup Steps

1. **Install Input System Package**
   - Open Package Manager (Window > Package Manager)
   - Search for "Input System" and install it
   - Restart Unity when prompted

2. **Scene Setup**
   - Create an empty GameObject named "GameManager"
   - Add the `CharacterSwitcher` component to it
   - Add the `PlayerInputActions` component to it

3. **Character Prefabs**
   - Ensure each character prefab has:
     - CharacterController component
     - ThirdPersonController script
     - Animator component (optional)
   - Add all character prefabs to the CharacterSwitcher's "Character Prefabs" list

4. **Camera Setup**
   - Create a Camera GameObject
   - Add the `ThirdPersonCamera` script
   - Tag it as "MainCamera"
   - Assign it to CharacterSwitcher's "Camera Rig" field

5. **Controls**
   - **T** - Switch between characters
   - **WASD** - Move
   - **Mouse** - Look around
   - **Space** - Jump
   - **Left Shift** - Sprint
   - **Left Click** - Attack (punch if unarmed)
   - **Right Click** - Block (with weapon) or Aim/Focus (without weapon)

## Script Components

- **CharacterSwitcher**: Manages character switching with T key
- **PlayerInputActions**: Handles all input using new Input System
- **ThirdPersonController**: Character movement and controls
- **ThirdPersonCamera**: Third-person camera following with aim mode
- **CombatController**: Handles attack, block, and aim mechanics
- **WeaponController**: Detects and manages equipped weapons
- **WeaponDamage**: Handles damage dealing for weapons

## Combat System Setup

1. **Weapon Setup**
   - Tag weapons with "Weapon" tag or name them with weapon keywords
   - Add a Collider component to weapons (set as Trigger)
   - WeaponController auto-detects weapons in character hierarchy

2. **Animation Parameters**
   - "Attack" (Trigger) - For attack animations
   - "Block" (Bool) - For blocking stance
   - "Speed" (Float) - Movement speed multiplier
   - "IsMoving" (Bool) - Character movement state

## Tips
- Adjust camera offset in ThirdPersonCamera for better view
- Set different walk/run speeds per character in ThirdPersonController
- Configure aim offset for better over-shoulder camera when aiming
- Weapon detection works with common names: sword, axe, spear, staff
- Combat animations should match the animation parameter names