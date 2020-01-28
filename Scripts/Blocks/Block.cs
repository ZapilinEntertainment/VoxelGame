using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Block {
    public const byte FWD_FACE_INDEX = 0, RIGHT_FACE_INDEX = 1, BACK_FACE_INDEX = 2, LEFT_FACE_INDEX = 3, UP_FACE_INDEX = 4, DOWN_FACE_INDEX = 5, SURFACE_FACE_INDEX = 6, CEILING_FACE_INDEX = 7;
    public const float QUAD_SIZE = 1, CEILING_THICKNESS = 0.1f;

    public readonly ChunkPos pos;
    public bool destroyed { get; private set; }
    public bool isBlockedByStructure { get { return mainStructure != null; } }
    public Chunk myChunk { get; private set; }
    
    private Structure mainStructure;
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

    public struct BlockMaterialsList
    {
        public int[] mlist;
        public const int MATERIALS_COUNT = 8;
        public BlockMaterialsList(int mat_fwd, int mat_right, int mat_back, int mat_left, int mat_up, int mat_down, int mat_surf, int mat_ceil)
        {
            mlist = new int[MATERIALS_COUNT];
            mlist[FWD_FACE_INDEX] = mat_fwd;
            mlist[RIGHT_FACE_INDEX] = mat_right;
            mlist[BACK_FACE_INDEX] = mat_back;
            mlist[LEFT_FACE_INDEX] = mat_left;
            mlist[UP_FACE_INDEX] = mat_up;
            mlist[DOWN_FACE_INDEX] = mat_down;
            mlist[SURFACE_FACE_INDEX] = mat_surf;
            mlist[CEILING_FACE_INDEX] = mat_ceil;
        }
        public int this[int i]
        {
            get
            {
                if (i >= 0 && i < MATERIALS_COUNT) return mlist[i];
                else return PoolMaster.NO_MATERIAL_ID;
            }
        }
        public byte GetExistenceMask()
        {
            byte m = 0; int nomat = PoolMaster.NO_MATERIAL_ID;
            if (mlist[0] != nomat) m += 1;
            if (mlist[1] != nomat) m += 2;
            if (mlist[2] != nomat) m += 4;
            if (mlist[3] != nomat) m += 8;
            if (mlist[4] != nomat) m += 16;
            if (mlist[5] != nomat) m += 32;
            if (mlist[6] != nomat) m += 64;
            if (mlist[7] != nomat) m += 128;
            return m;
        }
    }

    public Block (Chunk f_chunk, ChunkPos f_chunkPos, BlockMaterialsList bml, bool i_natural) : this(f_chunk, f_chunkPos)
    {
        extension = new BlockExtension(this, bml, i_natural);
    }
    public Block (Chunk f_chunk, ChunkPos f_chunkPos, int f_materialID, bool f_natural) : this(f_chunk, f_chunkPos)
    {        
        extension = new BlockExtension(this, f_materialID, f_natural);
    }
	public Block (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure) : this(f_chunk, f_chunkPos) {
        if (f_mainStructure != null)
        {
            mainStructure = f_mainStructure;
            AddBlockingMarker();
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
    public void SetMainStructure(Structure ms)
    {
        if (mainStructure != null) mainStructure.SectionDeleted(pos);
        mainStructure = ms;
        if (blockingMarker == null) AddBlockingMarker();
    }
    public void ResetMainStructure()
    {
        mainStructure = null;
        if (blockingMarker != null) MonoBehaviour.Destroy(blockingMarker);
    }    

    public int GetMaterialID() { if (extension == null) return PoolMaster.NO_MATERIAL_ID; else return extension.materialID; }

    public bool HavePlane(byte faceIndex) { if (extension == null) return false; else return extension.HavePlane(faceIndex); }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        if (extension == null) { result = null; return false; }
        else
        {
            return extension.TryGetPlane(faceIndex, out result);
        }
    }
    public bool IsFaceTransparent(byte faceIndex)
    {
        if (extension == null) return true;
        else return extension.IsFaceTransparent(faceIndex);
    }
    public bool InitializePlane(byte faceIndex)
    {
        if (extension != null) return extension.InitializePlane(faceIndex); else return false;
    }
    public void DeactivatePlane(byte faceIndex) {
        extension?.DeactivatePlane(faceIndex);
    }

    public float GetFossilsVolume() {
        if (extension == null) return 0f;
        else return extension.GetFossilsVolume();
    }
    public void TakeFossilsVolume(float f) {
        extension?.TakeFossilsVolume(f);
    }

    public void ChangeMaterial(int m_id, bool naturalAffect)
    {
        if (m_id == PoolMaster.NO_MATERIAL_ID) return;
        if (extension == null)
        {
            extension = new BlockExtension(this, m_id, naturalAffect);
        }
        else
        {
            extension.ChangeMaterial(m_id);
        }
    }
    public void RebuildBlock(BlockMaterialsList bml, bool i_natural, bool compensateStructures)
    {
        if (extension == null) extension = new BlockExtension(this, bml, i_natural);
        else extension.Rebuild(bml, i_natural, compensateStructures);
    }

    public List<BlockpartVisualizeInfo> GetVisualizeInfo(byte visualMask)
    {
        return extension?.GetVisualizeInfo(visualMask);
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        if (extension == null) return null;
        else
        {
            Plane p;
            if (extension.TryGetPlane(faceIndex, out p)) return p.GetVisualInfo(myChunk, pos);
            else return null;
        }
    }
    public byte GetVisualAffectionMask()
    {
        if (extension == null) return 0;
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
        extension?.Annihilate(false, compensateStructures);
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
