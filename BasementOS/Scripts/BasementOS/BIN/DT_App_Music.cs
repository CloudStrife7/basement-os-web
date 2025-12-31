using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

/// <summary>
/// BASEMENT OS MUSIC PLAYER (v2.1 - EVENT-DRIVEN)
///
/// ROLE: PROTV PLAYLIST BROWSER & MUSIC CONTROLLER
/// Multi-tab music player displaying ProTV playlist with playback controls.
/// Uses ProTV event-driven plugin pattern for real-time updates.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Music.cs
///
/// INTEGRATION:
/// - ProTV: Event-driven via _Tv* callbacks (registered as plugin)
/// - Core: Receives input events from DT_Core
/// - Core: Provides display content via GetDisplayContent()
///
/// PROTV REGISTRATION:
/// Must be added to TVManager's plugin list via SetupDTAppMusic editor script.
///
/// LIMITATIONS:
/// - Max 400 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Music : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // PROTV INTEGRATION - Event-Driven Plugin Pattern
    // =================================================================

    [Header("--- ProTV References ---")]
    [Tooltip("ProTV TVManager component")]
    [SerializeField] private UdonBehaviour tvManager;

    [Tooltip("ProTV Playlist plugin component")]
    [SerializeField] private UdonBehaviour playlistPlugin;

    // =================================================================
    // PROTV OUT_ VARIABLES (Injected by TVManager before events)
    // =================================================================

    [Header("--- ProTV Event Data (Auto-populated) ---")]
    [Tooltip("Current media URL - set by ProTV")]
    public VRCUrl OUT_URL;

    [Tooltip("Current media title - set by ProTV")]
    public string OUT_TITLE = "";

    [Tooltip("Current playback time in seconds - set by ProTV")]
    public float OUT_TIME = 0f;

    [Tooltip("Total media duration in seconds - set by ProTV")]
    public float OUT_DURATION = 0f;

    [Tooltip("Is media currently playing - set by ProTV")]
    public bool OUT_PLAYING = false;

    [Tooltip("Current volume level (0-1) - set by ProTV")]
    public float OUT_VOLUME = 0f;

    [Tooltip("Is media loading - set by ProTV")]
    public bool OUT_LOADING = false;

    // =================================================================
    // BASEMENT OS INTEGRATION
    // =================================================================

    [Header("--- Basement OS References ---")]
    [Tooltip("Reference to DT_Core")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Tooltip("Shell app to return to")]
    [SerializeField] private UdonSharpBehaviour shellApp;

    // =================================================================
    // PLAYLIST STATE
    // =================================================================

    private string[] trackTitles;
    private VRCUrl[] trackUrls;
    private int trackCount = 0;
    private int currentTrackIndex = 0;
    private int selectedTrackIndex = 0;

    // =================================================================
    // PLAYBACK STATE (From ProTV OUT_ variables)
    // =================================================================

    private bool isPlaying = false;
    private float currentTime = 0f;
    private float duration = 0f;
    private string currentTitle = "";

    // =================================================================
    // DISPLAY STATE
    // =================================================================

    private string cachedContent = "";
    private int scrollOffset = 0;
    private const int MAX_VISIBLE_TRACKS = 5;  // Reduced for cleaner UI
    private bool isAppOpen = false;
    private int loadingTrackIndex = -1;  // Track currently loading (-1 = none)

    // =================================================================
    // SETTINGS STATE
    // =================================================================

    private const string PLAYERDATA_MUTE_KEY = "basement_music_muted";
    private const int VIEW_PLAYLIST = 0;
    private const int VIEW_SETTINGS = 1;
    private int currentView = VIEW_PLAYLIST;
    private int settingsSelectedIndex = 0;
    private const int SETTINGS_OPTION_COUNT = 1;  // Currently just mute
    private bool isMuted = false;
    private float savedVolume = 1f;  // Volume before mute

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void Start()
    {
        // Register with ProTV TVManager to receive _Tv* events
        if (Utilities.IsValid(tvManager))
        {
            // ProTV 3.x expects IN_PRIORITY as sbyte, not int
            sbyte priority = 10;
            tvManager.SetProgramVariable("IN_LISTENER", (UdonBehaviour)GetComponent(typeof(UdonBehaviour)));
            tvManager.SetProgramVariable("IN_PRIORITY", priority);
            tvManager.SendCustomEvent("_RegisterListener");
            Debug.Log("[DT_App_Music] Registered as ProTV listener");
        }
        else
        {
            Debug.LogWarning("[DT_App_Music] TVManager not assigned - ProTV events will not be received");
        }

        // Load mute setting from PlayerData
        LoadMuteSetting();
    }

    private void LoadMuteSetting()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        if (PlayerData.HasKey(localPlayer, PLAYERDATA_MUTE_KEY))
        {
            isMuted = PlayerData.GetBool(localPlayer, PLAYERDATA_MUTE_KEY);
            Debug.Log("[DT_App_Music] Loaded mute setting: " + isMuted.ToString());

            // Apply mute state to ProTV
            if (isMuted)
            {
                ApplyMute();
            }
        }
    }

    private void ApplyMute()
    {
        if (!Utilities.IsValid(tvManager)) return;

        // Store current volume before muting
        object volObj = tvManager.GetProgramVariable("OUT_VOLUME");
        if (volObj != null && !isMuted)
        {
            savedVolume = (float)volObj;
            if (savedVolume < 0.1f) savedVolume = 1f;  // Don't save 0 as "previous"
        }

        // Set volume to 0
        tvManager.SetProgramVariable("IN_VOLUME", 0f);
        tvManager.SendCustomEvent("_ChangeVolume");
        Debug.Log("[DT_App_Music] Muted TV");
    }

    private void ApplyUnmute()
    {
        if (!Utilities.IsValid(tvManager)) return;

        // Restore previous volume
        tvManager.SetProgramVariable("IN_VOLUME", savedVolume);
        tvManager.SendCustomEvent("_ChangeVolume");
        Debug.Log("[DT_App_Music] Unmuted TV, volume: " + savedVolume.ToString());
    }

    public void OnAppOpen()
    {
        isAppOpen = true;
        currentView = VIEW_PLAYLIST;  // Always start on playlist
        settingsSelectedIndex = 0;
        LoadPlaylistData();
        RefreshPlaybackState();
        RebuildDisplay();
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        isAppOpen = false;
        cachedContent = "";
    }

    // =================================================================
    // PROTV EVENT HANDLERS (Called by TVManager when registered as plugin)
    // =================================================================

    /// <summary>
    /// Called when media is ready to play. OUT_ variables are populated.
    /// </summary>
    public void _TvMediaReady()
    {
        loadingTrackIndex = -1;  // Clear loading state - media is ready
        SyncFromOutVariables();
        if (isAppOpen)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    /// <summary>
    /// Called when media changes (new URL loaded).
    /// </summary>
    public void _TvMediaChange()
    {
        SyncFromOutVariables();
        if (isAppOpen)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    /// <summary>
    /// Called when playback starts.
    /// </summary>
    public void _TvPlay()
    {
        isPlaying = true;
        SyncFromOutVariables();
        if (isAppOpen)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    /// <summary>
    /// Called when playback pauses.
    /// </summary>
    public void _TvPause()
    {
        isPlaying = false;
        if (isAppOpen)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    /// <summary>
    /// Called when playback stops.
    /// </summary>
    public void _TvStop()
    {
        isPlaying = false;
        currentTime = 0f;
        loadingTrackIndex = -1;  // Clear loading state on stop
        if (isAppOpen)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    /// <summary>
    /// Called when volume changes.
    /// </summary>
    public void _TvVolumeChange()
    {
        // Volume changes don't affect our display, but we track it
        // for potential future use
    }

    /// <summary>
    /// Called periodically with updated time info during playback.
    /// </summary>
    public void _TvTimeUpdate()
    {
        currentTime = OUT_TIME;
        if (isAppOpen && isPlaying)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    /// <summary>
    /// Sync internal state from ProTV OUT_ variables.
    /// </summary>
    private void SyncFromOutVariables()
    {
        currentTitle = OUT_TITLE;
        currentTime = OUT_TIME;
        duration = OUT_DURATION;
        isPlaying = OUT_PLAYING;

        // Find current track in playlist by matching title
        for (int i = 0; i < trackCount; i++)
        {
            if (trackTitles[i] == currentTitle)
            {
                currentTrackIndex = i;
                break;
            }
        }
    }


    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        Debug.Log("[DT_App_Music] OnInput received: '" + inputKey + "' view=" + currentView.ToString());

        if (currentView == VIEW_PLAYLIST)
        {
            HandlePlaylistInput();
        }
        else if (currentView == VIEW_SETTINGS)
        {
            HandleSettingsInput();
        }

        inputKey = "";
    }

    private void HandlePlaylistInput()
    {
        if (inputKey == "UP")
        {
            selectedTrackIndex--;
            if (selectedTrackIndex < 0) selectedTrackIndex = trackCount - 1;

            // Scroll display if needed
            if (selectedTrackIndex < scrollOffset)
                scrollOffset = selectedTrackIndex;

            RebuildDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "DOWN")
        {
            selectedTrackIndex++;
            if (selectedTrackIndex >= trackCount) selectedTrackIndex = 0;

            // Scroll display if needed
            if (selectedTrackIndex >= scrollOffset + MAX_VISIBLE_TRACKS)
                scrollOffset = selectedTrackIndex - MAX_VISIBLE_TRACKS + 1;

            RebuildDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "ACCEPT")
        {
            Debug.Log("[DT_App_Music] ACCEPT pressed - calling PlayTrack(" + selectedTrackIndex + ")");
            PlayTrack(selectedTrackIndex);
        }
        else if (inputKey == "RIGHT")
        {
            // Open settings menu
            currentView = VIEW_SETTINGS;
            settingsSelectedIndex = 0;
            RebuildDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "LEFT")
        {
            ReturnToShell();
        }
    }

    private void HandleSettingsInput()
    {
        if (inputKey == "UP")
        {
            settingsSelectedIndex--;
            if (settingsSelectedIndex < 0) settingsSelectedIndex = SETTINGS_OPTION_COUNT - 1;
            RebuildDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "DOWN")
        {
            settingsSelectedIndex++;
            if (settingsSelectedIndex >= SETTINGS_OPTION_COUNT) settingsSelectedIndex = 0;
            RebuildDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT")
        {
            // Toggle selected setting
            if (settingsSelectedIndex == 0)
            {
                ToggleMute();
            }
            RebuildDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "LEFT")
        {
            // Return to playlist view
            currentView = VIEW_PLAYLIST;
            RebuildDisplay();
            PushDisplayToCore();
        }
    }

    private void ToggleMute()
    {
        isMuted = !isMuted;

        // Apply to ProTV
        if (isMuted)
        {
            ApplyMute();
        }
        else
        {
            ApplyUnmute();
        }

        // Save to PlayerData
        PlayerData.SetBool(PLAYERDATA_MUTE_KEY, isMuted);
        Debug.Log("[DT_App_Music] Mute toggled: " + isMuted.ToString() + " (saved to PlayerData)");
    }

    // =================================================================
    // PLAYBACK CONTROL
    // =================================================================

    private void PlayTrack(int trackIndex)
    {
        if (!Utilities.IsValid(playlistPlugin))
        {
            Debug.LogWarning("[DT_App_Music] playlistPlugin not valid - cannot play track");
            return;
        }

        // Set loading state immediately for visual feedback
        loadingTrackIndex = trackIndex;
        if (isAppOpen)
        {
            RebuildDisplay();
            PushDisplayToCore();
        }

        // ProTV Playlist uses sortView for shuffled playback
        // SwitchEntry(int) expects a sortViewIndex, not the original entry index
        // We need to find where trackIndex appears in sortView
        object sortViewObj = playlistPlugin.GetProgramVariable("sortView");
        if (sortViewObj != null)
        {
            int[] sortView = (int[])sortViewObj;
            int sortViewIndex = -1;

            // Find the sortViewIndex where sortView[i] == trackIndex
            for (int i = 0; i < sortView.Length; i++)
            {
                if (sortView[i] == trackIndex)
                {
                    sortViewIndex = i;
                    break;
                }
            }

            if (sortViewIndex >= 0)
            {
                playlistPlugin.SetProgramVariable("IN_INDEX", sortViewIndex);
                playlistPlugin.SendCustomEvent("SwitchEntry");
                Debug.Log("[DT_App_Music] Playing track " + trackIndex.ToString() + " (sortViewIndex=" + sortViewIndex.ToString() + ")");
                currentTrackIndex = trackIndex;
            }
            else
            {
                Debug.LogWarning("[DT_App_Music] Track " + trackIndex.ToString() + " not found in sortView");
                loadingTrackIndex = -1;  // Clear loading state on error
            }
        }
        else
        {
            Debug.LogWarning("[DT_App_Music] Could not get sortView from playlistPlugin");
            loadingTrackIndex = -1;  // Clear loading state on error
        }
    }

    // =================================================================
    // DATA GATHERING
    // =================================================================

    private void LoadPlaylistData()
    {
        if (!Utilities.IsValid(playlistPlugin))
        {
            Debug.LogWarning("[DT_App_Music] playlistPlugin not assigned");
            trackCount = 0;
            trackTitles = new string[0];
            trackUrls = new VRCUrl[0];
            return;
        }

        // ProTV Playlist plugin stores titles and mainUrls arrays directly
        object titlesObj = playlistPlugin.GetProgramVariable("titles");
        object urlsObj = playlistPlugin.GetProgramVariable("mainUrls");

        if (titlesObj != null)
        {
            trackTitles = (string[])titlesObj;
            trackCount = trackTitles.Length;
            Debug.Log("[DT_App_Music] Loaded " + trackCount.ToString() + " tracks from playlist");
        }
        else
        {
            Debug.LogWarning("[DT_App_Music] No titles array found on playlistPlugin");
            trackTitles = new string[0];
            trackCount = 0;
        }

        if (urlsObj != null)
        {
            trackUrls = (VRCUrl[])urlsObj;
        }
        else
        {
            trackUrls = new VRCUrl[0];
        }

        // Generate fallback titles if empty
        if (trackTitles == null || trackTitles.Length == 0)
        {
            if (trackUrls != null && trackUrls.Length > 0)
            {
                trackCount = trackUrls.Length;
                trackTitles = new string[trackCount];
                for (int i = 0; i < trackCount; i++)
                {
                    trackTitles[i] = "Track " + (i + 1).ToString();
                }
            }
        }
    }

    private void RefreshPlaybackState()
    {
        // Sync from OUT_ variables if they've been populated by events
        // Otherwise fall back to polling TVManager
        if (OUT_TITLE != null && OUT_TITLE.Length > 0)
        {
            SyncFromOutVariables();
        }
        else if (Utilities.IsValid(tvManager))
        {
            // Poll TVManager for current state (fallback)
            object titleObj = tvManager.GetProgramVariable("OUT_TITLE");
            if (titleObj != null) currentTitle = (string)titleObj;

            object timeObj = tvManager.GetProgramVariable("OUT_TIME");
            if (timeObj != null) currentTime = (float)timeObj;

            object durObj = tvManager.GetProgramVariable("OUT_DURATION");
            if (durObj != null) duration = (float)durObj;

            object playingObj = tvManager.GetProgramVariable("OUT_PLAYING");
            if (playingObj != null) isPlaying = (bool)playingObj;

            // Find current track in playlist by matching title
            for (int i = 0; i < trackCount; i++)
            {
                if (trackTitles[i] == currentTitle)
                {
                    currentTrackIndex = i;
                    break;
                }
            }
        }
    }

    // =================================================================
    // DISPLAY RENDERING
    // =================================================================

    private void RebuildDisplay()
    {
        if (currentView == VIEW_SETTINGS)
        {
            RebuildSettingsDisplay();
            return;
        }

        RebuildPlaylistDisplay();
    }

    private void RebuildPlaylistDisplay()
    {
        string output = "";

        // Line 0: Top separator
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Line 1: Now Playing Header
        if (isPlaying || duration > 0f)
        {
            string timeCurrent = FormatTime(currentTime);
            string timeDuration = FormatTime(duration);
            string timeDisplay = "[" + timeCurrent + " / " + timeDuration + "]";

            // Calculate padding to right-align time display
            string header = " \u266A NOW PLAYING";
            if (isMuted) header = " \u266A NOW PLAYING [MUTED]";
            int paddingLen = 80 - header.Length - timeDisplay.Length;
            string padding = "";
            for (int i = 0; i < paddingLen; i++)
            {
                padding = padding + " ";
            }
            output = output + DT_Format.EnforceLine80(header + padding + timeDisplay) + "\n";
        }
        else
        {
            string header = " \u266A NOW PLAYING";
            if (isMuted) header = " \u266A NOW PLAYING [MUTED]";
            output = output + DT_Format.EnforceLine80(header) + "\n";
        }

        // Line 2: Track Title
        if (currentTitle != null && currentTitle != "")
        {
            output = output + DT_Format.EnforceLine80(" " + TruncateTitle(currentTitle, 78)) + "\n";
        }
        else
        {
            output = output + DT_Format.EnforceLine80(" No track loaded") + "\n";
        }

        // Line 3: Progress Bar
        if (duration > 0f)
        {
            float percent = currentTime / duration;
            if (percent > 1f) percent = 1f;
            if (percent < 0f) percent = 0f;

            string progressBar = GenerateProgressBar(percent);
            int percentInt = (int)(percent * 100f);
            string percentText = percentInt.ToString() + "%";

            output = output + DT_Format.EnforceLine80(" " + progressBar + " " + percentText) + "\n";
        }
        else
        {
            output = output + DT_Format.EnforceLine80("") + "\n";
        }

        // Line 4: Separator
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Lines 5-14: Scrollable Playlist (10 visible tracks)
        if (trackCount > 0)
        {
            int endIdx = scrollOffset + MAX_VISIBLE_TRACKS;
            if (endIdx > trackCount) endIdx = trackCount;

            for (int i = scrollOffset; i < endIdx; i++)
            {
                string trackLine = FormatTrackLine(i);
                output = output + DT_Format.EnforceLine80(trackLine) + "\n";
            }

            // Fill remaining lines if fewer than 10 tracks visible
            int linesRendered = endIdx - scrollOffset;
            for (int i = linesRendered; i < MAX_VISIBLE_TRACKS; i++)
            {
                output = output + DT_Format.EnforceLine80("") + "\n";
            }
        }
        else
        {
            output = output + DT_Format.EnforceLine80(" NO PLAYLIST LOADED") + "\n";
            for (int i = 1; i < MAX_VISIBLE_TRACKS; i++)
            {
                output = output + DT_Format.EnforceLine80("") + "\n";
            }
        }

        // Line 15: Bottom separator
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Line 16: Controls footer with track counter
        if (trackCount > 0)
        {
            string controls = " [W/S] Nav  [D] Play  [>] Settings  [A] Back";
            int currentDisplayNum = selectedTrackIndex + 1;
            string trackCounter = "Track " + currentDisplayNum.ToString() + " of " + trackCount.ToString();

            int paddingLen = 80 - controls.Length - trackCounter.Length;
            string padding = "";
            for (int i = 0; i < paddingLen; i++)
            {
                padding = padding + " ";
            }

            output = output + DT_Format.EnforceLine80(controls + padding + trackCounter);
        }
        else
        {
            output = output + DT_Format.EnforceLine80(" [>] Settings  [A] Back");
        }

        cachedContent = output;
    }

    private void RebuildSettingsDisplay()
    {
        string output = "";

        // Line 0: Top separator
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Line 1: Settings Header
        output = output + DT_Format.EnforceLine80(" MUSIC.EXE SETTINGS") + "\n";

        // Line 2: Empty
        output = output + DT_Format.EnforceLine80("") + "\n";

        // Line 3: Separator
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Settings options
        string mutePrefix = (settingsSelectedIndex == 0) ? "> " : "  ";
        string muteStatus = isMuted ? "[X] MUTED" : "[ ] MUTED";
        string muteDesc = isMuted ? " (Audio disabled)" : " (Audio enabled)";
        output = output + DT_Format.EnforceLine80(mutePrefix + muteStatus + muteDesc) + "\n";

        // Fill remaining lines (4 more to match playlist area)
        for (int i = 0; i < 4; i++)
        {
            output = output + DT_Format.EnforceLine80("") + "\n";
        }

        // Bottom separator
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Controls footer
        output = output + DT_Format.EnforceLine80(" [W/S] Navigate  [D] Toggle  [A] Back");

        cachedContent = output;
    }

    private string FormatTrackLine(int trackIndex)
    {
        string prefix = "  ";

        // Cursor indicator
        if (trackIndex == selectedTrackIndex)
            prefix = "> ";

        // Track number with padding
        string trackNum = PadLeft((trackIndex + 1).ToString(), 2);

        // Currently playing indicator
        string playIndicator = "   ";
        if (trackIndex == currentTrackIndex && isPlaying)
            playIndicator = " \u266A";

        // Track title (truncate to fit within 80 chars)
        // Format: "> 01. Track Title                                            ♪"
        // Prefix (2) + trackNum (2) + ". " (2) + title (var) + playIndicator (3) = 80
        // With [LOADING]: subtract 10 more chars
        // Title max without loading = 80 - 2 - 2 - 2 - 3 = 71 chars
        // Title max with loading = 71 - 10 = 61 chars

        string title = trackTitles[trackIndex];

        // Show loading state only when actively loading
        string statusText = "";
        if (trackIndex == loadingTrackIndex)
        {
            statusText = "[LOADING]";
            if (title.Length > 58)
                title = title.Substring(0, 55) + "...";
        }
        else
        {
            if (title.Length > 68)
                title = title.Substring(0, 65) + "...";
        }

        // Calculate padding to right-align status and play indicator
        int usedSpace = prefix.Length + trackNum.Length + 2 + title.Length + statusText.Length + playIndicator.Length;
        int paddingNeeded = 80 - usedSpace;
        string padding = "";
        for (int i = 0; i < paddingNeeded; i++)
        {
            padding = padding + " ";
        }

        return prefix + trackNum + ". " + title + padding + statusText + playIndicator;
    }

    private string GenerateProgressBar(float percent)
    {
        int barWidth = 25;
        int filled = (int)(percent * barWidth);

        string bar = "[";
        for (int i = 0; i < barWidth; i++)
        {
            if (i < filled)
                bar = bar + "\u2588"; // █
            else
                bar = bar + "\u2591"; // ░
        }
        bar = bar + "]";

        return bar;
    }

    private string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);

        string minStr = minutes.ToString();
        string secStr = secs.ToString();
        if (secs < 10) secStr = "0" + secStr;

        return minStr + ":" + secStr;
    }

    private string TruncateTitle(string title, int maxLen)
    {
        if (title == null) return "";
        if (title.Length <= maxLen) return title;
        return title.Substring(0, maxLen - 3) + "...";
    }

    private string PadLeft(string text, int width)
    {
        if (text.Length >= width) return text;
        string result = "";
        for (int i = text.Length; i < width; i++)
        {
            result = result + " ";
        }
        return result + text;
    }

    public string GetDisplayContent()
    {
        if (cachedContent == "") RebuildDisplay();
        return cachedContent;
    }

    // =================================================================
    // CORE COMMUNICATION
    // =================================================================

    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", cachedContent);
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    private void ReturnToShell()
    {
        if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
        {
            coreReference.SetProgramVariable("nextProcess", shellApp);
            coreReference.SendCustomEvent("LoadNextProcess");
        }
    }
}
