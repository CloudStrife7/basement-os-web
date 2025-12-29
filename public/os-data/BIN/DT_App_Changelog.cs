using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BASEMENT OS CHANGELOG APPLICATION (v2.1)
///
/// ROLE: DEDICATED CHANGELOG VIEWER
/// Single-purpose app for focused changelog reading experience.
/// Enhanced pagination and formatting similar to v1 changelog system.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Changelog.cs
///
/// INTEGRATION:
/// - Core: Receives input events from DT_Core
/// - Core: Provides display content via GetDisplayContent()
/// - Content: Loads from DT_RemoteContent module
/// - Uses: Terminal_Style_Guide.md box format
///
/// FEATURES:
/// - Version selector with pagination
/// - Detailed entry view with scrolling
/// - Page indicators (Page X/Y)
/// - Line position tracking
/// - Word-wrapped 78-column content
/// - Fallback content for offline mode
///
/// LIMITATIONS:
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// - UdonSharp compatibility required
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Changelog : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";
    [HideInInspector] public string contentBuffer = "";

    // =================================================================
    // MODULE REFERENCES
    // =================================================================

    [Header("--- Core Reference ---")]
    [SerializeField] private UdonSharpBehaviour coreReference;
    [SerializeField] private UdonSharpBehaviour shellApp;

    [Header("--- Content Source ---")]
    [SerializeField] private DT_RemoteContent remoteContentModule;

    [Header("--- Fallback Content ---")]
    [TextArea(10, 30)]
    [SerializeField] private string fallbackChangelog = "# CHANGELOG\n\n## VERSION 2.1 - THE HACKER UPDATE\n\nDec 26, 2025\n\n### NEW FEATURES\n\n- Achievement Tracking Stats Page\n  Integrated player achievement system with 3-tab interface.\n\n- Weather Status in Header\n  Live weather display with temperature and conditions.\n\n- Terminal Tales\n  Interactive fiction engine with branching stories.\n\n- Settings Menu\n  Persistent user preferences for notifications and display.\n\n### CODE REFACTORING\n\n- Modularized DOSTerminalController into BasementOS architecture\n- Extracted boot sequence to DT_BootSequence module\n- Separated weather logic into DT_WeatherModule\n\n### BUG FIXES\n\n- Fixed terminal input lag on some platforms\n- Resolved RSS feed update failures\n- Corrected achievement progress calculation\n\n---\n\n## VERSION 2.0 - BASEMENT OS LAUNCH\n\nNov 15, 2025\n\n### NEW FEATURES\n\n- Complete terminal rewrite with 80-column display\n- Scrolling RSS feed ticker\n- Achievement system with Xbox 360 styling\n- GitHub integration for README/CHANGELOG/ISSUES\n\n### IMPROVEMENTS\n\n- Enhanced boot sequence with BIOS screen\n- Improved weather API integration\n- Better error handling for network operations";

    // =================================================================
    // DISPLAY CONSTANTS & COLORS
    // =================================================================

    private const int VISIBLE_LINES = 12;  // Fits within box
    private const int PAGE_JUMP = 10;
    private const int WIDTH = 80;
    private const int CONTENT_W = 76;

    private const string COLOR_BORDER = "#065F46";
    private const string COLOR_PRIMARY = "#10B981";
    private const string COLOR_HEADER = "#6EE7B7";
    private const string COLOR_DIM = "#6B7280";

    // =================================================================
    // DISPLAY STATE
    // =================================================================

    private string[] displayLines;
    private int totalLines = 0;
    private int scrollOffset = 0;

    // =================================================================
    // LIFECYCLE
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
            Debug.LogError("[DT_App_Changelog] coreReference is not assigned!");
            isValid = false;
        }

        if (shellApp == null)
        {
            Debug.LogError("[DT_App_Changelog] shellApp is not assigned!");
            isValid = false;
        }

        if (remoteContentModule == null)
        {
            Debug.LogWarning("[DT_App_Changelog] remoteContentModule is not assigned - will use fallback content");
        }

        if (!isValid)
        {
            Debug.LogError("[DT_App_Changelog] ValidateReferences FAILED - check Inspector assignments");
        }
        else
        {
            Debug.Log("[DT_App_Changelog] ValidateReferences passed");
        }
    }

    public void OnAppOpen()
    {
        scrollOffset = 0;
        LoadChangelogContent();
        GenerateDisplay();
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        scrollOffset = 0;
    }

    private void LoadChangelogContent()
    {
        string rawContent = "";

        if (Utilities.IsValid(remoteContentModule) && remoteContentModule.hasRemoteContent)
        {
            rawContent = remoteContentModule.remoteChangelogContent;
        }

        if (string.IsNullOrEmpty(rawContent))
        {
            rawContent = fallbackChangelog;
        }

        ProcessContent(rawContent);
    }

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        if (inputKey == "UP")
        {
            if (scrollOffset > 0)
            {
                scrollOffset--;
                GenerateDisplay();
                PushDisplayToCore();
            }
        }
        else if (inputKey == "DOWN")
        {
            int maxScroll = totalLines - VISIBLE_LINES;
            if (maxScroll < 0) maxScroll = 0;
            if (scrollOffset < maxScroll)
            {
                scrollOffset++;
                GenerateDisplay();
                PushDisplayToCore();
            }
        }
        else if (inputKey == "PAGE_UP")
        {
            scrollOffset = scrollOffset - PAGE_JUMP;
            if (scrollOffset < 0) scrollOffset = 0;
            GenerateDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "PAGE_DOWN")
        {
            int maxScroll = totalLines - VISIBLE_LINES;
            if (maxScroll < 0) maxScroll = 0;
            scrollOffset = scrollOffset + PAGE_JUMP;
            if (scrollOffset > maxScroll) scrollOffset = maxScroll;
            GenerateDisplay();
            PushDisplayToCore();
        }
        else if (inputKey == "LEFT" || inputKey == "ACCEPT" || inputKey == "BACK")
        {
            ReturnToShell();
        }

        inputKey = "";
    }

    private void ReturnToShell()
    {
        if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
        {
            coreReference.SetProgramVariable("nextProcess", shellApp);
            coreReference.SendCustomEvent("LoadNextProcess");
        }
    }

    // =================================================================
    // DISPLAY GENERATION
    // =================================================================

    private string BoxRow(string content)
    {
        return "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> " +
               DT_Format.PadLeft(content, CONTENT_W) +
               " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
    }

    public string GetDisplayContent()
    {
        return contentBuffer;
    }

    private void GenerateDisplay()
    {
        string o = "";

        // Page info
        int totalPages = 1;
        int currentPage = 1;
        if (totalLines > 0)
        {
            totalPages = (totalLines + VISIBLE_LINES - 1) / VISIBLE_LINES;
            currentPage = (scrollOffset / VISIBLE_LINES) + 1;
        }
        string pageInfo = "Page " + currentPage.ToString() + "/" + totalPages.ToString();

        // Top border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxTop(WIDTH) + "</color>\n";

        // Title with page info
        string titleLine = "CHANGELOG" + DT_Format.RepeatChar(' ', CONTENT_W - 9 - pageInfo.Length) + pageInfo;
        o = o + BoxRow("<color=" + COLOR_HEADER + ">" + titleLine + "</color>") + "\n";

        // Divider
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "</color>\n";

        // Content
        int rowsUsed = 0;
        if (displayLines != null && displayLines.Length > 0)
        {
            int startLine = scrollOffset;
            int endLine = startLine + VISIBLE_LINES;
            if (endLine > totalLines) endLine = totalLines;

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
            o = o + BoxRow("<color=" + COLOR_DIM + ">No changelog content available.</color>") + "\n";
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
        o = o + " <color=" + COLOR_PRIMARY + ">[W/S] Scroll  [A] Back</color>\n";

        contentBuffer = o;
    }

    /// <summary>
    /// Push display content to DT_Core for rendering
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", contentBuffer);
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    // =================================================================
    // CONTENT PROCESSING
    // =================================================================

    private void ProcessContent(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            displayLines = new string[0];
            totalLines = 0;
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

        totalLines = lineCount;
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

    // =================================================================
    // UTILITY METHODS
    // =================================================================

    /// <summary>
    /// Pads a string to a specific width with spaces
    /// </summary>
    private string PadToWidth(string text, int targetWidth)
    {
        if (text.Length >= targetWidth) return "";

        int spacesNeeded = targetWidth - text.Length;
        string padding = "";
        for (int i = 0; i < spacesNeeded; i++)
        {
            padding = padding + " ";
        }
        return padding;
    }

    /// <summary>
    /// Generates separator line with custom character
    /// </summary>
    private string GenerateSeparator(char c)
    {
        string separator = "";
        for (int i = 0; i < WIDTH; i++)
        {
            separator = separator + c;
        }
        return separator;
    }
}
