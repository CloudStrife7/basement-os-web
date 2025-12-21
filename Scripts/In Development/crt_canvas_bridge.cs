using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CRT Canvas Bridge - Captures Canvas UI and applies CRT shader effect
/// 
/// This script bridges Canvas UI with CRT material effects by:
/// 1. Rendering your Canvas to a RenderTexture
/// 2. Displaying that texture on a quad with CRT material
/// 3. Hiding the original Canvas to show only the CRT version
/// 
/// Setup Instructions:
/// 1. Attach this script to your DosBootCanvas GameObject
/// 2. Assign your Canvas in the "Target Canvas" field
/// 3. Create a material with CRT_master shader and assign to "CRT Material" 
/// 4. Adjust Render Texture Size (1024x1024 recommended for crisp text)
/// 5. The script will automatically create and position the CRT display quad
/// </summary>
public class CRT_Canvas_Bridge : MonoBehaviour
{
    [Header("Canvas Capture Settings")]
    [Tooltip("The Canvas you want to apply CRT effect to (DosBootCanvas)")]
    public Canvas targetCanvas;
    
    [Tooltip("The CRT material (with CRT_master shader)")]
    public Material crtMaterial;
    
    [Header("Render Texture Settings")]
    [Tooltip("Resolution of the captured texture (higher = sharper text)")]
    public Vector2Int renderTextureSize = new Vector2Int(1024, 1024);
    
    [Tooltip("How often to update the render texture (0 = every frame)")]
    public float updateInterval = 0.0f;
    
    [Header("Display Settings")]
    [Tooltip("Automatically position the CRT quad based on Canvas position")]
    public bool autoPosition = true;
    
    [Tooltip("Scale multiplier for the CRT display quad")]
    public float displayScale = 1.0f;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = true;
    
    // Private components
    private RenderTexture renderTexture;
    private Camera renderCamera;
    private GameObject crtQuad;
    private Renderer crtRenderer;
    private Canvas originalCanvas;
    private float lastUpdateTime;
    
    void Start()
    {
        InitializeCRTBridge();
    }
    
    void Update()
    {
        // Update render texture at specified interval
        if (updateInterval <= 0 || Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateRenderTexture();
            lastUpdateTime = Time.time;
        }
    }
    
    void InitializeCRTBridge()
    {
        // Validate setup
        if (targetCanvas == null)
        {
            LogError("Target Canvas not assigned! Please assign your DosBootCanvas.");
            return;
        }
        
        if (crtMaterial == null)
        {
            LogError("CRT Material not assigned! Please assign a material with CRT_master shader.");
            return;
        }
        
        // Store reference to original canvas
        originalCanvas = targetCanvas;
        
        // Create render texture
        CreateRenderTexture();
        
        // Create render camera
        CreateRenderCamera();
        
        // Create CRT display quad
        CreateCRTQuad();
        
        // Position everything
        if (autoPosition)
        {
            PositionCRTDisplay();
        }
        
        // Hide original canvas (we'll show the CRT version)
        SetCanvasVisibility(false);
        
        LogDebug("🖥️ CRT Canvas Bridge initialized successfully!");
        LogDebug($"📺 Render texture: {renderTextureSize.x}x{renderTextureSize.y}");
        LogDebug($"🎭 CRT Material: {crtMaterial.name}");
    }
    
    void CreateRenderTexture()
    {
        renderTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 24);
        renderTexture.format = RenderTextureFormat.ARGB32;
        renderTexture.antiAliasing = 4; // Smooth text
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.Create();
        
        LogDebug($"📱 Created render texture: {renderTexture.width}x{renderTexture.height}");
    }
    
    void CreateRenderCamera()
    {
        // Create camera GameObject
        GameObject cameraGO = new GameObject("CRT_RenderCamera");
        cameraGO.transform.SetParent(transform);
        
        // Add camera component
        renderCamera = cameraGO.AddComponent<Camera>();
        renderCamera.targetTexture = renderTexture;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.black;
        renderCamera.orthographic = true;
        renderCamera.nearClipPlane = 0.1f;
        renderCamera.farClipPlane = 10f;
        renderCamera.depth = -10; // Render before main camera
        
        // Position camera to capture the canvas
        PositionRenderCamera();
        
        LogDebug("📹 Created render camera for canvas capture");
    }
    
    void CreateCRTQuad()
    {
        // Create quad GameObject
        crtQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        crtQuad.name = "CRT_Display_Quad";
        crtQuad.transform.SetParent(transform);
        
        // Get renderer and apply CRT material
        crtRenderer = crtQuad.GetComponent<Renderer>();
        crtRenderer.material = new Material(crtMaterial); // Create instance
        
        // Set the render texture as the main texture
        crtRenderer.material.mainTexture = renderTexture;
        
        // Remove collider (we don't need it)
        Collider quadCollider = crtQuad.GetComponent<Collider>();
        if (quadCollider != null)
        {
            DestroyImmediate(quadCollider);
        }
        
        LogDebug("🖥️ Created CRT display quad with material");
    }
    
    void PositionRenderCamera()
    {
        if (renderCamera == null || originalCanvas == null) return;
        
        // Get canvas bounds
        RectTransform canvasRect = originalCanvas.GetComponent<RectTransform>();
        Bounds canvasBounds = GetCanvasBounds();
        
        // Position camera to frame the canvas perfectly
        Vector3 cameraPosition = canvasBounds.center + Vector3.back * 5f;
        renderCamera.transform.position = cameraPosition;
        renderCamera.transform.LookAt(canvasBounds.center);
        
        // Set orthographic size to fit canvas
        float canvasHeight = canvasBounds.size.y;
        renderCamera.orthographicSize = canvasHeight * 0.5f * 1.1f; // 10% padding
        
        LogDebug($"📷 Positioned render camera at {cameraPosition}");
    }
    
    void PositionCRTDisplay()
    {
        if (crtQuad == null || originalCanvas == null) return;
        
        // Match the original canvas position and scale
        RectTransform canvasRect = originalCanvas.GetComponent<RectTransform>();
        
        // Position CRT quad at canvas location
        crtQuad.transform.position = originalCanvas.transform.position;
        crtQuad.transform.rotation = originalCanvas.transform.rotation;
        
        // Scale to match canvas size
        Vector3 canvasScale = originalCanvas.transform.localScale;
        Vector3 canvasSize = new Vector3(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y, 1f);
        Vector3 quadScale = Vector3.Scale(canvasScale, canvasSize) * 0.001f * displayScale; // Convert UI units to world units
        
        crtQuad.transform.localScale = quadScale;
        
        LogDebug($"📺 Positioned CRT display at {crtQuad.transform.position}");
    }
    
    void UpdateRenderTexture()
    {
        if (renderCamera == null || renderTexture == null) return;
        
        // Temporarily show the canvas for rendering
        SetCanvasVisibility(true);
        
        // Render the canvas to texture
        renderCamera.Render();
        
        // Hide the canvas again
        SetCanvasVisibility(false);
    }
    
    void SetCanvasVisibility(bool visible)
    {
        if (originalCanvas != null)
        {
            originalCanvas.gameObject.SetActive(visible);
        }
    }
    
    Bounds GetCanvasBounds()
    {
        RectTransform canvasRect = originalCanvas.GetComponent<RectTransform>();
        
        // Get canvas bounds in world space
        Vector3[] corners = new Vector3[4];
        canvasRect.GetWorldCorners(corners);
        
        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        foreach (Vector3 corner in corners)
        {
            bounds.Encapsulate(corner);
        }
        
        return bounds;
    }
    
    // Public methods for runtime control
    
    /// <summary>
    /// Enable or disable the CRT effect
    /// </summary>
    public void SetCRTEnabled(bool enabled)
    {
        if (crtQuad != null)
        {
            crtQuad.SetActive(enabled);
        }
        
        // Show original canvas if CRT is disabled
        SetCanvasVisibility(!enabled);
        
        LogDebug($"🖥️ CRT effect {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Update the CRT material (for runtime material swapping)
    /// </summary>
    public void SetCRTMaterial(Material newMaterial)
    {
        if (crtRenderer != null && newMaterial != null)
        {
            crtRenderer.material = newMaterial;
            crtRenderer.material.mainTexture = renderTexture;
            LogDebug($"🎭 CRT material updated to: {newMaterial.name}");
        }
    }
    
    /// <summary>
    /// Force immediate render texture update
    /// </summary>
    public void ForceUpdate()
    {
        UpdateRenderTexture();
        LogDebug("🔄 Forced render texture update");
    }
    
    // Context menu methods for testing
    [ContextMenu("Enable CRT Effect")]
    void EnableCRT()
    {
        SetCRTEnabled(true);
    }
    
    [ContextMenu("Disable CRT Effect")]
    void DisableCRT()
    {
        SetCRTEnabled(false);
    }
    
    [ContextMenu("Force Update")]
    void ForceUpdateContext()
    {
        ForceUpdate();
    }
    
    [ContextMenu("Reposition CRT Display")]
    void RepositionDisplay()
    {
        if (autoPosition)
        {
            PositionCRTDisplay();
        }
    }
    
    // Debug logging
    void LogDebug(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[CRT Bridge] {message}");
        }
    }
    
    void LogError(string message)
    {
        Debug.LogError($"[CRT Bridge] ❌ {message}");
    }
    
    // Cleanup
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (originalCanvas != null)
        {
            // Draw canvas bounds
            Gizmos.color = Color.green;
            Bounds bounds = GetCanvasBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        
        if (renderCamera != null)
        {
            // Draw camera view
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(renderCamera.transform.position, originalCanvas.transform.position);
        }
    }
}