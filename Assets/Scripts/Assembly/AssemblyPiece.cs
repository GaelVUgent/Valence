using UnityEngine;

public class AssemblyPiece : Grabbable
{

    public AssemblyCheckpoint[] checkPoints { get; private set; }
    public AssemblyNode[] assemblyNodes { get; private set; }
    public AssemblyCheckpoint cp { get; set; }

    protected override void Awake()
    {
        base.Awake();
        checkPoints = GetComponentsInChildren<AssemblyCheckpoint>();
        assemblyNodes = GetComponentsInChildren<AssemblyNode>();
    }

    /// <summary>
    /// Reset this piece to its starting position at a random rotation.
    /// The rotation is chosen so that it's not too close to the target rotation.
    /// </summary>
    /// <param name="hard">If false this piece will not be resetted if it is locked onto the assembly.</param>
    public override bool Reset(bool hard)
    {
        if (!base.Reset(hard))
            return false;


        //randomize rotation
        Vector3 axis = Random.onUnitSphere;
        float angle = (2f * Random.Range(0, 2) - 1f) * Random.Range(70f, 110f);
        transform.rotation = Quaternion.AngleAxis(angle, axis) * cp.transform.rotation;

        if (hard)
        {
            foreach (AssemblyNode node in assemblyNodes)
                node.Reset();
        }

        return true;
    }
}
