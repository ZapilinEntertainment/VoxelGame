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
                model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/housePart"));
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

    public void SetData(Settlement.SettlementStructureType i_type, byte i_level, Settlement i_settlement)
    {
        type = i_type;
        level = i_level;
        switch (type)
        {
            case Settlement.SettlementStructureType.House:
                switch (level)
                {
                    case 0: value = 5f;  break;
                    default: value = 10f; break;
                }
                break;
            case Settlement.SettlementStructureType.Garden:
            case Settlement.SettlementStructureType.Shop:
                value = 1f;
                break;
        }
        settlement = i_settlement;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(clearFromSurface, returnResources, leaveRuins);
        basement = null;
        if (settlement != null) settlement.Recalculate();
        Destroy(gameObject);
    }
}
