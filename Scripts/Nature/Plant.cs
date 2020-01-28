using UnityEngine;
using System.Collections.Generic;

public abstract class Plant : Structure {
	public int plant_ID{get;protected set;}
	public float lifepower;
	public float lifepowerToGrow {get;protected set;}  // fixed by id		
	public float growth;
	public byte stage;	
    protected bool addedToClassList = false;

    public const int CROP_CORN_ID = 1, TREE_OAK_ID = 2, 
        TOTAL_PLANT_TYPES = 2;  // при создании нового добавить во все статические функции внизу
    public const int PLANT_SERIALIZER_LENGTH = 13;
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
        p.ID = PLANT_ID;
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
		lifepower = GetCreateCost(ID);
		lifepowerToGrow = 1;
		stage = 0;
		growth = 0;
	}

	override public void Prepare() {		
		PrepareStructure();
		lifepower = GetCreateCost(ID);
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

    override public void ApplyDamage(float d)
    {
        if (destroyed | indestructible) return;
        hp -= d;
        if (hp <= 0) if (hp > -100f) Dry(); else Annihilate(true, false, false);
    }
    virtual public void Dry() {
		Annihilate(true, false, false);
	}

	virtual public void Harvest(bool replenish) {
		// сбор ресурсов и перепосадка
	}

    protected void PreparePlantForDestruction(bool clearFromSurface, bool returnResources)
    {
        PrepareStructureForDestruction(clearFromSurface, returnResources, false);
        if (clearFromSurface)
        {
            if (basement != null && basement.grassland != null) basement.grassland.AddLifepower((int)(lifepower * GameMaster.realMaster.lifepowerLossesPercent));
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { UnsetBasement(); }
        PreparePlantForDestruction(clearFromSurface, returnResources);
        basement = null;
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        List<byte> data = SaveStructureData();
        data.AddRange(SerializePlant());
        return data;
    }

    public static void LoadPlant(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + PLANT_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        int plantSerializerIndex = STRUCTURE_SERIALIZER_LENGTH;
        int plantId = System.BitConverter.ToInt32(data, plantSerializerIndex);
        Plant p = GetNewPlant(plantId);
        p.LoadStructureData(data, sblock);
        p.lifepower = System.BitConverter.ToSingle(data, plantSerializerIndex + 4);
        p.SetStage(data[plantSerializerIndex + 12]);
        p.growth = System.BitConverter.ToSingle(data, plantSerializerIndex + 8);
    }

    protected List<byte> SerializePlant()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(plant_ID)); 
        data.AddRange(System.BitConverter.GetBytes(lifepower)); 
        data.AddRange(System.BitConverter.GetBytes(growth)); 
        data.Add(stage); 
        //SERIALIZER_LENGTH = 13
        return data;
    }
    #endregion
}
