﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed public class Corn : Plant {
    private Transform model;

	private static Sprite[] stageSprites;
    private static bool spritePackLoaded = false, subscribedToCameraUpdate = false;
    public const byte HARVESTABLE_STAGE = 3;

    private const byte MAX_STAGE = 3;
    private const int GATHER = 2;
    private const float LIFEPOWER_SURPLUS = 0.1f;

    override public void Prepare()
    {
        PrepareStructure();
        stage = 0;
        type = GetPlantType();
    }
    public static new PlantType GetPlantType()
    {
        return PlantType.Corn;
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
        var sr = model.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = stageSprites[0];
        sr.sharedMaterial = PoolMaster.verticalWavingBillboardMaterial;
    }
	override protected void SetStage( byte newStage) {
		if (newStage == stage) return;
		stage = newStage;
        if (model != null) model.GetComponent<SpriteRenderer>().sprite = stageSprites[stage];
		
	}

	override public void Harvest(bool replenish) {
		GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.Food, GATHER);
        if (replenish) SetStage(0);
        else Annihilate(true, false, false);
	}
    override public bool IsFullGrown()
    {
        return stage == MAX_STAGE;
    }
    override public float GetLifepowerSurplus()
    {
        return LIFEPOWER_SURPLUS;
    }
    override public void Dry(bool sendMessageToGrassland) {
        if (!sendMessageToGrassland) basement?.RemoveStructure(this);
		Structure s = GetStructureByID(DRYED_PLANT_ID);
        s.SetBasement(basement, new PixelPosByte(surfaceRect.x, surfaceRect.z));
        StructureTimer st =  s.gameObject.AddComponent<StructureTimer>();
        st.timer = 5;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) basement = null;
        PreparePlantForDestruction(clearFromSurface, returnResources);
        Destroy(gameObject);
    }
}