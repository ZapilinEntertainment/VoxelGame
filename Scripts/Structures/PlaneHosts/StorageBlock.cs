using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class StorageBlock : StorageHouse, IPlanable
{
    private Block myBlock;
    private Dictionary<byte, Plane> planes;
    private GameObject[] sideModels;       

    override public void SetBasement(Plane p, PixelPosByte pos)
    {
        if (p == null) return;
        SetBuildingData(p, pos);
        GameMaster.realMaster.colonyController.storage.AddWarehouse(this);

        IPlanableSupportClass.AddBlockRepresentation(this, basement, ref myBlock);
    }

    
    //individual meshtypes collection
    public Plane CreatePlane(byte faceIndex, bool redrawCall)
    {
        if (planes == null) planes = new Dictionary<byte, Plane>();
        else
        {
            if (planes.ContainsKey(faceIndex)) return planes[faceIndex];
        }
        var pos = myBlock.pos;
        MeshType mtype = MeshType.Quad;
        bool isSideMesh = faceIndex < 4;
        if (isSideMesh)
        {
            mtype = Random.value > 0.75f ? MeshType.StorageEntrance : MeshType.StorageSide;
        }
        else
        {
            if (faceIndex == Block.UP_FACE_INDEX && pos.y == Chunk.CHUNK_SIZE - 1)
            {
                var px = MeshMaster.GetRooftop(this, Random.value < 0.1f, true);
                planes.Add(faceIndex, px);
                if (redrawCall) myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
                return px;
            }
        }
        //
        var p = new Plane(this, mtype, PoolMaster.MATERIAL_ADVANCED_COVERING_ID, faceIndex, 0);
        planes.Add(faceIndex, p);
        if (isSideMesh)
        {
            if (sideModels[faceIndex] == null)
            {
                var m = MeshMaster.InstantiateAdvancedMesh(mtype);
                var t = m.transform;
                t.parent = myBlock.myChunk.GetRenderersHolderTransform(p.faceIndex);
                t.localPosition = p.GetCenterPosition();
                switch (faceIndex)
                {
                    case Block.FWD_FACE_INDEX: t.localRotation = Quaternion.identity; ; break;
                    case Block.RIGHT_FACE_INDEX: t.localRotation = Quaternion.Euler(0f, 90f, 0f); break;
                    case Block.BACK_FACE_INDEX: t.localRotation = Quaternion.Euler(0f, 180f, 0f); break;
                    case Block.LEFT_FACE_INDEX: t.localRotation = Quaternion.Euler(0f, -90f, 0f); break;
                    case Block.UP_FACE_INDEX: t.localRotation = Quaternion.Euler(-90f, 0f, 0f); break;
                    case Block.DOWN_FACE_INDEX: t.localRotation = Quaternion.Euler(90f, 90f, 0f); break;
                }
                m.AddComponent<StructurePointer>().SetStructureLink(this);
                sideModels[faceIndex] = m;
            }
            else sideModels[faceIndex].SetActive(planes[faceIndex].isVisible);
        }
        if (redrawCall) myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
        return p;
    }
    
    // copy to FarmBlock.cs, HouseBlock.cs
    // modified to FoundationBlock.cs
    #region cubeStructures standart functions
    protected override void SetModel() { }
    override public void SetModelRotation(int r) { } 
    override public void SetVisibility(bool x) { }

    // side-models only
    override protected void ChangeRenderersView(bool setOnline)
    {
        var myRenderers = new List<Renderer>();
        Renderer[] rrs;
        foreach (var g in sideModels)
        {
            if (g != null)
            {
                rrs = g.GetComponentsInChildren<Renderer>();
                if (rrs != null && rrs.Length > 0) myRenderers.AddRange(rrs);
            }
        }
        if (myRenderers.Count == 0) return;
        if (setOnline) PoolMaster.SwitchMaterialsToOnline(myRenderers);
        else PoolMaster.SwitchMaterialsToOffline(myRenderers);
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
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockHigher());
            if (b != null && b.HavePlane(Block.DOWN_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.DOWN_FACE_INDEX);
                basement = p;
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockForward());
            if (b != null && b.HavePlane(Block.BACK_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.BACK_FACE_INDEX);
                basement = p;
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockRight());
            if (b != null && b.HavePlane(Block.LEFT_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.LEFT_FACE_INDEX);
                basement = p;
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockBack());
            if (b != null && b.HavePlane(Block.FWD_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.FWD_FACE_INDEX);
                basement = p;
                return true;
            }
            b = chunk.GetBlock(pos.OneBlockLeft());
            if (b != null && b.HavePlane(Block.RIGHT_FACE_INDEX))
            {
                var p = b.FORCED_GetPlane(Block.RIGHT_FACE_INDEX);
                basement = p;
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
            myBlock.myChunk.DeleteBlock(myBlock.pos, compensateResources);
        }
    }
    #endregion

    #region interface
    public void Delete(bool clearFromSurface, bool compensateResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, compensateResources, leaveRuins);
        GameMaster.realMaster.colonyController.storage.RemoveWarehouse(this);
        if (planes != null)
        {
            foreach (var p in planes) p.Value.Annihilate(compensateResources);
        }
        foreach (var g in sideModels)
        {
            if (g != null) Destroy(g);
        }
        Destroy(gameObject);
    }

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
            return planes.TryGetValue(faceIndex, out result);
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
    public byte GetAffectionMask() {
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
                if (!planes[faceIndex].isVisible)
                {
                    planes[faceIndex].SetVisibility(true);
                    if (faceIndex < 4) sideModels[faceIndex].SetActive(true);
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
                else planes[faceIndex].SetVisibility(false);
                myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
            }
            if (faceIndex < 4 && sideModels[faceIndex] != null) Destroy(sideModels[faceIndex]);
        }
    }

    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte vismask)
    {
        var data = new List<BlockpartVisualizeInfo>();
        var cpos = myBlock.pos;
        var chunk = myBlock.myChunk;

        byte realVisMask = (byte)(vismask & Block.CUBE_MASK);
        byte i = 0;
        for (; i < 4; i++)
        {
            if ((realVisMask & (1 << i)) != 0)
            {
                if (planes != null && planes.ContainsKey(i))
                {
                    sideModels[i].SetActive(true);
                }
                else CreatePlane(i, false);
            }
            else
            {
                sideModels[i]?.SetActive(false);
            }
        }

        i = Block.UP_FACE_INDEX;
        if ((realVisMask & (1 << i)) != 0)
        {
            if (planes != null && planes.ContainsKey(i))
            {
                var bvi = planes[i].GetVisualInfo(chunk, cpos);
                if (bvi != null) data.Add(bvi);
            }
            else
            {
                var p = CreatePlane(i, false).GetVisualInfo(chunk, cpos);
                if (p != null) data.Add(p);
            }
        }

        i = Block.DOWN_FACE_INDEX;
        if ((realVisMask & (1 << i)) != 0)
        {
            if (planes != null && planes.ContainsKey(i))
            {
                var bvi = planes[i].GetVisualInfo(chunk, cpos);
                if (bvi != null) data.Add(bvi);
            }
            else
            {
                var p = CreatePlane(i, false).GetVisualInfo(chunk, cpos);
                if (p != null) data.Add(p);
            }
        }
        if (data.Count > 0) return data; else return null;
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if (faceIndex < 4)
        {
            if ((Block.CUBE_MASK & (1 << faceIndex)) != 0)
            {
                if (planes != null && planes.ContainsKey(faceIndex)) sideModels[faceIndex].SetActive(true);
                else CreatePlane(faceIndex, false);
            }
            return null;
        }
        else
        {
            if ((Block.CUBE_MASK & (1 << faceIndex)) != 0)
            {
                if (planes != null && planes.ContainsKey(faceIndex)) return planes[faceIndex].GetVisualInfo(myBlock.myChunk, myBlock.pos);
                else return CreatePlane(faceIndex, false)?.GetVisualInfo(myBlock.myChunk, myBlock.pos);
            }
            else return null;
        }
    }


    public void Damage(float f, byte faceIndex)
    {
        ApplyDamage(f);
    }
    #endregion
    
}
