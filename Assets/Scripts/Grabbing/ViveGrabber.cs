using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

/// <summary>
/// Grab pose detecter, using the trigger 
/// input from a vive controller in stead.
/// This is meant primarily for debugging.
/// </summary>
public class ViveGrabber : MonoBehaviour
{

    public SteamVR_Input_Sources inputSource;
    public SteamVR_Action_Boolean triggerAction;
    public SteamVR_Action_Boolean trackPadPress;
    public SteamVR_Action_Vector2 trackPadPosition;

    public GrabToggleEvent onGrabToggle;
    public TrackPadeEvent onTrackPad;

    [Serializable]
    public class GrabToggleEvent : UnityEvent { }

    [Serializable]
    public class TrackPadeEvent : UnityEvent<Vector2, bool, bool> { }

    private void Update()
    {
        if (triggerAction.GetStateDown(inputSource))
            onGrabToggle.Invoke();

        Vector2 trackPadVec = trackPadPosition.GetAxis(inputSource);
        bool trackpadPress = trackPadPress.GetState(inputSource);
        bool trackpadDown = trackPadPress.GetStateDown(inputSource);
        onTrackPad.Invoke(trackPadVec, trackpadPress, trackpadDown);
    }
}
