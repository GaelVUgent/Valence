using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class for managing the flow of an assembly.
/// Implementations exist for experiment and practice scene.
/// </summary>
public abstract class AssemblyTask : PieceFitter
{

    //connections
    public AssemblyHelper helper;
    public AssemblyCheckpoint startAttach;
    public GameObject startMarker;

    //settings
    public AssemblyHelper.HelpLevel helpLevel;
    public bool skipStartStep;
    public bool allowSkip;

    //task state
    protected int currentTaskIndex;
    protected AssemblyPiece[] assemblyPieces;
    protected AssemblyCheckpoint nextCP;
    protected List<AssemblyNode> assemblyNodes;
    protected List<AssemblyCheckpoint> availableCps;

    //confirmation buffer
    protected NodeAttach assemblyAttach;
    protected NodeAttach candidateAssemblyAttach; //attachment of the next part in the assembly, if any
    protected AssemblyCheckpoint checkpointAttach;

    protected bool done { get { return availableCps.Count == 0; } }

    protected override void Awake()
    {
        base.Awake();

        assemblyPieces = GetComponentsInChildren<AssemblyPiece>();
        if (Global.global == null)
        {
            Debug.LogError("Please start in the main menu scene so that assembly task has access to the global script.");
        }
    }

    protected override void Start()
    {
        base.Start();

        currentTaskIndex = 0;
        ResetAssembly();
    }

    public virtual void ResetAssembly()
    {
        nextPieceIndex = 0;
        availableCps = new List<AssemblyCheckpoint>();
        assemblyNodes = new List<AssemblyNode>();
        availableCps.Add(startAttach);
        startMarker.SetActive(true);
        availableCps[0].target.SetAvailable();

        StartCoroutine(StartAssemblySequence());
    }

    private IEnumerator StartAssemblySequence()
    {
        float angle = Global.global.flow.GetAssemblyAngle(currentTaskIndex);
        transform.eulerAngles = new Vector3(0f, angle, 0f);
        ResetPieces(true);
        yield return new WaitForSeconds(.1f);
        DataLogger.StartSequence();
        if (skipStartStep)
            AttachViaCheckpoint(0);
        yield return new WaitForSeconds(1f);
        UpdateHelp();
    }

    protected void UpdateHelp()
    {
        if (helper == null || !helper.gameObject.activeInHierarchy)
            return;
        if ((nextCP == null || !nextCP.target.IsGrabbable()) & availableCps.Count > 0)
            nextCP = availableCps[0];
        if (nextCP != null && nextCP.target.IsGrabbable())
            helper.Help(nextCP, helpLevel);
    }

    protected virtual void Update()
    {
        if (allowSkip & !done & Input.GetKeyDown(KeyCode.Space))
            AttachViaCheckpoint(0);
    }

    private void AttachViaCheckpoint(int i)
    {
        if (i >= availableCps.Count)
            return;
        Attach(availableCps[i].target);
        Continue();
    }

    private void Attach(AssemblyPiece piece)
    {
        piece.cp.Attach();
        availableCps.Remove(piece.cp);
        availableCps.AddRange(piece.checkPoints);
        assemblyNodes.AddRange(piece.assemblyNodes);

        nextPieceIndex++;
        startMarker.SetActive(nextPieceIndex == 0);
    }

    /// <summary>
    /// Get closest grabbable assembly piece to the given point;
    /// the pickup point of the relevant grabber.
    /// </summary>
    new public AssemblyPiece GetClosestPiece(Vector3 pos)
    {
        float d2 = grabDistance * grabDistance;
        AssemblyPiece closestPiece = null;
        foreach (AssemblyPiece ap in assemblyPieces)
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
    /// <param name="candidate">Candidate piece the user is trying to fit into the assembly</param>
    /// <param name="placeIfFits">Set to true if candidate piece should be placed if possible</param>
    /// <returns></returns>
    public override bool TryFit(Grabbable candidate, bool placeIfFits)
    {
        if (nextPieceIndex == 0)
            return TryFitToCP((AssemblyPiece)candidate, placeIfFits);
        else
            return TryFitToNode((AssemblyPiece)candidate, placeIfFits);
    }

    /// <summary>
    /// Try to fit piece to its checkpoint
    /// </summary>
    private bool TryFitToCP(AssemblyPiece candidate, bool placeIfFits)
    {
        float bestDistance = snapDistance;
        AssemblyCheckpoint attachPoint = null;
        for (int i = 0; i < availableCps.Count; i++)
        {
            AssemblyCheckpoint cp = availableCps[i];
            Vector3 pos = candidate.transform.position;
            float dist = (pos - cp.transform.position).magnitude;
            bool isSameCP = checkpointAttach == cp;
            dist -= isSameCP ? snapHysteresis : 0f;
            if (dist < bestDistance)
            {
                bestDistance = dist;
                attachPoint = cp;
                candidate.transform.position = cp.transform.position;
                candidate.transform.rotation = cp.transform.rotation;
                if (placeIfFits)
                    AttachViaCheckpoint(i);
            }
        }

        bool isNew = attachPoint != checkpointAttach;
        checkpointAttach = attachPoint;
        if (!attachPoint)
            return false;
        return placeIfFits | isNew;
    }

    /// <summary>
    /// Try to fit piece via an assembly node pair
    /// </summary>
    private bool TryFitToNode(AssemblyPiece candidate, bool placeIfFits)
    {
        NodeAttach bestAttach = new NodeAttach(candidate, snapDistance);
        bool isNew = assemblyAttach == null;
        if (!isNew)
            assemblyAttach.Hide();

        foreach (AssemblyNode.ComparisonType ct in new[] { 0, 1, 2, 3 })
        {
            foreach (AssemblyNode piv in assemblyNodes)
            {
                foreach (AssemblyNode att in candidate.assemblyNodes)
                {
                    if (piv.CanConnectTo(att, ct))
                    {
                        float effDist = piv.GetEffDist(att, snapDistance, snapAngle, ct);
                        bool isSameConnection = !isNew && assemblyAttach.Matches(piv, att, ct);
                        effDist -= isSameConnection ? snapHysteresis : 0f;
                        bestAttach.Update(piv, att, ct, effDist);
                    }
                }
            }
        }

        if (!bestAttach.IsValid())
        {
            assemblyAttach = null;
            return false;
        }

        bestAttach.Snap(false, checkDistance, checkAngle);
        isNew &= !bestAttach.Matches(assemblyAttach);
        assemblyAttach = bestAttach;
        
        if (isNew)
        {
            if (assemblyAttach != null)
                DataLogger.LogEventEnd(ExperimentData.Event.VisualSnap);
            string attDetails = bestAttach.ToString();
            DataLogger.LogEventStart(ExperimentData.Event.VisualSnap, attDetails);
        }

        if (placeIfFits)
        {
            assemblyAttach.Snap(true, checkDistance, checkAngle);
            if (nextCP != null && nextCP.target == assemblyAttach.piece)
                candidateAssemblyAttach = assemblyAttach;
            OnPlace(assemblyAttach);
        }
        return placeIfFits | isNew;
    }

    protected abstract void OnPlace(NodeAttach attach);

    /// <summary>
    /// Confirm piece currently attached to the assembly
    /// </summary>
    public void ConfirmNextPiece()
    {
        if (candidateAssemblyAttach != null)
            Attach(candidateAssemblyAttach.piece);
        else if (assemblyAttach != null)
            Attach(assemblyAttach.piece);
        else
            Debug.LogError("No piece to attach!!");

        OnConfirm();

        assemblyAttach = null;
    }

    protected abstract void OnConfirm();

    /// <summary>
    /// Move on to the next piece or end of the assembly
    /// </summary>
    public void Continue()
    {
        AssemblyCheckpoint nextCP = null;
        if (availableCps.Count > 0)
            nextCP = availableCps[0];

        OnContinue(nextCP);
    }

    protected abstract void OnContinue(AssemblyCheckpoint nextCP);

    /// <summary>
    /// Release given piece from the assembly so it may be placed again
    /// (unused)
    /// </summary>
    new public void RedoNextPiece()
    {
        assemblyAttach.piece.Release();
    }

    /// <summary>
    /// Reset all assembly pieces
    /// </summary>
    /// <param name="hard">if true the pieces placed on the assembly will be released and resetted as well</param>
    new public void ResetPieces(bool hard)
    {
        StartCoroutine(ResetSequence(hard));
    }

    private IEnumerator ResetSequence(bool hard)
    {
        foreach (AssemblyPiece p in assemblyPieces)
            p.Reset(hard);
        if (!hard)
        {
            OnReset();
            yield return new WaitForSeconds(1f);
            UpdateHelp();
        }
    }

    protected abstract void OnReset();

    new public AssemblyPiece GetNextPiece()
    {
        if (nextPieceIndex < assemblyPieces.Length)
            return assemblyPieces[nextPieceIndex];
        return null;
    }

    /// <summary>
    /// Helper class that encodes a node attachment; a pair of assembly nodes 
    /// and the way they connect.
    /// This helps to keep track of different connection candidates.
    /// </summary>
    protected class NodeAttach
    {
        private AssemblyNode piv, att;
        private AssemblyNode.ComparisonType ct;
        public AssemblyPiece piece { get; private set; }
        public bool onCP { get; private set; }

        private float effDist;
        private float normDist;

        /// <summary>
        /// Create an otherwise blank nodeAttach with the given 
        /// target piece and effective distance
        /// </summary>
        public NodeAttach(AssemblyPiece p, float d)
        {
            piece = p;
            effDist = d;

            piv = null;
            att = null;
            ct = 0;
        }

        /// <summary>
        /// True if the nodes and attachment type are the same as the other attach
        /// </summary>
        public bool Matches(NodeAttach other)
        {
            if (other == null)
                return false;
            return Matches(other.piv, other.att, other.ct);
        }

        public bool Matches(AssemblyNode piv, AssemblyNode att, AssemblyNode.ComparisonType ct)
        {
            return piv == this.piv & att == this.att & ct == this.ct;
        }

        /// <summary>
        /// Hide all visuals related to this attachment
        /// </summary>
        public void Hide()
        {
            piv.SetVisible(false);
            att.SetVisible(false);
        }

        public bool IsValid()
        {
            return piv != null;
        }

        /// <summary>
        /// Move the piece in position where it is attached by this node and calculates distance too.
        /// </summary>
        /// <param name="freeze">Set to false for a temporary visual snap, set to true for placing the piece and awaiting confirmation</param>
        /// <param name="cd">Distance measure that is equal to the given angle measure</param>
        /// <param name="ca">Angle measure, used to convert units from angles to distance when calculate the effective distance</param>
        public void Snap(bool freeze, float cd, float ca)
        {
            piv.Move(piece, att, ct);
            if (freeze)
                piece.Freeze();
            normDist = piece.cp.GetEffDist(cd, ca) / cd;
            onCP = normDist < 1f;
        }

        /// <summary>
        /// Confirm this attachment, snapping the piece to its checkpoint 
        /// and marking the relevant nodes as used and visible.
        /// </summary>
        public bool Confirm()
        {
            piece.transform.position = piece.cp.transform.position;
            piece.transform.rotation = piece.cp.transform.rotation;

            DataLogger.LogEventStart(ExperimentData.Event.ConfirmPiece, ToString());
            DataLogger.ConfirmStep(onCP, normDist);

            if (onCP)
            {
                piv.Attach(att, ct);
            }
            else
            {
                piv.SetVisible(false);
                att.SetVisible(false);
            }
            return onCP;
        }

        /// <summary>
        /// Update this attachment if the given distance is smaller
        /// (used as part of find minimum algorithm)
        /// </summary>
        /// <param name="piv">The node which is currently on the assembly</param>
        /// <param name="att">The node on the currently free piece</param>
        /// <param name="ct">Attachment type between the given nodes</param>
        /// <param name="d">effective distance</param>
        public void Update(AssemblyNode piv, AssemblyNode att, AssemblyNode.ComparisonType ct, float d)
        {
            if (d < this.effDist)
            {
                this.piv = piv;
                this.att = att;
                this.ct = ct;
                this.effDist = d;
            }
        }

        public override string ToString()
        {
            string corString = onCP ? "Correct" : "Incorrect";
            return piece.name + " by " + att.name + " on " + piv.name + ": "
                + corString + "(" + normDist.ToString("F2") + ")";
        }
    }
}
