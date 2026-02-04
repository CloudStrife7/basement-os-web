# NPC Heatmap System

**Two Unity Editor scripts for tracking and visualizing NPC movement patterns using NavMesh.**

Drop these into any Unity project with NavMesh agents to see exactly where your NPCs walk -- and more importantly, where they don't.

---

## What It Does

- **NpcHeatmapTracker** -- Records any GameObject's position every 2 seconds during Play mode. Writes to CSV with crash recovery.
- **NpcHeatmapVisualizer** -- Renders a color-coded density overlay directly in the Scene view. Logarithmic color scale from blue (cold) to red (hot).

No dependencies beyond Unity's built-in Editor API. Works with any NavMesh agent, not just VRChat.

---

## Quick Start

### 1. Install

Create `Assets/Editor/` if it doesn't exist, then add both scripts below.

### 2. Configure

In `NpcHeatmapTracker.cs`, change line 42 to match your NPC's GameObject name:

```csharp
GameObject npc = GameObject.Find("Your NPC Name Here");
```

### 3. Collect Data

Enter Play mode. Tracking starts automatically -- check Console for `[Heatmap]` messages. Let your NPC wander:
- **30 minutes** = rough picture
- **Overnight** = reliable coverage data

### 4. Visualize

Open `Tools > NPC Cat > Heatmap > Show Heatmap` and enable the overlay.

| Setting | Default | Purpose |
|---------|---------|---------|
| Cell Size | 0.25m | Grid resolution. Smaller = more detail, more cells |
| Opacity | 0.6 | Overlay transparency |
| Show Visit Counts | Off | Per-cell number labels |
| Height Offset | 0.02 | Y position of overlay. Match to your floor level |

---

## NpcHeatmapTracker.cs

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
    private static float sampleInterval = 2.0f;
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
            StartTracking();
        else if (state == PlayModeStateChange.ExitingPlayMode)
            StopTracking();
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
            writer.WriteLine("timestamp,x,y,z,session");

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
            Debug.Log("[Heatmap] Tracking stopped. " + sampleCount + " samples recorded to " + dataPath);
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
            Debug.Log("[Heatmap] No tracking data to clear.");
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

## NpcHeatmapVisualizer.cs

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
            SceneView.RepaintAll();
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

            float t = Mathf.Log(1 + heat) / Mathf.Log(1 + maxHeat);

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

---

## CSV Format

```csv
timestamp,x,y,z,session
142.50,6.184,0.031,0.193,2026-02-04
144.50,5.892,0.031,1.247,2026-02-04
```

| Column | Description |
|--------|-------------|
| timestamp | `EditorApplication.timeSinceStartup` in seconds |
| x, y, z | World-space position (3 decimal places) |
| session | Date string for grouping multiple runs |

## Tips

- Data persists between Play sessions (CSV appends). Use `Tools > NPC Cat > Heatmap > Clear Tracking Data` to start fresh.
- The tracker flushes to disk every 50 samples (~100 seconds), so data survives Unity crashes.
- `FileShare.ReadWrite` means you can reload the visualizer while the tracker is still recording.
- For multiple NPCs, duplicate the tracker and change the GameObject name and output path.

## Origin

Built during a pair programming session with Claude Code for the [Lower Level 2.0](https://basementos.com) VRChat world. Read the full story: [How an AI Cat Became My QA Tester](/devlog/2026-02-04-ai-cat-became-my-qa-tester).
