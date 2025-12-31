/// COMPONENT PURPOSE:
/// Sends external notifications with friend/visitor detection for basement activity
/// Distinguishes between known friends and new visitors joining the space
/// Creates social awareness for remote monitoring of basement community
/// 
/// LOWER LEVEL 2.0 INTEGRATION:
/// Perfect for knowing if it's your usual crew or new people to meet
/// Builds social anticipation - "2 friends + 1 visitor in basement!"
/// Creates Xbox Live-style friend presence with stranger awareness
/// 
/// DEPENDENCIES & REQUIREMENTS:
/// - GameObject with UdonSharp component attached
/// - VRCUrl set in inspector pointing to webhook endpoint (GitHub Pages recommended)
/// - Friends list configured with exact VRChat display names
/// - Setup: Place on persistent GameObject in scene
/// - Assets: None required
/// 
/// ARCHITECTURE PATTERN:
/// Event-driven notification system with fire-and-forget webhook calls
/// Uses VRCStringDownloader without callbacks to avoid UdonSharp casting issues
/// Implements friend vs visitor categorization for enhanced social awareness
/// Webhook endpoint counts requests to track basement activity

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

public class BasementSocialNotifier : UdonSharpBehaviour
{
    [Header("External Notification Settings")]
    [Tooltip("Set this VRCUrl in the inspector to your webhook endpoint (GitHub Pages recommended)")]
    public VRCUrl webhookVRCUrl;

    [Tooltip("Unique identifier for this basement instance")]
    public string basementID = "lowerlevel2_main";

    [Header("Friend Detection")]
    [Tooltip("Add your VRChat friends' display names here (case sensitive)")]
    public string[] friendsList = {
        "GameFuel",
        "∗Lexx∗",
        "Onawarren",
        "M0J170",
        "Joe․xino b349",
        "p_dreikosen 437e"
    };

    private int currentPlayerCount = 0;
    private int friendCount = 0;
    private int visitorCount = 0;
    private Component udonReceiver;

    void Start()
    {
        // Store a reference to this component for callback purposes
        udonReceiver = GetComponent("UdonBehaviour");

        if (webhookVRCUrl == null || string.IsNullOrEmpty(webhookVRCUrl.Get()))
        {
            Debug.LogError("[Basement] ERROR: webhookVRCUrl not set in inspector! External notifications disabled.");
            return;
        }

        currentPlayerCount = VRCPlayerApi.GetPlayerCount();
        AnalyzeBasementSocial();
        SendActivityUpdate();

        Debug.Log($"[Basement] Initialized - Friends: {friendCount}, Visitors: {visitorCount}");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == null || !player.IsValid())
        {
            Debug.LogWarning("[Basement] Invalid player in OnPlayerJoined");
            return;
        }

        currentPlayerCount = VRCPlayerApi.GetPlayerCount();
        AnalyzeBasementSocial();

        bool isFriend = IsPlayerFriend(player.displayName);
        string playerType = isFriend ? "friend" : "visitor";

        SendActivityUpdate();

        Debug.Log($"[Basement] {playerType} joined: {player.displayName} | Friends: {friendCount}, Visitors: {visitorCount}");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player == null || !player.IsValid())
        {
            Debug.LogWarning("[Basement] Invalid player in OnPlayerLeft");
            return;
        }

        bool isFriend = IsPlayerFriend(player.displayName);
        string playerType = isFriend ? "friend" : "visitor";

        currentPlayerCount = VRCPlayerApi.GetPlayerCount();
        AnalyzeBasementSocial();

        SendActivityUpdate();

        Debug.Log($"[Basement] {playerType} left: {player.displayName} | Friends: {friendCount}, Visitors: {visitorCount}");
    }

    private void AnalyzeBasementSocial()
    {
        friendCount = 0;
        visitorCount = 0;

        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(allPlayers);

        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i] != null && allPlayers[i].IsValid())
            {
                if (IsPlayerFriend(allPlayers[i].displayName))
                {
                    friendCount++;
                }
                else
                {
                    visitorCount++;
                }
            }
        }
    }

    private bool IsPlayerFriend(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return false;

        for (int i = 0; i < friendsList.Length; i++)
        {
            if (friendsList[i] == playerName)
            {
                return true;
            }
        }
        return false;
    }

    private void SendActivityUpdate()
    {
        if (webhookVRCUrl == null || string.IsNullOrEmpty(webhookVRCUrl.Get()))
        {
            Debug.LogError("[Basement] Cannot send notification - VRCUrl not configured");
            return;
        }

        if (udonReceiver == null)
        {
            Debug.LogError("[Basement] Udon receiver not initialized");
            return;
        }

        // Use the stored component reference for callback
        VRCStringDownloader.LoadUrl(webhookVRCUrl, (IUdonEventReceiver)udonReceiver);
        Debug.Log("[Basement] Webhook request sent to ping endpoint");
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        Debug.Log("[Basement] Social activity notification sent successfully!");
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        // FIXED: Replace ?. with explicit null check (UdonSharp doesn't support ?.)
        string errorMessage = result != null ? result.ErrorCode.ToString() : "Unknown error";
        Debug.LogError($"[Basement] Failed to send notification: {errorMessage}");
    }

    [ContextMenu("Test Notification")]
    public void TestNotification()
    {
        AnalyzeBasementSocial();
        SendActivityUpdate();
        Debug.Log($"[Basement] Manual test - Friends: {friendCount}, Visitors: {visitorCount}");
    }
}