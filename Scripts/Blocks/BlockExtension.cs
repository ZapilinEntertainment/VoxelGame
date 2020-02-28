using System.Collections.Generic;
using UnityEngine;
public sealed class BlockExtension : IPlanable
{
    public readonly Block myBlock;
    private bool isNatural;
    public int materialID { get; private set; }
    private float fossilsVolume, volume;
    private byte existingPlanesMask;    //не может быть нулем, иначе extension не нужен
    private Dictionary<byte, Plane> planes;
    public const float MAX_VOLUME = 4096;
    //private const byte SURFACE_MASK = (1 << Block.SURFACE_FACE_INDEX) + ( 1 << Block.UP_FACE_INDEX);

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        BlockExtension be = (BlockExtension)obj;
        return myBlock == be.myBlock && materialID == be.materialID && existingPlanesMask == be.existingPlanesMask;
    }
    public override int GetHashCode()
    {
        return myBlock.GetHashCode() + materialID + existingPlanesMask;
    }

    public BlockExtension(Block i_myBlock,  int i_materialID, bool i_natural)
    {
        //#mainConstructorPart
        myBlock = i_myBlock;
        materialID = i_materialID;
        isNatural = i_natural;
        fossilsVolume = isNatural ? MAX_VOLUME : 0f;
        volume = MAX_VOLUME;
        //
        existingPlanesMask = 
            (1 << Block.FWD_FACE_INDEX) + (1 << Block.RIGHT_FACE_INDEX) + (1 << Block.BACK_FACE_INDEX) + 
            (1 << Block.LEFT_FACE_INDEX) + (1 << Block.UP_FACE_INDEX) + ( 1 << Block.DOWN_FACE_INDEX);
    }   
    public BlockExtension(Block i_myBlock, BlockMaterialsList bml, bool i_natural, bool redrawCall) : this(i_myBlock,bml.mainMaterial, i_natural)  {
        int nomat = PoolMaster.NO_MATERIAL_ID;
        int mat = bml[Block.FWD_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.FWD_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.FWD_FACE_INDEX, mat, false);
        }
        mat = bml[Block.RIGHT_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.RIGHT_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.RIGHT_FACE_INDEX, mat, false);
        }
        mat = bml[Block.BACK_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.BACK_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.BACK_FACE_INDEX, mat, false);
        }
        mat = bml[Block.LEFT_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.LEFT_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.LEFT_FACE_INDEX, mat, false);
        }
        mat = bml[Block.UP_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.UP_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.UP_FACE_INDEX, mat, false);
        }
        mat = bml[Block.DOWN_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.DOWN_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.DOWN_FACE_INDEX, mat, false);
        }
        mat = bml[Block.SURFACE_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.SURFACE_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.SURFACE_FACE_INDEX, mat, false);
        }
        mat = bml[Block.CEILING_FACE_INDEX];
        if (mat != nomat)
        {
            existingPlanesMask += 1 << Block.CEILING_FACE_INDEX;
            if (materialID != mat) CreatePlane(Block.CEILING_FACE_INDEX, mat, false);
        }
        if (redrawCall) myBlock.myChunk.RefreshBlockVisualising(myBlock);
    }
    public BlockExtension(Block i_myBlock, BlockMaterialsList bml, float i_volume_pc, bool i_natural, bool redrawCall) : this(i_myBlock, bml, i_natural, redrawCall)
    {
        volume = MAX_VOLUME * i_volume_pc;
        fossilsVolume = isNatural ? volume : 0f;
    }

    public void ChangeMaterial(int i_materialID, bool redrawCall)
    {
        if (materialID != i_materialID)
        {
            materialID = i_materialID;
            if (planes != null)
            {
                foreach (var fp in planes)
                {
                    fp.Value.ChangeMaterial(materialID, redrawCall);
                }
            }
            myBlock.myChunk.RefreshBlockVisualising(myBlock);
        }
    }
    public void Rebuild(BlockMaterialsList bml, bool i_natural, bool compensateStructures, bool redrawCall)
    {
        byte newmask = bml.GetExistenceMask();
        byte x;
        for (byte i =0; i< 8; i++)
        {
            x = (byte)(1 << i);
            if ( (newmask & x) == 0 )
            { // удаление
                if ( (existingPlanesMask & x) != 0)
                {
                    if (planes.ContainsKey(i)) planes[i].Annihilate(compensateStructures);
                }
            }
            else
            { // создание или изменение
                if ((existingPlanesMask & x) != 0 && planes != null && planes.ContainsKey(i)) planes[i].ChangeMaterial(bml[i],redrawCall);
                else CreatePlane(i, bml[i],redrawCall);
            }
        }
        myBlock.myChunk.RefreshBlockVisualising(myBlock);
    }

    #region interface
    public bool IsStructure() { return false; }
    public bool IsFaceTransparent(byte faceIndex)
    {
        if ((existingPlanesMask & (1 << faceIndex)) != 0)
        {
            if (planes == null || !planes.ContainsKey(faceIndex)) return false; // дефолтные квады
            else
            {
                if (planes[faceIndex].isQuad) return false;
                else return MeshMaster.IsMeshTransparent(planes[faceIndex].meshType);
            }
        }
        else return true;
    }
    public bool HavePlane(byte faceIndex)
    {
        return (existingPlanesMask & (1 << faceIndex)) != 0;
    }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (!HavePlane(faceIndex)) { result = null; return false; }
        else
        {
            if (planes != null && planes.ContainsKey(faceIndex))
            {
                result = planes[faceIndex];
                return true;
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
        if ((existingPlanesMask & (1 << faceIndex)) == 0) return null;
        else
        {
            if (planes != null && planes.ContainsKey(faceIndex)) return planes[faceIndex];
            else return CreatePlane(faceIndex, true);
        }
    }
    public Block GetBlock() { return myBlock; }
    public bool IsCube()
    {
        byte fullmask = Block.CUBE_MASK;
        return ((fullmask & existingPlanesMask) == fullmask);
    }
    public bool ContainSurface()
    {
        if (planes == null) return false;
        else
        {
            if (planes.ContainsKey(Block.UP_FACE_INDEX))
            {
                return planes[Block.UP_FACE_INDEX].isQuad;
            }
            else
            {
                if (planes.ContainsKey(Block.SURFACE_FACE_INDEX)) return planes[Block.SURFACE_FACE_INDEX].isQuad;
                else return false;
            }
        }
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

    public byte GetAffectionMask()
    {
        if (IsCube()) return Block.CUBE_MASK;
        else
        {
            byte mask = 0, i = (1 << Block.FWD_FACE_INDEX);
            if ((existingPlanesMask & i) != 0)
            {
                if (!IsFaceTransparent(Block.FWD_FACE_INDEX)) mask += i;
            }
            i = (1 << Block.RIGHT_FACE_INDEX);
            if ((existingPlanesMask & i) != 0)
            {
                if (!IsFaceTransparent(Block.RIGHT_FACE_INDEX)) mask += i;
            }
            i = (1 << Block.BACK_FACE_INDEX);
            if ((existingPlanesMask & i) != 0)
            {
                if (!IsFaceTransparent(Block.BACK_FACE_INDEX)) mask += i;
            }
            i = (1 << Block.LEFT_FACE_INDEX);
            if ((existingPlanesMask & i) != 0)
            {
                if (!IsFaceTransparent(Block.LEFT_FACE_INDEX)) mask += i;
            }
            i = (1 << Block.UP_FACE_INDEX);
            if ((existingPlanesMask & i) != 0)
            {
                if (!IsFaceTransparent(Block.UP_FACE_INDEX)) mask += i;
            }
            i = (1 << Block.DOWN_FACE_INDEX);
            if ((existingPlanesMask & i) != 0)
            {
                if (!IsFaceTransparent(Block.DOWN_FACE_INDEX)) mask += i;
            }
            return mask;
        }
    }
    //returns false if transparent or wont be instantiated
    public bool InitializePlane(byte faceIndex)
    {
        if ((existingPlanesMask & (1 << faceIndex)) == 0) return false;
        else
        {
            if (planes != null && planes.ContainsKey(faceIndex))
            {
                if (!planes[faceIndex].isVisible) planes[faceIndex].SetVisibility(true);
                return MeshMaster.IsMeshTransparent(planes[faceIndex].meshType);
            }
            else
            {
                if (planes == null) planes = new Dictionary<byte, Plane>();
                var p =CreatePlane(faceIndex, materialID, false);
                return MeshMaster.IsMeshTransparent(p.meshType);
            }
        }
    }
    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte vismask)
    {
        if (existingPlanesMask == 0) return null;
        else
        {
            var data = new List<BlockpartVisualizeInfo>();
            var cpos = myBlock.pos;
            var chunk = myBlock.myChunk;

            byte realVisMask = (byte)(vismask & existingPlanesMask);
            if (realVisMask != 0)
            {
                for (byte i = 0; i < 8; i++)
                {
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
                            if (p != null) data.Add(p); else Debug.LogError("plane not created correctly");
                        }
                    }
                }
                return data;
            }
            else return null;
        }
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if ((existingPlanesMask & (1 << faceIndex)) != 0)
        {
            if (planes != null && planes.ContainsKey(faceIndex)) return planes[faceIndex].GetVisualInfo(myBlock.myChunk, myBlock.pos);
            else return CreatePlane(faceIndex, false)?.GetVisualInfo(myBlock.myChunk, myBlock.pos);
        }
        else return null;
    }
    public void DeactivatePlane(byte faceIndex)
    {
        if (planes != null)
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
        }
    }

    public void Damage(float f, byte faceIndex)
    {
        Dig((int)f, true, faceIndex);
    }
    #endregion
    public Plane CreatePlane(byte faceIndex, bool redrawCall) { return CreatePlane(faceIndex, materialID, redrawCall); }
    public Plane CreatePlane(byte faceIndex, int i_materialID, bool redrawCall)
    {
        Plane p = null;
        if (planes == null) planes = new Dictionary<byte, Plane>();
        else
        {
            if (planes.ContainsKey(faceIndex))
            {
                p = planes[faceIndex];
                p.ChangeMaterial(i_materialID, redrawCall);
                return p;
            }
        }
        var pos = myBlock.pos;
        if (faceIndex == Block.UP_FACE_INDEX && pos.y == Chunk.chunkSize - 1)
            p = MeshMaster.GetRooftop(this, Random.value < 0.14f, !isNatural);
        else p = new Plane(this, Plane.defaultMeshType, i_materialID, faceIndex, 0);
        planes.Add(faceIndex, p);
        if (redrawCall) myBlock.myChunk.RefreshBlockVisualising(myBlock, faceIndex);
        return p;
    }

    public float Dig(int d_volume, bool openPit, byte faceIndex)
    {
        if (d_volume > volume) d_volume = (int)volume;
        volume -= d_volume;
        if (fossilsVolume > 0f)
        {
            GameMaster.geologyModule.CalculateOutput(d_volume, this, GameMaster.realMaster.colonyController.storage);
            fossilsVolume -= d_volume;
        }
        else GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.GetResourceTypeById(materialID), d_volume);

        if (volume <= 1f)
        {
            var p = planes[faceIndex];
            if (p.haveWorksite)
            {
                GameMaster.realMaster.colonyController.RemoveWorksite(p);
                p.SetWorksitePresence(false);
            }
            myBlock.myChunk.DeleteBlock(myBlock.pos, true);
            return 0f;
        }
        else
        {
            if (openPit) {
                planes[faceIndex].VolumeChanges(volume / MAX_VOLUME);
            }
        }
        return d_volume;
    }
    public float PourIn(int d_volume, byte faceIndex)
    {
        if (volume + d_volume > MAX_VOLUME) d_volume = Mathf.CeilToInt(MAX_VOLUME - volume);
        if (d_volume > 0)
        {
            volume += d_volume;
            planes[faceIndex].VolumeChanges(volume / MAX_VOLUME);
            return d_volume;
        }
        else return 0f;
    }
    public float GetFossilsVolume() { return fossilsVolume; }
    public void TakeFossilsVolume(float f) { fossilsVolume -= f; if (fossilsVolume < 0f) fossilsVolume = 0f; }
    public float GetVolume() { if (materialID != PoolMaster.NO_MATERIAL_ID) return volume; else return 0f; }
    public float GetVolumePercent() { return volume / (float)MAX_VOLUME; }



    public void Delete(bool clearFromSurface, bool compensateResources, bool leaveRuins) { Annihilate(compensateResources); }
    /// <summary>
    /// Do not use directly, use chunk.DeleteBlock
    /// </summary>
    public void Annihilate(bool compensateStructures)
    {
        if (planes == null) foreach (var px in planes) px.Value.Annihilate(compensateStructures);
    }
}
