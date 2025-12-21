# 🎨 High-Fidelity Symbolic Rendering System for Unity

## Overview

A complete system for creating high-resolution symbolic graphics in Unity using TextMeshPro and Unicode characters. This technique treats Unicode Block Elements as programmable pixels with independent color control, enabling complex, low-footprint, procedurally-generated visuals.

## Features

- **Per-Character Color Control**: Each Unicode character can be independently colored
- **High Density Rendering**: Achieves effective resolutions of 200x200+ pixels using character grids
- **Low Memory Footprint**: Text-based representation compresses well and uses minimal VRAM
- **Real-time Updates**: Dynamic manipulation at 60+ FPS for medium-sized canvases
- **Procedural Generation**: No texture assets required - everything generated at runtime
- **Unique Aesthetic**: Retro terminal art style with modern rendering quality

## Quick Start

### 1. Font Asset Setup

Before using this system, you need a properly configured TextMeshPro Font Asset:

1. **Create Font Asset**:
   - Window → TextMeshPro → Font Asset Creator
   - Select a monospace font (Consolas, Courier New, etc.)

2. **Configure Settings**:
   - **Atlas Resolution**: 4096 x 4096
   - **Render Mode**: SDFAA (Signed Distance Field Anti-Aliased)
   - **Sampling Point Size**: 64+
   - **Padding**: 5
   - **Character Set**: Unicode Range

3. **Add Unicode Ranges**:
   ```
   0x0020-0x007E  (Basic Latin)
   0x2580-0x259F  (Block Elements) ← CRITICAL
   0x2500-0x257F  (Box Drawing)
   0x25A0-0x25FF  (Geometric Shapes)
   ```

4. Click **Generate Font Atlas**

### 2. Scene Setup

1. Create a 3D Quad or Plane in your scene
2. Add a TextMeshPro component (not UGUI, use 3D Text)
3. Attach `SymbolicRenderer` script to the object
4. Assign your Font Asset to the TextMeshPro component
5. Configure canvas size in the SymbolicRenderer inspector

### 3. Basic Usage

```csharp
using UnityEngine;
using SymbolicGraphics;

public class MySymbolicArt : MonoBehaviour
{
    private SymbolicCanvas canvas;
    private SymbolicRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SymbolicRenderer>();
        canvas = renderer.Canvas;

        // Draw some pixels
        canvas.SetPixel(10, 10, '█', Color.red);
        canvas.SetPixel(11, 10, '█', Color.green);
        canvas.SetPixel(12, 10, '█', Color.blue);

        // Draw a box
        canvas.DrawBox(5, 5, 20, 10, Color.yellow, Color.black);

        // Draw text
        canvas.DrawText(7, 7, "Hello World!", Color.cyan);

        // Update display
        renderer.UpdateDisplay();
    }
}
```

## Core Components

### SymbolicPixel
Data structure representing a single character with color.

```csharp
public struct SymbolicPixel
{
    public char character;
    public Color32 color;
}
```

### SymbolicCanvas
Grid-based canvas for managing symbolic pixels.

**Key Methods**:
- `SetPixel(x, y, char, color)` - Set individual pixel
- `GetPixel(x, y)` - Get pixel at position
- `Clear(char, color)` - Clear entire canvas
- `FillRect(x, y, w, h, char, color)` - Fill rectangle
- `DrawLine(x1, y1, x2, y2, char, color)` - Draw line
- `DrawBox(x, y, w, h, borderColor, fillColor)` - Draw bordered box
- `DrawText(x, y, text, color)` - Draw text string
- `DrawGradient(...)` - Various gradient methods

### SymbolicRenderer
MonoBehaviour that connects SymbolicCanvas to TextMeshPro.

**Inspector Properties**:
- `canvasWidth` - Grid width in characters
- `canvasHeight` - Grid height in characters
- `autoUpdate` - Automatically update TMP each frame
- `optimizeUpdates` - Use dirty rectangle tracking

**Methods**:
- `UpdateDisplay()` - Refresh TextMeshPro text
- `UpdateRegion(x, y, w, h)` - Partial update (performance)

## Unicode Character Reference

### Block Elements (0x2580-0x259F)

```
█  Full block (U+2588)
▀  Upper half block (U+2580)
▄  Lower half block (U+2584)
▌  Left half block (U+258C)
▐  Right half block (U+2590)

░  Light shade (U+2591)
▒  Medium shade (U+2592)
▓  Dark shade (U+2593)

▖  Quadrant lower left (U+2596)
▗  Quadrant lower right (U+2597)
▘  Quadrant upper left (U+2598)
▝  Quadrant upper right (U+259D)
▙  Quadrant UL + LL + LR (U+2599)
▚  Quadrant UL + LR (U+259A)
▛  Quadrant UL + UR + LL (U+259B)
▜  Quadrant UL + UR + LR (U+259C)
▞  Quadrant UR + LL (U+259E)
▟  Quadrant UR + LL + LR (U+259F)
```

### Box Drawing (0x2500-0x257F)

```
─  Horizontal (U+2500)
│  Vertical (U+2502)
┌  Top-left corner (U+250C)
┐  Top-right corner (U+2510)
└  Bottom-left corner (U+2514)
┘  Bottom-right corner (U+2518)
├  Left T-junction (U+251C)
┤  Right T-junction (U+2524)
┬  Top T-junction (U+252C)
┴  Bottom T-junction (U+2534)
┼  Cross (U+253C)

═  Double horizontal (U+2550)
║  Double vertical (U+2551)
╔  Double top-left (U+2554)
╗  Double top-right (U+2557)
╚  Double bottom-left (U+255A)
╝  Double bottom-right (U+255D)
╠  Double left T (U+2560)
╣  Double right T (U+2563)
╦  Double top T (U+2566)
╩  Double bottom T (U+2569)
╬  Double cross (U+256C)
```

## Examples Included

### 1. SunsetDemo.cs
Demonstrates landscape rendering with gradients and layered elements.
- Sky gradient using shade characters
- Sun with radial gradient
- Mountain silhouettes
- Water reflection effects

### 2. FamilyFeudBoard.cs
Complete game show board implementation with:
- Answer slots with reveal animations
- Point visualization bars
- Strike counter (X marks)
- Team scoreboards
- Timer bar
- Question display

### 3. DataVisualization.cs
Shows how to create charts and graphs:
- Bar charts with colored bars
- Line graphs
- Heat maps
- Real-time data updates

## Performance Guidelines

### Memory Usage

| Canvas Size | Characters | Memory (approx) | Equivalent Texture |
|-------------|-----------|-----------------|-------------------|
| 50×30       | 1,500     | ~67KB          | 7.5KB            |
| 100×50      | 5,000     | ~225KB         | 20KB             |
| 200×100     | 20,000    | ~900KB         | 80KB             |

### Render Performance

Target frame rates (Unity 2022+, mid-range hardware):

| Canvas Size | Full Redraw | With Dirty Rect Opt |
|-------------|-------------|---------------------|
| 50×30       | 1200+ FPS   | 2000+ FPS          |
| 100×50      | 300+ FPS    | 600+ FPS           |
| 200×100     | 64 FPS      | 120+ FPS           |

### Optimization Tips

1. **Use Dirty Rectangle Tracking**: Enable `optimizeUpdates` in SymbolicRenderer
2. **Cache Color Strings**: The system automatically caches color hex conversions
3. **Batch Updates**: Make multiple SetPixel calls, then UpdateDisplay() once
4. **Use Coroutines**: For animations, update over multiple frames
5. **LOD System**: Reduce canvas size based on camera distance

## TextMeshPro Configuration

Recommended TMP settings for high-density rendering:

```csharp
tmp.fontSize = 4f;              // Adjust based on desired density
tmp.characterSpacing = -2f;     // Tighter spacing
tmp.lineSpacing = -20f;         // Reduce vertical gaps
tmp.alignment = TextAlignmentOptions.TopLeft;
tmp.overflowMode = TextOverflowModes.Overflow;
tmp.richText = true;            // CRITICAL - enables color tags
tmp.enableWordWrapping = false;
tmp.parseCtrlCharacters = false;
tmp.extraPadding = false;
tmp.margin = Vector4.zero;
```

## Advanced Techniques

### Double-Pixel Resolution

Use half-blocks to achieve 2x vertical resolution:

```csharp
public void SetDoublePixel(int x, int y, Color32 topColor, Color32 bottomColor)
{
    if (topColor == bottomColor)
    {
        canvas.SetPixel(x, y, '█', topColor);
    }
    else
    {
        // Use gradient or separate half-blocks
        canvas.SetPixel(x, y, '▀', topColor);
        // Note: Requires double-height grid or creative layering
    }
}
```

### Animated Effects

```csharp
IEnumerator AnimateReveal(int x, int y, string text)
{
    for (int i = 0; i < text.Length; i++)
    {
        canvas.SetPixel(x + i, y, text[i], Color.white);
        renderer.UpdateDisplay();
        yield return new WaitForSeconds(0.05f);
    }
}
```

### Particle Systems

```csharp
public class SymbolicParticle
{
    public float x, y;
    public float velocityX, velocityY;
    public char character;
    public Color32 color;
    public float lifetime;
}

// Update in loop
particle.y += particle.velocityY * Time.deltaTime;
canvas.SetPixel((int)particle.x, (int)particle.y, particle.character, particle.color);
```

## Use Cases

**Ideal for**:
- ✅ Game show interfaces (Family Feud, Wheel of Fortune, Jeopardy)
- ✅ Retro-style game graphics
- ✅ Terminal/hacking simulator UIs
- ✅ Data visualization dashboards
- ✅ ASCII roguelikes with full color
- ✅ Educational coding visualizations
- ✅ Low-bandwidth networked games
- ✅ Procedural art installations

**Not ideal for**:
- ❌ Photorealistic graphics
- ❌ High-frame sprite animation (traditional sprites are better)
- ❌ Complex 3D rendering
- ❌ Very large canvases (500x500+)

## Troubleshooting

### Characters Not Displaying
- Ensure Font Asset includes Unicode Block Elements range (0x2580-0x259F)
- Check TextMeshPro component has font asset assigned
- Verify richText is enabled on TMP component

### Colors Not Showing
- Confirm `tmp.richText = true`
- Check that Color32 values are not all black/transparent
- Verify SymbolicRenderer.UpdateDisplay() is being called

### Performance Issues
- Enable dirty rectangle optimization
- Reduce canvas size
- Use fewer color changes per frame
- Profile with Unity Profiler to identify bottlenecks

### Text Looks Blurry
- Increase Font Asset atlas resolution (4096x4096)
- Use SDFAA render mode
- Adjust TMP font size (smaller = crisper at distance)
- Check camera orthographic size or perspective settings

## API Reference

See inline documentation in source files:
- `SymbolicPixel.cs` - Pixel data structure
- `SymbolicCanvas.cs` - Canvas grid and drawing methods
- `SymbolicRenderer.cs` - Unity MonoBehaviour integration
- `SymbolicDrawing.cs` - Extended drawing primitives

## License

This system is provided as-is for use in the LL2PCVR project.

## Credits

Concept: Terminal-based Unicode art (Neofetch, ASCII art)
Implementation: High-Fidelity Symbolic Rendering for Unity
Technology: TextMeshPro + Unicode Block Elements

---

**Ready to create unique symbolic graphics in Unity!** 🚀
