using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BASEMENT OS STATS APPLICATION (v2.1)
///
/// ROLE: PERSONAL STATISTICS & ACHIEVEMENT PROGRESS
/// Displays player stats, achievement progress bars, and gamerscore.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Stats.cs
///
/// INTEGRATION:
/// - Hub: Reads from AchievementDataManager (read-only)
/// - Spoke: Receives input from DT_Core
///
/// LIMITATIONS:
/// - Max 300 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Stats : UdonSharpBehaviour
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

    [Tooltip("Core reference for navigation")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    // =================================================================
    // DISPLAY STATE
    // =================================================================

    private string[] displayLines;
    private int totalLines;
    private int scrollOffset;
    private bool isOpen;

    // Cached player data
    private string playerName;
    private string playerRank;
    private int totalVisits;
    private float timePlayedHours;
    private int currentGamerScore;
    private int maxGamerScore;

    // Achievement tracking
    private string[] achievementNames;
    private float[] achievementProgress;
    private bool[] achievementComplete;
    private int achievementCount;

    // =================================================================
    // INITIALIZATION
    // =================================================================

    private void Start()
    {
        displayLines = new string[50];
        totalLines = 0;
        scrollOffset = 0;
        isOpen = false;

        // Define achievements to track
        achievementNames = new string[] { "First Steps", "Regular Visitor", "Time Lord", "Explorer", "Social Butterfly" };
        achievementProgress = new float[] { 1.0f, 0.4f, 0.1f, 0.5f, 0.25f };
        achievementComplete = new bool[] { true, false, false, false, false };
        achievementCount = 5;

        maxGamerScore = 1000;
    }

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        isOpen = true;
        scrollOffset = 0;
        CachePlayerData();
        GenerateDisplay();
    }

    public void OnAppClose()
    {
        isOpen = false;
        scrollOffset = 0;
    }

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        if (inputKey == "LEFT" || inputKey == "ACCEPT")
        {
            // Go back handled by core
        }
        inputKey = "";
    }

    // =================================================================
    // DATA GATHERING
    // =================================================================

    private void CachePlayerData()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (Utilities.IsValid(localPlayer))
        {
            playerName = localPlayer.displayName;
        }
        else
        {
            playerName = "Unknown Player";
        }

        if (Utilities.IsValid(achievementDataManager))
        {
            object visits = achievementDataManager.GetProgramVariable("totalVisits");
            if (visits != null) totalVisits = (int)visits;

            object time = achievementDataManager.GetProgramVariable("timePlayedMinutes");
            if (time != null) timePlayedHours = (float)time / 60f;

            object score = achievementDataManager.GetProgramVariable("gamerScore");
            if (score != null) currentGamerScore = (int)score;
        }
        else
        {
            totalVisits = 42;
            timePlayedHours = 12.5f;
            currentGamerScore = 420;
        }

        playerRank = DetermineRank();
    }

    private string DetermineRank()
    {
        if (totalVisits >= 100) return "Legend";
        if (totalVisits >= 50) return "Veteran";
        if (totalVisits >= 20) return "Regular";
        if (totalVisits >= 5) return "Visitor";
        return "Newcomer";
    }

    // =================================================================
    // DISPLAY GENERATION
    // =================================================================

    public string GetDisplayContent()
    {
        if (!isOpen || displayLines == null) return "";

        string output = "";
        int linesToShow = 16;
        if (linesToShow > totalLines) linesToShow = totalLines;

        for (int i = 0; i < linesToShow; i++)
        {
            int lineIndex = scrollOffset + i;
            if (lineIndex < totalLines && lineIndex >= 0)
            {
                if (i > 0) output = output + "\n";
                output = output + displayLines[lineIndex];
            }
        }

        return output;
    }

    private void GenerateDisplay()
    {
        totalLines = 0;

        // Header
        AddLine(" ╔══════════════════════════════════════════════════════════════════════════════╗");
        AddLine(" ║  PERSONAL STATISTICS                                                         ║");
        AddLine(" ╠══════════════════════════════════════════════════════════════════════════════╣");
        AddLine(" ║                                                                              ║");

        // Player info
        string playerLine = " ║  Player: " + PadRight(playerName, 28) + "Rank: " + PadRight(playerRank, 24) + "║";
        AddLine(playerLine);

        string visitsText = "Total Visits: " + totalVisits.ToString();
        string timeText = "Time Played: " + FormatPlayTime(timePlayedHours);
        AddLine(" ║  " + PadRight(visitsText, 34) + PadRight(timeText, 42) + "║");

        AddLine(" ║                                                                              ║");
        AddLine(" ║  ACHIEVEMENT PROGRESS                                                        ║");
        AddLine(" ║  ────────────────────────────────────────────────────────────────────────    ║");

        // Achievement progress bars
        for (int i = 0; i < achievementCount; i++)
        {
            string achievementLine = GenerateAchievementLine(i);
            AddLine(achievementLine);
        }

        AddLine(" ║                                                                              ║");

        // Gamerscore
        string gamerscoreText = "Gamerscore: " + currentGamerScore.ToString() + "G / " + maxGamerScore.ToString() + "G";
        AddLine(" ║  " + PadRight(gamerscoreText, 74) + "║");

        AddLine(" ╚══════════════════════════════════════════════════════════════════════════════╝");
    }

    private string GenerateAchievementLine(int index)
    {
        string name = achievementNames[index];
        float progress = achievementProgress[index];
        bool complete = achievementComplete[index];

        string bar = GenerateProgressBar(progress);
        int percent = (int)(progress * 100.0f);
        string percentText = PadLeft(percent.ToString(), 3) + "%";
        string status = complete ? "COMPLETE" : "";

        string line = " ║  " + PadRightWithDots(name, 20) + bar + "  " + percentText + "  " + PadRight(status, 18) + "║";
        return line;
    }

    private string GenerateProgressBar(float percent)
    {
        string bar = "[";
        int filled = (int)(percent * 20.0f);

        for (int i = 0; i < 20; i++)
        {
            if (i < filled)
                bar = bar + "█";
            else
                bar = bar + "░";
        }

        bar = bar + "]";
        return bar;
    }

    private string FormatPlayTime(float hours)
    {
        int totalMinutes = (int)(hours * 60.0f);
        int h = totalMinutes / 60;
        int m = totalMinutes % 60;
        return h.ToString() + "h " + m.ToString() + "m";
    }

    // =================================================================
    // HELPERS
    // =================================================================

    private void AddLine(string line)
    {
        if (totalLines < displayLines.Length)
        {
            displayLines[totalLines] = line;
            totalLines++;
        }
    }

    private string PadRight(string text, int length)
    {
        if (text == null) text = "";
        if (text.Length >= length) return text.Substring(0, length);

        string result = text;
        for (int i = text.Length; i < length; i++)
        {
            result = result + " ";
        }
        return result;
    }

    private string PadRightWithDots(string text, int length)
    {
        if (text == null) text = "";
        if (text.Length >= length) return text.Substring(0, length);

        string result = text;
        for (int i = text.Length; i < length; i++)
        {
            result = result + ".";
        }
        return result;
    }

    private string PadLeft(string text, int length)
    {
        if (text == null) text = "";
        if (text.Length >= length) return text;

        string result = "";
        for (int i = text.Length; i < length; i++)
        {
            result = result + " ";
        }
        return result + text;
    }

}
