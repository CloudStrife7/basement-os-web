using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using TMPro;
using UdonSharp;

/// <summary>
/// BASEMENT OS CORE (v2.0)
/// 
/// ROLE: SYSTEM CORE / HARDWARE ABSTRACTION LAYER
/// The Core is the only script allowed to talk directly to the hardware 
/// (Screen, Audio, Station). It routes "Input Interrupts" to the Active Process (App).
/// 
/// LOCATION: Assets/Scripts/BasementOS/CORE/DT_Core.cs
/// 
/// INTEGRATION:
/// - Hub: Receives notifications from NotificationEventHub
/// - Spoke: Routes input to apps in /BIN/ (e.g., DT_Shell)
/// 
/// LIMITATIONS:
/// - Max 600 Lines
/// - No UdonSynced variables (Local Only)
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Core : UdonSharpBehaviour 
{
    // =================================================================
    // HARDWARE INTERFACES (The "Bios")
    // =================================================================
    [Header("--- Hardware References ---")]
    [Tooltip("The 80x24 Character Display")]
    [SerializeField] private TextMeshProUGUI terminalScreen;
    
    [Tooltip("Player Lock / Input Capture")]
    // FIX: Explicitly use VRC.SDK3.Components.VRCStation to resolve ambiguity
    [SerializeField] private VRC.SDK3.Components.VRCStation playerStation;
    
    [Tooltip("System Beeps and Drive Noises")]
    [SerializeField] private AudioSource audioSource;

    [Header("--- Terminal Station ---")]
    [Tooltip("The VRCStation used for the terminal (for spacebar exit)")]
    [SerializeField] private VRC.SDK3.Components.VRCStation terminalStation;

    [Header("--- Boot Configuration ---")]
    [Tooltip("The first program to load (e.g. DT_Shell in /BIN/)")]
    [SerializeField] private UdonSharpBehaviour bootProcess;

    [Header("--- Ticker ---")]
    [Tooltip("RSS Ticker component for scrolling messages")]
    [SerializeField] private DT_Ticker ticker;

    [Header("--- Cache Manager ---")]
    [Tooltip("Cache manager for DateTime operations")]
    [SerializeField] private DT_CacheManager cacheManager;

    // =================================================================
    // NOTIFICATION INTEGRATION (for NotificationEventHub)
    // =================================================================
    [HideInInspector] public string queuePlayerName = "";
    [HideInInspector] public string queueAchievementTitle = "";
    [HideInInspector] public int queuePoints = 0;
    [HideInInspector] public bool queueIsFirstTime = false;

    // =================================================================
    // CORE STATE
    // =================================================================
    private UdonSharpBehaviour activeProcess;
    private bool isPlayerLocked = false;
    private string currentAppName = "MENU"; // Current app name for path display
    
    // Screen Buffer (80x24 Grid)
    private string headerLine = ""; // Line 0
    private string rssLine = "";    // Line 2 (Ticker)
    [HideInInspector] public string contentBuffer = ""; // Lines 4-20 (App Layer) - Public for app access
    private string footerLine = ""; // Line 22 (Status)
    private string promptLine = ""; // Line 23 (Input)

    // System Timers (Quest Optimization)
    private float lastBlinkTime = 0f;
    private bool isCursorVisible = true;
    private const float CURSOR_BLINK_RATE = 0.5f;
    
    // Constants
    private const int SCREEN_WIDTH = 80;

    // Flavor Text Pool (random selection on app transitions)
    private string[] flavorTexts = new string[]
    {
        "Mounting virtual volume... OK.",
        "Allocating memory banks... OK.",
        "Loading process into RAM... OK.",
        "Initializing display driver... OK.",
        "Handshake complete.",
        "Process spawned successfully.",
        "System resources allocated.",
        "Compiling bytecode... Done.",
        "Reading sector 0x7C00... OK."
    };

    // =================================================================
    // CORE INITIALIZATION
    // =================================================================
    void Start()
    {
        ValidateHardware();
        BootSequence();
    }

    private void ValidateHardware()
    {
        if (!Utilities.IsValid(terminalScreen)) Debug.LogError("[CORE] FATAL: Screen hardware missing!");
        if (!Utilities.IsValid(playerStation)) Debug.LogError("[CORE] FATAL: Input station missing!");
    }

    private void BootSequence()
    {
        Debug.Log("[DT_Core] ===== BOOT SEQUENCE START =====");

        // Initialize cache manager
        if (Utilities.IsValid(cacheManager))
        {
            cacheManager.InitializeCaches();
        }

        // Set initial flavor text
        SetFooterStatus("BIOS Check... OK. Initializing Core...");

        // Initialize UI Buffers with dynamic header
        UpdateHeaderTime(); // Set initial time/date
        rssLine = ""; // Reserved for RSS ticker with [PlayerName]
        promptLine = @"C:\BASEMENT> _";

        // Load Boot Process
        if (Utilities.IsValid(bootProcess))
        {
            Debug.Log("[DT_Core] bootProcess is Valid. Type: " + bootProcess.GetType().Name);
            Debug.Log("[DT_Core] bootProcess GameObject: " + bootProcess.gameObject.name);
            LoadProcess(bootProcess);
        }
        else
        {
            contentBuffer = "\n\n   FATAL ERROR: NO BOOTABLE MEDIUM FOUND.\n   PLEASE INSERT DISK (Assign bootProcess in Inspector).";
            RefreshDisplay();
        }
    }

    // =================================================================
    // SYSTEM LOOP (Quest Optimized)
    // =================================================================
    void Update()
    {
        // Update cache manager time (once per second)
        if (Utilities.IsValid(cacheManager))
        {
            cacheManager.UpdateTimeCache();
        }

        // Update ticker scroll
        if (Utilities.IsValid(ticker))
        {
            ticker.UpdateTick(Time.deltaTime);
            rssLine = ticker.GetVisibleText();
        }

        // Update header time display (once per second)
        UpdateHeaderTime();

        // Cursor Blink (Visual Only)
        if (Time.time - lastBlinkTime > CURSOR_BLINK_RATE)
        {
            lastBlinkTime = Time.time;
            isCursorVisible = !isCursorVisible;
            UpdatePromptDisplay(); // Only redraw the prompt line
        }
    }

    // =================================================================
    // PROCESS MANAGEMENT (Hub -> Spoke)
    // =================================================================
    
    /// <summary>
    /// Variable for apps to set the next process before calling LoadNextProcess
    /// Used because SendCustomEvent cannot pass parameters
    /// </summary>
    [HideInInspector] public UdonSharpBehaviour nextProcess;

    /// <summary>
    /// Called by apps via SendCustomEvent to load the nextProcess
    /// Apps should: coreReference.SetProgramVariable("nextProcess", targetApp);
    ///              coreReference.SendCustomEvent("LoadNextProcess");
    /// </summary>
    public void LoadNextProcess()
    {
        if (Utilities.IsValid(nextProcess))
        {
            LoadProcess(nextProcess);
            nextProcess = null; // Clear for next use
        }
        else
        {
            Debug.LogWarning("[DT_Core] LoadNextProcess called but nextProcess is null");
        }
    }

    /// <summary>
    /// Context switches to a new App (Spoke).
    /// </summary>
    public void LoadProcess(UdonSharpBehaviour newProcess)
    {
        Debug.Log("[DT_Core] ===== LoadProcess START =====");
        Debug.Log("[DT_Core] newProcess: " + (newProcess != null ? newProcess.GetType().Name + " on " + newProcess.gameObject.name : "NULL"));
        
        if (!Utilities.IsValid(newProcess)) 
        {
            Debug.LogError("[DT_Core] newProcess is INVALID - aborting LoadProcess");
            return;
        }

        // 1. Terminate current process
        if (Utilities.IsValid(activeProcess))
        {
            Debug.Log("[DT_Core] Calling OnAppClose on previous activeProcess");
            activeProcess.SendCustomEvent("OnAppClose");
        }

        // 2. Context Switch
        activeProcess = newProcess;
        Debug.Log("[DT_Core] activeProcess now set to: " + activeProcess.GetType().Name);

        // 3. Determine app name from GameObject name
        string goName = newProcess.gameObject.name.ToUpper();
        if (goName.Contains("DASHBOARD")) currentAppName = "DASHBOARD";
        else if (goName.Contains("SHELL") || goName.Contains("MENU")) currentAppName = "MENU";
        else if (goName.Contains("STATS")) currentAppName = "STATS";
        else if (goName.Contains("WEATHER")) currentAppName = "WEATHER";
        else if (goName.Contains("GITHUB")) currentAppName = "GITHUB";
        else if (goName.Contains("GAMES")) currentAppName = "GAMES";
        else if (goName.Contains("TALES")) currentAppName = "TALES";
        else currentAppName = "APP";

        // 4. Initialize new process
        Debug.Log("[DT_Core] Calling SendCustomEvent('OnAppOpen')...");
        activeProcess.SendCustomEvent("OnAppOpen");
        Debug.Log("[DT_Core] SendCustomEvent('OnAppOpen') returned successfully");

        // 5. Update Status (Random Flavor Text)
        int flavorIndex = Random.Range(0, flavorTexts.Length);
        SetFooterStatus(flavorTexts[flavorIndex]);

        // 6. Update Path with current app name (enforce 80-char width)
        promptLine = "C:\\BASEMENT\\" + currentAppName + "> ";
        RefreshDisplay();
    }

    // =================================================================
    // INPUT INTERRUPT HANDLERS (VRChat Events)
    // =================================================================
    
    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isPlayerLocked = true;
            SetFooterStatus("USER LOGIN: " + player.displayName);
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isPlayerLocked = false;
            SetFooterStatus("USER LOGOUT.");
        }
    }

    // =================================================================
    // EXTERNAL STATION RELAY HANDLERS (Called by DT_StationRelay)
    // =================================================================

    /// <summary>Player reference set by DT_StationRelay before event call</summary>
    [HideInInspector] public VRCPlayerApi relayedPlayer;

    /// <summary>Input key set by DT_StationRelay before input event call</summary>
    [HideInInspector] public string relayedInputKey;

    /// <summary>
    /// Called by DT_StationRelay when player enters the terminal chair
    /// </summary>
    public void OnTerminalStationEntered()
    {
        Debug.Log("[DT_Core] OnTerminalStationEntered called. relayedPlayer: " + (relayedPlayer != null ? relayedPlayer.displayName : "NULL"));

        if (relayedPlayer != null && relayedPlayer.isLocal)
        {
            isPlayerLocked = true;
            Debug.Log("[DT_Core] Player locked = TRUE. activeProcess = " + (activeProcess != null ? "Valid" : "NULL"));
            SetFooterStatus("USER LOGIN: " + relayedPlayer.displayName);
        }
    }

    /// <summary>
    /// Called by DT_StationRelay when player exits the terminal chair
    /// </summary>
    public void OnTerminalStationExited()
    {
        if (relayedPlayer != null && relayedPlayer.isLocal)
        {
            isPlayerLocked = false;
            SetFooterStatus("USER LOGOUT.");
        }
    }

    /// <summary>
    /// Called by DT_StationRelay when player presses WASD/E
    /// </summary>
    public void OnRelayedInput()
    {
        Debug.Log("[DT_Core] OnRelayedInput: key=" + relayedInputKey + " locked=" + isPlayerLocked + " activeProcess=" + (activeProcess != null ? "Valid" : "NULL"));

        if (!isPlayerLocked || !Utilities.IsValid(activeProcess)) return;

        RouteInput(relayedInputKey);
    }

    /// <summary>
    /// Called by DT_StationRelay when player presses Spacebar
    /// </summary>
    public void OnRelayedInputJump()
    {
        Debug.Log("[DT_Core] OnRelayedInputJump - Exiting station");

        if (isPlayerLocked && Utilities.IsValid(terminalStation))
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (player != null)
            {
                terminalStation.ExitStation(player);
            }
        }
    }

    // NOTE: Input override methods removed - all input now relayed from DT_StationRelay
    // See OnRelayedInput() and OnRelayedInputJump() below for relay handlers

    private void RouteInput(string key)
    {
        Debug.Log("[DT_Core] ===== RouteInput START =====");
        Debug.Log("[DT_Core] key=" + key);
        Debug.Log("[DT_Core] activeProcess valid=" + Utilities.IsValid(activeProcess));
        
        if (Utilities.IsValid(activeProcess))
        {
            Debug.Log("[DT_Core] activeProcess Type: " + activeProcess.GetType().Name);
            Debug.Log("[DT_Core] activeProcess GameObject: " + activeProcess.gameObject.name);
        }

        if (!Utilities.IsValid(activeProcess)) 
        {
            Debug.LogError("[DT_Core] activeProcess is INVALID - cannot route input!");
            return;
        }

        Debug.Log("[DT_Core] Calling SetProgramVariable('inputKey', '" + key + "')...");
        activeProcess.SetProgramVariable("inputKey", key);
        Debug.Log("[DT_Core] Calling SendCustomEvent('OnInput')...");
        activeProcess.SendCustomEvent("OnInput");
        Debug.Log("[DT_Core] SendCustomEvent('OnInput') returned successfully");

        if (Utilities.IsValid(audioSource)) audioSource.Play();
    }

    // =================================================================
    // NOTIFICATION BUS (Event Hub Integration)
    // =================================================================

    public void ReceiveNotification(string msg)
    {
        // Flash message in footer
        SetFooterStatus(">> NET MESSAGE: " + msg);
    }

    // =================================================================
    // VIDEO MEMORY (Display Rendering)
    // =================================================================

    public void SetFooterStatus(string status)
    {
        // DT_Format.EnforceLine80 will handle truncation and padding
        footerLine = DT_Format.EnforceLine80(status);
        RefreshDisplay();
    }

    /// <summary>
    /// Called by apps to update the content buffer
    /// </summary>
    public void SetContentBuffer(string content)
    {
        contentBuffer = content;
        RefreshDisplay();
    }

    /// <summary>
    /// Normalizes multi-line content to 80 chars per line
    /// </summary>
    private string NormalizeMultiLineContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return "";

        string[] lines = content.Split('\n');
        string result = "";

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) result = result + "\n";
            result = result + DT_Format.EnforceLine80(lines[i]);
        }

        return result;
    }

    /// <summary>
    /// Updates the header line with current time and date
    /// </summary>
    private void UpdateHeaderTime()
    {
        string timeStr = "00:00";
        string dateStr = "00.00.0000";

        if (Utilities.IsValid(cacheManager))
        {
            string fullTime = cacheManager.GetCachedTime();
            // Extract HH:mm from "h:mm:ss tt" format
            if (!string.IsNullOrEmpty(fullTime) && fullTime.Length >= 5)
            {
                // Parse time to get hours and minutes
                int spaceIdx = fullTime.IndexOf(" ");
                if (spaceIdx > 0)
                {
                    string timePart = fullTime.Substring(0, spaceIdx);
                    int colonIdx = timePart.IndexOf(":");
                    if (colonIdx > 0 && timePart.Length > colonIdx + 2)
                    {
                        timeStr = timePart.Substring(0, colonIdx + 3); // HH:mm
                    }
                }
            }
            string rawDate = cacheManager.GetCachedDate();
            // Convert MM.dd.yyyy to MM/dd/yyyy
            if (!string.IsNullOrEmpty(rawDate))
            {
                dateStr = rawDate.Replace(".", "/");
            }
        }

        // Format: "BASEMENT OS // VERSION 2                         [MM/dd/yyyy] [HH:mm] [ONLINE]"
        // Total length must be 80 chars
        string baseText = "BASEMENT OS // VERSION 2";
        string status = "[ONLINE]";
        string date = "[" + dateStr + "]";
        string time = "[" + timeStr + "]";

        // Calculate spacing: 80 - baseText - date - time - status - spaces between
        int usedSpace = baseText.Length + date.Length + time.Length + status.Length + 3; // 3 spaces between elements
        int paddingNeeded = SCREEN_WIDTH - usedSpace;

        string padding = "";
        for (int i = 0; i < paddingNeeded; i++)
        {
            padding = padding + " ";
        }

        headerLine = baseText + padding + date + " " + time + " " + status;
    }

    /// <summary>
    /// Called by NotificationEventHub when an achievement is earned
    /// Adds the achievement message to the scrolling ticker
    /// </summary>
    public void QueueAchievementNotificationEvent()
    {
        if (!Utilities.IsValid(ticker)) return;

        string message = queuePlayerName + " earned " + queueAchievementTitle + " (" + queuePoints.ToString() + "G)";
        ticker.AddMessage(message);

        Debug.Log("[DT_Core] Achievement ticker: " + message);
    }

    /// <summary>
    /// Called by NotificationEventHub when a player joins
    /// Adds the login message to the scrolling ticker
    /// </summary>
    public void QueueOnlineNotificationEvent()
    {
        if (!Utilities.IsValid(ticker)) return;

        string message = "";
        if (queueIsFirstTime)
        {
            message = queuePlayerName + " joined for the FIRST TIME!";
        }
        else
        {
            message = queuePlayerName + " has entered the basement";
        }

        ticker.AddMessage(message);

        Debug.Log("[DT_Core] Login ticker: " + message);
    }

    private void UpdatePromptDisplay()
    {
        if (!Utilities.IsValid(terminalScreen)) return;
        RefreshDisplay();
    }

    /// <summary>
    /// Refreshes the terminal screen display. Can be called by apps via SendCustomEvent.
    /// </summary>
    public void RefreshDisplay()
    {
        if (!Utilities.IsValid(terminalScreen)) return;

        // Generate 80-char separator
        string separator = DT_Format.GenerateSeparator80();

        // Enforce 80-char width on all dynamic content
        string normalizedHeader = NormalizeMultiLineContent(headerLine);
        string normalizedRSS = DT_Format.EnforceLine80(rssLine);
        string normalizedContent = NormalizeMultiLineContent(contentBuffer);
        string normalizedFooter = DT_Format.EnforceLine80(footerLine);
        string normalizedPrompt = DT_Format.EnforceLine80(promptLine + (isCursorVisible ? "_" : " "));

        // Assemble screen buffer with guaranteed 80-char lines
        terminalScreen.text = normalizedHeader + "\n" +
                              separator + "\n" +
                              normalizedRSS + "\n" +
                              separator + "\n" +
                              normalizedContent + "\n" +
                              separator + "\n" +
                              normalizedFooter + "\n" +
                              normalizedPrompt;
    }

    // =================================================================
    // TDD / DEBUGGING
    // =================================================================
    #if UNITY_EDITOR
    
    [Header("--- Debug Tools ---")]
    [SerializeField] private UdonSharpBehaviour debugTargetApp;

    [ContextMenu("TEST: Route Input 'UP'")]
    public void Test_RouteInput_Up()
    {
        Debug.Log("[CORE-TEST] Simulating UP Input...");
        
        if (Utilities.IsValid(debugTargetApp))
        {
            activeProcess = debugTargetApp;
            isPlayerLocked = true;
            RouteInput("UP");
            Debug.Log("[CORE-TEST] Signal sent. Check App Console.");
        }
        else
        {
            Debug.LogError("[CORE-TEST] No Debug App assigned!");
        }
    }

    [ContextMenu("TEST: Receive Notification")]
    public void Test_Notification_Wiring()
    {
        string testMsg = "ACHIEVEMENT UNLOCKED: HACKERMAN";
        ReceiveNotification(testMsg);
        
        if (footerLine.Contains("HACKERMAN")) Debug.Log("[CORE-TEST] Notification Wiring: PASS ✅");
        else Debug.LogError("[CORE-TEST] Notification Wiring: FAIL ❌");
    }
    
    #endif
}