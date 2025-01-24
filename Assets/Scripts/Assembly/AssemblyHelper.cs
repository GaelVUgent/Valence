using System.Collections;
using UnityEngine;

public class AssemblyHelper : MonoBehaviour
{

    public Transform grabProbe;
    public Transform restPoint;
    public Transform defaultDropPoint;
    public Transform grabTargetPoint;
    public Transform[] avoidPoints;
    public Grabber grabber;

    public float moveSpeed = 1f;
    public float rotationSpeed = 10f;
    public float accuracy = 1e-3f;
    public float catchupTime = .5f;
    public float grabTime = .5f;
    public float showTime = 2f;
    public bool allowIntermediateStep;
    public Vector3 restOffset = new Vector3(0f, 0.1f, 0f);

    private float MAX_MOVE_TIME = 3.5f;

    private bool holdPieceTrigger;
    private Coroutine currentRoutine;

    private Quaternion grabOffset { get { return grabTargetPoint.localRotation; } }

    public enum HelpLevel
    {
        PieceOnly = 0,
        Positional = 1,
        PosRot = 2
    }

    private void Update()
    {
        //check if user has grabbed piece out of the grabber, if so, stop movement and return to resting position
        if (holdPieceTrigger & !grabber.IsHolding())
        {
            holdPieceTrigger = false;
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(MoveRoutine(transform.position, transform.rotation));
            DataLogger.LogEventEnd(ExperimentData.Event.RobotHelp);
        }
    }

    /// <summary>
    /// Perform the entire help routine, grabbing the piece under the given 
    /// checkpoint and handing it to the player with the given helping level.
    /// </summary>
    public void Help(AssemblyCheckpoint cp, HelpLevel hl)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            DataLogger.LogEventEnd(ExperimentData.Event.RobotHelp);
        }
        currentRoutine = StartCoroutine(HelpRoutine(cp, hl));
    }

    private IEnumerator HelpRoutine(AssemblyCheckpoint cp, HelpLevel hl)
    {

        DataLogger.LogEventStart(ExperimentData.Event.RobotHelp, cp.target.name);

        //prepare parameters for movement
        Transform tt = cp.target.transform;
        Vector3 nextPos;
        Quaternion nextRot;

        //handle special cases
        bool isHoldingTarget = grabber.IsHolding(cp.target);
        if (!isHoldingTarget)
        {
            if (grabber.IsHolding())
            {
                holdPieceTrigger = false;
                grabber.Release();
                yield return new WaitForSeconds(grabTime);
            }

            //Move to object
            nextPos = tt.position;
            Quaternion ttRot = tt.rotation;
            nextRot = defaultDropPoint.rotation;
            if (hl >= HelpLevel.PosRot)
            {
                Quaternion deltaRotation = cp.transform.rotation * Quaternion.Inverse(ttRot);
                nextRot = Quaternion.Inverse(deltaRotation) * defaultDropPoint.rotation;
                Quaternion pitchRot = Quaternion.FromToRotation(Vector3.up, nextRot * Vector3.up);

                /* if the part has to be pitched too far
                 * i.e. the head would have to clip into the table
                 * Perform the rotation with an intermediate step.
                 */
                float coverAngle = Quaternion.Angle(Quaternion.identity, pitchRot);
                if (coverAngle > 70f & allowIntermediateStep)
                {
                    float sa = 55 / coverAngle;
                    Quaternion q1 = Quaternion.SlerpUnclamped(Quaternion.identity, pitchRot, sa);
                    Quaternion q2 = Quaternion.SlerpUnclamped(Quaternion.identity, pitchRot, -sa);
                    yield return MoveRoutine(nextPos, q1);
                    yield return new WaitForSeconds(.5f);
                    grabber.Grab();
                    holdPieceTrigger = true;
                    yield return new WaitForSeconds(grabTime);
                    yield return MoveRoutine(nextPos + restOffset, q2);
                    grabber.Release();
                    holdPieceTrigger = false;
                    yield return new WaitForSeconds(grabTime + .5f);

                    //recalculate target rotation, now that the piece has been dropped.
                    nextPos = tt.position;
                    ttRot = tt.rotation;
                    deltaRotation = cp.transform.rotation * Quaternion.Inverse(ttRot);
                    nextRot = Quaternion.Inverse(deltaRotation) * defaultDropPoint.rotation;
                }
            }

            yield return MoveRoutine(nextPos, nextRot);

            //snap piece exactly into grabber
            tt.position = grabber.transform.position;
            Quaternion expectedGrabRot = nextRot * grabOffset;
            Quaternion rotationCorrection = grabber.transform.rotation * Quaternion.Inverse(expectedGrabRot);
            tt.rotation = rotationCorrection * ttRot;
            grabber.Grab(cp.target);
            holdPieceTrigger = true;

            yield return new WaitForSeconds(grabTime);
        }

        //Carry object to target position
        nextPos = defaultDropPoint.position;
        nextRot = defaultDropPoint.rotation;
        if (hl >= HelpLevel.Positional)
            nextPos = cp.transform.position;
        yield return MoveRoutine(nextPos, nextRot);

        //if showing the correct position, move the object up afterwards
        if (hl >= HelpLevel.Positional)
        {
            yield return new WaitForSeconds(showTime);
            nextPos = cp.transform.position + restOffset;
            yield return MoveRoutine(nextPos, nextRot);
        }

        currentRoutine = null;
        DataLogger.LogEventStart(ExperimentData.Event.RobotReachHandPosition);
    }

    /// <summary>
    /// Perform one stretch of the arm movement by moving a probe
    /// (with the IK target) from one position and rotation to another.
    /// </summary>
    private IEnumerator MoveRoutine(Vector3 pos, Quaternion rot)
    {
        float t = 0f;
        Quaternion q0 = grabProbe.rotation;
        Vector3 p0 = grabProbe.position;
        float posDist = (pos - p0).magnitude;
        float angDist = Quaternion.Angle(rot, q0);
        float time = Mathf.Min(Mathf.Max(posDist / moveSpeed, angDist / rotationSpeed), MAX_MOVE_TIME);

        do
        {
            float dt = Time.deltaTime;
            t += dt;
            float r = t / time;
            grabProbe.position = Vector3.Lerp(p0, pos, r);
            grabProbe.rotation = Quaternion.Slerp(q0, rot, r);
            yield return null;
        }
        while (t < time);
        grabProbe.rotation = rot;
        grabProbe.position = pos;
        yield return new WaitForSeconds(catchupTime);
    }

    private Vector3 ClosestPoint(Vector3 from, Vector3 to, Vector3 point)
    {
        Vector3 relPoint = point - from;
        Vector3 line = to - from;
        float dNor = Vector3.Dot(relPoint, line) / line.sqrMagnitude;
        if (dNor <= 0f)
            return from;
        else if (dNor >= 1f)
            return to;
        return from + Vector3.Project(relPoint, line);
    }
}
