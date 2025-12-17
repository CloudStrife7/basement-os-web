using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using LowerLevel.Achievements;

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
    [SerializeField] private AchievementDataManager achievementDataManager;

    [Tooltip("Core reference for navigation")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Tooltip("Shell app to return to on LEFT key")]
    [SerializeField] private UdonSharpBehaviour shellApp;

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
        PushDisplayToCore();
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
        if (inputKey == "LEFT")
        {
            // Return to Shell menu
            if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
            {
                coreReference.SetProgramVariable("nextProcess", shellApp);
                coreReference.SendCustomEvent("LoadNextProcess");
            }
        }
        inputKey = "";
    }

    /// <summary>
    /// Push display content to DT_Core for rendering
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", GetDisplayContent());
            coreReference.SendCustomEvent("RefreshDisplay");
        }
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
            // Use typed reference and public getter methods
            totalVisits = achievementDataManager.GetPlayerVisits(playerName);
            
            // GetPlayerTotalTime returns seconds, convert to hours
            float totalTimeSeconds = achievementDataManager.GetPlayerTotalTime(playerName);
            timePlayedHours = totalTimeSeconds / 3600f;
            
            currentGamerScore = achievementDataManager.GetPlayerTotalPoints(playerName);
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
        AddLine(" PERSONAL STATISTICS");
        AddLine(DT_Format.GenerateSeparator80());

        // Player info
        AddLine(" Player: " + playerName + "     Rank: " + playerRank);
        AddLine(" Total Visits: " + totalVisits.ToString() + "     Time Played: " + FormatPlayTime(timePlayedHours));
        AddLine("");

        // Achievement progress
        AddLine(" ACHIEVEMENTS");
        for (int i = 0; i < achievementCount; i++)
        {
            string achievementLine = GenerateAchievementLine(i);
            AddLine(achievementLine);
        }

        AddLine("");
        AddLine(" Gamerscore: " + currentGamerScore.ToString() + "G / " + maxGamerScore.ToString() + "G");
        AddLine("");
        AddLine(" [A] Back");
    }

    private string GenerateAchievementLine(int index)
    {
        string name = achievementNames[index];
        float progress = achievementProgress[index];
        bool complete = achievementComplete[index];

        string bar = GenerateProgressBar(progress);
        int percent = (int)(progress * 100.0f);
        string percentText = PadLeft(percent.ToString(), 3) + "%";
        string status = complete ? "[DONE]" : "";

        string line = "   " + PadRight(name, 18) + bar + " " + percentText + " " + status;
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
