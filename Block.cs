using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {Shapeless, Cube, Surface, Cave}

public class Block {
    public const float QUAD_SIZE = 1;
    public BlockType type { get; protected set; }
    public int personalNumber { get; protected set; }
    public static int lastUsedNumber = 0;

    public Worksite worksite {get;protected set;}
    public GameObject model { get; protected set; }
	public Chunk myChunk {get; protected  set;}
	public bool isTransparent {get;protected  set;} // <- замени на transparent map
	public ChunkPos pos {get; protected  set;}
	public Structure mainStructure{get;protected set;}
	public bool blockedByStructure {get;protected  set;}
	public int material_id {get;protected  set;}
	public byte visibilityMask; // видимость относительно других блоков
	public byte renderMask = 0; // видимость относительно камеры
	public bool indestructible {get; protected set;}
    protected bool firstSet = true;

	public virtual void ReplaceMaterial(int newId) {
		material_id = newId;
	}

	public void ShapelessBlockSet (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure) {
        if (firstSet)
        {
            type = BlockType.Shapeless;
            personalNumber = lastUsedNumber++;
            firstSet = false;
        }
        isTransparent = true; material_id = 0;
		myChunk = f_chunk; 
		model.transform.parent = f_chunk.transform;
		pos = f_chunkPos; model.transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		model.transform.localRotation = Quaternion.Euler(Vector3.zero);
		mainStructure = f_mainStructure;
		if (mainStructure != null) blockedByStructure = true; else blockedByStructure = false;
		
		model.gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
}
	public virtual void BlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int newId) {
        if (firstSet)
        {
            type = BlockType.Shapeless;
            personalNumber = lastUsedNumber++;
            firstSet = false;
        }
		isTransparent = true; material_id = 0;
		myChunk = f_chunk;
        model.transform.parent = f_chunk.transform;
		pos = f_chunkPos;
        model.transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		model.transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = newId;
		blockedByStructure = false;
		model.gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
	}

	public void MakeIndestructible(bool x) {
		indestructible = x;
	}

	virtual public void SetRenderBitmask(byte x) {
		renderMask = x;
	}
	virtual public void SetVisibilityMask (byte x) {
		visibilityMask = x;
	}
	virtual public void ChangeVisibilityMask (byte index, bool value) { 
		byte vm = visibilityMask;
		byte val = (byte)( Mathf.Pow(2, index) );
		byte nmask= (byte)~val;
		vm &= nmask; // любое значение в позиции index меняется на 0
		if (value) { vm +=  val;}
		if (vm != visibilityMask) SetVisibilityMask((byte)vm);
	}

    public void SetWorksite(Worksite w)
    {
        Worksite oldw = worksite;
        worksite = w;
        if (oldw != null) oldw.StopWork();        
    }
    public void ResetWorksite()
    {
        if (worksite != null)
        {
            Worksite w = worksite;
            worksite = null;
            w.StopWork();            
        }
    }

    #region save-load
    virtual public BlockSerializer Save() {
		return GetBlockSerializer();
	} 

	virtual public void Load(BlockSerializer bs) {
		LoadBlockData(bs);
	}

	protected void LoadBlockData(BlockSerializer bs) {
		isTransparent = bs.isTransparent;
        personalNumber = bs.personalNumber;
	}

    public void Annihilate()
    {
        if (model != null) Object.Destroy(model);
        if (mainStructure == null) return;
        MultiblockStructure ms = mainStructure.gameObject.GetComponent<MultiblockStructure>();
        if (ms != null) ms.PartCollapse(pos);
    }

	protected BlockSerializer GetBlockSerializer() {
		BlockSerializer bs = new BlockSerializer();
		bs.type = type;
		bs.isTransparent = isTransparent;
		bs.pos = pos;
		bs.material_id = material_id;
        bs.personalNumber = personalNumber;
		return bs;
	}
    #endregion
}

[System.Serializable]
public class BlockSerializer {
	public BlockType type;
	public bool isTransparent;
	public ChunkPos pos;
	public int material_id, personalNumber;
	public byte[] specificData;
}
