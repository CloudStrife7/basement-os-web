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

    // =================================================================
    // STORY DATA
    // =================================================================

    [Header("--- Story Content ---")]
    [TextArea(3, 10)]
    [SerializeField] private string[] storyTexts = new string[]
    {
        "You find yourself in a dimly lit basement. The hum of old computers fills the air. A single CRT monitor glows in the corner, casting eerie green light across dusty pizza boxes.\n\nThe pizza box on the table is empty. A faint smell of pepperoni lingers in the stale air.",
        "You pull open the ancient refrigerator. The light flickers on, revealing: three cans of expired energy drinks, a half-eaten burger from 2019, and a mysterious Tupperware container.\n\nWait... there's a USB drive taped to the back wall. Someone didn't want this found.",
        "You approach the vintage arcade cabinet. The screen displays 'TERMINAL TALES - INSERT COIN'. But this isn't a normal arcade machine. The joystick has been replaced with a keyboard.\n\nA sticky note reads: 'The stories are real. They're warnings. -MT'",
        "The terminal displays scrolling text:\n\n> BASEMENTOS v2.0 KERNEL LOADED\n> WARNING: REALITY ANCHOR UNSTABLE\n> FICTION BOUNDARY: 47% INTEGRITY\n\nA message blinks: 'Every game tells a story. Every story hides a truth.'",
        "You plug the USB drive into the terminal. Files cascade across the screen - stories, logs, memories.\n\n'If you're reading this, you've gone too deep. The basement isn't just a place. It's a collection of moments, frozen in time.'\n\nThe drive corrupts and dies with a spark.",
        "You turn away and head back upstairs. The basement door creaks shut behind you.\n\nBut you can't shake the feeling that the stories down there are trying to tell you something...\n\n[STORY ENDED]"
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
        }
        else if (inputKey == "DOWN")
        {
            selectedChoice++;
            int count = GetChoiceCount(currentNode);
            if (selectedChoice >= count) selectedChoice = 0;
        }
        else if (inputKey == "ACCEPT")
        {
            int targetNode = GetChoiceTarget(currentNode, selectedChoice);
            if (targetNode == -1)
            {
                if (Utilities.IsValid(coreReference))
                {
                    coreReference.SendCustomEvent("ReturnToShell");
                }
            }
            else if (targetNode >= 0 && targetNode < storyTexts.Length)
            {
                currentNode = targetNode;
                selectedChoice = 0;
            }
        }
        else if (inputKey == "LEFT")
        {
            if (Utilities.IsValid(coreReference))
            {
                coreReference.SendCustomEvent("ReturnToShell");
            }
        }

        inputKey = "";
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
        output = output + WrapText(storyTexts[currentNode], 70) + "\n\n";
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
        if (text.Length <= maxWidth) return " " + text;

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
                    result = result + currentLine + "\n";
                    currentLine = " " + word;
                }
            }

            if (currentLine != " ") result = result + currentLine;
            if (p < paragraphs.Length - 1) result = result + "\n";
        }

        return result;
    }
}
