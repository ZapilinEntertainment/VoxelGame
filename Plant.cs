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
	public float growth;
	public PlantType plantType{get;protected set;}

	void Awake() {
		PrepareStructure();
		lifepower = 1;
		maxLifepower = 10;
		full = false;
		maxTall = 0.15f + Random.value * 0.05f;
		growth = 0;
		type = StructureType.Plant;
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

	public virtual void AddLifepower(float life) {
		if (full) return;
		lifepower += (int)life;
		if (lifepower >= maxLifepower) {full = true; growth = lifepower/ maxLifepower;}
	}
	public virtual void AddLifepowerAndCalculate(float life) {
		AddLifepower((int)life);
	}
		
	public virtual int TakeLifepower(float life) {
		float lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		if (lifepower < maxLifepower) {
			full = false;
			if (lifepower <= 0) Dry();
		}
		return (int)lifeTransfer;
	}


	virtual protected void Dry() {
		Annihilate( false );
	}

	virtual public void SetLifepower(float p) {
		lifepower = p; 
		growth = lifepower/ maxLifepower;
		if (lifepower < maxLifepower) full = false; else full = true;
	}

	public override void Annihilate( bool forced ) {
		if (basement != null && !forced ) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
		}
		basement =null;
		Destroy(gameObject);
	}

	void OnDestroy() {
	}
}
