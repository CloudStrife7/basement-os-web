using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;  // ← CRITICAL IMPORT FOR PLAYERDATA
using Varneon.VUdon.Logger.Abstract;

namespace LowerLevel.Achievements
{
    /// <summary>
    /// COMPONENT PURPOSE:
    /// Xbox 360 style achievement data manager using VRChat's PlayerData system directly.
    /// Tracks player visits (daily limited), session time, and achievement progress with authentic point values.
    /// Enhanced with hour detection for Night Owl/Early Bird and anniversary tracking.
    /// 
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// Creates authentic Xbox 360 achievement atmosphere with proper 0G, 5G, 10G point structure
    /// Building social progression system that encourages return visits and longer sessions
    /// Supports nostalgic Xbox Live achievement culture with time-based achievements
    /// 
    /// DEPENDENCIES & REQUIREMENTS:
    /// - VRChat SDK 3.8.2+ for PlayerData system
    /// - VRC.SDK3.Persistence namespace import
    /// - Must be attached to a GameObject in the scene
    /// - No external dependencies - uses VRChat's built-in persistence
    /// 
    /// CHANGELOG:
    /// - 07/19/25 - 🆕 XBOX 360: Updated to authentic Xbox 360 point values (0G-50G)
    /// - 07/19/25 - 🆕 DAILY LIMITING: Added one visit per calendar day maximum
    /// - 07/19/25 - 🆕 HOUR DETECTION: Added visit hour tracking for Night Owl/Early Bird
    /// - 07/19/25 - 🆕 ACTIVITY ACHIEVEMENTS: Added Heavy Rain, Night Owl, Early Bird, etc.
    /// - 07/19/25 - 🆕 ANNIVERSARY: Added anniversary date tracking system
    /// - 07/18/25 - 🆕 TIME TRACKING: Added session time, total time, averages, longest session
    /// - 07/13/25 - 🆕 SIMPLIFIED: Removed complex JSON system, uses PlayerData directly


    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AchievementDataManager : UdonSharpBehaviour
    {
        // =================================================================
        // QUERY/RESULT VARIABLES FOR UDONSHARP CROSS-COMPONENT COMMUNICATION
        // =================================================================
        
        // Query/Result pattern for UdonSharp cross-component communication
        [HideInInspector] public int queryLevel = -1;
        [HideInInspector] public string resultTitle = "";
        [HideInInspector] public int resultPoints = 0;

        [Header("Achievement Notifications")]
        [Tooltip("Reference to AchievementTracker for notifications")]
        [SerializeField] private UdonBehaviour achievementTracker;

        [Header("Achievement Keys")]
        [Tooltip("Reference to AchievementKeys component for accessing achievement key arrays")]
        [SerializeField] private AchievementKeys achievementKeys;

        [Header("Debug Settings")]
        [Tooltip("Enable detailed logging for troubleshooting achievement system")]
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Production Debugging")]
        [Tooltip("Reference to UdonLogger component for in-world console (optional)")]
        [SerializeField] private UdonLogger productionLogger;

        [Header("⏱️ Time Tracking Settings")]
        [Tooltip("How often to update session time (seconds) - 1.0 recommended")]
        [SerializeField] private float timeUpdateInterval = 1.0f;

        [Tooltip("Enable automatic session time tracking")]
        [SerializeField] private bool enableTimeTracking = true;

        [Header("🗓️ Anniversary Settings")]
        [Tooltip("Lower Level 2.0 launch date for anniversary achievement")]
        [SerializeField] private string lowerLevelLaunchDate = "2025-07-19"; // YYYY-MM-DD format

        // =================================================================
        // XBOX 360 ACHIEVEMENT CONFIGURATION - REVISED POINTS
        // Easy to modify achievement levels and point values
        // =================================================================

        // VISIT MILESTONES (8 achievements, 205G)
        private int[] achievementMilestones = { 1, 5, 10, 25, 50, 75, 100, 250 };
        private string[] achievementTitles = {
            "First Visit",
            "Return Visitor",
            "Couch Commander",
            "Basement Dweller",
            "Retro Regular",
            "Shag Squad",
            "Hottub Hero",
            "Lower Legend"
        };
        private int[] achievementPoints = { 0, 5, 10, 20, 30, 40, 50, 50 }; // Xbox 360 authentic points

        // TIME-BASED ACHIEVEMENT MILESTONES (5 achievements, 115G)
        private float[] timeAchievementMilestones = { 300f, 1800f, 3600f, 7200f, 18000f }; // 5min, 30min, 1hr, 2hr, 5hr
        private string[] timeAchievementTitles = {
            "Quick Visit",
            "Hangout",
            "Party Time",
            "2 Legit 2 Quit",
            "Marathon"
        };
        private int[] timeAchievementPoints = { 5, 10, 20, 30, 50 }; // Xbox 360 authentic points

        // ACTIVITY ACHIEVEMENTS (6 achievements, 100G)
        private string[] activityAchievementTitles = {
            "Heavy Rain",
            "Night Owl",
            "Early Bird",
            "Weekend Warrior",
            "Streak Master",
            "Party Animal"
        };
        private int[] activityAchievementPoints = { 10, 10, 10, 15, 20, 25 };

        // =================================================================
        // PLAYERDATA KEYS - CENTRALIZED
        // All PlayerData keys now centralized in AchievementKeys.cs
        // Direct references used throughout this script (e.g., AchievementKeys.VISITS_KEY)
        // This eliminates duplicate key definitions and prevents synchronization errors
        // =================================================================
        // See: AchievementKeys.cs for all key constant definitions

        // =================================================================
        // ⏱️ TIME TRACKING INTERNAL STATE
        // =================================================================

        private bool isInitialized = false;
        private bool sessionActive = false;
        private float sessionStartTime = 0f;
        private float lastTimeUpdate = 0f;

        // =================================================================
        // 💾 DEBOUNCED SAVE SYSTEM
        // =================================================================

        private bool pendingSaveNeeded = false;
        private float saveDelayTimer = 0f;
        private const float SAVE_DELAY = 2.0f;
        private float accumulatedTime = 0f; // Track time locally before saving

        // =================================================================
        // INITIALIZATION - Called when world loads
        // =================================================================

        void Start()
        {
            InitializeComponent();

            // 🔍 AUTO-RUN VISIT SYSTEM DIAGNOSTIC
            // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
            SendCustomEventDelayedSeconds("AutoVisitSystemDiagnostic", 3f);
        }

        protected virtual void InitializeComponent()
        {
            LogDebug("🚀 Achievement Data Manager initializing (Xbox 360 Edition v19)...");

            // UDONSHARP FIX (2025-11-13): Validate AchievementKeys reference
            if (achievementKeys == null)
            {
                Debug.LogError("[AchievementDataManager] AchievementKeys reference not assigned! Please assign in Unity Inspector.");
                return;
            }

            LogDebug("✅ Initialization complete. Using VRChat PlayerData for persistence.");
            isInitialized = true;

            // 🆕 Start time tracking for local player
            if (enableTimeTracking)
            {
                VRCPlayerApi localPlayer = Networking.LocalPlayer;
                if (Utilities.IsValid(localPlayer))
                {
                    StartPlayerSession(localPlayer.displayName);
                    StartTimeUpdateLoop();
                }
            }
        }

        // =================================================================
        // 🆕 DAILY VISIT LIMITING SYSTEM
        // =================================================================

        /// <summary>
        /// SIMPLIFIED: Increments visit count for a player (NO DAILY LIMITING)
        /// Uses PlayerData directly - automatically persists!
        /// Visit count goes up every time, but achievements use persistent flags to prevent duplicates
        /// UdonSharp compliant - no unsupported language features
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>New total visit count for this player</returns>
        public int AddPlayerVisit(string playerName)
        {
            if (!isInitialized)
            {
                LogDebug("⚠️ Data manager not initialized, skipping visit addition");
                return 0;
            }

            // ✅ UDONSHARP COMPLIANCE: Explicit null/empty checks instead of IsNullOrEmpty
            if (playerName == null)
            {
                LogDebug("❌ Player name is null");
                return 0;
            }

            if (playerName == "")
            {
                LogDebug("❌ Player name is empty");
                return 0;
            }

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null)
            {
                LogDebug("❌ Player " + playerName + " not found for visit addition");
                return 0;
            }

            if (!player.isLocal)
            {
                LogDebug("⚠️ Cannot modify data for non-local player " + playerName);
                return GetPlayerVisits(playerName);
            }

            // Get current visits
            int currentVisits = 0;
            if (PlayerData.HasKey(player, AchievementKeys.VISITS_KEY))
            {
                currentVisits = PlayerData.GetInt(player, AchievementKeys.VISITS_KEY);
            }

            // ✅ SIMPLIFIED: Always increment visits (no daily limiting)
            int newVisits = currentVisits + 1;
            PlayerData.SetInt(AchievementKeys.VISITS_KEY, newVisits);

            // Track first visit date if this is their first visit
            if (currentVisits == 0)
            {
                // ✅ UDONSHARP COMPLIANCE: Use string concatenation instead of interpolation
                string todayDate = System.DateTime.Now.ToString("yyyy-MM-dd");
                PlayerData.SetString(AchievementKeys.FIRST_VISIT_KEY, todayDate);
                LogDebug("🆕 First visit recorded for " + playerName + " on " + todayDate);
            }

            // Track visit hour for time-based achievements
            int currentHour = System.DateTime.Now.Hour;
            PlayerData.SetInt(AchievementKeys.LAST_VISIT_HOUR_KEY, currentHour);

            // Start session tracking when visit is added  
            if (enableTimeTracking)
            {
                StartPlayerSession(playerName);
            }

            LogDebug("📈 " + playerName + " visits updated: " + currentVisits.ToString() + " → " + newVisits.ToString());
            LogDebug("🔍 VISIT DEBUG: Method=AddPlayerVisit, Player=" + playerName + ", NewTotal=" + newVisits.ToString() + ", FirstTime=" + (currentVisits == 0).ToString());

            // Add this to verify the VISITS_KEY is being set correctly:
            if (PlayerData.HasKey(player, AchievementKeys.VISITS_KEY))
            {
                int verifyCount = PlayerData.GetInt(player, AchievementKeys.VISITS_KEY);
                LogDebug("✅ VERIFIED: PlayerData[" + AchievementKeys.VISITS_KEY + "] = " + verifyCount.ToString());
            }
            else
            {
                LogDebug("❌ ERROR: PlayerData[" + AchievementKeys.VISITS_KEY + "] not found after setting!");
            }

            return newVisits;
        }

        // =================================================================
        // 🆕 ACTIVITY ACHIEVEMENT SYSTEM
        // =================================================================

        /// <summary>
        /// Checks and potentially awards activity-based achievements
        /// Called when specific conditions are met (weather, player count, etc.)
        /// </summary>
        /// <param name="playerName">Player to check</param>
        /// <param name="activityType">Type of activity to check</param>
        /// <param name="currentCondition">Current condition (weather, player count, etc.)</param>
        public void CheckActivityAchievements(string playerName, string activityType, string currentCondition = "")
        {
            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null || !player.isLocal) return;

            switch (activityType.ToLower())
            {
                case "weather":
                    CheckWeatherAchievement(player, currentCondition);
                    break;
                case "time":
                    CheckTimeBasedAchievements(player);
                    break;
                case "playercount":
                    CheckPlayerCountAchievements(player, currentCondition);
                    break;
                case "weekend":
                    CheckWeekendAchievement(player);
                    break;
            }
        }

        /// <summary>
        /// Checks if the player qualifies for the Heavy Rain activity achievement.
        /// Triggered when weather conditions include rain, storm, or drizzle.
        /// </summary>
        /// <param name="player">VRCPlayerApi reference to the player</param>
        /// <param name="weatherCondition">Current weather condition string</param>
        private void CheckWeatherAchievement(VRCPlayerApi player, string weatherCondition)
        {
            LogDebug($"🔍 CheckWeatherAchievement - Player: {player.displayName}, Condition: {weatherCondition}");

            if (PlayerData.GetBool(player, AchievementKeys.HEAVY_RAIN_EARNED_KEY))
            {
                LogDebug("❌ Heavy Rain already earned, skipping");
                return;
            }

            bool isRaining = weatherCondition.ToLower().Contains("rain") ||
                            weatherCondition.ToLower().Contains("storm") ||
                            weatherCondition.ToLower().Contains("drizzle");

            LogDebug($"🌧️ Is raining check: {isRaining} for condition: '{weatherCondition.ToLower()}'");

            if (isRaining)
            {
                PlayerData.SetBool(AchievementKeys.HEAVY_RAIN_EARNED_KEY, true);
                LogDebug($"🎉 {player.displayName} earned Heavy Rain achievement!");

                // ✅ NEW: Track as most recently earned
                PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "activity_0");
                LogDebug("🕒 Updated most recent achievement: activity_0");
            }
        }

        /// <summary>
        /// Checks if the player qualifies for time-of-day activity achievements.
        /// Checks Night Owl (10 PM - 4 AM) and Early Bird (before 9 AM) achievements.
        /// </summary>
        /// <param name="player">VRCPlayerApi reference to the player</param>
        private void CheckTimeBasedAchievements(VRCPlayerApi player)
        {
            int visitHour = 0;
            if (PlayerData.HasKey(player, AchievementKeys.LAST_VISIT_HOUR_KEY))
            {
                visitHour = PlayerData.GetInt(player, AchievementKeys.LAST_VISIT_HOUR_KEY);
            }

            // Night Owl (10 PM - 4 AM)
            if ((visitHour >= 22 || visitHour <= 4) && !PlayerData.GetBool(player, AchievementKeys.NIGHT_OWL_EARNED_KEY))
            {
                PlayerData.SetBool(AchievementKeys.NIGHT_OWL_EARNED_KEY, true);
                LogDebug($"🦉 {player.displayName} earned Night Owl achievement!");

                // ✅ NEW: Track as most recently earned
                PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "activity_1");
                LogDebug("🕒 Updated most recent achievement: activity_1");
            }

            // Early Bird (before 9:30 AM)
            if (visitHour < 9 && !PlayerData.GetBool(player, AchievementKeys.EARLY_BIRD_EARNED_KEY))
            {
                PlayerData.SetBool(AchievementKeys.EARLY_BIRD_EARNED_KEY, true);
                LogDebug($"🐦 {player.displayName} earned Early Bird achievement!");

                // ✅ NEW: Track as most recently earned
                PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "activity_2");
                LogDebug("🕒 Updated most recent achievement: activity_2");
            }
        }

        /// <summary>
        /// Checks if the player qualifies for the Party Animal achievement (8+ players).
        /// </summary>
        /// <param name="player">VRCPlayerApi reference to the player</param>
        /// <param name="playerCountStr">Current player count as string</param>
        private void CheckPlayerCountAchievements(VRCPlayerApi player, string playerCountStr)
        {
            if (PlayerData.GetBool(player, AchievementKeys.PARTY_ANIMAL_EARNED_KEY)) return; // Already earned

            int playerCount = 0;
            if (int.TryParse(playerCountStr, out playerCount))
            {
                if (playerCount >= 8)
                {
                    PlayerData.SetBool(AchievementKeys.PARTY_ANIMAL_EARNED_KEY, true);
                    LogDebug($"🎉 {player.displayName} earned Party Animal achievement!");

                    // ✅ NEW: Track as most recently earned
                    PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "activity_5");
                    LogDebug("🕒 Updated most recent achievement: activity_5");
                }
            }
        }

        /// <summary>
        /// Checks if the player qualifies for the Weekend Warrior achievement.
        /// Triggered when visiting on Saturday or Sunday.
        /// </summary>
        /// <param name="player">VRCPlayerApi reference to the player</param>
        private void CheckWeekendAchievement(VRCPlayerApi player)
        {
            if (PlayerData.GetBool(player, AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY)) return; // Already earned

            System.DayOfWeek today = System.DateTime.Now.DayOfWeek;
            if (today == System.DayOfWeek.Saturday || today == System.DayOfWeek.Sunday)
            {
                // Check if they visited both days (implementation would need tracking)
                PlayerData.SetBool(AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY, true);
                LogDebug($"📅 {player.displayName} earned Weekend Warrior achievement!");

                // ✅ NEW: Track as most recently earned
                PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "activity_3");
                LogDebug("🕒 Updated most recent achievement: activity_3");
            }
        }

        // =================================================================
        // 🆕 VISIT STREAK SYSTEM
        // =================================================================

        /// <summary>
        /// Updates the visit streak tracking for Streak Master achievement.
        /// Checks if the player has visited on consecutive days.
        /// </summary>
        /// <param name="player">VRCPlayerApi reference to the player</param>
        /// <param name="todayDate">Today's date in YYYY-MM-DD format</param>
        private void UpdateVisitStreak(VRCPlayerApi player, string todayDate)
        {
            string lastStreakDate = "";
            int currentStreak = 0;

            if (PlayerData.HasKey(player, AchievementKeys.LAST_STREAK_DATE_KEY))
            {
                lastStreakDate = PlayerData.GetString(player, AchievementKeys.LAST_STREAK_DATE_KEY);
            }

            if (PlayerData.HasKey(player, AchievementKeys.VISIT_STREAK_COUNT_KEY))
            {
                currentStreak = PlayerData.GetInt(player, AchievementKeys.VISIT_STREAK_COUNT_KEY);
            }

            // Check if this continues the streak
            if (!string.IsNullOrEmpty(lastStreakDate))
            {
                System.DateTime lastDate = System.DateTime.Parse(lastStreakDate);
                System.DateTime today = System.DateTime.Parse(todayDate);

                if ((today - lastDate).Days == 1)
                {
                    // Consecutive day
                    currentStreak++;
                }
                else if ((today - lastDate).Days > 1)
                {
                    // Streak broken
                    currentStreak = 1;
                }
                // Same day visits don't affect streak
            }
            else
            {
                currentStreak = 1; // First visit
            }

            PlayerData.SetInt(AchievementKeys.VISIT_STREAK_COUNT_KEY, currentStreak);
            PlayerData.SetString(AchievementKeys.LAST_STREAK_DATE_KEY, todayDate);

            // Check for Streak Master achievement (7 consecutive days)
            if (currentStreak >= 7 && !PlayerData.HasKey(player, AchievementKeys.STREAK_MASTER_EARNED_KEY))
            {
                PlayerData.SetBool(AchievementKeys.STREAK_MASTER_EARNED_KEY, true);
                LogDebug($"🔥 {player.displayName} earned Streak Master achievement!");

                // ✅ NEW: Track as most recently earned
                PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "activity_4");
                LogDebug("🕒 Updated most recent achievement: activity_4");
            }

            LogDebug($"📅 {player.displayName} visit streak: {currentStreak} days");
        }

        // =================================================================
        // 🆕 ACTIVITY ACHIEVEMENT CHECKING METHODS
        // =================================================================

        /// <summary>
        /// Checks if a player has earned a specific activity achievement.
        /// Activity achievements include: Heavy Rain, Night Owl, Early Bird, Weekend Warrior, Streak Master, Party Animal
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="activityIndex">Activity achievement index (0-5)</param>
        /// <returns>True if the player has earned this activity achievement</returns>
        public bool HasPlayerEarnedActivityAchievement(string playerName, int activityIndex)
        {
            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return false;

            switch (activityIndex)
            {
                case 0: return PlayerData.HasKey(player, AchievementKeys.HEAVY_RAIN_EARNED_KEY) && PlayerData.GetBool(player, AchievementKeys.HEAVY_RAIN_EARNED_KEY);
                case 1: return PlayerData.HasKey(player, AchievementKeys.NIGHT_OWL_EARNED_KEY) && PlayerData.GetBool(player, AchievementKeys.NIGHT_OWL_EARNED_KEY);
                case 2: return PlayerData.HasKey(player, AchievementKeys.EARLY_BIRD_EARNED_KEY) && PlayerData.GetBool(player, AchievementKeys.EARLY_BIRD_EARNED_KEY);
                case 3: return PlayerData.HasKey(player, AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY) && PlayerData.GetBool(player, AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY);
                case 4: return PlayerData.HasKey(player, AchievementKeys.STREAK_MASTER_EARNED_KEY) && PlayerData.GetBool(player, AchievementKeys.STREAK_MASTER_EARNED_KEY);
                case 5: return PlayerData.HasKey(player, AchievementKeys.PARTY_ANIMAL_EARNED_KEY) && PlayerData.GetBool(player, AchievementKeys.PARTY_ANIMAL_EARNED_KEY);
                default: return false;
            }
        }

        /// <summary>
        /// Gets the title of an activity achievement by index.
        /// </summary>
        /// <param name="activityIndex">Activity achievement index (0-5)</param>
        /// <returns>Achievement title, or "Unknown Achievement" if index is invalid</returns>
        public string GetActivityAchievementTitle(int activityIndex)
        {
            if (activityIndex < 0 || activityIndex >= activityAchievementTitles.Length)
                return "Unknown Activity Achievement";
            return activityAchievementTitles[activityIndex];
        }

        /// <summary>
        /// Gets the point value (Gamerscore) of an activity achievement.
        /// </summary>
        /// <param name="activityIndex">Activity achievement index (0-5)</param>
        /// <returns>Point value in Gamerscore (0 if index is invalid)</returns>
        public int GetActivityAchievementPoints(int activityIndex)
        {
            if (activityIndex < 0 || activityIndex >= activityAchievementPoints.Length)
                return 0;
            return activityAchievementPoints[activityIndex];
        }

        /// <summary>
        /// Gets the total number of activity achievements available.
        /// </summary>
        /// <returns>Total activity achievement count (currently 6)</returns>
        public int GetActivityAchievementCount()
        {
            return activityAchievementTitles.Length;
        }

        // =================================================================
        // ⏱️ EXISTING TIME TRACKING METHODS (Unchanged)
        // =================================================================

        /// <summary>
        /// Starts tracking a player's session time.
        /// Increments session count and begins time accumulation for time-based achievements.
        /// Only works for the local player due to PlayerData limitations.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        public void StartPlayerSession(string playerName)
        {
            if (!isInitialized || !enableTimeTracking) return;

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null || !player.isLocal) return;

            sessionStartTime = Time.time;
            sessionActive = true;

            int sessionCount = 0;
            if (PlayerData.HasKey(player, AchievementKeys.SESSION_COUNT_KEY))
            {
                sessionCount = PlayerData.GetInt(player, AchievementKeys.SESSION_COUNT_KEY);
            }
            PlayerData.SetInt(AchievementKeys.SESSION_COUNT_KEY, sessionCount + 1);
            PlayerData.SetFloat(AchievementKeys.SESSION_START_KEY, sessionStartTime);

            LogDebug($"⏱️ Session started for {playerName} at {sessionStartTime:F1}s (Session #{sessionCount + 1})");
        }

        /// <summary>
        /// Initializes the recurring time tracking update loop.
        /// Schedules the first UpdateSessionTime call.
        /// </summary>
        private void StartTimeUpdateLoop()
        {
            if (!enableTimeTracking) return;
            LogDebug("⏱️ Starting time update loop");
            // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
            SendCustomEventDelayedSeconds("UpdateSessionTime", timeUpdateInterval);
        }

        /// <summary>
        /// Handles debounced save operations to batch rapid PlayerData writes.
        /// Batches saves with a 2-second delay to prevent excessive I/O operations.
        /// Called every frame by Unity when pendingSaveNeeded is true.
        /// </summary>
        private void Update()
        {
            if (pendingSaveNeeded)
            {
                saveDelayTimer += Time.deltaTime;

                if (saveDelayTimer >= SAVE_DELAY)
                {
                    FlushPendingSave();
                }
            }
        }

        /// <summary>
        /// Flushes accumulated time data to PlayerData.
        /// Called after debounce delay or on player exit.
        /// </summary>
        private void FlushPendingSave()
        {
            if (!pendingSaveNeeded) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            // Save accumulated time to PlayerData
            if (accumulatedTime > 0f)
            {
                float totalTime = 0f;
                if (PlayerData.HasKey(localPlayer, AchievementKeys.TOTAL_TIME_KEY))
                {
                    totalTime = PlayerData.GetFloat(localPlayer, AchievementKeys.TOTAL_TIME_KEY);
                }
                PlayerData.SetFloat(AchievementKeys.TOTAL_TIME_KEY, totalTime + accumulatedTime);

                LogDebug($"💾 Flushed {accumulatedTime:F1}s to PlayerData (Total: {totalTime + accumulatedTime:F1}s)");
                accumulatedTime = 0f;
            }

            // Reset debounce timer
            pendingSaveNeeded = false;
            saveDelayTimer = 0f;
        }

        public void UpdateSessionTime()
        {
            if (!enableTimeTracking || !sessionActive) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            float currentTime = Time.time;
            float sessionLength = currentTime - sessionStartTime;

            // Accumulate time locally instead of writing to PlayerData every second
            if (currentTime - lastTimeUpdate >= 1.0f)
            {
                accumulatedTime += 1.0f;
                lastTimeUpdate = currentTime;

                // Trigger debounced save
                pendingSaveNeeded = true;
                saveDelayTimer = 0f; // Reset timer for latest update

                // Log every 60 seconds
                if (Mathf.RoundToInt(sessionLength) % 60 == 0)
                {
                    float totalTime = 0f;
                    if (PlayerData.HasKey(localPlayer, AchievementKeys.TOTAL_TIME_KEY))
                    {
                        totalTime = PlayerData.GetFloat(localPlayer, AchievementKeys.TOTAL_TIME_KEY);
                    }
                    LogDebug($"⏱️ Session time: {FormatTimeDisplay(sessionLength)}, Total: {FormatTimeDisplay(totalTime + accumulatedTime)}");
                }
            }

            // 🚀 NEW: Check for newly earned time achievements every update
            CheckForTimeAchievementNotifications(localPlayer.displayName);

            // UDONSHARP FIX (Day 5): Replaced nameof() with string literal
            SendCustomEventDelayedSeconds("UpdateSessionTime", timeUpdateInterval);
        }

        /// <summary>
        /// Ends the current player session and saves session statistics.
        /// Records final session length and updates longest session record if applicable.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        public void EndPlayerSession(string playerName)
        {
            if (!sessionActive || !enableTimeTracking) return;

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null || !player.isLocal) return;

            float sessionLength = Time.time - sessionStartTime;
            PlayerData.SetFloat(AchievementKeys.LAST_SESSION_LENGTH_KEY, sessionLength);

            float longestSession = 0f;
            if (PlayerData.HasKey(player, AchievementKeys.LONGEST_SESSION_KEY))
            {
                longestSession = PlayerData.GetFloat(player, AchievementKeys.LONGEST_SESSION_KEY);
            }

            if (sessionLength > longestSession)
            {
                PlayerData.SetFloat(AchievementKeys.LONGEST_SESSION_KEY, sessionLength);
                LogDebug($"🏆 New longest session record: {FormatTimeDisplay(sessionLength)}!");
            }

            LogDebug($"⏱️ Session ended for {playerName}: {FormatTimeDisplay(sessionLength)}");
            sessionActive = false;
        }

        /// <summary>
        /// Ensures data is saved when the local player leaves the world.
        /// Flushes any pending debounced saves to prevent data loss.
        /// VRChat event override.
        /// </summary>
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player.isLocal && pendingSaveNeeded)
            {
                LogDebug("💾 Player leaving - flushing pending save...");
                FlushPendingSave();
            }
        }

        /// <summary>
        /// Notifies the AchievementTracker to check for newly earned time achievements.
        /// Called during session time updates to trigger achievement notifications.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        private void CheckForTimeAchievementNotifications(string playerName)
        {
            // Call the time achievement notification checker on AchievementTracker
            if (achievementTracker != null)
            {
                // Set the player name for the notification check
                achievementTracker.SetProgramVariable("tempPlayerName", playerName);

                // Trigger the time notification check
                achievementTracker.SendCustomEvent("CheckForTimeBasedAchievementNotifications");
            }
        }

        /// <summary>
        /// Gets the length of the current active session in seconds.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Session time in seconds, or 0 if no active session</returns>
        public float GetCurrentSessionTime(string playerName)
        {
            if (!sessionActive || !enableTimeTracking) return 0f;

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null || !player.isLocal) return 0f;

            return Time.time - sessionStartTime;
        }

        /// <summary>
        /// Gets the total cumulative time a player has spent in the world across all sessions.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Total time in seconds</returns>
        public float GetPlayerTotalTime(string playerName)
        {
            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return 0f;

            if (PlayerData.HasKey(player, AchievementKeys.TOTAL_TIME_KEY))
            {
                return PlayerData.GetFloat(player, AchievementKeys.TOTAL_TIME_KEY);
            }
            return 0f;
        }

        /// <summary>
        /// Calculates the average session length for a player.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Average session length in seconds</returns>
        public float GetAverageSessionLength(string playerName)
        {
            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return 0f;

            float totalTime = GetPlayerTotalTime(playerName);
            int sessionCount = 0;

            if (PlayerData.HasKey(player, AchievementKeys.SESSION_COUNT_KEY))
            {
                sessionCount = PlayerData.GetInt(player, AchievementKeys.SESSION_COUNT_KEY);
            }

            if (sessionCount > 0)
            {
                return totalTime / sessionCount;
            }
            return 0f;
        }

        /// <summary>
        /// Gets the longest single session time for a player.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Longest session time in seconds</returns>
        public float GetLongestSession(string playerName)
        {
            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return 0f;

            if (PlayerData.HasKey(player, AchievementKeys.LONGEST_SESSION_KEY))
            {
                return PlayerData.GetFloat(player, AchievementKeys.LONGEST_SESSION_KEY);
            }
            return 0f;
        }

        /// <summary>
        /// Formats a time value in seconds into a human-readable string.
        /// Format: "Xs" for seconds, "Xm" for minutes, "Xh Ym" for hours and minutes.
        /// </summary>
        /// <param name="totalSeconds">Time value in seconds</param>
        /// <returns>Formatted time string</returns>
        public string FormatTimeDisplay(float totalSeconds)
        {
            if (totalSeconds < 60f)
            {
                return $"{Mathf.RoundToInt(totalSeconds)}s";
            }
            else if (totalSeconds < 3600f)
            {
                int minutes = Mathf.RoundToInt(totalSeconds / 60f);
                return $"{minutes}m";
            }
            else
            {
                int hours = Mathf.FloorToInt(totalSeconds / 3600f);
                int minutes = Mathf.RoundToInt((totalSeconds % 3600f) / 60f);
                return $"{hours}h {minutes}m";
            }
        }

        /// <summary>
        /// Checks if a player has earned a specific time-based achievement.
        /// Time achievements: Quick Visit (5min), Hangout (30min), Party Time (1hr), 2 Legit 2 Quit (2hr), Marathon (5hr).
        /// Automatically saves achievement flag to PlayerData when earned.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <param name="timeAchievementLevel">Time achievement level (0-4)</param>
        /// <returns>True if the player has earned this time achievement</returns>
        public bool HasPlayerEarnedTimeAchievement(string playerName, int timeAchievementLevel)
        {
            if (achievementKeys == null) return false; // Null check added

            if (timeAchievementLevel < 0 || timeAchievementLevel >= timeAchievementMilestones.Length)
                return false;

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return false;

            // ✅ CHECK: If already earned via PlayerData flag
            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
            if (PlayerData.HasKey(player, achievementKeys.TIME_ACHIEVEMENT_KEYS[timeAchievementLevel]))
            {
                if (PlayerData.GetBool(player, achievementKeys.TIME_ACHIEVEMENT_KEYS[timeAchievementLevel]))
                {
                    LogDebug($"⏱️ {playerName} time achievement {timeAchievementLevel}: True (earned via PlayerData flag)");
                    return true;
                }
            }

            // ✅ CHECK: Current session time vs milestone
            float currentSessionTime = GetCurrentSessionTime(playerName);
            float longestSession = GetLongestSession(playerName);
            float bestTime = Mathf.Max(currentSessionTime, longestSession);
            bool hasEarned = bestTime >= timeAchievementMilestones[timeAchievementLevel];

            // ✅ NEW: AUTOMATICALLY SAVE FLAG when earned for the first time
            if (hasEarned)
            {
                // Only save if not already saved (avoid spam)
                if (!PlayerData.HasKey(player, achievementKeys.TIME_ACHIEVEMENT_KEYS[timeAchievementLevel]) ||
                    !PlayerData.GetBool(player, achievementKeys.TIME_ACHIEVEMENT_KEYS[timeAchievementLevel]))
                {
                    PlayerData.SetBool(achievementKeys.TIME_ACHIEVEMENT_KEYS[timeAchievementLevel], true);
                    LogDebug($"💾 AUTO-SAVED time achievement flag: {achievementKeys.TIME_ACHIEVEMENT_KEYS[timeAchievementLevel]} for {playerName}");

                    // ✅ NEW: Track as most recently earned
                    string achievementId = $"time_{timeAchievementLevel}";
                    PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, achievementId);
                    LogDebug($"🕒 Updated most recent achievement: {achievementId}");
                }
            }

            if (hasEarned)
            {
                LogDebug($"⏱️ {playerName} time achievement {timeAchievementLevel}: True (Current: {FormatTimeDisplay(currentSessionTime)}, Longest: {FormatTimeDisplay(longestSession)}, Best: {FormatTimeDisplay(bestTime)} vs {FormatTimeDisplay(timeAchievementMilestones[timeAchievementLevel])} required)");
            }

            return hasEarned;
        }

        /// <summary>
        /// Gets the title of a time-based achievement by level.
        /// </summary>
        /// <param name="timeAchievementLevel">Time achievement level (0-4)</param>
        /// <returns>Achievement title, or "Unknown Achievement" if level is invalid</returns>
        public string GetTimeAchievementTitle(int timeAchievementLevel)
        {
            if (timeAchievementLevel < 0 || timeAchievementLevel >= timeAchievementTitles.Length)
                return "Unknown Time Achievement";
            return timeAchievementTitles[timeAchievementLevel];
        }

        /// <summary>
        /// Gets the point value (Gamerscore) of a time-based achievement.
        /// </summary>
        /// <param name="timeAchievementLevel">Time achievement level (0-4)</param>
        /// <returns>Point value in Gamerscore (0 if level is invalid)</returns>
        public int GetTimeAchievementPoints(int timeAchievementLevel)
        {
            if (timeAchievementLevel < 0 || timeAchievementLevel >= timeAchievementPoints.Length)
                return 0;
            return timeAchievementPoints[timeAchievementLevel];
        }

        /// <summary>
        /// Gets the total number of time-based achievements available.
        /// </summary>
        /// <returns>Total time achievement count (currently 5)</returns>
        public int GetTimeAchievementCount()
        {
            return timeAchievementMilestones.Length;
        }

        // =================================================================
        // EXISTING PUBLIC API METHODS (Updated for Xbox 360 points)
        // =================================================================

        /// <summary>
        /// Gets the total number of visits for a player.
        /// Reads directly from PlayerData using the AchievementKeys.VISITS_KEY.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Total visit count (0 if player not found or no visits recorded)</returns>
        public int GetPlayerVisits(string playerName)
        {
            if (!isInitialized) return 0;

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null)
            {
                LogDebug($"❌ Player {playerName} not found");
                return 0;
            }

            int visits = 0;
            if (PlayerData.HasKey(player, AchievementKeys.VISITS_KEY))
            {
                visits = PlayerData.GetInt(player, AchievementKeys.VISITS_KEY);
            }

            LogDebug($"📊 Player {playerName} visits: {visits}");
            return visits;
        }

        public bool HasPlayerEarnedAchievement(string playerName, int achievementLevel)
        {
            if (achievementKeys == null) return false; // Null check added

            if (achievementLevel < 0 || achievementLevel >= achievementMilestones.Length)
            {
                LogDebug($"❌ Invalid achievement level: {achievementLevel}");
                return false;
            }

            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return false;

            // ✅ CHECK: If already earned via PlayerData flag
            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
            if (PlayerData.HasKey(player, achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel]))
            {
                if (PlayerData.GetBool(player, achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel]))
                {
                    LogDebug($"🏆 {playerName} visit achievement {achievementLevel}: True (earned via PlayerData flag)");
                    return true;
                }
            }

            // ✅ CHECK: Current visit count vs milestone
            int playerVisits = GetPlayerVisits(playerName);
            bool hasEarned = playerVisits >= achievementMilestones[achievementLevel];

            // ✅ NEW: AUTOMATICALLY SAVE FLAG when earned for the first time
            if (hasEarned)
            {
                // Only save if not already saved (avoid spam)
                if (!PlayerData.HasKey(player, achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel]) ||
                    !PlayerData.GetBool(player, achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel]))
                {
                    PlayerData.SetBool(achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel], true);
                    LogDebug($"💾 AUTO-SAVED visit achievement flag: {achievementKeys.VISIT_ACHIEVEMENT_KEYS[achievementLevel]} for {playerName}");

                    // ✅ NEW: Track as most recently earned
                    string achievementId = $"visit_{achievementLevel}";
                    PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, achievementId);
                    LogDebug($"🕒 Updated most recent achievement: {achievementId}");
                }
            }

            LogDebug($"🏆 {playerName} visit achievement {achievementLevel}: {hasEarned} ({playerVisits} visits >= {achievementMilestones[achievementLevel]} required)");
            return hasEarned;
        }

        /// <summary>
        /// Gets the title of a visit-based achievement by level.
        /// Visit achievements: First Visit, Return Visitor, Couch Commander, Basement Dweller,
        /// Retro Regular, Shag Squad, Hottub Hero, Lower Legend.
        /// </summary>
        /// <param name="achievementLevel">Achievement level (0-7)</param>
        /// <returns>Achievement title, or "Unknown Achievement" if level is invalid</returns>
        public string GetAchievementTitle(int achievementLevel)
        {
            if (achievementLevel < 0 || achievementLevel >= achievementTitles.Length)
                return "Unknown Achievement";
            return achievementTitles[achievementLevel];
        }

        /// <summary>
        /// Gets the point value (Gamerscore) of a visit-based achievement.
        /// Points follow Xbox 360 authentic structure: 0G, 5G, 10G, 20G, 30G, 40G, 50G, 50G.
        /// </summary>
        /// <param name="achievementLevel">Achievement level (0-7)</param>
        /// <returns>Point value in Gamerscore (0 if level is invalid)</returns>
        public int GetAchievementPoints(int achievementLevel)
        {
            if (achievementLevel < 0 || achievementLevel >= achievementPoints.Length)
                return 0;
            return achievementPoints[achievementLevel];
        }

        /// <summary>
        /// Gets the number of visits required to earn a specific visit-based achievement.
        /// Milestones: 1, 5, 10, 25, 50, 75, 100, 250 visits.
        /// </summary>
        /// <param name="achievementLevel">Achievement level (0-7)</param>
        /// <returns>Required visit count, or int.MaxValue if level is invalid</returns>
        public int GetRequiredVisits(int achievementLevel)
        {
            if (achievementLevel < 0 || achievementLevel >= achievementMilestones.Length)
                return int.MaxValue; // Invalid level

            return achievementMilestones[achievementLevel];
        }


        /// <summary>
        /// Calculates the total Gamerscore for a player across all achievement categories.
        /// Includes visit-based (8), time-based (5), and activity-based (6) achievements.
        /// Maximum possible: 420G total.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Total Gamerscore points earned</returns>
        public int GetPlayerTotalPoints(string playerName)
        {
            int totalPoints = 0;

            // Visit-based achievements
            for (int i = 0; i < achievementMilestones.Length; i++)
            {
                if (HasPlayerEarnedAchievement(playerName, i))
                {
                    totalPoints += achievementPoints[i];
                }
            }

            // Time-based achievements
            for (int i = 0; i < timeAchievementMilestones.Length; i++)
            {
                if (HasPlayerEarnedTimeAchievement(playerName, i))
                {
                    totalPoints += timeAchievementPoints[i];
                }
            }

            // Activity-based achievements
            for (int i = 0; i < activityAchievementTitles.Length; i++)
            {
                if (HasPlayerEarnedActivityAchievement(playerName, i))
                {
                    totalPoints += activityAchievementPoints[i];
                }
            }

            LogDebug($"🎯 {playerName} total points: {totalPoints}G (visits + time + activity achievements)");
            return totalPoints;
        }

        /// <summary>
        /// Gets the total number of achievements available across all categories.
        /// </summary>
        /// <returns>Total achievement count (currently 19: 8 visit + 5 time + 6 activity)</returns>
        public int GetTotalAchievementCount()
        {
            return achievementMilestones.Length + timeAchievementMilestones.Length + activityAchievementTitles.Length;
        }

        /// <summary>
        /// Gets the display names of all currently connected players in the world.
        /// Used for leaderboards and statistics display.
        /// </summary>
        /// <returns>Array of player display names</returns>
        public string[] GetAllPlayerNames()
        {
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(allPlayers);

            string[] playerNames = new string[allPlayers.Length];
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i] != null && allPlayers[i].IsValid())
                {
                    playerNames[i] = allPlayers[i].displayName;
                }
                else
                {
                    playerNames[i] = "Unknown";
                }
            }

            LogDebug($"👥 Returning {playerNames.Length} currently connected players");
            return playerNames;
        }

        /// <summary>
        /// Gets the date of a player's first visit to the world.
        /// Date is stored in YYYY-MM-DD format when the first visit is recorded.
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>First visit date string, or "Unknown" if not recorded</returns>
        public string GetPlayerFirstVisitDate(string playerName)
        {
            VRCPlayerApi player = GetPlayerByName(playerName);
            if (player == null) return "Unknown";

            if (PlayerData.HasKey(player, AchievementKeys.FIRST_VISIT_KEY))
            {
                return PlayerData.GetString(player, AchievementKeys.FIRST_VISIT_KEY);
            }

            return "Unknown";
        }

        // =================================================================
        // HELPER METHODS
        // =================================================================

        private VRCPlayerApi GetPlayerByName(string playerName)
        {
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(allPlayers);

            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i] != null && allPlayers[i].IsValid() &&
                    allPlayers[i].displayName == playerName)
                {
                    return allPlayers[i];
                }
            }

            return null;
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
                string coloredMessage = "<color=#FFD700>🏆 [AchievementDataManager] " + message + "</color>";

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
        // DEBUG AND UTILITY METHODS
        // =================================================================

        /// <summary>
        /// [DEBUG] Displays all PlayerData for the local player in the debug log.
        /// Useful for troubleshooting achievement tracking and progression.
        /// </summary>
        [ContextMenu("Debug - Show My PlayerData")]
        public void DebugShowMyPlayerData()
        {
            if (!enableDebugLogging) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            LogDebug("🔍 === MY PLAYERDATA DEBUG ===");
            LogDebug($"👤 Player: {localPlayer.displayName}");

            if (PlayerData.HasKey(localPlayer, AchievementKeys.VISITS_KEY))
            {
                int visits = PlayerData.GetInt(localPlayer, AchievementKeys.VISITS_KEY);
                LogDebug($"📊 Basement Visits: {visits}");
            }

            if (PlayerData.HasKey(localPlayer, AchievementKeys.FIRST_VISIT_KEY))
            {
                string firstVisit = PlayerData.GetString(localPlayer, AchievementKeys.FIRST_VISIT_KEY);
                LogDebug($"📅 First Visit: {firstVisit}");
            }

            LogDebug($"🎯 Total Points: {GetPlayerTotalPoints(localPlayer.displayName)}G");
            LogDebug("🏁 === END DEBUG ===");
        }

        /// <summary>
        /// [DEBUG] Manually adds a test visit for the local player.
        /// Useful for testing achievement unlocking without actually rejoining the world.
        /// </summary>
        [ContextMenu("Debug - Add Test Visit")]
        public void DebugAddTestVisit()
        {
            if (!enableDebugLogging) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            LogDebug("🧪 === ADDING TEST VISIT ===");
            int newVisits = AddPlayerVisit(localPlayer.displayName);
            LogDebug($"✅ Test visit added. New total: {newVisits}");
        }

        /// <summary>
        /// [DEBUG] Clears all PlayerData for the local player, resetting all progress.
        /// Resets visits, time tracking, and all achievement flags. Use with caution!
        /// </summary>
        [ContextMenu("Debug - Clear My PlayerData")]
        public void DebugClearMyPlayerData()
        {
            if (!enableDebugLogging) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            LogDebug("🗑️ === CLEARING MY PLAYERDATA ===");

            PlayerData.SetInt(AchievementKeys.VISITS_KEY, 0);
            PlayerData.SetString(AchievementKeys.FIRST_VISIT_KEY, "");
            PlayerData.SetString(AchievementKeys.LAST_VISIT_DATE_KEY, "");

            // Clear time tracking
            PlayerData.SetFloat(AchievementKeys.TOTAL_TIME_KEY, 0f);
            PlayerData.SetFloat(AchievementKeys.LONGEST_SESSION_KEY, 0f);
            PlayerData.SetInt(AchievementKeys.SESSION_COUNT_KEY, 0);

            // Clear activity achievements
            PlayerData.SetBool(AchievementKeys.HEAVY_RAIN_EARNED_KEY, false);
            PlayerData.SetBool(AchievementKeys.NIGHT_OWL_EARNED_KEY, false);
            PlayerData.SetBool(AchievementKeys.EARLY_BIRD_EARNED_KEY, false);
            PlayerData.SetBool(AchievementKeys.WEEKEND_WARRIOR_EARNED_KEY, false);
            PlayerData.SetBool(AchievementKeys.STREAK_MASTER_EARNED_KEY, false);
            PlayerData.SetBool(AchievementKeys.PARTY_ANIMAL_EARNED_KEY, false);

            // Clear time achievements
            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
            if (achievementKeys != null)
            {
                for (int i = 0; i < achievementKeys.TIME_ACHIEVEMENT_KEYS.Length; i++)
                {
                    PlayerData.SetBool(achievementKeys.TIME_ACHIEVEMENT_KEYS[i], false);
                }

                // Clear visit achievement flags
                // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
                for (int i = 0; i < achievementKeys.VISIT_ACHIEVEMENT_KEYS.Length; i++)
                {
                    PlayerData.SetBool(achievementKeys.VISIT_ACHIEVEMENT_KEYS[i], false);
                }
            }

            // ✅ NEW: Clear recent achievement tracking
            PlayerData.SetString(AchievementKeys.MOST_RECENT_ACHIEVEMENT, "");

            sessionActive = false;
            LogDebug("✅ PlayerData cleared for testing");

        }
        /// <summary>
        /// [DEBUG] Tests the auto-saving mechanism for all achievement types.
        /// Iterates through visit, time, and activity achievements to verify flags are saved correctly.
        /// </summary>
        [ContextMenu("🧪 Test Auto-Saving for All Achievement Types")]
        public void TestAutoSavingForAllAchievements()
        {
            if (!enableDebugLogging) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(localPlayer)) return;

            string playerName = localPlayer.displayName;
            LogDebug("🧪 === TESTING AUTO-SAVING FOR ALL ACHIEVEMENT TYPES ===");

            // Test visit achievements
            LogDebug("📊 Testing visit achievement auto-saving...");
            for (int i = 0; i < achievementMilestones.Length; i++)
            {
                bool earned = HasPlayerEarnedAchievement(playerName, i);
                LogDebug($"   Visit {i}: {earned}");
            }

            // Test time achievements  
            LogDebug("⏱️ Testing time achievement auto-saving...");
            for (int i = 0; i < timeAchievementMilestones.Length; i++)
            {
                bool earned = HasPlayerEarnedTimeAchievement(playerName, i);
                LogDebug($"   Time {i}: {earned}");
            }

            // Test activity achievements
            LogDebug("🎯 Testing activity achievement flags...");
            for (int i = 0; i < activityAchievementTitles.Length; i++)
            {
                bool earned = HasPlayerEarnedActivityAchievement(playerName, i);
                LogDebug($"   Activity {i}: {earned}");
            }

            LogDebug("✅ Auto-saving test complete!");
        }

        /// <summary>
        /// Auto-running diagnostic to verify simplified visit system is working correctly
        /// UdonSharp compliant - no unsupported language features
        /// </summary>
        public void AutoVisitSystemDiagnostic()
        {
            LogDebug("🔍 === AUTO VISIT SYSTEM DIAGNOSTIC START ===");

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null)
            {
                LogDebug("❌ No local player found for diagnostic");
                return;
            }

            string playerName = localPlayer.displayName;
            LogDebug("👤 Diagnostic Player: " + playerName);

            // 1. CHECK CURRENT VISIT COUNT
            int currentVisits = GetPlayerVisits(playerName);
            LogDebug("📊 Current visit count from GetPlayerVisits(): " + currentVisits.ToString());

            // 2. CHECK RAW PLAYERDATA VISIT COUNT
            int rawVisits = 0;
            if (PlayerData.HasKey(localPlayer, AchievementKeys.VISITS_KEY))
            {
                rawVisits = PlayerData.GetInt(localPlayer, AchievementKeys.VISITS_KEY);
                LogDebug("📊 Raw PlayerData visit count: " + rawVisits.ToString());
            }
            else
            {
                LogDebug("🔑 No PlayerData visit key found - this is expected for first-time users");
            }

            // 3. CHECK FIRST VISIT DATE
            if (PlayerData.HasKey(localPlayer, AchievementKeys.FIRST_VISIT_KEY))
            {
                string firstVisitDate = PlayerData.GetString(localPlayer, AchievementKeys.FIRST_VISIT_KEY);
                LogDebug("📅 First visit date: " + firstVisitDate);
            }
            else
            {
                LogDebug("📅 No first visit date recorded");
            }

            // 4. CHECK VISIT HOUR TRACKING
            if (PlayerData.HasKey(localPlayer, AchievementKeys.LAST_VISIT_HOUR_KEY))
            {
                int lastVisitHour = PlayerData.GetInt(localPlayer, AchievementKeys.LAST_VISIT_HOUR_KEY);
                LogDebug("🕐 Last visit hour: " + lastVisitHour.ToString());
            }
            else
            {
                LogDebug("🕐 No visit hour recorded");
            }

            // 5. CHECK ACHIEVEMENT FLAGS (first 3 as examples)
            // UDONSHARP FIX (2025-11-13): Changed from static to instance array access
            LogDebug("🏆 Achievement flag status (first 3):");
            if (achievementKeys != null)
            {
                int checkCount = Mathf.Min(3, achievementKeys.VISIT_ACHIEVEMENT_KEYS.Length);
                for (int i = 0; i < checkCount; i++)
                {
                    if (PlayerData.HasKey(localPlayer, achievementKeys.VISIT_ACHIEVEMENT_KEYS[i]))
                    {
                        bool flagValue = PlayerData.GetBool(localPlayer, achievementKeys.VISIT_ACHIEVEMENT_KEYS[i]);
                        LogDebug("   " + achievementKeys.VISIT_ACHIEVEMENT_KEYS[i] + ": " + flagValue.ToString());
                    }
                    else
                    {
                        LogDebug("   " + achievementKeys.VISIT_ACHIEVEMENT_KEYS[i] + ": [not set]");
                    }
                }
            }

            // 6. SYSTEM STATUS CHECK
            LogDebug("⚙️ System Status:");
            LogDebug("   isInitialized: " + isInitialized.ToString());
            LogDebug("   enableTimeTracking: " + enableTimeTracking.ToString());
            LogDebug("   sessionActive: " + sessionActive.ToString());

            // 7. NEXT VISIT PREDICTION
            int nextVisitCount = currentVisits + 1;
            LogDebug("🔮 Next visit will be #" + nextVisitCount.ToString());

            // Check which achievement would be earned
            int[] milestones = { 1, 5, 10, 25, 50, 75, 100, 250 };
            for (int i = 0; i < milestones.Length; i++)
            {
                if (nextVisitCount == milestones[i])
                {
                    string achievementTitle = GetAchievementTitle(i);
                    LogDebug("🎯 Next visit will earn: " + achievementTitle + " (" + GetAchievementPoints(i).ToString() + "G)");
                    break;
                }
            }

            LogDebug("🔍 === VISIT SYSTEM DIAGNOSTIC COMPLETE ===");
            LogDebug("✅ Deploy this diagnostic to verify the simplified visit system works correctly!");
        }

        // =================================================================
        // UDONSHARP CROSS-COMPONENT COMMUNICATION GETTER METHODS
        // =================================================================

        /// <summary>
        /// Gets visit achievement title for queryLevel
        /// Sets resultTitle with the achievement name
        /// </summary>
        public void GetAchievementTitle()
        {
            if (queryLevel < 0 || queryLevel >= achievementTitles.Length)
            {
                resultTitle = "Unknown Achievement";
                LogDebug($"❌ GetAchievementTitle: Invalid level {queryLevel}");
                return;
            }
            
            resultTitle = achievementTitles[queryLevel];
            LogDebug($"✅ GetAchievementTitle: Level {queryLevel} = {resultTitle}");
        }

        /// <summary>
        /// Gets visit achievement points for queryLevel
        /// Sets resultPoints with the gamerscore value
        /// </summary>
        public void GetAchievementPoints()
        {
            if (queryLevel < 0 || queryLevel >= achievementPoints.Length)
            {
                resultPoints = 0;
                LogDebug($"❌ GetAchievementPoints: Invalid level {queryLevel}");
                return;
            }
            
            resultPoints = achievementPoints[queryLevel];
            LogDebug($"✅ GetAchievementPoints: Level {queryLevel} = {resultPoints}G");
        }

        /// <summary>
        /// Gets time achievement title for queryLevel
        /// Sets resultTitle with the achievement name
        /// </summary>
        public void GetTimeAchievementTitle()
        {
            if (queryLevel < 0 || queryLevel >= timeAchievementTitles.Length)
            {
                resultTitle = "Unknown Time Achievement";
                LogDebug($"❌ GetTimeAchievementTitle: Invalid level {queryLevel}");
                return;
            }
            
            resultTitle = timeAchievementTitles[queryLevel];
            LogDebug($"✅ GetTimeAchievementTitle: Level {queryLevel} = {resultTitle}");
        }

        /// <summary>
        /// Gets time achievement points for queryLevel
        /// Sets resultPoints with the gamerscore value
        /// </summary>
        public void GetTimeAchievementPoints()
        {
            if (queryLevel < 0 || queryLevel >= timeAchievementPoints.Length)
            {
                resultPoints = 0;
                LogDebug($"❌ GetTimeAchievementPoints: Invalid level {queryLevel}");
                return;
            }
            
            resultPoints = timeAchievementPoints[queryLevel];
            LogDebug($"✅ GetTimeAchievementPoints: Level {queryLevel} = {resultPoints}G");
        }

        /// <summary>
        /// Gets activity achievement title for queryLevel
        /// Sets resultTitle with the achievement name
        /// </summary>
        public void GetActivityAchievementTitle()
        {
            if (queryLevel < 0 || queryLevel >= activityAchievementTitles.Length)
            {
                resultTitle = "Unknown Activity Achievement";
                LogDebug($"❌ GetActivityAchievementTitle: Invalid level {queryLevel}");
                return;
            }
            
            resultTitle = activityAchievementTitles[queryLevel];
            LogDebug($"✅ GetActivityAchievementTitle: Level {queryLevel} = {resultTitle}");
        }

        /// <summary>
        /// Gets activity achievement points for queryLevel
        /// Sets resultPoints with the gamerscore value
        /// </summary>
        public void GetActivityAchievementPoints()
        {
            if (queryLevel < 0 || queryLevel >= activityAchievementPoints.Length)
            {
                resultPoints = 0;
                LogDebug($"❌ GetActivityAchievementPoints: Invalid level {queryLevel}");
                return;
            }
            
            resultPoints = activityAchievementPoints[queryLevel];
            LogDebug($"✅ GetActivityAchievementPoints: Level {queryLevel} = {resultPoints}G");
        }
    }
}