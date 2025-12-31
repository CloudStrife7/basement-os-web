using UdonSharp;
using UnityEngine;

/// <summary>
/// DOS Terminal Rain Shader Module
/// Manages rain visual effects on window materials based on weather conditions.
/// Extracted from DOSTerminalController.cs as part of modularization (Phase 1).
/// </summary>
public class DT_RainShader : UdonSharpBehaviour
{
    [Header("Rain Shader Settings")]
    [Tooltip("Window material with rain shader")]
    public Material windowRainMaterial;

    [Header("Debug Settings")]
    public bool enableDebugLogging = true;

    // =================================================================
    // PUBLIC API
    // =================================================================

    /// <summary>
    /// Updates rain shader based on weather condition string.
    /// Triggers rain effects for conditions containing "rain", "storm", or "drizzle".
    /// Calculates intensity based on keywords (light, heavy, storm).
    /// </summary>
    /// <param name="weatherCondition">Weather condition string (e.g., "Light Rain", "Heavy Storm", "Clear")</param>
    public void UpdateRain(string weatherCondition)
    {
        if (windowRainMaterial == null)
        {
            LogDebug("⚠️ windowRainMaterial is null - cannot update rain shader");
            return;
        }

        bool isRaining = weatherCondition.ToLower().Contains("rain") ||
                        weatherCondition.ToLower().Contains("storm") ||
                        weatherCondition.ToLower().Contains("drizzle");

        if (isRaining)
        {
            float intensity = GetRainIntensity(weatherCondition.ToLower());
            EnableRainEffect(intensity);
            LogDebug($"🌧️ Rain ENABLED - Intensity: {intensity:F2} - Condition: {weatherCondition}");
        }
        else
        {
            DisableRainEffect();
            LogDebug($"🌧️ Rain DISABLED - Condition: {weatherCondition}");
        }
    }

    // =================================================================
    // CONTEXT MENU TEST METHODS
    // =================================================================

    [ContextMenu("🔧 Test Rain - Light")]
    public void TestRainLight()
    {
        UpdateRain("Light Rain");
    }

    [ContextMenu("🔧 Test Rain - Heavy")]
    public void TestRainHeavy()
    {
        UpdateRain("Heavy Storm");
    }

    [ContextMenu("🔧 Test Rain - Clear")]
    public void TestRainClear()
    {
        UpdateRain("Clear");
    }

    // =================================================================
    // PRIVATE METHODS
    // =================================================================

    /// <summary>
    /// Enables rain effect on window material with specified intensity.
    /// Sets shader properties for droplets, rivulets, and distortion.
    /// </summary>
    /// <param name="intensity">Rain intensity (0.0 to 1.0)</param>
    private void EnableRainEffect(float intensity)
    {
        windowRainMaterial.SetFloat("_Droplets_Strength", intensity);
        windowRainMaterial.SetFloat("Droplets_Strength", intensity);
        windowRainMaterial.SetFloat("Rivulets Strength", intensity * 0.6f);
        windowRainMaterial.SetFloat("Rivulet Speed", 0.0075f + (intensity * 0.005f));
        windowRainMaterial.SetFloat("Droplets Strike Speed", 0.08f + (intensity * 0.04f));
        windowRainMaterial.SetFloat("_Distortion", 0.01f + (intensity * 0.02f));
    }

    /// <summary>
    /// Disables rain effect by setting all shader properties to zero.
    /// </summary>
    private void DisableRainEffect()
    {
        windowRainMaterial.SetFloat("_Droplets_Strength", 0f);
        windowRainMaterial.SetFloat("Droplets_Strength", 0f);
        windowRainMaterial.SetFloat("Rivulets Strength", 0f);
        windowRainMaterial.SetFloat("Rivulet Speed", 0f);
        windowRainMaterial.SetFloat("Droplets Strike Speed", 0f);
        windowRainMaterial.SetFloat("_Distortion", 0f);
    }

    /// <summary>
    /// Calculates rain intensity based on weather condition keywords.
    /// </summary>
    /// <param name="condition">Weather condition string (lowercase)</param>
    /// <returns>Intensity value: 0.3 (light), 0.7 (normal), 1.0 (heavy/storm)</returns>
    private float GetRainIntensity(string condition)
    {
        if (condition.Contains("drizzle") || condition.Contains("light"))
            return 0.3f;
        else if (condition.Contains("heavy") || condition.Contains("storm"))
            return 1.0f;
        else if (condition.Contains("rain"))
            return 0.7f;

        return 0f;
    }

    /// <summary>
    /// Logs debug message if debug logging is enabled.
    /// </summary>
    /// <param name="message">Message to log</param>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[DT_RainShader] {message}");
        }
    }
}
