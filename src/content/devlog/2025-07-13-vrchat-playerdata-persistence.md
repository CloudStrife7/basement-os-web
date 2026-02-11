---
title: "VRChat PlayerData Persistence"
date: 2025-07-13
tags: ["MILESTONE", "persistence", "vrchat", "udonsharp", "VRChat PlayerData API", "UdonSharp"]
type: milestone
description: "11-hour debugging marathon to make visit counts persist — the foundation for everything."
---
Finally got VRChat's [PlayerData API](https://docs.vrchat.com/docs/player-persistence) working! Visit counts now persist between sessions. This took way longer than expected because of UdonSharp's limitations.

Can't use Dictionary or List in UdonSharp, so I'm tracking players with parallel arrays. Not elegant, but it works. Successfully tested with 3 visits - data persists across world reloads.

The 11-hour debugging marathon was worth it. This is the foundation for the entire achievement system.

[![PlayerData persistence working](/images/devlog/legacy/persistence-working.jpg)](/images/devlog/legacy/persistence-working.jpg)

[![Successfully tracking 3 visits](/images/devlog/legacy/tracking-3-visits.png)](/images/devlog/legacy/tracking-3-visits.png)
