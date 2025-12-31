/// <summary>
/// BASEMENT OS SHELL (v2.1)
///
/// ROLE: MAIN MENU / COMMAND SHELL
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_Shell.cs
///
/// INTEGRATION:
/// - Hub: Receives input from DT_Core
/// - Spoke: Launches other apps via Core.LoadProcess()
///
/// FEATURES:
/// - DOS DIR-style navigation interface with box borders
/// - Cursor-based menu selection
/// - Dynamic content generation with item types
/// - Keyboard navigation (UP/DOWN/ACCEPT)
/// - Unified terminal style (Terminal_Style_Guide.md)
/// </summary>

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Shell : UdonSharpBehaviour
{
    // ============================================================
    // APP INTERFACE (Required by DT_Core)
    // ============================================================

    [Header("App Interface")]
    [Tooltip("Input key set by DT_Core before OnInput()")]
    public string inputKey = "";

    // ============================================================
    // MENU CONFIGURATION
    // ============================================================

    [Header("Menu Items")]
    [Tooltip("App references for each menu item")]
    public UdonSharpBehaviour[] menuTargets;

    [Tooltip("Display names for menu items")]
    public string[] menuNames;

    [Tooltip("Descriptions for menu items")]
    public string[] menuDescriptions;

    [Tooltip("Item types: <APP>, <CFG>, <FX>, <BAR>, <DAT>, <DOC>")]
    public string[] menuTypes;

    // ============================================================
    // CORE REFERENCE
    // ============================================================

    [Header("Hub Reference")]
    [Tooltip("Reference to DT_Core for LoadProcess calls")]
    public UdonSharpBehaviour coreReference;

    // ============================================================
    // STATE
    // ============================================================

    private int cursorIndex = 0;
    private int menuItemCount = 0;

    // ============================================================
    // THEME COLORS (from DT_Theme - Web Terminal Palette)
    // ============================================================

    private const string COLOR_BORDER = "#065F46";     // Dark emerald for box borders
    private const string COLOR_PRIMARY = "#10B981";    // Bright emerald for text
    private const string COLOR_HIGHLIGHT = "#34D399";  // Light emerald for selected
    private const string COLOR_DIM = "#6EE7B7";        // Pale emerald for headers

    // ============================================================
    // BOX DIMENSIONS (Terminal_Style_Guide.md)
    // ============================================================

    private const int WIDTH = 80;
    private const int CONTENT_W = 76;  // 80 - 4 for "║ " and " ║"
    private const int VISIBLE_ROWS = 12;  // Rows available for menu items

    // ============================================================
    // INITIALIZATION
    // ============================================================

    void Start()
    {
        // Validate menu configuration
        if (menuNames != null)
        {
            menuItemCount = menuNames.Length;
        }

        // Ensure cursor starts at valid position
        if (menuItemCount > 0)
        {
            cursorIndex = 0;
        }
    }

    // ============================================================
    // APP LIFECYCLE (Called by DT_Core)
    // ============================================================

    /// <summary>
    /// Called when app is activated by DT_Core
    /// </summary>
    public void OnAppOpen()
    {
        Debug.Log("[DT_Shell] ===== OnAppOpen CALLED =====");
        
        // Initialize menu count (in case OnAppOpen runs before Start due to script order)
        if (menuNames != null)
        {
            menuItemCount = menuNames.Length;
            Debug.Log("[DT_Shell] menuItemCount = " + menuItemCount);
        }
        else
        {
            Debug.LogWarning("[DT_Shell] menuNames is NULL!");
        }
        
        // NOTE: cursorIndex is NOT reset here - cursor persists when returning from apps
        
        // Push display content to core
        Debug.Log("[DT_Shell] Calling PushDisplayToCore()...");
        PushDisplayToCore();
        Debug.Log("[DT_Shell] OnAppOpen complete");
    }

    /// <summary>
    /// Called when app is deactivated by DT_Core
    /// </summary>
    public void OnAppClose()
    {
        // No cleanup needed - stateless menu
    }

    // ============================================================
    // INPUT HANDLING (Called by DT_Core)
    // ============================================================

    /// <summary>
    /// Handle input from DT_Core
    /// inputKey is set before this method is called
    /// </summary>
    public void OnInput()
    {
        Debug.Log("[DT_Shell] ===== OnInput CALLED =====");
        Debug.Log("[DT_Shell] inputKey = '" + inputKey + "'");
        Debug.Log("[DT_Shell] menuItemCount = " + menuItemCount);
        Debug.Log("[DT_Shell] cursorIndex = " + cursorIndex);
        
        if (menuItemCount == 0) 
        {
            Debug.LogWarning("[DT_Shell] menuItemCount is 0 - returning early");
            return;
        }

        // Navigate up
        if (inputKey == "UP")
        {
            Debug.Log("[DT_Shell] Processing UP navigation");
            cursorIndex--;
            if (cursorIndex < 0)
            {
                cursorIndex = menuItemCount - 1; // Wrap to bottom
            }
            Debug.Log("[DT_Shell] New cursorIndex = " + cursorIndex);
            PushDisplayToCore();
        }

        // Navigate down
        else if (inputKey == "DOWN")
        {
            Debug.Log("[DT_Shell] Processing DOWN navigation");
            cursorIndex++;
            if (cursorIndex >= menuItemCount)
            {
                cursorIndex = 0; // Wrap to top
            }
            Debug.Log("[DT_Shell] New cursorIndex = " + cursorIndex);
            PushDisplayToCore();
        }

        // Select current item (E/Click or D key)
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT")
        {
            Debug.Log("[DT_Shell] Processing ACCEPT/RIGHT - launching selected item");
            LaunchSelectedItem();
        }

        // Go back (A key) - for future submenu navigation
        else if (inputKey == "LEFT")
        {
            Debug.Log("[DT_Shell] Processing LEFT - back navigation (TODO)");
            // TODO: Implement back navigation when submenus are added
        }
        else
        {
            Debug.LogWarning("[DT_Shell] Unknown inputKey: '" + inputKey + "'");
        }
    }

    // ============================================================
    // MENU ACTIONS
    // ============================================================

    /// <summary>
    /// Launch the currently selected menu item
    /// </summary>
    private void LaunchSelectedItem()
    {
        // Validate index
        if (cursorIndex < 0 || cursorIndex >= menuItemCount)
        {
            Debug.LogWarning("[DT_Shell] Invalid cursor index: " + cursorIndex);
            return;
        }

        // Validate core reference
        if (!Utilities.IsValid(coreReference))
        {
            Debug.LogError("[DT_Shell] Core reference not set");
            return;
        }

        // Validate target exists
        if (menuTargets == null || cursorIndex >= menuTargets.Length)
        {
            Debug.LogWarning("[DT_Shell] No target for index: " + cursorIndex);
            return;
        }

        UdonSharpBehaviour targetApp = menuTargets[cursorIndex];

        if (!Utilities.IsValid(targetApp))
        {
            Debug.LogWarning("[DT_Shell] Invalid target at index: " + cursorIndex);
            return;
        }

        // Tell Core to load the selected app
        coreReference.SetProgramVariable("nextProcess", targetApp);
        coreReference.SendCustomEvent("LoadNextProcess");
    }

    // ============================================================
    // DISPLAY RENDERING (Called by DT_Core)
    // ============================================================

    /// <summary>
    /// Helper to wrap content in box row borders.
    /// IMPORTANT: Content must already be exactly CONTENT_W (76) visible chars!
    /// Color tags in content don't count toward visible width.
    /// </summary>
    private string BoxRow(string content)
    {
        // Don't pad here - content should already be correct width
        // This avoids truncating color tags
        return "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> " +
               content +
               " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
    }

    /// <summary>
    /// Generate display content for lines 4-20
    /// Returns DOS DIR-style menu listing with box borders
    /// </summary>
    public string GetDisplayContent()
    {
        string o = "";

        // Top border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxTop(WIDTH) + "</color>\n";

        // Title row (centered)
        o = o + BoxRow("<color=" + COLOR_DIM + ">" + DT_Format.PadCenter("BASEMENT OS - MAIN MENU", CONTENT_W) + "</color>") + "\n";

        // Divider
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "</color>\n";

        // Column headers (same layout as menu items: 2+8+1+16+1+48=76)
        string headerLine = "  " + PadRight("TYPE", 8) + " " + PadRight("NAME", 16) + " " + PadRight("DESCRIPTION", 48);
        o = o + BoxRow("<color=" + COLOR_DIM + ">" + headerLine + "</color>") + "\n";

        // Menu items
        int rowsUsed = 0;
        if (menuItemCount > 0)
        {
            for (int i = 0; i < menuItemCount && rowsUsed < VISIBLE_ROWS; i++)
            {
                o = o + RenderMenuItem(i) + "\n";
                rowsUsed++;
            }
        }
        else
        {
            // Pad message to exactly CONTENT_W chars
            o = o + BoxRow("<color=" + COLOR_DIM + ">" + DT_Format.PadLeft("  No menu items configured", CONTENT_W) + "</color>") + "\n";
            rowsUsed++;
        }

        // Pad remaining rows to fill box (empty row = 76 spaces)
        string emptyRow = DT_Format.RepeatChar(' ', CONTENT_W);
        for (int i = rowsUsed; i < VISIBLE_ROWS; i++)
        {
            o = o + BoxRow(emptyRow) + "\n";
        }

        // Bottom border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxBottom(WIDTH) + "</color>\n";

        // Navigation footer (outside box)
        o = o + " <color=" + COLOR_PRIMARY + ">[D] Launch  [W/S] Navigate</color>\n";

        return o;
    }

    /// <summary>
    /// Render a single menu item with cursor and formatting inside box.
    /// Layout: cursor(2) + type(8) + space(1) + name(16) + space(1) + desc(48) = 76 chars
    /// </summary>
    private string RenderMenuItem(int index)
    {
        // Build plain text content first, then wrap in color at the end
        // This ensures BoxRow receives exactly 76 visible chars

        // Cursor indicator (2 chars: "> " or "  ")
        string cursor = (index == cursorIndex) ? "> " : "  ";

        // Item type (8 chars) + space (1 char)
        string itemType = PadRight(GetItemType(index), 8);

        // Item name (16 chars) + space (1 char)
        string itemName = PadRight(GetItemName(index), 16);

        // Description (48 chars - truncate if too long, PAD if too short)
        string itemDesc = GetItemDescription(index);
        if (itemDesc.Length > 48)
        {
            itemDesc = itemDesc.Substring(0, 45) + "...";
        }
        itemDesc = PadRight(itemDesc, 48);  // Ensure exactly 48 chars

        // Assemble plain text: 2 + 8 + 1 + 16 + 1 + 48 = 76 chars exactly
        string plainContent = cursor + itemType + " " + itemName + " " + itemDesc;

        // Wrap entire line in color (color tags don't affect visible width)
        string color = (index == cursorIndex) ? COLOR_HIGHLIGHT : COLOR_PRIMARY;
        string coloredContent = "<color=" + color + ">" + plainContent + "</color>";

        return BoxRow(coloredContent);
    }

    // ============================================================
    // CORE COMMUNICATION
    // ============================================================

    /// <summary>
    /// Push current display content to DT_Core
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", GetDisplayContent());
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================

    /// <summary>
    /// Get item type with fallback
    /// </summary>
    private string GetItemType(int index)
    {
        if (menuTypes != null && index < menuTypes.Length)
        {
            return menuTypes[index];
        }
        return "<APP>";
    }

    /// <summary>
    /// Get item name with fallback
    /// </summary>
    private string GetItemName(int index)
    {
        if (menuNames != null && index < menuNames.Length)
        {
            return menuNames[index];
        }
        return "UNKNOWN";
    }

    /// <summary>
    /// Get item description with fallback
    /// </summary>
    private string GetItemDescription(int index)
    {
        if (menuDescriptions != null && index < menuDescriptions.Length)
        {
            return menuDescriptions[index];
        }
        return "";
    }

    /// <summary>
    /// Pad string to specified width, truncating if too long (UdonSharp-compatible)
    /// </summary>
    private string PadRight(string text, int totalWidth)
    {
        if (text == null) text = "";

        int currentLength = text.Length;

        // Truncate if too long
        if (currentLength >= totalWidth)
        {
            return text.Substring(0, totalWidth);
        }

        // Pad if too short
        string padded = text;
        int spacesNeeded = totalWidth - currentLength;

        for (int i = 0; i < spacesNeeded; i++)
        {
            padded = padded + " ";
        }

        return padded;
    }
}
