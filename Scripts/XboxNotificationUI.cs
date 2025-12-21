using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using Varneon.VUdon.Logger.Abstract;

namespace LowerLevel.Notifications
{
    // Enums must be outside the class for UdonSharp compatibility
    public enum NotificationType
    {
        Achievement,
        Online,
        FirstTime,
        Supporter,
        Pwnerer
    }

    public enum NotificationState
    {
        Inactive,
        FadingIn,
        Blinking,
        FadingOut,
        QueueDelay
    }

    /// <summary>
    /// COMPONENT PURPOSE:
    /// Xbox 360 style achievement notification system with role-based icons and sounds
    /// Uses frame-based timing for reliable execution and smooth FIFO queue processing
    ///
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// Handles achievement unlocks, online notifications, and supporter recognition with authentic Xbox styling
    /// Queues multiple notifications in FIFO order to display events naturally as they occur
    /// Role-based icons and sounds create VIP experience for special community members
    ///
    /// DEPENDENCIES & REQUIREMENTS:
    /// - UI Canvas in World Space mode with proper VRC_UIShape component
    /// - Role-specific icon sprites for each user tier (Regular through Pwnerer)
    /// - Xbox logo and Trophy icon sprites positioned identically (overlapping)
    /// - TextMeshPro components for notification text
    /// - CanvasGroup component on NotificationBackground for fade effects
    /// - Audio clips for different notification types and user roles
    /// - NotificationRolesModule reference for role detection and sounds
    ///
    /// CHANGELOG:
    /// - 2025-11-13 v2.1 - DEPLOYED: FIFO queue system now in production
    /// - 2025-11-11 v2.1 - CRITICAL FIX: Removed priority queue system, implemented FIFO queue
    /// - 2025-11-11 v2.1 - QUEUE: Notifications now display in timestamp order (live feed)
    /// - 2025-11-11 v2.1 - REMOVED: SortQueue(), SwapQueueItems(), queuePriorities array
    /// - 2025-11-11 v2.1 - OVERFLOW: Queue now drops oldest notification instead of lowest priority
    /// - 2025-07-13 v2.0 - MAJOR UPDATE: Added role-based icon and sound system
    /// - 2025-07-13 v2.0 - INTEGRATION: Connected with NotificationRolesModule for user role detection
    /// - 2025-07-13 v2.0 - CLEANUP: Removed excessive debug logging, streamlined for production use
    /// - 2025-07-13 v2.0 - ICONS: Sign-in notifications now use role-specific icons alternating with Xbox logo
    /// - 2025-07-13 v2.0 - SOUNDS: Role-based audio system for personalized notification experience
    /// - 2025-01-16 v1.5 - Fixed queue management issues and notification display timing
    /// - 2025-01-16 v1.4 - Added comprehensive notification queue system with priority handling
    /// - 2025-01-16 v1.3 - Added animation debugging to track fade-in animation failures
    /// - 2025-01-16 v1.2 - Replaced ALL instances of queueCount with QueueCountProperty throughout script
    /// - 2025-01-16 v1.1 - Added QueueCountProperty debugging to track queue count changes
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class XboxNotificationUI : UdonSharpBehaviour
    {
        // =================================================================
        // EVENT-DRIVEN VARIABLES FOR UDONSHARP CROSS-COMPONENT COMMUNICATION
        // =================================================================

        // Event-driven variables for UdonSharp cross-component communication
        [HideInInspector] public string queuePlayerName = "";
        [HideInInspector] public string queueAchievementTitle = "";
        [HideInInspector] public int queuePoints = 0;
        [HideInInspector] public bool queueIsFirstTime = false;

        [Header("UI References")]
        [SerializeField] private GameObject notificationBackground;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI subText;

        [Header("Role System Integration")]
        [Tooltip("Reference to NotificationRolesModule for role detection")]
        [SerializeField] private NotificationRolesModule rolesModule;

        [Header("Core Icons")]
        [Tooltip("Xbox logo (used as secondary icon for all notifications)")]
        [SerializeField] private GameObject xboxLogo;
        [Tooltip("Trophy icon for all achievement notifications")]
        [SerializeField] private GameObject trophyIcon;

        [Header("Role-Based Sign-In Icons")]
        [Tooltip("Regular user icon (or Lower Level logo)")]
        [SerializeField] private GameObject regularUserIcon;
        [Tooltip("Friend icon for non-role based friends")]
        [SerializeField] private GameObject friendIcon;
        [Tooltip("Supporter role icon")]
        [SerializeField] private GameObject supporterIcon;
        [Tooltip("Founding Member role icon")]
        [SerializeField] private GameObject foundingMemberIcon;
        [Tooltip("Architect role icon")]
        [SerializeField] private GameObject architectIcon;
        [Tooltip("Bobby role icon")]
        [SerializeField] private GameObject bobbyIcon;
        [Tooltip("Peter role icon")]
        [SerializeField] private GameObject peterIcon;
        [Tooltip("Lexx role icon")]
        [SerializeField] private GameObject lexxIcon;
        [Tooltip("Supa role icon")]
        [SerializeField] private GameObject supaIcon;
        [Tooltip("Pwnerer role icon")]
        [SerializeField] private GameObject pwnererIcon;

        [Header("Audio System")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip achievementSound;
        [SerializeField] private AudioClip onlineSound;
        [SerializeField] private AudioClip supporterSound;
        [SerializeField] private AudioClip pwnererSound;

        [Header("Queue Configuration")]
        [SerializeField] private int maxQueueSize = 10;
        [SerializeField] private float displayDuration = 4.0f;
        [SerializeField] private float blinkInterval = 0.5f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float queueDelay = 0.2f;

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = false;

        [Header("Production Debugging")]
        [Tooltip("Reference to UdonLogger component for in-world console (optional)")]
        [SerializeField] private UdonLogger productionLogger;

        // Queue Management - Using separate arrays for UdonSharp compatibility (FIFO)
        private NotificationType[] queueTypes;
        private string[] queuePlayerNames;
        private string[] queueAchievementTitles;
        private int[] queuePointsArray;
        private float[] queueTimestamps;
        private int queueCount = 0;

        // Current Notification State
        private bool isDisplayingNotification = false;
        private bool isBlinking = false;
        private bool showingXboxIcon = true;
        private NotificationState currentState = NotificationState.Inactive;

        // Current notification context for role-based display
        private NotificationType currentNotificationType;
        private UserRole currentUserRole;
        private GameObject currentPrimaryIcon;
        private GameObject currentSecondaryIcon;
        private string currentNotificationPlayerName = "";

        // Timing Variables
        private float stateTimer = 0f;
        private float blinkTimer = 0f;
        private float currentAlpha = 0f;

        // Component References
        private bool isInitialized = false;

        void Start()
        {
            InitializeComponent();
        }

        void InitializeComponent()
        {
            // Initialize queue arrays (no priority array - FIFO only)
            queueTypes = new NotificationType[maxQueueSize];
            queuePlayerNames = new string[maxQueueSize];
            queueAchievementTitles = new string[maxQueueSize];
            queuePointsArray = new int[maxQueueSize];
            queueTimestamps = new float[maxQueueSize];

            // Validate required components
            if (!ValidateComponents())
            {
                LogDebug("Component validation failed - notification system disabled");
                enabled = false;
                return;
            }

            // Initialize UI state
            SetNotificationVisible(false);
            currentState = NotificationState.Inactive;

            LogDebug($"Xbox Notification Queue initialized - Max queue: {maxQueueSize} (FIFO mode)");
            isInitialized = true;
        }

        void Update()
        {
            if (!isInitialized) return;

            // Process queue if not currently displaying and queue has items
            if (!isDisplayingNotification && queueCount > 0)
            {
                StartNextNotification();
            }

            // Handle current notification state
            if (isDisplayingNotification)
            {
                UpdateCurrentNotification();
            }
        }

        // =================================================================
        // PUBLIC QUEUE METHODS
        // =================================================================

        /// <summary>
        /// Queues an achievement notification for display.
        /// Notifications are displayed in FIFO order (first-in, first-out).
        /// </summary>
        /// <param name="playerName">Player who earned the achievement</param>
        /// <param name="achievementTitle">Achievement title text</param>
        /// <param name="points">Gamerscore value</param>
        public void QueueAchievementNotification(string playerName, string achievementTitle, int points)
        {
            AddToQueue(NotificationType.Achievement, playerName, achievementTitle, points);
        }

        /// <summary>
        /// Queues a player online notification (login/join event).
        /// Notifications are displayed in FIFO order (first-in, first-out).
        /// </summary>
        /// <param name="playerName">Player who joined</param>
        /// <param name="isFirstTime">True if this is the player's first visit</param>
        public void QueueOnlineNotification(string playerName, bool isFirstTime)
        {
            var type = isFirstTime ? NotificationType.FirstTime : NotificationType.Online;
            AddToQueue(type, playerName, "", 0);
        }

        /// <summary>
        /// Queues a supporter-level achievement notification with special icon and sound.
        /// Notifications are displayed in FIFO order (first-in, first-out).
        /// </summary>
        /// <param name="playerName">Supporter who earned the achievement</param>
        /// <param name="achievementTitle">Achievement title text</param>
        /// <param name="points">Gamerscore value</param>
        public void QueueSupporterNotification(string playerName, string achievementTitle, int points)
        {
            AddToQueue(NotificationType.Supporter, playerName, achievementTitle, points);
        }

        /// <summary>
        /// Queues a Pwnerer-level achievement notification with special icon and sound.
        /// Notifications are displayed in FIFO order (first-in, first-out).
        /// </summary>
        /// <param name="playerName">Pwnerer who earned the achievement</param>
        /// <param name="achievementTitle">Achievement title text</param>
        /// <param name="points">Gamerscore value</param>
        public void QueuePwnererNotification(string playerName, string achievementTitle, int points)
        {
            AddToQueue(NotificationType.Pwnerer, playerName, achievementTitle, points);
        }

        // =================================================================
        // EVENT-DRIVEN WRAPPER METHODS FOR UDONSHARP COMMUNICATION
        // =================================================================

        /// <summary>
        /// Event-driven wrapper for QueueAchievementNotification
        /// Called via SendCustomEvent after SetProgramVariable
        /// </summary>
        public void QueueAchievementNotificationEvent()
        {
            // Use the public variables that were set via SetProgramVariable
            if (string.IsNullOrEmpty(queuePlayerName))
            {
                LogDebug("❌ QueueAchievementNotificationEvent: playerName is empty!");
                return;
            }

            if (string.IsNullOrEmpty(queueAchievementTitle))
            {
                LogDebug("❌ QueueAchievementNotificationEvent: achievementTitle is empty!");
                return;
            }

            LogDebug($"✅ QueueAchievementNotificationEvent: {queuePlayerName} - {queueAchievementTitle} - {queuePoints}G");

            // Call the actual method with the parameters
            QueueAchievementNotification(queuePlayerName, queueAchievementTitle, queuePoints);

            // Clear the variables after use
            queuePlayerName = "";
            queueAchievementTitle = "";
            queuePoints = 0;
        }

        /// <summary>
        /// Event-driven wrapper for QueueOnlineNotification
        /// Called via SendCustomEvent after SetProgramVariable
        /// </summary>
        public void QueueOnlineNotificationEvent()
        {
            // Use the public variables that were set via SetProgramVariable
            if (string.IsNullOrEmpty(queuePlayerName))
            {
                LogDebug("❌ QueueOnlineNotificationEvent: playerName is empty!");
                return;
            }

            LogDebug($"✅ QueueOnlineNotificationEvent: {queuePlayerName} - FirstTime: {queueIsFirstTime}");

            // Call the actual method with the parameters
            QueueOnlineNotification(queuePlayerName, queueIsFirstTime);

            // Clear the variables after use
            queuePlayerName = "";
            queueIsFirstTime = false;
        }

        // =================================================================
        // QUEUE MANAGEMENT (FIFO)
        // =================================================================

        /// <summary>
        /// Adds a notification to the queue in FIFO order.
        /// If queue is full, removes the oldest notification.
        /// </summary>
        /// <param name="type">Notification type (Achievement, Online, FirstTime, Supporter, Pwnerer)</param>
        /// <param name="playerName">Player name for the notification</param>
        /// <param name="achievementTitle">Achievement title (empty for login notifications)</param>
        /// <param name="points">Gamerscore value (0 for login notifications)</param>
        private void AddToQueue(NotificationType type, string playerName, string achievementTitle, int points)
        {
            if (queueCount >= maxQueueSize)
            {
                LogDebug("Queue full! Dropping oldest notification");
                RemoveOldestNotification();
            }

            // Add to queue (FIFO - no sorting)
            queueTypes[queueCount] = type;
            queuePlayerNames[queueCount] = playerName;
            queueAchievementTitles[queueCount] = achievementTitle;
            queuePointsArray[queueCount] = points;
            queueTimestamps[queueCount] = Time.time;
            queueCount++;

            LogDebug($"Queued notification #{queueCount} for {playerName} (Type: {type}, Queue size: {queueCount})");
        }

        private void RemoveOldestNotification()
        {
            if (queueCount == 0) return;

            // Remove oldest notification (index 0) by shifting array
            RemoveFromQueue(0);
        }

        private void RemoveFromQueue(int index)
        {
            if (index < 0 || index >= queueCount) return;

            // Shift all remaining items forward
            for (int i = index; i < queueCount - 1; i++)
            {
                queueTypes[i] = queueTypes[i + 1];
                queuePlayerNames[i] = queuePlayerNames[i + 1];
                queueAchievementTitles[i] = queueAchievementTitles[i + 1];
                queuePointsArray[i] = queuePointsArray[i + 1];
                queueTimestamps[i] = queueTimestamps[i + 1];
            }

            queueCount--;
        }

        // =================================================================
        // NOTIFICATION DISPLAY SYSTEM
        // =================================================================

        /// <summary>
        /// Starts displaying the next notification from the queue.
        /// Called when current notification finishes or when queue becomes non-empty.
        /// </summary>
        private void StartNextNotification()
        {
            if (queueCount == 0) return;

            LogDebug($"Starting notification: {queueTypes[0]} for {queuePlayerNames[0]}");

            // Setup notification display BEFORE removing from queue
            SetupNotificationDisplay(0);

            // Remove from queue and shift remaining items
            RemoveFromQueue(0);

            // Start fade-in animation
            isDisplayingNotification = true;
            currentState = NotificationState.FadingIn;
            stateTimer = 0f;
            currentAlpha = 0f;
        }

        /// <summary>
        /// Sets up UI elements for displaying a queued notification.
        /// Configures text, icons, and sound based on notification type and player role.
        /// </summary>
        /// <param name="queueIndex">Index of the notification in the queue (always 0 for current)</param>
        private void SetupNotificationDisplay(int queueIndex)
        {
            // Store current notification context
            currentNotificationType = queueTypes[queueIndex];
            currentNotificationPlayerName = queuePlayerNames[queueIndex];

            string mainTextContent = "";
            string subTextContent = "";

            // Set up text content based on notification type
            switch (queueTypes[queueIndex])
            {
                case NotificationType.Achievement:
                    mainTextContent = "Achievement Unlocked";
                    subTextContent = $"{queuePlayerNames[queueIndex]} - {queueAchievementTitles[queueIndex]} - {queuePointsArray[queueIndex]}G";
                    break;

                case NotificationType.FirstTime:
                    mainTextContent = "Welcome!";
                    subTextContent = $"{queuePlayerNames[queueIndex]} joined the party for the first time!";
                    break;

                case NotificationType.Online:
                    mainTextContent = "Online";
                    subTextContent = $"{queuePlayerNames[queueIndex]} is now online - Welcome back to the basement!";
                    break;

                case NotificationType.Supporter:
                    mainTextContent = "Supporter Achievement Unlocked";
                    subTextContent = $"{queuePlayerNames[queueIndex]} - {queueAchievementTitles[queueIndex]} - {queuePointsArray[queueIndex]}G";
                    break;

                case NotificationType.Pwnerer:
                    mainTextContent = "Pwnerer Achievement Unlocked";
                    subTextContent = $"{queuePlayerNames[queueIndex]} - {queueAchievementTitles[queueIndex]} - {queuePointsArray[queueIndex]}G";
                    break;
            }

            mainText.text = mainTextContent;
            subText.text = subTextContent;

            // Setup role-based icons
            SetupIconsForNotification(queueTypes[queueIndex], queuePlayerNames[queueIndex]);

            // Play role-based sound
            PlayNotificationSound(queueTypes[queueIndex]);
        }

        // =================================================================
        // ROLE-BASED ICON SYSTEM
        // =================================================================

        /// <summary>
        /// Configures notification icons based on notification type and player role.
        /// Achievement notifications show trophy icon, login notifications show role-specific icons.
        /// </summary>
        /// <param name="notificationType">Type of notification being displayed</param>
        /// <param name="playerName">Player name for role lookup</param>
        private void SetupIconsForNotification(NotificationType notificationType, string playerName)
        {
            // Hide all icons first
            HideAllIcons();

            if (notificationType == NotificationType.Achievement ||
                notificationType == NotificationType.Supporter ||
                notificationType == NotificationType.Pwnerer)
            {
                // Achievement notifications: Trophy + Xbox Logo
                currentPrimaryIcon = trophyIcon;
                currentSecondaryIcon = xboxLogo;
                LogDebug("Achievement notification - Trophy + Xbox Logo");
            }
            else
            {
                // Sign-in notifications: Role-specific icon + Xbox Logo
                currentUserRole = GetPlayerRole(playerName);
                currentPrimaryIcon = GetRoleIcon(currentUserRole);
                currentSecondaryIcon = xboxLogo;
                LogDebug($"Sign-in notification - Role: {currentUserRole}");
            }

            // Validate we have valid icons
            if (currentPrimaryIcon == null)
            {
                currentPrimaryIcon = regularUserIcon;
            }
            if (currentSecondaryIcon == null)
            {
                currentSecondaryIcon = xboxLogo;
            }

            // Start with primary icon visible
            if (currentPrimaryIcon != null)
            {
                currentPrimaryIcon.SetActive(true);
                showingXboxIcon = false; // We're showing the primary (non-Xbox) icon first
            }
            if (currentSecondaryIcon != null)
            {
                currentSecondaryIcon.SetActive(false);
            }

            blinkTimer = 0f;
        }

        private GameObject GetRoleIcon(UserRole role)
        {
            switch (role)
            {
                case UserRole.Pwnerer: return pwnererIcon;
                case UserRole.Supa: return supaIcon;
                case UserRole.Lexx: return lexxIcon;
                case UserRole.Peter: return peterIcon;
                case UserRole.Bobby: return bobbyIcon;
                case UserRole.Architect: return architectIcon;
                case UserRole.FoundingMember: return foundingMemberIcon;
                case UserRole.Supporter: return supporterIcon;
                case UserRole.Regular:
                default:
                    return regularUserIcon;
            }
        }

        private UserRole GetPlayerRole(string playerName)
        {
            if (rolesModule != null)
            {
                return rolesModule.GetPlayerRole(playerName);
            }
            else
            {
                LogDebug("WARNING: NotificationRolesModule not assigned, defaulting to Regular");
                return UserRole.Regular;
            }
        }

        private void HideAllIcons()
        {
            // Hide all role-specific icons
            if (regularUserIcon != null) regularUserIcon.SetActive(false);
            if (friendIcon != null) friendIcon.SetActive(false);
            if (supporterIcon != null) supporterIcon.SetActive(false);
            if (foundingMemberIcon != null) foundingMemberIcon.SetActive(false);
            if (architectIcon != null) architectIcon.SetActive(false);
            if (bobbyIcon != null) bobbyIcon.SetActive(false);
            if (peterIcon != null) peterIcon.SetActive(false);
            if (lexxIcon != null) lexxIcon.SetActive(false);
            if (supaIcon != null) supaIcon.SetActive(false);
            if (pwnererIcon != null) pwnererIcon.SetActive(false);

            // Hide standard icons
            if (trophyIcon != null) trophyIcon.SetActive(false);
            if (xboxLogo != null) xboxLogo.SetActive(false);
        }

        // =================================================================
        // ROLE-BASED SOUND SYSTEM
        // =================================================================

        /// <summary>
        /// Plays the appropriate notification sound based on type and player role.
        /// Uses role-specific sounds from NotificationRolesModule when available.
        /// </summary>
        /// <param name="type">Notification type to determine sound</param>
        private void PlayNotificationSound(NotificationType type)
        {
            if (audioSource == null) return;

            AudioClip soundToPlay = null;

            // For sign-in notifications, use role-based sounds
            if (type == NotificationType.FirstTime || type == NotificationType.Online)
            {
                // Get role-specific sound from NotificationRolesModule
                if (rolesModule != null)
                {
                    soundToPlay = rolesModule.GetRoleSound(currentNotificationPlayerName);
                }
                else
                {
                    soundToPlay = onlineSound;
                }
            }
            else
            {
                // For achievement notifications, use achievement-type sounds
                switch (type)
                {
                    case NotificationType.Achievement:
                        soundToPlay = achievementSound;
                        break;
                    case NotificationType.Supporter:
                        soundToPlay = supporterSound;
                        break;
                    case NotificationType.Pwnerer:
                        soundToPlay = pwnererSound;
                        break;
                    default:
                        soundToPlay = achievementSound;
                        break;
                }
            }

            // Fallback to default sounds if role-specific sound is null
            if (soundToPlay == null)
            {
                switch (type)
                {
                    case NotificationType.Achievement:
                    case NotificationType.FirstTime:
                        soundToPlay = achievementSound;
                        break;
                    case NotificationType.Online:
                        soundToPlay = onlineSound;
                        break;
                    case NotificationType.Supporter:
                        soundToPlay = supporterSound;
                        break;
                    case NotificationType.Pwnerer:
                        soundToPlay = pwnererSound;
                        break;
                }
            }

            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
                LogDebug($"Playing sound: {soundToPlay.name} for {type}");
            }
        }

        // =================================================================
        // ANIMATION SYSTEM
        // =================================================================

        private void UpdateCurrentNotification()
        {
            stateTimer += Time.deltaTime;

            switch (currentState)
            {
                case NotificationState.FadingIn:
                    UpdateFadeIn();
                    break;

                case NotificationState.Blinking:
                    UpdateBlinking();
                    break;

                case NotificationState.FadingOut:
                    UpdateFadeOut();
                    break;

                case NotificationState.QueueDelay:
                    UpdateQueueDelay();
                    break;
            }
        }

        private void UpdateFadeIn()
        {
            currentAlpha = Mathf.Clamp01(stateTimer / fadeInDuration);
            canvasGroup.alpha = currentAlpha;

            if (stateTimer >= fadeInDuration)
            {
                currentState = NotificationState.Blinking;
                stateTimer = 0f;
                isBlinking = true;
                SetNotificationVisible(true);
            }
        }

        private void UpdateBlinking()
        {
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                showingXboxIcon = !showingXboxIcon;

                // Switch between current primary and secondary icons
                if (showingXboxIcon)
                {
                    // Show secondary icon (Xbox logo)
                    if (currentPrimaryIcon != null) currentPrimaryIcon.SetActive(false);
                    if (currentSecondaryIcon != null) currentSecondaryIcon.SetActive(true);
                }
                else
                {
                    // Show primary icon (role-specific or trophy)
                    if (currentSecondaryIcon != null) currentSecondaryIcon.SetActive(false);
                    if (currentPrimaryIcon != null) currentPrimaryIcon.SetActive(true);
                }
            }

            if (stateTimer >= displayDuration)
            {
                currentState = NotificationState.FadingOut;
                stateTimer = 0f;
                isBlinking = false;
            }
        }

        private void UpdateFadeOut()
        {
            currentAlpha = 1f - Mathf.Clamp01(stateTimer / fadeOutDuration);
            canvasGroup.alpha = currentAlpha;

            if (stateTimer >= fadeOutDuration)
            {
                SetNotificationVisible(false);
                currentState = NotificationState.QueueDelay;
                stateTimer = 0f;
            }
        }

        private void UpdateQueueDelay()
        {
            if (stateTimer >= queueDelay)
            {
                isDisplayingNotification = false;
                currentState = NotificationState.Inactive;
                stateTimer = 0f;
            }
        }

        private void SetNotificationVisible(bool visible)
        {
            float targetAlpha = visible ? 1f : 0f;
            canvasGroup.alpha = targetAlpha;

            if (!visible)
            {
                // Hide all icons when notification is not visible
                HideAllIcons();
                // Clear current player name when hiding notification
                currentNotificationPlayerName = "";
            }
        }

        // =================================================================
        // VALIDATION AND UTILITIES
        // =================================================================

        private bool ValidateComponents()
        {
            bool isValid = true;

            if (notificationBackground == null) { LogDebug("Missing: Notification Background"); isValid = false; }
            if (canvasGroup == null) { LogDebug("Missing: Canvas Group"); isValid = false; }
            if (mainText == null) { LogDebug("Missing: Main Text"); isValid = false; }
            if (subText == null) { LogDebug("Missing: Sub Text"); isValid = false; }

            // Check core icons
            if (xboxLogo == null) { LogDebug("Missing: Xbox Logo"); isValid = false; }
            if (trophyIcon == null) { LogDebug("Missing: Trophy Icon"); isValid = false; }

            // Check role-specific icons (warnings only)
            if (regularUserIcon == null) { LogDebug("Warning: Regular User Icon not assigned"); }
            if (rolesModule == null) { LogDebug("Warning: NotificationRolesModule not assigned - role detection disabled"); }

            return isValid;
        }

        /// <summary>
        /// Clears all queued notifications.
        /// Useful for testing or resetting the notification system.
        /// </summary>
        public void ClearQueue()
        {
            queueCount = 0;
            isDisplayingNotification = false;
            currentState = NotificationState.Inactive;
            SetNotificationVisible(false);
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
                string coloredMessage = $"<color=#107C10>📱 [XboxNotificationUI] {message}</color>";

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

        [ContextMenu("Test Achievement Notification")]
        public void TestAchievementNotification()
        {
            QueueAchievementNotification("TestPlayer", "Basement Dweller", 50);
        }

        [ContextMenu("Test Sign-In Notification")]
        public void TestSignInNotification()
        {
            QueueOnlineNotification("TestPlayer", false);
        }

        [ContextMenu("Test First-Time Notification")]
        public void TestFirstTimeNotification()
        {
            QueueOnlineNotification("NewPlayer", true);
        }

        [ContextMenu("Test FIFO Order - Queue 3 Notifications")]
        public void TestFIFOOrder()
        {
            QueueOnlineNotification("RegularPlayer", false);
            QueueAchievementNotification("TestPlayer", "Basement Dweller", 50);
            QueueOnlineNotification("Pwnerer", false);
            LogDebug("Queued 3 notifications - should display in order: Regular → Achievement → Pwnerer");
        }

        [ContextMenu("Clear Queue")]
        public void TestClearQueue()
        {
            ClearQueue();
        }
    }
}
