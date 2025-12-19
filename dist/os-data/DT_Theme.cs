using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UdonSharp;

/// <summary>
/// BASEMENT OS THEME LIBRARY (v2.1)
///
/// ROLE: COLOR PALETTE & TEXT STYLING UTILITIES
/// Provides TextMeshPro rich text wrappers for consistent terminal theming.
/// All methods are static helpers - no instantiation required.
///
/// LOCATION: Assets/Scripts/BasementOS/LIB/DT_Theme.cs
///
/// INTEGRATION:
/// - Used by: DT_Shell, DT_Core, all /BIN/ apps
/// - Enforces: 80s DOS phosphor green aesthetic
///
/// LIMITATIONS:
/// - Static utility class (no UdonBehaviour methods needed)
/// - All colors use TextMeshPro hex format (#RRGGBB)
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Theme : UdonSharpBehaviour
{
    // =================================================================
    // COLOR PALETTE (DOS Phosphor Terminal)
    // =================================================================

    /// <summary>Primary phosphor green - default text color</summary>
    public const string COLOR_PRIMARY = "#33FF33";

    /// <summary>Dimmed green - secondary text, disabled states</summary>
    public const string COLOR_DIM = "#008800";

    /// <summary>Amber warning - alerts, important notifications</summary>
    public const string COLOR_ALERT = "#FFB000";

    /// <summary>Red error - critical warnings, failures</summary>
    public const string COLOR_ERROR = "#FF3333";

    /// <summary>Cyan link - hyperlinks, selectable items</summary>
    public const string COLOR_LINK = "#00FFFF";

    /// <summary>White - high contrast text, headers</summary>
    public const string COLOR_WHITE = "#FFFFFF";

    /// <summary>Gray - muted text, comments</summary>
    public const string COLOR_MUTED = "#666666";

    // =================================================================
    // TEXT STYLING HELPERS
    // =================================================================

    /// <summary>
    /// Wraps text in custom color tag
    /// </summary>
    /// <param name="text">Text to colorize</param>
    /// <param name="hexColor">Hex color code (e.g., "#33FF33")</param>
    /// <returns>TextMeshPro formatted string</returns>
    public static string Colorize(string text, string hexColor)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return "<color=" + hexColor + ">" + text + "</color>";
    }

    /// <summary>
    /// Primary phosphor green text (default terminal color)
    /// </summary>
    public static string Primary(string text)
    {
        return Colorize(text, COLOR_PRIMARY);
    }

    /// <summary>
    /// Dimmed green text (secondary information)
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
    /// Red error text (critical failures)
    /// </summary>
    public static string Error(string text)
    {
        return Colorize(text, COLOR_ERROR);
    }

    /// <summary>
    /// Cyan link text (selectable items, hyperlinks)
    /// </summary>
    public static string Link(string text)
    {
        return Colorize(text, COLOR_LINK);
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
