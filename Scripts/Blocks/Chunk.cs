using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkRaycastHit
{
    public readonly Block block;
    public readonly byte faceIndex;
    public ChunkRaycastHit(Block b, byte face)
    {
        block = b;
        faceIndex = face;
    }
}
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
    public Vector3 ToWorldSpace()
    {
        return new Vector3(x, y, z) * Block.QUAD_SIZE;
    }
    public static bool operator ==(ChunkPos lhs, ChunkPos rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ChunkPos lhs, ChunkPos rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        ChunkPos p = (ChunkPos)obj;
        return (x == p.x) & (y == p.y) & (z == p.z);
    }
    public override int GetHashCode()
    {
        return x * 100 + y * 10 + z;
    }
    public override string ToString()
    {
        return '(' + x.ToString() +' '+ y.ToString() + ' '+ z.ToString() + ')';
    }
}

public enum ChunkGenerationMode : byte { Standart, GameLoading, Cube, Peak, TerrainLoading, DontGenerate }

public sealed partial class Chunk : MonoBehaviour
{  
    public Dictionary<ChunkPos, Block > blocks;
    public bool needSurfacesUpdate = false;
    public byte prevBitmask = 63;
    public float lifePower = 0;
    public static byte CHUNK_SIZE { get; private set; }
    //private bool allGrasslandsCreated = false;
   
    public delegate void ChunkUpdateHandler();
    public event ChunkUpdateHandler ChunkUpdateEvent;

    private List<Plane> surfaces;

    public const float CHUNK_UPDATE_TICK = 0.5f;
    public const byte MIN_CHUNK_SIZE = 3 , NO_FACE_VALUE = 10;
    public const string BLOCK_COLLIDER_TAG = "BlockCollider";    

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
        blocks = new Dictionary<ChunkPos, Block>();
        redrawRequiredTypes = new List<MeshVisualizeInfo>();
        blockVisualizersList = new List<BlockpartVisualizeInfo>();
        SetShadowCastingMode(PoolMaster.shadowCasting);
        if (renderersHolders == null)
        {
            RemakeRenderersHolders();
        }
        lightMap = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    lightMap[x, y, z] = UP_LIGHT;
                }
            }
        }
        LIGHT_DECREASE_PER_BLOCK = 1 - 1f / (PoolMaster.MAX_MATERIAL_LIGHT_DIVISIONS + 1);

        GameMaster.layerCutHeight = CHUNK_SIZE;
        GameMaster.prevCutHeight = CHUNK_SIZE;
    }

    public void Awake()
    {
        FollowingCamera.main.cameraChangedEvent += CullingUpdate;

        //var g = new GameObject("test");
        //g.AddComponent<MeshFilter>().sharedMesh = PoolMaster.GetMesh(MeshType.Quad, ResourceType.METAL_E_ID);
        //g.AddComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetMaterial(MaterialType.Metal);
    }

    private void LateUpdate()
    {
        if (chunkDataUpdateRequired)
        {
            if (ChunkUpdateEvent != null) ChunkUpdateEvent();           
            chunkDataUpdateRequired = false;
        }
        if (chunkRenderUpdateRequired)  RenderStatusUpdate();
        if (PoolMaster.shadowCasting & shadowsUpdateRequired)      ShadowsUpdate();
        if (needSurfacesUpdate) RecalculateSurfacesList();
    }  
    #region operating blocks data

    public void CreateNewChunk(int[,,] newData)
    {
        int size = newData.GetLength(0);
        CHUNK_SIZE = (byte)size;
        if (blocks != null) ClearChunk();
        else blocks = new Dictionary<ChunkPos, Block>();
        Prepare();

        ChunkPos cpos;
        for (int y = size - 1; y > -1; y--)
        {
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (newData[x,y,z] != 0)
                    {
                        cpos = new ChunkPos(x, y, z);
                        blocks.Add(cpos, new Block(this,cpos, newData[x, y, z], true));
                    }
                }
            }
        }
        PrepareAllPlanes();
        if (surfaces != null & GameMaster.realMaster.gameMode != GameMode.Editor)
        {
           GameMaster.geologyModule.SpreadMinerals(surfaces);
        }
        RenderDataFullRecalculation();
        FollowingCamera.main.WeNeedUpdate();
    }
    private void PrepareAllPlanes() 
    {
        if (blocks != null)
        {
            Block[,,] blockArray = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
            Block b;
            foreach(var fb in blocks)
            {
                b = fb.Value;
                var cpos = b.pos;
                blockArray[cpos.x, cpos.y, cpos.z] = b;
            }
            bool transparency = true;
            Block prevBlock = null;
            //left to right
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        b = blockArray[x, y, z];
                        if (b == null)
                        {
                            transparency = true;
                            if (prevBlock != null)
                            {
                                prevBlock.InitializePlane(Block.RIGHT_FACE_INDEX);
                                prevBlock = null;
                            }
                            continue;
                        }
                        else
                        {
                            if (transparency)
                            {
                                if (b.InitializePlane(Block.LEFT_FACE_INDEX)) // возвращает true, если скроет следующий блок
                                {
                                    transparency = false; 
                                    prevBlock?.DeactivatePlane(Block.RIGHT_FACE_INDEX);
                                }
                                else
                                {
                                    transparency = true;
                                    prevBlock?.InitializePlane(Block.RIGHT_FACE_INDEX);
                                }
                            }
                            else
                            {
                                b.DeactivatePlane(Block.LEFT_FACE_INDEX);
                            }
                            prevBlock = b;
                        }
                    }
                    if (prevBlock != null)
                    {
                        prevBlock.InitializePlane(Block.RIGHT_FACE_INDEX);
                        prevBlock = null;
                    }
                    transparency = true;
                }
            }
            //back to fwd
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int x = 0; x < CHUNK_SIZE; x++)
                    {
                        b = blockArray[x, y, z];
                        if (b == null)
                        {
                            transparency = true;
                            if (prevBlock != null)
                            {
                                prevBlock.InitializePlane(Block.FWD_FACE_INDEX);
                                prevBlock = null;
                            }
                            continue;
                        }
                        else
                        {
                            if (transparency)
                            {
                                if (b.InitializePlane(Block.BACK_FACE_INDEX)) // возвращает true, если скроет следующий блок
                                {
                                    transparency = false;
                                    prevBlock?.DeactivatePlane(Block.FWD_FACE_INDEX);
                                }
                                else
                                {
                                    transparency = true;
                                    prevBlock?.InitializePlane(Block.FWD_FACE_INDEX);
                                }
                            }
                            else
                            {
                                b.DeactivatePlane(Block.BACK_FACE_INDEX);
                            }
                            prevBlock = b;
                        }
                    }
                    if (prevBlock != null)
                    {
                        prevBlock.InitializePlane(Block.FWD_FACE_INDEX);
                        prevBlock = null;
                    }
                    transparency = true;
                }
            }
            //down to up
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    for (int x = 0; x < CHUNK_SIZE; x++)
                    {
                        b = blockArray[x, y, z];
                        if (b == null)
                        {
                            transparency = true;
                            if (prevBlock != null)
                            {
                                prevBlock.InitializePlane(Block.UP_FACE_INDEX);
                                prevBlock = null;
                            }
                            continue;
                        }
                        else
                        {
                            if (transparency)
                            {
                                if (b.InitializePlane(Block.DOWN_FACE_INDEX)) // возвращает true, если скроет следующий блок
                                {
                                    transparency = false;
                                    prevBlock?.DeactivatePlane(Block.UP_FACE_INDEX);
                                }
                                else
                                {
                                    transparency = true;
                                    prevBlock?.InitializePlane(Block.UP_FACE_INDEX);
                                }
                            }
                            else
                            {
                                b.DeactivatePlane(Block.DOWN_FACE_INDEX);
                            }
                            prevBlock = b;
                        }
                    }
                    if (prevBlock != null)
                    {
                        prevBlock.InitializePlane(Block.UP_FACE_INDEX);
                        prevBlock = null;
                    }
                    transparency = true;
                }
            }

            blockArray = null;
            RecalculateSurfacesList();
        }
    }
    private void RecalculateSurfacesList()
    {
        if (blocks != null)
        {
            byte upcode = Block.SURFACE_FACE_INDEX;
            Block b;
            Plane p = null;
            if (surfaces == null) surfaces = new List<Plane>(); else surfaces.Clear();
            foreach (var fb in blocks)
            {
                b = fb.Value;
                if (b.pos.y != Chunk.CHUNK_SIZE - 1 && b.TryGetPlane(upcode, out p)) surfaces.Add(p);
            }
            if (surfaces.Count == 0) surfaces = null;
        }
        else surfaces = null;
        needSurfacesUpdate = false;
    }

    private Block AddBlock_NoCheck(ChunkPos f_pos, Structure mainStructure)
    {
        var b = new Block(this, f_pos, mainStructure);
        if (blocks == null) blocks = new Dictionary<ChunkPos, Block>();
        blocks.Add(f_pos, b);
        if (PoolMaster.useIlluminationSystem)  RecalculateIlluminationAtPoint(b.pos);
        return b;
    }
    private Block AddBlock(ChunkPos f_pos, Block.BlockMaterialsList bml, bool i_naturalGeneration )
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (x >= CHUNK_SIZE | y >= CHUNK_SIZE | z >= CHUNK_SIZE) return null;
        //
        Block prv = GetBlock(x, y, z);
        if (prv != null)
        {
            if (bml.GetExistenceMask() == 0) prv.Annihilate(!i_naturalGeneration);
            else prv.RebuildBlock(bml, i_naturalGeneration, false);
            return prv;
        }
        else
        {
            var b = new Block(this, f_pos, bml, i_naturalGeneration);
            blocks.Add(f_pos, b);
            if (PoolMaster.useIlluminationSystem)   RecalculateIlluminationAtPoint(b.pos);
            return b;
        }
    }
    public Block AddBlock(ChunkPos f_pos, int i_materialID, bool i_naturalGeneration)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (x >= CHUNK_SIZE | y >= CHUNK_SIZE | z >= CHUNK_SIZE) return null;
        //
        Block b = GetBlock(x, y, z);
        if (b != null)
        {
            b.ChangeMaterial(i_materialID, i_naturalGeneration);
            return b;
        }
        else
        {
            if (i_materialID == PoolMaster.NO_MATERIAL_ID) return AddBlock_NoCheck(f_pos, null);
            else
            {
                b = new Block(this, f_pos, i_materialID, i_naturalGeneration);
                if (blocks == null) blocks = new Dictionary<ChunkPos, Block>();
                blocks.Add(f_pos, b);
                if (PoolMaster.useIlluminationSystem)  RecalculateIlluminationAtPoint(b.pos);
            }
        }

        RecalculateVisibilityAtPoint(x, y, z);
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        RefreshBlockVisualising(b);
        chunkRenderUpdateRequired = true;
        return b;
    }


    public Block GetBlock(ChunkPos cpos) {
        if (blocks.ContainsKey(cpos))
        {
            Block b;
            if (blocks.TryGetValue(cpos, out b)) return b;
        }
        return null;
    }
    public Block GetBlock(int x, int y, int z)
    {
        return GetBlock(new ChunkPos(x, y, z));
    }
    public ChunkRaycastHit GetBlock(Vector3 hitpoint, Vector3 normal)
    {
        byte face = NO_FACE_VALUE;
        float bs = Block.QUAD_SIZE;
        var orig = hitpoint;
        Vector3Int blockpos = new Vector3Int((int)(hitpoint.x / bs), (int)(hitpoint.y / bs), (int)(hitpoint.z / bs));

        hitpoint = ( hitpoint - new Vector3(blockpos.x * bs, blockpos.y * bs, blockpos.z * bs) ) / bs;        
        if (hitpoint.x > 0.5f) { blockpos.x++; hitpoint.x -= 0.5f; }
        if (hitpoint.y > 0.5f) { blockpos.y++; hitpoint.y -= 0.5f; }
        if (hitpoint.z > 0.5f) { blockpos.z++; hitpoint.z -= 0.5f; }
        //print(blockpos.ToString() + ' ' + hitpoint.ToString());

        Block b = GetBlock(blockpos.x, blockpos.y, blockpos.z);        
        if (hitpoint.y == 0.5f)
        {
            if (normal == Vector3.down)
            {
                b = GetBlock(blockpos.x, blockpos.y + 1, blockpos.z);
                face = Block.DOWN_FACE_INDEX;
            }
            else
            {
                if (normal == Vector3.up)
                {
                    if ((GetVisibilityMask(blockpos.x, blockpos.y, blockpos.z) & (1 << Block.UP_FACE_INDEX)) == 0)
                    {
                        b = GetBlock(blockpos.x, blockpos.y + 1, blockpos.z);
                        face = Block.SURFACE_FACE_INDEX; // surface block
                    }
                    else
                    {
                        face = Block.UP_FACE_INDEX;
                    }
                }
            }
        }
        else
        {            
            if (hitpoint.y == -0.5f)
            {
                if (normal == Vector3.up)
                {
                    b = GetBlock(blockpos.x, blockpos.y - 1, blockpos.z);
                    face = Block.UP_FACE_INDEX;
                }
                else
                {
                    face = Block.DOWN_FACE_INDEX;
                }
            }
            else
            {
                if (hitpoint.x == 0.5f)
                {
                    if (normal == Vector3.left)
                    {
                        b = GetBlock(blockpos.x + 1, blockpos.y, blockpos.z);
                        face = Block.LEFT_FACE_INDEX;
                    }
                    else
                    {
                        face = Block.RIGHT_FACE_INDEX;
                    }
                }
                else
                {
                    if (hitpoint.x == -0.5f)
                    {
                        if (normal == Vector3.right)
                        {
                            b = GetBlock(blockpos.x - 1, blockpos.y, blockpos.z);
                            face = Block.RIGHT_FACE_INDEX;
                        }
                        else
                        {
                            face = Block.LEFT_FACE_INDEX;
                        }
                    }
                    else
                    {
                        if (hitpoint.z == 0.5f)
                        {
                            if (normal == Vector3.back)
                            {
                                b = GetBlock(blockpos.x, blockpos.y, blockpos.z + 1);
                                face = Block.BACK_FACE_INDEX;
                            }
                            else
                            {
                                face = Block.FWD_FACE_INDEX;
                            }
                        }
                        else
                        {
                            if (hitpoint.z == -0.5f)
                            {
                                if (normal == Vector3.forward)
                                {
                                    b = GetBlock(blockpos.x, blockpos.y, blockpos.z - 1);
                                    face = Block.FWD_FACE_INDEX;
                                }
                                else
                                {
                                    face = Block.BACK_FACE_INDEX;
                                }
                            }
                            else
                            {
                                if (hitpoint.y < 0.5f - Block.CEILING_THICKNESS + 0.001f & normal == Vector3.down)
                                {
                                    b = GetBlock(blockpos.x, blockpos.y, blockpos.z);
                                    face = Block.CEILING_FACE_INDEX;
                                }
                                else
                                {
                                    // часто попадает сюда, неизвестно почему (отключение невидимых коллайдеров, скорее всего)
                                }
                            }
                        }
                    }
                }
            }
        }
        return new ChunkRaycastHit(b, face);
    }

    public Plane GetHighestSurfacePlane(int x, int z)
    {
        if (surfaces == null || x < 0 || z < 0 || x >= CHUNK_SIZE || z >= CHUNK_SIZE) return null;
        Plane fp = null;
        int maxY = -1;
        ChunkPos cpos;
        foreach (var s in surfaces)
        {
            cpos = s.GetChunkPosition();
            if (cpos.x == x && cpos.z == z)
            {
                if (cpos.y > maxY)
                {
                    maxY = cpos.y;
                    fp = s;
                }
            }
            else continue;
        }
        return fp;
    }
    public Plane GetRandomSurface(byte faceIndex)
    {
        if (surfaces != null)
        {
            int x = surfaces.Count;
            return surfaces[Random.Range(0,x)];
        }
        return null;
    }
    public Plane GetNearestUnoccupiedSurface( ChunkPos origin)
    {
        if (surfaces == null) return null;
        else
        {
            int nindex = -1; float minSqr = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
            float a, b, c, m;
            Plane p = null;
            ChunkPos cpos;
            for (int i = 0; i < surfaces.Count; i++)
            {
                p = surfaces[i];
                if (p.extension != null && p.extension.noEmptySpace == true) continue;
                else
                {
                    cpos = p.GetChunkPosition();
                    a = cpos.x - origin.x;
                    b = cpos.y - origin.y;
                    c = cpos.z - origin.z;
                    m = a * a + b * b + c * c;
                    if (m < minSqr)
                    {
                        minSqr = m;
                        nindex = i;
                    }
                }
            }
            if (nindex > 0) return surfaces[nindex];
            else return null;
        }
    }
    /// <summary>
    /// Seek a position for structure somewhere. Returns (xpos, zpos, surface_block_index)
    /// </summary>
    public bool TryGetPlace(ref Vector3Int answer, byte size)
    {
        if (surfaces == null) return false;
        var suitable = new List<int>();
        int i = 0;
        Plane p = null;
        if (size == PlaneExtension.INNER_RESOLUTION)
        {
            for (; i < surfaces.Count; i++)
            {
                p = surfaces[i];
                if (p.extension == null || p.extension.artificialStructuresCount == 0) suitable.Add(i);
            }
            answer = new Vector3Int(0, 0, suitable[Random.Range(0, suitable.Count - 1)]);
            return true;
        }
        else
        {
            if (size == 1)
            {
                for (; i < surfaces.Count; i++)
                {
                    p = surfaces[i];
                    if (p.extension == null || p.extension.noEmptySpace != true) suitable.Add(i);
                }
                i = Random.Range(0, suitable.Count - 1);
                int realIndex = suitable[i];
                var ppos = surfaces[realIndex].GetExtension().GetRandomCell();
                answer = new Vector3Int(ppos.x, ppos.y, i);
                return true;
            }
            else
            {
                for (; i < surfaces.Count; i++)
                {
                    p = surfaces[i];
                    if (p.extension == null || p.extension.noEmptySpace != true) suitable.Add(i);
                }
                PixelPosByte ppos = PixelPosByte.Empty;
                int realIndex = 0;
                while (suitable.Count > 0)
                {
                    i = Random.Range(0, suitable.Count - 1);
                    realIndex = suitable[i];
                    ppos = surfaces[realIndex].GetExtension().GetRandomPosition(size);
                    if (ppos.exists)
                    {
                        answer = new Vector3Int(ppos.x, ppos.y, realIndex);
                        return true;
                    }
                    else
                    {
                        suitable.RemoveAt(i);
                    }
                }
                return false;
            }
        }
    }

    public void DeleteBlock(ChunkPos pos, bool compensateStructures)
    {
        // в сиквеле стоит пересмотреть всю иерархию классов ><
        //12.06 нет, я так не думаю
        // 24.04.2019 фига сколько времени прошло
        // 26.01.2020 ну привет
        Block b = GetBlock(pos);
        if (b == null) return;
        int x = pos.x, y = pos.y, z = pos.z;
        b.Annihilate(compensateStructures);
        blocks.Remove(b.pos);
        RemoveBlockVisualisers(b.pos);
        RecalculateVisibilityAtPoint(x, y, z);
        if (PoolMaster.useIlluminationSystem) RecalculateIlluminationAtPoint(pos);
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        // chunkRenderUpdateRequired = true; < в свитче
    }
    public void ClearChunk()
    {
        if (blocks.Count > 0)
        {
            foreach (var b in blocks) { b.Value.Annihilate(false); }
            blocks.Clear();
        }
        blocks = new Dictionary<ChunkPos, Block>();
        surfaces = null;
        lifePower = 0;
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        RenderDataFullRecalculation();
    }

    public void BlockRegion(List<ChunkPos> positions, Structure s, ref List<Block> dependentBlocks)
    {
        foreach (ChunkPos pos in positions)
        {
            if ((pos.x >= CHUNK_SIZE || pos.x < 0) | (pos.y >= CHUNK_SIZE || pos.y < 0) | (pos.z >= CHUNK_SIZE || pos.z < 0)) continue;
            Block b = GetBlock(pos);
            if (b != null) continue;
            else
            {
                dependentBlocks.Add(AddBlock_NoCheck(pos, s));
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
                        if (GetBlock(x, y, z) != null) return false;
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
                        if (GetBlock(x, y, z) != null) return false;
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
                        ChunkPos cpos = new ChunkPos(x, y, z);
                        bk = new Block(this, cpos, sender);
                        blocks.Add(cpos, bk);
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
                        ChunkPos cpos = new ChunkPos(x, y, z);
                       
                        bk = new Block(this, cpos, sender);
                        blocks.Add(cpos, bk);
                        dependentBlocksList.Add(bk);
                    }
                }
            }
        }
        chunkDataUpdateRequired = true;
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
                                if (blocks.ContainsKey(new ChunkPos(x,y,z))) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z < CHUNK_SIZE; z++)
                    {
                        if (blocks.ContainsKey(new ChunkPos(startPoint.x, startPoint.y, z))) return false;
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
                                if (blocks.ContainsKey(new ChunkPos(x, y, z))) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x < CHUNK_SIZE; x++)
                    {
                        if (blocks.ContainsKey(new ChunkPos(x, startPoint.y, startPoint.z))) return false;
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
                                if (blocks.ContainsKey(new ChunkPos(x, y, z))) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z >= 0; z--)
                    {
                        if (blocks.ContainsKey(new ChunkPos(startPoint.x, startPoint.y, z))) return false;
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
                                if (blocks.ContainsKey(new ChunkPos(x, y, z))) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x >= 0; x--)
                    {
                        if (blocks.ContainsKey(new ChunkPos(x, startPoint.y, startPoint.z))) return false;
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
                                var cpos = new ChunkPos(x, y, z);
                                bk = new Block(this, cpos, sender);
                                blocks.Add(cpos, bk);
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z < CHUNK_SIZE; z++)
                    {
                        var cpos = new ChunkPos(xStart, yStart, z);
                        bk = new Block(this, cpos, sender);
                        blocks.Add(cpos, bk);
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
                                var cpos = new ChunkPos(x, y, z);
                                bk = new Block(this, cpos, sender);
                                blocks.Add(cpos, bk);
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x < CHUNK_SIZE; x++)
                    {
                        var cpos = new ChunkPos(x, yStart, zStart);
                        bk = new Block(this, cpos, sender);
                        blocks.Add(cpos, bk);
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
                                var cpos = new ChunkPos(x, y, z);
                                bk = new Block(this, cpos, sender);
                                blocks.Add(cpos, bk);
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z >= 0; z--)
                    {
                        var cpos = new ChunkPos(xStart, yStart, z);
                        bk = new Block(this, cpos, sender);
                        blocks.Add(cpos, bk);
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
                                var cpos = new ChunkPos(x, y, z);
                                bk = new Block(this, cpos, sender);
                                blocks.Add(cpos, bk);
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x >= 0; x--)
                    {
                        var cpos = new ChunkPos(x, yStart, zStart);
                        bk = new Block(this, cpos, sender);
                        blocks.Add(cpos, bk);
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
        }
        chunkDataUpdateRequired = true;
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
                blocks.Remove(b.pos);
                b.Annihilate(false);                
                actions = true;
            }
        }
        if (actions) chunkDataUpdateRequired = true;
    }
    #endregion

    #region save-load system
    public void SaveChunkData(System.IO.FileStream fs)
    {
        fs.WriteByte(CHUNK_SIZE);

    }

    public void LoadChunkData(System.IO.FileStream fs)
    {
        if (blocks != null) ClearChunk();   
        CHUNK_SIZE = (byte)fs.ReadByte();
        Prepare();

        var data = new byte[4];
        fs.Read(data, 0, 4);
        surfaces = null;
        int blocksCount = System.BitConverter.ToInt32(data, 0);
        if (blocksCount > 1000000)
        {
            Debug.Log("chunk load error - too much blocks");
            GameMaster.LoadingFail();
            return;
        }
        
        
        if (borderDrawn) DrawBorder();
        if (PoolMaster.shadowCasting) ShadowsUpdate();
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
        FollowingCamera.main.cameraChangedEvent -= CullingUpdate;
    }
}
