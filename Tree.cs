using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Plant {
	public const int MAXIMUM_LIFEPOWER = 1000;
	[SerializeField]
	Renderer[] myRenderers;

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		if (theoreticalGrowth < 0) theoreticalGrowth = 0;
		if (theoreticalGrowth != growth) {
			growth = Mathf.MoveTowards(growth, theoreticalGrowth,  Mathf.Cos(Mathf.Clamp01(growth) * Mathf.PI/2f) * growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
			hp = growth * maxHp;
			transform.localScale = Vector3.one * (growth * maxTall + startSize);	
		}
	}	
	override public void AddLifepowerAndCalculate( float lifepower ) {
		AddLifepower((int)lifepower);
		SetGrowth(lifepower / maxLifepower);
	}
	override public void SetLifepower (float p) {
		lifepower = p;
		if (maxLifepower == 0) maxLifepower = MAXIMUM_LIFEPOWER;
		if (lifepower >= maxLifepower) full = true; else full =false;
	}
	public override void SetGrowth(float t) {
		growth = t;
		hp = growth * maxHp;
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
			UnsetBasement();
		}
		if (GetComponent<FallingTree>() == null) gameObject.AddComponent<FallingTree>();
		this.enabled = false;
	}

	public float CalculateLumberCount() {
		return SurfaceBlock.INNER_RESOLUTION * transform.localScale.y * transform.localScale.x * transform.localScale.z * 100;
	}

	override public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			if (myRenderer != null) {
				myRenderer.enabled = x;
				MastSpriter ms = myRenderer.GetComponent<MastSpriter>();
				if (ms!= null) ms.SetVisibility(x);
			}
			if (myRenderers != null) {
				foreach (Renderer r in myRenderers) {
					r.enabled = x;
					MastSpriter ms = r.GetComponent<MastSpriter>();
					if (ms!= null) ms.SetVisibility(x);
					else {
						CroneOptimizer co =  r.GetComponent<CroneOptimizer>();
						if (co != null) co.SetVisibility(x);
						}
				}
			}
		}
	}
		
}
