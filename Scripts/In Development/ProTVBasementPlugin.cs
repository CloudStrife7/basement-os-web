using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace LowerLevel.Notifications
{
    /// <summary>
    /// COMPONENT PURPOSE:
    /// ProTV plugin integration for basement notification system
    /// Hooks into ProTV events to trigger contextual Xbox-style notifications
    /// Creates seamless movie night experience with achievement/social overlays
    /// 
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// Triggers "Movie Night" achievements when videos start/end
    /// Shows social notifications during video playback
    /// Creates immersive basement entertainment experience
    /// 
    /// DEPENDENCIES & REQUIREMENTS:
    /// - ProTV 3 Beta TVManager (must be added to TV's plugin list)
    /// - NotificationEventHub system (existing basement notification system)
    /// - XboxNotificationUI (for displaying notifications)
    /// 
    /// ARCHITECTURE PATTERN:
    /// ProTV Plugin pattern using event-driven callbacks
    /// Listens for TV state changes and triggers appropriate notifications
    /// Uses existing notification queue system for consistent experience
    /// 
    /// CHANGELOG:
    /// - 2025-07-15 v1.1 - FIXED: Added missing using LowerLevel.Integration; for NotificationEventHub
    /// - 2025-07-15 v1.0 - Initial ProTV event integration
    /// - 2025-07-15 v1.0 - Video start/end notification triggers
    /// - 2025-07-15 v1.0 - Movie night achievement system
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ProTVBasementPlugin : UdonSharpBehaviour
    {
        [Header("Notification Integration")]
        [Tooltip("Reference to existing Xbox notification system")]
        [SerializeField] private XboxNotificationUI xboxNotificationUI;
        [Tooltip("Reference to ProTV overlay positioning system")]
        [SerializeField] private LowerLevel.Integration.NotificationEventHub notificationEventHub;

        [Header("Movie Night Features")]
        [Tooltip("Enable movie night achievement notifications")]
        [SerializeField] private bool enableMovieNightAchievements = true;
        [Tooltip("Enable video change notifications")]
        [SerializeField] private bool enableVideoChangeNotifications = true;
        [Tooltip("Show video requester notifications")]
        [SerializeField] private bool showVideoRequesterNotifications = true;

        [Header("Achievement Settings")]
        [Tooltip("Points awarded for starting a movie")]
        [SerializeField] private int movieStartPoints = 10;
        [Tooltip("Points for movie night host")]
        [SerializeField] private int movieHostPoints = 25;

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;

        // ProTV Event Variables (received from TVManager)
        [Header("ProTV Event Data - DO NOT MODIFY")]
        [Tooltip("These variables are set by ProTV before events are called")]
        public string OUT_URL = "";           // Current video URL
        public float OUT_VOLUME = 0f;         // Current volume (0-1)
        public bool OUT_LOCKED = false;       // TV lock state
        public bool OUT_MUTED = false;        // Mute state
        public bool OUT_LOADING = false;      // Loading state
        public bool OUT_PLAYING = false;      // Playing state
        public float OUT_TIME = 0f;           // Current video time
        public float OUT_DURATION = 0f;       // Video duration

        // Internal state tracking
        private bool wasPlaying = false;
        private string lastVideoUrl = "";
        private string currentVideoTitle = "";
        private string videoRequester = "";
        private bool isMovieNight = false;

        void Start()
        {
            LogDebug("ProTV Basement Plugin initialized");
        }

        // =================================================================
        // PROTV EVENT CALLBACKS
        // =================================================================

        /// <summary>
        /// Called when ProTV is ready and initialized
        /// </summary>
        public void _TvReady()
        {
            LogDebug("ProTV is ready!");

            if (enableVideoChangeNotifications && notificationEventHub != null)
            {
                notificationEventHub.eventPlayerName = "System";
                notificationEventHub.eventAchievementLevel = 0;
                notificationEventHub.OnAchievementEarned();
            }
        }

        /// <summary>
        /// Called when video URL changes
        /// </summary>
        public void _TvUrlChange()
        {
            LogDebug($"Video URL changed to: {OUT_URL}");

            // Extract video title from URL (basic implementation)
            currentVideoTitle = ExtractVideoTitle(OUT_URL);
            lastVideoUrl = OUT_URL;

            if (enableVideoChangeNotifications && !string.IsNullOrEmpty(currentVideoTitle))
            {
                ShowVideoChangeNotification();
            }
        }

        /// <summary>
        /// Called when video starts playing
        /// </summary>
        public void _TvVideoStart()
        {
            LogDebug($"Video started: {currentVideoTitle}");
            wasPlaying = true;

            if (enableMovieNightAchievements)
            {
                TriggerMovieStartAchievement();
            }

            // Mark as movie night if video is longer than 30 minutes
            isMovieNight = OUT_DURATION > 1800f; // 30 minutes in seconds

            if (isMovieNight)
            {
                ShowMovieNightStartNotification();
            }
        }

        /// <summary>
        /// Called when video ends or stops
        /// </summary>
        public void _TvVideoEnd()
        {
            LogDebug($"Video ended: {currentVideoTitle}");
            wasPlaying = false;

            if (isMovieNight && enableMovieNightAchievements)
            {
                TriggerMovieEndAchievement();
            }

            isMovieNight = false;
        }

        /// <summary>
        /// Called when TV volume changes
        /// </summary>
        public void _TvVolumeChange()
        {
            LogDebug($"Volume changed to: {OUT_VOLUME * 100f}%");

            // Could trigger volume-related notifications if desired
            // For now, we'll skip this to avoid spam
        }

        /// <summary>
        /// Called when TV loading state changes
        /// </summary>
        public void _TvLoadingChange()
        {
            LogDebug($"Loading state: {OUT_LOADING}");

            if (OUT_LOADING && enableVideoChangeNotifications)
            {
                if (notificationEventHub != null)
                {
                    notificationEventHub.eventPlayerName = "System";
                    notificationEventHub.eventAchievementLevel = 0;
                    notificationEventHub.OnAchievementEarned();
                }
            }
        }

        // =================================================================
        // NOTIFICATION TRIGGERS
        // =================================================================

        private void ShowVideoChangeNotification()
        {
            if (notificationEventHub == null) return;

            string mainText = "🎬 Now Playing";
            string subText = $"{currentVideoTitle}";

            if (!string.IsNullOrEmpty(videoRequester))
            {
                subText += $"\nRequested by {videoRequester}";
            }

            notificationEventHub.eventPlayerName = "System";
            notificationEventHub.eventAchievementLevel = 0;
            notificationEventHub.OnAchievementEarned();
        }

        private void TriggerMovieStartAchievement()
        {
            if (xboxNotificationUI == null) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer != null)
            {
                string playerName = localPlayer.displayName;

                // Different achievements based on context
                if (Networking.IsOwner(gameObject))
                {
                    // Player is hosting the movie
                    xboxNotificationUI.QueueAchievementNotification(
                        playerName,
                        "Movie Night Host",
                        movieHostPoints
                    );
                }
                else
                {
                    // Player is joining movie night
                    xboxNotificationUI.QueueAchievementNotification(
                        playerName,
                        "Movie Night Participant",
                        movieStartPoints
                    );
                }
            }
        }

        private void TriggerMovieEndAchievement()
        {
            if (xboxNotificationUI == null) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer != null)
            {
                string playerName = localPlayer.displayName;

                xboxNotificationUI.QueueAchievementNotification(
                    playerName,
                    "Movie Night Complete",
                    movieStartPoints * 2
                );
            }
        }

        private void ShowMovieNightStartNotification()
        {
            if (notificationEventHub == null) return;

            notificationEventHub.eventPlayerName = "System";
            notificationEventHub.eventAchievementLevel = 0;
            notificationEventHub.OnAchievementEarned();

            LogDebug($"Movie night activated for: {currentVideoTitle}");
        }

        // =================================================================
        // UTILITY METHODS
        // =================================================================

        private string ExtractVideoTitle(string url)
        {
            if (string.IsNullOrEmpty(url)) return "Unknown Video";

            // Basic title extraction (could be enhanced)
            if (url.Contains("youtube") || url.Contains("youtu.be"))
            {
                return "YouTube Video";
            }
            else if (url.Contains("twitch"))
            {
                return "Twitch Stream";
            }
            else if (url.Contains(".mp4") || url.Contains(".webm"))
            {
                // Extract filename from direct video URLs
                string[] urlParts = url.Split('/');
                if (urlParts.Length > 0)
                {
                    string filename = urlParts[urlParts.Length - 1];
                    return filename.Replace(".mp4", "").Replace(".webm", "").Replace("%20", " ");
                }
            }

            return "Basement Cinema";
        }

        /// <summary>
        /// Set who requested the current video (for notification display)
        /// </summary>
        public void SetVideoRequester(string requesterName)
        {
            videoRequester = requesterName;
            LogDebug($"Video requester set to: {requesterName}");
        }

        /// <summary>
        /// Manual trigger for movie night achievement (for testing)
        /// </summary>
        [ContextMenu("Test Movie Start Achievement")]
        public void TestMovieStartAchievement()
        {
            currentVideoTitle = "Test Movie";
            TriggerMovieStartAchievement();
        }

        [ContextMenu("Test Video Change Notification")]
        public void TestVideoChangeNotification()
        {
            currentVideoTitle = "Epic Basement Movie Night";
            videoRequester = "TestPlayer";
            ShowVideoChangeNotification();
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"🎬 [ProTVBasementPlugin] {message}");
            }
        }
    }
}