/// COMPONENT PURPOSE:
/// Comprehensive PlayerData testing system for Lower Level 2.0 basement
/// Tests all PlayerData types with basement-themed keys and values
/// Provides debugging interface for persistence development
/// 
/// LOWER LEVEL 2.0 INTEGRATION:
/// Enables persistent basement experiences - saves player preferences, achievements, stats
/// Tracks nostalgic gaming milestones and social basement interactions  
/// Maintains continuity of the authentic 2000s basement experience across sessions
/// 
/// DEPENDENCIES & REQUIREMENTS:
/// - TextMeshPro components for data display
/// - UI Button components for testing actions
/// - VRC.SDK3.Persistence namespace (critical import)
/// - Setup: Attach to GameObject with UI elements in scene
/// - Assets: No additional assets required
/// 
/// ARCHITECTURE PATTERN:
/// Event-driven PlayerData testing with comprehensive type validation
/// Uses OnPlayerDataUpdated callback for real-time data monitoring
/// Implements safe data access patterns with type checking
/// Optimized for Quest compatibility with efficient UI updates

using System;
using System.Globalization;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Persistence;  // ← CRITICAL IMPORT FOR PLAYERDATA
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerDataTester : UdonSharpBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI dataDisplayText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button setTestDataButton;
    [SerializeField] private Button clearDataButton;
    [SerializeField] private Button refreshDisplayButton;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool autoRefreshDisplay = true;

    // Basement-themed PlayerData keys
    private const string BASEMENT_VISITS_KEY = "basement_total_visits";
    private const string FAVORITE_GAME_KEY = "basement_favorite_game";
    private const string SNAKE_HIGH_SCORE_KEY = "basement_snake_high_score";
    private const string TV_VOLUME_KEY = "basement_tv_volume";
    private const string LIGHTING_MOOD_KEY = "basement_lighting_mood";
    private const string FRIEND_INVITE_COUNT_KEY = "basement_friends_invited";
    private const string ACHIEVEMENT_UNLOCKED_KEY = "basement_achievement_unlocked";
    private const string LAST_VISIT_TIME_KEY = "basement_last_visit_time";
    private const string PREFERRED_SPOT_KEY = "basement_preferred_spot";
    private const string TOTAL_PLAYTIME_KEY = "basement_total_playtime";

    // Test data tracking
    private int testDataSetCount = 0;
    private float lastUpdateTime = 0f;

    void Start()
    {
        DebugLog("PlayerDataTester initialized");
        UpdateStatusDisplay("PlayerData Tester Ready");

        // Test the method directly after 2 seconds (for debugging)
        SendCustomEventDelayedSeconds(nameof(SetBasementTestData), 2f);

        // Initial display update after brief delay to ensure PlayerData is ready
        SendCustomEventDelayedSeconds(nameof(RefreshDataDisplay), 3f);
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        if (!player.isLocal) return;

        DebugLog($"OnPlayerDataUpdated called with {infos.Length} updates");

        for (int i = 0; i < infos.Length; i++)
        {
            string key = infos[i].Key;
            PlayerData.State state = infos[i].State;

            DebugLog($"PlayerData update - Key: {key}, State: {state}");

            // Handle different states (only Restored is commonly used)
            if (state == PlayerData.State.Restored)
            {
                DebugLog($"Data restored from server: {key}");
            }
            else
            {
                DebugLog($"PlayerData state for {key}: {state}");
            }
        }

        if (autoRefreshDisplay)
        {
            RefreshDataDisplay();
        }

        lastUpdateTime = Time.time;
    }

    /// <summary>
    /// Sets comprehensive test data covering all basement features
    /// </summary>
    public void SetBasementTestData()
    {
        testDataSetCount++;
        DebugLog("Setting basement-themed test data...");

        // Basement visit tracking
        PlayerData.SetInt(BASEMENT_VISITS_KEY, testDataSetCount * 5);

        // Gaming data
        PlayerData.SetString(FAVORITE_GAME_KEY, "Nokia Snake");
        PlayerData.SetInt(SNAKE_HIGH_SCORE_KEY, 2500 + (testDataSetCount * 100));

        // Environment preferences  
        PlayerData.SetFloat(TV_VOLUME_KEY, 0.75f);
        PlayerData.SetString(LIGHTING_MOOD_KEY, "Cozy Dim");

        // Social tracking
        PlayerData.SetInt(FRIEND_INVITE_COUNT_KEY, testDataSetCount * 2);
        PlayerData.SetBool(ACHIEVEMENT_UNLOCKED_KEY, testDataSetCount >= 3);

        // Time tracking (using double for precision)
        PlayerData.SetDouble(LAST_VISIT_TIME_KEY, Time.time);
        PlayerData.SetFloat(TOTAL_PLAYTIME_KEY, 1337.5f + (testDataSetCount * 60f));

        // Vector data for preferred basement location
        Vector3 preferredSpot = new Vector3(2.5f, 1.0f, -3.2f);
        PlayerData.SetVector3(PREFERRED_SPOT_KEY, preferredSpot);

        UpdateStatusDisplay($"Test data set #{testDataSetCount} - Success!");
        DebugLog($"Basement test data set successfully (batch #{testDataSetCount})");
    }

    /// <summary>
    /// Clears all basement PlayerData for testing
    /// </summary>
    public void ClearAllBasementData()
    {
        DebugLog("Clearing all basement PlayerData...");

        // Note: VRChat doesn't have a direct "delete" method, so we set to default values
        PlayerData.SetInt(BASEMENT_VISITS_KEY, 0);
        PlayerData.SetString(FAVORITE_GAME_KEY, "");
        PlayerData.SetInt(SNAKE_HIGH_SCORE_KEY, 0);
        PlayerData.SetFloat(TV_VOLUME_KEY, 0.5f);
        PlayerData.SetString(LIGHTING_MOOD_KEY, "Default");
        PlayerData.SetInt(FRIEND_INVITE_COUNT_KEY, 0);
        PlayerData.SetBool(ACHIEVEMENT_UNLOCKED_KEY, false);
        PlayerData.SetDouble(LAST_VISIT_TIME_KEY, 0.0);
        PlayerData.SetFloat(TOTAL_PLAYTIME_KEY, 0f);
        PlayerData.SetVector3(PREFERRED_SPOT_KEY, Vector3.zero);

        testDataSetCount = 0;
        UpdateStatusDisplay("All basement data cleared!");
        DebugLog("All basement PlayerData cleared successfully");
    }

    /// <summary>
    /// Refreshes the data display with current PlayerData values
    /// </summary>
    public void RefreshDataDisplay()
    {
        if (dataDisplayText == null) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        string displayOutput = $"🏠 BASEMENT PLAYERDATA STATUS 🏠\n";
        displayOutput += $"Player: {localPlayer.displayName} (ID: {localPlayer.playerId})\n";
        displayOutput += $"Last Update: {Time.time - lastUpdateTime:F1}s ago\n\n";

        // Display all basement data with type safety
        displayOutput += FormatPlayerDataValue(localPlayer, BASEMENT_VISITS_KEY, "Basement Visits", typeof(int));
        displayOutput += FormatPlayerDataValue(localPlayer, FAVORITE_GAME_KEY, "Favorite Game", typeof(string));
        displayOutput += FormatPlayerDataValue(localPlayer, SNAKE_HIGH_SCORE_KEY, "Snake High Score", typeof(int));
        displayOutput += FormatPlayerDataValue(localPlayer, TV_VOLUME_KEY, "TV Volume", typeof(float));
        displayOutput += FormatPlayerDataValue(localPlayer, LIGHTING_MOOD_KEY, "Lighting Mood", typeof(string));
        displayOutput += FormatPlayerDataValue(localPlayer, FRIEND_INVITE_COUNT_KEY, "Friends Invited", typeof(int));
        displayOutput += FormatPlayerDataValue(localPlayer, ACHIEVEMENT_UNLOCKED_KEY, "Achievement Unlocked", typeof(bool));
        displayOutput += FormatPlayerDataValue(localPlayer, LAST_VISIT_TIME_KEY, "Last Visit Time", typeof(double));
        displayOutput += FormatPlayerDataValue(localPlayer, TOTAL_PLAYTIME_KEY, "Total Playtime", typeof(float));
        displayOutput += FormatPlayerDataValue(localPlayer, PREFERRED_SPOT_KEY, "Preferred Spot", typeof(Vector3));

        dataDisplayText.text = displayOutput;
        DebugLog("Data display refreshed");
    }

    /// <summary>
    /// Safely formats PlayerData values with type checking
    /// </summary>
    private string FormatPlayerDataValue(VRCPlayerApi player, string key, string displayName, Type expectedType)
    {
        if (!PlayerData.HasKey(player, key))
        {
            return $"📭 {displayName}: Not Set\n";
        }

        Type actualType = PlayerData.GetType(player, key);
        if (actualType != expectedType)
        {
            return $"⚠️ {displayName}: Type Mismatch (expected {expectedType.Name}, got {actualType.Name})\n";
        }

        string value = PlayerDataToString(player, key, actualType);
        return $"✅ {displayName}: {value}\n";
    }

    /// <summary>
    /// Converts PlayerData to string representation (adapted from VRChat examples)
    /// </summary>
    private string PlayerDataToString(VRCPlayerApi player, string key, Type type)
    {
        if (!PlayerData.HasKey(player, key)) return "Key does not exist";

        if (type == typeof(bool)) return PlayerData.GetBool(player, key).ToString();
        if (type == typeof(int)) return PlayerData.GetInt(player, key).ToString();
        if (type == typeof(float)) return PlayerData.GetFloat(player, key).ToString("F2");
        if (type == typeof(double)) return PlayerData.GetDouble(player, key).ToString("F2");
        if (type == typeof(string)) return $"\"{PlayerData.GetString(player, key)}\"";
        if (type == typeof(Vector3)) return PlayerData.GetVector3(player, key).ToString("F1");
        if (type == typeof(Vector2)) return PlayerData.GetVector2(player, key).ToString("F1");
        if (type == typeof(Color)) return PlayerData.GetColor(player, key).ToString();
        if (type == typeof(Quaternion)) return PlayerData.GetQuaternion(player, key).ToString("F1");

        return $"Unknown type: {type.Name}";
    }

    /// <summary>
    /// Updates the status display text
    /// </summary>
    private void UpdateStatusDisplay(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{Time.time:F0}s] {message}";
        }
    }

    /// <summary>
    /// Debug logging with toggle
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[PlayerDataTester] {message}");
        }
    }

    /// <summary>
    /// Simulates a basement achievement unlock
    /// </summary>
    public void UnlockBasementAchievement()
    {
        PlayerData.SetBool(ACHIEVEMENT_UNLOCKED_KEY, true);
        PlayerData.SetString("achievement_name", "Basement Master");
        PlayerData.SetFloat("achievement_unlock_time", Time.time);

        UpdateStatusDisplay("🏆 Achievement Unlocked: Basement Master!");
        DebugLog("Basement achievement unlocked");
    }

    /// <summary>
    /// Increments basement visit counter
    /// </summary>
    public void IncrementBasementVisits()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        int currentVisits = 0;

        if (PlayerData.HasKey(localPlayer, BASEMENT_VISITS_KEY) &&
            PlayerData.GetType(localPlayer, BASEMENT_VISITS_KEY) == typeof(int))
        {
            currentVisits = PlayerData.GetInt(localPlayer, BASEMENT_VISITS_KEY);
        }

        currentVisits++;
        PlayerData.SetInt(BASEMENT_VISITS_KEY, currentVisits);

        UpdateStatusDisplay($"Visit #{currentVisits} recorded!");
        DebugLog($"Basement visits incremented to {currentVisits}");
    }
}