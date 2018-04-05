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
	public StructureType type {get;protected set;}
	public float hp = 1;
	public float maxHp = 1;
	public string structureName = "structure";


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
		if (xsize_to_set == 1 && zsize_to_set == 1) b.AddStructure(this, pos);
		else	b.AddStructure(new SurfaceObject(innerPosition, this));
	} 

	public void UnsetBasement() {
		basement = null;
		innerPosition = new SurfaceRect(0,0,xsize_to_set,zsize_to_set);
	}

	virtual public void Annihilate() { // for pooling
		Destroy(gameObject);
	}

	void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
	}
}
