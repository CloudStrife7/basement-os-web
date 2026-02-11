---
title: "Symbolic Games POC"
date: 2025-11-29
tags: ["terminal", "games", "optimization", "TextMeshPro", "UdonSharp"]
description: "Testing unicode-based games in the terminal — worked, but sprites won on Quest."
---
Experimented with terminal-based games using unicode characters instead of sprites. Built breakout and pong prototypes that render using block characters.

Interesting concept, but after testing found sprites are significantly less resource-intensive on Quest. Keeping the code for reference but won't ship in prod.

[![Symbolic rendering engine working](/images/devlog/legacy/symbolic-rendering-engine.png)](/images/devlog/legacy/symbolic-rendering-engine.png)

[![Symbolic breakout game](/images/devlog/legacy/symbolic-breakout.png)](/images/devlog/legacy/symbolic-breakout.png)
