using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSapling : Plant {
	public LineRenderer body;
	public const float MAXIMUM_LIFEPOWER = 50;

	void Awake () {
		PrepareStructure();
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.1f;
		full = false;
		type = StructureType.Plant;
		plantType = PlantType.TreeSapling;
	}


	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		if (theoreticalGrowth != growth) {
			growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
			body.SetPosition(1, body.GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
			body.startWidth = growth * maxTall + startSize;
		}
		if (growth >= 1) {
			full = true; 
			Tree t = PoolMaster.current.GetTree();
			t.gameObject.SetActive(true);
			float lp = lifepower;
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			t.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
			basement = null;
			t.SetLifepower(lp);
			PoolMaster.current.ReturnSaplingToPool(this);
		}
	}

	override public void AddLifepowerAndCalculate( float lifepower ) {
		AddLifepower((int)lifepower);
		growth = lifepower / MAXIMUM_LIFEPOWER;
		body.SetPosition(1, body.GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
		body.startWidth = growth * maxTall + startSize;
		if (growth >= 1) full = true;
	}

	override public void SetLifepower (float p) {
		lifepower = p;
		if (lifepower >= maxLifepower) full = true; else full =false;
		growth = lifepower/ maxLifepower;
		body.SetPosition(1, body.GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
		body.startWidth = growth * maxTall + startSize;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		body.SetPosition(0, transform.position);
		body.SetPosition(1,transform.position + Vector3.up * startSize);
	}

	override public void Annihilate(bool forced) {
		if (basement != null && !forced) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
		}
		PoolMaster.current.ReturnSaplingToPool(this);
	}
}
