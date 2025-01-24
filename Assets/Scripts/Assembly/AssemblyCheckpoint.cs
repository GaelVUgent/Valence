using UnityEngine;

public class AssemblyCheckpoint : MonoBehaviour
{

    public AssemblyPiece target;
    public int instructionStep;

    public bool attached
    {
        get
        {
            return target.transform.parent == transform;
        }
    }

    private void Awake()
    {
        target.cp = this;
    }

    /// <summary>
    /// Attach target piece to the assembly, locking it and returning checkpoints 
    /// available on that piece, to be added to the list maintained in assembly task.
    /// </summary>
    public AssemblyCheckpoint[] Attach()
    {
        target.transform.position = transform.position;
        target.transform.rotation = transform.rotation;
        target.Lock();
        return target.checkPoints;
    }

    /// <summary>
    /// Get effective distance of the target assembly piece 
    /// to this checkpoint. The effective distance takes into 
    /// account the difference in rotation.
    /// </summary>
    /// <param name="snapDist">The equivalent distance value for the given angle</param>
    /// <param name="snapAngle">The equivalent angle value for the given distance</param>
    public float GetEffDist(float snapDist, float snapAngle)
    {
        float dist = Vector3.Distance(transform.position, target.transform.position);
        float angDist = Quaternion.Angle(transform.rotation, target.transform.rotation);
        float effAngDist = angDist * snapDist / snapAngle;
        return Mathf.Max(dist, effAngDist);
    }
}
