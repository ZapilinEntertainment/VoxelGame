using UnityEngine;
[System.Serializable]
public class PlantSerializer {
	public int plant_id;
	public float lifepower, growth;
	public byte stage;
}

public abstract class Plant : Structure {
	public int plant_ID{get;protected set;}
	public float lifepower;
	public float lifepowerToGrow {get;protected set;}  // fixed by id		
	public float growth;
	public byte stage;	
    protected bool addedToClassList = false;

    public const int CROP_CORN_ID = 1, TREE_OAK_ID = 2, 
        TOTAL_PLANT_TYPES = 2;  // при создании нового добавить во все статические функции внизу
    public static int existingPlantsMask = 0;

    public static Plant GetNewPlant(int i_plant_id)
    {
        Plant p;
        switch (i_plant_id)
        {
            default: return null;
            case CROP_CORN_ID: p = new GameObject("Corn").AddComponent<Corn>(); break;
            case TREE_OAK_ID: p = new GameObject("Oak Tree").AddComponent<OakTree>(); break;
        }
        p.id = PLANT_ID;
        p.plant_ID = i_plant_id;
        p.Prepare();
        return p;
    }

    public static int GetCreateCost(int id)
    {
        switch (id)
        {
            case CROP_CORN_ID: return Corn.CREATE_COST;
            case TREE_OAK_ID: return OakTree.CREATE_COST;
            default: return 1;
        }
    }
    public static int GetMaxLifeTransfer(int id) {
        switch (id)
        {
            default: return 1;
            case CROP_CORN_ID: return Corn.maxLifeTransfer;
            case TREE_OAK_ID: return OakTree.maxLifeTransfer;
        }
    }
    public static byte GetHarvestableStage(int id)
    {
        switch (id)
        {
            default: return 1;
            case CROP_CORN_ID: return Corn.HARVESTABLE_STAGE;
            case TREE_OAK_ID: return OakTree.HARVESTABLE_STAGE;
        }
    }

    virtual public void ResetToDefaults() {
		lifepower = GetCreateCost(id);
		lifepowerToGrow = 1;
		stage = 0;
		growth = 0;
	}

	override public void Prepare() {		
		PrepareStructure();
		lifepower = GetCreateCost(id);
		growth = 0;
	}

	public static void PlantUpdate() { // можно выделить в потоки
        if (existingPlantsMask != 0) {
            if ((existingPlantsMask & (1 << CROP_CORN_ID)) != 0) Corn.UpdatePlants();
            if ((existingPlantsMask & (1 << TREE_OAK_ID)) != 0) OakTree.UpdatePlants();
        }
	}

    virtual public int GetMaxLifeTransfer()
    {
        return 1;
    }
    virtual public byte GetHarvestableStage()
    {
        return 255;
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
	public virtual void SetStage( byte newStage) {
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
	public static void Load (StructureSerializer i_ss, PlantSerializer i_ps, SurfaceBlock sblock) {
        Plant p = GetNewPlant(i_ps.plant_id);
		p.LoadStructureData(i_ss,sblock);
        p.LoadPlantData(i_ps);
    }

	protected void LoadPlantData(PlantSerializer ps) {
        lifepower = ps.lifepower;
		SetStage(ps.stage);
		growth = ps.growth;
	}

	protected PlantSerializer GetPlantSerializer() {
		PlantSerializer ps = new PlantSerializer();
		ps.plant_id = plant_ID;
		ps.lifepower = lifepower;
		ps.growth = growth;
		ps.stage = stage;
		return ps;
	}
	#endregion

	virtual public void Dry() {
		Annihilate(false);
	}

	virtual public void Harvest() {
		// аннигиляция со сбором ресурсов
	}

    protected bool PreparePlantForDestruction(bool forced)
    {
        if (PrepareStructureForDestruction(forced))
        {
            if (basement.grassland != null) basement.grassland.AddLifepower((int)(lifepower * GameMaster.realMaster.lifepowerLossesPercent));
            return true;
        }
        else return false;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PreparePlantForDestruction(forced);
        basement = null;
        Destroy(gameObject);
    }
}
