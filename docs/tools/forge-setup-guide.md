# FORGE Setup Guide

**Focus, Order, Rating, Generation, Execution**

Our issue intake system that transforms raw ideas into priority-scored, implementation-ready specs. Like the Norse dwarves who forged legendary weapons, FORGE crafts actionable plans from raw materials.

---

## Table of Contents

1. [Value Proposition](#value-proposition)
2. [Quick Start](#quick-start)
3. [Scoring System](#scoring-system)
4. [Label Workflow](#label-workflow)
5. [Workflow Files](#workflow-files)
6. [Customization](#customization)
7. [Troubleshooting](#troubleshooting)

---

## Value Proposition

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ROI CALCULATOR                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Manual Issue Triage (per issue):                                        │
│    - Read and understand issue:           5-10 min                       │
│    - Search codebase for context:         5-15 min                       │
│    - Estimate effort/priority:            5-10 min                       │
│    - Write implementation spec:           10-20 min                      │
│    - Apply labels and routing:            2-5 min                        │
│    ─────────────────────────────────────────────────                     │
│    TOTAL per issue:                       27-60 min                      │
│                                                                          │
│  FORGE Automated (per issue):                                            │
│    - Workflow runs automatically:         0 min (human)                  │
│    - Human review of FORGE output:        2-5 min                        │
│    ─────────────────────────────────────────────────                     │
│    TOTAL per issue:                       2-5 min                        │
│                                                                          │
│  SAVINGS per issue:                       25-55 min (85-90%)             │
├─────────────────────────────────────────────────────────────────────────┤
│  Weekly ROI (10 issues/week):             4-9 hours saved                │
│  Monthly ROI (40 issues/month):           16-36 hours saved              │
└─────────────────────────────────────────────────────────────────────────┘
```

### What FORGE Does

1. **Triggers** on new issue creation
2. **Searches** your codebase for related patterns
3. **Estimates** story points and automation candidacy
4. **Generates** an implementation spec with phases
5. **Applies** labels (bucket + automation candidate)
6. **Posts** a structured analysis comment with forcing function

---

## Quick Start

### Step 1: Add the OAuth Token Secret

1. Go to your repository **Settings** > **Secrets and variables** > **Actions**
2. Click **New repository secret**
3. Name: `CLAUDE_CODE_OAUTH_TOKEN`
4. Value: Your Claude Code OAuth token
5. Click **Add secret**

### Step 2: Create the Workflow Files

Copy the workflow YAML from [Workflow Files](#workflow-files) section into:
- `.github/workflows/forge-auto-triage.yml` (required)
- `.github/workflows/auto-build-on-ready.yml` (optional)

### Step 3: Create Required Labels

Create these labels in your repository (**Settings** > **Labels**):

**Bucket Labels:**
| Label | Hex Color | Description |
|-------|-----------|-------------|
| `🥇 Focus (1 Max)` | `#ff0000` | 🥇 GOLD - Ship This Week |
| `🥈 Next Up (3 Max)` | `#bfd4f2` | 🥈 SILVER - Next in queue |
| `🥉 Someday (Unlimited)` | `#e99695` | 🥉 BRONZE - Backlog (default) |

**Story Point Labels:**
| Label | Hex Color |
|-------|-----------|
| `SP: 1` | `#c5def5` |
| `SP: 2` | `#c5def5` |
| `SP: 3` | `#c5def5` |
| `SP: 5` | `#c5def5` |
| `SP: 8` | `#c5def5` |

**Workflow Labels:**
| Label | Hex Color | Description |
|-------|-----------|-------------|
| `needs-review` | `#5319e7` | Awaiting human review |
| `ready-for-dev` | `#0075ca` | Approved for development |
| `Good Agentic Build` | `#1d76db` | Automation Candidate = Y |
| `skip-forge` | `#cccccc` | Skip FORGE analysis |

**Quick CLI label creation:**
```bash
# Bucket labels
gh label create "🥇 Focus (1 Max)" --color "ff0000" --description "GOLD - Ship This Week"
gh label create "🥈 Next Up (3 Max)" --color "bfd4f2" --description "SILVER - Next in queue"
gh label create "🥉 Someday (Unlimited)" --color "e99695" --description "BRONZE - Backlog"

# Story point labels
gh label create "SP: 1" --color "c5def5" --description "Trivial (<1 hour)"
gh label create "SP: 2" --color "c5def5" --description "Small (1-2 hours)"
gh label create "SP: 3" --color "c5def5" --description "Medium (half day)"
gh label create "SP: 5" --color "c5def5" --description "Large (full day)"
gh label create "SP: 8" --color "c5def5" --description "XL (multiple days)"

# Workflow labels
gh label create "needs-review" --color "5319e7" --description "Awaiting human review"
gh label create "ready-for-dev" --color "0075ca" --description "Approved for development"
gh label create "Good Agentic Build" --color "1d76db" --description "Automation Candidate = Y"
gh label create "skip-forge" --color "cccccc" --description "Skip FORGE analysis"
```

### Step 4: Test

1. Create a new issue with a clear feature request
2. Wait 2-3 minutes for FORGE to analyze
3. Check for the FORGE Analysis comment and applied labels

---

## Scoring System

FORGE v2.0 uses a simplified bucket-based system with a **forcing function**:

> **Work ONLY on 🥇 or 🥈 features. Everything else waits.**

### The Three Buckets

```
🥇 GOLD (Ship This Week)   🥈 SILVER (Next Up)       🥉 BRONZE (Someday/Maybe)
────────────────────────   ───────────────────       ─────────────────────────
MAX: 1 feature             MAX: 3 features           UNLIMITED backlog
Must ship by Friday        Priority ranked 1-3       New ideas go here (default)
```

| Bucket | Max Items | Purpose |
|--------|-----------|---------|
| 🥇 GOLD | 1 | Current sprint focus |
| 🥈 SILVER | 3 (ranked) | Next up queue |
| 🥉 BRONZE | Unlimited | Idea backlog |

### Story Points (1-8)

Modified Fibonacci scale based on effort:

| SP | Effort | Time Estimate |
|----|--------|---------------|
| 1 | Trivial | < 1 hour |
| 2 | Small | 1-2 hours |
| 3 | Medium | Half day |
| 5 | Large | Full day |
| 8 | XL | Multiple days |

**If SP > 5:** Consider breaking into smaller issues.

### Automation Candidate (Y/N)

Replaces the old feasibility percentage:

| Value | Criteria |
|-------|----------|
| **Y** | Clear requirements, follows existing patterns, pure code changes |
| **N** | Requires Unity scene work, creative decisions, external setup |

Issues with `Good Agentic Build` label (Automation Candidate = Y) can be auto-implemented.

### The Forcing Function

**Monday-Friday:** Work ONLY on 🥇 or 🥈 features.

**Saturday:** Wildcard day - can work on 🥉 or fun projects.

**Sunday:** Weekly planning and bucket review.

---

## Label Workflow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        LABEL STATE MACHINE                               │
└─────────────────────────────────────────────────────────────────────────┘

    Issue Created
         │
         ▼
┌─────────────────┐
│  needs-review   │◄──── Applied by FORGE (always)
└────────┬────────┘
         │
         ▼
   Human Reviews
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌────────┐  ┌────────┐
│APPROVE │  │ REJECT │
└────┬───┘  └────┬───┘
     │           │
     ▼           ▼
┌─────────────┐  ┌──────────┐
│ready-for-dev│  │ wontfix  │
└──────┬──────┘  │ or close │
       │         └──────────┘
       ▼
┌─────────────────────────────┐
│ Has "Good Agentic Build"?   │
└──────┬──────────────┬───────┘
       │              │
      YES            NO
       │              │
       ▼              ▼
┌────────────┐  ┌─────────────────┐
│ Auto-Build │  │ Manual Start    │
│ Triggered  │  │ Required        │
└────────────┘  └─────────────────┘
```

### Quick Commands

```bash
# Approve an issue
gh issue edit [NUMBER] --remove-label "needs-review" --add-label "ready-for-dev"

# Reject an issue
gh issue close [NUMBER] --comment "Closing: [reason]"

# Skip FORGE for an issue
gh issue edit [NUMBER] --add-label "skip-forge"
```

---

## Workflow Files

### forge-auto-triage.yml (Required)

Copy this to `.github/workflows/forge-auto-triage.yml`:

```yaml
name: FORGE Auto-Triage

on:
  issues:
    types: [opened]

jobs:
  forge-triage:
    # Skip if issue already analyzed, is from a bot, or has skip-forge label
    if: |
      !contains(github.event.issue.body, '## FORGE Analysis') &&
      !contains(github.event.issue.user.login, '[bot]') &&
      !contains(github.event.issue.labels.*.name, 'skip-forge')
    runs-on: ubuntu-latest
    permissions:
      contents: read
      issues: write
      id-token: write
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          fetch-depth: 1

      - name: Run FORGE Analysis
        uses: anthropics/claude-code-action@v1
        with:
          claude_code_oauth_token: ${{ secrets.CLAUDE_CODE_OAUTH_TOKEN }}
          prompt: |
            You are FORGE (Focus, Order, Rating, Generation, Execution) - an expert technical analyst who forges implementation-ready specs from raw ideas.

            ## Issue to Analyze
            - **Title**: ${{ github.event.issue.title }}
            - **Number**: #${{ github.event.issue.number }}
            - **Body**:
            ```
            ${{ github.event.issue.body }}
            ```

            ## Your Task: Full FORGE Analysis

            ### Step 1: Research (REQUIRED)
            Before scoring, search the codebase for related patterns:
            - Use Glob to find related files
            - Use Grep to find relevant methods or patterns
            - Identify existing implementations to follow

            ### Step 2: Estimate Story Points

            **Story Points (1-8)**:
            - 1 = Trivial (<1 hour)
            - 2 = Small (1-2 hours)
            - 3 = Medium (half day)
            - 5 = Large (full day)
            - 8 = XL (multiple days)

            If SP > 5, suggest breaking into smaller issues.

            ### Step 3: Determine Automation Candidate

            **Automation Candidate (Y/N)**:
            - Y = Clear requirements, follows existing patterns, pure code changes
            - N = Requires Unity scene work, creative decisions, external setup

            ### Step 4: Generate Implementation Spec

            Create a comprehensive spec including:
            1. Summary (1-2 sentences)
            2. Suggested branch name: `feat/[short-name]-issue-${{ github.event.issue.number }}`
            3. Technical analysis with code snippets from existing files
            4. Phased implementation plan with checkboxes
            5. Success criteria
            6. Copy-paste resume prompt

            ### Step 5: Apply Labels

            Use `gh issue edit` to add labels:
            - `🥉 Someday (Unlimited)` (ALL new issues start in BRONZE)
            - `SP: X` where X is the story point estimate
            - `Good Agentic Build` if Automation Candidate = Y
            - `needs-review` (ALWAYS - human must approve before dev starts)

            ### Step 6: Post Comment with Forcing Function

            Use `gh issue comment` with this format:

            ```markdown
            ## FORGE Analysis

            | Metric | Value | Reason |
            |--------|-------|--------|
            | Story Points | SP: X | [scope reasoning] |
            | Automation Candidate | Y/N | [why] |
            | Bucket | 🥉 BRONZE | New issues start here |

            **Branch**: `feat/[name]-issue-${{ github.event.issue.number }}`

            ---

            ## Implementation Spec

            ### Summary
            [1-2 sentence description]

            ### Technical Analysis

            **Existing Patterns**:
            [Code snippets from codebase search with file paths]

            **Dependencies**:
            - [Dependency 1]
            - [Dependency 2]

            ### Implementation Plan

            #### Phase 1: Setup
            - [ ] [Task 1]
            - [ ] [Task 2]

            #### Phase 2: Core Logic
            - [ ] [Task 3]
            - [ ] [Task 4]

            #### Phase 3: Testing
            - [ ] [Test requirement]

            ### Success Criteria

            1. [Measurable outcome 1]
            2. [Measurable outcome 2]

            ---

            ## Resume Prompt

            ```
            Resume issue #${{ github.event.issue.number }} - ${{ github.event.issue.title }}
            Branch: feat/[name]-issue-${{ github.event.issue.number }}

            Context:
            - [Key point 1]
            - [Key point 2]

            Next: [Specific next action]
            ```

            ---

            ## 🔥 Forcing Function

            **Should this be promoted?**
            - Reply `promote to gold` → Make it your 🥇 Ship This Week
            - Reply `promote to silver` → Add to 🥈 Next Up queue
            - Reply `keep in bronze` → Leave for Sunday review

            *Auto-generated by FORGE. Human review required before development.*
            *Remove `needs-review` and add `ready-for-dev` to approve.*
            ```

            ## Important Notes

            - ALWAYS search the codebase before scoring (use Glob/Grep)
            - ALWAYS add `needs-review` label - only humans approve work
            - ALL new issues default to 🥉 BRONZE bucket
            - If issue is unclear, add clarifying questions in the comment

          # Allow the workflow to add labels and comments
          claude_args: '--allowedTools Bash(gh issue edit:*) Bash(gh issue comment:*) Read Glob Grep'
```

### auto-build-on-ready.yml (Optional)

Copy this to `.github/workflows/auto-build-on-ready.yml`:

```yaml
name: Auto-Build on Ready

on:
  issues:
    types: [labeled]

jobs:
  check-and-build:
    # Only run when ready-for-dev label is added
    if: github.event.label.name == 'ready-for-dev'
    runs-on: ubuntu-latest
    permissions:
      contents: write
      issues: write
      pull-requests: write
      id-token: write

    steps:
      - name: Check for Good Agentic Build label
        id: check-gab
        uses: actions/github-script@v7
        with:
          script: |
            const labels = context.payload.issue.labels.map(l => l.name);
            const hasGAB = labels.includes('Good Agentic Build');
            const issueNumber = context.payload.issue.number;
            const issueTitle = context.payload.issue.title;

            console.log(`Issue #${issueNumber}: ${issueTitle}`);
            console.log(`Labels: ${labels.join(', ')}`);
            console.log(`Has Good Agentic Build: ${hasGAB}`);

            if (!hasGAB) {
              await github.rest.issues.createComment({
                owner: context.repo.owner,
                repo: context.repo.repo,
                issue_number: issueNumber,
                body: `## Auto-Build Skipped\n\nThis issue has \`ready-for-dev\` but NOT \`Good Agentic Build\` label.\n\n**Reason**: Automation Candidate = N - requires manual guidance.\n\n**To start manually**: \`@claude Resume issue #${issueNumber}\`\n\n---\n*Auto-build requires both \`ready-for-dev\` AND \`Good Agentic Build\` labels.*`
              });
              return { proceed: false };
            }

            return {
              proceed: true,
              issueNumber: issueNumber,
              issueTitle: issueTitle
            };

      - name: Checkout repository
        if: fromJSON(steps.check-gab.outputs.result).proceed
        uses: actions/checkout@v4
        with:
          fetch-depth: 1

      - name: Run Claude Implementation
        if: fromJSON(steps.check-gab.outputs.result).proceed
        uses: anthropics/claude-code-action@v1
        with:
          claude_code_oauth_token: ${{ secrets.CLAUDE_CODE_OAUTH_TOKEN }}
          prompt: |
            ## Auto-Build Triggered

            Issue #${{ github.event.issue.number }} has been approved for autonomous development.

            **Title**: ${{ github.event.issue.title }}
            **Labels**: ${{ join(github.event.issue.labels.*.name, ', ') }}

            ## Instructions

            1. **Read the FORGE spec** from the issue comments (look for "## FORGE Analysis")
            2. **Follow the implementation plan** from the FORGE spec
            3. **Create a feature branch**: Use the branch name from the spec
            4. **Implement the feature** following the phased plan
            5. **Create a PR** with proper description and link to issue

            ## Issue Body

            ```
            ${{ github.event.issue.body }}
            ```

            ## On Completion

            - Create PR linking to issue #${{ github.event.issue.number }}
            - Comment on the issue with progress summary
            - If blocked, document blockers clearly in a comment

          # Full tool access for implementation
          claude_args: '--allowedTools Bash Read Write Edit Glob Grep'
```

---

## Customization

### Project-Specific Prompts

Add project context to the FORGE prompt for better analysis:

**For Unity/VRChat projects**, add to the prompt:
```yaml
- This is a VRChat UdonSharp project
- Check for UdonSharp constraints (no List<T>, LINQ, try/catch)
- Modifies `Assets/Scripts/**/*.cs` -> UdonSharp code
```

**For Web projects**, add to the prompt:
```yaml
- This is a web project using [React/Vue/etc]
- Check `src/components/` for React components
- Check `src/api/` for API routes
```

### Skill Detection

Add skill detection rules to route issues to appropriate agents:

```yaml
## Skill Detection Rules

| Pattern | Skill | Reason |
|---------|-------|--------|
| `src/components/**/*.tsx` | `/frontend` | React component |
| `src/api/**/*` | `/backend` | API changes |
| `**/*.md` | `/docs` | Documentation |
| `*` | `/general` | Default |
```

### Custom Bucket Names

Replace bucket labels with your team's conventions:

```yaml
# Instead of emoji buckets
- `P0 - Focus` instead of `🥇 Focus (1 Max)`
- `P1 - Next Up` instead of `🥈 Next Up (3 Max)`
- `P2 - Backlog` instead of `🥉 Someday (Unlimited)`
```

### Assignee Configuration

Add automatic assignment by adding to the labels step:
```bash
gh issue edit ${{ github.event.issue.number }} --add-assignee "your-username"
```

---

## Troubleshooting

### FORGE Does Not Trigger

**Check**:
1. Is the workflow file in `.github/workflows/`?
2. Does the issue already have a FORGE Analysis comment? (skipped to prevent duplicates)
3. Is the issue from a bot? (skipped by default)
4. Does the issue have `skip-forge` label?

**Debug**: Check Actions tab for workflow run logs.

### Labels Not Applied

**Check**:
1. Do the labels exist in your repository?
2. Does the workflow have `issues: write` permission?
3. Check the workflow logs for gh CLI errors

### Analysis Is Inaccurate

**Improve accuracy**:
1. Add project-specific context to the prompt
2. Ensure codebase is well-organized
3. Add more detailed issue descriptions

### Auto-Build Does Not Start

**Requirements**:
1. Issue must have `ready-for-dev` label
2. Issue must have `Good Agentic Build` label
3. Both labels must be present simultaneously

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | February 2026 | Rebranded from PRISM to FORGE. Replaced BBP scoring with bucket system (🥇/🥈/🥉). Added forcing function. |
| 1.0.0 | January 2026 | Initial release as PRISM |

---

*Built with Claude Code Action by Anthropic*
