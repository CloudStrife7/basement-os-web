using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Varneon.VUdon.Logger.Abstract;

namespace LowerLevel.Notifications
{
    /// <summary>
    /// Role enumeration for easy identification
    /// </summary>
    public enum UserRole
    {
        Regular = 0,
        Supporter = 1,
        FoundingMember = 2,
        Architect = 3,
        Bobby = 4,
        Peter = 5,
        Lexx = 6,
        Supa = 7,
        Pwnerer = 8
    }

    /// <summary>
    /// COMPONENT PURPOSE:
    /// Manages user roles and provides role-specific notification customization for Xbox notifications
    /// Determines notification styling, sounds, and text based on player's assigned role
    /// Integrates with XboxNotificationUI to provide enhanced notifications for special users
    /// 
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// Creates VIP recognition system that honors project supporters and core community members
    /// Enhances the nostalgic Xbox Live atmosphere with role-based status recognition
    /// Builds community hierarchy that encourages engagement and celebrates contributions
    /// 
    /// DEPENDENCIES & REQUIREMENTS:
    /// - XboxNotificationUI component for notification display
    /// - AudioSource component for role-specific sound effects
    /// - Role-specific sound clips assigned in Inspector
    /// - Username lists configured for each role tier
    /// - Optional: Custom notification icons for each role
    /// 
    /// ARCHITECTURE PATTERN:
    /// Service Provider pattern - XboxNotificationUI queries this module for role information
    /// Data-driven configuration through Inspector arrays for easy role management
    /// Fallback system ensures unknown users get default "Regular" role treatment
    /// Quest-optimized with minimal memory footprint and no Update() loops
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NotificationRolesModule : UdonSharpBehaviour
    {
        [Header("Debug Settings")]
        [Tooltip("Enable detailed role detection logging for troubleshooting")]
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Production Debugging")]
        [Tooltip("Reference to UdonLogger component for in-world console (optional)")]
        [SerializeField] private UdonLogger productionLogger;

        [Header("Integration")]
        [Tooltip("Reference to XboxNotificationUI for direct queue integration")]
        [SerializeField] private XboxNotificationUI notificationUI;

        [Header("Role Configuration")]
        [Tooltip("Usernames for the project owner - highest priority role")]
        [SerializeField] private string[] pwnererUsernames = { };

        [Tooltip("Usernames for Supa role - high priority community members")]
        [SerializeField] private string[] supaUsernames = { };

        [Tooltip("Usernames for Lexx role - special recognition")]
        [SerializeField] private string[] lexxUsernames = { };

        [Tooltip("Usernames for Peter role - special recognition")]
        [SerializeField] private string[] peterUsernames = { };

        [Tooltip("Usernames for Bobby role - special recognition")]
        [SerializeField] private string[] bobbyUsernames = { };

        [Tooltip("Usernames for Architect role - development contributors")]
        [SerializeField] private string[] architectUsernames = { };

        [Tooltip("Usernames for Founding Members - early community supporters")]
        [SerializeField] private string[] foundingMemberUsernames = { };

        [Tooltip("Usernames for Supporters - financial/community contributors")]
        [SerializeField] private string[] supporterUsernames = { };

        [Header("Role Audio")]
        [Tooltip("Sound effect for Pwnerer notifications - most prestigious")]
        [SerializeField] private AudioClip pwnererSound;

        [Tooltip("Sound effect for Supa role notifications")]
        [SerializeField] private AudioClip supaSound;

        [Tooltip("Sound effect for Lexx role notifications")]
        [SerializeField] private AudioClip lexxSound;

        [Tooltip("Sound effect for Peter role notifications")]
        [SerializeField] private AudioClip peterSound;

        [Tooltip("Sound effect for Bobby role notifications")]
        [SerializeField] private AudioClip bobbySound;

        [Tooltip("Sound effect for Architect role notifications")]
        [SerializeField] private AudioClip architectSound;

        [Tooltip("Sound effect for Founding Member notifications")]
        [SerializeField] private AudioClip foundingMemberSound;

        [Tooltip("Sound effect for Supporter notifications")]
        [SerializeField] private AudioClip supporterSound;

        [Tooltip("Default sound for Regular users")]
        [SerializeField] private AudioClip regularSound;

        [Header("Role Display Names")]
        [Tooltip("Custom display names for each role - shown in notifications")]
        [SerializeField] private string pwnererDisplayName = "Pwnerer";
        [SerializeField] private string supaDisplayName = "Supa";
        [SerializeField] private string lexxDisplayName = "Lexx";
        [SerializeField] private string peterDisplayName = "Peter";
        [SerializeField] private string bobbyDisplayName = "Bobby";
        [SerializeField] private string architectDisplayName = "Architect";
        [SerializeField] private string foundingMemberDisplayName = "Founding Member";
        [SerializeField] private string supporterDisplayName = "Supporter";
        [SerializeField] private string regularDisplayName = ""; // Empty = no role prefix

        // Component state
        private bool isInitialized = false;

        /// <summary>
        /// Initialize the roles module
        /// Validates configuration and prepares role lookup system
        /// </summary>
        private void Start()
        {
            InitializeRolesModule();
        }

        /// <summary>
        /// Initialize role management system
        /// Validates that required arrays are configured
        /// Sets up role detection for optimal performance
        /// </summary>
        private void InitializeRolesModule()
        {
            LogDebug("NotificationRolesModule initializing...");

            // Validate configuration
            int totalConfiguredUsers = pwnererUsernames.Length + supaUsernames.Length +
                                     lexxUsernames.Length + peterUsernames.Length +
                                     bobbyUsernames.Length + architectUsernames.Length +
                                     foundingMemberUsernames.Length + supporterUsernames.Length;

            LogDebug($"Total configured special users: {totalConfiguredUsers}");

            if (totalConfiguredUsers == 0)
            {
                LogDebug("Warning: No special users configured. All users will be treated as Regular.");
            }

            isInitialized = true;
            LogDebug("NotificationRolesModule initialized successfully");
        }

        /// <summary>
        /// Determine a player's role based on their VRChat username
        /// Uses hierarchical role system - higher roles take precedence
        /// Returns UserRole enum for easy role identification
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>UserRole enum representing the player's assigned role</returns>
        public UserRole GetPlayerRole(string playerName)
        {
            if (!isInitialized || string.IsNullOrEmpty(playerName))
            {
                LogDebug($"Cannot determine role - module not initialized or invalid player name: '{playerName}'");
                return UserRole.Regular;
            }

            // Check roles in priority order (highest to lowest)

            // Pwnerer (Project Owner) - Highest Priority
            if (IsPlayerInRole(playerName, pwnererUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Pwnerer");
                return UserRole.Pwnerer;
            }

            // Supa (High Priority Community Members)
            if (IsPlayerInRole(playerName, supaUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Supa");
                return UserRole.Supa;
            }

            // Named Roles (Special Recognition)
            if (IsPlayerInRole(playerName, lexxUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Lexx");
                return UserRole.Lexx;
            }

            if (IsPlayerInRole(playerName, peterUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Peter");
                return UserRole.Peter;
            }

            if (IsPlayerInRole(playerName, bobbyUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Bobby");
                return UserRole.Bobby;
            }

            // Architect (Development Contributors)
            if (IsPlayerInRole(playerName, architectUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Architect");
                return UserRole.Architect;
            }

            // Founding Member (Early Supporters)
            if (IsPlayerInRole(playerName, foundingMemberUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Founding Member");
                return UserRole.FoundingMember;
            }

            // Supporter (Contributors)
            if (IsPlayerInRole(playerName, supporterUsernames))
            {
                LogDebug($"Player '{playerName}' identified as Supporter");
                return UserRole.Supporter;
            }

            // Default to Regular user
            LogDebug($"Player '{playerName}' identified as Regular user");
            return UserRole.Regular;
        }

        /// <summary>
        /// Get the appropriate sound effect for a player's role
        /// Returns null if no specific sound is configured (falls back to default)
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>AudioClip for the player's role, or null for default sound</returns>
        public AudioClip GetRoleSound(string playerName)
        {
            UserRole role = GetPlayerRole(playerName);
            return GetRoleSoundByEnum(role);
        }

        /// <summary>
        /// Get sound effect by role enum
        /// Provides direct access to role sounds for performance
        /// </summary>
        /// <param name="role">UserRole enum</param>
        /// <returns>AudioClip for the specified role</returns>
        public AudioClip GetRoleSoundByEnum(UserRole role)
        {
            switch (role)
            {
                case UserRole.Pwnerer: return pwnererSound != null ? pwnererSound : regularSound;
                case UserRole.Supa: return supaSound != null ? supaSound : regularSound;
                case UserRole.Lexx: return lexxSound != null ? lexxSound : regularSound;
                case UserRole.Peter: return peterSound != null ? peterSound : regularSound;
                case UserRole.Bobby: return bobbySound != null ? bobbySound : regularSound;
                case UserRole.Architect: return architectSound != null ? architectSound : regularSound;
                case UserRole.FoundingMember: return foundingMemberSound != null ? foundingMemberSound : regularSound;
                case UserRole.Supporter: return supporterSound != null ? supporterSound : regularSound;
                case UserRole.Regular:
                default: return regularSound;
            }
        }

        /// <summary>
        /// Get the display name for a player's role
        /// Returns empty string for Regular users (no role prefix)
        /// </summary>
        /// <param name="playerName">VRChat display name of the player</param>
        /// <returns>Role display name or empty string for regular users</returns>
        public string GetRoleDisplayName(string playerName)
        {
            UserRole role = GetPlayerRole(playerName);
            return GetRoleDisplayNameByEnum(role);
        }

        /// <summary>
        /// Get role display name by enum
        /// Used for consistent role naming across the system
        /// </summary>
        /// <param name="role">UserRole enum</param>
        /// <returns>Human-readable role name</returns>
        public string GetRoleDisplayNameByEnum(UserRole role)
        {
            switch (role)
            {
                case UserRole.Pwnerer: return pwnererDisplayName;
                case UserRole.Supa: return supaDisplayName;
                case UserRole.Lexx: return lexxDisplayName;
                case UserRole.Peter: return peterDisplayName;
                case UserRole.Bobby: return bobbyDisplayName;
                case UserRole.Architect: return architectDisplayName;
                case UserRole.FoundingMember: return foundingMemberDisplayName;
                case UserRole.Supporter: return supporterDisplayName;
                case UserRole.Regular:
                default: return regularDisplayName; // Empty string for regular users
            }
        }

        /// <summary>
        /// Queue an achievement notification with automatic role detection
        /// Determines the appropriate notification type based on player's role
        /// Uses your existing queue system with proper priority handling
        /// </summary>
        /// <param name="playerName">VRChat display name</param>
        /// <param name="achievementTitle">Achievement title (e.g., "Basement Dwellers")</param>
        /// <param name="points">Achievement point value</param>
        public void QueueAchievementWithRole(string playerName, string achievementTitle, int points)
        {
            if (notificationUI == null)
            {
                LogDebug("Cannot queue notification - XboxNotificationUI reference not set");
                return;
            }

            UserRole role = GetPlayerRole(playerName);
            LogDebug($"Queueing achievement for {playerName} with role: {role}");

            switch (role)
            {
                case UserRole.Pwnerer:
                    notificationUI.QueuePwnererNotification(playerName, achievementTitle, points);
                    break;

                case UserRole.Supporter:
                case UserRole.FoundingMember:
                case UserRole.Architect:
                    notificationUI.QueueSupporterNotification(playerName, achievementTitle, points);
                    break;

                case UserRole.Supa:
                case UserRole.Lexx:
                case UserRole.Peter:
                case UserRole.Bobby:
                    // These special named roles use supporter-level notifications
                    notificationUI.QueueSupporterNotification(playerName, achievementTitle, points);
                    break;

                case UserRole.Regular:
                default:
                    notificationUI.QueueAchievementNotification(playerName, achievementTitle, points);
                    break;
            }
        }

        /// <summary>
        /// Queue an online notification with automatic role detection
        /// Handles both first-time and returning player notifications
        /// </summary>
        /// <param name="playerName">VRChat display name</param>
        /// <param name="isFirstTime">True if this is the player's first visit</param>
        public void QueueOnlineWithRole(string playerName, bool isFirstTime)
        {
            if (notificationUI == null)
            {
                LogDebug("Cannot queue notification - XboxNotificationUI reference not set");
                return;
            }

            UserRole role = GetPlayerRole(playerName);
            LogDebug($"Queueing online notification for {playerName} with role: {role} (First time: {isFirstTime})");

            // Use the existing online notification system
            // Role-specific styling can be handled through text formatting if needed
            notificationUI.QueueOnlineNotification(playerName, isFirstTime);
        }

        /// <summary>
        /// Check if a player belongs to a specific role array
        /// Uses case-insensitive comparison for username matching
        /// Handles null and empty arrays gracefully
        /// </summary>
        /// <param name="playerName">Player's VRChat username</param>
        /// <param name="roleUsernames">Array of usernames for this role</param>
        /// <returns>True if player is found in the role array</returns>
        private bool IsPlayerInRole(string playerName, string[] roleUsernames)
        {
            if (roleUsernames == null || roleUsernames.Length == 0 || string.IsNullOrEmpty(playerName))
            {
                return false;
            }

            // Case-insensitive username comparison
            string lowerPlayerName = playerName.ToLower();

            for (int i = 0; i < roleUsernames.Length; i++)
            {
                if (!string.IsNullOrEmpty(roleUsernames[i]) &&
                    roleUsernames[i].ToLower() == lowerPlayerName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get total number of configured special users across all roles
        /// Useful for analytics and configuration validation
        /// </summary>
        /// <returns>Total count of users with special roles</returns>
        public int GetTotalSpecialUsers()
        {
            return pwnererUsernames.Length + supaUsernames.Length +
                   lexxUsernames.Length + peterUsernames.Length +
                   bobbyUsernames.Length + architectUsernames.Length +
                   foundingMemberUsernames.Length + supporterUsernames.Length;
        }

        /// <summary>
        /// Generate role statistics for debugging
        /// Shows how many users are configured for each role
        /// </summary>
        /// <returns>Formatted string with role statistics</returns>
        public string GetRoleStatistics()
        {
            return $"Role Statistics:\n" +
                   $"Pwnerer: {pwnererUsernames.Length}\n" +
                   $"Supa: {supaUsernames.Length}\n" +
                   $"Lexx: {lexxUsernames.Length}\n" +
                   $"Peter: {peterUsernames.Length}\n" +
                   $"Bobby: {bobbyUsernames.Length}\n" +
                   $"Architect: {architectUsernames.Length}\n" +
                   $"Founding Member: {foundingMemberUsernames.Length}\n" +
                   $"Supporter: {supporterUsernames.Length}\n" +
                   $"Total Special Users: {GetTotalSpecialUsers()}";
        }

        /// <summary>
        /// Centralized debug logging system
        /// Only logs when debug mode is enabled to avoid console spam
        /// Includes component name for easy identification in console
        /// </summary>
        /// <param name="message">Debug message to log</param>
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                // UdonSharp limitation: No string interpolation with complex expressions
                // Use string concatenation instead
                string coloredMessage = "<color=#9932CC>👑 [NotificationRolesModule] " + message + "</color>";
                
                // Unity console WITH color (rich text supported)
                Debug.Log(coloredMessage);
                
                // VUdon-Logger with SAME color - DIRECT METHOD CALL!
                if (productionLogger != null)
                {
                    productionLogger.Log(coloredMessage);
                }
            }
        }

        // ==============================
        // TESTING METHODS
        // ==============================

        [ContextMenu("Test Role Integration - Achievement")]
        public void TestRoleIntegrationAchievement()
        {
            if (notificationUI == null)
            {
                LogDebug("Cannot test - XboxNotificationUI reference not set");
                return;
            }

            if (pwnererUsernames.Length > 0)
            {
                string testUser = pwnererUsernames[0];
                LogDebug($"Testing achievement notification for Pwnerer: {testUser}");
                QueueAchievementWithRole(testUser, "Test Achievement", 100);
            }
            else if (supporterUsernames.Length > 0)
            {
                string testUser = supporterUsernames[0];
                LogDebug($"Testing achievement notification for Supporter: {testUser}");
                QueueAchievementWithRole(testUser, "Test Achievement", 50);
            }
            else
            {
                LogDebug("Testing achievement notification for Regular user");
                QueueAchievementWithRole("TestRegularUser", "Test Achievement", 25);
            }
        }

        [ContextMenu("Test Role Integration - Online")]
        public void TestRoleIntegrationOnline()
        {
            if (notificationUI == null)
            {
                LogDebug("Cannot test - XboxNotificationUI reference not set");
                return;
            }

            if (supporterUsernames.Length > 0)
            {
                string testUser = supporterUsernames[0];
                LogDebug($"Testing online notification for Supporter: {testUser}");
                QueueOnlineWithRole(testUser, false);
            }
            else
            {
                LogDebug("Testing online notification for Regular user");
                QueueOnlineWithRole("TestRegularUser", false);
            }
        }

        [ContextMenu("Test Multiple Role Notifications")]
        public void TestMultipleRoleNotifications()
        {
            if (notificationUI == null)
            {
                LogDebug("Cannot test - XboxNotificationUI reference not set");
                return;
            }

            // Queue multiple notifications to test priority system
            QueueAchievementWithRole("RegularUser1", "Test Achievement", 25);
            QueueAchievementWithRole("RegularUser2", "Another Achievement", 30);

            if (supporterUsernames.Length > 0)
            {
                QueueAchievementWithRole(supporterUsernames[0], "Supporter Achievement", 50);
            }

            if (pwnererUsernames.Length > 0)
            {
                QueueAchievementWithRole(pwnererUsernames[0], "Pwnerer Achievement", 100);
            }

            LogDebug("Queued multiple test notifications - check priority order!");
        }
    }
}
