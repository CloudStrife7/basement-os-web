using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using LowerLevel.Achievements;
using Varneon.VUdon.Logger.Abstract;

namespace LowerLevel.Terminal
{
    /// <summary>
    /// COMPONENT PURPOSE:
    /// Integrates live achievement data with the DOS terminal display system
    /// Generates formatted terminal pages showing player achievements, leaderboards, and stats
    /// Only displays achievement tiers that have been unlocked by the community
    /// 
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// Creates authentic DOS terminal experience showing real player progression data
    /// Updates automatically when achievements are earned, maintaining the retro computing feel
    /// 
    /// DEPENDENCIES & REQUIREMENTS:
    /// - AchievementDataManager script (provides live player data)
    /// - DOSTerminalController script (displays formatted pages)
    /// - Must be attached to the same GameObject as DOSTerminalController
    /// 
    /// ARCHITECTURE PATTERN:
    /// Reads live data from AchievementDataManager and formats it for DOS terminal display
    /// No data storage - pure presentation layer that queries live achievement data
    /// 
    /// CHANGELOG:
    /// - 07/10/25 - Initial implementation of achievement stats integration
    /// - 07/10/25 - Added community-unlocked tier system (only show unlocked achievements)
    /// - 07/10/25 - Added leaderboard generation with proper DOS terminal formatting
    /// - 07/10/25 - Fixed UdonSharp compatibility issues (removed struct initialization)
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DOSTerminalAchievementModule : UdonSharpBehaviour
    {
        [Header("Component References")]
        [Tooltip("Reference to AchievementDataManager for live player data")]
        [SerializeField] private AchievementDataManager dataManager;

        [Tooltip("Reference to DOSTerminalController for page display")]
        [SerializeField] private DOSTerminalController terminalController;

        [Header("Display Settings")]
        [Tooltip("Maximum players to show per leaderboard page")]
        [SerializeField] private int playersPerPage = 8;

        [Tooltip("How often to refresh achievement data (seconds)")]
        [SerializeField] private float updateIntervalSeconds = 30f;

        [Header("Debug Settings")]
        [Tooltip("Enable detailed logging for troubleshooting")]
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Production Debugging")]
        [Tooltip("Reference to UdonLogger component for in-world console (optional)")]
        [SerializeField] private UdonLogger productionLogger;

        // =================================================================
        // ACHIEVEMENT TIER CONFIGURATION
        // Must match AchievementDataManager milestones exactly
        // =================================================================

        private int[] achievementMilestones = { 1, 5, 10, 25, 50, 75, 100, 250 };
        private string[] achievementTitles = {
            "First Time Visitor",
            "Regular Visitor",
            "Shag Squad",
            "Basement Dweller",
            "Lower Level Legend",
            "Founding Members",
            "Century Club",
            "Legendary Status"
        };

        // =================================================================
        // INTERNAL STATE
        // =================================================================

        private bool isInitialized = false;
        private string cachedStatsPage = "";
        private string cachedLeaderboardPage = "";
        private float lastUpdateTime = 0f;

        // =================================================================
        // INITIALIZATION
        // =================================================================

        void Start()
        {
            InitializeModule();

            // Schedule periodic updates
            SendCustomEventDelayedSeconds(nameof(UpdateAchievementData), updateIntervalSeconds);
        }

        private void InitializeModule()
        {
            LogDebug("DOSTerminalAchievementModule initializing...");

            // Validate required components
            if (dataManager == null)
            {
                LogDebug("ERROR: AchievementDataManager not assigned!");
                return;
            }

            if (terminalController == null)
            {
                LogDebug("ERROR: DOSTerminalController not assigned!");
                return;
            }

            LogDebug("Achievement module initialized successfully");
            isInitialized = true;

            // Generate initial pages
            UpdateAchievementData();
        }

        // =================================================================
        // PUBLIC API - Called by NotificationEventHub when achievements change
        // =================================================================

        /// <summary>
        /// Called when achievements are earned to refresh terminal data
        /// This ensures the terminal shows up-to-date information
        /// </summary>
        public void RefreshAchievementData()
        {
            if (!isInitialized) return;

            LogDebug("[REFRESH] === REFRESHING ACHIEVEMENT DATA ===");

            // Clear cached pages to force regeneration with latest data
            cachedStatsPage = "";
            cachedLeaderboardPage = "";

            // Regenerate all achievement data
            UpdateAchievementData();

            // Force immediate update of any currently displayed pages
            if (terminalController != null)
            {
                LogDebug("[REFRESH] Forcing terminal display update");

                // Get current page and refresh it
                object currentPageObj = terminalController.GetProgramVariable("currentPage");
                if (currentPageObj != null)
                {
                    int currentPage = (int)currentPageObj;

                    switch (currentPage)
                    {
                        case 6: // Achievement overview
                            terminalController.SendCustomEvent("ShowAchievementOverview");
                            LogDebug("[REFRESH] Achievement overview page refreshed");
                            break;
                        case 7: // Achievement details  
                            terminalController.SendCustomEvent("ShowAchievementDetails");
                            LogDebug("[REFRESH] Achievement details page refreshed");
                            break;
                        case 1: // Personal stats
                            terminalController.SendCustomEvent("ShowMyStats");
                            LogDebug("[REFRESH] Personal stats page refreshed");
                            break;
                    }
                }
            }

            LogDebug("[REFRESH] Achievement data refresh complete");
        }

        /// <summary>
        /// Gets a formatted stats page for display on the terminal
        /// Returns cached data to avoid regenerating on every call
        /// </summary>
        /// <returns>Formatted DOS terminal page text</returns>
        public string GetStatsPage()
        {
            if (!isInitialized)
            {
                return GetErrorPage("Achievement system not initialized");
            }

            return !string.IsNullOrEmpty(cachedStatsPage)
                ? cachedStatsPage
                : GenerateStatsPage();
        }

        /// <summary>
        /// Gets a formatted leaderboard page for display on the terminal
        /// Shows top players by total achievement points
        /// </summary>
        /// <returns>Formatted DOS terminal leaderboard text</returns>
        public string GetLeaderboardPage()
        {
            if (!isInitialized)
            {
                return GetErrorPage("Achievement system not initialized");
            }

            return !string.IsNullOrEmpty(cachedLeaderboardPage)
                ? cachedLeaderboardPage
                : GenerateLeaderboardPage();
        }

        // =================================================================
        // TERMINAL PAGE COMMANDS - Can be called by DOSTerminalController
        // =================================================================

        /// <summary>
        /// Command to show stats page on terminal
        /// Can be called by terminal command system
        /// </summary>
        public void ShowStatsPage()
        {
            if (terminalController != null)
            {
                string statsContent = GetStatsPage();
                LogDebug("Stats page generated and ready for display");
                LogDebug($"Page content length: {statsContent.Length} characters");
            }
        }

        /// <summary>
        /// Command to show leaderboard page on terminal
        /// Can be called by terminal command system
        /// </summary>
        public void ShowLeaderboardPage()
        {
            if (terminalController != null)
            {
                string leaderboardContent = GetLeaderboardPage();
                LogDebug("Leaderboard page generated and ready for display");
                LogDebug($"Page content length: {leaderboardContent.Length} characters");
            }
        }

        // =================================================================
        // PAGE GENERATION LOGIC
        // =================================================================

        /// <summary>
        /// Updates cached achievement data and regenerates terminal pages
        /// Called periodically and when new achievements are earned
        /// </summary>
        public void UpdateAchievementData()
        {
            if (!isInitialized) return;

            LogDebug("Updating achievement data for terminal display");

            lastUpdateTime = Time.time;
            cachedStatsPage = GenerateStatsPage();
            cachedLeaderboardPage = GenerateLeaderboardPage();

            LogDebug($"Achievement data updated - Stats: {cachedStatsPage.Length} chars, Leaderboard: {cachedLeaderboardPage.Length} chars");

            // Schedule next update
            SendCustomEventDelayedSeconds(nameof(UpdateAchievementData), updateIntervalSeconds);
        }

        /// <summary>
        /// Generates the main stats page showing community achievement progress
        /// Only shows achievement tiers that have been unlocked by someone
        /// </summary>
        /// <returns>Formatted DOS terminal stats page</returns>
        private string GenerateStatsPage()
        {
            if (dataManager == null) return GetErrorPage("No achievement data available");

            string[] allPlayers = dataManager.GetAllPlayerNames();

            if (allPlayers.Length == 0)
            {
                return GetNoDataPage();
            }

            string pageContent = "";
            pageContent += "═════════════════════════════════════════════\n";
            pageContent += "                  LOWER LEVEL ACHIEVEMENT STATS\n";
            pageContent += "═════════════════════════════════════════════\n\n";

            pageContent += $"Total Players Tracked: {allPlayers.Length}\n";
            pageContent += $"Last Updated: {System.DateTime.Now.ToString("HH:mm:ss")}\n\n";

            // Show community progress for each unlocked tier
            for (int tier = 0; tier < achievementMilestones.Length; tier++)
            {
                int playersWithThisTier = CountPlayersWithAchievement(allPlayers, tier);

                if (playersWithThisTier > 0)
                {
                    pageContent += $"🏆 {achievementTitles[tier]}\n";
                    pageContent += $"   {playersWithThisTier} player(s) earned this achievement\n";
                    pageContent += $"   Requirement: {achievementMilestones[tier]} visit(s)\n";
                    pageContent += $"   Points: {dataManager.GetAchievementPoints(tier)}G\n\n";
                }
            }

            // Show total community stats
            int totalPoints = CalculateTotalCommunityPoints(allPlayers);
            int totalVisits = CalculateTotalCommunityVisits(allPlayers);

            pageContent += "═════════════════════════════════════════════\n";
            pageContent += "                  COMMUNITY TOTALS\n";
            pageContent += "═════════════════════════════════════════════\n";
            pageContent += $"Total Community Points: {totalPoints}G\n";
            pageContent += $"Total Community Visits: {totalVisits}\n";
            pageContent += "═════════════════════════════════════════════\n";
            pageContent += "Type 'leaderboard' to see top players\n";
            pageContent += "═════════════════════════════════════════════\n\n";
            pageContent += "C:\\BASEMENT> ";

            return pageContent;
        }

        /// <summary>
        /// Generates the leaderboard page showing top players by achievement points
        /// Sorted by total points descending - UdonSharp compatible version
        /// </summary>
        /// <returns>Formatted DOS terminal leaderboard page</returns>
        private string GenerateLeaderboardPage()
        {
            if (dataManager == null) return GetErrorPage("No achievement data available");

            string[] allPlayers = dataManager.GetAllPlayerNames();

            if (allPlayers.Length == 0)
            {
                return GetNoDataPage();
            }

            // UdonSharp compatible: Use separate arrays instead of structs
            string[] sortedPlayerNames = new string[allPlayers.Length];
            int[] sortedPlayerPoints = new int[allPlayers.Length];
            int[] sortedPlayerVisits = new int[allPlayers.Length];

            // Initialize arrays
            for (int i = 0; i < allPlayers.Length; i++)
            {
                sortedPlayerNames[i] = allPlayers[i];
                sortedPlayerPoints[i] = dataManager.GetPlayerTotalPoints(allPlayers[i]);
                sortedPlayerVisits[i] = dataManager.GetPlayerVisits(allPlayers[i]);
            }

            // Sort by points (bubble sort for UdonSharp compatibility)
            SortPlayersByPoints(sortedPlayerNames, sortedPlayerPoints, sortedPlayerVisits);

            // Generate leaderboard page
            string pageContent = "";
            pageContent += "═════════════════════════════════════════════\n";
            pageContent += "                  LOWER LEVEL LEADERBOARD\n";
            pageContent += "═════════════════════════════════════════════\n\n";

            pageContent += "Rank  Player Name           Points    Visits\n";
            pageContent += "----  ------------------    ------    ------\n";

            int displayCount = Mathf.Min(playersPerPage, allPlayers.Length);
            for (int i = 0; i < displayCount; i++)
            {
                string playerName = TruncatePlayerName(sortedPlayerNames[i], 18);
                string rank = (i + 1).ToString();
                if (rank.Length == 1) rank = " " + rank; // Pad single digits
                string points = sortedPlayerPoints[i].ToString() + "G";
                string visits = sortedPlayerVisits[i].ToString();

                pageContent += $" {rank}.  {playerName}    {points.PadLeft(6)}    {visits.PadLeft(6)}\n";
            }

            pageContent += "\n═════════════════════════════════════════════\n";
            pageContent += $"Showing top {displayCount} of {allPlayers.Length} players\n";
            pageContent += "═════════════════════════════════════════════\n\n";
            pageContent += "C:\\BASEMENT> ";

            return pageContent;
        }

        // =================================================================
        // HELPER METHODS
        // =================================================================

        /// <summary>
        /// Counts how many players have earned a specific achievement tier
        /// </summary>
        private int CountPlayersWithAchievement(string[] allPlayers, int achievementLevel)
        {
            int count = 0;
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (dataManager.HasPlayerEarnedAchievement(allPlayers[i], achievementLevel))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Calculates total achievement points across all players
        /// </summary>
        private int CalculateTotalCommunityPoints(string[] allPlayers)
        {
            int total = 0;
            for (int i = 0; i < allPlayers.Length; i++)
            {
                total += dataManager.GetPlayerTotalPoints(allPlayers[i]);
            }
            return total;
        }

        /// <summary>
        /// Calculates total visits across all players
        /// </summary>
        private int CalculateTotalCommunityVisits(string[] allPlayers)
        {
            int total = 0;
            for (int i = 0; i < allPlayers.Length; i++)
            {
                total += dataManager.GetPlayerVisits(allPlayers[i]);
            }
            return total;
        }

        /// <summary>
        /// Sorts player arrays by total points (descending)
        /// UdonSharp compatible bubble sort using separate arrays
        /// </summary>
        private void SortPlayersByPoints(string[] playerNames, int[] playerPoints, int[] playerVisits)
        {
            for (int i = 0; i < playerNames.Length - 1; i++)
            {
                for (int j = 0; j < playerNames.Length - i - 1; j++)
                {
                    if (playerPoints[j] < playerPoints[j + 1])
                    {
                        // Swap all three arrays
                        string tempName = playerNames[j];
                        playerNames[j] = playerNames[j + 1];
                        playerNames[j + 1] = tempName;

                        int tempPoints = playerPoints[j];
                        playerPoints[j] = playerPoints[j + 1];
                        playerPoints[j + 1] = tempPoints;

                        int tempVisits = playerVisits[j];
                        playerVisits[j] = playerVisits[j + 1];
                        playerVisits[j + 1] = tempVisits;
                    }
                }
            }
        }

        /// <summary>
        /// Truncates player names to fit terminal display width
        /// </summary>
        private string TruncatePlayerName(string playerName, int maxLength)
        {
            if (string.IsNullOrEmpty(playerName)) return "Unknown".PadRight(maxLength);

            if (playerName.Length <= maxLength)
            {
                return playerName.PadRight(maxLength);
            }
            else
            {
                return playerName.Substring(0, maxLength - 2) + "..";
            }
        }

        /// <summary>
        /// Generates error page when system is not working
        /// </summary>
        private string GetErrorPage(string errorMessage)
        {
            return $@"===============================================
         ACHIEVEMENT SYSTEM ERROR
===============================================

{errorMessage}

Please check component references in Inspector.

===============================================

C:\BASEMENT> ";
        }

        /// <summary>
        /// Generates page when no player data is available
        /// </summary>
        private string GetNoDataPage()
        {
            return @"===============================================
         LOWER LEVEL ACHIEVEMENT STATS
===============================================

No achievement data available yet.

Achievements will appear here as players
visit the Lower Level basement.

Be the first to earn an achievement!

===============================================

C:\BASEMENT> ";
        }

        /// <summary>
        /// Centralized debug logging system
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                // UdonSharp limitation: No string interpolation with complex expressions
                // Use string concatenation instead
                string coloredMessage = "<color=#FFB000>📊 [DOSTerminalAchievementModule] " + message + "</color>";
                
                // Unity console WITH color (rich text supported)
                Debug.Log(coloredMessage);
                
                // VUdon-Logger with SAME color - DIRECT METHOD CALL!
                if (productionLogger != null)
                {
                    productionLogger.Log(coloredMessage);
                }
            }
        }

        // =================================================================
        // TESTING METHODS
        // =================================================================

        /// <summary>
        /// Test method to generate sample achievement data display
        /// </summary>
        [ContextMenu("Test Stats Page Generation")]
        public void TestStatsPageGeneration()
        {
            if (!enableDebugLogging)
            {
                LogDebug("Testing only available in debug mode");
                return;
            }

            LogDebug("=== TESTING STATS PAGE GENERATION ===");

            string testPage = GetStatsPage();
            LogDebug($"Generated stats page ({testPage.Length} characters):");
            LogDebug(testPage);

            LogDebug("=== STATS PAGE TEST COMPLETE ===");
        }

        /// <summary>
        /// Test method to generate sample leaderboard display
        /// </summary>
        [ContextMenu("Test Leaderboard Page Generation")]
        public void TestLeaderboardPageGeneration()
        {
            if (!enableDebugLogging)
            {
                LogDebug("Testing only available in debug mode");
                return;
            }

            LogDebug("=== TESTING LEADERBOARD PAGE GENERATION ===");

            string testPage = GetLeaderboardPage();
            LogDebug($"Generated leaderboard page ({testPage.Length} characters):");
            LogDebug(testPage);

            LogDebug("=== LEADERBOARD PAGE TEST COMPLETE ===");
        }
    }
}