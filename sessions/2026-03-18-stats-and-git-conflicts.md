# Session: Stats Page Fixes and Git Conflict Investigation

**Date:** 2026-03-18
**Issues Addressed:**
1. Cat stats not showing on /stats/ page
2. Visit/level stats not updating
3. Massive git conflicts when updating the website

---

## Issue 1: Cat Stats Not Displaying

### Root Cause
The `fetchCatStats()` function in `src/pages/stats.astro` only fetched from the API endpoint with no fallback:
- API: `https://rags-analytics.cloudflare-landscape202.workers.dev/rags/stats`
- This API is currently returning `{"error": "Failed to fetch stats"}`

A local JSON file exists at `public/data/rags-stats.json` (updated by n8n) but was not being used.

### Fix Applied
Modified `src/pages/stats.astro` to:
1. Added constant: `LOCAL_CAT_STATS_URL = '/data/rags-stats.json'`
2. Created `fetchLocalCatStats()` function
3. Updated `fetchCatStats()` to try API first, fall back to local JSON

```javascript
async function fetchCatStats() {
  // Try API first (most up-to-date)
  try {
    const response = await fetch(CAT_STATS_API);
    if (!response.ok) throw new Error('Failed to fetch cat stats');
    return await response.json();
  } catch (e) {
    console.log('Cat stats API unavailable, trying local JSON');
  }
  // Fall back to local JSON
  try {
    return await fetchLocalCatStats();
  } catch (e) {
    console.error('Cat stats fetch failed:', e);
    return null;
  }
}
```

### Remaining Work
- The Cloudflare Worker at `rags-analytics.cloudflare-landscape202.workers.dev` needs debugging
- The `/rags/stats` endpoint is failing server-side
- Check Cloudflare dashboard logs for the worker

---

## Issue 2: VRChat World Stats Stale

### Finding
- Local file `public/data/vrchat-world.json` last updated: 2026-03-03 (15 days stale)
- This is updated by an n8n workflow that appears to have stopped

### Action Required
- Check n8n instance for the VRChat stats workflow
- Verify it's scheduled and running

---

## Issue 3: Git Conflicts (570+ commits divergence)

### Investigation Findings

#### What the automated commits actually do:
1. **Weather Bot** (GitHub Actions - `.github/workflows/weather-update.yml`)
   - Runs hourly on cron: `0 * * * *`
   - Updates `public/data/weather.json` only
   - Commits with message: `weather: {temp} {condition}`
   - Author: `Weather Bot <action@github.com>`

2. **World Stats** (n8n workflow)
   - Updates `public/data/world-stats.json`
   - Commits with message: `Update world stats: {timestamp}`
   - Author: `CloudStrife7`

3. **Roadmap Sync** (n8n workflow)
   - Updates `public/data/roadmap*.json`
   - Commits with message: `chore: sync roadmap data from GitHub Issues`

**These automated commits do NOT touch the dist/ folder.**

#### What the dist/ folder is:
- **Build output** from Astro (`npm run build`)
- Contains compiled HTML, CSS, JS with hashed filenames (e.g., `BLBAHujv.js`)
- **IS tracked in git** (not in .gitignore)

#### How deployment works:
From `.github/workflows/deploy.yml`:
- Triggered on push to main (except data file changes)
- Runs `npm ci` then `npm run build` fresh on GitHub Actions
- Uploads `./dist` artifact to GitHub Pages
- **Does NOT use the committed dist/ folder** - builds fresh every time

#### Why dist/ is being committed:
Looking at commit history:
```
0a32f9ab4 chore: update build artifacts and package-lock after Pork-OS addition
Author: Claude <noreply@anthropic.com>
```
Claude AI sessions are committing dist/ as part of feature development.

#### Why conflicts happen:
1. Remote has dist/ from the latest feature builds (committed by Claude)
2. Local machine has different dist/ from local `npm run build`
3. Astro generates new hashed filenames on every build
4. When pulling/rebasing: hundreds of HTML/CSS/JS files conflict

### Root Cause
**The dist/ folder should NOT be tracked in git.** It's build output that:
- Changes on every build
- Is rebuilt fresh by GitHub Actions for deployment
- Causes massive merge conflicts

### Solution
Add `dist/` to `.gitignore` and remove it from git tracking:
```bash
echo "dist/" >> .gitignore
git rm -r --cached dist/
git commit -m "chore: stop tracking dist/ build artifacts"
```

This will:
- Stop future dist conflicts
- Not affect deployment (GitHub Actions builds fresh)
- Reduce repo size

---

## Issue 4: Mode Selection Dialog Disabled

### Change Made
In `src/components/TerminalShell.astro`, commented out the mode selection dialog and defaulted to "both" view:

```javascript
// Boot sequence complete
// COMMENTED OUT: Mode selection dialog - now defaulting to "both" view
setAudienceMode('both');
runLoginSequence('both');
```

---

## Commits Made This Session

```
cccd903b0 fix: Add local JSON fallback for cat stats and disable mode selection dialog
```

---

## Outstanding Items

1. **Fix Cloudflare Worker** - `/rags/stats` endpoint returning error
2. **Check n8n workflows** - VRChat stats workflow may be stopped
3. **Consider removing dist/ from git** - Would eliminate future conflicts
