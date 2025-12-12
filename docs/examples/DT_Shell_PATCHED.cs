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
    public string[] menuNames = new string[] { 
        "GITHUB", 
        "DASHBOARD", 
        "GAMES", 
        "STATS", 
        "TALES", 
        "WEATHER" 
    };

    [Tooltip("Descriptions for menu items")]
    public string[] menuDescriptions = new string[] { 
        "Remote Repository Viewer", 
        "System Overview & Status", 
        "Arcade & Mini-games", 
        "World Traffic Analytics", 
        "Audio Log Archives", 
        "External Weather Uplink" 
    };

    [Tooltip("Item types: <DIR>, <EXE>, <BAT>")]
    public string[] menuTypes = new string[] { 
        "<EXE>", 
        "<APP>", 
        "<DIR>", 
        "<DAT>", 
        "<DIR>", 
        "<NET>" 
    };

    // ============================================================
    // CORE REFERENCE
    // ============================================================

    [Header("Hub Reference")]
    [Tooltip("Reference to DT_Core for LoadProcess calls")]
    public UdonSharpBehaviour coreReference;

    // ... (The rest of the file logic is identical) ...
}
