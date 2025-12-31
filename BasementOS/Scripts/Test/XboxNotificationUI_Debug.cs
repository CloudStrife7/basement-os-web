using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

/// <summary>
/// COMPONENT PURPOSE:
/// Debug companion for XboxNotificationUI - monitors and logs detailed state information
/// without modifying the original working component
///
/// LOWER LEVEL 2.0 INTEGRATION:
/// Temporary debugging tool to diagnose notification display issues
/// Can be safely added/removed without affecting core functionality
///
/// v2.1 UPDATE:
/// Added FIFO queue testing capabilities to verify timestamp ordering
/// Includes queue order verification and comparison with priority-based behavior
///
/// DEPENDENCIES & REQUIREMENTS:
/// - XboxNotificationUI or XboxNotificationUI_FIFO component on same GameObject
/// - All UI references that XboxNotificationUI uses
/// - Should be removed after debugging is complete
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class XboxNotificationUI_Debug : UdonSharpBehaviour
{
    [Header("Component References")]
    [SerializeField] private UdonBehaviour targetNotificationUI;
    [SerializeField] private GameObject notificationBackground;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject xboxIcon;
    [SerializeField] private GameObject trophyIcon;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI subText;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool enableComponentValidation = true;
    [SerializeField] private bool enableStateMonitoring = true;
    [SerializeField] private bool enableVisualInspection = true;
    [SerializeField] private bool enableQueueTracking = true;

    [Header("FIFO Testing")]
    [Tooltip("Track notification display order for FIFO verification")]
    [SerializeField] private bool trackNotificationOrder = true;

    // Monitoring variables
    private bool isInitialized = false;
    private int lastKnownQueueCount = 0;
    private float lastAlphaValue = 0f;
    private bool lastNotificationBackgroundState = false;
    private bool lastXboxIconState = false;
    private bool lastTrophyIconState = false;
    private string lastMainTextContent = "";
    private string lastSubTextContent = "";

    // FIFO testing variables
    private string[] notificationOrderLog;
    private int notificationOrderCount = 0;
    private const int MAX_ORDER_LOG = 20;
    private float lastNotificationTime = 0f;

    void Start()
    {
        InitializeDebugger();
    }

    void InitializeDebugger()
    {
        LogDebug("=== XboxNotificationUI Debugger v2.1 Starting ===");

        // Initialize FIFO tracking
        notificationOrderLog = new string[MAX_ORDER_LOG];
        notificationOrderCount = 0;

        // Check if target is assigned
        if (targetNotificationUI == null)
        {
            LogDebug("ERROR: Target Notification UI not assigned! Please assign it in Inspector.");
            enabled = false;
            return;
        }

        LogDebug("Target XboxNotificationUI component found");

        // Validate all components
        if (enableComponentValidation)
        {
            ValidateAllComponents();
        }

        isInitialized = true;
        LogDebug("=== XboxNotificationUI Debugger Ready ===");
    }

    void Update()
    {
        if (!isInitialized || !enableStateMonitoring) return;

        MonitorComponentState();

        // Track notification display order for FIFO verification
        if (trackNotificationOrder && enableQueueTracking)
        {
            TrackNotificationDisplay();
        }
    }

    private void ValidateAllComponents()
    {
        LogDebug("=== COMPONENT VALIDATION ===");

        // Check XboxNotificationUI
        bool hasTarget = targetNotificationUI != null;
        bool targetEnabled = false;
        if (hasTarget)
        {
            targetEnabled = targetNotificationUI.enabled;
        }
        LogDebug("XboxNotificationUI: " + hasTarget + " (Enabled: " + targetEnabled + ")");

        // Check UI References
        bool hasBg = notificationBackground != null;
        bool bgActive = false;
        if (hasBg)
        {
            bgActive = notificationBackground.activeSelf;
        }
        LogDebug("NotificationBackground: " + hasBg + " (Active: " + bgActive + ")");

        bool hasCG = canvasGroup != null;
        float cgAlpha = 0f;
        if (hasCG)
        {
            cgAlpha = canvasGroup.alpha;
        }
        LogDebug("CanvasGroup: " + hasCG + " (Alpha: " + cgAlpha + ")");

        bool hasXbox = xboxIcon != null;
        bool xboxActive = false;
        if (hasXbox)
        {
            xboxActive = xboxIcon.activeSelf;
        }
        LogDebug("XboxIcon: " + hasXbox + " (Active: " + xboxActive + ")");

        bool hasTrophy = trophyIcon != null;
        bool trophyActive = false;
        if (hasTrophy)
        {
            trophyActive = trophyIcon.activeSelf;
        }
        LogDebug("TrophyIcon: " + hasTrophy + " (Active: " + trophyActive + ")");

        bool hasMain = mainText != null;
        string mainTextStr = "";
        if (hasMain)
        {
            mainTextStr = mainText.text;
        }
        LogDebug("MainText: " + hasMain + " (Text: '" + mainTextStr + "')");

        bool hasSub = subText != null;
        string subTextStr = "";
        if (hasSub)
        {
            subTextStr = subText.text;
        }
        LogDebug("SubText: " + hasSub + " (Text: '" + subTextStr + "')");

        // Check GameObject hierarchy
        LogDebug("=== HIERARCHY CHECK ===");
        LogDebug("This GameObject: " + gameObject.name + " (Active: " + gameObject.activeSelf + ")");

        bool hasParent = transform.parent != null;
        if (hasParent)
        {
            LogDebug("Parent: " + transform.parent.name + " (Active: " + transform.parent.gameObject.activeSelf + ")");
        }
        else
        {
            LogDebug("Parent: None");
        }

        LogDebug("=== VALIDATION COMPLETE ===");
    }

    private void MonitorComponentState()
    {
        // Monitor visual state changes
        if (enableVisualInspection)
        {
            CheckForStateChanges();
        }
    }

    private void CheckForStateChanges()
    {
        // Check CanvasGroup alpha changes
        if (canvasGroup != null)
        {
            float currentAlpha = canvasGroup.alpha;
            if (currentAlpha != lastAlphaValue)
            {
                LogDebug("ALPHA CHANGE: " + lastAlphaValue + " → " + currentAlpha);
                lastAlphaValue = currentAlpha;
            }
        }

        // Check GameObject active states
        bool currentBgState = false;
        if (notificationBackground != null)
        {
            currentBgState = notificationBackground.activeSelf;
        }
        if (currentBgState != lastNotificationBackgroundState)
        {
            LogDebug("BACKGROUND STATE: " + lastNotificationBackgroundState + " → " + currentBgState);
            lastNotificationBackgroundState = currentBgState;
        }

        bool currentXboxState = false;
        if (xboxIcon != null)
        {
            currentXboxState = xboxIcon.activeSelf;
        }
        if (currentXboxState != lastXboxIconState)
        {
            LogDebug("XBOX ICON STATE: " + lastXboxIconState + " → " + currentXboxState);
            lastXboxIconState = currentXboxState;
        }

        bool currentTrophyState = false;
        if (trophyIcon != null)
        {
            currentTrophyState = trophyIcon.activeSelf;
        }
        if (currentTrophyState != lastTrophyIconState)
        {
            LogDebug("TROPHY ICON STATE: " + lastTrophyIconState + " → " + currentTrophyState);
            lastTrophyIconState = currentTrophyState;
        }

        // Check text content changes
        string currentMainText = "";
        if (mainText != null)
        {
            currentMainText = mainText.text;
        }
        if (currentMainText != lastMainTextContent)
        {
            LogDebug("MAIN TEXT CHANGE: '" + lastMainTextContent + "' → '" + currentMainText + "'");
            lastMainTextContent = currentMainText;
        }

        string currentSubText = "";
        if (subText != null)
        {
            currentSubText = subText.text;
        }
        if (currentSubText != lastSubTextContent)
        {
            LogDebug("SUB TEXT CHANGE: '" + lastSubTextContent + "' → '" + currentSubText + "'");
            lastSubTextContent = currentSubText;
        }
    }

    // =================================================================
    // FIFO QUEUE TESTING METHODS
    // =================================================================

    private void TrackNotificationDisplay()
    {
        // Detect when a new notification starts displaying (alpha goes from 0 to non-zero)
        if (canvasGroup != null && canvasGroup.alpha > 0.1f && lastAlphaValue < 0.1f)
        {
            // New notification is displaying
            if (mainText != null && Time.time - lastNotificationTime > 0.5f)
            {
                string notificationInfo = mainText.text;
                if (subText != null)
                {
                    notificationInfo = notificationInfo + " | " + subText.text;
                }

                RecordNotificationOrder(notificationInfo);
                lastNotificationTime = Time.time;
            }
        }
    }

    private void RecordNotificationOrder(string notificationInfo)
    {
        if (notificationOrderCount < MAX_ORDER_LOG)
        {
            notificationOrderLog[notificationOrderCount] = notificationInfo;
            notificationOrderCount++;
            LogDebug("🔢 NOTIFICATION #" + notificationOrderCount + ": " + notificationInfo);
        }
    }

    /// <summary>
    /// Test FIFO ordering by queuing 3 notifications in sequence
    /// Expected: Regular → Achievement → Pwnerer (FIFO)
    /// Old behavior: Achievement → Regular → Pwnerer (priority)
    /// </summary>
    [ContextMenu("Test FIFO Order - 3 Notifications")]
    public void TestFIFOOrder()
    {
        LogDebug("=== FIFO ORDER TEST START ===");
        LogDebug("Queuing 3 notifications in order:");
        LogDebug("  1. Online (Regular Player)");
        LogDebug("  2. Achievement (TestPlayer)");
        LogDebug("  3. Online (Pwnerer)");
        LogDebug("");
        LogDebug("EXPECTED FIFO: Regular → Achievement → Pwnerer");
        LogDebug("OLD PRIORITY: Pwnerer → Achievement → Regular");
        LogDebug("");

        ClearNotificationLog();

        if (targetNotificationUI != null)
        {
            // Queue notification 1: Regular player login (lowest priority in old system: 25)
            targetNotificationUI.SetProgramVariable("queuePlayerName", "RegularPlayer");
            targetNotificationUI.SetProgramVariable("queueIsFirstTime", false);
            targetNotificationUI.SendCustomEvent("QueueOnlineNotificationEvent");
            LogDebug("✅ Queued: Regular Player (Online)");

            // Small delay to ensure timestamp ordering
            SendCustomEventDelayedFrames(nameof(QueueSecondNotification), 5);
        }
        else
        {
            LogDebug("❌ ERROR: targetNotificationUI is null!");
        }
    }

    public void QueueSecondNotification()
    {
        // Queue notification 2: Achievement (medium priority in old system: 50)
        if (targetNotificationUI != null)
        {
            targetNotificationUI.SetProgramVariable("queuePlayerName", "TestPlayer");
            targetNotificationUI.SetProgramVariable("queueAchievementTitle", "Basement Dweller");
            targetNotificationUI.SetProgramVariable("queuePoints", 50);
            targetNotificationUI.SendCustomEvent("QueueAchievementNotificationEvent");
            LogDebug("✅ Queued: Achievement (TestPlayer - Basement Dweller)");

            SendCustomEventDelayedFrames(nameof(QueueThirdNotification), 5);
        }
    }

    public void QueueThirdNotification()
    {
        // Queue notification 3: Pwnerer login (highest priority in old system: 100)
        if (targetNotificationUI != null)
        {
            targetNotificationUI.SetProgramVariable("queuePlayerName", "Pwnerer");
            targetNotificationUI.SetProgramVariable("queueIsFirstTime", false);
            targetNotificationUI.SendCustomEvent("QueueOnlineNotificationEvent");
            LogDebug("✅ Queued: Pwnerer (Online)");

            LogDebug("");
            LogDebug("All 3 notifications queued! Watch the display order.");
            LogDebug("Will check results in 20 seconds...");

            SendCustomEventDelayedSeconds(nameof(VerifyFIFOResults), 20.0f);
        }
    }

    public void VerifyFIFOResults()
    {
        LogDebug("");
        LogDebug("=== FIFO ORDER TEST RESULTS ===");
        LogDebug("Notifications displayed in this order:");

        for (int i = 0; i < notificationOrderCount && i < 3; i++)
        {
            LogDebug("  " + (i + 1) + ". " + notificationOrderLog[i]);
        }

        LogDebug("");
        LogDebug("EXPECTED FIFO ORDER:");
        LogDebug("  1. Online | RegularPlayer is now online");
        LogDebug("  2. Achievement Unlocked | TestPlayer - Basement Dweller - 50G");
        LogDebug("  3. Online | Pwnerer is now online");

        LogDebug("");
        if (notificationOrderCount >= 3)
        {
            bool isFIFO = CheckIfDisplayedInFIFO();
            if (isFIFO)
            {
                LogDebug("✅ TEST PASSED: Notifications displayed in FIFO order!");
            }
            else
            {
                LogDebug("❌ TEST FAILED: Notifications NOT in FIFO order (priority-based)");
            }
        }
        else
        {
            LogDebug("⚠️ WARNING: Only " + notificationOrderCount + " notifications logged (expected 3)");
        }

        LogDebug("=== FIFO ORDER TEST COMPLETE ===");
    }

    private bool CheckIfDisplayedInFIFO()
    {
        if (notificationOrderCount < 3) return false;

        // Check if first notification contains "RegularPlayer" or "Online"
        bool firstIsRegular = notificationOrderLog[0].Contains("RegularPlayer") ||
                             (notificationOrderLog[0].Contains("Online") && !notificationOrderLog[0].Contains("Pwnerer"));

        // Check if second notification is achievement
        bool secondIsAchievement = notificationOrderLog[1].Contains("Achievement") ||
                                   notificationOrderLog[1].Contains("Basement Dweller");

        // Check if third notification contains "Pwnerer"
        bool thirdIsPwnerer = notificationOrderLog[2].Contains("Pwnerer");

        return firstIsRegular && secondIsAchievement && thirdIsPwnerer;
    }

    /// <summary>
    /// Test queue overflow behavior - should drop oldest (FIFO) not lowest priority
    /// </summary>
    [ContextMenu("Test Queue Overflow (11 Notifications)")]
    public void TestQueueOverflow()
    {
        LogDebug("=== QUEUE OVERFLOW TEST START ===");
        LogDebug("Queuing 11 notifications (max queue = 10)");
        LogDebug("");
        LogDebug("FIFO Expected: First notification dropped");
        LogDebug("Priority Expected: Lowest priority notification dropped");

        ClearNotificationLog();

        if (targetNotificationUI != null)
        {
            // Queue 11 notifications rapidly
            for (int i = 0; i < 11; i++)
            {
                targetNotificationUI.SetProgramVariable("queuePlayerName", "Player" + i);
                targetNotificationUI.SetProgramVariable("queueIsFirstTime", false);
                targetNotificationUI.SendCustomEvent("QueueOnlineNotificationEvent");
                LogDebug("Queued: Player" + i);
            }

            LogDebug("");
            LogDebug("All 11 notifications queued!");
            LogDebug("Watch console for which one gets dropped...");
        }
    }

    /// <summary>
    /// Rapid spam test - 20 notifications in quick succession
    /// </summary>
    [ContextMenu("Test Rapid Notification Spam")]
    public void TestRapidSpam()
    {
        LogDebug("=== RAPID SPAM TEST START ===");
        LogDebug("Queuing 20 notifications rapidly");

        ClearNotificationLog();

        if (targetNotificationUI != null)
        {
            for (int i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    // Online notification
                    targetNotificationUI.SetProgramVariable("queuePlayerName", "SpamPlayer" + i);
                    targetNotificationUI.SetProgramVariable("queueIsFirstTime", false);
                    targetNotificationUI.SendCustomEvent("QueueOnlineNotificationEvent");
                }
                else
                {
                    // Achievement notification
                    targetNotificationUI.SetProgramVariable("queuePlayerName", "SpamPlayer" + i);
                    targetNotificationUI.SetProgramVariable("queueAchievementTitle", "Spam Test");
                    targetNotificationUI.SetProgramVariable("queuePoints", 10);
                    targetNotificationUI.SendCustomEvent("QueueAchievementNotificationEvent");
                }
            }

            LogDebug("20 notifications queued! Check for stability...");
        }
    }

    [ContextMenu("Clear Notification Log")]
    public void ClearNotificationLog()
    {
        notificationOrderCount = 0;
        for (int i = 0; i < MAX_ORDER_LOG; i++)
        {
            notificationOrderLog[i] = "";
        }
        LogDebug("Notification order log cleared");
    }

    [ContextMenu("Show Notification Order Log")]
    public void ShowNotificationOrderLog()
    {
        LogDebug("=== NOTIFICATION ORDER LOG ===");
        LogDebug("Total notifications logged: " + notificationOrderCount);

        for (int i = 0; i < notificationOrderCount; i++)
        {
            LogDebug("  " + (i + 1) + ". " + notificationOrderLog[i]);
        }

        LogDebug("=== END OF LOG ===");
    }

    // =================================================================
    // ORIGINAL DEBUG METHODS
    // =================================================================

    public void DebugQueueAchievement()
    {
        LogDebug("=== DEBUG QUEUE ACHIEVEMENT CALLED ===");
        LogDebug("About to call targetNotificationUI.TestAchievementNotification...");

        if (targetNotificationUI != null)
        {
            targetNotificationUI.SendCustomEvent("TestAchievementNotification");
            LogDebug("TestAchievementNotification called successfully");

            // Start monitoring for changes
            StartDetailedMonitoring();
        }
        else
        {
            LogDebug("ERROR: targetNotificationUI is null!");
        }
    }

    private void StartDetailedMonitoring()
    {
        LogDebug("=== STARTING DETAILED MONITORING ===");

        // Capture current state
        LogDebug("Current State Snapshot:");

        string alphaStr = "unknown";
        if (canvasGroup != null)
        {
            alphaStr = canvasGroup.alpha.ToString();
        }
        LogDebug("  CanvasGroup Alpha: " + alphaStr);

        string bgActiveStr = "unknown";
        if (notificationBackground != null)
        {
            bgActiveStr = notificationBackground.activeSelf.ToString();
        }
        LogDebug("  Background Active: " + bgActiveStr);

        string xboxActiveStr = "unknown";
        if (xboxIcon != null)
        {
            xboxActiveStr = xboxIcon.activeSelf.ToString();
        }
        LogDebug("  Xbox Icon Active: " + xboxActiveStr);

        string trophyActiveStr = "unknown";
        if (trophyIcon != null)
        {
            trophyActiveStr = trophyIcon.activeSelf.ToString();
        }
        LogDebug("  Trophy Icon Active: " + trophyActiveStr);

        string mainTextStr = "unknown";
        if (mainText != null)
        {
            mainTextStr = mainText.text;
        }
        LogDebug("  Main Text: '" + mainTextStr + "'");

        string subTextStr = "unknown";
        if (subText != null)
        {
            subTextStr = subText.text;
        }
        LogDebug("  Sub Text: '" + subTextStr + "'");

        // Schedule follow-up checks
        SendCustomEventDelayedFrames(nameof(CheckAfter1Frame), 1);
        SendCustomEventDelayedFrames(nameof(CheckAfter30Frames), 30);
        SendCustomEventDelayedFrames(nameof(CheckAfter60Frames), 60);
    }

    public void CheckAfter1Frame()
    {
        LogDebug("=== STATE CHECK: 1 Frame Later ===");
        LogCurrentState();
    }

    public void CheckAfter30Frames()
    {
        LogDebug("=== STATE CHECK: 30 Frames Later ===");
        LogCurrentState();
    }

    public void CheckAfter60Frames()
    {
        LogDebug("=== STATE CHECK: 60 Frames Later ===");
        LogCurrentState();
    }

    private void LogCurrentState()
    {
        string alphaStr = "unknown";
        if (canvasGroup != null)
        {
            alphaStr = canvasGroup.alpha.ToString();
        }
        LogDebug("  CanvasGroup Alpha: " + alphaStr);

        string bgActiveStr = "unknown";
        if (notificationBackground != null)
        {
            bgActiveStr = notificationBackground.activeSelf.ToString();
        }
        LogDebug("  Background Active: " + bgActiveStr);

        string xboxActiveStr = "unknown";
        if (xboxIcon != null)
        {
            xboxActiveStr = xboxIcon.activeSelf.ToString();
        }
        LogDebug("  Xbox Icon Active: " + xboxActiveStr);

        string trophyActiveStr = "unknown";
        if (trophyIcon != null)
        {
            trophyActiveStr = trophyIcon.activeSelf.ToString();
        }
        LogDebug("  Trophy Icon Active: " + trophyActiveStr);

        string mainTextStr = "unknown";
        if (mainText != null)
        {
            mainTextStr = mainText.text;
        }
        LogDebug("  Main Text: '" + mainTextStr + "'");

        string subTextStr = "unknown";
        if (subText != null)
        {
            subTextStr = subText.text;
        }
        LogDebug("  Sub Text: '" + subTextStr + "'");

        // Check if anything is actually visible
        bool somethingVisible = false;
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            somethingVisible = true;
        }
        if (notificationBackground != null && notificationBackground.activeSelf)
        {
            somethingVisible = true;
        }
        if (xboxIcon != null && xboxIcon.activeSelf)
        {
            somethingVisible = true;
        }
        if (trophyIcon != null && trophyIcon.activeSelf)
        {
            somethingVisible = true;
        }
        LogDebug("  SOMETHING VISIBLE: " + somethingVisible);
    }

    public void ForceVisibilityTest()
    {
        LogDebug("=== FORCE VISIBILITY TEST ===");

        if (canvasGroup != null)
        {
            LogDebug("Setting CanvasGroup alpha to 1.0...");
            canvasGroup.alpha = 1.0f;
        }

        if (notificationBackground != null)
        {
            LogDebug("Activating notification background...");
            notificationBackground.SetActive(true);
        }

        if (xboxIcon != null)
        {
            LogDebug("Activating xbox icon...");
            xboxIcon.SetActive(true);
        }

        if (mainText != null)
        {
            LogDebug("Setting main text...");
            mainText.text = "DEBUG TEST";
        }

        if (subText != null)
        {
            LogDebug("Setting sub text...");
            subText.text = "Visibility Test";
        }

        LogDebug("Force visibility test complete - check if notification is now visible!");
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log("[XboxNotificationUI_DEBUG v2.1] " + message);
        }
    }

    [ContextMenu("Run Component Validation")]
    public void TestValidateComponents()
    {
        ValidateAllComponents();
    }

    [ContextMenu("Debug Queue Achievement")]
    public void TestDebugQueueAchievement()
    {
        DebugQueueAchievement();
    }

    [ContextMenu("Force Visibility Test")]
    public void TestForceVisibility()
    {
        ForceVisibilityTest();
    }

    [ContextMenu("Check Current State")]
    public void TestCurrentState()
    {
        LogDebug("=== MANUAL STATE CHECK ===");
        LogCurrentState();
    }

    [ContextMenu("Enable All Debug Options")]
    public void EnableAllDebug()
    {
        enableDebugLogging = true;
        enableComponentValidation = true;
        enableStateMonitoring = true;
        enableVisualInspection = true;
        enableQueueTracking = true;
        trackNotificationOrder = true;
        LogDebug("All debug options enabled (including FIFO tracking)");
    }
}
