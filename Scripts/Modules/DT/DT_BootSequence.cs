using UdonSharp;
using UnityEngine;
using TMPro;
using LowerLevel.Terminal;

/// <summary>
/// DOS Terminal Boot Sequence Module
/// Manages BIOS/POST boot sequences with retro computer startup animation.
/// Extracted from DOSTerminalController.cs as part of modularization (Phase 5).
/// </summary>
public class DT_BootSequence : UdonSharpBehaviour
{
    [Header("Core Controller Reference")]
    [Tooltip("Reference to main controller for callbacks")]
    public DOSTerminalController coreController;

    [Header("UI References")]
    [Tooltip("Boot display for BIOS/POST sequences")]
    public TextMeshProUGUI bootDisplay;
    [Tooltip("Terminal display (disabled during boot)")]
    public TextMeshProUGUI terminalDisplay;

    [Header("Audio References")]
    public AudioSource audioSource;
    public AudioClip bootBeepSound;
    public AudioClip fanSound;
    public AudioClip keystrokeSound;

    [Header("Module References")]
    [Tooltip("Remote content module for splash screen")]
    public DT_RemoteContent remoteContentModule;
    [Tooltip("Weather module for status checking (DT_WeatherModule)")]
    public UdonSharpBehaviour weatherModule;
    [Tooltip("Achievement data manager for status checking")]
    public UdonSharpBehaviour achievementDataManager;
    [Tooltip("Achievement module for status checking")]
    public UdonSharpBehaviour achievementModule;

    [Header("Boot Sequence Settings")]
    [Tooltip("Enable menu system (affects boot completion)")]
    public bool enableMenuSystem = true;
    [Tooltip("Delay between BIOS lines (seconds)")]
    public float biosLineDelay = 0.15f;
    [Tooltip("Delay between BIOS modules (seconds)")]
    public float biosModuleDelay = 0.3f;
    [Tooltip("Delay between POST modules (seconds)")]
    public float postModuleDelay = 0.3f;

    [Header("Debug Settings")]
    public bool enableDebugLogging = true;

    // =================================================================
    // BOOT SEQUENCE STATE
    // =================================================================

    [HideInInspector] public bool isShowingBios = false;
    [HideInInspector] public int biosCurrentModule = 0;
    [HideInInspector] public int postCurrentModule = 0;
    [HideInInspector] public float biosProgress = 0f;

    // BIOS modules for loading sequence
    private string[] biosModules = new string[] {
        "Initializing CRT Shader",
        "Loading Xbox Notifications",
        "Downloading Achievements",
        "Loading ROM v1.02",
        "Connecting to Ko-Fi API",
        "Loading Supporter Database",
        "Syncing Weather Data",
        "Syncing Player Time"
    };

    // POST screen modules for component verification
    private string[] postModules = new string[] {
        "Loading Persistent Module",
        "Loading Achievement Module",
        "Loading High Score Module",
        "Loading User Preferences Module",
        "Loading Notification System",
        "Ko-Fi Module API",
        "Open Weather API",
        "GitHub Integration",
        "PlayerData Persistence",
        "Audio System Initialization"
    };

    // =================================================================
    // PUBLIC API - BOOT SEQUENCE CONTROL
    // =================================================================

    /// <summary>
    /// Starts the regular boot sequence for returning visitors.
    /// Plays boot sounds, shows splash screen, then proceeds to BIOS sequence.
    /// </summary>
    public void StartRegularBootSequence()
    {
        LogDebug("🔄 Starting regular boot sequence for returning visitor");

        if (audioSource && bootBeepSound)
            audioSource.PlayOneShot(bootBeepSound);

        if (audioSource && fanSound)
        {
            audioSource.clip = fanSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (bootDisplay)
        {
            bootDisplay.gameObject.SetActive(true);
            ShowRegularSplash();
        }

        if (terminalDisplay)
            terminalDisplay.gameObject.SetActive(false);

        SendCustomEventDelayedSeconds(nameof(StartBiosSequence), 2.0f);
    }

    /// <summary>
    /// Starts the BIOS loading sequence.
    /// Shows hardware initialization messages line-by-line.
    /// </summary>
    public void StartBiosSequence()
    {
        LogDebug("⚙️ Starting BIOS sequence");
        isShowingBios = true;
        biosCurrentModule = 0;
        biosProgress = 0f;
        bootDisplay.text = GetBiosHeader();
        SendCustomEventDelayedSeconds(nameof(ShowNextBiosLine), biosLineDelay);
    }

    /// <summary>
    /// Displays the next BIOS line in the boot sequence.
    /// Called recursively via SendCustomEventDelayedSeconds until all modules loaded.
    /// </summary>
    public void ShowNextBiosLine()
    {
        if (!isShowingBios) return;

        if (biosModules == null || biosCurrentModule >= biosModules.Length)
        {
            LogDebug("📋 BIOS modules completed or array is null");
            CompleteBiosSequence();
            return;
        }

        if (biosCurrentModule < biosModules.Length)
        {
            biosProgress = (float)(biosCurrentModule + 1) / biosModules.Length;
            string biosDisplay = GetBiosHeader() + "\n";

            for (int i = 0; i <= biosCurrentModule && i < biosModules.Length; i++)
            {
                biosDisplay += $"     {biosModules[i]}... OK\n";
            }

            bootDisplay.text = biosDisplay;

            if (audioSource && keystrokeSound)
                audioSource.PlayOneShot(keystrokeSound);

            biosCurrentModule++;
            SendCustomEventDelayedSeconds(nameof(ShowNextBiosLine), biosModuleDelay);
        }
        else
        {
            CompleteBiosSequence();
        }
    }

    /// <summary>
    /// Starts the POST (Power-On Self-Test) sequence.
    /// Shows system component verification messages.
    /// </summary>
    public void StartPostSequence()
    {
        LogDebug("⚙️ Starting POST sequence");
        isShowingBios = true;
        postCurrentModule = 0;
        bootDisplay.text = GetPostHeader();
        SendCustomEventDelayedSeconds(nameof(ShowNextPostLine), biosLineDelay);
    }

    /// <summary>
    /// Displays the next POST line in the boot sequence.
    /// Called recursively via SendCustomEventDelayedSeconds until all modules checked.
    /// </summary>
    public void ShowNextPostLine()
    {
        if (!isShowingBios) return;

        if (postModules == null || postCurrentModule >= postModules.Length)
        {
            LogDebug("📋 POST modules completed or array is null");
            CompleteBootSequence();
            return;
        }

        if (postCurrentModule < postModules.Length)
        {
            string postDisplay = GetPostHeader();

            for (int i = 0; i <= postCurrentModule && i < postModules.Length; i++)
            {
                string status = GetModuleStatus(i);
                postDisplay += $"{postModules[i]}... {status}\n";
            }

            bootDisplay.text = postDisplay;

            if (audioSource && keystrokeSound)
                audioSource.PlayOneShot(keystrokeSound);

            postCurrentModule++;
            SendCustomEventDelayedSeconds(nameof(ShowNextPostLine), postModuleDelay);
        }
        else
        {
            CompleteBootSequence();
        }
    }

    /// <summary>
    /// Completes the boot sequence and transitions to main terminal.
    /// Hides boot display, shows terminal display, and notifies core controller.
    /// </summary>
    public void CompleteBootSequence()
    {
        LogDebug("✅ Boot sequence complete, showing welcome screen");

        if (bootDisplay) bootDisplay.gameObject.SetActive(false);
        if (terminalDisplay) terminalDisplay.gameObject.SetActive(true);

        isShowingBios = false;

        // Notify core controller that boot is complete
        if (coreController != null)
        {
            coreController.OnBootSequenceComplete(enableMenuSystem);
        }
    }

    // =================================================================
    // PRIVATE HELPER METHODS
    // =================================================================

    /// <summary>
    /// Shows the regular splash screen (for returning visitors).
    /// Uses remote content if available, otherwise shows default splash.
    /// </summary>
    private void ShowRegularSplash()
    {
        string remoteSplash = (remoteContentModule != null) ? remoteContentModule.remoteSplashContent : "";
        string splashContent = !string.IsNullOrEmpty(remoteSplash)
            ? remoteSplash
            : GetDefaultSplash();
        bootDisplay.text = splashContent;
        LogDebug("📺 Showing regular splash screen");
    }

    /// <summary>
    /// Returns the default splash screen text.
    /// </summary>
    private string GetDefaultSplash()
    {
        return "================================================================\n" +
               "     WELCOME TO BASEMENT OS v28\n" +
               "        (C) 2025 Lower Level Devs\n" +
               "================================================================";
    }

    /// <summary>
    /// Returns the BIOS header text.
    /// </summary>
    private string GetBiosHeader()
    {
        return "================================================================" + "\n" +
               "                    WELCOME TO BASEMENT OS v28" + "\n" +
               "                    (C) 2025 Lower Level" + "\n" +
               "================================================================" + "\n" +
               "Loading System Components...";
    }

    /// <summary>
    /// Returns the POST header text.
    /// </summary>
    private string GetPostHeader()
    {
        return @"░▒▓█ LOADING BASEMENT OS v28 █▓▒░
═════════════════════════════════════════════

System Component Verification:

";
    }

    /// <summary>
    /// Completes the BIOS sequence and schedules POST sequence.
    /// </summary>
    private void CompleteBiosSequence()
    {
        bootDisplay.text += "\n═════════════════════════════════════════════\n";
        bootDisplay.text += "Loading complete. Starting Basement OS v28...\n";
        bootDisplay.text += "═════════════════════════════════════════════";
        SendCustomEventDelayedSeconds(nameof(CompleteBootSequence), 2.0f);
    }

    /// <summary>
    /// Returns the status of a POST module based on actual component connections.
    /// Checks if modules are properly wired and functional.
    /// </summary>
    /// <param name="moduleIndex">Index of the POST module to check</param>
    /// <returns>Status string (OK, NOT FOUND, NOT INSTALLED, PENDING, UNKNOWN)</returns>
    private string GetModuleStatus(int moduleIndex)
    {
        if (moduleIndex < 0 || moduleIndex >= postModules.Length)
        {
            return "UNKNOWN";
        }

        switch (moduleIndex)
        {
            case 0: // Persistent Module
                return (achievementDataManager != null) ? "OK" : "NOT FOUND";
            case 1: // Achievement Module
                return (achievementModule != null) ? "OK" : "NOT FOUND";
            case 2: // High Score Module
                return "OK"; // Always available through PlayerData
            case 3: // User Preferences
                return "OK"; // Always available through PlayerData
            case 4: // Notification System
                return (audioSource != null) ? "OK" : "NOT FOUND";
            case 5: // Ko-Fi Module API
                return "NOT INSTALLED"; // TODO: Replace when Ko-fi integrated
            case 6: // Open Weather API
                if (weatherModule != null)
                {
                    object enabled = weatherModule.GetProgramVariable("enableSimpleWeather");
                    return (enabled != null && (bool)enabled) ? "OK" : "NOT INSTALLED";
                }
                return "NOT INSTALLED";
            case 7: // GitHub Integration
                return (remoteContentModule != null && remoteContentModule.hasRemoteContent) ? "OK" : "PENDING";
            case 8: // PlayerData Persistence
                return "OK"; // Always available in VRChat
            case 9: // Audio System
                return (audioSource != null) ? "OK" : "NOT FOUND";
            default:
                return "UNKNOWN";
        }
    }

    /// <summary>
    /// Logs debug message if debug logging is enabled.
    /// </summary>
    /// <param name="message">Message to log</param>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[DT_BootSequence] {message}");
        }
    }
}
