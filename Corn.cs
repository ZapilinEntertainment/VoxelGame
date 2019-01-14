using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed public class Corn : Plant {
    private Transform model;

	static Sprite[] stageSprites;
    static bool spritePackLoaded = false;
    private static List<Corn> corns;

    private static float growSpeed, decaySpeed; // in tick
    public static int maxLifeTransfer { get; private set; }  // fixed by class
    public const byte HARVESTABLE_STAGE = 3;

    public const byte MAX_STAGE = 3;
    public const int CREATE_COST = 1, GATHER = 2;

    static Corn()
    {
        corns = new List<Corn>();
        maxLifeTransfer = 1;
        growSpeed = 0.01f;
        decaySpeed = growSpeed * 0.01f;        
    }

    public static void ResetToDefaults_Static_Corn()
    {
        corns = new List<Corn>();
        FollowingCamera.main.cameraChangedEvent += CameraUpdate;
    }    

    override public void Prepare()
    {
        PrepareStructure();
        plant_ID = CROP_CORN_ID;
        lifepower = CREATE_COST;
        lifepowerToGrow = GetLifepowerLevelForStage(stage);
        growth = 0;
    }

    override protected void SetModel()
    {
        if (!spritePackLoaded)
        {
            stageSprites = Resources.LoadAll<Sprite>("Textures/Plants/corn"); // 4 images
            spritePackLoaded = true;
        }
        model = new GameObject("sprite").transform;
        model.parent = transform;
        model.transform.localPosition = Vector3.zero;
        model.gameObject.AddComponent<SpriteRenderer>().sprite = stageSprites[0];
    }

    public static void UpdatePlants()
    {
        if (corns.Count > 0)
        {
            int i = 0;
            while (i < corns.Count)
            {
                Corn c = corns[i];
                if (c == null)
                {
                    corns.RemoveAt(i);
                    continue;
                }
                else
                {
                    float theoreticalGrowth = c.lifepower / c.lifepowerToGrow;
                    if (c.growth < theoreticalGrowth)
                    {
                        c.growth = Mathf.MoveTowards(c.growth, theoreticalGrowth, growSpeed);                       
                    }
                    else
                    {
                        c.growth = Mathf.MoveTowards(c.growth, theoreticalGrowth, decaySpeed);
                        if (c.lifepower <= 0)
                        {
                            c.Dry();
                        }
                    }
                    if (c.growth >= 1 & c.stage < MAX_STAGE)
                    {
                        c.SetStage((byte)(c.stage + 1));
                    }
                    i++;
                }
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
    public static void CameraUpdate()
    {
        Vector3 camPos = FollowingCamera.camPos;
        foreach (Corn c in corns)
        {
            if (c != null)
            {
                Vector3 cpos = Vector3.ProjectOnPlane(camPos - c.transform.position, c.transform.up);
                c.transform.forward = cpos.normalized;
            }
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        //#setStructureData
        basement = b;
        innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.size);
        if (model == null) SetModel();
        b.AddStructure(this);
            // isbasement check deleted
        //---
        if (!addedToClassList)
        {
            corns.Add(this);
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
		if (model != null) model.GetComponent<SpriteRenderer>().sprite = stageSprites[0];
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
        if (model != null) model.GetComponent<SpriteRenderer>().sprite = stageSprites[stage];
		lifepowerToGrow  = GetLifepowerLevelForStage(stage);
		growth = lifepower / lifepowerToGrow;
	}
	public static float GetLifepowerLevelForStage(byte st) {
		return (st + 1);
	}
	#endregion

	override public void Harvest() {
		GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.Food, GATHER);
		ResetToDefaults();
	}

	override public void Dry() {
		Structure s = GetStructureByID(DRYED_PLANT_ID);
        s.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
        StructureTimer st =  s.gameObject.AddComponent<StructureTimer>();
        st.timer = 5;
	}

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PreparePlantForDestruction(forced);
        if (addedToClassList)
        {
            int index = corns.IndexOf(this);
            if (index > 0) corns[index] = null;
            addedToClassList = false;
        }
        Destroy(gameObject);
    }
}
