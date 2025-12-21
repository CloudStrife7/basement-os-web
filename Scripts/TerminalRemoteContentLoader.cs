/// <summary>
/// COMPONENT PURPOSE:
/// Manages loading of remote content for DOS Terminal from Dropbox
/// Handles all remote file fetching and updates the terminal controller
/// Enhanced with comprehensive debug logging to diagnose connection issues
///
/// LOWER LEVEL 2.0 INTEGRATION:
/// Enables live updates to terminal content without world rebuilds
/// Maintains separation between content loading and display logic
///
/// DEPENDENCIES & REQUIREMENTS:
/// - DOSTerminalController component on same GameObject
/// - VRC String Downloader component for fetching remote files
/// - Dropbox public links for text files (must be direct download links)
///
/// MODIFICATION HISTORY:
/// 2025-07-07 - Initial remote content manager
/// 2025-07-07 - Fixed VRCStringDownloader API usage for SDK 3.8.2+
/// 2025-07-07 - Added comprehensive debug logging for connection diagnosis
/// </summary>

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using Varneon.VUdon.Logger.Abstract;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TerminalRemoteContentLoader : UdonSharpBehaviour
{
    [Header("Component References")]
    [Tooltip("Reference to the DOS Terminal Controller that will display the content")]
    public DOSTerminalController terminalController;

    [Header("Remote File URLs")]
    [Tooltip("Dropbox direct download link for splash.txt")]
    public VRCUrl splashUrl;

    [Tooltip("Dropbox direct download link for terminal_boot.txt")]
    public VRCUrl biosUrl;

    [Tooltip("Dropbox direct download link for dashboard.txt")]
    public VRCUrl dashboardUrl;

    [Tooltip("GitHub Pages URL for changelog.txt (auto-generated from commits)")]
    public VRCUrl changelogUrl = new VRCUrl("https://cloudstrife7.github.io/DOS-Terminal/changelog/changelog.txt");

    [Tooltip("Dropbox direct download link for halloffame.txt")]
    public VRCUrl hallOfFameUrl;

    [Header("Settings")]
    [Tooltip("Automatically load all remote content when the component starts")]
    public bool autoLoadOnStart = true;

    [Tooltip("How long to wait before retrying failed downloads (seconds)")]
    public float retryDelay = 30f;

    [Tooltip("Enable detailed console logging for debugging")]
    public bool enableDebugLogging = true; // Default to true for debugging

    [Header("Production Debugging")]
    [Tooltip("Reference to UdonLogger component for in-world console (optional)")]
    [SerializeField] private UdonLogger productionLogger;

    [Header("Advanced Debug Settings")]
    [Tooltip("Maximum time to wait for any single download before considering it failed")]
    public float downloadTimeout = 15f;

    [Tooltip("Show first N characters of downloaded content in debug log")]
    public int debugContentPreviewLength = 200;

    // Internal state tracking
    private bool isLoading = false;
    private bool sawFailure = false;
    private int pendingLoads = 0;
    private int totalLoadsAttempted = 0;
    private int successfulLoads = 0;
    private int failedLoads = 0;
    private float loadingStartTime = 0f;

    [Header("Debug Testing")]
    [Tooltip("Use local test content instead of remote URLs for testing")]
    public bool useLocalTestContent = false;

    [Tooltip("Test content for changelog")]
    [TextArea(5, 10)]
    public string testChangelogContent = "=== TEST CHANGELOG ===\n[2025-07-07] Remote content system added\n[2025-07-06] Terminal improvements\n[2025-07-05] Bug fixes";

    /// <summary>
    /// Initialize component and start auto-loading if enabled
    /// </summary>
    void Start()
    {
        LogDebug("=== TerminalRemoteContentLoader Starting ===");

        // Auto-assign terminal controller if not set
        if (terminalController == null)
        {
            terminalController = GetComponent<DOSTerminalController>();
            LogDebug($"Auto-assigned terminal controller: {(terminalController != null ? "SUCCESS" : "FAILED")}");
        }
        else
        {
            LogDebug("Terminal controller already assigned");
        }

        // Validate all URLs and log them
        ValidateAndLogUrls();

        // Check for test mode
        if (useLocalTestContent)
        {
            LogDebug("🧪 Using local test content mode");
            LoadTestContent();
            return;
        }

        // Check VRCStringDownloader availability
        LogDebug("Checking VRCStringDownloader availability...");

        // Start loading after a brief delay to ensure everything is initialized
        if (autoLoadOnStart)
        {
            LogDebug("Auto-load enabled, starting in 2 seconds...");
            SendCustomEventDelayedSeconds(nameof(LoadAllRemoteContent), 2f);
        }
        else
        {
            LogDebug("Auto-load disabled, waiting for manual trigger");
        }
    }

    /// <summary>
    /// Load test content for debugging when remote URLs don't work
    /// </summary>
    private void LoadTestContent()
    {
        LogDebug("📝 Loading local test content");

        if (terminalController != null)
        {
            terminalController.UpdateRemoteContent("changelog", testChangelogContent);
            terminalController.UpdateRemoteContent("splash", "=== LOWER LEVEL 2.0 ===\nWelcome to the basement!\nTest mode active.");
            terminalController.UpdateRemoteContent("dashboard", "SYSTEM STATUS: OK\nTEST MODE: ACTIVE\nREMOTE LOADING: DISABLED");
            LogDebug("✅ Test content loaded into terminal");
        }
        else
        {
            LogDebug("❌ Cannot load test content - terminal controller missing");
        }
    }

    /// <summary>
    /// Validate and log all configured URLs for debugging
    /// </summary>
    private void ValidateAndLogUrls()
    {
        LogDebug("=== URL VALIDATION ===");

        ValidateUrl("Splash", splashUrl);
        ValidateUrl("BIOS/Terminal Boot", biosUrl);
        ValidateUrl("Dashboard", dashboardUrl);
        ValidateUrl("Changelog", changelogUrl);
        ValidateUrl("Hall of Fame", hallOfFameUrl);

        LogDebug("=== END URL VALIDATION ===");
    }

    /// <summary>
    /// Validate a single URL and log detailed information
    /// </summary>
    private void ValidateUrl(string name, VRCUrl url)
    {
        if (url == null)
        {
            LogDebug($"{name}: NULL URL");
            return;
        }

        string urlString = url.Get();
        if (string.IsNullOrEmpty(urlString))
        {
            LogDebug($"{name}: EMPTY URL");
            return;
        }

        LogDebug($"{name}: {urlString}");

        // Check URL format
        if (urlString.StartsWith("https://dl.dropboxusercontent.com/"))
        {
            LogDebug($"  ✓ Correct Dropbox format");
        }
        else if (urlString.StartsWith("https://www.dropbox.com/"))
        {
            LogDebug($"  ✗ WRONG FORMAT - needs dl.dropboxusercontent.com");
        }
        else
        {
            LogDebug($"  ? Unknown URL format - may or may not work");
        }

        // Check for common issues
        if (urlString.Contains("&dl=0"))
        {
            LogDebug($"  ✗ Contains &dl=0 - should be removed for direct download");
        }
        if (urlString.Contains("&st="))
        {
            LogDebug($"  ✗ Contains &st= parameter - should be removed");
        }
    }

    /// <summary>
    /// Load all configured remote content files
    /// Uses VRCStringDownloader to fetch text files from remote URLs
    /// </summary>
    public void LoadAllRemoteContent()
    {
        if (isLoading)
        {
            LogDebug("Already loading content, skipping request");
            return;
        }

        LogDebug("=== STARTING REMOTE CONTENT LOADING ===");
        loadingStartTime = Time.time;
        isLoading = true;
        sawFailure = false;
        totalLoadsAttempted = 0;
        successfulLoads = 0;
        failedLoads = 0;

        // Count only the URLs that are actually configured
        pendingLoads = 0;
        if (IsValidUrl(splashUrl)) { pendingLoads++; totalLoadsAttempted++; }
        if (IsValidUrl(biosUrl)) { pendingLoads++; totalLoadsAttempted++; }
        if (IsValidUrl(dashboardUrl)) { pendingLoads++; totalLoadsAttempted++; }
        if (IsValidUrl(changelogUrl)) { pendingLoads++; totalLoadsAttempted++; }
        if (IsValidUrl(hallOfFameUrl)) { pendingLoads++; totalLoadsAttempted++; }

        if (pendingLoads == 0)
        {
            isLoading = false;
            LogDebug("❌ No valid URLs configured to load");
            return;
        }

        LogDebug($"📥 Loading {pendingLoads} remote files");
        LogDebug($"⏱️ Timeout set to {downloadTimeout} seconds per file");

        // Start all downloads - VRCStringDownloader will call our callback methods
        // NOTE: Using IUdonEventReceiver cast like the working TerminalRemoteChangelogAddon
        IUdonEventReceiver callbackTarget = (IUdonEventReceiver)this;

        LogDebug($"🎯 Callback target found: {gameObject.name}");

        if (IsValidUrl(splashUrl))
        {
            LogDebug($"🔄 Starting download: Splash from {splashUrl.Get()}");
            VRCStringDownloader.LoadUrl(splashUrl, callbackTarget);
        }

        if (IsValidUrl(biosUrl))
        {
            LogDebug($"🔄 Starting download: BIOS from {biosUrl.Get()}");
            VRCStringDownloader.LoadUrl(biosUrl, callbackTarget);
        }

        if (IsValidUrl(dashboardUrl))
        {
            LogDebug($"🔄 Starting download: Dashboard from {dashboardUrl.Get()}");
            VRCStringDownloader.LoadUrl(dashboardUrl, callbackTarget);
        }

        if (IsValidUrl(changelogUrl))
        {
            LogDebug($"🔄 Starting download: Changelog from {changelogUrl.Get()}");
            VRCStringDownloader.LoadUrl(changelogUrl, callbackTarget);
        }

        if (IsValidUrl(hallOfFameUrl))
        {
            LogDebug($"🔄 Starting download: Hall of Fame from {hallOfFameUrl.Get()}");
            VRCStringDownloader.LoadUrl(hallOfFameUrl, callbackTarget);
        }

        // Set up timeout monitoring
        SendCustomEventDelayedSeconds(nameof(CheckForTimeouts), downloadTimeout);
    }

    /// <summary>
    /// Check if a VRCUrl is valid and not empty
    /// </summary>
    private bool IsValidUrl(VRCUrl url)
    {
        return url != null && !string.IsNullOrEmpty(url.Get());
    }

    /// <summary>
    /// Monitor for downloads that take too long
    /// </summary>
    public void CheckForTimeouts()
    {
        if (!isLoading) return;

        float elapsed = Time.time - loadingStartTime;
        LogDebug($"⚠️ Download timeout check: {elapsed:F1}s elapsed, {pendingLoads} still pending");

        if (elapsed > downloadTimeout)
        {
            LogDebug($"❌ Downloads taking too long ({elapsed:F1}s > {downloadTimeout}s), considering failed");
            sawFailure = true;

            // Force completion
            pendingLoads = 0;
            CheckLoadingComplete();
        }
    }

    /// <summary>
    /// VRCStringDownloader callback - called when any string download completes
    /// We determine which file was loaded by checking the URL
    /// </summary>
    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        LogDebug("🎉 OnStringLoadSuccess callback triggered!");

        float elapsed = Time.time - loadingStartTime;
        string url = result.Url.Get();
        string content = result.Result;

        successfulLoads++;
        LogDebug($"✅ SUCCESS after {elapsed:F1}s from: {url}");
        LogDebug($"📄 Content length: {content.Length} characters");

        if (content.Length > 0)
        {
            int previewLength = Mathf.Min(debugContentPreviewLength, content.Length);
            string preview = content.Substring(0, previewLength);
            LogDebug($"📝 Content preview: '{preview}'{(content.Length > previewLength ? "..." : "")}");
        }
        else
        {
            LogDebug("⚠️ Warning: Downloaded content is empty");
        }

        if (terminalController == null)
        {
            LogDebug("❌ Warning: Terminal controller is null, cannot update content");
            CheckLoadingComplete();
            return;
        }

        // Determine which content was loaded based on URL
        bool contentUpdated = false;

        if (splashUrl != null && url == splashUrl.Get())
        {
            terminalController.UpdateRemoteContent("splash", content);
            LogDebug("✅ Splash screen content updated in terminal");
            contentUpdated = true;
        }
        else if (biosUrl != null && url == biosUrl.Get())
        {
            terminalController.UpdateRemoteContent("bios", content);
            LogDebug("✅ BIOS content updated in terminal");
            contentUpdated = true;
        }
        else if (dashboardUrl != null && url == dashboardUrl.Get())
        {
            terminalController.UpdateRemoteContent("dashboard", content);
            LogDebug("✅ Dashboard content updated in terminal");
            contentUpdated = true;
        }
        else if (changelogUrl != null && url == changelogUrl.Get())
        {
            terminalController.UpdateRemoteContent("changelog", content);
            LogDebug("✅ Changelog content updated in terminal");
            contentUpdated = true;
        }
        else if (hallOfFameUrl != null && url == hallOfFameUrl.Get())
        {
            terminalController.UpdateRemoteContent("halloffame", content);
            LogDebug("✅ Hall of Fame content updated in terminal");
            contentUpdated = true;
        }

        if (!contentUpdated)
        {
            LogDebug($"⚠️ Warning: Unknown URL loaded, content not updated: {url}");
        }

        CheckLoadingComplete();
    }

    /// <summary>
    /// VRCStringDownloader callback - called when a string download fails
    /// </summary>
    public override void OnStringLoadError(IVRCStringDownload result)
    {
        LogDebug("💥 OnStringLoadError callback triggered!");

        float elapsed = Time.time - loadingStartTime;
        string url = result.Url.Get();
        string error = result.Error;

        failedLoads++;
        sawFailure = true;

        LogDebug($"❌ FAILED after {elapsed:F1}s from: {url}");
        LogDebug($"💥 Error: {error}");
        LogDebug($"🔢 Error code: {result.ErrorCode}");

        // Try to identify common issues based on error message
        if (url.Contains("www.dropbox.com"))
        {
            LogDebug("🔧 SUGGESTION: Use dl.dropboxusercontent.com instead of www.dropbox.com");
        }
        if (url.Contains("&dl=0"))
        {
            LogDebug("🔧 SUGGESTION: Remove &dl=0 from URL for direct download");
        }
        if (error.Contains("403") || error.Contains("Forbidden"))
        {
            LogDebug("🔧 SUGGESTION: 403 Forbidden - check Dropbox sharing permissions");
        }
        if (error.Contains("404") || error.Contains("Not Found"))
        {
            LogDebug("🔧 SUGGESTION: 404 Not Found - check URL format and file existence");
        }

        CheckLoadingComplete();
    }

    /// <summary>
    /// Check if all downloads are complete and handle retry logic
    /// </summary>
    private void CheckLoadingComplete()
    {
        pendingLoads--;

        LogDebug($"📊 Load progress: {successfulLoads} success, {failedLoads} failed, {pendingLoads} pending");

        if (pendingLoads <= 0)
        {
            float totalElapsed = Time.time - loadingStartTime;
            isLoading = false;

            LogDebug("=== LOADING COMPLETED ===");
            LogDebug($"⏱️ Total time: {totalElapsed:F1} seconds");
            LogDebug($"📈 Results: {successfulLoads}/{totalLoadsAttempted} successful");

            if (sawFailure)
            {
                LogDebug($"🔄 Some downloads failed. Retrying in {retryDelay} seconds");
                SendCustomEventDelayedSeconds(nameof(LoadAllRemoteContent), retryDelay);
            }
            else
            {
                LogDebug("🎉 All remote content loaded successfully!");
            }
        }
    }

    /// <summary>
    /// Public method to manually trigger a content update
    /// Useful for testing or manual refresh
    /// </summary>
    public void RequestRemoteContentUpdate()
    {
        LogDebug("🔄 Manual content update requested");
        LoadAllRemoteContent();
    }

    /// <summary>
    /// Test method to verify the component is working
    /// </summary>
    [ContextMenu("Test Connection")]
    public void TestConnection()
    {
        LogDebug("=== CONNECTION TEST ===");
        LogDebug($"Component active: {gameObject.activeInHierarchy}");
        LogDebug($"Terminal controller: {(terminalController != null ? "Found" : "Missing")}");
        LogDebug($"Current status: {GetLoadingStatus()}");
        ValidateAndLogUrls();
        LogDebug("=== END CONNECTION TEST ===");
    }

    /// <summary>
    /// Centralized debug logging
    /// Only logs when debug mode is enabled to avoid console spam
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            // UdonSharp limitation: No string interpolation with complex expressions
            // Use string concatenation instead
            string coloredMessage = "<color=#CC88FF>📡 [TerminalRemoteContentLoader] " + message + "</color>";
            
            // Unity console WITH color (rich text supported)
            Debug.Log(coloredMessage);
            
            // VUdon-Logger with SAME color - DIRECT METHOD CALL!
            if (productionLogger != null)
            {
                productionLogger.Log(coloredMessage);
            }
        }
    }

    /// <summary>
    /// Get current loading status for debugging
    /// </summary>
    public string GetLoadingStatus()
    {
        if (isLoading)
            return $"Loading {pendingLoads}/{totalLoadsAttempted} files...";
        else
            return $"Idle (Last session: {successfulLoads}/{totalLoadsAttempted} successful)";
    }
}