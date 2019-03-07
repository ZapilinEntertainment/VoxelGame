using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType : byte {Shapeless, Cube, Surface, Cave}

public class Block {
    public const int SERIALIZER_LENGTH = 8;
    public const float QUAD_SIZE = 1;

    public bool destroyed { get; protected set; }
    public BlockType type { get; protected set; }
    public Worksite worksite { get; protected set; }
    public Chunk myChunk { get; protected set; }
    public ChunkPos pos { get; protected set; }
    public Structure mainStructure {get;protected set;}
	public bool blockedByStructure {get;protected  set;}
	public int material_id {get;protected  set;}
    protected List<GameObject> decorations;

	public virtual void ReplaceMaterial(int newId) {
		material_id = newId;
	}

	public void InitializeShapelessBlock (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure) {
        destroyed = false;
        type = BlockType.Shapeless;
        material_id = -1;
		myChunk = f_chunk; 
		pos = f_chunkPos; 
		mainStructure = f_mainStructure;
        if (mainStructure != null)
        {
            blockedByStructure = true;
            GameObject spriteHolder = new GameObject("mark");
            SpriteRenderer sr = spriteHolder.AddComponent<SpriteRenderer>();
            sr.sprite = PoolMaster.current.GetStarSprite();
            sr.sharedMaterial = PoolMaster.starsBillboardMaterial;
            spriteHolder.transform.localPosition = pos.ToWorldSpace();
            decorations = new List<GameObject>();
            decorations.Add(spriteHolder);
        }
        else blockedByStructure = false;
}
	public virtual void InitializeBlock (Chunk f_chunk, ChunkPos f_chunkPos, int newId) {
        destroyed = false;
        type = BlockType.Shapeless;
        material_id = -1;
		myChunk = f_chunk;
		pos = f_chunkPos;
		blockedByStructure = false;
	}

    virtual public List<BlockpartVisualizeInfo> GetVisualDataList(byte visibilityMask)
    {
        return null;
    }
    virtual public BlockpartVisualizeInfo GetVisualData(byte face)
    {
        return null;
    }

    public void SetWorksite(Worksite w)
    {
        if (worksite != null) worksite.StopWork();
        worksite = w;
    }
    public void ResetWorksite()
    {
        if (worksite != null)
        {
            worksite.StopWork();
            worksite = null;
        }
    }  

    virtual public void SetMainStructure(Structure ms)
    {
        if (mainStructure != null) mainStructure.SectionDeleted(pos);
        mainStructure = ms;
    }
    virtual public void ResetMainStructure()
    {
        mainStructure = null;
    }
    public void AddDecoration(GameObject g)
    {
        decorations.Add(g);
    }
    /// <summary>
    /// Don not use directly, use chunk.DeleteBlock() instead
    /// </summary>
    virtual public void Annihilate()
    {
        //#block annihilate
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        if (worksite != null) worksite.StopWork();
        if (mainStructure != null)
        {
            mainStructure.SectionDeleted(pos);
            mainStructure = null;
        }
        if (pos.y == Chunk.CHUNK_SIZE - 1) myChunk.DeleteRoof(pos.x, pos.z);
        if (decorations != null && decorations.Count > 0)
        {
            while (decorations.Count > 0)
            {
                MonoBehaviour.Destroy(decorations[0]);
            }
        }
    }

    #region save-load
    virtual public void Save(System.IO.FileStream fs)
    {
        SaveBlockData(fs);
    }
    protected void SaveBlockData(System.IO.FileStream fs)
    {
        fs.WriteByte((byte)type);
        fs.WriteByte(pos.x);
        fs.WriteByte(pos.y);
        fs.WriteByte(pos.z);
        fs.Write(System.BitConverter.GetBytes(material_id), 0, 4);
    }
    #endregion
}
