# VRChat API → GitHub Stats Sync System

## Overview

This document explains how to fetch VRChat world statistics from the VRChat API 2.0, sync them to GitHub, and display them in your VRChat world using UdonSharp.

**Architecture:**
```
VRChat API → GitHub Actions (Scheduled) → GitHub Repository (JSON file) → UdonSharp (VRCStringDownloader) → In-World Display
```

---

## Part 1: VRChat API Endpoints

### World Statistics Endpoint

**Endpoint:** `https://api.vrchat.cloud/api/1/worlds/{worldId}`

**Parameters:**
- `worldId`: Your world ID (e.g., `wrld_7302897c-be0f-4037-ac67-76f0ea065c2b`)
- `apiKey`: Public VRChat API key: `JlE5Jldo5Jibnk5O5hTx6XVqsJyIkZVd`

**Example Request:**
```bash
curl "https://api.vrchat.cloud/api/1/worlds/wrld_7302897c-be0f-4037-ac67-76f0ea065c2b?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJyIkZVd"
```

**Response Fields:**
```json
{
  "id": "wrld_7302897c-be0f-4037-ac67-76f0ea065c2b",
  "name": "Lower Level 2.0",
  "visits": 875,
  "favorites": 68,
  "popularity": 4,
  "heat": 0,
  "publicOccupants": 2,
  "privateOccupants": 0,
  "occupants": 2,
  "updated_at": "2025-12-17T00:00:00.000Z",
  "created_at": "2025-07-22T00:00:00.000Z",
  "capacity": 16,
  "recommendedCapacity": 8
}
```

---

## Part 2: GitHub Actions Workflow

### Create Stats Sync Workflow

**File:** `.github/workflows/sync-vrchat-stats.yml`

```yaml
name: Sync VRChat World Stats

on:
  # Run every 30 minutes
  schedule:
    - cron: '*/30 * * * *'
  # Allow manual trigger
  workflow_dispatch:

jobs:
  fetch-stats:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}

      - name: Fetch VRChat World Stats
        run: |
          WORLD_ID="wrld_7302897c-be0f-4037-ac67-76f0ea065c2b"
          API_KEY="JlE5Jldo5Jibnk5O5hTx6XVqsJyIkZVd"

          # Fetch stats from VRChat API
          curl -s "https://api.vrchat.cloud/api/1/worlds/${WORLD_ID}?apiKey=${API_KEY}" \
            -H "User-Agent: LLPCVR-Stats-Sync/1.0" \
            -o raw_stats.json

          # Extract relevant fields and create simplified JSON
          jq '{
            worldId: .id,
            worldName: .name,
            visits: .visits,
            favorites: .favorites,
            popularity: .popularity,
            heat: .heat,
            occupants: .occupants,
            capacity: .capacity,
            recommendedCapacity: .recommendedCapacity,
            updatedAt: .updated_at,
            createdAt: .created_at,
            lastSync: (now | strftime("%Y-%m-%dT%H:%M:%SZ"))
          }' raw_stats.json > public/api/vrchat-stats.json

          # Create a backup with timestamp
          TIMESTAMP=$(date +%Y%m%d_%H%M%S)
          cp public/api/vrchat-stats.json "public/api/backups/stats_${TIMESTAMP}.json"

          # Clean up old backups (keep last 100)
          cd public/api/backups
          ls -t stats_*.json | tail -n +101 | xargs -r rm

      - name: Commit and push stats
        run: |
          git config user.name "VRChat Stats Bot"
          git config user.email "bot@llpcvr.github.io"

          git add public/api/vrchat-stats.json
          git add public/api/backups/

          # Only commit if there are changes
          if git diff --staged --quiet; then
            echo "No changes to commit"
          else
            git commit -m "chore: update VRChat world stats [$(date +'%Y-%m-%d %H:%M:%S UTC')]"
            git push
          fi
```

---

## Part 3: GitHub Repository Structure

### Directory Structure

```
LLPCVR/
├── .github/
│   └── workflows/
│       └── sync-vrchat-stats.yml
├── public/
│   └── api/
│       ├── vrchat-stats.json          # Latest stats (UdonSharp fetches this)
│       └── backups/
│           ├── stats_20251224_143000.json
│           ├── stats_20251224_140000.json
│           └── ... (last 100 backups)
```

### Stats JSON Format

**File:** `public/api/vrchat-stats.json`

```json
{
  "worldId": "wrld_7302897c-be0f-4037-ac67-76f0ea065c2b",
  "worldName": "Lower Level 2.0",
  "visits": 875,
  "favorites": 68,
  "popularity": 4,
  "heat": 0,
  "occupants": 2,
  "capacity": 16,
  "recommendedCapacity": 8,
  "updatedAt": "2025-12-17T00:00:00.000Z",
  "createdAt": "2025-07-22T00:00:00.000Z",
  "lastSync": "2025-12-24T14:30:00Z"
}
```

---

## Part 4: UdonSharp Implementation

### Stats Fetcher Script

**File:** `Scripts/VRChatStatsDisplay.cs`

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using TMPro;

namespace LowerLevel.Stats
{
    /// <summary>
    /// Fetches VRChat world statistics from GitHub and displays them in-world
    /// </summary>
    public class VRChatStatsDisplay : UdonSharpBehaviour
    {
        [Header("GitHub Stats URL")]
        [SerializeField]
        private VRCUrl statsUrl = new VRCUrl("https://cloudstrife7.github.io/LLPCVR/api/vrchat-stats.json");

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI visitsText;
        [SerializeField] private TextMeshProUGUI favoritesText;
        [SerializeField] private TextMeshProUGUI popularityText;
        [SerializeField] private TextMeshProUGUI occupantsText;
        [SerializeField] private TextMeshProUGUI lastUpdatedText;

        [Header("Settings")]
        [SerializeField] private float autoRefreshInterval = 300f; // 5 minutes
        [SerializeField] private bool autoRefreshEnabled = true;

        // Stats data
        private int currentVisits = 0;
        private int currentFavorites = 0;
        private int currentPopularity = 0;
        private int currentOccupants = 0;
        private string lastSyncTime = "";

        private bool isFetching = false;

        void Start()
        {
            // Fetch stats on world load
            FetchStats();

            // Start auto-refresh timer
            if (autoRefreshEnabled)
            {
                SendCustomEventDelayedSeconds(nameof(AutoRefresh), autoRefreshInterval);
            }
        }

        /// <summary>
        /// Fetch stats from GitHub (call this from buttons or events)
        /// </summary>
        public void FetchStats()
        {
            if (isFetching)
            {
                Debug.Log("[VRChatStats] Already fetching, skipping...");
                return;
            }

            isFetching = true;
            Debug.Log("[VRChatStats] Fetching stats from: " + statsUrl);
            VRCStringDownloader.LoadUrl(statsUrl, (IUdonEventReceiver)this);
        }

        /// <summary>
        /// Auto-refresh timer callback
        /// </summary>
        public void AutoRefresh()
        {
            if (autoRefreshEnabled)
            {
                FetchStats();
                SendCustomEventDelayedSeconds(nameof(AutoRefresh), autoRefreshInterval);
            }
        }

        /// <summary>
        /// VRCStringDownloader success callback
        /// </summary>
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            isFetching = false;
            string jsonData = result.Result;

            Debug.Log("[VRChatStats] Fetch successful! Parsing JSON...");

            // Parse JSON (UdonSharp doesn't support JSON libraries, so we parse manually)
            ParseStatsJSON(jsonData);

            // Update UI
            UpdateUI();
        }

        /// <summary>
        /// VRCStringDownloader error callback
        /// </summary>
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            isFetching = false;
            Debug.LogError("[VRChatStats] Failed to fetch stats: " + result.ErrorCode + " - " + result.Error);

            // Update UI with error message
            if (lastUpdatedText != null)
            {
                lastUpdatedText.text = "FETCH ERROR - RETRY IN " + autoRefreshInterval.ToString("F0") + "s";
                lastUpdatedText.color = new Color(1f, 0.3f, 0.3f); // Red error color
            }
        }

        /// <summary>
        /// Manual JSON parsing (UdonSharp-compatible)
        /// </summary>
        private void ParseStatsJSON(string json)
        {
            // Extract visits
            currentVisits = ExtractIntValue(json, "\"visits\":");

            // Extract favorites
            currentFavorites = ExtractIntValue(json, "\"favorites\":");

            // Extract popularity
            currentPopularity = ExtractIntValue(json, "\"popularity\":");

            // Extract occupants
            currentOccupants = ExtractIntValue(json, "\"occupants\":");

            // Extract lastSync timestamp
            lastSyncTime = ExtractStringValue(json, "\"lastSync\":\"", "\"");

            Debug.Log("[VRChatStats] Parsed - Visits: " + currentVisits + ", Favorites: " + currentFavorites + ", Popularity: " + currentPopularity);
        }

        /// <summary>
        /// Extract integer value from JSON string
        /// </summary>
        private int ExtractIntValue(string json, string key)
        {
            int keyIndex = json.IndexOf(key);
            if (keyIndex == -1) return 0;

            int valueStart = keyIndex + key.Length;
            int valueEnd = json.IndexOf(',', valueStart);
            if (valueEnd == -1) valueEnd = json.IndexOf('}', valueStart);

            string valueStr = json.Substring(valueStart, valueEnd - valueStart).Trim();

            int result;
            if (int.TryParse(valueStr, out result))
            {
                return result;
            }
            return 0;
        }

        /// <summary>
        /// Extract string value from JSON
        /// </summary>
        private string ExtractStringValue(string json, string startKey, string endKey)
        {
            int startIndex = json.IndexOf(startKey);
            if (startIndex == -1) return "";

            int valueStart = startIndex + startKey.Length;
            int valueEnd = json.IndexOf(endKey, valueStart);
            if (valueEnd == -1) return "";

            return json.Substring(valueStart, valueEnd - valueStart);
        }

        /// <summary>
        /// Update UI text elements
        /// </summary>
        private void UpdateUI()
        {
            if (visitsText != null)
            {
                visitsText.text = FormatNumber(currentVisits);
            }

            if (favoritesText != null)
            {
                favoritesText.text = FormatNumber(currentFavorites);
            }

            if (popularityText != null)
            {
                popularityText.text = FormatNumber(currentPopularity);
            }

            if (occupantsText != null)
            {
                occupantsText.text = currentOccupants.ToString();
            }

            if (lastUpdatedText != null)
            {
                lastUpdatedText.text = "LAST SYNC: " + FormatTimestamp(lastSyncTime);
                lastUpdatedText.color = new Color(0.2f, 1f, 0.2f); // Green success color
            }

            Debug.Log("[VRChatStats] UI updated successfully");
        }

        /// <summary>
        /// Format number with commas (875 → "875")
        /// </summary>
        private string FormatNumber(int number)
        {
            return number.ToString("N0");
        }

        /// <summary>
        /// Format ISO timestamp to readable format
        /// </summary>
        private string FormatTimestamp(string isoTimestamp)
        {
            if (string.IsNullOrEmpty(isoTimestamp)) return "UNKNOWN";

            // Extract date parts (simplified parsing)
            // Input: "2025-12-24T14:30:00Z"
            // Output: "DEC 24, 2025 14:30 UTC"

            if (isoTimestamp.Length < 19) return isoTimestamp;

            string year = isoTimestamp.Substring(0, 4);
            string month = isoTimestamp.Substring(5, 2);
            string day = isoTimestamp.Substring(8, 2);
            string time = isoTimestamp.Substring(11, 5); // HH:MM

            // Convert month number to name
            string[] monthNames = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            int monthNum;
            if (int.TryParse(month, out monthNum) && monthNum >= 1 && monthNum <= 12)
            {
                month = monthNames[monthNum - 1];
            }

            return month + " " + day + ", " + year + " " + time + " UTC";
        }

        /// <summary>
        /// Public method to get current visits count
        /// </summary>
        public int GetVisits()
        {
            return currentVisits;
        }

        /// <summary>
        /// Public method to get current favorites count
        /// </summary>
        public int GetFavorites()
        {
            return currentFavorites;
        }

        /// <summary>
        /// Public method to get current popularity score
        /// </summary>
        public int GetPopularity()
        {
            return currentPopularity;
        }
    }
}
```

---

## Part 5: Unity Setup

### Canvas UI Setup (Basement OS Style)

**Hierarchy:**
```
Canvas (World Space)
├── StatsPanel
│   ├── HeaderText ("LOWER LEVEL STATS")
│   ├── VisitsRow
│   │   ├── Label ("Total Visits")
│   │   └── ValueText (→ visitsText)
│   ├── FavoritesRow
│   │   ├── Label ("Total Favorites")
│   │   └── ValueText (→ favoritesText)
│   ├── PopularityRow
│   │   ├── Label ("Popularity")
│   │   └── ValueText (→ popularityText)
│   ├── OccupantsRow
│   │   ├── Label ("Current Occupants")
│   │   └── ValueText (→ occupantsText)
│   └── FooterText (→ lastUpdatedText)
└── RefreshButton
    └── VRChatStatsDisplay.cs (OnClick → FetchStats())
```

### TextMeshPro Settings (Matrix/Terminal Style)

**Font:** Monospace (e.g., Courier New, IBM Plex Mono)
**Color:** `#00FF41` (Matrix green)
**Outline:** Black, 0.1 width
**Size:** 24-36pt

---

## Part 6: Integration with Existing Achievement System

### Combine Player Stats + World Stats

**File:** `Scripts/BasementOS/BIN/DT_App_Stats.cs`

```csharp
[Header("World Stats Display")]
[SerializeField] private VRChatStatsDisplay worldStatsDisplay;

public void RefreshAllStats()
{
    // Refresh player-specific stats (from AchievementDataManager)
    RefreshPlayerStats();

    // Refresh world-level stats (from GitHub/VRChat API)
    if (worldStatsDisplay != null)
    {
        worldStatsDisplay.FetchStats();
    }
}
```

---

## Part 7: Deployment Checklist

### Step 1: Setup GitHub Repository

- [ ] Create `.github/workflows/sync-vrchat-stats.yml`
- [ ] Create `public/api/` directory
- [ ] Create `public/api/backups/` directory
- [ ] Enable GitHub Pages (Settings → Pages → Source: `main` branch, `/public` folder)
- [ ] Verify GitHub Pages URL: `https://cloudstrife7.github.io/LLPCVR/api/vrchat-stats.json`

### Step 2: Test Workflow

- [ ] Trigger workflow manually (Actions tab → Sync VRChat World Stats → Run workflow)
- [ ] Verify `vrchat-stats.json` is created
- [ ] Check JSON is valid (use https://jsonlint.com)
- [ ] Verify URL is publicly accessible

### Step 3: Unity/VRChat Setup

- [ ] Add `VRChatStatsDisplay.cs` to Unity project
- [ ] Create Canvas UI with TextMeshPro elements
- [ ] Assign UI references in Inspector
- [ ] Set `statsUrl` to GitHub Pages URL
- [ ] Test in Unity Editor (VRChat SDK → Build & Test)

### Step 4: Test In-World

- [ ] Upload world to VRChat
- [ ] Join world and check logs for "[VRChatStats] Fetch successful!"
- [ ] Verify stats display correctly
- [ ] Test manual refresh button
- [ ] Wait 5 minutes to test auto-refresh

---

## Part 8: Troubleshooting

### Common Issues

**Issue:** "Fetch ERROR - RETRY IN 300s"

**Solutions:**
- Check GitHub Pages is enabled and URL is correct
- Verify JSON file exists at URL (test in browser)
- Check VRChat world has internet permissions enabled
- Verify CORS is not blocking (GitHub Pages should work by default)

**Issue:** Stats show as "0"

**Solutions:**
- Check JSON parsing is working (add debug logs)
- Verify JSON format matches expected structure
- Check for typos in JSON field names (`"visits"` not `"Visits"`)

**Issue:** GitHub Actions workflow fails

**Solutions:**
- Check VRChat API key is valid
- Verify `jq` command syntax is correct
- Check repository has write permissions for bot
- Review workflow logs in Actions tab

---

## Part 9: Advanced Features

### Feature 1: Historical Stats Graphs

Fetch backup files to create time-series graphs:

```csharp
// Fetch last 24 hours of backups
string[] backupUrls = {
    "https://cloudstrife7.github.io/LLPCVR/api/backups/stats_20251224_140000.json",
    "https://cloudstrife7.github.io/LLPCVR/api/backups/stats_20251224_120000.json",
    // ... etc
};

// Plot visits over time on a LineRenderer
```

### Feature 2: Leaderboard Integration

Combine world stats with player stats:

```csharp
public string GetWorldRankMessage()
{
    int worldVisits = worldStatsDisplay.GetVisits();
    int playerVisits = achievementDataManager.GetPlayerVisits(Networking.LocalPlayer.displayName);

    float percentage = (float)playerVisits / worldVisits * 100f;

    return "You've contributed " + percentage.ToString("F1") + "% of total visits!";
}
```

### Feature 3: Discord Webhooks

Add Discord notifications when milestones are reached:

```yaml
# In .github/workflows/sync-vrchat-stats.yml
- name: Send Discord notification
  if: steps.check-milestone.outputs.milestone_reached == 'true'
  run: |
    curl -X POST "${{ secrets.DISCORD_WEBHOOK_URL }}" \
      -H "Content-Type: application/json" \
      -d '{
        "content": "🎉 Lower Level 2.0 just hit 1,000 visits!"
      }'
```

---

## Part 10: API Rate Limits & Best Practices

### VRChat API Rate Limits

- **Public API:** ~60 requests/hour per IP
- **Authenticated API:** Higher limits (requires login)

### Recommendations

1. **GitHub Actions Schedule:** 30 minutes (48 requests/day)
2. **UdonSharp Refresh:** 5 minutes (in-world fetches GitHub, not VRChat)
3. **Caching:** Use backups to reduce API calls
4. **Error Handling:** Always have fallback default values

---

## References

- **VRChat API Docs:** https://vrchatapi.github.io/
- **UdonSharp Docs:** https://udonsharp.docs.vrchat.com/
- **VRCStringDownloader:** https://docs.vrchat.com/docs/string-loading
- **GitHub Actions Docs:** https://docs.github.com/en/actions

---

## Support

For issues or questions:
- **GitHub Issues:** https://github.com/CloudStrife7/LLPCVR/issues
- **VRChat Discord:** https://discord.gg/vrchat
- **Basement OS Docs:** https://basementos.com

---

**Last Updated:** 2025-12-24
**Version:** 1.0.0
**Author:** CloudStrife7
