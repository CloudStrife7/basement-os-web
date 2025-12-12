# Porting Guide: Udon to Basement OS Web Emulator

This guide documents the process of porting a Unity/Udon C# script to the JavaScript-based Web Terminal Emulator (`basement-os.js`). It uses the **DT_Shell** port as a case study.

## Core Concept
The Web Terminal is an **emulator**. It mimics the behavior of the original C# code but runs entirely in the browser using JavaScript.
The porting process involves three main steps:
1.  **Data**: Making the C# source data available to the web app.
2.  **Logic**: Re-writing the C# logic (rendering and input) into JavaScript.
3.  **Integration**: Hooking the new "app" into the main OS loop.

---

## Case Study: DT_Shell

The Shell is the main menu of the OS. In Unity, it displays a list of apps defined in the Inspector.

### Step 1: Copying and Patching the Source
The original C# file (`DT_Shell.cs`) defines variables like `menuNames` but doesn't initialize them because the data is stored in Unity's metadata (serialized assets). For the web version to parse this data, we must "patch" the file with hardcoded values.

**Action**:
1.  Copied `DT_Shell.cs` from Unity Project to `public/os-data/DT_Shell.cs`.
2.  Modified the file to include initializer lists for the arrays.

**Diff:**
```csharp
// ORIGINAL (Unity)
public string[] menuNames;

// PATCHED (Web)
public string[] menuNames = new string[] { 
    "GITHUB", 
    "DASHBOARD", 
    "GAMES", 
    // ... 
};
```

### Step 2: Implementing the JavaScript Logic
In `public/js/basement-os.js`, we need to teach the OS how to read this file and how to display it.

#### A. The Parser (`loadShellData`)
We wrote a regex-based parser to extract the array data from the text of the C# file.

```javascript
async loadShellData() {
    // 1. Fetch the text content of the C# file
    const text = await this.fetchSource('DT_Shell.cs');
    
    // 2. Extract arrays using Regex
    // Looks for: public string[] menuNames = { ... };
    this.menuItems = this.extractArray(text, 'menuNames');
    // ... extract other arrays
}
```

#### B. The Renderer (`renderShell`)
We ported the `GetDisplayContent()` method from C# to JavaScript. This method builds the text string that appears on screen.

*   **C#**: Uses `string output = output + "..."` loops.
*   **JS**: Uses template literals and `buffer += ...` loops.
*   **Logic**: Iterates through `this.menuItems`, checks `this.menuCursor` to draw the `>` selection marker.

#### C. Input Handling (`handleShellInput`)
We ported the `OnInput()` method.

*   **C#**: Checks `inputKey == "UP"`.
*   **JS**: Checks `e.key === 'ArrowUp'` inside an event listener.
*   **Logic**: Increments/decrements `menuCursor` and handles wrapping.

---

## How to Port a New App (e.g., DASHBOARD)

To port a new app, follow this checklist:

### 1. Prepare Data
1.  Find the source file (e.g., `DT_App_Dashboard.cs`).
2.  Copy it to `public/os-data/`.
3.  **Check for Inspector Data**: Does the class relies on `[SerializeField]` variables?
    *   **Yes**: Open the file and manually add the default values (strings, numbers) that match what you see in the Unity Inspector.
    *   **No**: Proceed.

### 2. Implement Logic in `basement-os.js`
Open `public/js/basement-os.js` and add the following:

#### A. State
Add variables to the `constructor` to track the app's state.
```javascript
// Example for Dashboard
this.dashboardStats = []; // To hold data
```

#### B. Data Loader
Add a method `async loadDashboardData()`:
1.  Fetch `DT_App_Dashboard.cs`.
2.  Use `this.extractField()` or `this.extractArray()` to parse values from the C# text.
3.  Store them in your state variables.
4.  Call this method in `init()`.

#### C. Input Handler
Add `handleDashboardInput(e)`:
*   Map `ArrowUp`, `ArrowDown`, `Enter`, `Escape` to the app's logic.
*   **Important**: `Escape` should always set `this.currentApp = 'SHELL'`.

#### D. Renderer
Add `renderDashboard()`:
*   Recreate the layout string (headers, lines, data).
*   Use the state variables you parsed.

### 3. Register the App
1.  Update `handleShellInput`: When User presses Enter on "DASHBOARD", set `this.currentApp = 'DASHBOARD'`.
2.  Update `render`: Add `else if (this.currentApp === 'DASHBOARD') buffer += this.renderDashboard();`.
3.  Update `attachInput`: Add dispatch logic to call `handleDashboardInput(e)`.

---

## Tips
*   **Regex Parsing**: The helper `extractField(text, varName)` looks for `string var = "value";`. If the C# code is complex, you might need to write a custom regex or simpler mock data patch.
*   **State Reset**: Remember to reset cursor positions or scroll offsets when opening an app (similar to `OnAppOpen` in Unity).
