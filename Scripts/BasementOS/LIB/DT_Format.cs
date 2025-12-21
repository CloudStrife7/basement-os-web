using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UdonSharp;

/// <summary>
/// BASEMENT OS FORMAT LIBRARY (v2.1)
///
/// ROLE: TERMINAL LAYOUT & FORMATTING UTILITIES
/// Provides box drawing, alignment, padding, and progress bars for 80-column
/// DOS terminal displays. All methods are static helpers.
///
/// LOCATION: Assets/Scripts/BasementOS/LIB/DT_Format.cs
///
/// INTEGRATION:
/// - Used by: DT_Shell, DT_Core, all /BIN/ apps
/// - Enforces: 80x24 terminal grid, DOS box-drawing characters
///
/// LIMITATIONS:
/// - Static utility class (no UdonBehaviour methods needed)
/// - All layouts assume 80-character width
/// - Uses Unicode box-drawing characters (requires proper font)
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Format : UdonSharpBehaviour
{
    // =================================================================
    // TERMINAL DIMENSIONS
    // =================================================================

    /// <summary>Standard DOS terminal width</summary>
    public const int SCREEN_WIDTH = 80;

    /// <summary>Content area height (excluding header/footer)</summary>
    public const int CONTENT_HEIGHT = 16;

    // =================================================================
    // BOX DRAWING CHARACTERS (Double Lines)
    // =================================================================

    public const char BORDER_HORIZONTAL = '═';
    public const char BORDER_VERTICAL = '║';
    public const char BORDER_TOP_LEFT = '╔';
    public const char BORDER_TOP_RIGHT = '╗';
    public const char BORDER_BOTTOM_LEFT = '╚';
    public const char BORDER_BOTTOM_RIGHT = '╝';
    public const char BORDER_LEFT_T = '╠';
    public const char BORDER_RIGHT_T = '╣';
    public const char BORDER_TOP_T = '╦';
    public const char BORDER_BOTTOM_T = '╩';
    public const char BORDER_CROSS = '╬';

    // =================================================================
    // BOX DRAWING CHARACTERS (Single Lines)
    // =================================================================

    public const char SINGLE_HORIZONTAL = '─';
    public const char SINGLE_VERTICAL = '│';
    public const char SINGLE_TOP_LEFT = '┌';
    public const char SINGLE_TOP_RIGHT = '┐';
    public const char SINGLE_BOTTOM_LEFT = '└';
    public const char SINGLE_BOTTOM_RIGHT = '┘';

    // =================================================================
    // PROGRESS BAR CHARACTERS
    // =================================================================

    public const char BLOCK_FULL = '█';
    public const char BLOCK_DARK = '▓';
    public const char BLOCK_MEDIUM = '▒';
    public const char BLOCK_LIGHT = '░';

    // =================================================================
    // BORDER GENERATION
    // =================================================================

    /// <summary>
    /// Generates double-line horizontal border
    /// </summary>
    /// <param name="width">Width in characters</param>
    /// <returns>String of ═ characters</returns>
    public static string GenerateBorder(int width)
    {
        if (width <= 0) return "";
        return RepeatChar(BORDER_HORIZONTAL, width);
    }

    /// <summary>
    /// Generates single-line horizontal border
    /// </summary>
    /// <param name="width">Width in characters</param>
    /// <returns>String of ─ characters</returns>
    public static string GenerateSingleBorder(int width)
    {
        if (width <= 0) return "";
        return RepeatChar(SINGLE_HORIZONTAL, width);
    }

    /// <summary>
    /// Generates full box top border
    /// </summary>
    /// <param name="width">Total width including corners</param>
    /// <returns>╔═══...═══╗</returns>
    public static string GenerateBoxTop(int width)
    {
        if (width <= 2) return "";
        return BORDER_TOP_LEFT + RepeatChar(BORDER_HORIZONTAL, width - 2) + BORDER_TOP_RIGHT;
    }

    /// <summary>
    /// Generates full box bottom border
    /// </summary>
    /// <param name="width">Total width including corners</param>
    /// <returns>╚═══...═══╝</returns>
    public static string GenerateBoxBottom(int width)
    {
        if (width <= 2) return "";
        return BORDER_BOTTOM_LEFT + RepeatChar(BORDER_HORIZONTAL, width - 2) + BORDER_BOTTOM_RIGHT;
    }

    /// <summary>
    /// Generates box row with vertical borders
    /// </summary>
    /// <param name="content">Content text (will be padded/truncated)</param>
    /// <param name="width">Total width including borders</param>
    /// <returns>║ content... ║</returns>
    public static string GenerateBoxRow(string content, int width)
    {
        if (width <= 2) return "";
        string padded = PadLeft(content, width - 4); // Account for "║ " and " ║"
        if (padded.Length > width - 4) padded = Truncate(padded, width - 4);
        return BORDER_VERTICAL + " " + padded + " " + BORDER_VERTICAL;
    }

    // =================================================================
    // TEXT ALIGNMENT & PADDING
    // =================================================================

    /// <summary>
    /// Centers text within specified width
    /// </summary>
    /// <param name="text">Text to center</param>
    /// <param name="width">Total width</param>
    /// <returns>Centered string with padding</returns>
    public static string PadCenter(string text, int width)
    {
        if (string.IsNullOrEmpty(text)) return RepeatChar(' ', width);
        if (text.Length >= width) return text.Substring(0, width);

        int totalPadding = width - text.Length;
        int leftPadding = totalPadding / 2;
        int rightPadding = totalPadding - leftPadding;

        return RepeatChar(' ', leftPadding) + text + RepeatChar(' ', rightPadding);
    }

    /// <summary>
    /// Left-aligns text with right padding
    /// </summary>
    /// <param name="text">Text to align</param>
    /// <param name="width">Total width</param>
    /// <returns>Left-aligned string</returns>
    public static string PadLeft(string text, int width)
    {
        if (string.IsNullOrEmpty(text)) return RepeatChar(' ', width);
        if (text.Length >= width) return text.Substring(0, width);

        return text + RepeatChar(' ', width - text.Length);
    }

    /// <summary>
    /// Right-aligns text with left padding
    /// </summary>
    /// <param name="text">Text to align</param>
    /// <param name="width">Total width</param>
    /// <returns>Right-aligned string</returns>
    public static string PadRight(string text, int width)
    {
        if (string.IsNullOrEmpty(text)) return RepeatChar(' ', width);
        if (text.Length >= width) return text.Substring(0, width);

        return RepeatChar(' ', width - text.Length) + text;
    }

    // =================================================================
    // TEXT MANIPULATION
    // =================================================================

    /// <summary>
    /// Truncates text to max length, adding "..." if too long
    /// </summary>
    /// <param name="text">Text to truncate</param>
    /// <param name="maxLength">Maximum length</param>
    /// <returns>Truncated string with ellipsis if needed</returns>
    public static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (maxLength <= 3) return "...".Substring(0, maxLength);
        if (text.Length <= maxLength) return text;

        return text.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Repeats a character N times
    /// </summary>
    /// <param name="c">Character to repeat</param>
    /// <param name="count">Number of repetitions</param>
    /// <returns>String of repeated characters</returns>
    public static string RepeatChar(char c, int count)
    {
        if (count <= 0) return "";

        // UdonSharp-safe implementation (no string constructor with char/count)
        string result = "";
        for (int i = 0; i < count; i++)
        {
            result = result + c;
        }
        return result;
    }

    // =================================================================
    // PROGRESS BARS
    // =================================================================

    /// <summary>
    /// Generates ASCII progress bar with brackets
    /// </summary>
    /// <param name="percent">Progress 0.0 to 1.0</param>
    /// <param name="width">Total width including brackets</param>
    /// <returns>[████░░░░] style progress bar</returns>
    public static string GenerateProgressBar(float percent, int width)
    {
        if (width <= 2) return "";

        // Clamp percent
        if (percent < 0f) percent = 0f;
        if (percent > 1f) percent = 1f;

        int barWidth = width - 2; // Account for [ ]
        int filledWidth = (int)(barWidth * percent);
        int emptyWidth = barWidth - filledWidth;

        string filled = RepeatChar(BLOCK_FULL, filledWidth);
        string empty = RepeatChar(BLOCK_LIGHT, emptyWidth);

        return "[" + filled + empty + "]";
    }

    /// <summary>
    /// Generates progress bar with percentage label
    /// </summary>
    /// <param name="percent">Progress 0.0 to 1.0</param>
    /// <param name="width">Total width including brackets and label</param>
    /// <returns>[████░░░░] 50%</returns>
    public static string GenerateProgressBarWithLabel(float percent, int width)
    {
        if (width <= 7) return ""; // Minimum: [█] 0%

        // Format percentage (0-100)
        int percentInt = (int)(percent * 100f);
        string label = " " + percentInt.ToString() + "%";

        int barWidth = width - label.Length;
        string bar = GenerateProgressBar(percent, barWidth);

        return bar + label;
    }

    // =================================================================
    // COLUMN LAYOUTS
    // =================================================================

    /// <summary>
    /// Formats two-column layout with left and right aligned text
    /// </summary>
    /// <param name="left">Left-aligned text</param>
    /// <param name="right">Right-aligned text</param>
    /// <param name="totalWidth">Total width of row</param>
    /// <returns>Left-aligned text + padding + right-aligned text</returns>
    public static string FormatTwoColumn(string left, string right, int totalWidth)
    {
        if (string.IsNullOrEmpty(left)) left = "";
        if (string.IsNullOrEmpty(right)) right = "";

        int combinedLength = left.Length + right.Length;

        // If combined text exceeds width, truncate left text
        if (combinedLength >= totalWidth)
        {
            int maxLeftWidth = totalWidth - right.Length - 1; // -1 for safety
            if (maxLeftWidth < 0) maxLeftWidth = 0;
            left = Truncate(left, maxLeftWidth);
        }

        int paddingWidth = totalWidth - left.Length - right.Length;
        if (paddingWidth < 0) paddingWidth = 0;

        return left + RepeatChar(' ', paddingWidth) + right;
    }

    /// <summary>
    /// Formats three-column layout with even spacing
    /// </summary>
    /// <param name="left">Left text</param>
    /// <param name="center">Center text</param>
    /// <param name="right">Right text</param>
    /// <param name="totalWidth">Total width</param>
    /// <returns>Left | Center | Right</returns>
    public static string FormatThreeColumn(string left, string center, string right, int totalWidth)
    {
        if (string.IsNullOrEmpty(left)) left = "";
        if (string.IsNullOrEmpty(center)) center = "";
        if (string.IsNullOrEmpty(right)) right = "";

        int columnWidth = totalWidth / 3;

        string leftPart = PadLeft(Truncate(left, columnWidth), columnWidth);
        string centerPart = PadCenter(Truncate(center, columnWidth), columnWidth);
        string rightPart = PadRight(Truncate(right, columnWidth), columnWidth);

        string result = leftPart + centerPart + rightPart;

        // Ensure exact width
        if (result.Length > totalWidth)
            return result.Substring(0, totalWidth);
        else if (result.Length < totalWidth)
            return result + RepeatChar(' ', totalWidth - result.Length);

        return result;
    }

    // =================================================================
    // WIDTH ENFORCEMENT (80-Column Grid Compliance)
    // =================================================================

    /// <summary>
    /// Ensures a line is exactly SCREEN_WIDTH (80) characters.
    /// Truncates if too long, pads with spaces if too short.
    /// Handles TextMeshPro color tags correctly.
    /// </summary>
    /// <param name="line">Line to enforce</param>
    /// <returns>Exactly 80-character line</returns>
    public static string EnforceLine80(string line)
    {
        if (line == null) line = "";

        // Strip any existing color tags for width calculation
        string plainText = StripColorTags(line);

        // If line is too long, truncate
        if (plainText.Length > SCREEN_WIDTH)
        {
            line = line.Substring(0, SCREEN_WIDTH);
        }
        // If line is too short, pad with spaces
        else if (plainText.Length < SCREEN_WIDTH)
        {
            int spacesNeeded = SCREEN_WIDTH - plainText.Length;
            for (int i = 0; i < spacesNeeded; i++)
            {
                line = line + " ";
            }
        }

        return line;
    }

    /// <summary>
    /// Generates an 80-character separator line using double-line borders
    /// </summary>
    /// <returns>String of 80 ═ characters</returns>
    public static string GenerateSeparator80()
    {
        return new string(BORDER_HORIZONTAL, SCREEN_WIDTH);
    }

    /// <summary>
    /// Strips TextMeshPro color tags for accurate width measurement.
    /// Removes &lt;color=#XXXXXX&gt; and &lt;/color&gt; tags.
    /// </summary>
    /// <param name="text">Text with possible color tags</param>
    /// <returns>Plain text without color tags</returns>
    public static string StripColorTags(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        string result = text;

        // Remove <color=#XXXXXX> tags
        // UdonSharp doesn't support Regex, so use simple string operations
        while (result.Contains("<color="))
        {
            int startIdx = result.IndexOf("<color=");
            int endIdx = result.IndexOf(">", startIdx);
            if (endIdx > startIdx)
            {
                result = result.Substring(0, startIdx) +
                         result.Substring(endIdx + 1);
            }
            else break;
        }

        // Remove </color> tags
        while (result.Contains("</color>"))
        {
            int startIdx = result.IndexOf("</color>");
            result = result.Substring(0, startIdx) +
                     result.Substring(startIdx + 8);
        }

        // Remove <b> and </b> tags (if present)
        while (result.Contains("<b>"))
        {
            int startIdx = result.IndexOf("<b>");
            result = result.Substring(0, startIdx) +
                     result.Substring(startIdx + 3);
        }

        while (result.Contains("</b>"))
        {
            int startIdx = result.IndexOf("</b>");
            result = result.Substring(0, startIdx) +
                     result.Substring(startIdx + 4);
        }

        return result;
    }
}
