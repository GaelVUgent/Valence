using UnityEngine;

/// <summary>
/// Assembly nodes mark points where assembly pieces fit together. 
/// These nodes can have 1 or 2 sides, the first being defined in +z the second -z.
/// Nodes can connect in 2 - 4 ways depending on which are 2-sided.
/// thickness is used to create an offset, so that the connection point corresponds 
/// to the surface of the parent piece.
/// </summary>
public class AssemblyNode : MonoBehaviour
{

    public bool twoSided;
    public float thickness;

    private Renderer rend;

    private bool firstOccupied;
    private bool secondOccupied;

    private bool firstAvailable { get { return !firstOccupied; } }
    private bool secondAvailable { get { return twoSided & !secondOccupied; } }

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>(true);
        Reset();
    }

    public void Reset()
    {
        rend.enabled = false;
        firstOccupied = false;
        secondOccupied = false;
    }

    public enum ComparisonType
    {
        FirstToFirst,
        FirstToSecond,
        SecondToFirst,
        SecondToSecond
    }

    /// <summary>
    /// Get effective distance value between this node and another 
    /// that takes into account the difference in rotation.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="snapDist"></param>
    /// <param name="snapAngle"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public float GetEffDist(AssemblyNode other, float snapDist, float snapAngle, ComparisonType ct)
    {
        float dist = GetDist(this, other, ct);
        float angDist = GetAngle(this.transform, other.transform, ct);
        float effAngDist = angDist * snapDist / snapAngle;
        return Mathf.Max(dist, effAngDist);
    }


    /// <summary>
    /// Return final node position, taking into account the thickness applied 
    /// in the appropriate direction based on the comparison type.
    /// </summary>
    /// <param name="a">True for the first node, false for the second</param>
    /// <param name="ct">Comparison type</param>
    private Vector3 GetAttachPosition(bool a, ComparisonType ct)
    {
        Vector3 pos = transform.position;
        Vector3 offset = thickness * transform.forward;
        if (IsForward(a, ct))
            return pos + offset;
        else
            return pos - offset;
    }

    /// <summary>
    /// True if the given node can be connected to this node under the 
    /// given comparison type.
    /// </summary>
    public bool CanConnectTo(AssemblyNode node, ComparisonType ct)
    {
        //at least one part should be two sided so a screw can be attached
        if (!twoSided & !node.twoSided)
            return false;

        switch (ct)
        {
            default: return true;
            case ComparisonType.FirstToFirst: return firstAvailable & node.firstAvailable;
            case ComparisonType.FirstToSecond: return firstAvailable & node.secondAvailable;
            case ComparisonType.SecondToFirst: return secondAvailable & node.firstAvailable;
            case ComparisonType.SecondToSecond: return secondAvailable & node.secondAvailable;
        }
    }

    /// <summary>
    /// Moves the given piece to the rotation and position where it is connected 
    /// to this node, via the given attach node and comparison type.
    /// Note that this move procedure has a free rotational degree of motion, 
    /// which is chosen so that the rotation of the piece changes as little 
    /// as possible (virtue of Quaternion.FromToRotation)
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="attachNode"></param>
    /// <param name="ct"></param>
    public void Move(AssemblyPiece piece, AssemblyNode attachNode, ComparisonType ct)
    {
        Vector3 axis = transform.forward;
        Vector3 pivot = GetAttachPosition(true, ct);
        Vector3 attAxis = attachNode.GetAttachPosition(false, ct) - piece.transform.position;
        Vector3 attDir = attachNode.transform.forward;
        if (OppositeDirections(ct))
            attDir = -attDir;

        Quaternion relRot = Quaternion.FromToRotation(attDir, axis);
        Vector3 relPos = pivot - piece.transform.position - relRot * attAxis;

        piece.transform.rotation = relRot * piece.transform.rotation;
        piece.transform.position += relPos;

        AssemblyNode displayNode = this;
        if (twoSided & !attachNode.twoSided)
            displayNode = attachNode;
        displayNode.SetVisible(true);
    }

    /// <summary>
    /// Marks the used sides of this node and the given node as 
    /// occupied, so they no longer turn up as available in 
    /// CanConnectTo().
    /// </summary>
    public void Attach(AssemblyNode node, ComparisonType ct)
    {
        if (IsForward(true, ct))
            firstOccupied = true;
        else
            secondOccupied = true;
        if (IsForward(false, ct))
            node.firstOccupied = true;
        else
            node.secondOccupied = true;
    }



    private static float GetDist(AssemblyNode a, AssemblyNode b, ComparisonType ct)
    {
        Vector3 aPos = a.GetAttachPosition(true, ct);
        Vector3 bPos = b.GetAttachPosition(false, ct);
        return Vector3.Distance(aPos, bPos);
    }

    private static bool OppositeDirections(ComparisonType ct)
    {
        switch (ct)
        {
            case ComparisonType.FirstToFirst:
            case ComparisonType.SecondToSecond:
                return true;

            default:
                return false;
        }
    }

    private static bool IsForward(bool a, ComparisonType ct)
    {
        switch (ct)
        {
            case ComparisonType.FirstToSecond:
                return a;
            case ComparisonType.SecondToFirst:
                return !a;
            case ComparisonType.FirstToFirst:
                return true;
            default:
                return false;
        }
    }

    private static float GetAngle(Transform a, Transform b, ComparisonType ct)
    {
        switch (ct)
        {
            case ComparisonType.FirstToFirst:
            case ComparisonType.SecondToSecond:
                return Vector3.Angle(a.forward, -b.forward);

            case ComparisonType.FirstToSecond:
            case ComparisonType.SecondToFirst:
                return Vector3.Angle(a.forward, b.forward);

            default:
                return Quaternion.Angle(a.rotation, b.rotation);
        }
    }

    public void SetVisible(bool visible)
    {
        rend.enabled = visible;
    }







#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(AssemblyNode))]
    [UnityEditor.CanEditMultipleObjects]
    public class AssemblyNodeEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AssemblyNode an = (AssemblyNode)target;

            if (GUILayout.Button("Flip"))
            {
                foreach (Object o in targets)
                {
                    Transform t = ((AssemblyNode)o).transform;
                    t.forward = -t.forward;
                }
            }
        }

    }

#endif
}
