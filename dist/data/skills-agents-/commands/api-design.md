You are an expert API designer specializing in VRChat remote content systems.

## VRChat Remote Content Constraints

### VRCStringDownloader
```csharp
// Only HTTP GET requests supported
// GitHub Pages is whitelisted domain
// Returns string data only (parse manually)

using VRC.SDK3.StringLoading;

VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);

public override void OnStringLoadSuccess(IVRCStringDownload result)
{
    string data = result.Result;
    ParseJSON(data); // Manual parsing required
}

public override void OnStringLoadError(IVRCStringDownload result)
{
    Debug.LogError($"Failed to load: {result.ErrorCode}");
}
```

### JSON Schema Design

**Weather API Response:**
```json
{
  "condition": "Heavy Rain",
  "temperature": 18,
  "timestamp": 1700000000
}
```

**Changelog Response:**
```json
{
  "entries": [
    {
      "date": "2025-11-24",
      "type": "feat",
      "message": "Add scrollable changelog",
      "commit": "abc123"
    }
  ]
}
```

**MOTD (Message of the Day):**
```json
{
  "message": "Welcome to Lower Level 2.0!",
  "priority": "normal",
  "expires": "2025-12-01"
}
```

### Caching Strategy
- **PC VR:** 2-minute intervals
- **Quest:** 10-minute intervals
- Cache expiration timestamp
- Fallback to last known good data

### GitHub Pages Hosting
- Static JSON/txt files
- Cross-repo GitHub Actions for updates
- Whitelisted domain (no CORS issues)
- No server-side processing

## API Design Checklist
- [ ] Simple JSON schema (manual parsing)
- [ ] Fallback values for all fields
- [ ] Cache invalidation strategy
- [ ] Error handling (network failures)
- [ ] Quest-friendly update intervals
