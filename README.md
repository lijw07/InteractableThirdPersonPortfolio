# Interactable Third Person Portfolio

A Unity project showcasing a third-person character controller with combat mechanics, weapon switching, and interactive gameplay features.

## Overview

This portfolio project demonstrates advanced Unity development skills including character controllers, combat systems, camera management, and interactive gameplay mechanics. Built with Unity's Universal Render Pipeline (URP) and the new Input System for optimal performance and cross-platform compatibility.

## Features

### Core Gameplay
- **Advanced Third Person Controller**: Smooth movement with walk/run states, jumping, and sprinting
- **Dynamic Camera System**: Intelligent third-person camera with collision detection and smooth follow
- **Combat System**: Attack and blocking mechanics with animation integration and cooldowns
- **Weapon System**: Dynamic weapon switching and management with damage calculation
- **Character Switching**: Seamlessly switch between multiple playable characters
- **Input System Integration**: Full support for keyboard/mouse and gamepad controls

### Technical Features
- Unity's new Input System for flexible control schemes
- Optimized for both PC and Mobile platforms with separate render pipelines
- Modular component-based architecture
- Ground detection system with configurable parameters
- Animation-driven combat with proper timing windows

## Technologies

- **Unity 2022.3** (6000.0.33f1)
- **Universal Render Pipeline (URP)** for modern rendering
- **C# Scripting** with component-based architecture
- **Unity Input System** for cross-platform controls
- **Animator Controller** for smooth character animations

## Project Structure

```
Assets/
├── Scripts/                          # Core gameplay scripts
│   ├── ThirdPersonController.cs     # Character movement and physics
│   ├── ThirdPersonCamera.cs         # Camera follow and collision
│   ├── CombatController.cs          # Combat mechanics and animations
│   ├── WeaponController.cs          # Weapon management system
│   ├── WeaponDamage.cs             # Damage calculation
│   ├── CharacterSwitcher.cs        # Character switching logic
│   └── PlayerInputActions.cs       # Input handling
├── Scenes/
│   └── SampleScene.unity           # Main gameplay scene
├── Settings/                       # Project configuration
│   ├── PC_RPAsset.asset           # PC render settings
│   ├── Mobile_RPAsset.asset       # Mobile render settings
│   └── URPGlobalSettings.asset    # Global URP settings
└── InputSystem_Actions.inputactions # Input configuration file
```

## Controls

### Keyboard & Mouse
- **Movement**: WASD
- **Camera**: Mouse movement
- **Jump**: Space
- **Sprint**: Left Shift (hold)
- **Attack**: Left Mouse Button
- **Block**: Right Mouse Button (hold)
- **Switch Character**: Tab
- **Aim Mode**: Right Mouse Button (hold while ranged weapon equipped)

### Gamepad
- **Movement**: Left Stick
- **Camera**: Right Stick
- **Jump**: South Button (A/X)
- **Sprint**: Left Trigger
- **Attack**: West Button (X/Square)
- **Block**: East Button (B/Circle)
- **Switch Character**: North Button (Y/Triangle)

## Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone [repository-url]
   ```

2. **Open in Unity**
   - Use Unity 2022.3 or later (6000.0.33f1 recommended)
   - Unity will automatically import project settings

3. **Import Required Third-Party Assets**
   The following assets need to be imported separately:
   - Hot Reload (SingularityGroup)
   - vInspector, vFolders 2, vHierarchy 2 (VioloStudio)
   - Odin Inspector (Sirenix) - Optional
   - Required pathfinding and world generation tools

4. **Configure Project Settings**
   - Ensure URP is selected as the render pipeline
   - Verify Input System package is active
   - Set up layers for ground detection

5. **Open Sample Scene**
   - Navigate to Assets/Scenes/SampleScene.unity
   - Press Play to test

## Component Overview

### ThirdPersonController
Main character controller handling:
- Movement input processing
- Physics-based movement with CharacterController
- Jump mechanics with gravity
- Sprint toggle system
- Ground detection
- Smooth rotation with camera-relative movement

### CombatController
Manages all combat-related functionality:
- Attack combo system
- Block mechanics with timing
- Weapon integration
- Animation triggers
- Combat cooldowns
- Aim mode for ranged weapons

### WeaponController
Handles weapon management:
- Weapon inventory system
- Weapon switching logic
- Weapon-specific behaviors
- Integration with combat system

### ThirdPersonCamera
Intelligent camera system featuring:
- Smooth follow behavior
- Collision detection and avoidance
- Adjustable distance and angles
- Combat mode adjustments

## Building the Project

### For PC
1. File > Build Settings
2. Select "Windows, Mac, Linux" platform
3. Switch to PC_RPAsset in Graphics settings
4. Configure Player Settings
5. Build

### For Mobile
1. File > Build Settings
2. Select "Android" or "iOS" platform
3. Switch to Mobile_RPAsset in Graphics settings
4. Configure mobile-specific Player Settings
5. Build

### For WebGL
1. File > Build Settings
2. Select "WebGL" platform
3. Configure compression settings
4. Enable exceptions handling if needed
5. Build and deploy to web server

## Performance Optimization

- LOD groups for complex models
- Occlusion culling enabled
- Optimized texture sizes
- Mobile-specific quality settings
- Efficient animation controllers

## Development Notes

- Uses modular component design for easy extension
- All third-party assets excluded from version control
- Separate render pipelines for PC and Mobile
- Ground check automatically configured if missing
- Animation events used for combat timing

## Future Enhancements

- [ ] Multiplayer support
- [ ] Additional weapon types
- [ ] Skill system
- [ ] Enemy AI integration
- [ ] Inventory system
- [ ] Save/Load functionality

## Troubleshooting

**Character won't move**: Check if Input System is active in Project Settings
**Camera issues**: Ensure MainCamera tag is set on camera object
**Combat not working**: Verify animation clips are assigned in Animator
**Missing textures**: Re-import third-party assets

## License

This project is for portfolio demonstration purposes. All code written by the developer is available for review. Third-party assets are subject to their respective licenses and are not included in the repository.

## Contact

[Add your contact information here]

---

*This project showcases proficiency in Unity development, C# programming, and game mechanics implementation.*