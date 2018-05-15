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
		foreach (MeshRenderer mr in faces) {
			if (mr == null) continue;
			else mr.material = m;
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
		foreach (MeshRenderer mr in faces) {
				if (mr == null) continue;
				else mr.material = m;
		}
		if (ceilingRenderer == null) print ("no ceiling renderer!");
		ceilingRenderer.material= ResourceType.GetMaterialById(material_id);
		surfaceRenderer.material =  ResourceType.GetMaterialById(f_down_material_id);

		type = BlockType.Cave; isTransparent = false;
		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
	}

	override public void SetRenderBitmask(byte x) {
			if (renderMask != x) {
				renderMask = x;
				if (visibilityMask == 0) return;
				for (int i = 0; i< 4; i++) {
					if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true;
					else faces[i].enabled = false;
				}
			if ((renderMask & 15) == 0) {
				ceilingRenderer.enabled = false;
				surfaceRenderer.enabled = false;
			}
			else {
				ceilingRenderer.enabled = true;
				surfaceRenderer.enabled = true;
			}
			if ( structureBlock != null) structureBlock.SetRenderBitmask(x); 
			}
		}

		override public void SetVisibilityMask (byte x) {
			visibilityMask = x;
			if (renderMask == 0) return;
			for (int i = 0; i< 4; i++) {
				if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true;
				else faces[i].enabled = false;
			}
			if ((renderMask & 15) == 0) {
				ceilingRenderer.enabled = false;
				surfaceRenderer.enabled = false;
			}
			else {
				ceilingRenderer.enabled = true;
				surfaceRenderer.enabled = true;
			}
			
			if ( structureBlock != null) structureBlock.SetVisibilityMask(x);
		}
}
