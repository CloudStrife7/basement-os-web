using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BASEMENT OS GITHUB APPLICATION (v2.2)
///
/// ROLE: GITHUB CONTENT BROWSER
/// Multi-tab documentation viewer for README, CHANGELOG, ISSUES, THANKS.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_GitHub.cs
///
/// INTEGRATION:
/// - Core: Receives input events from DT_Core
/// - Core: Provides display content via GetDisplayContent()
/// - Uses: Terminal_Style_Guide.md box format
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
    // DISPLAY CONSTANTS & COLORS
    // =================================================================

    private const int VISIBLE_LINES = 12;      // Fits within box
    private const int WIDTH = 80;
    private const int CONTENT_W = 76;

    private const string COLOR_BORDER = "#065F46";
    private const string COLOR_PRIMARY = "#10B981";
    private const string COLOR_HEADER = "#6EE7B7";
    private const string COLOR_HIGHLIGHT = "#34D399";
    private const string COLOR_DIM = "#6B7280";

    // =================================================================
    // STATE
    // =================================================================

    private int currentTabIndex = 0;
    private int scrollOffset = 0;
    private string[] displayLines;

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    private void Start()
    {
        ValidateReferences();
    }

    /// <summary>
    /// Validates all SerializeField references (UdonSharp compliance pattern)
    /// </summary>
    private void ValidateReferences()
    {
        bool isValid = true;

        if (coreReference == null)
        {
            Debug.LogError("[DT_App_GitHub] coreReference is not assigned!");
            isValid = false;
        }

        if (shellApp == null)
        {
            Debug.LogError("[DT_App_GitHub] shellApp is not assigned!");
            isValid = false;
        }

        if (remoteContentModule == null)
        {
            Debug.LogWarning("[DT_App_GitHub] remoteContentModule is not assigned - will use fallback content");
        }

        if (!isValid)
        {
            Debug.LogError("[DT_App_GitHub] ValidateReferences FAILED - check Inspector assignments");
        }
        else
        {
            Debug.Log("[DT_App_GitHub] ValidateReferences passed");
        }
    }

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
            Debug.Log("[DT_App_GitHub] DOWN pressed, scrollOffset=" + scrollOffset.ToString() + " displayLines=" + (displayLines != null ? displayLines.Length.ToString() : "NULL") + " VISIBLE_LINES=" + VISIBLE_LINES.ToString());
            if (displayLines != null && displayLines.Length > VISIBLE_LINES)
            {
                int maxOffset = displayLines.Length - VISIBLE_LINES;
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

    /// <summary>
    /// Wrap content in box borders. Content should already be CONTENT_W visible chars.
    /// </summary>
    private string BoxRow(string content)
    {
        // Ensure content is exactly CONTENT_W to prevent border misalignment
        // Don't use DT_Format.PadLeft on content with color tags - it truncates them
        if (content == null) content = "";

        // Strip color tags for length check, then pad the original
        string plainContent = DT_Format.StripColorTags(content);
        if (plainContent.Length < CONTENT_W)
        {
            // Pad after any existing content
            int padding = CONTENT_W - plainContent.Length;
            for (int i = 0; i < padding; i++)
            {
                content = content + " ";
            }
        }

        return "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> " +
               content +
               " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
    }

    public string GetDisplayContent()
    {
        string o = "";

        // Top border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxTop(WIDTH) + "</color>\n";

        // Tab header inside box (don't use BoxRow - color tags would be truncated)
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> " + BuildTabHeader() + " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>\n";

        // Divider
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "</color>\n";

        // Content area
        int rowsUsed = 0;
        if (displayLines != null && displayLines.Length > 0)
        {
            int startLine = scrollOffset;
            int endLine = startLine + VISIBLE_LINES;
            if (endLine > displayLines.Length) endLine = displayLines.Length;

            for (int i = startLine; i < endLine && rowsUsed < VISIBLE_LINES; i++)
            {
                string line = displayLines[i];
                if (line.Length > CONTENT_W) line = line.Substring(0, CONTENT_W);
                o = o + BoxRow("<color=" + COLOR_PRIMARY + ">" + line + "</color>") + "\n";
                rowsUsed++;
            }
        }
        else
        {
            o = o + BoxRow("<color=" + COLOR_DIM + ">No content available for this tab.</color>") + "\n";
            rowsUsed++;
        }

        // Pad remaining rows
        for (int i = rowsUsed; i < VISIBLE_LINES; i++)
        {
            o = o + BoxRow("") + "\n";
        }

        // Bottom border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxBottom(WIDTH) + "</color>\n";

        // Navigation footer
        o = o + " <color=" + COLOR_PRIMARY + ">[Q/E] Tab  [W/S] Scroll  [A] Back</color>\n";

        return o;
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
        string header = "";

        for (int i = 0; i < tabNames.Length; i++)
        {
            if (i == currentTabIndex)
            {
                // Active tab with highlight
                header = header + "<color=" + COLOR_HIGHLIGHT + "><[" + tabNames[i] + "]></color>";
            }
            else
            {
                // Inactive tab
                header = header + "<color=" + COLOR_DIM + ">[" + tabNames[i] + "]</color>";
            }

            if (i < tabNames.Length - 1) header = header + " ";
        }

        return header;
    }

    private string BuildScrollIndicator()
    {
        if (displayLines == null || displayLines.Length <= VISIBLE_LINES)
        {
            return " [A] Back to Menu";
        }

        // Calculate page info
        int totalPages = (displayLines.Length + VISIBLE_LINES - 1) / VISIBLE_LINES;
        int currentPage = (scrollOffset / VISIBLE_LINES) + 1;

        // Line position info
        int startLine = scrollOffset + 1;
        int endLine = scrollOffset + VISIBLE_LINES;
        if (endLine > displayLines.Length) endLine = displayLines.Length;

        // Build indicator with page and line info
        string pageInfo = "Page " + currentPage.ToString() + "/" + totalPages.ToString();
        string lineInfo = "Lines " + startLine.ToString() + "-" + endLine.ToString();
        string controls = "[W/S] Scroll  [A] Back";

        // Format: controls left, page/line info right
        int spacing = WIDTH - controls.Length - pageInfo.Length - lineInfo.Length - 6;
        if (spacing < 1) spacing = 1;

        string pad = "";
        for (int i = 0; i < spacing; i++)
        {
            pad = pad + " ";
        }

        return " " + controls + pad + lineInfo + "  " + pageInfo;
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

        // Split into raw lines
        string[] rawLines = content.Split('\n');

        // Pre-allocate temp buffer (estimate 3x for wrapped lines)
        string[] tempLines = new string[rawLines.Length * 3];
        int lineCount = 0;

        for (int i = 0; i < rawLines.Length; i++)
        {
            string line = rawLines[i];
            line = line.Replace("\t", "    ");
            line = line.Replace("\r", "");

            // Word wrap long lines
            if (line.Length <= CONTENT_W)
            {
                if (lineCount < tempLines.Length)
                {
                    tempLines[lineCount] = " " + line;  // Add margin
                    lineCount++;
                }
            }
            else
            {
                // Wrap at word boundaries
                string[] wrapped = WrapLine(line, CONTENT_W - 2);
                for (int j = 0; j < wrapped.Length; j++)
                {
                    if (lineCount < tempLines.Length && wrapped[j] != null)
                    {
                        tempLines[lineCount] = " " + wrapped[j];
                        lineCount++;
                    }
                }
            }
        }

        // Copy to final array
        displayLines = new string[lineCount];
        for (int i = 0; i < lineCount; i++)
        {
            displayLines[i] = tempLines[i];
        }
    }

    /// <summary>
    /// Word-wrap a single line at word boundaries
    /// </summary>
    private string[] WrapLine(string line, int maxWidth)
    {
        string[] result = new string[10];  // Max 10 wrapped lines per source line
        int resultCount = 0;

        string current = "";
        int pos = 0;

        while (pos < line.Length && resultCount < result.Length)
        {
            // Find next word end
            int wordEnd = pos;
            while (wordEnd < line.Length && line[wordEnd] != ' ')
            {
                wordEnd++;
            }

            string word = line.Substring(pos, wordEnd - pos);

            // Check if word fits on current line
            if (current.Length == 0)
            {
                current = word;
            }
            else if (current.Length + 1 + word.Length <= maxWidth)
            {
                current = current + " " + word;
            }
            else
            {
                // Start new line
                result[resultCount] = current;
                resultCount++;
                current = word;
            }

            // Skip space
            pos = wordEnd + 1;
        }

        // Add remaining content
        if (current.Length > 0 && resultCount < result.Length)
        {
            result[resultCount] = current;
            resultCount++;
        }

        return result;
    }
}
