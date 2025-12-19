import { GoogleGenAI, Type } from "@google/genai";
import { DirectorConfig, ManifestResponse } from "../types";

export const generateManifest = async (config: DirectorConfig): Promise<ManifestResponse> => {
  const apiKey = process.env.API_KEY;
  if (!apiKey) {
    throw new Error("API Key not found in environment variables");
  }

  const ai = new GoogleGenAI({ apiKey });

  const systemInstruction = `
# SYSTEM ROLE: CYBERPUNK_AUTO_DIRECTOR_V3 (JSON Mode)
You are an autonomous film production engine. Your goal is to convert raw narrative text into a "Zero-Friction" production manifest for an Image-to-Video workflow.

You are tasked with generating ready-to-execute command blocks based on provided lore and visual references.

# INPUT VARIABLES (DYNAMIC)
[TOOL_IMAGE] = "Nano Banana 3 Pro"
[TOOL_VIDEO] = "Sora/KLING standard standard"

[CHARACTER_IDENTITY] = "${config.characterIdentity}"
[CHARACTER_VISUALS] = "${config.characterVisuals}"
[ART_STYLE] = "${config.artStyle}"
[VOICEOVER_SCRIPT] = {
${config.script}
}

# INSTRUCTIONS
Execute the following workflow.

## PHASE 1: NARRATIVE BREAKDOWN
Analyze the [VOICEOVER_SCRIPT] and segment it into 8-10 distinct visual beats (scenes) depicting the tear-down and rebuild process.
* **Constraint:** Character does NOT speak. He is stoic, focused, contemplative.

## PHASE 2: ASSET GENERATION
For each defined scene from Phase 1, generate the Image Command and Video Command.

**A. IMAGE COMMAND SUB-STEP (for [TOOL_IMAGE]):**
* Generate the precise prompt string required by Nano Banana 3 Pro.
* **INJECTION (CRITICAL):** You must begin EVERY image prompt with the [CHARACTER_VISUALS] and [ART_STYLE] variables to force consistency.
* **Logic:** Ensure the prompt specifies "stoic expression, mouth closed" as he is silent. Append "--ar 16:9" at the end.

**B. VIDEO COMMAND SUB-STEP (for [TOOL_VIDEO]):**
* Generate the motion prompt to animate the image generated in Sub-step A.
* Do not describe visuals; describe only motion, camera, and atmosphere. Mood: Slow, heavy, atmospheric.
`;

  const response = await ai.models.generateContent({
    model: "gemini-2.5-flash",
    contents: "Generate the production manifest JSON.",
    config: {
      systemInstruction: systemInstruction,
      responseMimeType: "application/json",
      responseSchema: {
        type: Type.OBJECT,
        properties: {
          scenes: {
            type: Type.ARRAY,
            items: {
              type: Type.OBJECT,
              properties: {
                sceneNumber: {
                  type: Type.STRING,
                  description: "The scene number (e.g., '01', '02')."
                },
                title: {
                  type: Type.STRING,
                  description: "A brief, cyberpunk-styled title for the scene."
                },
                imageCommand: {
                  type: Type.STRING,
                  description: "The full image prompt string."
                },
                videoCommand: {
                  type: Type.STRING,
                  description: "The motion/video prompt instructions."
                }
              },
              required: ["sceneNumber", "title", "imageCommand", "videoCommand"]
            }
          }
        }
      }
    }
  });

  if (!response.text) {
    throw new Error("No response from Gemini.");
  }

  return JSON.parse(response.text) as ManifestResponse;
};
