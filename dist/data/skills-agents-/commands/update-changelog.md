# Update Changelog Agent

> Reference: Issue #34

This command generates and updates the in-world changelog from recent Git commits for the DOS terminal display.

## Instructions

When this command is run, perform the following steps:

### 1. Get Recent Commits

```bash
git log --oneline --no-merges -30
```

### 2. Parse and Categorize Commits

Categorize each commit by its conventional commit prefix:

- **feat**: New features
- **fix**: Bug fixes
- **refactor**: Code refactoring
- **docs**: Documentation changes
- **style**: Formatting changes
- **test**: Test additions/changes
- **chore**: Maintenance tasks
- **perf**: Performance improvements

### 3. Format for Terminal Display

Format the changelog for 45-character terminal width (current) with proper text wrapping:

```
[DATE] - VERSION
────────────────────────────────────
Category:
 - Short description that wraps
   at 43 characters for proper
   display on the terminal
```

### 4. Generate Output Files

Create two files in `docs/changelog/`:

**changelog.json**
```json
{
  "version": "v28",
  "generated": "2025-11-18T10:30:00Z",
  "entries": [
    {
      "date": "2025-11-18",
      "category": "feat",
      "message": "Add 80-character terminal support",
      "hash": "abc1234"
    }
  ]
}
```

**changelog.txt**
```
BASEMENT OS CHANGELOG
═════════════════════════════════════════════

[2025-11-18] v28
─────────────────────────────────────────────
Features:
 - Add 80-character terminal support
 - Implement Quest optimizations

Fixes:
 - Fix cursor blinking performance
 - Resolve weather API timeout
```

### 5. Deploy Instructions

After generating the files:

1. Commit the updated changelog files:
   ```bash
   git add docs/changelog/
   git commit -m "docs: update changelog for terminal"
   ```

2. Push to trigger GitHub Pages deployment:
   ```bash
   git push origin main
   ```

3. Verify deployment at:
   - https://cloudstrife7.github.io/DOS-Terminal/changelog.txt
   - https://cloudstrife7.github.io/DOS-Terminal/changelog.json

## Example Output

When run, this agent should output:

```
Changelog Update Summary
========================
Commits processed: 30
Categories found:
  - feat: 5
  - fix: 8
  - refactor: 3
  - docs: 2
  - other: 12

Files generated:
  - docs/changelog/changelog.json
  - docs/changelog/changelog.txt

Next steps:
  1. Review generated files
  2. Commit changes
  3. Push to deploy
```

## Troubleshooting

### No commits found
- Ensure you're in the correct repository
- Check if the branch has commit history

### Invalid commit format
- Commits without conventional prefixes will be categorized as "other"
- The agent will still process them with their full message

### Deployment issues
- Verify GitHub Pages is enabled for the repository
- Check that the docs folder is set as the source
- Wait 2-3 minutes for deployment to complete
