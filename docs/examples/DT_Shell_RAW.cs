/// <summary>
/// BASEMENT OS SHELL (v2.1)
///
/// ROLE: MAIN MENU / COMMAND SHELL
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_Shell.cs
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

    [Tooltip("Item types: <DIR>, <EXE>, <BAT>")]
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
    // THEME COLORS (from DT_Theme)
    // ============================================================

    private const string COLOR_PRIMARY = "#33FF33";
    private const string COLOR_ALERT = "#FFB000";
    private const string COLOR_DIM = "#808080";

    // ... (Remainder of file omitted for brevity, logic remains same) ...
}
