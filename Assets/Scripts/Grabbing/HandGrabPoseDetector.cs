using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects pose of a manus glove and judges whether the hand 
/// is closed or not. This script works in tandem with a 
/// (probe)grabber to grab assembly pieces.
/// The hand is considered 'grabbing' if either the fingers are rolled 
/// into a fist or the thumb and index finger are squeezed together.
/// </summary>
public class HandGrabPoseDetector : MonoBehaviour {
    
    public float thumbIndexDist = 0.1f;

    public Transform handball;
    public FingerProbe thumb, index, middle;

    public GrabPoseChangeEvent onGrabPoseChange;

    [System.Serializable]
    public class GrabPoseChangeEvent : UnityEvent<bool> { }

    private bool lastGrab;

    private void Start() {
        lastGrab = CheckGrabByPose();
    }

    private void Update() {
        bool newGrab = CheckGrabByPose();
        if(lastGrab != newGrab) {
            lastGrab = newGrab;
            onGrabPoseChange.Invoke(newGrab);
        }
    }

    private bool CheckGrabByPose() {
        float tid = (thumb.transform.position - index.transform.position).magnitude;
        float iff = GetForwardFactor(index.transform);
        float mff = GetForwardFactor(middle.transform);
        float ff = Mathf.Min(iff, mff);
        return ff < 0f | tid < thumbIndexDist;
    }

    private float GetForwardFactor(Transform t) {
        Vector3 relPos = t.position - handball.position;
        return Vector3.Dot(relPos, handball.forward);
    }
}
