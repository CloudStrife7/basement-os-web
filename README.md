# Basement OS Web 2.0 (Astro)

This is the Astro-based implementation of the Basement OS Devlog.
It replicates the "Terminal" aesthetic of the original VRChat internal browser site but using modern Static Site Generation for better performance, SEO, and content management.

## Project Structure

- `src/pages`: Main entry points (`index.astro`, `devlog/index.astro`).
- `src/layouts`: `BaseLayout` containing global `<head>` and Theme logic.
- `src/components`:
  - `TerminalShell.astro`: The persistent UI wrapper (Header, Nav, Footer).
  - `DevlogEntry.astro`: Post rendering.
- `src/content/devlog`: Markdown content for the blog.
- `public/images`: Static assets.

## Commands

- `npm run dev`: Start development server.
- `npm run build`: Build for production.
- `npm run preview`: Preview the build.

## Search Indexing

This site uses **Pagefind** for static search.
The search index is generated **after build**.

To test search locally:
1. `npm run build`
2. `npx pagefind --site dist` (This patches the dist folder with the index)
3. `npm run preview`

## Devlog Workflow

To create a new devlog entry, you can use the AI Workflow:
1. Ask the AI agent: "Run the devlog workflow" or "Start a devlog session".
2. Follow the interview process.
3. A new file will be created in `src/content/devlog`.

Manual creation:
Create `src/content/devlog/YYYY-MM-DD-title.md` with:
```yaml
---
title: "Title"
date: YYYY-MM-DD
type: update # or milestone, meta
tags: ["tag1", "tag2"]
---
Content...
```
