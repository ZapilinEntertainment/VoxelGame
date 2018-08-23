using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corn : Plant {
	static Sprite[] stageSprites;
	const byte MAX_STAGE = 3;
	public const int CREATE_COST = 1, GATHER = 5;

	void Start() {
		if (stageSprites == null) {
			stageSprites = Resources.LoadAll<Sprite>("Textures/Plants/corn"); // 4 images
			maxStage = MAX_STAGE;
		}
		gameObject.name = "Plant - Corn";
		GameObject spriteSatellite = new GameObject("sprite");
		spriteSatellite.transform.parent = transform;
		spriteSatellite.transform.localPosition = Vector3.zero;
		GameMaster.realMaster.mastSpritesList.Add(spriteSatellite);
        myRenderers = new List<Renderer>();
        SpriteRenderer sr = spriteSatellite.AddComponent<SpriteRenderer>();
        myRenderers.Add( sr );
		sr.sprite = stageSprites[0];
	}

	override public void Prepare() {		
		PrepareStructure();
		plant_ID = CROP_CORN_ID;
		innerPosition = SurfaceRect.one; isArtificial = false; 
		lifepower = CREATE_COST;
		lifepowerToGrow = GetLifepowerLevelForStage(stage);
		maxLifeTransfer = 1;
		growSpeed = 0.03f;
		decaySpeed = growSpeed;
		harvestableStage = 3;
		growth = 0;
	}

	override public void ResetPlant() {
		lifepower = CREATE_COST;
        stage = 0;
        lifepowerToGrow = GetLifepowerLevelForStage(stage);		
		growth = 0;
		hp = maxHp;
		(myRenderers[0] as SpriteRenderer).sprite = stageSprites[0];
	}

	#region lifepower operations
	public override void AddLifepowerAndCalculate(int life) {
		lifepower += life;
		byte nstage = 0;
		float lpg = GetLifepowerLevelForStage(nstage);
		while ( lifepower > lifepowerToGrow & nstage < maxStage) {
			nstage++;
			lpg = GetLifepowerLevelForStage(nstage);
		}
		lifepowerToGrow = lpg;
		SetStage(stage);
	}
	public override int TakeLifepower(int life) {
		int lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = (int)lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		return lifeTransfer;
	}
	override public void SetLifepower(float p) {
		lifepower = p; 
	}
	public override void SetGrowth(float t) {
		growth = t;
	}
	override public void SetStage( byte newStage) {
		if (newStage == stage) return;
		stage = newStage;
		(myRenderers[0] as SpriteRenderer).sprite = stageSprites[stage];
		lifepowerToGrow  = GetLifepowerLevelForStage(stage);
		growth = lifepower / lifepowerToGrow;
	}
	new static public float GetLifepowerLevelForStage(byte st) {
		return (st+1);
	}
	#endregion

	override public void Harvest() {
		GameMaster.colonyController.storage.AddResource(ResourceType.Food, GATHER);
		ResetPlant();
	}

	override public void Dry() {
		Destroy(gameObject);
		Structure s = Structure.GetNewStructure(Structure.DRYED_PLANT_ID);
		basement.AddCellStructure(s, new PixelPosByte(innerPosition.x, innerPosition.z));
	}
}
