using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType {NotAssigned, Plant, HarvestableResources, Structure, MainStructure}
public class Structure : MonoBehaviour {
	[SerializeField]
	protected bool markAsArtificial = false, useAsBasement = false;
	[SerializeField]
	protected byte xsize_to_set = 1, zsize_to_set =1;
	[SerializeField]
	protected StructureType setType = StructureType.NotAssigned;

	public SurfaceBlock basement{get;protected set;}
	public SurfaceRect innerPosition {get;protected set;}
	public bool isArtificial {get;protected set;}
	public bool isBasement{get;protected set;}
	public bool undestructible = false;
	public StructureType type {get;protected set;}
	public float hp = 1;
	public float maxHp = 1;
	public int nameIndex = 0;
	public bool randomRotation = false, rotate90only = true;
	public bool showOnGUI = false;
	public float gui_ypos = 0;
	[SerializeField]
	protected Renderer myRenderer;
	public bool visible {get;protected set;}

	void Awake() {
		PrepareStructure();
	}

	protected void PrepareStructure() {
		hp = maxHp;
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
		isBasement = useAsBasement;
		visible = true;
	}

	virtual public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		if ( isBasement ) basement.myChunk.chunkUpdateSubscribers.Add(this);
	}
	protected void SetStructureData(SurfaceBlock b, PixelPosByte pos) {
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		if (xsize_to_set == 1 && zsize_to_set == 1) b.AddCellStructure(this, pos);
		else	b.AddStructure(this);
		if (isBasement) {
			if (basement.pos.y + 1 < Chunk.CHUNK_SIZE) {
				ChunkPos npos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
				Block upperBlock = basement.myChunk.GetBlock(npos.x, npos.y, npos.z);
				if ( upperBlock == null ) basement.myChunk.AddBlock(npos, BlockType.Surface, ResourceType.metal_K.ID);
			}
		}
	} 

	public void UnsetBasement() {
		if ( isBasement ) {
			int i = 0;
			List <Component> clist = basement.myChunk.chunkUpdateSubscribers;
			while ( i < clist.Count ) {
				if (clist[i] == this) {
					clist.RemoveAt(i);
					continue;
				}
				i++;
			}
		}
		basement = null;
		innerPosition = new SurfaceRect(0,0,xsize_to_set,zsize_to_set);
		transform.parent = null;
	}

	/// <summary>
	/// forces means that this object will be deleted without basement-linked actions
	/// </summary>
	/// <param name="forced">If set to <c>true</c> forced.</param>
	virtual public void Annihilate( bool forced ) { // for pooling
		if (forced) basement = null;
		Destroy(gameObject);
	}

	public void ApplyDamage(float d) {
		hp -= d;
		if ( hp <= 0 ) Annihilate(false);
	}

	public void ChunkUpdated() {
		if ( !isBasement || basement == null) return;
		Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y+1, basement.pos.z);
		if (upperBlock == null) {
			basement.myChunk.AddBlock( new ChunkPos(basement.pos.x, basement.pos.y+1, basement.pos.z), BlockType.Surface, ResourceType.CONCRETE_ID);
		}
	}

	virtual public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			myRenderer.enabled = x;
			Collider c = gameObject.GetComponent<Collider>();
			if ( c != null ) c.enabled = x;
		}
	}

	virtual public string SaveStructure() {
		string sdata = "";
		switch (type) {
		case StructureType.NotAssigned: sdata += '0';break;
		case StructureType.Structure: sdata += '1';break;
		case StructureType.MainStructure: sdata += '2';break;
		case StructureType.Plant: sdata += '3';break;
		case StructureType.HarvestableResources: sdata += '4';break;
		}

		return sdata;
	}

	void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
	}
}
