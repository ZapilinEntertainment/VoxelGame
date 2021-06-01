using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Hotel : Building, IPlanable
{
    private byte lodgersCount;
    private Block myBlock;
    private Plane upperPlane, bottomPlane;
    private ColonyController colony;
    private const float RENT = 1f, NEGATIVE_EFFECT_TIMER = 25f;
    private const byte MAX_LODGERS_COUNT = 150;
    private static List<Hotel> hotels;

    static Hotel()
    {
        GameMaster.staticResetFunctions += ResetHotelStaticData;
    }
    public static void ResetHotelStaticData()
    {
        hotels = new List<Hotel>();
    }
    public static void DistributeLodgers(int x)
    {        
        if (hotels != null && hotels.Count > 0)
        {
            int count = hotels.Count;
            if (count == 1)
            {
                if (x > MAX_LODGERS_COUNT) x = MAX_LODGERS_COUNT;
                hotels[0].AddLodgers((byte)x);
            }
            else
            {
                var h = hotels[Random.Range(0, count)];
                byte n;
                if (x > MAX_LODGERS_COUNT) n = MAX_LODGERS_COUNT;
                else n = (byte)x;
                x -= n;
                x += h.AddLodgers(n);
                if (x > 0)
                {
                    h = hotels[Random.Range(0, count)];
                    if (x > MAX_LODGERS_COUNT) x = MAX_LODGERS_COUNT;
                    h.AddLodgers((byte)x);
                }
            }
        }
    }
    public static string GetInfo()
    {
        int lodgersCount = 0;
        if (hotels != null && hotels.Count > 0)
        {
            foreach (var h in hotels) lodgersCount += h.lodgersCount;
        }
        if (lodgersCount != 0) return Localization.GetPhrase(LocalizedPhrase.LodgersCount) + ": " + lodgersCount.ToString() +
            "\n" + Localization.GetPhrase(LocalizedPhrase.TotalRent) + ": +" + ((int)(lodgersCount * RENT));
        else return Localization.GetPhrase(LocalizedPhrase.LodgersCount) + ": 0" +
            "\n" + Localization.GetPhrase(LocalizedPhrase.TotalRent) + ": 0";
    }

    /// <summary>
    /// returns excess lodgers
    /// </summary>
    private int AddLodgers(byte x)
    {
        if (x == 0 || lodgersCount == MAX_LODGERS_COUNT) return 0;
        else {
            int v = lodgersCount + x, excess;
            if (v > MAX_LODGERS_COUNT)
            {
                x = (byte)(MAX_LODGERS_COUNT - lodgersCount);
                lodgersCount = MAX_LODGERS_COUNT;
                excess = v - MAX_LODGERS_COUNT;
            }
            else
            {
                lodgersCount += x;
                excess = 0;
            }
            colony.AddCitizens(x, false);
            return excess;
        }        
    }
    private void RemoveLodgers( byte x)
    {
        if (x == 0 | lodgersCount == 0) return;
        if (lodgersCount >= x)
        {
            lodgersCount -= x;
            colony.RemoveCitizens(x, false);
        }
        else
        {
            colony.RemoveCitizens(lodgersCount, false);
            lodgersCount = 0;
        }
    }

    //--------
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
        var master = GameMaster.realMaster;
        if (!subscribedToUpdate)
        {
            master.everydayUpdate += EverydayUpdate;
            subscribedToUpdate = true;           
        }
        if (hotels == null)
        {
            hotels = new List<Hotel>();
            hotels.Add(this);
        }
        else
        {
            if (!hotels.Contains(this)) hotels.Add(this);
        }
        colony = master.colonyController;

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
            c.AddEnergyCrystals(lodgersCount * RENT * c.happinessCoefficient);
            if (Random.value > c.happinessCoefficient)
            {
                RemoveLodgers((byte)(Random.value * 0.2f * lodgersCount));
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

    protected override void INLINE_SetVisibility(VisibilityMode vmode) { }
    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (!destroyed)
        {
            IPlanableSupportClass.Annihilate(this, order);
        }
    }

    #region interface
    public bool HaveBlock() { return myBlock != null; }
    public void NullifyBlockLink() { myBlock = null; }
    public void IPlanable_SetVisibility(VisibilityMode vmode) {
        if (vmode != visibilityMode )
        {
            if (upperPlane != null) upperPlane.SetVisibilityMode(vmode);
            if (bottomPlane != null) bottomPlane.SetVisibilityMode(vmode);
            transform.GetChild(0).gameObject.SetActive(vmode != VisibilityMode.Invisible & vmode != VisibilityMode.LayerCutHide);
            visibilityMode = vmode;
        }
    } 
    public void Delete(BlockAnnihilationOrder bo)
    {
        if (destroyed) return;
        else destroyed = true;
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.everydayUpdate -= EverydayUpdate;
            subscribedToUpdate = false;            
        }
        var so = bo.GetStructureOrder();
        if (so.doSpecialChecks) hotels.Remove(this);
        PrepareBuildingForDestruction(so);
        if (so.doSpecialChecks && lodgersCount > 0) DistributeLodgers(lodgersCount);
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
    public bool IsSurface() { return false; }
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
                if (upperPlane.isClean) upperPlane = null; else upperPlane.SetVisibilityMode(VisibilityMode.Invisible);
                myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
            }
        }
        else
        {
            if (faceIndex == Block.DOWN_FACE_INDEX)
            {
                if (bottomPlane != null)
                {
                    if (bottomPlane.isClean) bottomPlane = null; else bottomPlane.SetVisibilityMode(VisibilityMode.Invisible);
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

    override public void Load(System.IO.Stream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        lodgersCount = (byte)fs.ReadByte();
    }

    public void SavePlanesData(System.IO.Stream fs)
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
    public void LoadPlanesData(System.IO.Stream fs)
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

    public override UIObserver ShowOnGUI()
    {
        var bo = base.ShowOnGUI();
        UIObserver.mycanvas.EnableTextfield(ID);
        return bo;
    }
    override public void DisabledOnGUI()
    {
        showOnGUI = false;
        UIObserver.mycanvas.DisableTextfield(ID);
    }
}
