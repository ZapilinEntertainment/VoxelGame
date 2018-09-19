using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

[System.Serializable]
public sealed class ChunkSerializer
{
    public List<BlockSerializer> blocksData;
    public float lifepower;
    public byte chunkSize;
}

public sealed class Chunk : MonoBehaviour
{
    public Block[,,] blocks { get; private set; }
    public List<SurfaceBlock> surfaceBlocks { get; private set; }
    public byte prevBitmask = 63;
    public float lifePower = 0;
    public static byte CHUNK_SIZE { get; private set; }  
    private bool allGrasslandsCreated = false;
    public byte[,,] lightMap { get; private set; }
    float LIGHT_DECREASE_PER_BLOCK = 1 - 1f / (PoolMaster.MAX_MATERIAL_LIGHT_DIVISIONS + 1);
    public delegate void ChunkUpdateHandler(ChunkPos pos);
    public event ChunkUpdateHandler ChunkUpdateEvent;

    public void Awake()
    {
        surfaceBlocks = new List<SurfaceBlock>();
        lightMap = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        FollowingCamera.main.cameraChangedEvent += CameraUpdate;
    }

    #region updating
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
                        if (sb.material_id == ResourceType.DIRT_ID & sb.grassland == null) dirt_for_grassland.Add(sb);
                    }
                    if (dirt_for_grassland.Count > 0)
                    {
                        int pos = (int)(Random.value * (dirt_for_grassland.Count - 1));
                        SurfaceBlock sb = dirt_for_grassland[pos];
                            
                            Grassland.CreateOn(sb);
                            int lifeTransfer = (int)(GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
                            if (lifePower > lifeTransfer) { sb.grassland.AddLifepower(lifeTransfer); lifePower -= lifeTransfer; }
                            else { sb.grassland.AddLifepower((int)lifePower); lifePower = 0; }
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

    IEnumerator CullingUpdate()
    {
        Vector3 cpos = transform.InverseTransformPoint(FollowingCamera.camPos);
        Vector3 v = Vector3.one * (-1);
        int size = blocks.GetLength(0);
        if (cpos.x > 0) { if (cpos.x > size) v.x = 1; else v.x = 0; }
        if (cpos.y > 0) { if (cpos.y > size) v.y = 1; else v.y = 0; }
        if (cpos.z > 0) { if (cpos.z > size) v.z = 1; else v.z = 0; }
        //print (v);
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
                if (bx.type == BlockType.Cube & !bx.isTransparent) vmask -= 1;
            }
            bx = GetBlock(x + 1, y, z);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube & !bx.isTransparent) vmask -= 2;
            }
            bx = GetBlock(x , y, z - 1);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube & !bx.isTransparent) vmask -= 4;
            }
            bx = GetBlock(x - 1, y, z);
            if (bx != null)
            {
                if (bx.type == BlockType.Cube & !bx.isTransparent) vmask -= 8;
            }
            // up and down
            bx = GetBlock(x , y + 1, z);
            if (bx == null || bx.isTransparent) vmask += 16;

            bx = GetBlock(x, y - 1, z);
            if (bx == null ) vmask += 32;
            else
            {
                if (bx.type == BlockType.Surface)
                {
                    SurfaceBlock sb = bx as SurfaceBlock;
                    if (!sb.haveSupportingStructure) vmask += 32;
                }
                else
                {
                    if (bx.isTransparent) vmask += 32;
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
        if (ChunkUpdateEvent != null) ChunkUpdateEvent(new ChunkPos(x,y,z));
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
        x = 0; y = 0;z = 0;
       for (x = 0; x < CHUNK_SIZE; x++)
        {
            for (z = 0; z < CHUNK_SIZE; z++)
            {
                for (y = 0; y < CHUNK_SIZE; y++)
                {
                    b = blocks[x, y, z];
                    if (b == null) lightMap[x, y, z] = DOWN_LIGHT ;
                    else
                    {
                        if (b.type == BlockType.Cave) {
                            if (!(b as CaveBlock).haveSurface)
                            {
                                lightMap[x, y, z] = DOWN_LIGHT;
                                break;
                            }
                            else break;
                        }
                        else {
                            if (b.type == BlockType.Shapeless) { lightMap[x, y, z] = DOWN_LIGHT; }
                            else  break; 
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
                decreasedVal = (byte)(lightMap[x,y,CHUNK_SIZE - 1] * LIGHT_DECREASE_PER_BLOCK);
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
                decreasedVal = (byte)(lightMap[CHUNK_SIZE - 1, y,z] * LIGHT_DECREASE_PER_BLOCK);
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
                decreasedVal = (byte)(lightMap[0,y,z] * LIGHT_DECREASE_PER_BLOCK);
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
    public void InitializeBlocksArray()
    {
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    }

    public static void SetChunkSize(byte x)
    {
        CHUNK_SIZE = x;
        //функция ресайза при наличии уже существующего массива блоков?
    }

    public void SetChunk(int[,,] newData)
    {
        int size = newData.GetLength(0);
        CHUNK_SIZE = (byte)size;
        if (blocks != null) ClearChunk();
        else blocks = new Block[size, size, size];
        
        
        if (CHUNK_SIZE < 3) CHUNK_SIZE = 16;
        GameMaster.layerCutHeight = CHUNK_SIZE;
        GameMaster.prevCutHeight = GameMaster.layerCutHeight;
                
        surfaceBlocks = new List<SurfaceBlock>();
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                bool noBlockAbove = false;
                for (int y = size - 1; y >= 0; y--)
                {
                    if (newData[x, y, z] == 0) {
                        noBlockAbove = true;
                        continue;
                    }
                    else
                    {
                        CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
                        blocks[x, y, z] = cb;
                        cb.InitializeCubeBlock(this, new ChunkPos(x, y, z), newData[x, y, z], true);
                        if (noBlockAbove) {
                            if (y < size - 2 && newData[x, y + 2, z] != 0)
                            {
                                CaveBlock cvb = new GameObject().AddComponent<CaveBlock>();
                                blocks[x, y + 1, z] = cvb;
                                cvb.InitializeCaveBlock(this, new ChunkPos(x, y + 1, z), newData[x, y + 2, z], newData[x, y, z]);
                                surfaceBlocks.Add(cvb);
                                GameMaster.geologyModule.SpreadMinerals(cvb); // <- замена на пещерные структуры
                            }
                            else
                            {
                                SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
                                blocks[x, y + 1, z] = sb;
                                sb.InitializeSurfaceBlock(this, new ChunkPos(x, y + 1, z), newData[x, y, z]);
                                surfaceBlocks.Add(sb);
                                GameMaster.geologyModule.SpreadMinerals(sb);
                            }
                        }
                        noBlockAbove = false;
                    }
                }
            }
        }
        ChunkLightmapFullRecalculation();
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                int y = CHUNK_SIZE - 1;
                    for (; y > GameMaster.layerCutHeight; y--)
                    {
                        if (blocks[x, y, z] != null) blocks[x, y, z].SetVisibilityMask(0);
                    }
                for (; y > -1; y--)
                {
                    if (blocks[x, y, z] != null) blocks[x, y, z].SetVisibilityMask(GetVisibilityMask(x, y, z));
                }
            }
        }        
        FollowingCamera.main.WeNeedUpdate();
    }

    public Block GetBlock(ChunkPos cpos) { return GetBlock(cpos.x, cpos.y, cpos.z); }
    public Block GetBlock(int x, int y, int z)
    {
        int size = blocks.GetLength(0);
        if (x < 0 || x > size - 1 || y < 0 || y > size - 1 || z < 0 || z > size - 1) return null;
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
        if (f_type == BlockType.Cave)
        {
            float pts = CalculateSupportPoints(x, y, z);
            if (pts < 1) f_type = BlockType.Surface;
        }
        if (GetBlock(x, y, z) != null) return ReplaceBlock(f_pos, f_type, i_floorMaterialID, i_ceilingMaterialID, i_naturalGeneration);
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
                    if (cb.isTransparent == false) influenceMask = 0; // закрывает собой все соседние стенки
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
                        if (x < CHUNK_SIZE - 1 && lightMap[x + 1, y, z] > light) light = (byte)(lightMap[x + 1, y, z ] * LIGHT_DECREASE_PER_BLOCK);
                        if (z > 0 && lightMap[x, y, z - 1] > light) light = (byte)(lightMap[x, y, z - 1] * LIGHT_DECREASE_PER_BLOCK);
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                        if (y < CHUNK_SIZE - 1 && lightMap[x, y + 1, z] > light) light = lightMap[x, y + 1, z];
                        if (y > 0 && lightMap[x, y - 1, z] > light) light = (byte)(lightMap[x, y - 1, z ] * LIGHT_DECREASE_PER_BLOCK);
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
                    b = GetBlock(x, y - 1, z);
                    if (b == null)
                    {
                        return null;
                    }
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
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z ] * LIGHT_DECREASE_PER_BLOCK);
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
                    float spoints = CalculateSupportPoints(x, y, z);
                    if (spoints < 1) goto case BlockType.Surface;

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
                        if (x > 0 && lightMap[x - 1, y, z] > light) light = (byte)(lightMap[x - 1, y, z ] * LIGHT_DECREASE_PER_BLOCK);
                        if (!caveb.haveSurface & y > 0 && lightMap[x, y - 1, z] > light) light = (byte)(lightMap[x, y - 1, z ] * LIGHT_DECREASE_PER_BLOCK);
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
                AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, i_ceilingMaterialID, i_ceilingMaterialID, i_naturalGeneration);
            }
        }
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        return b;
    }

    public Block ReplaceBlock(ChunkPos f_pos, BlockType f_newType, int material1_id, bool naturalGeneration)
    {
        return ReplaceBlock(f_pos, f_newType, material1_id, material1_id, naturalGeneration);
    }
    public Block ReplaceBlock(ChunkPos f_pos, BlockType f_newType, int material1_id, int material2_id, bool naturalGeneration)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (GetBlock(x, y, z) == null) return null;
        Block originalBlock = GetBlock(x, y, z);
        if (originalBlock == null) return AddBlock(f_pos, f_newType, material1_id, material2_id, naturalGeneration);
        if (originalBlock.type == f_newType)
        {
            originalBlock.ReplaceMaterial(material1_id);
            return originalBlock;
        }
        else
        {
            if (originalBlock.indestructible)
            {
                if ((originalBlock.type == BlockType.Surface || originalBlock.type == BlockType.Cave) && f_newType != BlockType.Surface && f_newType != BlockType.Cave) return originalBlock;
            }
        }
        Block b = null;
        byte influenceMask = 63;
        bool calculateUpperBlock = false;
        switch (f_newType)
        {

            case BlockType.Shapeless:
                {
                    b = new GameObject().AddComponent<Block>();
                    b.InitializeShapelessBlock(this, f_pos, null);
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
                    sb.InitializeSurfaceBlock(this, f_pos, material1_id);
                    surfaceBlocks.Add(sb);
                    b = sb;
                    blocks[x, y, z] = sb;
                    influenceMask = 31;
                    if (originalBlock.type == BlockType.Cave)
                    {
                        CaveBlock originalSurface = originalBlock as CaveBlock;
                        foreach (Structure s in originalSurface.surfaceObjects)
                        {
                            if (s == null) continue;
                            s.SetBasement(sb, new PixelPosByte(s.innerPosition.x, s.innerPosition.z));
                        }
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
                    cb.InitializeCubeBlock(this, f_pos, material1_id, naturalGeneration);
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
                    cvb.InitializeCaveBlock(this, f_pos, material2_id, material1_id);
                    surfaceBlocks.Add(cvb);
                    blocks[x, y, z] = cvb;
                    b = cvb;
                    if (cvb.haveSurface) influenceMask = 15; else influenceMask = 47;

                    if (originalBlock.type == BlockType.Surface)
                    {
                        SurfaceBlock originalSurface = originalBlock as SurfaceBlock;
                        foreach (Structure s in originalSurface.surfaceObjects)
                        {
                            if (s == null) continue;
                            s.SetBasement(cvb, new PixelPosByte(s.innerPosition.x, s.innerPosition.z));
                        }
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
        blocks[x, y, z].MakeIndestructible(originalBlock.indestructible);
        originalBlock.Annihilate();
        originalBlock = null;

        b.SetVisibilityMask(GetVisibilityMask(x, y, z));
        b.SetRenderBitmask(prevBitmask);
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        if (calculateUpperBlock)
        {
            if (GetBlock(x, y + 1, z) == null)
            {
                if (GetBlock(x, y + 2, z) != null)
                {
                    AddBlock(new ChunkPos(x, y + 1, z), BlockType.Cave, material2_id, blocks[x, y + 2, z].material_id, naturalGeneration);
                }
                else AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, material2_id, naturalGeneration);
            }
        }
        
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

    public void BlockByStructure(byte x, byte y, byte z, Structure s)
    {
        if (x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE || x < 0 || y < 0 || z < 0 || s == null) return;
        Block b = GetBlock(x, y, z);
        if (b != null) { ReplaceBlock(new ChunkPos(x, y, z), BlockType.Shapeless, 0, false); }
        else blocks[x, y, z] = new GameObject().AddComponent<Block>();
        blocks[x, y, z].InitializeShapelessBlock(this, new ChunkPos(x, y, z), s);
    }

    public void DeleteBlock(ChunkPos pos)
    {
        // в сиквеле стоит пересмотреть всю иерархию классов ><
        //12.06 нет, я так не думаю
        Block b = GetBlock(pos);
        if (b == null || b.indestructible == true) return;
        int x = pos.x, y = pos.y, z = pos.z;
        bool neighboursSupportCalculation = (b.type == BlockType.Cube | b.type == BlockType.Cave) , makeSurface = false;
        switch (b.type)
        {
            case BlockType.Cube:
                {
                    Block upperBlock = GetBlock(x, y + 1, z);
                    if (upperBlock != null)
                    {
                        if (upperBlock.type == BlockType.Surface) DeleteBlock(new ChunkPos(x, y + 1, z));
                        else
                        {
                            if (upperBlock.type == BlockType.Cave) (upperBlock as CaveBlock).DestroySurface();
                        }
                    }
                    else
                    {
                        Block lowerBlock = GetBlock(x, y - 1, z);
                        if (lowerBlock != null)
                        {
                            SurfaceBlock surf = lowerBlock as SurfaceBlock;
                            if (surf != null)
                            {
                                if (surf.cellsStatus != 0)
                                {
                                    foreach (Structure s in surf.surfaceObjects)
                                    {
                                        if (s == null) continue;
                                        if (s.isBasement)
                                        {
                                            makeSurface = true;
                                            break;
                                        }
                                    }
                                }
                                if (surf.type == BlockType.Cave) makeSurface = true;
                            }
                            else
                            {
                                if (lowerBlock.type == BlockType.Cube) makeSurface = true;
                            }
                        }
                    }
                }
                break;
            case BlockType.Surface:
            case BlockType.Cave:
                SurfaceBlock sb = b as SurfaceBlock;
                if (sb.grassland != null) sb.grassland.Annihilation();
                sb.ClearSurface(false); // false так как все равно удаляется
                break;
        }
        blocks[x, y, z].Annihilate();
        blocks[x, y, z] = null;
        ApplyVisibleInfluenceMask(x, y, z, 63);

        if (neighboursSupportCalculation)
        {
            Block sideBlock = GetBlock(x, y, z + 1);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x, sideBlock.pos.y, sideBlock.pos.z);
                if (supportPoints < 1) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
            sideBlock = GetBlock(x + 1, y, z);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x, sideBlock.pos.y, sideBlock.pos.z);
                if (supportPoints < 1) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
            sideBlock = GetBlock(x, y, z - 1);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x, sideBlock.pos.y, sideBlock.pos.z);
                if (supportPoints < 1) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
            sideBlock = GetBlock(x - 1, y, z);
            if (sideBlock != null && sideBlock.type == BlockType.Cave)
            {
                float supportPoints = CalculateSupportPoints(sideBlock.pos.x, sideBlock.pos.y, sideBlock.pos.z);
                if (supportPoints < 1) ReplaceBlock(sideBlock.pos, BlockType.Surface, sideBlock.material_id, false);
            }
        }
        if (makeSurface) AddBlock(pos, BlockType.Surface, b.material_id, false);
        ChunkLightmapFullRecalculation();
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
                    blocks[i, j, k] = null;
                }
            }
        }
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        surfaceBlocks.Clear();
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
        float lifepowerPerBlock = GameMaster.LIFEPOWER_PER_BLOCK;
        int dirtID = ResourceType.DIRT_ID;
       foreach (SurfaceBlock sb in surfaceBlocks)
        {
            if (sb.material_id == dirtID)
            {
                Grassland gl = Grassland.CreateOn(sb);                
                if (gl != null)
                {
                    float lval = lifepowerPerBlock * (( 1 -(sb.transform.position - lifeSourcePos).magnitude / CHUNK_SIZE) * 0.5f + 0.5f);
                    gl.AddLifepowerAndCalculate((int)lval);
                }
            }
        }
    }   

    public void AddLifePower(int count) { lifePower += count;  }
    public int TakeLifePower(int count)
    {
        if (count < 0) return 0;
        float lifeTransfer = count;
        if (lifeTransfer > lifePower) { if (lifePower >= 0) lifeTransfer = lifePower; else lifeTransfer = 0; }
        lifePower -= lifeTransfer;
        return (int)lifeTransfer;
    }
    public int TakeLifePowerWithForce(int count)
    {
        if (count < 0) return 0;
        lifePower -= count;
        return count;
    }

    #endregion    

    public void LayersCut()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                int y = CHUNK_SIZE - 1;
                if (GameMaster.layerCutHeight != CHUNK_SIZE)
                {
                    for (; y > GameMaster.layerCutHeight; y--)
                    {
                        if (blocks[x, y, z] != null) blocks[x, y, z].SetVisibilityMask(0);
                    }
                }
                for (; y > -1; y--)
                {
                    if (blocks[x, y, z] != null) blocks[x, y, z].SetVisibilityMask(GetVisibilityMask(x, y, z));
                }
            }
        }
    }

    public bool BlockShipCorridorIfPossible(int xpos, int ypos, bool xyAxis, int width, Structure sender, ref List<Block> dependentBlocksList)
    {
        int xStart = xpos; int xEnd = xStart + width - 1;
        if (xStart < 0) xStart = 0; if (xEnd >= CHUNK_SIZE) xEnd = CHUNK_SIZE - 1;
        int yStart = ypos; int yEnd = yStart + width - 1;
        if (yStart < 0) yStart = 0; if (yEnd >= CHUNK_SIZE) yEnd = CHUNK_SIZE - 1;
        if (xyAxis)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if (blocks[x, y, z] != null) return false;
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
        return true;
    }
    public void ClearBlocksList( List<Block> list, bool clearMainStructureField)
    {
        foreach (Block b in list)
        {
            if (b != null)
            {
                if (clearMainStructureField) b.mainStructure = null;
                b.Annihilate();
            }
        }
    }

    #region save-load system
    public ChunkSerializer SaveChunkData()
    {
        ChunkSerializer cs = new ChunkSerializer();
        cs.blocksData = new List<BlockSerializer>();
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (blocks[x, y, z] == null) continue;
                    cs.blocksData.Add(blocks[x, y, z].Save());
                }
            }
        }
        cs.chunkSize = CHUNK_SIZE;
        cs.lifepower = lifePower;
        return cs;
    }

    public void LoadChunkData(ChunkSerializer cs)
    {
        if (cs == null) print("chunk serialization failed!");
        CHUNK_SIZE = cs.chunkSize;
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        foreach (BlockSerializer bs in cs.blocksData)
        {
            Block b = AddBlock(bs.pos, bs.type, bs.material_id, true);
            b.Load(bs);
        }
        lifePower = cs.lifepower;
    }
    #endregion

    void OnGUI()
    { //test
        GUI.Label(new Rect(0, 32, 64, 32), lifePower.ToString());
    }

    private void OnDestroy()
    {
        if (GameMaster.applicationStopWorking) return;
        foreach (Block b in blocks)
        {
            if (b != null) b.Annihilate();
        }
        surfaceBlocks.Clear();
        FollowingCamera.main.cameraChangedEvent -= CameraUpdate;
    }
}
