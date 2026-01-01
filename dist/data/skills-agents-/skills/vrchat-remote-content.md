# VRChat Remote Content Loading Skill

You are an expert in VRChat remote content loading using UdonSharp and VRCStringDownloader.

## Overview

This skill covers loading remote text/JSON content from web servers into VRChat worlds using the VRC SDK3 StringLoading API.

---

## Core API: VRCStringDownloader

### Basic Usage

```csharp
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

public class MyRemoteLoader : UdonSharpBehaviour
{
    public VRCUrl contentUrl;

    public void LoadContent()
    {
        IUdonEventReceiver callback = (IUdonEventReceiver)this;
        VRCStringDownloader.LoadUrl(contentUrl, callback);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string url = result.Url.Get();
        string content = result.Result;
        Debug.Log("Loaded: " + content.Length + " chars from " + url);
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        string url = result.Url.Get();
        string error = result.Error;
        Debug.LogError("Failed: " + url + " - " + error);
    }
}
```

### Key Points

- `VRCStringDownloader.LoadUrl()` is async - use callbacks
- Cast `this` to `IUdonEventReceiver` for the callback parameter
- Override `OnStringLoadSuccess` and `OnStringLoadError` to handle results
- Use `result.Url.Get()` to identify which URL completed (for multiple downloads)

---

## VRCUrl Type

### Characteristics

- **Special VRChat SDK type** - not a standard C# string
- Serializes differently than normal fields
- **Inspector-only editing preferred** - programmatic setting is unreliable
- Use `VRCUrl.Get()` to extract the string value at runtime

### Unity Editor Configuration

VRCUrl fields appear in Inspector with a text field. To configure via Editor scripts:

```csharp
// In Editor script
SerializedObject so = new SerializedObject(component);
SerializedProperty prop = so.FindProperty("myUrl");
SerializedProperty urlProp = prop.FindPropertyRelative("url");
urlProp.stringValue = "https://example.com/data.txt";
so.ApplyModifiedProperties();
```

### MCP/Automation Note

VRCUrl fields may appear as empty `{}` when queried via Unity MCP, but values ARE set correctly. Always verify by testing in Play Mode rather than relying on component inspection.

---

## Critical: GitHub Pages Redirect Issue

### Problem

GitHub Pages with custom domains cause 301 redirects:
```
cloudstrife7.github.io/repo-name/ → customdomain.com/ (301)
```

VRCStringDownloader has a **redirect limit** that gets exceeded, causing:
```
"Redirect limit exceeded"
```

### Solution

**Always use the direct custom domain URL**, not the github.io URL:

```csharp
// WRONG - causes redirect loop
public VRCUrl contentUrl = "https://cloudstrife7.github.io/basement-os-web/data/file.txt";

// CORRECT - direct URL, no redirects
public VRCUrl contentUrl = "https://basementos.com/data/file.txt";
```

### Debugging

If you see "Redirect limit exceeded":
1. Test the URL in a browser
2. Check Network tab for 301/302 redirects
3. Use the final destination URL directly

---

## JSON Parsing in UdonSharp

### Constraints

- **NO System.Text.Json** - not available
- **NO Newtonsoft.Json** - not available
- **NO LINQ** - forbidden in UdonSharp
- Must use **manual string parsing**

### Pattern: Simple JSON Value Extraction

```csharp
private string ExtractJsonString(string json, string key, int startPos)
{
    int colonPos = json.IndexOf(":", startPos);
    if (colonPos < 0) return "";

    int quoteStart = json.IndexOf("\"", colonPos);
    if (quoteStart < 0) return "";

    int quoteEnd = json.IndexOf("\"", quoteStart + 1);
    if (quoteEnd < 0) return "";

    return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
}
```

### Pattern: Iterate Through Array Items

```csharp
private void ParseIssuesArray(string json)
{
    int searchPos = json.IndexOf("\"issues\"");
    if (searchPos < 0) return;

    int sectionEnd = json.IndexOf("],", searchPos);
    if (sectionEnd < 0) sectionEnd = json.Length;

    while (searchPos < sectionEnd)
    {
        int titlePos = json.IndexOf("\"title\"", searchPos);
        if (titlePos < 0 || titlePos > sectionEnd) break;

        string title = ExtractJsonString(json, "title", titlePos);
        // Process title...

        searchPos = titlePos + 7; // Move past "title"
    }
}
```

### Pattern: Check for Nested Object Property

```csharp
// Check if issue has a specific label
int labelsPos = json.IndexOf("\"labels\"", titlePos);
int nextItemPos = json.IndexOf("\"title\"", titlePos + 7);

if (labelsPos > 0 && labelsPos < nextItemPos)
{
    int labelEnd = json.IndexOf("]", labelsPos);
    string labelsSection = json.Substring(labelsPos, labelEnd - labelsPos);
    bool hasLabel = labelsSection.ToLower().Contains("priority");
}
```

---

## Architecture: Hub-Spoke Remote Content

### Pattern

```
┌─────────────────────────────────────┐
│        DT_RemoteContent             │
│     (Central Content Module)        │
│                                     │
│  VRCUrl fields:                     │
│  - splashUrl, biosUrl, etc.         │
│                                     │
│  Content storage:                   │
│  - remoteSplashContent              │
│  - remoteBiosContent                │
│  - hasRemoteContent flag            │
│                                     │
│  Methods:                           │
│  - LoadAllContent()                 │
│  - OnStringLoadSuccess()            │
└─────────────────────────────────────┘
              │
    ┌─────────┼─────────┐
    ▼         ▼         ▼
┌───────┐ ┌───────┐ ┌───────┐
│App 1  │ │App 2  │ │App 3  │
│       │ │       │ │       │
│pulls→ │ │pulls→ │ │pulls→ │
│content│ │content│ │content│
└───────┘ └───────┘ └───────┘
```

### Benefits

- Single point of URL configuration
- Apps don't need individual download logic
- Centralized retry/error handling
- Easy to add new content types

### App Integration

```csharp
public class DT_App_MyApp : UdonSharpBehaviour
{
    [SerializeField] private DT_RemoteContent remoteContentModule;

    private void LoadContent()
    {
        if (Utilities.IsValid(remoteContentModule) && remoteContentModule.hasRemoteContent)
        {
            string content = remoteContentModule.remoteMyContent;
            // Use content...
        }
        else
        {
            // Use fallback content
        }
    }
}
```

---

## Content Deployment Pipeline

### GitHub Actions Workflow (sync-roadmap.yml)

```yaml
name: Sync Roadmap to Website
on:
  schedule:
    - cron: '0 */6 * * *'  # Every 6 hours
  workflow_dispatch:       # Manual trigger

jobs:
  sync-roadmap:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Web Repo
        uses: actions/checkout@v4
        with:
          repository: Owner/web-repo
          token: ${{ secrets.PAT_TOKEN }}
          path: web-target

      - name: Generate JSON
        run: |
          # Fetch from GitHub API, transform, save to web-target/public/data/

      - name: Commit and Push
        run: |
          git add public/data/
          git commit -m "chore: sync data"
          git push
```

### Content Sources

| Content | Source | Destination |
|---------|--------|-------------|
| roadmap.json | GitHub Issues API | basement-os-web/public/data/ |
| changelog.txt | Manual/Dropbox | basement-os-web/public/data/ |
| splash.txt | Manual | basement-os-web/public/data/ |

---

## Common Pitfalls

### 1. Redirect Errors
**Symptom:** "Redirect limit exceeded"
**Cause:** GitHub Pages custom domain redirect
**Fix:** Use direct custom domain URL

### 2. Empty VRCUrl in Inspector Queries
**Symptom:** MCP shows `"splashUrl": {}`
**Cause:** VRCUrl serialization format
**Fix:** Test in Play Mode - values are actually set

### 3. Content Not Updating
**Symptom:** Old content still showing
**Cause:** GitHub Pages cache (2-10 min) or VRChat cache
**Fix:** Wait for cache expiry, or add cache-busting query param

### 4. JSON Parse Failures
**Symptom:** Empty/missing data from JSON
**Cause:** Manual parsing edge cases
**Fix:** Add bounds checking, handle null sections

### 5. Multiple Downloads Colliding
**Symptom:** Wrong content in wrong field
**Cause:** Async callbacks not matching URL
**Fix:** Always check `result.Url.Get()` to match URL to content type

---

## Testing Checklist

- [ ] URLs accessible in browser (no 404)
- [ ] URLs don't redirect (check Network tab)
- [ ] Play Mode shows "SUCCESS" logs
- [ ] Content displays correctly in app
- [ ] Fallback content works when offline
- [ ] Retry logic triggers on failure

---

## Files Reference

| File | Purpose |
|------|---------|
| `Assets/Scripts/Modules/DT/DT_RemoteContent.cs` | Central content loader |
| `Assets/Editor/ConfigureRemoteContentUrls.cs` | URL configuration helper |
| `Assets/Editor/WireRemoteContentModule.cs` | Scene wiring automation |
| `.github/workflows/sync-roadmap.yml` | Content deployment pipeline |

---

## URL Configuration

**Production URLs (basementos.com):**
```
https://basementos.com/data/splash.txt
https://basementos.com/data/bios.txt
https://basementos.com/data/dashboard.txt
https://basementos.com/data/changelog.txt
https://basementos.com/data/roadmap.json
https://basementos.com/data/halloffame.txt
```

**Editor Menu:** `Tools/BasementOS/Configure Remote Content URLs`

---

*Skill created: December 31, 2025*
*Based on Issue #153 Remote Content Migration session*
