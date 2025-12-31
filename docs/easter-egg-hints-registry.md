# Easter Egg Hints Registry

This document tracks all hidden hints throughout the basementos.com Easter egg hunt.
Update this as achievements are defined and hints are placed.

## Hint Locations

### 1. `/egg-hunt` (Fake 404 Page)

**Hidden in SVG Path Data:**
- The Astro rocket SVG has been modified with hint coordinates
- Original path data replaced with encoded hints

**Data Attributes:**
- `data-hint-1`: First achievement clue (placeholder)
- `data-hint-2`: Second achievement clue (placeholder)

**CSS Comments:**
- Hidden comments in the style block with additional hints

**Timing Easter Egg:**
- Page glitches after 39 seconds (Xbox 360 launch date: 11/22/05 → 11+22+5+1 = 39)
- Clicking the "0" in 404 leads to retro achievements page

---

### 2. `/gunter` (Main Easter Egg Landing)

**Console Easter Eggs:**
- `gunter()` function reveals project stats
- Achievement progress tracker in console
- Hints about GitHub commits at 3AM

**Code Comments:**
- Coordinates for Fond du Lac, WI (weather data source)
- References to "The Basement Codex"

---

### 3. `/v1-achievements-retro` (2000s Vintage Achievement List)

**Visible Hints (in l33tsp34k):**
| Achievement | Points | Hint Text | Actual Requirement (TBD) |
|------------|--------|-----------|--------------------------|
| S3CR3T_K33P3R | 50G | "thr33 c0mm4nds" | Find 3 terminal commands |
| 4RCH430L0G1ST | 100G | "365 d4ys" | Visit 365 days |
| C0MPL3T10N1ST | 50G | "4ll st4nd4rd" | All standard achievements |
| SP33DRUNN3R | 75G | "0n3 v1s1t" | All time achievements in 1 visit |
| S0C14L_BUTT3RFLY | 25G | "10 fr13nds" | 10 unique visitors in one day |
| CUR4T0R | 30G | "r4t3 g4m3s" | Rate all arcade games |
| DJ_M4ST3R | 50G | "100 s0ngs" | Play 100 songs on music.exe |
| N0ST4LG14_TR1P | 50G | "N0v 22" | Visit on November 22 |
| Y2K_SURV1V0R | 50G | "J4n 1" | Visit on January 1st |
| SOURC3_D1V3R | 0G | "c0mm3nts" | Read code comments |
| C0NS0L3_C0WB0Y | 0G | "typ3 1t" | Type gunter() in console |
| P4T13NC3 | 0G | "w41t" | Wait 39 seconds on egg-hunt |

**Console Easter Eggs:**
- Visitor counter starts at 1337

---

## Binary/Encoded Hints (Placeholders)

These can be inserted into SVG paths, data attributes, or comments:

```
HINT_PLACEHOLDER_01 = "48 69 6E 74 31" (Hex for "Hint1")
HINT_PLACEHOLDER_02 = "01001000 01101001 01101110 01110100 00110010" (Binary for "Hint2")
HINT_PLACEHOLDER_03 = "Uvag3" (ROT13 for "Hint3")
```

---

## SVG Path Hint Injection Points

The Astro logo SVG paths contain coordinates that look like data:
- Inject hints as fake coordinate values: `20.5253 67.6322` → `ACHIEVEMENT.CODE`
- Use decimal ASCII: `72.101.108.112` = "Help"
- Binary segments: `0.1.0.0.1` patterns

---

## Future Hint Locations (Reserved)

- [ ] `/devlog` - Hidden comments in articles
- [ ] `/terminal` - Secret commands
- [ ] `/achievements` - Real achievement page (public)
- [ ] GitHub commit messages with "3AM" keyword

---

## The Prize

**GOLDEN TURTLE** - First to 1000G
- Contact: cloudstrife7 on Discord
- Proof required
- One winner only
- Prize TBD

---

## Gamerscore Math

| Category | Points |
|----------|--------|
| Standard Achievements | 420G |
| Hidden Achievements | 580G |
| **TOTAL** | **1000G** |

---

*Last Updated: December 31, 2024*
