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
    public string[] menuNames = new string[] { "DASHBOARD", "STATS", "WEATHER", "GITHUB", "GAMES", "TALES" };

    [Tooltip("Descriptions for menu items")]
    public string[] menuDescriptions = new string[] { "System overview and player stats", "Personal statistics & achievements", "Weather forecast for Fond du Lac, WI", "Dev logs & project tracking", "Arcade game library", "Interactive fiction engine" };

    [Tooltip("Item types: <DIR>, <EXE>, <BAT>")]
    public string[] menuTypes = new string[] { "<APP>", "<DAT>", "<NET>", "<EXE>", "<DIR>", "<DIR>" };

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
    // THEME COLORS (from DT_Theme)
    // ============================================================

    private const string COLOR_PRIMARY = "#33FF33";
    private const string COLOR_ALERT = "#FFB000";
    private const string COLOR_DIM = "#808080";

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
        // Reset cursor to top of menu
        cursorIndex = 0;
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
        if (menuItemCount == 0) return;

        // Navigate up
        if (inputKey == "UP")
        {
            cursorIndex--;
            if (cursorIndex < 0)
            {
                cursorIndex = menuItemCount - 1; // Wrap to bottom
            }
        }

        // Navigate down
        else if (inputKey == "DOWN")
        {
            cursorIndex++;
            if (cursorIndex >= menuItemCount)
            {
                cursorIndex = 0; // Wrap to top
            }
        }

        // Select current item
        else if (inputKey == "ACCEPT")
        {
            LaunchSelectedItem();
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
        coreReference.SendCustomEvent("LoadProcess");
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
            line = line + " <color=" + COLOR_ALERT + ">";
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
        return "<DIR>";
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
