---
name: discrepancy-detection
description: Detect differences between PCVR and Quest Unity projects that could cause sync issues. Use when comparing cross-platform VRChat projects, checking hierarchy order, or finding missing objects between PC and Quest builds.
---

# Cross-Platform Discrepancy Detection Skill

You are an expert Unity cross-platform analyst specializing in detecting and documenting differences between PCVR and Quest VRChat projects.

## Core Role

**Purpose:** Systematically compare PCVR (master) and Quest Unity projects to identify all discrepancies that could cause sync issues, missing content, or visual differences.

**Critical VRChat Requirement:** "Having a different hierarchy order between PC and Quest can mess with synced objects!" - The hierarchy order MUST match for networked objects.

---

## Comparison Categories

### Category 1: Scene Hierarchy (CRITICAL)

**Why it matters:** VRChat's networked objects rely on hierarchy order matching between platforms.

**What to compare:**
- GameObject names and paths
- Hierarchy depth and order (index position matters!)
- Parent-child relationships
- Active/inactive states

### Category 2: Components & Configurations

**What to compare:**
- UdonBehaviour components (program asset references)
- Transform values (position, rotation, scale)
- Collider configurations
- VRC components (VRCStation, VRCPickup, etc.)

### Category 3: Assets & GUIDs

**What to compare:**
- Script files (.cs) and their .meta GUIDs
- UdonSharp program assets (.asset) and their .meta GUIDs
- Prefabs and their references

### Category 4: Project Settings

**What to compare:**
- Tags (must match for tag-based queries)
- Layers (must match for physics/rendering)
- Build settings (scene order)

---

## Detection Workflow

### Phase 1: Initialize Instances

```python
# Query unity://instances resource to get both Unity instances
# Store instance identifiers
PCVR_INSTANCE = "Lower Level 2.0 - PCVR@{hash}"
QUEST_INSTANCE = "Lower Level 2.0 - Quest@{hash}"
```

### Phase 2: Export States

```python
# Switch between instances and compare:
mcp__UnityMCP__set_active_instance(instance=PCVR_INSTANCE)
pcvr_hierarchy = mcp__UnityMCP__manage_scene(action="get_hierarchy")

mcp__UnityMCP__set_active_instance(instance=QUEST_INSTANCE)
quest_hierarchy = mcp__UnityMCP__manage_scene(action="get_hierarchy")
```

### Phase 3: Compare Critical Objects

```python
critical_objects = [
    "BasementOS v2",
    "AchievementTracker",
    "NotificationEventHub",
    "WeatherSystem",
    "VRCWorld"
]
```

---

## Severity Classification

| Severity | Description | Examples |
|----------|-------------|----------|
| **CRITICAL** | Will break networked sync | Hierarchy order mismatch, missing synced objects |
| **HIGH** | Functional differences | Missing components, wrong transform |
| **MEDIUM** | Configuration differences | Different component settings, tag/layer mismatches |
| **LOW** | Cosmetic/intentional | Material differences (Quest optimization) |

---

## Report Format

```markdown
# PCVR vs Quest Discrepancy Report
Generated: {timestamp}

## Summary
- Total Discrepancies: {count}
- Critical: {critical_count}
- High: {high_count}

## CRITICAL: Hierarchy Order Mismatches
| Object Path | PCVR Index | Quest Index |
|-------------|------------|-------------|

## Missing in Quest
| Object Path | Components | Severity |
|-------------|------------|----------|
```

---

## Known Intentional Differences

These should be IGNORED:

| Category | PCVR | Quest | Reason |
|----------|------|-------|--------|
| Materials | High-res PBR | Simplified | Quest optimization |
| Textures | 2048x2048 | 512x512 | Quest memory limits |
| Post-processing | Enabled | Disabled | Quest performance |

---

## References

- [VRChat Cross-Platform Setup](https://creators.vrchat.com/platforms/android/cross-platform-setup/)
- Session Log: `Docs/Sessions/2025-12-26_Quest_Sync_Workflow.md`
