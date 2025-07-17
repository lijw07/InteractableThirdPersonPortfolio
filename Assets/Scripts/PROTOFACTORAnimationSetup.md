# PROTOFACTOR Animation Setup Guide

You have the PROTOFACTOR Ultimate Animation Collection with organized animation sets. Here's how to set up these animations with your 8 character prefabs:

## Found Animation Sets:
- Basic Locomotion Animset (unarmed movement)
- 1Handed Melee Weapon Animset
- 2Handed Melee Weapon Animset
- Combat Bare Fists Animset
- And many more weapon-specific sets

## Step 1: Prepare Character Prefabs

For each character (Man_Rooster_Easter, Woman_Rabbit_Easter, etc.):

1. **Set to Humanoid Rig**
   - Select character model in Project
   - Inspector > Model tab > Rig
   - Animation Type: **Humanoid**
   - Avatar Definition: **Create From This Model**
   - Click Apply

## Step 2: Prepare PROTOFACTOR Animations

1. **Configure Animation Import Settings**
   - Navigate to: `Assets/PROTOFACTOR/Ultimate Animation Collection/Animations/Basic Locomotion Animset/FBX Motions/`
   - Select all .fbx files
   - Inspector > Rig tab:
     - Animation Type: **Humanoid**
     - Avatar Definition: **Copy From Other Avatar** (optional)
   - Inspector > Animation tab:
     - Check animations are properly named
   - Click Apply

## Step 3: Create Shared Animator Controller

1. **Create Controller**
   - Right-click in Project > Create > Animator Controller
   - Name it "SharedCharacterController"

2. **Build States Using PROTOFACTOR Animations**

### Ground Movement Blend Tree:
- **Idle**: `Humanoid@IdleUnarmed.fbx`
- **Walk**: `Humanoid@WalkForwardUnarmed2_RM.fbx`
- **Run**: `Humanoid@RunForward2Unarmed.fbx`

### Jump Animations:
- **Jump Start**: Use walk jump `Humanoid@WalkJumpToApexRightFootUnarmed.fbx` (first part)
- **Jump Loop**: Use the apex/middle part of the jump
- **Jump Land**: Use the landing part of the jump

### Combat Animations:
- **Attack**: From `Combat Bare Fists Animset` folder
- **Block**: Look for defensive animations in combat sets

## Step 4: Animation Mapping

Here are the specific animations to use:

```
IDLE ANIMATIONS:
- Basic Idle: /1Handed Melee Weapon Animset/FBX Motions/Humanoid@IdleUnarmed.fbx
- Alternative: /Basic Locomotion Animset/FBX Motions/Humanoid@IdleLookAroundScratchYawnUnarmed.fbx

MOVEMENT ANIMATIONS:
- Walk Forward: /Basic Locomotion Animset/FBX Motions/Humanoid@WalkForwardUnarmed2_RM.fbx
- Walk Right: /Basic Locomotion Animset/FBX Motions/Humanoid@WalkRightUnarmed_RM.fbx
- Walk Left: /Basic Locomotion Animset/FBX Motions/Humanoid@WalkLeftUnarmed_RM.fbx
- Walk Back: /Basic Locomotion Animset/FBX Motions/Humanoid@WalkBackwardsUnarmed_RM.fbx
- Run Forward: /Basic Locomotion Animset/FBX Motions/Humanoid@RunForward2Unarmed.fbx
- Run Right: /Basic Locomotion Animset/FBX Motions/Humanoid@RunRightUnarmed_RM.fbx
- Run Left: /Basic Locomotion Animset/FBX Motions/Humanoid@RunLeftUnarmed_RM.fbx

JUMP ANIMATIONS:
- Jump: /Basic Locomotion Animset/FBX Motions/Humanoid@RunJumpToApexLeftFootUnarmed_RM.fbx

COMBAT ANIMATIONS:
- Check: /Combat Bare Fists Animset/FBX Motions/ for punch animations
- Block animations might be in defensive poses
```

## Step 5: Extract Animation Clips

Since FBX files contain both model and animation:

1. Select the FBX file
2. In Animation tab, you'll see animation clips
3. Click on each clip to rename if needed
4. Set proper loop settings:
   - Loop: Idle, Walk, Run
   - No Loop: Jump, Attack

## Step 6: Apply to All Characters

1. Select all 8 character prefabs
2. Add components if not present:
   - Animator (assign SharedCharacterController)
   - CharacterController
   - ThirdPersonController
   - CombatController
   - WeaponController

## Quick Setup Script

Create this helper script to batch setup:

```csharp
using UnityEngine;
using UnityEditor;

public class CharacterSetupHelper : EditorWindow
{
    [MenuItem("Tools/Setup PROTOFACTOR Characters")]
    static void SetupCharacters()
    {
        string[] characterPaths = {
            "Assets/Prefabs/Character/Man_Rooster_Easter.prefab",
            "Assets/Prefabs/Character/Woman_Rabbit_Easter.prefab",
            // Add all 8 character paths
        };
        
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animation/SharedCharacterController.controller");
        
        foreach (string path in characterPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Ensure Animator
                var animator = prefab.GetComponent<Animator>();
                if (animator == null)
                    animator = prefab.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                
                // Save changes
                EditorUtility.SetDirty(prefab);
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("Characters setup complete!");
    }
}
```

## Tips:
- PROTOFACTOR animations are already Humanoid-ready
- Use "_RM" suffix animations (Root Motion) for smoother movement
- The collection has weapon-specific animations for future use
- All 8 characters can share the same Animator Controller
- Test with one character first, then apply to all