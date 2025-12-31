using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// QA test harness for Lower Level 2.0
/// Drives achievement triggers, notifications, terminal refreshes, caching stress,
/// late-join behavior, and “41st player” overflow simulation via Inspector buttons.
/// Attach to an empty GameObject and wire references in the Inspector.
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class AchievementSystemTestHarness : UdonSharpBehaviour
{
    [Header("Core References (assign in Inspector)")]
    [Tooltip("Your NotificationEventHub instance")]
    public UdonBehaviour notificationEventHub;           // NotificationEventHub
    [Tooltip("DOS Terminal Controller (optional but recommended)")]
    public UdonBehaviour dosTerminalController;          // DOSTerminalController
    [Tooltip("DOS Terminal Achievement Module (optional)")]
    public UdonBehaviour dosTerminalAchievementModule;   // DOSTerminalAchievementModule

    [Header("Logging")]
    public bool enableDebugLogging = true;

    [Header("Test Parameters")]
    [Tooltip("Name used when simulating an achievement/login")]
    public string testPlayerName = "TestPlayer";
    [Tooltip("Achievement level index to trigger")]
    public int testAchievementLevel = 1;
    [Tooltip("Achievement type: \"visit\" | \"time\" | \"activity\" | \"default\"")]
    public string testAchievementType = "visit";
    [Tooltip("Simulate 'first time' login")]
    public bool testIsFirstTime = false;

    [Header("Caching / Timing")]
    [Tooltip("How long to wait between repeated triggers in seconds")]
    public float shortDelaySeconds = 1f;
    [Tooltip("Cache TTL window to verify fresh vs cached (5s in your impl)")]
    public float cacheWindowSeconds = 5f;

    [Header("Perf Probe (approximate FPS)")]
    [Tooltip("Enable lightweight FPS sampling during stress tests")]
    public bool sampleFPS = true;

    // ---- internal state ----
    private float _fpsAccum;
    private int _fpsFrames;
    private float _fpsTimer;

    void Update()
    {
        if (!sampleFPS) return;
        _fpsFrames++;
        _fpsAccum += (1f / Mathf.Max(Time.deltaTime, 0.00001f));
        _fpsTimer += Time.deltaTime;
        if (_fpsTimer >= 2f) // print every ~2s
        {
            float avg = _fpsAccum / Mathf.Max(_fpsFrames, 1);
            Log($"[FPS] ~{avg:0.0}");
            _fpsFrames = 0;
            _fpsAccum = 0f;
            _fpsTimer = 0f;
        }
    }

    // =========================================================
    // 1) ACHIEVEMENT TRIGGER VERIFICATION
    // =========================================================
    [ContextMenu("1) Trigger Single Achievement (once)")]
    public void Test_TriggerSingleAchievement()
    {
        if (!HubValid()) return;
        SetHubEvent("eventPlayerName", testPlayerName);
        SetHubEvent("eventAchievementLevel", testAchievementLevel);
        SetHubEvent("eventAchievementType", testAchievementType);
        Log($"[1] TriggerSingleAchievement → {testPlayerName} / L{testAchievementLevel} / {testAchievementType}");
        notificationEventHub.SendCustomEvent("OnAchievementEarned");
    }

    [ContextMenu("1b) Trigger Same Achievement Twice Quickly (duplicate guard)")]
    public void Test_TriggerTwiceQuick()
    {
        if (!HubValid()) return;
        Test_TriggerSingleAchievement();
        SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), shortDelaySeconds);
        Log("[1b] Scheduled second trigger to verify no duplicate earn.");
    }

    // =========================================================
    // 2) PLAYER DATA PERSISTENCE (manual rejoin step)
    // =========================================================
    [ContextMenu("2) Mark Achievement Then Prompt Manual Rejoin")]
    public void Test_PersistenceRoundTrip()
    {
        Test_TriggerSingleAchievement();
        Log("[2] Now leave & rejoin the world. After rejoin, verify the achievement stays unlocked.");
    }

    // =========================================================
    // 3) NOTIFICATION VISUALS
    // (This rides the hub → XboxNotificationUI path)
    // =========================================================
    [ContextMenu("3) Visual Notification Smoke Test")]
    public void Test_NotificationVisual()
    {
        Test_TriggerSingleAchievement();
        Log("[3] Verify popup shows correct title/icon and disappears on time.");
    }

    // =========================================================
    // 4) DOS TERMINAL INTEGRATION
    // =========================================================
    [ContextMenu("4) Terminal Log & Dashboard Refresh")]
    public void Test_TerminalIntegration()
    {
        if (dosTerminalAchievementModule != null)
        {
            dosTerminalAchievementModule.SendCustomEvent("RefreshAchievementData");
            Log("[4] DOSTerminalAchievementModule → RefreshAchievementData()");
        }
        if (dosTerminalController != null)
        {
            // Optional: force dashboard refresh if page==0
            object currentPageObj = dosTerminalController.GetProgramVariable("currentPage");
            if (currentPageObj != null && (int)currentPageObj == 0)
            {
                dosTerminalController.SendCustomEvent("ShowDashboard");
                Log("[4] DOSTerminalController → ShowDashboard() (page 0)");
            }
            else
            {
                Log("[4] Terminal not on page 0; open the dashboard to see immediate updates.");
            }
        }
        else
        {
            LogWarn("[4] No terminal references assigned.");
        }
    }

    // =========================================================
    // 5) ACHIEVEMENT PROGRESSION LOGIC
    // (A → B handoff is owned by your core systems; we just trigger A then prompt to verify B)
    // =========================================================
    [ContextMenu("5) Progression: Trigger A then Inspect B")]
    public void Test_ProgressionChain()
    {
        Test_TriggerSingleAchievement();
        Log("[5] Inspect AchievementTracker/terminal to confirm next achievement in sequence is now active.");
    }

    // =========================================================
    // 6) TRACKER TERMINAL OUTPUT
    // =========================================================
    [ContextMenu("6) Tracker Screen Real-Time Update")]
    public void Test_TrackerTerminalOutput()
    {
        Test_TriggerSingleAchievement();
        SendCustomEventDelayedSeconds(nameof(Test_TerminalIntegration), shortDelaySeconds);
        Log("[6] After unlocking, verify tracker shows updated icon/text, no blanks or lag; page switching remains correct.");
    }

    // =========================================================
    // 7) CACHING SYSTEM (DataManager & Terminal)
    // =========================================================
    [ContextMenu("7) Cache: Hit Within 5s Window")]
    public void Test_CacheWithinWindow()
    {
        // Trigger once, then re-trigger frequently inside cache window to ensure reuse/no log spam
        Test_TriggerSingleAchievement();
        SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), 1f);
        SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), 2f);
        SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), 3f);
        Log("[7] Multiple triggers within cache window → expect cached reads / minimal PlayerData hits.");
    }

    [ContextMenu("7b) Cache: Miss After >5s")]
    public void Test_CacheAfterExpiry()
    {
        Test_TriggerSingleAchievement();
        SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), cacheWindowSeconds + 0.5f);
        Log("[7b] Trigger after cache expiry → expect fresh read.");
    }

    // =========================================================
    // 8) ERROR / EDGE CASES
    // =========================================================
    [ContextMenu("8) Late Join: Broadcast Login Notification")]
    public void Test_LateJoinNotification()
    {
        if (!HubValid()) return;
        SetHubEvent("eventPlayerName", testPlayerName);
        SetHubEventBool("eventIsFirstTime", testIsFirstTime);
        Log($"[8] Simulate OnPlayerJoinedWorld for '{testPlayerName}', firstTime={testIsFirstTime}");
        notificationEventHub.SendCustomEvent("OnPlayerJoinedWorld");
    }

    [ContextMenu("8b) Simulate 41st Player Overflow (safe ignore)")]
    public void Test_Simulate41stOverflow()
    {
        if (!HubValid()) return;

        // Fill trackedPlayerCount up to 40 and ping the hub's own test logger.
        // Note: We cannot call OnPlayerJoined(VRCPlayerApi) directly from here,
        // so we simulate reaching capacity by setting the counter and then invoking hub's test menu if present.
        notificationEventHub.SetProgramVariable("trackedPlayerCount", 40);
        Log("[8b] Forced hub.trackedPlayerCount = 40. Any additional join should be ignored by bounds check.");

        // FIXED: Removed broken SendCustomEvent call - method TestPlayerTrackingLimits doesn't exist in NotificationEventHub
        // TODO: Implement TestPlayerTrackingLimits() in NotificationEventHub if this test is needed
        // notificationEventHub.SendCustomEvent("TestPlayerTrackingLimits");
        Log("[8b] ⚠️ TestPlayerTrackingLimits test disabled - method not implemented");
    }

    // =========================================================
    // 9) PERFORMANCE MONITORING (Quest focus)
    // =========================================================
    [ContextMenu("9) Perf: Stress Notifications for 10s")]
    public void Test_PerfStress()
    {
        // Fire a burst of achievement notifications to exercise UI/terminal throughput
        for (int i = 0; i < 6; i++)
        {
            SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), i * 1.5f);
        }
        Log("[9] Scheduled ~10s of intermittent achievement notifications. Watch FPS and terminal smoothness (especially on Quest).");
    }

    // =========================================================
    // 10) SESSION CLEANUP & EDGE PERSISTENCE
    // =========================================================
    [ContextMenu("10) Near-Miss Then Rejoin (manual step)")]
    public void Test_NearMissThenRejoin()
    {
        // Dev note: you’ll need to manually get yourself close to the criteria (e.g., time-based threshold),
        // then leave and rejoin to ensure it was NOT incorrectly awarded.
        Log("[10] Get near an award threshold, leave & rejoin. Verify it was NOT granted incorrectly.");
    }

    [ContextMenu("10b) Queue Flood Then Flush")]
    public void Test_QueueFloodThenFlush()
    {
        if (!HubValid()) return;

        // Pause → queue a few → resume
        notificationEventHub.SendCustomEvent("PauseNotifications");
        for (int i = 0; i < 5; i++)
        {
            SendCustomEventDelayedSeconds(nameof(Test_TriggerSingleAchievement), i * 0.5f);
        }
        SendCustomEventDelayedSeconds(nameof(__ResumeNotifications), 3f);
        Log("[10b] Queued 5 while paused. Will resume after ~3s → verify queued delivery order and no UI/terminal spam.");
    }
    public void __ResumeNotifications()
    {
        if (!HubValid()) return;
        notificationEventHub.SendCustomEvent("ResumeNotifications");
    }

    [ContextMenu("10c) Cache Cleanup After 30s (observe)")]
    public void Test_CacheCleanupObserve()
    {
        Log("[10c] Do some interactions, wait ~30s, then repeat. Verify stale cache doesn’t leak into UI/logs.");
    }

    // =========================================================
    // OPTIONAL: FINAL PASS CRITERIA QUICK RUN
    // =========================================================
    [ContextMenu("Final Pass: Quick Smoke Suite")]
    public void Test_FinalPassQuick()
    {
        Test_TriggerSingleAchievement();
        SendCustomEventDelayedSeconds(nameof(Test_TerminalIntegration), 0.75f);
        SendCustomEventDelayedSeconds(nameof(Test_NotificationVisual), 1.5f);
        SendCustomEventDelayedSeconds(nameof(Test_ProgressionChain), 2.5f);
        SendCustomEventDelayedSeconds(nameof(Test_CacheWithinWindow), 4.0f);
        SendCustomEventDelayedSeconds(nameof(Test_PerfStress), 8.0f);
        Log("[FINAL] Quick suite scheduled. Validate persistence & Quest perf manually.");
    }

    // ===================== UTILITIES =========================
    private bool HubValid()
    {
        if (notificationEventHub == null)
        {
            LogWarn("Assign NotificationEventHub in Inspector first.");
            return false;
        }
        return true;
    }

    private void SetHubEvent(string varName, string value)
    {
        notificationEventHub.SetProgramVariable(varName, value);
    }
    private void SetHubEvent(string varName, int value)
    {
        notificationEventHub.SetProgramVariable(varName, value);
    }
    private void SetHubEventBool(string varName, bool value)
    {
        notificationEventHub.SetProgramVariable(varName, value);
    }

    private void Log(string msg)
    {
        if (!enableDebugLogging) return;
        Debug.Log($"<color=#88FFAA>🧪[QA] {msg}</color>");
    }
    private void LogWarn(string msg)
    {
        Debug.LogWarning($"<color=#FFDD55>🧪[QA-WARN] {msg}</color>");
    }
}
