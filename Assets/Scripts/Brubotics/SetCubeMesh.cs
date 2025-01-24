using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCubeMesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;

		Color[] colors = new Color[vertices.Length];

		for (int i = 0; i < vertices.Length; i++)
			colors[i] = Color.Lerp(Color.gray, Color.white, vertices[i].y);

		mesh.colors = colors;
	}

	// Update is called once per frame
	void Update () {

	}
}
