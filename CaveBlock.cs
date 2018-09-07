using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CaveBlockSerializer
{
    public SurfaceBlockSerializer surfaceBlockSerializer;
    public int upMaterial_ID;
    public bool haveSurface;
}

public class CaveBlock : SurfaceBlock
{
    MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west
    MeshRenderer ceilingRenderer;
    public bool haveSurface { get; private set; }

    public override void ReplaceMaterial(int newId)
    {
        material_id = newId;
        if (grassland != null)
        {
            grassland.Annihilation();
            CellsStatusUpdate();
        }
        foreach (MeshRenderer mr in faces)
        {
            if (mr == null) continue;
            else mr.sharedMaterial = ResourceType.GetMaterialById(newId, mr.GetComponent<MeshFilter>());
        }
        surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(newId, surfaceRenderer.GetComponent<MeshFilter>());
    }

    public void CaveBlockSet(Chunk f_chunk, ChunkPos f_chunkPos, int f_up_material_id, int f_down_material_id)
    {
        if (firstSet)
        {
            cellsStatus = 0; map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++) map[i, j] = false;
            }
            haveSurface = true;
            material_id = 0;
            surfaceObjects = new List<Structure>();
            artificialStructures = 0;
            isTransparent = false;
            visibilityMask = 0;
            firstSet = false;
            personalNumber = lastUsedNumber++;
        }
        myChunk = f_chunk;
        model = Object.Instantiate(PoolMaster.cavePref);
        Transform t = model.transform;
        // setting renderers
        {
            faces = new MeshRenderer[4];
            faces[0] = t.GetChild(0).GetComponent<MeshRenderer>();
            faces[1] = t.GetChild(1).GetComponent<MeshRenderer>();
            faces[2] = t.GetChild(2).GetComponent<MeshRenderer>();
            faces[3] = t.GetChild(3).GetComponent<MeshRenderer>();
            ceilingRenderer = t.GetChild(4).GetComponent<MeshRenderer>();
            surfaceRenderer = t.GetChild(5).GetComponent<MeshRenderer>();
            material_id = f_up_material_id;
            foreach (MeshRenderer mr in faces)
            {
                if (mr == null) continue;
                else mr.sharedMaterial = ResourceType.GetMaterialById(material_id, mr.GetComponent<MeshFilter>()); ;
            }
            ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(material_id, ceilingRenderer.GetComponent<MeshFilter>());
            if (f_down_material_id != -1)
            {
                haveSurface = true;
                surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(f_down_material_id, surfaceRenderer.GetComponent<MeshFilter>());
            }
            else
            {
                if (surfaceObjects.Count != 0) ClearSurface(false);
                haveSurface = false;
                surfaceRenderer.GetComponent<Collider>().enabled = false;
                surfaceRenderer.enabled = false;
            }
        }
        t.parent = f_chunk.transform;
        pos = f_chunkPos;
        t.localPosition = new Vector3(pos.x, pos.y, pos.z);
        t.localRotation = Quaternion.Euler(Vector3.zero);
        type = BlockType.Cave; isTransparent = false;
        model.name = "block " + pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
    }

    override public void SetRenderBitmask(byte x)
    {
        if (renderMask != x)
        {
            renderMask = x;
            if (visibilityMask == 0) return;
            for (int i = 0; i < 4; i++)
            {
                if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true;
                else faces[i].enabled = false;
            }
            if ((renderMask & 15) == 0)
            {
                ceilingRenderer.enabled = false;
                surfaceRenderer.enabled = false;
            }
            else
            {
                ceilingRenderer.enabled = true;
                if (haveSurface) surfaceRenderer.enabled = true;
            }
            if (structureBlock != null) structureBlock.SetRenderBitmask(x);
        }
    }

    override public void SetVisibilityMask(byte x)
    {
        byte prevVisibility = visibilityMask;
        if (visibilityMask == x) return;
        visibilityMask = x;
        if (haveSurface)
        {
            if (visibilityMask == 0)
            {
                int i = 0; bool listChanged = false;
                while (i < surfaceObjects.Count)
                {
                    surfaceObjects[i].SetVisibility(false);
                    i++;
                }
                surfaceRenderer.GetComponent<MeshCollider>().enabled = false;
                if (listChanged) CellsStatusUpdate();
            }
            else
            {
                if (prevVisibility == 0)
                {
                    int i = 0; bool listChanged = false;
                    while (i < surfaceObjects.Count)
                    {
                        surfaceObjects[i].SetVisibility(true);
                        i++;
                    }
                    if (listChanged) CellsStatusUpdate();
                    surfaceRenderer.GetComponent<MeshCollider>().enabled = true;
                }
            }
        }
        if (renderMask == 0) return;
        for (int i = 0; i < 4; i++)
        {
            if ((renderMask & ((int)Mathf.Pow(2, i)) & visibilityMask) != 0) faces[i].enabled = true;
            else faces[i].enabled = false;
        }
        if ((renderMask & 15) == 0)
        {
            ceilingRenderer.enabled = false;
            surfaceRenderer.enabled = false;
        }
        else
        {
            ceilingRenderer.enabled = true;
            if (haveSurface) surfaceRenderer.enabled = true;
        }

        if (structureBlock != null) structureBlock.SetVisibilityMask(x);
    }

    public void DestroySurface()
    {
        if (!haveSurface) return;
        haveSurface = false;
        if (grassland != null) grassland.Annihilation();
        if (surfaceObjects.Count != 0) ClearSurface(false);
        surfaceRenderer.GetComponent<Collider>().enabled = false;
        surfaceRenderer.enabled = false;
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 47);
    }
    public void RestoreSurface(int newMaterialID)
    {
        if (haveSurface) return;
        haveSurface = true;
        surfaceRenderer.enabled = true;
        surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(newMaterialID, surfaceRenderer.GetComponent<MeshFilter>());
        surfaceRenderer.GetComponent<Collider>().enabled = true;
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 15);

    }

    #region save-load system
    override public BlockSerializer Save()
    {
        BlockSerializer bs = GetBlockSerializer();
        CaveBlockSerializer cbs = new CaveBlockSerializer();
        cbs.upMaterial_ID = material_id;
        cbs.haveSurface = haveSurface;
        cbs.surfaceBlockSerializer = GetSurfaceBlockSerializer();
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        {
            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, cbs);
            bs.specificData = stream.ToArray();
        }
        return bs;
    }

    override public void Load(BlockSerializer bs)
    {
        LoadBlockData(bs);
        CaveBlockSerializer cbs = new CaveBlockSerializer();
        GameMaster.DeserializeByteArray<CaveBlockSerializer>(bs.specificData, ref cbs);
        LoadCaveBlockData(cbs);
    }

    protected void LoadCaveBlockData(CaveBlockSerializer cbs)
    {
        LoadSurfaceBlockData(cbs.surfaceBlockSerializer);
        ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(cbs.upMaterial_ID, ceilingRenderer.GetComponent<MeshFilter>());
        haveSurface = cbs.haveSurface;
        if (!haveSurface)
        {
            surfaceRenderer.enabled = false;
            surfaceRenderer.GetComponent<Collider>().enabled = false;
        }
    }
    #endregion
}
