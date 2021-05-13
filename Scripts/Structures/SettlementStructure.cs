using System.Collections.Generic;
using UnityEngine;

public sealed class SettlementStructure : Structure
{
    public Settlement.SettlementStructureType type { get; private set; }
    public bool isActive { get; private set; }
    public byte level { get; private set; }
    public float value { get; private set; }
    private Settlement settlement;
    private static GameObject[] housesPrefs = new GameObject[6];
    // byte subIndex - подвид модели

    public const byte CELLSIZE = 2;
    override protected void SetModel()
    {
        if (type == Settlement.SettlementStructureType.Empty) return;
        GameObject model;
        Quaternion prevRot = Quaternion.identity;
        if (transform.childCount != 0)
            {
                var p = transform.GetChild(0);
                prevRot = p.localRotation;
                Destroy(p.gameObject);
            }
        bool replacementCheck = true;
        switch (type)
        {
            case Settlement.SettlementStructureType.House:
                model = GetHouseModel(level);
                replacementCheck = false;
                break;
            case Settlement.SettlementStructureType.Shop:
                model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/shopPart"));
                break;
            case Settlement.SettlementStructureType.Garden:
                model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/parkPart"));
                // also using by Gardens.cs
                break;
            default:
                model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }
        model.transform.parent = transform;
        model.transform.localRotation = prevRot;
        model.transform.localPosition = Vector3.zero;
        if (!PoolMaster.useDefaultMaterials & replacementCheck) PoolMaster.ReplaceMaterials(model);
    }
    public static GameObject GetHouseModel(byte lvl)
    {
        GameObject m;
        switch (lvl)
        {
            case 8:
            case 7:
            case 6:
                if (housesPrefs[5] == null) housesPrefs[5] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl6");
                m= Instantiate(housesPrefs[5]);
                break;
            case 5:
                if (housesPrefs[4] == null) housesPrefs[4] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl5");
                m = Instantiate(housesPrefs[4]);
                break;
            case 4:
                if (housesPrefs[3] == null) housesPrefs[3] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl4");
                m = Instantiate(housesPrefs[3]);
                break;
            case 3:
                if (housesPrefs[2] == null) housesPrefs[2] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl3");
                m = Instantiate(housesPrefs[2]);
                break;
            case 2:
                if (housesPrefs[1] == null) housesPrefs[1] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl2");
                m = Instantiate(housesPrefs[1]);
                break;
            default:
                if (housesPrefs[0] == null) housesPrefs[0] = Resources.Load<GameObject>("Structures/Settlement/housePart_lvl1");
                m = Instantiate(housesPrefs[0]);
                break;
        }
        if (!PoolMaster.useDefaultMaterials) PoolMaster.ReplaceMaterials(m);
        return m;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        basement = b;
        surfaceRect = new SurfaceRect(pos.x, pos.y, surfaceRect.size);
        if (!GameMaster.loading)
        {
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

    public void SetActivationStatus(bool x)
    {
        // source: Building. ChangeRenderersView
        if (destroyed) return;
        isActive = x;
        if (transform.childCount == 0) return;
        Renderer[] myRenderers = transform.GetChild(0).GetComponentsInChildren<Renderer>();
        if (myRenderers == null | myRenderers.Length == 0) return;
        if (isActive) PoolMaster.SwitchMaterialsToOnline(myRenderers);
        else PoolMaster.SwitchMaterialsToOffline(myRenderers);
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(order);
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
    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + 2];
        fs.Read(data, 0, data.Length);
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        var ppos = new PixelPosByte(data[0], data[1]);        
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 12);

        SetData((Settlement.SettlementStructureType)data[STRUCTURE_SERIALIZER_LENGTH + 1], data[STRUCTURE_SERIALIZER_LENGTH],  null);
        SetBasement(sblock, ppos);
    }
}
