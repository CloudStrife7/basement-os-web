/// <summary>
/// BASEMENT OS TICKER (v2.1)
///
/// ROLE: RSS TICKER / SCROLLING TEXT
/// LOCATION: Assets/Scripts/BasementOS/LIB/DT_Ticker.cs
///
/// INTEGRATION:
/// - Called by: DT_Core.Update() via UpdateTick(deltaTime)
/// - Read by: DT_Core.GetVisibleText() for Line 2
///
/// BEHAVIOR:
/// - Maintains array of feed messages
/// - Concatenates all with " *** " separators
/// - Scrolls left over time at configurable speed
/// - Returns current 80-char visible window
/// - Wraps seamlessly at end of full text
/// </summary>

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Ticker : UdonSharpBehaviour
{
    // =================================================================
    // CONFIGURATION
    // =================================================================

    [Header("Feed Configuration")]
    [SerializeField]
    [Tooltip("Default messages for the ticker feed")]
    private string[] feedMessages = new string[]
    {
        "WELCOME TO LOWER LEVEL 2.0                                        ",
        "UPTIME: ETERNAL                                        "
    };

    [SerializeField]
    [Tooltip("Scroll speed in characters per second")]
    private float scrollSpeed = 4.0f;

    // =================================================================
    // INTERNAL STATE
    // =================================================================

    [Header("Display Settings")]
    private int displayWidth = 80;

    private string fullText = "";
    private float scrollOffset = 0f;
    private bool isInitialized = false;

    // =================================================================
    // INITIALIZATION
    // =================================================================

    void Start()
    {
        InitializeTicker();
    }

    private void InitializeTicker()
    {
        if (isInitialized)
            return;

        BuildFullText();
        scrollOffset = 0f;
        isInitialized = true;
    }

    // =================================================================
    // TEXT BUILDING
    // =================================================================

    private void BuildFullText()
    {
        if (feedMessages == null || feedMessages.Length == 0)
        {
            fullText = " *** NO FEED DATA *** ";
            return;
        }

        // Concatenate all messages with separator
        fullText = "";
        for (int i = 0; i < feedMessages.Length; i++)
        {
            if (i > 0)
                fullText = fullText + " *** ";
            fullText = fullText + feedMessages[i];
        }

        // Add trailing separator for seamless wrap
        fullText = fullText + " *** ";
    }

    // =================================================================
    // PUBLIC INTERFACE
    // =================================================================

    /// <summary>
    /// Returns the current 80-character visible window of scrolling text
    /// </summary>
    public string GetVisibleText()
    {
        if (!isInitialized)
            InitializeTicker();

        if (string.IsNullOrEmpty(fullText))
            return new string(' ', displayWidth);

        int fullLength = fullText.Length;
        if (fullLength == 0)
            return new string(' ', displayWidth);

        // Get starting position (wrap around)
        int startPos = Mathf.FloorToInt(scrollOffset) % fullLength;

        // Build visible window
        string result = "";
        for (int i = 0; i < displayWidth; i++)
        {
            int pos = (startPos + i) % fullLength;
            result = result + fullText[pos];
        }

        return result;
    }

    /// <summary>
    /// Adds a temporary message to the ticker feed
    /// </summary>
    public void AddMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg))
            return;

        // Expand array and add message
        int oldLength = feedMessages.Length;
        string[] newMessages = new string[oldLength + 1];

        for (int i = 0; i < oldLength; i++)
        {
            newMessages[i] = feedMessages[i];
        }
        newMessages[oldLength] = msg;

        feedMessages = newMessages;
        BuildFullText();
    }

    /// <summary>
    /// Updates the scroll position - called by DT_Core.Update()
    /// </summary>
    public void UpdateTick(float deltaTime)
    {
        if (!isInitialized)
            InitializeTicker();

        if (string.IsNullOrEmpty(fullText))
            return;

        // Advance scroll position
        scrollOffset = scrollOffset + (scrollSpeed * deltaTime);

        // Wrap around at end of text
        int fullLength = fullText.Length;
        if (fullLength > 0 && scrollOffset >= fullLength)
        {
            scrollOffset = scrollOffset % fullLength;
        }
    }
}
