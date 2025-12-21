using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BASEMENT OS MUSIC PLAYER (v2.0)
///
/// ROLE: PROTV PLAYLIST BROWSER & MUSIC CONTROLLER
/// Multi-tab music player displaying ProTV playlist with playback controls.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Music.cs
///
/// INTEGRATION:
/// - ProTV: Manual integration with TVManager for playback events
/// - Core: Receives input events from DT_Core
/// - Core: Provides display content via GetDisplayContent()
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
    // PROTV INTEGRATION
    // =================================================================

    [Header("--- ProTV References ---")]
    [Tooltip("ProTV TVManager component")]
    [SerializeField] private UdonBehaviour tvManager;

    [Tooltip("ProTV Playlist plugin component")]
    [SerializeField] private UdonBehaviour playlistPlugin;

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
    private const int MAX_VISIBLE_TRACKS = 10;

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void Start()
    {
        // No special setup needed
    }

    public void OnAppOpen()
    {
        LoadPlaylistData();
        RefreshPlaybackState();
        RebuildDisplay();
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        cachedContent = "";
    }

    // =================================================================
    // PROTV INTEGRATION (Manual polling - no event system)
    // =================================================================

    // Note: Since we're not extending TVPlugin, we don't get automatic
    // ProTV events. We'll poll state when app opens and on input.

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
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
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT")
        {
            PlayTrack(selectedTrackIndex);
        }
        else if (inputKey == "LEFT")
        {
            ReturnToShell();
        }

        inputKey = "";
    }

    // =================================================================
    // PLAYBACK CONTROL
    // =================================================================

    private void PlayTrack(int trackIndex)
    {
        if (!Utilities.IsValid(playlistPlugin))
            return;

        // Tell Playlist plugin to play this track
        playlistPlugin.SetProgramVariable("selectedIndex", trackIndex);
        playlistPlugin.SendCustomEvent("PlaySelectedEntry");

        currentTrackIndex = trackIndex;
    }

    // =================================================================
    // DATA GATHERING
    // =================================================================

    private void LoadPlaylistData()
    {
        if (!Utilities.IsValid(playlistPlugin)) return;

        // Get playlist data from ProTV Playlist plugin
        object playlistDataObj = playlistPlugin.GetProgramVariable("playlistData");

        if (playlistDataObj != null)
        {
            UdonBehaviour playlistData = (UdonBehaviour)playlistDataObj;

            // PlaylistData has: urls[], titles[], trackCount
            VRCUrl[] urls = (VRCUrl[])playlistData.GetProgramVariable("mainUrls");
            string[] titles = (string[])playlistData.GetProgramVariable("titles");
            int count = (int)playlistData.GetProgramVariable("trackCount");

            trackUrls = urls;
            trackTitles = titles;
            trackCount = count;

            // Fallback if titles are empty
            if (trackTitles == null || trackTitles.Length == 0)
            {
                trackTitles = new string[trackCount];
                for (int i = 0; i < trackCount; i++)
                {
                    trackTitles[i] = "Track " + (i + 1).ToString();
                }
            }
        }
        else
        {
            // No playlist loaded - show empty state
            trackCount = 0;
            trackTitles = new string[0];
            trackUrls = new VRCUrl[0];
        }
    }

    private void RefreshPlaybackState()
    {
        if (!Utilities.IsValid(tvManager)) return;

        // Read ProTV OUT_ variables
        currentTitle = (string)tvManager.GetProgramVariable("OUT_TITLE");

        // Get time and duration from TVManager
        float time = (float)tvManager.GetProgramVariable("OUT_TIME");
        float dur = (float)tvManager.GetProgramVariable("OUT_DURATION");
        bool playing = (bool)tvManager.GetProgramVariable("OUT_PLAYING");

        currentTime = time;
        duration = dur;
        isPlaying = playing;

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
    // DISPLAY RENDERING
    // =================================================================

    private void RebuildDisplay()
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
            output = output + DT_Format.EnforceLine80(" \u266A NOW PLAYING                                                    [STOPPED]") + "\n";
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
            string controls = " [W/S] Navigate  [ACCEPT] Play  [A] Back";
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
            output = output + DT_Format.EnforceLine80(" [A] Back");
        }

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
        // Format: "> 01. Track Title                                       [03:47] ♪"
        // Prefix (2) + trackNum (2) + ". " (2) + title (var) + " [MM:SS]" (8) + playIndicator (3) = 80
        // Title max = 80 - 2 - 2 - 2 - 8 - 3 = 63 chars

        string title = trackTitles[trackIndex];
        if (title.Length > 63)
            title = title.Substring(0, 60) + "...";

        // Get duration if available (placeholder for now)
        string duration = "[--:--]";

        // Calculate padding to right-align duration
        int usedSpace = prefix.Length + trackNum.Length + 2 + title.Length + 1 + duration.Length + playIndicator.Length;
        int paddingNeeded = 80 - usedSpace;
        string padding = "";
        for (int i = 0; i < paddingNeeded; i++)
        {
            padding = padding + " ";
        }

        return prefix + trackNum + ". " + title + padding + duration + playIndicator;
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
