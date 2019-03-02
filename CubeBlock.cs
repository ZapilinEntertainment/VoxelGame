using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBlock : Block
{
    public MeshRenderer[] faces { get; private set; } // 0 - north, 1 - east, 2 - south, 3 - west, 4 - up, 5 - down
    public float naturalFossils = 0;
    public byte excavatingStatus { get; private set; } // 0 is 75%+, 1 is 50%+, 2 is 25%+, 3 is less than 25%
    byte prevDrawMask = 0;
    public int volume;
    public static readonly int MAX_VOLUME;
    public bool career { get; private set; } // изменена ли верхняя поверхность на котлован?

    public new const int SERIALIZER_LENGTH = 9;
    public const string FWD_PLANE_NAME = "forwardPlane", RIGHT_PLANE_NAME = "rightPlane", BACK_PLANE_NAME = "backPlane", LEFT_PLANE_NAME = "leftPlane", UP_PLANE_NAME = "upperPlane", DOWN_PLANE_NAME = "bottomPlane";

    static CubeBlock()
    {
        MAX_VOLUME = SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION;
    }

    public int PourIn(int blocksCount)
    {
        if (volume == MAX_VOLUME) return blocksCount;
        if (blocksCount > (MAX_VOLUME - volume))
        {
            blocksCount = MAX_VOLUME - volume;
        }
        volume += blocksCount;
        CheckExcavatingStatus();
        return blocksCount;
    }

    public int Dig(int blocksCount, bool show)
    {
        if (volume == 0) return 0;
        if (blocksCount > volume) blocksCount = volume;
        volume -= blocksCount;
        if (show) career = true;
        if (career) CheckExcavatingStatus();
        else
        {
            if (volume == 0)
            {
                Block lowerBlock = myChunk.GetBlock(pos.x, pos.y - 1, pos.z);
                Block upperBlock = myChunk.GetBlock(pos.x, pos.y + 1, pos.z);
                int ceilingMaterial = material_id;
                bool convertToSurfaceType = true;
                if (upperBlock != null)
                {
                    ceilingMaterial = upperBlock.material_id;
                    if (upperBlock is SurfaceBlock) convertToSurfaceType = false;
                }

                if (lowerBlock == null)
                {
                    if (convertToSurfaceType) myChunk.DeleteBlock(pos);
                    else
                    {
                        myChunk.ReplaceBlock(pos, BlockType.Cave, -1, upperBlock.material_id, false);
                    }
                }
                else
                {
                    bool haveSupport = true;
                    int surfMaterial = material_id;
                    switch (lowerBlock.type)
                    {
                        case BlockType.Shapeless:
                            haveSupport = false;
                            break;
                        case BlockType.Cube:
                            {
                                CubeBlock cb = lowerBlock as CubeBlock;
                                if (cb.excavatingStatus != 0) haveSupport = false;
                                else {
                                    haveSupport = true;
                                    surfMaterial = lowerBlock.material_id;
                                }
                            }
                            break;
                        case BlockType.Surface:
                            {
                                SurfaceBlock sb = lowerBlock as SurfaceBlock;
                                if (sb.haveSupportingStructure)
                                {
                                    haveSupport = true;
                                    surfMaterial = sb.structureBlockRenderer != null ? ResourceType.ADVANCED_COVERING_ID : ResourceType.CONCRETE_ID;
                                }
                                else haveSupport = false;
                            }
                            break;
                        case BlockType.Cave:
                            {
                                CaveBlock cvb = lowerBlock as CaveBlock;
                                if (cvb != null) surfMaterial = cvb.ceilingMaterial;
                                else surfMaterial = lowerBlock.material_id;
                            }
                            break;
                    }
                    if (haveSupport)
                    {
                        if (convertToSurfaceType) myChunk.ReplaceBlock(pos, BlockType.Surface, lowerBlock.material_id, false);
                        else myChunk.ReplaceBlock(pos, BlockType.Cave, surfMaterial, ceilingMaterial, false);
                    }
                    else
                    {
                        if (convertToSurfaceType) myChunk.DeleteBlock(pos);
                        else myChunk.ReplaceBlock(pos, BlockType.Cave, -1, ceilingMaterial, false);
                    }
                }                
            }
        }
        return blocksCount;
    }

    public void SetFossilsVolume(int x)
    {
        naturalFossils = x;
    }

    public void InitializeCubeBlock(Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id, bool naturalGeneration)
    {
        visibilityMask = 0;
        excavatingStatus = 0;
        naturalFossils = MAX_VOLUME;
        volume = MAX_VOLUME; career = false;
        type = BlockType.Cube;

        myChunk = f_chunk;
        pos = f_chunkPos;
        Transform t = transform;
        t.parent = f_chunk.transform;
        t.localPosition = new Vector3(pos.x, pos.y, pos.z);
        t.localRotation = Quaternion.Euler(Vector3.zero);
        name = "block " + pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
        material_id = f_material_id;
        illumination = 255;

        faces = new MeshRenderer[6];
        if (naturalGeneration) { naturalFossils = MAX_VOLUME; }
        else naturalFossils = 0;
    }

    public override void ReplaceMaterial(int newId)
    {
        if (newId == material_id) return;
        material_id = newId;
        foreach (MeshRenderer mr in faces)
        {
            if (mr == null) continue;
            else mr.sharedMaterial = ResourceType.GetMaterialById(material_id, mr.GetComponent<MeshFilter>(), illumination);
        }
    }

    override public void SetRenderBitmask(byte x)
    {
        renderMask = x;
        ChangeFacesStatus();
    }

    override public void SetVisibilityMask(byte x)
    {
        byte prevMask = visibilityMask;
        // блоки, отключающиеся полностью, возвращают модели обратно в пул
        if (prevMask == 0 & x != 0) // включение
        {
            visibilityMask = x;
            for (int i = 0; i < 6; i++)
            {
                if (faces[i] == null) CreateFace(i);
                else faces[i].gameObject.SetActive(true);
            }
            ChangeFacesStatus(); // т.к в случае полного отключение вырубаем не рендереры, а сами объекты
        }
        else
        {
            if (prevMask != 0 & x == 0) // полное выключение
            {
                visibilityMask = 0;
                if (faces[4] != null)
                {
                    if (excavatingStatus == 0) { PoolMaster.ReturnQuadToPool(faces[4].gameObject); faces[4] = null; }
                    else faces[4].gameObject.SetActive(false);
                }
                if (faces[0] != null) { PoolMaster.ReturnQuadToPool(faces[0].gameObject); faces[0] = null; }
                if (faces[1] != null) { PoolMaster.ReturnQuadToPool(faces[1].gameObject); faces[1] = null; }
                if (faces[2] != null) { PoolMaster.ReturnQuadToPool(faces[2].gameObject); faces[2] = null; }
                if (faces[3] != null) { PoolMaster.ReturnQuadToPool(faces[3].gameObject); faces[3] = null; }
                if (faces[5] != null) { PoolMaster.ReturnQuadToPool(faces[5].gameObject); faces[5] = null; }
            }
            else
            {
                visibilityMask = x;
                ChangeFacesStatus();
            }
        }
    }

    void ChangeFacesStatus()
    {
        byte mask = (byte)(renderMask & visibilityMask);
        if (mask == prevDrawMask) return;
        else prevDrawMask = mask;
        byte[] arr = new byte[] { 1, 2, 4, 8, 16, 32 };
        for (int i = 0; i < 6; i++)
        {
            if (faces[i] == null) CreateFace(i);
            if (((mask & arr[i]) == 0))
            {
                faces[i].enabled = false;
                faces[i].GetComponent<Collider>().enabled = false;
            }
            else
            {
                faces[i].enabled = true;
                faces[i].GetComponent<Collider>().enabled = true;
            }
        }
    }

    void CreateFace(int i)
    {
        GameObject g = PoolMaster.GetQuad();
        g.tag = BLOCK_COLLIDER_TAG;
        Transform t = g.transform;
        t.parent = transform;
        faces[i] = g.GetComponent<MeshRenderer>();

        bool roofPlane = false;
        byte faceIllumination = 255;
        switch (i)
        {
            case 0: // fwd
                g.name = FWD_PLANE_NAME;
                t.localRotation = Quaternion.Euler(0, 180, 0);
                t.localPosition = new Vector3(0, 0, QUAD_SIZE / 2f);
                if (pos.z != Chunk.CHUNK_SIZE - 1) faceIllumination = myChunk.lightMap[pos.x, pos.y, pos.z + 1];
                break;
            case 1: // right
                g.name = RIGHT_PLANE_NAME;
                t.localRotation = Quaternion.Euler(0, 270, 0);
                t.localPosition = new Vector3(QUAD_SIZE / 2f, 0, 0);
                if (pos.x != Chunk.CHUNK_SIZE - 1) faceIllumination = myChunk.lightMap[pos.x + 1, pos.y, pos.z];
                break;
            case 2: // back
                g.name = BACK_PLANE_NAME;
                t.localRotation = Quaternion.Euler(0, 0, 0);
                t.localPosition = new Vector3(0, 0, -QUAD_SIZE / 2f);
                if (pos.z != 0) faceIllumination = myChunk.lightMap[pos.x, pos.y, pos.z - 1];
                break;
            case 3: // left
                g.name = LEFT_PLANE_NAME;
                t.localRotation = Quaternion.Euler(0, 90, 0);
                t.localPosition = new Vector3(-QUAD_SIZE / 2f, 0, 0);
                if (pos.x != 0) faceIllumination = myChunk.lightMap[pos.x - 1, pos.y, pos.z];
                break;
            case 4: // up
                g.name = UP_PLANE_NAME;
                t.localPosition = new Vector3(0, QUAD_SIZE / 2f, 0);
                t.localRotation = Quaternion.Euler(90, 0, 0);
                if (pos.y != Chunk.CHUNK_SIZE - 1) faceIllumination = myChunk.lightMap[pos.x, pos.y + 1, pos.z];
                else
                {
                    roofPlane = true;
                    t.tag = "Untagged";
                }
                break;
            case 5: // down
                g.name = DOWN_PLANE_NAME;
                t.localRotation = Quaternion.Euler(-90, 0, 0);
                t.localPosition = new Vector3(0, -QUAD_SIZE / 2f, 0);
                if (pos.y != 0) faceIllumination = myChunk.lightMap[pos.x, pos.y - 1, pos.z];
                //GameObject.Destroy( faces[i].gameObject.GetComponent<MeshCollider>() );
                break;
        }
        if (!roofPlane) faces[i].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[i].GetComponent<MeshFilter>(), faceIllumination);
        else faces[i].sharedMaterial = ResourceType.GetMaterialById(ResourceType.SNOW_ID, faces[i].GetComponent<MeshFilter>(), faceIllumination);
        if (!PoolMaster.useAdvancedMaterials)
        {
            faces[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            faces[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            faces[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
        //if (Block.QUAD_SIZE != 1) faces[i].transform.localScale = Vector3.one * Block.QUAD_SIZE;
        faces[i].enabled = true;
    }

    void CheckExcavatingStatus()
    {
        if (!career) return;
        if (volume == 0)
        {
            //myChunk.DeleteBlock(pos);
            Block lowerBlock = myChunk.GetBlock(pos.x, pos.y - 1, pos.z);
            if (lowerBlock == null) {
                myChunk.DeleteBlock(pos);
            }
            else
            {
                if (lowerBlock.type == BlockType.Surface)
                {
                    if (!(lowerBlock as SurfaceBlock).haveSupportingStructure & myChunk.CalculateSupportPoints(lowerBlock.pos.x, lowerBlock.pos.y, lowerBlock.pos.z) > Chunk.SUPPORT_POINTS_ENOUGH_FOR_HANGING) myChunk.ReplaceBlock(lowerBlock.pos, BlockType.Cave, lowerBlock.material_id, material_id, false);
                }
                //проверка на верхний блок не нужна, так как добывается открыто
                myChunk.ReplaceBlock(pos, BlockType.Surface, material_id, false);
            }
            
        }
        float pc = volume / (float)MAX_VOLUME;
        if (pc > 0.5f)
        {
            if (pc > 0.75f)
            {
                if (excavatingStatus != 0)
                {
                    excavatingStatus = 0;
                    if (faces[4] == null) CreateFace(4);
                    MeshFilter mf = faces[4].GetComponent<MeshFilter>();
                    mf.sharedMesh = PoolMaster.GetOriginalQuadMesh();
                    ResourceType.GetMaterialById(material_id, mf, illumination);
                }
            }
            else
            {
                if (excavatingStatus != 1)
                {
                    excavatingStatus = 1;
                    if (faces[4] == null) CreateFace(4);
                    MeshFilter mf = faces[4].GetComponent<MeshFilter>();
                    mf.sharedMesh = PoolMaster.plane_excavated_025;
                    ResourceType.GetMaterialById(material_id, mf, illumination);
                }
            }
        }
        else
        { // выкопано больше половины
            if (pc > 0.25f)
            {
                if (excavatingStatus != 2)
                {
                    excavatingStatus = 2;
                    if (faces[4] == null) CreateFace(4);
                    MeshFilter mf = faces[4].GetComponent<MeshFilter>();
                    mf.sharedMesh = PoolMaster.plane_excavated_05;
                    ResourceType.GetMaterialById(material_id, mf, illumination);
                }
            }
            else
            {
                if (excavatingStatus != 3)
                {
                    excavatingStatus = 3;
                    if (faces[4] == null) CreateFace(4);
                    MeshFilter mf = faces[4].GetComponent<MeshFilter>();
                    mf.sharedMesh = PoolMaster.plane_excavated_075;
                    ResourceType.GetMaterialById(material_id, mf, illumination);
                }
            }

        }
    }

    override public void SetIllumination()
    {
        illumination = myChunk.lightMap[pos.x, pos.y, pos.z];
        int size = Chunk.CHUNK_SIZE;
        byte[,,] lmap = myChunk.lightMap;
        if (faces[0] != null)
        {
            if (pos.z + 1 >= size) illumination = 255; else illumination = lmap[pos.x, pos.y, pos.z + 1];
            faces[0].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[0].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[1] != null)
        {
            if (pos.x + 1 >= size) illumination = 255; else illumination = lmap[pos.x + 1, pos.y, pos.z];
            faces[1].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[1].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[2] != null)
        {
            if (pos.z - 1 < 0) illumination = 255; else illumination = lmap[pos.x, pos.y, pos.z - 1];
            faces[2].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[2].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[3] != null)
        {
            if (pos.x - 1 < 0) illumination = 255; else illumination = lmap[pos.x - 1, pos.y, pos.z];
            faces[3].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[3].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[4] != null)
        {
            if (pos.y >= size - 1) illumination = 255; else illumination = lmap[pos.x, pos.y + 1, pos.z];
            faces[4].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[4].GetComponent<MeshFilter>(), illumination);
        }
        if (faces[5] != null)
        {
            if (pos.y == 0) illumination = 255; else illumination = lmap[pos.x, pos.y - 1, pos.z];
            faces[5].sharedMaterial = ResourceType.GetMaterialById(material_id, faces[5].GetComponent<MeshFilter>(), illumination);
        }
    } 

    override public void Annihilate()
    {
        // #block annihilate
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        if (worksite != null) worksite.StopWork();
        if (mainStructure != null) mainStructure.SectionDeleted(pos);
        // end
        if (excavatingStatus == 0 & faces[4] != null) PoolMaster.ReturnQuadToPool(faces[4].gameObject);
        if (faces[0] != null) PoolMaster.ReturnQuadToPool(faces[0].gameObject);
        if (faces[1] != null) PoolMaster.ReturnQuadToPool(faces[1].gameObject);
        if (faces[2] != null) PoolMaster.ReturnQuadToPool(faces[2].gameObject);
        if (faces[3] != null) PoolMaster.ReturnQuadToPool(faces[3].gameObject);
        if (faces[5] != null) PoolMaster.ReturnQuadToPool(faces[5].gameObject);
        if (pos.y == Chunk.CHUNK_SIZE - 1) myChunk.DeleteRoof(pos.x, pos.z);
        Destroy(gameObject);
    }

    #region save-load system
    override public void Save( System.IO.FileStream fs)
    {
        SaveBlockData(fs);
        if (career) fs.WriteByte(1); else fs.WriteByte(0);
        fs.Write(System.BitConverter.GetBytes(naturalFossils),0,4);
        fs.Write(System.BitConverter.GetBytes(volume),0,4);
        //SERIALIZER_LENGTH = 9;
    }

    public void LoadCubeBlockData(System.IO.FileStream fs)
    {
        career = fs.ReadByte() == 1;
        var data = new byte[8];
        fs.Read(data, 0, data.Length);
        naturalFossils = System.BitConverter.ToSingle(data, 0);
        volume = System.BitConverter.ToInt32(data, 4);
        if (career) CheckExcavatingStatus();
    }
    #endregion    
}
