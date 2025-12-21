using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace LowerLevel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CinematicChairActivation : UdonSharpBehaviour
    {
        [Header("🎬 CINEMATIC CEREMONY SYSTEM v2.0")]
        [Header("═══════════════════════════════════════")]

        [Header("🎵 Section 1 - Music Control")]
        [SerializeField] private UdonBehaviour proTVPlaylist;

        [Header("📢 Section 0 - Notification Control")]
        [SerializeField] private UdonBehaviour notificationHub;
        [SerializeField] private float notificationResumeTime = 15f;

        [Header("🖥️ Section 2a - ProTV Quad Fade")]
        [SerializeField] private CanvasGroup proTVOverlayCanvasGroup;
        [SerializeField] private float proTVFadeStartTime = 3f;
        [SerializeField] private float proTVFadeDuration = 3f;

        [Header("🖥️ Section 2b - H3Menu Quad Fade")]
        [SerializeField] private CanvasGroup h3MenuOverlayCanvasGroup;
        [SerializeField] private float h3MenuFadeStartTime = 3f;
        [SerializeField] private float h3MenuFadeDuration = 3f;

        [Header("💨 Section 2c - Fan Control")]
        [SerializeField] private GameObject fanAnimatorObject;
        [SerializeField] private float fanStartTime = 5f;

        [Header("🌊 Section 2d - Hot Tub Quad Pulse")]
        [SerializeField] private CanvasGroup hotTubOverlayCanvasGroup;
        [SerializeField] private AudioSource hotTubAudioSource;
        [SerializeField] private AudioClip hotTubSound;
        [SerializeField] private float hotTubPulseStartTime = 6f;
        [SerializeField] private float hotTubPulseDuration = 12f;

        [Header("💻 Section 2e - Terminal Control")]
        [SerializeField] private CanvasGroup terminalBlockerCanvasGroup;
        [SerializeField] private GameObject spotLight;
        [SerializeField] private float terminalFadeStartTime = 8f;
        [SerializeField] private float terminalFadeDuration = 0.5f;
        [SerializeField] private float spotLightEnableTime = 8f;
        [SerializeField] private bool spotLightStartsDisabled = true;

        [Header("🚪 Section 2f - Door Control")]
        [SerializeField] private GameObject doorClosedState;
        [SerializeField] private GameObject doorOpenState;
        [SerializeField] private float doorSwitchTime = 12f;

        [Header("🔴 Section 3 - Button Management")]
        [SerializeField] private GameObject ceremonyButton;
        [SerializeField] private string myUsername = "GameFuel"; // Set your username here

        [Header("⚙️ Ceremony Settings")]
        [SerializeField] private bool showTimerDebug = true;
        [SerializeField] private bool pauseNotificationsOnStart = true;

        // Timer system state
        private bool ceremonyActive = false;
        private float ceremonyStartTime = 0f;
        private bool ceremonyHasBeenRun = false;

        // Individual section completion tracking
        private bool notificationsPaused = false;
        private bool musicStarted = false;
        private bool proTVFadeStarted = false;
        private bool h3MenuFadeStarted = false;
        private bool fanStarted = false;
        private bool hotTubPulseStarted = false;
        private bool terminalFadeStarted = false;
        private bool spotLightEnabled = false;
        private bool doorSwitched = false;
        private bool notificationsResumed = false;

        // Fade tracking for multiple simultaneous fades (using parallel arrays for UdonSharp)
        private CanvasGroup[] fadeTargets = new CanvasGroup[4];
        private float[] fadeStartTimes = new float[4];
        private float[] fadeDurations = new float[4];
        private float[] fadeStartAlphas = new float[4];
        private float[] fadeEndAlphas = new float[4];
        private bool[] fadeActives = new bool[4];
        private string[] fadeNames = new string[4];

        // Hot tub pulse state
        private bool hotTubPulsing = false;
        private float hotTubPulseStartTimeActual = 0f;

        // Network sync
        [UdonSynced] private bool syncCeremonyActive = false;
        [UdonSynced] private bool syncCeremonyHasBeenRun = false;
        [UdonSynced] private string syncInstigatorName = "";

        // NEW: deterministic start timestamp (server time, ms)
        [UdonSynced] private int syncCeremonyStartMs = 0;

        // =================================================================
        // 🎯 INITIALIZATION
        // =================================================================

        void Start()
        {
            LogDebug("🎬 Cinematic Chair Activation System v2.0 initialized");

            if (pauseNotificationsOnStart && notificationHub != null)
            {
                notificationHub.SendCustomEvent("PauseNotifications");
                LogDebug("📢 Attempted immediate notification pause on Start()");
                SendCustomEventDelayedSeconds(nameof(PauseNotificationsOnJoin), 0.5f);
            }

            for (int i = 0; i < 4; i++)
            {
                fadeTargets[i] = null;
                fadeStartTimes[i] = 0f;
                fadeDurations[i] = 0f;
                fadeStartAlphas[i] = 0f;
                fadeEndAlphas[i] = 0f;
                fadeActives[i] = false;
                fadeNames[i] = "";
            }

            if (spotLightStartsDisabled && spotLight != null)
            {
                spotLight.SetActive(false);
                LogDebug("💡 Spot light initialized as disabled");
            }

            CheckButtonVisibility();
            DebugCanvasGroupStates();
        }

        public void PauseNotificationsOnJoin()
        {
            if (notificationHub != null)
            {
                notificationHub.SendCustomEvent("PauseNotifications");
                LogDebug("📢 Backup notification pause executed (0.5s delay)");
            }
        }

        // =================================================================
        // 🔴 BUTTON MANAGEMENT SYSTEM
        // =================================================================

        private void CheckButtonVisibility()
        {
            if (ceremonyButton == null) return;

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;

            bool shouldShowButton = false;

            if (IsTargetUser(localPlayer) && !ceremonyHasBeenRun && !syncCeremonyHasBeenRun)
            {
                shouldShowButton = true;
                LogDebug("🔴 Button visible for: " + localPlayer.displayName);
            }
            else
            {
                LogDebug("🔴 Button hidden - User: " + localPlayer.displayName + ", CeremonyRun: " + (ceremonyHasBeenRun || syncCeremonyHasBeenRun).ToString());
            }

            ceremonyButton.SetActive(shouldShowButton);
        }

        private bool IsTargetUser(VRCPlayerApi player)
        {
            if (player == null || string.IsNullOrEmpty(myUsername)) return false;
            return player.displayName.Equals(myUsername, System.StringComparison.OrdinalIgnoreCase);
        }

        private void HideButton()
        {
            if (ceremonyButton != null)
            {
                ceremonyButton.SetActive(false);
                LogDebug("🔴 Button hidden after ceremony activation");
            }
        }

        // =================================================================
        // 🌐 LATE JOINER MANAGEMENT
        // =================================================================

        private void SetPostCeremonyState()
        {
            LogDebug("🌐 Setting post-ceremony state for late joiner");

            if (proTVOverlayCanvasGroup != null) proTVOverlayCanvasGroup.alpha = 0f;
            if (h3MenuOverlayCanvasGroup != null) h3MenuOverlayCanvasGroup.alpha = 0f;
            if (terminalBlockerCanvasGroup != null) terminalBlockerCanvasGroup.alpha = 0f;
            if (hotTubOverlayCanvasGroup != null) hotTubOverlayCanvasGroup.alpha = 0f;

            if (doorClosedState != null && doorOpenState != null)
            {
                doorClosedState.SetActive(false);
                doorOpenState.SetActive(true);
            }

            if (fanAnimatorObject != null)
            {
                Animator fanAnimator = fanAnimatorObject.GetComponent<Animator>();
                if (fanAnimator != null) fanAnimator.enabled = true;
            }

            if (spotLight != null) spotLight.SetActive(true);

            LogDebug("✅ Post-ceremony state set for late joiner");
        }

        // =================================================================
        // 🎬 MAIN CEREMONY ACTIVATION
        // =================================================================

        public override void Interact()
        {
            StartCinematicCeremony();
        }

        public void OnInteract()
        {
            StartCinematicCeremony();
        }

        public void StartCinematicCeremony()
        {
            if (ceremonyActive || syncCeremonyHasBeenRun)
            {
                LogDebug("⚠️ Ceremony already active or completed, ignoring new request");
                return;
            }

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (localPlayer == null) return;

            // Ensure owner, then retry next frame (first-press reliability)
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(localPlayer, gameObject);
                SendCustomEventDelayedFrames(nameof(StartCinematicCeremony), 1);
                return;
            }

            // Network sync state + deterministic start timestamp
            syncCeremonyActive = true;
            syncCeremonyHasBeenRun = true;
            syncInstigatorName = localPlayer.displayName;
            syncCeremonyStartMs = Networking.GetServerTimeInMilliseconds();
            RequestSerialization();

            // Local start
            ceremonyHasBeenRun = true;
            HideButton();

            LogDebug("🚀 INITIATING EPIC FOUNDER'S NIGHT CEREMONY!");
            BeginCinematicSequence();
        }

        private void BeginCinematicSequence()
        {
            ceremonyActive = true;
            ceremonyStartTime = Time.time;

            LogDebug("🎼 Beginning cinematic sequence with modular sections");
            LogDebug("⏰ All sections will execute based on their configured timing!");
        }

        // =================================================================
        // 🔄 UPDATE LOOP - RELIABLE TIMER SYSTEM
        // =================================================================

        void Update()
        {
            if (!ceremonyActive) return;

            float elapsed = Time.time - ceremonyStartTime;

            if (showTimerDebug && Time.frameCount % 60 == 0)
            {
                LogDebug("⏱️ Ceremony Timer: " + elapsed.ToString("F1") + "s elapsed");
            }

            if (!notificationsPaused) ExecuteSection0_PauseNotifications();
            if (!musicStarted) ExecuteSection1_StartMusic();

            if (!proTVFadeStarted && elapsed >= proTVFadeStartTime) ExecuteSection2a_ProTVFade();
            if (!h3MenuFadeStarted && elapsed >= h3MenuFadeStartTime) ExecuteSection2b_H3MenuFade();
            if (!fanStarted && elapsed >= fanStartTime) ExecuteSection2c_FanControl();
            if (!hotTubPulseStarted && elapsed >= hotTubPulseStartTime) ExecuteSection2d_HotTubPulse();
            if (!terminalFadeStarted && elapsed >= terminalFadeStartTime) ExecuteSection2e_TerminalControl();
            if (!spotLightEnabled && elapsed >= spotLightEnableTime) EnableSpotLight();
            if (!doorSwitched && elapsed >= doorSwitchTime) ExecuteSection2f_DoorControl();
            if (!notificationsResumed && elapsed >= notificationResumeTime) ExecuteSection3_ResumeNotifications();

            if (elapsed >= notificationResumeTime + 1f)
            {
                LogDebug("🎉 CEREMONY SEQUENCE COMPLETE!");
                ceremonyActive = false;
            }
        }

        void LateUpdate()
        {
            UpdateAllFades();
            UpdateHotTubPulse();
        }

        // =================================================================
        // 📋 SECTION IMPLEMENTATIONS
        // =================================================================

        private void ExecuteSection0_PauseNotifications()
        {
            if (notificationHub != null)
            {
                notificationHub.SendCustomEvent("PauseNotifications");
                LogDebug("📢 SECTION 0: Notifications paused");
            }
            notificationsPaused = true;
        }

        private void ExecuteSection1_StartMusic()
        {
            if (proTVPlaylist != null)
            {
                proTVPlaylist.SendCustomEvent("_OnMediaEnd");
                proTVPlaylist.SendCustomEvent("_PlayCurrent");
                proTVPlaylist.SendCustomEvent("_Next");
                LogDebug("🎵 SECTION 1: ProTV playlist triggered with multiple methods");
            }
            musicStarted = true;
        }

        private void ExecuteSection2a_ProTVFade()
        {
            if (proTVOverlayCanvasGroup != null)
            {
                GameObject proTVObject = proTVOverlayCanvasGroup.gameObject;
                if (!proTVObject.activeSelf) proTVObject.SetActive(true);

                proTVOverlayCanvasGroup.alpha = 1f;
                AddFade(proTVOverlayCanvasGroup, 1f, 0f, proTVFadeDuration, "ProTV Overlay");
                LogDebug("🖥️ SECTION 2a: ProTV fade started - " + proTVFadeDuration.ToString() + "s duration");
            }
            else
            {
                LogDebug("❌ SECTION 2a: ProTV overlay CanvasGroup is null!");
            }
            proTVFadeStarted = true;
        }

        private void ExecuteSection2b_H3MenuFade()
        {
            if (h3MenuOverlayCanvasGroup != null)
            {
                GameObject h3MenuObject = h3MenuOverlayCanvasGroup.gameObject;
                if (!h3MenuObject.activeSelf) h3MenuObject.SetActive(true);

                h3MenuOverlayCanvasGroup.alpha = 1f;
                AddFade(h3MenuOverlayCanvasGroup, 1f, 0f, h3MenuFadeDuration, "H3Menu Overlay");
                LogDebug("🖥️ SECTION 2b: H3Menu fade started - " + h3MenuFadeDuration.ToString() + "s duration");
            }
            else
            {
                LogDebug("❌ SECTION 2b: H3Menu overlay CanvasGroup is null!");
            }
            h3MenuFadeStarted = true;
        }

        private void ExecuteSection2c_FanControl()
        {
            if (fanAnimatorObject != null)
            {
                Animator fanAnimator = fanAnimatorObject.GetComponent<Animator>();
                if (fanAnimator != null)
                {
                    fanAnimator.enabled = true;
                    LogDebug("💨 SECTION 2c: Fan animator enabled");
                }
            }
            fanStarted = true;
        }

        private void ExecuteSection2d_HotTubPulse()
        {
            if (hotTubOverlayCanvasGroup != null)
            {
                GameObject hotTubObject = hotTubOverlayCanvasGroup.gameObject;
                if (!hotTubObject.activeSelf) hotTubObject.SetActive(true);

                hotTubOverlayCanvasGroup.alpha = 0f;
                hotTubPulsing = true;
                hotTubPulseStartTimeActual = Time.time;

                if (hotTubAudioSource != null && hotTubSound != null)
                {
                    hotTubAudioSource.PlayOneShot(hotTubSound);
                    LogDebug("🔊 Hot tub sound playing");
                }

                LogDebug("🌊 SECTION 2d: Hot tub pulse started - " + hotTubPulseDuration.ToString() + "s duration");
            }
            else
            {
                LogDebug("❌ SECTION 2d: Hot tub overlay CanvasGroup is null!");
            }
            hotTubPulseStarted = true;
        }

        private void ExecuteSection2e_TerminalControl()
        {
            if (terminalBlockerCanvasGroup != null)
            {
                GameObject terminalObject = terminalBlockerCanvasGroup.gameObject;
                if (!terminalObject.activeSelf) terminalObject.SetActive(true);

                terminalBlockerCanvasGroup.alpha = 1f;
                AddFade(terminalBlockerCanvasGroup, 1f, 0f, terminalFadeDuration, "Terminal Blocker");
                LogDebug("💻 SECTION 2e: Terminal fade started - " + terminalFadeDuration.ToString() + "s duration");
            }
            else
            {
                LogDebug("❌ SECTION 2e: Terminal blocker CanvasGroup is null!");
            }
            terminalFadeStarted = true;
        }

        private void EnableSpotLight()
        {
            if (spotLight != null)
            {
                spotLight.SetActive(true);
                LogDebug("💡 Spot light enabled");
            }
            spotLightEnabled = true;
        }

        private void ExecuteSection2f_DoorControl()
        {
            if (doorClosedState != null && doorOpenState != null)
            {
                doorClosedState.SetActive(false);
                doorOpenState.SetActive(true);
                LogDebug("🚪 SECTION 2f: Door switched from closed to open state");
            }
            doorSwitched = true;
        }

        private void ExecuteSection3_ResumeNotifications()
        {
            if (notificationHub != null)
            {
                notificationHub.SendCustomEvent("ResumeNotifications");
                LogDebug("📢 SECTION 3: Notifications resumed");
            }
            notificationsResumed = true;
        }

        // =================================================================
        // 🎨 FADE SYSTEM - SUPPORTS MULTIPLE SIMULTANEOUS FADES
        // =================================================================

        private void AddFade(CanvasGroup target, float startAlpha, float endAlpha, float duration, string name)
        {
            if (target == null) return;

            int slot = -1;
            for (int i = 0; i < 4; i++)
            {
                if (!fadeActives[i]) { slot = i; break; }
            }
            if (slot == -1) slot = 0;

            fadeTargets[slot] = target;
            fadeStartTimes[slot] = Time.time;
            fadeDurations[slot] = duration;
            fadeStartAlphas[slot] = startAlpha;
            fadeEndAlphas[slot] = endAlpha;
            fadeActives[slot] = true;
            fadeNames[slot] = name;

            target.alpha = startAlpha;

            LogDebug("🎨 Started fade: " + name + " (" + startAlpha.ToString("F1") + " → " + endAlpha.ToString("F1") + " over " + duration.ToString("F1") + "s)");
        }

        private void UpdateAllFades()
        {
            for (int i = 0; i < 4; i++)
            {
                if (!fadeActives[i]) continue;

                float elapsed = Time.time - fadeStartTimes[i];
                float progress = Mathf.Clamp01(elapsed / fadeDurations[i]);

                if (fadeTargets[i] != null)
                {
                    float currentAlpha = Mathf.Lerp(fadeStartAlphas[i], fadeEndAlphas[i], progress);
                    fadeTargets[i].alpha = currentAlpha;

                    if (Time.frameCount % 10 == 0)
                    {
                        LogDebug("🎨 Fade '" + fadeNames[i] + "' progress: " + progress.ToString("F2") + ", alpha: " + currentAlpha.ToString("F2"));
                    }
                }

                if (progress >= 1f)
                {
                    LogDebug("✅ Fade complete: " + fadeNames[i] + " - Final alpha: " + fadeTargets[i].alpha.ToString());
                    fadeActives[i] = false;
                }
            }
        }

        // =================================================================
        // 🌊 HOT TUB PULSE SYSTEM
        // =================================================================

        private void UpdateHotTubPulse()
        {
            if (!hotTubPulsing || hotTubOverlayCanvasGroup == null) return;

            float elapsed = Time.time - hotTubPulseStartTimeActual;

            if (elapsed >= hotTubPulseDuration)
            {
                hotTubPulsing = false;
                hotTubOverlayCanvasGroup.alpha = 0f;
                LogDebug("✅ Hot tub pulse complete");
                return;
            }

            float pulseSpeed = 2f;
            float pulseValue = Mathf.Sin(elapsed * pulseSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
            hotTubOverlayCanvasGroup.alpha = pulseValue;
        }

        // =================================================================
        // 🌐 NETWORK SYNCHRONIZATION
        // =================================================================

        public override void OnDeserialization()
        {
            // If a ceremony is active in the instance, align our local timer to the server timestamp
            if (syncCeremonyActive && !ceremonyActive)
            {
                int nowMs = Networking.GetServerTimeInMilliseconds();
                float elapsed = 0f;
                if (syncCeremonyStartMs > 0)
                {
                    elapsed = Mathf.Max(0f, (nowMs - syncCeremonyStartMs) * 0.001f);
                }

                ceremonyActive = true;
                ceremonyStartTime = Time.time - elapsed;
                HideButton();
                LogDebug("🌐 Network sync: Starting ceremony triggered by " + syncInstigatorName + " (+" + elapsed.ToString("F2") + "s)");
            }

            // Keep local button visibility consistent with sync flags
            CheckButtonVisibility();

            // If we joined after it finished, snap to post-ceremony state
            if (syncCeremonyHasBeenRun && !ceremonyHasBeenRun)
            {
                ceremonyHasBeenRun = true;
                SetPostCeremonyState();
                LogDebug("🌐 Late joiner detected - set to post-ceremony state");
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player != null && player.isLocal)
            {
                CheckButtonVisibility();
            }
        }

        // =================================================================
        // 🧪 DEBUG MENU COMMANDS (unchanged)
        // =================================================================

        [ContextMenu("ALL - Test Full Ceremony")]
        public void TestFullCeremony()
        {
            LogDebug("🧪 TESTING: Complete cinematic ceremony");
            StartCinematicCeremony();
        }

        [ContextMenu("Section 0 - PAUSE ON JOIN NOTIFICATIONS")]
        public void TestSection0_Notifications()
        {
            LogDebug("🧪 TESTING: Section 0 - Pause Notifications");
            ExecuteSection0_PauseNotifications();
        }

        [ContextMenu("Section 1 - Start Music")]
        public void TestSection1_Music()
        {
            LogDebug("🧪 TESTING: Section 1 - Start Music");
            ExecuteSection1_StartMusic();
        }

        [ContextMenu("Section 2a - ProTV Fade")]
        public void TestSection2a_ProTV()
        {
            LogDebug("🧪 TESTING: Section 2a - ProTV Fade");
            ExecuteSection2a_ProTVFade();
        }

        [ContextMenu("Section 2b - H3Menu Fade")]
        public void TestSection2b_H3Menu()
        {
            LogDebug("🧪 TESTING: Section 2b - H3Menu Fade");
            ExecuteSection2b_H3MenuFade();
        }

        [ContextMenu("Section 2c - Fan Control")]
        public void TestSection2c_Fan()
        {
            LogDebug("🧪 TESTING: Section 2c - Fan Control");
            ExecuteSection2c_FanControl();
        }

        [ContextMenu("Section 2d - Hot Tub Pulse")]
        public void TestSection2d_HotTub()
        {
            LogDebug("🧪 TESTING: Section 2d - Hot Tub Pulse");
            ExecuteSection2d_HotTubPulse();
        }

        [ContextMenu("Section 2e - Terminal Control")]
        public void TestSection2e_Terminal()
        {
            LogDebug("🧪 TESTING: Section 2e - Terminal Control");
            ExecuteSection2e_TerminalControl();
        }

        [ContextMenu("Section 2f - Door Control")]
        public void TestSection2f_Door()
        {
            LogDebug("🧪 TESTING: Section 2f - Door Control");
            ExecuteSection2f_DoorControl();
        }

        [ContextMenu("Section 3 - Resume Notifications")]
        public void TestSection3_Resume()
        {
            LogDebug("🧪 TESTING: Section 3 - Resume Notifications");
            ExecuteSection3_ResumeNotifications();
        }

        [ContextMenu("🔴 Test Button Visibility")]
        public void TestButtonVisibility()
        {
            LogDebug("🧪 TESTING: Button visibility check");
            CheckButtonVisibility();
        }

        [ContextMenu("🌐 Test Post-Ceremony State")]
        public void TestPostCeremonyState()
        {
            LogDebug("🧪 TESTING: Post-ceremony state setup");
            SetPostCeremonyState();
        }

        [ContextMenu("🔧 Debug All References")]
        public void DebugAllReferences()
        {
            LogDebug("🔧 === DEBUGGING ALL COMPONENT REFERENCES ===");
            LogDebug($"ProTV Playlist: {(proTVPlaylist != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Notification Hub: {(notificationHub != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"ProTV Overlay: {(proTVOverlayCanvasGroup != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"H3Menu Overlay: {(h3MenuOverlayCanvasGroup != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Fan Animator: {(fanAnimatorObject != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Hot Tub Overlay: {(hotTubOverlayCanvasGroup != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Terminal Blocker: {(terminalBlockerCanvasGroup != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Spot Light: {(spotLight != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Door Closed State: {(doorClosedState != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Door Open State: {(doorOpenState != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"Ceremony Button: {(ceremonyButton != null ? "✅ Connected" : "❌ Missing")}");
            LogDebug($"My Username: {myUsername}");
            LogDebug("🏁 === REFERENCE DEBUG COMPLETE ===");
        }

        [ContextMenu("⏱️ Reset Ceremony")]
        public void ResetCeremony()
        {
            LogDebug("⏱️ Resetting ceremony system");
            ceremonyActive = false;
            ceremonyHasBeenRun = false;

            if (Networking.IsOwner(this.gameObject))
            {
                syncCeremonyActive = false;
                syncCeremonyHasBeenRun = false;
                syncInstigatorName = "";
                syncCeremonyStartMs = 0; // clear deterministic timestamp
                RequestSerialization();
            }

            notificationsPaused = false;
            musicStarted = false;
            proTVFadeStarted = false;
            h3MenuFadeStarted = false;
            fanStarted = false;
            hotTubPulseStarted = false;
            terminalFadeStarted = false;
            spotLightEnabled = false;
            doorSwitched = false;
            notificationsResumed = false;
            hotTubPulsing = false;

            for (int i = 0; i < 4; i++) fadeActives[i] = false;

            CheckButtonVisibility();

            LogDebug("✅ Ceremony reset complete");
        }

        [ContextMenu("🔧 Debug Canvas Group States")]
        public void DebugCanvasGroupStates()
        {
            LogDebug("🔧 === CANVAS GROUP STATE DEBUG ===");

            if (proTVOverlayCanvasGroup != null)
                LogDebug("ProTV Overlay - Alpha: " + proTVOverlayCanvasGroup.alpha.ToString() + ", GameObject Active: " + proTVOverlayCanvasGroup.gameObject.activeSelf.ToString());
            else
                LogDebug("ProTV Overlay - NULL");

            if (h3MenuOverlayCanvasGroup != null)
                LogDebug("H3Menu Overlay - Alpha: " + h3MenuOverlayCanvasGroup.alpha.ToString() + ", GameObject Active: " + h3MenuOverlayCanvasGroup.gameObject.activeSelf.ToString());
            else
                LogDebug("H3Menu Overlay - NULL");

            if (hotTubOverlayCanvasGroup != null)
                LogDebug("Hot Tub Overlay - Alpha: " + hotTubOverlayCanvasGroup.alpha.ToString() + ", GameObject Active: " + hotTubOverlayCanvasGroup.gameObject.activeSelf.ToString());
            else
                LogDebug("Hot Tub Overlay - NULL");

            if (terminalBlockerCanvasGroup != null)
                LogDebug("Terminal Blocker - Alpha: " + terminalBlockerCanvasGroup.alpha.ToString() + ", GameObject Active: " + terminalBlockerCanvasGroup.gameObject.activeSelf.ToString());
            else
                LogDebug("Terminal Blocker - NULL");

            LogDebug("🏁 === CANVAS GROUP DEBUG COMPLETE ===");
        }

        [ContextMenu("🔧 Force Reset All Alphas to 1")]
        public void ForceResetAlphas()
        {
            LogDebug("🔧 Forcing all CanvasGroup alphas to 1.0");

            if (proTVOverlayCanvasGroup != null) { proTVOverlayCanvasGroup.alpha = 1f; LogDebug("ProTV alpha set to 1.0"); }
            if (h3MenuOverlayCanvasGroup != null) { h3MenuOverlayCanvasGroup.alpha = 1f; LogDebug("H3Menu alpha set to 1.0"); }
            if (hotTubOverlayCanvasGroup != null) { hotTubOverlayCanvasGroup.alpha = 1f; LogDebug("Hot Tub alpha set to 1.0"); }
            if (terminalBlockerCanvasGroup != null) { terminalBlockerCanvasGroup.alpha = 1f; LogDebug("Terminal blocker alpha set to 1.0"); }
        }

        // =================================================================
        // 🔧 DEBUG UTILITIES
        // =================================================================

        private void LogDebug(string message)
        {
            Debug.Log("<color=#FF6B35>🎬 [CinematicChair] " + message + "</color>");
        }
    }
}
