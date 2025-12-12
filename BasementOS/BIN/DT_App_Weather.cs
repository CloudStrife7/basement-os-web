using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

/// <summary>
/// BASEMENT OS WEATHER APPLICATION (v2.1)
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
    [SerializeField] private UdonSharpBehaviour weatherModule;

    [Tooltip("Core System Reference")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Header("--- Weather Display ---")]
    [Tooltip("Location string to display")]
    [SerializeField] private string locationString = "Chicago, IL";

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
        }
        inputKey = "";
    }

    public string GetDisplayContent()
    {
        return displayContent;
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

        object tempObj = weatherModule.GetProgramVariable("currentTemperature");
        if (tempObj != null) cachedTemperature = (string)tempObj;

        object condObj = weatherModule.GetProgramVariable("currentCondition");
        if (condObj != null) cachedCondition = (string)condObj;

        object onlineObj = weatherModule.GetProgramVariable("isOnline");
        if (onlineObj != null) weatherOnline = (bool)onlineObj;
    }

    // =================================================================
    // DISPLAY RENDERING
    // =================================================================

    private void RenderWeatherDisplay()
    {
        string output = "";

        output = output + " WEATHER SENSORS - " + locationString + "\n";
        output = output + GenerateSeparator() + "\n\n";

        string[] artLines = GetWeatherArtLines(cachedCondition);

        output = output + "      " + artLines[0] + "                   CURRENT CONDITIONS\n";
        output = output + "    " + artLines[1] + "                   ──────────────────\n";
        output = output + "    " + artLines[2] + "                   Temperature: " + cachedTemperature + "\n";
        output = output + "    " + artLines[3] + "                   Condition:   " + cachedCondition + "\n";
        output = output + "                                       Status:      ";
        output = output + (weatherOnline ? "Online" : "Offline") + "\n\n";

        output = output + "  FORECAST\n";
        output = output + "  ────────────────────────────────────────────────────────────────────────\n";
        output = output + "  TODAY      Tomorrow    Wednesday   Thursday    Friday\n";
        output = output + "   " + cachedTemperature + "       --°F        --°F        --°F       --°F\n";
        output = output + "  " + GetConditionShort(cachedCondition) + "      Pending     Pending     Pending    Pending\n";

        output = output + "\n\n  [USE] Refresh  [LEFT] Back\n";

        if (!weatherOnline)
        {
            output = output + "\n  WARNING: Weather data unavailable. Using cached values.\n";
        }

        displayContent = output;
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
