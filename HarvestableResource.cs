using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResourceType {
	public float mass , toughness;
	public bool isBuildingMaterial, isFood;
	public static ResourceType Nothing, Lumber, Stone;

	public ResourceType(float f_mass, float f_toughness, bool useAsBuildingMaterial, bool useAsFood ) {
		mass = f_mass;
		toughness = f_toughness;
		isBuildingMaterial = useAsBuildingMaterial;
		isFood = useAsFood;
	}

	static ResourceType() {
		Nothing = new ResourceType(0, 0, false,false);
		Lumber = new ResourceType(0.5f, 5, true, false);
		Stone = new ResourceType(2.5f, 30, true, false);
	}
}

public class HarvestableResource : Structure {
	public ResourceType mainResource {get;protected set;}
	public ResourceType secondaryResource {get;protected set;}
	public int count1 {get;protected set;}
	public int count2 {get;protected set;}
	public float hp {get;protected set;}
	public float decaySpeed = 0.01f;


	void Awake() {
		mainResource = ResourceType.Nothing; 
		secondaryResource = ResourceType.Nothing; 
		hp = maxHp;
		count1 = 0; count2= 0;
	}

	void Update() {
		if (innerPosition.Equals(SurfaceRect.Empty) == false) {
			hp -= Time.deltaTime * GameMaster.gameSpeed * decaySpeed;
			if (hp <= 0) Destroy(gameObject);
		}
	}

	public void SetResources(ResourceType main, int f_count1, ResourceType secondary,  int f_count2) {
		mainResource = main;
		secondaryResource = secondary;
		count1 = f_count1;
		count2 = f_count2;
	}

}
