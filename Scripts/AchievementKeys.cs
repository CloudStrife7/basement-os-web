using UdonSharp;
using UnityEngine;

namespace LowerLevel.Achievements
{
    /// <summary>
    /// COMPONENT PURPOSE:
    /// Centralized PlayerData key constants for the Lower Level 2.0 achievement system.
    /// Single source of truth for all PlayerData persistence keys to prevent duplication and typos.
    ///
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// Provides consistent key naming across all achievement-related scripts.
    /// Eliminates synchronization risk from duplicate hardcoded strings.
    ///
    /// DEPENDENCIES & REQUIREMENTS:
    /// - None - pure constant definitions
    /// - Referenced by: AchievementDataManager.cs, AchievementTracker.cs, DOSTerminalController.cs
    ///
    /// ARCHITECTURE PATTERN:
    /// Constants class with instance arrays - requires GameObject instantiation for array access
    ///
    /// UDONSHARP COMPLIANCE:
    /// ✅ Uses public const (compile-time constants) - fully supported in UdonSharp
    /// ✅ No static fields (not supported) - only const values and instance arrays
    ///
    /// CHANGELOG:
    /// - 11/08/25 - 🆕 CREATED: Centralized all PlayerData keys from AchievementDataManager.cs
    /// - 11/25/25 - 🔧 FIX: Backup system now stores backups outside Unity Assets folder (#120)
    /// </summary>
    public class AchievementKeys : UdonSharpBehaviour
    {
        // =================================================================
        // VISIT TRACKING KEYS
        // =================================================================

        /// <summary>Total visits counter</summary>
        public const string VISITS_KEY = "basement_total_visits";

        /// <summary>First visit date (YYYY-MM-DD format)</summary>
        public const string FIRST_VISIT_KEY = "basement_first_visit_date";

        /// <summary>Last visit date for daily limiting (YYYY-MM-DD format)</summary>
        public const string LAST_VISIT_DATE_KEY = "basement_last_visit_date";

        // =================================================================
        // TIME TRACKING KEYS
        // =================================================================

        /// <summary>Total cumulative time in seconds</summary>
        public const string TOTAL_TIME_KEY = "basement_total_time_seconds";

        /// <summary>Current session start timestamp</summary>
        public const string SESSION_START_KEY = "basement_session_start_time";

        /// <summary>Longest single session in seconds</summary>
        public const string LONGEST_SESSION_KEY = "basement_longest_session_seconds";

        /// <summary>Total number of sessions</summary>
        public const string SESSION_COUNT_KEY = "basement_total_sessions";

        /// <summary>Last completed session length in seconds</summary>
        public const string LAST_SESSION_LENGTH_KEY = "basement_last_session_length";

        // =================================================================
        // ACTIVITY ACHIEVEMENT KEYS (Weather, Time-of-Day, Player Count)
        // =================================================================

        /// <summary>Hour of last visit (0-23) for Night Owl/Early Bird detection</summary>
        public const string LAST_VISIT_HOUR_KEY = "basement_last_visit_hour";

        /// <summary>Heavy Rain achievement earned flag</summary>
        public const string HEAVY_RAIN_EARNED_KEY = "basement_heavy_rain_earned";

        /// <summary>Night Owl achievement earned flag (visit 10pm-4am)</summary>
        public const string NIGHT_OWL_EARNED_KEY = "basement_night_owl_earned";

        /// <summary>Early Bird achievement earned flag (visit before 9am)</summary>
        public const string EARLY_BIRD_EARNED_KEY = "basement_early_bird_earned";

        /// <summary>Weekend Warrior achievement earned flag (visit Sat/Sun)</summary>
        public const string WEEKEND_WARRIOR_EARNED_KEY = "basement_weekend_warrior_earned";

        /// <summary>Streak Master achievement earned flag (7 day streak)</summary>
        public const string STREAK_MASTER_EARNED_KEY = "basement_streak_master_earned";

        /// <summary>Party Animal achievement earned flag (10+ players online)</summary>
        public const string PARTY_ANIMAL_EARNED_KEY = "basement_party_animal_earned";

        // =================================================================
        // STREAK TRACKING KEYS
        // =================================================================

        /// <summary>Current consecutive day visit streak</summary>
        public const string VISIT_STREAK_COUNT_KEY = "basement_visit_streak_count";

        /// <summary>Last date counted for streak (YYYY-MM-DD format)</summary>
        public const string LAST_STREAK_DATE_KEY = "basement_last_streak_date";

        // =================================================================
        // VISIT ACHIEVEMENT KEYS (8 milestones)
        // =================================================================

        /// <summary>First Visit achievement (1 visit) - 0G</summary>
        public const string FIRST_VISIT_ACHIEVEMENT = "basement_first_visit_achievement";

        /// <summary>Return Visitor achievement (5 visits) - 5G</summary>
        public const string REGULAR_VISITOR_ACHIEVEMENT = "basement_regular_visitor_achievement";

        /// <summary>Couch Commander achievement (10 visits) - 10G</summary>
        public const string SHAG_SQUAD_ACHIEVEMENT = "basement_shag_squad_achievement";

        /// <summary>Basement Dweller achievement (25 visits) - 20G</summary>
        public const string BASEMENT_DWELLER_ACHIEVEMENT = "basement_basement_dweller_achievement";

        /// <summary>Retro Regular achievement (50 visits) - 30G</summary>
        public const string RETRO_REGULAR_ACHIEVEMENT = "basement_retro_regular_achievement";

        /// <summary>Shag Squad achievement (75 visits) - 40G</summary>
        public const string HOTTUB_HERO_ACHIEVEMENT = "basement_hottub_hero_achievement";

        /// <summary>Hottub Hero achievement (100 visits) - 50G</summary>
        public const string CENTURY_CLUB_ACHIEVEMENT = "basement_century_club_achievement";

        /// <summary>Lower Legend achievement (250 visits) - 50G</summary>
        public const string LOWER_LEGEND_ACHIEVEMENT = "basement_lower_legend_achievement";

        // =================================================================
        // TIME ACHIEVEMENT KEYS (5 milestones)
        // =================================================================

        /// <summary>Quick Visit achievement (5 minutes) - 5G</summary>
        public const string QUICK_VISIT_EARNED = "basement_quick_visit_earned";

        /// <summary>Hangout achievement (30 minutes) - 10G</summary>
        public const string HANGOUT_EARNED = "basement_hangout_earned";

        /// <summary>Party Time achievement (1 hour) - 20G</summary>
        public const string PARTY_TIME_EARNED = "basement_party_time_earned";

        /// <summary>2 Legit 2 Quit achievement (2 hours) - 30G</summary>
        public const string TWO_LEGIT_TWO_QUIT_EARNED = "basement_2_legit_2_quit_earned";

        /// <summary>Marathon achievement (5 hours) - 50G</summary>
        public const string MARATHON_EARNED = "basement_marathon_earned";

        // =================================================================
        // MOST RECENT ACHIEVEMENT KEY
        // =================================================================

        /// <summary>Most recently earned achievement ID for display</summary>
        public const string MOST_RECENT_ACHIEVEMENT = "basement_most_recent_achievement";

        // =================================================================
        // ARRAY ACCESSORS FOR ITERATION
        // =================================================================

        /// <summary>
        /// Array of visit achievement keys in milestone order (1, 5, 10, 25, 50, 75, 100, 250 visits)
        /// </summary>
        // UDONSHARP FIX (2025-11-13): Changed from static readonly to instance array for UdonSharp compliance
        public string[] VISIT_ACHIEVEMENT_KEYS = {
            FIRST_VISIT_ACHIEVEMENT,
            REGULAR_VISITOR_ACHIEVEMENT,
            SHAG_SQUAD_ACHIEVEMENT,
            BASEMENT_DWELLER_ACHIEVEMENT,
            RETRO_REGULAR_ACHIEVEMENT,
            HOTTUB_HERO_ACHIEVEMENT,
            CENTURY_CLUB_ACHIEVEMENT,
            LOWER_LEGEND_ACHIEVEMENT
        };

        /// <summary>
        /// Array of time achievement keys in milestone order (5min, 30min, 1hr, 2hr, 5hr)
        /// </summary>
        // UDONSHARP FIX (2025-11-13): Changed from static readonly to instance array for UdonSharp compliance
        public string[] TIME_ACHIEVEMENT_KEYS = {
            QUICK_VISIT_EARNED,
            HANGOUT_EARNED,
            PARTY_TIME_EARNED,
            TWO_LEGIT_TWO_QUIT_EARNED,
            MARATHON_EARNED
        };
    }
}