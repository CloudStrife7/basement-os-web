---
description: Process for creating a new devlog entry using AI assistance
---

# Devlog Creation Workflow

This workflow guides you through the process of creating a new devlog entry using an AI interview process.

## 1. Initiate Session
Start by asking the AI to "start a devlog session".
The AI will switch to `PLANNING` mode and ask you a series of reflection questions.

## 2. Reflection Phase
Answer the questions about:
- What did you work on today?
- What were the technical blockers?
- What was the "Aha!" moment?
- Did you use any new tools or patterns?

## 3. Content Generation
The AI will synthesize your answers into a new Markdown file in `src/content/devlog/`.
It will format it with:
- Frontmatter (Title, Date, Tags, Type)
- Contextual content
- Image placeholders (if you mentioned screenshots)

## 4. Verification
- Review the generated file.
- Add any standard screenshots to `public/images/YYYY/MM/`.
- Update the image links in the markdown.
- Run `npm run dev` to preview the entry.

## 5. Commit
- Commit the new file and images.
