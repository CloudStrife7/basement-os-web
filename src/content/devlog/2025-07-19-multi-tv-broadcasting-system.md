---title: "Multi-TV Broadcasting System"
date: 2025-07-19
tags: ["notifications", "ui", "networking", "UdonSharp", "ProTV"]
---
Got notifications working on all 3 TVs simultaneously! The NotificationEventHub broadcasts to each display independently, so everyone in the basement sees achievements pop regardless of which room they're in.

Each TV maintains its own FIFO queue and animation timing. Had to be careful with the ProTV prefab integration - it uses a different Canvas setup than standard UI.

[![Notifications working on all 3 TVs](/Manual Change Logs and Images/images/July 2025/Notifications working on 3 Tvs!!!.jpg)](/Manual Change Logs and Images/images/July 2025/Notifications working on 3 Tvs!!!.jpg)

[![Multi-TV setup in basement](/Manual Change Logs and Images/images/July 2025/Multiple TVs.jpg)](/Manual Change Logs and Images/images/July 2025/Multiple TVs.jpg)
