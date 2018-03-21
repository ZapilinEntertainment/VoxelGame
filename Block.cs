using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {Shapeless, Cube, Surface}
public struct AccessConnection{
	public Structure structure;
	public byte accessMask;
	public AccessConnection (Structure f_structure, byte f_mask) {
		structure = f_structure;
		accessMask = f_mask;
	}
}

public class Block : MonoBehaviour {
	public const float QUAD_SIZE = 1;
	public BlockType type {get;protected set;}

	public Chunk myChunk {get; protected  set;}
	public bool isTransparent {get;protected  set;}
	public ChunkPos pos {get; protected  set;}
	public Structure mainStructure{get;protected set;}
	public bool blockedByStructure {get;protected  set;}
	public int material_id {get;protected  set;}
	public bool indestructible {get; protected set;}

	public virtual void ReplaceMaterial(int newId) {
		material_id = newId;
	}

	public void ShapelessBlockSet (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure) {
		isTransparent = true; material_id = 0;
		myChunk = f_chunk; 
		transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		mainStructure = f_mainStructure;
		if (mainStructure != null) blockedByStructure = true; else blockedByStructure = false;
		type = BlockType.Shapeless;
		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
}
	public virtual void BlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int newId) {
		isTransparent = true; material_id = 0;
		myChunk = f_chunk; transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = newId;
		blockedByStructure = false;
		type = BlockType.Shapeless;
		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
	}

	public void MakeIndestructible(bool x) {
		indestructible = x;
	}

	void OnDestroy() {
		if (mainStructure == null) return;
		MultiblockStructure ms =  mainStructure.gameObject.GetComponent<MultiblockStructure>();
		if (ms != null) ms.PartCollapse(pos);
	}
}
