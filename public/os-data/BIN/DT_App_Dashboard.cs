using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

/// <summary>
/// BASEMENT OS - DASHBOARD APPLICATION (v2.1)
///
/// ROLE: FIRST SCREEN / SYSTEM OVERVIEW
/// The default app that greets users when they sit at the terminal.
/// Two-column layout displaying system status, player stats, online users, and weather.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Dashboard.cs
///
/// INTEGRATION:
/// - Hub: Reads from AchievementDataManager for stats
/// - Hub: Reads from DT_WeatherModule for weather
/// - Spoke: Launches DT_Shell (menu) on ACCEPT
///
/// LIMITATIONS:
/// - Max 350 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Dashboard : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // MODULE REFERENCES
    // =================================================================

    [Header("--- Data Sources ---")]
    [Tooltip("Achievement manager for player stats")]
    [SerializeField] private UdonSharpBehaviour achievementDataManager;

    [Tooltip("Weather module for temperature and conditions")]
    [SerializeField] private UdonSharpBehaviour weatherModule;

    [Header("--- System References ---")]
    [Tooltip("Shell/Menu app to launch on ACCEPT")]
    [SerializeField] private UdonSharpBehaviour shellApp;

    [Tooltip("Core reference for app switching")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    // =================================================================
    // DISPLAY CONSTANTS
    // =================================================================

    private const int SCREEN_WIDTH = 80;
    private const int LEFT_COL_WIDTH = 38;
    private const int RIGHT_COL_WIDTH = 38;
    private const int COLUMN_GAP = 4;

    // =================================================================
    // CACHED DISPLAY DATA
    // =================================================================

    private string cachedContent = "";
    private string playerName = "";
    private int playerVisits = 0;
    private int playerScore = 0;
    private string playerRank = "Visitor";

    private string memoryStatus = "[OK]";
    private string weatherSensorStatus = "[OK]";
    private string mailServerStatus = "[!!]";

    private string weatherTemp = "74°F";
    private string weatherCondition = "Clear";
    private string weatherLocation = "Chicago, IL";

    private int onlineCount = 0;
    private string[] onlinePlayerNames = new string[0];

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        RefreshData();
        RebuildDisplay();
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        cachedContent = "";
    }

    public void OnInput()
    {
        Debug.Log("[DT_App_Dashboard] OnInput called with inputKey='" + inputKey + "'");
        
        // ACCEPT or LEFT both return to Shell menu
        if (inputKey == "ACCEPT" || inputKey == "LEFT")
        {
            Debug.Log("[DT_App_Dashboard] Launching Shell...");
            LaunchShell();
        }
        inputKey = "";
    }

    public string GetDisplayContent()
    {
        if (cachedContent == "") RebuildDisplay();
        return cachedContent;
    }

    // =================================================================
    // DATA GATHERING
    // =================================================================

    private void RefreshData()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (Utilities.IsValid(localPlayer))
        {
            playerName = localPlayer.displayName;
        }
        else
        {
            playerName = "Unknown";
        }

        // NOTE: Achievement and Weather module integration disabled
        // GetProgramVariable crashes Udon when variables don't exist
        // TODO: Re-enable when modules have matching variable names
        
        // Use default values for now
        playerVisits = 1;
        playerScore = 0;
        playerRank = CalculateRank(playerScore);
        
        weatherTemp = "N/A";
        weatherCondition = "Offline";
        weatherSensorStatus = "[--]";

        GatherOnlinePlayers();
        memoryStatus = "[OK]";
        mailServerStatus = "[!!]";
    }

    private void GatherOnlinePlayers()
    {
        onlineCount = VRCPlayerApi.GetPlayerCount();

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[onlineCount];
        VRCPlayerApi.GetPlayers(allPlayers);

        int displayCount = onlineCount < 4 ? onlineCount : 4;
        onlinePlayerNames = new string[displayCount];

        for (int i = 0; i < displayCount; i++)
        {
            if (Utilities.IsValid(allPlayers[i]))
            {
                string prefix = " * ";
                if (allPlayers[i].isMaster) prefix = " # ";
                if (allPlayers[i].isLocal) prefix = " > ";
                onlinePlayerNames[i] = prefix + allPlayers[i].displayName;
            }
            else
            {
                onlinePlayerNames[i] = " * Unknown";
            }
        }
    }

    private string CalculateRank(int score)
    {
        if (score >= 300) return "Legend";
        if (score >= 200) return "Veteran";
        if (score >= 100) return "Regular";
        if (score >= 50) return "Dweller";
        if (score >= 20) return "Visitor";
        return "Newcomer";
    }

    // =================================================================
    // DISPLAY RENDERING
    // =================================================================

    private void RebuildDisplay()
    {
        string[] leftLines = BuildLeftColumn();
        string[] rightLines = BuildRightColumn();

        string output = "";
        int maxLines = leftLines.Length > rightLines.Length ? leftLines.Length : rightLines.Length;

        for (int i = 0; i < maxLines; i++)
        {
            string leftText = i < leftLines.Length ? leftLines[i] : "";
            string rightText = i < rightLines.Length ? rightLines[i] : "";
            output = output + FormatTwoColumnLine(leftText, rightText) + "\n";
        }

        cachedContent = output;
    }

    /// <summary>
    /// Push display content to DT_Core for rendering
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", cachedContent);
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    private string[] BuildLeftColumn()
    {
        string[] lines = new string[14];
        int idx = 0;

        lines[idx++] = "SYSTEM STATUS";
        lines[idx++] = memoryStatus + " Memory Check";
        lines[idx++] = weatherSensorStatus + " Weather Sensors";
        lines[idx++] = mailServerStatus + " Mail Server (Offline)";
        lines[idx++] = "";
        lines[idx++] = "PLAYER STATS";
        lines[idx++] = "Rank:      " + playerRank;
        lines[idx++] = "Score:     " + playerScore.ToString() + "G / 420G";
        lines[idx++] = "Visits:    " + playerVisits.ToString();
        lines[idx++] = "";
        lines[idx++] = "[>] SYSTEM MENU (Open)";
        lines[idx++] = "    Press [ACCEPT] to continue";
        lines[idx++] = "";
        lines[idx++] = "";

        return lines;
    }

    private string[] BuildRightColumn()
    {
        string[] lines = new string[14];
        int idx = 0;

        lines[idx++] = "WHO IS ONLINE (" + onlineCount.ToString() + ")";

        for (int i = 0; i < onlinePlayerNames.Length && i < 4; i++)
        {
            lines[idx++] = onlinePlayerNames[i];
        }

        while (idx < 5) lines[idx++] = "";

        lines[idx++] = "";
        lines[idx++] = "WEATHER (" + weatherLocation + ")";
        lines[idx++] = weatherTemp + "  " + weatherCondition;
        lines[idx++] = "";

        while (idx < 14) lines[idx++] = "";

        return lines;
    }

    private string FormatTwoColumnLine(string leftText, string rightText)
    {
        // Truncate if too long
        if (leftText.Length > LEFT_COL_WIDTH)
            leftText = leftText.Substring(0, LEFT_COL_WIDTH);
        if (rightText.Length > RIGHT_COL_WIDTH)
            rightText = rightText.Substring(0, RIGHT_COL_WIDTH);

        // Pad left column to exact width
        string paddedLeft = PadRight(leftText, LEFT_COL_WIDTH);
        string gap = "    "; // 4 chars

        // Build line: 38 + 4 + 38 = 80
        return paddedLeft + gap + PadRight(rightText, RIGHT_COL_WIDTH);
    }

    private string PadRight(string text, int maxWidth)
    {
        if (text == null) text = "";
        if (text.Length >= maxWidth) return text.Substring(0, maxWidth);

        int paddingNeeded = maxWidth - text.Length;
        string result = text;
        for (int i = 0; i < paddingNeeded; i++)
        {
            result = result + " ";
        }
        return result;
    }

    // =================================================================
    // APP SWITCHING
    // =================================================================

    private void LaunchShell()
    {
        if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
        {
            coreReference.SetProgramVariable("nextProcess", shellApp);
            coreReference.SendCustomEvent("LoadNextProcess");
        }
    }

    // =================================================================
    // TDD / TESTING
    // =================================================================

#if UNITY_EDITOR
    [ContextMenu("TEST: Refresh Display")]
    public void Test_RefreshDisplay()
    {
        Debug.Log("[DASHBOARD-TEST] Refreshing display...");

        playerName = "TestUser";
        playerVisits = 42;
        playerScore = 150;
        playerRank = "Regular";
        weatherTemp = "72°F";
        weatherCondition = "Cloudy";

        onlineCount = 3;
        onlinePlayerNames = new string[] {
            " # pwnerer (Owner)",
            " > TestUser",
            " * BobbyTables"
        };

        RebuildDisplay();
        Debug.Log("[DASHBOARD-TEST] Display Content:\n" + cachedContent);
    }
#endif
}
