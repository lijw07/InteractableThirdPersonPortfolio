# Finding Model Settings in Unity

## For Character Models/Prefabs:

1. **In Project Window**
   - Navigate to your character location: `Assets/Prefabs/Character/`
   - Look for the SOURCE model file (usually .fbx or .obj file)
   - The prefab (.prefab) doesn't have model settings - you need the original 3D model file

2. **Finding the Original Model**
   - Your prefabs (Man_Rooster_Easter.prefab, etc.) are instances of 3D models
   - Look for folders like:
     - `Assets/Models/Characters/`
     - `Assets/Characters/`
     - Or within the same folder, look for .fbx files

3. **Once You Find the Model File (.fbx)**
   - Click on the .fbx file in Project window
   - Look at the Inspector panel
   - You'll see tabs: **Model | Rig | Animation | Materials**
   - Click on **Rig** tab
   - Change Animation Type to **Humanoid**

## If You Can't Find the Model Files:

Your characters might be from an asset pack. Check:
- The original asset folder you imported
- Common locations like `Assets/[AssetPackName]/Models/`

## Alternative Method - Check Prefab Source:

1. Click on your prefab (e.g., Man_Rooster_Easter.prefab)
2. In Inspector, look at the Mesh Renderer component
3. Find the "Mesh" field - click the circle selector
4. This shows which mesh it uses
5. Right-click that mesh > "Show in Project"
6. This takes you near the source model file

## Quick Visual Guide:
```
Wrong ❌: Man_Rooster_Easter.prefab (no model settings here)
Right ✅: Man_Rooster_Easter.fbx (this has the Model/Rig tabs)
```

The .prefab is just a configured instance - the .fbx (or other 3D format) is where you change rig settings!