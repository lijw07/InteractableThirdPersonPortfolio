# Sharing Animations Between Multiple Characters

## Option 1: Humanoid Rig (Recommended)

If your characters are humanoid (human-like with standard bone structure):

### Setup Steps:
1. **Configure Character Models**
   - Select each character model in Project
   - In Inspector > Rig tab:
     - Animation Type: **Humanoid**
     - Avatar Definition: **Create From This Model**
     - Click Apply

2. **Create One Master Animator Controller**
   - Create it once with all states and parameters
   - Use generic animations (from Mixamo, Unity Store, etc.)
   - Assign this same controller to all characters

3. **Benefits**
   - One set of animations works on ALL humanoid characters
   - Can use free animations from Mixamo
   - Animations automatically retarget to different body proportions
   - Only need to set up the Animator Controller once

## Option 2: Animation Override Controllers

For more customization per character:

### Setup Steps:
1. **Create Base Animator Controller**
   - Set up all states and transitions once

2. **Create Override Controllers**
   - Right-click in Project > Create > Animator Override Controller
   - Name it "Character1_Override", "Character2_Override", etc.
   - Assign the base controller to "Controller" field
   - Override only specific animations if needed

3. **Usage**
   ```
   Base Controller (shared logic)
   ├── Character1_Override (uses mostly base animations)
   ├── Character2_Override (custom attack animation)
   └── Character3_Override (custom idle pose)
   ```

## Option 3: Generic Rig with Same Skeleton

If characters share the same bone structure but aren't humanoid:

1. Set Animation Type to **Generic**
2. Ensure all characters have identical bone names
3. Animations will work across all characters

## Recommended Workflow for 8 Characters

1. **Use Humanoid Rig** for all characters
2. **Get Free Animations** from:
   - Mixamo.com (free with Adobe account)
   - Unity Asset Store (free packs)
   - Kevin Iglesias Basic Motions (free)

3. **Create One Animator Controller**
   - Set it up completely with the animation guide
   - Test on one character
   - Apply to all 8 characters

4. **Character-Specific Tweaks** (optional):
   - Use Override Controllers only if a character needs unique animations
   - Most characters can share everything

## Quick Mixamo Workflow

1. Go to Mixamo.com
2. Search for animations:
   - "Idle"
   - "Walking" 
   - "Running"
   - "Jump"
   - "Punch" or "Sword Attack"
   - "Block"
3. Download as:
   - Format: FBX for Unity
   - Skin: Without Skin
   - Frames: 30fps
4. Import to Unity
5. Set to Humanoid rig
6. Drag into Animator States

## Script Configuration

The CharacterSwitcher already handles this! Each character just needs:
- Same component setup (ThirdPersonController, etc.)
- Animator component pointing to the shared controller
- That's it!

## Example Setup Time

- Creating animations for 8 characters: 40+ hours
- Using shared humanoid animations: 1-2 hours

The scripts are already set up to work with any Animator configuration!