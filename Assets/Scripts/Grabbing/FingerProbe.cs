using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerProbe : MonoBehaviour {

    public Grabbable piece { get; private set; }

    private void OnTriggerEnter(Collider other) {
        Grabbable p = other.GetComponentInParent<Grabbable>();
        if(p != null && p.IsGrabbable())
            piece = p;
    }

    private void OnTriggerExit(Collider other) {
        Grabbable p = other.GetComponentInParent<Grabbable>();
        if(p == piece)
            piece = null;
    }
}
