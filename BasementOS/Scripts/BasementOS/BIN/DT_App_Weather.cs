using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;
using LowerLevel.Terminal;

/// <summary>
/// BASEMENT OS WEATHER APPLICATION (v2.2)
///
/// ROLE: WEATHER FORECAST DISPLAY
/// Displays weather using CACHED data from DT_WeatherModule.
/// CRITICAL: NEVER calls weather API directly - reads cached data only.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Weather.cs
///
/// INTEGRATION:
/// - DT_WeatherModule: Source of cached weather data (read-only)
/// - DT_Core: System core for app lifecycle
/// - Uses: Terminal_Style_Guide.md box format
///
/// LIMITATIONS:
/// - Max 250 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Weather : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // MODULE REFERENCES
    // =================================================================

    [Header("--- Module References ---")]
    [Tooltip("Weather Module - provides cached weather data")]
    [SerializeField] private DT_WeatherModule weatherModule;

    [Tooltip("Core System Reference")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Tooltip("Shell app to return to on LEFT key")]
    [SerializeField] private UdonSharpBehaviour shellApp;

    [Header("--- Weather Display ---")]
    [Tooltip("Location string to display")]
    [SerializeField] private string locationString = "Chicago, IL";

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

    // =================================================================
    // CACHED WEATHER DATA
    // =================================================================

    private string cachedTemperature = "74°F";
    private string cachedCondition = "Clear";
    private bool weatherOnline = true;
    private string displayContent = "";

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        RefreshWeatherFromCache();
        RenderWeatherDisplay();
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        displayContent = "";
    }

    public void OnInput()
    {
        if (inputKey == "ACCEPT" || inputKey == "RIGHT")
        {
            RefreshWeatherFromCache();
            RenderWeatherDisplay();
            PushDisplayToCore();
        }
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

    public string GetDisplayContent()
    {
        return displayContent;
    }

    /// <summary>
    /// Push display content to DT_Core for rendering
    /// </summary>
    private void PushDisplayToCore()
    {
        if (Utilities.IsValid(coreReference))
        {
            coreReference.SetProgramVariable("contentBuffer", displayContent);
            coreReference.SendCustomEvent("RefreshDisplay");
        }
    }

    // =================================================================
    // WEATHER DATA ACCESS
    // =================================================================

    private void RefreshWeatherFromCache()
    {
        if (!Utilities.IsValid(weatherModule))
        {
            cachedTemperature = "N/A";
            cachedCondition = "Offline";
            weatherOnline = false;
            return;
        }

        // Use the public getter methods from DT_WeatherModule
        cachedTemperature = weatherModule.GetTemperature();
        cachedCondition = weatherModule.GetCondition();
        weatherOnline = weatherModule.IsWeatherOnline();
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

    private void RenderWeatherDisplay()
    {
        string o = "";

        // Top border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxTop(WIDTH) + "</color>\n";

        // Title row
        o = o + BoxRow("<color=" + COLOR_HEADER + ">" + DT_Format.PadCenter("WEATHER SENSORS - " + locationString.ToUpper(), CONTENT_W) + "</color>") + "\n";

        // Divider
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.BORDER_LEFT_T + DT_Format.RepeatChar(DT_Format.BORDER_HORIZONTAL, WIDTH - 2) + DT_Format.BORDER_RIGHT_T + "</color>\n";

        // Weather content
        string[] artLines = GetWeatherArtLines(cachedCondition);
        string statusText = weatherOnline ? "<color=" + COLOR_HIGHLIGHT + ">Online</color>" : "<color=#EF4444>OFFLINE</color>";

        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">    " + artLines[0] + "                   CURRENT CONDITIONS</color>") + "\n";
        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">  " + artLines[1] + "                   Temperature: " + cachedTemperature + "</color>") + "\n";
        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">  " + artLines[2] + "                   Condition:   " + cachedCondition + "</color>") + "\n";
        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">  " + artLines[3] + "                   Status:      </color>" + statusText) + "\n";
        o = o + BoxRow("") + "\n";

        // Forecast section
        o = o + BoxRow("<color=" + COLOR_HEADER + ">FORECAST</color>") + "\n";
        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">TODAY        Tomorrow     Wednesday    Thursday     Friday</color>") + "\n";
        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">" + DT_Format.PadLeft(cachedTemperature, 8) + "     --°F         --°F         --°F        --°F</color>") + "\n";
        o = o + BoxRow("<color=" + COLOR_PRIMARY + ">" + GetConditionShort(cachedCondition) + "     Pending      Pending      Pending     Pending</color>") + "\n";

        // Pad remaining rows
        o = o + BoxRow("") + "\n";
        o = o + BoxRow("") + "\n";

        // Bottom border
        o = o + "<color=" + COLOR_BORDER + ">" + DT_Format.GenerateBoxBottom(WIDTH) + "</color>\n";

        // Navigation footer
        o = o + " <color=" + COLOR_PRIMARY + ">[D] Refresh  [A] Back</color>\n";

        displayContent = o;
    }

    private string[] GetWeatherArtLines(string condition)
    {
        string[] lines = new string[4];
        string condLower = condition.ToLower();

        if (condLower.Contains("clear") || condLower.Contains("sunny"))
        {
            lines[0] = ".-~~~-.";
            lines[1] = ".-~ ~ ~-.";
            lines[2] = "(   O   )";
            lines[3] = "'-.___.-'";
        }
        else if (condLower.Contains("rain") || condLower.Contains("drizzle"))
        {
            lines[0] = ".-~~~-.";
            lines[1] = ".-~~~-. '.";
            lines[2] = "(      )  )";
            lines[3] = "' ' ' ' '";
        }
        else
        {
            lines[0] = ".-~~~-.";
            lines[1] = ".-~~~-. '.";
            lines[2] = "(      )  )";
            lines[3] = "'-.___.-'";
        }

        return lines;
    }

    private string GetConditionShort(string condition)
    {
        string condLower = condition.ToLower();

        if (condLower.Contains("clear") || condLower.Contains("sunny")) return "Sunny  ";
        if (condLower.Contains("rain")) return "Rain   ";
        if (condLower.Contains("cloud")) return "Cloudy ";
        if (condLower.Contains("storm")) return "Storm  ";

        return "Unknown";
    }

    private string GenerateSeparator()
    {
        return "════════════════════════════════════════════════════════════════════════════";
    }

    // =================================================================
    // TDD / TESTING
    // =================================================================

#if UNITY_EDITOR
    [ContextMenu("TEST: Cache Validation")]
    public void Test_CacheValidation()
    {
        Debug.Log("[DT_App_Weather] TEST: Cache Validation");

        RefreshWeatherFromCache();
        RenderWeatherDisplay();

        bool hasTemp = !string.IsNullOrEmpty(cachedTemperature);
        bool hasCondition = !string.IsNullOrEmpty(cachedCondition);

        if (hasTemp && hasCondition)
        {
            Debug.Log("[TEST] PASS - Cache read: " + cachedTemperature + " " + cachedCondition);
        }
        else
        {
            Debug.LogError("[TEST] FAIL - Cache data incomplete");
        }
    }
#endif
}
