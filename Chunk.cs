using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkPos
{
    public byte x, y, z;
    public ChunkPos(byte xpos, byte ypos, byte zpos)
    {
        x = xpos; y = ypos; z = zpos;
    }
    public ChunkPos(int xpos, int ypos, int zpos)
    {
        if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0; if (zpos < 0) zpos = 0;
        x = (byte)xpos; y = (byte)ypos; z = (byte)zpos;
    }
}

public enum ChunkGenerationMode : byte { Standart, GameLoading, Cube, TerrainLoading, DontGenerate }

public sealed class Chunk : MonoBehaviour
{
    public Block[,,] blocks { get; private set; }
    public List<SurfaceBlock> surfaceBlocks { get; private set; }
    public byte prevBitmask = 63;
    public float lifePower = 0;
    public static byte CHUNK_SIZE { get; private set; }
    //private bool allGrasslandsCreated = false;
    public byte[,,] lightMap { get; private set; }
    public int MAX_BLOCKS_COUNT = 100;
    public delegate void ChunkUpdateHandler();
    public event ChunkUpdateHandler ChunkUpdateEvent;

    private float LIGHT_DECREASE_PER_BLOCK = 1 - 1f / (PoolMaster.MAX_MATERIAL_LIGHT_DIVISIONS + 1), chunkUpdateTimer;
    private bool chunkUpdated = false, borderDrawn = false;
    private Roof[,] roofs;
    private GameObject roofObjectsHolder;

    public const float SUPPORT_POINTS_ENOUGH_FOR_HANGING = 2, CHUNK_UPDATE_TICK = 0.5f;
    public const byte MIN_CHUNK_SIZE = 3;

    static Chunk() 
    {
        CHUNK_SIZE = 16;
    }
    public static void SetChunkSizeValue(byte x)
    {
        CHUNK_SIZE = x;
    }

    private void Prepare()
    {
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        surfaceBlocks = new List<SurfaceBlock>();
        lightMap = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        roofs = new Roof[CHUNK_SIZE, CHUNK_SIZE];
        if (roofObjectsHolder == null)
        {
            roofObjectsHolder = new GameObject("roofObjectsHolder");
            roofObjectsHolder.transform.parent = transform;
            roofObjectsHolder.transform.localPosition = Vector3.zero;
            roofObjectsHolder.transform.localRotation = Quaternion.identity;
        }
        GameMaster.layerCutHeight = CHUNK_SIZE;
        GameMaster.prevCutHeight = CHUNK_SIZE;

        Grassland.ScriptReset();
    }

    public void Awake()
    {
        FollowingCamera.main.cameraChangedEvent += CameraUpdate;
    }

    #region updating
    private void FixedUpdate()
    {
        chunkUpdateTimer -= Time.fixedDeltaTime;
        if (chunkUpdateTimer <= 0)
        {
            if (chunkUpdated)
            {
                if (ChunkUpdateEvent != null) ChunkUpdateEvent();
                chunkUpdated = false;
            }
            chunkUpdateTimer = CHUNK_UPDATE_TICK;
        }
    }

    public void CameraUpdate()
    {
        StartCoroutine(CullingUpdate());
    }

    public void LifepowerUpdate()
    {
        if (surfaceBlocks.Count > 0)
        {
            float grasslandLifepowerChanges = 0;
            if (lifePower > 100)
            {
                // создание новых и снабжение существующих
                List<SurfaceBlock> dirt_for_grassland = new List<SurfaceBlock>();
                foreach (SurfaceBlock sb in surfaceBlocks)
                {
                    if (sb.material_id == ResourceType.DIRT_ID & sb.grassland == null & sb.worksite == null) dirt_for_grassland.Add(sb);
                }
                if (dirt_for_grassland.Count > 0)
                {
                    int pos = (int)(Random.value * (dirt_for_grassland.Count - 1));
                    SurfaceBlock sb = dirt_for_grassland[pos];
                    Grassland gl = Grassland.CreateOn(sb);
                    int lifeTransfer = (int)(GameConstants.MAX_LIFEPOWER_TRANSFER * GameMaster.realMaster.lifeGrowCoefficient);
                    if (lifePower > lifeTransfer) { gl.AddLifepower(lifeTransfer); lifePower -= lifeTransfer; }
                    else { gl.AddLifepower((int)lifePower); lifePower = 0; }
                }
                grasslandLifepowerChanges = lifePower * 0.75f;
            }
            if (lifePower < -100)
            { // выкачивание жизненной силы обратно
                grasslandLifepowerChanges = lifePower / 2f;
            }
            lifePower = Grassland.GrasslandUpdate(grasslandLifepowerChanges);
        }
    }

    IEnumerator CullingUpdate() // фпс увеличивается, когда идет апдейт. То есть все-таки выделяет корутины в отдельный поток?
    {
        Vector3 cpos = transform.InverseTransformPoint(FollowingCamera.camPos);
        Vector3 v = Vector3.one * (-1);
        float size = CHUNK_SIZE * Block.QUAD_SIZE;
        if (cpos.x > 0) { if (cpos.x > size) v.x = 1; else v.x = 0; }
        if (cpos.y > 0) { if (cpos.y > size) v.y = 1; else v.y = 0; }
        if (cpos.z > 0) { if (cpos.z > size) v.z = 1; else v.z = 0; }
        if (v != Vector3.zero)
        {
            //easy-culling	
            byte renderBitmask = 63;
            if (v.x == 1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
            if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
            if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;
            if (renderBitmask != prevBitmask)
            {
                foreach (Block b in blocks)
                {
                    if (b != null)
                    {
                        b.SetRenderBitmask(renderBitmask);
                    }
                }
                prevBitmask = renderBitmask;
            }
        }
        else
        {
            //camera in chunk
            foreach (Block b in blocks)
            {
                if (b == null) continue;
                Vector3 icpos = FollowingCamera.camTransform.worldToLocalMatrix * (new Vector3(b.pos.x, b.pos.y, b.pos.z) * Block.QUAD_SIZE);
                Vector3 vn = Vector3.one * (-1);
                if (icpos.x > 0) { if (icpos.x > size) vn.x = 1; else vn.x = 0; }
                if (icpos.y > 0) { if (icpos.y > size) vn.y = 1; else vn.y = 0; }
                if (icpos.z > 0) { if (icpos.z > size) vn.z = 1; else vn.z = 0; }
                byte renderBitmask = 63;
                if (v.x == 1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
                if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
                if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;
                b.SetRenderBitmask(renderBitmask);
            }
        }
        yield return null;
    }
    #endregion

    public byte GetVisibilityMask(int x, int y, int z)
    {
        if (y > GameMaster.layerCutHeight) return 0;
        else
        {
            byte vmask = 15; // видны только боковые (0,1,2,3)
            Block bx = GetBlock(x, y, z + 1);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube ) vmask -= 1;
            }
            bx = GetBlock(x + 1, y, z);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube ) vmask -= 2;
            }
            bx = GetBlock(x, y, z - 1);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube ) vmask -= 4;
            }
            bx = GetBlock(x - 1, y, z);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube ) vmask -= 8;
            }
            // up and down
            bx = GetBlock(x, y + 1, z);
            if (bx == null & y != CHUNK_SIZE - 1) vmask += 16;

            bx = GetBlock(x, y - 1, z);
            if (bx == null) vmask += 32;
            else
            {
                if (bx.type == BlockType.Surface)
                {
                    SurfaceBlock sb = bx as SurfaceBlock;
                    if (!sb.haveSupportingStructure) vmask += 32;
                }
            }
            return vmask;
        }
    }
    public void ApplyVisibleInfluenceMask(int x, int y, int z, byte mask)
    {
        Block b = GetBlock(x, y, z + 1); if (b != null) b.ChangeVisibilityMask(2, ((mask & 1) != 0));
        b = GetBlock(x + 1, y, z); if (b != null) b.ChangeVisibilityMask(3, ((mask & 2) != 0));
        b = GetBlock(x, y, z - 1); if (b != null) b.ChangeVisibilityMask(0, ((mask & 4) != 0));
        b = GetBlock(x - 1, y, z); if (b != null) b.ChangeVisibilityMask(1, ((mask & 8) != 0));
        b = GetBlock(x, y + 1, z); if (b != null) b.ChangeVisibilityMask(5, ((mask & 16) != 0));
        b = GetBlock(x, y - 1, z); if (b != null) b.ChangeVisibilityMask(4, ((mask & 32) != 0));
    }

    public void ChunkLightmapFullRecalculation()
    {
        byte UP_LIGHT = 255, DOWN_LIGHT = 128;
        int x = 0, y = 0, z = 0;
        for (x = 0; x < CHUNK_SIZE; x++)
        {
            for (z = 0; z < CHUNK_SIZE; z++)
            {
                for (y = 0; y < CHUNK_SIZE; y++)
                {
                    lightMap[x, y, z] = 0;
                }
            }
        }
        // проход снизу
        Block b = null;
        x = 0; y = 0; z = 0;
        for (x = 0; x < CHUNK_SIZE; x++)
        {
            for (z = 0; z < CHUNK_SIZE; z++)
            {
                for (y = 0; y < CHUNK_SIZE; y++)
                {
                    b = blocks[x, y, z];
                    if (b == null) lightMap[x, y, z] = DOWN_LIGHT;
                    else
                    {
                        if (b.type == BlockType.Cave)
                        {
                            if (!(b as CaveBlock).haveSurface)
                            {
                                lightMap[x, y, z] = DOWN_LIGHT;
                                break;
                            }
                            else break;
                        }
                        else
                        {
                            if (b.type == BlockType.Shapeless) { lightMap[x, y, z] = DOWN_LIGHT; }
                            else break;
                        }
                    }
                }
            }
        }
        // проход сверху
        x = 0; y = 0; z = 0;
        for (x = 0; x < CHUNK_SIZE; x++)
        {
            for (z = 0; z < CHUNK_SIZE; z++)
            {
                for (y = CHUNK_SIZE - 1; y >= 0; y--)
                {
                    b = blocks[x, y, z];
                    if (b == null) lightMap[x, y, z] = UP_LIGHT;
                    else
                    {
                        if (b.type == BlockType.Shapeless) lightMap[x, y, z] = UP_LIGHT;
                        else
                        {
                            if (b.type == BlockType.Surface) { lightMap[x, y, z] = UP_LIGHT; break; }
                            else break;
                        }
                    }
                }
            }
        }
        //проход спереди
        byte decreasedVal;
        for (x = 0; x < CHUNK_SIZE; x++)
        {
            for (y = 0; y < CHUNK_SIZE; y++)
            {
                decreasedVal = (byte)(lightMap[x, y, CHUNK_SIZE - 1] * LIGHT_DECREASE_PER_BLOCK);
                for (z = CHUNK_SIZE - 2; z >= 0; z--)
                {
                    if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        //проход сзади
        for (x = 0; x < CHUNK_SIZE; x++)
        {
            for (y = 0; y < CHUNK_SIZE; y++)
            {
                decreasedVal = (byte)(lightMap[x, y, 0] * LIGHT_DECREASE_PER_BLOCK);
                for (z = 1; z < CHUNK_SIZE; z++)
                {
                    if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        //проход справа
        for (z = 0; z < CHUNK_SIZE; z++)
        {
            for (y = 0; y < CHUNK_SIZE; y++)
            {
                decreasedVal = (byte)(lightMap[CHUNK_SIZE - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                for (x = CHUNK_SIZE - 2; x >= 0; x--)
                {
                    b = blocks[x, y, z];
                    if (b == null)
                    {
                        if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    }
                    else
                    {
                        if (b.type == BlockType.Shapeless | b.type == BlockType.Surface)
                        {
                            if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                        }
                        else
                        {
                            if (b.type == BlockType.Cave) lightMap[x, y, z] = decreasedVal;
                        }
                    }
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        //проход слева
        for (z = 0; z < CHUNK_SIZE; z++)
        {
            for (y = 0; y < CHUNK_SIZE; y++)
            {
                decreasedVal = (byte)(lightMap[0, y, z] * LIGHT_DECREASE_PER_BLOCK);
                for (x = 1; x < CHUNK_SIZE; x++)
                {
                    if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        foreach (Block block in blocks)
        {
            if (block != null)
            {
                if (block.illumination != lightMap[block.pos.x, block.pos.y, block.pos.z]) block.SetIllumination();
            }
        }
    }
    public void RecalculateIlluminationAtPoint(ChunkPos pos)
    {
        ChunkLightmapFullRecalculation(); // в разработке
    }

    #region operating blocks data

    public void CreateNewChunk(int[,,] newData)
    {
        int size = newData.GetLength(0);
        CHUNK_SIZE = (byte)size;
        if (blocks != null) ClearChunk();
        Prepare();

        for (int y = size - 1; y > -1; y--)
        {
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (newData[x,y,z] != 0)
                    {
                        AddBlock(new ChunkPos(x, y, z), BlockType.Cube, newData[x, y, z], true);
                    }
                }
            }
        }       
        if (surfaceBlocks.Count > 0)
        {
            foreach (SurfaceBlock sb in surfaceBlocks)
            {
                GameMaster.geologyModule.SpreadMinerals(sb);
            }
        }
        FollowingCamera.main.WeNeedUpdate();
    }

    public Block GetBlock(ChunkPos cpos) { return GetBlock(cpos.x, cpos.y, cpos.z); }
    public Block GetBlock(int x, int y, int z)
    {
        if (x < 0 | x > CHUNK_SIZE - 1 | y < 0 | y > CHUNK_SIZE - 1 | z < 0 | z > CHUNK_SIZE - 1) return null;
        else { return blocks[x, y, z]; }
    }

    public Block AddBlock(ChunkPos f_pos, BlockType f_type, int material1_id, bool naturalGeneration)
    {
        return AddBlock(f_pos, f_type, material1_id, material1_id, naturalGeneration);
    }
    public Block AddBlock(ChunkPos f_pos, BlockType f_type, int i_floorMaterialID, int i_ceilingMaterialID, bool i_naturalGeneration)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (x >= CHUNK_SIZE | y >= CHUNK_SIZE | z >= CHUNK_SIZE) return null;
        Block prv = GetBlock(x, y, z);        
        if (prv != null)
        {
            if (prv.type != f_type) return ReplaceBlock(f_pos, f_type, i_floorMaterialID, i_ceilingMaterialID, i_naturalGeneration);
            else
            {
                if (prv.type != BlockType.Cave | i_floorMaterialID == i_ceilingMaterialID)
                {
                    prv.ReplaceMaterial(i_floorMaterialID);
                    return prv;
                }
                else
                {
                    CaveBlock cvb = prv as CaveBlock;
                    if (cvb.material_id != i_floorMaterialID) cvb.ReplaceSurfaceMaterial(i_floorMaterialID);
                    if (cvb.ceilingMaterial != i_ceilingMaterialID) cvb.ReplaceCeilingMaterial(i_ceilingMaterialID);
                    return cvb;
                }
            }
        }
        CubeBlock cb = null;
        Block b = null;
        byte visMask = GetVisibilityMask(x, y, z), influenceMask = 63; // видимость объекта, видимость стенок соседних объектов
        bool calculateUpperBlock = false;

        switch (f_type)
        {
            case BlockType.Cube:
                {
                    cb = new GameObject().AddComponent<CubeBlock>();
                    cb.InitializeCubeBlock(this, f_pos, i_floorMaterialID, i_naturalGeneration);
                    blocks[x, y, z] = cb;
                    influenceMask = 0; // закрывает собой все соседние стенки
                    calculateUpperBlock = true;
                    i_ceilingMaterialID = i_floorMaterialID;

                    lightMap[x, y, z] = 0;
                    RecalculateIlluminationAtPoint(cb.pos);
                    break;
                }
            case BlockType.Shapeless:
                {
                    b = new GameObject().AddComponent<Block>();
                    blocks[x, y, z] = b;
                    b.InitializeBlock(this, f_pos, i_floorMaterialID);

                    //#shapeless light recalculation
                    byte light = lightMap[x, y, z];
                    if (light != 255)
                    {
                        if (z < CHUNK_SIZE - 1 && lightMap[x, y, z + 1] > light) light = (byte)(lightMap[x, y, z + 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (y < CHUNK_SIZE - 1 && lightMap[x, y + 1, z] > light) light = lightMap[x, y + 1, z];
                        if (y > 0 && lightMap[x, y - 1, z] > light) light = (byte)(lightMap[x, y - 1, z] * LIGHT_DECREASE_PER_BLOCK);
                    }
                    if (light != lightMap[x, y, z])
                    {
                        lightMap[x, y, z] = light;
                        RecalculateIlluminationAtPoint(b.pos);
                    }
                    // eo shapeless light recalculation
                    break;
                }
            case BlockType.Surface:
                {
                    influenceMask = 31;

                    SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
                    sb.InitializeSurfaceBlock(this, f_pos, i_floorMaterialID);
                    blocks[x, y, z] = sb;
                    surfaceBlocks.Add(sb);

                    //#surface light recalculation
                    byte light = lightMap[x, y, z];
                    if (light != 255)
                    {
                        if (z < CHUNK_SIZE - 1 && lightMap[x, y, z + 1] > light) light = (byte)(lightMap[x, y, z + 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (y < CHUNK_SIZE - 1 && lightMap[x, y + 1, z] > light) light = lightMap[x, y + 1, z];
                    }
                    if (light != lightMap[x, y, z])
                    {
                        lightMap[x, y, z] = light;
                        RecalculateIlluminationAtPoint(sb.pos);
                    }
                    // eo surface light recalculation
                    break;
                }
            case BlockType.Cave:
                {
                    Block lowerBlock = blocks[x, y - 1, z];
                    if (lowerBlock == null) i_floorMaterialID = -1;
                    CaveBlock caveb = new GameObject().AddComponent<CaveBlock>();
                    caveb.InitializeCaveBlock(this, f_pos, i_ceilingMaterialID, i_floorMaterialID);
                    blocks[x, y, z] = caveb;
                    if (caveb.haveSurface) influenceMask = 15; else influenceMask = 47;
                    calculateUpperBlock = true;
                    surfaceBlocks.Add(caveb);

                    //#cave light recalculation
                    byte light = lightMap[x, y, z];
                    if (light != 255)
                    {
                        if (z < CHUNK_SIZE - 1 && lightMap[x, y, z + 1] > light) light = (byte)(lightMap[x, y, z + 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (!caveb.haveSurface & y > 0 && lightMap[x, y - 1, z] > light) light = (byte)(lightMap[x, y - 1, z] * LIGHT_DECREASE_PER_BLOCK);
                    }
                    if (light != lightMap[x, y, z])
                    {
                        lightMap[x, y, z] = light;
                        RecalculateIlluminationAtPoint(caveb.pos);
                    }
                    // eo cave light recalculation
                }
                break;
        }
        b = blocks[x, y, z];
        b.SetVisibilityMask(visMask);
        b.SetRenderBitmask(prevBitmask);
        if (calculateUpperBlock)
        {
            if (GetBlock(x, y + 1, z) == null)
            {
                if (y < CHUNK_SIZE - 1) AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, i_ceilingMaterialID, i_ceilingMaterialID, i_naturalGeneration);
                else SetRoof(x, z, !i_naturalGeneration);
            }
        }
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        chunkUpdated = true;
        return b;
    }

    public Block ReplaceBlock(ChunkPos f_pos, BlockType f_newType, int material1_id, bool naturalGeneration)
    {
        return ReplaceBlock(f_pos, f_newType, material1_id, material1_id, naturalGeneration);
    }
    public Block ReplaceBlock(ChunkPos f_pos, BlockType f_newType, int surfaceMaterial_id, int ceilingMaterial_id, bool naturalGeneration)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        Block originalBlock = GetBlock(x, y, z);
        if (originalBlock == null) {
            return AddBlock(f_pos, f_newType, surfaceMaterial_id, ceilingMaterial_id, naturalGeneration);
        }
        if (originalBlock.type == f_newType)
        {
            if (originalBlock.type != BlockType.Cave | surfaceMaterial_id == ceilingMaterial_id)
            {
                originalBlock.ReplaceMaterial(surfaceMaterial_id);
                return originalBlock;
            }
            else
            {
                CaveBlock ocb = originalBlock as CaveBlock;
                ocb.ReplaceSurfaceMaterial(surfaceMaterial_id);
                ocb.ReplaceCeilingMaterial(ceilingMaterial_id);
                return originalBlock;
            }
        }
        else
        {
            if (originalBlock.type == BlockType.Surface | originalBlock.type == BlockType.Cave)
            {
                SurfaceBlock sb = originalBlock as SurfaceBlock;
                if (sb.structureBlockRenderer != null | sb.haveSupportingStructure) return originalBlock;
            }
            Structure fillingStructure = originalBlock.mainStructure;
        }
        Block b = null;
        byte influenceMask = 63;
        bool calculateUpperBlock = false;
        switch (f_newType)
        {
            case BlockType.Shapeless:
                {
                    b = new GameObject().AddComponent<Block>();
                    blocks[x, y, z] = b;

                    //#shapeless light recalculation
                    byte light = lightMap[x, y, z];
                    if (light != 255)
                    {
                        if (z < CHUNK_SIZE - 1 && lightMap[x, y, z + 1] > light) light = (byte)(lightMap[x, y, z + 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (y < CHUNK_SIZE - 1 && lightMap[x, y + 1, z] > light) light = lightMap[x, y + 1, z];
                        if (y > 0 && lightMap[x, y - 1, z] > light) light = (byte)(lightMap[x, y - 1, z] * LIGHT_DECREASE_PER_BLOCK);
                    }
                    if (light != lightMap[x, y, z])
                    {
                        lightMap[x, y, z] = light;
                        RecalculateIlluminationAtPoint(b.pos);
                    }
                    // eo shapeless light recalculation
                    break;
                }
            case BlockType.Surface:
                {
                    SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
                    sb.InitializeSurfaceBlock(this, f_pos, surfaceMaterial_id);
                    surfaceBlocks.Add(sb);
                    b = sb;
                    blocks[x, y, z] = sb;
                    influenceMask = 31;
                    if (originalBlock.type == BlockType.Cave)
                    {
                        (originalBlock as CaveBlock).TransferStructures(sb);
                    }
                    //#surface light recalculation
                    byte light = lightMap[x, y, z];
                    if (light != 255)
                    {
                        if (z < CHUNK_SIZE - 1 && lightMap[x, y, z + 1] > light) light = (byte)(lightMap[x, y, z + 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (y < CHUNK_SIZE - 1 && lightMap[x, y + 1, z] > light) light = lightMap[x, y + 1, z];
                    }
                    if (light != lightMap[x, y, z])
                    {
                        lightMap[x, y, z] = light;
                        RecalculateIlluminationAtPoint(sb.pos);
                    }
                    // eo surface light recalculation
                    break;
                }
            case BlockType.Cube:
                {
                    CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
                    cb.InitializeCubeBlock(this, f_pos, surfaceMaterial_id, naturalGeneration);
                    b = cb;
                    blocks[x, y, z] = cb;
                    influenceMask = 0;
                    calculateUpperBlock = true;
                    lightMap[x, y, z] = 0;
                    RecalculateIlluminationAtPoint(b.pos);
                }
                break;
            case BlockType.Cave:
                {
                    CaveBlock cvb = new GameObject().AddComponent<CaveBlock>();
                    cvb.InitializeCaveBlock(this, f_pos, ceilingMaterial_id, surfaceMaterial_id);
                    surfaceBlocks.Add(cvb);
                    blocks[x, y, z] = cvb;
                    b = cvb;
                    if (cvb.haveSurface) influenceMask = 15; else influenceMask = 47;

                    if (originalBlock.type == BlockType.Surface)
                    {
                        SurfaceBlock originalSurface = originalBlock as SurfaceBlock;
                        originalSurface.TransferStructures(cvb);
                        if (originalSurface.grassland != null)
                        {
                            Grassland gl = Grassland.CreateOn(cvb);
                            gl.SetLifepower(originalSurface.grassland.lifepower);
                            originalSurface.grassland.SetLifepower(0);
                        }
                    }
                    calculateUpperBlock = true;

                    //#cave light recalculation
                    byte light = lightMap[x, y, z];
                    if (light != 255)
                    {
                        if (z < CHUNK_SIZE - 1 && lightMap[x, y, z + 1] > light) light = (byte)(lightMap[x, y, z + 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (!cvb.haveSurface & y > 0 && lightMap[x, y - 1, z] > light) light = (byte)(lightMap[x, y - 1, z] * LIGHT_DECREASE_PER_BLOCK);
                    }
                    if (light != lightMap[x, y, z])
                    {
                        lightMap[x, y, z] = light;
                        RecalculateIlluminationAtPoint(cvb.pos);
                    }
                    // eo cave light recalculation
                }
                break;
        }
        originalBlock.Annihilate();
        originalBlock = null;

        b.SetVisibilityMask(GetVisibilityMask(x, y, z));
        b.SetRenderBitmask(prevBitmask);
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        if (calculateUpperBlock)
        {
            if (GetBlock(x, y + 1, z) == null)
            {
                if (y < CHUNK_SIZE - 1) AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, surfaceMaterial_id, naturalGeneration);
                else SetRoof(x, z, !naturalGeneration);
            }
        }
        chunkUpdated = true;
        return b;
    }

    public SurfaceBlock GetSurfaceBlock(int x, int z)
    {
        if (x < 0 || z < 0 || x >= CHUNK_SIZE || z >= CHUNK_SIZE) return null;
        SurfaceBlock fsb = null;
        foreach (SurfaceBlock sb in surfaceBlocks)
        {
            if (sb.pos.x == x && sb.pos.z == z) fsb = sb; // to find the highest
        }
        return fsb; // we are not watching you. Honestly.
    }
    public void RecalculateSurfaceBlocks()
    {
        surfaceBlocks = new List<SurfaceBlock>();
        foreach (Block b in blocks)
        {
            if (b == null) continue;
            if (b.type == BlockType.Surface | b.type == BlockType.Cave) surfaceBlocks.Add(b as SurfaceBlock);
        }
    }

    /// <summary>
    /// занимается только удалением и побочными эффектами, никакого добавления-замещения
    /// </summary>
    /// <param name="pos"></param>
    public void DeleteBlock(ChunkPos pos)
    {
        // в сиквеле стоит пересмотреть всю иерархию классов ><
        //12.06 нет, я так не думаю
        Block b = GetBlock(pos);
        if (b == null) return;
        int x = pos.x, y = pos.y, z = pos.z;

        bool neighboursInfluence = false, upperBlockInfluence = false, lowerBlockInfluence = false;
        switch (b.type)
        {
            case BlockType.Cube:
                upperBlockInfluence = true;
                lowerBlockInfluence = true;
                neighboursInfluence = true;
                break;
            case BlockType.Surface:
                {
                    SurfaceBlock sb = b as SurfaceBlock;
                    if (sb.cellsStatus != 0)
                    {
                        foreach (Structure s in sb.surfaceObjects)
                        {
                            if (s != null && s.indestructible)
                            {
                                if (GetBlock(x,y-1,z) == null)
                                {
                                    Block ub = AddBlock(new ChunkPos(x, y - 1, z), BlockType.Shapeless, ResourceType.METAL_S_ID, false);
                                    GameObject g = PoolMaster.GetFlyingPlatform();
                                    g.transform.parent = ub.transform;
                                    g.transform.localPosition = Vector3.up * Block.QUAD_SIZE / 2f;
                                }
                                return;
                            }
                        }
                    }
                    if (sb.haveSupportingStructure)
                    {
                        upperBlockInfluence = true;
                        neighboursInfluence = true;
                    }
                    lowerBlockInfluence = true;
                    break;
                }
            case BlockType.Cave:
                neighboursInfluence = true;
                upperBlockInfluence = true;
                if ((b as CaveBlock).haveSurface) lowerBlockInfluence = true;
                break;
        }
        blocks[x, y, z].Annihilate();
        blocks[x, y, z] = null;
        ApplyVisibleInfluenceMask(x, y, z, 63);

        if (neighboursInfluence)
        {
            Block sideBlock = GetBlock(x, y, z + 1);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x, sideBlock.pos.y, sideBlock.pos.z + 1);
                if (supportPoints < SUPPORT_POINTS_ENOUGH_FOR_HANGING) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
            sideBlock = GetBlock(x + 1, y, z);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x + 1, sideBlock.pos.y, sideBlock.pos.z);
                if (supportPoints < SUPPORT_POINTS_ENOUGH_FOR_HANGING) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
            sideBlock = GetBlock(x, y, z - 1);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x, sideBlock.pos.y, sideBlock.pos.z - 1);
                if (supportPoints < SUPPORT_POINTS_ENOUGH_FOR_HANGING) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
            sideBlock = GetBlock(x - 1, y, z);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x - 1, sideBlock.pos.y, sideBlock.pos.z);
                if (supportPoints < SUPPORT_POINTS_ENOUGH_FOR_HANGING) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
        }
        if (upperBlockInfluence)
        {
            Block upperBlock = GetBlock(x, y + 1, z);
            if (upperBlock != null)
            {
                if (upperBlock.type == BlockType.Surface) DeleteBlock(upperBlock.pos);
                else
                {
                    CaveBlock cb = upperBlock as CaveBlock;
                    if (cb != null)
                    {
                        if (cb.haveSurface) cb.DestroySurface();
                    }
                }
            }
        }
        if (lowerBlockInfluence)
        {
            CaveBlock lowerBlock = GetBlock(x, y - 1, z) as CaveBlock;
            if (lowerBlock != null )
            {
                if (!lowerBlock.haveSurface) DeleteBlock(lowerBlock.pos);
                else ReplaceBlock(lowerBlock.pos, BlockType.Surface, lowerBlock.material_id, false);
            }
        }
        ChunkLightmapFullRecalculation();
        chunkUpdated = true;
    }
    public void ClearChunk()
    {
        int size = blocks.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    if (blocks[i, j, k] == null) continue;
                    blocks[i, j, k].Annihilate();
                }
            }
        }
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        surfaceBlocks.Clear();
        lifePower = 0;
        chunkUpdated = true;
    }
    public void RemoveFromSurfacesList(SurfaceBlock sb)
    {
        if (surfaceBlocks.Count > 0)
        {
            for (int i = 0; i < surfaceBlocks.Count; i++)
            {
                if (surfaceBlocks[i] == sb) { surfaceBlocks.RemoveAt(i); return; }
            }
        }
    }
    #endregion

    public float CalculateSupportPoints(int x, int y, int z)
    {
        Block sideBlock = null;
        float supportPoints = 0;
        float caveSupportPoint = 0.5f;
        sideBlock = GetBlock(x + 1, y, z);
        if (sideBlock != null)
        {
            switch (sideBlock.type)
            {
                case BlockType.Cube:
                    supportPoints++;
                    break;
                case BlockType.Cave:
                    supportPoints += caveSupportPoint;
                    caveSupportPoint *= 0.9f;
                    break;
                case BlockType.Surface:
                    SurfaceBlock sb = sideBlock as SurfaceBlock;
                    if (sb.cellsStatus != 0)
                    {
                        foreach (Structure s in sb.surfaceObjects)
                        {
                            if (s != null)
                            {
                                if (s.isBasement)
                                {
                                    supportPoints += 0.55f;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
        }
        sideBlock = GetBlock(x - 1, y, z);
        if (sideBlock != null)
        {
            switch (sideBlock.type)
            {
                case BlockType.Cube:
                    supportPoints++;
                    break;
                case BlockType.Cave:
                    supportPoints += caveSupportPoint;
                    caveSupportPoint *= 0.9f;
                    break;
                case BlockType.Surface:
                    SurfaceBlock sb = sideBlock as SurfaceBlock;
                    if (sb.cellsStatus != 0)
                    {
                        foreach (Structure s in sb.surfaceObjects)
                        {
                            if (s != null)
                            {
                                if (s.isBasement)
                                {
                                    supportPoints += 0.55f;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
        }
        sideBlock = GetBlock(x, y, z + 1);
        if (sideBlock != null)
        {
            switch (sideBlock.type)
            {
                case BlockType.Cube:
                    supportPoints++;
                    break;
                case BlockType.Cave:
                    supportPoints += caveSupportPoint;
                    caveSupportPoint *= 0.9f;
                    break;
                case BlockType.Surface:
                    SurfaceBlock sb = sideBlock as SurfaceBlock;
                    if (sb.cellsStatus != 0)
                    {
                        foreach (Structure s in sb.surfaceObjects)
                        {
                            if (s != null)
                            {
                                if (s.isBasement)
                                {
                                    supportPoints += 0.55f;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
        }
        sideBlock = GetBlock(x, y, z - 1);
        if (sideBlock != null)
        {
            switch (sideBlock.type)
            {
                case BlockType.Cube:
                    supportPoints++;
                    break;
                case BlockType.Cave:
                    supportPoints += caveSupportPoint;
                    caveSupportPoint *= 0.9f;
                    break;
                case BlockType.Surface:
                    SurfaceBlock sb = sideBlock as SurfaceBlock;
                    if (sb.cellsStatus != 0)
                    {
                        foreach (Structure s in sb.surfaceObjects)
                        {
                            if (s != null)
                            {
                                if (s.isBasement)
                                {
                                    supportPoints += 0.55f;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
        }
        return supportPoints;
    }

    #region operating lifepower

    public void GenerateNature(Vector3 lifeSourcePos)
    {
        float lifepowerPerBlock = GameConstants.LIFEPOWER_PER_BLOCK;
        int dirtID = ResourceType.DIRT_ID;
        foreach (SurfaceBlock sb in surfaceBlocks)
        {
            if (sb.material_id == dirtID)
            {
                Grassland gl = Grassland.CreateOn(sb);
                if (gl != null)
                {
                    float lval = lifepowerPerBlock * ((1 - (sb.transform.position - lifeSourcePos).magnitude / CHUNK_SIZE) * 0.5f + 0.5f);
                    gl.AddLifepowerAndCalculate((int)lval);
                }
            }
        }
    }

    public void AddLifePower(int count) { lifePower += count; }
    public int TakeLifePower(int count)
    {
        if (count < 0) return 0;
        float lifeTransfer = count;
        if (lifeTransfer > lifePower) { if (lifePower >= 0) lifeTransfer = lifePower; else lifeTransfer = 0; }
        lifePower -= lifeTransfer;
        return (int)lifeTransfer;
    }
    public void TakeLifePowerWithForce(int count)
    {
        lifePower -= count;
    }

    #endregion    

    public void LayersCut()
    {
        int layerCutHeight = GameMaster.layerCutHeight;
        roofObjectsHolder.SetActive(layerCutHeight == CHUNK_SIZE);
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                int y = CHUNK_SIZE - 1;
                if (layerCutHeight != CHUNK_SIZE)
                {
                    for (; y > layerCutHeight; y--)
                    {
                        if (blocks[x, y, z] != null) blocks[x, y, z].SetVisibilityMask(0);
                    }
                }
                byte m = GetVisibilityMask(x, y, z);
                if ((m & 16) == 0) m += 16;
                if (blocks[x, y, z] != null) blocks[x,y,z].SetVisibilityMask(m);
                y--;
                for (; y > -1; y--)
                {
                    if (blocks[x, y, z] != null) blocks[x, y, z].SetVisibilityMask(GetVisibilityMask(x, y, z));
                }
            }
        }
    }

    public void SetRoof(int x, int z, bool artificial)
    {
        bool canBePeak = false;
        Roof r;
        if (roofs[x, z] != null)
        {
            r = roofs[x, z];
            if (artificial)
            {
                if (r.artificial) return;
                else
                {
                    Destroy(r);
                    // #creating_new_model
                    { // peaks checking
                        Roof n = null;
                        int peaksAround = 0;
                        bool back = (z > 0), fwd = (z < CHUNK_SIZE - 1), left = (x > 0), right = (x < CHUNK_SIZE - 1);
                        if (left)
                        {
                            n = roofs[x - 1, z]; if (n != null && n.peak) peaksAround++;
                            if (fwd)
                            {
                                n = roofs[x - 1, z + 1]; if (n != null && n.peak) peaksAround++;
                            }
                            if (back)
                            {
                                n = roofs[x - 1, z - 1]; if (n != null && n.peak) peaksAround++;
                            }
                        }
                        if (peaksAround == 0)
                        {
                            if (right)
                            {
                                n = roofs[x + 1, z]; if (n != null && n.peak) peaksAround++;
                                if (fwd)
                                {
                                    n = roofs[x + 1, z + 1]; if (n != null && n.peak) peaksAround++;
                                }
                                if (back)
                                {
                                    n = roofs[x + 1, z - 1]; if (n != null && n.peak) peaksAround++;
                                }
                            }
                        }
                        if (peaksAround == 0)
                        {
                            if (fwd)
                            {
                                n = roofs[x, z + 1]; if (n != null && n.peak) peaksAround++;
                            }
                            if (back)
                            {
                                n = roofs[x, z - 1]; if (n != null && n.peak) peaksAround++;
                            }
                        }
                        canBePeak = (peaksAround == 0) & (Random.value > 0.4f);
                    }

                    GameObject newModel = PoolMaster.GetRooftop(canBePeak, true);
                    newModel.transform.parent = roofObjectsHolder.transform;
                    newModel.transform.localPosition = new Vector3(x * Block.QUAD_SIZE, (CHUNK_SIZE - 0.5f) * Block.QUAD_SIZE, z * Block.QUAD_SIZE);
                    float t = Random.value;
                    if (t > 0.5f)
                    {
                        if (t >= 0.75f) t = 3;
                        else t = 2;
                    }
                    else
                    {
                        if (t <= 0.25f) t = 1;
                        else t = 0;
                    }
                    newModel.transform.localRotation = Quaternion.Euler(0, t * 90, 0);
                    r = newModel.AddComponent<Roof>();
                    r.artificial = artificial;
                    r.peak = canBePeak;
                    roofs[x, z] = r;
                }
            }
            else
            {
                if (!r.artificial)
                {
                    return;
                }
                else
                {
                    Destroy(r);
                    // #creating_new_model
                    { // peaks checking
                        Roof n = null;
                        int peaksAround = 0;
                        bool back = (z > 0), fwd = (z < CHUNK_SIZE - 1), left = (x > 0), right = (x < CHUNK_SIZE - 1);
                        if (left)
                        {
                            n = roofs[x - 1, z]; if (n != null && n.peak) peaksAround++;
                            if (fwd)
                            {
                                n = roofs[x - 1, z + 1]; if (n != null && n.peak) peaksAround++;
                            }
                            if (back)
                            {
                                n = roofs[x - 1, z - 1]; if (n != null && n.peak) peaksAround++;
                            }
                        }
                        if (peaksAround == 0)
                        {
                            if (right)
                            {
                                n = roofs[x + 1, z]; if (n != null && n.peak) peaksAround++;
                                if (fwd)
                                {
                                    n = roofs[x + 1, z + 1]; if (n != null && n.peak) peaksAround++;
                                }
                                if (back)
                                {
                                    n = roofs[x + 1, z - 1]; if (n != null && n.peak) peaksAround++;
                                }
                            }
                        }
                        if (peaksAround == 0)
                        {
                            if (fwd)
                            {
                                n = roofs[x, z + 1]; if (n != null && n.peak) peaksAround++;
                            }
                            if (back)
                            {
                                n = roofs[x, z - 1]; if (n != null && n.peak) peaksAround++;
                            }
                        }
                        canBePeak = (peaksAround == 0) & (Random.value > 0.4f);
                    }
                    GameObject newModel = PoolMaster.GetRooftop(canBePeak, false);
                    newModel.transform.parent = roofObjectsHolder.transform;
                    newModel.transform.localPosition = new Vector3(x * Block.QUAD_SIZE, (CHUNK_SIZE - 0.5f) * Block.QUAD_SIZE, z * Block.QUAD_SIZE);
                    float t = Random.value;
                    if (t > 0.5f)
                    {
                        if (t >= 0.75f) t = 3;
                        else t = 2;
                    }
                    else
                    {
                        if (t <= 0.25f) t = 1;
                        else t = 0;
                    }
                    newModel.transform.localRotation = Quaternion.Euler(0, t * 90, 0);
                    r = newModel.AddComponent<Roof>();
                    r.artificial = artificial;
                    r.peak = canBePeak;
                    roofs[x, z] = r;
                }
            }
        }
        else
        {
            // #creating_new_model
            { // peaks checking
                Roof n = null;
                int peaksAround = 0;
                bool back = (z > 0), fwd = (z < CHUNK_SIZE - 1), left = (x > 0), right = (x < CHUNK_SIZE - 1);
                if (left)
                {
                    n = roofs[x - 1, z]; if (n != null && n.peak) peaksAround++;
                    if (fwd)
                    {
                        n = roofs[x - 1, z + 1]; if (n != null && n.peak) peaksAround++;
                    }
                    if (back)
                    {
                        n = roofs[x - 1, z - 1]; if (n != null && n.peak) peaksAround++;
                    }
                }
                if (peaksAround == 0)
                {
                    if (right)
                    {
                        n = roofs[x + 1, z]; if (n != null && n.peak) peaksAround++;
                        if (fwd)
                        {
                            n = roofs[x + 1, z + 1]; if (n != null && n.peak) peaksAround++;
                        }
                        if (back)
                        {
                            n = roofs[x + 1, z - 1]; if (n != null && n.peak) peaksAround++;
                        }
                    }
                }
                if (peaksAround == 0)
                {
                    if (fwd)
                    {
                        n = roofs[x, z + 1]; if (n != null && n.peak) peaksAround++;
                    }
                    if (back)
                    {
                        n = roofs[x, z - 1]; if (n != null && n.peak) peaksAround++;
                    }
                }
                canBePeak = (peaksAround == 0) & (Random.value > 0.4f);
            }
            GameObject newModel = PoolMaster.GetRooftop(canBePeak, artificial);
            newModel.transform.parent = roofObjectsHolder.transform;
            newModel.transform.localPosition = new Vector3(x * Block.QUAD_SIZE, (CHUNK_SIZE - 0.5f) * Block.QUAD_SIZE, z * Block.QUAD_SIZE);
            float t = Random.value;
            if (t > 0.5f)
            {
                if (t >= 0.75f) t = 3;
                else t = 2;
            }
            else
            {
                if (t <= 0.25f) t = 1;
                else t = 0;
            }
            newModel.transform.localRotation = Quaternion.Euler(0, t * 90, 0);
            r = newModel.AddComponent<Roof>();
            r.artificial = artificial;
            r.peak = canBePeak;
            roofs[x, z] = r;
        }
    }
    public void DeleteRoof(int x, int z)
    {
        if (roofs[x, z] != null)
        {
            Destroy(roofs[x, z].gameObject);
        }
    }

    public bool BlockByStructure(int x, int y, int z, Structure s)
    {
        if ((x >= CHUNK_SIZE | x < 0) || (y >= CHUNK_SIZE | y < 0) || (z >= CHUNK_SIZE | z < 0) | (s == null)) return false;
        Block b = GetBlock(x, y, z);
        if (b != null) b = ReplaceBlock(new ChunkPos(x, y, z), BlockType.Shapeless, 0, false);
        else b = AddBlock(new ChunkPos(x, y, z), BlockType.Shapeless, 0, false);
        if (b != null)
        {
            b.SetMainStructure(s);
            chunkUpdated = true;
            return true;
        }
        else return false;
    }

    public void BlockRegion (List<ChunkPos> positions, Structure s, ref List<Block> dependentBlocks)
    {
        foreach (ChunkPos pos in positions)
        {
            if ((pos.x >= CHUNK_SIZE || pos.x < 0) | (pos.y >= CHUNK_SIZE || pos.y < 0) | (pos.z >= CHUNK_SIZE || pos.z < 0) ) continue;
            Block b = blocks[pos.x, pos.y, pos.z];
            if (b != null) continue;
            else
            {
                b = new GameObject().AddComponent<Block>();
                b.InitializeShapelessBlock(this, pos, s);
                blocks[pos.x, pos.y, pos.z] = b;
                dependentBlocks.Add(b);
            }
        }
    }
    /// <summary>
    /// uses min coordinates ( left down corner); start positions including!
    /// </summary>
    public bool BlockShipCorridorIfPossible(int xpos, int ypos, bool xyAxis, int width, Structure sender, ref List<Block> dependentBlocksList)
    {
        int xStart = xpos; int xEnd = xStart + width;
        if (xStart < 0) xStart = 0; if (xEnd >= CHUNK_SIZE) xEnd = CHUNK_SIZE;
        int yStart = ypos; int yEnd = yStart + width;
        if (yStart < 0) yStart = 0; if (yEnd >= CHUNK_SIZE) yEnd = CHUNK_SIZE;
        if (xyAxis)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if (blocks[x, y, z] != null) return false;
                        //if (blocks[x, y, z] != null) DeleteBlock(new ChunkPos(x,y,z));
                    }
                }
            }
        }
        else
        {
            for (int z = xStart; z < xEnd; z++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int x = 0; x < CHUNK_SIZE; x++)
                    {
                        if (blocks[x, y, z] != null) return false;
                        //if (blocks[x, y, z] != null) DeleteBlock(new ChunkPos(x, y, z));
                    }
                }
            }
        }
        Block bk;
        if (xyAxis)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        bk = new GameObject().AddComponent<Block>();
                        bk.InitializeShapelessBlock(this, new ChunkPos(x, y, z), sender);
                        blocks[x, y, z] = bk;
                        dependentBlocksList.Add(bk);
                    }
                }
            }
        }
        else
        {
            for (int z = xStart; z < xEnd; z++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int x = 0; x < CHUNK_SIZE; x++)
                    {
                        bk = new GameObject().AddComponent<Block>();
                        bk.InitializeShapelessBlock(this, new ChunkPos(x, y, z), sender);
                        blocks[x, y, z] = bk;
                        dependentBlocksList.Add(bk);
                    }
                }
            }
        }
        chunkUpdated = true;
        return true;
    }
    /// <summary>
    /// specify startpoint as point with min Coordinates(left down corner); startPoint including!
    /// </summary>
    public bool BlockShipCorridorIfPossible(Vector3Int startPoint, byte modelRotation, int width, Structure sender, ref List<Block> dependentBlocksList)
    {
        int xStart = startPoint.x, xEnd = startPoint.x + width;
        if (xStart < 0) xStart = 0; if (xEnd >= CHUNK_SIZE) xEnd = CHUNK_SIZE;
        int yStart = startPoint.y, yEnd = startPoint.y + width;
        if (yStart < 0) yStart = 0; if (yEnd >= CHUNK_SIZE) yEnd = CHUNK_SIZE;
        int zStart = startPoint.z, zEnd = startPoint.z + width;
        if (zStart < 0) xStart = 0; if (zEnd >= CHUNK_SIZE) zEnd = CHUNK_SIZE;
        switch (modelRotation)
        {
            default: return false;
            case 0: // fwd
                if (width != 1)
                {
                    for (int x = xStart; x < xEnd; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < CHUNK_SIZE; z++)
                            {
                                if (blocks[x, y, z] != null) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z < CHUNK_SIZE; z++)
                    {
                        if (blocks[startPoint.x, startPoint.y, z] != null) return false;
                    }
                }
                break;
            case 2: // right
                if (width != 1)
                {
                    for (int x = xStart; x < CHUNK_SIZE; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < zEnd; z++)
                            {
                                if (blocks[x, y, z] != null) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x < CHUNK_SIZE; x++)
                    {
                        if (blocks[x, startPoint.y, startPoint.z] != null) return false;
                    }
                }
                break;
            case 4: // back
                if (width != 1)
                {
                    for (int x = xStart; x < xEnd; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z >= 0; z--)
                            {
                                if (blocks[x, y, z] != null) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z >= 0; z--)
                    {
                        if (blocks[startPoint.x, startPoint.y, z] != null) return false;
                    }
                }
                break;
            case 6: // left
                if (width != 1)
                {
                    for (int x = xStart; x >= 0; x--)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < zEnd; z++)
                            {
                                if (blocks[x, y, z] != null) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x >= 0; x--)
                    {
                        if (blocks[x, startPoint.y, startPoint.z] != null) return false;
                    }
                }
                break;
        } // blocks check
        Block bk;
        switch (modelRotation) // blocks set
        {
            default: return false;
            case 0: // fwd
                if (width != 1)
                {
                    for (int x = xStart; x < xEnd; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < CHUNK_SIZE; z++)
                            {
                                bk = new GameObject().AddComponent<Block>();
                                bk.InitializeShapelessBlock(this, new ChunkPos(x, y, z), sender);
                                blocks[x, y, z] = bk;
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z < CHUNK_SIZE; z++)
                    {
                        bk = new GameObject().AddComponent<Block>();
                        bk.InitializeShapelessBlock(this, new ChunkPos(xStart, yStart, z), sender);
                        blocks[xStart, yStart, z] = bk;
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
            case 2: // right
                if (width != 1)
                {
                    for (int x = xStart; x < CHUNK_SIZE; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < zEnd; z++)
                            {
                                bk = new GameObject().AddComponent<Block>();
                                bk.InitializeShapelessBlock(this, new ChunkPos(x, y, z), sender);
                                blocks[x, y, z] = bk;
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x < CHUNK_SIZE; x++)
                    {
                        bk = new GameObject().AddComponent<Block>();
                        bk.InitializeShapelessBlock(this, new ChunkPos(x, yStart, zStart), sender);
                        blocks[x, yStart, zStart] = bk;
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
            case 4: // back
                if (width != 1)
                {
                    for (int x = xStart; x < xEnd; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z >= 0; z--)
                            {
                                bk = new GameObject().AddComponent<Block>();
                                bk.InitializeShapelessBlock(this, new ChunkPos(x, y, z), sender);
                                blocks[x, y, z] = bk;
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z >= 0; z--)
                    {
                        bk = new GameObject().AddComponent<Block>();
                        bk.InitializeShapelessBlock(this, new ChunkPos(xStart, yStart, z), sender);
                        blocks[xStart, yStart, z] = bk;
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
            case 6: // left
                if (width != 1)
                {
                    for (int x = xStart; x >= 0; x--)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < zEnd; z++)
                            {
                                bk = new GameObject().AddComponent<Block>();
                                bk.InitializeShapelessBlock(this, new ChunkPos(x, y, z), sender);
                                blocks[x, y, z] = bk;
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x >= 0; x--)
                    {
                        bk = new GameObject().AddComponent<Block>();
                        bk.InitializeShapelessBlock(this, new ChunkPos(x, yStart, zStart), sender);
                        blocks[x, yStart, zStart] = bk;
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
        }
        chunkUpdated = true;
        return true;
    }
    public void ClearBlocksList(List<Block> list, bool clearMainStructureField)
    {
        bool actions = false;
        foreach (Block b in list)
        {
            if (b != null)
            {
                if (clearMainStructureField) b.ResetMainStructure();
                // поле mainStructure чистится, чтобы блок не посылал SectionDeleted обратно структуре
                b.Annihilate();
                actions = true;
            }
        }
        if (actions) chunkUpdated = true;
    }

    public void DrawBorder()
    {
        LineRenderer lr = gameObject.GetComponent<LineRenderer>();
        if (lr == null)
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.sharedMaterial = Resources.Load<Material>("Materials/borderMaterial");
            lr.receiveShadows = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.positionCount = 4;
            lr.loop = true;

        }
        else lr.enabled = true;
        float qh = Block.QUAD_SIZE / 2f;
        float s = CHUNK_SIZE * Block.QUAD_SIZE - qh;
        float h = CHUNK_SIZE / 2f * Block.QUAD_SIZE - qh;
        gameObject.GetComponent<LineRenderer>().SetPositions(new Vector3[4] {
            new Vector3( -qh, h, -qh),
            new Vector3( -qh, h, s),
            new Vector3(s, h, s),
            new Vector3(s, h, -qh)
        });
        borderDrawn = true;
    }
    public void HideBorderLine()
    {
        LineRenderer lr = gameObject.GetComponent<LineRenderer>();
        if (lr != null) lr.enabled = false;
    }

    #region save-load system
    public void SaveChunkData(System.IO.FileStream fs)
    {
        fs.WriteByte(CHUNK_SIZE);
        var saveableBlocks = new List<Block>();
        foreach (Block b in blocks)
        {
            if (b == null || b.type == BlockType.Shapeless) continue;
            else
            {
                saveableBlocks.Add(b);
            }
        }
        int count = saveableBlocks.Count;
        fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        if (count > 0)
        {
            foreach (Block b in saveableBlocks) b.Save(fs);
        }

        fs.Write(System.BitConverter.GetBytes(lifePower), 0, 4);

        int roofsCount = 0;
        foreach (Roof r in roofs)
        {
            if (r != null) roofsCount++;
        }
        fs.Write(System.BitConverter.GetBytes(roofsCount), 0, 4);
        if (roofsCount > 0)
        {
            byte zero = 0, one = 1;
            for (byte i = 0; i < CHUNK_SIZE; i++)
            {
                for (byte j = 0; j < CHUNK_SIZE; j++)
                {
                    Roof r = roofs[i, j];
                    if (r == null) continue;
                    else
                    {
                        fs.WriteByte(i); //0
                        fs.WriteByte(j); // 1
                        fs.WriteByte(r.artificial ? one : zero); // 2
                        fs.WriteByte(r.peak ? one : zero); // 3
                        fs.WriteByte((byte)(r.transform.rotation.eulerAngles.y / 90)); // 4
                        fs.WriteByte(byte.Parse(r.name[2].ToString())); // 5
                    }
                }
            }
        }
    }

    public void LoadChunkData(System.IO.FileStream fs)
    {
        if (blocks != null) ClearChunk();        
        CHUNK_SIZE = (byte)fs.ReadByte();
        Prepare();

        var data = new byte[4];
        fs.Read(data, 0, 4);
        surfaceBlocks = new List<SurfaceBlock>();
        int blocksCount = System.BitConverter.ToInt32(data, 0);

        if (blocksCount > 0) {
            var loadedBlocks = new Block[blocksCount];
            BlockType type;
            ChunkPos pos;
            int materialID;
            data = new byte[8];
            for (int i = 0; i < blocksCount; i++)
            {
                fs.Read(data, 0, data.Length);
                type = (BlockType)data[0];
                pos = new ChunkPos(data[1], data[2], data[3]);
                materialID = System.BitConverter.ToInt32(data, 4);
                switch (type)
                {
                    case BlockType.Cube:
                        {
                            CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
                            blocks[pos.x, pos.y, pos.z] = cb;
                            cb.InitializeCubeBlock(this, pos, materialID, false);
                            cb.LoadCubeBlockData(fs);
                            loadedBlocks[i] = cb;
                            break;
                        }
                    case BlockType.Surface:
                        {
                            SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
                            blocks[pos.x, pos.y, pos.z] = sb;
                            sb.InitializeSurfaceBlock(this, pos, materialID);
                            sb.LoadSurfaceBlockData(fs);
                            loadedBlocks[i] = sb;
                            surfaceBlocks.Add(sb);
                            break;
                        }
                    case BlockType.Cave:
                        {
                            CaveBlock cvb = new GameObject().AddComponent<CaveBlock>();
                            blocks[pos.x, pos.y, pos.z] = cvb;
                            var cdata = new byte[4];
                            fs.Read(cdata, 0, 4);
                            int ceilingMaterial = System.BitConverter.ToInt32(cdata, 0);
                            cvb.InitializeCaveBlock(this, pos, ceilingMaterial, materialID);
                            cvb.LoadSurfaceBlockData(fs);
                            loadedBlocks[i] = cvb;
                            surfaceBlocks.Add(cvb);
                            break;
                        }

                    default: continue;
                }
            }
            bool corruptedData = false;
            foreach (Block b in loadedBlocks) {
                if (b == null & !corruptedData)
                {
                    if (UIController.current != null) UIController.current.MakeAnnouncement("error desu : block hasn't loaded");
                    corruptedData = true;
                    continue;
                }
                b.SetVisibilityMask(GetVisibilityMask(b.pos.x, b.pos.y, b.pos.z));
            } // ужасное решение
            ChunkLightmapFullRecalculation();
        }
        if (surfaceBlocks.Count > 0)
        {
            foreach (SurfaceBlock sb in surfaceBlocks)
            {
                if (sb.cellsStatus != 0)
                {
                    foreach (Structure s in sb.surfaceObjects)
                    {
                        if (s.isBasement)
                        {
                            BlockRendererController brc = s.transform.GetChild(0).GetComponent<BlockRendererController>();
                            if (brc != null) {
                                ChunkPos cpos = s.basement.pos;
                                brc.SetVisibilityMask(GetVisibilityMask(cpos.x, cpos.y, cpos.z));
                            }
                        }
                    }
                }
            }
        }

        data = new byte[8];
        fs.Read(data, 0, 8);
        lifePower = System.BitConverter.ToSingle(data, 0);

        if (roofs != null) 
        {
            foreach (Roof r in roofs)
            {
                if (r != null) Destroy(r.gameObject);
            }
        }
        int roofsCount = System.BitConverter.ToInt32(data, 4);
        if (roofsCount > 0)
        {
            Roof r;
            int roofSerializerLength = 6;
            data = new byte[roofSerializerLength];
            for (int i = 0; i < roofsCount; i++)
            {
                fs.Read(data, 0, roofSerializerLength);
                bool peak = data[3] == 1;
                bool artificial = data[2] == 1;
                r = PoolMaster.GetRooftop(peak, artificial, data[5]).AddComponent<Roof>();
                r.peak = peak;
                r.artificial = artificial;
                r.transform.parent = roofObjectsHolder.transform;
                byte x = data[0],
                    y = data[1];
                r.transform.localPosition = new Vector3(x * Block.QUAD_SIZE, (CHUNK_SIZE - 0.5f) * Block.QUAD_SIZE, y * Block.QUAD_SIZE);
                r.transform.localRotation = Quaternion.Euler(0, data[4] * 90, 0);
                roofs[x, y] = r;
            }
        }
        if (borderDrawn) DrawBorder();
    }
    #endregion

    void OnGUI()
    { //test
        GUI.Label(new Rect(0, 32, 64, 32), lifePower.ToString());
    }

    private void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        //foreach (Block b in blocks)
        //{
        //    if (b != null) b.Annihilate();
        // }
        //surfaceBlocks.Clear();
        FollowingCamera.main.cameraChangedEvent -= CameraUpdate;
    }
}
