using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

/// <summary>
/// BASEMENT OS INTERACTION HANDLER (v2.1)
///
/// ROLE: PLAYER SEATING TRIGGER
/// When player clicks on the terminal, this script seats them in the
/// designated VRCStation (chair), which triggers DT_Core input capture.
///
/// LOCATION: Assets/Scripts/BasementOS/CORE/DT_Interact.cs
///
/// SETUP:
/// 1. Attach to a GameObject with a Collider (trigger)
/// 2. Assign the chair's VRCStation to terminalStation
/// 3. When player interacts, they'll be seated and DT_Core will capture input
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_Interact : UdonSharpBehaviour
{
    [Header("--- Station Reference ---")]
    [Tooltip("The VRCStation (chair) to seat the player in")]
    [SerializeField] private VRC.SDK3.Components.VRCStation terminalStation;

    [Header("--- Optional References ---")]
    [Tooltip("Optional: Reference to DT_Core for direct notification")]
    [SerializeField] private UdonSharpBehaviour dtCore;

    /// <summary>
    /// Called when player interacts with this object (click/trigger)
    /// </summary>
    public override void Interact()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        
        if (player == null)
        {
            Debug.LogWarning("[DT_Interact] No local player found");
            return;
        }

        if (terminalStation == null)
        {
            Debug.LogError("[DT_Interact] Terminal station not assigned!");
            return;
        }

        // Seat the player in the terminal chair
        terminalStation.UseStation(player);
        
    }
}
