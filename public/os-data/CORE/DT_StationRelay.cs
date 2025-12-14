using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

/// <summary>
/// BASEMENT OS STATION RELAY (v2.1)
///
/// ROLE: VRCStation EVENT FORWARDER
/// Relays OnStationEntered/OnStationExited events from the chair's VRCStation
/// to DT_Core. This is needed because station events only fire on UdonBehaviours
/// attached to the same GameObject as the VRCStation.
///
/// LOCATION: Assets/Scripts/BasementOS/CORE/DT_StationRelay.cs
///
/// SETUP:
/// 1. Attach to the same GameObject as the chair's VRCStation component
/// 2. Assign DT_Core reference
/// 3. Events will automatically relay to DT_Core
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DT_StationRelay : UdonSharpBehaviour
{
    [Header("--- Core Reference ---")]
    [Tooltip("Reference to DT_Core to relay station events")]
    [SerializeField] private UdonSharpBehaviour dtCore;

    // Temporary storage for player reference (used by DT_Core)
    [HideInInspector] public VRCPlayerApi stationPlayer;

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (dtCore == null)
        {
            Debug.LogError("[DT_StationRelay] DT_Core not assigned!");
            return;
        }

        if (player.isLocal)
        {
            Debug.Log("[DT_StationRelay] Local player entered station: " + player.displayName);
            
            // Store player reference and notify DT_Core
            stationPlayer = player;
            dtCore.SetProgramVariable("relayedPlayer", player);
            dtCore.SendCustomEvent("OnTerminalStationEntered");
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (dtCore == null)
        {
            Debug.LogError("[DT_StationRelay] DT_Core not assigned!");
            return;
        }

        if (player.isLocal)
        {
            Debug.Log("[DT_StationRelay] Local player exited station: " + player.displayName);
            
            // Store player reference and notify DT_Core
            stationPlayer = player;
            dtCore.SetProgramVariable("relayedPlayer", player);
            dtCore.SendCustomEvent("OnTerminalStationExited");
        }
    }
}
