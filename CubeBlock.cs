using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBlock : Block{
	MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west, 4 - up, 5 - down
	public byte visibilityMask {get;private set;}
	byte renderMask = 0;

	void Awake() {
		visibilityMask = 0; 
	}

	public override void BlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id) {
		isTransparent = false;
		myChunk = f_chunk; transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = f_material_id;
		type = BlockType.Cube;

		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
	}

	public override void Replace( int newId) {
		material_id = newId;
		if (faces != null) {
			for (int i =0; i< 6; i++) {
				if (faces[i] != null) {faces[i].material =  PoolMaster.GetMaterialById(newId);	}
			}
		}
	}

	public void SetRenderBitmask(byte x) {
		if (renderMask != x) {
			renderMask = x;
			if (visibilityMask == 0) return;
			for (int i = 0; i< 6; i++) {
				if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) {
					if (faces != null && faces[i]!= null) faces[i].enabled = true;
					else CreateFace(i);
				}
				else {if (faces != null && faces[i]!=null) faces[i].enabled = false;}
			}
		}
	}

	public void SetVisibilityMask (byte x) {
		visibilityMask = x;
		if (renderMask == 0) return;
		for (int i = 0; i< 6; i++) {
			if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) {
				if (faces != null && faces[i]!= null) faces[i].enabled = true;
				else CreateFace(i);
			}
			else {if (faces != null && faces[i]!=null) faces[i].enabled = false;}
		}
	}
	public void ChangeVisibilityMask (byte index, bool value) { 
		int vm = visibilityMask;
		int im =(byte) (63 - (int)Mathf.Pow(2, index));
		if (value == false) { vm &= im;}
		else {vm = ~vm; vm &= im; vm = ~vm;}
		if (vm != visibilityMask) SetVisibilityMask((byte)vm);
	}

	void CreateFace(int i) {
		if (faces == null) faces =new MeshRenderer[6];
		else {if (faces[i] != null) return;}
		GameObject g = GameObject.Instantiate(PoolMaster.quad_pref) as GameObject;
		faces[i] =g.GetComponent <MeshRenderer>();
		g.transform.parent = transform;
		switch (i) {
		case 0: faces[i].name = "north_plane"; faces[i].transform.localRotation = Quaternion.Euler(0, 180, 0); faces[i].transform.localPosition = new Vector3(0, 0, Block.QUAD_SIZE/2f); break;
		case 1: faces[i].transform.localRotation = Quaternion.Euler(0, 270, 0); faces[i].name = "east_plane"; faces[i].transform.localPosition = new Vector3(Block.QUAD_SIZE/2f, 0, 0); break;
		case 2: faces[i].name = "south_plane"; faces[i].transform.localPosition = new Vector3(0, 0, -Block.QUAD_SIZE/2f); break;
		case 3: faces[i].transform.localRotation = Quaternion.Euler(0, 90, 0);faces[i].name = "west_plane"; faces[i].transform.localPosition = new Vector3(-Block.QUAD_SIZE/2f, 0, 0); break;
		case 4: 
				faces[i].transform.localPosition = new Vector3(0, Block.QUAD_SIZE/2f, 0); 
				faces[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
				faces[i].name = "upper_plane"; 
			break;
		case 5: 
			faces[i].transform.localRotation = Quaternion.Euler(-90, 0, 0); 
			faces[i].name = "bottom_plane"; 
			faces[i].transform.localPosition = new Vector3(0, -Block.QUAD_SIZE/2f, 0); 
			GameObject.Destroy( faces[i].gameObject.GetComponent<MeshCollider>() );
			break;
		}
		faces[i].material = PoolMaster.GetMaterialById(material_id);
		faces[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		if (Block.QUAD_SIZE != 1) faces[i].transform.localScale = Vector3.one * Block.QUAD_SIZE;
		faces[i].enabled = true;
	}

}
