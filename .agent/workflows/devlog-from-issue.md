---
description: How to publish a devlog post using GitHub Issues
---

# Publishing a Devlog via GitHub Issues

## Quick Steps

1. **Go to:** [New Devlog Issue](https://github.com/CloudStrife7/basement-os-web/issues/new?template=devlog-post.yml)

2. **Fill out the form:**
   - **Title:** `[Devlog] Your Post Title Here`
   - **Post Type:** Select `update`, `milestone`, or `meta`
   - **Short Description:** *(optional)* One-liner for previews
   - **Post Content:** Write your devlog in Markdown

3. **Submit the issue**

4. **Wait ~30 seconds** — The GitHub Action will:
   - Create the markdown file in `src/content/devlog/`
   - Commit it to `main`
   - Close the issue with a confirmation comment
   - Trigger a site rebuild

5. **Your post is live!** 🎉

---

## Example Post Content

```markdown
### What I Did
Implemented the new weather system integration for the terminal.

### Why It Matters
This connects the VRChat world to real-time weather data, enabling
weather-based achievements and dynamic ambiance.
```

---

## Post Types

| Type | Use When |
|------|----------|
| `update` | Regular development progress |
| `milestone` | Major feature completion |
| `meta` | Posts about the project/process itself |

---

## Troubleshooting

- **Issue not closing?** Check the [Actions tab](https://github.com/CloudStrife7/basement-os-web/actions) for errors
- **Wrong formatting?** Edit the created file directly and push
- **Need to delete?** Just delete the `.md` file from `src/content/devlog/`
