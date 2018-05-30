using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSapling : Plant {
	public const float MAXIMUM_LIFEPOWER = 50;

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		if (theoreticalGrowth != growth) {
			growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
			(myRenderer as LineRenderer).SetPosition(1, (myRenderer as LineRenderer).GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
			(myRenderer as LineRenderer).startWidth = growth * maxTall + startSize;
		}
		if (growth >= 1) {
			full = true; 
			Tree t = PoolMaster.current.GetTree();
			float lp = lifepower;
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			t.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
			basement = null;
			t.SetLifepower(lp);
			t.SetGrowth(0.2f);
			t.gameObject.SetActive(true);
			t.SetVisibility(true);
			PoolMaster.current.ReturnSaplingToPool(this);
		}
	}

	override public void AddLifepowerAndCalculate( float lifepower ) {
		AddLifepower((int)lifepower);
		growth = lifepower / MAXIMUM_LIFEPOWER;
		(myRenderer as LineRenderer).SetPosition(1, (myRenderer as LineRenderer).GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
		(myRenderer as LineRenderer).startWidth = growth * maxTall + startSize;
		if (growth >= 1) full = true;
	}

	override public void SetLifepower (float p) {
		lifepower = p;
		if (lifepower >= maxLifepower) full = true; else full =false;
		growth = lifepower/ maxLifepower;
		(myRenderer as LineRenderer).SetPosition(1, (myRenderer as LineRenderer).GetPosition(0) + Vector3.up * (growth * maxTall+ startSize));
		(myRenderer as LineRenderer).startWidth = growth * maxTall + startSize;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		(myRenderer as LineRenderer).SetPosition(0, transform.position);
		(myRenderer as LineRenderer).SetPosition(1,transform.position + Vector3.up * startSize);
	}

	virtual public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			myRenderer.enabled = x;
		}
	}

	override public void Annihilate(bool forced) {
		if (basement != null && !forced) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
		}
		PoolMaster.current.ReturnSaplingToPool(this);
	}
}
