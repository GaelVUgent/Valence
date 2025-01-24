using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class for managing the flow of an assembly.
/// Implementations exist for experiment and practice scene.
/// </summary>
public class PieceFitter : MonoBehaviour
{

    public Transform pieceHolder;
    public Transform fitHolder;

    //connections
    protected Grabbable[] pieces;

    public float grabDistance = 0.1f;
    public float snapDistance = 0.05f;
    public float snapAngle = 20f;
    public float snapHysteresis = 0.02f;
    public float checkDistance = 0.03f;
    public float checkAngle = 10f;

    //task state
    protected int nextPieceIndex;
    protected List<AttachNode> availableNodes;

    //confirmation buffer
    protected PieceAttach nextAttach;

    protected bool Done => nextPieceIndex >= pieces.Length;

    protected virtual void Awake()
    {
        pieces = pieceHolder.GetComponentsInChildren<Grabbable>();
        availableNodes = new List<AttachNode>(fitHolder.GetComponentsInChildren<AttachNode>());
        if (Global.global == null)
        {
            Debug.LogError("Please start in the main menu scene so that we have access to the global script.");
        }
    }

    protected virtual void Start()
    {
        ResetPieces();
    }

    public virtual void ResetPieces()
    {
        nextPieceIndex = 0;
        ResetPieces(true);
    }

    /// <summary>
    /// Get closest grabbable assembly piece to the given point;
    /// the pickup point of the relevant grabber.
    /// </summary>
    public Grabbable GetClosestPiece(Vector3 pos)
    {
        float d2 = grabDistance * grabDistance;
        Grabbable closestPiece = null;
        foreach (Grabbable ap in pieces)
        {
            if (!ap.IsGrabbable())
                continue;
            float dp2 = (ap.transform.position - pos).sqrMagnitude;
            if (dp2 < d2)
            {
                dp2 = d2;
                closestPiece = ap;
            }
        }
        return closestPiece;
    }

    /// <summary>
    /// Check if candidate piece can be attached to the assembly in its current position.
    /// If so, the candidate piece will be (visually) snapped to the target position.
    /// Returns true if a change has occured; either the part has been placed or 
    /// it is visually snapping onto a new point.
    /// </summary>
    public virtual bool TryFit(Grabbable candidate, bool placeIfFits)
    {
        PieceAttach bestAttach = new PieceAttach(candidate, snapDistance);

        bool isNew = nextAttach == null;
        if (!isNew)
            nextAttach.Hide();

        foreach (AttachNode piv in availableNodes)
        {
            foreach (AttachNode att in candidate.nodes)
            {
                if (piv.CanConnectTo(att))
                {
                    float effDist = piv.GetEffDist(att, snapDistance, snapAngle);
                    bool isSameConnection = !isNew && nextAttach.Matches(piv, att);
                    effDist -= isSameConnection ? snapHysteresis : 0f;
                    bestAttach.Update(piv, att, effDist);
                }
            }
        }

        if (!bestAttach.IsValid())
        {
            nextAttach = null;
            return false;
        }

        bestAttach.Snap(false, checkDistance, checkAngle);
        isNew &= !bestAttach.Matches(nextAttach);
        nextAttach = bestAttach;

        /*
        if(isNew) {
            if(nextAttach != null)
                DataLogger.LogEventEnd(ExperimentData.Event.VisualSnap);
            string attDetails = bestAttach.ToString();
            DataLogger.LogEventStart(ExperimentData.Event.VisualSnap, attDetails);
        }
        */

        if (placeIfFits)
            nextAttach.Snap(true, checkDistance, checkAngle);
        // OnPlace(nextAttach);
        return placeIfFits | isNew;
    }

    /// <summary>
    /// Release given piece from the assembly so it may be placed again
    /// (unused)
    /// </summary>
    public void RedoNextPiece()
    {
        nextAttach.piece.Release();
    }

    /// <summary>
    /// Reset all assembly pieces
    /// </summary>
    /// <param name="hard">if true the pieces placed on the assembly will be released and resetted as well</param>
    public void ResetPieces(bool hard)
    {
        foreach (Grabbable p in pieces)
            p.Reset(hard);
    }

    public Grabbable GetNextPiece()
    {
        if (nextPieceIndex < pieces.Length)
            return pieces[nextPieceIndex];
        return null;
    }

    /// <summary>
    /// Override to trigger when next piece is no longer being held
    /// </summary>
    public virtual void UnlockPiece(Grabbable piece) { }
}
