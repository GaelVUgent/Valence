using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BoneReordering : MonoBehaviour {

	public SkinnedMeshRenderer original;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			UpdateMeshRenderer (original);
		}
	}

	public void UpdateMeshRenderer (SkinnedMeshRenderer newMeshRenderer)
	{
		// update mesh
		SkinnedMeshRenderer meshrenderer = GetComponentInChildren<SkinnedMeshRenderer> ();
		meshrenderer.sharedMesh = newMeshRenderer.sharedMesh;
		meshrenderer.material = newMeshRenderer.material;

		Transform[] childrens = transform.GetComponentsInChildren<Transform> (true);

		// sort bones.
		Transform[] bones = new Transform[newMeshRenderer.bones.Length];

		for (int boneOrder = 0; boneOrder < newMeshRenderer.bones.Length; boneOrder++) {
			bones [boneOrder] = Array.Find<Transform> (childrens, c => c.name == newMeshRenderer.bones [boneOrder].name);
			//Debug.Log(bones[boneOrder].name);
		}

		/*
	     for (int i = 0; i < bones.Length; i++)
		 {
			 Debug.Log(bones[i].name);
	         for (int a = 0; a < childrens.Length; a++)
			 {
	             if (bones[i].name == childrens[a].name) {
	                 bones[i] = childrens[a];
	                 break;
	             }
			 }
		}*/
		meshrenderer.bones = bones;
	}
}
