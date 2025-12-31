using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

/// <summary>
/// MCP AUTOMATION TEST SCRIPT
/// Tests the full automation pipeline: create script -> generate asset -> compile -> play
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MCPAutomationTest : UdonSharpBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private string testMessage = "MCP Automation Test Successful!";
    [SerializeField] private int testValue = 42;

    private bool hasStarted = false;

    void Start()
    {
        hasStarted = true;
        Debug.Log("[MCPAutomationTest] Script loaded successfully!");
        Debug.Log("[MCPAutomationTest] Message: " + testMessage);
        Debug.Log("[MCPAutomationTest] Value: " + testValue.ToString());
    }

    public override void Interact()
    {
        Debug.Log("[MCPAutomationTest] Interact triggered!");
        Debug.Log("[MCPAutomationTest] hasStarted = " + hasStarted.ToString());
    }

    public void RunTest()
    {
        Debug.Log("[MCPAutomationTest] RunTest called externally!");
        Debug.Log("[MCPAutomationTest] Test Value doubled = " + (testValue * 2).ToString());
    }
}
