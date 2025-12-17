using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BASEMENT OS TERMINAL TALES (v2.1)
///
/// ROLE: INTERACTIVE FICTION / TEXT ADVENTURE ENGINE
/// Explores Lower Level lore through text-based adventures.
///
/// LOCATION: Assets/Scripts/BasementOS/BIN/DT_App_Tales.cs
///
/// INTEGRATION:
/// - Spoke: Receives input from DT_Core
/// - Core: Returns to shell on exit
///
/// LIMITATIONS:
/// - Max 350 Lines
/// - No properties, LINQ, string interpolation, try/catch, foreach
/// - Event-driven only (no Update())
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_App_Tales : UdonSharpBehaviour
{
    // =================================================================
    // APP INTERFACE (Required by DT_Core)
    // =================================================================

    [HideInInspector] public string inputKey = "";

    // =================================================================
    // REFERENCES
    // =================================================================

    [Header("--- Core Reference ---")]
    [SerializeField] private UdonSharpBehaviour coreReference;

    [Tooltip("Shell app to return to")]
    [SerializeField] private UdonSharpBehaviour shellApp;

    // =================================================================
    // STORY DATA
    // =================================================================

    [Header("--- Story Content ---")]
    [TextArea(3, 10)]
    [SerializeField] private string[] storyTexts = new string[]
    {
        "You find yourself in a dimly lit basement. Old computers hum quietly. A CRT monitor glows green in the corner.",
        "You open the fridge. Three expired energy drinks, a fossilized burger, and... a USB drive taped to the back wall.",
        "The arcade cabinet displays 'TERMINAL TALES - INSERT COIN'. A sticky note reads: 'The stories are real. -MT'",
        "The terminal shows:\n> BASEMENTOS v2.0 LOADED\n> WARNING: REALITY ANCHOR UNSTABLE\n\n'Every game tells a story.'",
        "Files cascade across the screen. 'If you're reading this, you've gone too deep...'\n\nThe USB drive sparks and dies.",
        "You head back upstairs. The door creaks shut.\n\n[STORY ENDED]"
    };

    [SerializeField] private string[] choiceATexts = new string[] { "Check the Fridge", "Take the USB Drive", "Insert a Floppy", "Read Warnings Again", "Examine Files", "Return to Basement" };
    [SerializeField] private string[] choiceBTexts = new string[] { "Inspect Arcade Machine", "Leave it Alone", "Read Sticky Note", "Check Logs", "Go Back Upstairs", "" };
    [SerializeField] private string[] choiceCTexts = new string[] { "Read Terminal Screen", "Go Back", "Go Back", "Go Back", "", "" };
    [SerializeField] private string[] choiceDTexts = new string[] { "Go Back Upstairs", "", "", "", "", "" };

    [SerializeField] private int[] choiceATargets = new int[] { 1, 4, 2, 3, 4, 0 };
    [SerializeField] private int[] choiceBTargets = new int[] { 2, 0, 3, 3, 5, -1 };
    [SerializeField] private int[] choiceCTargets = new int[] { 3, 0, 0, 0, -1, -1 };
    [SerializeField] private int[] choiceDTargets = new int[] { 5, -1, -1, -1, -1, -1 };

    [SerializeField] private int[] choiceCounts = new int[] { 4, 3, 3, 3, 2, 1 };

    // =================================================================
    // STATE
    // =================================================================

    private int currentNode = 0;
    private int selectedChoice = 0;
    private bool isInitialized = false;

    // =================================================================
    // APP LIFECYCLE
    // =================================================================

    public void OnAppOpen()
    {
        currentNode = 0;
        selectedChoice = 0;
        isInitialized = true;
        PushDisplayToCore();
    }

    public void OnAppClose()
    {
        isInitialized = false;
    }

    // =================================================================
    // INPUT HANDLING
    // =================================================================

    public void OnInput()
    {
        if (!isInitialized) return;

        if (inputKey == "UP")
        {
            selectedChoice--;
            int count = GetChoiceCount(currentNode);
            if (selectedChoice < 0) selectedChoice = count - 1;
            PushDisplayToCore();
        }
        else if (inputKey == "DOWN")
        {
            selectedChoice++;
            int count = GetChoiceCount(currentNode);
            if (selectedChoice >= count) selectedChoice = 0;
            PushDisplayToCore();
        }
        else if (inputKey == "ACCEPT" || inputKey == "RIGHT")
        {
            int targetNode = GetChoiceTarget(currentNode, selectedChoice);
            if (targetNode == -1)
            {
                ReturnToShell();
            }
            else if (targetNode >= 0 && targetNode < storyTexts.Length)
            {
                currentNode = targetNode;
                selectedChoice = 0;
                PushDisplayToCore();
            }
        }
        else if (inputKey == "LEFT")
        {
            ReturnToShell();
        }

        inputKey = "";
    }

    private void ReturnToShell()
    {
        if (Utilities.IsValid(coreReference) && Utilities.IsValid(shellApp))
        {
            coreReference.SetProgramVariable("nextProcess", shellApp);
            coreReference.SendCustomEvent("LoadNextProcess");
        }
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
    // DISPLAY RENDERING
    // =================================================================

    public string GetDisplayContent()
    {
        if (!isInitialized) return "TERMINAL TALES - NOT INITIALIZED";
        if (currentNode < 0 || currentNode >= storyTexts.Length) return "ERROR: INVALID STORY NODE";

        string output = "";

        output = output + " *** STORY LOADED: \"The Basement\" (Chapter 1) ***\n\n";
        output = output + WrapText(storyTexts[currentNode], 78) + "\n\n";
        output = output + " WHAT DO YOU DO?\n\n";

        int choiceCount = GetChoiceCount(currentNode);

        for (int i = 0; i < choiceCount; i++)
        {
            string choiceText = GetChoiceText(currentNode, i);
            string choiceLabel = GetChoiceLabel(i);

            if (i == selectedChoice)
            {
                output = output + " > [" + choiceLabel + "] " + choiceText + "\n";
            }
            else
            {
                output = output + "   [" + choiceLabel + "] " + choiceText + "\n";
            }
        }

        return output;
    }

    // =================================================================
    // HELPERS
    // =================================================================

    private int GetChoiceCount(int nodeIndex)
    {
        if (choiceCounts != null && nodeIndex >= 0 && nodeIndex < choiceCounts.Length)
        {
            return choiceCounts[nodeIndex];
        }
        return 0;
    }

    private string GetChoiceText(int nodeIndex, int choiceIndex)
    {
        if (choiceIndex == 0 && choiceATexts != null && nodeIndex < choiceATexts.Length) return choiceATexts[nodeIndex];
        if (choiceIndex == 1 && choiceBTexts != null && nodeIndex < choiceBTexts.Length) return choiceBTexts[nodeIndex];
        if (choiceIndex == 2 && choiceCTexts != null && nodeIndex < choiceCTexts.Length) return choiceCTexts[nodeIndex];
        if (choiceIndex == 3 && choiceDTexts != null && nodeIndex < choiceDTexts.Length) return choiceDTexts[nodeIndex];
        return "ERROR";
    }

    private int GetChoiceTarget(int nodeIndex, int choiceIndex)
    {
        if (choiceIndex == 0 && choiceATargets != null && nodeIndex < choiceATargets.Length) return choiceATargets[nodeIndex];
        if (choiceIndex == 1 && choiceBTargets != null && nodeIndex < choiceBTargets.Length) return choiceBTargets[nodeIndex];
        if (choiceIndex == 2 && choiceCTargets != null && nodeIndex < choiceCTargets.Length) return choiceCTargets[nodeIndex];
        if (choiceIndex == 3 && choiceDTargets != null && nodeIndex < choiceDTargets.Length) return choiceDTargets[nodeIndex];
        return -1;
    }

    private string GetChoiceLabel(int choiceIndex)
    {
        if (choiceIndex == 0) return "A";
        if (choiceIndex == 1) return "B";
        if (choiceIndex == 2) return "C";
        if (choiceIndex == 3) return "D";
        return "?";
    }

    private string WrapText(string text, int maxWidth)
    {
        if (text.Length <= maxWidth) return DT_Format.EnforceLine80(" " + text);

        string result = "";
        string[] paragraphs = text.Split('\n');

        for (int p = 0; p < paragraphs.Length; p++)
        {
            string paragraph = paragraphs[p];
            string[] words = paragraph.Split(' ');
            string currentLine = " ";

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (currentLine.Length + word.Length + 1 <= maxWidth)
                {
                    if (currentLine == " ") currentLine = currentLine + word;
                    else currentLine = currentLine + " " + word;
                }
                else
                {
                    // Pad line to 80 chars before adding newline
                    result = result + DT_Format.EnforceLine80(currentLine) + "\n";
                    currentLine = " " + word;
                }
            }

            // Pad final line to 80 chars
            if (currentLine != " ") result = result + DT_Format.EnforceLine80(currentLine);
            if (p < paragraphs.Length - 1) result = result + "\n";
        }

        return result;
    }
}
