using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Persistence;

/// <summary>
/// BASEMENT OS TERMINAL TALES (v3.1) - Loading Screen Update
///
/// ROLE: Story browser with Markdown parsing and loading screen animation
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Tales.cs
///
/// FEATURES:
/// - Box-drawing borders (DOS style)
/// - Animated loading screen with preamble
/// - Progress bar with blocks
/// - [A][D] navigation pattern
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Tales : UdonSharpBehaviour
{
    // =================================================================
    // INTERFACE
    // =================================================================

    [HideInInspector] public string inputKey = "";

    [Header("Core References")]
    [SerializeField] private UdonSharpBehaviour coreReference;
    [SerializeField] private UdonSharpBehaviour shellApp;

    [Header("Story Content (Markdown with header + preamble)")]
    [TextArea(10, 30)]
    [SerializeField] private string[] storyContent;

    // =================================================================
    // STATE
    // =================================================================

    private const int STATE_BROWSER = 0;
    private const int STATE_LOADING = 1;
    private const int STATE_READING = 2;

    private int currentState = STATE_BROWSER;
    private int cursorIndex = 0;
    private int scrollOffset = 0;

    // Parsed story data
    private string[] titles;
    private string[] authors;
    private string[] categories;
    private string[] readTimes;
    private string[] preambles;  // Loading screen text (between ```)
    private string[] bodies;
    private bool[] storyRead;
    private int storyCount = 0;

    // Loading animation state
    private string[] currentPreambleLines;
    private int preambleLineCount = 0;
    private int loadingStep = 0;
    private int dotCount = 0;
    private const float ANIMATION_DELAY = 0.5f;

    // Wrapped content for reader
    private string[] wrappedLines;
    private int wrappedCount = 0;

    // Display constants
    private const int WIDTH = 80;
    private const int CONTENT_W = 76;
    private const int VISIBLE = 14;

    // =================================================================
    // LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        ParseAllStories();
        LoadReadProgress();
        currentState = STATE_BROWSER;
        cursorIndex = 0;
        ShowBrowser();
    }

    public void OnAppClose()
    {
        SaveReadProgress();
    }

    // =================================================================
    // MARKDOWN PARSING (Header + Preamble format)
    // =================================================================

    private void ParseAllStories()
    {
        if (storyContent == null || storyContent.Length == 0)
        {
            LoadFallbackStory();
            return;
        }

        storyCount = storyContent.Length;
        titles = new string[storyCount];
        authors = new string[storyCount];
        categories = new string[storyCount];
        readTimes = new string[storyCount];
        preambles = new string[storyCount];
        bodies = new string[storyCount];
        storyRead = new bool[storyCount];

        for (int i = 0; i < storyCount; i++)
        {
            ParseMarkdown(storyContent[i], i);
        }
    }

    private void ParseMarkdown(string md, int idx)
    {
        // Default values
        titles[idx] = "Untitled";
        authors[idx] = "Anonymous";
        categories[idx] = "Story";
        readTimes[idx] = "5 min";
        preambles[idx] = "";
        bodies[idx] = "";

        if (string.IsNullOrEmpty(md)) return;

        string[] lines = md.Split('\n');
        int lineIdx = 0;

        // Parse header: # TITLE.TXT
        if (lineIdx < lines.Length && lines[lineIdx].StartsWith("#"))
        {
            string titleLine = lines[lineIdx].Substring(1).Trim();
            // Remove .TXT extension for display
            if (titleLine.EndsWith(".TXT")) titleLine = titleLine.Substring(0, titleLine.Length - 4);
            titles[idx] = titleLine;
            lineIdx++;
        }

        // Parse metadata lines (Category:, Created:, Rating:, Tags:)
        while (lineIdx < lines.Length)
        {
            string line = lines[lineIdx].Trim();
            if (line.Length == 0) { lineIdx++; continue; }
            if (line.StartsWith("```")) break;  // Hit preamble

            int colonIdx = line.IndexOf(':');
            if (colonIdx > 0)
            {
                string key = line.Substring(0, colonIdx).Trim().ToLower();
                string val = line.Substring(colonIdx + 1).Trim();

                if (key == "category") categories[idx] = val;
                else if (key == "created") authors[idx] = val;  // Use created date as author for now
                else if (key == "tags") authors[idx] = val;     // Or use tags
            }
            lineIdx++;
        }

        // Parse preamble (between first ``` and second ```)
        if (lineIdx < lines.Length && lines[lineIdx].Trim().StartsWith("```"))
        {
            lineIdx++;  // Skip opening ```
            string preambleText = "";
            while (lineIdx < lines.Length && !lines[lineIdx].Trim().StartsWith("```"))
            {
                if (preambleText.Length > 0) preambleText = preambleText + "\n";
                preambleText = preambleText + lines[lineIdx];
                lineIdx++;
            }
            preambles[idx] = preambleText;
            lineIdx++;  // Skip closing ```
        }

        // Skip to story body (after ## TITLE or next content)
        while (lineIdx < lines.Length)
        {
            string line = lines[lineIdx].Trim();
            if (line.StartsWith("## "))
            {
                lineIdx++;  // Skip ## header
                break;
            }
            lineIdx++;
        }

        // Skip subtitle line (*A Terminal Tale...*)
        if (lineIdx < lines.Length && lines[lineIdx].Trim().StartsWith("*"))
        {
            lineIdx++;
        }

        // Rest is the body - collect until we hit the ending code block
        string bodyText = "";
        while (lineIdx < lines.Length)
        {
            string line = lines[lineIdx];
            // Stop at ending code block (```\n> STORY COMPLETE)
            if (line.Trim().StartsWith("```") || line.Trim() == "---")
            {
                break;
            }
            if (bodyText.Length > 0) bodyText = bodyText + "\n";
            bodyText = bodyText + line;
            lineIdx++;
        }
        bodies[idx] = bodyText.Trim();

        // Estimate read time based on word count
        int wordCount = CountWords(bodies[idx]);
        int minutes = (wordCount / 200) + 1;  // ~200 words per minute
        readTimes[idx] = minutes.ToString() + " min";
    }

    private int CountWords(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        string[] words = text.Split(' ');
        return words.Length;
    }

    private void LoadFallbackStory()
    {
        storyCount = 1;
        titles = new string[] { "Starry Night (Demo)" };
        authors = new string[] { "CloudStrife7" };
        categories = new string[] { "Gaming" };
        readTimes = new string[] { "8 min" };
        preambles = new string[] { "> ACCESSING MEMORY BANKS...\n> LOADING STORY: \"STARRY NIGHT\"\n> DATE: LATE 2006\n> STATUS: [HALO 3 HYPE DETECTED]\n> WARNING: CONTAINS GAME FUEL" };
        storyRead = new bool[] { false };
        bodies = new string[] {
            "Joe's grandma's basement had become our sanctuary during college - the only place in our friend group with actual high-speed internet.\n\nThe wood-paneled walls were covered with old family photos, but in the corner sat Joe's setup: a decent TV, his Xbox 360, and most importantly, that blessed broadband connection.\n\nIt was late 2006, and Halo 3 marketing had reached fever pitch. Mountain Dew had just released Game Fuel - that red stuff we were convinced gave us actual gaming powers.\n\n\"Dude, check this out,\" Joe said, navigating through the Xbox 360 dashboard. \"I just downloaded this Halo 3 trailer. It's called Starry Night.\"\n\nOne minute and thirty-three seconds, the runtime said. I had no idea what I was in for.\n\nIt started with just a piano. A single, melancholic note. Then we saw a kid standing in a museum, gazing up at Master Chief's battle-damaged helmet displayed like some ancient artifact.\n\nThe kid's eyes changed. Became determined. Then we saw him older, running through a forest at night in makeshift Spartan armor.\n\nNeither of us moved when it ended.\n\n\"Play it again,\" I said quietly.\n\nWe watched it seven times that night."
        };
    }

    // =================================================================
    // LOADING SCREEN ANIMATION
    // =================================================================

    private void StartLoadingScreen()
    {
        currentState = STATE_LOADING;
        loadingStep = 0;
        dotCount = 0;

        // Parse preamble into lines
        string preamble = preambles[cursorIndex];
        if (string.IsNullOrEmpty(preamble))
        {
            // No preamble, skip to reading
            StartReading();
            return;
        }

        string[] rawLines = preamble.Split('\n');
        currentPreambleLines = new string[20];
        preambleLineCount = 0;

        for (int i = 0; i < rawLines.Length && preambleLineCount < 20; i++)
        {
            string line = rawLines[i].Trim();
            if (line.Length > 0)
            {
                currentPreambleLines[preambleLineCount] = line;
                preambleLineCount++;
            }
        }

        if (preambleLineCount == 0)
        {
            StartReading();
            return;
        }

        // Show first line immediately
        ShowLoadingFrame();

        // Start dot animation after delay
        SendCustomEventDelayedSeconds(nameof(AnimateDot), ANIMATION_DELAY);
    }

    public void AnimateDot()
    {
        if (currentState != STATE_LOADING) return;

        dotCount++;
        ShowLoadingFrame();

        if (dotCount < 3)
        {
            // Continue dot animation
            SendCustomEventDelayedSeconds(nameof(AnimateDot), ANIMATION_DELAY);
        }
        else
        {
            // Start showing remaining lines
            loadingStep = 1;
            SendCustomEventDelayedSeconds(nameof(AnimateNextLine), ANIMATION_DELAY);
        }
    }

    public void AnimateNextLine()
    {
        if (currentState != STATE_LOADING) return;

        loadingStep++;
        ShowLoadingFrame();

        if (loadingStep < preambleLineCount)
        {
            SendCustomEventDelayedSeconds(nameof(AnimateNextLine), ANIMATION_DELAY);
        }
        else
        {
            // Animation complete, show "Press [D] to continue"
            loadingStep = preambleLineCount + 1;  // Signal complete
            ShowLoadingFrame();
        }
    }

    private void ShowLoadingFrame()
    {
        string o = "";

        // Top border
        o = o + DT_Format.GenerateBoxTop(WIDTH) + "\n";

        // Header
        string header = "LOADING...";
        o = o + BoxRow(DT_Format.PadCenter(header, CONTENT_W)) + "\n";

        // Divider
        o = o + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "\n";

        // Content area - show animated preamble
        int contentLines = 0;

        // First line with animated dots at the end
        if (preambleLineCount > 0)
        {
            string firstLine = currentPreambleLines[0];
            // Add dots to end of first line during dot animation phase
            if (dotCount > 0 && dotCount <= 3)
            {
                for (int d = 0; d < dotCount; d++)
                {
                    firstLine = firstLine + ".";
                }
            }
            o = o + BoxRow(" " + DT_Format.PadLeft(firstLine, CONTENT_W - 1)) + "\n";
            contentLines++;
        }

        // Show remaining preamble lines based on loadingStep
        for (int i = 1; i < preambleLineCount && i <= loadingStep; i++)
        {
            o = o + BoxRow(" " + DT_Format.PadLeft(currentPreambleLines[i], CONTENT_W - 1)) + "\n";
            contentLines++;
        }

        // Pad to fill content area (14 lines total)
        for (int i = contentLines; i < VISIBLE; i++)
        {
            o = o + BoxRow(DT_Format.RepeatChar(' ', CONTENT_W)) + "\n";
        }

        // Bottom border
        o = o + DT_Format.GenerateBoxBottom(WIDTH) + "\n";

        // Footer
        if (loadingStep >= preambleLineCount)
        {
            o = o + " Press [D] to continue...\n";
        }
        else
        {
            o = o + "\n";
        }

        PushDisplay(o);
    }

    private void StartReading()
    {
        currentState = STATE_READING;
        scrollOffset = 0;
        wrappedCount = 0;
        ShowReader();
    }

    // =================================================================
    // BROWSER DISPLAY (Style Guide Compliant)
    // =================================================================

    private void ShowBrowser()
    {
        string o = "";
        int readCount = CountRead();

        // Top border
        o = o + DT_Format.GenerateBoxTop(WIDTH) + "\n";

        // Header with centered title
        string header = "TERMINAL TALES";
        o = o + BoxRow(DT_Format.PadCenter(header, CONTENT_W)) + "\n";

        // Divider
        o = o + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "\n";

        // Story list (show 10 stories max with scrolling)
        int startIdx = 0;
        int showCount = 10;
        if (cursorIndex >= showCount) startIdx = cursorIndex - showCount + 1;

        for (int i = startIdx; i < startIdx + showCount && i < storyCount; i++)
        {
            string cursor = (i == cursorIndex) ? ">" : " ";
            string status = storyRead[i] ? "[READ]" : "[NEW] ";
            string title = DT_Format.Truncate(titles[i], 24) + ".TXT";
            string cat = "[" + DT_Format.PadLeft(categories[i], 10) + "]";

            string line = cursor + " " + DT_Format.PadLeft(title, 28) + " " + cat + " " + status;
            o = o + BoxRow(DT_Format.PadLeft(line, CONTENT_W)) + "\n";
        }

        // Pad empty rows
        int shown = (startIdx + showCount > storyCount) ? storyCount - startIdx : showCount;
        for (int i = shown; i < 10; i++)
        {
            o = o + BoxRow(DT_Format.RepeatChar(' ', CONTENT_W)) + "\n";
        }

        // Progress bar row
        string progress = DT_Format.GenerateStyleProgress(readCount, storyCount, 25);
        o = o + BoxRow(DT_Format.PadLeft(" Stories Read: " + progress, CONTENT_W)) + "\n";

        // Bottom border
        o = o + DT_Format.GenerateBoxBottom(WIDTH) + "\n";

        // Navigation footer
        o = o + " [A] Back  [D] Read Story  [W/S] Navigate\n";

        PushDisplay(o);
    }

    // =================================================================
    // READER DISPLAY
    // =================================================================

    private void ShowReader()
    {
        if (wrappedCount == 0) WrapContent(bodies[cursorIndex]);

        string o = "";
        int totalPages = (wrappedCount + VISIBLE - 1) / VISIBLE;
        if (totalPages < 1) totalPages = 1;
        int curPage = (scrollOffset / VISIBLE) + 1;

        // Top border
        o = o + DT_Format.GenerateBoxTop(WIDTH) + "\n";

        // Title + page indicator
        string titleTrunc = DT_Format.Truncate(titles[cursorIndex], 50);
        string pageInfo = "[" + curPage.ToString() + "/" + totalPages.ToString() + "]";
        o = o + BoxRow(DT_Format.FormatTwoColumn(" " + titleTrunc, pageInfo + " ", CONTENT_W)) + "\n";

        // Divider
        o = o + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "\n";

        // Content
        int endLine = scrollOffset + VISIBLE;
        if (endLine > wrappedCount) endLine = wrappedCount;

        for (int i = scrollOffset; i < endLine; i++)
        {
            o = o + BoxRow(" " + DT_Format.PadLeft(wrappedLines[i], CONTENT_W - 1)) + "\n";
        }

        // Pad remaining
        for (int i = endLine - scrollOffset; i < VISIBLE; i++)
        {
            o = o + BoxRow(DT_Format.RepeatChar(' ', CONTENT_W)) + "\n";
        }

        // Bottom border
        o = o + DT_Format.GenerateBoxBottom(WIDTH) + "\n";

        // Footer
        bool atEnd = (scrollOffset + VISIBLE >= wrappedCount);
        if (atEnd)
        {
            MarkRead(cursorIndex);
            o = o + " [A] Back to Stories  -- Story Complete! --\n";
        }
        else
        {
            o = o + " [A] Back  [W/S] Scroll  [D] Page Down\n";
        }

        PushDisplay(o);
    }

    /// <summary>
    /// Wrap content in box borders. Content MUST be exactly CONTENT_W (76) chars!
    /// Result: ║ + space + content(76) + space + ║ = 80 chars
    /// </summary>
    private string BoxRow(string content)
    {
        // Ensure content is exactly CONTENT_W to prevent border misalignment
        if (content == null) content = "";
        if (content.Length < CONTENT_W)
        {
            content = DT_Format.PadLeft(content, CONTENT_W);
        }
        else if (content.Length > CONTENT_W)
        {
            content = content.Substring(0, CONTENT_W);
        }
        return DT_Format.BORDER_VERTICAL + " " + content + " " + DT_Format.BORDER_VERTICAL;
    }

    // =================================================================
    // INPUT
    // =================================================================

    public void OnInput()
    {
        if (currentState == STATE_BROWSER) HandleBrowserInput();
        else if (currentState == STATE_LOADING) HandleLoadingInput();
        else HandleReaderInput();
        inputKey = "";
    }

    private void HandleBrowserInput()
    {
        if (inputKey == "UP") { cursorIndex--; if (cursorIndex < 0) cursorIndex = storyCount - 1; ShowBrowser(); }
        else if (inputKey == "DOWN") { cursorIndex++; if (cursorIndex >= storyCount) cursorIndex = 0; ShowBrowser(); }
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT") { StartLoadingScreen(); }
        else if (inputKey == "LEFT") { ReturnToShell(); }
    }

    private void HandleLoadingInput()
    {
        // Only respond to input when animation is complete
        if (loadingStep >= preambleLineCount)
        {
            if (inputKey == "ACCEPT" || inputKey == "RIGHT")
            {
                StartReading();
            }
            else if (inputKey == "LEFT")
            {
                currentState = STATE_BROWSER;
                ShowBrowser();
            }
        }
    }

    private void HandleReaderInput()
    {
        int maxScroll = wrappedCount - VISIBLE; if (maxScroll < 0) maxScroll = 0;
        if (inputKey == "UP") { if (scrollOffset > 0) scrollOffset--; ShowReader(); }
        else if (inputKey == "DOWN" || inputKey == "ACCEPT") { if (scrollOffset < maxScroll) scrollOffset++; ShowReader(); }
        else if (inputKey == "LEFT") { currentState = STATE_BROWSER; scrollOffset = 0; ShowBrowser(); }
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
    // TEXT WRAPPING
    // =================================================================

    private void WrapContent(string text)
    {
        wrappedLines = new string[200];
        wrappedCount = 0;
        if (string.IsNullOrEmpty(text)) return;

        string[] paras = text.Split('\n');
        for (int p = 0; p < paras.Length && wrappedCount < 199; p++)
        {
            string para = paras[p].Trim();
            if (para.Length == 0) { wrappedLines[wrappedCount++] = ""; continue; }

            string[] words = para.Split(' ');
            string line = "";
            for (int w = 0; w < words.Length; w++)
            {
                if (line.Length == 0) line = words[w];
                else if (line.Length + words[w].Length + 1 <= CONTENT_W - 2) line = line + " " + words[w];
                else { wrappedLines[wrappedCount++] = line; line = words[w]; }
            }
            if (line.Length > 0 && wrappedCount < 200) wrappedLines[wrappedCount++] = line;
        }
    }

    // =================================================================
    // PERSISTENCE
    // =================================================================

    private void LoadReadProgress()
    {
        VRCPlayerApi p = Networking.LocalPlayer;
        if (!Utilities.IsValid(p) || !PlayerData.HasKey(p, "TT_Read")) return;
        string readList = PlayerData.GetString(p, "TT_Read");
        string[] ids = readList.Split(',');
        for (int i = 0; i < storyCount; i++)
            for (int j = 0; j < ids.Length; j++)
                if (titles[i] == ids[j]) storyRead[i] = true;
    }

    private void SaveReadProgress()
    {
        string list = "";
        for (int i = 0; i < storyCount; i++)
            if (storyRead[i]) list = list + (list.Length > 0 ? "," : "") + titles[i];
        PlayerData.SetString("TT_Read", list);
    }

    private void MarkRead(int idx) { if (!storyRead[idx]) { storyRead[idx] = true; SaveReadProgress(); } }
    private int CountRead() { int c = 0; for (int i = 0; i < storyCount; i++) if (storyRead[i]) c++; return c; }

    private void PushDisplay(string content)
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", content);
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    public string GetDisplayContent() { return "Terminal Tales"; }
}
