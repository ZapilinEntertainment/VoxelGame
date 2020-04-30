using UnityEngine;
using System.Collections.Generic;

public sealed class Grassland 
{    
    public Plane plane { get; private set; }
    private Nature nature;
    private PlantCategory[] categoriesCatalog;
    private Plant[] plants;
    public bool canBeBoosted { get; private set; }
    public bool needRecalculation = false;
    private bool ignoreRecalculationsRequest = false;
    public byte level { get; private set; }
    private bool cultivating = false;
    private float lifepowerSurplus;
    private float lifepower;
    private const int MAX_CATEGORIES_COUNT = 3;
    private const byte MAX_LEVEL = 8;
    public const float BOOST_VALUE = 5f, CREATE_COST_VAL = 10f;
    public byte faceIndex { get { if (plane != null) return plane.faceIndex; else return Block.UP_FACE_INDEX; } }
    public ChunkPos pos { get { if (plane != null) return plane.pos; else return ChunkPos.zer0; } }

    #region save-load
    public void Save(System.IO.FileStream fs)
    {
        var ppos = plane.pos;
        fs.WriteByte(ppos.x); // 0
        fs.WriteByte(ppos.y); // 1
        fs.WriteByte(ppos.z); //2
        fs.WriteByte(plane.faceIndex); //3 
        //
        fs.WriteByte((byte)categoriesCatalog[0]); // 4
        fs.WriteByte((byte)categoriesCatalog[1]); //5
        fs.WriteByte((byte)categoriesCatalog[2]); // 6
        fs.WriteByte(level); //7
        fs.WriteByte(cultivating ? (byte)1 : (byte)0); //8
        fs.Write(System.BitConverter.GetBytes(lifepower),0,4); // 9-12
    }
    public static Grassland Load(System.IO.FileStream fs, Nature n, Chunk c)
    {
        var data = new byte[13];
        fs.Read(data, 0, data.Length);
        var b = c.GetBlock(data[0], data[1], data[2]);
        if (b != null)
        {
            Plane p;
            if (b.TryGetPlane(data[3], out p))
            {
                var g = new Grassland(p, n);
                g.categoriesCatalog[0] = (PlantCategory)data[4];
                g.categoriesCatalog[1] = (PlantCategory)data[5];
                g.categoriesCatalog[2] = (PlantCategory)data[6];
                g.cultivating = data[8] == 1;
                g.lifepower = System.BitConverter.ToSingle(data, 9);
                g.SetLevel(data[7]);               
                return g;
            }
            else
            {
                Debug.Log("grassland load error: no plane found");
                return null;
            }
        }
        else
        {
            Debug.Log("grassland load error: no block");
            return null;
        }
    }
    #endregion 

    public Grassland(Plane p, Nature n)
    {
        plane = p;
        nature = n;
        categoriesCatalog = new PlantCategory[MAX_CATEGORIES_COUNT];
        categoriesCatalog[0] = (PlantCategory)Random.Range(0, 3);
        categoriesCatalog[1] = (PlantCategory)Random.Range(0, 3);
        categoriesCatalog[2] = (PlantCategory)Random.Range(0, 3);
        if (plane.FORCED_GetExtension().SetGrassland(this))
        {
            plane.SetMeshRotation((byte)Random.Range(0, 4),false);            
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
            //#update inners
            var luv = GetLevelUpValue();
            if (level < MAX_LEVEL && lifepower > luv)
            {
                SetLevel((byte)(level + 1));
                lifepower -= luv;
                if (lifepower <= BOOST_VALUE) return;
            }
            bool creating = plane.fulfillStatus != FullfillStatus.Full;
            if (plants != null)
            {
                if (plants.Length >= GetMaxPlantsCount()) creating = false;
                else
                {
                    if (Random.value > 0.5f) creating = false;
                }
            }
            else
            {
                if (!creating) return;
            }
            //
            if (creating)
            {
                var pcat = categoriesCatalog[Random.Range(0, MAX_CATEGORIES_COUNT)];
                var p = Plant.GetNewPlant(nature.GetPlantType(pcat));                
                lifepower -= CREATE_COST_VAL * nature.lifepowerSupport;
                p?.SetBasement(plane);  //перерасчет вызовет сама plane             
            }
            else
            {
                var p = plants[Random.Range(0, plants.Length)];
                if (!p.IsFullGrown())
                {
                    p.UpdatePlant();
                    lifepower -= BOOST_VALUE;
                }
            }
            //
        }
    }
    public void FORCED_AddLifepower(float f)
    {
        if (f < 10f)
        {
            lifepower += f;
            return;
        }
        else
        {
            ignoreRecalculationsRequest = true;
            //#update inners ~
            var luv = GetLevelUpValue();
            plants = plane.GetPlants();
            int actionsCountNeeded = 0;
            if (plants!= null) actionsCountNeeded = GetMaxPlantsCount() - plants.Length;
            while (f > 0f)
            {
                if (actionsCountNeeded <= 0 && level < MAX_LEVEL && f > luv)
                {
                    SetLevel((byte)(level + 1));
                    f -= luv;
                    luv = GetLevelUpValue();
                    actionsCountNeeded += 2 * level;
                    continue;
                }
                else
                {
                    bool creating = true;
                    if (plants != null)
                    {
                        if (plants.Length >= GetMaxPlantsCount()) creating = false;
                        else
                        {
                            if (Random.value > 0.5f) creating = false;
                        }
                    }
                    //
                    if (creating)
                    {
                        var pcat = categoriesCatalog[Random.Range(0, MAX_CATEGORIES_COUNT)];
                        var p = Plant.GetNewPlant(nature.GetPlantType(pcat));
                        f -= CREATE_COST_VAL * nature.lifepowerSupport;
                        p?.SetBasement(plane);
                    }
                    else
                    {
                        if (plants != null)
                        {
                            foreach (var p in plants)
                            {
                                if (Random.value > 0.33f)
                                {
                                    p.UpdatePlant();
                                    f -= BOOST_VALUE;
                                }
                            }
                        }
                    }
                    actionsCountNeeded--;
                }
            }
            if (f > 0f) lifepower += f;
            ignoreRecalculationsRequest = false;
            Recalculation();
        }
    }
    private void Recalculation()
    {
        if (ignoreRecalculationsRequest) return;
        var prevlps = lifepowerSurplus;
        lifepowerSurplus = 0f;
        canBeBoosted = true;
        plants = plane.GetPlants();
        if (plants != null)
        {
            if (level == MAX_LEVEL) canBeBoosted = false;
            foreach (Plant p in plants)
            {
                lifepowerSurplus += p.GetLifepowerSurplus();
                if (!p.IsFullGrown()) canBeBoosted = true;
            }
        }
        if (lifepowerSurplus != prevlps) nature.needRecalculation = true;
        needRecalculation = false;
    }
    public void Dry()
    {
        if (level > 1) SetLevel((byte)(level - 1));
        else
        {
            if (plants != null)
            {
                foreach (var p in plants)
                {
                    p.Dry(false);
                }
            }
            Annihilate(false, true);
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
                //var fi = plane.faceIndex;
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
            var plist = plane.GetPlants();
            for (int i = 0; i < plist.Length; i++)
            {
                plist[i].Annihilate(true, false, false);
            }
        }
        nature.RemoveGrassland(this);
        if (sendMessageToPlane) plane?.RemoveGrassland(this, false);
    }    
}
