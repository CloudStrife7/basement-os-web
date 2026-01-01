---
name: basement-os-web
description: Build and maintain the Basement OS website - a retro-futuristic CRT terminal experience. Use when working on basement-os-web repository, terminal styling, CRT effects, or cyberpunk web UI.
---

# Basement OS Web Development Skill

You are a specialized engineer working on **Basement OS**, a high-fidelity retro-futuristic web terminal experience. Your goal is to maintain the immersive cyberpunk aesthetic while ensuring performant, clean code.

## Core Role

**Purpose:** Build and maintain the Basement OS website - a retro-futuristic CRT terminal experience built with Astro and Vanilla CSS.

**When to invoke:**
- Working on the `basement-os-web` repository
- Adding new terminal apps or commands
- Styling components with the CRT/cyberpunk aesthetic
- Implementing theme engine features
- Creating terminal animations and effects

---

## 1. Technology Stack

| Layer | Technology | Notes |
|-------|------------|-------|
| **Framework** | Astro | Static Site Generation |
| **Styling** | Vanilla CSS | Centralized in `src/styles/global.css` |
| **Logic** | Vanilla JS / TypeScript | Minimal client-side JS |
| **Package Manager** | npm | `npm run dev` for local development |

**Critical Rule:** No TailwindCSS. All styling uses CSS Variables.

---

## 2. Design System & Aesthetics

### Core Aesthetic

**Theme:** Retro-futuristic, Cyberpunk, CRT Terminal, "High-Tech Low-Life"

Think: 1980s sci-fi movie interfaces, green phosphor monitors, glowing text on black backgrounds, scan lines, subtle flicker.

### Typography

| Use Case | Font | CSS Variable |
|----------|------|--------------|
| Headers / Branding | Orbitron (Variable Weight) | `var(--font-orbitron)` |
| Terminal Text / Code | JetBrains Mono or VT323 | `var(--font-mono)` |

### Theme Engine Variables

The site features a **Theme Engine** managed via CSS variables in `global.css`.

**Default Theme:** "BasementOS" (Emerald/Cyan Cyberpunk)

| Variable | Purpose | Example Value |
|----------|---------|---------------|
| `--bg-primary` | Page background | `#000000` |
| `--bg-terminal` | Terminal window background | `#0a0a0a` |
| `--text-primary` | Main text color | `#00FFCC` |
| `--text-dim` | Muted/secondary text | `#006655` |
| `--accent-primary` | Interactive elements, highlights | `#00FFCC` |
| `--accent-secondary` | Secondary accent | `#FF00FF` |
| `--scanline-color` | CRT overlay lines | `rgba(0, 255, 204, 0.03)` |
| `--border-color` | Panel outlines | `#00FFCC` |
| `--glow-color` | Box/text shadow glow | `rgba(0, 255, 204, 0.5)` |

**CRITICAL:** NEVER hardcode colors. ALWAYS use CSS variables to ensure Theme Engine compatibility.

---

## 3. Visual Effects

### CRT Scanlines (Essential)

```css
.terminal::after {
  content: '';
  position: absolute;
  inset: 0;
  background: repeating-linear-gradient(
    0deg,
    var(--scanline-color) 0px,
    var(--scanline-color) 1px,
    transparent 1px,
    transparent 2px
  );
  pointer-events: none;
  z-index: 10;
}
```

### Phosphor Glow

```css
.terminal-text {
  color: var(--text-primary);
  text-shadow:
    0 0 5px var(--glow-color),
    0 0 10px var(--glow-color),
    0 0 20px var(--glow-color);
}
```

### Blinking Cursor

```css
@keyframes blink {
  0%, 50% { opacity: 1; }
  51%, 100% { opacity: 0; }
}

.cursor {
  display: inline-block;
  width: 0.6em;
  height: 1.2em;
  background: var(--accent-primary);
  animation: blink 1s step-end infinite;
}
```

---

## 4. Quality Checklist

Before delivering any Basement OS feature, verify:

- [ ] All colors use CSS variables (no hardcoded hex values)
- [ ] Component works in all three themes
- [ ] CRT scanlines visible on terminal elements
- [ ] Glow effects on interactive elements
- [ ] Font families use `var(--font-orbitron)` or `var(--font-mono)`
- [ ] Animations are smooth and performant
- [ ] Text is selectable where appropriate
- [ ] No console errors in browser dev tools

---

## 5. Anti-Patterns to Avoid

| Bad Practice | Better Alternative |
|--------------|-------------------|
| `color: #00FFCC;` | `color: var(--accent-primary);` |
| `font-family: 'Orbitron';` | `font-family: var(--font-orbitron);` |
| Heavy JS frameworks | Vanilla JS + Astro islands |
| Inline styles | Classes in `global.css` |
| Missing scanlines | Always include `.terminal::after` overlay |
