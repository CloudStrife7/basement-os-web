/// <summary>
/// COMPONENT PURPOSE:
/// Standalone debug script for testing rain shader functionality
/// Discovers shader property names and tests rain effects without modifying main scripts
/// Isolated testing environment for rain shader integration development
/// 
/// LOWER LEVEL 2.0 INTEGRATION:
/// Development tool for testing basement window rain effects
/// Helps identify correct shader property names before integrating with weather system
/// Provides safe testing environment without affecting main DOSTerminalController
/// 
/// DEPENDENCIES & REQUIREMENTS:
/// - Material with rain shader applied to test object
/// - Test GameObject with renderer (plane, quad, or window mesh)
/// - RainOnGlass or similar rain shader material
/// 
/// ARCHITECTURE PATTERN:
/// Context menu driven testing with comprehensive property discovery
/// Real-time material property manipulation for immediate visual feedback
/// Logging system for debugging shader communication issues
/// </summary>

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RainShaderDebugger : UdonSharpBehaviour
{
    [Header("Rain Shader Testing")]
    [Tooltip("Material with rain shader to test")]
    public Material rainMaterial;

    [Tooltip("Test object renderer (your plane)")]
    public Renderer testRenderer;

    [Tooltip("Material slot index (usually 0)")]
    public int materialSlot = 0;

    [Header("Test Values")]
    [Tooltip("Rain intensity for testing (0-1)")]
    public float testIntensity = 1.0f;

    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = true;

    // Property names to test
    private string[] commonRainProperties = {
        "Droplets_Strength", "_Droplets_Strength", "DropletsStrength",
        "Rivulets Strength", "_Rivulets_Strength", "RivuletsStrength",
        "Rivulet Speed", "_Rivulet_Speed", "RivuletSpeed",
        "Droplets Strike Speed", "_Droplets_Strike_Speed", "DropletsStrikeSpeed",
        "Droplets_Gravity", "_Droplets_Gravity", "DropletsGravity",
        "_MainTex", "_Color", "_Cutoff", "_Distortion", "_Tiling"
    };

    void Start()
    {
        LogDebug("🧪 RainShaderDebugger initialized");

        if (rainMaterial == null)
        {
            LogDebug("❌ No rain material assigned - assign in Inspector!");
        }

        if (testRenderer == null)
        {
            LogDebug("❌ No test renderer assigned - assign your plane!");
        }
    }

    [ContextMenu("🔍 1. Discovery - List All Shader Properties")]
    public void DiscoverShaderProperties()
    {
        if (rainMaterial == null)
        {
            LogDebug("❌ No material assigned!");
            return;
        }

        LogDebug("=== SHADER PROPERTY DISCOVERY ===");
        LogDebug($"Material: {rainMaterial.name}");
        LogDebug($"Shader: {rainMaterial.shader.name}");
        LogDebug("");

        LogDebug("Testing common rain property names:");
        foreach (string prop in commonRainProperties)
        {
            if (rainMaterial.HasProperty(prop))
            {
                // UdonSharp doesn't support try/catch - just check if property exists
                float currentValue = rainMaterial.GetFloat(prop);
                LogDebug($"✅ FOUND: '{prop}' = {currentValue:F3}");
            }
            else
            {
                LogDebug($"❌ NOT FOUND: '{prop}'");
            }
        }

        LogDebug("=== DISCOVERY COMPLETE ===");
    }

    [ContextMenu("🌧️ 2. Test Rain - Enable Heavy")]
    public void TestEnableHeavyRain()
    {
        if (rainMaterial == null)
        {
            LogDebug("❌ No material assigned!");
            return;
        }

        LogDebug("🌧️ Testing HEAVY RAIN - Using EXACT property names...");

        float intensity = testIntensity;  // Use Inspector value

        // Set both droplet strength properties (the underscore one seems to be the main one)
        rainMaterial.SetFloat("Droplets_Strength", intensity);
        LogDebug($"✅ Set 'Droplets_Strength' to {intensity}");

        rainMaterial.SetFloat("_Droplets_Strength", intensity);  // ← Probably the main control
        LogDebug($"✅ Set '_Droplets_Strength' to {intensity}");

        // Set rivulets (no underscore version)
        rainMaterial.SetFloat("Rivulets Strength", intensity * 0.6f);
        LogDebug($"✅ Set 'Rivulets Strength' to {intensity * 0.6f}");

        // Set speeds (no underscore versions)
        rainMaterial.SetFloat("Rivulet Speed", 0.0075f + (intensity * 0.005f));
        LogDebug($"✅ Set 'Rivulet Speed' to {0.0075f + (intensity * 0.005f):F4}");

        rainMaterial.SetFloat("Droplets Strike Speed", 0.08f + (intensity * 0.04f));
        LogDebug($"✅ Set 'Droplets Strike Speed' to {0.08f + (intensity * 0.04f):F4}");

        // Restore distortion to original value
        rainMaterial.SetFloat("_Distortion", 0.01f);
        LogDebug("✅ Set '_Distortion' to 0.01");

        LogDebug("🌧️ Heavy rain test complete - should see dramatic rain!");
    }

    [ContextMenu("☀️ 3. Test Rain - Disable")]
    public void TestDisableRain()
    {
        if (rainMaterial == null)
        {
            LogDebug("❌ No material assigned!");
            return;
        }

        LogDebug("☀️ Testing CLEAR WEATHER - Using EXACT property names...");

        // Use the EXACT property names we discovered
        rainMaterial.SetFloat("Droplets_Strength", 0f);
        LogDebug("✅ Set 'Droplets_Strength' to 0");

        rainMaterial.SetFloat("_Droplets_Strength", 0f);  // ← This is probably the key one!
        LogDebug("✅ Set '_Droplets_Strength' to 0");

        rainMaterial.SetFloat("Rivulets Strength", 0f);
        LogDebug("✅ Set 'Rivulets Strength' to 0");

        rainMaterial.SetFloat("Rivulet Speed", 0f);
        LogDebug("✅ Set 'Rivulet Speed' to 0");

        rainMaterial.SetFloat("Droplets Strike Speed", 0f);
        LogDebug("✅ Set 'Droplets Strike Speed' to 0");

        // Also try disabling distortion which might affect drips
        rainMaterial.SetFloat("_Distortion", 0f);
        LogDebug("✅ Set '_Distortion' to 0");

        LogDebug("☀️ Complete disable test finished - should be totally clear now!");
    }

    [ContextMenu("🌦️ 4. Test Rain - Light Drizzle")]
    public void TestLightRain()
    {
        if (rainMaterial == null)
        {
            LogDebug("❌ No material assigned!");
            return;
        }

        LogDebug("🌦️ Testing LIGHT DRIZZLE - Using EXACT property names...");

        float lightIntensity = 0.3f;

        // Use the EXACT property names we discovered
        rainMaterial.SetFloat("Droplets_Strength", lightIntensity);
        LogDebug($"✅ Set 'Droplets_Strength' to {lightIntensity}");

        rainMaterial.SetFloat("_Droplets_Strength", lightIntensity);  // ← Main control
        LogDebug($"✅ Set '_Droplets_Strength' to {lightIntensity}");

        rainMaterial.SetFloat("Rivulets Strength", lightIntensity * 0.4f);  // Lighter rivulets
        LogDebug($"✅ Set 'Rivulets Strength' to {lightIntensity * 0.4f:F2}");

        rainMaterial.SetFloat("Rivulet Speed", 0.005f);  // Slower speed for drizzle
        LogDebug("✅ Set 'Rivulet Speed' to 0.005");

        rainMaterial.SetFloat("Droplets Strike Speed", 0.06f);  // Slower strikes
        LogDebug("✅ Set 'Droplets Strike Speed' to 0.06");

        // Light distortion for subtle effect
        rainMaterial.SetFloat("_Distortion", 0.005f);
        LogDebug("✅ Set '_Distortion' to 0.005");

        LogDebug($"🌦️ Light drizzle test complete - should see subtle rain!");
    }

    [ContextMenu("📊 6. Get Current Property Values")]
    public void GetCurrentValues()
    {
        if (rainMaterial == null)
        {
            LogDebug("❌ No material assigned!");
            return;
        }

        LogDebug("=== CURRENT PROPERTY VALUES ===");

        string[] checkProperties = {
            "Droplets_Strength", "_Droplets_Strength",
            "Rivulets Strength", "_Rivulets_Strength",
            "Rivulet Speed", "_Rivulet_Speed",
            "Droplets Strike Speed", "_Droplets_Strike_Speed"
        };

        foreach (string prop in checkProperties)
        {
            if (rainMaterial.HasProperty(prop))
            {
                float value = rainMaterial.GetFloat(prop);
                LogDebug($"'{prop}' = {value:F3}");
            }
        }

        LogDebug("=== VALUES COMPLETE ===");
    }

    [ContextMenu("🎛️ 7. Manual Property Test")]
    public void ManualPropertyTest()
    {
        if (rainMaterial == null)
        {
            LogDebug("❌ No material assigned!");
            return;
        }

        LogDebug($"🎛️ Manual test with intensity: {testIntensity}");

        // Use the Test Intensity value from Inspector
        if (rainMaterial.HasProperty("Droplets_Strength"))
        {
            rainMaterial.SetFloat("Droplets_Strength", testIntensity);
            LogDebug($"✅ Set Droplets_Strength to {testIntensity}");
        }

        LogDebug("🎛️ Manual test complete - adjust 'Test Intensity' in Inspector and run again");
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"🧪 [RainShaderDebugger] {message}");
        }
    }
}