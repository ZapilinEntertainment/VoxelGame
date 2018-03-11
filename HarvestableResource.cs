using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableResource : Structure {
	public ResourceType mainResource {get;protected set;}
	public int count1 {get;protected set;}


	void Awake() {
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		count1 = 0;
	}

	public void SetResources(ResourceType main, int f_count1) {
		mainResource = main;
		count1 = f_count1;
	}

}
