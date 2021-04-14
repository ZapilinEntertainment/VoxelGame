using UnityEngine;
using System.Collections.Generic;

public enum PlantCategory : byte { Flower, Bush, Tree} // dependency : grassland, nature
public enum PlantType : byte { Abstract,OakTree, Corn, CrystalPine, PollenFlower}
public abstract class Plant : Structure {
    public byte stage { get; protected set; }
    public PlantType type { get; protected set; }

    #region save-load system
    override public List<byte> Save()
    {
        List<byte> data = SaveStructureData();
        data.AddRange(SerializePlant());
        return data;        
    }


    public static Plant LoadPlant(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + 2];
        fs.Read(data, 0, data.Length);
        int plantSerializerIndex = STRUCTURE_SERIALIZER_LENGTH;
        Plant p = GetNewPlant((PlantType)data[plantSerializerIndex]);
        p.LoadStructureData(data, sblock);
        p.SetStage(data[plantSerializerIndex + 1]);
        return p;
    }

    protected List<byte> SerializePlant()
    {
        return new List<byte>() { (byte)type, stage };
    }
    #endregion

    public static Plant GetNewPlant(PlantType ptype)
    {
        Plant p;
        switch (ptype)
        {
            default: return null;
            case PlantType.Corn: p = new GameObject("Corn").AddComponent<Corn>(); break;
            case PlantType.OakTree:
                p = new GameObject("Oak Tree").AddComponent<OakTree>();
                break;
        }
        p.ID = PLANT_ID;
        p.Prepare();
        return p;
    }

    override public void Prepare()
    {
        PrepareStructure();
        stage = 0;
        type = GetPlantType();
    }
    public override bool CanBeRotated()
    {
        return false;
    }
    /// <summary>
    /// sets to random position
    /// </summary>
    override public void SetBasement(Plane p)
    {
        if (surfaceRect.size == 1)  SetBasement(p, p.FORCED_GetExtension().GetRandomCell());
        else
        {
            if (surfaceRect.size == PlaneExtension.INNER_RESOLUTION) SetBasement(p, PixelPosByte.zero);
            else SetBasement(p, p.FORCED_GetExtension().GetRandomPosition(surfaceRect.size));
        }
    }

    virtual public void UpdatePlant()
    {
        if (!IsFullGrown())
        {
            byte x = stage;
            if (x < 255) x++;
            SetStage(x);
        }
    }
    virtual protected void SetStage(byte newStage)
    {
        stage = newStage;
    }
    public void FORCED_SetStage(byte newStage) { SetStage(newStage); }
    virtual public bool IsFullGrown()
    {
        return true;
    }
    virtual public float GetLifepowerSurplus()
    {
        return 0f;
    }
    public static PlantType GetPlantType()
    {
        return PlantType.Abstract;
    }
    virtual public float GetPlantComplexity()
    {
        return 1f;
    }
    //

    override public void ApplyDamage(float d)
    {
        if (destroyed | indestructible) return;
        hp -= d;
        if (hp <= 0) if (hp > -100f) Dry(true); else Annihilate(true, false, false);
    }    

	virtual public void Harvest(bool replenish) {
        // сбор ресурсов и перепосадка
        Annihilate(true, false, false);
	}
    virtual public void Dry(bool sendMessageToGrassland)
    {
        Annihilate(true, sendMessageToGrassland, false);
    }
    protected void PreparePlantForDestruction(bool clearFromSurface, bool sendMessageToGrassland)
    {
        PrepareStructureForDestruction(clearFromSurface, false, false);
        if (sendMessageToGrassland)
        {
            if (basement != null && basement.haveGrassland) basement.GetGrassland().needRecalculation = true;
        }
    }
    override public void Annihilate(bool clearFromSurface, bool sendMessageToGrassland, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) {
            basement = null;
        }
        PreparePlantForDestruction(clearFromSurface, sendMessageToGrassland);
        basement = null;
        Destroy(gameObject);
    }   
}
