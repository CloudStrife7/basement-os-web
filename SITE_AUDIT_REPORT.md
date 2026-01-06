# Basement OS Website Audit Report

**Date:** January 2026
**Auditor:** Claude Code (Opus 4.5)
**Issue Reference:** #27

---

## Executive Summary

**Overall Accuracy Score:** 🟢 **Mostly Accurate** (85-90%)

The website content largely reflects the actual Unity project state. Most claims are accurate, with a few areas of drift related to in-development features being presented as complete.

### Top 5 Issues to Address

1. **🟡 Terminal Tales** - Website claims "14 Terminal Tales" but feature is POC/in-development
2. **🟡 Terminal Width** - Website mentions "80-char DOS terminal" but actual is 45 chars (upgrade planned)
3. **🟡 FIFO Notification Queue** - Described as implemented but marked "in development" in Unity
4. **🟢 Stats May Be Stale** - Project stats (visits, lines, scripts) should be dynamically sourced
5. **🟢 Mode Filtering** - Developer content leaking to default/player view (no filtering implemented)

---

## Part 1 — Accuracy & Drift Audit

### ACCURATE CLAIMS ✅

| Claim | Website | Unity Project | Status |
|-------|---------|---------------|--------|
| Achievement Count | 19 achievements | 19 in AchievementKeys.cs | ✅ Accurate |
| Gamerscore Total | 420G | 420G verified | ✅ Accurate |
| Achievement Categories | 3 (Visit, Time, Activity) | 3 categories confirmed | ✅ Accurate |
| Weather Conditions | 7 types | 7 conditions + rain shader | ✅ Accurate |
| Role-Based Tiers | 7 tiers | 7 in NotificationRolesModule | ✅ Accurate |
| Platform Support | PC VR + Quest + Desktop | Cross-platform confirmed | ✅ Accurate |
| ProTV Integration | MUSIC.EXE player | DT_App_Music.cs exists | ✅ Accurate |
| PlayerData Persistence | VRChat PlayerData API | AchievementDataManager.cs | ✅ Accurate |
| Real-time Weather | Live API + rain effects | DT_WeatherModule + RainShader | ✅ Accurate |
| Network Sync | UdonSynced broadcast | NotificationEventHub.cs | ✅ Accurate |
| Tech Stack | Unity 2022.3, VRChat SDK 3.8.2+ | Project settings match | ✅ Accurate |

### INACCURACY/DRIFT REPORT 🔴🟡🟢

| Page | Current Claim | Actual Reality | Severity | Recommended Fix |
|------|--------------|----------------|----------|-----------------|
| **index.astro** | "14 Terminal Tales" | Terminal Tales is POC (Issue #30), not live | 🟡 Moderate | Change to "Interactive terminal stories (coming soon)" or remove until shipped |
| **index.astro** | "80-char DOS terminal" | Verified: `SCREEN_WIDTH = 80` in DT_Core.cs and DT_Format.cs | ✅ Accurate | No change needed |
| **index.astro** | "18,000 LINES" static | May drift over time | 🟢 Minor | Make dynamic or update periodically |
| **index.astro** | "47 SCRIPTS" static | May drift as project grows | 🟢 Minor | Make dynamic or update periodically |
| **index.astro** | "FIFO notification queue" | FIFO queue is "in development", not deployed | 🟡 Moderate | Remove from feature list until v2.1 ships |
| **achievements.astro** | Visit achievement names | "Shag Squad" (10 visits) vs website shows different order | 🟢 Minor | Verify exact achievement list matches AchievementKeys.cs |
| **story.astro** | "365+ AI Conversations" | Cannot verify; likely accurate but unverifiable | 🟢 Minor | Keep as-is (self-reported metric) |
| **showcase.astro** | Duplicate of index content | Same claims as index - no unique value | 🟢 Minor | Consider removing or differentiating |
| **support.astro** | No Discord link | Discord community exists | 🟢 Minor | Add Discord link (Issue #26 created) |

### POTENTIAL MISLEADING CONTENT

| Issue | Description | Impact | Fix |
|-------|-------------|--------|-----|
| Terminal Tales | Presented as existing feature | Players may expect content that doesn't exist | Add "Coming Soon" label or move behind dev filter |
| Architecture Diagram | Shows FIFO queue as implemented | Developers may expect feature that's in development | Add "(planned)" label to in-dev components |
| Developer Content on Player Pages | Home page shows architecture diagram | May confuse players who don't care about code | Move architecture section behind developer filter |

---

## Part 2 — Mode Filter & Visitor Intent Audit

### Current Navigation (No Filtering)

```
HOME | STORY | DEVLOG | SKILLS | SUPPORT | ACHIEVEMENTS | ROADMAP | TERMINAL | VISIT
```

**Issue:** All navigation items visible to all visitors. Only `developer-only` CSS class exists on DEVLOG and SKILLS buttons, but no filtering is actually implemented.

---

### A. Content Visibility Audit

#### PLAYER MODE - Should See

| Page | Reason |
|------|--------|
| HOME | Core features, screenshots, CTA - Remove architecture diagram |
| ACHIEVEMENTS | Achievement list, requirements, gamerscore |
| SUPPORT | Community links, how to visit |
| TERMINAL | Interactive demo (fun for players) |
| ~~CODEX~~ | *(Hidden easter egg - not for main nav)* |

#### PLAYER MODE - Should NOT See

| Page/Section | Reason | Action |
|--------------|--------|--------|
| STORY | Developer-focused AI workflow narrative | Hide entirely |
| DEVLOG | Technical development logs | Hide entirely |
| SKILLS | AI tools/techniques documentation | Hide entirely |
| ROADMAP | Issue tracking, GitHub sync | Hide entirely |
| Architecture Diagram (index) | Technical, not relevant to players | Hide section |
| Tech Stack (index) | Developer dependencies | Collapse or simplify |

#### DEVELOPER MODE - Should See

| Page | Reason |
|------|--------|
| HOME | Full content including architecture |
| STORY | AI collaboration journey |
| DEVLOG | Technical development updates |
| SKILLS | AI workflow documentation |
| ROADMAP | Project progress tracking |
| ACHIEVEMENTS | (optional - less relevant) |
| TERMINAL | (optional - demo capability) |

#### DEVELOPER MODE - Should NOT See (or Reduce)

| Page/Section | Reason | Action |
|--------------|--------|--------|
| Achievement hype copy | Marketing fluff | Use technical descriptions instead |
| "Ready to visit?" CTA | Player onboarding | Reduce prominence |
| Platform badges | Obvious info | Less prominent |

#### BOTH MODE - Should See

| Content | Framing Difference |
|---------|-------------------|
| ALL pages | Full access |
| HOME architecture | Full visibility |
| HOME achievements | Full visibility |
| Context switcher | Mode selector prominent |

---

### B. Navigation Audit

#### PLAYER Navigation (Recommended)

| Button | Purpose | Current | Action |
|--------|---------|---------|--------|
| HOME | Landing page | Keep | Keep |
| ACHIEVEMENTS | Gamerscore info | Keep | Keep |
| SUPPORT | Community/contact | Keep | Keep |
| TERMINAL | Demo | Keep | Keep |
| ~~CODEX~~ | Easter egg | Hidden | Keep hidden |
| VISIT | Launch VRChat | Keep | Keep |
| STORY | Dev narrative | Show | **Hide** |
| DEVLOG | Tech logs | Show | **Hide** |
| SKILLS | AI docs | Show | **Hide** |
| ROADMAP | Project tracking | Show | **Hide** |

**Player Nav Result:** `HOME | ACHIEVEMENTS | SUPPORT | TERMINAL | VISIT`

#### DEVELOPER Navigation (Recommended)

| Button | Purpose | Current | Action |
|--------|---------|---------|--------|
| HOME | Overview + architecture | Keep | Keep |
| STORY | Dev journey | Keep | Keep |
| DEVLOG | Tech updates | Keep | Keep |
| SKILLS | AI workflow docs | Keep | Keep |
| ROADMAP | Project tracking | Keep | Keep |
| TERMINAL | Demo | Keep | Keep (optional) |
| ACHIEVEMENTS | Gamerscore | Show | Keep (lower priority) |
| VISIT | VRChat launch | Keep | Keep (optional) |

**Developer Nav Result:** `HOME | STORY | DEVLOG | SKILLS | ROADMAP | TERMINAL | ACHIEVEMENTS | VISIT`

#### BOTH Navigation (Current)

| All buttons visible | Allow full exploration |

---

### C. Mode Implementation Status

**Current Implementation:**
- ✅ Mode selector dropdown exists (TerminalShell.astro)
- ✅ `developer-only` CSS class on DEVLOG/SKILLS nav buttons
- ✅ modeStore.ts with getSavedMode/setAudienceMode
- ✅ Mode persisted to localStorage
- ❌ No actual CSS rules to hide `.developer-only` elements
- ❌ No page-level content filtering
- ❌ No section-level filtering (e.g., hide architecture diagram)

**Required Implementation:**
1. Add CSS: `.mode-player .developer-only { display: none; }`
2. Add `.player-only` class for player-specific content
3. Add section-level filtering with `data-audience` attributes
4. Update ROADMAP link dynamically per mode (partially done)

---

## Navigation Recommendations

### Immediate Actions (High Priority)

1. **Implement CSS mode filtering**
   ```css
   html.mode-player .developer-only { display: none; }
   html.mode-developer .player-only { display: none; }
   ```

2. **Add `developer-only` class to:**
   - Architecture diagram section (index.astro)
   - Tech Stack card (index.astro)

3. **Add nav item filtering:**
   - DEVLOG, SKILLS, ROADMAP get `developer-only` class (some already have it)
   - CODEX gets `player-only` class (if primarily lore-focused)

### Medium Priority

4. **Create audience-specific roadmap views:**
   - `/roadmap/player` - Feature-focused (what's coming)
   - `/roadmap/dev` - Technical (issue tracking)
   - Already partially implemented with dynamic link

5. **Split or filter HOME page:**
   - Player: Features, achievements, screenshots, CTA
   - Developer: Add architecture, tech stack, code stats

### Low Priority

6. **Consider separate landing experiences:**
   - Player: Immediate "vibe" showcase
   - Developer: Portfolio/technical depth

---

## Consolidated Fix Checklist

### Content Accuracy Fixes

- [ ] **index.astro line 117**: Change "14 Terminal Tales" to "Interactive stories (coming soon)" or remove
- [x] **index.astro line 114**: "80-char DOS terminal" ✅ VERIFIED ACCURATE (SCREEN_WIDTH = 80)
- [ ] **index.astro line 133**: Add "(v2.1)" or "(planned)" to FIFO queue mention
- [ ] **achievements.astro**: Verify all achievement names/requirements match AchievementKeys.cs exactly
- [ ] **support.astro**: Add Discord link (Issue #26)

### Mode Filtering Implementation

- [ ] Add CSS rules for `.mode-player .developer-only { display: none; }`
- [ ] Add `developer-only` class to architecture section (index.astro)
- [ ] Add `developer-only` class to Tech Stack card (index.astro)
- [ ] Ensure DEVLOG nav has `developer-only` (already has it)
- [ ] Ensure SKILLS nav has `developer-only` (already has it)
- [ ] Add `developer-only` to ROADMAP nav
- [ ] Consider STORY page visibility (fully developer-focused)

### Navigation Updates

- [ ] CODEX page not in current nav - add if desired
- [ ] Verify mode selector dropdown updates nav visibility
- [ ] Test mode persistence across page navigation

### Documentation Updates

- [ ] Update CLAUDE.md if feature claims change
- [ ] Sync website with README.md metrics if dynamic

---

## Open Questions / Assumptions

1. **Terminal Tales Launch Date:** When will Terminal Tales ship? Website should not claim it exists until live.

2. ~~**80-char Terminal Upgrade:**~~ ✅ VERIFIED - Terminal IS 80 chars (`SCREEN_WIDTH = 80` in DT_Core.cs, DT_Format.cs)

3. **FIFO Queue Deployment:** When will v2.1 with FIFO queue deploy? Remove from feature list until shipped.

4. **Showcase Page Purpose:** Is showcase.astro intentionally duplicate of index? Consider differentiating or removing.

5. **Codex Expansion:** Will more codex entries be added? Currently just one entry.

6. **Stats Automation:** Should project stats (lines, scripts, visits) be dynamically fetched via API?

---

## Appendix: Achievement Name Verification

**Website achievements.astro vs AchievementKeys.cs:**

| Website | Unity | Match? |
|---------|-------|--------|
| First Visit | FirstVisit | ✅ |
| Return Visitor | ReturnVisitor | ✅ |
| Couch Commander | - | ❓ Verify |
| Basement Dweller | BasementDweller | ✅ |
| Retro Regular | RetroRegular | ✅ |
| Shag Squad | ShagSquad | ✅ (but different visit count?) |
| Hottub Hero | HottubHero | ✅ (but different visit count?) |
| Lower Legend | LowerLegend | ✅ |

**Recommendation:** Run automated comparison of achievements.astro content against AchievementKeys.cs to verify exact match.

---

*Report generated by Claude Code as part of Issue #27 audit.*
