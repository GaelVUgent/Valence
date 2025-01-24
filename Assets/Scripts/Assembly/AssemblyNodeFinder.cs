using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Editor script for automatically finding holes in the mesh of 
/// an assembly that can act as nodes to connect them to eachother.
/// Each node will be assigned whatever gameobject is provided as a node prefab,
/// clones of this prefab will be parented under the mesh objects at the location 
/// of the holes.
/// The holes are detected by looking for small clusters of vertices in the mesh,
/// which typically correspond with holes.
/// For this approach to work the mesh has to be clean, with none or few redundant 
/// vertices in the faces or edges of each object represented by the mesh(es).
/// </summary>
public class AssemblyNodeFinder : MonoBehaviour
{

    public GameObject nodePrefab;
    public float maxVertexDist = 0.01f;
    public float minVertexDist = 1e-5f;
    public float minClusterDist = 0.05f;
    public float maxClusterSize = 0.1f;
    public float twoSidedThreshold = 0.0005f;
    public int minClusterCount = 8;
    public float nodeSize = 1f;
    public string nodeName = "Node";

    /// <summary>
    /// Main search method
    /// </summary>
    private void Search()
    {
        minClusterCount = Mathf.Max(3, minClusterCount);
        List<Cluster> clusters = new List<Cluster>();
        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
        {
            if (!IsPartOfNode(mf.gameObject))
                clusters.AddRange(Search(mf.transform, mf.sharedMesh));
        }

        //filter clusters based on their properties
        List<Cluster> validClusters = new List<Cluster>();
        foreach (Cluster c in clusters)
        {
            c.Finish(minClusterCount);
            if (c.count < minClusterCount | c.size > maxClusterSize)
                continue;
            float d2 = Mathf.Infinity;
            foreach (Cluster cv in validClusters)
                d2 = Mathf.Min((cv.position - c.position).sqrMagnitude);
            if (d2 < minClusterDist * minClusterDist)
                continue;
            validClusters.Add(c);
        }

        int totalCount = validClusters.Count;
        int digits = 1 + Mathf.FloorToInt(Mathf.Log10(totalCount));
        int count = 1;
        foreach (Cluster c in validClusters)
            CreateNode(c, count++, digits);
    }

    /// <summary>
    /// Create a node representing the given cluster.
    /// Extra information is needed to accurately name the new node.
    /// </summary>
    private void CreateNode(Cluster c, int count, int digits)
    {
        GameObject node = Instantiate(nodePrefab, c.parent);
        node.name = nodeName + "_" + count.ToString().PadLeft(digits, '0');
        node.transform.localPosition = c.center;
        if (c.normal != Vector3.zero)
            node.transform.localRotation = Quaternion.LookRotation(c.normal);
        node.transform.localScale = (c.size / nodeSize) * Vector3.one;
        AssemblyNode an = node.GetComponent<AssemblyNode>();
        if (an != null)
        {
            an.twoSided = c.thickness > twoSidedThreshold;
            an.thickness = an.twoSided ? c.thickness : 0f;
        }
        node.gameObject.SetActive(true);
    }

    /// <summary>
    /// Clear all nodes created with the current name
    /// </summary>
    private void Clear()
    {
        Clear(transform);
    }

    private void Clear(Transform t)
    {
        if (IsPartOfNode(t.gameObject))
            DestroyImmediate(t.gameObject);
        else
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                Clear(t.GetChild(i));
        }
    }

    private bool IsPartOfNode(GameObject g)
    {
        //TODO handle case where g is child of node object
        bool match = g.name.ToLower().Contains(nodeName.ToLower());
        match &= g != nodePrefab.gameObject;
        return match;
    }

    /// <summary>
    /// Search for vertex clusters in the given mesh
    /// </summary>
    private List<Cluster> Search(Transform parent, Mesh mesh)
    {
        int n = mesh.vertexCount;
        List<Cluster> clusters = new List<Cluster>();
        foreach (Vector3 vertex in mesh.vertices)
        {
            if (!TryAdd(clusters, vertex))
                clusters.Add(new Cluster(parent, vertex));
        }
        return clusters;
    }

    /// <summary>
    /// Try adding the given vertex to this cluster. This only works 
    /// if the vertex is nearby enough.
    /// </summary>
    private bool TryAdd(List<Cluster> clusters, Vector3 v)
    {
        foreach (Cluster c in clusters)
        {
            if (c.AddInRange(v, maxVertexDist, minVertexDist))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Helper class for encoding a cluster of vertices
    /// </summary>
    private class Cluster
    {

        private List<Vector3> vertices;
        public readonly Transform parent;

        //properties
        public int count { get { return vertices.Count; } }
        public Vector3 center { get; private set; }
        public Vector3 normal { get; private set; }
        public float size { get; private set; }
        public float thickness { get; private set; }
        public Vector3 position { get { return parent.localToWorldMatrix.MultiplyPoint(center); } }

        public Cluster(Transform parent, Vector3 v)
        {
            this.parent = parent;
            vertices = new List<Vector3>();
            vertices.Add(v);
        }

        public bool AddInRange(Vector3 v, float maxD, float minD)
        {
            //work with square distances
            maxD = maxD * maxD;
            minD = minD * minD;

            bool inRange = false;
            foreach (Vector3 vd in vertices)
            {
                float nextD = (vd - v).sqrMagnitude;
                if (nextD < minD)
                    return true;
                inRange |= nextD < maxD;
            }
            if (inRange)
            {
                vertices.Add(v);
                return true;
            }
            return false;
        }

        public void Finish(int countThreshold)
        {
            if (count < countThreshold)
                return;

            //center
            center = Vector3.zero;
            foreach (Vector3 v in vertices)
                center += v;
            center /= count;

            //size: median distance to center
            List<float> d = new List<float>(count);
            foreach (Vector3 v in vertices)
                d.Add((v - center).sqrMagnitude);
            d.Sort();
            size = 2f * Mathf.Sqrt(d[count / 2]);

            //normal
            normal = GetNormal();

            //thickness: median distance along normal
            for (int i = 0; i < count; i++)
                d[i] = Mathf.Abs(Vector3.Dot(vertices[i] - center, normal));
            d.Sort();
            thickness = d[count / 2];
        }

        /// <summary>
        /// Get normal vector of a plane fitted to the points in this cluster
        /// </summary>
        private Vector3 GetNormal()
        {
            float xx = 0f, xy = 0f, xz = 0f, yy = 0f, yz = 0f, zz = 0f;
            foreach (Vector3 v in vertices)
            {
                Vector3 r = v - center;
                xx += r.x * r.x;
                xy += r.x * r.y;
                xz += r.x * r.z;
                yy += r.y * r.y;
                yz += r.y * r.z;
                zz += r.z * r.z;
            }

            float det_x = yy * zz - yz * yz;
            float det_y = xx * zz - xz * xz;
            float det_z = xx * yy - xy * xy;

            float det_max = Mathf.Max(det_x, det_y, det_z);
            //degenerate case, points do not span a plane
            if (det_max <= 0f)
            {
                Debug.LogWarning("Some clusters did not have enough points to span a plane");
                Debug.Log(vertices.Count);
                Debug.Log(xx + " " + yy + " " + zz + " " + xy + " " + yz + " " + zz);
                return Vector3.zero;
            }

            // Pick path with best conditioning:
            Vector3 normal;
            if (det_max == det_x)
                normal = new Vector3(det_x, xz * yz - xy * zz, xy * yz - xz * yy);
            else if (det_max == det_y)
                normal = new Vector3(xz * yz - xy * zz, det_y, xy * xz - yz * xx);
            else
                normal = new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, det_z);

            //normalize manually, since Unity zeroes vectors with such small components
            float l = normal.magnitude;
            if (l == 0f)
            {
                Debug.LogWarning("Some points were too close to eachother to find plane normal");
                return Vector3.zero;
            }
            return normal / normal.magnitude;
        }
    }


#if UNITY_EDITOR

    //provide controls in the Unity Editor.

    [UnityEditor.CustomEditor(typeof(AssemblyNodeFinder))]
    public class AssemblyNodeFinderEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AssemblyNodeFinder anf = (AssemblyNodeFinder)target;

            if (GUILayout.Button("Find node points in children"))
                anf.Search();

            if (GUILayout.Button("Delete nodes by name"))
                anf.Clear();
        }

    }

#endif
}
