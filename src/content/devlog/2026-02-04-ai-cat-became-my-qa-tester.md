---
title: "How an AI Cat Became My QA Tester"
date: 2026-02-04
tags: ["ai", "navmesh", "vrchat", "meta", "debugging"]
type: meta
---

![Rags the cat wandering through the basement](/images/devlog/ai-cat-qa/rags-basement.png)
*Rags the AI cat exploring Lower Level 2.0 -- CRT terminal glowing, Master Chief watching from the papasan chair.*

I asked Claude Code to generate a heatmap of my AI cat's movement patterns, mostly as a joke. What I got back taught me how AI agents actually navigate in Unity -- and it's kind of changing my mind about what's possible with AI collaboration.

### The Setup

Lower Level 2.0 is a nostalgic 2000s basement VRChat world -- shag carpet, CRT monitors, a DOS terminal, Xbox achievements. To make the space feel lived-in, I added an AI cat named Rags using the [Sisters In Gaming NavMesh NPC/AI System](https://msmalvolio.gumroad.com/l/NPCAI). The idea was simple: a cat that wanders freely around the basement, explores, sits down, stretches, naps in a bed. A living detail that makes the world feel like someone actually lives there.

The implementation worked. Rags walked around, did cat things, responded to petting. But I had no real sense of *where* the cat was spending its time or whether it was actually reaching all the areas I'd built.

### The Experiment

During a pair programming session with Claude Code, I casually asked if it could build a heatmap showing where Rags walks. I expected either "that's not really possible from an editor script" or some half-working prototype I'd have to finish myself.

Instead, I got two fully functional Editor tools:

- **NpcHeatmapTracker** -- automatically records Rags' position every 2 seconds during Play mode, writing timestamped coordinates to a CSV file
- **NpcHeatmapVisualizer** -- an Editor window that renders a color-coded density overlay directly in the Scene view, from blue (cold/never visited) through cyan, green, yellow, to red (hot/frequently visited)

Claude implemented it itself using its MCP tool to write and compile the scripts directly in the Unity Editor. I hit Play, let the cat walk, opened the visualizer, hit refresh, and data appeared.

### What the Heatmap Revealed

I let Rags run overnight. By morning: **14,446 position samples** across 1,200 grid cells.

![Overnight heatmap showing cat movement patterns](/images/devlog/ai-cat-qa/heatmap-overnight.png)
*The overnight heatmap: 14,446 samples. Green/cyan areas show regular cat traffic. Blue zones? The cat never goes there.*

The visualization immediately told a story. The main living area had healthy green and cyan coverage -- Rags was patrolling the carpet, weaving between furniture, visiting the game room. But there were obvious cold zones -- the question was why?

### The NavMesh Optimization

The heatmap motivated a deeper look at the NavMesh itself. The bake volume's Y-range was set from -2 to 8 -- meaning Unity was generating navigation triangles on walls, ceilings, and elevated surfaces where Rags should never walk.

Shrinking the bake volume to floor-only (-0.2 to 0.8) and removing a duplicate NavMeshSurface component that was stacking bakes cut the NavMesh dramatically:

| Metric | Before | After | Reduction |
|--------|--------|-------|-----------|
| Area | 637 sq m | 275 sq m | 57% |
| Triangles | 297 | 100 | 66% |

Less geometry for the pathfinding system to evaluate, cleaner paths, and no more phantom navigation surfaces on vertical walls.

![Detailed heatmap with visit counts](/images/devlog/ai-cat-qa/heatmap-detailed.png)
*After optimization: cell-by-cell visit counts visible. 9,994 samples across 1,315 grid cells. The hot tub cold zone (white outline, upper right) now correctly shows zero visits.*

### What Wasn't Working (Before This)

Traditional approach to validating NPC pathing: load into VRChat, watch the cat for a while, take mental notes, hope you notice if it gets stuck somewhere. Repeat across multiple sessions. Maybe you catch the problems. Maybe you don't.

The issue isn't effort -- it's that human observation is terrible at accumulating spatial data over time. You can watch a cat for 30 minutes and have a vague sense of where it goes. A CSV file with 14,000 position samples gives you certainty.

### The Principle

I went in expecting Claude to say no. The request felt like a stretch -- generating custom Editor tooling for a niche diagnostic need, writing file I/O and Scene view rendering code, integrating with Play mode lifecycle hooks. The kind of thing I'd never build myself because the time-to-value ratio felt too steep for a "nice to have."

But that calculation was wrong. The heatmap took one session to build, runs automatically with zero setup, and has already found real issues I'd missed. The tool now lives permanently in the project, and I collect data to verify Rags' behavior.

The broader realization: **AI collaboration collapses the cost of building diagnostic tools.** Things I'd normally dismiss as "not worth the effort" become trivial to create when you can describe what you want and get a working implementation back. The heatmap wasn't on any roadmap. It wasn't a planned feature. It came from a casual "I wonder if..." moment -- and it turned out to be one of the most useful debugging tools in the project.

### Beyond the Basement

The technique generalizes. Any game with NavMesh agents can benefit from movement heatmaps:

- **NPC patrol validation** -- are guards actually covering the areas you designed them to cover?
- **Spawn point auditing** -- do players cluster in predictable spots?
- **Accessibility testing** -- can all agent types reach all intended areas?
- **Performance profiling** -- where do entities spend compute time pathfinding?

Game studios have used telemetry heatmaps for player behavior analysis for years. What's different here is the barrier to entry: instead of dedicated analytics infrastructure, I got a working heatmap from a conversation. The cat became an automated level auditor not because I planned it, but because I asked an AI tool to visualize something I was curious about.

**Key Insight:** The best debugging tools sometimes come from playful experimentation. When AI collaboration makes building diagnostic tools nearly free, "I wonder if..." becomes a viable development strategy.

---

### Try It Yourself: NPC Heatmap for Any Unity NavMesh Agent

Want to track where your own NPCs walk? The system is two standalone Editor scripts -- no dependencies beyond Unity's built-in NavMesh. Drop them into your project's `Assets/Editor/` folder and you're done.

#### Step 1: Setup

1. Create an `Assets/Editor/` folder if you don't have one
2. Add both scripts below to that folder
3. Change the GameObject name in `NpcHeatmapTracker.cs` line 42 to match your NPC (default: `"Rags Parent Prefab"`)

#### Step 2: Collect Data

1. Enter Play mode -- tracking starts automatically
2. Let your NPC wander (30+ minutes gives reliable data; overnight is ideal)
3. Check the Console for `[Heatmap]` log messages confirming samples are being recorded

#### Step 3: Visualize

1. Open `Tools > NPC Cat > Heatmap > Show Heatmap`
2. Enable "Show Heatmap Overlay" in the panel
3. Adjust cell size, opacity, and height offset to match your scene
4. Toggle "Show Visit Counts" for per-cell numbers

#### NpcHeatmapTracker.cs

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Tracks an NPC's position during Play mode and writes to CSV.
/// Auto-starts when entering Play mode, auto-stops on exit.
/// Data file: Assets/Editor/npc_heatmap_data.csv
/// </summary>
[InitializeOnLoad]
public class NpcHeatmapTracker
{
    private static float sampleInterval = 2.0f; // seconds between samples
    private static float lastSampleTime = 0f;
    private static Transform npcTransform = null;
    private static StreamWriter writer = null;
    private static int sampleCount = 0;
    private static string dataPath = "Assets/Editor/npc_heatmap_data.csv";
    private static bool isTracking = false;

    static NpcHeatmapTracker()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += OnUpdate;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            StartTracking();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            StopTracking();
        }
    }

    private static void StartTracking()
    {
        // CHANGE THIS to your NPC's GameObject name
        GameObject npc = GameObject.Find("Rags Parent Prefab");
        if (npc == null)
        {
            Debug.Log("[Heatmap] Could not find NPC GameObject - tracking disabled");
            return;
        }

        npcTransform = npc.transform;
        sampleCount = 0;
        lastSampleTime = 0f;

        bool fileExists = File.Exists(dataPath);
        FileStream fs = new FileStream(dataPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        writer = new StreamWriter(fs, Encoding.UTF8);
        if (!fileExists)
        {
            writer.WriteLine("timestamp,x,y,z,session");
        }

        isTracking = true;
        Debug.Log("[Heatmap] Tracking started. Sampling every " + sampleInterval + "s. Data: " + dataPath);
    }

    private static void StopTracking()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }

        if (isTracking)
        {
            Debug.Log("[Heatmap] Tracking stopped. " + sampleCount + " samples recorded to " + dataPath);
        }

        isTracking = false;
        npcTransform = null;
    }

    private static void OnUpdate()
    {
        if (!isTracking || !EditorApplication.isPlaying || npcTransform == null || writer == null)
            return;

        float time = (float)EditorApplication.timeSinceStartup;
        if (time - lastSampleTime < sampleInterval)
            return;

        lastSampleTime = time;
        Vector3 pos = npcTransform.position;

        string session = System.DateTime.Now.ToString("yyyy-MM-dd");
        writer.WriteLine(time.ToString("F2") + "," + pos.x.ToString("F3") + "," + pos.y.ToString("F3") + "," + pos.z.ToString("F3") + "," + session);

        sampleCount++;

        // Flush every 50 samples so data isn't lost on crash
        if (sampleCount % 50 == 0)
        {
            writer.Flush();
            Debug.Log("[Heatmap] " + sampleCount + " samples recorded. Last pos: (" + pos.x.ToString("F1") + ", " + pos.z.ToString("F1") + ")");
        }
    }

    [MenuItem("Tools/NPC Cat/Heatmap/Clear Tracking Data")]
    public static void ClearData()
    {
        if (File.Exists(dataPath))
        {
            File.Delete(dataPath);
            Debug.Log("[Heatmap] Tracking data cleared.");
            NpcHeatmapVisualizer.ClearInMemoryData();
        }
        else
        {
            Debug.Log("[Heatmap] No tracking data to clear.");
        }
    }

    [MenuItem("Tools/NPC Cat/Heatmap/Show Sample Count")]
    public static void ShowSampleCount()
    {
        if (!File.Exists(dataPath))
        {
            Debug.Log("[Heatmap] No data file found.");
            return;
        }

        string[] lines = File.ReadAllLines(dataPath);
        int count = lines.Length > 0 ? lines.Length - 1 : 0;
        Debug.Log("[Heatmap] " + count + " samples in " + dataPath);
    }
}
```

#### NpcHeatmapVisualizer.cs

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Reads NPC heatmap CSV data and renders a color-coded overlay in the Scene view.
/// Open via Tools > NPC Cat > Heatmap > Show Heatmap
/// </summary>
public class NpcHeatmapVisualizer : EditorWindow
{
    private static string dataPath = "Assets/Editor/npc_heatmap_data.csv";
    private static float cellSize = 0.25f;
    private static bool showHeatmap = false;
    private static Dictionary<Vector2Int, int> heatGrid = new Dictionary<Vector2Int, int>();
    private static int maxHeat = 1;
    private static int totalSamples = 0;
    private static float opacity = 0.6f;
    private static bool showNumbers = false;
    private static float heightOffset = 0.02f;

    [MenuItem("Tools/NPC Cat/Heatmap/Show Heatmap")]
    public static void ShowWindow()
    {
        NpcHeatmapVisualizer window = GetWindow<NpcHeatmapVisualizer>("NPC Heatmap");
        window.minSize = new Vector2(300, 200);
        window.Show();
        LoadData();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        LoadData();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC Cat Heatmap", EditorStyles.boldLabel);
        GUILayout.Space(5);

        showHeatmap = EditorGUILayout.Toggle("Show Heatmap Overlay", showHeatmap);
        cellSize = EditorGUILayout.Slider("Cell Size (m)", cellSize, 0.1f, 1.0f);
        opacity = EditorGUILayout.Slider("Opacity", opacity, 0.1f, 1.0f);
        showNumbers = EditorGUILayout.Toggle("Show Visit Counts", showNumbers);
        heightOffset = EditorGUILayout.Slider("Height Offset (Y)", heightOffset, -2.0f, 5.0f);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Total Samples", totalSamples.ToString());
        EditorGUILayout.LabelField("Grid Cells", heatGrid.Count.ToString());
        EditorGUILayout.LabelField("Hottest Cell", maxHeat + " visits");

        GUILayout.Space(10);
        if (GUILayout.Button("Reload Data"))
        {
            LoadData();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Clear Data"))
        {
            if (EditorUtility.DisplayDialog("Clear Heatmap Data",
                "Delete all recorded tracking data?", "Yes", "Cancel"))
            {
                if (File.Exists(dataPath))
                    File.Delete(dataPath);
                ClearInMemoryData();
            }
        }

        if (showHeatmap)
        {
            SceneView.RepaintAll();
        }
    }

    public static void ClearInMemoryData()
    {
        heatGrid.Clear();
        totalSamples = 0;
        maxHeat = 1;
        SceneView.RepaintAll();
    }

    private static void LoadData()
    {
        heatGrid.Clear();
        totalSamples = 0;
        maxHeat = 1;

        if (!File.Exists(dataPath))
        {
            Debug.Log("[Heatmap] No data file found at " + dataPath);
            return;
        }

        string[] lines;
        using (FileStream fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(fs))
        {
            lines = reader.ReadToEnd().Split('\n');
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 4) continue;

            float x, z;
            if (!float.TryParse(parts[1],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out x)) continue;
            if (!float.TryParse(parts[3],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out z)) continue;

            Vector2Int cell = new Vector2Int(
                Mathf.FloorToInt(x / cellSize),
                Mathf.FloorToInt(z / cellSize)
            );

            if (heatGrid.ContainsKey(cell))
                heatGrid[cell]++;
            else
                heatGrid[cell] = 1;

            if (heatGrid[cell] > maxHeat)
                maxHeat = heatGrid[cell];

            totalSamples++;
        }

        Debug.Log("[Heatmap] Loaded " + totalSamples + " samples into " +
            heatGrid.Count + " cells. Hottest: " + maxHeat + " visits");
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!showHeatmap || heatGrid.Count == 0) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        foreach (KeyValuePair<Vector2Int, int> kvp in heatGrid)
        {
            Vector2Int cell = kvp.Key;
            int heat = kvp.Value;

            // Log scale for better color distribution
            float t = Mathf.Log(1 + heat) / Mathf.Log(1 + maxHeat);

            // Color ramp: dark blue -> blue -> cyan -> green -> yellow -> red
            Color color;
            if (t < 0.2f)
                color = Color.Lerp(new Color(0, 0, 0.4f), Color.blue, t * 5f);
            else if (t < 0.4f)
                color = Color.Lerp(Color.blue, Color.cyan, (t - 0.2f) * 5f);
            else if (t < 0.6f)
                color = Color.Lerp(Color.cyan, Color.green, (t - 0.4f) * 5f);
            else if (t < 0.8f)
                color = Color.Lerp(Color.green, Color.yellow, (t - 0.6f) * 5f);
            else
                color = Color.Lerp(Color.yellow, Color.red, (t - 0.8f) * 5f);

            color.a = opacity * Mathf.Lerp(0.4f, 1.0f, t);

            float worldX = cell.x * cellSize;
            float worldZ = cell.y * cellSize;
            float y = heightOffset;

            Vector3 center = new Vector3(worldX + cellSize * 0.5f, y, worldZ + cellSize * 0.5f);

            Handles.color = color;
            Vector3[] verts = new Vector3[]
            {
                new Vector3(worldX, y, worldZ),
                new Vector3(worldX + cellSize, y, worldZ),
                new Vector3(worldX + cellSize, y, worldZ + cellSize),
                new Vector3(worldX, y, worldZ + cellSize)
            };

            Handles.DrawAAConvexPolygon(verts);

            if (showNumbers && heat > 1)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 9;
                style.alignment = TextAnchor.MiddleCenter;
                Handles.Label(center + Vector3.up * 0.05f, heat.ToString(), style);
            }
        }
    }
}
```

#### Tips

- **30 minutes** of wandering gives a rough picture; **overnight** gives reliable coverage data
- **Cell size 0.25m** is good for detail; increase to 0.5m or 1.0m for a broader overview
- **Height offset** should match your floor level -- adjust until the overlay sits flat on the ground
- Data persists between sessions (CSV appends). Use `Tools > NPC Cat > Heatmap > Clear Tracking Data` to start fresh
- The tracker flushes to disk every 50 samples (~100 seconds), so data survives Unity crashes

---

*Technical details: NavMeshAgent (radius 0.1, height 0.2), position sampling every 2s, logarithmic color scale, CSV storage with crash recovery. Works with any Unity NavMesh project -- not VRChat-specific.*
