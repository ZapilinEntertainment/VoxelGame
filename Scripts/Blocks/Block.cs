using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Block {
    public const byte FWD_FACE_INDEX = 0, RIGHT_FACE_INDEX = 1, BACK_FACE_INDEX = 2, LEFT_FACE_INDEX = 3, UP_FACE_INDEX = 4, DOWN_FACE_INDEX = 5, SURFACE_FACE_INDEX = 6, CEILING_FACE_INDEX = 7;
    public const float QUAD_SIZE = 1, CEILING_THICKNESS = 0.1f;

    public readonly ChunkPos pos;
    public bool destroyed { get; private set; }
    public Chunk myChunk { get; private set; }
    
    public Structure mainStructure; private bool mainStructureBlockingMode; // распологается ли структура в этом блоке, или прост облокирует его?
    private GameObject blockingMarker;
    private BlockExtension extension;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Block b = (Block)obj;
        return pos == b.pos && destroyed == b.destroyed && myChunk == b.myChunk;
    }
    public override int GetHashCode()
    {
        return pos.x + pos.y * 3 + pos.z * 5;
    }

    public Block(Chunk f_chunk, ChunkPos f_chunkPos, BlockMaterialsList bml, float i_volume_pc, bool i_natural, bool redrawCall) : this(f_chunk, f_chunkPos)
    {
        extension = new BlockExtension(this, bml, i_volume_pc, i_natural, redrawCall);
    }
    public Block (Chunk f_chunk, ChunkPos f_chunkPos, BlockMaterialsList bml,bool i_natural, bool redrawCall) : this(f_chunk, f_chunkPos)
    {
        extension = new BlockExtension(this, bml, i_natural, redrawCall);
    }
    public Block (Chunk f_chunk, ChunkPos f_chunkPos, int f_materialID, bool f_natural) : this(f_chunk, f_chunkPos)
    {        
        extension = new BlockExtension(this, f_materialID, f_natural);
    }
	public Block (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure, bool blockingMode) : this(f_chunk, f_chunkPos) {
        mainStructure = f_mainStructure;
        if (blockingMode)
        {            
            mainStructureBlockingMode = true;
            AddBlockingMarker();
        }
        else
        {
            extension = new BlockExtension(mainStructure);
            mainStructureBlockingMode = false;
        }
    }
	public Block (Chunk f_chunk, ChunkPos f_chunkPos) {
        destroyed = false;
        myChunk = f_chunk;
        pos = f_chunkPos;
	}    

    private void AddBlockingMarker()
    {
        blockingMarker = new GameObject("mark");
        SpriteRenderer sr = blockingMarker.AddComponent<SpriteRenderer>();
        sr.sprite = PoolMaster.GetStarSprite(true);
        sr.sharedMaterial = PoolMaster.starsBillboardMaterial;
        blockingMarker.transform.localPosition = pos.ToWorldSpace();
    }
    public void SetMainStructure(Structure ms, bool blockingMode, bool forced, bool compensateStructures)
    {
        if (mainStructure == ms | ms == null) return;
        else
        {
            if (extension != null & !forced) return;
            if (mainStructure != null)
            {
                if (!forced) return;
                else
                {
                    if (mainStructureBlockingMode) mainStructure.SectionDeleted(pos);
                    else
                    {
                        extension?.Annihilate(compensateStructures);
                    }
                }
            }
            mainStructureBlockingMode = blockingMode;
            if (mainStructureBlockingMode)
            {
                extension?.Annihilate(compensateStructures);
                if (blockingMarker == null) AddBlockingMarker();
            }
            else
            {
                if (mainStructure.IsCube())
                {
                    if (extension == null) extension = new BlockExtension(mainStructure);
                    else extension.Rebuild(mainStructure, compensateStructures);
                }
                else
                {
                    extension?.Annihilate(compensateStructures);
                }
                if (blockingMarker != null)
                {
                    Object.Destroy(blockingMarker);
                    blockingMarker = null;
                }
            }
            mainStructure = ms;
            
        }
    }
    public void RemoveMainStructureLink(Structure ms)
    {
        if (ms == mainStructure && ms != null)
        {
            mainStructure = null;
            if (blockingMarker != null)
            {
                Object.Destroy(blockingMarker);
                blockingMarker = null;
            }
            if (extension == null) myChunk.DeleteBlock(pos, false);
        }
    }    

    public int GetMaterialID() { if (extension == null) return PoolMaster.NO_MATERIAL_ID; else return extension.materialID; }
    public bool ContainStructures()
    {
        if (mainStructure != null) return true;
        else
        {
            if (extension != null) return extension.ContainsStructures();
            else return false;
        }
    }
    public bool TryGetStructuresList(ref List<Structure> result)
    {
        if (result == null) result = new List<Structure>();
        if (extension != null) {
            extension.TryGetStructuresList(ref result);
            return true;
        }
        else return false;
        //ignore mainstructure
    }
    public bool IsCube()
    {
        if (extension == null) return false;
        else return extension.IsCube();
    }
    public bool IsSurface()
    {
        if (extension == null) return false;
        else return extension.IsSurface();
    }

    public bool HavePlane(byte faceIndex) { if (extension == null) return false; else return extension.HavePlane(faceIndex); }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (extension == null) { result = null; return false; }
        else
        {
            return extension.TryGetPlane(faceIndex, out result);
        }
    }
    public Plane GetPlane(byte faceIndex)
    {
        if (extension != null) return extension.GetPlane(faceIndex);
        else return null;
    }
    public bool IsFaceTransparent(byte faceIndex)
    {
        if (extension == null) return true;
        else return extension.IsFaceTransparent(faceIndex);
    }
    public bool InitializePlane(byte faceIndex)
    {
        if (extension != null) return extension.InitializePlane(faceIndex);
        else return false;
    }
    public void DeactivatePlane(byte faceIndex) {
        extension?.DeactivatePlane(faceIndex);
    }
    public void DeletePlane(byte faceIndex, bool compensateStructures, bool redrawCall)
    {
        extension?.DeletePlane(faceIndex, compensateStructures, redrawCall);
    }

    public float GetFossilsVolume() {
        if (extension == null) return 0f;
        else return extension.GetFossilsVolume();
    }
    public void TakeFossilsVolume(float f) {
        extension?.TakeFossilsVolume(f);
    }
    public float GetVolume() { return extension?.GetVolume() ?? 0f; }

    public void ChangeMaterial(int m_id, bool naturalAffect, bool redrawCall)
    {
        if (m_id == PoolMaster.NO_MATERIAL_ID) return;
        if (extension == null)
        {
            extension = new BlockExtension(this, m_id, naturalAffect);
        }
        else
        {
            extension.ChangeMaterial(m_id,redrawCall);
        }
    }
    public void RebuildBlock(BlockMaterialsList bml, bool i_natural, bool compensateStructures, bool redrawCall)
    {
        if (extension == null) extension = new BlockExtension(this, bml, i_natural, redrawCall);
        else extension.Rebuild(bml, i_natural, compensateStructures,redrawCall);
    }
    public void EnvironmentalStrike(byte i_faceIndex, Vector3 hitpoint, byte radius, float damage)
    {
        if (mainStructure != null) mainStructure.ApplyDamage(damage);
        else
        {
            Plane p = null;
            if (TryGetPlane(i_faceIndex, out p))
            {
                p.EnvironmentalStrike(hitpoint, radius, damage);
            }
            else extension?.Dig((int)damage, true, i_faceIndex);
        }
    }

    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask)
    {
        return extension?.GetVisualizeInfo(visualMask);
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if (extension == null) return null;
        else  return extension.GetPlane(faceIndex)?.GetVisualInfo(myChunk, pos);
    }
    public byte GetVisualAffectionMask()
    {
        if (extension == null) return 255;
        else return extension.GetVisualAffectionMask();
    }
    
    /// <summary>
    /// Don not use directly, use chunk.DeleteBlock() instead
    /// </summary>
    public void Annihilate(bool compensateStructures)
    {
        //#block annihilate
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        extension?.Annihilate(compensateStructures);
        if (mainStructure != null)
        {
            mainStructure.SectionDeleted(pos);
            mainStructure = null;
        }
    }

    #region save-load
    public void Save(System.IO.FileStream fs) 
    {
        SaveBlockData(fs);
    }
    private void SaveBlockData(System.IO.FileStream fs)
    {
        
    }
    #endregion
}
