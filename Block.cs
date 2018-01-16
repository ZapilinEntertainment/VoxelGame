using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {
	public readonly GameObject body;
	byte renderMask = 63; // bitmask
	readonly int f_id = 0;
	MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west, 4 - up, 5 - down
	static readonly Mesh blockside;
	static readonly Material defMaterial;
	byte visibilityMask = 63;
	Chunk myChunk;
	bool isTransparent = false;

	static Block () {
		blockside = new Mesh();
		blockside.vertices = new Vector3[]{new Vector3(-0.5f, 0.5f,0), new Vector3 (0.5f, 0.5f,0), new Vector3(0.5f, -0.5f,0), new Vector3(-0.5f, -0.5f,0)};
		blockside.triangles = new	 int[] {2,1,0,2,0,3};
		blockside.uv = new Vector2[] {new Vector2 (0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)};
		blockside.RecalculateNormals();
		defMaterial = new Material(Shader.Find("Diffuse"));
	}

	public Block() {
		body = new GameObject("block");
		renderMask = 0;
		f_id =0;
	}

	public Block (int id) {
		body = new GameObject("block");
		MeshFilter mf;
		faces = new MeshRenderer[6];
		for (int i = 0; i < 6; i++) {
			GameObject g = new GameObject ();
			faces[i] =g.AddComponent <MeshRenderer>();
			faces[i].material = defMaterial;
			mf = g.AddComponent<MeshFilter>();
			mf.mesh = blockside;
			g.transform.parent = body.transform;
			switch (i) {
			case 0: faces[i].name = "north_plane"; faces[i].transform.localPosition = new Vector3(0, 0, 0.5f); break;
			case 1: faces[i].transform.localRotation = Quaternion.Euler(0, 90, 0); faces[i].name = "east_plane"; faces[i].transform.localPosition = new Vector3(0.5f, 0, 0); break;
			case 2: faces[i].transform.localRotation = Quaternion.Euler(0, 180, 0); faces[i].name = "south_plane"; faces[i].transform.localPosition = new Vector3(0, 0, -0.5f); break;
			case 3: faces[i].transform.localRotation = Quaternion.Euler(0, 270, 0);faces[i].name = "west_plane"; faces[i].transform.localPosition = new Vector3(-0.5f, 0, 0); break;
			case 4: faces[i].transform.localRotation = Quaternion.Euler(-90, 0, 0);faces[i].name = "upper_plane"; faces[i].transform.localPosition = new Vector3(0, 0.5f, 0); break;
			case 5: faces[i].transform.localRotation = Quaternion.Euler(90, 0, 0); faces[i].name = "bottom_plane"; faces[i].transform.localPosition = new Vector3(0, -0.5f, 0); break;
		}
	}
		renderMask = 0;
		f_id = id;
}

	public void SetRenderBitmask(byte x) {
		if (renderMask != x) {
			renderMask = x;
			if (visibilityMask == 0) return;
		for (int i = 0; i< 6; i++) {
				if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true; else faces[i].enabled = false;
		}
		}
	}

	public void SetVisibilityMask (byte x) {
		visibilityMask = x;
		for (int i = 0; i< 6; i++) {
			if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true; else faces[i].enabled = false;
		}
	}
	public void ChangeVisibilityMask (byte index, bool value) { 
		int vm = visibilityMask;
		int im =(byte) (63 - (int)Mathf.Pow(2, index));
		if (value == false) { vm &= im;}
		else {vm = ~vm; vm &= im; vm = ~vm;}
		if (vm != visibilityMask) SetVisibilityMask((byte)vm);
	}
		
	public void SetChunk(Chunk c) {myChunk = c;}
	public bool IsTransparent() {return isTransparent;}

	public void Destroy() {
		
		for (int i = 0; i< 6; i++) {
			if ((visibilityMask & ((int)Mathf.Pow(2, i))) != 0) faces[i].enabled = true; else faces[i].enabled = false;
		}
		GameObject.Destroy(body);
		if (faces != null) foreach(MeshRenderer g in faces) GameObject.Destroy(g.gameObject);
	}
}
