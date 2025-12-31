using UdonSharp;
using TMPro;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SymbolicGraphics.Examples
{
    /// <summary>
    /// UdonSharp-compatible DOS Terminal Symbolic Rendering POC
    /// Demonstrates 90-character unicode rendering for VRChat
    /// </summary>
    public class DOSTerminalSymbolicPOC : UdonSharpBehaviour
    {
        [Header("Display")]
        public TextMeshProUGUI symbolicDisplay;

        [Header("Settings")]
        public int canvasWidth = 90;
        public int canvasHeight = 30;

        private void Start()
        {
            if (symbolicDisplay == null)
            {
                Debug.LogError("[DOSTerminalSymbolicPOC] symbolicDisplay is null! Assign TextMeshProUGUI component.");
                return;
            }

            // Rich text must be enabled manually in Unity Inspector
            // symbolicDisplay.richText is not exposed to Udon

            RenderDOSMenu();
        }

        private void RenderDOSMenu()
        {
            // Simple test: render colored blocks like Space Invaders / Mario pixel art
            string output = "";

            // Test colored full blocks (█)
            output += "<color=#FF0000>█████</color> <color=#00FF00>█████</color> <color=#0000FF>█████</color>\n";
            output += "<color=#FFFF00>█████</color> <color=#FF00FF>█████</color> <color=#00FFFF>█████</color>\n";
            output += "\n";
            output += "90-CHARACTER SYMBOLIC RENDERING POC\n";
            output += "If you see colored blocks above, it works!\n";

            symbolicDisplay.text = output;
        }

        /// <summary>
        /// Build a line of repeated characters with color
        /// </summary>
        private string BuildColoredLine(char character, int length, string hexColor)
        {
            string line = "<color=" + hexColor + ">";
            for (int i = 0; i < length; i++)
            {
                line += character;
            }
            line += "</color>";
            return line;
        }

        /// <summary>
        /// Build a colored block of full block characters
        /// </summary>
        private string BuildColoredBlock(int length, string hexColor)
        {
            string block = "<color=" + hexColor + ">";
            for (int i = 0; i < length; i++)
            {
                block += '█'; // Full block (U+2588)
            }
            block += "</color>";
            return block;
        }

        /// <summary>
        /// Center text within a given width (accounts for rich text tags)
        /// </summary>
        private string CenterText(string text, int width)
        {
            // Strip rich text tags to get actual text length
            string plainText = StripRichTextTags(text);
            int textLength = plainText.Length;

            if (textLength >= width)
            {
                return text; // Can't center, already too wide
            }

            int padding = (width - textLength) / 2;
            string line = "";
            for (int i = 0; i < padding; i++)
            {
                line += " ";
            }
            line += text;
            return line;
        }

        /// <summary>
        /// Strip rich text tags from string to get actual displayed length
        /// Simplified - handles basic color tags
        /// </summary>
        private string StripRichTextTags(string text)
        {
            string result = "";
            bool inTag = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '<')
                {
                    inTag = true;
                }
                else if (c == '>')
                {
                    inTag = false;
                }
                else if (!inTag)
                {
                    result += c;
                }
            }

            return result;
        }
    }
}