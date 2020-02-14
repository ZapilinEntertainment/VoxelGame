using UnityEngine;
using System.Collections.Generic;

public sealed class Grassland 
{    
    public Plane plane { get; private set; }
    private Nature nature;
    private PlantCategory[] categoriesCatalog;
    private List<Plant> plantsList;
    public bool canBeBoosted { get; private set; }
    public bool needRecalculation = false;
    public byte level { get; private set; }
    private bool cultivating = false;
    private float lifepowerSurplus;
    private float lifepower;
    private const int MAX_CATEGORIES_COUNT = 3;
    private const byte MAX_LEVEL = 8;
    public const float BOOST_VALUE = 5f, CREATE_COST_VAL = 10f;
    public byte faceIndex { get { if (plane != null) return plane.faceIndex; else return Block.UP_FACE_INDEX; } }
    public ChunkPos pos { get { if (plane != null) return plane.pos; else return ChunkPos.zer0; } }

    public Grassland(Plane p, Nature n)
    {
        plane = p;
        nature = n;
        categoriesCatalog = new PlantCategory[MAX_CATEGORIES_COUNT];
        categoriesCatalog[0] = (PlantCategory)Random.Range(0, 2);
        categoriesCatalog[1] = (PlantCategory)Random.Range(0, 2);
        categoriesCatalog[2] = (PlantCategory)Random.Range(0, 2);
        if (plane.GetExtension().SetGrassland(this))
        {
            nature.AddGrassland(this);
            Recalculation();
        }
    }
    private Grassland() { }

    private byte GetMaxPlantsCount()
    {
        switch (level)
        {
            case 8: return 16;
            case 7: return 12;
            case 6: return 10;
            case 5: return 8;
            case 4: return 5;
            case 3: return 4;
            case 2: return 3;
            case 1: return 2;
            default: return 1;
        }
    }
    private float GetLevelUpValue()
    {
        switch (level)
        {
            case 8: return 12f * BOOST_VALUE;
            case 7: return 8f * BOOST_VALUE;
            case 6: return 4f * BOOST_VALUE;
            case 5:
            case 4:
            case 3:
            case 2: 
            case 1: return 2f * BOOST_VALUE;
            default: return BOOST_VALUE;
        }
    }
    public float GetLifepowerSurplus()
    {
        if (lifepower < 0f) return -1f;
        else
        {
            if (needRecalculation) Recalculation();
            return lifepowerSurplus;
        }
    }

    public void Update()
    {
        if (!canBeBoosted) return;
        if (needRecalculation) Recalculation();
        lifepower += lifepowerSurplus;
        if (lifepower <= BOOST_VALUE | cultivating) return;
        else
        {
            var luv = GetLevelUpValue();
            if (level < MAX_LEVEL && lifepower > luv)
            {
                SetLevel((byte)(level + 1));
                lifepower -= luv;
                if (lifepower <= BOOST_VALUE) return;
            }
            bool creating = true;
            if (plantsList != null)
            {
                if (plantsList.Count >= GetMaxPlantsCount()) creating = false;
                else
                {
                    if (Random.value > 0.5f) creating = false;
                }
            }
            //
            if (creating)
            {
                var pcat = categoriesCatalog[Random.Range(0, MAX_CATEGORIES_COUNT - 1)];
                var p = Plant.GetNewPlant(nature.GetPlantType(pcat));                
                lifepower -= CREATE_COST_VAL * nature.environmentalConditions;
                p?.SetBasement(plane);  //перерасчет вызовет сама plane             
            }
            else
            {
                var p = plantsList[Random.Range(0, plantsList.Count)];
                if (!p.IsFullGrown())
                {
                    p.UpdatePlant();
                    lifepower -= BOOST_VALUE;
                }
            }
        }
    }
    private void Recalculation()
    {
        var prevlps = lifepowerSurplus;
        lifepowerSurplus = 0f;
        canBeBoosted = true;
        plantsList = plane.GetPlantsList();
        if (plantsList != null)
        {
            if (level == MAX_LEVEL) canBeBoosted = false;
            foreach (Plant p in plantsList)
            {
                lifepowerSurplus += p.GetLifepowerSurplus();
                if (!p.IsFullGrown()) canBeBoosted = true;
            }
        }
        if (lifepowerSurplus != prevlps) nature.needRecalculation = true;
        needRecalculation = false;
    }
    public void Extinct()
    {
        if (plantsList != null)
        {
            var p = plantsList[Random.Range(0, plantsList.Count)];
            lifepowerSurplus -= p.GetLifepowerSurplus();
            p.Dry(false);
            plantsList.Remove(p);
            if (plantsList.Count == 0) plantsList = null;
        }
        else
        {
            if (level > 1) SetLevel((byte)(level - 1));
            else Annihilate(false, true);
        }
        canBeBoosted = true;
    }
    public void AddLifepower(float f)
    {
        lifepower += f;
        Update();
    }
    public void TakeLifepower(float f)
    {
        lifepower -= f;
        needRecalculation = true;
    }

    private void SetLevel(byte l)
    {
        if (level == l) return;
        else
        {
            if (level > MAX_LEVEL) level = MAX_LEVEL;
            level = l;
            if (!cultivating)
            {
                var fi = plane.faceIndex;
                if (fi == Block.SURFACE_FACE_INDEX | fi == Block.UP_FACE_INDEX)
                {
                    switch (level)
                    {
                        case 1: plane.ChangeMaterial(PoolMaster.MATERIAL_GRASS_20_ID, true); break;
                        case 2: plane.ChangeMaterial(PoolMaster.MATERIAL_GRASS_40_ID, true); break;
                        case 3: plane.ChangeMaterial(PoolMaster.MATERIAL_GRASS_60_ID, true); break;
                        case 4: plane.ChangeMaterial(PoolMaster.MATERIAL_GRASS_80_ID, true); break;
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                            plane.ChangeMaterial(PoolMaster.MATERIAL_GRASS_100_ID, true); break;
                    }
                }
            }
        }
    }
    public void SetCultivatingStatus(bool x)
    {
        cultivating = x;
    }

    public void Annihilate(bool plantsDestruction, bool sendMessageToPlane)
    {
        if (plantsDestruction)
        {
            var plist = plane.GetPlantsList().ToArray();
            for (int i = 0; i < plist.Length; i++)
            {
                plist[i].Dry(false);
            }
        }
        nature.RemoveGrassland(this);
        if (sendMessageToPlane) plane?.RemoveGrassland(this, false);
    }    
}
