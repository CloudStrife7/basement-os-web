---title: "Achievement System Overhaul"
date: 2025-10-20
tags: ["MILESTONE", "achievements", "persistence", "xbox", "VRChat PlayerData API", "UdonSharp", "TextMeshPro"]
type: milestone
---
Finally finished the Xbox 360-style achievement system! 19 achievements worth 420G implemented out of 1,000G leaves plenty of room for expansion with future ideas. This matches the original Xbox 360 gamer score point structure. The notifications pop up just like they did on the 360 - that satisfying sound effect and the animated banner!

Using VRChat's [PlayerData API](https://docs.vrchat.com/docs/player-persistence) for persistence. This was tricky because you can't use fancy C# features in UdonSharp - no `List<T>`, no Dictionary, no LINQ. Everything's done with arrays and careful indexing.

The FIFO queue for notifications took a few iterations. Originally had a priority system but it felt weird when achievements popped up out of order. The chronological approach matches the "basement live feed" vibe I was going for.
