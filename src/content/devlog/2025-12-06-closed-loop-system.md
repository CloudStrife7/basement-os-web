---title: "Full Closed-Loop Automation"
date: 2025-12-06
type: milestone
tags: ["MILESTONE", "architecture", "automation", "ai", "Unity MCP", "Claude Code", "Agent Swarm"]
description: "The moment the loop closed. AI writing code, compiling it, and testing it without human hands."
---
This is a big one. I've been working with <span class="term" tabindex="0">Claude Code<span class="tooltip">An AI coding assistant by Anthropic that can read, write, and execute code autonomously</span></span> to build out the Basement OS kernel, and we finally cracked the automation problem.

**The Problem:** With <span class="term" tabindex="0">UdonSharp<span class="tooltip">A C# to Udon compiler that lets you write VRChat scripts in familiar C# syntax instead of visual programming</span></span>, AI can write perfect code that won't run. Unity needs to generate .asset files, attach them to GameObjects, and compile everything. My first two attempts failed because Claude would generate code with no way to test it. I was still the button-clicker.

**The Solution:** <span class="term" tabindex="0">Unity MCP<span class="tooltip">Model Context Protocol - a way for AI agents to communicate with Unity Editor directly</span></span> gave Claude hands. Now it does the full loop: write script → trigger compilation → check errors → attach to GameObjects → enter Play mode → verify. Zero human intervention.

**The Learning:** This taught me that real automation isn't about speed - it's about eliminating the feedback loop. I went from "human as button-clicker" to "human as architect." That's the leadership transfer I'm after. As [HumanLayer puts it](https://www.humanlayer.dev/blog/writing-a-good-claude-md), good AI tooling is about leveraging stateless functions correctly.

**Why It Matters:** This pattern applies beyond VRChat. Any runtime environment (web apps, mobile, game engines) needs autonomous test → fix → verify loops. Companies pay for people who build these internal tools.
