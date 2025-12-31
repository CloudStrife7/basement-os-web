# DT_App_Music Event-Driven ProTV Integration

**Date:** 2025-12-26
**File:** `Assets/Scripts/BasementOS/BIN/DT_App_Music.cs`
**Status:** Complete

## Summary

Converted DT_App_Music from polling-based to event-driven ProTV 3.x integration, enabling real-time music player functionality in the Basement OS terminal.

## Changes Made

### 1. ProTV Event Registration (Start method)
- Registers as ProTV listener using `IN_LISTENER` and `IN_PRIORITY` (sbyte type)
- Calls `_RegisterListener` on TVManager

### 2. OUT_ Variables Added
```csharp
public VRCUrl OUT_URL;
public string OUT_TITLE = "";
public float OUT_TIME = 0f;
public float OUT_DURATION = 0f;
public bool OUT_PLAYING = false;
public float OUT_VOLUME = 0f;
public bool OUT_LOADING = false;
```

### 3. ProTV Event Handlers Implemented
- `_TvMediaReady()` - Media ready to play
- `_TvMediaChange()` - New media loaded
- `_TvPlay()` / `_TvPause()` / `_TvStop()` - Playback state changes
- `_TvVolumeChange()` - Volume updates
- `_TvTimeUpdate()` - Periodic time updates

### 4. Playlist Loading Fix
- Changed from looking for nested `playlistData` object to accessing `titles` and `mainUrls` arrays directly from the Playlist plugin

### 5. Track Selection Fix (SortView Handling)
- ProTV Playlist uses `sortView` array for shuffled playback
- `SwitchEntry(int)` expects a `sortViewIndex`, not original entry index
- Solution: Find the sortViewIndex where `sortView[i] == trackIndex`

```csharp
int[] sortView = (int[])playlistPlugin.GetProgramVariable("sortView");
for (int i = 0; i < sortView.Length; i++)
{
    if (sortView[i] == trackIndex)
    {
        sortViewIndex = i;
        break;
    }
}
playlistPlugin.SetProgramVariable("IN_INDEX", sortViewIndex);
playlistPlugin.SendCustomEvent("SwitchEntry");
```

### 6. UI Cleanup
- Removed "[STOPPED]" text from NOW PLAYING header

## Key Learnings

1. **ProTV IN_PRIORITY must be sbyte, not int** - Caused runtime crash
2. **ProTV Playlist stores data directly** - `titles` and `mainUrls` arrays are on the plugin, not nested
3. **SwitchEntry uses sortViewIndex** - Must map original entry index through sortView when shuffle is enabled

## Files Modified
- `Assets/Scripts/BasementOS/BIN/DT_App_Music.cs`
- `Assets/Editor/SetupDTAppMusic.cs` (v2.1 logging updates)

## Files Deleted
- `Assets/Scripts/In Development/ProTVBasementPlugin.cs` (obsolete)
- `Assets/Scripts/In Development/ProTVBasementPlugin.asset`

## Testing
Run via: `Tools > Setup DT_App_Music` then enter Play mode and navigate to MUSIC in the shell menu.
