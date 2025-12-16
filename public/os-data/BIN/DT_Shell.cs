/// <summary>
/// BASEMENT OS SHELL (v2.0)
///
/// ROLE: MAIN MENU / COMMAND SHELL
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_Shell.cs
///
/// INTEGRATION:
/// - Hub: Receives input from DT_Core
/// - Spoke: Launches other apps via Core.LoadProcess()
///
/// FEATURES:
/// - DOS DIR-style navigation interface
/// - Cursor-based menu selection
/// - Dynamic content generation with item types
/// - Keyboard navigation (UP/DOWN/ACCEPT)
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

    private const string COLOR_STRUCT = "#064E3B";     // Borders, inactive
    private const string COLOR_PRIMARY = "#10B981";    // Body text, menu names
    private const string COLOR_HIGHLIGHT = "#34D399";  // Active selections
    private const string COLOR_DIM = "#0A5240";        // Subtle elements

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
    /// Generate display content for lines 4-20
    /// Returns DOS DIR-style menu listing
    /// </summary>
    public string GetDisplayContent()
    {
        string output = "";

        // Header: Current directory
        output = output + " C:\\BASEMENT\\MENU\n";
        output = output + "\n";

        // Column headers
        output = output + "   TYPE     NAME             DESCRIPTION\n";
        output = output + "   ----     ------------     ---------------------------------------\n";

        // Menu items
        if (menuItemCount > 0)
        {
            for (int i = 0; i < menuItemCount; i++)
            {
                output = output + RenderMenuItem(i);
            }
        }
        else
        {
            output = output + "   <color=" + COLOR_DIM + ">No menu items configured</color>\n";
        }

        return output;
    }

    /// <summary>
    /// Render a single menu item with cursor and formatting
    /// </summary>
    private string RenderMenuItem(int index)
    {
        string line = "";

        // Cursor indicator
        if (index == cursorIndex)
        {
            line = line + " <color=" + COLOR_HIGHLIGHT + ">";
            line = line + ">";
        }
        else
        {
            line = line + "  ";
        }

        // Item type (padded to 8 chars)
        string itemType = GetItemType(index);
        line = line + " " + PadRight(itemType, 8);

        // Item name (padded to 16 chars)
        string itemName = GetItemName(index);
        line = line + " " + PadRight(itemName, 16);

        // Description
        string itemDesc = GetItemDescription(index);
        line = line + " " + itemDesc;

        // Close color tag if cursor line
        if (index == cursorIndex)
        {
            line = line + "</color>";
        }

        line = line + "\n";

        return line;
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
    /// Pad string to specified width (UdonSharp-compatible)
    /// </summary>
    private string PadRight(string text, int totalWidth)
    {
        if (text == null) text = "";

        int currentLength = text.Length;

        if (currentLength >= totalWidth)
        {
            return text;
        }

        string padded = text;
        int spacesNeeded = totalWidth - currentLength;

        for (int i = 0; i < spacesNeeded; i++)
        {
            padded = padded + " ";
        }

        return padded;
    }
}
