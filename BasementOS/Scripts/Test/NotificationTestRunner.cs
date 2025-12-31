using UdonSharp;
using UnityEngine;
using LowerLevel.Notifications;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class NotificationTestRunner : UdonSharpBehaviour
{
    [Header("Drag your XboxNotificationUI here")]
    [SerializeField] private XboxNotificationUI notificationUI;

    void Start()
    {
        if (notificationUI == null)
        {
            Debug.LogError("NotificationTestRunner: no XboxNotificationUI assigned!");
            return;
        }

        // Defer the actual queueing until the next frame
        SendCustomEventDelayedFrames(nameof(RunAllTests), 1);
    }

    public void RunAllTests()
    {
        // by this point, notificationUI.InitializeComponent() has run
        notificationUI.QueueAchievementNotification("TestPlayer", "Debug Achievement", 50);
        notificationUI.QueueOnlineNotification("ReturningPlayer", false);
        notificationUI.QueueOnlineNotification("FirstTimePlayer", true);
        notificationUI.QueueSupporterNotification("SupporterPlayer", "Debug Support", 20);
        notificationUI.QueuePwnererNotification("PwnererPlayer", "Debug Pwn", 100);
    }

    // Context‐menu hooks (still work if you prefer manual testing)
    [ContextMenu("Test Achievement")]
    public void TestAchievement()
    {
        if (notificationUI == null) return;
        notificationUI.QueueAchievementNotification("DBG", "Achievements!", 10);
    }

    [ContextMenu("Test Online Returning")]
    public void TestOnlineReturning()
    {
        if (notificationUI == null) return;
        notificationUI.QueueOnlineNotification("DBG_Player", false);
    }

    [ContextMenu("Test Online FirstTime")]
    public void TestOnlineFirstTime()
    {
        if (notificationUI == null) return;
        notificationUI.QueueOnlineNotification("NewPlayer", true);
    }

    [ContextMenu("Test Supporter")]
    public void TestSupporter()
    {
        if (notificationUI == null) return;
        notificationUI.QueueSupporterNotification("SupporterDBG", "Debug Support", 25);
    }

    [ContextMenu("Test Pwnerer")]
    public void TestPwnerer()
    {
        if (notificationUI == null) return;
        notificationUI.QueuePwnererNotification("PwnererDBG", "Debug Pwn", 75);
    }
}
