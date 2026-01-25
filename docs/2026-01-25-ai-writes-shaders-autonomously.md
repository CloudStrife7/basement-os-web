# AI Writes Shaders Autonomously: A VRChat CRT Terminal Case Study

**Date:** January 25, 2026
**Tags:** `[Milestone]` `[TIL]` `[AI Development]` `[Shaders]`

---

## The Challenge I Didn't Think Was Possible

When I created [Issue #295: Retro CRT Terminal Effects](https://github.com/CloudStrife7/LL2PCVR/issues/295), I genuinely didn't think an AI could handle it. Shader programming is notoriously difficult - it requires understanding GPU architecture, HLSL/GLSL syntax, platform-specific quirks, and the subtle art of making things look good while maintaining performance.

I was wrong.

---

## The Inspiration: Remo H Jansen's CRT Terminal

I have to give a huge shoutout to **Remo H Jansen** and his excellent article: [Building a Retro CRT Terminal Website with WebGL and GitHub Copilot/Claude Opus 3.5](https://dev.to/remojansen/building-a-retro-crt-terminal-website-with-webgl-and-github-copilot-claude-opus-35-3jfd).

Remo's work demonstrated that AI could assist with WebGL shader development for browser-based CRT effects. His Three.js implementation was the reference I gave Claude to study and adapt for Unity/VRChat.

Key resources from the issue:
- [cool-retro-term](https://github.com/Swordfish90/cool-retro-term) - QML terminal emulator
- [remojansen.github.io](https://github.com/CloudStrife7/remojansen.github.io) - Remo's implementation
- The challenge: Make it work in VRChat, on Quest standalone, with TextMeshPro

---

## The Vision

Lower Level 2.0 is a nostalgic 2000s basement VRChat world featuring a DOS-style terminal. The terminal worked great, but I wanted to take it further - bring it to life with that authentic CRT glow, the subtle flicker of phosphors warming up, scanlines rolling across the screen. The kind of details that make you feel like you're back in a dimly lit basement at 2 AM, the monitor humming quietly as you type.

**Requirements from Issue #295:**
- Scanline effects (those horizontal lines from CRT displays)
- Phosphor glow (that characteristic green bloom)
- Screen curvature (barrel distortion)
- Flicker/jitter (subtle, not seizure-inducing)
- Vignette (darkened edges)
- **Must be Quest-compatible** (Shader Model 3.0, <5ms GPU)
- **Easy to apply for Unity beginners** (step-by-step guide)
- **Bonus: Works on TV displays too**

---

## The Solution: Autonomous Shader Development

Claude researched the reference materials, studied HLSL shader patterns, and wrote three complete shader variants:

| Shader | Purpose | Performance |
|--------|---------|-------------|
| `TerminalCRT_Quest.shader` | Quest 2/3 optimized | <5ms GPU |
| `TerminalCRT_PC.shader` | PCVR enhanced | 5-10ms GPU |
| `TerminalCRT_Standard.shader` | Fallback/MeshRenderer | Variable |

### What Claude Actually Wrote

```hlsl
// Terminal CRT Shader - Quest Compatible
// Optimized for Meta Quest 2/3 with TextMeshPro support
// Inspired by cool-retro-term and remojansen.github.io
// Performance budget: <5ms GPU time

Shader "LL2/Terminal/CRT_Quest"
{
    Properties
    {
        // CRT Effect Parameters (exposed in Inspector)
        [Toggle(_CRT_ENABLED)] _CRTEnabled ("Enable CRT Effects", Float) = 1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.15
        _ScanlineCount ("Scanline Count", Range(100, 1000)) = 480
        _GlowStrength ("Phosphor Glow", Range(0, 1)) = 0.2
        _CurvatureAmount ("Screen Curvature", Range(0, 0.1)) = 0.02
        // ... full implementation
    }
}
```

The shader includes:
- Barrel distortion math for screen curvature
- Sin-wave based scanline patterns
- Time-driven flicker with multiple frequencies
- Radial vignette calculations
- Full TextMeshPro SDF compatibility (the tricky part)

### The Comprehensive Guide

Claude also produced a 368-line beginner-friendly setup guide:

**[CRT_Effect_Setup.md](https://github.com/CloudStrife7/basement-os-web/blob/main/docs/CRT_Effect_Setup.md)**

Contents:
- 5-step Quick Start for beginners
- Quest vs PC shader comparison table
- Complete parameter reference with ranges and defaults
- Troubleshooting section for common issues
- Bonus section for applying to TV displays
- Accessibility warnings (flicker intensity)
- Platform-specific material switching code

---

## How Autonomous Was It?

**~80% autonomous, ~20% Unity sync session**

### What Claude Did Autonomously:
1. Researched Remo's WebGL implementation
2. Translated Three.js shader concepts to Unity HLSL
3. Wrote Quest-compatible variant with SM 3.0 constraints
4. Wrote PC-enhanced variant with chromatic aberration, bloom
5. Created materials with tuned default values
6. Wrote the comprehensive 368-line setup guide
7. Added shader files to correct project locations
8. Created README in Assets/Shaders/ explaining the system

### What Required Unity Sync Session:
1. Applying materials to TextMeshPro components (MCP can't serialize font materials)
2. Visual tuning of parameters in Play Mode
3. Creating TV material variant
4. Final verification in VR

The blocking factor was **Unity MCP's limitation with TextMeshPro material assignment** - everything else was done without human intervention.

---

## Evidence: The Commits

```
a498e57d feat: Add two-layer CRT terminal system with full-screen scanlines (#295)
979df09c feat: Add retro CRT terminal effects for Quest and PC
```

Files created:
- `Assets/Shaders/TerminalCRT_Quest.shader`
- `Assets/Shaders/TerminalCRT_PC.shader`
- `Assets/Shaders/TerminalCRT_Standard.shader`
- `Assets/Materials/TerminalCRT_Quest.mat`
- `Assets/Materials/TerminalCRT_PC.mat`
- `Assets/Materials/TV_CRT.mat`
- `Assets/Shaders/README.md`
- `Docs/Modules/CRT_Effect_Setup.md`

---

## Why This Matters

I genuinely believed shader programming was beyond AI capabilities. It requires:
- Deep graphics programming knowledge
- Platform-specific optimization
- Artistic judgment for visual quality
- Integration with complex systems (TextMeshPro, VRChat, Quest)

But Claude handled it. Not perfectly on the first try - there was iteration. But the final result:

- **Looks authentic** - captures the CRT aesthetic I remember
- **Performs well** - stays under Quest performance budget
- **Is documented** - beginners can follow the guide
- **Is maintainable** - clean code with comments

This changes what I think is possible with agentic AI development.

---

## Try It Yourself

If you're working on a VRChat world and want that retro CRT look:

1. Read the [CRT Effect Setup Guide](https://github.com/CloudStrife7/basement-os-web/blob/main/docs/CRT_Effect_Setup.md) - beginner-friendly, step-by-step
2. Study [Remo's original implementation](https://dev.to/remojansen/building-a-retro-crt-terminal-website-with-webgl-and-github-copilot-claude-opus-35-3jfd) for the WebGL approach
3. The shaders and guide are MIT licensed - adapt freely for your own projects

---

## Thanks

- **Remo H Jansen** - For the inspiration and proving AI-assisted shader dev is viable
- **cool-retro-term** team - For the original CRT effect reference
- **Claude** - For proving me wrong about AI shader capabilities

---

*This devlog documents the first autonomous shader work in the Lower Level 2.0 project. It turned out well.*

---

**Links:**
- [CRT Effect Setup Guide](https://github.com/CloudStrife7/basement-os-web/blob/main/docs/CRT_Effect_Setup.md) - Full shader documentation
- [Remo's Article](https://dev.to/remojansen/building-a-retro-crt-terminal-website-with-webgl-and-github-copilot-claude-opus-35-3jfd) - The inspiration
