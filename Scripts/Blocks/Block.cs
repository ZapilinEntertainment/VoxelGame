using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Block : MyObject {
    public const byte FWD_FACE_INDEX = 0, RIGHT_FACE_INDEX = 1, BACK_FACE_INDEX = 2, LEFT_FACE_INDEX = 3, UP_FACE_INDEX = 4, DOWN_FACE_INDEX = 5, SURFACE_FACE_INDEX = 6, CEILING_FACE_INDEX = 7;
    public const float QUAD_SIZE = 1, CEILING_THICKNESS = 0.1f;
    public const byte CUBE_MASK = (1 << FWD_FACE_INDEX) + (1 << RIGHT_FACE_INDEX) + (1 << BACK_FACE_INDEX)
        + (1 << LEFT_FACE_INDEX) + (1 << UP_FACE_INDEX) + (1 << DOWN_FACE_INDEX);

    public readonly ChunkPos pos;
    private VisibilityMode visibilityMode = VisibilityMode.DrawAll;
    public bool destroyed { get; private set; }
    public Chunk myChunk { get; private set; }
    
    public Structure mainStructure { get; private set; }
    private bool mainStructureIsABlocker; // распологается ли структура в этом блоке, или просто блокирует его?
    private GameObject blockingMarker;
    private BlockExtension extension;
    public bool haveExtension { get { return extension != null; } }
    public bool isInvincible {
        get {
            if (extension == null) return mainStructure?.indestructible ?? false;
            else return extension.isInvincible;
        }
    }   


    override protected bool IsEqualNoCheck(object x)
    {
        // проверки не нужны
        var b = (Block) x;
        return pos == b.pos && destroyed == b.destroyed && myChunk == b.myChunk;
    }
    public override int GetHashCode()
    {
        return pos.x + pos.y * 3 + pos.z * 5;
    }  

    #region save-load
    public void Save(System.IO.FileStream fs)
    {
        if (destroyed) return;
        fs.WriteByte(pos.x);
        fs.WriteByte(pos.y);
        fs.WriteByte(pos.z);
        if (extension != null) {
            fs.WriteByte(1);
            extension.Save(fs);
        }
        else fs.WriteByte(0);
    }
    public static bool TryToLoadBlock(System.IO.FileStream fs, Chunk c, ref Block b)
    {
        b = new Block(c, new ChunkPos(fs.ReadByte(), fs.ReadByte(), fs.ReadByte()));
        var csize = Chunk.chunkSize;
        if (b.pos.x >= csize  | b.pos.y >= csize | b.pos.z >= csize)
        {
            Debug.Log("block load error - wrong position");
            GameMaster.LoadingFail();
            return false;
        }
        if (fs.ReadByte() == 1)
        {
            b.extension = BlockExtension.Load(fs, b);
        }
        return true;
    }
    #endregion

    #region constructors
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
	public Block (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure, bool useMarker) : this(f_chunk, f_chunkPos) {
        mainStructure = f_mainStructure;
        mainStructureIsABlocker = true;
        if (useMarker) AddBlockingMarker();
    }
    public Block (Chunk i_chunk, ChunkPos i_pos, IPlanable i_mainStructure) : this(i_chunk, i_pos)
    {
        BuildBlock(i_mainStructure);
    }
	public Block (Chunk f_chunk, ChunkPos f_chunkPos) {
        destroyed = false;
        myChunk = f_chunk;
        pos = f_chunkPos;
	}

    private void BuildBlock(IPlanable i_mainStructure)
    {
        if (i_mainStructure.IsStructure())
        {
            mainStructure = (Structure)i_mainStructure;
            mainStructureIsABlocker = false;
        }
        else
        {
            extension = (BlockExtension)i_mainStructure;
        }
    }
    #endregion

    private IPlanable GetPlanesHost()
    {
        if (extension != null) return extension;
        else
        {
            if (mainStructure != null && !mainStructureIsABlocker) return (IPlanable)mainStructure;
            else return null;
        }
    }
    public BlockExtension GetExtension()
    {
        return extension;
    }

    private void AddBlockingMarker()
    {
        blockingMarker = new GameObject("mark");
        SpriteRenderer sr = blockingMarker.AddComponent<SpriteRenderer>();
        sr.sprite = PoolMaster.GetStarSprite(true);
        sr.sharedMaterial = PoolMaster.starsBillboardMaterial;
        blockingMarker.transform.localPosition = pos.ToWorldSpace();
    }
    public bool IsBlocker()
    {
        return (mainStructure != null && mainStructureIsABlocker);
    }
    public void ReplaceBlocker(Structure ms)
    {
        if (!mainStructureIsABlocker) return;
        else
        {
            if (mainStructure != null) mainStructure.SectionDeleted(pos);
            mainStructure = ms;
            if (blockingMarker == null) AddBlockingMarker();
        }
    }
    public void DropBlockerLink(Structure ms)
    {
        if (!mainStructureIsABlocker) return;
        else
        {
            if (ms == mainStructure && ms != null)
            {
                mainStructure = null;
                if (blockingMarker != null)
                {
                    Object.Destroy(blockingMarker);
                    blockingMarker = null;
                }
                if (extension == null) myChunk.DeleteBlock(pos, BlockAnnihilationOrder.BlockersClearOrder);
            }
        }
    }    

    public int GetMaterialID() { if (extension == null) return PoolMaster.NO_MATERIAL_ID; else return extension.materialID; }
    public bool ContainStructures()
    {
        return GetPlanesHost()?.ContainsStructures() ?? false;
    }
    public bool TryGetStructuresList(ref List<Structure> result)
    {
        var h = GetPlanesHost();
        if (h != null)
        {
            return h.TryGetStructuresList(ref result);
        }
        else
        {
            result = null;
            return false;
        }
    }
    public bool IsCube()
    {
        return GetPlanesHost()?.IsCube() ?? false;
    }
    public bool ContainSurface()
    {
        return GetPlanesHost()?.ContainSurface() ?? false;
    }

    public bool HavePlane(byte faceIndex) {
        return GetPlanesHost()?.HavePlane(faceIndex) ?? false;
    }
    public bool TryGetPlane(byte faceIndex, out Plane result)
    {
        var h = GetPlanesHost();
        if (h != null)  return h.TryGetPlane(faceIndex, out result);
        else
        {
            result = null;
            return false;
        }
    }
    public Plane FORCED_GetPlane(byte faceIndex)
    {
        return GetPlanesHost()?.FORCED_GetPlane(faceIndex);
    }
    public bool IsFaceTransparent(byte faceIndex)
    {
        return GetPlanesHost()?.IsFaceTransparent(faceIndex) ?? true;
    }
    public byte GetAffectionMask()
    {
        return GetPlanesHost()?.GetAffectionMask() ?? 0;
    }
    //returns false if transparent or wont be instantiated
    public bool InitializePlane(byte faceIndex)
    {
        return GetPlanesHost()?.InitializePlane(faceIndex) ?? false;
    }
    public void DeactivatePlane(byte faceIndex) {
        GetPlanesHost()?.DeactivatePlane(faceIndex);
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
        return GetPlanesHost()?.GetVisualizeInfo(visualMask);
    }
    public BlockpartVisualizeInfo GetFaceVisualData(byte faceIndex)
    {
        return GetPlanesHost()?.GetFaceVisualData(faceIndex);
    }

    public void SetVisibilityMode(VisibilityMode vm) { SetVisibilityMode(vm, false); }
    public void SetVisibilityMode(VisibilityMode vm, bool forcedRefresh)
    {
        if (visibilityMode != vm | forcedRefresh) {
            visibilityMode = vm;
            if (extension != null)
            {
                extension.SetVisibility(vm);
            }
            else
            {
                mainStructure?.SetVisibility(vm, forcedRefresh);
            }
        }
    }
    public VisibilityMode GetVisibilityMode() { return visibilityMode; }
   
    
    /// <summary>
    /// Don not use directly, use chunk.DeleteBlock() instead
    /// </summary>
    public void Annihilate(BlockAnnihilationOrder order)
    {
        //#block annihilate
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        extension?.Annihilate(order);
        if (mainStructure != null)
        {
            if (mainStructureIsABlocker)
            {
                if (!order.chunkClearing) mainStructure.SectionDeleted(pos);
            }
            else
            {
                mainStructure.Annihilate(StructureAnnihilationOrder.ChunkClearing);
            }
            mainStructure = null;
        }
    }
}
