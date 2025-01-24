using UnityEngine;

public class GradientIK : MonoBehaviour {

    public Transform probe;
    public Transform target;
    public Transform attractor;

    public int iterations = 10;
    public float stepSize = 5f;
    public float jointScale = 0.1f;
    [Range(0f, 1f)]
    public float stabilityFactor = .5f;
    [Range(0f, 1f)]
    public float angleFactor = 1f;
    [Range(0f, 1f)]
    public float relaxFactor = 0f;
    [Range(0f, 1f)]
    public float attractorFactor = 0.1f;

    private int n;
    private float[] x;
    private GIKJoint[] joints;

    private void Start() {
        BuildIK();
    }

    private void Update() {
        GradDescent();
    }

    private void BuildIK() {
        joints = GetComponentsInChildren<GIKJoint>();
        n = joints.Length;
        x = new float[n];
        for(int i = 0; i < n; i++)
            x[i] = joints[i].Init();
    }

    private void GradDescent() {
        float[] grad = new float[n];
        float[] div = new float[n];
        float c, dc, gm2, step;
        float gmp = -.5f * Fct(stabilityFactor);

        if(stepSize <= 0f)
            return;

        for(int i = 0; i < iterations; i++) {
            //calculate gradient
            c = Cost();
            for(int j = 0; j < n; j++) {
                dc = Cost(j, stepSize) - Cost(j, -stepSize);
                grad[j] = dc / stepSize;
            }

            //calculate square magnitude of the gradient vector
            gm2 = 0f;
            for(int j = 0; j < n; j++)
                gm2 += grad[j] * grad[j];
            if(gm2 == 0f)
                return;
            step = stepSize * Mathf.Pow(gm2, gmp);

            //gradient descent
            for(int j = 0; j < n; j++)
                joints[j].x -= step * grad[j];
        }
    }

    private float Cost(int i, float dx) {
        return Cost(i, joints[i].GetTransformMatrix(dx), dx);
    }

    private float Cost() {
        return Cost(0, Matrix4x4.identity, 0f);
    }

    private float Cost(int i, Matrix4x4 probeTransform, float dx) {
        Vector3 pp = probeTransform.MultiplyPoint3x4(probe.transform.position);
        Quaternion pr = probeTransform.rotation * probe.transform.rotation;
        Vector3 jp;
        float c = Fct(angleFactor) * Quaternion.Angle(pr, target.rotation);
        float d = (pp - target.position).magnitude;
        c += d * jointScale * Mathf.Rad2Deg;
        if (!target.gameObject.activeInHierarchy)
            c = 0;
        c += Fct(relaxFactor) * Mathf.Abs(joints[i].x + dx);
        if(attractorFactor > 0f) {
            for(int j = i + 1; j < n; j++) {
                jp = probeTransform.MultiplyPoint3x4(joints[j].transform.position);
                d = (jp - attractor.transform.position).magnitude;
                c += Fct(attractorFactor) * jointScale * Mathf.Rad2Deg * d / n;
            }
        }
        return c;
    }

    /// <summary>
    /// Rescale normalized value to emphasize factors close to 0.
    /// </summary>
    private float Fct(float value) {
        return value * value * value;
    }
}
