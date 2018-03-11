using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {
	protected SurfaceBlock basement; 
	public bool isMainStructure = false;
	public byte xsize_to_set = 1, zsize_to_set =1;
	public SurfaceRect innerPosition {get;protected set;}
	public float hp {get; protected set;}
	public float maxHp = 1;

	void Awake() {
		hp = maxHp;
		innerPosition = SurfaceRect.Empty;
	}

	virtual public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		basement = b;
		Content myContent = Content.Structure; 
		if (isMainStructure) {myContent = Content.MainStructure;}
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set ,zsize_to_set, myContent, gameObject);
		b.AddStructure(innerPosition);
	}

	public void UnsetBasement() {
		basement = null;
	}

	protected virtual void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(innerPosition);
		}
	}
}
