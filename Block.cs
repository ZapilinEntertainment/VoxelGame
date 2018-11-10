using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {Shapeless, Cube, Surface, Cave}

public class Block : MonoBehaviour {
    public const float QUAD_SIZE = 1;

    public BlockType type { get; protected set; }
    public Worksite worksite {get;protected set;}
	public Chunk myChunk {get; protected  set;}
	public bool isTransparent {get;protected  set;} // <- замени на transparent map
	public ChunkPos pos {get; protected  set;}
    public Structure mainStructure; // <---- ЗАМЕНИТЬ
	public bool blockedByStructure {get;protected  set;}
	public int material_id {get;protected  set;}
	public byte visibilityMask { get; protected set; } // видимость относительно других блоков
	protected byte renderMask = 0; // видимость относительно камеры
	public bool indestructible {get; protected set;}
    protected bool destroyed = false;
    public byte illumination { get; protected set; }

	public virtual void ReplaceMaterial(int newId) {
		material_id = newId;
	}

	public void InitializeShapelessBlock (Chunk f_chunk, ChunkPos f_chunkPos, Structure f_mainStructure) {
        type = BlockType.Shapeless;
        isTransparent = true;
        material_id = 0;
        illumination = 255;
		myChunk = f_chunk; 
		transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		mainStructure = f_mainStructure;
		if (mainStructure != null) blockedByStructure = true; else blockedByStructure = false;
		
		name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
}
	public virtual void InitializeBlock (Chunk f_chunk, ChunkPos f_chunkPos, int newId) {
        type = BlockType.Shapeless;
		isTransparent = true;
        material_id = 0;
        illumination = 255;
		myChunk = f_chunk;
        transform.parent = f_chunk.transform;
		pos = f_chunkPos;
        transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = newId;
		blockedByStructure = false;
		name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
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
	virtual public void ChangeVisibilityMask (byte index, bool val) {
        byte vm = visibilityMask;
        //длинно, зато ясно
		switch (index)
        {
            case 0:// forward
                if (val == true)
                {
                    if ((visibilityMask & 1) == 0) vm += 1;
                }
                else
                {
                    if ((visibilityMask & 1) == 1) vm -= 1;
                }
                break;
            case 1:// right
                if (val == true)
                {
                    if ((visibilityMask & 2) == 0) vm += 2;
                }
                else
                {
                    if ((visibilityMask & 2) == 2) vm -= 2;
                }
                break;
            case 2:// back
                if (val == true)
                {
                    if ((visibilityMask & 4) == 0) vm += 4;
                }
                else
                {
                    if ((visibilityMask & 4) == 4) vm -= 4;
                }
                break;
            case 3:// left
                if (val == true)
                {
                    if ((visibilityMask & 8) == 0) vm += 8;
                }
                else
                {
                    if ((visibilityMask & 8) == 8) vm -= 8;
                }
                break;
            case 4:// up
                if (val == true)
                {
                    if ((visibilityMask & 16) == 0) vm += 16;
                }
                else
                {
                    if ((visibilityMask & 16) == 16) vm -= 16;
                }
                break;
            case 5:// down
                if (val == true)
                {
                    if ((visibilityMask & 32) == 0) vm += 32;
                }
                else
                {
                    if ((visibilityMask & 32) == 32) vm -= 32;
                }
                break;
        }
		if (vm != visibilityMask) SetVisibilityMask(vm);
	}
    virtual public void SetIllumination()
    {
       illumination = myChunk.lightMap[pos.x, pos.y,pos.z];
       MeshRenderer[] rrs = gameObject.GetComponentsInChildren<MeshRenderer>();
       foreach (MeshRenderer mr in rrs) mr.sharedMaterial = ResourceType.GetMaterialById(material_id, mr.GetComponent<MeshFilter>(), illumination);
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

    #region save-load
    virtual public BlockSerializer Save() {
		return GetBlockSerializer();
	} 

	virtual public void Load(BlockSerializer bs) {
		LoadBlockData(bs);
	}

	protected void LoadBlockData(BlockSerializer bs) {
		isTransparent = bs.isTransparent;
	} 

    protected BlockSerializer GetBlockSerializer() {
		BlockSerializer bs = new BlockSerializer();
		bs.type = type;
		bs.isTransparent = isTransparent;
		bs.pos = pos;
		bs.material_id = material_id;
		return bs;
	}
    #endregion

    virtual public void Annihilate()
    {
        //#block annihilate
        if (destroyed) return;
        else destroyed = true;
        if (worksite != null) worksite.StopWork();
        if (mainStructure != null) mainStructure.SectionDeleted(pos);
        Destroy(gameObject);
    }
}

[System.Serializable]
public class BlockSerializer {
	public BlockType type;
	public bool isTransparent;
	public ChunkPos pos;
	public int material_id, personalNumber;
	public byte[] specificData;
}
