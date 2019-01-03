using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CaveBlock : SurfaceBlock
{
    MeshRenderer[] faces; // 0 - north, 1 - east, 2 - south, 3 - west
    MeshRenderer ceilingRenderer;
    public bool haveSurface { get; private set; }
    public int ceilingMaterial { get; private set; }

    public override void ReplaceMaterial(int newId)
    {
        if (newId == material_id) return;
        material_id = newId;
        ceilingMaterial = newId;
        if (grassland != null & material_id != ResourceType.DIRT_ID & material_id != ResourceType.FERTILE_SOIL_ID)
        {
            grassland.Annihilation();
        }
        foreach (MeshRenderer mr in faces)
        {
            if (mr == null) continue;
            else mr.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, mr.GetComponent<MeshFilter>(), illumination);
        }
        ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, ceilingRenderer.GetComponent<MeshFilter>(), illumination);
        if (haveSurface) surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(material_id, surfaceRenderer.GetComponent<MeshFilter>(), illumination);
    }
    public void ReplaceSurfaceMaterial(int i_id)
    {
        material_id = i_id;
        if (haveSurface) surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(material_id, surfaceRenderer.GetComponent<MeshFilter>(), illumination);
        if (grassland != null & material_id != ResourceType.DIRT_ID & material_id != ResourceType.FERTILE_SOIL_ID)
        {
            grassland.Annihilation();
        }
    }
    public void ReplaceCeilingMaterial(int i_id)
    {
        ceilingMaterial = i_id;
        foreach (MeshRenderer mr in faces)
        {
            if (mr == null) continue;
            else mr.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, mr.GetComponent<MeshFilter>(), illumination);
        }
        ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, ceilingRenderer.GetComponent<MeshFilter>(), illumination);
    }

    public void InitializeCaveBlock(Chunk f_chunk, ChunkPos f_chunkPos, int f_up_material_id, int f_down_material_id)
    {
        cellsStatus = 0; map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++) map[i, j] = false;
        }
        material_id = f_down_material_id;
        ceilingMaterial = f_up_material_id;
        surfaceObjects = new List<Structure>();
        artificialStructures = 0;
        visibilityMask = 0;
        illumination = 255;

        myChunk = f_chunk;
        Transform t = transform;
        t.parent = f_chunk.transform;
        pos = f_chunkPos;
        t.localPosition = new Vector3(pos.x, pos.y, pos.z);
        t.localRotation = Quaternion.Euler(Vector3.zero);
        type = BlockType.Cave;
        name = "block " + pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();

        GameObject model = Instantiate(PoolMaster.cavePref, Vector3.zero, Quaternion.identity, transform);
        t = model.transform;
        t.transform.parent = transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.Euler(Vector3.zero);
        // setting renderers
        {
            faces = new MeshRenderer[4];
            faces[0] = t.GetChild(0).GetComponent<MeshRenderer>();
            faces[1] = t.GetChild(1).GetComponent<MeshRenderer>();
            faces[2] = t.GetChild(2).GetComponent<MeshRenderer>();
            faces[3] = t.GetChild(3).GetComponent<MeshRenderer>();
            ceilingRenderer = t.GetChild(4).GetComponent<MeshRenderer>();
            surfaceRenderer = t.GetChild(5).GetComponent<MeshRenderer>();
            foreach (MeshRenderer mr in faces)
            {
                mr.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, mr.GetComponent<MeshFilter>(), illumination); ;
            }
            ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, ceilingRenderer.GetComponent<MeshFilter>(), illumination);
            if (material_id != -1)
            {
                haveSurface = true;
                surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(material_id, surfaceRenderer.GetComponent<MeshFilter>(), illumination);
                surfaceRenderer.gameObject.SetActive(true);
            }
            else
            {
                if (surfaceObjects.Count != 0) ClearSurface(true);
                haveSurface = false;
                surfaceRenderer.gameObject.SetActive(false);
            }
        }

    }

    override public void SetRenderBitmask(byte x)
    {
        if (renderMask == x ) return;
        renderMask = x;
        if (visibilityMask == 0) return;
        byte[] arr = new byte[] { 1, 2, 4, 8 };
        // #visibility check
        bool allFacesDisabled = true;
        bool e = false;
        for (int i = 0; i < 4; i++)
        {
            e = ((visibilityMask & arr[i]) != 0);
            faces[i].enabled = e;
            if (e == true) allFacesDisabled = false;
        }
        e = ((visibilityMask & renderMask & 32) == 32) & haveSurface;
        if (e == false & allFacesDisabled)
        {
            surfaceRenderer.enabled = false;
            surfaceRenderer.GetComponent<Collider>().enabled = false;
        }
        else
        {
            surfaceRenderer.enabled = true;
            surfaceRenderer.GetComponent<Collider>().enabled = true;
        }

        e = ((visibilityMask & renderMask & 16) == 16);
        if (e == false & allFacesDisabled) ceilingRenderer.enabled = false; else ceilingRenderer.enabled = true;
        //eo visibility check
    }
    override public void SetVisibilityMask(byte x)
    {
        if (visibilityMask == x) return;
        byte prevVisibility = visibilityMask;
        visibilityMask = x;

        if (visibilityMask == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (faces[i] != null) faces[i].gameObject.SetActive(false);
            }
            if (ceilingRenderer != null) ceilingRenderer.gameObject.SetActive(false);
            if (surfaceRenderer != null)
            {
                // отключать surfaceRenderer нельзя, так как на нем могут быть структуры
                if (cellsStatus != 0)
                {
                    foreach (Structure s in surfaceObjects)
                    {
                        if (s != null) s.SetVisibility(false);
                    }
                }
                surfaceRenderer.enabled = false;
                surfaceRenderer.GetComponent<MeshCollider>().enabled = false;
            }
        }
        else
        {
            byte[] arr = new byte[] { 1, 2, 4, 8 };
            if (prevVisibility == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (faces[i] != null)
                    {
                        faces[i].gameObject.SetActive(true);
                        faces[i].enabled = ((visibilityMask & renderMask & arr[i]) != 0);
                    }
                }
                if (ceilingRenderer != null)
                {
                    ceilingRenderer.gameObject.SetActive(true);
                    ceilingRenderer.enabled = ((visibilityMask & renderMask & 16) != 0);
                }
                if ( haveSurface & surfaceRenderer != null)
                {
                    if (cellsStatus != 0)
                    {
                        foreach (Structure s in surfaceObjects)
                        {
                            if (s != null) s.SetVisibility(true);
                        }
                    }
                    surfaceRenderer.enabled = ((visibilityMask & renderMask & 32) != 0);
                    surfaceRenderer.GetComponent<MeshCollider>().enabled = true;
                }
                SetIllumination();
            }
            else
            {
                // #visibility check
                bool allFacesDisabled = true;
                bool e = false;
                for (int i = 0; i < 4; i++)
                {
                    e = ((visibilityMask & arr[i]) != 0);
                    faces[i].enabled = e;
                    if (e == true) allFacesDisabled = false;
                }
                e = ((visibilityMask & renderMask & 32) == 32) & haveSurface;
                if  (e == false & allFacesDisabled)
                {
                    surfaceRenderer.enabled = false;
                    surfaceRenderer.GetComponent<Collider>().enabled = false;
                }
                else
                {
                    surfaceRenderer.enabled = true;
                    surfaceRenderer.GetComponent<Collider>().enabled = true;
                }
                
                e = ((visibilityMask & renderMask & 16) == 16);
                if (e == false & allFacesDisabled) ceilingRenderer.enabled = false; else ceilingRenderer.enabled = true;
                //eo visibility check
            }
        }
        if (structureBlock != null) structureBlock.SetVisibilityMask(x);
    }

    public void DestroySurface()
    {
        if (!haveSurface) return;
        haveSurface = false;
        if (grassland != null) grassland.Annihilation();
        if (surfaceObjects.Count != 0) ClearSurface(true);
        surfaceRenderer.gameObject.SetActive(false);
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 47);
    }
    public void RestoreSurface(int newMaterialID)
    {
        if (haveSurface) return;
        ceilingMaterial = newMaterialID;
        haveSurface = true;
        illumination = myChunk.lightMap[pos.x, pos.y, pos.z];
        surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, surfaceRenderer.GetComponent<MeshFilter>(), illumination);
        surfaceRenderer.gameObject.SetActive(true);
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 15);
    }

    override public void SetIllumination()
    {
        byte prevIllumination = illumination;
        int size = Chunk.CHUNK_SIZE;
        byte[,,] lmap = myChunk.lightMap;
        if (faces[0] != null)
        {
            if (pos.z + 1 >= size) illumination = 255; else illumination = lmap[pos.x, pos.y, pos.z + 1];
            faces[0].sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, faces[0].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[1] != null)
        {
            if (pos.x + 1 >= size) illumination = 255; else illumination = lmap[pos.x + 1, pos.y, pos.z];
            faces[1].sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, faces[1].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[2] != null)
        {
            if (pos.z - 1 < 0) illumination = 255; else illumination = lmap[pos.x, pos.y, pos.z - 1];
            faces[2].sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, faces[2].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[3] != null)
        {
            if (pos.x - 1 < 0) illumination = 255; else illumination = lmap[pos.x - 1, pos.y, pos.z];
            faces[3].sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, faces[3].GetComponent<MeshFilter>(), illumination);
        }
        illumination = lmap[pos.x, pos.y, pos.z];
        if (illumination != prevIllumination)
        {
            if (ceilingRenderer != null)
            {
                ceilingRenderer.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, ceilingRenderer.GetComponent<MeshFilter>(), illumination);
            }
            if (surfaceRenderer != null)
            {
                if (grassland == null) surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(ceilingMaterial, surfaceRenderer.GetComponent<MeshFilter>(), illumination);
                else grassland.SetGrassTexture();
            }
        }
    }

    #region save-load system
    override public List<byte> Save()
    {
        int m_id = material_id;
        if (!haveSurface) material_id = -1;

        var data = GetBlockData(); // << запись материала
        material_id = m_id;

        data.AddRange(System.BitConverter.GetBytes(ceilingMaterial));
        data.AddRange(SaveSurfaceBlockData());     

        return data;
    }
    #endregion
}
