---
title: "Project BIFROST: Shipping Code While I Sleep"
date: 2026-01-21
type: milestone
tags: ["MILESTONE", "automation", "bifrost", "pipeline", "ai", "infrastructure"]
description: "The autonomous AI development pipeline that picks up GitHub issues, writes code, tests in Unity, and creates pull requests without human intervention."
---

<div style="text-align: center; margin: 20px 0 30px 0;">
<span style="font-size: 1.3em; color: var(--accent-primary); font-weight: bold;">From GitHub Issue to Pull Request. Autonomously.</span>
</div>

I built an overnight agent. Label a GitHub issue, approve it from my phone, go to sleep, and wake up to a pull request. This is how it works and what I learned building it.

---

### The Bottleneck

Even with AI writing my code, every feature still looked like this:

<div style="display: flex; gap: 20px; margin: 20px 0; flex-wrap: wrap;">
<div style="flex: 1; min-width: 250px; padding: 16px; background: rgba(255, 68, 68, 0.08); border: 1px solid #ff4444; border-radius: 0;">
<div style="font-size: 0.75em; font-weight: bold; letter-spacing: 1px; color: var(--text-secondary); margin-bottom: 8px;">BEFORE (Generative AI)</div>
<div style="font-weight: bold; color: var(--text-primary); margin-bottom: 6px;">Human Drives Every Step</div>
<div style="font-size: 0.85em; color: var(--text-secondary); line-height: 1.8;">
Idea<br>
&darr; Ask AI (Chat)<br>
&darr; Copy Code<br>
&darr; Paste into Editor<br>
&darr; Fix Errors Manually<br>
&darr; Re-ask AI<br>
<em>Repeat forever</em>
</div>
</div>
<div style="display: flex; align-items: center; font-size: 2em; color: var(--accent-primary);">&rarr;</div>
<div style="flex: 1; min-width: 250px; padding: 16px; background: rgba(16, 185, 129, 0.08); border: 1px solid var(--accent-primary); border-radius: 0;">
<div style="font-size: 0.75em; font-weight: bold; letter-spacing: 1px; color: var(--text-secondary); margin-bottom: 8px;">AFTER (Project BIFROST)</div>
<div style="font-weight: bold; color: var(--text-primary); margin-bottom: 6px;">AI Executes After Human Intent</div>
<div style="font-size: 0.85em; color: var(--text-secondary); line-height: 1.8;">
Idea<br>
&darr; GitHub Issue<br>
&darr; PRISM Spec (Intent)<br>
&darr; Ready-for-Dev (Human Approval)<br>
&darr; <strong style="color: var(--accent-primary);">BIFROST (Autonomous Run)</strong><br>
&darr; Pull Request<br>
<em>Done</em>
</div>
</div>
</div>

I was the middleware between the AI and my codebase. The backlog grew faster than I could work through it. I had the ideas. I had the AI. The glue between intent and execution was still me, sitting at the keyboard, for every single issue.

**What if I could go to sleep and wake up to a pull request?**

---

### The Pipeline

<span class="term" tabindex="0">BIFROST<span class="tooltip">Named after the Norse rainbow bridge connecting Midgard to Asgard. Here it connects GitHub issues to finished pull requests.</span></span> is an orchestration system that connects a GitHub issue to a finished pull request. The human stays in the loop for *intent* and *approval*. The AI handles *execution*.

<div style="display: flex; align-items: center; justify-content: center; gap: 6px; margin: 24px 0; flex-wrap: wrap;">
<div style="display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 10px 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<span style="font-size: 1.3em;">&#x1f4a1;</span>
<span style="font-size: 0.7em; font-family: var(--font-mono); color: var(--text-secondary); text-transform: uppercase;">Idea</span>
</div>
<span style="color: var(--accent-primary); font-family: var(--font-mono);">&rarr;</span>
<div style="display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 10px 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<span style="font-size: 1.3em;">&#x1f4cb;</span>
<span style="font-size: 0.7em; font-family: var(--font-mono); color: var(--text-secondary); text-transform: uppercase;">Issue</span>
</div>
<span style="color: var(--accent-primary); font-family: var(--font-mono);">&rarr;</span>
<div style="display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 10px 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<span style="font-size: 1.3em;">&#x1f4d0;</span>
<span style="font-size: 0.7em; font-family: var(--font-mono); color: var(--text-secondary); text-transform: uppercase;">PRISM Spec</span>
</div>
<span style="color: var(--accent-primary); font-family: var(--font-mono);">&rarr;</span>
<div style="display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 10px 14px; background: rgba(16, 185, 129, 0.1); border: 1px solid var(--accent-primary);">
<span style="font-size: 1.3em;">&#x2705;</span>
<span style="font-size: 0.7em; font-family: var(--font-mono); color: var(--accent-primary); text-transform: uppercase;">Approve</span>
</div>
<span style="color: var(--accent-primary); font-family: var(--font-mono);">&rarr;</span>
<div style="display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 10px 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<span style="font-size: 1.3em;">&#x26A1;</span>
<span style="font-size: 0.7em; font-family: var(--font-mono); color: var(--text-secondary); text-transform: uppercase;">BIFROST</span>
</div>
<span style="color: var(--accent-primary); font-family: var(--font-mono);">&rarr;</span>
<div style="display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 10px 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<span style="font-size: 1.3em;">&#x1F500;</span>
<span style="font-size: 0.7em; font-family: var(--font-mono); color: var(--text-secondary); text-transform: uppercase;">PR</span>
</div>
</div>

I write the *what* and *why*. BIFROST handles the *how*.

---

### How It Works

<div style="display: flex; flex-direction: column; gap: 20px; margin: 20px 0;">

<div style="display: flex; gap: 16px; align-items: flex-start;">
<div style="width: 44px; height: 44px; border-radius: 50%; background: #ffaa00; color: #000; display: flex; align-items: center; justify-content: center; font-weight: bold; font-family: var(--font-mono); flex-shrink: 0; font-size: 0.85em;">01</div>
<div>
<strong>Issue Creation & Triage</strong><br>
<span style="font-size: 0.8em; font-family: var(--font-mono); color: var(--accent-primary);">Automated</span><br>
<span style="color: var(--text-secondary);">I create a GitHub issue. <span class="term" tabindex="0">PRISM<span class="tooltip">AI triage system that auto-generates technical specs with skill routing, scope, and acceptance criteria</span></span> auto-generates a technical spec with skill routing, scope, and acceptance criteria. The issue gets a BBP priority score for queue ordering.</span>
</div>
</div>

<div style="display: flex; gap: 16px; align-items: flex-start;">
<div style="width: 44px; height: 44px; border-radius: 50%; background: var(--accent-primary); color: #000; display: flex; align-items: center; justify-content: center; font-weight: bold; font-family: var(--font-mono); flex-shrink: 0; font-size: 0.85em;">02</div>
<div>
<strong>Human Approval Gate</strong><br>
<span style="font-size: 0.8em; font-family: var(--font-mono); color: #ffaa00;">Human-in-the-Loop</span><br>
<span style="color: var(--text-secondary);">I review the spec and add the <code>ready-for-dev</code> label. BIFROST detects it, queues the issue, and sends a notification to my phone. I reply <code>APPROVE</code> from anywhere&mdash;phone, laptop, watch.</span><br>
<span style="font-size: 0.9em; color: var(--accent-primary); margin-top: 8px; display: inline-block;"><strong>Key design choice:</strong> AI executes <em>after</em> human intent, never before. The approval gate is non-negotiable.</span>
</div>
</div>

<div style="display: flex; gap: 16px; align-items: flex-start;">
<div style="width: 44px; height: 44px; border-radius: 50%; background: #00ccff; color: #000; display: flex; align-items: center; justify-content: center; font-weight: bold; font-family: var(--font-mono); flex-shrink: 0; font-size: 0.85em;">03</div>
<div>
<strong>Autonomous Execution</strong><br>
<span style="font-size: 0.8em; font-family: var(--font-mono); color: #00ccff;">Fully Autonomous</span><br>
<span style="color: var(--text-secondary);">BIFROST spins up a <span class="term" tabindex="0">Claude Code<span class="tooltip">Anthropic's AI coding agent that can read, write, and execute code autonomously via CLI</span></span> session with the PRISM spec. Routes to the correct skill agent (UdonSharp, Terminal UI, Game Dev, etc.). Claude writes code, compiles via <span class="term" tabindex="0">Unity MCP<span class="tooltip">Model Context Protocol&mdash;lets AI agents communicate directly with Unity Editor</span></span>, reads console errors, fixes them, tests in Play mode, and creates a PR.</span><br>
<span style="font-size: 0.9em; color: var(--accent-primary); margin-top: 8px; display: inline-block;"><strong>The loop:</strong> Write &rarr; Compile &rarr; Read Console &rarr; Fix &rarr; Repeat. No human needed.</span>
</div>
</div>

<div style="display: flex; gap: 16px; align-items: flex-start;">
<div style="width: 44px; height: 44px; border-radius: 50%; background: #cc66ff; color: #000; display: flex; align-items: center; justify-content: center; font-weight: bold; font-family: var(--font-mono); flex-shrink: 0; font-size: 0.85em;">04</div>
<div>
<strong>Delivery & Review</strong><br>
<span style="font-size: 0.8em; font-family: var(--font-mono); color: var(--accent-primary);">Back to Human</span><br>
<span style="color: var(--text-secondary);">A PR appears on GitHub with a full summary, self-review checklist, and test evidence. I review in the morning&mdash;merge, request changes, or close. A structured event log provides a full audit trail of everything the AI did.</span>
</div>
</div>

</div>

---

### The Infrastructure

BIFROST is five orchestration modules working together:

<div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin: 16px 0;">
<div style="padding: 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<strong style="color: var(--text-primary);">GitHub Bridge</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">Polls for <code>ready-for-dev</code> issues, orchestrates the full workflow</span>
</div>
<div style="padding: 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<strong style="color: var(--text-primary);">Queue Manager</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">FIFO with priority scoring, persistence across restarts</span>
</div>
<div style="padding: 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<strong style="color: var(--text-primary);">Session Manager</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">Launches Claude Code + Unity, manages lifecycle and timeouts</span>
</div>
<div style="padding: 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<strong style="color: var(--text-primary);">Approval Manager</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">Human gate via Home Assistant or email from phone</span>
</div>
<div style="padding: 14px; background: rgba(16, 185, 129, 0.08); border: 1px solid var(--accent-primary);">
<strong style="color: var(--text-primary);">Email System</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">Dedicated Bifrost mailbox for approving issues from anywhere</span>
</div>
</div>

---

### The First Test

**January 21, 2026.** Issue #296&mdash;"Add Boot Screen + Sounds to Terminal"&mdash;was labeled `ready-for-dev`. BIFROST picked it up, queued it, requested approval, spun up Claude Code with the `/terminal-ui` skill, and produced PR #297.

<div style="display: flex; flex-direction: column; gap: 6px; margin: 16px 0;">
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">GitHub polling detected the <code>ready-for-dev</code> label</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">Queue system with FIFO persistence</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">Dry-run approval via Home Assistant notification</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">Claude invoked with correct skill routing</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">Code written, compiled, PR created with self-review</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">10+ notification events fired to phone</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid var(--accent-primary);">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: var(--accent-primary); min-width: 36px;">PASS</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">Full structured audit trail in JSONL</span>
</div>
<div style="display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--bg-secondary); border-left: 3px solid #ff4444;">
<span style="font-family: var(--font-mono); font-weight: bold; font-size: 0.8em; color: #ff4444; min-width: 36px;">FAIL</span>
<span style="color: var(--text-secondary); font-size: 0.9em;">Unity MCP scene navigation&mdash;couldn't wire GameObject references</span>
</div>
</div>

The PR was closed because Unity wiring wasn't completed. But the pipeline&mdash;from issue detection to PR creation&mdash;worked flawlessly.

> "The infrastructure works. The failure was at the execution layer, not the orchestration layer."

The gap was solvable. And it's since been fixed.

---

### Remote Approval

The first test used Home Assistant notifications&mdash;functional, but limited to push notifications. I wanted to approve from any device, any email client. So I built a dedicated email system.

<div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin: 16px 0;">
<div style="padding: 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<strong style="color: var(--text-primary);">Security</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">ProtonMail with sender whitelist, DKIM/SPF/DMARC validation, Message-ID replay prevention</span>
</div>
<div style="padding: 14px; background: var(--bg-secondary); border: 1px solid var(--border-color);">
<strong style="color: var(--text-primary);">Architecture</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">ProtonMail Split Mode &rarr; Proton Bridge (local IMAP/SMTP) &rarr; BIFROST polling loop</span>
</div>
</div>

BIFROST sends a request. I reply `APPROVE` from my phone. That's it.

**Problem solved along the way:** ProtonMail marks emails as "read" at the conversation level, not per-mailbox. Switched from read/unread tracking to Message-ID deduplication&mdash;more robust anyway.

---

### By The Numbers

<div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; text-align: center; margin: 20px 0;">
<div>
<div style="font-size: 2em; font-weight: bold; color: var(--accent-primary); font-family: var(--font-mono);">1,500+</div>
<div style="font-size: 0.8em; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 1px;">Lines of Python</div>
</div>
<div>
<div style="font-size: 2em; font-weight: bold; color: var(--accent-primary); font-family: var(--font-mono);">6</div>
<div style="font-size: 0.8em; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 1px;">Modules</div>
</div>
<div>
<div style="font-size: 2em; font-weight: bold; color: var(--accent-primary); font-family: var(--font-mono);">10+</div>
<div style="font-size: 0.8em; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 1px;">Notification Events</div>
</div>
<div>
<div style="font-size: 2em; font-weight: bold; color: var(--accent-primary); font-family: var(--font-mono);">2</div>
<div style="font-size: 0.8em; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 1px;">Weeks to Build</div>
</div>
</div>

---

### What I Learned

<div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin: 16px 0;">
<div style="padding: 18px; background: var(--bg-secondary); border: 1px solid var(--border-color); text-align: center;">
<div style="font-size: 2em; margin-bottom: 8px;">&#x1F512;</div>
<strong style="color: var(--text-primary);">Human-in-the-Loop</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">AI should execute after human intent, never autonomously decide <em>what</em> to build. The approval gate is the feature.</span>
</div>
<div style="padding: 18px; background: var(--bg-secondary); border: 1px solid var(--border-color); text-align: center;">
<div style="font-size: 2em; margin-bottom: 8px;">&#x1F3D7;</div>
<strong style="color: var(--text-primary);">Orchestration > Execution</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">The hard part isn't getting AI to write code. It's building the pipeline&mdash;queue, approval, session, cleanup.</span>
</div>
<div style="padding: 18px; background: var(--bg-secondary); border: 1px solid var(--border-color); text-align: center;">
<div style="font-size: 2em; margin-bottom: 8px;">&#x1F504;</div>
<strong style="color: var(--text-primary);">Fail at the Right Layer</strong><br>
<span style="font-size: 0.85em; color: var(--text-secondary);">The first test "failed" but proved the architecture. Infrastructure worked; only the last-mile execution needed fixing.</span>
</div>
</div>

---

<div style="text-align: center; margin: 24px 0;">
<span style="font-size: 1.2em; color: var(--accent-primary); font-weight: bold;">The bridge is built. Now I'm scaling what crosses it.</span>
<br><br>
<span style="color: var(--text-secondary);">BIFROST is running as a proof-of-concept. Issues go in, pull requests come out—when it works. The overnight agent is real, but still learning.</span>
</div>
