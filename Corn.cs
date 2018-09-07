using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed public class Corn : Plant {
	static Sprite[] stageSprites;
    static bool spritePackLoaded = false;
    private static Corn[] corns;
    private static List<Corn> addToCornsList;
    private static List<int> removeFromCornsList;

    private static float growSpeed, decaySpeed; // fixed by class
    public static int maxLifeTransfer { get; private set; }  // fixed by class
    public const byte HARVESTABLE_STAGE = 3;

    public const byte MAX_STAGE = 3;
    public const int CREATE_COST = 1, GATHER = 2;

    static Corn()
    {
        corns = new Corn[0];
        addToCornsList = new List<Corn>();
        removeFromCornsList = new List<int>();
        maxLifeTransfer = 1;
        growSpeed = 0.03f;
        decaySpeed = growSpeed;
    }

    public static void UpdatePlants()
    {
        float t = GameMaster.LIFEPOWER_TICK;
        int removeCount = removeFromCornsList.Count, addCount = addToCornsList.Count;
        if (removeCount > 0 | addCount > 0)
        {
            if (removeCount > 0)
            {
                foreach (int index in removeFromCornsList) corns[index] = null;
                removeFromCornsList.Clear();
            }
            Corn[] newCornsArray = new Corn[corns.Length - removeCount + addCount];
            if (newCornsArray.Length != 0)
            {
                int i = 0;
                if (addCount > 0)
                {
                    for (; i < addCount; i++) newCornsArray[i] = addToCornsList[i];
                    addToCornsList.Clear();
                }
                if (corns.Length > 0)
                {
                    for (int j = 0; j < corns.Length; j++)
                    {
                        if (corns[j] != null) newCornsArray[j + i] = corns[j];
                    }
                }
            }
            corns = newCornsArray;
        }
        if (corns.Length > 0)
        {
            foreach (Corn c in corns)
            {
                float theoreticalGrowth = c.lifepower / c.lifepowerToGrow;
                if (c.growth < theoreticalGrowth)
                {
                    c.growth = Mathf.MoveTowards(c.growth, theoreticalGrowth, growSpeed * t);
                }
                else
                {
                    c.lifepower -= decaySpeed * t;
                    if (c.lifepower == 0) c.Dry();
                }
                if (c.growth >= 1 & c.stage < MAX_STAGE) c.SetStage((byte)(c.stage + 1));
            }
        }
        else
        {
            if (((existingPlantsMask >> CROP_CORN_ID) & 1) != 0)
            {
                int val = 1;
                val = val << CROP_CORN_ID;
                existingPlantsMask -= val;
            }
        }
    }

    override protected void SetModel()
    {
        if (!spritePackLoaded)
        {
            stageSprites = Resources.LoadAll<Sprite>("Textures/Plants/corn"); // 4 images
            spritePackLoaded = true;
        }
        model = new GameObject("Plant - Corn");
        GameObject spriteSatellite = new GameObject("sprite");
        spriteSatellite.transform.parent = model.transform;
        GameMaster.realMaster.mastSpritesList.Add(spriteSatellite);
        spriteSatellite.AddComponent<SpriteRenderer>().sprite = stageSprites[0];
    }

	override public void Prepare() {
        PrepareStructure();        
        plant_ID = CROP_CORN_ID;
		lifepower = CREATE_COST;
		lifepowerToGrow = GetLifepowerLevelForStage(stage);		
		growth = 0;
	}

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetStructureData(b, pos);
        //if (isBasement) basement.myChunk.chunkUpdateSubscribers_structures.Add(this);
        if (!addedToClassList)
        {
            addToCornsList.Add(this);
            addedToClassList = true;
            if ( ((existingPlantsMask >> CROP_CORN_ID) & 1)  != 1 )
            {
                int val = 1;
                val = val << CROP_CORN_ID;
                existingPlantsMask += val;
            }
        }
    }

    override public void ResetToDefaults() {
		lifepower = CREATE_COST;
        stage = 0;
        lifepowerToGrow = GetLifepowerLevelForStage(stage);		
		growth = 0;
		hp = maxHp;
		if (model != null) model.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = stageSprites[0];
	}

    override public int GetMaxLifeTransfer()
    {
        return maxLifeTransfer;
    }
    override public byte GetHarvestableStage()
    {
        return HARVESTABLE_STAGE;
    }

    #region lifepower operations
    public override void AddLifepowerAndCalculate(int life) {
		lifepower += life;
		byte nstage = 0;
		float lpg = GetLifepowerLevelForStage(nstage);
		while ( lifepower > lifepowerToGrow & nstage < MAX_STAGE) {
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
		if (model != null) model.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = stageSprites[stage];
		lifepowerToGrow  = GetLifepowerLevelForStage(stage);
		growth = lifepower / lifepowerToGrow;
	}
	public static float GetLifepowerLevelForStage(byte st) {
		return (st+1);
	}
	#endregion

	override public void Harvest() {
		GameMaster.colonyController.storage.AddResource(ResourceType.Food, GATHER);
		ResetToDefaults();
	}

	override public void Dry() {
		Structure s = GetStructureByID(DRYED_PLANT_ID);
		basement.AddCellStructure(s, new PixelPosByte(innerPosition.x, innerPosition.z));
	}

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PreparePlantForDestruction(forced);
        if (addedToClassList)
        {
            if (corns.Length > 0) { 
                int i = -1;
                foreach (Corn c in corns)
                {
                    if (c.personalNumber == personalNumber)
                    {
                        removeFromCornsList.Add(i);
                    }
                }
            }
            addedToClassList = false;
        }
    }
}
