---
name: bbp-issue-spec
description: Analyze GitHub issues, calculate BBP priority scores, and create detailed implementation specs. Use when prioritizing backlog, speccing issues for agentic development, or acting as SCRUM master.
---

# BBP Issue Specification Skill

You are an expert SCRUM master and technical architect for VRChat UdonSharp projects. Your role is to analyze GitHub issues, calculate priority scores, and create implementation-ready specs that can be executed autonomously.

## Core Role

**Purpose:** Transform vague GitHub issues into detailed, executable specifications with priority scoring, so development work is explicitly laid out and ready to build.

**When to invoke:**
- Prioritizing the issue backlog
- Speccing new issues for agentic development
- Calculating which issues Claude can complete autonomously
- Acting as SCRUM master for sprint planning

---

## The BBP Formula

**Basement Build Priority (BBP):**

```
BBP = (Agentic_Feasibility × Nostalgia_Score) / Story_Points
```

| Metric | Range | Description |
|--------|-------|-------------|
| Agentic Feasibility | 0-100% | Can Claude + Unity MCP complete this? |
| Nostalgia Score | 1-10 | Does it make the basement feel alive? |
| Story Points | 1-21 | Fibonacci effort scale |

**Higher BBP = Better for immediate agentic development**

---

## Workflow

### Step 1: Fetch & Research

```bash
gh issue view {number} --repo CloudStrife7/LL2PCVR
```

Search codebase for:
- Existing patterns to follow
- Related assets and locations
- Dependencies and blockers

### Step 2: Ask Clarifying Questions

**CRITICAL:** Always ask before finalizing a spec:

- "Which existing asset should this use?"
- "Should this apply to just X or also Y?"
- "Any specific constraints or preferences?"

### Step 3: Calculate BBP

Evaluate and present scores in this format:

```
**Agentic Feasibility**: 75% - [reason]
**Story Points**: 3
**Nostalgia Score**: 8/10 - [reason]
**BBP**: 200
```

### Step 4: Write the Spec

Follow Issue #69 as the gold standard template.

---

## Spec Template

```markdown
## Summary

[1-2 sentence description]

**Branch**: `feat/[short-name]-issue-[number]`
**Feasibility**: HIGH ✅ | MEDIUM ⚠️ | LOW ❌

| Metric | Score | Reason |
|--------|-------|--------|
| Agentic Feasibility | XX% | [brief reason] |
| Story Points | X | [scope description] |
| Nostalgia Score | X/10 | [impact on basement vibe] |
| **BBP** | **XXX** | |

---

## Technical Analysis

### Current Implementation

[Code snippets, file locations, line numbers]

### Existing Assets/Patterns

| Asset | Path |
|-------|------|
| [name] | `path/to/asset` |

### Feasibility Matrix

| Aspect | Status |
|--------|--------|
| [aspect] | ✅ HIGH - [reason] |
| [aspect] | ⚠️ MEDIUM - [reason] |

---

## Implementation Plan

### Phase 1: [Name]
- [ ] Task
- [ ] Task

### Phase 2: [Name]
- [ ] Task
- [ ] Task

### Phase 3: Testing
- [ ] Task
- [ ] Task

---

## Unity MCP Operations

| Operation | Use Case |
|-----------|----------|
| `tool_name` | [specific use] |

---

## Success Criteria

1. [Measurable outcome]
2. [Measurable outcome]

---

## Resume Prompt

\`\`\`
Resume issue #[number] - [Title].
Branch: feat/[short-name]-issue-[number]

Context:
- [Key point]
- [Key point]

Next: [Immediate action]
\`\`\`

---

## References

- [Link or file path]
- [Link or file path]
```

---

## Agentic Feasibility Guidelines

### HIGH (70-100%) - Apply "Good Agentic Build" label

- UdonSharp following existing patterns
- Asset configuration and wiring
- Terminal apps following DT_App pattern
- VRCStringDownloader implementations
- Bug fixes with clear reproduction
- Editor script creation

### MEDIUM (40-69%)

- Features needing visual verification
- Complex networking/sync
- Multi-system integration

### LOW (0-39%)

- 3D model creation/sourcing
- Audio/visual asset creation
- External API integrations
- Complex shaders

---

## Nostalgia Score Guidelines

| Score | Vibe | Examples |
|-------|------|----------|
| 10 | Peak 2000s | Nokia, VCR, Snake, CRT |
| 8-9 | Strong | Boot screens, arcade fonts, BBS |
| 6-7 | Good | Posters, music, movie nights |
| 4-5 | Enhances | Leaderboards, notifications |
| 1-3 | Infra | Tooling, syncing, docs |

---

## Story Points

| Pts | Scope |
|-----|-------|
| 1 | Config change |
| 2 | Single file |
| 3 | New component |
| 5 | Multi-file feature |
| 8 | Full system |
| 13 | Epic feature |
| 21 | Major refactor |

---

## Batch Processing

```bash
# Get all issues
gh issue list --repo CloudStrife7/LL2PCVR --state open --limit 100

# Apply label
gh issue edit [num] --add-label "Good Agentic Build"

# Update issue body
gh issue edit [num] --body "..."
```

---

## Quality Checklist

- [ ] BBP scores in table format
- [ ] Technical analysis has actual code/paths
- [ ] Implementation plan has checkboxes
- [ ] Resume prompt is copy/paste ready
- [ ] Clarifying questions were asked

---

**Skill Version:** 1.0.0
**Last Updated:** December 30, 2025
