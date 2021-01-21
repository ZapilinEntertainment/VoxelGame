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
public enum ChunkGenerationMode : byte { Standart, GameLoading, Cube, Peak, TerrainLoading, DontGenerate }

public sealed partial class Chunk : MonoBehaviour
{  
    public Dictionary<ChunkPos, Block > blocks;
    public bool needSurfacesUpdate = false; // hot
    public byte prevBitmask = 63;
    public float lifePower = 0;
    public static byte chunkSize { get; private set; }
    //private bool allGrasslandsCreated = false;
   
    public event System.Action ChunkUpdateEvent;

    public Plane[] surfaces { get; private set; }
    private Nature nature;

    public const float CHUNK_UPDATE_TICK = 0.5f;
    public const byte MIN_CHUNK_SIZE = 3 , NO_FACE_VALUE = 10;
    public const string BLOCK_COLLIDER_TAG = "BlockCollider";    

    static Chunk() 
    {
        chunkSize = 16;
    }
    public static void SetChunkSizeValue(byte x)
    {
        chunkSize = x;
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
        lightMap = new byte[chunkSize, chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    lightMap[x, y, z] = UP_LIGHT;
                }
            }
        }
        LIGHT_DECREASE_PER_BLOCK = 1 - 1f / (PoolMaster.MAX_MATERIAL_LIGHT_DIVISIONS + 1);

        GameMaster.layerCutHeight = chunkSize;
        GameMaster.prevCutHeight = chunkSize;
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
        if (PoolMaster.shadowCasting & shadowsUpdateRequired) ShadowsUpdate();
        if (needSurfacesUpdate) RecalculateSurfacesList();
    }  

    public Nature GetNature()
    {
        if (nature == null)
        {
            nature = gameObject.AddComponent<Nature>();
            nature.Prepare(this);
        }
        return nature;
    }
    public Nature TryGetNature()
    {
        return nature;
    }
    public bool CheckForPlanttype(PlantType pt)
    {
       return nature?.islandFlora?.Contains(pt) ?? false;
    }

    public bool IsAnyStructureInABlockSpace(ChunkPos cpos)
    {
        Block b = GetBlock(cpos.OneBlockForward());
        Plane p;
        if (b != null && b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && p.artificialStructuresCount > 0) return true;
        b = GetBlock(cpos.OneBlockRight());
        if (b != null && b.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && p.artificialStructuresCount > 0) return true;
        b = GetBlock(cpos.OneBlockBack());
        if (b != null && b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && p.artificialStructuresCount > 0) return true;
        b = GetBlock(cpos.OneBlockLeft());
        if (b != null && b.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && p.artificialStructuresCount > 0) return true;
        b = GetBlock(cpos.OneBlockHigher());
        if (b != null && b.TryGetPlane(Block.DOWN_FACE_INDEX, out p) && p.artificialStructuresCount > 0) return true;
        b = GetBlock(cpos.OneBlockDown());
        if (b != null && b.TryGetPlane(Block.UP_FACE_INDEX, out p) && p.artificialStructuresCount > 0) return true;
        return false;
    }

    #region operating blocks data

    public void CreateNewChunk(int[,,] newData)
    {
        int size = newData.GetLength(0);
        chunkSize = (byte)size;
        if (blocks != null) ClearChunk();
        else blocks = new Dictionary<ChunkPos, Block>();
        Prepare();

        ChunkPos cpos;
        for (int y = 0; y < size; y++)
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
        PreparePlanes();
        RenderDataFullRecalculation();
        RecalculateSurfacesList();
        if (surfaces != null & GameMaster.realMaster.gameMode != GameMode.Editor)
        {
           GameMaster.geologyModule.SpreadMinerals(surfaces);
        }
        
        FollowingCamera.main.WeNeedUpdate();
    }
    private void PreparePlanes()
    {
        if (blocks != null)
        {
            Block[,,] blockArray = new Block[chunkSize, chunkSize, chunkSize];
            Block b;            
            foreach (var fb in blocks)
            {              
                b = fb.Value;
                var cpos = b.pos;
                blockArray[cpos.x, cpos.y, cpos.z] = b;
            }
            bool transparency = true;
            Block prevBlock = null;
            //left to right
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
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
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
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
                transparency = true;
                prevBlock = null;
            }
            //down to up                  
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int y = 0; y < chunkSize; y++)
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
        }
    }
    private void RecalculateSurfacesList()
    {
        if (blocks != null)
        {
            byte upcode = Block.UP_FACE_INDEX;
            Plane p = null;
            var plist = new List<Plane>();
            foreach (var fb in blocks)
            {
                if (fb.Value.TryGetPlane(upcode, out p))
                {
                    Block b;
                    Plane p2;
                    if (blocks.TryGetValue(fb.Key.OneBlockHigher(), out b) && b.TryGetPlane(Block.DOWN_FACE_INDEX, out p2) && !p2.isTransparent)
                    {
                        fb.Value.DeactivatePlane(Block.UP_FACE_INDEX);
                    }
                    else
                    {
                        if (p.isQuad) plist.Add(p);
                    }
                }
            }
            
            if (plist.Count == 0) surfaces = null;
            else
            {
                int i = 0;
                Block b;
                Plane p2;
                while (i < plist.Count)
                {
                    p = plist[i];
                    b = GetBlock(p.pos.OneBlockHigher());
                    if (b != null && (b.TryGetPlane(Block.DOWN_FACE_INDEX, out p2) || b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p2)))
                    {
                        if (p2.isSurface)
                        {
                            plist.RemoveAt(i);
                            continue;
                        }
                    }
                    i++;
                }
                if (plist.Count > 0) surfaces = plist.ToArray();
            }
        }
        else surfaces = null;
        needSurfacesUpdate = false;
    }


    public Block AddBlock(ChunkPos f_pos, int i_materialID, bool i_naturalGeneration, bool redrawCall)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if ((x >= chunkSize) || (y >= chunkSize) || (z >= chunkSize) || (x == 255) || (y == 255) || (z == 255))
        {
            Debug.LogException(new System.Exception("Chunk - cannot create chunk with such coordinates"));
            return null;
        }
        //
        Block b = GetBlock(f_pos);
        if (b != null)
        {
            b.ChangeMaterial(i_materialID, i_naturalGeneration, redrawCall);
            return b;
        }
        else
        {
            bool planesCheck = false;
            b = new Block(this, f_pos, i_materialID, i_naturalGeneration);
            if (blocks == null)  blocks = new Dictionary<ChunkPos, Block>();                
            else planesCheck = true;
            blocks.Add(f_pos, b);
            if (PoolMaster.useIlluminationSystem) RecalculateIlluminationAtPoint(b.pos);            

            RecalculateVisibilityAtPoint(f_pos, b.GetAffectionMask());
            if (planesCheck) PlanesCheck(b, i_naturalGeneration);

            if (b.ContainSurface()) needSurfacesUpdate = true;

            chunkDataUpdateRequired = true;
            chunkRenderUpdateRequired = true;
            shadowsUpdateRequired = true;
            return b;
        }      
    }
    public Block AddBlock(ChunkPos i_pos, IPlanable ms, bool i_natural, bool planesCheck)
    {
        int x = i_pos.x, y = i_pos.y, z = i_pos.z;
        if ((x >= chunkSize) || (y >= chunkSize) || (z >= chunkSize) || (x == 255) || (y == 255) || (z == 255))
        {
            Debug.LogException(new System.Exception("Chunk - cannot create chunk with such coordinates"));
            return null;
        }
        var b = GetBlock(i_pos);
        if (b != null)
        {
            if (b.ContainSurface()) needSurfacesUpdate = true;
            DeleteBlock(b.pos, !i_natural);
            planesCheck = true;
        }
        b = new Block(this, i_pos, ms);
        if (blocks == null)
        {
            blocks = new Dictionary<ChunkPos, Block>();
            planesCheck = false;
        }
        blocks.Add(i_pos, b);
        if (PoolMaster.useIlluminationSystem) RecalculateIlluminationAtPoint(b.pos);
        if (planesCheck) PlanesCheck(b, i_natural);
        if (b.ContainSurface()) needSurfacesUpdate = true;
        chunkDataUpdateRequired = true;
        chunkRenderUpdateRequired = true;
        shadowsUpdateRequired = true;
        return b;
    }
    private void PlanesCheck(Block b, bool i_naturalGeneration)
    {
        Block b2;
        Plane p, p2;
        if (blocks.TryGetValue(b.pos.OneBlockForward(), out b2))
        {
            if (b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && b2.TryGetPlane(Block.BACK_FACE_INDEX, out p2))
            {
                if (p.isSurface && p2.isSurface)
                {
                    p.Annihilate(!i_naturalGeneration);
                    p2.Annihilate(!i_naturalGeneration);
                }
            }
        }
        if (blocks.TryGetValue(b.pos.OneBlockRight(), out b2))
        {
            if (b.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && b2.TryGetPlane(Block.LEFT_FACE_INDEX, out p2))
            {
                if (p.isSurface && p2.isSurface)
                {
                    p.Annihilate(!i_naturalGeneration);
                    p2.Annihilate(!i_naturalGeneration);
                }
            }
        }
        if (blocks.TryGetValue(b.pos.OneBlockBack(), out b2))
        {
            if (b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && b2.TryGetPlane(Block.FWD_FACE_INDEX, out p2))
            {
                if (p.isSurface && p2.isSurface)
                {
                    p.Annihilate(!i_naturalGeneration);
                    p2.Annihilate(!i_naturalGeneration);
                }
            }
        }
        if (blocks.TryGetValue(b.pos.OneBlockLeft(), out b2))
        {
            if (b.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && b2.TryGetPlane(Block.RIGHT_FACE_INDEX, out p2))
            {
                if (p.isSurface && p2.isSurface)
                {
                    p.Annihilate(!i_naturalGeneration);
                    p2.Annihilate(!i_naturalGeneration);
                }
            }
        }
        if (blocks.TryGetValue(b.pos.OneBlockHigher(), out b2))
        {
            if (b2.TryGetPlane(Block.DOWN_FACE_INDEX, out p2))
            {
                if (b.TryGetPlane(Block.UP_FACE_INDEX, out p))
                {
                    if (p.isSurface && p2.isSurface)
                    {
                        p.Annihilate(!i_naturalGeneration);
                        p2.Annihilate(!i_naturalGeneration);
                    }
                }
                else
                {
                    if (b.TryGetPlane(Block.CEILING_FACE_INDEX, out p))
                    {
                        if (p.isSurface && p2.isSurface)
                        {
                            p2.Annihilate(!i_naturalGeneration);
                        }
                    }
                }
            }
        }
        if (blocks.TryGetValue(b.pos.OneBlockDown(), out b2))
        {
            if (b2.TryGetPlane(Block.UP_FACE_INDEX, out p2))
            {
                if (b.TryGetPlane(Block.DOWN_FACE_INDEX, out p))
                {
                    if (p.isSurface && p2.isSurface)
                    {
                        p.Annihilate(!i_naturalGeneration);
                        p2.Annihilate(!i_naturalGeneration);
                    }
                }
                else
                {
                    if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p))
                    {
                        if (p.isSurface && p2.isSurface)
                        {
                            p2.Annihilate(!i_naturalGeneration);
                        }
                    }
                }
            }
        }
    }

    public Block GetBlock(ChunkPos cpos) {
        if (blocks.ContainsKey(cpos))
        {
            Block b;
            if (blocks.TryGetValue(cpos, out b))
            {
                if (b != null && !b.destroyed) return b;
                else return null;
            }
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
                    /*
                    if ((GetVisibilityMask(blockpos.x, blockpos.y, blockpos.z) & (1 << Block.UP_FACE_INDEX)) == 0)
                    {
                        b = GetBlock(blockpos.x, blockpos.y + 1, blockpos.z);
                        face = Block.SURFACE_FACE_INDEX; // surface block
                    }
                    else
                    {
                        face = Block.UP_FACE_INDEX;
                    }
                    */
                    face = Block.UP_FACE_INDEX;
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
    public int DEBUG_GetStructuresCount()
    {
        if (blocks == null || blocks.Count == 0) return 0;
        List<Structure> str = null;
        int x = 0;
        foreach (var b in blocks)
        {
            if (b.Value.TryGetStructuresList(ref str))
            {
                x += str.Count;
                str = null;
            }
        }
        return x;
    }

    #region taking surfaces
    public Plane GetHighestSurfacePlane(int x, int z)
    {
        if (surfaces == null || x < 0 || z < 0 || x >= chunkSize || z >= chunkSize) return null;
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
            return surfaces[Random.Range(0, surfaces.Length)];
        }
        return null;
    }
    public Plane GetNearestUnoccupiedSurface( ChunkPos origin)
    {
        if (surfaces == null) return null;
        else
        {
            int nindex = -1; float minSqr = chunkSize * chunkSize * chunkSize;
            float a, b, c, m;
            Plane p = null;
            ChunkPos cpos;
            for (int i = 0; i < surfaces.Length; i++)
            {
                p = surfaces[i];
                if (p.fulfillStatus == FullfillStatus.Full) continue;
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
    public Plane GetSurfacePlane(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return null;
        else return GetSurfacePlane(new ChunkPos(x, y, z));
    }
    public Plane GetSurfacePlane(ChunkPos cpos)
    {
        if (surfaces == null) return null;
        else
        {
            foreach (var s in surfaces)
            {
                if (s.pos == cpos) return s;
            }
            return null;
        }
    }   
    #endregion
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
            for (; i < surfaces.Length; i++)
            {
                p = surfaces[i];
                if (p.fulfillStatus ==FullfillStatus.Empty || p.artificialStructuresCount == 0) suitable.Add(i);
            }
            answer = new Vector3Int(0, 0, suitable[Random.Range(0, suitable.Count)]);
            return true;
        }
        else
        {
            if (size == 1)
            {
                for (; i < surfaces.Length; i++)
                {
                    p = surfaces[i];
                    if (p.fulfillStatus != FullfillStatus.Full) suitable.Add(i);
                }
                i = Random.Range(0, suitable.Count);
                int realIndex = suitable[i];
                var ppos = surfaces[realIndex].FORCED_GetExtension().GetRandomCell();
                answer = new Vector3Int(ppos.x, ppos.y, i);
                return true;
            }
            else
            {
                for (; i < surfaces.Length; i++)
                {
                    p = surfaces[i];
                    if (p.fulfillStatus != FullfillStatus.Full) suitable.Add(i);
                }
                PixelPosByte ppos = PixelPosByte.Empty;
                int realIndex = 0;
                while (suitable.Count > 0)
                {
                    i = Random.Range(0, suitable.Count );
                    realIndex = suitable[i];
                    ppos = surfaces[realIndex].FORCED_GetExtension().GetRandomPosition(size);
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
        // 21.01.21 ...
        Block b = GetBlock(pos);
        if (b == null) return;
        else
        {
            if (b.isInvincible) return;
        }
        int x = pos.x, y = pos.y, z = pos.z;
        if (b.ContainSurface()) needSurfacesUpdate = true;
        var affectionMask = b.GetAffectionMask();
        blocks.Remove(b.pos);
        b.Annihilate(compensateStructures);        
        RemoveBlockVisualisers(b.pos);
        if (PoolMaster.useIlluminationSystem) RecalculateIlluminationAtPoint(pos);
        if (affectionMask != 0)
        {
            if ((affectionMask & (1 << Block.FWD_FACE_INDEX)) != 0) GetBlock(pos.OneBlockForward())?.InitializePlane(Block.BACK_FACE_INDEX);
            if ((affectionMask & (1 << Block.RIGHT_FACE_INDEX)) != 0) GetBlock(pos.OneBlockRight())?.InitializePlane(Block.LEFT_FACE_INDEX);
            if ((affectionMask & (1 << Block.BACK_FACE_INDEX)) != 0) GetBlock(pos.OneBlockBack())?.InitializePlane(Block.FWD_FACE_INDEX);
            if ((affectionMask & (1 << Block.UP_FACE_INDEX)) != 0) GetBlock(pos.OneBlockHigher())?.InitializePlane(Block.DOWN_FACE_INDEX);
            if ((affectionMask & (1 << Block.LEFT_FACE_INDEX)) != 0) GetBlock(pos.OneBlockLeft())?.InitializePlane(Block.RIGHT_FACE_INDEX);
            if ((affectionMask & (1 << Block.DOWN_FACE_INDEX)) != 0) GetBlock(pos.OneBlockDown())?.InitializePlane(Block.UP_FACE_INDEX);
            RecalculateVisibilityAtPoint(pos, affectionMask);
        }
        shadowsUpdateRequired = true;
        chunkDataUpdateRequired = true;
        // chunkRenderUpdateRequired = true; < в свитче
    }
    public void ClearChunk()
    {
        if (blocks.Count > 0)
        {
            foreach (var b in blocks) { b.Value.Annihilate(false); }
        }
        blocks = new Dictionary<ChunkPos, Block>();
        surfaces = null;
        lifePower = 0;
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        RenderDataFullRecalculation();
    }

    #region blocking
    public void CreateBlocker(ChunkPos f_pos, Structure main_structure, bool forced, bool useMarker)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (x >= chunkSize | y >= chunkSize | z >= chunkSize) return;
        //
        Block b = GetBlock(x, y, z);
        if (b != null)
        {
            if (b.haveExtension) return;
            else
            {
                if (b.mainStructure != null)
                {
                    if (!forced) return;
                    else
                    {
                        b.ReplaceBlocker(main_structure);
                        return;
                    }
                }
                else
                {
                    b.ReplaceBlocker(main_structure);
                    return;
                }
            }
        }
        else
        {
            b = new Block(this, f_pos, main_structure, useMarker);
            if (blocks == null) blocks = new Dictionary<ChunkPos, Block>();
            blocks.Add(f_pos, b);
            return;
        }
    }
    public bool BlockRegion(List<ChunkPos> positions, Structure s, ref List<Block> dependentBlocks)
    {
        foreach (ChunkPos pos in positions)
        {
            if (blocks.ContainsKey(pos)) return false;
        }
        // все проверки пройдены
        Block b;
        foreach (ChunkPos pos in positions)
        {
            b = new Block(this, pos, s, true);
            blocks.Add(pos, b);
            dependentBlocks.Add(b);
        }
        return true;
    }
    public bool IsSpaceBlocked(ChunkPos cpos)
    {
        var b = GetBlock(cpos);
        return b != null && b.IsBlocker();
    }
    /// <summary>
    /// uses min coordinates ( left down corner); start positions including!
    /// </summary>
    public bool BlockShipCorridorIfPossible(int xpos, int ypos, bool xyAxis, int width, Structure sender, ref List<Block> dependentBlocksList)
    {
        int xStart = xpos; int xEnd = xStart + width;
        if (xStart < 0) xStart = 0; if (xEnd >= chunkSize) xEnd = chunkSize;
        int yStart = ypos; int yEnd = yStart + width;
        if (yStart < 0) yStart = 0; if (yEnd >= chunkSize) yEnd = chunkSize;
        if (xyAxis)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (blocks.ContainsKey(new ChunkPos(x, y, z))) return false;
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
                    for (int x = 0; x < chunkSize; x++)
                    {
                        if (blocks.ContainsKey(new ChunkPos(x, y, z))) return false;
                        //if (blocks[x, y, z] != null) DeleteBlock(new ChunkPos(x, y, z));
                    }
                }
            }
        }
        // все проверки на наличие блоков пройдены
        Block bk;
        if (xyAxis)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        ChunkPos cpos = new ChunkPos(x, y, z);
                        bk = new Block(this, cpos, sender, true);
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
                    for (int x = 0; x < chunkSize; x++)
                    {
                        ChunkPos cpos = new ChunkPos(x, y, z);
                       
                        bk = new Block(this, cpos, sender,true );
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
        if (xStart < 0) xStart = 0; if (xEnd >= chunkSize) xEnd = chunkSize;
        int yStart = startPoint.y, yEnd = startPoint.y + width;
        if (yStart < 0) yStart = 0; if (yEnd >= chunkSize) yEnd = chunkSize;
        int zStart = startPoint.z, zEnd = startPoint.z + width;
        if (zStart < 0) xStart = 0; if (zEnd >= chunkSize) zEnd = chunkSize;
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
                            for (int z = zStart; z < chunkSize; z++)
                            {
                                if (blocks.ContainsKey(new ChunkPos(x,y,z))) return false;
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z < chunkSize; z++)
                    {
                        if (blocks.ContainsKey(new ChunkPos(startPoint.x, startPoint.y, z))) return false;
                    }
                }
                break;
            case 2: // right
                if (width != 1)
                {
                    for (int x = xStart; x < chunkSize; x++)
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
                    for (int x = xStart; x < chunkSize; x++)
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
                            for (int z = zStart; z < chunkSize; z++)
                            {
                                var cpos = new ChunkPos(x, y, z);
                                bk = new Block(this, cpos, sender, true);
                                blocks.Add(cpos, bk);
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int z = zStart; z < chunkSize; z++)
                    {
                        var cpos = new ChunkPos(xStart, yStart, z);
                        bk = new Block(this, cpos, sender, true);
                        blocks.Add(cpos, bk);
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
            case 2: // right
                if (width != 1)
                {
                    for (int x = xStart; x < chunkSize; x++)
                    {
                        for (int y = yStart; y < yEnd; y++)
                        {
                            for (int z = zStart; z < zEnd; z++)
                            {
                                var cpos = new ChunkPos(x, y, z);
                                bk = new Block(this, cpos, sender, true);
                                blocks.Add(cpos, bk);
                                dependentBlocksList.Add(bk);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = xStart; x < chunkSize; x++)
                    {
                        var cpos = new ChunkPos(x, yStart, zStart);
                        bk = new Block(this, cpos, sender, true);
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
                                bk = new Block(this, cpos, sender, true);
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
                        bk = new Block(this, cpos, sender, true);
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
                                bk = new Block(this, cpos, sender, true);
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
                        bk = new Block(this, cpos, sender, true);
                        blocks.Add(cpos, bk);
                        dependentBlocksList.Add(bk);
                    }
                }
                break;
        }
        chunkDataUpdateRequired = true;
        return true;
    }
    public void ClearBlocksList(Structure s, List<Block> list, bool clearMainStructureField)
    {
        bool actions = false;
        foreach (Block b in list)
        {
            if (b != null)
            {
                if (clearMainStructureField) b.DropBlockerLink(s);
                // поле mainStructure чистится, чтобы блок не посылал SectionDeleted обратно структуре
                blocks.Remove(b.pos);
                b.Annihilate(false);                
                actions = true;
            }
        }
        if (actions) chunkDataUpdateRequired = true;
    }
    #endregion
    #endregion

    #region save-load system
    public void SaveChunkData(System.IO.FileStream fs)
    {
        fs.WriteByte(chunkSize);
        int count = 0;
        if (blocks != null)
        {
            var blist = new List<Block>();
            Block b = null;
            foreach (var fb in blocks)
            {
                b = fb.Value;
                if (b != null && !b.destroyed && b.haveExtension) blist.Add(b);
            }
            count = blist.Count;
            fs.Write(System.BitConverter.GetBytes(count),0,4);
            if (blist.Count > 0)
            {
                foreach (var bx in blist) bx.Save(fs);
            }
        }
        else fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        if (nature != null)
        {
            fs.WriteByte(1);
            nature.Save(fs);
        }
        else fs.WriteByte(0);
    }

    public void LoadChunkData(System.IO.FileStream fs)
    {
        if (blocks != null) ClearChunk();   
        chunkSize = (byte)fs.ReadByte();
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
        else
        {
            blocks = new Dictionary<ChunkPos, Block>();
            Block b;
            for (int i = 0; i < blocksCount; i++)
            {
                b = Block.Load(fs, this);
                if (b != null) {
                    blocks.Add(b.pos, b);
                }
            }
        }
        PreparePlanes();
        RecalculateSurfacesList();
        //
        var rb = fs.ReadByte();
        if (rb == 1)
        {
            if (nature == null) nature = GetNature();
            nature.Load(fs, this);
        }
        
        if (borderDrawn) DrawBorder();       
        
        RenderDataFullRecalculation();
        FollowingCamera.main.WeNeedUpdate();
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
