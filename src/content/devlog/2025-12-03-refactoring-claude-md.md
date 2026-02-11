---
title: "Refactoring CLAUDE.md"
date: 2025-12-03
tags: ["documentation", "meta-learning", "ai", "Claude Opus", "Claude Code"]
description: "Using AI to improve how you work with AI. Meta-learning in practice."
---
A coworker sent me [HumanLayer's guide to writing good CLAUDE.md files](https://www.humanlayer.dev/blog/writing-a-good-claude-md), and I couldn't help myself - had to try it immediately.

Opened Claude Opus and fed it my entire 953-line CLAUDE.md for review, citing the HumanLayer article as the comparison benchmark. "How does mine stack up against their recommendations?"

Opus came back with a refactor plan: too many instructions causing "instruction-following decay," embedded code examples getting stale, mixed universal and task-specific rules. The solution? Modular structure - keep CLAUDE.md lean (~150 lines) and create reference documents in `Docs/Reference/` that Claude can pull when needed.

[![Claude Opus refactor analysis showing before/after comparison](/images/2025/12/claude-md-refactor-opus-dec3.png)](/images/2025/12/claude-md-refactor-opus-dec3.png)

This is how I learn best. Read something interesting, apply it immediately while it's fresh. No analysis paralysis, just iteration. By the end of the session, I had a new doc structure that made every future Claude conversation more effective.

Meta-learning: using AI to improve how you work with AI. That's leverage.
