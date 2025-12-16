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
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DT_StationRelay : UdonSharpBehaviour
{
    [Header("--- Core Reference ---")]
    [Tooltip("Reference to DT_Core to relay station events")]
    [SerializeField] private UdonSharpBehaviour dtCore;

    // Temporary storage for player reference (used by DT_Core)
    [HideInInspector] public VRCPlayerApi stationPlayer;

    // Input debounce to prevent repeated triggers
    private const float INPUT_DEBOUNCE = 0.15f;
    private float lastVerticalTime = 0f;
    private float lastHorizontalTime = 0f;
    
    // Debounce for InputUse after station entry (click to sit also fires InputUse)
    private const float STATION_ENTRY_COOLDOWN = 0.5f;
    private float stationEntryTime = 0f;

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (dtCore == null)
        {
            Debug.LogError("[DT_StationRelay] DT_Core not assigned!");
            return;
        }

        if (player.isLocal)
        {
            // Record entry time for InputUse debounce (click to sit also fires InputUse)
            stationEntryTime = Time.time;
            
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

            // Store player reference and notify DT_Core
            stationPlayer = player;
            dtCore.SetProgramVariable("relayedPlayer", player);
            dtCore.SendCustomEvent("OnTerminalStationExited");
        }
    }

    // =================================================================
    // INPUT EVENT RELAY (Forward station input to DT_Core)
    // =================================================================

    public override void InputMoveVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (dtCore == null) return;

        // Debounce: prevent rapid repeated triggers
        if (Time.time - lastVerticalTime < INPUT_DEBOUNCE) return;

        Debug.Log("[DT_StationRelay] InputMoveVertical: value=" + value);

        // Convert analog value to directional input
        if (value > 0.5f)
        {
            dtCore.SetProgramVariable("relayedInputKey", "UP");
            dtCore.SendCustomEvent("OnRelayedInput");
            lastVerticalTime = Time.time;
        }
        else if (value < -0.5f)
        {
            dtCore.SetProgramVariable("relayedInputKey", "DOWN");
            dtCore.SendCustomEvent("OnRelayedInput");
            lastVerticalTime = Time.time;
        }
    }

    public override void InputMoveHorizontal(float value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (dtCore == null) return;

        // Debounce: prevent rapid repeated triggers
        if (Time.time - lastHorizontalTime < INPUT_DEBOUNCE) return;

        Debug.Log("[DT_StationRelay] InputMoveHorizontal: value=" + value);

        // Convert analog value to directional input
        if (value > 0.5f)
        {
            dtCore.SetProgramVariable("relayedInputKey", "RIGHT");
            dtCore.SendCustomEvent("OnRelayedInput");
            lastHorizontalTime = Time.time;
        }
        else if (value < -0.5f)
        {
            dtCore.SetProgramVariable("relayedInputKey", "LEFT");
            dtCore.SendCustomEvent("OnRelayedInput");
            lastHorizontalTime = Time.time;
        }
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (!value) return; // Press only

        if (dtCore == null) return;

        // Ignore InputUse immediately after station entry (the click to sit also fires InputUse)
        if (Time.time - stationEntryTime < STATION_ENTRY_COOLDOWN)
        {
            Debug.Log("[DT_StationRelay] InputUse ignored (within station entry cooldown)");
            return;
        }

        Debug.Log("[DT_StationRelay] InputUse pressed");

        dtCore.SetProgramVariable("relayedInputKey", "ACCEPT");
        dtCore.SendCustomEvent("OnRelayedInput");
    }

    public override void InputJump(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (!value) return; // Press only

        if (dtCore == null) return;

        Debug.Log("[DT_StationRelay] InputJump pressed (Spacebar exit)");

        // Spacebar exit is handled by DT_Core's InputJump override
        // Just relay the event
        dtCore.SendCustomEvent("OnRelayedInputJump");
    }
}
