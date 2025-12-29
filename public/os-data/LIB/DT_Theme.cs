using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UdonSharp;

/// <summary>
/// BASEMENT OS THEME LIBRARY (v2.0)
///
/// ROLE: COLOR PALETTE & TEXT STYLING UTILITIES
/// Provides TextMeshPro rich text wrappers for consistent terminal theming.
/// All methods are static helpers - no instantiation required.
///
/// LOCATION: Assets/Scripts/BasementOS/LIB/DT_Theme.cs
///
/// INTEGRATION:
/// - Used by: DT_Shell, DT_Core, all /BIN/ apps
/// - Enforces: Web terminal emerald/teal aesthetic
///
/// LIMITATIONS:
/// - Static utility class (no UdonBehaviour methods needed)
/// - All colors use TextMeshPro hex format (#RRGGBB)
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Theme : UdonSharpBehaviour
{
    // =================================================================
    // COLOR PALETTE (Web Terminal - Emerald/Teal)
    // =================================================================

    /// <summary>Structure color - borders, box drawing, inactive elements</summary>
    public const string COLOR_STRUCT = "#10B981";

    /// <summary>Primary emerald green - main body text, menu names</summary>
    public const string COLOR_PRIMARY = "#10B981";

    /// <summary>Highlight mint - active selections, headers, important values</summary>
    public const string COLOR_HIGHLIGHT = "#10B981";

    /// <summary>Dim dark teal - help text, subtle elements</summary>
    public const string COLOR_DIM = "#10B981";

    /// <summary>Error coral red - warnings, errors, glitch effects</summary>
    public const string COLOR_ERROR = "#F87171";

    /// <summary>Amber alert - important notifications (legacy compatibility)</summary>
    public const string COLOR_ALERT = "#FFB000";

    /// <summary>White - high contrast text</summary>
    public const string COLOR_WHITE = "#FFFFFF";

    /// <summary>Muted gray - comments, metadata</summary>
    public const string COLOR_MUTED = "#666666";

    // =================================================================
    // TEXT STYLING HELPERS
    // =================================================================

    /// <summary>
    /// Wraps text in custom color tag
    /// </summary>
    /// <param name="text">Text to colorize</param>
    /// <param name="hexColor">Hex color code (e.g., "#10B981")</param>
    /// <returns>TextMeshPro formatted string</returns>
    public static string Colorize(string text, string hexColor)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return "<color=" + hexColor + ">" + text + "</color>";
    }

    /// <summary>
    /// Primary emerald green text (main terminal color)
    /// </summary>
    public static string Primary(string text)
    {
        return Colorize(text, COLOR_PRIMARY);
    }

    /// <summary>
    /// Structure color text (borders, inactive elements)
    /// </summary>
    public static string Struct(string text)
    {
        return Colorize(text, COLOR_STRUCT);
    }

    /// <summary>
    /// Highlight mint text (active selections, headers)
    /// </summary>
    public static string Highlight(string text)
    {
        return Colorize(text, COLOR_HIGHLIGHT);
    }

    /// <summary>
    /// Dim dark teal text (secondary information, help text)
    /// </summary>
    public static string Dim(string text)
    {
        return Colorize(text, COLOR_DIM);
    }

    /// <summary>
    /// Amber alert text (warnings, important notices)
    /// </summary>
    public static string Alert(string text)
    {
        return Colorize(text, COLOR_ALERT);
    }

    /// <summary>
    /// Coral red error text (critical failures, glitch effects)
    /// </summary>
    public static string Error(string text)
    {
        return Colorize(text, COLOR_ERROR);
    }

    /// <summary>
    /// White text (high contrast headers)
    /// </summary>
    public static string White(string text)
    {
        return Colorize(text, COLOR_WHITE);
    }

    /// <summary>
    /// Gray muted text (comments, metadata)
    /// </summary>
    public static string Muted(string text)
    {
        return Colorize(text, COLOR_MUTED);
    }

    /// <summary>
    /// Bold text (emphasis)
    /// </summary>
    public static string Bold(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return "<b>" + text + "</b>";
    }

    // =================================================================
    // COMPOUND STYLES (Common Patterns)
    // =================================================================

    /// <summary>
    /// Bold primary green (emphasized headers)
    /// </summary>
    public static string BoldPrimary(string text)
    {
        return Bold(Primary(text));
    }

    /// <summary>
    /// Bold alert amber (important warnings)
    /// </summary>
    public static string BoldAlert(string text)
    {
        return Bold(Alert(text));
    }

    /// <summary>
    /// Bold error red (critical failures)
    /// </summary>
    public static string BoldError(string text)
    {
        return Bold(Error(text));
    }
}
