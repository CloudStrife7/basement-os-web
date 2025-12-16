using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BASEMENT OS GITHUB APPLICATION (v2.1)
///
/// ROLE: GITHUB CONTENT BROWSER
/// Multi-tab documentation viewer for README, CHANGELOG, ISSUES, THANKS.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_GitHub.cs
///
/// INTEGRATION:
/// - Core: Receives input events from DT_Core
/// - Core: Provides display content via GetDisplayContent()
///
/// LIMITATIONS:
/// - Max 300 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_GitHub : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // MODULE REFERENCES
    // =================================================================

    [Header("--- External Systems ---")]
    [Tooltip("Remote content module with cached GitHub content")]
    [SerializeField] private DT_RemoteContent remoteContentModule;

    [Tooltip("Reference to DT_Core")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Tooltip("Shell app to return to on ACCEPT key")]
    [SerializeField] private UdonSharpBehaviour shellApp;

    // =================================================================
    // TAB CONFIGURATION
    // =================================================================

    [Header("--- Tab Configuration ---")]
    [SerializeField] private string[] tabNames = new string[] { "README", "CHANGELOG", "ISSUES", "THANKS" };

    [Header("--- Fallback Content ---")]
    [TextArea(5, 15)]
    [SerializeField] private string fallbackReadme = "# Lower Level 2.0 - BASEMENT OS\n\nA VRChat world featuring an immersive retro-futuristic terminal.\n\n## Features\n- 80-character wide terminal display\n- Multiple applications and utilities\n- Achievement system\n- Weather integration";

    [TextArea(5, 15)]
    [SerializeField] private string fallbackChangelog = "# CHANGELOG\n\n## v2.1 - The Hacker Update\n+ Added 80-character wide terminal\n+ Implemented scrolling RSS feed\n+ New achievement system\n+ Weather module with live API";

    [TextArea(5, 15)]
    [SerializeField] private string fallbackIssues = "# Known Issues\n\n- Terminal input lag on some platforms\n- RSS feed occasionally fails to update\n\n## Planned Features\n- User customizable themes\n- More built-in applications";

    [TextArea(5, 15)]
    [SerializeField] private string fallbackThanks = "# Special Thanks\n\nThis project would not be possible without the community.\n\n## Technology\n- VRChat SDK3\n- UdonSharp\n- Unity\n\nThank you for visiting Lower Level 2.0!";

    // =================================================================
    // STATE
    // =================================================================

    private int currentTabIndex = 0;
    private int scrollOffset = 0;
    private string[] displayLines;
    private int visibleLines = 12;
    private int terminalWidth = 80;

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        currentTabIndex = 0;
        scrollOffset = 0;
        LoadCurrentTab();
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        scrollOffset = 0;
    }

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        Debug.Log("[DT_App_GitHub] OnInput called with key='" + inputKey + "' currentTabIndex=" + currentTabIndex.ToString());
        
        if (inputKey == "LEFT")
        {
            Debug.Log("[DT_App_GitHub] LEFT pressed, currentTabIndex=" + currentTabIndex.ToString());
            // If on first tab, return to Shell menu
            if (currentTabIndex == 0)
            {
                Debug.Log("[DT_App_GitHub] On first tab, attempting to return to Shell");
                Debug.Log("[DT_App_GitHub] coreReference valid: " + Utilities.IsValid(coreReference).ToString());
                Debug.Log("[DT_App_GitHub] shellApp valid: " + Utilities.IsValid(shellApp).ToString());
                
                if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
                {
                    coreReference.SetProgramVariable("nextProcess", shellApp);
                    coreReference.SendCustomEvent("LoadNextProcess");
                    Debug.Log("[DT_App_GitHub] LoadNextProcess called successfully");
                }
                else
                {
                    Debug.LogWarning("[DT_App_GitHub] Cannot return to shell - coreReference or shellApp not assigned!");
                }
            }
            else
            {
                // Go to previous tab
                currentTabIndex--;
                scrollOffset = 0;
                LoadCurrentTab();
                PushDisplayToCore();
            }
        }
        else if (inputKey == "RIGHT")
        {
            currentTabIndex++;
            if (currentTabIndex >= tabNames.Length) currentTabIndex = 0;
            scrollOffset = 0;
            LoadCurrentTab();
            PushDisplayToCore();
        }
        else if (inputKey == "UP")
        {
            Debug.Log("[DT_App_GitHub] UP pressed, scrollOffset=" + scrollOffset.ToString() + " displayLines=" + (displayLines != null ? displayLines.Length.ToString() : "NULL"));
            if (scrollOffset > 0) scrollOffset--;
            PushDisplayToCore();
        }
        else if (inputKey == "DOWN")
        {
            Debug.Log("[DT_App_GitHub] DOWN pressed, scrollOffset=" + scrollOffset.ToString() + " displayLines=" + (displayLines != null ? displayLines.Length.ToString() : "NULL") + " visibleLines=" + visibleLines.ToString());
            if (displayLines != null && displayLines.Length > visibleLines)
            {
                int maxOffset = displayLines.Length - visibleLines;
                Debug.Log("[DT_App_GitHub] maxOffset=" + maxOffset.ToString());
                if (scrollOffset < maxOffset) scrollOffset++;
            }
            PushDisplayToCore();
        }
        else if (inputKey == "ACCEPT" || inputKey == "BACK")
        {
            // Return to Shell menu
            if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
            {
                coreReference.SetProgramVariable("nextProcess", shellApp);
                coreReference.SendCustomEvent("LoadNextProcess");
            }
        }

        inputKey = "";
    }

    // =================================================================
    // DISPLAY RENDERING
    // =================================================================

    public string GetDisplayContent()
    {
        string output = "";

        output = output + BuildTabHeader() + "\n";
        output = output + "────────────────────────────────────────────────────────────────────────────────\n";

        if (displayLines != null && displayLines.Length > 0)
        {
            int startLine = scrollOffset;
            int endLine = startLine + visibleLines;
            if (endLine > displayLines.Length) endLine = displayLines.Length;

            for (int i = startLine; i < endLine; i++)
            {
                output = output + displayLines[i] + "\n";
            }

            if (displayLines.Length > visibleLines)
            {
                output = output + "\n";
                output = output + BuildScrollIndicator();
            }
        }
        else
        {
            output = output + "No content available for this tab.\n";
        }

        return output;
    }

    /// <summary>
    /// Push display content to DT_Core for rendering
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", GetDisplayContent());
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    private string BuildTabHeader()
    {
        string header = " ";

        for (int i = 0; i < tabNames.Length; i++)
        {
            if (i == currentTabIndex)
            {
                header = header + ">[" + tabNames[i] + "]<";
            }
            else
            {
                header = header + "[" + tabNames[i] + "]";
            }

            if (i < tabNames.Length - 1) header = header + "  ";
        }

        return header;
    }

    private string BuildScrollIndicator()
    {
        if (displayLines == null || displayLines.Length <= visibleLines) return "";

        int currentLine = scrollOffset + 1;
        int endLine = scrollOffset + visibleLines;
        if (endLine > displayLines.Length) endLine = displayLines.Length;

        return "                        Lines " + currentLine.ToString() + "-" + endLine.ToString() + " of " + displayLines.Length.ToString();
    }

    // =================================================================
    // CONTENT MANAGEMENT
    // =================================================================

    private void LoadCurrentTab()
    {
        string rawContent = "";

        if (currentTabIndex == 0)
        {
            // README - check for remote content first
            if (Utilities.IsValid(remoteContentModule) && remoteContentModule.hasRemoteContent)
            {
                string remoteReadme = remoteContentModule.remoteDashboardContent;
                if (!string.IsNullOrEmpty(remoteReadme))
                {
                    rawContent = remoteReadme;
                }
                else
                {
                    rawContent = fallbackReadme;
                }
            }
            else
            {
                rawContent = fallbackReadme;
            }
        }
        else if (currentTabIndex == 1)
        {
            // CHANGELOG - check for remote content first
            if (Utilities.IsValid(remoteContentModule) && remoteContentModule.hasRemoteContent)
            {
                string remoteChangelog = remoteContentModule.remoteChangelogContent;
                if (!string.IsNullOrEmpty(remoteChangelog))
                {
                    rawContent = remoteChangelog;
                }
                else
                {
                    rawContent = fallbackChangelog;
                }
            }
            else
            {
                rawContent = fallbackChangelog;
            }
        }
        else if (currentTabIndex == 2)
        {
            rawContent = fallbackIssues;
        }
        else if (currentTabIndex == 3)
        {
            // THANKS - check for Hall of Fame content
            if (Utilities.IsValid(remoteContentModule) && remoteContentModule.hasRemoteContent)
            {
                string remoteHoF = remoteContentModule.remoteHallOfFameContent;
                if (!string.IsNullOrEmpty(remoteHoF))
                {
                    rawContent = remoteHoF;
                }
                else
                {
                    rawContent = fallbackThanks;
                }
            }
            else
            {
                rawContent = fallbackThanks;
            }
        }

        ProcessContent(rawContent);
    }

    private void ProcessContent(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            displayLines = new string[0];
            return;
        }

        string[] rawLines = content.Split('\n');
        string[] tempLines = new string[rawLines.Length * 2];
        int lineCount = 0;

        for (int i = 0; i < rawLines.Length; i++)
        {
            string line = rawLines[i];
            line = line.Replace("\t", "    ");
            line = line.Replace("\r", "");

            if (line.Length <= terminalWidth)
            {
                tempLines[lineCount] = line;
                lineCount++;
            }
            else
            {
                int pos = 0;
                while (pos < line.Length)
                {
                    int remaining = line.Length - pos;
                    int chunkSize = remaining < terminalWidth ? remaining : terminalWidth;
                    tempLines[lineCount] = line.Substring(pos, chunkSize);
                    lineCount++;
                    pos = pos + chunkSize;
                }
            }
        }

        displayLines = new string[lineCount];
        for (int i = 0; i < lineCount; i++)
        {
            displayLines[i] = tempLines[i];
        }
    }
}
