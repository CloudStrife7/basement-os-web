---
title: "Have AI Research Tools to Improve Its Own Workflow"
date: 2026-02-19
type: meta
tags: ["automation", "ai", "self-improvement", "agentic", "meta"]
description: "Designing a pipeline where Claude Code subscribes to newsletters, finds relevant articles, and auto-creates GitHub issues for workflow improvements."
---

What if your AI assistant could find its own upgrades?

### The Idea

Today I designed a pipeline where Claude Code subscribes to the TLDR AI newsletter, reads incoming articles, and decides which ones could improve its own development workflow. When it finds something useful, it auto-creates a GitHub issue with a cost-benefit analysis.

The AI researches tools to make itself better.

### The Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                            NAS                                  │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐    ┌──────────────┐    ┌────────────────────┐  │
│  │ Proton      │───▶│ n8n/cron     │───▶│ Claude Code (OAuth)│  │
│  │ Bridge      │    │ (scheduler)  │    │ (relevance scoring)│  │
│  │ IMAP        │    │ Daily 9 AM   │    │                    │  │
│  └─────────────┘    └──────────────┘    └────────────────────┘  │
│                                                   │             │
│                                                   ▼             │
│                                         ┌──────────────────┐    │
│                                         │ gh issue create  │    │
│                                         │ (high relevance) │    │
│                                         └──────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                                   │
                                                   ▼
                                         GitHub Issues (Backlog)
```

### The Workflow

1. Newsletter arrives daily
2. AI scans summaries, filters for relevance
3. For promising articles, it fetches and reads the full content
4. Extracts implementable ideas with effort estimates
5. Auto-creates issues if the FORGE score is high enough
6. Presents valuable skills to me for review before implementation

No human in the loop until review time.

### Why It Matters

This flips the traditional model. Instead of me finding tools and teaching the AI, the AI scouts for improvements and presents them to me. It's a self-evolving development assistant that grows its own capabilities over time.

**Key Insight:** The most powerful AI workflows are the ones where the AI improves itself.
