using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBlock : SurfaceBlock {
		[SerializeField]
		MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west
		[SerializeField]
		MeshRenderer ceilingRenderer, _surfaceRenderer;

	void Awake() 
	{
		cellsStatus = 0; map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
		for (int i =0; i < map.GetLength(0); i++) {
			for (int j =0; j< map.GetLength(1); j++) map[i,j] = false;
		}
		surfaceRenderer = _surfaceRenderer;
		material_id = 0;
		surfaceObjects = new List<Structure>();
		artificialStructures = 0;
		isTransparent = false;
		visibilityMask = 0; 
	}

	public override void ReplaceMaterial( int newId) {
		material_id = newId;
		if (grassland != null) {
			grassland.Annihilation();
			CellsStatusUpdate();
		}
		Material m = ResourceType.GetMaterialById(newId);
		if (faces != null) {
			foreach (MeshRenderer mr in faces) {
				if (mr == null) continue;
				else mr.material = m;
			}
		}
		surfaceRenderer.material =  m;
	}

	public void CaveBlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int f_up_material_id, int f_down_material_id) {
		// проверки при повторном использовании?
		isTransparent = false;
		myChunk = f_chunk; transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = f_up_material_id;
		Material m = ResourceType.GetMaterialById(material_id);
		if (faces != null) {
			foreach (MeshRenderer mr in faces) {
				if (mr == null) continue;
				else mr.material = m;
			}
		}
		if (ceilingRenderer == null) print ("no ceiling renderer!");
		ceilingRenderer.material= ResourceType.GetMaterialById(material_id);
		surfaceRenderer.material =  ResourceType.GetMaterialById(f_down_material_id);

		type = BlockType.Cave; isTransparent = false;
		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
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
			for (int i = 0; i< 4; i++) {
				if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) {
					if (faces != null && faces[i]!= null) faces[i].enabled = true;
					else CreateFace(i);
				}
				else {if (faces != null && faces[i]!=null) faces[i].enabled = false;}
			}
		if ((renderMask & 16 & visibilityMask) != 0) surfaceRenderer.enabled = true;
		else surfaceRenderer.enabled = false;
		if ((renderMask & 32 & visibilityMask) != 0) ceilingRenderer.enabled = true;
		else ceilingRenderer.enabled = false;
		}
		public void ChangeVisibilityMask (byte index, bool value) { 
			int vm = visibilityMask;
			int im =(byte) (63 - (int)Mathf.Pow(2, index));
			if (value == false) { vm &= im;}
			else {vm = ~vm; vm &= im; vm = ~vm;}
			if (vm != visibilityMask) SetVisibilityMask((byte)vm);
		}



		void CreateFace(int i) {
			if (faces == null) faces =new MeshRenderer[4];
			else {if (faces[i] != null) return;}
			GameObject g = GameObject.Instantiate(PoolMaster.quad_pref) as GameObject;
			faces[i] =g.GetComponent <MeshRenderer>();
			g.transform.parent = transform;
			switch (i) {
			case 0: faces[i].name = "north_plane"; faces[i].transform.localRotation = Quaternion.Euler(0, 180, 0); faces[i].transform.localPosition = new Vector3(0, 0, Block.QUAD_SIZE/2f); break;
			case 1: faces[i].transform.localRotation = Quaternion.Euler(0, 270, 0); faces[i].name = "east_plane"; faces[i].transform.localPosition = new Vector3(Block.QUAD_SIZE/2f, 0, 0); break;
			case 2: faces[i].name = "south_plane"; faces[i].transform.localPosition = new Vector3(0, 0, -Block.QUAD_SIZE/2f); break;
			case 3: faces[i].transform.localRotation = Quaternion.Euler(0, 90, 0);faces[i].name = "west_plane"; faces[i].transform.localPosition = new Vector3(-Block.QUAD_SIZE/2f, 0, 0); break;
			}
			faces[i].material = ResourceType.GetMaterialById(material_id);
			faces[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			if (Block.QUAD_SIZE != 1) faces[i].transform.localScale = Vector3.one * Block.QUAD_SIZE;
			faces[i].enabled = true;
		}

}
