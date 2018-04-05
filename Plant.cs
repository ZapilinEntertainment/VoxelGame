using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType {TreeSapling, Tree, Crop}

public class Plant : Structure {
	public float lifepower {get;protected set;}
	public float maxLifepower {get;protected set;}
	[SerializeField]
	protected float startSize = 0.05f;
	public float maxTall {get; protected set;}
	public bool full {get;protected set;}
	[SerializeField]
	protected float growSpeed = 0.1f;
	public float growth{get;protected set;}
	public PlantType plantType;

	void Awake() {
		PrepareStructure();
		lifepower = 1;
		maxLifepower = 10;
		full = false;
		maxTall = 0.15f + Random.value * 0.05f;
		growth = 0;
	}

	void Update() {
		if (full || GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
		if (growth >= 1) full = true;
	}	

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		transform.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
	}

	public virtual void AddLifepower(int life) {
		if (full) return;
		lifepower += life;
		if (lifepower >= maxLifepower) {full = true; growth = lifepower/ maxLifepower;}
	}
		
	public virtual float TakeLifepower(int life) {
		float lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		if (lifepower < maxLifepower) full = false;
		return (int)lifeTransfer;
	}

	virtual public void SetLifepower(float p) {
		lifepower = p; 
		growth = lifepower/ maxLifepower;
		if (lifepower < maxLifepower) full = false; else full = true;
	}

	public override void Annihilate() {
		basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
		basement =null;
		Destroy(gameObject);
	}

	void OnDestroy() {
	}
}
