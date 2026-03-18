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

### Additional Issue Found: rags-stats-proxy Workflow

The GitHub Action `.github/workflows/rags-stats-proxy.yml` was overwriting good data with error responses:
- Runs every 5 minutes
- Fetches from Cloudflare API
- When API returns `{"error": "..."}`, it's valid JSON and passed validation
- This overwrote the good local JSON file

**Fix applied:** Updated workflow to:
1. Check for `.error` field before saving
2. Validate `session_count` exists (required field)
3. Only copy to public/data if response is valid stats

**Data restored:** Retrieved good data from commit `e5328f07d` (2026-02-26)

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

### Solution (APPLIED)
Added `dist/` to `.gitignore` and removed from git tracking:
```bash
echo "dist/" >> .gitignore
git rm -r --cached dist/
git commit -m "chore: stop tracking dist/ build artifacts"
```

Result:
- 224 files removed from tracking
- 53,921 lines deleted from repo
- Future pulls/rebases will no longer conflict on build output
- Deployment unaffected (GitHub Actions builds fresh)

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
d67f0fbf6 chore: stop tracking dist/ build artifacts (224 files, -53,921 lines)
f7ea6afe0 fix: Prevent rags-stats-proxy from committing error responses
```

---

## Action Items (TODO)

### 1. Fix Cloudflare Worker - Cat Stats API
**Priority:** High
**Status:** Broken since at least 2026-02-26

The `/rags/stats` endpoint on your Cloudflare Worker is returning an error.

**Steps:**
1. Go to Cloudflare Dashboard → Workers & Pages
2. Find `rags-analytics` worker (domain: `cloudflare-landscape202.workers.dev`)
3. Check the Logs/Analytics for errors
4. Debug why `/rags/stats` returns `{"error": "Failed to fetch stats"}`
5. The `/world/stats` endpoint on the same worker works fine, so compare them

**When fixed:**
- Uncomment the schedule in `.github/workflows/rags-stats-proxy.yml`
- Or manually trigger the workflow to test

---

### 2. Fix n8n Workflow - VRChat World Stats
**Priority:** Medium
**Status:** Stale since 2026-03-03 (15 days)

The `public/data/vrchat-world.json` file hasn't been updated.

**Steps:**
1. Open your n8n instance
2. Find the VRChat stats workflow
3. Check if it's active/scheduled
4. Check execution history for errors
5. Re-enable or fix as needed

---

### 3. Re-enable Rags Stats Proxy (After #1 is fixed)
**Priority:** Low (blocked by #1)

The GitHub Action that fetches cat stats is paused.

**When ready:**
1. Edit `.github/workflows/rags-stats-proxy.yml`
2. Uncomment the `schedule` and `workflow_run` triggers
3. Commit and push
4. Verify fresh data flows through

---

## Completed Items

- [x] Added local JSON fallback for cat stats
- [x] Disabled mode selection dialog (defaults to "both")
- [x] Removed dist/ from git tracking (fixes merge conflicts)
- [x] Fixed rags-stats-proxy to not commit error responses
- [x] Restored good cat stats data from 2026-02-26
- [x] Paused rags-stats-proxy workflow until API is fixed
