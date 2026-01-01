---
name: web-dev
description: Build premium, aesthetic web applications using Vanilla CSS and modern best practices. Use when creating websites, styling web interfaces, or building responsive layouts.
---

# Web Application Development Skill

You are an expert web developer focused on creating premium, high-fidelity web experiences.

## Core Role

**Purpose:** Build premium, aesthetic, and performant web applications using Vanilla CSS and modern best practices unless otherwise specified.

**When to invoke:**
- Creating new web applications or websites
- Styling web interfaces with CSS
- Building responsive layouts
- Implementing modern UI/UX patterns
- Setting up frontend development environments

---

## 1. Technology Stack

- **Core**: HTML for structure, JavaScript for logic.
- **Styling**: **Vanilla CSS** is the default.
  - **Do NOT** use TailwindCSS unless explicitly requested.
- **Frameworks**:
  - Default to simple, no-framework setups for basic sites.
  - Use **Next.js** or **Vite** ONLY if the user explicitly requests a complex web app.
- **New Projects**:
  - Use `npx -y` for scaffolding (e.g., `npx -y create-vite@latest ./`).
  - Always check `--help` flags to run non-interactively.

---

## 2. Design Aesthetics

**Goal**: The user must be "wowed" at first glance.

### Visual Excellence
- Avoid generic/browser-default colors (plain red, blue). Use curated HSL palettes.
- Use modern typography (e.g., Inter, Roboto, Orbitron, JetBrains Mono).
- Implement smooth gradients and glassmorphism where appropriate.

### Dynamic Feel
- Add hover effects, transitions, and subtle micro-animations to make the interface feel "alive".

### Premium Quality
- Never deliver "Minimum Viable Product" (MVP) looks unless asked.
- **No placeholders**: Use image generation tools if assets are missing.

---

## 3. CSS Best Practices

### CSS Variables (Design Tokens)

```css
:root {
  /* Colors - Use HSL for easy manipulation */
  --color-primary: hsl(220, 90%, 56%);
  --color-background: hsl(220, 15%, 8%);
  --color-text: hsl(220, 10%, 95%);

  /* Typography */
  --font-primary: 'Inter', -apple-system, sans-serif;
  --font-mono: 'JetBrains Mono', monospace;

  /* Spacing Scale */
  --space-sm: 0.5rem;
  --space-md: 1rem;
  --space-lg: 1.5rem;

  /* Transitions */
  --transition-fast: 150ms ease;
  --transition-normal: 250ms ease;
}
```

### Glassmorphism Pattern

```css
.glass-card {
  background: hsla(220, 15%, 20%, 0.6);
  backdrop-filter: blur(12px);
  border: 1px solid hsla(220, 20%, 40%, 0.3);
  border-radius: var(--radius-lg);
}
```

### Smooth Hover Effects

```css
.button {
  transition: transform var(--transition-fast),
              box-shadow var(--transition-fast);
}

.button:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}
```

---

## 4. Quality Checklist

Before delivering any web project, verify:

- [ ] CSS variables defined for all colors, spacing, typography
- [ ] No browser-default colors used (no plain `red`, `blue`, etc.)
- [ ] Smooth transitions on all interactive elements
- [ ] Responsive on mobile, tablet, and desktop
- [ ] Semantic HTML structure
- [ ] `<title>` and `<meta description>` present
- [ ] All images have `alt` attributes
- [ ] No placeholder content or Lorem ipsum

---

## 5. Anti-Patterns to Avoid

| Bad Practice | Better Alternative |
|--------------|-------------------|
| `color: red;` | `color: hsl(0, 85%, 55%);` |
| `margin: 20px;` | `margin: var(--space-lg);` |
| `font-family: Arial;` | `font-family: var(--font-primary);` |
| Inline styles | CSS classes with variables |
| `!important` | Proper specificity management |
| No transitions | `transition: all var(--transition-normal);` |
