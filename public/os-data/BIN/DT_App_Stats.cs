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
    // TAB SYSTEM STATE
    // =================================================================

    private const int TAB_STATS = 0;
    private const int TAB_VISITS = 1;
    private const int TAB_ACTIVITY = 2;
    private const int TAB_COUNT = 3;

    private string[] tabNames;
    private int currentTabIndex;

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

    // =================================================================
    // INITIALIZATION
    // =================================================================

    private void Start()
    {
        displayLines = new string[50];
        totalLines = 0;
        scrollOffset = 0;
        isOpen = false;
        currentTabIndex = 0;

        // Initialize tab names
        tabNames = new string[TAB_COUNT];
        tabNames[TAB_STATS] = "STATS";
        tabNames[TAB_VISITS] = "VISITS";
        tabNames[TAB_ACTIVITY] = "ACTIVITY";

        maxGamerScore = 420; // Total possible gamerscore: 205G visits + 115G time + 100G activity
    }

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        isOpen = true;
        scrollOffset = 0;
        currentTabIndex = 0; // Always start on STATS tab
        CachePlayerData();
        GenerateCurrentTabDisplay();
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
        if (inputKey == "UP")
        {
            // Scroll up within tab
            if (scrollOffset > 0)
            {
                scrollOffset--;
                PushDisplayToCore();
            }
        }
        else if (inputKey == "DOWN")
        {
            // Scroll down within tab (13 visible lines)
            int maxScroll = totalLines - 13;
            if (maxScroll < 0) maxScroll = 0;
            if (scrollOffset < maxScroll)
            {
                scrollOffset++;
                PushDisplayToCore();
            }
        }
        else if (inputKey == "LEFT")
        {
            // Previous tab or exit
            if (currentTabIndex == 0)
            {
                ReturnToShell();
            }
            else
            {
                currentTabIndex--;
                scrollOffset = 0;
                GenerateCurrentTabDisplay();
                PushDisplayToCore();
            }
        }
        else if (inputKey == "RIGHT")
        {
            // Next tab (wrap)
            currentTabIndex++;
            if (currentTabIndex >= TAB_COUNT)
            {
                currentTabIndex = 0;
            }
            scrollOffset = 0;
            GenerateCurrentTabDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "PREV_TAB")
        {
            // Q key - previous tab (wrap)
            currentTabIndex--;
            if (currentTabIndex < 0)
            {
                currentTabIndex = TAB_COUNT - 1;
            }
            scrollOffset = 0;
            GenerateCurrentTabDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "NEXT_TAB")
        {
            // E key - next tab (wrap)
            currentTabIndex++;
            if (currentTabIndex >= TAB_COUNT)
            {
                currentTabIndex = 0;
            }
            scrollOffset = 0;
            GenerateCurrentTabDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "ACCEPT" || inputKey == "BACK")
        {
            ReturnToShell();
        }

        inputKey = "";
    }

    private void ReturnToShell()
    {
        if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
        {
            coreReference.SetProgramVariable("nextProcess", shellApp);
            coreReference.SendCustomEvent("LoadNextProcess");
        }
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
        if (totalVisits >= 250) return "Mythic";
        if (totalVisits >= 100) return "Legend";
        if (totalVisits >= 75) return "Veteran";
        if (totalVisits >= 50) return "Loyal";
        if (totalVisits >= 25) return "Devoted";
        if (totalVisits >= 10) return "Dedicated";
        if (totalVisits >= 5) return "Regular";
        if (totalVisits >= 1) return "Newcomer";
        return "First Timer";
    }

    // =================================================================
    // DISPLAY GENERATION
    // =================================================================

    public string GetDisplayContent()
    {
        if (!isOpen || displayLines == null) return "";

        string output = "";

        // Add tab header
        output = output + BuildTabHeader() + "\n";
        output = output + DT_Format.GenerateSeparator80() + "\n";

        // Add content lines with scrolling
        // Terminal has ~17 content lines, minus 2 header lines, minus 2 footer lines = 13 visible
        int linesToShow = 13;
        if (linesToShow > totalLines) linesToShow = totalLines;

        for (int i = 0; i < linesToShow; i++)
        {
            int lineIndex = scrollOffset + i;
            if (lineIndex < totalLines && lineIndex >= 0)
            {
                output = output + displayLines[lineIndex] + "\n";
            }
        }

        // Pad to ensure consistent height
        for (int i = linesToShow; i < 13; i++)
        {
            output = output + "\n";
        }

        // Add footer with navigation hints
        output = output + DT_Format.GenerateSeparator80() + "\n";
        output = output + " [Q/E] Switch Tab  [W/S] Scroll  [A] Back to Menu";

        return output;
    }

    private string BuildTabHeader()
    {
        string header = " ";
        string gamerscoreText = "GAMERSCORE: " + currentGamerScore.ToString() + "G";

        for (int i = 0; i < TAB_COUNT; i++)
        {
            if (i == currentTabIndex)
            {
                header = header + "<[" + tabNames[i] + "]>";
            }
            else
            {
                header = header + "[" + tabNames[i] + "]";
            }

            if (i < TAB_COUNT - 1)
            {
                header = header + "  ";
            }
        }

        // Pad to align gamerscore to the right
        int headerLength = header.Length;
        int gamerscoreLength = gamerscoreText.Length;
        int totalWidth = 80;
        int paddingNeeded = totalWidth - headerLength - gamerscoreLength;

        if (paddingNeeded > 0)
        {
            for (int i = 0; i < paddingNeeded; i++)
            {
                header = header + " ";
            }
        }

        header = header + gamerscoreText;

        return header;
    }

    private void GenerateCurrentTabDisplay()
    {
        totalLines = 0;

        switch (currentTabIndex)
        {
            case TAB_STATS:
                GenerateStatsTab();
                break;
            case TAB_VISITS:
                GenerateVisitsTab();
                break;
            case TAB_ACTIVITY:
                GenerateActivityTab();
                break;
        }
    }

    // =================================================================
    // TAB 0: STATS (Landing Page)
    // =================================================================

    private void GenerateStatsTab()
    {
        AddLine("");
        AddLine(" PLAYER PROFILE");
        AddLine(" " + DT_Format.RepeatChar('═', 78));

        // Player profile row 1
        string playerLine = " Player:         " + PadRight(playerName, 24);
        playerLine = playerLine + "Rank: " + playerRank;
        AddLine(playerLine);

        // Player profile row 2
        string memberSince = GetMemberSinceDate();
        string memberLine = " Member Since:   " + PadRight(memberSince, 24);
        memberLine = memberLine + "Total Visits: " + totalVisits.ToString();
        AddLine(memberLine);

        AddLine("");
        AddLine(" SESSION");
        AddLine(" " + DT_Format.RepeatChar('═', 78));

        // Session info
        float currentSessionTime = 0f;
        float totalTime = 0f;
        float longestSession = 0f;
        float avgSession = 0f;

        if (Utilities.IsValid(achievementDataManager))
        {
            currentSessionTime = achievementDataManager.GetCurrentSessionTime(playerName);
            totalTime = achievementDataManager.GetPlayerTotalTime(playerName);
            longestSession = achievementDataManager.GetLongestSession(playerName);
            avgSession = achievementDataManager.GetAverageSessionLength(playerName);
        }

        string sessionLine1 = " Current:        " + PadRight(FormatTimeDisplay(currentSessionTime), 24);
        sessionLine1 = sessionLine1 + "Total Time: " + FormatTimeDisplay(totalTime);
        AddLine(sessionLine1);

        string sessionLine2 = " Longest:        " + PadRight(FormatTimeDisplay(longestSession), 24);
        sessionLine2 = sessionLine2 + "Avg Session: " + FormatTimeDisplay(avgSession);
        AddLine(sessionLine2);

        AddLine("");
        AddLine(" ACHIEVEMENT PROGRESS");
        AddLine(" " + DT_Format.RepeatChar('═', 78));

        // Calculate category progress
        int visitEarned = 0;
        int timeEarned = 0;
        int activityEarned = 0;

        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 8; i++)
            {
                if (achievementDataManager.HasPlayerEarnedAchievement(playerName, i))
                {
                    visitEarned++;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (achievementDataManager.HasPlayerEarnedTimeAchievement(playerName, i))
                {
                    timeEarned++;
                }
            }

            for (int i = 0; i < 6; i++)
            {
                if (achievementDataManager.HasPlayerEarnedActivityAchievement(playerName, i))
                {
                    activityEarned++;
                }
            }
        }

        // Visit Milestones progress
        string visitBar = GenerateProgressBar70(visitEarned, 8);
        int visitPercent = visitEarned * 100 / 8;
        AddLine(" Visit Milestones    " + visitBar + "  " + visitPercent.ToString() + "%");

        // Time Challenges progress
        string timeBar = GenerateProgressBar70(timeEarned, 5);
        int timePercent = timeEarned * 100 / 5;
        AddLine(" Time Challenges     " + timeBar + "  " + timePercent.ToString() + "%");

        // Activity Awards progress
        string activityBar = GenerateProgressBar70(activityEarned, 6);
        int activityPercent = activityEarned * 100 / 6;
        AddLine(" Activity Awards     " + activityBar + "  " + activityPercent.ToString() + "%");

        AddLine("");

        // Overall progress
        int totalEarned = visitEarned + timeEarned + activityEarned;
        int totalAchievements = 19;
        string overallBar = GenerateProgressBar70(totalEarned, totalAchievements);
        int overallPercent = totalEarned * 100 / totalAchievements;
        AddLine(" Overall Progress    " + overallBar + "  " + overallPercent.ToString() + "%");

        string earnedText = "Earned: " + totalEarned.ToString() + "/" + totalAchievements.ToString();
        earnedText = earnedText + " achievements (" + currentGamerScore.ToString() + "G / " + maxGamerScore.ToString() + "G)";
        AddLine("                     " + earnedText);
    }

    // =================================================================
    // TAB 1: VISITS (Visit & Time Achievements)
    // =================================================================

    private void GenerateVisitsTab()
    {
        AddLine("");

        // Visit Milestones Section
        int visitEarned = 0;
        int visitPoints = 0;

        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 8; i++)
            {
                if (achievementDataManager.HasPlayerEarnedAchievement(playerName, i))
                {
                    visitEarned++;
                    visitPoints = visitPoints + achievementDataManager.GetAchievementPoints(i);
                }
            }
        }

        string visitHeader = " VISIT MILESTONES";
        int visitHeaderPadding = 80 - visitHeader.Length - 10;
        for (int i = 0; i < visitHeaderPadding; i++)
        {
            visitHeader = visitHeader + " ";
        }
        visitHeader = visitHeader + "[" + visitEarned.ToString() + "/8] " + visitPoints.ToString() + "G";
        AddLine(visitHeader);

        AddLine(" " + DT_Format.RepeatChar('═', 78));

        // Visit progress bar
        string visitBar = GenerateProgressBar70(visitEarned, 8);
        int visitPercent = visitEarned * 100 / 8;
        AddLine(" " + visitBar + "  " + visitPercent.ToString() + "%");
        AddLine("");

        // List visit achievements
        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 8; i++)
            {
                bool earned = achievementDataManager.HasPlayerEarnedAchievement(playerName, i);
                string title = achievementDataManager.GetAchievementTitle(i);
                int points = achievementDataManager.GetAchievementPoints(i);
                string check = earned ? "☑" : "☐";

                string line = " " + check + " " + PadRight(title, 16) + PadLeft(points.ToString() + "G", 4);
                AddLine(line);
            }
        }

        AddLine("");

        // Time Challenges Section
        int timeEarned = 0;
        int timePoints = 0;

        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 5; i++)
            {
                if (achievementDataManager.HasPlayerEarnedTimeAchievement(playerName, i))
                {
                    timeEarned++;
                    timePoints = timePoints + achievementDataManager.GetTimeAchievementPoints(i);
                }
            }
        }

        string timeHeader = " TIME CHALLENGES";
        int timeHeaderPadding = 80 - timeHeader.Length - 10;
        for (int i = 0; i < timeHeaderPadding; i++)
        {
            timeHeader = timeHeader + " ";
        }
        timeHeader = timeHeader + "[" + timeEarned.ToString() + "/5] " + timePoints.ToString() + "G";
        AddLine(timeHeader);

        AddLine(" " + DT_Format.RepeatChar('═', 78));

        // Time progress bar
        string timeBar = GenerateProgressBar70(timeEarned, 5);
        int timePercent = timeEarned * 100 / 5;
        AddLine(" " + timeBar + "  " + timePercent.ToString() + "%");
        AddLine("");

        // List time achievements
        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 5; i++)
            {
                bool earned = achievementDataManager.HasPlayerEarnedTimeAchievement(playerName, i);
                string title = achievementDataManager.GetTimeAchievementTitle(i);
                int points = achievementDataManager.GetTimeAchievementPoints(i);
                string check = earned ? "☑" : "☐";

                string line = " " + check + " " + PadRight(title, 16) + PadLeft(points.ToString() + "G", 4);
                AddLine(line);
            }
        }
    }

    // =================================================================
    // TAB 2: ACTIVITY (Activity Achievements)
    // =================================================================

    private void GenerateActivityTab()
    {
        AddLine("");

        // Activity Achievements Section
        int activityEarned = 0;
        int activityPoints = 0;

        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 6; i++)
            {
                if (achievementDataManager.HasPlayerEarnedActivityAchievement(playerName, i))
                {
                    activityEarned++;
                    activityPoints = activityPoints + achievementDataManager.GetActivityAchievementPoints(i);
                }
            }
        }

        string activityHeader = " ACTIVITY ACHIEVEMENTS";
        int activityHeaderPadding = 80 - activityHeader.Length - 10;
        for (int i = 0; i < activityHeaderPadding; i++)
        {
            activityHeader = activityHeader + " ";
        }
        activityHeader = activityHeader + "[" + activityEarned.ToString() + "/6] " + activityPoints.ToString() + "G";
        AddLine(activityHeader);

        AddLine(" " + DT_Format.RepeatChar('═', 78));

        // Activity progress bar
        string activityBar = GenerateProgressBar70(activityEarned, 6);
        int activityPercent = activityEarned * 100 / 6;
        AddLine(" " + activityBar + "  " + activityPercent.ToString() + "%");
        AddLine("");

        // List activity achievements with descriptions
        string[] activityDescriptions = new string[6];
        activityDescriptions[0] = "Visit during rainy weather";
        activityDescriptions[1] = "Visit between 10 PM and 4 AM";
        activityDescriptions[2] = "Visit before 9 AM";
        activityDescriptions[3] = "Visit on Saturday and Sunday";
        activityDescriptions[4] = "Visit 3 days in a row";
        activityDescriptions[5] = "Be in world with 8+ players";

        if (Utilities.IsValid(achievementDataManager))
        {
            for (int i = 0; i < 6; i++)
            {
                bool earned = achievementDataManager.HasPlayerEarnedActivityAchievement(playerName, i);
                string title = achievementDataManager.GetActivityAchievementTitle(i);
                int points = achievementDataManager.GetActivityAchievementPoints(i);
                string check = earned ? "☑" : "☐";
                string status = earned ? "EARNED - Unlocked!" : activityDescriptions[i];

                string line = " " + check + " " + PadRight(title, 16) + PadLeft(points.ToString() + "G", 4);
                AddLine(line);
                AddLine("     Status: " + status);

                if (i < 5) AddLine("");
            }
        }
    }

    // =================================================================
    // HELPER METHODS
    // =================================================================

    private string GenerateProgressBar70(int current, int total)
    {
        if (total <= 0) return GenerateEmptyBar(70);

        int barWidth = 70;
        int filled = (int)((float)current / (float)total * (float)barWidth);
        if (filled > barWidth) filled = barWidth;
        int empty = barWidth - filled;

        string bar = "[";
        for (int i = 0; i < filled; i++)
        {
            bar = bar + "█";
        }
        for (int i = 0; i < empty; i++)
        {
            bar = bar + "░";
        }
        bar = bar + "]";

        return bar;
    }

    private string GenerateEmptyBar(int width)
    {
        string bar = "[";
        for (int i = 0; i < width; i++)
        {
            bar = bar + "░";
        }
        return bar + "]";
    }

    private string FormatTimeDisplay(float totalSeconds)
    {
        if (totalSeconds < 60f)
        {
            int seconds = (int)totalSeconds;
            return seconds.ToString() + "s";
        }
        else if (totalSeconds < 3600f)
        {
            int minutes = (int)(totalSeconds / 60f);
            return minutes.ToString() + "m";
        }
        else
        {
            int hours = (int)(totalSeconds / 3600f);
            int minutes = (int)((totalSeconds - (float)(hours * 3600)) / 60f);
            return hours.ToString() + "h " + minutes.ToString() + "m";
        }
    }

    private string GetMemberSinceDate()
    {
        if (!Utilities.IsValid(achievementDataManager)) return "Unknown";

        string dateStr = achievementDataManager.GetPlayerFirstVisitDate(playerName);
        if (dateStr == null || dateStr == "" || dateStr == "Unknown")
        {
            return "Today";
        }

        // Format: yyyy-MM-dd -> MM/dd/yy
        if (dateStr.Length >= 10)
        {
            string year = dateStr.Substring(2, 2);
            string month = dateStr.Substring(5, 2);
            string day = dateStr.Substring(8, 2);
            return month + "/" + day + "/" + year;
        }

        return "Unknown";
    }

    // =================================================================
    // STRING FORMATTING HELPERS
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
