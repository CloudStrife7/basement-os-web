# Sync & Connection Workflow

## Overview
This document explains the connection between the **Lower Level 2.0 (Unity Project)** and the **Basement OS Web Terminal**.

**Current Status:** [MANUAL SYNC ONLY]
There is **no automated connection** (via GitHub Actions or otherwise) that syncs changes from the Unity project to this website.

---

## How the Connection Works
The Web Terminal does not connect to the live Unity project or a database. Instead, it relies on static "snapshots" of the code:

1.  **Unity Project**: The source of truth for the logic (`.cs` files).
2.  **Manual Export**: A developer (or agent) manually copies the file.
3.  **Web Project**: The file is saved in `public/os-data/`.
4.  **Runtime**: The browser fetches this local static file via `fetch('/os-data/file.cs')`.

## The Manual Update Workflow
When the underlying OS code changes in the game (Unity), follow these steps to update the website:

### 1. Locate the Source
Find the updated C# script in the Unity repo:
`Assets/Scripts/BasementOS/BIN/`

### 2. Copy the File
Copy the `.cs` file to the website directory:
`basement-os-web/public/os-data/`

### 3. CRITICAL: The "Inspector Data" Patch
Unity scripts often use empty variables populated by the Inspector (e.g., `public string[] menuNames;`). The raw C# file does not contain this data.
*   **Problem**: If you just copy the file, the Web Terminal will see an empty array and fail or show defaults.
*   **Fix**: You must manually edit the C# file after copying it to "bake in" the data.

**Example (DT_Shell.cs):**

**BEFORE: Raw File from Unity**
*(The arrays are declared but empty. The data is hidden in the .asset file)*
```csharp
    [Tooltip("Display names for menu items")]
    public string[] menuNames;

    [Tooltip("Descriptions for menu items")]
    public string[] menuDescriptions;

    [Tooltip("Item types: <DIR>, <EXE>, <BAT>")]
    public string[] menuTypes;
```

**AFTER: Patched File for Web**
*(We manually initialize the arrays with the data found in the Unity Inspector)*
```csharp
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
```

### 4. Verify
Run `npm run dev` and test the terminal to ensure the new values are parsed correctly.

---

## Future Automation
To automate this, a custom pipeline would be needed:
1.  **Unity-Side Script**: A script that runs at build time to export the ScriptableObject/Inspector data into a JSON file or injects it into the C# source.
2.  **GitHub Action**: A workflow that triggers on Unity commits, runs the export, and commits the result to the Web repo.
