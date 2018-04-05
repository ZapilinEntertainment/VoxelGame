using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant2D : Plant {
	public LineRenderer body;
	public const float MAXIMUM_LIFEPOWER = 50;

	void Awake () {
		PrepareStructure();
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.1f;
		full = false;
	}


	void Update() {
		if (full || GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		if (theoreticalGrowth != growth) {
			growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
			if (growth >= 1) full = true; 
			body.SetPosition(1, body.GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
			body.startWidth = growth * maxTall + startSize;
		}
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

	override public void Annihilate() {
		if (basement != null) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
			basement =null;
		}
		PoolMaster.current.ReturnGrassToPool(this);
	}
}
