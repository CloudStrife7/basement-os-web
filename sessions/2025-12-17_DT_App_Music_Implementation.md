# Session Notes: DT_App_Music Implementation
**Date**: December 17, 2025
**Branch**: feat/music.exe
**Status**: ⏸️ Paused - Awaiting Manual Unity Inspector Setup

---

## Session Summary

Implemented the ProTV music player integration for Basement OS v2. Code is complete and compiled successfully, but Unity MCP tools encountered limitations with object reference assignments. Manual Inspector setup required to complete.

---

## ✅ Completed

### 1. Code Implementation
- **File**: `Assets/Scripts/BasementOS/BIN/DT_App_Music.cs` (497 lines)
- **Architecture Change**: Modified from `TVPlugin` to `UdonSharpBehaviour` to avoid cross-assembly inheritance issues
- **Integration**: Manual ProTV polling instead of event-based (reads OUT_ variables directly from TVManager)
- **Features Implemented**:
  - Playlist browser (10 visible tracks, scrollable)
  - Track navigation (UP/DOWN keys)
  - Track playback control (ACCEPT to play)
  - Real-time progress bar with time counters
  - Now playing display with track title
  - Return to Shell (LEFT key)

### 2. Asset Files Created
- **DT_App_Music.asset**: UdonSharp program source (`Assets/Scripts/BasementOS/BIN/`)
  - GUID: a97a9937f44fdc3428e48dadbf72c01a
  - Successfully compiled (compiledVersion: 2, no errors)
  - SerializedUdonProgram GUID: f2ae1ba4ffc382e478f6305164a3ee37

### 3. GameObject Setup
- **GameObject**: DT_App_Music
  - Parent: DT_Core (instanceID 41674)
  - Components: Transform, VRC.Udon.UdonBehaviour
  - **⚠️ UdonBehaviour NOT YET CONFIGURED** - programSource is null

### 4. Compilation Status
- ✅ Zero errors in Unity console
- ✅ DT_App_Music.cs compiled successfully to Udon
- ✅ All 133 UdonSharp scripts compiled (21.8s compile time)

---

## ⏸️ Remaining Tasks - RESUME HERE

### Step 1: Assign Program Source (CRITICAL)
**Action**: Select DT_App_Music GameObject in Unity Hierarchy

**Inspector Steps**:
1. Locate **UdonBehaviour** component
2. Click the **Program Source** dropdown (currently shows "None")
3. Assign: `Assets/Scripts/BasementOS/BIN/DT_App_Music.asset`
4. Unity will automatically upgrade the component and serialize fields

**Expected Result**: Inspector will show new fields (tvManager, playlistPlugin, coreReference, shellApp)

---

### Step 2: Wire Component References
**After Step 1 completes**, assign these references in DT_App_Music Inspector:

| Field | GameObject to Assign | Instance ID | Location |
|-------|---------------------|-------------|----------|
| **Tv Manager** | ProTV (Game Room) | 44820 | Scene Root → ProTV (Game Room) |
| **Playlist Plugin** | Playlist (Game Room) | 39980 | ProTV (Game Room) → Playlist (Game Room) |
| **Core Reference** | DT_Core | 41674 | Scene Root → DT_Core |
| **Shell App** | Shell | 41838 | DT_Core → Shell |

**Verification**: All 4 fields should have blue GameObject references (no "None" values)

---

### Step 3: Add MUSIC to DT_Shell Menu
**Action**: Select **Shell** GameObject (under DT_Core)

**Inspector Steps**:
1. In **DT_Shell** component, scroll to **Menu Items** section
2. Expand arrays (currently 6 items: Dashboard, Stats, Weather, GitHub, Games, Tales)
3. Increase array size to **7**
4. Assign new entry at **index [6]**:

```
menuTargets[6]     → DT_App_Music
menuNames[6]       → MUSIC
menuDescriptions[6]→ ProTV music player with playlist control
menuTypes[6]       → <APP>
```

**Verification**: Shell menu should now have 7 entries

---

### Step 4: Test in Play Mode
**Unity Editor Play Mode Testing**:

1. **Enter Play Mode**
2. **Interact with terminal** (using VRChat ClientSim or direct interaction)
3. **Navigate to MUSIC in Shell menu** (UP/DOWN keys)
4. **Press ACCEPT** to launch music app
5. **Verify Display**:
   - Shows "♪ NOW PLAYING" header
   - Displays playlist from ProTV (Game Room)
   - Cursor (>) appears on selected track
   - UP/DOWN navigates tracks
   - ACCEPT plays selected track
   - LEFT returns to Shell

**Expected Issues to Debug**:
- Playlist may be empty if ProTV (Game Room) has no tracks loaded
- Progress bar won't update without ProTV events (polling happens on input only)
- Track duration shows `[--:--]` (duration data not available from ProTV Playlist)

---

## Technical Notes

### Architecture Decision: UdonSharpBehaviour vs TVPlugin

**Problem**: Initial implementation extended `ArchiTech.ProTV.TVPlugin`, but UdonSharp compiler threw error:
```
Script with U# program asset referencing it must have an UdonSharpBehaviour definition
```

**Root Cause**: TVPlugin → ATEventHandler → ATBehaviour → UdonSharpBehaviour is a cross-assembly inheritance chain that UdonSharp can't serialize properly.

**Solution**: Changed DT_App_Music to extend `UdonSharpBehaviour` directly and manually poll ProTV state:

```csharp
// Old (broken):
public class DT_App_Music : TVPlugin

// New (working):
public class DT_App_Music : UdonSharpBehaviour
{
    [SerializeField] private UdonBehaviour tvManager;

    private void RefreshPlaybackState()
    {
        currentTitle = (string)tvManager.GetProgramVariable("OUT_TITLE");
        currentTime = (float)tvManager.GetProgramVariable("OUT_TIME");
        duration = (float)tvManager.GetProgramVariable("OUT_DURATION");
        isPlaying = (bool)tvManager.GetProgramVariable("OUT_PLAYING");
    }
}
```

**Trade-off**: No automatic ProTV events, but polling on `OnAppOpen()` and `OnInput()` is sufficient for terminal UI.

---

### ProTV Integration Details

**Data Sources**:
1. **TVManager (ProTV Game Room)**:
   - `OUT_TITLE` → Current track title
   - `OUT_TIME` → Current playback time (seconds)
   - `OUT_DURATION` → Track duration (seconds)
   - `OUT_PLAYING` → Playback state (bool)

2. **Playlist (Game Room)**:
   - `playlistData` → UdonBehaviour with track arrays
   - `playlistData.mainUrls[]` → Track URLs (VRCUrl[])
   - `playlistData.titles[]` → Track titles (string[])
   - `playlistData.trackCount` → Number of tracks (int)

**Playback Control**:
```csharp
playlistPlugin.SetProgramVariable("selectedIndex", trackIndex);
playlistPlugin.SendCustomEvent("PlaySelectedEntry");
```

---

### Display Layout (80×16 Terminal)

```
Line 0:  ════════════════════════════════════════════════════════════════════════════════
Line 1:   ♪ NOW PLAYING                                            [00:03:42 / 04:15:20]
Line 2:   LukHash - Raster Bar
Line 3:   [████████████████████░░░░░░░] 87%
Line 4:  ════════════════════════════════════════════════════════════════════════════════
Line 5:    01. Waveshaper - No Turning Back                                      [--:--]
Line 6:    02. Dance With The Dead - Diabolic                                    [--:--]
Line 7:  > 03. LukHash - Raster Bar                                       [--:--] ♪
Line 8:    04. Carpenter Brut - Turbo Killer                                     [--:--]
Line 9:    05. Perturbator - Humans Are Such Easy Prey                           [--:--]
Line 10:   06. Mega Drive - Acid Spit                                            [--:--]
Line 11:   07. GosT - Behemoth                                                   [--:--]
Line 12:   08. Power Glove - Streets of 2043                                     [--:--]
Line 13:   09. Dan Terminus - The Wrath of Code                                  [--:--]
Line 14:   10. Magic Sword - In The Face Of Evil                                 [--:--]
Line 15: ════════════════════════════════════════════════════════════════════════════════
Line 16:  [W/S] Navigate  [ACCEPT] Play  [A] Back                      Track 3 of 47
```

---

## Files Modified This Session

```
Assets/Scripts/BasementOS/BIN/DT_App_Music.cs (NEW - 497 lines)
Assets/Scripts/BasementOS/BIN/DT_App_Music.asset (NEW - UdonSharp program)
Assets/Editor/SetupDTAppMusic.cs (NEW - attempted automation, not working)
Docs/Modules/DT_App_Music_Setup.md (created previously)
CHANGELOG.md (updated previously)
```

---

## Unity MCP Limitations Encountered

**Issue**: `manage_gameobject` tool cannot set Unity Object references via JSON:
```json
{"programSource": "Assets/Scripts/BasementOS/BIN/DT_App_Music.asset"}
```

**Error**:
```
Input should be a valid dictionary [type=dict_type]
```

**Workaround**: Manual Inspector setup required (Steps 1-3 above)

**Future Improvement**: Consider using `manage_asset` or direct scene file editing for object references.

---

## Known Issues / Future Enhancements

### Current Limitations
1. **No real-time progress updates** - Progress only refreshes on user input (UP/DOWN/ACCEPT)
2. **No track duration display** - Shows `[--:--]` (ProTV Playlist doesn't expose individual track durations)
3. **No pause/resume control** - Only play selected track (no pause button)
4. **No volume control** - Would require additional TVManager integration

### Potential Improvements (Out of Scope)
- Subscribe to ProTV events via custom plugin system (if TVPlugin inheritance can be fixed)
- Add Update() loop to refresh progress bar every second (Quest performance consideration)
- Query track metadata from ProTV VideoPlayer for duration
- Add play/pause/stop controls
- Add volume slider

---

## References

- **Plan File**: `C:\Users\cloud\.claude\plans\sorted-hugging-taco.md`
- **Documentation**: `Docs/Modules/DT_App_Music_Setup.md`
- **Changelog**: `CHANGELOG.md` (UNRELEASED section, lines 7-31)
- **Related PR**: feat/music.exe branch (not yet merged)

---

## Follow-Up Session

**December 25, 2025**: Autonomous setup completed! See [2025-12-25_DT_App_Music_Completion.md](2025-12-25_DT_App_Music_Completion.md) for full automation solution using Editor script pattern.

---

## Next Session Checklist

- [ ] Complete Step 1: Assign Program Source
- [ ] Complete Step 2: Wire Component References
- [ ] Complete Step 3: Add MUSIC to DT_Shell menu
- [ ] Complete Step 4: Test in Play Mode
- [ ] Debug any playlist loading issues
- [ ] Verify ProTV playback control works
- [ ] Test with empty playlist (edge case)
- [ ] Test with 50+ track playlist (scrolling)
- [ ] Update CHANGELOG.md with testing results
- [ ] Consider committing changes or merging to main

---

**Resume Point**: Start with Step 1 - Assign Program Source in Unity Inspector

**Estimated Time to Complete**: 10-15 minutes (manual Inspector work) + 10 minutes testing

---

*Session ended: 6:35 PM PST*
*Unity Editor: Running, MCP Connected*
*Compilation Status: Clean (no errors)*
