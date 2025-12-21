using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;
using LowerLevel.Integration;

/// <summary>
/// COMPONENT PURPOSE:
/// Large proximity zone around TV area that reliably catches first-time visitors
/// Uses generous detection area and backup timer to ensure notification delivery
/// 
/// LOWER LEVEL 2.0 INTEGRATION:
/// Creates natural "discovered the TV area" notification experience
/// Much larger detection zone catches players regardless of spawn point
/// Backup systems ensure notification shows even if they don't move around
/// 
/// DEPENDENCIES & REQUIREMENTS:
/// - Large Box Collider set as Trigger (covers most of basement area)
/// - NotificationEventHub component
/// - Position this near TV but make collider MUCH larger than just TV area
/// 
/// ARCHITECTURE PATTERN:
/// Spatial detection with backup timer and large capture area
/// Multiple fallback methods to ensure notification delivery
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SmartTVProximityNotifier : UdonSharpBehaviour
{
    [Header("Detection Configuration")]
    [Tooltip("How long to wait before showing backup notification (seconds)")]
    [SerializeField] private float backupNotificationDelay = 10.0f;

    [Tooltip("Custom notification text")]
    [SerializeField] private string notificationText = "Welcome to the Lower Level TV area!";

    [Header("System Integration")]
    [Tooltip("Reference to your existing NotificationEventHub")]
    [SerializeField] private NotificationEventHub notificationHub;

    [Header("Debug Settings")]
    [Tooltip("Enable debug logging")]
    [SerializeField] private bool enableDebugLogging = true;

    // Constants and variables
    private const string TV_AREA_VISITED_KEY = "basement_tv_area_discovered";
    private bool hasScheduledBackup = false;
    private string currentPlayerName = "";

    void Start()
    {
        LogDebug("SmartTVProximityNotifier initialized");

        // Validate collider setup
        Collider col = GetComponent<Collider>();
        if (col == null || !col.isTrigger)
        {
            LogDebug("WARNING: Need large Box Collider set as Trigger!");
        }

        if (notificationHub == null)
        {
            LogDebug("ERROR: NotificationEventHub not assigned!");
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // Only process local player
        if (!Utilities.IsValid(player) || !player.isLocal)
        {
            return;
        }

        currentPlayerName = player.displayName;

        // Check if first time visitor
        bool isFirstTimeVisitor = !PlayerData.HasKey(player, "basement_visit_count");
        bool hasSeenTVNotification = PlayerData.HasKey(player, TV_AREA_VISITED_KEY);

        if (isFirstTimeVisitor && !hasSeenTVNotification && !hasScheduledBackup)
        {
            LogDebug($"Scheduling backup TV notification for {currentPlayerName}");

            // Schedule backup notification in case they don't trigger the spatial zone
            SendCustomEventDelayedSeconds("ShowBackupTVNotification", backupNotificationDelay);
            hasScheduledBackup = true;
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        // Only process local player
        if (!Utilities.IsValid(player) || !player.isLocal)
        {
            return;
        }

        string playerName = player.displayName;

        // Check if they've already seen the TV area notification
        if (PlayerData.HasKey(player, TV_AREA_VISITED_KEY))
        {
            LogDebug($"Player {playerName} already discovered TV area");
            return;
        }

        // Check if this is a first-time visitor
        bool isFirstTimeVisitor = !PlayerData.HasKey(player, "basement_visit_count");

        if (isFirstTimeVisitor)
        {
            LogDebug($"First-time visitor {playerName} discovered TV area!");
            ShowTVAreaNotification(playerName);
        }
    }

    /// <summary>
    /// Shows the TV area discovery notification
    /// </summary>
    private void ShowTVAreaNotification(string playerName)
    {
        if (notificationHub == null)
        {
            LogDebug("ERROR: Cannot show TV notification - NotificationEventHub not assigned");
            return;
        }

        // Mark that they've seen the TV area notification
        PlayerData.SetBool(TV_AREA_VISITED_KEY, true);

        LogDebug($"🎮 Showing TV area notification for: {playerName}");

        // Send notification using existing system
        notificationHub.SetProgramVariable("eventPlayerName", playerName);
        notificationHub.SetProgramVariable("eventIsFirstTime", true);
        notificationHub.SendCustomEvent("OnPlayerJoinedWorld");

        LogDebug($"✅ TV area notification sent for: {playerName}");

        // Cancel any pending backup notification
        hasScheduledBackup = false;
    }

    /// <summary>
    /// Backup notification in case spatial trigger doesn't fire
    /// </summary>
    public void ShowBackupTVNotification()
    {
        if (string.IsNullOrEmpty(currentPlayerName))
        {
            LogDebug("No current player for backup notification");
            return;
        }

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer))
        {
            return;
        }

        // Only show if they still haven't seen it
        if (!PlayerData.HasKey(localPlayer, TV_AREA_VISITED_KEY))
        {
            LogDebug($"🔄 Showing backup TV notification for: {currentPlayerName}");
            ShowTVAreaNotification(currentPlayerName);
        }
        else
        {
            LogDebug("Backup notification cancelled - player already saw TV notification");
        }

        hasScheduledBackup = false;
    }

    /// <summary>
    /// Reset TV area discovery for testing
    /// </summary>
    [ContextMenu("Reset TV Area Discovery")]
    public void ResetTVAreaDiscovery()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (Utilities.IsValid(localPlayer))
        {
            PlayerData.SetBool(TV_AREA_VISITED_KEY, false);
            LogDebug("Reset TV area discovery - next visit will show notification");
        }
    }

    /// <summary>
    /// Test TV area notification
    /// </summary>
    [ContextMenu("Test TV Area Notification")]
    public void TestTVAreaNotification()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (Utilities.IsValid(localPlayer))
        {
            LogDebug("=== TESTING TV AREA NOTIFICATION ===");
            ShowTVAreaNotification(localPlayer.displayName);
            LogDebug("=== TEST COMPLETE ===");
        }
    }

    /// <summary>
    /// Centralized debug logging
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            string coloredMessage = "<color=#00CED1>📺 [TVProximity] " + message + "</color>";
            Debug.Log(coloredMessage);
        }
    }
}