using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves sliding fingers of the robot over the piece it's grabbing.
/// This effect is purely visual.
/// </summary>
public class RobotFinger : MonoBehaviour {

    public Grabber parent;
    public Transform probePoint;
    public float speed = .3f;
    public float maxDist = .5f;
    public LayerMask probeMask;
    private Vector3 openPos;

    private bool closed;
    private Vector3 targetPos;

    private void Awake() {
        openPos = transform.localPosition;
        targetPos = openPos;
    }

    private void Update() {
        if(parent.IsHolding() != closed) {
            if(closed)
                Release();
            else
                Grab();
        }


        float dx = speed * Time.deltaTime;
        Vector3 p = transform.localPosition;
        p = Vector3.MoveTowards(p, targetPos, dx);
        transform.localPosition = p;
    }

    private void Grab() {
        Ray ray = new Ray(probePoint.position, probePoint.forward);
        float d = maxDist;
        RaycastHit info;
        if(Physics.Raycast(ray, out info, d, probeMask)) {
            d = info.distance;
        }
        Vector3 closeDir = parent.transform.InverseTransformVector(probePoint.forward);
        targetPos = openPos + d * closeDir;

        closed = true;
    }

    private void Release() {
        targetPos = openPos;

        closed = false;
    }
}
