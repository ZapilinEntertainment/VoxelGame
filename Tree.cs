﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Plant {
	public const int MAXIMUM_LIFEPOWER = 1000;

	void Awake() {
		PrepareStructure();
		full = false;
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.4f + Random.value * 0.1f;
		maxHp = maxTall * 1000;
		type = StructureType.Plant;
		plantType = PlantType.Tree;
	}


	void Update() {
		if (full || GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		if (theoreticalGrowth < 0) theoreticalGrowth = 0;
		if (theoreticalGrowth != growth) {
			growth = Mathf.MoveTowards(growth, theoreticalGrowth,  Mathf.Cos(growth * Mathf.PI/2f) * growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
			if (growth >= 1) full = true; 
			hp = growth * maxHp;
			transform.localScale = Vector3.one * (growth * maxTall + startSize);	
		}
	}	
	override public void AddLifepowerAndCalculate( float lifepower ) {
		AddLifepower((int)lifepower);
		growth = lifepower / MAXIMUM_LIFEPOWER;
		hp = growth * maxHp;
		transform.localScale = Vector3.one * (growth * maxTall + startSize);	
		if (growth >= 1) full = true;
	}
	override public void SetLifepower (float p) {
		lifepower = p;
		if (maxLifepower == 0) maxLifepower = MAXIMUM_LIFEPOWER;
		if (lifepower >= maxLifepower) full = true; else full =false;
		growth = lifepower/ maxLifepower;
		transform.localScale = Vector3.one * (growth * maxTall + startSize);	
	}
		
	public override void Annihilate( bool forced ) {
		if (basement != null && !forced) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
		}
		basement =null;
		PoolMaster.current.ReturnTreeToPool(this);
	}

	override protected void Dry() { 
		GameObject g = Instantiate(Resources.Load<GameObject>("Lifeforms/DeadTree")) as GameObject;
		g.transform.localScale = transform.localScale;
		HarvestableResource hr = g.GetComponent<HarvestableResource>();
		hr.SetResources(ResourceType.Lumber, (int)(100 * transform.localScale.y));
		hr.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
	}

	public void Chop() {
		if (basement != null) {
			basement.grassland.AddLifepower((int)lifepower);
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
		gameObject.AddComponent<FallingTree>();
		Destroy(this); // script "Tree"
	}

	public float CalculateLumberCount() {
		return SurfaceBlock.INNER_RESOLUTION * transform.localScale.y * transform.localScale.x * transform.localScale.z * 100;
	}
}
