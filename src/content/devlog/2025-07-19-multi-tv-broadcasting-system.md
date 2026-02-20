---
title: "Multi-TV Broadcasting System"
date: 2025-07-19
tags: ["notifications", "ui", "networking", "UdonSharp"]
description: "Achievement notifications on all 3 TVs simultaneously via NotificationEventHub."
---
Got notifications working on all 3 TVs simultaneously! The NotificationEventHub acts as the central orchestrator, using UdonSynced variables to broadcast achievement and login notifications to all players in the world.

The system works by having the master player own the NotificationEventHub and broadcast via `RequestSerialization()`. When a notification fires, `OnDeserialization()` triggers on all clients, which then forwards the notification to a primary display plus any additional displays configured in the array. Each TV has its own XboxNotificationUI component that receives the event and handles the fade animation and sound independently.

This means everyone in the basement sees achievements pop regardless of which room they're in, and all displays stay in sync across the network.

[![Notifications working on all 3 TVs](/images/devlog/legacy/notifications-3-tvs.jpg)](/images/devlog/legacy/notifications-3-tvs.jpg)

[![Multi-TV setup in basement](/images/devlog/legacy/multiple-tvs.jpg)](/images/devlog/legacy/multiple-tvs.jpg)
