---
name: cross-platform-sync
description: Synchronize UdonSharp prefabs, scripts, and assets between PCVR and Quest Unity projects while preserving GUID references.
---

# Cross-Platform Sync Skill

You are an expert cross-platform synchronization specialist for VRChat Unity projects.

## Core Capability

Synchronize UdonSharp prefabs, scripts, and assets between PCVR and Quest Unity projects while preserving GUID references.

---

## Critical Rule: GUID Preservation

**ALWAYS sync files WITH their .meta files:**
- `.cs` + `.cs.meta` (scripts)
- `.asset` + `.asset.meta` (UdonSharp program assets - CRITICAL)
- `.prefab` + `.prefab.meta` (prefabs)

**NEVER let Quest auto-generate .asset files** - they get new GUIDs that break prefab references.

---

## Multi-Instance MCP Workflow

### Step 1: Discover Instances

```python
# Get available Unity instances
# Check response for instance identifiers
mcp__UnityMCP__manage_editor(action="get_state")
```

### Step 2: Script Synchronization

```bash
# Option A: Use automation script
python Automation/sync-udonsharp-scripts.py --clean --folder BasementOS

# Option B: Manual PowerShell
$src = "C:\...\Lower Level 2.0 - PCVR"
$dst = "C:\...\Lower Level 2.0 - Quest"

$scripts = @("Assets\Scripts\MyScript.cs")
foreach ($s in $scripts) {
    Copy-Item "$src\$s" "$dst\$s" -Force
    Copy-Item "$src\$s.meta" "$dst\$s.meta" -Force
}
```

### Step 3: Switch to Quest Instance

```python
mcp__UnityMCP__set_active_instance(instance="Lower Level 2.0 - Quest@{hash}")
```

### Step 4: Wait for Compilation

```python
# Wait 20-30 seconds for Unity to compile
powershell -Command "Start-Sleep -Seconds 30"

# Check for errors
mcp__UnityMCP__read_console(action="get", types=["error"], count=10)
```

### Step 5: Execute Wiring (if needed)

```python
# Run Editor wiring script
mcp__UnityMCP__execute_menu_item(menu_path="Tools/Wire Remote Content Module")

# Verify success
mcp__UnityMCP__read_console(action="get", count=20)
```

### Step 6: Save Scene

```python
mcp__UnityMCP__manage_scene(action="save")
```

### Step 7: Test in Play Mode

```python
mcp__UnityMCP__manage_editor(action="play")
powershell -Command "Start-Sleep -Seconds 15"
mcp__UnityMCP__read_console(action="get", types=["error"], count=20)
mcp__UnityMCP__manage_editor(action="stop")
```

---

## Prefab Synchronization

### Copy Prefab with Meta

```bash
robocopy "PCVR\Assets\Prefabs" "Quest\Assets\Prefabs" BasementOS_v2.prefab* /IS
```

### Instantiate in Quest Scene

```python
mcp__UnityMCP__set_active_instance(instance="Quest@{hash}")

mcp__UnityMCP__manage_gameobject(
    action="create",
    prefab_path="Assets/Prefabs/BasementOS_v2.prefab",
    name="BasementOS v2",
    position=[3.37, 1.24, 4.53],
    rotation=[0, 90, 0],
    scale=[0.124, 0.124, 0.124]
)
```

---

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| Missing script errors | .asset GUIDs don't match | Re-sync .asset + .asset.meta from PCVR |
| Menu item not found | Unity recompiling | Wait 20-30 seconds |
| Old component overlapping | v1 still active | Disable old object |
| Wrong transform | Not copied from PCVR | Query PCVR transform first |
| `set_active_instance` fails | Instance hash changed | Query fresh instance list |

---

## Key Files

| File | Purpose |
|------|---------|
| `Automation/sync-udonsharp-scripts.py` | Automated script sync |
| `Automation/sync-config.json` | Sync configuration |
| `Assets/Editor/WireRemoteContentModule.cs` | Reference wiring automation |
| `Assets/Editor/BasementOSv2Wiring.cs` | BasementOS v2 wiring |

---

## Session References

- `Docs/Sessions/2025-12-26_Quest_Sync_Workflow.md` - First successful multi-instance sync
- `Docs/Sessions/2025-12-27_Quest_Sync_Checkpoint.md` - Quest wiring fixes

---

## Visual Verification

```powershell
# Capture screenshot for comparison
powershell -ExecutionPolicy Bypass -File "Screenshots/capture.ps1" -ProjectName "Quest"
```

Or use Editor script:
```python
mcp__UnityMCP__execute_menu_item(menu_path="Tools/Screenshots/Capture Current View")
```

---

*Skill created from proven workflow (Dec 26-27, 2025)*
