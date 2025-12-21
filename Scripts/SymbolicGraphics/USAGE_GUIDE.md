# 🚀 Quick Start Usage Guide

## Step-by-Step Setup

### 1. Create a Font Asset (One-time setup)

Before you can use symbolic rendering, you need a properly configured TextMeshPro Font Asset:

1. **Open Font Asset Creator**:
   - `Window → TextMeshPro → Font Asset Creator`

2. **Select a monospace font**:
   - Consolas, Courier New, or any monospace font
   - Monospace ensures consistent character spacing

3. **Configure settings**:
   ```
   Sampling Point Size: 64 (or higher for better quality)
   Padding: 5
   Packing Method: Optimum
   Atlas Resolution: 4096 x 4096
   Character Set: Unicode Range (Hex)
   ```

4. **Add Unicode ranges** (CRITICAL):
   ```
   Add these character ranges in the Character Sequence field:

   0020-007E  (Basic Latin - letters, numbers, symbols)
   2580-259F  (Block Elements - ░▒▓█▀▄▌▐ etc.)
   2500-257F  (Box Drawing - ┌┐└┘├┤┬┴┼─│═║ etc.)
   ```

5. **Generate**:
   - Click "Generate Font Atlas"
   - Wait for generation to complete
   - Save the asset (e.g., `Consolas_SDF_Symbolic.asset`)

### 2. Create Your First Symbolic Canvas

1. **Create a Quad in your scene**:
   ```
   GameObject → 3D Object → Quad
   ```

2. **Add SymbolicRenderer component**:
   ```
   Add Component → Symbolic Graphics → Symbolic Renderer
   ```

3. **Configure SymbolicRenderer**:
   - Canvas Width: 80 (characters)
   - Canvas Height: 40 (characters)
   - Font Size: 4.0
   - Character Spacing: -2.0
   - Line Spacing: -20.0

4. **Assign Font Asset**:
   - Select the TextMeshPro component (auto-added)
   - Assign your font asset to the "Font Asset" field

5. **Scale the Quad**:
   - Adjust scale to fit your scene (e.g., 10 x 6 x 1)

### 3. Write Your First Script

Create a new script `MyFirstSymbolicArt.cs`:

```csharp
using UnityEngine;
using SymbolicGraphics;

public class MyFirstSymbolicArt : MonoBehaviour
{
    void Start()
    {
        // Get the renderer
        SymbolicRenderer renderer = GetComponent<SymbolicRenderer>();
        SymbolicCanvas canvas = renderer.Canvas;

        // Draw a welcome message
        canvas.DrawBox(5, 5, 50, 10, Color.cyan, Color.black);
        canvas.DrawText(10, 7, "Welcome to Symbolic Graphics!", Color.yellow);
        canvas.DrawText(10, 9, "Made with Unicode Art ♥", Color.white);

        // Update the display
        renderer.UpdateDisplay();
    }
}
```

4. **Attach script** to your Quad
5. **Play** and see your first symbolic art!

---

## Common Patterns

### Pattern 1: Static Display (Menu, Scoreboard)

```csharp
void Start()
{
    var canvas = GetComponent<SymbolicRenderer>().Canvas;

    // Draw once
    canvas.DrawBox(0, 0, canvas.Width, canvas.Height, Color.yellow, Color.black);
    canvas.DrawText(10, 5, "GAME PAUSED", Color.white);
    canvas.DrawText(10, 10, "Press SPACE to continue", Color.gray);

    // Update once
    GetComponent<SymbolicRenderer>().UpdateDisplay();
}
```

### Pattern 2: Dynamic Updates (Score Counter)

```csharp
private int score = 0;

void UpdateScore(int newScore)
{
    score = newScore;

    var renderer = GetComponent<SymbolicRenderer>();
    var canvas = renderer.Canvas;

    // Clear score area
    canvas.FillRect(10, 5, 20, 3, ' ', Color.black);

    // Draw new score
    canvas.DrawText(10, 5, $"Score: {score}", Color.yellow);

    // Draw score bar
    int barLength = Mathf.Min(score / 10, 15);
    for (int i = 0; i < barLength; i++)
    {
        canvas.SetPixel(10 + i, 7, '█', Color.green);
    }

    renderer.UpdateDisplay();
}
```

### Pattern 3: Animated Elements

```csharp
void Update()
{
    var renderer = GetComponent<SymbolicRenderer>();
    var canvas = renderer.Canvas;

    // Animated loading bar
    int barWidth = 30;
    int progress = (int)(Time.time * 10) % barWidth;

    // Clear previous frame
    canvas.FillRect(10, 10, barWidth, 1, ' ', Color.black);

    // Draw progress
    for (int i = 0; i < progress; i++)
    {
        float hue = (i / (float)barWidth + Time.time * 0.5f) % 1.0f;
        Color color = Color.HSVToRGB(hue, 1f, 1f);
        canvas.SetPixel(10 + i, 10, '█', color);
    }

    renderer.UpdateDisplay();
}
```

### Pattern 4: Event-Driven Updates (Answer Reveal)

```csharp
public void RevealAnswer(string answer, int points)
{
    StartCoroutine(RevealAnimated(answer, points));
}

IEnumerator RevealAnimated(string answer, int points)
{
    var renderer = GetComponent<SymbolicRenderer>();
    var canvas = renderer.Canvas;

    int x = 10;
    int y = 15;

    // Type out answer
    for (int i = 0; i < answer.Length; i++)
    {
        canvas.SetPixel(x + i, y, answer[i], Color.green);
        renderer.UpdateDisplay();
        yield return new WaitForSeconds(0.05f);
    }

    // Show points
    canvas.DrawText(x + answer.Length + 5, y, $"[{points}]", Color.yellow);
    renderer.UpdateDisplay();
}
```

---

## Examples Explained

### QuickStartExample.cs
**What it does**: Shows all basic drawing functions in one place
**When to use**: Learning the API, testing your setup
**Run it**: Attach to a Quad with SymbolicRenderer

### SunsetDemo.cs
**What it does**: Creates a beautiful sunset landscape scene
**When to use**: Understanding gradients, layering, and procedural art
**Features**: Sky gradients, sun with glow, mountains, water reflection, trees
**Run it**: Attach to a Quad, press Play

### FamilyFeudBoard.cs
**What it does**: Complete game show interface with answers, scoring, strikes
**When to use**: Building complex UI, game boards, interactive displays
**Features**:
- Answer reveal animations
- Point bars
- Team scoreboards
- Strike counter
- Context menu controls (right-click component)
**Run it**: Attach to a Quad, customize answers in Inspector

---

## Tips & Tricks

### Performance Optimization

1. **Batch your updates**:
   ```csharp
   // BAD - Updates after each pixel
   canvas.SetPixel(0, 0, '█', Color.red);
   renderer.UpdateDisplay();
   canvas.SetPixel(1, 0, '█', Color.green);
   renderer.UpdateDisplay();

   // GOOD - Update once after all changes
   canvas.SetPixel(0, 0, '█', Color.red);
   canvas.SetPixel(1, 0, '█', Color.green);
   canvas.SetPixel(2, 0, '█', Color.blue);
   renderer.UpdateDisplay();
   ```

2. **Use autoUpdate sparingly**:
   - Only enable if canvas changes every frame
   - For static displays, disable and call UpdateDisplay() manually

3. **Smaller canvases are faster**:
   - 50x30 = excellent performance
   - 100x50 = good performance
   - 200x100 = lower performance, use for detailed scenes only

### Visual Quality

1. **Font size affects density**:
   - Smaller font size = more characters = higher resolution
   - Try: 6.0 (low res), 4.0 (medium), 2.0 (high res)

2. **Character spacing matters**:
   - Negative values create tighter grids
   - Try: 0 (spaced), -2 (tight), -4 (very tight)

3. **Use appropriate characters**:
   - Full blocks (█): Solid areas
   - Shades (░▒▓): Gradients and depth
   - Half-blocks (▀▄▌▐): Double resolution
   - Box drawing (─│┌┐): UI borders

### Color Usage

1. **Use Color32 for better performance**:
   ```csharp
   Color32 red = new Color32(255, 0, 0, 255); // Better
   Color red = Color.red;  // Also works, but Color32 is more efficient
   ```

2. **Gradients with Color32.Lerp**:
   ```csharp
   Color32 start = new Color32(255, 0, 0, 255);
   Color32 end = new Color32(0, 0, 255, 255);
   Color32 middle = Color32.Lerp(start, end, 0.5f);
   ```

3. **HSV for rainbows**:
   ```csharp
   for (int i = 0; i < width; i++)
   {
       float hue = i / (float)width;
       Color color = Color.HSVToRGB(hue, 1f, 1f);
       canvas.SetPixel(i, 10, '█', color);
   }
   ```

---

## Troubleshooting

### Problem: Characters not showing up
**Solution**:
- Check that Font Asset includes Block Elements range (0x2580-0x259F)
- Verify font asset is assigned to TextMeshPro component
- Ensure richText is enabled (should be automatic)

### Problem: Colors are all white/wrong
**Solution**:
- Verify `tmp.richText = true` in TextMeshPro component
- Check that you're calling `renderer.UpdateDisplay()` after drawing
- Ensure colors are not fully transparent (alpha = 0)

### Problem: Text looks blurry
**Solution**:
- Increase Font Asset atlas resolution to 4096x4096
- Use SDF or SDFAA render mode
- Adjust camera distance or TMP font size
- Check Quad scale matches your needs

### Problem: Performance is slow
**Solution**:
- Reduce canvas size (try 80x40 instead of 200x100)
- Enable dirty rectangle optimization
- Don't call UpdateDisplay() every frame unless needed
- Profile with Unity Profiler to identify bottleneck

### Problem: Spacing looks weird
**Solution**:
- Ensure you're using a monospace font
- Adjust characterSpacing and lineSpacing values
- Try negative values for tighter grids
- Check that font asset was generated correctly

---

## Next Steps

1. **Experiment with QuickStartExample.cs**
   - Modify colors, positions, shapes
   - Try different characters
   - Create your own designs

2. **Study SunsetDemo.cs**
   - Understand gradient techniques
   - See how layers create depth
   - Learn procedural generation

3. **Customize FamilyFeudBoard.cs**
   - Change questions and answers
   - Modify color scheme
   - Add new features

4. **Build your own project**
   - Game show interfaces
   - Retro game graphics
   - Data visualization
   - Terminal simulations
   - Educational tools

---

## API Quick Reference

### SymbolicCanvas Methods

| Method | Description |
|--------|-------------|
| `SetPixel(x, y, char, color)` | Set single pixel |
| `GetPixel(x, y)` | Get pixel at position |
| `Clear(char, color)` | Clear entire canvas |
| `FillRect(x, y, w, h, char, color)` | Fill rectangle |
| `DrawLine(x1, y1, x2, y2, char, color)` | Draw line |
| `DrawRect(x, y, w, h, char, color)` | Draw rectangle outline |
| `DrawBox(x, y, w, h, borderColor, fillColor)` | Draw bordered box |
| `DrawText(x, y, text, color)` | Draw text string |
| `DrawCircle(x, y, radius, char, color)` | Draw filled circle |
| `DrawHorizontalGradient(...)` | Horizontal color gradient |
| `DrawVerticalGradient(...)` | Vertical color gradient |
| `DrawRadialGradient(...)` | Radial gradient from center |

### SymbolicChars Constants

| Constant | Character | Use |
|----------|-----------|-----|
| `FULL_BLOCK` | █ | Solid areas |
| `LIGHT_SHADE` | ░ | Light fill |
| `MEDIUM_SHADE` | ▒ | Medium fill |
| `DARK_SHADE` | ▓ | Dark fill |
| `UPPER_HALF` | ▀ | Top half |
| `LOWER_HALF` | ▄ | Bottom half |
| `BOX_TL` | ┌ | Box top-left |
| `BOX_TR` | ┐ | Box top-right |
| `BOX_BL` | └ | Box bottom-left |
| `BOX_BR` | ┘ | Box bottom-right |
| `BOX_H` | ─ | Horizontal line |
| `BOX_V` | │ | Vertical line |

---

**You're ready to create amazing symbolic graphics in Unity!** 🎨

For more details, see README.md in the SymbolicGraphics folder.
