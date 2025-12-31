using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace LowerLevel.Integration
{
    /// <summary>
    /// COMPONENT PURPOSE:
    /// NotificationEventHub with network broadcasting and pause/queue system.
    /// Bridges AchievementTracker and Xbox-style notification displays.
    ///
    /// NETWORK ARCHITECTURE:
    /// - Uses UdonSynced variables for achievement/login data (VRChat-recommended).
    /// - OnDeserialization triggers on remote clients; late-joiners don't replay old toasts.
    /// - Preserves pause/resume + queue behavior.
    ///
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// - Delivers authentic Xbox Live-style popups, with ceremony/movie-friendly pause.
    ///
    /// DEPENDENCIES & REQUIREMENTS:
    /// - AchievementTracker (event source), AchievementDataManager (lookup),
    ///   XboxNotificationUI displays, optional UdonLogger, inspector wiring.
    ///
    /// ARCHITECTURE PATTERN:
    /// - Event-driven, explicit null checks, manual loops for UdonSharp compliance.
    ///
    /// CHANGELOG:
    /// 8/10/25 - FIXED: Component validation, null checks, network ownership
    /// 8/10/25 - FIX APPLIED: Master/Owner broadcasts ALL notifications
    /// 8/7/25 - Enforced VRChat player limits (MAX_PLAYERS=40)
    /// 8/7/25 - Implemented network broadcasting via UdonSynced vars
    /// 8/6/25 - Added pause/resume system with queued notifications
    /// 8/6/25 - UdonSharp compliance cleanup
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NotificationEventHub : UdonSharpBehaviour
    {
        [Header("Component References")]
        [Tooltip("Reference to AchievementTracker (data source)")]
        [SerializeField] private UdonBehaviour achievementTracker;

        [Tooltip("Reference to AchievementDataManager for achievement lookups")]
        [SerializeField] private UdonBehaviour dataManager;

        [Header("Display Components")]
        [Tooltip("Primary Xbox-style notification display (REQUIRED)")]
        [SerializeField] private UdonBehaviour primaryNotificationDisplay;

        [Tooltip("Additional displays that should receive notifications")]
        [SerializeField] private UdonBehaviour[] additionalDisplays;

        [Header("Terminal Integration")]
        [Tooltip("DOS Terminal Controller for achievement page refresh")]
        [SerializeField] private UdonBehaviour dosTerminalController;

        [Tooltip("DOS Terminal Achievement Module for direct refresh")]
        [SerializeField] private UdonBehaviour dosTerminalAchievementModule;

        [Header("Pause System")]
        [Tooltip("Enable notification pause/queue system for ceremonies")]
        [SerializeField] private bool enablePauseSystem = true;

        [Tooltip("Maximum queued notifications (default: 20)")]
        [SerializeField] private int maxQueueSize = 20;

        [Header("Debug Settings")]
        [Tooltip("Enable detailed console logging")]
        [SerializeField] private bool enableDebugLogging = true;

        [Tooltip("Optional production logger")]
        [SerializeField] private UdonBehaviour productionLogger;

        // =================================================================
        // NETWORK SYNCED VARIABLES
        // =================================================================

        // Achievement notification data - synced to all players
        [UdonSynced] private string syncedAchievementPlayerName = "";
        [UdonSynced] private int syncedAchievementLevel = -1;
        [UdonSynced] private string syncedAchievementType = "";
        [UdonSynced] private int syncedAchievementCounter = 0;

        // Login notification data - synced to all players
        [UdonSynced] private string syncedLoginPlayerName = "";
        [UdonSynced] private bool syncedLoginIsFirstTime = false;
        [UdonSynced] private int syncedLoginCounter = 0;

        // =================================================================
        // EVENT VARIABLES (Set via SetProgramVariable)
        // =================================================================

        [HideInInspector] public string eventPlayerName = "";
        [HideInInspector] public int eventAchievementLevel = -1;
        [HideInInspector] public string eventAchievementType = "";
        [HideInInspector] public bool eventIsFirstTime = false;

        // =================================================================
        // PRIVATE VARIABLES
        // =================================================================

        // Network sync tracking
        private int lastAchievementCounter = 0;
        private int lastLoginCounter = 0;

        // Pause system state
        private bool notificationsPaused = false;

        // Queue arrays
        private string[] queuedPlayerNames;
        private int[] queuedAchievementLevels;
        private string[] queuedAchievementTypes;
        private bool[] queuedIsFirstTime;
        private int[] queuedNotificationTypes;
        private int currentQueueSize = 0;

        // Player tracking
        private const int MAX_PLAYERS = 40;
        private VRCPlayerApi[] trackedPlayers;
        private bool[] remotePlayersRestored;
        private int trackedPlayerCount = 0;
        private bool localPlayerDataRestored = false;

        // =================================================================
        // INITIALIZATION
        // =================================================================

        void Start()
        {
            LogDebug("🚀 NotificationEventHub v3.1 initializing...");

            // Validate critical components
            if (!ValidateAllComponents())
            {
                LogDebug("❌ CRITICAL: Missing required components! Check inspector assignments!");
                LogDebug("primaryNotificationDisplay null: " + (primaryNotificationDisplay == null).ToString());
                LogDebug("dataManager null: " + (dataManager == null).ToString());
                LogDebug("achievementTracker null: " + (achievementTracker == null).ToString());
                return;
            }

            // Initialize queue arrays
            if (enablePauseSystem)
            {
                queuedNotificationTypes = new int[maxQueueSize];
                queuedPlayerNames = new string[maxQueueSize];
                queuedAchievementLevels = new int[maxQueueSize];
                queuedAchievementTypes = new string[maxQueueSize];
                queuedIsFirstTime = new bool[maxQueueSize];
                LogDebug("📦 Queue system initialized (size: " + maxQueueSize.ToString() + ")");
            }

            // Initialize player tracking
            trackedPlayers = new VRCPlayerApi[MAX_PLAYERS];
            remotePlayersRestored = new bool[MAX_PLAYERS];

            // Take ownership if we're the master
            if (Networking.LocalPlayer != null && Networking.LocalPlayer.isMaster)
            {
                if (!Networking.IsOwner(gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    LogDebug("👑 Master took ownership of NotificationEventHub");
                }
            }

            LogDebug("✅ NotificationEventHub initialized successfully");
        }

        // =================================================================
        // PLAYER TRACKING
        // =================================================================

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == null) return;

            LogDebug("👤 Player joined: " + player.displayName);

            // Track the player
            if (trackedPlayerCount < MAX_PLAYERS)
            {
                trackedPlayers[trackedPlayerCount] = player;
                remotePlayersRestored[trackedPlayerCount] = false;
                trackedPlayerCount++;
            }

            // Master maintains ownership for all broadcasts
            if (Networking.LocalPlayer != null && Networking.LocalPlayer.isMaster)
            {
                if (!Networking.IsOwner(gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    LogDebug("👑 Master maintained ownership after player join");
                }
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == null) return;

            LogDebug("👤 Player left: " + player.displayName);

            // Remove from tracking
            for (int i = 0; i < trackedPlayerCount && i < MAX_PLAYERS; i++)
            {
                if (trackedPlayers[i] != null && trackedPlayers[i].playerId == player.playerId)
                {
                    // Shift remaining players
                    for (int j = i; j < trackedPlayerCount - 1 && j < MAX_PLAYERS - 1; j++)
                    {
                        trackedPlayers[j] = trackedPlayers[j + 1];
                        remotePlayersRestored[j] = remotePlayersRestored[j + 1];
                    }
                    trackedPlayerCount--;
                    break;
                }
            }
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (player == null) return;

            if (player.isLocal)
            {
                localPlayerDataRestored = true;
                LogDebug("✅ Local player data restored");
                ProcessPendingLocalOperations();
            }
            else
            {
                for (int i = 0; i < trackedPlayerCount && i < MAX_PLAYERS; i++)
                {
                    if (trackedPlayers[i] != null && trackedPlayers[i].playerId == player.playerId)
                    {
                        remotePlayersRestored[i] = true;
                        LogDebug("✅ Remote player data restored: " + player.displayName);
                        break;
                    }
                }
            }
        }

        private void ProcessPendingLocalOperations()
        {
            LogDebug("📋 Ready to process notification operations");
        }

        // =================================================================
        // NETWORK SYNCHRONIZATION CALLBACK
        // =================================================================

        public override void OnDeserialization()
        {
            // Check for new achievement notification
            if (syncedAchievementCounter != lastAchievementCounter &&
                syncedAchievementPlayerName != "" &&
                syncedAchievementLevel >= 0)
            {
                lastAchievementCounter = syncedAchievementCounter;

                // Copy synced data to event variables
                eventPlayerName = syncedAchievementPlayerName;
                eventAchievementLevel = syncedAchievementLevel;
                eventAchievementType = syncedAchievementType;

                LogDebug("🌐 NETWORK: Received achievement - " + eventPlayerName +
                        " Level: " + eventAchievementLevel.ToString());

                // Process the notification locally
                if (enablePauseSystem && notificationsPaused)
                {
                    QueueAchievementNotification(eventPlayerName, eventAchievementLevel, eventAchievementType);
                }
                else
                {
                    ProcessAchievementNotification();
                }
                ClearEventVariables();
            }

            // Check for new login notification
            if (syncedLoginCounter != lastLoginCounter &&
                syncedLoginPlayerName != "")
            {
                lastLoginCounter = syncedLoginCounter;

                // Copy synced data to event variables
                eventPlayerName = syncedLoginPlayerName;
                eventIsFirstTime = syncedLoginIsFirstTime;

                LogDebug("🌐 NETWORK: Received login - " + eventPlayerName +
                        " FirstTime: " + eventIsFirstTime.ToString());

                // Process the notification locally
                if (enablePauseSystem && notificationsPaused)
                {
                    QueueLoginNotification(eventPlayerName, eventIsFirstTime);
                }
                else
                {
                    ProcessLoginNotification();
                }
                ClearEventVariables();
            }
        }

        // =================================================================
        // PUBLIC EVENT HANDLERS
        // =================================================================

        /// <summary>
        /// Called when any player earns an achievement
        /// Master/Owner broadcasts ALL notifications to ensure everyone sees them
        /// </summary>
        public void OnAchievementEarned()
        {
            if (!ValidateAllComponents()) return;

            LogDebug("🏆 OnAchievementEarned - Player: '" + eventPlayerName +
                    "', Level: " + eventAchievementLevel.ToString() +
                    ", Type: " + eventAchievementType);

            // Master broadcasts ALL notifications
            bool shouldBroadcast = false;
            if (Networking.LocalPlayer != null)
            {
                shouldBroadcast = Networking.LocalPlayer.isMaster || Networking.IsOwner(gameObject);

                if (shouldBroadcast)
                {
                    // Take ownership if we don't have it
                    if (!Networking.IsOwner(gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        LogDebug("🔧 Took ownership for broadcasting");
                    }

                    // Update synced variables for ALL achievements
                    syncedAchievementPlayerName = eventPlayerName;
                    syncedAchievementLevel = eventAchievementLevel;
                    syncedAchievementType = eventAchievementType;
                    syncedAchievementCounter++;

                    // Request network sync to all players
                    RequestSerialization();
                    LogDebug("📡 BROADCASTING achievement to ALL players: " + eventPlayerName);
                }
                else
                {
                    LogDebug("⚠️ Not Master/Owner - processing locally only");
                }
            }

            // Always process locally
            if (enablePauseSystem && notificationsPaused)
            {
                QueueAchievementNotification(eventPlayerName, eventAchievementLevel, eventAchievementType);
                ClearEventVariables();
                return;
            }

            ProcessAchievementNotification();
            ClearEventVariables();
        }

        /// <summary>
        /// Called when any player joins the world
        /// Master/Owner broadcasts ALL login notifications
        /// </summary>
        public void OnPlayerJoinedWorld()
        {
            if (!ValidateAllComponents()) return;

            LogDebug("👤 OnPlayerJoinedWorld - Player: '" + eventPlayerName +
                    "', FirstTime: " + eventIsFirstTime.ToString());

            // Master broadcasts ALL notifications
            bool shouldBroadcast = false;
            if (Networking.LocalPlayer != null)
            {
                shouldBroadcast = Networking.LocalPlayer.isMaster || Networking.IsOwner(gameObject);

                if (shouldBroadcast)
                {
                    // Take ownership if we don't have it
                    if (!Networking.IsOwner(gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, gameObject);
                        LogDebug("🔧 Took ownership for broadcasting");
                    }

                    // Update synced variables for ALL login notifications
                    syncedLoginPlayerName = eventPlayerName;
                    syncedLoginIsFirstTime = eventIsFirstTime;
                    syncedLoginCounter++;

                    // Request network sync to all players
                    RequestSerialization();
                    LogDebug("📡 BROADCASTING login to ALL players: " + eventPlayerName);
                }
                else
                {
                    LogDebug("⚠️ Not Master/Owner - processing locally only");
                }
            }

            // Always process locally
            if (enablePauseSystem && notificationsPaused)
            {
                QueueLoginNotification(eventPlayerName, eventIsFirstTime);
                ClearEventVariables();
                return;
            }

            ProcessLoginNotification();
            ClearEventVariables();
        }

        // =================================================================
        // PAUSE/RESUME SYSTEM
        // =================================================================

        public void PauseNotifications()
        {
            if (!enablePauseSystem)
            {
                LogDebug("⚠️ Pause system is disabled");
                return;
            }

            notificationsPaused = true;
            LogDebug("⏸️ NOTIFICATIONS PAUSED - queuing all incoming notifications");
        }

        public void ResumeNotifications()
        {
            if (!enablePauseSystem)
            {
                LogDebug("⚠️ Pause system is disabled");
                return;
            }

            notificationsPaused = false;
            LogDebug("▶️ NOTIFICATIONS RESUMED - releasing queued notifications");

            ProcessQueuedNotifications();
        }

        // =================================================================
        // QUEUE MANAGEMENT
        // =================================================================

        private void QueueAchievementNotification(string playerName, int level, string type)
        {
            if (!enablePauseSystem || currentQueueSize >= maxQueueSize)
            {
                LogDebug("❌ Cannot queue: Queue full or pause system disabled");
                return;
            }

            queuedNotificationTypes[currentQueueSize] = 0; // 0 = achievement
            queuedPlayerNames[currentQueueSize] = playerName;
            queuedAchievementLevels[currentQueueSize] = level;
            queuedAchievementTypes[currentQueueSize] = type;
            currentQueueSize++;

            LogDebug("📥 Queued achievement notification (" + currentQueueSize.ToString() +
                    "/" + maxQueueSize.ToString() + ")");
        }

        private void QueueLoginNotification(string playerName, bool isFirstTime)
        {
            if (!enablePauseSystem || currentQueueSize >= maxQueueSize)
            {
                LogDebug("❌ Cannot queue: Queue full or pause system disabled");
                return;
            }

            queuedNotificationTypes[currentQueueSize] = 1; // 1 = login
            queuedPlayerNames[currentQueueSize] = playerName;
            queuedIsFirstTime[currentQueueSize] = isFirstTime;
            currentQueueSize++;

            LogDebug("📥 Queued login notification (" + currentQueueSize.ToString() +
                    "/" + maxQueueSize.ToString() + ")");
        }

        private void ProcessQueuedNotifications()
        {
            if (currentQueueSize == 0)
            {
                LogDebug("📭 No queued notifications to process");
                return;
            }

            LogDebug("📤 Processing " + currentQueueSize.ToString() + " queued notifications");

            for (int i = 0; i < currentQueueSize; i++)
            {
                if (queuedNotificationTypes[i] == 0) // Achievement notification
                {
                    eventPlayerName = queuedPlayerNames[i];
                    eventAchievementLevel = queuedAchievementLevels[i];
                    eventAchievementType = queuedAchievementTypes[i];

                    ProcessAchievementNotification();
                }
                else if (queuedNotificationTypes[i] == 1) // Login notification
                {
                    eventPlayerName = queuedPlayerNames[i];
                    eventIsFirstTime = queuedIsFirstTime[i];

                    ProcessLoginNotification();
                }

                if (i < currentQueueSize - 1)
                {
                    SendCustomEventDelayedSeconds("ContinueQueueProcessing", 1.5f);
                    return;
                }
            }

            currentQueueSize = 0;
            ClearEventVariables();
            LogDebug("✅ All queued notifications processed and queue cleared");
        }

        public void ContinueQueueProcessing()
        {
            // This method is called by the delayed event system
            // Continue processing remaining queued notifications
            ProcessQueuedNotifications();
        }

        // =================================================================
        // NOTIFICATION PROCESSING
        // =================================================================

        private void ProcessAchievementNotification()
        {
            if (eventPlayerName == null || eventPlayerName == "" || eventAchievementLevel < 0)
            {
                LogDebug("❌ Invalid achievement data");
                return;
            }

            // Validate dataManager exists
            if (dataManager == null)
            {
                LogDebug("❌ DataManager is null - cannot process achievement");
                return;
            }

            string achievementTitle = "";
            int achievementPoints = 0;

            // Get achievement details based on type using proper UdonSharp method calls
            if (eventAchievementType == "activity")
            {
                // Call the appropriate getter methods on dataManager
                dataManager.SetProgramVariable("queryLevel", eventAchievementLevel);
                dataManager.SendCustomEvent("GetActivityAchievementTitle");
                achievementTitle = (string)dataManager.GetProgramVariable("resultTitle");

                dataManager.SendCustomEvent("GetActivityAchievementPoints");
                achievementPoints = (int)dataManager.GetProgramVariable("resultPoints");
            }
            else if (eventAchievementType == "time")
            {
                // Call the appropriate getter methods on dataManager
                dataManager.SetProgramVariable("queryLevel", eventAchievementLevel);
                dataManager.SendCustomEvent("GetTimeAchievementTitle");
                achievementTitle = (string)dataManager.GetProgramVariable("resultTitle");

                dataManager.SendCustomEvent("GetTimeAchievementPoints");
                achievementPoints = (int)dataManager.GetProgramVariable("resultPoints");
            }
            else // visit achievements
            {
                // Call the appropriate getter methods on dataManager
                dataManager.SetProgramVariable("queryLevel", eventAchievementLevel);
                dataManager.SendCustomEvent("GetAchievementTitle");
                achievementTitle = (string)dataManager.GetProgramVariable("resultTitle");

                dataManager.SendCustomEvent("GetAchievementPoints");
                achievementPoints = (int)dataManager.GetProgramVariable("resultPoints");
            }

            // Use fallback values if still empty
            if (string.IsNullOrEmpty(achievementTitle))
            {
                achievementTitle = "Achievement " + eventAchievementLevel.ToString();
                achievementPoints = 10; // Default points
                LogDebug("⚠️ Using fallback achievement data");
            }

            LogDebug("📋 Achievement: '" + achievementTitle + "' (" + achievementPoints.ToString() + "G)");

            ForwardAchievementToDisplays(eventPlayerName, achievementTitle, achievementPoints);
            RefreshTerminalDisplay();
        }

        private void ProcessLoginNotification()
        {
            if (eventPlayerName == null || eventPlayerName == "")
            {
                LogDebug("❌ Invalid login data");
                return;
            }

            LogDebug("📋 Login: '" + eventPlayerName + "' FirstTime: " + eventIsFirstTime.ToString());

            ForwardLoginToDisplays(eventPlayerName, eventIsFirstTime);
        }

        private void ForwardAchievementToDisplays(string playerName, string achievementTitle, int points)
        {
            LogDebug("📨 Forwarding achievement: " + playerName + " - " + achievementTitle +
                    " - " + points.ToString() + "G");

            // Forward to primary display
            if (primaryNotificationDisplay != null)
            {
                primaryNotificationDisplay.SetProgramVariable("queuePlayerName", playerName);
                primaryNotificationDisplay.SetProgramVariable("queueAchievementTitle", achievementTitle);
                primaryNotificationDisplay.SetProgramVariable("queuePoints", points);
                primaryNotificationDisplay.SendCustomEvent("QueueAchievementNotificationEvent"); // Changed to Event wrapper
                LogDebug("✅ Sent to primary display");
            }
            else
            {
                LogDebug("❌ Primary display is null!");
            }

            // Forward to additional displays
            if (additionalDisplays != null)
            {
                for (int i = 0; i < additionalDisplays.Length; i++)
                {
                    if (additionalDisplays[i] != null)
                    {
                        additionalDisplays[i].SetProgramVariable("queuePlayerName", playerName);
                        additionalDisplays[i].SetProgramVariable("queueAchievementTitle", achievementTitle);
                        additionalDisplays[i].SetProgramVariable("queuePoints", points);
                        additionalDisplays[i].SendCustomEvent("QueueAchievementNotificationEvent"); // Changed to Event wrapper
                        LogDebug("✅ Sent to additional display " + i.ToString());
                    }
                }
            }
        }

        private void ForwardLoginToDisplays(string playerName, bool isFirstTime)
        {
            LogDebug("📨 Forwarding login: " + playerName + " FirstTime: " + isFirstTime.ToString());

            // Forward to primary display
            if (primaryNotificationDisplay != null)
            {
                primaryNotificationDisplay.SetProgramVariable("queuePlayerName", playerName);
                primaryNotificationDisplay.SetProgramVariable("queueIsFirstTime", isFirstTime);
                primaryNotificationDisplay.SendCustomEvent("QueueOnlineNotificationEvent"); // Changed to Event wrapper
                LogDebug("✅ Sent login to primary display");
            }
            else
            {
                LogDebug("❌ Primary display is null!");
            }

            // Forward to additional displays
            if (additionalDisplays != null)
            {
                for (int i = 0; i < additionalDisplays.Length; i++)
                {
                    if (additionalDisplays[i] != null)
                    {
                        additionalDisplays[i].SetProgramVariable("queuePlayerName", playerName);
                        additionalDisplays[i].SetProgramVariable("queueIsFirstTime", isFirstTime);
                        additionalDisplays[i].SendCustomEvent("QueueOnlineNotificationEvent"); // Changed to Event wrapper
                        LogDebug("✅ Sent login to additional display " + i.ToString());
                    }
                }
            }
        }

        private void RefreshTerminalDisplay()
        {
            // Refresh terminal achievement displays (null-safe for remote clients)
            if (dosTerminalAchievementModule != null)
            {
                dosTerminalAchievementModule.SendCustomEvent("RefreshAchievementData");
                LogDebug("🖥️ Terminal achievement module refreshed");
            }

            if (dosTerminalController != null)
            {
                dosTerminalController.SendCustomEvent("RefreshDisplay");
                LogDebug("🖥️ Terminal controller refreshed");
            }
        }

        // =================================================================
        // VALIDATION & UTILITIES
        // =================================================================

        private bool ValidateAllComponents()
        {
            bool isValid = true;

            if (dataManager == null)
            {
                LogDebug("❌ AchievementDataManager not assigned!");
                isValid = false;
            }

            if (primaryNotificationDisplay == null)
            {
                LogDebug("❌ Primary notification display not assigned!");
                isValid = false;
            }

            if (achievementTracker == null)
            {
                LogDebug("⚠️ AchievementTracker not assigned (non-critical)");
            }

            return isValid;
        }

        private void ClearEventVariables()
        {
            eventPlayerName = "";
            eventAchievementLevel = -1;
            eventAchievementType = "";
            eventIsFirstTime = false;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                string coloredMessage = "<color=#00FFFF>🔔 [NotificationEventHub] " + message + "</color>";
                Debug.Log(coloredMessage);

                if (productionLogger != null)
                {
                    productionLogger.SetProgramVariable("logMessage", coloredMessage);
                    productionLogger.SendCustomEvent("Log");
                }
            }
        }

        // =================================================================
        // DIAGNOSTIC METHODS
        // =================================================================

        [ContextMenu("Test Achievement Notification")]
        public void TestAchievementNotification()
        {
            LogDebug("🧪 Testing achievement notification...");
            eventPlayerName = "TestPlayer";
            eventAchievementLevel = 0;
            eventAchievementType = "visit";
            OnAchievementEarned();
        }

        [ContextMenu("Test Login Notification")]
        public void TestLoginNotification()
        {
            LogDebug("🧪 Testing login notification...");
            eventPlayerName = "TestPlayer";
            eventIsFirstTime = true;
            OnPlayerJoinedWorld();
        }

        [ContextMenu("Show Component Status")]
        public void ShowComponentStatus()
        {
            LogDebug("=== COMPONENT STATUS ===");
            LogDebug("DataManager: " + (dataManager != null ? "✅" : "❌"));
            LogDebug("PrimaryDisplay: " + (primaryNotificationDisplay != null ? "✅" : "❌"));
            LogDebug("AchievementTracker: " + (achievementTracker != null ? "✅" : "❌"));
            LogDebug("Terminal Controller: " + (dosTerminalController != null ? "✅" : "❌"));
            LogDebug("Terminal Achievement: " + (dosTerminalAchievementModule != null ? "✅" : "❌"));
            LogDebug("Additional Displays: " + (additionalDisplays != null ? additionalDisplays.Length.ToString() : "0"));
            LogDebug("Queue Size: " + currentQueueSize.ToString() + "/" + maxQueueSize.ToString());
            LogDebug("Paused: " + notificationsPaused.ToString());
        }
    }
}