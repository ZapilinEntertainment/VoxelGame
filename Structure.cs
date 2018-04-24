using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType {NotAssigned, Plant, HarvestableResources, Structure, MainStructure}
public class Structure : MonoBehaviour {
	[SerializeField]
	protected bool markAsArtificial = false;
	[SerializeField]
	protected byte xsize_to_set = 1, zsize_to_set =1;
	[SerializeField]
	protected StructureType setType = StructureType.NotAssigned;

	public SurfaceBlock basement{get;protected set;}
	public SurfaceRect innerPosition {get;protected set;}
	public bool isArtificial {get;protected set;}
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
	}

	virtual public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
	}
	protected void SetStructureData(SurfaceBlock b, PixelPosByte pos) {
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		if (xsize_to_set == 1 && zsize_to_set == 1) b.AddCellStructure(this, pos);
		else	b.AddStructure(this);
	} 

	public void UnsetBasement() {
		basement = null;
		innerPosition = new SurfaceRect(0,0,xsize_to_set,zsize_to_set);
		transform.parent = null;
	}

	virtual public void Annihilate( bool forced ) { // for pooling
		if (forced) basement = null;
		Destroy(gameObject);
	}

	void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
	}
}
