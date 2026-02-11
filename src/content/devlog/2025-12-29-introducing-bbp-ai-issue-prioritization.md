---
title: "Introducing BBP: An AI-Driven Issue Prioritization System"
date: 2025-12-29
tags: ["automation", "ai", "workflow", "project-management", "meta"]
description: "Building a system where agents don't just log bugs — they triage and prioritize them like a real team."
---

After successfully having Claude Code autonomously implement `music.exe`—a Basement OS terminal app that integrates with ProTV 3.x for real-time playlist browsing and playback control—I wanted to scale that approach. Instead of picking issues randomly or by gut feeling, what if AI could act as a SCRUM master and pre-spec *everything*?

### The Challenge

With 58 open issues across features, bugs, concepts, and epics, there was no clear way to know:
- Which issues Claude Code could handle autonomously
- Which had the highest "basement nostalgia" impact
- How to balance effort vs. payoff

I needed a system that would lay out work explicitly, so I could return and start building immediately.

### The Solution

I created **Basement Build Priority (BBP)**—a scoring formula:

```
BBP = (Agentic_Feasibility × Nostalgia_Score) / Story_Points
```

| Metric | Range | Purpose |
|--------|-------|---------|
| Agentic Feasibility | 0-100% | Can Claude + Unity MCP complete this? |
| Nostalgia Score | 1-10 | Does it make the basement feel alive? |
| Story Points | 1-21 | Fibonacci effort scale |

Claude analyzed all 58 issues, assigned scores, and applied a **"Good Agentic Build"** label to 36 issues with ≥70% feasibility. The result: a prioritized backlog where high-BBP items are high-automation, high-nostalgia, low-effort wins.

### Why It Matters

This shifts Claude from "assistant" to "project manager." Instead of asking "what should I work on?", the backlog is pre-scored and ready. If the Agentic + MCP combo continues to prove itself (like it did with music.exe), it will take over a meaningful portion of the workload—in theory I could assign Claude to work on agentic issues while I focus on the non-agentic ones. My bandwidth is now "monitoring and review" rather than code, integrate, test, verify.

### The Paradigm Shift

This represents the third evolution in my AI journey:

| Phase | Mindset | Limiting Factor |
|-------|---------|-----------------|
| **Before AI** | "What *can* I build?" | Skill |
| **With AI** | "What *should* I build?" | Imagination |
| **Agentic AI** | "What will Claude build while I review?" | Bandwidth |

**Key Insight:** Pre-scoring issues with AI as SCRUM master (and reviewing its accuracy) means when I dedicate time to build, I can put Claude to work on low-impact agentic issues while I tackle the highest-impact ones—and have a head start. It could in theory complete an issue 60% of the way if it has 60% agentic feasibility, and I only have to finish the last 40% instead of 100%.

