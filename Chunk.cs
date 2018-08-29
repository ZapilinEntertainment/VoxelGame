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
    public float lifepower, lifepowerTimer;
    public byte chunkSize;
}

public sealed class Chunk : MonoBehaviour
{
    Block[,,] blocks;
    public List<SurfaceBlock> surfaceBlocks { get; private set; }
    public byte prevBitmask = 63;
    public float lifePower = 0;
    float lifepower_timer = 0;
    public static byte CHUNK_SIZE { get; private set; }
    GameObject cave_pref;
    public List<Component> chunkUpdateSubscribers;
    public bool[,] sideBlockingMap { get; private set; }

    public void Awake()
    {
        surfaceBlocks = new List<SurfaceBlock>();
        cave_pref = Resources.Load<GameObject>("Prefs/CaveBlock_pref");
        chunkUpdateSubscribers = new List<Component>();
        sideBlockingMap = new bool[CHUNK_SIZE, 4];
        for (int a = 0; a < CHUNK_SIZE; a++)
        {
            for (int b = 0; b < 4; b++)
            {
                sideBlockingMap[a, b] = false;
            }
        }

        GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
    }

    void Start()
    {
        if (Camera.main != null) CullingUpdate(Camera.main.transform);
    }

    public void CameraUpdate(Transform t)
    {
        CullingUpdate(t);
    }

    void Update()
    {
        lifepower_timer -= Time.deltaTime * GameMaster.gameSpeed;
        if (lifepower_timer <= 0)
        {
            if (surfaceBlocks.Count > 0)
            {
                float grasslandLifepowerChanges = 0;
                if (lifePower > 0)
                {
                    // creating new grasslands - можно было и получше написать!
                    List<SurfaceBlock> dirt_for_grassland = new List<SurfaceBlock>();
                    foreach (SurfaceBlock sb in surfaceBlocks)
                    {
                        if (sb.material_id == ResourceType.DIRT_ID && sb.grassland == null) dirt_for_grassland.Add(sb);
                    }
                    SurfaceBlock b = null;
                    while (b == null && dirt_for_grassland.Count > 0)
                    {
                        int pos = (int)(Random.value * (dirt_for_grassland.Count - 1));
                        b = dirt_for_grassland[pos];
                        if (b != null)
                        {
                            {
                                int x = b.pos.x; int z = b.pos.z;
                                List<SurfaceBlock> candidats = new List<SurfaceBlock>();
                                bool rightSide = false, leftSide = false;
                                SurfaceBlock candidateSurface = null;
                                if (x + 1 < CHUNK_SIZE)
                                {
                                    candidateSurface = GetSurfaceBlock(x + 1, z);
                                    if (candidateSurface != null)
                                    {
                                        candidats.Add(candidateSurface);
                                        rightSide = true;
                                    }
                                }
                                if (x - 1 >= 0)
                                {
                                    candidateSurface = GetSurfaceBlock(x - 1, z);
                                    if (candidateSurface != null)
                                    {
                                        candidats.Add(candidateSurface);
                                        rightSide = true;
                                    }
                                }
                                if (z + 1 < CHUNK_SIZE)
                                {
                                    candidateSurface = GetSurfaceBlock(x, z + 1);
                                    if (candidateSurface != null) candidats.Add(candidateSurface);
                                    if (rightSide)
                                    {
                                        candidateSurface = GetSurfaceBlock(x + 1, z + 1);
                                        if (candidateSurface != null) candidats.Add(candidateSurface);
                                    }
                                    if (leftSide)
                                    {
                                        candidateSurface = GetSurfaceBlock(x - 1, z + 1);
                                        if (candidateSurface != null) candidats.Add(candidateSurface);
                                    }
                                }
                                if (z - 1 >= 0)
                                {
                                    candidateSurface = GetSurfaceBlock(x, z - 1);
                                    if (candidateSurface != null) candidats.Add(candidateSurface);
                                    if (rightSide)
                                    {
                                        candidateSurface = GetSurfaceBlock(x + 1, z - 1);
                                        if (candidateSurface != null) candidats.Add(candidateSurface);
                                    }
                                    if (leftSide)
                                    {
                                        candidateSurface = GetSurfaceBlock(x - 1, z - 1);
                                        if (candidateSurface != null) candidats.Add(candidateSurface);
                                    }
                                }
                                foreach (SurfaceBlock n in candidats)
                                {
                                    if (n == null | n.grassland != null) continue;
                                    if (n.material_id == ResourceType.DIRT_ID & !dirt_for_grassland.Contains(n) & Mathf.Abs(b.pos.y - n.pos.y) < 2) dirt_for_grassland.Add(n);
                                }
                            }
                            Grassland.Create(b);
                            int lifeTransfer = (int)(GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
                            if (lifePower > lifeTransfer) { b.grassland.AddLifepower(lifeTransfer); lifePower -= lifeTransfer; }
                            else { b.grassland.AddLifepower((int)lifePower); lifePower = 0; }
                        }
                        else dirt_for_grassland.RemoveAt(pos);
                    }
                    grasslandLifepowerChanges = lifePower;
                }
                if (lifePower < -100)
                { // LifePower decreases
                    grasslandLifepowerChanges = -1 * lifePower;
                }
                lifePower = Grassland.GrasslandUpdate(grasslandLifepowerChanges);
            }
            lifepower_timer = GameMaster.LIFEPOWER_TICK;
        }
    }

    public byte GetVisibilityMask(int i, int j, int k)
    {
        byte vmask = 63;
        if (j > GameMaster.layerCutHeight) vmask = 0;
        else
        {
            Block bx = GetBlock(i + 1, j, k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 61;
            bx = GetBlock(i - 1, j, k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 55;
            bx = GetBlock(i, j + 1, k); if (bx != null && !bx.isTransparent && bx.type != BlockType.Shapeless && (bx.pos.y != GameMaster.layerCutHeight + 1)) vmask &= 47;
            bx = GetBlock(i, j - 1, k); if (bx != null && !bx.isTransparent && (bx.type == BlockType.Cube | (bx.type == BlockType.Cave && (bx as CaveBlock).haveSurface ))) vmask &= 31;
            bx = GetBlock(i, j, k + 1); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 62;
            bx = GetBlock(i, j, k - 1); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 59;
        }
        return vmask;
    }
    public void ApplyVisibleInfluenceMask(int x, int y, int z, byte mask)
    {
        Block b = GetBlock(x, y, z + 1); if (b != null) b.ChangeVisibilityMask(2, ((mask & 1) != 0));
        b = GetBlock(x + 1, y, z); if (b != null) b.ChangeVisibilityMask(3, ((mask & 2) != 0));
        b = GetBlock(x, y, z - 1); if (b != null) b.ChangeVisibilityMask(0, ((mask & 4) != 0));
        b = GetBlock(x - 1, y, z); if (b != null) b.ChangeVisibilityMask(1, ((mask & 8) != 0));
        b = GetBlock(x, y + 1, z); if (b != null) b.ChangeVisibilityMask(5, ((mask & 16) != 0));
        b = GetBlock(x, y - 1, z); if (b != null) b.ChangeVisibilityMask(4, ((mask & 32) != 0));
        BroadcastChunkUpdate(new ChunkPos(x, y, z));
    }

    public Block AddBlock(ChunkPos f_pos, BlockType f_type, int material1_id, bool naturalGeneration)
    {
        return AddBlock(f_pos, f_type, material1_id, material1_id, naturalGeneration);
    }
    public Block AddBlock(ChunkPos f_pos, BlockType f_type, int i_floorMaterialID, int i_ceilingMaterialID, bool i_naturalGeneration)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (f_type == BlockType.Cave)
        {
            float pts = CalculateSupportPoints(x, y, z);
            if (pts < 1) f_type = BlockType.Surface;
        }
        if (GetBlock(x, y, z) != null) return ReplaceBlock(f_pos, f_type, i_floorMaterialID, i_ceilingMaterialID, i_naturalGeneration);
        GameObject g = null;
        CubeBlock cb = null;
        Block b = null;
        byte visMask = GetVisibilityMask(x, y, z), influenceMask = 63; // видимость объекта, видимость стенок соседних объектов
        bool calculateUpperBlock = false;

        switch (f_type)
        {
            case BlockType.Cube:
                g = new GameObject();
                cb = g.AddComponent<CubeBlock>();
                cb.BlockSet(this, f_pos, i_floorMaterialID, i_naturalGeneration);
                blocks[x, y, z] = cb;
                if (cb.isTransparent == false) influenceMask = 0; else influenceMask = 1; // закрывает собой все соседние стенки
                calculateUpperBlock = true;
                break;
            case BlockType.Shapeless:
                g = new GameObject();
                blocks[x, y, z] = g.AddComponent<Block>();
                blocks[x, y, z].BlockSet(this, f_pos, i_floorMaterialID);
                break;
            case BlockType.Surface:
                b = GetBlock(x, y-1,z);
                if (b == null)
                {
                    return null;
                }
                if ( b.type != BlockType.Surface) influenceMask = 31;
                else influenceMask = 63;

                g = new GameObject();
                SurfaceBlock sb = g.AddComponent<SurfaceBlock>();
                sb.SurfaceBlockSet(this, f_pos, i_floorMaterialID);
                blocks[x, y, z] = sb;
                surfaceBlocks.Add(sb);
                
                break;
            case BlockType.Cave:
                float spoints = CalculateSupportPoints(x, y, z);
                if (spoints < 1) goto case BlockType.Surface;

                Block lowerBlock = blocks[x, y - 1, z];
                if (lowerBlock == null) i_floorMaterialID = -1;
                g = Instantiate(cave_pref);
                CaveBlock caveb = g.GetComponent<CaveBlock>();
                caveb.CaveBlockSet(this, f_pos, i_ceilingMaterialID, i_floorMaterialID);
                blocks[x, y, z] = caveb;
                if (lowerBlock.type == BlockType.Surface) // снизу структура с isBasement == true ?
                {
                    influenceMask = 47; // все грани, кроме верхней
                }
                else
                {
                    if (caveb.haveSurface) influenceMask = 15; else influenceMask = 47;
                    calculateUpperBlock = true;
                }
                surfaceBlocks.Add(caveb);
                break;
        }
        b = blocks[x, y, z];
        b.SetVisibilityMask(visMask);
        b.SetRenderBitmask(prevBitmask);
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        if (calculateUpperBlock)
        {
            if (GetBlock(x, y + 1, z) == null)
            {
                if (GetBlock(x, y + 2, z) != null)
                {
                    AddBlock(new ChunkPos(x, y + 1, z), BlockType.Cave, i_ceilingMaterialID, blocks[x, y + 2, z].material_id, i_naturalGeneration);
                }
                else AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, i_ceilingMaterialID, i_ceilingMaterialID, i_naturalGeneration);
            }
        }
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
        if (f_newType == BlockType.Cave)
        {
            float pts = CalculateSupportPoints(x, y, z);
            if (pts < 1) f_newType = BlockType.Surface;
        }
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
                b = new GameObject().AddComponent<Block>();
                b.ShapelessBlockSet(this, f_pos, null);
                blocks[x, y, z] = b;
                break;

            case BlockType.Surface:
                SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
                sb.SurfaceBlockSet(this, f_pos, material1_id);
                surfaceBlocks.Add(sb);
                b = sb;
                blocks[x, y, z] = sb;
                Block blockBelow = GetBlock(x, y - 1, z);
                if (blockBelow != null && (blockBelow.type != BlockType.Surface & blockBelow.type != BlockType.Cave)) influenceMask = 31;
                else influenceMask = 63;
                if (originalBlock.type == BlockType.Cave)
                {
                    CaveBlock originalSurface = originalBlock as CaveBlock;
                    foreach (Structure s in originalSurface.surfaceObjects)
                    {
                        if (s == null) continue;
                        s.SetBasement(sb, new PixelPosByte(s.innerPosition.x, s.innerPosition.z));
                    }
                }
                break;

            case BlockType.Cube:
                CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
                cb.BlockSet(this, f_pos, material1_id, naturalGeneration);
                b = cb;
                blocks[x, y, z] = cb;
                influenceMask = 0;
                calculateUpperBlock = true;
                break;

            case BlockType.Cave:
                float supportPoints = CalculateSupportPoints(x, y, z);
                if (supportPoints < 1) goto case BlockType.Surface;

                CaveBlock cvb = Instantiate(cave_pref).GetComponent<CaveBlock>();
                cvb.CaveBlockSet(this, f_pos, material2_id, material1_id);
                surfaceBlocks.Add(cvb);
                blocks[x, y, z] = cvb;
                b = cvb;
                if (GetBlock(x, y - 1, z) != null)
                {
                    if (GetBlock(x, y - 1, z).type != BlockType.Surface) influenceMask = 31;
                    else
                    {
                        influenceMask = 63;
                    }
                }
                else influenceMask = 31;
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
                        Grassland gl = Grassland.Create(cvb);
                        gl.SetLifepower(originalSurface.grassland.lifepower);
                        originalSurface.grassland.SetLifepower(0);
                    }
                }
                calculateUpperBlock = true;
                break;
        }
        b.SetVisibilityMask(originalBlock.visibilityMask);
        blocks[x, y, z].MakeIndestructible(originalBlock.indestructible);
        Destroy(originalBlock.gameObject);
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
        Destroy(blocks[x, y, z].gameObject);
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
    }

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

    public void GenerateNature(PixelPosByte lifeSourcePos, int lifeVolume)
    {
        byte px = lifeSourcePos.x, py = lifeSourcePos.y;
        float[,] lifepowers = new float[CHUNK_SIZE, CHUNK_SIZE];
        lifepowers[px, py] = 1;
        float power = 1, half = (float)CHUNK_SIZE / 2f;
        bool leftSide = false, rightSide = false, upSide = false, downSide = false;
        SurfaceBlock[,] t_surfaceBlocks = new SurfaceBlock[CHUNK_SIZE, CHUNK_SIZE];
        RecalculateSurfaceBlocks();
        foreach (SurfaceBlock sb in surfaceBlocks)
        {
            if (t_surfaceBlocks[sb.pos.x, sb.pos.z] == null) t_surfaceBlocks[sb.pos.x, sb.pos.z] = sb;
            else
            {
                if (sb.pos.y > t_surfaceBlocks[sb.pos.x, sb.pos.z].pos.y) t_surfaceBlocks[sb.pos.x, sb.pos.z] = sb;
            }
        }
        if (px > 0)
        {
            leftSide = true;
            for (int i = px - 1; i >= 0; i--)
            {
                byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i + 1, py].pos.y - t_surfaceBlocks[i, py].pos.y);
                lifepowers[i, py] = power * (1 - (delta / half) * (delta / half));
                power = lifepowers[i, py] * 0.9f;
            }
        }
        power = 1;
        if (px < CHUNK_SIZE - 1)
        {
            rightSide = true;
            for (int i = px + 1; i < CHUNK_SIZE; i++)
            {
                byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i - 1, py].pos.y - t_surfaceBlocks[i, py].pos.y);
                lifepowers[i, py] = power * (1 - (delta / half) * (delta / half));
                power = lifepowers[i, py] * 0.9f;
            }
        }
        power = 1;
        if (py > 0)
        {
            downSide = true;
            for (int i = py - 1; i >= 0; i--)
            {
                byte delta = (byte)Mathf.Abs(t_surfaceBlocks[px, i + 1].pos.y - t_surfaceBlocks[px, i].pos.y);
                lifepowers[px, i] = power * (1 - (delta / half) * (delta / half));
                power = lifepowers[px, i] * 0.9f;
            }
        }
        power = 1;
        if (px < CHUNK_SIZE - 1)
        {
            upSide = true;
            for (int i = py + 1; i < CHUNK_SIZE; i++)
            {
                byte delta = (byte)Mathf.Abs(t_surfaceBlocks[px, i - 1].pos.y - t_surfaceBlocks[px, i].pos.y);
                lifepowers[px, i] = power * (1 - (delta / half) * (delta / half));
                power = lifepowers[px, i] * 0.9f;
            }
        }

        // горизонтальная обработка
        if (leftSide)
        {
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                if (i == py) continue;
                power = lifepowers[i, px];
                for (int j = px - 1; j >= 0; j--)
                {
                    byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i, j + 1].pos.y - t_surfaceBlocks[i, j].pos.y);
                    lifepowers[i, j] = power * (1 - (delta / half) * (delta / half));
                    power = lifepowers[i, j] * 0.9f;
                }
            }
        }
        if (rightSide)
        {
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                if (i == py) continue;
                power = lifepowers[i, px];
                for (int j = px + 1; j < CHUNK_SIZE; j++)
                {
                    byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i, j].pos.y - t_surfaceBlocks[i, j - 1].pos.y);
                    lifepowers[i, j] = power * (1 - (delta / half) * (delta / half));
                    power = lifepowers[i, j] * 0.9f;
                }
            }
        }
        // вертикальная обработка + усреднение
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            if (i == px) continue;
            if (upSide)
            {
                power = lifepowers[i, py];
                for (int j = py + 1; j < CHUNK_SIZE; j++)
                {
                    byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i, j].pos.y - t_surfaceBlocks[i, j - 1].pos.y);
                    lifepowers[i, j] = (power * (1 - (delta / half) * (delta / half)) + lifepowers[i, j]) / 2f;
                    power = lifepowers[i, j] * 0.9f;
                }
            }
            if (downSide)
            {
                power = lifepowers[i, py];
                for (int j = py - 1; j >= 0; j--)
                {
                    byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i, j].pos.y - t_surfaceBlocks[i, j + 1].pos.y);
                    lifepowers[i, j] = (power * (1 - (delta / half) * (delta / half)) + lifepowers[i, j]) / 2f;
                    power = lifepowers[i, j] * 0.9f;
                }
            }
        }

        float total = 0;
        List<SurfaceBlock> afl = new List<SurfaceBlock>();
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                SurfaceBlock b = t_surfaceBlocks[i, j];
                if (b == null) continue;
                if (b.material_id == ResourceType.DIRT_ID)
                { // Acceptable for life
                    total += lifepowers[i, j];
                    afl.Add(b);
                }
            }
        }
        float lifePiece = lifeVolume / total;
        lifePower = lifeVolume;
        foreach (SurfaceBlock b in afl)
        {
            Grassland gl = b.grassland;
            if (b.grassland == null) gl = Grassland.Create(b);
            float vol = lifepowers[b.pos.x, b.pos.z] * lifePiece;
            gl.AddLifepowerAndCalculate((int)(vol));
            lifePower -= vol;
        }
    }

    public static void SetChunkSize(byte x)
    {
        CHUNK_SIZE = x;
    }

    public void SetChunk(int[,,] newData)
    {
        if (blocks != null) ClearChunk();
        int size = newData.GetLength(0);
        CHUNK_SIZE = (byte)size;
        if (CHUNK_SIZE < 3) CHUNK_SIZE = 16;
        GameMaster.layerCutHeight = CHUNK_SIZE;
        GameMaster.prevCutHeight = GameMaster.layerCutHeight;

        blocks = new Block[size, size, size];
        surfaceBlocks = new List<SurfaceBlock>();

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                byte surfaceFound = 2;
                for (int y = size - 1; y >= 0; y--)
                {
                    if (newData[x, y, z] != 0)
                    {
                        if (surfaceFound == 2)
                        {
                            SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
                            sb.SurfaceBlockSet(this, new ChunkPos(x, y, z), newData[x, y, z]);
                            surfaceBlocks.Add(sb);
                            blocks[x, y, z] = sb;
                            surfaceFound--;
                        }
                        else
                        {
                            CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
                            cb.BlockSet(this, new ChunkPos(x, y, z), newData[x, y, z], true);
                            blocks[x, y, z] = cb;
                            if (surfaceFound == 1)
                            {
                                surfaceFound = 0;
                            }
                        }
                    }
                }
            }
        }
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (blocks[x, y, z] == null) continue;
                    blocks[x, y, z].SetVisibilityMask(GetVisibilityMask(x, y, z));
                }
            }
        }
        if (surfaceBlocks.Count != 0)
        {
            foreach (SurfaceBlock sb in surfaceBlocks)
            {
                GameMaster.geologyModule.SpreadMinerals(sb);
            }
        }
    }

    public Block GetBlock(ChunkPos cpos) { return GetBlock(cpos.x, cpos.y, cpos.z); }
    public Block GetBlock(int x, int y, int z)
    {
        int size = blocks.GetLength(0);
        if (x < 0 || x > size - 1 || y < 0 || y > size - 1 || z < 0 || z > size - 1) return null;
        else { return blocks[x, y, z]; }
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
                    Destroy(blocks[i, j, k].gameObject);
                }
            }
        }
        blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        surfaceBlocks.Clear();
    }


    void CullingUpdate(Transform campoint)
    {
        if (campoint == null) campoint = Camera.main.transform;
        Vector3 cpos = transform.InverseTransformPoint(campoint.position);
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
                Vector3 icpos = campoint.InverseTransformPoint(b.transform.position);
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
    }

    public void AddLifePower(int count) { lifePower += count; if (lifepower_timer == 0) lifepower_timer = GameMaster.LIFEPOWER_TICK; }
    public int TakeLifePower(int count)
    {
        if (count < 0) return 0;
        float lifeTransfer = count;
        if (lifeTransfer > lifePower) { if (lifePower >= 0) lifeTransfer = lifePower; else lifeTransfer = 0; }
        lifePower -= lifeTransfer;
        if (lifepower_timer == 0) lifepower_timer = GameMaster.LIFEPOWER_TICK;
        return (int)lifeTransfer;
    }
    public int TakeLifePowerWithForce(int count)
    {
        if (count < 0) return 0;
        lifePower -= count;
        if (lifepower_timer == 0) lifepower_timer = GameMaster.LIFEPOWER_TICK;
        return count;
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

    public void BlockByStructure(byte x, byte y, byte z, Structure s)
    {
        if (x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE || x < 0 || y < 0 || z < 0 || s == null) return;
        Block b = GetBlock(x, y, z);
        if (b != null) { ReplaceBlock(new ChunkPos(x, y, z), BlockType.Shapeless, 0, false); }
        else blocks[x, y, z] = new GameObject().AddComponent<Block>();
        blocks[x, y, z].ShapelessBlockSet(this, new ChunkPos(x, y, z), s);
    }

    public void RecalculateSurfaceBlocks()
    {
        surfaceBlocks = new List<SurfaceBlock>();
        foreach (Block b in blocks)
        {
            if (b == null) continue;
            if (b.type == BlockType.Surface || b.type == BlockType.Cave) surfaceBlocks.Add(b as SurfaceBlock);
        }
    }

    void BroadcastChunkUpdate(ChunkPos pos)
    {
        int i = 0;
        while (i < chunkUpdateSubscribers.Count)
        {
            if (chunkUpdateSubscribers[i] == null)
            {
                chunkUpdateSubscribers.RemoveAt(i);
                continue;
            }
            chunkUpdateSubscribers[i].BroadcastMessage("ChunkUpdated", pos, SendMessageOptions.DontRequireReceiver);
            i++;
        }
    }

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
        cs.lifepowerTimer = lifepower_timer;
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
        lifepower_timer = cs.lifepowerTimer;
    }
    #endregion

    public void BlockRow(int index, int side)
    {
        sideBlockingMap[index, side] = true;
    }
    public void UnblockRow(int index, int side)
    {
        sideBlockingMap[index, side] = false;
    }

    void OnGUI()
    { //test
        GUI.Label(new Rect(0, 32, 64, 32), lifePower.ToString());
    }
}
