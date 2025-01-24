using UnityEngine;

public class Grabbable : HCCIInteractionObject
{

    protected Rigidbody rb;
    public Grabber currentGrabber { get; private set; }
    private Outline[] outlines;

    public AttachNode[] nodes { get; private set; }

    private Vector3 startPos;
    private Quaternion startRot;

    private bool available;
    private bool locked;
    private Color marking;

    protected virtual void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        outlines = GetComponentsInChildren<Outline>();
        nodes = GetComponentsInChildren<AttachNode>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    protected override void Start()
    {
        base.Start();

        available = true;
        locked = false;
        marking = Color.clear;

        foreach (Outline outline in outlines)
        {
            outline.OutlineColor = marking;
            outline.enabled = false;
        }
    }

    /// <summary>
    /// Should be called every Update to maintain the marking color,
    /// otherwise the marking will smoothly fade away
    /// </summary>
    public void Mark(Color c)
    {
        marking = c;
    }

    private void Update()
    {
        //fade marking color to transparancy
        float a = marking.a;
        foreach (Outline outline in outlines)
        {
            outline.OutlineColor = marking;
            outline.enabled = a > 0f;
        }
        a = Mathf.MoveTowards(a, 0f, Time.deltaTime / .2f);
        marking.a = a;
    }

    /// <summary>
    /// Mark this piece is being possible to pick up by a user-controlled grabber
    /// </summary>
    public void SetAvailable()
    {
        available = true;
    }

    /// <summary>
    /// Lock this piece onto the assembly, so it can no longer be picked up.
    /// This should be done when the user confirms this piece.
    /// </summary>
    public void Lock()
    {
        available = false;
        locked = true;
        rb.isKinematic = true;
    }

    /// <summary>
    /// Temporarily freeze the piece in place, turning off it's physics.
    /// Usually used for temporarily placing a piece on the assembly, 
    /// before receiving confirmation.
    /// </summary>
    public void Freeze()
    {
        rb.isKinematic = true;
    }

    /// <summary>
    /// Reset this piece to its starting position at a random rotation.
    /// The rotation is chosen so that it's not too close to the target rotation.
    /// </summary>
    /// <param name="hard">If false this piece will not be resetted if it is locked into.</param>
    public virtual bool Reset(bool hard)
    {
        if (locked & !hard)
            return false;
        Release();
        transform.position = startPos;
        transform.rotation = startRot;
        return true;
    }

    /// <summary>
    /// Remove this piece from the clutches of its grabber (if any) and set it free.
    /// </summary>
    public void Release()
    {
        if (currentGrabber != null)
        {
            currentGrabber.Release();
            base.OnDroppedObject();
        }
        locked = false;
        currentGrabber = null;
        rb.isKinematic = false;
    }

    /// <summary>
    /// Set this piece as currently being held by the given grabber.
    /// </summary>
    public void SetGrabbed(Grabber grabber)
    {
        if (currentGrabber != null)
        {
            if (grabber.canPlace)
            {
                if (currentGrabber.canPlace)
                    DataLogger.LogEventStart(ExperimentData.Event.PieceChangeHand, name);
                else
                    DataLogger.LogEventStart(ExperimentData.Event.RobotHandPiece, name);
            }
            currentGrabber.Release(true);
        }
        base.OnGrabbedObject();
        currentGrabber = grabber;
        rb.isKinematic = true;

        foreach (AttachNode node in nodes)
            node.Detach();
    }

    /// <summary>
    /// Mark this piece as no longer being held. This method does not change the state of the 
    /// grabber, so in practice it should only ever be called by the grabber holding it.
    /// </summary>
    public void SetReleased()
    {
        if (currentGrabber != null)
            base.OnDroppedObject();
        currentGrabber = null;
        rb.isKinematic = locked;
    }

    /// <summary>
    /// true if this piece is currently available for being grabbed.
    /// Note that that already being held by a grabber will not cause this method to return false.
    /// </summary>
    public bool IsGrabbable()
    {
        return available;
    }
}
