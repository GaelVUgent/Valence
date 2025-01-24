using UnityEngine;

public class GIKJoint : MonoBehaviour{

    //public enum type { Hinge, Ball, Slider }

    //hinge
    public Vector3 localAxis = Vector3.forward;
    
    public float x {
        get {
            return _x;
        }
        set {
            _x = Mathf.Repeat(value + 180f, 360f) - 180f;
            transform.localRotation = Quaternion.AngleAxis(_x, baseRot * localAxis) * baseRot;
        }
    }
    private float _x;

    private Quaternion baseRot;
    
    public float Init() {
        baseRot = transform.localRotation;
        _x = 0f;
        return 0f;
    }

    public Matrix4x4 GetTransformMatrix(float dx) {
        Quaternion rotation = Quaternion.AngleAxis(dx, localAxis);
        return
            transform.localToWorldMatrix
            * Matrix4x4.Rotate(rotation)
            * transform.worldToLocalMatrix;
    }
}
