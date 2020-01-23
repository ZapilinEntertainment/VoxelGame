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

    private byte allowedPlanesMask = 0;
    private Structure mainStructure;
    private GameObject blockingMarker;
    private BlockExtension extension;

    public struct BlockMaterialsList
    {
        public int[] mlist;
        private const int MATERIALS_COUNT = 8;
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
    }

    public Block (Chunk f_chunk, ChunkPos f_chunkPos, BlockMaterialsList bml, bool i_natural) : this(f_chunk, f_chunkPos)
    {
        allowedPlanesMask = 0;
        int nomat = PoolMaster.NO_MATERIAL_ID;
        var pot = GameConstants.powersOfTwo;
        bool haveExtension = extension != null;
        if (bml.mlist[FWD_FACE_INDEX] != nomat)
        {
            allowedPlanesMask += pot[FWD_FACE_INDEX];
            if (!haveExtension) extension = new BlockExtension()
        }
    }
    public Block (Chunk f_chunk, ChunkPos f_chunkPos, int f_materialID, bool f_natural) : this(f_chunk, f_chunkPos)
    {
        allowedPlanesMask = FWD_FACE_INDEX + RIGHT_FACE_INDEX + BACK_FACE_INDEX + LEFT_FACE_INDEX + UP_FACE_INDEX + DOWN_FACE_INDEX;
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

    public int GetMaterialID() { return PoolMaster.NO_MATERIAL_ID; }
    public bool HavePlane(byte faceIndex) { return false; }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        result = null; return false;
    }
    public bool IsFaceTransparent(byte faceIndex)
    {
        return true;
    }
    public bool InitializePlane(byte faceIndex)
    {
        return false;
    }
    public void DeactivatePlane(byte faceIndex) { }
    public float GetFossilsVolume() { return 0f; }
    public void TakeFossilsVolume(float f) { }
    
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
    /// <summary>
    /// Don not use directly, use chunk.DeleteBlock() instead
    /// </summary>
    public void Annihilate()
    {
        //#block annihilate
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
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
