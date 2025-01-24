using UnityEngine;

/// <summary>
/// Changes pitch of connected audio source based on the speed at which object rotates, 
/// creating the effect of a mechanical servo.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ServoSound : MonoBehaviour {

    private AudioSource source;
    Quaternion previousRot;
    private float pitchChange;

    public float movementScale = 1f;
    public float smoothing = .15f;
    public float maxChange = 2f;

    private void Awake() {
        source = this.GetComponent<AudioSource>();
    }

    void Start () {
        previousRot = transform.localRotation;
	}
	
	void LateUpdate () {
        float delta = Quaternion.Angle(transform.localRotation, previousRot);
        previousRot = transform.localRotation;
        float pitch = source.pitch;
        float pitchTarget = movementScale * delta;
        source.pitch = Mathf.SmoothDamp(pitch, pitchTarget, ref pitchChange, smoothing, maxChange, Time.deltaTime);
    }
}
