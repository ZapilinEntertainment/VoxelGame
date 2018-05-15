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


	void Awake() {
		PrepareStructure();
	}

	protected void PrepareStructure() {
		hp = maxHp;
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
		isBasement = useAsBasement;
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
			ChunkPos npos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
			Block upperBlock = basement.myChunk.GetBlock(npos.x, npos.y, npos.z);
			if ( upperBlock == null ) basement.myChunk.AddBlock(npos, BlockType.Surface, ResourceType.metal_K.ID);
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

	void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
	}
}
