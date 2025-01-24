
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GIKJoint))]
[CanEditMultipleObjects]
public class DynamicJointLimitHingeInspector : Editor {
    private GIKJoint joint { get { return target as GIKJoint; } }
    

    /*
    public override void OnInspectorGUI() {
        GUI.changed = false;

        base.OnInspectorGUI();
        //DrawDefaultInspector();

        if(GUI.changed)
            EditorUtility.SetDirty(script);

    }
    */


    void OnSceneGUI() {
        if(joint.localAxis == Vector3.zero)
            return;

        Vector3 p = joint.transform.position;
        Vector3 ax = joint.transform.localToWorldMatrix.MultiplyVector(joint.localAxis);
        Quaternion chcr = Quaternion.AngleAxis(360f, joint.localAxis);
        
        DrawArrow(p - .5f * ax, ax, colorDefault, "Axis", 0.02f);

        Handles.color = Color.white;
        GUI.color = Color.white;
        /*

        Vector3 swing = Direction(offsetRot * joint.mainAxis.normalized);

        //Vector3 secondaryAxis = new Vector3(script.mainAxis.y, script.mainAxis.z, script.mainAxis.x);
        //Vector3 cross = Direction(Vector3.Cross(script.mainAxis, secondaryAxis));
        if(joint.cross.magnitude == 0)
            return;
        Vector3 cross = Direction(offsetRot * joint.cross);
        Vector3.OrthoNormalize(ref swing, ref cross);
        Vector3 secondaryAxis = Vector3.Cross(joint.mainAxis, cross);

        Handles.CircleHandleCap(0, joint.transform.position, Quaternion.LookRotation(swing, cross), boneLength, EventType.Repaint);

        //DrawArrow(script.transform.position, cross * boneLength, colorDefault, " 0", 0.02f);
        DrawArrow(joint.transform.position, cross * boneLength, colorDefault, " 0", 0.02f);

        Quaternion hingeOffset = Quaternion.AngleAxis(joint.hingeAngleOffset, swing);

        // Arcs for the rotation limit
        Handles.color = colorDefaultTransparent;
        Handles.DrawSolidArc(joint.transform.position, swing, hingeOffset * cross, -joint.limitAngle, boneLength);
        Handles.DrawSolidArc(joint.transform.position, swing, hingeOffset * cross, joint.limitAngle, boneLength);

        Quaternion minRotation = Quaternion.AngleAxis(-joint.limitAngle, swing);
        Vector3 minHandleDir = minRotation * (hingeOffset * cross);
        Handles.DrawLine(joint.transform.position, joint.transform.position + minHandleDir.normalized * boneLength);

        Quaternion maxRotation = Quaternion.AngleAxis(joint.limitAngle, swing);
        Vector3 maxHandleDir = maxRotation * (hingeOffset * cross);
        Handles.DrawLine(joint.transform.position, joint.transform.position + maxHandleDir.normalized * boneLength);

        Handles.color = colorHandles;
        //Draw Editable Handles
        float originalLimit = joint.limitAngle;
        float limitAngleMin = joint.limitAngle;
        limitAngleMin = DrawLimitHandle(limitAngleMin, joint.transform.position + (minHandleDir.normalized * boneLength * 1.25f), Quaternion.identity, 0.5f, "Limit", -10);
        if(limitAngleMin != joint.limitAngle) {
            if(!Application.isPlaying)
                Undo.RecordObject(joint, "Min Limit");
            joint.limitAngle = limitAngleMin;
            joint.hingeAngleOffset -= (limitAngleMin - originalLimit);
        }

        originalLimit = joint.limitAngle;
        //Draw Editable Handles
        float limitAngleMax = joint.limitAngle;
        limitAngleMax = DrawLimitHandle(limitAngleMax, joint.transform.position + (maxHandleDir.normalized * boneLength * 1.25f), Quaternion.identity, 0.5f, "Limit", 10);
        if(limitAngleMax != joint.limitAngle) {
            if(!Application.isPlaying)
                Undo.RecordObject(joint, "Max Limit");
            joint.limitAngle = limitAngleMax;
            joint.hingeAngleOffset += (limitAngleMax - originalLimit);
        }

        // clamp limits
        joint.limitAngle = Mathf.Clamp(joint.limitAngle, 0, 180f);
        if(joint.hingeAngleOffset < 0)
            joint.hingeAngleOffset += 360;
        if(joint.hingeAngleOffset > 360)
            joint.hingeAngleOffset -= 360;
        */

    }


    // Universal color pallettes
    public static Color colorDefault { get { return new Color(0.0f, 0.0f, 1.0f, 1.0f); } }

    public static Color colorDefaultTransparent {
        get {
            Color d = colorDefault;
            return new Color(d.r, d.g, d.b, 0.2f);
        }
    }

    public static Color colorHandles { get { return new Color(1.0f, 0.5f, 0.25f, 1.0f); } }
    public static Color colorRotationSphere { get { return new Color(1.0f, 1.0f, 1.0f, 0.1f); } }
    public static Color colorInvalid { get { return new Color(1.0f, 0.3f, 0.3f, 1.0f); } }
    public static Color colorValid { get { return new Color(0.2f, 1.0f, 0.2f, 1.0f); } }

    public static void DrawArrow(Vector3 position, Vector3 direction, Color color, string label = "", float size = 0.01f) {
        Handles.color = color;
        Handles.DrawLine(position, position + direction);
        Handles.SphereHandleCap(0, position + direction, Quaternion.identity, size, EventType.Repaint);
        Handles.color = Color.white;

        if(label != "") {
            GUI.color = color;
            Handles.Label(position + direction, label);
            GUI.color = Color.white;
        }
    }

    /*
     * Draws a handle for adjusting rotation limits in the scene
     * */
    public static float DrawLimitHandle(float limit, Vector3 position, Quaternion rotation, float radius, string label, float openingValue, bool reverseScaleDirection = false) {
        float scaleFactor = reverseScaleDirection ? -1 : 1;

        limit = scaleFactor * Handles.ScaleValueHandle(limit, position, rotation, radius, Handles.SphereHandleCap, 1);

        string labelInfo = label + ": " + limit.ToString();

        // If value is 0, draw a button to 'open' the value, because we cant scale 0
        if(limit == 0) {
            labelInfo = "Open " + label;

            if(Handles.Button(position, rotation, radius * 0.2f, radius * 0.07f, Handles.SphereHandleCap)) {
                limit = openingValue;
            }
        }

        Handles.Label(position, labelInfo);

        return limit;
    }
}

