using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

/// <summary>
/// BASEMENT OS ARCADE GAME LAUNCHER (v2.2)
///
/// ROLE: ARCADE CARTRIDGE SELECTION INTERFACE
/// "Select a Cartridge" - Browse and launch arcade games from the terminal.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Games.cs
///
/// INTEGRATION:
/// - Spoke: Receives input from DT_Core
/// - Games: SymbolicPong, SymbolicSnake, future titles
/// - Uses: Terminal_Style_Guide.md box format
///
/// LIMITATIONS:
/// - Max 250 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Games : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // GAME REFERENCES
    // =================================================================

    [Header("--- Game Cartridges ---")]
    [Tooltip("Game GameObjects")]
    [SerializeField] private GameObject[] gameObjects;

    [Tooltip("Display names for each game")]
    [SerializeField] private string[] gameNames;

    [Tooltip("One-line descriptions")]
    [SerializeField] private string[] gameDescriptions;

    [Tooltip("Which games are playable")]
    [SerializeField] private bool[] gameEnabled;

    [Header("--- Camera Management ---")]
    [Tooltip("Terminal camera to disable")]
    [SerializeField] private Camera terminalCamera;

    [Tooltip("Game cameras to enable")]
    [SerializeField] private Camera[] gameCameras;

    [Header("--- System References ---")]
    [Tooltip("Reference to DT_Core")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Tooltip("Shell app to return to on LEFT key")]
    [SerializeField] private UdonSharpBehaviour shellApp;

    [Tooltip("High scores")]
    [SerializeField] private int[] highScores;

    // =================================================================
    // BOX DIMENSIONS & COLORS (Terminal_Style_Guide.md)
    // =================================================================

    private const int WIDTH = 80;
    private const int CONTENT_W = 76;
    private const int VISIBLE_ROWS = 12;

    private const string COLOR_BORDER = "#065F46";
    private const string COLOR_PRIMARY = "#10B981";
    private const string COLOR_HEADER = "#6EE7B7";
    private const string COLOR_HIGHLIGHT = "#34D399";
    private const string COLOR_DIM = "#6B7280";

    // =================================================================
    // STATE
    // =================================================================

    private int selectedIndex = 0;
    private bool isGameRunning = false;
    private int activeGameIndex = -1;

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        Debug.Log("[DT_App_Games] OnAppOpen called");
        selectedIndex = 0;
        isGameRunning = false;
        activeGameIndex = -1;

        if (highScores == null || (gameObjects != null && highScores.Length != gameObjects.Length))
        {
            if (gameObjects != null) highScores = new int[gameObjects.Length];
        }
        
        Debug.Log("[DT_App_Games] gameObjects count: " + (gameObjects != null ? gameObjects.Length.ToString() : "NULL"));
        Debug.Log("[DT_App_Games] coreReference valid: " + Utilities.IsValid(coreReference).ToString());
        
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        if (isGameRunning) OnGameExit();
    }

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        if (isGameRunning)
        {
            if (inputKey == "LEFT") OnGameExit();
            inputKey = "";
            return;
        }

        if (inputKey == "UP") NavigateUp();
        else if (inputKey == "DOWN") NavigateDown();
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT") LaunchSelectedGame();
        else if (inputKey == "LEFT")
        {
            // Return to Shell menu
            if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
            {
                coreReference.SetProgramVariable("nextProcess", shellApp);
                coreReference.SendCustomEvent("LoadNextProcess");
            }
        }

        inputKey = "";
    }

    private void NavigateUp()
    {
        if (gameObjects == null || gameObjects.Length == 0) return;

        selectedIndex--;
        if (selectedIndex < 0) selectedIndex = gameObjects.Length - 1;
    }

    private void NavigateDown()
    {
        if (gameObjects == null || gameObjects.Length == 0) return;

        selectedIndex++;
        if (selectedIndex >= gameObjects.Length) selectedIndex = 0;
    }

    // =================================================================
    // GAME LAUNCH
    // =================================================================

    private void LaunchSelectedGame()
    {
        if (gameObjects == null || selectedIndex >= gameObjects.Length) return;
        if (gameEnabled != null && selectedIndex < gameEnabled.Length && !gameEnabled[selectedIndex]) return;

        GameObject selectedGame = gameObjects[selectedIndex];
        if (!Utilities.IsValid(selectedGame)) return;

        activeGameIndex = selectedIndex;
        isGameRunning = true;

        if (Utilities.IsValid(terminalCamera)) terminalCamera.enabled = false;

        if (gameCameras != null && selectedIndex < gameCameras.Length)
        {
            Camera gameCamera = gameCameras[selectedIndex];
            if (Utilities.IsValid(gameCamera)) gameCamera.enabled = true;
        }

        selectedGame.SetActive(true);

        UdonBehaviour gameScript = (UdonBehaviour)selectedGame.GetComponent(typeof(UdonBehaviour));
        if (Utilities.IsValid(gameScript)) gameScript.SendCustomEvent("StartGame");
    }

    // =================================================================
    // GAME EXIT (Called by games)
    // =================================================================

    public void OnGameExit()
    {
        if (!isGameRunning || activeGameIndex < 0) return;

        if (gameObjects != null && activeGameIndex < gameObjects.Length)
        {
            GameObject game = gameObjects[activeGameIndex];
            if (Utilities.IsValid(game)) game.SetActive(false);
        }

        if (gameCameras != null && activeGameIndex < gameCameras.Length)
        {
            Camera gameCamera = gameCameras[activeGameIndex];
            if (Utilities.IsValid(gameCamera)) gameCamera.enabled = false;
        }

        if (Utilities.IsValid(terminalCamera)) terminalCamera.enabled = true;

        isGameRunning = false;
        activeGameIndex = -1;
    }

    // =================================================================
    // DISPLAY RENDERING
    // =================================================================

    private string BoxRow(string content)
    {
        return "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color> " +
               DT_Format.PadLeft(content, CONTENT_W) +
               " <color=" + COLOR_BORDER + ">" + DT_Format.BORDER_VERTICAL + "</color>";
    }

    public string GetDisplayContent()
    {
        string o = "";

        // Top border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxTop(WIDTH) + "</color>\n";

        // Title
        o = o + BoxRow("<color=" + COLOR_HEADER + ">" + DT_Format.PadCenter("SELECT CARTRIDGE", CONTENT_W) + "</color>") + "\n";

        // Divider
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "</color>\n";

        if (gameObjects == null || gameObjects.Length == 0)
        {
            o = o + BoxRow("<color=" + COLOR_PRIMARY + ">  ERROR: No game cartridges found.</color>") + "\n";
            o = o + BoxRow("<color=" + COLOR_DIM + ">  Configure gameObjects in Inspector.</color>") + "\n";
        }
        else
        {
            int rowsUsed = 0;
            for (int i = 0; i < gameObjects.Length && rowsUsed < VISIBLE_ROWS - 2; i++)
            {
                string indexDisplay = "[" + (i + 1).ToString() + "]";
                string gameName = (gameNames != null && i < gameNames.Length) ? gameNames[i] : "UNKNOWN";

                // Game name row
                if (i == selectedIndex)
                {
                    o = o + BoxRow("<color=" + COLOR_HIGHLIGHT + ">> " + indexDisplay + "  " + gameName + "</color>") + "\n";
                }
                else
                {
                    o = o + BoxRow("<color=" + COLOR_PRIMARY + ">  " + indexDisplay + "  " + gameName + "</color>") + "\n";
                }
                rowsUsed++;

                // Description row
                string description = (gameDescriptions != null && i < gameDescriptions.Length)
                    ? gameDescriptions[i] : "No description.";

                bool enabled = (gameEnabled == null || i >= gameEnabled.Length) ? true : gameEnabled[i];
                string status = enabled ? "Ready" : "<color=" + COLOR_DIM + ">Coming Soon</color>";
                if (enabled && highScores != null && i < highScores.Length && highScores[i] > 0)
                {
                    status = "High Score: " + highScores[i].ToString();
                }

                o = o + BoxRow("<color=" + COLOR_DIM + ">       " + description + " [" + status + "]</color>") + "\n";
                rowsUsed++;
            }

            // Pad remaining rows
            for (int i = rowsUsed; i < VISIBLE_ROWS; i++)
            {
                o = o + BoxRow("") + "\n";
            }
        }

        // Bottom border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxBottom(WIDTH) + "</color>\n";

        // Navigation footer
        o = o + " <color=" + COLOR_PRIMARY + ">[D] Launch  [W/S] Navigate  [A] Back</color>\n";

        return o;
    }

    /// <summary>
    /// Push display content to DT_Core for rendering
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", GetDisplayContent());
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    // =================================================================
    // HIGH SCORE (Called by games)
    // =================================================================

    public void UpdateHighScore(int gameIndex, int score)
    {
        if (highScores == null || gameIndex < 0 || gameIndex >= highScores.Length) return;
        if (score > highScores[gameIndex]) highScores[gameIndex] = score;
    }
}
