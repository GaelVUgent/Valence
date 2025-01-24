
/// <summary>
/// Helper class that encodes a node attachment; a pair of attachNodes 
/// and the way they connect. 
/// This helps to keep track of different connection candidates.
/// </summary>
public class PieceAttach
{

    private AttachNode piv, att;
    public Grabbable piece { get; private set; }

    private float effDist;
    private float normDist;

    /// <summary>
    /// Create an otherwise blank nodeAttach with the given 
    /// target piece and effective distance
    /// </summary>
    public PieceAttach(Grabbable p, float d)
    {
        piece = p;
        effDist = d;

        piv = null;
        att = null;
    }

    public bool Matches(PieceAttach other)
    {
        if (other == null)
            return false;
        return Matches(other.piv, other.att);
    }

    public bool Matches(AttachNode piv, AttachNode att)
    {
        return piv == this.piv & att == this.att;
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
    /// <param name="place">Set to false for a temporary visual snap, set to true for placing the piece and awaiting confirmation</param>
    /// <param name="cd">Distance measure that is equal to the given angle measure</param>
    /// <param name="ca">Angle measure, used to convert units from angles to distance when calculate the effective distance</param>
    public void Snap(bool place, float cd, float ca)
    {
        piv.Move(piece, att, piv.freeRoll & att.freeRoll);
        if (place)
        {
            piv.Attach(att);
            piece.Freeze();
        }
    }

    /// <summary>
    /// Update this attachment if the given distance is smaller
    /// (used as part of find minimum algorithm)
    /// </summary>
    /// <param name="piv">The node which is currently on the assembly</param>
    /// <param name="att">The node on the currently free piece</param>
    /// <param name="d">effective distance</param>
    public void Update(AttachNode piv, AttachNode att, float d)
    {
        if (d < this.effDist)
        {
            this.piv = piv;
            this.att = att;
            this.effDist = d;
        }
    }

    public override string ToString()
    {
        return $"{piece.name} by {att.name} on {piv.name} ({normDist.ToString("F2")})";
    }
}
