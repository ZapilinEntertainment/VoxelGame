using UnityEngine;
using System.Collections.Generic;

public sealed class Grassland : MyObject
{    
    public Plane plane { get; private set; }
    private Nature nature;
    private PlantCategory[] categoriesCatalog;
    private Plant[] plants;
    public bool canBeBoosted { get; private set; }
    public bool deleted { get; private set; }
    public bool needRecalculation = false;
    public readonly int ID;
    private bool ignoreRecalculationsRequest = false;
    public byte level { get; private set; }
    public bool isCultivating
    {
        get
        {
            return (plane.mainStructure != null && plane.mainStructure is Farm);
        }
    }
    private float lifepowerSurplus;
    private float lifepower;
    private const int MAX_CATEGORIES_COUNT = 3;
    private const byte MAX_LEVEL = 8;
    public const float BOOST_VALUE = 5f, CREATE_COST_VAL = 10f;
    public byte faceIndex { get { if (plane != null) return plane.faceIndex; else return Block.UP_FACE_INDEX; } }
    public ChunkPos pos { get { if (plane != null) return plane.pos; else return ChunkPos.zer0; } }
    public PlanePos planePos { get { return new PlanePos(pos, faceIndex); } }
    private static int nextID = 1;

    protected override bool IsEqualNoCheck(object obj)
    {
        var g = obj as Grassland;
        return (ID == g.ID) && (planePos == g.planePos);
    }
    public override int GetHashCode()
    {
        return level + faceIndex + ID;
    }

    #region save-load
    public void Save(System.IO.FileStream fs)
    {        
        var cpos = plane.pos;
        fs.WriteByte(cpos.x); // 0
        fs.WriteByte(cpos.y); // 1
        fs.WriteByte(cpos.z); //2
        fs.WriteByte(plane.faceIndex); //3 
        fs.Write(System.BitConverter.GetBytes(ID), 0, 4); // 4 - 7
        //
        fs.WriteByte((byte)categoriesCatalog[0]); // 8
        fs.WriteByte((byte)categoriesCatalog[1]); //9
        fs.WriteByte((byte)categoriesCatalog[2]); // 10
        fs.WriteByte(level); //11
        fs.Write(System.BitConverter.GetBytes(lifepower),0,4); // 12 - 15
    }
    public static Grassland Load(System.IO.FileStream fs, Chunk c)
    {
        var data = new byte[16];
        fs.Read(data, 0, data.Length);

        var chunkPos = new ChunkPos(data[0], data[1], data[2]);

        var b = c.GetBlock(chunkPos);
        if (b != null)
        {
            Plane p;
            if (b.TryGetPlane(data[3], out p))
            {                
                var g = new Grassland(p, System.BitConverter.ToInt32(data,4));
                g.categoriesCatalog[0] = (PlantCategory)data[8];
                g.categoriesCatalog[1] = (PlantCategory)data[9];
                g.categoriesCatalog[2] = (PlantCategory)data[10];
                g.lifepower = System.BitConverter.ToSingle(data, 12);
                g.SetLevel(data[11]);               
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
            Debug.Log("grassland load error: no block " + data[0].ToString() + ':' + data[1].ToString() + ':' + data[2].ToString());
            return null;
        }
    }
    #endregion 

    public static Grassland CreateAt(Plane p, bool checks)
    {
        if (p == null || p.destroyed || (checks && !p.IsSuitableForGrassland())) return null;
        else
        {
            return new Grassland(p);            
        }
    }
    private Grassland(Plane p) 
    {
        ID = nextID++;
        INLINE_Constructor(p);
    }
    private Grassland(Plane p, int i_ID)
    {
        ID = i_ID;
        INLINE_Constructor(p);
    }
    private void INLINE_Constructor(Plane p)
    {
        deleted = false;
        plane = p; plane.AssignGrassland(this);
        nature = p.myChunk.GetNature();
        categoriesCatalog = new PlantCategory[MAX_CATEGORIES_COUNT];
        categoriesCatalog[0] = (PlantCategory)Random.Range(0, 3);
        categoriesCatalog[1] = (PlantCategory)Random.Range(0, 3);
        categoriesCatalog[2] = (PlantCategory)Random.Range(0, 3);
        plane.SetMeshRotation((byte)Random.Range(0, 4), false);       
        Recalculation();
        nature.AddGrassland(this);
    }

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
        if (lifepower <= BOOST_VALUE ) return;
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
        lifepower += f;
        if (f < 10f) return;
        else SYSTEM_UseLifepower();
    }
    public void SYSTEM_UseLifepower()
    {
        ignoreRecalculationsRequest = true;
        //#update inners ~
        var luv = GetLevelUpValue();
        plants = plane.GetPlants();
        int actionsCountNeeded = 0;
        if (plants != null) actionsCountNeeded = GetMaxPlantsCount() - plants.Length;
        while (lifepower > 0f)
        {
            if (actionsCountNeeded <= 0 && level < MAX_LEVEL && lifepower > luv)
            {
                SetLevel((byte)(level + 1));
                lifepower -= luv;
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
                        if (Random.value > 0.15f) creating = false;
                    }
                }
                //
                Plant p;
                if (creating)
                {
                    var pcat = categoriesCatalog[Random.Range(0, MAX_CATEGORIES_COUNT)];
                    p = Plant.GetNewPlant(nature.GetPlantType(pcat));
                    lifepower -= CREATE_COST_VAL * nature.lifepowerSupport;
                    p?.SetBasement(plane);
                    if (plants != null)
                    {
                        var pl2 = new Plant[plants.Length + 1];
                        for (int a = 0; a < plants.Length; a++)
                        {
                            pl2[a] = plants[a];
                        }
                        pl2[pl2.Length - 1] = p;
                        plants = pl2;
                    }
                    else
                    {
                        plants = new Plant[] { p };
                    }
                }
                else
                {
                    if (plants != null)
                    {
                        p = plants[Random.Range(0, plants.Length)];
                        if (!p.IsFullGrown())
                        {
                            p.UpdatePlant();
                            lifepower -= BOOST_VALUE * p.stage;
                        }
                    }
                }
                actionsCountNeeded--;
            }
        }
        if (lifepower < 0f) lifepower = 0f;        
        ignoreRecalculationsRequest = false;
        Recalculation();
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
            Annihilate(GrasslandAnnihilationOrder.SelfDestruction);
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

    public float DEBUG_GetLifepower() { return lifepower; }
    public static int SYSTEM_GetNextID() { return nextID; }
    public static void SYSTEM_SetNextID(int sid) { nextID = sid; }

    private void SetLevel(byte l)
    {
        if (level == l) return;
        else
        {
            if (level > MAX_LEVEL) level = MAX_LEVEL;
            level = l;
            if (!isCultivating && plane.materialID != PoolMaster.MATERIAL_ADVANCED_COVERING_ID)
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
    public void Annihilate(GrasslandAnnihilationOrder order)
    {
        if (deleted) return;
        else deleted = true;
        if (order.destroyPlants)
        {
            var plist = plane.GetPlants();
            if (plist != null)
            {
                var so = order.GetStructureOrder();
                for (int i = 0; i < plist.Length; i++)
                {
                    plist[i].Annihilate(so);
                }
            }
        }
        if (order.doSpecialChecks)
        {
            nature.RemoveGrassland(this);
            if (order.sendMessageToPlane) plane?.RemoveGrassland(this, false);
        }
    }    
}
