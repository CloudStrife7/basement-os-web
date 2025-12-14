using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using TMPro;
using UdonSharp;

/// <summary>
/// BASEMENT OS CORE (v2.1)
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

    [Header("--- Boot Configuration ---")]
    [Tooltip("The first program to load (e.g. DT_Shell in /BIN/)")]
    [SerializeField] private UdonSharpBehaviour bootProcess;

    // =================================================================
    // CORE STATE
    // =================================================================
    private UdonSharpBehaviour activeProcess;
    private bool isPlayerLocked = false;
    
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
        // Set initial flavor text
        SetFooterStatus("BIOS Check... OK. Initializing Core...");
        
        // Initialize UI Buffers
        headerLine = "BASEMENT OS v2.1 | MEM: 640K OK";
        rssLine = " *** WELCOME TO LOWER LEVEL 2.0 *** ";
        promptLine = @"C:\BASEMENT> _";

        // Load Boot Process
        if (Utilities.IsValid(bootProcess))
        {
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
    /// Context switches to a new App (Spoke).
    /// </summary>
    public void LoadProcess(UdonSharpBehaviour newProcess)
    {
        if (!Utilities.IsValid(newProcess)) return;

        // 1. Terminate current process
        if (Utilities.IsValid(activeProcess))
        {
            activeProcess.SendCustomEvent("OnAppClose");
        }

        // 2. Context Switch
        activeProcess = newProcess;

        // 3. Initialize new process
        activeProcess.SendCustomEvent("OnAppOpen");

        // 4. Update Status (Flavor Text)
        SetFooterStatus("Mounting virtual volume... OK.");
        
        // 5. Update Path
        promptLine = @"C:\BASEMENT\APP> " + (isCursorVisible ? "_" : " ");
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

    /// <summary>
    /// Called by DT_StationRelay when player enters the terminal chair
    /// </summary>
    public void OnTerminalStationEntered()
    {
        if (relayedPlayer != null && relayedPlayer.isLocal)
        {
            isPlayerLocked = true;
            SetFooterStatus("USER LOGIN: " + relayedPlayer.displayName);
            Debug.Log("[DT_Core] Terminal station entered via relay: " + relayedPlayer.displayName);
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
            Debug.Log("[DT_Core] Terminal station exited via relay");
        }
    }

    public override void InputMoveVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (!isPlayerLocked || !Utilities.IsValid(activeProcess)) return;

        if (value > 0.5f) RouteInput("UP");
        else if (value < -0.5f) RouteInput("DOWN");
    }

    public override void InputMoveHorizontal(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (!isPlayerLocked || !Utilities.IsValid(activeProcess)) return;

        if (value > 0.5f) RouteInput("RIGHT");
        else if (value < -0.5f) RouteInput("LEFT");
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (!value) return; // Press only
        if (isPlayerLocked && Utilities.IsValid(activeProcess)) RouteInput("ACCEPT");
    }

    private void RouteInput(string key)
    {
        activeProcess.SetProgramVariable("inputKey", key);
        activeProcess.SendCustomEvent("OnInput");
        
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
        if (status.Length > SCREEN_WIDTH) status = status.Substring(0, SCREEN_WIDTH - 3) + "...";
        footerLine = status;
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

        // Assemble Screen Buffer
        string separator = new string('═', SCREEN_WIDTH);
        
        terminalScreen.text = headerLine + "\n" +
                              separator + "\n" +
                              rssLine + "\n" +
                              separator + "\n" +
                              contentBuffer + "\n" +
                              separator + "\n" +
                              footerLine + "\n" +
                              promptLine + (isCursorVisible ? "_" : " ");
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