using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Gardens : WorkBuilding
{
    private byte[] modelsInfo;
    private int lifepowerAffectID = -1;
    private const float LIFEPOWER_AFFECTION = 40f;
    private const byte SMALL_PARK_ID = 0, SMALL_TANK_ID = 1, SMALL_HIGHTREE_ID = 2, SMALL_TOWER_ID = 3,
        DOUBLE_MIDPARK_ID = 4, DOUBLE_HIGHPARK_ID = 5, BIG_MIDPARK_ID = 6, BIG_HIGHPARK_ID = 7;

    override protected void SetModel()
    {
        //switch skin index        
        GameObject model;
        Quaternion prevRot = Quaternion.identity;
        if (transform.childCount != 0) return;
        model = new GameObject("gardenModel");
        model.transform.parent = transform;
        model.transform.localRotation = prevRot;
        model.transform.localPosition = Vector3.zero;

        //generating model info
        if (modelsInfo == null)
        {
            var milist = new List<byte>();
            float f = Random.value;
            byte modelCode = 0, posCodeX = 0, posCodeZ = 0;
            // 0-8, 0-8
            if (f > 0.5f)
            {
                f *= 2f;
                if (f <= 0.4f) // small
                {
                    f /= 0.4f;
                    if (f > 0.6f)
                    {
                        if (f > 0.9f) modelCode = SMALL_TOWER_ID;
                        else modelCode = SMALL_TANK_ID;
                    }
                    else
                    {
                        if (f < 0.4f) modelCode = SMALL_HIGHTREE_ID;
                        else modelCode = SMALL_PARK_ID;
                    }
                    f = Random.value;
                    if (f > 0.5f)
                    {
                        if (f > 0.75f) { posCodeX = 0; posCodeZ = 4; }
                        else { posCodeX = 0; posCodeZ = 0; }
                    }
                    else
                    {
                        if (f > 0.25f) { posCodeX = 4; posCodeZ = 0; }
                        else { posCodeX = 4; posCodeZ = 4; }
                    }
                }
                else
                {
                    if (f >= 0.8f) // big
                    {
                        f = (f - 0.8f) / 0.2f;
                        if (f > 0.65f) modelCode = BIG_HIGHPARK_ID; else modelCode = BIG_MIDPARK_ID;
                        posCodeX = 0;
                        posCodeZ = 0;
                    }
                    else // double
                    {
                        f = (f - 0.4f) / 0.4f;
                        if (f > 0.65f) modelCode = DOUBLE_HIGHPARK_ID; else modelCode = DOUBLE_MIDPARK_ID;
                        posCodeX = 0;
                        posCodeZ = 0;
                    }
                }
                milist.Add(posCodeX);
                milist.Add(posCodeZ);
                milist.Add(modelCode);
            }
            // 8-16, 0-8
            f = Random.value;
            if (f > 0.5f)
            {
                f *= 2f;
                if (f <= 0.4f) // small
                {
                    f /= 0.4f;
                    if (f > 0.6f)
                    {
                        if (f > 0.9f) modelCode = SMALL_TOWER_ID;
                        else modelCode = SMALL_TANK_ID;
                    }
                    else
                    {
                        if (f < 0.4f) modelCode = SMALL_HIGHTREE_ID;
                        else modelCode = SMALL_PARK_ID;
                    }
                    f = Random.value;
                    if (f > 0.5f)
                    {
                        if (f > 0.75f) { posCodeX = 8; posCodeZ = 4; }
                        else { posCodeX = 8; posCodeZ = 0; }
                    }
                    else
                    {
                        if (f > 0.25f) { posCodeX = 12; posCodeZ = 0; }
                        else { posCodeX = 12; posCodeZ = 4; }
                    }
                }
                else
                {
                    if (f >= 0.8f) // big
                    {
                        f = (f - 0.8f) / 0.2f;
                        if (f > 0.65f) modelCode = BIG_HIGHPARK_ID; else modelCode = BIG_MIDPARK_ID;
                        posCodeX = 8;
                        posCodeZ = 0;
                    }
                    else // double
                    {
                        f = (f - 0.4f) / 0.4f;
                        if (f > 0.65f) modelCode = DOUBLE_HIGHPARK_ID; else modelCode = DOUBLE_MIDPARK_ID;
                        posCodeX = 8;
                        posCodeZ = 0;
                    }
                }
                milist.Add(posCodeX);
                milist.Add(posCodeZ);
                milist.Add(modelCode);
            }
            // 0-8, 8-16
            f = Random.value;
            if (f > 0.5f)
            {
                f *= 2f;
                if (f <= 0.4f) // small
                {
                    f /= 0.4f;
                    if (f > 0.6f)
                    {
                        if (f > 0.9f) modelCode = SMALL_TOWER_ID;
                        else modelCode = SMALL_TANK_ID;
                    }
                    else
                    {
                        if (f < 0.4f) modelCode = SMALL_HIGHTREE_ID;
                        else modelCode = SMALL_PARK_ID;
                    }
                    f = Random.value;
                    if (f > 0.5f)
                    {
                        if (f > 0.75f) { posCodeX = 0; posCodeZ = 12; }
                        else { posCodeX = 0; posCodeZ = 8; }
                    }
                    else
                    {
                        if (f > 0.25f) { posCodeX = 4; posCodeZ = 8; }
                        else { posCodeX = 4; posCodeZ = 12; }
                    }
                }
                else
                {
                    if (f >= 0.8f) // big
                    {
                        f = (f - 0.8f) / 0.2f;
                        if (f > 0.65f) modelCode = BIG_HIGHPARK_ID; else modelCode = BIG_MIDPARK_ID;
                        posCodeX = 0;
                        posCodeZ = 8;
                    }
                    else // double
                    {
                        f = (f - 0.4f) / 0.4f;
                        if (f > 0.65f) modelCode = DOUBLE_HIGHPARK_ID; else modelCode = DOUBLE_MIDPARK_ID;
                        posCodeX = 0;
                        posCodeZ = 8;
                    }
                }
                milist.Add(posCodeX);
                milist.Add(posCodeZ);
                milist.Add(modelCode);
            }
            // 8- 16, 8 - 16
            f = Random.value;
            if (f <= 0.4f) // small
            {
                f /= 0.4f;
                if (f > 0.6f)
                {
                    if (f > 0.9f) modelCode = SMALL_TOWER_ID;
                    else modelCode = SMALL_TANK_ID;
                }
                else
                {
                    if (f < 0.4f) modelCode = SMALL_HIGHTREE_ID;
                    else modelCode = SMALL_PARK_ID;
                }
                f = Random.value;
                if (f > 0.5f)
                {
                    if (f > 0.75f) { posCodeX = 8; posCodeZ = 12; }
                    else { posCodeX = 8; posCodeZ = 8; }
                }
                else
                {
                    if (f > 0.25f) { posCodeX = 12; posCodeZ = 8; }
                    else { posCodeX = 12; posCodeZ = 12; }
                }
            }
            else
            {
                if (f >= 0.8f) // big
                {
                    f = (f - 0.8f) / 0.2f;
                    if (f > 0.65f) modelCode = BIG_HIGHPARK_ID; else modelCode = BIG_MIDPARK_ID;
                    posCodeX = 8;
                    posCodeZ = 8;
                }
                else // double
                {
                    f = (f - 0.4f) / 0.4f;
                    if (f > 0.65f) modelCode = DOUBLE_HIGHPARK_ID; else modelCode = DOUBLE_MIDPARK_ID;
                    posCodeX = 8;
                    posCodeZ = 8;
                }
            }
            milist.Add(posCodeX);
            milist.Add(posCodeZ);
            milist.Add(modelCode);
            //
            modelsInfo = milist.ToArray();
        }
        //constructing model
        int i = 0;
        byte size = 0;
        var mlist = new List<GameObject>();
        GameObject g = null;
        Transform parent = model.transform;
        float ir = (float)PlaneExtension.INNER_RESOLUTION;
        while (i < modelsInfo.Length)
        {
            switch (modelsInfo[i])
            {
                case SMALL_PARK_ID:
                    g = Instantiate(Resources.Load<GameObject>("Structures/Settlement/parkPart"));
                    break;
                case SMALL_TOWER_ID:
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/small_mast"));
                    break;
                case SMALL_TANK_ID:
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/small_tank"));
                    break;
                case SMALL_HIGHTREE_ID:
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/small_hightree"));
                    break;
                case DOUBLE_HIGHPARK_ID:
                    size = 2;
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/double_highpark"));
                    break;
                case DOUBLE_MIDPARK_ID:
                    size = 2;
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/double_midhpark"));
                    break;
                case BIG_HIGHPARK_ID:
                    size = 1;
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/big_highpark"));
                    break;
                case BIG_MIDPARK_ID:
                    size = 1;
                    g = Instantiate(Resources.Load<GameObject>("Prefs/Gardenparts/big_midpark"));
                    break;
            }
            if (g != null)
            {
                g.transform.parent = parent;
                g.transform.localPosition =
                    (Vector3.right * (modelsInfo[i + 1] / ir - 0.5f)) * Block.QUAD_SIZE +
                    (Vector3.forward * (modelsInfo[i + 2] / ir - 0.5f)) * Block.QUAD_SIZE;
            }
            i += 3;
        }
        //
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(model, true);
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (lifepowerAffectID == -1)
        {
            var n = GameMaster.realMaster.mainChunk.GetNature();
            if (n != null)
            {
                lifepowerAffectID =  n.AddLifepowerAffection(isEnergySupplied ? LIFEPOWER_AFFECTION : LIFEPOWER_AFFECTION / 2f);
            }
        }
    }
    override protected void SwitchActivityState()
    {
        base.SwitchActivityState();
        if (lifepowerAffectID != -1)
        {
            GameMaster.realMaster.mainChunk.GetNature()?.ChangeLifepowerAffection(lifepowerAffectID, isEnergySupplied ? LIFEPOWER_AFFECTION : LIFEPOWER_AFFECTION / 2f);
        }
        else
        {
            if (isEnergySupplied)
                lifepowerAffectID = GameMaster.realMaster.mainChunk.GetNature().AddLifepowerAffection(isEnergySupplied ? LIFEPOWER_AFFECTION : LIFEPOWER_AFFECTION / 2f);
        }
    }
}
