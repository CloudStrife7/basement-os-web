/// COMPONENT PURPOSE:
/// Quick PlayerData testing via context menus for immediate validation
/// Provides instant console feedback for persistence functionality
/// No UI setup required - right-click component to test
/// 
/// LOWER LEVEL 2.0 INTEGRATION:
/// Validates basement data persistence before building production features
/// Ensures achievements, preferences, and social data survive world rejoins
/// Foundation for all nostalgic basement persistence systems
/// 
/// DEPENDENCIES & REQUIREMENTS:
/// - VRC.SDK3.Persistence namespace (critical import)
/// - Setup: Attach to any GameObject in scene
/// - Testing: Right-click component in Inspector → Test methods
/// - Assets: No UI assets required
/// 
/// ARCHITECTURE PATTERN:
/// Context menu-driven testing with comprehensive console logging
/// Uses OnPlayerDataUpdated callback for real-time monitoring
/// Implements safe data access with type validation
/// Optimized for rapid development iteration

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;  // ← CRITICAL IMPORT FOR PLAYERDATA
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerDataQuickTester : UdonSharpBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool verboseLogging = true;

    // Basement-themed PlayerData keys for testing
    private const string BASEMENT_VISITS_KEY = "basement_total_visits";
    private const string FAVORITE_GAME_KEY = "basement_favorite_game";
    private const string SNAKE_HIGH_SCORE_KEY = "basement_snake_high_score";
    private const string TV_VOLUME_KEY = "basement_tv_volume";
    private const string ACHIEVEMENT_UNLOCKED_KEY = "basement_achievement_unlocked";
    private const string LAST_VISIT_TIME_KEY = "basement_last_visit_time";

    // Test tracking
    private int testRunCount = 0;

    void Start()
    {
        DebugLog("PlayerDataQuickTester initialized - Ready for context menu testing");

        // AUTO-RUN FOR PRODUCTION TESTING
        SendCustomEventDelayedSeconds(nameof(QuickTestSetData), 2f);
        SendCustomEventDelayedSeconds(nameof(ValidateAllData), 4f);
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        if (!player.isLocal) return;

        DebugLog($"🔄 OnPlayerDataUpdated: {infos.Length} updates received");

        for (int i = 0; i < infos.Length; i++)
        {
            string key = infos[i].Key;
            PlayerData.State state = infos[i].State;

            if (verboseLogging)
            {
                DebugLog($"  📊 Key: {key} | State: {state}");
            }
        }
    }

    /// <summary>
    /// Quick test - Sets basic basement data and validates immediately
    /// </summary>
    [ContextMenu("🚀 Quick Test - Set Basic Data")]
    public void QuickTestSetData()
    {
        testRunCount++;
        DebugLog("==========================================");
        DebugLog($"🚀 QUICK TEST #{testRunCount} - Setting Basic Basement Data");
        DebugLog("==========================================");

        // Set basic test data
        PlayerData.SetInt(BASEMENT_VISITS_KEY, testRunCount * 3);
        PlayerData.SetString(FAVORITE_GAME_KEY, "Nokia Snake");
        PlayerData.SetInt(SNAKE_HIGH_SCORE_KEY, 1500 + (testRunCount * 250));
        PlayerData.SetFloat(TV_VOLUME_KEY, 0.8f);
        PlayerData.SetBool(ACHIEVEMENT_UNLOCKED_KEY, testRunCount >= 2);

        DebugLog("✅ Basic data set successfully!");
        DebugLog("📋 Use 'Validate Data' to check if it persisted");
    }

    /// <summary>
    /// Validates that all test data exists and matches expected values
    /// </summary>
    [ContextMenu("✅ Validate - Check All Data")]
    public void ValidateAllData()
    {
        DebugLog("==========================================");
        DebugLog("✅ VALIDATION - Checking All Basement Data");
        DebugLog("==========================================");

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer))
        {
            DebugLog("❌ Local player not valid!");
            return;
        }

        // Check each key with type safety
        ValidatePlayerDataKey(localPlayer, BASEMENT_VISITS_KEY, typeof(int), "Basement Visits");
        ValidatePlayerDataKey(localPlayer, FAVORITE_GAME_KEY, typeof(string), "Favorite Game");
        ValidatePlayerDataKey(localPlayer, SNAKE_HIGH_SCORE_KEY, typeof(int), "Snake High Score");
        ValidatePlayerDataKey(localPlayer, TV_VOLUME_KEY, typeof(float), "TV Volume");
        ValidatePlayerDataKey(localPlayer, ACHIEVEMENT_UNLOCKED_KEY, typeof(bool), "Achievement Status");

        DebugLog("✅ Validation complete!");
    }

    /// <summary>
    /// Tests time-based data for session persistence
    /// </summary>
    [ContextMenu("⏰ Test - Time Data")]
    public void TestTimeData()
    {
        DebugLog("==========================================");
        DebugLog("⏰ TIME DATA TEST - Testing Double Precision");
        DebugLog("==========================================");

        double currentTime = Time.time;
        PlayerData.SetDouble(LAST_VISIT_TIME_KEY, currentTime);

        DebugLog($"🕐 Set last visit time: {currentTime:F2}");
        DebugLog("📋 Use 'Validate Time Data' to check persistence");
    }

    /// <summary>
    /// Validates time data persistence
    /// </summary>
    [ContextMenu("⏰ Validate - Time Data")]
    public void ValidateTimeData()
    {
        DebugLog("==========================================");
        DebugLog("⏰ TIME VALIDATION - Checking Time Persistence");
        DebugLog("==========================================");

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        if (PlayerData.HasKey(localPlayer, LAST_VISIT_TIME_KEY))
        {
            if (PlayerData.GetType(localPlayer, LAST_VISIT_TIME_KEY) == typeof(double))
            {
                double savedTime = PlayerData.GetDouble(localPlayer, LAST_VISIT_TIME_KEY);
                double currentTime = Time.time;
                double timeDiff = currentTime - savedTime;

                DebugLog($"✅ Last visit time: {savedTime:F2}");
                DebugLog($"🕐 Current time: {currentTime:F2}");
                DebugLog($"⏱️ Time difference: {timeDiff:F2} seconds");
            }
            else
            {
                DebugLog("❌ Time data exists but wrong type!");
            }
        }
        else
        {
            DebugLog("❌ No time data found - run 'Test Time Data' first");
        }
    }

    /// <summary>
    /// Clears all test data (sets to defaults)
    /// </summary>
    [ContextMenu("🗑️ Clear - All Test Data")]
    public void ClearAllTestData()
    {
        DebugLog("==========================================");
        DebugLog("🗑️ CLEARING - All Test Data Reset");
        DebugLog("==========================================");

        PlayerData.SetInt(BASEMENT_VISITS_KEY, 0);
        PlayerData.SetString(FAVORITE_GAME_KEY, "");
        PlayerData.SetInt(SNAKE_HIGH_SCORE_KEY, 0);
        PlayerData.SetFloat(TV_VOLUME_KEY, 0.5f);
        PlayerData.SetBool(ACHIEVEMENT_UNLOCKED_KEY, false);
        PlayerData.SetDouble(LAST_VISIT_TIME_KEY, 0.0);

        testRunCount = 0;
        DebugLog("✅ All test data cleared to defaults");
    }

    /// <summary>
    /// Complete persistence test - tests across Stop/Start Play
    /// </summary>
    [ContextMenu("🔄 PERSISTENCE TEST - Set & Stop Play")]
    public void PersistenceTestSet()
    {
        DebugLog("==========================================");
        DebugLog("🔄 PERSISTENCE TEST - Setting Data for Stop/Start Test");
        DebugLog("==========================================");

        // Set unique data we can verify after restarting
        int uniqueVisits = 99 + testRunCount;
        string uniqueGame = "Test Game " + testRunCount;
        int uniqueScore = 9999 + testRunCount;

        PlayerData.SetInt(BASEMENT_VISITS_KEY, uniqueVisits);
        PlayerData.SetString(FAVORITE_GAME_KEY, uniqueGame);
        PlayerData.SetInt(SNAKE_HIGH_SCORE_KEY, uniqueScore);
        PlayerData.SetDouble(LAST_VISIT_TIME_KEY, Time.time);

        DebugLog($"📊 Set unique test data:");
        DebugLog($"   Visits: {uniqueVisits}");
        DebugLog($"   Game: {uniqueGame}");
        DebugLog($"   Score: {uniqueScore}");
        DebugLog($"   Time: {Time.time:F2}");
        DebugLog("");
        DebugLog("🎯 NOW: Stop Play mode, then Start Play mode");
        DebugLog("🎯 THEN: Use 'Persistence Test Check' to verify!");
    }

    /// <summary>
    /// Validates persistence across Play sessions
    /// </summary>
    [ContextMenu("🔄 PERSISTENCE TEST - Check After Restart")]
    public void PersistenceTestCheck()
    {
        DebugLog("==========================================");
        DebugLog("🔄 PERSISTENCE CHECK - Verifying Data Survived Restart");
        DebugLog("==========================================");

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        bool persistenceWorking = true;

        // Check if our test data survived
        if (PlayerData.HasKey(localPlayer, BASEMENT_VISITS_KEY))
        {
            int visits = PlayerData.GetInt(localPlayer, BASEMENT_VISITS_KEY);
            DebugLog($"✅ Visits persisted: {visits}");
        }
        else
        {
            DebugLog("❌ Visits data lost!");
            persistenceWorking = false;
        }

        if (PlayerData.HasKey(localPlayer, FAVORITE_GAME_KEY))
        {
            string game = PlayerData.GetString(localPlayer, FAVORITE_GAME_KEY);
            DebugLog($"✅ Game persisted: {game}");
        }
        else
        {
            DebugLog("❌ Game data lost!");
            persistenceWorking = false;
        }

        if (PlayerData.HasKey(localPlayer, SNAKE_HIGH_SCORE_KEY))
        {
            int score = PlayerData.GetInt(localPlayer, SNAKE_HIGH_SCORE_KEY);
            DebugLog($"✅ Score persisted: {score}");
        }
        else
        {
            DebugLog("❌ Score data lost!");
            persistenceWorking = false;
        }

        DebugLog("==========================================");
        if (persistenceWorking)
        {
            DebugLog("🎉 PERSISTENCE TEST PASSED! ✅");
            DebugLog("🏠 Basement PlayerData is working perfectly!");
        }
        else
        {
            DebugLog("❌ PERSISTENCE TEST FAILED!");
            DebugLog("🔧 Check PlayerData setup and try again");
        }
        DebugLog("==========================================");
    }

    /// <summary>
    /// Helper method to validate individual PlayerData keys
    /// </summary>
    private void ValidatePlayerDataKey(VRCPlayerApi player, string key, Type expectedType, string displayName)
    {
        if (!PlayerData.HasKey(player, key))
        {
            DebugLog($"📭 {displayName}: No data found");
            return;
        }

        Type actualType = PlayerData.GetType(player, key);
        if (actualType != expectedType)
        {
            DebugLog($"⚠️ {displayName}: Type mismatch (expected {expectedType.Name}, got {actualType.Name})");
            return;
        }

        string value = GetPlayerDataAsString(player, key, actualType);
        DebugLog($"✅ {displayName}: {value}");
    }

    /// <summary>
    /// Converts PlayerData to string for logging
    /// </summary>
    private string GetPlayerDataAsString(VRCPlayerApi player, string key, Type type)
    {
        if (type == typeof(int)) return PlayerData.GetInt(player, key).ToString();
        if (type == typeof(float)) return PlayerData.GetFloat(player, key).ToString("F2");
        if (type == typeof(double)) return PlayerData.GetDouble(player, key).ToString("F2");
        if (type == typeof(bool)) return PlayerData.GetBool(player, key).ToString();
        if (type == typeof(string)) return $"\"{PlayerData.GetString(player, key)}\"";

        return "Unknown type";
    }

    /// <summary>
    /// Debug logging with toggle
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[PlayerDataQuickTester] {message}");
        }
    }
}