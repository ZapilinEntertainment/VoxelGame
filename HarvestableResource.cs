using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableResource : Structure {
	public ResourceType mainResource {get;protected set;}
	public float count1;


	void Awake() {
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		count1 = 0;
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
		PrepareStructure();
	}

	public void SetResources(ResourceType main, float f_count1) {
		mainResource = main;
		count1 = f_count1;
	}

}
