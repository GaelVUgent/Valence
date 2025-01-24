using UnityEngine;

/// <summary>
/// Nodes are used to encode ways that a piece can be attached to some sort of assembly.
/// 2 nodes can attach to eachother when their position and rotation match.
/// For freerolling nodes the forward vectors need to be opposite to eachother 
/// </summary>
public class AttachNode : MonoBehaviour
{

    [Tooltip("Allow piece to roll around forward direction of the node freely, relaxing the angle constraint")]
    public bool freeRoll;
    [Tooltip("Nodes that should be flagged as occupied along with this one, this can be used when having a double node for a forward/backward freerolling connection, or when allowing the same connection at different angles")]
    public AttachNode[] siblingNodes;

    private AttachNode connectedNode;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend)
            rend.enabled = false;
        connectedNode = null;
    }

    /// <summary>
    /// Get effective distance value between this node and another 
    /// that takes into account the difference in rotation.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="snapDist"></param>
    /// <param name="snapAngle"></param>
    public float GetEffDist(AttachNode other, float snapDist, float snapAngle)
    {
        float dist = GetDist(this.transform, other.transform);
        float angDist = GetAngle(this.transform, other.transform, freeRoll & other.freeRoll);
        float effAngDist = angDist * snapDist / snapAngle;
        return Mathf.Max(dist, effAngDist);
    }

    /// <summary>
    /// True if the given node can be connected to this node under the 
    /// given comparison type.
    /// </summary>
    public bool CanConnectTo(AttachNode node)
    {
        return connectedNode == null & node.connectedNode == null;
    }

    /// <summary>
    /// Moves the given piece to the rotation and position where it is connected 
    /// to this node, via the given attach node and comparison type.
    /// Note that this move procedure can have a free rotational degree of motion, 
    /// which is chosen so that the rotation of the piece changes as little 
    /// as possible (virtue of Quaternion.FromToRotation)
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="attachNode"></param>
    /// <param name="ct"></param>
    public void Move(Grabbable piece, AttachNode attachNode, bool freeRoll)
    {
        Vector3 attAxis = attachNode.transform.position - piece.transform.position;

        Quaternion relRot;
        if (freeRoll)
        {
            Vector3 axis = transform.forward;
            Vector3 attDir = -attachNode.transform.forward;
            relRot = Quaternion.FromToRotation(attDir, axis);
        }
        else
            relRot = transform.rotation * Quaternion.Inverse(attachNode.transform.rotation);
        Vector3 relPos = transform.position - piece.transform.position - relRot * attAxis;

        piece.transform.rotation = relRot * piece.transform.rotation;
        piece.transform.position += relPos;

        SetVisible(true);
    }

    /// <summary>
    /// Connects this node to the given node and back.
    /// The node will be marked as occupied and will no longer receive 
    /// connections until Detached.
    /// </summary>
    public void Attach(AttachNode node)
    {
        if (connectedNode == node)
            return;
        else if (connectedNode != null)
            Debug.LogWarning("Overwriting attach node connection!");

        connectedNode = node;
        foreach (AttachNode sibling in siblingNodes)
            sibling.connectedNode = node;

        node.Attach(this);
    }

    /// <summary>
    /// Detaches this nodes and its connection, so they both 
    /// become available again.
    /// </summary>
    public void Detach()
    {
        if (connectedNode == null)
            return;

        AttachNode oldConnection = connectedNode;
        connectedNode = null;
        foreach (AttachNode sibling in siblingNodes)
            sibling.connectedNode = null;

        oldConnection.Detach();
    }

    private static float GetDist(Transform a, Transform b)
    {
        return Vector3.Distance(a.position, b.position);
    }

    private static float GetAngle(Transform a, Transform b, bool freeRoll)
    {
        if (freeRoll)
            return Vector3.Angle(a.forward, -b.forward);
        else
            return Quaternion.Angle(a.rotation, b.rotation);
    }

    public void SetVisible(bool visible)
    {
        if (rend)
            rend.enabled = visible;
    }







#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(AttachNode))]
    [UnityEditor.CanEditMultipleObjects]
    public class AttachNodeEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AttachNode an = (AttachNode)target;

            if (GUILayout.Button("Flip"))
            {
                foreach (Object o in targets)
                {
                    Transform t = ((AttachNode)o).transform;
                    t.forward = -t.forward;
                }
            }
        }

    }

#endif
}
