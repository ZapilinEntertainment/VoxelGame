using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType {Tree, Crop}

[System.Serializable]
public class PlantSerializer {
	public float lifepower, growth, maxLifeTransfer, growSpeed;
}

public class Plant : Structure {
	public int plant_ID{get;protected set;}
	public float lifepower;
	public float lifepowerToGrow {get;protected set;}  // fixed by id
	public int maxLifeTransfer{get;protected set;} 
	protected float growSpeed, decaySpeed; // fixed by id
	public float growth;
	public byte stage;
	protected byte maxStage;
	public byte harvestableStage{get;protected set;}

	public const int CROP_CORN_ID = 1, TREE_OAK_ID = 2;

	public static Plant GetNewPlant(int id) {
		Plant p = null;
		switch (id) {
		case CROP_CORN_ID: p = new GameObject().AddComponent<Corn>(); break;
		case TREE_OAK_ID: p = new GameObject().AddComponent<OakTree>();break;
		}
		p.id = PLANT_ID;
		p.Prepare();
		return p;
	}

	virtual public void Reset() {
		lifepower = GetCreateCost(id);
		lifepowerToGrow = 1;
		stage = 0;
		growth = 0;
		maxStage = stage;
	}

	override public void Prepare() {		
		PrepareStructure();
		innerPosition = SurfaceRect.one; isArtificial = false; 
		lifepower = GetCreateCost(id);
		growth = 0;
	}

	public static int GetCreateCost(int id) {
		switch (id) {
		case CROP_CORN_ID: return Corn.CREATE_COST; break;
		case TREE_OAK_ID: return OakTree.CREATE_COST; break;
		default: return 1;
		}
	}
	static public float GetLifepowerLevelForStage(byte st) {
		return 1;
	}

	void Update() {
		float t= GameMaster.gameSpeed * Time.deltaTime;
		if (t == 0) return;
		float theoreticalGrowth = lifepower / lifepowerToGrow;
		if (growth < theoreticalGrowth) {
			growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * t);
		}
		else {
			lifepower -= decaySpeed * t;
			if (lifepower == 0) Dry();
		}
		if (growth >= 1 & stage < maxStage) SetStage((byte)(stage+1));
	}	

	#region lifepower operations
	public virtual void AddLifepower(int life) {
		lifepower += life;
	}
	public virtual void AddLifepowerAndCalculate(int life) {
		lifepower += life;
		growth = lifepower / lifepowerToGrow;
	}
	public virtual int TakeLifepower(int life) {
		int lifeTransfer = life;
		if (life > lifepower) {if (lifepower >= 0) lifeTransfer = (int)lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		return lifeTransfer;
	}
	virtual public void SetLifepower(float p) {
		lifepower = p; 
	}
	public virtual void SetGrowth(float t) {
		growth = t;
	}
	virtual protected void SetStage( byte newStage) {
		if (newStage == stage) return;
		stage = newStage;
		growth = 0;
	}
	#endregion

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetPlantSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss,sblock);
		PlantSerializer ps = new PlantSerializer();
		GameMaster.DeserializeByteArray<PlantSerializer>(ss.specificData, ref ps);
		LoadPlantData(ps);
	}

	protected void LoadPlantData(PlantSerializer ps) {
		lifepower = ps.lifepower;
		SetGrowth(ps.growth);
	}

	protected PlantSerializer GetPlantSerializer() {
		PlantSerializer ps = new PlantSerializer();
		ps.growth = growth;
		ps.lifepower = lifepower;
		return ps;
	}
	#endregion

	virtual public void Dry() {
		Annihilate(false);
	}

	virtual public void Harvest() {
		// аннигиляция со сбором ресурсов
	}

	public override void Annihilate( bool forced ) {
		if (basement != null && !forced ) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
			basement.RemoveStructure(this);
		}
		Destroy(gameObject);
	}
}
