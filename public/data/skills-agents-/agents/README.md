# Claude Agents for VRChat DOS Terminal Project

This directory contains specialized agent configurations for Claude Code to assist with different aspects of the VRChat DOS Terminal project.

## Available Agents

### 1. UdonSharp Developer (`udonsharp-developer.md`)

**Purpose:** All UdonSharp scripting tasks

**When to invoke:**
- Writing new UdonSharp scripts
- Reviewing existing code for UdonSharp compatibility
- Converting standard C# patterns to UdonSharp
- Debugging UdonSharp-specific issues

**Key constraints this agent enforces:**
- NO properties (get/set)
- NO LINQ
- NO async/await
- NO try/catch
- NO foreach loops
- NO List<T>/Dictionary<K,V>
- NO ?./??
- NO $"" string interpolation
- Required use of `Utilities.IsValid()`

**Example tasks:**
- "Write a player tracking script"
- "Review this script for UdonSharp compatibility"
- "Convert this LINQ query to UdonSharp"

---

### 2. VRChat Game Developer (`vrchat-game-developer.md`)

**Purpose:** Snake game and other arcade game development

**When to invoke:**
- Implementing game loops
- Adding input handling
- Managing game state
- Implementing scoring and high scores
- Level progression systems

**Key patterns this agent uses:**
- Update() + Time.time game loops
- VRChat input overrides (InputMoveVertical, etc.)
- PlayerData/PlayerTags for persistence
- Simple flag-based state management

**Example tasks:**
- "Implement the Snake movement system"
- "Add high score persistence"
- "Create a pause menu system"
- "Handle player input for game controls"

---

### 3. Terminal UI Designer (`terminal-ui-designer.md`)

**Purpose:** DOS terminal visual interface design

**When to invoke:**
- Creating terminal screen layouts
- Designing menus and displays
- Working with TextMeshPro formatting
- Implementing color themes
- Building bordered UI elements

**Key standards this agent maintains:**
- 80-character width layouts
- Consistent border characters (═, ║, ╔, etc.)
- TextMeshPro rich text color tags
- Theme color definitions

**Example tasks:**
- "Design the main menu screen"
- "Create an achievement notification popup"
- "Format the game HUD display"
- "Implement the settings screen layout"

---

### 4. VRChat QA Tester (`vrchat-qa-tester.md`)

**Purpose:** Testing documentation and test method implementation

**When to invoke:**
- Writing test cases
- Creating test matrices for achievements
- Adding context menu test methods
- Planning QA coverage
- Documenting test results

**Key formats this agent provides:**
- Test case documentation templates
- Achievement test matrices
- Context menu test methods (ContextMenu attribute)
- Platform compatibility checklists

**Example tasks:**
- "Create test cases for the Snake game"
- "Build an achievement test matrix"
- "Add debug context menu methods to this script"
- "Write a pre-release testing checklist"

---

### 5. Unity MCP Specialist (`unity-mcp-specialist.md`)

**Purpose:** Unity Editor automation via Model Context Protocol (MCP)

**When to invoke:**
- Navigating Unity scene hierarchies programmatically
- Inspecting GameObject components and properties
- Applying structured edits to UdonSharp scripts
- Comparing PCVR vs Quest project states
- Debugging via console logs
- Executing Unity menu items
- Managing prefabs and assets
- Running automated tests

**Key tools this agent uses:**
- `manage_gameobject`: Find/modify GameObjects by name/tag/component
- `script_apply_edits`: Structured C# method insertion/replacement
- `validate_script`: UdonSharp syntax validation
- `set_active_instance`: Multi-instance coordination (PCVR + Quest)
- `read_console`: Capture compilation errors and logs
- `run_tests`: Execute EditMode/PlayMode tests
- `find_in_file`: Locate method definitions and usages

**Example tasks:**
- "Find all GameObjects with AchievementTracker component"
- "Add CheckWeatherAchievements method to AchievementTracker.cs"
- "Compare PCVR and Quest scene hierarchies"
- "Validate all UdonSharp scripts for compilation errors"
- "Run play mode tests and capture console output"

---

## How to Use These Agents

### In Claude Code

When working on a task, specify the agent context by referencing the relevant configuration:

```
Use the udonsharp-developer agent to review this script for compatibility issues.
```

```
Use the terminal-ui-designer agent to create the game over screen layout.
```

### Combining Agents

For complex tasks, you may need multiple agent perspectives:

1. **Game feature implementation:**
   - Use `udonsharp-developer` for the core logic
   - Use `vrchat-game-developer` for game patterns
   - Use `terminal-ui-designer` for the display

2. **Full feature with tests:**
   - Implement with appropriate agent
   - Then use `vrchat-qa-tester` to add test methods

3. **Automated script modification:**
   - Use `unity-mcp-specialist` to locate and edit scripts
   - Use `udonsharp-developer` to provide UdonSharp-compliant code
   - Use `unity-mcp-specialist` to validate changes

4. **Multi-platform development:**
   - Use `unity-mcp-specialist` to compare PCVR and Quest projects
   - Use `udonsharp-developer` to fix platform-specific issues
   - Use `vrchat-qa-tester` to verify parity

### Agent Selection Guide

| Task Type | Primary Agent | Secondary Agent |
|-----------|---------------|-----------------|
| New script | udonsharp-developer | - |
| Game mechanic | vrchat-game-developer | udonsharp-developer |
| UI screen | terminal-ui-designer | - |
| Achievement system | vrchat-game-developer | udonsharp-developer |
| Test coverage | vrchat-qa-tester | - |
| Code review | udonsharp-developer | relevant domain agent |
| Hierarchy navigation | unity-mcp-specialist | - |
| Script modification (automated) | unity-mcp-specialist | udonsharp-developer |
| PCVR vs Quest comparison | unity-mcp-specialist | - |
| Automated testing | unity-mcp-specialist | vrchat-qa-tester |
| Console debugging | unity-mcp-specialist | udonsharp-developer |
| Asset management | unity-mcp-specialist | - |

## Project Context

This project is a VRChat world featuring:
- **DOS Terminal Interface** - Retro-styled terminal with TextMeshPro
- **Xbox-style Achievements** - Achievement system with gamerscore
- **Snake Game** - Classic arcade game with VRChat controls
- **Player Persistence** - High scores and unlocks saved per player

All code must comply with UdonSharp restrictions while maintaining the DOS aesthetic.

---

## 🏗️ Hub-Spoke Architecture (CRITICAL)

**ALL agents must understand and follow the hub-spoke architecture pattern.**

### Architecture Overview

The project uses a **hub-spoke model** where `NotificationEventHub` acts as the central mediator:

```
                    User Interaction
                          │
         ┌────────────────┼────────────────┐
         ▼                ▼                ▼
┌─────────────┐  ┌──────────────┐  ┌────────────────┐
│Achievement  │  │DOS Terminal  │  │Weather API     │
│Tracker      │  │Controller    │  │Integration     │
└──────┬──────┘  └──────┬───────┘  └────────┬───────┘
       │                │                   │
       └────────────────┼───────────────────┘
                        ▼
┌─────────────────────────────────────┐
│    NotificationEventHub (HUB)       │  ← CENTRAL MEDIATOR
│   - Network Broadcasting            │
│   - Event Routing                   │
│   - Queue Management                │
└─────────────┬───────────────────────┘
              │
       ┌──────┴──────┐
       ▼             ▼
┌────────────┐  ┌────────────────┐
│Xbox        │  │Notification    │
│Notification│  │Roles Module    │
│UI (FIFO)   │  │                │
└────────────┘  └────────────────┘
       │
       ▼
┌─────────────────────────────┐
│  AchievementDataManager     │
│  (VRChat PlayerData)        │
└─────────────────────────────┘
```

### Key Principles

1. **NO Direct Component Coupling**: Components NEVER call each other directly
2. **Hub Mediates ALL Events**: All cross-component communication flows through `NotificationEventHub`
3. **Read-Only Data Access OK**: Components CAN read from `AchievementDataManager` directly
4. **Hub Handles Network Sync**: Only the hub uses `[UdonSynced]` for cross-component state

### Agent Responsibilities

| Agent | Hub-Spoke Responsibility |
|-------|-------------------------|
| **udonsharp-developer** | Ensures code uses `eventHub.Broadcast*()` methods (no direct component refs) |
| **unity-mcp-specialist** | Validates scripts for hub-spoke violations using `find_in_file` |
| **vrchat-game-developer** | Games broadcast achievements via hub (not directly to UI) |
| **terminal-ui-designer** | Terminal is a spoke (receives updates FROM hub) |
| **vrchat-qa-tester** | Tests hub-spoke event flows (integration tests) |

**See `.claude/Claude.MD` Lines 247-323 for complete architecture diagram.**

---

## 🧪 Test-Driven Development (TDD)

**Selective TDD is recommended for critical features.**

### When to Use TDD (Decision Matrix)

| Feature Type | Use TDD? | Rationale |
|--------------|----------|-----------|
| Achievement unlock logic | ✅ YES | Complex edge cases, regression-prone |
| Hub-spoke event routing | ✅ YES | Critical architecture pattern |
| Quest caching systems | ✅ YES | Performance-critical, must not break |
| PlayerData persistence | ✅ YES | Data integrity critical |
| Network sync logic | ✅ YES | Hard to debug in multiplayer |
| Terminal UI layout | ❌ NO | Visual, changes frequently |
| Audio/visual effects | ❌ NO | Subjective, hard to test |
| One-off utility scripts | ❌ NO | Not worth test overhead |
| Rapid prototyping | ❌ NO | Slows iteration |

### TDD Workflow (Red → Green → Refactor)

1. **Write Test FIRST** (using `[ContextMenu]` pattern)
2. **Run Test** → Should FAIL (Red phase)
3. **Write Code** → Minimal implementation to pass test
4. **Run Test** → Should PASS (Green phase)
5. **Refactor** → Improve code while keeping tests green

### Example Test Method Pattern

```csharp
// From Docs/Reference/TDD_Guidelines_UdonSharp.md

[ContextMenu("Test: Heavy Rain Achievement - Valid")]
public void Test_HeavyRainAchievement_Valid()
{
    // Arrange
    string condition = "Heavy Rain";

    // Act
    CheckWeatherAchievements(condition);

    // Assert
    bool hasAchievement = achievementDataManager.HasAchievement(
        Networking.LocalPlayer.displayName,
        "HEAVY_RAIN"
    );
    Debug.Log("Test Heavy Rain (Valid): " +
              (hasAchievement ? "PASS ✅" : "FAIL ❌"));
}
```

### TDD Resources

- **Complete Guide**: `Docs/Reference/TDD_Guidelines_UdonSharp.md`
- **Testing Patterns**: AAA (Arrange-Act-Assert), edge cases, hub-spoke integration tests
- **Unity MCP Automation**: Automated test execution via `unity-mcp-specialist` agent
- **Example Test Suites**: Achievement tests, network sync tests, boundary tests

### Agent Responsibilities

| Agent | TDD Responsibility |
|-------|-------------------|
| **vrchat-qa-tester** | Designs test cases, suggests test methods following TDD_Guidelines |
| **udonsharp-developer** | Implements test methods, writes code to pass tests |
| **unity-mcp-specialist** | Automates test execution (`run_tests`, `read_console`) |
| **vrchat-game-developer** | Uses TDD for collision detection, scoring, state machines |

**Principle**: Use TDD where it adds value (critical features). Skip TDD where it's overhead (UI, prototypes).

---

## Branch Information

- **Feature Branch:** `modularize-dos-terminal`
- **Development Branch:** `Development`

## Quick Reference

### UdonSharp Prohibited Features
```
❌ Properties (get/set)
❌ LINQ (.Where(), .Select(), etc.)
❌ async/await
❌ try/catch
❌ foreach
❌ List<T>, Dictionary<K,V>
❌ ?. ?? operators
❌ $"" string interpolation
```

### Required Patterns
```
✅ Utilities.IsValid() for VRChat objects
✅ SendCustomEventDelayedSeconds() for delays
✅ Traditional for loops
✅ Public fields instead of properties
✅ "" + var string concatenation
✅ StringBuilder for complex displays
```

### Project Coding Standards
```
✅ Docstring format (COMPONENT PURPOSE, INTEGRATION, etc.)
✅ Section headers (=== format)
✅ ValidateReferences() in Start()
✅ StringBuilder pre-allocated as field
```

See `Docs/Reference/UdonSharp_Code_Review_Checklist.md` for complete standards.

### Terminal Dimensions
```
Width: 80 characters
Border: ═ ║ ╔ ╗ ╚ ╝ ╠ ╣
Colors: #00FF00 (green), #FFFF00 (yellow), #FF0000 (red)
```
