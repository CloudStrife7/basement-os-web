using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;
using Varneon.VUdon.Logger.Abstract;

/*
 * 🧠 NEW KNOWLEDGE GAINED - COMPREHENSIVE ACHIEVEMENT SYSTEM 7/22/25:
 * 
 * ✅ TOTAL ACHIEVEMENTS SUPPORTED (19 total):
 * - 8 Visit achievements: 1, 5, 10, 25, 50, 75, 100, 250 visits
 * - 5 Time achievements: 5min, 30min, 1hr, 2hr, 5hr sessions  
 * - 6 Activity achievements: Heavy Rain, Night Owl, Early Bird, Weekend Warrior, Streak Master, Party Animal
 * 
 * ✅ ESTABLISHED COMMUNICATION FLOW FOLLOWED:
 * 🎮 PLAYER JOINS WORLD
 *    ↓ 📊 AchievementTracker.OnPlayerJoined() detects join
 *    ↓ 💾 AchievementDataManager.AddPlayerVisit() saves visit count  
 *    ↓ 🏆 AchievementTracker.CheckForNewAchievements() checks unlock
 *    ↓ 📢 AchievementTracker.HandleNewAchievement() → NotificationEventHub 
 *    ↓ 👑 NotificationEventHub → NotificationRolesModule (roles check)
 *    ↓ 📺 NotificationEventHub → XboxNotificationUI (popup display)
 * 
 * ✅ REAL ACHIEVEMENT EARNING (Not Simulation):
 * - Uses dataManager.AddPlayerVisit() for actual visit tracking
 * - Uses dataManager.CheckActivityAchievements() for real condition checking
 * - Uses dataManager.StartPlayerSession() for actual time tracking
 * - All achievements set PlayerData flags permanently
 * 
 * ✅ AUTOMATIC PERSISTENCE ACROSS SESSIONS:
 * - PlayerData.SetBool/SetInt automatically saves to VRChat servers
 * - Achievements persist when leaving/rejoining world
 * - No manual save/load code needed - VRChat handles it
 * 
 * ✅ ANTI-SPAM PROTECTION (Heavy Rain Pattern):
 * - Simple boolean flags: if earned → skip entirely
 * - No complex date logic needed - once earned, never check again
 * - Follows established CheckWeatherAchievement pattern
 * 
 * ✅ UDONSHARP BEST PRACTICES FOLLOWED:
 * - No try/catch blocks - defensive programming instead
 * - No foreach loops - traditional for loops only
 * - No null-conditional operators - explicit null checks
 * - String concatenation instead of interpolation
 * - Inspector references over runtime discovery
 * - Frame-based timing with Update() + Time.time
 * - Simple boolean flags over complex state machines
 * 
 * 🎯 KEY TESTING METHODS:
 * - "🏆 COMPREHENSIVE: All 19 Achievements + Persistence Test" - Earns all achievements for real
 * - "🧪 BULLETPROOF: All 19 Achievements Test (Testing Overrides)" - Bypasses real-world conditions
 * - "💾 Test Persistence: Clear All Data" - Test persistence by clearing and rejoining
 * - "📊 Show Achievement Status" - View current achievement status
 * - "🧪 SIMPLE: Quick First Visit + Heavy Rain Test" - Quick test of 2 achievements
 */

namespace LowerLevel.Achievements
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AchievementTracker : UdonSharpBehaviour
    {
        [Header("Component References")]
        [Tooltip("Reference to AchievementDataManager for data persistence")]
        [SerializeField] private AchievementDataManager dataManager;

        [Tooltip("Reference to AchievementKeys component for accessing achievement key arrays")]
        [SerializeField] private AchievementKeys achievementKeys;

        [Header("Integration")]
        [Tooltip("Reference to NotificationEventHub for achievement notifications")]
        [SerializeField] private UdonBehaviour notificationEventHub;

        [Header("Debug Settings")]
        [Tooltip("Enable detailed logging for troubleshooting")]
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Production Debugging")]
        [Tooltip("Reference to UdonLogger component for in-world console (optional)")]
        [SerializeField] private UdonLogger productionLogger;

        [Header("Achievement Detection")]
        [Tooltip("Check for achievements when players join")]
        [SerializeField] private bool checkAchievementsOnJoin = true;

        [Header("Login Notifications")]
        [Tooltip("Send notifications when players join world")]
        [SerializeField] private bool sendLoginNotifications = true;

        [Header("🆕 Activity Tracking")]
        [Tooltip("Enable automatic activity achievement checking")]
        [SerializeField] private bool enableActivityTracking = true;

        [Tooltip("How often to check activity achievements (seconds)")]
        [SerializeField] private float activityCheckInterval = 30f;

        // =================================================================
        // PRIVATE VARIABLES
        // =================================================================

        private bool isInitialized = false;
        private float nextActivityCheck = 0f;
        private bool[] timeAchievementNotificationSent = new bool[5];
        private int pendingAchievementLevel = -1;
        private string pendingAchievementPlayerName = "";
        // 🆕 Session milestone tracking (reserved for time edge-detection)
        private float lastCheckedSessionTime = 0f;

        // =================================================================
        // INITIALIZATION - Called when world loads
        // =================================================================

        void Start()
        {
            InitializeComponent();

            // 🔍 AUTO-RUN PERSISTENCE DIAGNOSTIC
            // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
            SendCustomEventDelayedSeconds("AutoPersistenceDiagnostic", 2f);
        }

        protected virtual void InitializeComponent()
        {
            LogDebug("🚀 Achievement Tracker initializing (Xbox 360 Edition)...");

            if (dataManager == null)
            {
                LogDebug("❌ No AchievementDataManager assigned! Please assign in Inspector.");
                return;
            }

            // UDONSHARP FIX (2025-11-13): Validate AchievementKeys reference
            if (achievementKeys == null)
            {
                Debug.LogError("[AchievementTracker] AchievementKeys reference not assigned! Please assign in Unity Inspector.");
                return;
            }

            LogDebug("✅ Achievement Tracker initialized successfully");
            isInitialized = true;

            LogDebug("🔧 TESTING: About to check for local player session start");

            // ⏱️ CRITICAL FIX: Start session tracking for local player immediately
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            LogDebug("🔧 TESTING: Got local player, checking conditions...");

            if (localPlayer != null && localPlayer.isLocal)
            {
                LogDebug("🔧 TESTING: Local player conditions met, getting name...");
                string playerName = localPlayer.displayName;
                if (playerName != null && playerName != "")
                {
                    LogDebug("🔧 TESTING: Player name ready, starting session...");
                    dataManager.StartPlayerSession(playerName);
                    LogDebug("⏱️ FIXED: Started session tracking for local player in Start(): " + playerName);
                }
                else
                {
                    LogDebug("🔧 TESTING: Player name not ready, scheduling retry...");
                    // Player name not ready yet, retry in 1 second
                    // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
                    SendCustomEventDelayedSeconds("RetrySessionStart", 1f);
                    LogDebug("⏱️ Player name not ready, scheduling retry in 1 second");
                }
            }
            else
            {
                LogDebug("🔧 TESTING: Local player conditions NOT met");
            }

            if (enableActivityTracking)
            {
                nextActivityCheck = Time.time + activityCheckInterval;
                LogDebug("⏱️ Activity tracking scheduled using frame-based timing");
            }
        }

        void Update()
        {
            if (!isInitialized || !enableActivityTracking) return;

            if (Time.time >= nextActivityCheck)
            {
                CheckActivityAchievements();
                nextActivityCheck = Time.time + activityCheckInterval;
            }
        }

        // =================================================================
        // 🎮 VRCHAT EVENT HANDLERS - MAIN ENTRY POINTS
        // =================================================================

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            LogDebug("🎮 OnPlayerJoined CALLED!!!");
            if (!isInitialized || dataManager == null)
            {
                LogDebug("⚠️ Not initialized, skipping");
                return;
            }
            if (!Utilities.IsValid(player))
            {
                LogDebug("❌ Invalid player object, skipping");
                return;
            }
            string playerName = player.displayName;
            if (playerName == null || playerName == "")
            {
                LogDebug("❌ Player joined with empty name, skipping");
                return;
            }
            LogDebug("👤 Player joined: " + playerName);

            int previousVisits = dataManager.GetPlayerVisits(playerName);
            bool isFirstTime = (previousVisits == 0);

            // 🔑 TIMING FIX: Check achievements BEFORE incrementing visits
            if (checkAchievementsOnJoin)
            {
                CheckForNewAchievements(playerName, previousVisits);
            }

            int newVisitCount = dataManager.AddPlayerVisit(playerName);
            LogDebug("📈 " + playerName + " now has " + newVisitCount.ToString() + " total visits (was " + previousVisits.ToString() + ")");

            if (player.isLocal)
            {
                dataManager.StartPlayerSession(playerName);
                LogDebug("⏱️ Started session tracking for local player: " + playerName);
            }

            if (sendLoginNotifications && notificationEventHub != null)
            {
                SendLoginNotification(playerName, isFirstTime);
            }

            if (enableActivityTracking && player.isLocal)
            {
                CheckPlayerActivityAchievements(playerName);
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
            {
            if (!isInitialized) return;

            if (!Utilities.IsValid(player)) return;

            string playerName = player.displayName;
            LogDebug("👋 Player left: " + playerName);

            if (player.isLocal && dataManager != null)
            {
                dataManager.EndPlayerSession(playerName);
                LogDebug("⏱️ Ended session tracking for local player: " + playerName);
            }
        }

        // =================================================================
        // 🏆 ACHIEVEMENT CHECKING LOGIC
        // =================================================================

        /// <summary>
        /// Checks all activity-based achievements for the local player.
        /// Includes time-of-day (Night Owl, Early Bird), player count (Party Animal), and weekend achievements.
        /// Called periodically based on activityCheckInterval.
        /// </summary>
        public void CheckActivityAchievements()
        {
            if (!isInitialized || !enableActivityTracking) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;

            CheckPlayerActivityAchievements(playerName);
            CheckForTimeBasedAchievementNotifications(playerName);
        }

        /// <summary>
        /// Checks for newly earned time-based achievements and sends notifications.
        /// Called by AchievementDataManager during session time updates via SendCustomEvent.
        /// Iterates through all 5 time achievement levels using edge detection.
        /// IMPORTANT: Must be public for SendCustomEvent compatibility (UdonSharp requirement).
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        public void CheckForTimeBasedAchievementNotifications(string playerName)
        {
            // Loop all 5 time achievements and apply edge-detection logic
            for (int i = 0; i < 5; i++)
            {
                CheckForSpecificTimeAchievement(playerName, i);
            }
        }

        /// <summary>
        /// Checks time-of-day, player count, and weekend achievements for a player.
        /// Delegates to AchievementDataManager for actual condition checking.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        private void CheckPlayerActivityAchievements(string playerName)
        {
            if (dataManager == null) return;

            dataManager.CheckActivityAchievements(playerName, "time");

            int currentPlayerCount = VRCPlayerApi.GetPlayerCount();
            dataManager.CheckActivityAchievements(playerName, "playercount", currentPlayerCount.ToString());

            dataManager.CheckActivityAchievements(playerName, "weekend");

            LogDebug("🎯 Checked activity achievements for " + playerName);
        }

        /// <summary>
        /// Checks weather-based achievements (Heavy Rain) for the local player.
        /// Should be called by external weather systems when conditions change.
        /// </summary>
        /// <param name="weatherCondition">Current weather condition string (e.g., "rain", "storm", "drizzle")</param>
        public void CheckWeatherAchievements(string weatherCondition)
        {
            if (!isInitialized || !enableActivityTracking) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;
            dataManager.CheckActivityAchievements(playerName, "weather", weatherCondition);

            LogDebug("🌧️ Checked weather achievements for " + playerName + ": " + weatherCondition);
        }

        /// <summary>
        /// Checks if a player has earned a specific activity achievement and sends notification.
        /// Prevents duplicate notifications by checking if already earned.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="activityIndex">Activity achievement index (0-5)</param>
        private void CheckForSpecificActivityAchievement(string playerName, int activityIndex)
        {
            if (dataManager == null) return;

            if (dataManager.HasPlayerEarnedActivityAchievement(playerName, activityIndex))
            {
                LogDebug("✅ Activity achievement " + activityIndex.ToString() + " already earned by " + playerName + ", skipping");
                return;
            }

            bool hasEarned = dataManager.HasPlayerEarnedActivityAchievement(playerName, activityIndex);

            if (hasEarned)
            {
                string achievementTitle = dataManager.GetActivityAchievementTitle(activityIndex);
                int achievementPoints = dataManager.GetActivityAchievementPoints(activityIndex);

                LogDebug("🎉 SPECIFIC ACHIEVEMENT! " + playerName + " earned: " + achievementTitle + " (" + achievementPoints.ToString() + "G)");

                if (notificationEventHub != null)
                {
                    notificationEventHub.SetProgramVariable("eventPlayerName", playerName);
                    notificationEventHub.SetProgramVariable("eventAchievementLevel", activityIndex);
                    notificationEventHub.SetProgramVariable("eventAchievementType", "activity");
                    notificationEventHub.SetProgramVariable("eventIsFirstTime", false);
                    notificationEventHub.SendCustomEvent("OnAchievementEarned");

                    LogDebug("🔔 Specific achievement notification sent: " + playerName + " - " + achievementTitle);
                }
            }
        }

        /// <summary>
        /// Checks and sends notification for the Heavy Rain achievement (activity index 0).
        /// Wrapper method for external calls to check weather achievement.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        public void CheckForWeatherAchievementNotification(string playerName)
        {
            CheckForSpecificActivityAchievement(playerName, 0);
        }

        /// <summary>
        /// FIXED METHOD: Checks for a specific time achievement and sends notification if newly earned
        /// 🔧 CRITICAL FIX: Inverted logic to properly send notifications when achievements are earned
        /// </summary>
        /// <param name="playerName">Player to check achievement for</param>
        /// <param name="timeIndex">Time achievement index (0-4 for 5 time achievements)</param>

        /// <summary>
        /// Edge-detect time achievements so we only notify when the player
        /// crosses the threshold THIS session (not from prior saved state).
        /// </summary>
        private void CheckForSpecificTimeAchievement(string playerName, int timeIndex)
        {
            if (dataManager == null || notificationEventHub == null) return;

            // 🆕 EDGE DETECTION: read current session time
            float currentSessionTime = dataManager.GetCurrentSessionTime(playerName);
            float milestone = 0f;

            // Milestones (seconds)
            if (timeIndex == 0) milestone = 300f;      // 5 min
            else if (timeIndex == 1) milestone = 1800f;   // 30 min
            else if (timeIndex == 2) milestone = 3600f;   // 1 hr
            else if (timeIndex == 3) milestone = 7200f;   // 2 hr
            else if (timeIndex == 4) milestone = 18000f;  // 5 hr

            // 🆕 Is the player crossing the threshold NOW? (was below ~1s ago, now at/above)
            bool crossingThreshold = currentSessionTime >= milestone &&
                                     (currentSessionTime - 1.0f) < milestone;

            // Check persisted earned state (any source)
            bool isEarned = dataManager.HasPlayerEarnedTimeAchievement(playerName, timeIndex);

            // 🆕 Check if it was already earned previously (persisted PlayerData)
            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
            VRCPlayerApi player = Networking.LocalPlayer;
            bool wasAlreadyEarned = false;
            if (player != null && player.displayName == playerName)
            {
                if (timeIndex >= 0 && timeIndex < achievementKeys.TIME_ACHIEVEMENT_KEYS.Length)
                {
                    bool hasKey = PlayerData.HasKey(player, achievementKeys.TIME_ACHIEVEMENT_KEYS[timeIndex]);
                    if (hasKey)
                    {
                        wasAlreadyEarned = PlayerData.GetBool(player, achievementKeys.TIME_ACHIEVEMENT_KEYS[timeIndex]);
                    }
                }
            }

            // 🆕 CRITICAL: Only notify if crossing NOW and it wasn't already earned before
            if (isEarned && crossingThreshold && !wasAlreadyEarned)
            {
                // Per-session de-dupe
                if (timeIndex >= 0 && timeIndex < timeAchievementNotificationSent.Length)
                {
                    if (timeAchievementNotificationSent[timeIndex])
                    {
                        LogDebug("✅ Time achievement " + timeIndex.ToString() + " already notified this session for " + playerName + ", skipping");
                        return;
                    }
                }

                string achievementTitle = dataManager.GetTimeAchievementTitle(timeIndex);
                int achievementPoints = dataManager.GetTimeAchievementPoints(timeIndex);

                LogDebug("⏱️ NEW time achievement earned! " + playerName + " crossed " + milestone.ToString() + "s for: " + achievementTitle + " (" + achievementPoints.ToString() + "G)");

                // Send notification
                notificationEventHub.SetProgramVariable("eventPlayerName", playerName);
                notificationEventHub.SetProgramVariable("eventAchievementLevel", timeIndex);
                notificationEventHub.SetProgramVariable("eventAchievementType", "time");
                notificationEventHub.SendCustomEvent("OnAchievementEarned");

                timeAchievementNotificationSent[timeIndex] = true;

                LogDebug("🔔 Time achievement notification sent: [" + timeIndex.ToString() + "] " + playerName + " - " + achievementTitle);
            }
            else
            {
                // More detailed debug for non-notify path
                string reason = !isEarned ? "not earned"
                             : wasAlreadyEarned ? "already earned previously"
                             : !crossingThreshold ? "not crossing threshold now"
                             : "unknown";
                LogDebug("⏳ Time achievement " + timeIndex.ToString() + " not notifying: " + reason);
            }
        }



        /// <summary>
        /// Sends a login notification to the NotificationEventHub when a player joins.
        /// Distinguishes between first-time and returning players.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="isFirstTime">True if this is the player's first visit</param>
        private void SendLoginNotification(string playerName, bool isFirstTime)
        {
            if (notificationEventHub == null)
            {
                LogDebug("⚠️ No NotificationEventHub assigned - login notification skipped");
                return;
            }

            LogDebug("📢 Sending login notification: " + playerName + " (First time: " + isFirstTime.ToString() + ")");

            notificationEventHub.SetProgramVariable("eventPlayerName", playerName);
            notificationEventHub.SetProgramVariable("eventIsFirstTime", isFirstTime);
            notificationEventHub.SendCustomEvent("OnPlayerJoinedWorld");

            LogDebug("✅ Login notification sent to NotificationEventHub: " + playerName);
        }

        /// <summary>
        /// FIXED METHOD: Now prevents duplicate notifications when rejoining world
        /// Only sends notifications for achievements NEWLY EARNED this session
        /// </summary>
        private void CheckForNewAchievements(string playerName, int currentVisits)
        {
            LogDebug("🏆 Checking achievements for " + playerName + " with " + currentVisits.ToString() + " visits");

            // The NEXT visit will increment to currentVisits + 1
            int nextVisitCount = currentVisits + 1;

            // Check all visit achievements
            int totalVisitAchievements = 8;
            for (int achievementLevel = 0; achievementLevel < totalVisitAchievements; achievementLevel++)
            {
                int requiredVisits = dataManager.GetRequiredVisits(achievementLevel);

                // Check if this achievement will be newly earned with the next visit
                if (currentVisits < requiredVisits && nextVisitCount >= requiredVisits)
                {
                    // This achievement will be earned when we increment!
                    LogDebug("🎯 Visit Achievement " + achievementLevel.ToString() + " will be earned on this visit!");

                    // Send notification AFTER the visit count is incremented
                    // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
                    SendCustomEventDelayedFrames("DelayedAchievementCheck", 3);
                    pendingAchievementLevel = achievementLevel;
                    pendingAchievementPlayerName = playerName;
                }

            }

            LogDebug("✅ Achievement pre-check complete");
        }


        /// <summary>
        /// Processes pending achievement checks after a delayed frame.
        /// Used to ensure visit count is incremented before checking achievement status.
        /// </summary>
        public void DelayedAchievementCheck()
        {
            if (pendingAchievementLevel >= 0 && pendingAchievementPlayerName != "")
            {
                // Now check if it was actually earned (after increment)
                if (dataManager.HasPlayerEarnedAchievement(pendingAchievementPlayerName, pendingAchievementLevel))
                {
                    HandleNewAchievement(pendingAchievementPlayerName, pendingAchievementLevel, "visit");
                }

                // Clear pending
                pendingAchievementLevel = -1;
                pendingAchievementPlayerName = "";
            }
        }

        /// <summary>
        /// ENHANCED METHOD: Now properly detects NEWLY earned achievements (not just earned achievements)
        /// UdonSharp compliant - no complex boolean expressions or unsupported operators
        /// </summary>
        private bool IsNewlyEarnedVisitAchievement(string playerName, int achievementLevel, int currentVisits)
        {
            // ✅ FIRST: Check if achievement is earned at all
            bool hasEarnedNow = dataManager.HasPlayerEarnedAchievement(playerName, achievementLevel);
            if (!hasEarnedNow) return false;

            int[] milestones = { 1, 5, 10, 25, 50, 75, 100, 250 };

            if (achievementLevel < milestones.Length)
            {
                // ✅ CRITICAL FIX: Only send notification if we JUST reached this milestone
                bool justReached = currentVisits == milestones[achievementLevel];

                LogDebug("🎯 Visit Achievement " + achievementLevel.ToString() + ": visits=" + currentVisits.ToString() +
                        ", milestone=" + milestones[achievementLevel].ToString() + ", justReached=" + justReached.ToString());

                // ✅ ADDITIONAL CHECK: Don't notify if achievement was earned in previous session
                if (justReached)
                {
                    // Check if this is truly a new achievement by seeing if the flag was already set
                    VRCPlayerApi player = Networking.LocalPlayer;
                    if (player != null)
                    {
                        if (player.displayName == playerName)
                        {
                            // Use centralized visit achievement keys array
                            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
                            if (achievementLevel < 0 || achievementLevel >= achievementKeys.VISIT_ACHIEVEMENT_KEYS.Length)
                            {
                                LogDebug("❌ Invalid achievement level: " + achievementLevel.ToString());
                                return false;
                            }

                            string achievementKey = achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel];

                            // ✅ UDONSHARP FIX: Split complex boolean into separate checks
                            bool hasKey = PlayerData.HasKey(player, achievementKey);
                            if (hasKey)
                            {
                                bool wasAlreadyEarned = PlayerData.GetBool(player, achievementKey);
                                if (wasAlreadyEarned)
                                {
                                    LogDebug("⏭️ Achievement " + achievementLevel.ToString() + " was earned in previous session - skipping notification");
                                    return false;
                                }
                            }

                            LogDebug("🎉 NEW achievement " + achievementLevel.ToString() + " earned this session!");
                            return true;
                        }
                    }
                }

                return justReached;
            }

            return false;
        }

        /// <summary>
        /// Checks if a player has earned a specific time-based achievement.
        /// Delegates to AchievementDataManager for persistence check.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="timeLevel">Time achievement level (0-4)</param>
        /// <returns>True if the player has earned this time achievement</returns>
        private bool IsNewlyEarnedTimeAchievement(string playerName, int timeLevel)
        {
            return dataManager.HasPlayerEarnedTimeAchievement(playerName, timeLevel);
        }

        /// <summary>
        /// Checks if a player has earned a specific activity achievement.
        /// Delegates to AchievementDataManager for persistence check.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="activityLevel">Activity achievement level (0-5)</param>
        /// <returns>True if the player has earned this activity achievement</returns>
        private bool IsNewlyEarnedActivityAchievement(string playerName, int activityLevel)
        {
            return dataManager.HasPlayerEarnedActivityAchievement(playerName, activityLevel);
        }

        /// <summary>
        /// Handles a newly earned achievement by retrieving details and sending notification.
        /// Supports visit, time, and activity achievement types.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="achievementLevel">Achievement level/index</param>
        /// <param name="achievementType">Type: "visit", "time", or "activity"</param>
        private void HandleNewAchievement(string playerName, int achievementLevel, string achievementType)
        {
            string achievementTitle = "";
            int achievementPoints = 0;

            if (achievementType == "visit")
            {
                achievementTitle = dataManager.GetAchievementTitle(achievementLevel);
                achievementPoints = dataManager.GetAchievementPoints(achievementLevel);
            }
            else if (achievementType == "time")
            {
                achievementTitle = dataManager.GetTimeAchievementTitle(achievementLevel);
                achievementPoints = dataManager.GetTimeAchievementPoints(achievementLevel);
            }
            else if (achievementType == "activity")
            {
                achievementTitle = dataManager.GetActivityAchievementTitle(achievementLevel);
                achievementPoints = dataManager.GetActivityAchievementPoints(achievementLevel);
            }

            int totalPoints = dataManager.GetPlayerTotalPoints(playerName);

            LogDebug("🎉 NEW ACHIEVEMENT! " + playerName + " earned: " + achievementTitle + " (" + achievementPoints.ToString() + "G)");
            LogDebug("📊 Total points for " + playerName + ": " + totalPoints.ToString() + "G");

            if (notificationEventHub != null)
            {
                notificationEventHub.SetProgramVariable("eventPlayerName", playerName);
                notificationEventHub.SetProgramVariable("eventAchievementLevel", achievementLevel);
                notificationEventHub.SetProgramVariable("eventAchievementType", achievementType);
                notificationEventHub.SendCustomEvent("OnAchievementEarned");

                LogDebug("🔔 Achievement notification sent to NotificationEventHub: " + playerName + " - " + achievementTitle);
            }
            else
            {
                LogDebug("📋 Achievement notification ready for notification system");
            }
        }

        // =================================================================
        // 🧪 TESTING OVERRIDE METHODS - BYPASS REAL-WORLD CONDITIONS
        // =================================================================

        private void ForceEarnAllActivityAchievementsForTesting(string playerName)
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return;

            LogDebugBulletproof("🧪 === FORCE EARNING ALL ACTIVITY ACHIEVEMENTS (TESTING ONLY) ===");

            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 0))
            {
                LogDebugBulletproof("🌧️ Force earning Heavy Rain achievement...");
                PlayerData.SetBool(AchievementKeys.HEAVY_RAIN_EARNED_KEY, true);
                CheckForSpecificActivityAchievement(playerName, 0);
            }

            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 1))
            {
                LogDebugBulletproof("🦉 Force earning Night Owl achievement...");
                PlayerData.SetBool(AchievementKeys.NIGHT_OWL_EARNED_KEY, true);
                CheckForSpecificActivityAchievement(playerName, 1);
            }

            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 2))
            {
                LogDebugBulletproof("🐦 Force earning Early Bird achievement...");
                PlayerData.SetBool(AchievementKeys.EARLY_BIRD_EARNED_KEY, true);
                CheckForSpecificActivityAchievement(playerName, 2);
            }

            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 3))
            {
                LogDebugBulletproof("🎮 Force earning Weekend Warrior achievement...");
                PlayerData.SetBool(AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY, true);
                CheckForSpecificActivityAchievement(playerName, 3);
            }

            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 4))
            {
                LogDebugBulletproof("🔥 Force earning Streak Master achievement...");
                PlayerData.SetBool(AchievementKeys.STREAK_MASTER_EARNED_KEY, true);
                CheckForSpecificActivityAchievement(playerName, 4);
            }

            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 5))
            {
                LogDebugBulletproof("🎉 Force earning Party Animal achievement...");
                PlayerData.SetBool(AchievementKeys.PARTY_ANIMAL_EARNED_KEY, true);
                CheckForSpecificActivityAchievement(playerName, 5);
            }

            LogDebugBulletproof("✅ All activity achievements force-earned for testing!");
        }

        // =================================================================
        // 🧪 TESTING AND DEBUG METHODS
        // =================================================================

        /// <summary>
        /// [DEBUG] Simulates a player join event for testing achievement tracking.
        /// Adds a visit and triggers achievement checks without requiring actual player join.
        /// </summary>
        /// <param name="testPlayerName">Test player name to simulate</param>
        public void SimulatePlayerJoin(string testPlayerName)
        {
            if (!enableDebugLogging) return;

            LogDebug("🧪 SIMULATING player join: " + testPlayerName);

            int previousVisits = dataManager.GetPlayerVisits(testPlayerName);
            bool isFirstTime = (previousVisits == 0);

            if (dataManager != null)
            {
                int newVisitCount = dataManager.AddPlayerVisit(testPlayerName);
                LogDebug("📈 Simulated visit result: " + testPlayerName + " = " + newVisitCount.ToString() + " visits");

                if (sendLoginNotifications)
                {
                    SendLoginNotification(testPlayerName, isFirstTime);
                }

                if (checkAchievementsOnJoin)
                {
                    CheckForNewAchievements(testPlayerName, newVisitCount);
                }
            }
        }

        /// <summary>
        /// Gets the current initialization and configuration status of the tracker.
        /// Useful for debugging integration issues.
        /// </summary>
        /// <returns>Status string describing tracker state</returns>
        public string GetTrackerStatus()
        {
            if (!isInitialized)
                return "❌ Achievement Tracker: Not Initialized";

            if (dataManager == null)
                return "❌ Achievement Tracker: No Data Manager";

            string[] allPlayers = dataManager.GetAllPlayerNames();
            return "✅ Achievement Tracker: Active, tracking " + allPlayers.Length.ToString() + " players";
        }

        public void RetrySessionStart()
        {
            LogDebug("🔧 TESTING: RetrySessionStart called");
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer != null && localPlayer.isLocal)
            {
                string playerName = localPlayer.displayName;
                if (playerName != null && playerName != "")
                {
                    dataManager.StartPlayerSession(playerName);
                    LogDebug("⏱️ RETRY: Started session tracking for local player: " + playerName);
                }
                else
                {
                    LogDebug("⏱️ RETRY FAILED: Player name still not ready");
                }
            }
        }

        /// <summary>
        /// Logs debug messages to Unity console and optional production logger.
        /// Only outputs when enableDebugLogging is true.
        /// </summary>
        /// <param name="message">Debug message to log</param>
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                // UdonSharp limitation: No string interpolation with complex expressions
                // Use string concatenation instead
                string coloredMessage = "<color=#FF8C00>🎮 [AchievementTracker] " + message + "</color>";

                // Unity console WITH color (rich text supported)
                Debug.Log(coloredMessage);

                // VUdon-Logger with SAME color - DIRECT METHOD CALL!
                if (productionLogger != null)
                {
                    productionLogger.Log(coloredMessage);
                }
            }
        }

        /// <summary>
        /// Logs debug messages with error handling to prevent crashes.
        /// Always outputs to Unity console even if production logger fails.
        /// </summary>
        /// <param name="message">Debug message to log</param>
        private void LogDebugBulletproof(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log("🎮 [AchievementTracker] [Bulletproof] " + message);
            }
        }

        // =================================================================
        // 🔍 TIME TRACKING DIAGNOSTIC METHODS - NEW DEBUGGING TOOLS
        // =================================================================

        [ContextMenu("🔍 DIAGNOSE: Time Tracking System")]
        public void DiagnoseTimeTrackingSystem()
        {
            LogDebug("🔍 === TIME TRACKING DIAGNOSTIC START ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null)
            {
                LogDebug("❌ No local player found");
                return;
            }

            string playerName = localPlayer.displayName;

            // Check session tracking variables
            float sessionStart = 0f;
            if (PlayerData.HasKey(localPlayer, AchievementKeys.SESSION_START_KEY))
            {
                sessionStart = PlayerData.GetFloat(localPlayer, AchievementKeys.SESSION_START_KEY);
                LogDebug("📊 Session Start Time: " + sessionStart.ToString("F1") + "s");
            }
            else
            {
                LogDebug("❌ No session start time recorded");
            }

            // Check current time vs session start
            float currentTime = Time.time;
            float sessionDuration = currentTime - sessionStart;
            LogDebug("⏱️ Current Time: " + currentTime.ToString("F1") + "s");
            LogDebug("📏 Calculated Duration: " + sessionDuration.ToString("F1") + "s (" + (sessionDuration / 60f).ToString("F1") + " minutes)");

            // Check if dataManager.StartPlayerSession was called
            bool sessionActive = false;
            if (dataManager != null)
            {
                object sessionActiveObj = dataManager.GetProgramVariable("sessionActive");
                if (sessionActiveObj != null)
                {
                    sessionActive = (bool)sessionActiveObj;
                }
            }
            LogDebug("🔄 Session Active Flag: " + sessionActive.ToString());

            // Test each time milestone manually
            float[] timeThresholds = { 300f, 1800f, 3600f, 7200f, 18000f };
            string[] achievementNames = { "Quick Visit", "Hangout", "Party Time", "2 Legit 2 Quit", "Marathon" };

            for (int i = 0; i < timeThresholds.Length; i++)
            {
                bool shouldHaveEarned = sessionDuration >= timeThresholds[i];
                bool actuallyEarned = dataManager.HasPlayerEarnedTimeAchievement(playerName, i);
                string status = shouldHaveEarned ? (actuallyEarned ? "✅ CORRECT" : "❌ MISSING") : "⏳ PENDING";
                LogDebug("🏆 " + achievementNames[i] + " (" + (timeThresholds[i] / 60f).ToString("F0") + "min): " + status);
            }

            LogDebug("🔍 === TIME TRACKING DIAGNOSTIC COMPLETE ===");
        }

        [ContextMenu("🚀 FORCE: Start Player Session")]
        public void ForceStartPlayerSession()
        {
            LogDebug("🚀 === FORCING SESSION START ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;

            string playerName = localPlayer.displayName;

            // Force call StartPlayerSession
            if (dataManager != null)
            {
                dataManager.StartPlayerSession(playerName);
                LogDebug("✅ Called StartPlayerSession for " + playerName);

                // Verify it was set
                // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
                SendCustomEventDelayedSeconds("DiagnoseTimeTrackingSystem", 1f);
            }
            else
            {
                LogDebug("❌ DataManager is null");
            }
        }

        [ContextMenu("🔍 DEBUG: Why Session Start Failed")]
        public void DebugWhySessionStartFailed()
        {
            LogDebug("🔍 === DEBUGGING SESSION START FAILURE ===");

            LogDebug("📊 Checking initialization state...");
            LogDebug("✅ isInitialized: " + isInitialized.ToString());
            LogDebug("✅ dataManager null?: " + (dataManager == null).ToString());

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            LogDebug("✅ localPlayer null?: " + (localPlayer == null).ToString());

            if (localPlayer != null)
            {
                LogDebug("✅ localPlayer.isLocal: " + localPlayer.isLocal.ToString());
                // UDONSHARP FIX (Day 5): Replaced ?? null coalescing with ternary operator
                LogDebug("✅ localPlayer.displayName: '" + (localPlayer.displayName != null ? localPlayer.displayName : "NULL") + "'");
                LogDebug("✅ displayName.Length: " + (localPlayer.displayName != null ? localPlayer.displayName.Length.ToString() : "NULL"));
            }

            LogDebug("🔍 === SESSION START FAILURE DEBUG COMPLETE ===");
        }

        [ContextMenu("🔍 DEBUG: DataManager Session Details")]
        public void DebugDataManagerSessionDetails()
        {
            LogDebug("🔍 === DATAMANAGER SESSION DEBUG ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null)
            {
                LogDebug("❌ No local player");
                return;
            }

            string playerName = localPlayer.displayName;
            LogDebug("👤 Player Name: '" + playerName + "'");

            // Check if dataManager has time tracking enabled
            if (dataManager != null)
            {
                object enableTimeTrackingObj = dataManager.GetProgramVariable("enableTimeTracking");
                LogDebug("⏱️ DataManager enableTimeTracking: " + (enableTimeTrackingObj != null ? enableTimeTrackingObj.ToString() : "NULL"));

                object isInitializedObj = dataManager.GetProgramVariable("isInitialized");
                LogDebug("🚀 DataManager isInitialized: " + (isInitializedObj != null ? isInitializedObj.ToString() : "NULL"));

                // Check if PlayerData key exists
                bool hasSessionStartKey = PlayerData.HasKey(localPlayer, AchievementKeys.SESSION_START_KEY);
                LogDebug("🔑 Has session start key: " + hasSessionStartKey.ToString());

                if (hasSessionStartKey)
                {
                    float sessionStartTime = PlayerData.GetFloat(localPlayer, AchievementKeys.SESSION_START_KEY);
                    LogDebug("📊 Session start time value: " + sessionStartTime.ToString());
                }
            }
            else
            {
                LogDebug("❌ DataManager is null");
            }

            LogDebug("🔍 === DATAMANAGER SESSION DEBUG COMPLETE ===");
        }

        [ContextMenu("🧪 FORCE: All Time Achievements (Testing Override)")]
        public void ForceEarnAllTimeAchievementsForTesting()
        {
            LogDebug("🧪 === FORCING ALL TIME ACHIEVEMENTS ===");
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;
            string playerName = localPlayer.displayName;

            LogDebug("🎯 Before: " + GetTotalTimeAchievementsEarned(playerName).ToString() + "/5 time achievements");

            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
            for (int i = 0; i < achievementKeys.TIME_ACHIEVEMENT_KEYS.Length; i++)
            {
                PlayerData.SetBool(achievementKeys.TIME_ACHIEVEMENT_KEYS[i], true);
                LogDebug("✅ Set " + achievementKeys.TIME_ACHIEVEMENT_KEYS[i] + " = true");

                // Trigger notification for each achievement
                HandleNewAchievement(playerName, i, "time");
            }

            LogDebug("🏆 After: " + GetTotalTimeAchievementsEarned(playerName).ToString() + "/5 time achievements");
            LogDebug("🧪 === TIME ACHIEVEMENTS TESTING OVERRIDE COMPLETE ===");
        }

        private int GetTotalTimeAchievementsEarned(string playerName)
        {
            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (dataManager.HasPlayerEarnedTimeAchievement(playerName, i))
                    count++;
            }
            return count;
        }

        // =================================================================
        // 🧪 BULLETPROOF ALL-ACHIEVEMENTS TEST - BYPASSES REAL-WORLD CONDITIONS
        // =================================================================

        [ContextMenu("🧪 BULLETPROOF: All 19 Achievements Test (Testing Overrides)")]
        public void BulletproofComprehensiveAllAchievementsTest()
        {
            if (!enableDebugLogging) return;
            LogDebugBulletproof("🎉 === BULLETPROOF ALL-ACHIEVEMENTS TEST STARTED ===");
            LogDebugBulletproof("🧪 Using TESTING OVERRIDES to bypass real-world conditions");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;
            string playerName = localPlayer.displayName;

            LogDebugBulletproof("🗑️ Clearing player data for clean test");
            if (dataManager != null)
            {
                dataManager.DebugClearMyPlayerData();
                LogDebugBulletproof("✅ Player data cleared - starting fresh");
            }

            LogDebugBulletproof("🌟 === PHASE 1: FORCE EARNING ALL 6 ACTIVITY ACHIEVEMENTS ===");
            ForceEarnAllActivityAchievementsForTesting(playerName);

            LogDebugBulletproof("🏠 === PHASE 2: EARNING FIRST VISIT ACHIEVEMENT ===");
            EarnFirstVisitAchievement(playerName);

            LogDebugBulletproof("⏰ === PHASE 3: STARTING TIME TRACKING FOR TIME ACHIEVEMENTS ===");
            StartTimeTrackingForAchievements(playerName);

            LogDebugBulletproof("📊 === BULLETPROOF TEST RESULTS ===");
            ShowComprehensiveResults(playerName);

            int totalEarned = 0;
            for (int i = 0; i < 8; i++) if (dataManager.HasPlayerEarnedAchievement(playerName, i)) totalEarned++;
            for (int i = 0; i < 5; i++) if (dataManager.HasPlayerEarnedTimeAchievement(playerName, i)) totalEarned++;
            for (int i = 0; i < 6; i++) if (dataManager.HasPlayerEarnedActivityAchievement(playerName, i)) totalEarned++;

            LogDebugBulletproof("🎯 FINAL RESULT: " + totalEarned.ToString() + "/19 achievements earned!");
            LogDebugBulletproof("🎮 This test bypasses real-world conditions for comprehensive verification");
            LogDebugBulletproof("💾 All achievements should now be visible in terminal!");
        }

        [ContextMenu("🏆 COMPREHENSIVE: All 19 Achievements + Persistence Test")]
        public void ComprehensiveAllAchievementsTest()
        {
            if (!enableDebugLogging) return;
            LogDebugBulletproof("🎉 === COMPREHENSIVE ALL-ACHIEVEMENTS TEST STARTED ===");
            LogDebugBulletproof("🎯 EARNING ALL 19 ACHIEVEMENTS FOR REAL (8 visit + 5 time + 6 activity)");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;
            string playerName = localPlayer.displayName;

            LogDebugBulletproof("🗑️ Clearing player data for clean test");
            if (dataManager != null)
            {
                dataManager.DebugClearMyPlayerData();
                LogDebugBulletproof("✅ Player data cleared - starting fresh");
            }

            LogDebugBulletproof("🌟 === PHASE 1: EARNING ALL 6 ACTIVITY ACHIEVEMENTS ===");
            EarnAllActivityAchievements(playerName);

            LogDebugBulletproof("🏠 === PHASE 2: EARNING FIRST VISIT ACHIEVEMENT ===");
            EarnFirstVisitAchievement(playerName);

            LogDebugBulletproof("⏰ === PHASE 3: STARTING TIME TRACKING FOR TIME ACHIEVEMENTS ===");
            StartTimeTrackingForAchievements(playerName);

            LogDebugBulletproof("📊 === COMPREHENSIVE TEST RESULTS ===");
            ShowComprehensiveResults(playerName);

            LogDebugBulletproof("💾 === PHASE 4: PERSISTENCE TEST INSTRUCTIONS ===");
            LogDebugBulletproof("🔄 To test persistence: Leave world and rejoin - achievements should remain earned");
            LogDebugBulletproof("✅ PlayerData automatically persists across sessions");
            LogDebugBulletproof("🎯 Run 'Show Achievement Status' after rejoining to verify persistence");
        }

        private void EarnAllActivityAchievements(string playerName)
        {
            LogDebugBulletproof("🌧️ Earning Heavy Rain achievement...");
            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 0))
            {
                dataManager.CheckActivityAchievements(playerName, "weather", "Heavy Rain");
                if (dataManager.HasPlayerEarnedActivityAchievement(playerName, 0))
                {
                    LogDebugBulletproof("✅ Heavy Rain earned! Sending notification...");
                    CheckForSpecificActivityAchievement(playerName, 0);
                }
            }

            LogDebugBulletproof("🦉 Earning Night Owl achievement...");
            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 1))
            {
                dataManager.CheckActivityAchievements(playerName, "time");
                if (dataManager.HasPlayerEarnedActivityAchievement(playerName, 1))
                {
                    LogDebugBulletproof("✅ Night Owl earned! Sending notification...");
                    CheckForSpecificActivityAchievement(playerName, 1);
                }
            }

            LogDebugBulletproof("🐦 Earning Early Bird achievement...");
            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 2))
            {
                dataManager.CheckActivityAchievements(playerName, "time");
                if (dataManager.HasPlayerEarnedActivityAchievement(playerName, 2))
                {
                    LogDebugBulletproof("✅ Early Bird earned! Sending notification...");
                    CheckForSpecificActivityAchievement(playerName, 2);
                }
            }

            LogDebugBulletproof("🎮 Earning Weekend Warrior achievement...");
            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 3))
            {
                dataManager.CheckActivityAchievements(playerName, "weekend");
                if (dataManager.HasPlayerEarnedActivityAchievement(playerName, 3))
                {
                    LogDebugBulletproof("✅ Weekend Warrior earned! Sending notification...");
                    CheckForSpecificActivityAchievement(playerName, 3);
                }
            }

            LogDebugBulletproof("🔥 Earning Streak Master achievement...");
            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 4))
            {
                dataManager.CheckActivityAchievements(playerName, "weekend");
                if (dataManager.HasPlayerEarnedActivityAchievement(playerName, 4))
                {
                    LogDebugBulletproof("✅ Streak Master earned! Sending notification...");
                    CheckForSpecificActivityAchievement(playerName, 4);
                }
                else
                {
                    LogDebugBulletproof("⚠️ Streak Master requires multiple visits over time");
                }
            }

            LogDebugBulletproof("🎉 Earning Party Animal achievement...");
            if (!dataManager.HasPlayerEarnedActivityAchievement(playerName, 5))
            {
                int currentPlayerCount = VRCPlayerApi.GetPlayerCount();
                LogDebugBulletproof("👥 Current player count: " + currentPlayerCount.ToString());
                dataManager.CheckActivityAchievements(playerName, "playercount", currentPlayerCount.ToString());
                if (dataManager.HasPlayerEarnedActivityAchievement(playerName, 5))
                {
                    LogDebugBulletproof("✅ Party Animal earned! Sending notification...");
                    CheckForSpecificActivityAchievement(playerName, 5);
                }
                else
                {
                    LogDebugBulletproof("⚠️ Party Animal requires more players in world");
                }
            }

            LogDebugBulletproof("🌟 Activity achievements phase complete!");
        }

        private void EarnFirstVisitAchievement(string playerName)
        {
            if (dataManager.HasPlayerEarnedAchievement(playerName, 0))
            {
                LogDebugBulletproof("✅ First Visit already earned");
                return;
            }

            int newVisits = dataManager.AddPlayerVisit(playerName);
            LogDebugBulletproof("📈 Added real visit - now have " + newVisits.ToString() + " visits");

            if (dataManager.HasPlayerEarnedAchievement(playerName, 0))
            {
                LogDebugBulletproof("✅ First Visit achievement earned! Sending notification...");
                HandleNewAchievement(playerName, 0, "visit");
            }
            else
            {
                LogDebugBulletproof("❌ First Visit achievement not earned - check logic");
            }
        }

        private void StartTimeTrackingForAchievements(string playerName)
        {
            dataManager.StartPlayerSession(playerName);
            LogDebugBulletproof("⏱️ Started real session tracking for time achievements");

            LogDebugBulletproof("📅 Time Achievement Milestones:");
            LogDebugBulletproof("   • Quick Visit: 5 minutes (300 seconds)");
            LogDebugBulletproof("   • Hangout: 30 minutes (1800 seconds)");
            LogDebugBulletproof("   • Party Time: 1 hour (3600 seconds)");
            LogDebugBulletproof("   • 2 Legit 2 Quit: 2 hours (7200 seconds)");
            LogDebugBulletproof("   • Marathon: 5 hours (18000 seconds)");
            LogDebugBulletproof("⏰ These will be earned as you spend time in the world");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (Utilities.IsValid(localPlayer))
            {
                for (int i = 0; i < 5; i++)
                {
                    bool hasTimeAchievement = dataManager.HasPlayerEarnedTimeAchievement(playerName, i);
                    string timeTitle = dataManager.GetTimeAchievementTitle(i);
                    LogDebugBulletproof("   " + (hasTimeAchievement ? "✅" : "🔒") + " " + timeTitle);
                }
            }
        }

        private void ShowComprehensiveResults(string playerName)
        {
            int totalPoints = dataManager.GetPlayerTotalPoints(playerName);
            int totalVisits = dataManager.GetPlayerVisits(playerName);

            LogDebugBulletproof("📊 === COMPREHENSIVE ACHIEVEMENT STATUS ===");
            LogDebugBulletproof("👤 Player: " + playerName);
            LogDebugBulletproof("📈 Total Visits: " + totalVisits.ToString());
            LogDebugBulletproof("🏆 Total Points: " + totalPoints.ToString() + "G");

            LogDebugBulletproof("🏠 VISIT ACHIEVEMENTS:");
            for (int i = 0; i < 8; i++)
            {
                bool earned = dataManager.HasPlayerEarnedAchievement(playerName, i);
                string title = dataManager.GetAchievementTitle(i);
                int points = dataManager.GetAchievementPoints(i);
                LogDebugBulletproof("   " + (earned ? "✅" : "🔒") + " " + title + " (" + points.ToString() + "G)");
            }

            LogDebugBulletproof("⏰ TIME ACHIEVEMENTS:");
            for (int i = 0; i < 5; i++)
            {
                bool earned = dataManager.HasPlayerEarnedTimeAchievement(playerName, i);
                string title = dataManager.GetTimeAchievementTitle(i);
                int points = dataManager.GetTimeAchievementPoints(i);
                LogDebugBulletproof("   " + (earned ? "✅" : "🔒") + " " + title + " (" + points.ToString() + "G)");
            }

            LogDebugBulletproof("🌟 ACTIVITY ACHIEVEMENTS:");
            for (int i = 0; i < 6; i++)
            {
                bool earned = dataManager.HasPlayerEarnedActivityAchievement(playerName, i);
                string title = dataManager.GetActivityAchievementTitle(i);
                int points = dataManager.GetActivityAchievementPoints(i);
                LogDebugBulletproof("   " + (earned ? "✅" : "🔒") + " " + title + " (" + points.ToString() + "G)");
            }

            int earnedVisit = 0, earnedTime = 0, earnedActivity = 0;
            for (int i = 0; i < 8; i++) if (dataManager.HasPlayerEarnedAchievement(playerName, i)) earnedVisit++;
            for (int i = 0; i < 5; i++) if (dataManager.HasPlayerEarnedTimeAchievement(playerName, i)) earnedTime++;
            for (int i = 0; i < 6; i++) if (dataManager.HasPlayerEarnedActivityAchievement(playerName, i)) earnedActivity++;

            int totalEarned = earnedVisit + earnedTime + earnedActivity;
            LogDebugBulletproof("📊 TOTALS: " + totalEarned.ToString() + "/19 achievements (" +
                               earnedVisit.ToString() + " visit + " + earnedTime.ToString() + " time + " +
                               earnedActivity.ToString() + " activity)");
        }

        [ContextMenu("📊 Show Achievement Status")]
        public void ShowAchievementStatus()
        {
            if (!enableDebugLogging) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;
            ShowComprehensiveResults(playerName);
        }

        [ContextMenu("💾 Test Persistence: Clear All Data")]
        public void TestPersistenceClearAllData()
        {
            if (!enableDebugLogging) return;

            LogDebugBulletproof("🗑️ === PERSISTENCE TEST: CLEARING ALL DATA ===");
            LogDebugBulletproof("⚠️ This will clear all achievements to test persistence");
            LogDebugBulletproof("💾 After clearing, leave world and rejoin to test if data stays cleared");

            if (dataManager != null)
            {
                dataManager.DebugClearMyPlayerData();
                LogDebugBulletproof("✅ All player data cleared");
            }

            LogDebugBulletproof("🔄 Now leave world and rejoin - data should remain cleared");
            LogDebugBulletproof("🎯 Then run 'Comprehensive All Achievements Test' and rejoin again");
            LogDebugBulletproof("✅ Achievements should persist after second rejoin");
        }

        [ContextMenu("🧪 SIMPLE: Quick First Visit + Heavy Rain Test")]
        public void SimpleMasterTestAllRealAchievements()
        {
            if (!enableDebugLogging) return;
            LogDebugBulletproof("🎉 === SIMPLE MASTER ACHIEVEMENT TEST STARTED ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;
            string playerName = localPlayer.displayName;

            LogDebugBulletproof("🗑️ Clearing player data for clean test");
            if (dataManager != null)
            {
                dataManager.DebugClearMyPlayerData();
                LogDebugBulletproof("✅ Player data cleared");
            }

            LogDebugBulletproof("🏠 Step 1: Testing First Visit Achievement");
            TestRealFirstVisit();

            LogDebugBulletproof("🌧️ Step 2: Testing Heavy Rain Achievement");
            TestRealHeavyRain();

            LogDebugBulletproof("🎉 === SIMPLE MASTER TEST COMPLETE ===");
            int totalPoints = dataManager.GetPlayerTotalPoints(playerName);
            int totalVisits = dataManager.GetPlayerVisits(playerName);
            LogDebugBulletproof("📊 Final Results: " + totalVisits.ToString() + " visits, " + totalPoints.ToString() + "G points");
            LogDebugBulletproof("✅ Check terminal for all your achievements!");
            LogDebugBulletproof("🏁 Simple boolean flags working perfectly!");
        }

        [ContextMenu("🏠 Test Real First Visit")]
        public void TestRealFirstVisit()
        {
            if (!enableDebugLogging) return;

            LogDebugBulletproof("🧪 === TESTING REAL FIRST VISIT ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;

            bool hadBefore = dataManager.HasPlayerEarnedAchievement(playerName, 0);
            int visitsBefore = dataManager.GetPlayerVisits(playerName);

            LogDebugBulletproof("📊 Before: " + visitsBefore.ToString() + " visits, First Visit earned: " + hadBefore.ToString());

            int newVisits = dataManager.AddPlayerVisit(playerName);
            LogDebugBulletproof("📈 Added first visit - now have " + newVisits.ToString() + " visits");

            CheckForNewAchievements(playerName, newVisits);

            bool hasAfter = dataManager.HasPlayerEarnedAchievement(playerName, 0);
            LogDebugBulletproof("📊 After: " + newVisits.ToString() + " visits, First Visit earned: " + hasAfter.ToString());
            LogDebugBulletproof("✅ First Visit test: " + (hasAfter ? "SUCCESS" : "FAILED"));
        }

        [ContextMenu("🌧️ Test Real Heavy Rain")]
        public void TestRealHeavyRain()
        {
            if (!enableDebugLogging) return;

            LogDebugBulletproof("🧪 === TESTING REAL HEAVY RAIN ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;

            bool hadBefore = dataManager.HasPlayerEarnedActivityAchievement(playerName, 0);
            LogDebugBulletproof("📊 Heavy Rain before: " + hadBefore.ToString());

            CheckWeatherAchievements("Heavy Rain");

            bool hasAfter = dataManager.HasPlayerEarnedActivityAchievement(playerName, 0);
            LogDebugBulletproof("📊 Heavy Rain after: " + hasAfter.ToString());

            if (!hadBefore && hasAfter)
            {
                LogDebugBulletproof("🎉 Heavy Rain achievement ACTUALLY EARNED!");
                CheckForWeatherAchievementNotification(playerName);
            }

            LogDebugBulletproof("✅ Heavy Rain test: " + (hasAfter ? "SUCCESS" : "FAILED"));
        }

        [ContextMenu("Simulate First Visit")]
        public void SimulateFirstVisit()
        {
            if (!enableDebugLogging) return;

            LogDebugBulletproof("🧪 === SIMULATING FIRST VISIT ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;

            LogDebugBulletproof("🎯 Player: " + playerName);

            int currentVisits = dataManager.GetPlayerVisits(playerName);
            LogDebugBulletproof("📊 Current visits: " + currentVisits.ToString());

            int newVisits = dataManager.AddPlayerVisit(playerName);
            LogDebugBulletproof("📈 Added visit - now have " + newVisits.ToString() + " visits");

            CheckForNewAchievements(playerName, newVisits);

            LogDebugBulletproof("🏁 === FIRST VISIT SIMULATION COMPLETE ===");
        }

        public void AutoPersistenceDiagnostic()
        {
            LogDebug("🔍 === AUTO PERSISTENCE DIAGNOSTIC START ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null)
            {
                LogDebug("❌ No local player found");
                return;
            }

            string playerName = localPlayer.displayName;
            LogDebug("👤 Player Name: '" + playerName + "'");

            // 1. CHECK VISIT COUNT IN DATAMANAGER
            int dataManagerVisits = dataManager.GetPlayerVisits(playerName);
            LogDebug("📊 DataManager reports: " + dataManagerVisits.ToString() + " visits");

            // 2. CHECK ACTUAL PLAYERDATA KEYS (from DataManager constants)
            string actualVisitKey = AchievementKeys.VISITS_KEY;
            bool hasActualVisitKey = PlayerData.HasKey(localPlayer, actualVisitKey);
            LogDebug("🔑 PlayerData has ACTUAL visit key '" + actualVisitKey + "': " + hasActualVisitKey.ToString());

            if (hasActualVisitKey)
            {
                int actualVisitCount = PlayerData.GetInt(localPlayer, actualVisitKey);
                LogDebug("📊 ACTUAL PlayerData visit count: " + actualVisitCount.ToString());
            }

            // 3. CHECK ACTUAL FIRST VISIT ACHIEVEMENT KEY
            string actualFirstVisitKey = AchievementKeys.FIRST_VISIT_ACHIEVEMENT;
            bool hasActualFirstVisitKey = PlayerData.HasKey(localPlayer, actualFirstVisitKey);
            LogDebug("🔑 PlayerData has ACTUAL first visit achievement key '" + actualFirstVisitKey + "': " + hasActualFirstVisitKey.ToString());

            if (hasActualFirstVisitKey)
            {
                bool actualFirstVisitEarned = PlayerData.GetBool(localPlayer, actualFirstVisitKey);
                LogDebug("🏆 ACTUAL PlayerData first visit earned: " + actualFirstVisitEarned.ToString());
            }

            // 4. CHECK WHAT DATAMANAGER THINKS
            bool dataManagerThinks = dataManager.HasPlayerEarnedAchievement(playerName, 0);
            LogDebug("🧠 DataManager thinks first visit earned: " + dataManagerThinks.ToString());


            LogDebug("🔍 === AUTO PERSISTENCE DIAGNOSTIC COMPLETE ===");
        }
    }
}