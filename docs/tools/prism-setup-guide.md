# PRISM Setup Guide

**Priority Rating, Issue Spec & Management**

A drop-in GitHub Action framework that uses Claude to automatically analyze, score, and spec out new issues - turning vague requests into actionable development tickets.

---

## Table of Contents

1. [Value Proposition](#value-proposition)
2. [Quick Start](#quick-start)
3. [BBP Scoring System](#bbp-scoring-system)
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
│  PRISM Automated (per issue):                                            │
│    - Workflow runs automatically:         0 min (human)                  │
│    - Human review of PRISM output:        2-5 min                        │
│    ─────────────────────────────────────────────────                     │
│    TOTAL per issue:                       2-5 min                        │
│                                                                          │
│  SAVINGS per issue:                       25-55 min (85-90%)             │
├─────────────────────────────────────────────────────────────────────────┤
│  Weekly ROI (10 issues/week):             4-9 hours saved                │
│  Monthly ROI (40 issues/month):           16-36 hours saved              │
└─────────────────────────────────────────────────────────────────────────┘
```

### What PRISM Does

1. **Triggers** on new issue creation
2. **Searches** your codebase for related patterns
3. **Calculates** a priority score (BBP)
4. **Generates** an implementation spec with phases
5. **Applies** labels and assigns for review
6. **Posts** a structured analysis comment

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
- `.github/workflows/prism-auto-triage.yml` (required)
- `.github/workflows/auto-build-on-ready.yml` (optional)

### Step 3: Create Required Labels

Create these labels in your repository (**Settings** > **Labels**):

| Label | Hex Color | Description |
|-------|-----------|-------------|
| `priority: critical` | `#b60205` | BBP 120+ - Do immediately |
| `priority: high` | `#d93f0b` | BBP 90-119 - Do soon |
| `priority: medium` | `#fbca04` | BBP 60-89 - Normal priority |
| `priority: low` | `#0e8a16` | BBP <60 - When time permits |
| `needs-review` | `#5319e7` | Awaiting human review |
| `ready-for-dev` | `#0075ca` | Approved for development |
| `Good Agentic Build` | `#1d76db` | High feasibility (70%+) |
| `skip-prism` | `#cccccc` | Skip PRISM analysis |

**Quick CLI label creation:**
```bash
gh label create "priority: critical" --color "b60205" --description "BBP 120+"
gh label create "priority: high" --color "d93f0b" --description "BBP 90-119"
gh label create "priority: medium" --color "fbca04" --description "BBP 60-89"
gh label create "priority: low" --color "0e8a16" --description "BBP <60"
gh label create "needs-review" --color "5319e7" --description "Awaiting human review"
gh label create "ready-for-dev" --color "0075ca" --description "Approved for development"
gh label create "Good Agentic Build" --color "1d76db" --description "High feasibility (70%+)"
gh label create "skip-prism" --color "cccccc" --description "Skip PRISM analysis"
```

### Step 4: Test

1. Create a new issue with a clear feature request
2. Wait 2-3 minutes for PRISM to analyze
3. Check for the PRISM Analysis comment and applied labels

---

## BBP Scoring System

### Formula

```
BBP = (Story Points x 10) + (Impact x 5) + Feasibility Bonus
```

### Story Points (1-8)

| Points | Scope | Time Estimate |
|--------|-------|---------------|
| 1 | Trivial | < 1 hour |
| 2 | Small | 1-2 hours |
| 3 | Medium | Half day |
| 5 | Large | Full day |
| 8 | XL | Multiple days |

### Impact (1-10)

**For User-Facing Work** ("How much will users love this?"):
- 9-10: Core features, major wow moments
- 7-8: Noticeable delight, visible improvements
- 5-6: Nice to have, some users appreciate
- 3-4: Minor polish, few will notice
- 1-2: Tiny tweaks

**For Developer Work** ("How much will this speed up development?"):
- 9-10: Saves 30+ min/day, critical automation
- 7-8: Saves 10-30 min/day, workflow improvements
- 5-6: Occasional time saver
- 3-4: Minor convenience
- 1-2: Marginal improvement

### Feasibility Bonus

| Range | Bonus | Meaning |
|-------|-------|---------|
| 90-100% | +20 | Fully automatable, clear spec, proven patterns |
| 70-89% | +10 | Mostly automatable, may need minor clarification |
| <70% | +0 | Needs significant human decisions |

### Priority Thresholds

| BBP Range | Priority | Action |
|-----------|----------|--------|
| 120+ | Critical | Do immediately |
| 90-119 | High | Do this sprint |
| 60-89 | Medium | Normal backlog |
| <60 | Low | When time permits |

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
│  needs-review   │◄──── Applied by PRISM (always)
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

# Skip PRISM for an issue
gh issue edit [NUMBER] --add-label "skip-prism"
```

---

## Workflow Files

### prism-auto-triage.yml (Required)

Copy this to `.github/workflows/prism-auto-triage.yml`:

```yaml
name: PRISM Auto-Triage

on:
  issues:
    types: [opened]

jobs:
  prism-triage:
    # Skip if issue already has BBP table, is from a bot, or has skip-prism label
    if: |
      !contains(github.event.issue.body, '| **BBP**') &&
      !contains(github.event.issue.user.login, '[bot]') &&
      !contains(github.event.issue.labels.*.name, 'skip-prism')
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

      - name: Run PRISM Analysis
        uses: anthropics/claude-code-action@v1
        with:
          claude_code_oauth_token: ${{ secrets.CLAUDE_CODE_OAUTH_TOKEN }}
          prompt: |
            You are PRISM (Priority Rating, Issue Spec & Management) - an expert technical analyst.

            ## Issue to Analyze
            - **Title**: ${{ github.event.issue.title }}
            - **Number**: #${{ github.event.issue.number }}
            - **Body**:
            ```
            ${{ github.event.issue.body }}
            ```

            ## Your Task: Full PRISM Analysis

            ### Step 1: Research (REQUIRED)
            Before scoring, search the codebase for related patterns:
            - Use Glob to find related files
            - Use Grep to find relevant methods or patterns
            - Identify existing implementations to follow

            ### Step 2: Calculate BBP Score

            **Formula**:
            ```
            BBP = (Story Points x 10) + (Impact x 5) + Feasibility Bonus
            ```

            **Story Points (1-8)**:
            - 1 = Trivial (<1 hour)
            - 2 = Small (1-2 hours)
            - 3 = Medium (half day)
            - 5 = Large (full day)
            - 8 = XL (multiple days)

            **Impact (1-10)**:
            - User items: "How much will users love this?"
            - Dev items: "How much will this speed up development?"

            **Agentic Feasibility (0-100%)**:
            - 90%+: Pure code, clear spec, existing patterns -> Bonus +20
            - 70-89%: Mostly code, may need some manual work -> Bonus +10
            - <70%: Complex work, external deps -> Bonus +0

            ### Step 3: Generate Implementation Spec

            Create a comprehensive spec including:
            1. Summary (1-2 sentences)
            2. Suggested branch name: `feat/[short-name]-issue-${{ github.event.issue.number }}`
            3. Technical analysis with code snippets from existing files
            4. Phased implementation plan with checkboxes
            5. Success criteria
            6. Copy-paste resume prompt

            ### Step 4: Apply Labels

            Use `gh issue edit` to add labels:
            - `priority: critical` if BBP >= 120
            - `priority: high` if BBP 90-119
            - `priority: medium` if BBP 60-89
            - `priority: low` if BBP < 60
            - `Good Agentic Build` if feasibility >= 70%
            - `needs-review` (ALWAYS - human must approve before dev starts)

            ### Step 5: Post Comment

            Use `gh issue comment` with this format:

            ```markdown
            ## PRISM Analysis

            | Metric | Score | Reason |
            |--------|-------|--------|
            | Category | [user/dev]-focused | [why] |
            | Story Points | X | [scope] |
            | Impact | X/10 | [value delivered] |
            | Agentic Feasibility | X% | [factors] |
            | **BBP** | **X** | (SPx10)+(Impactx5)+Bonus |

            **Priority**: [Critical/High/Medium/Low]
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

            *Auto-generated by PRISM. Human review required before development.*
            *Remove `needs-review` and add `ready-for-dev` to approve.*
            ```

            ## Important Notes

            - ALWAYS search the codebase before scoring (use Glob/Grep)
            - ALWAYS add `needs-review` label - only humans approve work
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
                body: `## Auto-Build Skipped\n\nThis issue has \`ready-for-dev\` but NOT \`Good Agentic Build\` label.\n\n**Reason**: Low agentic feasibility (<70%) - requires manual guidance.\n\n**To start manually**: \`@claude Resume issue #${issueNumber}\`\n\n---\n*Auto-build requires both \`ready-for-dev\` AND \`Good Agentic Build\` labels.*`
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

            1. **Read the PRISM spec** from the issue comments (look for "## PRISM Analysis")
            2. **Follow the implementation plan** from the PRISM spec
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

Add project context to the PRISM prompt for better analysis:

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

### Custom Labels

Replace priority labels with your team's conventions:

```yaml
# Instead of priority: critical/high/medium/low
- `P0` if BBP >= 120
- `P1` if BBP 90-119
- `P2` if BBP 60-89
- `P3` if BBP < 60
```

### Assignee Configuration

Add automatic assignment by adding to the labels step:
```bash
gh issue edit ${{ github.event.issue.number }} --add-assignee "your-username"
```

---

## Troubleshooting

### PRISM Does Not Trigger

**Check**:
1. Is the workflow file in `.github/workflows/`?
2. Does the issue already have a BBP table? (skipped to prevent duplicates)
3. Is the issue from a bot? (skipped by default)
4. Does the issue have `skip-prism` label?

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
| 1.0.0 | February 2026 | Initial release |

---

*Built with Claude Code Action by Anthropic*
