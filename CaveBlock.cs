using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CaveBlockSerializer {
	public SurfaceBlockSerializer surfaceBlockSerializer;
	public int upMaterial_ID;
	public bool surfaceEnabled;
}

public class CaveBlock : SurfaceBlock {
		[SerializeField]
		MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west
		[SerializeField] MeshRenderer ceilingRenderer, _surfaceRenderer; // fiti

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
		foreach (MeshRenderer mr in faces) {
			if (mr == null) continue;
			else mr.sharedMaterial = ResourceType.GetMaterialById(newId, mr.GetComponent<MeshFilter>()); 
		}
		surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(newId, surfaceRenderer.GetComponent<MeshFilter>());
	}

	public void CaveBlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int f_up_material_id, int f_down_material_id) {
		// проверки при повторном использовании?
		isTransparent = false;
		myChunk = f_chunk; transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = f_up_material_id;
		foreach (MeshRenderer mr in faces) {
				if (mr == null) continue;
				else mr.sharedMaterial = ResourceType.GetMaterialById(material_id, mr.GetComponent<MeshFilter>()); ;
        }
		if (ceilingRenderer == null) print ("no ceiling renderer!");
		ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(material_id, ceilingRenderer.GetComponent<MeshFilter>());
		surfaceRenderer.sharedMaterial =  ResourceType.GetMaterialById(f_down_material_id, surfaceRenderer.GetComponent<MeshFilter>());

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
		byte prevVisibility = visibilityMask;
		if (visibilityMask == x) return;
			visibilityMask = x;
			if (visibilityMask == 0) {
				int i = 0; bool listChanged = false;
				while ( i < surfaceObjects.Count ) {
					if (surfaceObjects[i] == null || !surfaceObjects[i].gameObject.activeSelf ) {
						surfaceObjects.RemoveAt(i);
						listChanged = true;
						continue;
					}
					else {
						surfaceObjects[i].SetVisibility(false);
						i++;
					}
				} 
				surfaceRenderer.GetComponent<MeshCollider>().enabled = false;
				if (listChanged) CellsStatusUpdate();
			}
		else {
			if (prevVisibility == 0) {
				int i = 0; bool listChanged = false;
				while ( i < surfaceObjects.Count ) {
					if (surfaceObjects[i] == null || !surfaceObjects[i].gameObject.activeSelf ) {
						surfaceObjects.RemoveAt(i);
						listChanged = true;
						continue;
					}
					else {
						surfaceObjects[i].SetVisibility(true);
						i++;
					}
				} 
				if (listChanged) CellsStatusUpdate();
				surfaceRenderer.GetComponent<MeshCollider>().enabled = true;
			}
		}
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

	#region save-load system
	override public BlockSerializer Save() {
		BlockSerializer bs = GetBlockSerializer();
		CaveBlockSerializer cbs = new CaveBlockSerializer();
		cbs.upMaterial_ID = material_id;
		cbs.surfaceEnabled = true;
		cbs.surfaceBlockSerializer = GetSurfaceBlockSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, cbs);
			bs.specificData =  stream.ToArray();
		}
		return bs;
	} 

	override public void Load(BlockSerializer bs) {
		LoadBlockData(bs);
		CaveBlockSerializer cbs = new CaveBlockSerializer();
		GameMaster.DeserializeByteArray<CaveBlockSerializer>(bs.specificData, ref cbs);
		LoadCaveBlockData(cbs);
	}

	protected void LoadCaveBlockData(CaveBlockSerializer cbs) {
		LoadSurfaceBlockData(cbs.surfaceBlockSerializer);
		//cbs.upMaterial_ID
		//cbs.surfaceEnabled
	}
	#endregion
}
