---
description: Create a new devlog entry using an interactive interview process.
---

# Devlog Creation Workflow

This workflow guides you through creating a new devlog entry that focuses on *learning* and *methodology* rather than just features.

## Step 1: Initial Setup

1.  **Ask the User:** "What type of update is this? Please choose one:\n\n*   **[Milestone]** - Major feature completion or system breakthrough.\n*   **[TIL]** - Practical 'Today I Learned' technical nugget.\n*   **[Meta]** - Reflection on process, tools (like this devlog system), or workflow."
2.  **Wait** for the user's selection.

## Step 2: The Hook

1.  **Ask the User:** "What is your one-liner takeaway? (This will be the main hook of the post)."
2.  **Wait** for the user's response.

## Step 3: The Reflection Interview

Based on the User's choice in Step 1, ask the corresponding set of questions. **Ask them one by one or grouped, whichever feels more natural.**

### If [Milestone]:
*   "What was the main constraint or challenge that needed solving?"
*   "What technique did you learn or apply to solve it?"
*   "What does this prove you can do now? (The career/skill implication)"

### If [TIL]:
*   "What was the blocker? Why did this take time?"
*   "What is the specific solution/snippet/rule?"
*   "Where else does this knowledge apply?"

### If [Meta]:
*   "What triggered this reflection? What wasn't working?"
*   "What principle did you extract? What's the new workflow?"
*   "How will this change your development process going forward?"

**Wait** for the user's responses.

## Step 4: Draft Generation

1.  **Synthesize** the user's answers into a new Markdown file.
2.  **Target Path:** `src/content/devlog/YYYY-MM-DD-slug-from-hook.md`
3.  **Frontmatter:**
    *   `title`: The One-Liner Hook (formatted as a title)
    *   `date`: Current Date (YYYY-MM-DD)
    *   `tags`: Generate 3-5 relevant tags based on content (e.g. `["automation", "ai", "workflow"]`)
4.  **Content Structure:**

```markdown
---
(Frontmatter)
---

(A short opening paragraph introducing the context/problem, using the "Hook" info.)

### The Challenge
(Synthesis of the "Problem" answer)

### The Solution
(Synthesis of the "Solution" answer)

### Why It Matters
(Synthesis of the "Learning/Implication" answer)

**Key Insight:** (A bolded summary sentence)
```

5.  **Present** the drafted content to the User for review.

## Step 5: Finalize

1.  **Ask the User:** "Does this look good to save? Or would you like to tweak anything?"
2.  If **Approved**:
    *   Use `write_to_file` to save the file to `src/content/devlog/...`.
    *   Run `npm run build` to verify it builds.
    *   Inform the user it's live.
3.  If **Revisions Needed**:
    *   Ask for specific changes, update the draft, and repeat Step 5.
