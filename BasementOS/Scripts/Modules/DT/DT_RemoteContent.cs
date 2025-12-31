using UdonSharp;
using UnityEngine;

/// <summary>
/// DOS Terminal Remote Content Module
/// Manages remote content loading and storage from external sources (GitHub Pages).
/// Extracted from DOSTerminalController.cs as part of modularization (Phase 2).
/// </summary>
public class DT_RemoteContent : UdonSharpBehaviour
{
    [Header("Core Controller Reference")]
    [Tooltip("Reference to main controller for callbacks")]
    public DOSTerminalController coreController;

    [Header("Remote Content Loader")]
    [Tooltip("Reference to TerminalRemoteContentLoader for direct method calls")]
    public TerminalRemoteContentLoader remoteContentLoader;

    [Header("Remote Content Settings")]
    public bool useRemoteContent = true;
    public float remoteCheckInterval = 300f;

    [Header("Debug Settings")]
    public bool enableDebugLogging = true;

    // =================================================================
    // CONTENT STORAGE (Public for Core to read)
    // =================================================================

    [HideInInspector] public string remoteSplashContent = "";
    [HideInInspector] public string remoteBiosContent = "";
    [HideInInspector] public string remoteDashboardContent = "";
    [HideInInspector] public string remoteChangelogContent = "";
    [HideInInspector] public string remoteHallOfFameContent = "";
    [HideInInspector] public bool hasRemoteContent = false;

    // =================================================================
    // PUBLIC API
    // =================================================================

    /// <summary>
    /// Receives and stores remotely loaded content from TerminalRemoteContentLoader.
    /// Updates the appropriate content field based on contentType parameter.
    /// Supported content types: "splash", "bios", "dashboard", "changelog", "halloffame".
    /// Called by TerminalRemoteContentLoader after successful async download.
    /// </summary>
    /// <param name="contentType">Type of content to update (case-insensitive)</param>
    /// <param name="content">Text content to store for the specified type</param>
    public void UpdateContent(string contentType, string content)
    {
        LogDebug($"📡 Remote content updated: {contentType}");

        switch (contentType.ToLower())
        {
            case "splash": remoteSplashContent = content; break;
            case "bios": remoteBiosContent = content; break;
            case "dashboard": remoteDashboardContent = content; break;
            case "changelog": remoteChangelogContent = content; break;
            case "halloffame": remoteHallOfFameContent = content; break;
        }
        hasRemoteContent = true;

        // Notify core controller that content changed
        if (coreController != null)
        {
            coreController.MarkContentDirty();
        }
    }

    /// <summary>
    /// Checks for remote content updates and schedules next update cycle.
    /// Directly calls RequestRemoteContentUpdate() on TerminalRemoteContentLoader.
    /// Recursively schedules itself for periodic updates if useRemoteContent is enabled.
    /// FIXED: Changed from SendCustomEvent to direct method call (CLAUDE.md pattern).
    /// </summary>
    public void CheckForUpdates()
    {
        LogDebug("🔄 Checking for remote content updates");

        if (remoteContentLoader != null)
        {
            remoteContentLoader.RequestRemoteContentUpdate();
        }
        else
        {
            LogDebug("⚠️ TerminalRemoteContentLoader reference not assigned");
        }

        if (useRemoteContent)
        {
            SendCustomEventDelayedSeconds("CheckForUpdates", remoteCheckInterval);
        }
    }

    /// <summary>
    /// Creates scrollable content by truncating to last N lines.
    /// Utility method for pagination (currently unused but preserved for future use).
    /// </summary>
    /// <param name="fullContent">Full content string</param>
    /// <param name="maxLines">Maximum lines to show (0 = no limit)</param>
    /// <returns>Truncated content showing only last maxLines</returns>
    public string CreateScrollableContent(string fullContent, int maxLines)
    {
        if (maxLines <= 0) return fullContent;

        string[] allLines = fullContent.Split('\n');

        if (allLines.Length <= maxLines)
            return fullContent;

        string[] visibleLines = new string[maxLines];
        int startIndex = allLines.Length - maxLines;

        for (int i = 0; i < maxLines; i++)
        {
            visibleLines[i] = allLines[startIndex + i];
        }

        return string.Join("\n", visibleLines);
    }

    // =================================================================
    // PRIVATE METHODS
    // =================================================================

    /// <summary>
    /// Logs debug message if debug logging is enabled.
    /// </summary>
    /// <param name="message">Message to log</param>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[DT_RemoteContent] {message}");
        }
    }
}
