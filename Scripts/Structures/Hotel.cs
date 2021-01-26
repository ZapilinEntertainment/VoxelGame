using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Hotel : Building, IPlanable
{
    public byte lodgersCount { get; private set; }
    private Block myBlock;
    private Plane upperPlane, bottomPlane;
    private const float RENT = 1f, NEGATIVE_EFFECT_TIMER = 25f;
    private const byte MAX_LODGERS_COUNT = 150;
    private static List<Hotel> hotels;

    public static void DistributeLodgers(int x)
    {
        if (hotels != null )
        {
            int count = hotels.Count;
            if (count == 1)
            {
                var h = hotels[0];
                if (h.lodgersCount + x > MAX_LODGERS_COUNT) h.lodgersCount = MAX_LODGERS_COUNT;
                else h.lodgersCount += (byte)x;
            }
            else
            {
                int i = Random.Range(0, count);
                var h = hotels[i];
                if ( h.lodgersCount + x <= MAX_LODGERS_COUNT) h.lodgersCount += (byte)x;
                else
                {
                    x -= MAX_LODGERS_COUNT - h.lodgersCount;
                    h.lodgersCount = MAX_LODGERS_COUNT;
                    i += 1;
                    if (i == count) i = 0;
                    h = hotels[i];
                    if (h.lodgersCount + x <= MAX_LODGERS_COUNT) h.lodgersCount += (byte)x;
                    else h.lodgersCount = MAX_LODGERS_COUNT;
                }
            }
        }
    }

    override public void Prepare() {
        PrepareBuilding();
        lodgersCount = 0;
    }
    private Plane PrepareUpperPlane()
    {
        upperPlane = new Plane(this, MeshType.Quad, ResourceType.CONCRETE_ID, Block.UP_FACE_INDEX, 0);
        return upperPlane;
    }
    private Plane PrepareBottomPlane()
    {
        bottomPlane = new Plane(this, MeshType.Quad, ResourceType.CONCRETE_ID, Block.DOWN_FACE_INDEX, 0);
        return bottomPlane;
    }
    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.everydayUpdate += EverydayUpdate;
            subscribedToUpdate = true;
            if (hotels != null) hotels.Add(this);
            else hotels = new List<Hotel>() { this };
        }
        if (!GameMaster.loading) IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock, true);
    }

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        bool x = isActive & isEnergySupplied;
        if (x == false)
        {
            if (subscribedToUpdate)
            {
                var gm = GameMaster.realMaster;
                gm.everydayUpdate -= EverydayUpdate;
                subscribedToUpdate = false;
                var cc = gm.colonyController;
                cc.AddHappinessAffect(lodgersCount * 2f / (float)cc.citizenCount, NEGATIVE_EFFECT_TIMER);
            }
            lodgersCount = 0;
        }
        else
        {
            if (!subscribedToUpdate)
            {
                GameMaster.realMaster.everydayUpdate += EverydayUpdate;
                subscribedToUpdate = true;
            }
        }
    }
    private void EverydayUpdate()
    {
        if (lodgersCount > 0)
        {            
            var c = GameMaster.realMaster.colonyController;            
            c.AddEnergyCrystals(lodgersCount * RENT * c.happiness_coefficient);
            if (Random.value > c.happiness_coefficient)
            {
                if (lodgersCount == 1) lodgersCount = 0;
                else
                {
                    lodgersCount -= (byte)(Random.value * 0.5f * lodgersCount);
                }
            }
        }
    }

    public static bool CheckSpecialBuildingCondition(Plane p, ref string reason)
    {
        if (p.materialID != PoolMaster.MATERIAL_ADVANCED_COVERING_ID)
        {
            reason = Localization.GetRefusalReason(RefusalReason.MustBeBuildedOnFoundationBlock);
            return false;
        }
        else return true;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.everydayUpdate -= EverydayUpdate;
            subscribedToUpdate = false;
        }
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        hotels.Remove(this);
        if (lodgersCount > 0) DistributeLodgers(lodgersCount);
        if (myBlock == null) Delete(clearFromSurface, returnResources, leaveRuins);
        else
        {
            myBlock.myChunk.DeleteBlock(myBlock.pos, returnResources);
        }
    }

    #region interface
    public void Delete(bool clearFromSurface, bool compensateResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(clearFromSurface, compensateResources, leaveRuins);
        basement = null;
        Destroy(gameObject);
    }
    override public bool IsIPlanable() { return true; }
    public bool IsStructure() { return true; }
    public bool IsFaceTransparent(byte faceIndex)
    {
        return !(faceIndex == Block.UP_FACE_INDEX | faceIndex == Block.DOWN_FACE_INDEX);
    }
    public bool HavePlane(byte faceIndex)
    {
        return (faceIndex == Block.UP_FACE_INDEX | faceIndex == Block.DOWN_FACE_INDEX);
    }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane != null)
            {
                result = upperPlane;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
        else
        {
            if (faceIndex == Block.DOWN_FACE_INDEX)
            {
                if (bottomPlane != null)
                {
                    result = bottomPlane;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
    public Plane FORCED_GetPlane(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane == null) PrepareUpperPlane();
            return upperPlane;
        }
        else {
            if (faceIndex == Block.DOWN_FACE_INDEX)
            {
                if (bottomPlane == null) PrepareBottomPlane();
                return bottomPlane;
            }
            else return null;
        }
    }
    public Block GetBlock() { return myBlock; }
    public bool IsCube() { return false; }
    public bool TryToRebasement()
    {
        if (myBlock == null) return false;
        else
        {
            var pos = myBlock.pos;
            var chunk = myBlock.myChunk;
            Block b = chunk.GetBlock(pos.OneBlockDown());
            if (b != null && b.HavePlane(Block.UP_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.UP_FACE_INDEX);
                basement = p;
                basement.AddStructure(this);
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockHigher());
            if (b != null && b.HavePlane(Block.DOWN_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.DOWN_FACE_INDEX);
                basement = p;
                basement.AddStructure(this);
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockForward());
            if (b != null && b.HavePlane(Block.BACK_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.BACK_FACE_INDEX);
                basement = p;
                basement.AddStructure(this);
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockRight());
            if (b != null && b.HavePlane(Block.LEFT_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.LEFT_FACE_INDEX);
                basement = p;
                basement.AddStructure(this);
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockBack());
            if (b != null && b.HavePlane(Block.FWD_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.FWD_FACE_INDEX);
                basement = p;
                basement.AddStructure(this);
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockLeft());
            if (b != null && b.HavePlane(Block.RIGHT_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.RIGHT_FACE_INDEX);
                basement = p;
                basement.AddStructure(this);
                return true;
            }
            return false;
        }
    }
    public bool ContainSurface()
    {
        return upperPlane != null;
    }
    public byte GetAffectionMask() { return ((1 << Block.UP_FACE_INDEX) + (1 << Block.DOWN_FACE_INDEX)); }

    public bool ContainsStructures()
    {
        if (upperPlane == null && bottomPlane == null) return false;
        else return (upperPlane?.ContainStructures() ?? false) & (bottomPlane?.ContainStructures() ?? false);
    }
    public bool TryGetStructuresList(ref List<Structure> result)
    {
        if (upperPlane == null && bottomPlane == null)
        {
            result = null;
            return false;
        }
        else
        {
            var slist = upperPlane?.GetStructuresList();
            if (bottomPlane != null)
            {
                if (slist == null) slist = bottomPlane.GetStructuresList();
                else slist.AddRange(bottomPlane.GetStructuresList());
            }
            if (slist != null)
            {
                result = slist;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }

    //returns false if transparent or wont be instantiated
    public bool InitializePlane(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane == null) PrepareUpperPlane();
            return true;
        }
        else {
            if (faceIndex == Block.DOWN_FACE_INDEX)
            {
                if (bottomPlane == null) PrepareBottomPlane();
                return true;
            }
            else return false;
        }
    }
    public void DeactivatePlane(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane != null)
            {
                if (upperPlane.isClean) upperPlane = null; else upperPlane.SetVisibility(false);
                myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
            }
        }
        else
        {
            if (faceIndex == Block.DOWN_FACE_INDEX)
            {
                if (bottomPlane != null)
                {
                    if (bottomPlane.isClean) bottomPlane = null; else bottomPlane.SetVisibility(false);
                    myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
                }
            }
        }
    }
    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask)
    {
        var result = new List<BlockpartVisualizeInfo>();
        if ((visualMask & (1 << Block.UP_FACE_INDEX)) != 0)
        {
            result.Add(GetFaceVisualData(Block.UP_FACE_INDEX));
        }
        if ((visualMask & (1 << Block.DOWN_FACE_INDEX)) != 0)
        {
            result.Add(GetFaceVisualData(Block.DOWN_FACE_INDEX));
        }
        if (result.Count > 0)
        {
            return result;
        }
        else return null;
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if (faceIndex == Block.UP_FACE_INDEX)
        {
            if (upperPlane != null) return upperPlane.GetVisualInfo(myBlock.myChunk, myBlock.pos);
            else return PrepareUpperPlane().GetVisualInfo(myBlock.myChunk, myBlock.pos);
        }
        else
        {
            if (faceIndex == Block.DOWN_FACE_INDEX)
            {
                if (bottomPlane != null) return bottomPlane.GetVisualInfo(myBlock.myChunk, myBlock.pos);
                else return PrepareBottomPlane().GetVisualInfo(myBlock.myChunk, myBlock.pos);
            }
            else return null;
        }
    }


    public void Damage(float f, byte faceIndex)
    {
        ApplyDamage(f);
    }
    #endregion

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.Add(lodgersCount);
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        lodgersCount = (byte)fs.ReadByte();
    }

    public void SavePlanesData(System.IO.FileStream fs)
    {
        if (upperPlane != null)
        {
            fs.WriteByte(1);
            upperPlane.Save(fs);
        }
        else fs.WriteByte(0);
        if (bottomPlane != null)
        {
            fs.WriteByte(1);
            bottomPlane.Save(fs);
        }
        else fs.WriteByte(0);
    }
    public void LoadPlanesData(System.IO.FileStream fs)
    {
        var b = fs.ReadByte();
        IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock, false);
        if (b == 1)
        {
            upperPlane = Plane.Load(fs, this);
        }
        b = fs.ReadByte();
        if (b == 1)
        {
            bottomPlane = Plane.Load(fs, this);
        }
    }
    #endregion
}
