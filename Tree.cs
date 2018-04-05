using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Plant {
	public Transform crone, trunk;
	public const int MAXIMUM_LIFEPOWER = 1000;

	void Awake() {
		PrepareStructure();
		full = false;
		lifepower = 1;
		maxLifepower = MAXIMUM_LIFEPOWER;
		maxTall = 0.4f + Random.value * 0.1f;
		maxHp = maxTall * 1000;
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

	override public void SetLifepower (float p) {
		lifepower = p;
		if (lifepower >= maxLifepower) full = true; else full =false;
		if (maxLifepower == 0) maxLifepower = MAXIMUM_LIFEPOWER;
		growth = lifepower/ maxLifepower;
		transform.localScale = Vector3.one * (growth * maxTall + startSize);	
	}

	public virtual int TakeLifepower(int life) {
		float lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		if (lifepower < maxLifepower) {
			full = false;
			if (lifepower <= 0) Dry();
		}
		return (int)lifeTransfer;
	}

	public override void Annihilate() {
		if (basement != null) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
			basement =null;
		}
		PoolMaster.current.ReturnTreeToPool(this);
	}

	public void Dry() { 
		GameObject g = Instantiate(Resources.Load<GameObject>("Lifeforms/DeadTree")) as GameObject;
		g.transform.localScale = transform.localScale;
		HarvestableResource hr = g.GetComponent<HarvestableResource>();
		hr.SetResources(ResourceType.Lumber, (int)(100 * trunk.transform.localScale.y));
		basement.ReplaceStructure(new SurfaceObject(innerPosition, hr));
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
		return SurfaceBlock.INNER_RESOLUTION * transform.localScale.y * transform.localScale.x * transform.localScale.z * 200;
	}
}
