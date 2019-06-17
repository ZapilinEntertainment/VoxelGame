using System.Collections.Generic;
using UnityEngine;

public sealed class SettlementStructure : Structure
{
    public Settlement.SettlementStructureType type { get; private set; }
    public byte level { get; private set; }
    public float value { get; private set; }
    private Settlement settlement;
    // byte subIndex - подвид модели

    public const byte CELLSIZE = 2;
    override protected void SetModel()
    {
        if (type == Settlement.SettlementStructureType.Empty) return;
        GameObject model;
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        switch (type)
        {
            case Settlement.SettlementStructureType.House:
                switch (level) {
                    case 8:
                    case 7:
                    case 6: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart_lvl6")); break;
                    case 5: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart_lvl5")); break;
                    case 4: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart_lvl4")); break;
                    case 3: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart_lvl3")); break;
                    case 2: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart_lvl2")); break;
                    default:  model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart_lvl1")); break;
                }
                break;
            case Settlement.SettlementStructureType.Shop:
                model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/shopPart"));
                break;
            case Settlement.SettlementStructureType.Garden:
                model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/parkPart"));
                break;
            default:
                model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(model, true);
    }
    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        basement = b;
        surfaceRect = new SurfaceRect(pos.x, pos.y, surfaceRect.size);
        float f = Random.value;
        if (f > 0.5f)
        {
            if (f > 0.75f) modelRotation = 6;
            else modelRotation = 4;
        }
        else
        {
            if (f < 0.25f) modelRotation = 2;
        }
        if (transform.childCount == 0) SetModel();
        basement.AddStructure(this);
    }
    public void SetData(Settlement.SettlementStructureType i_type, byte i_level, Settlement i_settlement)
    {
        type = i_type;
        if (i_type == Settlement.SettlementStructureType.House) level = i_level;
        else level = 1;
        if (level >= Settlement.FIRST_EXTENSION_LEVEL)
        {
            if (level < Settlement.SECOND_EXTENSION_LEVEL) surfaceRect = new SurfaceRect(0, 0, 2 * CELLSIZE);
            else surfaceRect = new SurfaceRect(0, 0, 3 * CELLSIZE);
        }
        else surfaceRect = new SurfaceRect(0, 0, CELLSIZE);
        switch (type)
        {
            case Settlement.SettlementStructureType.House:
                switch (level)
                {
                    case 8: value = 576; break;
                    case 7: value = 504; break;
                    case 6: value = 432f; break;
                    case 5: value = 192f; break;
                    case 4: value = 152f; break;
                    case 3: value = 112f; break;
                    case 2: value = 18f;break;
                    default: value = 10f; break;
                }
                break;
            case Settlement.SettlementStructureType.Garden:
            case Settlement.SettlementStructureType.Shop:
                value = 1f;
                break;
        }
        settlement = i_settlement;        // может быть null
    }
    public void AssignSettlement(Settlement s)
    {
        settlement = s;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(clearFromSurface, returnResources, leaveRuins);
        basement = null;
        if (settlement != null) settlement.needRecalculation = true;
        Destroy(gameObject);
    }

    public override List<byte> Save()
    {
        var data = base.Save();
        data.Add(level);
        data.Add((byte)type);
        return data;
    }
    public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + 2];
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        var ppos = new PixelPosByte(data[0], data[1]);        
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 12);

        SetData((Settlement.SettlementStructureType)data[17], data[16],  null);
        SetBasement(sblock, ppos);
    }
}
