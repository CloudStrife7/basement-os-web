using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace LowerLevel.Achievements
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AchievementDebugTrigger : UdonSharpBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the AchievementTracker script (UdonBehaviour)")]
        public UdonBehaviour achievementTrackerUdon;

        [Tooltip("Name of the method to trigger on AchievementTracker")]
        public string methodNameToCall = "BulletproofComprehensiveAllAchievementsTest";

        public override void Interact()
        {
            if (achievementTrackerUdon == null)
            {
                Debug.LogError("❌ AchievementDebugTrigger: No AchievementTracker reference assigned.");
                return;
            }

            achievementTrackerUdon.SendCustomEvent(methodNameToCall);
        }
    }
}
