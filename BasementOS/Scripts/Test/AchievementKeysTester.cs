using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Persistence;
using LowerLevel.Achievements;

/// <summary>
/// PURPOSE-BUILT TEST SCRIPT FOR ACHIEVEMENT KEYS VALIDATION
///
/// Created: 2025-12-28 for Issue #67
///
/// This script ONLY uses AchievementKeys constants - NO hardcoded strings.
/// Each test explicitly logs which key constant is being used.
///
/// HOW TO USE:
/// 1. Attach to a GameObject in the scene
/// 2. Assign AchievementKeys reference in Inspector
/// 3. Enter Play Mode (ClientSim)
/// 4. Use Context Menu to run tests
///
/// TEST COVERAGE:
/// - 8 Visit Achievement Keys
/// - 5 Time Achievement Keys
/// - 6 Activity Achievement Keys
/// - 6 Tracking Keys (visits, time, streaks)
/// = 25 total keys validated
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class AchievementKeysTester : UdonSharpBehaviour
{
    // =================================================================
    // INSPECTOR REFERENCES
    // =================================================================

    [Header("Required References")]
    [Tooltip("Reference to AchievementKeys component - REQUIRED")]
    [SerializeField] private AchievementKeys achievementKeys;

    [Header("Test Settings")]
    [Tooltip("Enable verbose logging for each key tested")]
    [SerializeField] private bool verboseLogging = true;

    // =================================================================
    // TEST STATE TRACKING
    // =================================================================

    private int testsPassed = 0;
    private int testsFailed = 0;
    private VRCPlayerApi localPlayer;

    // =================================================================
    // INITIALIZATION
    // =================================================================

    void Start()
    {
        localPlayer = Networking.LocalPlayer;

        if (achievementKeys == null)
        {
            LogError("AchievementKeys reference not assigned! Assign in Inspector.");
        }
        else
        {
            LogInfo("AchievementKeysTester initialized. Use ContextMenu to run tests.");
        }
    }

    // =================================================================
    // MAIN TEST ENTRY POINTS
    // =================================================================

    [ContextMenu("Run All Key Tests")]
    public void RunAllKeyTests()
    {
        LogHeader("ACHIEVEMENT KEYS VALIDATION TEST SUITE");
        LogInfo("Testing ALL keys from AchievementKeys.cs");
        LogInfo("Only centralized constants are used - NO hardcoded strings");

        testsPassed = 0;
        testsFailed = 0;

        if (!ValidateReferences()) return;

        // Run all test categories
        TestTrackingKeys();
        TestVisitAchievementKeys();
        TestTimeAchievementKeys();
        TestActivityAchievementKeys();
        TestArrayAccess();

        // Print summary
        PrintTestSummary();
    }

    [ContextMenu("Test: Tracking Keys Only")]
    public void TestTrackingKeys()
    {
        LogHeader("TESTING TRACKING KEYS");

        if (!ValidateReferences()) return;

        // Test each tracking key constant
        TestKeyExists("VISITS_KEY", AchievementKeys.VISITS_KEY);
        TestKeyExists("FIRST_VISIT_KEY", AchievementKeys.FIRST_VISIT_KEY);
        TestKeyExists("LAST_VISIT_DATE_KEY", AchievementKeys.LAST_VISIT_DATE_KEY);
        TestKeyExists("TOTAL_TIME_KEY", AchievementKeys.TOTAL_TIME_KEY);
        TestKeyExists("SESSION_START_KEY", AchievementKeys.SESSION_START_KEY);
        TestKeyExists("LONGEST_SESSION_KEY", AchievementKeys.LONGEST_SESSION_KEY);
        TestKeyExists("SESSION_COUNT_KEY", AchievementKeys.SESSION_COUNT_KEY);
        TestKeyExists("LAST_SESSION_LENGTH_KEY", AchievementKeys.LAST_SESSION_LENGTH_KEY);
        TestKeyExists("LAST_VISIT_HOUR_KEY", AchievementKeys.LAST_VISIT_HOUR_KEY);
        TestKeyExists("VISIT_STREAK_COUNT_KEY", AchievementKeys.VISIT_STREAK_COUNT_KEY);
        TestKeyExists("LAST_STREAK_DATE_KEY", AchievementKeys.LAST_STREAK_DATE_KEY);
        TestKeyExists("MOST_RECENT_ACHIEVEMENT", AchievementKeys.MOST_RECENT_ACHIEVEMENT);
    }

    [ContextMenu("Test: Visit Achievement Keys Only")]
    public void TestVisitAchievementKeys()
    {
        LogHeader("TESTING VISIT ACHIEVEMENT KEYS (8 total)");

        if (!ValidateReferences()) return;

        // Test each visit achievement key constant
        TestKeyExists("FIRST_VISIT_ACHIEVEMENT", AchievementKeys.FIRST_VISIT_ACHIEVEMENT);
        TestKeyExists("REGULAR_VISITOR_ACHIEVEMENT", AchievementKeys.REGULAR_VISITOR_ACHIEVEMENT);
        TestKeyExists("SHAG_SQUAD_ACHIEVEMENT", AchievementKeys.SHAG_SQUAD_ACHIEVEMENT);
        TestKeyExists("BASEMENT_DWELLER_ACHIEVEMENT", AchievementKeys.BASEMENT_DWELLER_ACHIEVEMENT);
        TestKeyExists("RETRO_REGULAR_ACHIEVEMENT", AchievementKeys.RETRO_REGULAR_ACHIEVEMENT);
        TestKeyExists("HOTTUB_HERO_ACHIEVEMENT", AchievementKeys.HOTTUB_HERO_ACHIEVEMENT);
        TestKeyExists("CENTURY_CLUB_ACHIEVEMENT", AchievementKeys.CENTURY_CLUB_ACHIEVEMENT);
        TestKeyExists("LOWER_LEGEND_ACHIEVEMENT", AchievementKeys.LOWER_LEGEND_ACHIEVEMENT);
    }

    [ContextMenu("Test: Time Achievement Keys Only")]
    public void TestTimeAchievementKeys()
    {
        LogHeader("TESTING TIME ACHIEVEMENT KEYS (5 total)");

        if (!ValidateReferences()) return;

        // Test each time achievement key constant
        TestKeyExists("QUICK_VISIT_EARNED", AchievementKeys.QUICK_VISIT_EARNED);
        TestKeyExists("HANGOUT_EARNED", AchievementKeys.HANGOUT_EARNED);
        TestKeyExists("PARTY_TIME_EARNED", AchievementKeys.PARTY_TIME_EARNED);
        TestKeyExists("TWO_LEGIT_TWO_QUIT_EARNED", AchievementKeys.TWO_LEGIT_TWO_QUIT_EARNED);
        TestKeyExists("MARATHON_EARNED", AchievementKeys.MARATHON_EARNED);
    }

    [ContextMenu("Test: Activity Achievement Keys Only")]
    public void TestActivityAchievementKeys()
    {
        LogHeader("TESTING ACTIVITY ACHIEVEMENT KEYS (6 total)");

        if (!ValidateReferences()) return;

        // Test each activity achievement key constant
        TestKeyExists("HEAVY_RAIN_EARNED_KEY", AchievementKeys.HEAVY_RAIN_EARNED_KEY);
        TestKeyExists("NIGHT_OWL_EARNED_KEY", AchievementKeys.NIGHT_OWL_EARNED_KEY);
        TestKeyExists("EARLY_BIRD_EARNED_KEY", AchievementKeys.EARLY_BIRD_EARNED_KEY);
        TestKeyExists("WEEKEND_WARRIOR_EARNED_KEY", AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY);
        TestKeyExists("STREAK_MASTER_EARNED_KEY", AchievementKeys.STREAK_MASTER_EARNED_KEY);
        TestKeyExists("PARTY_ANIMAL_EARNED_KEY", AchievementKeys.PARTY_ANIMAL_EARNED_KEY);
    }

    [ContextMenu("Test: Array Access")]
    public void TestArrayAccess()
    {
        LogHeader("TESTING ARRAY ACCESS");

        if (!ValidateReferences()) return;

        // Test VISIT_ACHIEVEMENT_KEYS array
        LogInfo("Testing VISIT_ACHIEVEMENT_KEYS array...");
        if (achievementKeys.VISIT_ACHIEVEMENT_KEYS == null)
        {
            LogFail("VISIT_ACHIEVEMENT_KEYS", "Array is null");
        }
        else
        {
            int visitCount = achievementKeys.VISIT_ACHIEVEMENT_KEYS.Length;
            if (visitCount == 8)
            {
                LogPass("VISIT_ACHIEVEMENT_KEYS", "Array has " + visitCount.ToString() + "/8 elements");

                // Log each element
                for (int i = 0; i < visitCount; i++)
                {
                    string key = achievementKeys.VISIT_ACHIEVEMENT_KEYS[i];
                    LogInfo("  [" + i.ToString() + "] = \"" + key + "\"");
                }
            }
            else
            {
                LogFail("VISIT_ACHIEVEMENT_KEYS", "Array has " + visitCount.ToString() + " elements, expected 8");
            }
        }

        // Test TIME_ACHIEVEMENT_KEYS array
        LogInfo("Testing TIME_ACHIEVEMENT_KEYS array...");
        if (achievementKeys.TIME_ACHIEVEMENT_KEYS == null)
        {
            LogFail("TIME_ACHIEVEMENT_KEYS", "Array is null");
        }
        else
        {
            int timeCount = achievementKeys.TIME_ACHIEVEMENT_KEYS.Length;
            if (timeCount == 5)
            {
                LogPass("TIME_ACHIEVEMENT_KEYS", "Array has " + timeCount.ToString() + "/5 elements");

                // Log each element
                for (int i = 0; i < timeCount; i++)
                {
                    string key = achievementKeys.TIME_ACHIEVEMENT_KEYS[i];
                    LogInfo("  [" + i.ToString() + "] = \"" + key + "\"");
                }
            }
            else
            {
                LogFail("TIME_ACHIEVEMENT_KEYS", "Array has " + timeCount.ToString() + " elements, expected 5");
            }
        }
    }

    // =================================================================
    // PLAYERDATA INTEGRATION TESTS
    // =================================================================

    [ContextMenu("Test: PlayerData Read/Write Cycle")]
    public void TestPlayerDataCycle()
    {
        LogHeader("TESTING PLAYERDATA READ/WRITE WITH ACHIEVEMENT KEYS");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        LogInfo("Testing PlayerData operations with centralized keys...");

        // Test integer key (visits)
        TestPlayerDataInt("VISITS_KEY", AchievementKeys.VISITS_KEY);

        // Test float key (time)
        TestPlayerDataFloat("TOTAL_TIME_KEY", AchievementKeys.TOTAL_TIME_KEY);

        // Test bool key (achievement)
        TestPlayerDataBool("FIRST_VISIT_ACHIEVEMENT", AchievementKeys.FIRST_VISIT_ACHIEVEMENT);

        // Test string key
        TestPlayerDataString("FIRST_VISIT_KEY", AchievementKeys.FIRST_VISIT_KEY);

        LogInfo("PlayerData cycle tests complete");
    }

    [ContextMenu("Test: Trigger First Visit Achievement")]
    public void TestTriggerFirstVisit()
    {
        LogHeader("TRIGGER TEST: FIRST_VISIT_ACHIEVEMENT");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        string keyName = "FIRST_VISIT_ACHIEVEMENT";
        string keyValue = AchievementKeys.FIRST_VISIT_ACHIEVEMENT;

        LogInfo("Using key constant: AchievementKeys." + keyName);
        LogInfo("Key value: \"" + keyValue + "\"");

        // Check current state
        bool hasKey = PlayerData.HasKey(localPlayer, keyValue);
        LogInfo("PlayerData.HasKey: " + hasKey.ToString());

        if (hasKey)
        {
            bool currentValue = PlayerData.GetBool(localPlayer, keyValue);
            LogInfo("Current value: " + currentValue.ToString());
        }

        // Set the achievement
        LogInfo("Setting PlayerData.SetBool(" + keyValue + ", true)...");
        PlayerData.SetBool(keyValue, true);

        // Verify it was set
        bool newValue = PlayerData.GetBool(localPlayer, keyValue);
        if (newValue)
        {
            LogPass(keyName, "Achievement triggered successfully via centralized key");
        }
        else
        {
            LogFail(keyName, "Failed to set achievement");
        }
    }

    [ContextMenu("Test: Trigger All Visit Achievements")]
    public void TestTriggerAllVisitAchievements()
    {
        LogHeader("TRIGGER TEST: ALL VISIT ACHIEVEMENTS");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        LogInfo("Triggering all 8 visit achievements using centralized keys...");

        // Use the array from AchievementKeys
        for (int i = 0; i < achievementKeys.VISIT_ACHIEVEMENT_KEYS.Length; i++)
        {
            string keyValue = achievementKeys.VISIT_ACHIEVEMENT_KEYS[i];
            LogInfo("[" + i.ToString() + "] Setting: \"" + keyValue + "\"");
            PlayerData.SetBool(keyValue, true);

            // Verify
            bool wasSet = PlayerData.GetBool(localPlayer, keyValue);
            if (wasSet)
            {
                LogPass("VISIT[" + i.ToString() + "]", keyValue);
            }
            else
            {
                LogFail("VISIT[" + i.ToString() + "]", "Failed to set " + keyValue);
            }
        }
    }

    [ContextMenu("Test: Trigger All Time Achievements")]
    public void TestTriggerAllTimeAchievements()
    {
        LogHeader("TRIGGER TEST: ALL TIME ACHIEVEMENTS");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        LogInfo("Triggering all 5 time achievements using centralized keys...");

        // Use the array from AchievementKeys
        for (int i = 0; i < achievementKeys.TIME_ACHIEVEMENT_KEYS.Length; i++)
        {
            string keyValue = achievementKeys.TIME_ACHIEVEMENT_KEYS[i];
            LogInfo("[" + i.ToString() + "] Setting: \"" + keyValue + "\"");
            PlayerData.SetBool(keyValue, true);

            // Verify
            bool wasSet = PlayerData.GetBool(localPlayer, keyValue);
            if (wasSet)
            {
                LogPass("TIME[" + i.ToString() + "]", keyValue);
            }
            else
            {
                LogFail("TIME[" + i.ToString() + "]", "Failed to set " + keyValue);
            }
        }
    }

    [ContextMenu("Test: Trigger All Activity Achievements")]
    public void TestTriggerAllActivityAchievements()
    {
        LogHeader("TRIGGER TEST: ALL ACTIVITY ACHIEVEMENTS");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        LogInfo("Triggering all 6 activity achievements using centralized keys...");

        // Activity keys (no array, use constants directly)
        string[] activityKeys = new string[6];
        activityKeys[0] = AchievementKeys.HEAVY_RAIN_EARNED_KEY;
        activityKeys[1] = AchievementKeys.NIGHT_OWL_EARNED_KEY;
        activityKeys[2] = AchievementKeys.EARLY_BIRD_EARNED_KEY;
        activityKeys[3] = AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY;
        activityKeys[4] = AchievementKeys.STREAK_MASTER_EARNED_KEY;
        activityKeys[5] = AchievementKeys.PARTY_ANIMAL_EARNED_KEY;

        string[] activityNames = new string[6];
        activityNames[0] = "HEAVY_RAIN_EARNED_KEY";
        activityNames[1] = "NIGHT_OWL_EARNED_KEY";
        activityNames[2] = "EARLY_BIRD_EARNED_KEY";
        activityNames[3] = "WEEKEND_WARRIOR_EARNED_KEY";
        activityNames[4] = "STREAK_MASTER_EARNED_KEY";
        activityNames[5] = "PARTY_ANIMAL_EARNED_KEY";

        for (int i = 0; i < 6; i++)
        {
            string keyValue = activityKeys[i];
            string keyName = activityNames[i];
            LogInfo("[" + i.ToString() + "] Setting: " + keyName + " = \"" + keyValue + "\"");
            PlayerData.SetBool(keyValue, true);

            // Verify
            bool wasSet = PlayerData.GetBool(localPlayer, keyValue);
            if (wasSet)
            {
                LogPass("ACTIVITY[" + i.ToString() + "]", keyName);
            }
            else
            {
                LogFail("ACTIVITY[" + i.ToString() + "]", "Failed to set " + keyName);
            }
        }
    }

    [ContextMenu("Test: Trigger ALL 19 Achievements")]
    public void TestTriggerAll19Achievements()
    {
        LogHeader("COMPREHENSIVE TEST: ALL 19 ACHIEVEMENTS");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        testsPassed = 0;
        testsFailed = 0;

        LogInfo("This test uses ONLY AchievementKeys constants");
        LogInfo("No hardcoded strings are used anywhere");

        TestTriggerAllVisitAchievements();
        TestTriggerAllTimeAchievements();
        TestTriggerAllActivityAchievements();

        PrintTestSummary();
    }

    [ContextMenu("Clear All Achievement Data")]
    public void ClearAllAchievementData()
    {
        LogHeader("CLEARING ALL ACHIEVEMENT DATA");

        if (!ValidateReferences()) return;
        if (!ValidateLocalPlayer()) return;

        LogInfo("Resetting all achievements using centralized keys...");

        // Clear visit achievements
        for (int i = 0; i < achievementKeys.VISIT_ACHIEVEMENT_KEYS.Length; i++)
        {
            PlayerData.SetBool(achievementKeys.VISIT_ACHIEVEMENT_KEYS[i], false);
        }
        LogInfo("Cleared 8 visit achievements");

        // Clear time achievements
        for (int i = 0; i < achievementKeys.TIME_ACHIEVEMENT_KEYS.Length; i++)
        {
            PlayerData.SetBool(achievementKeys.TIME_ACHIEVEMENT_KEYS[i], false);
        }
        LogInfo("Cleared 5 time achievements");

        // Clear activity achievements
        PlayerData.SetBool(AchievementKeys.HEAVY_RAIN_EARNED_KEY, false);
        PlayerData.SetBool(AchievementKeys.NIGHT_OWL_EARNED_KEY, false);
        PlayerData.SetBool(AchievementKeys.EARLY_BIRD_EARNED_KEY, false);
        PlayerData.SetBool(AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY, false);
        PlayerData.SetBool(AchievementKeys.STREAK_MASTER_EARNED_KEY, false);
        PlayerData.SetBool(AchievementKeys.PARTY_ANIMAL_EARNED_KEY, false);
        LogInfo("Cleared 6 activity achievements");

        // Clear tracking data
        PlayerData.SetInt(AchievementKeys.VISITS_KEY, 0);
        PlayerData.SetFloat(AchievementKeys.TOTAL_TIME_KEY, 0f);
        PlayerData.SetInt(AchievementKeys.VISIT_STREAK_COUNT_KEY, 0);
        LogInfo("Cleared tracking data");

        LogInfo("All achievement data cleared");
    }

    // =================================================================
    // HELPER METHODS
    // =================================================================

    private bool ValidateReferences()
    {
        if (achievementKeys == null)
        {
            LogError("AchievementKeys reference is null! Assign in Inspector.");
            return false;
        }
        return true;
    }

    private bool ValidateLocalPlayer()
    {
        localPlayer = Networking.LocalPlayer;
        if (localPlayer == null)
        {
            LogError("LocalPlayer is null - must run in Play Mode with ClientSim");
            return false;
        }
        return true;
    }

    private void TestKeyExists(string constantName, string keyValue)
    {
        if (keyValue == null || keyValue == "")
        {
            LogFail(constantName, "Key value is null or empty");
            return;
        }

        if (!keyValue.StartsWith("basement_"))
        {
            LogFail(constantName, "Key doesn't follow basement_* convention: " + keyValue);
            return;
        }

        LogPass(constantName, keyValue);
    }

    private void TestPlayerDataInt(string constantName, string keyValue)
    {
        LogInfo("Testing INT key: " + constantName + " = \"" + keyValue + "\"");

        int testValue = 42;
        PlayerData.SetInt(keyValue, testValue);

        if (PlayerData.HasKey(localPlayer, keyValue))
        {
            int readValue = PlayerData.GetInt(localPlayer, keyValue);
            if (readValue == testValue)
            {
                LogPass(constantName, "INT read/write OK");
            }
            else
            {
                LogFail(constantName, "INT mismatch: wrote " + testValue.ToString() + ", read " + readValue.ToString());
            }
        }
        else
        {
            LogFail(constantName, "Key not found after SetInt");
        }
    }

    private void TestPlayerDataFloat(string constantName, string keyValue)
    {
        LogInfo("Testing FLOAT key: " + constantName + " = \"" + keyValue + "\"");

        float testValue = 123.456f;
        PlayerData.SetFloat(keyValue, testValue);

        if (PlayerData.HasKey(localPlayer, keyValue))
        {
            float readValue = PlayerData.GetFloat(localPlayer, keyValue);
            float diff = readValue - testValue;
            if (diff < 0) diff = -diff;

            if (diff < 0.001f)
            {
                LogPass(constantName, "FLOAT read/write OK");
            }
            else
            {
                LogFail(constantName, "FLOAT mismatch: wrote " + testValue.ToString() + ", read " + readValue.ToString());
            }
        }
        else
        {
            LogFail(constantName, "Key not found after SetFloat");
        }
    }

    private void TestPlayerDataBool(string constantName, string keyValue)
    {
        LogInfo("Testing BOOL key: " + constantName + " = \"" + keyValue + "\"");

        PlayerData.SetBool(keyValue, true);

        if (PlayerData.HasKey(localPlayer, keyValue))
        {
            bool readValue = PlayerData.GetBool(localPlayer, keyValue);
            if (readValue)
            {
                LogPass(constantName, "BOOL read/write OK");
            }
            else
            {
                LogFail(constantName, "BOOL mismatch: wrote true, read false");
            }
        }
        else
        {
            LogFail(constantName, "Key not found after SetBool");
        }
    }

    private void TestPlayerDataString(string constantName, string keyValue)
    {
        LogInfo("Testing STRING key: " + constantName + " = \"" + keyValue + "\"");

        string testValue = "test_value_2025";
        PlayerData.SetString(keyValue, testValue);

        if (PlayerData.HasKey(localPlayer, keyValue))
        {
            string readValue = PlayerData.GetString(localPlayer, keyValue);
            if (readValue == testValue)
            {
                LogPass(constantName, "STRING read/write OK");
            }
            else
            {
                LogFail(constantName, "STRING mismatch: wrote " + testValue + ", read " + readValue);
            }
        }
        else
        {
            LogFail(constantName, "Key not found after SetString");
        }
    }

    // =================================================================
    // LOGGING HELPERS
    // =================================================================

    private void LogHeader(string title)
    {
        Debug.Log("<color=#00FFFF>========================================</color>");
        Debug.Log("<color=#00FFFF>  " + title + "</color>");
        Debug.Log("<color=#00FFFF>========================================</color>");
    }

    private void LogInfo(string message)
    {
        if (verboseLogging)
        {
            Debug.Log("<color=#FFFFFF>[KeysTester] " + message + "</color>");
        }
    }

    private void LogPass(string testName, string details)
    {
        testsPassed++;
        Debug.Log("<color=#00FF00>[PASS] " + testName + ": " + details + "</color>");
    }

    private void LogFail(string testName, string details)
    {
        testsFailed++;
        Debug.LogError("<color=#FF0000>[FAIL] " + testName + ": " + details + "</color>");
    }

    private void LogError(string message)
    {
        Debug.LogError("<color=#FF0000>[KeysTester ERROR] " + message + "</color>");
    }

    private void PrintTestSummary()
    {
        Debug.Log("<color=#00FFFF>========================================</color>");
        Debug.Log("<color=#00FFFF>  TEST SUMMARY</color>");
        Debug.Log("<color=#00FFFF>========================================</color>");
        Debug.Log("<color=#00FF00>PASSED: " + testsPassed.ToString() + "</color>");

        if (testsFailed > 0)
        {
            Debug.LogError("<color=#FF0000>FAILED: " + testsFailed.ToString() + "</color>");
        }
        else
        {
            Debug.Log("<color=#FFFFFF>FAILED: 0</color>");
        }

        Debug.Log("<color=#00FFFF>----------------------------------------</color>");

        if (testsFailed == 0)
        {
            Debug.Log("<color=#00FF00>RESULT: ALL TESTS PASSED</color>");
            Debug.Log("<color=#00FF00>All achievement keys validated successfully!</color>");
        }
        else
        {
            Debug.LogError("<color=#FF0000>RESULT: " + testsFailed.ToString() + " TESTS FAILED</color>");
        }

        Debug.Log("<color=#00FFFF>========================================</color>");
    }
}
