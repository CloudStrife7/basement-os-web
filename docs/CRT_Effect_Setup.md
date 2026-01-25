# CRT Effect Setup Guide

> **Beginner-Friendly Guide**: Step-by-step instructions to apply retro CRT terminal effects to your VRChat Lower Level 2.0 terminal or TV displays.

---

## Table of Contents

1. [What You'll Need](#what-youll-need)
2. [Quick Start (5 Steps)](#quick-start-5-steps)
3. [Choosing Quest vs PC Shader](#choosing-quest-vs-pc-shader)
4. [Detailed Setup Instructions](#detailed-setup-instructions)
5. [Parameter Reference](#parameter-reference)
6. [Troubleshooting](#troubleshooting)
7. [Applying to TV Displays (Bonus)](#applying-to-tv-displays-bonus)

---

## What You'll Need

- Unity 2019.4.31f1 or newer
- VRChat SDK3 (Worlds)
- TextMeshPro (included with Unity)
- The CRT shader files (included in this project):
  - `Assets/Shaders/TerminalCRT_Quest.shader` (Quest-compatible)
  - `Assets/Shaders/TerminalCRT_PC.shader` (PC enhanced)
  - `Assets/Materials/TerminalCRT_Quest.mat` (Quest material)
  - `Assets/Materials/TerminalCRT_PC.mat` (PC material)

---

## Quick Start (5 Steps)

### For Quest Users (Standalone):

1. **Open** the Unity scene containing your terminal
2. **Find** the TextMeshPro component displaying the terminal text (usually named "TerminalScreen")
3. **Click** on the TextMeshPro component in the Inspector
4. **Drag** `Assets/Materials/TerminalCRT_Quest.mat` to the **Material** field
5. **Press Play** in Unity to see the CRT effect

### For PC VR Users (PCVR/Link):

1. **Open** the Unity scene containing your terminal
2. **Find** the TextMeshPro component displaying the terminal text
3. **Click** on the TextMeshPro component in the Inspector
4. **Drag** `Assets/Materials/TerminalCRT_PC.mat` to the **Material** field
5. **Press Play** in Unity to see the enhanced CRT effect

**That's it!** The effect should now be visible on your terminal.

---

## Choosing Quest vs PC Shader

| Feature | Quest Shader | PC Shader |
|---------|-------------|-----------|
| **Scanlines** | ✅ Yes | ✅ Yes (higher quality) |
| **Phosphor Glow** | ✅ Simplified | ✅ Enhanced with bloom |
| **Screen Curvature** | ✅ Yes | ✅ Yes (more pronounced) |
| **Flicker** | ✅ Basic | ✅ Multi-frequency |
| **Vignette** | ✅ Yes | ✅ Yes |
| **Chromatic Aberration** | ❌ No | ✅ Yes |
| **Pixel Grid** | ❌ No | ✅ Yes (CRT phosphor dots) |
| **Performance** | Optimized (<5ms GPU) | Higher quality (5-10ms GPU) |
| **Use Case** | Quest 2/3, Mobile VR | PCVR, Oculus Link, Steam |

**Recommendation:**
- Building for **Quest standalone**? Use `TerminalCRT_Quest.mat`
- Building for **PC VR only**? Use `TerminalCRT_PC.mat`
- Building for **both platforms**? Use Quest shader and enable PC-specific effects via platform detection script (advanced)

---

## Detailed Setup Instructions

### Step 1: Locate Your Terminal TextMeshPro Object

1. In the Unity **Hierarchy** window, expand your scene
2. Find the GameObject with the TextMeshPro - Text (UI) component
   - In Lower Level 2.0, this is typically under: `DT_Core > Canvas > TerminalScreen`
3. Click on the object to select it
4. The **Inspector** window will show the TextMeshPro component

**Screenshot Reference:**
```
Hierarchy:
  └─ DT_Core
      └─ Canvas
          └─ TerminalScreen ← Select this
```

### Step 2: Apply the CRT Material

1. In the Inspector, find the **TextMeshPro - Text (UI)** component
2. Scroll down to find the **Main Settings** section
3. Locate the **Material** field (it currently shows a default TMP material)
4. **Drag and drop** the appropriate material:
   - **Quest**: `Assets/Materials/TerminalCRT_Quest.mat`
   - **PC**: `Assets/Materials/TerminalCRT_PC.mat`

**Before:**
```
Material: LiberationSans SDF Material
```

**After:**
```
Material: TerminalCRT_Quest (or TerminalCRT_PC)
```

### Step 3: Test in Play Mode

1. Press the **Play** button at the top of Unity Editor
2. Observe your terminal - you should see:
   - Horizontal scanlines moving across the screen
   - Subtle green phosphor glow around bright text
   - Slight screen curvature at the edges
   - Occasional flicker effect
   - Darkened edges (vignette)

3. If you don't see effects, check [Troubleshooting](#troubleshooting)

### Step 4: Adjust Parameters (Optional)

1. **Stop Play Mode** (press Play button again)
2. In the **Project** window, navigate to the material you applied:
   - `Assets/Materials/TerminalCRT_Quest.mat` or
   - `Assets/Materials/TerminalCRT_PC.mat`
3. Click on the material to view it in the **Inspector**
4. Adjust the sliders under **CRT Effects** (see [Parameter Reference](#parameter-reference))
5. Press **Play** to test your changes

### Step 5: Build and Upload to VRChat

1. Follow the normal VRChat world build process:
   - **File > Build Settings**
   - Select **Android** (Quest) or **PC, Mac & Linux Standalone** (PCVR)
   - Click **Switch Platform** if needed
   - Click **Build**

2. Upload to VRChat using the **VRChat SDK > Build & Publish** menu

**Important:** Make sure you've applied the correct material for your target platform!

---

## Parameter Reference

### Quest Shader Parameters

Click on `TerminalCRT_Quest.mat` in the Project window to edit these values:

| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Enable CRT Effects** | On/Off | On | Master toggle - turns all effects on/off |
| **Scanline Intensity** | 0 to 1 | 0.15 | How dark the scanlines are (0 = invisible, 1 = very dark) |
| **Scanline Count** | 100 to 1000 | 480 | Number of horizontal lines (higher = more lines) |
| **Scanline Speed** | 0 to 5 | 1.0 | How fast scanlines scroll (0 = static, 5 = fast) |
| **Phosphor Glow** | 0 to 1 | 0.2 | Intensity of green glow around text (0 = none, 1 = strong) |
| **Glow Tint** | Color | Green | Color of the phosphor glow effect |
| **Screen Curvature** | 0 to 0.1 | 0.02 | Amount of barrel distortion (0 = flat, 0.1 = very curved) |
| **Flicker Intensity** | 0 to 1 | 0.05 | How much the screen flickers (0 = none, 1 = strong) |
| **Flicker Speed** | 0 to 10 | 5.0 | How fast the flicker oscillates |
| **Vignette Strength** | 0 to 1 | 0.3 | How dark the edges are (0 = none, 1 = very dark) |
| **Vignette Size** | 0.1 to 1 | 0.5 | Size of the bright center area (smaller = bigger vignette) |

### PC Shader Parameters (Additional)

The PC shader includes all Quest parameters PLUS:

| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Scanline Thickness** | 0.1 to 1 | 0.5 | Thickness of scanlines (lower = thinner, sharper) |
| **Bloom Radius** | 0 to 0.01 | 0.002 | Size of the bloom effect around bright pixels |
| **Chromatic Aberration** | 0 to 0.01 | 0.002 | RGB color separation at screen edges (0 = none) |
| **Pixel Grid Intensity** | 0 to 1 | 0.1 | Visibility of CRT phosphor dot grid (0 = none) |
| **Pixel Grid Scale** | 100 to 2000 | 800 | Density of the pixel grid pattern |

---

## Troubleshooting

### Problem: No CRT effects visible

**Solutions:**
1. **Check the toggle**: Make sure "Enable CRT Effects" is turned ON in the material Inspector
2. **Verify material**: Confirm the TextMeshPro component has the CRT material applied (not the default)
3. **Check Play Mode**: Effects only appear in Play Mode or in VRChat - they won't show in Edit Mode
4. **Intensity too low**: Try increasing "Scanline Intensity" to 0.5 to make effects more obvious

### Problem: Screen is completely black (Quest)

**Solutions:**
1. **Curvature too high**: Set "Screen Curvature" to 0 temporarily, then slowly increase
2. **Shader compilation failed**: Check Unity Console (Ctrl+Shift+C) for errors
3. **Wrong shader model**: Quest requires Shader Model 3.0 - make sure you're using `TerminalCRT_Quest.shader`

### Problem: Text colors look wrong

**Solutions:**
1. **Glow Color**: The default green glow tint is intentional for retro CRT aesthetic
2. **Change glow color**: Click the "Glow Tint" color picker and adjust to match your theme
3. **Reduce glow**: Lower "Phosphor Glow" to 0.1 or 0 to preserve original text colors

### Problem: Poor performance on Quest

**Solutions:**
1. **Use Quest shader**: Make sure you're using `TerminalCRT_Quest.mat`, not the PC version
2. **Reduce scanline count**: Lower "Scanline Count" from 480 to 240
3. **Disable heavy effects**: Set "Flicker Intensity" and "Phosphor Glow" to 0
4. **Profile GPU**: Use Unity Profiler to check if CRT shader is the bottleneck

### Problem: Effects look different on Quest vs PC

**Expected behavior**: Quest shader is optimized and intentionally has fewer effects than PC

**If they look too different:**
1. Match the common parameters (scanline intensity, curvature, vignette)
2. On PC, disable "Chromatic Aberration" and "Pixel Grid Intensity" to match Quest look

### Problem: Shader compilation errors

**Common errors:**

| Error | Solution |
|-------|----------|
| `undeclared identifier` | Make sure you copied the ENTIRE shader file |
| `target 3.0 not supported` | Quest requires Shader Model 3.0 - check platform is Android |
| `syntax error` | Check for missing semicolons or braces |

**How to check:**
1. Open **Console** window (Window > General > Console)
2. Look for red error messages mentioning the shader
3. Double-click the error to open the shader at the problematic line

---

## Applying to TV Displays (Bonus)

The CRT shader works great on TV prefabs too!

### Steps:

1. Find the TV GameObject in your scene hierarchy
2. Locate the **Quad** or **Mesh** that displays the TV screen
3. The TV likely has a **MeshRenderer** component with a **Material**
4. Create a new material instance:
   - Right-click in Project window
   - **Create > Material**
   - Name it `TV_CRT`
5. In the new material's Inspector:
   - Set **Shader** to `LL2/Terminal/CRT_Quest` (or PC)
   - Adjust parameters for TV aesthetic (try higher curvature: 0.05)
6. Apply the material to the TV's MeshRenderer

### Recommended TV Settings:

```
Scanline Intensity: 0.25 (more pronounced)
Screen Curvature: 0.05 (TVs had more curve)
Vignette Strength: 0.5 (stronger edge darkening)
Glow Tint: RGB (0.2, 0.5, 1.0) - Bluish for broadcast TV
```

---

## Tips & Best Practices

### Performance Optimization

- **Quest builds**: Always use the Quest shader variant
- **Test on device**: Unity Editor performance != Quest 2 performance
- **Profile first**: Use Unity Profiler before assuming shader is the issue
- **Batch wisely**: Multiple terminals with same material = better batching

### Visual Tuning

- **Less is more**: Start with default values, adjust slowly
- **Match your theme**: Change "Glow Tint" to match terminal color scheme
  - Green terminal: RGB (0.1, 1.0, 0.3) - Default
  - Amber terminal: RGB (1.0, 0.7, 0.0)
  - White terminal: RGB (0.5, 0.5, 0.5)
  - Cyan terminal: RGB (0.0, 1.0, 1.0)

- **Nostalgia level**: Adjust parameters based on desired era
  - **1970s mainframe**: High scanlines (800), low glow (0.1), strong vignette
  - **1980s DOS**: Medium scanlines (480), medium glow (0.2), some curvature
  - **1990s monitor**: Low scanlines (240), subtle glow (0.1), minimal curvature

### Accessibility

- **Flicker warning**: High flicker can cause discomfort
  - Keep **Flicker Intensity** below 0.1 for most users
  - Provide an option to disable effects entirely

- **Readability**: Ensure text remains readable
  - Don't exceed 0.3 on **Scanline Intensity**
  - Keep **Screen Curvature** below 0.04

---

## Advanced: Platform-Specific Materials via Script

If you want to automatically use Quest shader on Quest and PC shader on PCVR:

```csharp
using UnityEngine;
using TMPro;

public class PlatformCRTMaterial : MonoBehaviour
{
    [SerializeField] private Material questMaterial;
    [SerializeField] private Material pcMaterial;

    void Start()
    {
        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            #if UNITY_ANDROID
                tmp.fontMaterial = questMaterial;
            #else
                tmp.fontMaterial = pcMaterial;
            #endif
        }
    }
}
```

**Note:** This requires UdonSharp adaptation for VRChat - consult `/udonsharp-dev` skill for proper implementation.

---

## References

- **Inspiration**: [cool-retro-term](https://github.com/Swordfish90/cool-retro-term) - QML terminal emulator
- **WebGL Implementation**: [remojansen.github.io](https://github.com/CloudStrife7/remojansen.github.io) - Three.js CRT effects
- **Technical Article**: [Building a Retro CRT Terminal Website](https://dev.to/remojansen/building-a-retro-crt-terminal-website-with-webgl-and-github-copilot-claude-opus-35-3jfd)
- **Related Issue**: [#145 - Add LCD Screen Filter](https://github.com/CloudStrife7/LL2PCVR/issues/145)

---

## Success Checklist

After following this guide, you should have:

- ✅ CRT scanline effect visible on terminal screen
- ✅ Phosphor glow enhances retro aesthetic without obscuring text
- ✅ Effects work on your target platform (PC or Quest)
- ✅ Text remains readable with emerald green theme (#10B981) preserved
- ✅ No significant performance impact (<5ms GPU on Quest 2)
- ✅ Optional: TV prefabs have CRT effect applied

---

**Need Help?**

- Check the [Troubleshooting](#troubleshooting) section
- Review Unity Console for shader errors
- Test with default material parameters first
- Consult Lower Level 2.0 Discord or GitHub Issues

---

*Last Updated: January 2026*
*Issue: #295 - Retro CRT Terminal Effects*
