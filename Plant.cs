using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType {TreeSapling, Tree, Crop}

[System.Serializable]
public class PlantSerializer {
	public float lifepower, growth;
	public bool full;
}

public class Plant : Structure {
	public float lifepower;
	public float maxLifepower {get;protected set;}  // fixed by id
	[SerializeField]
	protected float startSize = 0.05f; // fixed by asset
	public float maxTall {get; protected set;} //fixed by id
	public bool full {get;protected set;}
	protected float growSpeed = 0.1f; // fixed by id
	public float growth;
	public PlantType plantType{get;protected set;} // fixed by id

	override public void Prepare() {
		innerPosition = SurfaceRect.one; isArtificial = false; type = StructureType.Plant;
		full = false;
		switch ( id ) {
		case WHEAT_CROP_ID:
			plantType = PlantType.Crop;
			lifepower = 1;
			maxLifepower = 10;
			growth = 0;
			growSpeed = 0.01f;
			GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
			break;
		case TREE_SAPLING_ID:
			plantType = PlantType.TreeSapling;
			lifepower = 1;
			maxLifepower = TreeSapling.MAXIMUM_LIFEPOWER;
			maxTall = 0.1f;
			growSpeed = 0.03f;
			break;
		case TREE_ID: 
			plantType = PlantType.Tree;
			lifepower = 10;
			maxLifepower = Tree.MAXIMUM_LIFEPOWER;
			maxTall = 0.4f + Random.value * 0.1f;
			maxHp = maxTall * 1000;
			growSpeed = 0.05f;
			break;	
		}
		growth = 0;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
	}	

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
	}

	public virtual void AddLifepower(float life) {
		if (full) return;
		lifepower += (int)life;
		if (lifepower >= maxLifepower) full = true;  else full =false;
	}
	public virtual void AddLifepowerAndCalculate(float life) {
		lifepower += (int) life;
		if (lifepower >= maxLifepower) full = true;  else full =false;
		SetGrowth( lifepower / maxLifepower);
	}
	public virtual void SetGrowth(float t) {
		growth = t;
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
		if (lifepower < maxLifepower) full = false; else full = true;
	}
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
		full = ps.full;
		SetGrowth(ps.growth);
	}

	protected PlantSerializer GetPlantSerializer() {
		PlantSerializer ps = new PlantSerializer();
		ps.full = full;
		ps.growth = growth;
		ps.lifepower = lifepower;
		return ps;
	}
	#endregion

	public override void Annihilate( bool forced ) {
		if (basement != null && !forced ) {
			basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
			basement.RemoveStructure(this);
		}
		Destroy(gameObject);
	}
}
