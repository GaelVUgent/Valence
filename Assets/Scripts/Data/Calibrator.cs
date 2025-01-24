using System.Collections;
using UnityEngine;

/// <summary>
/// handles calibration of the VR setup
/// Should be available globally.
/// </summary>
public class Calibrator : MonoBehaviour
{

    public Camera vrCamera;
    public Transform vrBase;
    public Transform vrTarget;
    public Transform leftHandLogPoint, rightHandLogPoint;

    private void Update()
    {
        bool reqCalib = Input.GetKey(KeyCode.K) | Input.GetKey(KeyCode.C);
        if (reqCalib)
            Calibrate();
    }

    private void Calibrate()
    {
        Vector3 camAxis = vrCamera.transform.position - vrBase.position;

        Vector3 camHorDir = Vector3.ProjectOnPlane(vrCamera.transform.forward, Vector3.up);
        Vector3 targetHorDir = Vector3.ProjectOnPlane(vrTarget.forward, Vector3.up);
        Quaternion rotCor = Quaternion.FromToRotation(camHorDir, targetHorDir);
        vrBase.transform.rotation *= rotCor;

        Vector3 posCor = vrTarget.position - vrCamera.transform.position;
        vrBase.transform.position += posCor;
    }

}
