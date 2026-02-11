---
title: "Terminal Menu System Complete"
date: 2025-11-15
tags: ["terminal", "input", "weather", "VRCStation", "UdonSharp", "GitHub Pages API"]
description: "Turning the terminal from an auto-cycling display into an interactive DOS-style menu."
---
In a development instance I ran a POC that replaces the original auto-cycling display with an actual interactive menu. Players could use their movement controls to navigate up/down through options and select with the interact button.

Had to implement player immobilization when they're at the terminal - otherwise pressing up/down would move your avatar AND the cursor. Using [VRCStation](https://docs.vrchat.com/docs/vrcstation) for this also solves the "walking away mid-interaction" problem.

Also extracted the weather module into its own script. The terminal now pulls real-time weather data from my [GitHub Pages](https://pages.github.com/) endpoint and displays it in the header. When it's actually raining in Fond du Lac, you'll see rain in the basement too once I figure out how to re-bake the lighting with the shader enabled windows.
