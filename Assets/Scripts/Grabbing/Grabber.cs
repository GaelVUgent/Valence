using UnityEngine;

/// <summary>
/// Manages grabbing of assembly pieces, by the robot arm or the player.
/// This base implementation uses a custom distance measure to find 
/// candidate pieces for the grabber.
/// This script works in tandem with a pose detector that can trigger the grab changes.
/// </summary>
public class Grabber : MonoBehaviour
{

    private PieceFitter fitter
    {
        get
        {
            if (_fitter == null)
                _fitter = FindObjectOfType<PieceFitter>();
            return _fitter;
        }
    }
    private PieceFitter _fitter;
    public bool canPlace;
    public TrackPadBehaviour trackPadBehaviour;

    public enum TrackPadBehaviour { None, RollRotate, SnapRotate }

    private Grabbable currentPiece;
    private Vector3 pieceOffset;
    private Quaternion pieceRotOffset;
    private int snapRotationIndex;

    private AudioSource sound;
    public AudioClip snapClip, placeClip, grabClip, dropClip;

    private void Awake()
    {
        sound = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Play given audio clip if possible
    /// </summary>
    private void PlayClip(AudioClip clip)
    {
        if (sound != null & clip != null)
            sound.PlayOneShot(clip);
    }

    /// <summary>
    /// Try toggling current grab state
    /// </summary>
    public void ToggleGrab()
    {
        SetGrabState(currentPiece == null);
    }

    /// <summary>
    /// Change grab state and propogate changes. This method 
    /// may be called by a secondary script that is monitoring player input. 
    /// Note that the grab state will not always change
    /// i.e. a grabber can only go to the grab state if a piece is available 
    /// in its detection range.
    /// </summary>
    public void SetGrabState(bool grab)
    {
        if (grab)
            Grab();
        else
            Release();
    }

    /// <summary>
    /// Is there a piece currently being held by this grabber
    /// </summary>
    public bool IsHolding()
    {
        return currentPiece != null;
    }

    /// <summary>
    /// Is the grabber currently holding the given piece
    /// </summary>
    public bool IsHolding(Grabbable piece)
    {
        return piece == currentPiece;
    }

    /// <summary>
    /// Change this grabber to the grab state, but only if a
    /// piece is in range for this type of grabber's detection range.
    /// </summary>
    public bool Grab()
    {
        return Grab(FindPiece());
    }

    /// <summary>
    /// Grab the given piece with this grabber, regardless of its current position.
    /// Note that any other object currently held will be dropped.
    /// </summary>
    public bool Grab(Grabbable piece)
    {
        if (piece == currentPiece)
            return false;
        Release();

        /*
        if(controller != null)
            Valve.VR.OpenVR.System.TriggerHapticPulse((uint)controller.index, 0, (char)0);
         */

        currentPiece = piece;
        if (currentPiece != null)
        {
            if (canPlace)
            {
                PlayClip(grabClip);
                // DataLogger.LogEventStart(ExperimentData.Event.GrabPiece, currentPiece.name);
            }
            /*
            else
                DataLogger.LogEventStart(ExperimentData.Event.RobotGrabPiece, currentPiece.name);
            */
            currentPiece.SetGrabbed(this);
            pieceOffset = transform.worldToLocalMatrix.MultiplyPoint(currentPiece.transform.position);
            pieceRotOffset = Quaternion.Inverse(transform.rotation) * currentPiece.transform.rotation;
            snapRotationIndex = -1;
            fitter.UnlockPiece(currentPiece);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Drop the piece currently held if any, turning it's physics back on.
    /// </summary>
    /// <param name="handOver">Can be used to mark when the piece is being changed to another hand and not dropped in mid-air. This is relevant for events and sound feedback.</param>
    public bool Release(bool handOver = false)
    {
        if (currentPiece != null)
        {
            /*
            if(canPlace)
                DataLogger.LogEventEnd(ExperimentData.Event.GrabPiece);
            else
                DataLogger.LogEventEnd(ExperimentData.Event.RobotGrabPiece);
                */
            currentPiece.SetReleased();
            if (canPlace & !handOver)
            {
                if (fitter.TryFit(currentPiece, true))
                {
                    PlayClip(placeClip);
                }
                else
                {
                    PlayClip(dropClip);
                    // DataLogger.LogEventStart(ExperimentData.Event.DropPiece);
                }

            }
            currentPiece = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handle visual snapping and candidate marking in late update, so 
    /// all transform changes have been applied.
    /// </summary>
    private void LateUpdate()
    {
        if (currentPiece == null & canPlace)
        {
            Grabbable candidate = FindPiece();
            if (candidate != null)
                candidate.Mark(Color.white);
        }
        SnapPiece();
    }

    /// <summary>
    /// Find the best candidate piece to be picked up by this assembly.
    /// This method is seperated out to be overwritten.
    /// </summary>
    protected virtual Grabbable FindPiece()
    {
        if (fitter == null)
            return null;
        return fitter.GetClosestPiece(transform.position);
    }

    /// <summary>
    /// Visuall snap the piece currently held onto the assembly (if possible)
    /// </summary>
    protected void SnapPiece()
    {
        if (currentPiece != null)
        {
            currentPiece.transform.position = transform.localToWorldMatrix.MultiplyPoint(pieceOffset);
            currentPiece.transform.rotation = transform.rotation * pieceRotOffset;
            if (canPlace)
                if (fitter.TryFit(currentPiece, false))
                    PlayClip(snapClip);
        }
    }

    public void TrackPad(Vector2 delta, bool held, bool down)
    {
        if (currentPiece == null)
            return;

        switch (trackPadBehaviour)
        {
            case TrackPadBehaviour.RollRotate:
                if (held)
                {
                    Vector3 roll = new Vector3(delta.y, 0f, -delta.x);
                    Quaternion rot = Quaternion.Euler(Time.deltaTime * 200f * roll);
                    // Quaternion localRot = Quaternion.Inverse(transform.rotation) * rot * transform.rotation;
                    pieceRotOffset = rot * pieceRotOffset;
                }
                break;
            case TrackPadBehaviour.SnapRotate:
                if (down)
                {
                    int intDelta = -1;
                    if (delta.x >= 0f)
                        intDelta = 1;
                    snapRotationIndex += intDelta;

                    //TODO Make integer repeat function in utilities (modulo doesn't handle negatives correctly)
                    if (snapRotationIndex < 0)
                        snapRotationIndex += 4;
                    if (snapRotationIndex > 4)
                        snapRotationIndex -= 4;

                    pieceRotOffset = Quaternion.Euler(0f, 0f, snapRotationIndex * 90f);
                }
                break;
        }
    }
}
