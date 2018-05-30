using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType {TreeSapling, Tree, Crop}

public class Plant : Structure {
	public float lifepower;
	public float maxLifepower {get;protected set;}
	[SerializeField]
	protected float startSize = 0.05f;
	public float maxTall {get; protected set;}
	public bool full {get;protected set;}
	[SerializeField]
	protected float growSpeed = 0.1f;
	public float growth;
	public PlantType plantType{get;protected set;}

	override public void Prepare() {
		innerPosition = SurfaceRect.one; isArtificial = false; type = StructureType.Plant;
		full = false;
		switch ( id ) {
		case WHEAT_CROP_ID:
			plantType = PlantType.Crop;
			lifepower = 1;
			maxLifepower = 10;
			growth = 0;
			GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
			break;
		case TREE_SAPLING_ID:
			plantType = PlantType.TreeSapling;
			lifepower = 1;
			maxLifepower = TreeSapling.MAXIMUM_LIFEPOWER;
			maxTall = 0.1f;
			break;
		case TREE_ID: 
			plantType = PlantType.Tree;
			lifepower = 10;
			maxLifepower = Tree.MAXIMUM_LIFEPOWER;
			maxTall = 0.4f + Random.value * 0.1f;
			maxHp = maxTall * 1000;
			break;	
		}
		growth = 0;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
		if (growth >= 1) full = true;
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
	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		string s =  SaveStructureData() + SavePlantData();
		//if (s.Length != 17) print (s.Length);
		return s;
	}

	protected string SavePlantData() {
		string s = "";
		float f = lifepower/maxLifepower; 
		if (f > 10) {
			f = 9.99f;
			print ("save error : " +lifepower.ToString() + " -  too much lifepower! " + name);
		}
		s += string.Format("{0:d3}", (int)(f * 100f));
		f = growth; 
		if (f > 10) {
			f= 9.99f;
			print ("save error : unexpectable growth");
		}
		s += string.Format("{0:d3}", (int)(f * 100f));
		if (s.Length != 6) print(s);
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
		// PLANT class part
		SetLifepower(int.Parse(s_data.Substring(11,3)) / 100f * maxLifepower );
		float g = int.Parse(s_data.Substring(14,3)) / 100f;
		if (g > 1) {
			//print (s_data + "  " + g.ToString());
		}
		SetGrowth( int.Parse(s_data.Substring(14,3)) / 100f );
	}
	//---------------------------------------------------------------------------------------------------	

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
