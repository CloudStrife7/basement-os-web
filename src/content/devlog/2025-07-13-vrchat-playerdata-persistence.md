---title: "VRChat PlayerData Persistence"
date: 2025-07-13
tags: ["MILESTONE", "persistence", "vrchat", "udonsharp", "VRChat PlayerData API", "UdonSharp"]
type: milestone
---
Finally got VRChat's [PlayerData API](https://docs.vrchat.com/docs/player-persistence) working! Visit counts now persist between sessions. This took way longer than expected because of UdonSharp's limitations.

Can't use Dictionary or List in UdonSharp, so I'm tracking players with parallel arrays. Not elegant, but it works. Successfully tested with 3 visits - data persists across world reloads.

The 11-hour debugging marathon was worth it. This is the foundation for the entire achievement system.

[![PlayerData persistence working](/Manual Change Logs and Images/images/July 2025/Persistence Working.jpg)](/Manual Change Logs and Images/images/July 2025/Persistence Working.jpg)

[![Successfully tracking 3 visits](/Manual Change Logs and Images/images/July 2025/tracking 3 visits.png)](/Manual Change Logs and Images/images/July 2025/tracking 3 visits.png)
