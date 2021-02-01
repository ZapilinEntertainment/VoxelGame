using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorBlock : Powerplant, IPlanable
{
    private Block myBlock;
    private Dictionary<byte, Plane> planes;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        ChangeRenderersView(energySurplus > 0f);

        if (!GameMaster.loading) IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock, true);
    }

    #region individual functions
    public Plane CreatePlane(byte faceIndex, bool redrawCall)
    {
        if (planes == null) planes = new Dictionary<byte, Plane>();
        else
        {
            if (planes.ContainsKey(faceIndex)) return planes[faceIndex];
        }

        var pos = myBlock.pos;
        Plane p;
        if (faceIndex < 4)
        {
            MeshType mtype = MeshType.ReactorSide_0;
            var f = Random.value;
            if (f < 0.7f)
            {
                if (f > 0.35f) mtype = MeshType.ReactorSide_1;
            }
            else
            {
                if (f > 0.9f) mtype = MeshType.DoubleWindows;
                else
                {
                    mtype = f > 0.8f ? MeshType.IndustryHeater0 : MeshType.IndustryHeater1;
                }
            }
            var mp = new MultimaterialPlane(this, mtype, faceIndex, 0);
            mp.SetActivationStatus(isActive);
            p = mp;
        }
        else
        {
            if (faceIndex == Block.UP_FACE_INDEX && pos.y == Chunk.chunkSize - 1)
            {
                p = MeshMaster.GetRooftop(this, Random.value < 0.1f, true);
            }
            else p = new Plane(this, MeshType.Quad, PoolMaster.MATERIAL_ADVANCED_COVERING_ID, faceIndex, 0);
        }
        //
        planes.Add(faceIndex, p);
        if (redrawCall) myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
        return p;
    }
    override public void SetModelRotation(int r)
    {
        if (r < 11 | planes == null) return;
        else
        {
            byte f = (byte)(r - 11);
            if (f < 4 && planes.ContainsKey(f))
            {
                var p = planes[f] as MultimaterialPlane;
                if (p != null)
                {
                    switch (p.meshType)
                    {
                        case MeshType.ReactorSide_0: p.ChangeMesh(MeshType.ReactorSide_1); break;
                        case MeshType.ReactorSide_1: p.ChangeMesh(MeshType.DoubleWindows); break;
                        case MeshType.DoubleWindows: p.ChangeMesh(MeshType.IndustryHeater0); break;
                        case MeshType.IndustryHeater0: p.ChangeMesh(MeshType.IndustryHeater1); break;
                    }
                }
            }
        }
    }
    public void Delete(bool clearFromSurface, bool compensateResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, compensateResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        //
        if (planes != null)
        {
            foreach (var p in planes) p.Value.Annihilate(compensateResources);
        }
        //
        Destroy(gameObject);
    }
    #endregion

    #region cubeStructures standart functions
    protected override void SetModel() { }
    protected override void INLINE_SetVisibility(VisibilityMode vmode)
    {
        // нужно переопределение, чтобы не действовали функции предков
    }

    // side-models only
    override protected void ChangeRenderersView(bool setOnline)
    {
        if (planes == null) return;
        else
        {
            MultimaterialPlane mp;
            foreach (var p in planes)
            {
                mp = p.Value as MultimaterialPlane;
                if (mp != null) mp.SetActivationStatus(setOnline);
            }
        }
    }
    //
    override public void SectionDeleted(ChunkPos pos)
    {
        if (basement == null && !TryToRebasement()) Annihilate(false, false, false);
    }
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

    override public void ApplyDamage(float d)
    {
        if (destroyed | indestructible) return;
        hp -= d;
        if (hp <= 0) Delete(true, false, true);
    }
    override public void Annihilate(bool clearFromSurface, bool compensateResources, bool leaveRuins)
    {
        if (myBlock == null) Delete(clearFromSurface, compensateResources, leaveRuins);
        else
        {
            var b = myBlock;
            myBlock = null;
            b.myChunk.DeleteBlock(b.pos, compensateResources);
        }
    }
    #endregion

    #region interface 
    override public bool IsIPlanable() { return true; }
    public bool IsStructure() { return true; }
    public bool IsFaceTransparent(byte faceIndex)
    {
        return false;
    }
    public bool HavePlane(byte faceIndex)
    {
        return !(faceIndex == Block.SURFACE_FACE_INDEX | faceIndex == Block.CEILING_FACE_INDEX);
    }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (!HavePlane(faceIndex)) { result = null; return false; }
        else
        {
            if (planes != null) return planes.TryGetValue(faceIndex, out result);
            else
            {
                result = null;
                return false;
            }
        }
    }
    public Plane FORCED_GetPlane(byte faceIndex)
    {
        if (faceIndex != Block.SURFACE_FACE_INDEX & faceIndex != Block.CEILING_FACE_INDEX)
        {
            if (planes == null || !planes.ContainsKey(faceIndex))
            {
                return CreatePlane(faceIndex, true);
            }
            else return planes[faceIndex];
        }
        else return null;
    }
    public Block GetBlock() { return myBlock; }
    public bool IsCube() { return true; }
    public bool ContainSurface()
    {
        return planes?.ContainsKey(Block.UP_FACE_INDEX) ?? false;
    }
    public byte GetAffectionMask()
    {
        return Block.CUBE_MASK;
    }

    public bool ContainsStructures()
    {
        if (planes == null) return false;
        else
        {
            foreach (var p in planes)
            {
                if (p.Value.ContainStructures()) return true;
            }
            return false;
        }
    }
    public bool TryGetStructuresList(ref List<Structure> result)
    {
        if (planes == null) return false;
        else
        {
            if (result == null) result = new List<Structure>();
            List<Structure> slist;
            foreach (var p in planes)
            {
                slist = p.Value.GetStructuresList();
                if (slist != null) result.AddRange(slist);
            }
            return true;
        }
    }
    //returns false if transparent or wont be instantiated
    public bool InitializePlane(byte faceIndex)
    {
        //#cubeStructure_InitializePlane
        if (faceIndex == Block.SURFACE_FACE_INDEX | faceIndex == Block.CEILING_FACE_INDEX) return false;
        else
        {
            if (planes != null && planes.ContainsKey(faceIndex))
            {
                var p = planes[faceIndex];
                if (p.visibilityMode == VisibilityMode.Invisible)
                {
                    p.SetBasisVisibility();
                }
                return true;
            }
            else
            {
                CreatePlane(faceIndex, false);
                return true;
            }
        }
        //
    }
    public void DeactivatePlane(byte faceIndex)
    {
        if (planes == null) return;
        if (faceIndex != Block.SURFACE_FACE_INDEX & faceIndex != Block.CEILING_FACE_INDEX)
        {
            if (planes.ContainsKey(faceIndex))
            {
                if (planes[faceIndex].isClean)
                {
                    planes.Remove(faceIndex);
                    if (planes.Count == 0) planes = null;
                }
                else planes[faceIndex].SetVisibilityMode(VisibilityMode.Invisible);
                if (!GameMaster.loading) myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
            }
        }
    }

    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte vismask)
    {
        return IPlanableSupportClass.GetVisualizeInfo(ref vismask, this, ref myBlock, ref planes, CreatePlane);
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if ((Block.CUBE_MASK & (1 << faceIndex)) != 0)
        {
            if (planes != null && planes.ContainsKey(faceIndex)) return planes[faceIndex].GetVisualInfo(myBlock.myChunk, myBlock.pos);
            else return CreatePlane(faceIndex, false)?.GetVisualInfo(myBlock.myChunk, myBlock.pos);
        }
        else
        {
            if (planes != null && planes.ContainsKey(faceIndex)) planes[faceIndex].SetVisibilityMode(VisibilityMode.Invisible);
            return null;
        }
    }


    public void Damage(float f, byte faceIndex)
    {
        ApplyDamage(f);
    }
    #endregion

    #region save-load
    public void SavePlanesData(System.IO.FileStream fs)
    {
        if (planes != null && planes.Count > 0)
        {
            fs.WriteByte((byte)planes.Count);
            foreach (var p in planes)
            {
                p.Value.Save(fs);
            }
        }
        else fs.WriteByte(0);
    }
    public void LoadPlanesData(System.IO.FileStream fs)
    {
        var count = fs.ReadByte();
        if (count > 0)
        {
            IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock, false);
            planes = new Dictionary<byte, Plane>();
            for (int i = 0; i < count; i++)
            {
                var p = Plane.Load(fs, this);
                planes.Add(p.faceIndex, p);
            }
        }
    }
    #endregion
}
