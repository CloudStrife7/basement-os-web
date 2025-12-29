using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Persistence;
using VRC.Udon;

/// <summary>
/// BASEMENT OS SETTINGS APPLICATION (v2.1)
///
/// ROLE: USER PREFERENCE MANAGEMENT
/// Persistent settings stored in PlayerData.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Settings.cs
///
/// INTEGRATION:
/// - Shell: Menu entry as [CFG] type
/// - Core: Reads settings for display behavior
/// - Notifications: Reads settings for popup behavior
/// - Boot: Reads settings for boot sequence mode
/// - Uses: Terminal_Style_Guide.md box format
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Settings : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // MODULE REFERENCES
    // =================================================================

    [Header("--- Core Reference ---")]
    [SerializeField] private UdonSharpBehaviour coreReference;
    [SerializeField] private UdonSharpBehaviour shellApp;

    [Header("--- Systems to Notify ---")]
    [SerializeField] private UdonSharpBehaviour notificationHub;
    [SerializeField] private UdonSharpBehaviour bootSequence;
    [SerializeField] private UdonSharpBehaviour weatherModule;

    // =================================================================
    // PLAYERDATA KEYS
    // =================================================================

    // Notification settings
    private const string KEY_ACHIEVEMENT_DISPLAY = "Settings_AchievementDisplay";  // 0=HUD, 1=TV, 2=BOTH
    private const string KEY_LOGIN_NOTIFICATIONS = "Settings_LoginNotifications";
    private const string KEY_SOUND_EFFECTS = "Settings_SoundEffects";

    // Display settings
    private const string KEY_WEATHER_HEADER = "Settings_WeatherHeader";
    private const string KEY_RSS_TICKER = "Settings_RssTicker";
    private const string KEY_BOOT_MODE = "Settings_BootMode";  // 0=FULL, 1=QUICK, 2=SKIP

    // Accessibility settings
    private const string KEY_HIGH_CONTRAST = "Settings_HighContrast";
    private const string KEY_LARGE_CURSOR = "Settings_LargeCursor";
    private const string KEY_SLOW_SCROLL = "Settings_SlowScroll";

    // =================================================================
    // SETTINGS DATA
    // =================================================================

    // Section 0: Notifications
    private int settingAchievementDisplay = 0;  // 0=HUD, 1=TV, 2=BOTH
    private bool settingLoginNotifications = true;
    private bool settingSoundEffects = true;

    // Section 1: Display
    private bool settingWeatherHeader = true;
    private bool settingRssTicker = true;
    private int settingBootMode = 0;  // 0=FULL, 1=QUICK, 2=SKIP

    // Section 2: Accessibility
    private bool settingHighContrast = false;
    private bool settingLargeCursor = false;
    private bool settingSlowScroll = false;

    // =================================================================
    // MENU STATE
    // =================================================================

    private const int SECTION_NOTIFICATIONS = 0;
    private const int SECTION_DISPLAY = 1;
    private const int SECTION_ACCESSIBILITY = 2;
    private const int SECTION_COUNT = 3;

    private int currentSection = 0;
    private int cursorIndex = 0;
    private bool unsavedChanges = false;

    // Items per section
    private int[] itemsPerSection = { 3, 3, 3 };  // 3 items in each

    // Mode name arrays (allocated once to avoid Quest performance issues)
    private string[] achievementDisplayModes = { "HUD", "TV", "BOTH" };
    private string[] bootModes = { "FULL", "QUICK", "SKIP" };

    // =================================================================
    // BOX DIMENSIONS & COLORS (Terminal_Style_Guide.md)
    // =================================================================

    private const int WIDTH = 80;
    private const int CONTENT_W = 76;

    private const string COLOR_BORDER = "#065F46";
    private const string COLOR_PRIMARY = "#10B981";
    private const string COLOR_HEADER = "#6EE7B7";
    private const string COLOR_HIGHLIGHT = "#34D399";
    private const string COLOR_WARN = "#FBBF24";

    // =================================================================
    // INITIALIZATION
    // =================================================================

    /// <summary>
    /// Validates serialized field references on startup
    /// </summary>
    private void Start()
    {
        if (coreReference == null)
        {
            Debug.LogError("[DT_App_Settings] coreReference not assigned!");
        }
        if (shellApp == null)
        {
            Debug.LogError("[DT_App_Settings] shellApp not assigned!");
        }
        if (notificationHub == null)
        {
            Debug.LogWarning("[DT_App_Settings] notificationHub not assigned - notifications will not work");
        }
        if (bootSequence == null)
        {
            Debug.LogWarning("[DT_App_Settings] bootSequence not assigned - boot mode changes will not work");
        }
    }

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        LoadAllSettings();
        currentSection = 0;
        cursorIndex = 0;
        unsavedChanges = false;
        ShowSettings();
    }

    public void OnAppClose()
    {
        if (unsavedChanges)
        {
            SaveAllSettings();
            NotifySystems();
        }
    }

    // =================================================================
    // SETTINGS LOAD/SAVE
    // =================================================================

    private void LoadAllSettings()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return;

        // Notifications
        settingAchievementDisplay = GetIntSetting(player, KEY_ACHIEVEMENT_DISPLAY, 0);  // Default: HUD
        settingLoginNotifications = GetBoolSetting(player, KEY_LOGIN_NOTIFICATIONS, true);
        settingSoundEffects = GetBoolSetting(player, KEY_SOUND_EFFECTS, true);

        // Display (default: true, boot mode 0)
        settingWeatherHeader = GetBoolSetting(player, KEY_WEATHER_HEADER, true);
        settingRssTicker = GetBoolSetting(player, KEY_RSS_TICKER, true);
        settingBootMode = GetIntSetting(player, KEY_BOOT_MODE, 0);

        // Accessibility (default: false)
        settingHighContrast = GetBoolSetting(player, KEY_HIGH_CONTRAST, false);
        settingLargeCursor = GetBoolSetting(player, KEY_LARGE_CURSOR, false);
        settingSlowScroll = GetBoolSetting(player, KEY_SLOW_SCROLL, false);
    }

    private bool GetBoolSetting(VRCPlayerApi player, string key, bool defaultValue)
    {
        if (!PlayerData.HasKey(player, key))
        {
            return defaultValue;
        }
        return PlayerData.GetInt(player, key) == 1;
    }

    private int GetIntSetting(VRCPlayerApi player, string key, int defaultValue)
    {
        if (!PlayerData.HasKey(player, key))
        {
            return defaultValue;
        }
        return PlayerData.GetInt(player, key);
    }

    private void SaveAllSettings()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return;

        // Notifications - SetInt only takes (key, value), always sets for local player
        PlayerData.SetInt(KEY_ACHIEVEMENT_DISPLAY, settingAchievementDisplay);
        PlayerData.SetInt(KEY_LOGIN_NOTIFICATIONS, settingLoginNotifications ? 1 : 0);
        PlayerData.SetInt(KEY_SOUND_EFFECTS, settingSoundEffects ? 1 : 0);

        // Display
        PlayerData.SetInt(KEY_WEATHER_HEADER, settingWeatherHeader ? 1 : 0);
        PlayerData.SetInt(KEY_RSS_TICKER, settingRssTicker ? 1 : 0);
        PlayerData.SetInt(KEY_BOOT_MODE, settingBootMode);

        // Accessibility
        PlayerData.SetInt(KEY_HIGH_CONTRAST, settingHighContrast ? 1 : 0);
        PlayerData.SetInt(KEY_LARGE_CURSOR, settingLargeCursor ? 1 : 0);
        PlayerData.SetInt(KEY_SLOW_SCROLL, settingSlowScroll ? 1 : 0);

        unsavedChanges = false;
    }

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        if (inputKey == "UP")
        {
            MoveCursorUp();
        }
        else if (inputKey == "DOWN")
        {
            MoveCursorDown();
        }
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT")
        {
            ToggleCurrentSetting();
        }
        else if (inputKey == "LEFT")
        {
            SaveAndExit();
        }
        else if (inputKey == "RESET")  // 'D' key mapped to RESET
        {
            ResetAllSettings();
        }

        inputKey = "";
    }

    private void MoveCursorUp()
    {
        cursorIndex--;

        if (cursorIndex < 0)
        {
            // Move to previous section
            currentSection--;
            if (currentSection < 0)
            {
                currentSection = SECTION_COUNT - 1;
            }
            cursorIndex = itemsPerSection[currentSection] - 1;
        }

        ShowSettings();
    }

    private void MoveCursorDown()
    {
        cursorIndex++;

        if (cursorIndex >= itemsPerSection[currentSection])
        {
            // Move to next section
            currentSection++;
            if (currentSection >= SECTION_COUNT)
            {
                currentSection = 0;
            }
            cursorIndex = 0;
        }

        ShowSettings();
    }

    private void ToggleCurrentSetting()
    {
        unsavedChanges = true;

        if (currentSection == SECTION_NOTIFICATIONS)
        {
            if (cursorIndex == 0)
            {
                // Cycle achievement display mode: HUD -> TV -> BOTH -> HUD
                settingAchievementDisplay++;
                if (settingAchievementDisplay > 2) settingAchievementDisplay = 0;
            }
            else if (cursorIndex == 1) settingLoginNotifications = !settingLoginNotifications;
            else if (cursorIndex == 2) settingSoundEffects = !settingSoundEffects;
        }
        else if (currentSection == SECTION_DISPLAY)
        {
            if (cursorIndex == 0) settingWeatherHeader = !settingWeatherHeader;
            else if (cursorIndex == 1) settingRssTicker = !settingRssTicker;
            else if (cursorIndex == 2)
            {
                // Cycle boot mode
                settingBootMode++;
                if (settingBootMode > 2) settingBootMode = 0;
            }
        }
        else if (currentSection == SECTION_ACCESSIBILITY)
        {
            if (cursorIndex == 0) settingHighContrast = !settingHighContrast;
            else if (cursorIndex == 1) settingLargeCursor = !settingLargeCursor;
            else if (cursorIndex == 2) settingSlowScroll = !settingSlowScroll;
        }

        ShowSettings();
    }

    private void ResetAllSettings()
    {
        // Reset to defaults
        settingAchievementDisplay = 0;  // HUD
        settingLoginNotifications = true;
        settingSoundEffects = true;
        settingWeatherHeader = true;
        settingRssTicker = true;
        settingBootMode = 0;
        settingHighContrast = false;
        settingLargeCursor = false;
        settingSlowScroll = false;

        unsavedChanges = true;
        ShowSettings();
    }

    private void SaveAndExit()
    {
        SaveAllSettings();
        NotifySystems();
        ReturnToShell();
    }

    private void ReturnToShell()
    {
        if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
        {
            coreReference.SetProgramVariable("nextProcess", shellApp);
            coreReference.SendCustomEvent("LoadNextProcess");
        }
    }

    // =================================================================
    // DISPLAY GENERATION
    // =================================================================

    /// <summary>
    /// Wrap content in box borders. Content should already be CONTENT_W visible chars.
    /// </summary>
    private string BoxRow(string content)
    {
        // Ensure content is exactly CONTENT_W to prevent border misalignment
        // Don't use DT_Format.PadLeft on content with color tags - it truncates them
        if (content == null) content = "";

        // Strip color tags for length check, then pad the original
        string plainContent = DT_Format.StripColorTags(content);
        if (plainContent.Length < CONTENT_W)
        {
            // Pad after any existing content
            int padding = CONTENT_W - plainContent.Length;
            for (int i = 0; i < padding; i++)
            {
                content = content + " ";
            }
        }

        return "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> " +
               content +
               " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
    }

    private void ShowSettings()
    {
        string o = "";

        // Top border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxTop(WIDTH) + "</color>\n";

        // Title with unsaved indicator
        string titleText = "SETTINGS";
        if (unsavedChanges)
        {
            titleText = titleText + " <color=" + COLOR_WARN + ">[*]</color>";
        }
        o = o + BoxRow("<color=" + COLOR_HEADER + ">" + DT_Format.PadCenter(titleText, CONTENT_W) + "</color>") + "\n";

        // Divider
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "</color>\n";

        // Generate each section
        o = o + GenerateSectionNotifications();
        o = o + GenerateSectionDisplay();
        o = o + GenerateSectionAccessibility();

        // Bottom border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxBottom(WIDTH) + "</color>\n";

        // Navigation footer
        o = o + " <color=" + COLOR_PRIMARY + ">[W/S] Navigate  [D] Toggle  [A] Back (saves)</color>\n";

        PushDisplayToCore(o);
    }

    private string GenerateSectionHeader(string title, int sectionId)
    {
        string cursor = (currentSection == sectionId) ? ">" : " ";
        string row = "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> ";
        row = row + "<color=" + COLOR_HEADER + ">" + cursor + " " + title + "</color>";
        row = row + " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
        return row + "\n";
    }

    private string GenerateSectionNotifications()
    {
        string o = GenerateSectionHeader("NOTIFICATIONS", SECTION_NOTIFICATIONS);
        o = o + GenerateAchievementDisplayLine();
        o = o + GenerateSettingLine(0, 1, "Login Notifications", settingLoginNotifications, "Welcome msg");
        o = o + GenerateSettingLine(0, 2, "Sound Effects", settingSoundEffects, "Play sounds");
        return o;
    }

    private string GenerateAchievementDisplayLine()
    {
        bool isSelected = (currentSection == SECTION_NOTIFICATIONS && cursorIndex == 0);
        string cursor = isSelected ? ">" : " ";
        string color = isSelected ? COLOR_HIGHLIGHT : COLOR_PRIMARY;
        string valueStr = "[" + achievementDisplayModes[settingAchievementDisplay] + "]";

        string row = "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> ";
        row = row + "<color=" + color + ">  " + cursor + " " + DT_Format.PadLeft("Achievement Display", 18) + " " + valueStr + " HUD/TV/BOTH</color>";
        row = row + " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
        return row + "\n";
    }

    private string GenerateSectionDisplay()
    {
        string o = GenerateSectionHeader("DISPLAY", SECTION_DISPLAY);
        o = o + GenerateSettingLine(1, 0, "Weather in Header", settingWeatherHeader, "Show weather");
        o = o + GenerateSettingLine(1, 1, "RSS Ticker", settingRssTicker, "Enable ticker");
        o = o + GenerateBootModeLine();
        return o;
    }

    private string GenerateSectionAccessibility()
    {
        string o = GenerateSectionHeader("ACCESSIBILITY", SECTION_ACCESSIBILITY);
        o = o + GenerateSettingLine(2, 0, "High Contrast", settingHighContrast, "More contrast");
        o = o + GenerateSettingLine(2, 1, "Large Cursor", settingLargeCursor, "Larger cursor");
        o = o + GenerateSettingLine(2, 2, "Slow Scroll", settingSlowScroll, "Reduced speed");
        return o;
    }

    private string GenerateSettingLine(int section, int itemIndex, string label, bool value, string description)
    {
        bool isSelected = (currentSection == section && cursorIndex == itemIndex);
        string cursor = isSelected ? ">" : " ";
        string color = isSelected ? COLOR_HIGHLIGHT : COLOR_PRIMARY;
        string valueStr = value ? "[ON ]" : "[OFF]";

        // Build without BoxRow to avoid truncation of color tags
        string row = "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> ";
        row = row + "<color=" + color + ">  " + cursor + " " + DT_Format.PadLeft(label, 18) + " " + valueStr + " " + description + "</color>";
        row = row + " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";

        return row + "\n";
    }

    private string GenerateBootModeLine()
    {
        bool isSelected = (currentSection == SECTION_DISPLAY && cursorIndex == 2);
        string cursor = isSelected ? ">" : " ";
        string color = isSelected ? COLOR_HIGHLIGHT : COLOR_PRIMARY;
        string valueStr = "[" + bootModes[settingBootMode] + "]";

        string row = "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> ";
        row = row + "<color=" + color + ">  " + cursor + " " + DT_Format.PadLeft("Boot Animation", 18) + " " + valueStr + " Boot style</color>";
        row = row + " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";

        return row + "\n";
    }

    // =================================================================
    // SYSTEM NOTIFICATION
    // =================================================================

    /// <summary>
    /// Notify other systems of settings changes
    /// </summary>
    private void NotifySystems()
    {
        // Notify NotificationEventHub
        if (Utilities.IsValid(notificationHub))
        {
            notificationHub.SetProgramVariable("settingSoundEnabled", settingSoundEffects);
            notificationHub.SetProgramVariable("settingAchievementDisplay", settingAchievementDisplay);
            // 0=HUD only, 1=TV only, 2=BOTH
            // NotificationEventHub should check: if display == 1, skip HUD popup
            // If display == 0 or 2, show HUD popup
            notificationHub.SendCustomEvent("OnSettingsChanged");
        }

        // Notify DT_Core for display settings
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("settingShowWeather", settingWeatherHeader);
            coreReference.SetProgramVariable("settingShowTicker", settingRssTicker);
            coreReference.SendCustomEvent("OnSettingsChanged");
        }

        // Notify Boot Sequence
        if (Utilities.IsValid(bootSequence))
        {
            bootSequence.SetProgramVariable("bootMode", settingBootMode);
            bootSequence.SendCustomEvent("OnSettingsChanged");
        }
    }

    // =================================================================
    // PUBLIC SETTINGS QUERY API
    // =================================================================

    /// <summary>
    /// Public methods for other scripts to check settings
    /// These read directly from PlayerData without needing a reference
    /// </summary>

    // For use by NotificationEventHub - returns 0=HUD, 1=TV, 2=BOTH
    public int GetAchievementDisplayMode()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return 0;  // Default: HUD
        return GetIntSetting(player, KEY_ACHIEVEMENT_DISPLAY, 0);
    }

    // Convenience methods for common checks
    public bool ShouldShowHUDPopups()
    {
        int mode = GetAchievementDisplayMode();
        return (mode == 0 || mode == 2);  // HUD or BOTH
    }

    public bool ShouldShowTVNotifications()
    {
        int mode = GetAchievementDisplayMode();
        return (mode == 1 || mode == 2);  // TV or BOTH
    }

    // For use by AchievementTracker
    public bool GetLoginNotificationsEnabled()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return true;
        return GetBoolSetting(player, KEY_LOGIN_NOTIFICATIONS, true);
    }

    // For use by DT_Core
    public bool GetWeatherHeaderEnabled()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return true;
        return GetBoolSetting(player, KEY_WEATHER_HEADER, true);
    }

    // For use by DT_Core
    public bool GetRssTickerEnabled()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return true;
        return GetBoolSetting(player, KEY_RSS_TICKER, true);
    }

    // For use by DT_BootSequence
    public int GetBootMode()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        if (!Utilities.IsValid(player)) return 0;
        return GetIntSetting(player, KEY_BOOT_MODE, 0);
    }

    // =================================================================
    // CORE COMMUNICATION
    // =================================================================

    /// <summary>
    /// Push current display content to DT_Core
    /// </summary>
    private void PushDisplayToCore(string content)
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", content);
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    // =================================================================
    // HELPERS
    // =================================================================

    /// <summary>
    /// Generate separator line with custom character (for subsection dividers)
    /// </summary>
    private string GenerateSeparator(int width, string character)
    {
        if (width <= 0) return "";

        string result = "";
        for (int i = 0; i < width; i++)
        {
            result = result + character;
        }
        return result;
    }
}
