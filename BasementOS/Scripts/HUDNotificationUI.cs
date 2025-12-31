using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace LowerLevel.Notifications
{
    /// <summary>
    /// COMPONENT PURPOSE:
    /// Head-following HUD notification display for Xbox 360 style achievement/login popups.
    /// Displays notifications attached to the player's view, controlled by BasementOS settings.
    ///
    /// LOWER LEVEL 2.0 INTEGRATION:
    /// - Uses same interface as XboxNotificationUI for drop-in compatibility
    /// - Controlled by DT_App_Settings (0=HUD, 1=TV, 2=BOTH)
    /// - Follows player head position for VR/Desktop compatibility
    ///
    /// SETUP REQUIREMENTS:
    /// 1. Attach to a Canvas with Render Mode: World Space
    /// 2. Canvas should be a child of an empty GameObject that this script moves
    /// 3. Assign all UI references in Inspector
    ///
    /// LOCATION: Assets/Scripts/HUDNotificationUI.cs
    ///
    /// CHANGELOG:
    /// 2025-12-26 v1.0 - Initial implementation for HUD notification system
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HUDNotificationUI : UdonSharpBehaviour
    {
        // =================================================================
        // EVENT-DRIVEN VARIABLES (Same interface as XboxNotificationUI)
        // =================================================================

        [HideInInspector] public string queuePlayerName = "";
        [HideInInspector] public string queueAchievementTitle = "";
        [HideInInspector] public int queuePoints = 0;
        [HideInInspector] public bool queueIsFirstTime = false;

        // =================================================================
        // HUD POSITIONING
        // =================================================================

        [Header("HUD Positioning")]
        [Tooltip("Distance from player head")]
        [SerializeField] private float hudDistance = 1.5f;

        [Tooltip("Vertical offset from eye level")]
        [SerializeField] private float hudVerticalOffset = -0.3f;

        [Tooltip("Scale of HUD canvas")]
        [SerializeField] private float hudScale = 0.001f;

        [Tooltip("Smoothing for HUD follow (0 = instant, 1 = very smooth)")]
        [SerializeField] private float followSmoothing = 0.1f;

        // =================================================================
        // UI REFERENCES
        // =================================================================

        [Header("UI References")]
        [SerializeField] private GameObject notificationBackground;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI subText;

        [Header("Icons")]
        [SerializeField] private GameObject xboxLogo;
        [SerializeField] private GameObject trophyIcon;
        [SerializeField] private GameObject onlineIcon;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip achievementSound;
        [SerializeField] private AudioClip onlineSound;

        [Header("Timing")]
        [SerializeField] private int maxQueueSize = 10;
        [SerializeField] private float displayDuration = 4.0f;
        [SerializeField] private float blinkInterval = 0.5f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float queueDelay = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = false;

        // =================================================================
        // QUEUE STATE
        // =================================================================

        private int[] queueTypes;           // 0=Achievement, 1=Online, 2=FirstTime
        private string[] queuePlayerNames;
        private string[] queueAchievementTitles;
        private int[] queuePointsArray;
        private int queueCount = 0;

        // =================================================================
        // DISPLAY STATE
        // =================================================================

        private bool isDisplayingNotification = false;
        private int currentState = 0;  // 0=Inactive, 1=FadingIn, 2=Blinking, 3=FadingOut, 4=QueueDelay
        private float stateTimer = 0f;
        private float blinkTimer = 0f;
        private float currentAlpha = 0f;
        private bool showingXboxIcon = false;

        // Current notification data
        private int currentNotificationType = 0;
        private GameObject currentPrimaryIcon;

        // =================================================================
        // COMPONENT STATE
        // =================================================================

        private bool isInitialized = false;
        private VRCPlayerApi localPlayer;
        private Transform hudTransform;

        // =================================================================
        // INITIALIZATION
        // =================================================================

        void Start()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Initialize queue arrays
            queueTypes = new int[maxQueueSize];
            queuePlayerNames = new string[maxQueueSize];
            queueAchievementTitles = new string[maxQueueSize];
            queuePointsArray = new int[maxQueueSize];

            // Validate components
            if (!ValidateComponents())
            {
                LogDebug("Component validation failed - HUD notification disabled");
                enabled = false;
                return;
            }

            // Cache transform
            hudTransform = transform;

            // Set initial scale
            hudTransform.localScale = new Vector3(hudScale, hudScale, hudScale);

            // Start hidden
            SetNotificationVisible(false);
            currentState = 0;

            // Get local player
            localPlayer = Networking.LocalPlayer;

            LogDebug("HUDNotificationUI initialized - Max queue: " + maxQueueSize.ToString());
            isInitialized = true;
        }

        // =================================================================
        // UPDATE LOOP
        // =================================================================

        void Update()
        {
            if (!isInitialized) return;

            // Update HUD position to follow player head
            UpdateHUDPosition();

            // Process queue if not currently displaying
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

        private void UpdateHUDPosition()
        {
            if (localPlayer == null || !Utilities.IsValid(localPlayer)) return;

            // Get head tracking data
            VRCPlayerApi.TrackingData headData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Vector3 headPos = headData.position;
            Quaternion headRot = headData.rotation;

            // Calculate target position in front of player
            Vector3 forward = headRot * Vector3.forward;
            Vector3 targetPos = headPos + (forward * hudDistance) + (Vector3.up * hudVerticalOffset);

            // Calculate target rotation to face player
            Quaternion targetRot = Quaternion.LookRotation(targetPos - headPos);

            // Apply smoothing
            if (followSmoothing > 0f)
            {
                hudTransform.position = Vector3.Lerp(hudTransform.position, targetPos, 1f - followSmoothing);
                hudTransform.rotation = Quaternion.Slerp(hudTransform.rotation, targetRot, 1f - followSmoothing);
            }
            else
            {
                hudTransform.position = targetPos;
                hudTransform.rotation = targetRot;
            }
        }

        // =================================================================
        // PUBLIC QUEUE METHODS (Same interface as XboxNotificationUI)
        // =================================================================

        public void QueueAchievementNotification(string playerName, string achievementTitle, int points)
        {
            AddToQueue(0, playerName, achievementTitle, points);
        }

        public void QueueOnlineNotification(string playerName, bool isFirstTime)
        {
            int type = isFirstTime ? 2 : 1;
            AddToQueue(type, playerName, "", 0);
        }

        // =================================================================
        // EVENT WRAPPER METHODS (Same interface as XboxNotificationUI)
        // =================================================================

        public void QueueAchievementNotificationEvent()
        {
            if (queuePlayerName == null || queuePlayerName == "")
            {
                LogDebug("QueueAchievementNotificationEvent: playerName is empty!");
                return;
            }

            if (queueAchievementTitle == null || queueAchievementTitle == "")
            {
                LogDebug("QueueAchievementNotificationEvent: achievementTitle is empty!");
                return;
            }

            LogDebug("QueueAchievementNotificationEvent: " + queuePlayerName + " - " + queueAchievementTitle);

            QueueAchievementNotification(queuePlayerName, queueAchievementTitle, queuePoints);

            // Clear variables
            queuePlayerName = "";
            queueAchievementTitle = "";
            queuePoints = 0;
        }

        public void QueueOnlineNotificationEvent()
        {
            if (queuePlayerName == null || queuePlayerName == "")
            {
                LogDebug("QueueOnlineNotificationEvent: playerName is empty!");
                return;
            }

            LogDebug("QueueOnlineNotificationEvent: " + queuePlayerName + " - FirstTime: " + queueIsFirstTime.ToString());

            QueueOnlineNotification(queuePlayerName, queueIsFirstTime);

            // Clear variables
            queuePlayerName = "";
            queueIsFirstTime = false;
        }

        // =================================================================
        // QUEUE MANAGEMENT (FIFO)
        // =================================================================

        private void AddToQueue(int type, string playerName, string achievementTitle, int points)
        {
            if (queueCount >= maxQueueSize)
            {
                LogDebug("Queue full! Dropping oldest notification");
                RemoveFromQueue(0);
            }

            queueTypes[queueCount] = type;
            queuePlayerNames[queueCount] = playerName;
            queueAchievementTitles[queueCount] = achievementTitle;
            queuePointsArray[queueCount] = points;
            queueCount++;

            LogDebug("Queued notification #" + queueCount.ToString() + " for " + playerName);
        }

        private void RemoveFromQueue(int index)
        {
            if (index < 0 || index >= queueCount) return;

            for (int i = index; i < queueCount - 1; i++)
            {
                queueTypes[i] = queueTypes[i + 1];
                queuePlayerNames[i] = queuePlayerNames[i + 1];
                queueAchievementTitles[i] = queueAchievementTitles[i + 1];
                queuePointsArray[i] = queuePointsArray[i + 1];
            }

            queueCount--;
        }

        // =================================================================
        // NOTIFICATION DISPLAY
        // =================================================================

        private void StartNextNotification()
        {
            if (queueCount == 0) return;

            LogDebug("Starting notification for " + queuePlayerNames[0]);

            SetupNotificationDisplay(0);
            RemoveFromQueue(0);

            isDisplayingNotification = true;
            currentState = 1; // FadingIn
            stateTimer = 0f;
            currentAlpha = 0f;
        }

        private void SetupNotificationDisplay(int queueIndex)
        {
            currentNotificationType = queueTypes[queueIndex];
            string mainTextContent = "";
            string subTextContent = "";

            if (currentNotificationType == 0) // Achievement
            {
                mainTextContent = "Achievement Unlocked";
                subTextContent = queuePlayerNames[queueIndex] + " - " + queueAchievementTitles[queueIndex] + " - " + queuePointsArray[queueIndex].ToString() + "G";
                currentPrimaryIcon = trophyIcon;
                PlaySound(achievementSound);
            }
            else if (currentNotificationType == 2) // FirstTime
            {
                mainTextContent = "Welcome!";
                subTextContent = queuePlayerNames[queueIndex] + " joined for the first time!";
                currentPrimaryIcon = onlineIcon;
                PlaySound(achievementSound);
            }
            else // Online
            {
                mainTextContent = "Online";
                subTextContent = queuePlayerNames[queueIndex] + " is now online";
                currentPrimaryIcon = onlineIcon;
                PlaySound(onlineSound);
            }

            if (mainText != null) mainText.text = mainTextContent;
            if (subText != null) subText.text = subTextContent;

            // Setup icons
            HideAllIcons();
            if (currentPrimaryIcon != null) currentPrimaryIcon.SetActive(true);
            showingXboxIcon = false;
            blinkTimer = 0f;
        }

        private void HideAllIcons()
        {
            if (trophyIcon != null) trophyIcon.SetActive(false);
            if (onlineIcon != null) onlineIcon.SetActive(false);
            if (xboxLogo != null) xboxLogo.SetActive(false);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        // =================================================================
        // ANIMATION SYSTEM
        // =================================================================

        private void UpdateCurrentNotification()
        {
            stateTimer += Time.deltaTime;

            if (currentState == 1) // FadingIn
            {
                currentAlpha = Mathf.Clamp01(stateTimer / fadeInDuration);
                if (canvasGroup != null) canvasGroup.alpha = currentAlpha;

                if (stateTimer >= fadeInDuration)
                {
                    currentState = 2; // Blinking
                    stateTimer = 0f;
                    SetNotificationVisible(true);
                }
            }
            else if (currentState == 2) // Blinking
            {
                blinkTimer += Time.deltaTime;

                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    showingXboxIcon = !showingXboxIcon;

                    if (showingXboxIcon)
                    {
                        if (currentPrimaryIcon != null) currentPrimaryIcon.SetActive(false);
                        if (xboxLogo != null) xboxLogo.SetActive(true);
                    }
                    else
                    {
                        if (xboxLogo != null) xboxLogo.SetActive(false);
                        if (currentPrimaryIcon != null) currentPrimaryIcon.SetActive(true);
                    }
                }

                if (stateTimer >= displayDuration)
                {
                    currentState = 3; // FadingOut
                    stateTimer = 0f;
                }
            }
            else if (currentState == 3) // FadingOut
            {
                currentAlpha = 1f - Mathf.Clamp01(stateTimer / fadeOutDuration);
                if (canvasGroup != null) canvasGroup.alpha = currentAlpha;

                if (stateTimer >= fadeOutDuration)
                {
                    SetNotificationVisible(false);
                    currentState = 4; // QueueDelay
                    stateTimer = 0f;
                }
            }
            else if (currentState == 4) // QueueDelay
            {
                if (stateTimer >= queueDelay)
                {
                    isDisplayingNotification = false;
                    currentState = 0; // Inactive
                    stateTimer = 0f;
                }
            }
        }

        private void SetNotificationVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }

            if (!visible)
            {
                HideAllIcons();
            }
        }

        // =================================================================
        // VALIDATION
        // =================================================================

        private bool ValidateComponents()
        {
            bool isValid = true;

            if (notificationBackground == null)
            {
                LogDebug("Missing: Notification Background");
                isValid = false;
            }
            if (canvasGroup == null)
            {
                LogDebug("Missing: Canvas Group");
                isValid = false;
            }
            if (mainText == null)
            {
                LogDebug("Missing: Main Text");
                isValid = false;
            }
            if (subText == null)
            {
                LogDebug("Missing: Sub Text");
                isValid = false;
            }
            if (xboxLogo == null)
            {
                LogDebug("Warning: Xbox Logo not assigned");
            }
            if (trophyIcon == null)
            {
                LogDebug("Warning: Trophy Icon not assigned");
            }

            return isValid;
        }

        public void ClearQueue()
        {
            queueCount = 0;
            isDisplayingNotification = false;
            currentState = 0;
            SetNotificationVisible(false);
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log("<color=#00BFFF>[HUDNotificationUI] " + message + "</color>");
            }
        }

        // =================================================================
        // TESTING
        // =================================================================

        [ContextMenu("Test Achievement Notification")]
        public void TestAchievementNotification()
        {
            QueueAchievementNotification("TestPlayer", "Basement Dweller", 50);
        }

        [ContextMenu("Test Online Notification")]
        public void TestOnlineNotification()
        {
            QueueOnlineNotification("TestPlayer", false);
        }
    }
}
