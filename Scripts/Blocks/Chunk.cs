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
public struct MeshVisualizeInfo
{
    public readonly byte faceIndex;
    public readonly byte illumination;
    public readonly MaterialType materialType;

    public MeshVisualizeInfo(byte i_face, MaterialType mtype, byte i_illumination)
    {
        faceIndex = i_face;
        illumination = i_illumination;
        materialType = mtype;
    }

    public static bool operator ==(MeshVisualizeInfo lhs, MeshVisualizeInfo rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(MeshVisualizeInfo lhs, MeshVisualizeInfo rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        MeshVisualizeInfo p = (MeshVisualizeInfo)obj;
        return (faceIndex == p.faceIndex) & (illumination == p.illumination) & (materialType == p.materialType);
    }
    public override int GetHashCode()
    {
        return faceIndex + illumination + (byte)materialType;
    }

    public override string ToString()
    {
        return (materialType.ToString() + " f:" + faceIndex.ToString() + " i:" + illumination.ToString() );
    }
}
public sealed class BlockpartVisualizeInfo 
{
    public readonly ChunkPos pos;
    public MeshVisualizeInfo rinfo;
    public MeshType meshType;
    public int materialID;

    public BlockpartVisualizeInfo(ChunkPos i_pos, MeshVisualizeInfo i_meshVI, MeshType i_meshType , int i_materialID)
    {
        pos = i_pos;
        rinfo = i_meshVI;
        meshType = i_meshType;
        materialID = i_materialID;
    }

    public Matrix4x4 GetPositionMatrix()
    {
        var faceVector = Vector3.zero;
        var rotation = Quaternion.identity;
        float step = Block.QUAD_SIZE * 0.5f;
        switch (rinfo.faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                faceVector = Vector3.forward * step;
                break;
            case Block.RIGHT_FACE_INDEX:
                faceVector = Vector3.right * step;
                rotation = Quaternion.Euler(0, 90,0);
                break;
            case Block.BACK_FACE_INDEX:
                faceVector = Vector3.back * step;
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case Block.LEFT_FACE_INDEX:
                faceVector = Vector3.left * step;
                rotation = Quaternion.Euler(0, 270,  0);
                break;
            case Block.UP_FACE_INDEX:
                faceVector = Vector3.up * step;
                rotation = Quaternion.Euler(-90, 0, 0);
                break;
            case Block.DOWN_FACE_INDEX:
                faceVector = Vector3.down * step;
                rotation = Quaternion.Euler(90, 0, 0);
                break;
            case Block.SURFACE_FACE_INDEX:
                faceVector = Vector3.down * step;
                rotation = Quaternion.Euler(-90, 0, 0);
                break;
            case Block.CEILING_FACE_INDEX:
                faceVector = Vector3.up * (0.5f - Block.CEILING_THICKNESS) * Block.QUAD_SIZE;
                rotation = Quaternion.Euler(90, 0, 0);
                break;
        }
        return Matrix4x4.TRS(
            pos.ToWorldSpace() + faceVector,
            rotation,
            Vector3.one * Block.QUAD_SIZE
            );
    }

    public static bool operator ==(BlockpartVisualizeInfo lhs, BlockpartVisualizeInfo rhs) {
        if (ReferenceEquals(lhs, null))
        {
            return ReferenceEquals(rhs, null);
        }
        else return lhs.Equals(rhs);
    }
    public static bool operator !=(BlockpartVisualizeInfo lhs, BlockpartVisualizeInfo rhs) {
        return !(lhs == rhs);
    }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        BlockpartVisualizeInfo p = (BlockpartVisualizeInfo)obj;
        return (p.pos == pos) && (p.rinfo == rinfo) && (meshType == p.meshType) && (materialID == p.materialID);
    }
    public override int GetHashCode()
    {
        return pos.GetHashCode() + 1000 * rinfo.faceIndex;
    }
}

public enum ChunkGenerationMode : byte { Standart, GameLoading, Cube, Peak, TerrainLoading, DontGenerate }

public sealed partial class Chunk : MonoBehaviour
{  
    public Dictionary<ChunkPos, Block > blocks;
    public byte prevBitmask = 63;
    public float lifePower = 0;
    public static byte CHUNK_SIZE { get; private set; }
    //private bool allGrasslandsCreated = false;
   
    public delegate void ChunkUpdateHandler();
    public event ChunkUpdateHandler ChunkUpdateEvent;

    private List<Plane> surfaces;

    public const float SUPPORT_POINTS_ENOUGH_FOR_HANGING = 2, CHUNK_UPDATE_TICK = 0.5f;
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
        surfaces = new List<Plane>();
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

    #region updating 
    private void LateUpdate()
    {
        if (chunkDataUpdateRequired)
        {
            if (ChunkUpdateEvent != null) ChunkUpdateEvent();           
            chunkDataUpdateRequired = false;
        }
        if (chunkRenderUpdateRequired)  RenderStatusUpdate();
        if (PoolMaster.shadowCasting & shadowsUpdateRequired)      ShadowsUpdate();
    }
    private void ShadowsUpdate()
    {
        var count = renderers.Count;
        if (count > 0)
        {
            CombineInstance[] ci = new CombineInstance[count];
            Quaternion or = Quaternion.identity;
            Vector3 scale = Vector3.one;
            GameObject g;
            int i = 0;
            foreach (var r in renderers)
            {
                g = r.Value;
                ci[i].mesh = g.GetComponent<MeshFilter>().sharedMesh;
                ci[i].transform = Matrix4x4.TRS(g.transform.position, or, scale);
                i++;
            }
            MeshFilter m = combinedShadowCaster.GetComponent<MeshFilter>();
            m.mesh = new Mesh();
            m.mesh.CombineMeshes(ci);
            combinedShadowCaster.SetActive(true);
        }
        else
        {
            combinedShadowCaster.SetActive(false);
        }
        shadowsUpdateRequired = false;
    }
    public void CullingUpdate()
    {
        Vector3 cpos = transform.InverseTransformPoint(FollowingCamera.camPos);
        Vector3 v = Vector3.one * (-1);
        float size = CHUNK_SIZE * Block.QUAD_SIZE;
        if (cpos.x > 0) { if (cpos.x > size) v.x = 1; else v.x = 0; }
        if (cpos.y > 0) { if (cpos.y > size) v.y = 1; else v.y = 0; }
        if (cpos.z > 0) { if (cpos.z > size) v.z = 1; else v.z = 0; }
        byte renderBitmask = 63;
        if (v != Vector3.zero)
        {
            //easy-culling	            
            if (v.x == 1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
            if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
            if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;            
        }
        if ((renderBitmask & 16) != 0) renderBitmask += 64;
        if ((renderBitmask & 32) != 0) renderBitmask += 128;
        var pot = GameConstants.powersOfTwo;
        if (renderBitmask != prevBitmask)
        {
            if (renderers.Count > 0)
            {
                bool visible;
                GameObject g;
                foreach (var r in renderers)
                {
                    visible = ((renderBitmask & pot[r.Key.faceIndex]) != 0);
                    g = r.Value;
                    if (g.activeSelf != visible) g.SetActive(visible);
                }
            }
            prevBitmask = renderBitmask;
        }
    }

    public void RenderStatusUpdate()
    {
        if (redrawRequiredTypes.Count > 0)
        {
            foreach (MeshVisualizeInfo mvi in redrawRequiredTypes)
            {
                if (renderers.ContainsKey(mvi))
                {
                    RedrawRenderer(mvi);
                }
                else
                {
                    CreateBlockpartsRenderer(mvi);
                }
            }
            redrawRequiredTypes.Clear();
        }
        chunkRenderUpdateRequired = false;
    }
    #endregion
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
            byte upcode = Block.UP_FACE_INDEX;
            Block b;
            Plane p = null;
            if (surfaces == null) surfaces = new List<Plane>(); else surfaces.Clear();
            foreach (var fb in blocks)
            {
                b = fb.Value;
                if (b.pos.y != Chunk.CHUNK_SIZE - 1 && b.TryGetPlane(upcode, out p) ) surfaces.Add(p);
            }
            if (surfaces.Count == 0) surfaces = null;
        }
    }

    public Block AddBlock(ChunkPos f_pos, Structure mainStructure)
    {
        var b = new Block(this, f_pos, mainStructure);
        if (blocks == null) blocks = new Dictionary<ChunkPos, Block>();
        blocks.Add(f_pos, b);
        if (PoolMaster.useIlluminationSystem)
        {
            int x = f_pos.x, y = f_pos.y, z = f_pos.z;
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
        }
        return b;
    }
    public Block AddBlock(ChunkPos f_pos, int[] materialsIDs, bool i_naturalGeneration )
    {
        var b = new Block(this,f_pos,)
    }
    public Block AddBlock(ChunkPos f_pos, int i_materialID, bool i_naturalGeneration)
    {
        int x = f_pos.x, y = f_pos.y, z = f_pos.z;
        if (x >= CHUNK_SIZE | y >= CHUNK_SIZE | z >= CHUNK_SIZE) return null;
        Block prv = GetBlock(x, y, z);
        if (prv != null)
        {
            prv.ChangeMaterial(i_materialID, i_naturalGeneration);
            return prv;
        }
        else
        {
            if (i_materialID == PoolMaster.NO_MATERIAL_ID) return AddBlock(f_pos, null);
            else
            {
                var b = new Block(this, f_pos, i_materialID, i_naturalGeneration);
                if (blocks == null) blocks = new Dictionary<ChunkPos, Block>();
                blocks.Add(f_pos, b);
                if (PoolMaster.useIlluminationSystem)
                {
                    lightMap[x, y, z] = 0;
                    RecalculateIlluminationAtPoint(b.pos);
                }
            }
        }

        byte influenceMask = 63; // видимость объекта, видимость стенок соседних объектов
        bool calculateUpperBlock = false;

        switch (f_type)
        {
            case BlockType.Surface:
                {
                    influenceMask = 31;

                    SurfaceBlock sb = new SurfaceBlock(this, f_pos, i_floorMaterialID);
                    b = sb;
                    blocks.Add(f_pos, sb);
                    surfaceBlocks.Add(sb);

                    if (PoolMaster.useIlluminationSystem)
                    {
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
                    }
                    break;
                }
            case BlockType.Cave:
                {
                    Block lowerBlock = GetBlock(x, y - 1, z);
                    if (lowerBlock == null) i_floorMaterialID = -1;
                    CaveBlock caveb = new CaveBlock(this, f_pos, i_ceilingMaterialID, i_floorMaterialID);
                    b = caveb;
                    blocks.Add(f_pos, caveb);
                    if (caveb.haveSurface) influenceMask = 15; else influenceMask = 47;
                    calculateUpperBlock = true;
                    surfaceBlocks.Add(caveb);

                    if (PoolMaster.useIlluminationSystem)
                    {
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
                    }
                    // eo cave light recalculation
                }
                break;
        }
        if (calculateUpperBlock)
        {
            var ub = GetBlock(x, y + 1, z);
            if (ub == null)
            {
                if (y < CHUNK_SIZE - 1) AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, i_ceilingMaterialID, i_ceilingMaterialID, i_naturalGeneration);
                else SetRoof(x, z, !i_naturalGeneration);
            }
            else
            {
                if (ub.type == BlockType.Cave && !(ub as CaveBlock).haveSurface)
                {
                    (ub as CaveBlock).RestoreSurface(i_ceilingMaterialID);
                }
            }
        }
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        RefreshBlockVisualising(b);
        if (f_type != BlockType.Shapeless) chunkRenderUpdateRequired = true;
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
        if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_SIZE || z < 0 || z >= CHUNK_SIZE) return null;
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
                    if ((GetVisibilityMask(blockpos.x, blockpos.y, blockpos.z) & powersOfTwo[Block.UP_FACE_INDEX]) == 0)
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
                                if (hitpoint.y < 0.5f - CaveBlock.CEILING_THICKNESS + 0.001f & normal == Vector3.down)
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
            Structure fillingStructure = originalBlock.blockingStructure;
        }
        blocks.Remove(originalBlock.pos);
        RemoveBlockVisualisers(originalBlock.pos);
        Block b = null;
        byte influenceMask = 63;
        bool calculateUpperBlock = false;
        switch (f_newType)
        {
            case BlockType.Shapeless:
                {
                    b = new Block(this, f_pos);
                    blocks.Add(f_pos, b);

                    if (PoolMaster.useIlluminationSystem)
                    {
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
                    }
                    break;
                }
            case BlockType.Surface:
                {
                    SurfaceBlock sb = new SurfaceBlock(this, f_pos, surfaceMaterial_id);
                    b = sb;
                    surfaceBlocks.Add(sb);
                    b = sb;
                    blocks.Add(f_pos, b);
                    influenceMask = 31;
                    if (originalBlock.type == BlockType.Cave)
                    {
                        (originalBlock as CaveBlock).TransferStructures(sb);
                    }
                    if (PoolMaster.useIlluminationSystem)
                    {
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
                    }
                    break;
                }
            case BlockType.Cube:
                {
                    CubeBlock cb = new CubeBlock(this, f_pos, surfaceMaterial_id, naturalGeneration);
                    b = cb;
                    blocks.Add(f_pos, cb);
                    influenceMask = 0;
                    calculateUpperBlock = true;
                    if (PoolMaster.useIlluminationSystem)
                    {
                        lightMap[x, y, z] = 0;
                        RecalculateIlluminationAtPoint(b.pos);
                    }
                }
                break;
            case BlockType.Cave:
                {
                    CaveBlock cvb = new CaveBlock(this, f_pos, ceilingMaterial_id, surfaceMaterial_id);
                    surfaceBlocks.Add(cvb);
                    b = cvb;
                    blocks.Add(f_pos, b);
                    
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

                    if (PoolMaster.useIlluminationSystem)
                    {
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
                }
                break;
        }
        originalBlock.Annihilate();
        originalBlock = null;
        ApplyVisibleInfluenceMask(x, y, z, influenceMask);
        if (calculateUpperBlock)
        {
            if (GetBlock(x, y + 1, z) == null)
            {
                if (y < CHUNK_SIZE - 1) AddBlock(new ChunkPos(x, y + 1, z), BlockType.Surface, surfaceMaterial_id, naturalGeneration);
                else SetRoof(x, z, !naturalGeneration);
            }
        }
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        RefreshBlockVisualising(b);
        chunkRenderUpdateRequired = true;
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
    public SurfaceBlock GetRandomSurfaceBlock()
    {
        int x = surfaceBlocks.Count;
        if (x == 0) return null;
        else
        {
            return surfaceBlocks[Random.Range(0, x - 1)];
        }
    }
    public SurfaceBlock GetNearestUnoccupiedSurfaceBlock( ChunkPos origin)
    {
        if (surfaceBlocks.Count == 0) return null;
        else
        {
            int nindex = -1; float minSqr = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
            float a, b, c, m;
            SurfaceBlock sb = null;
            for (int i = 0; i < surfaceBlocks.Count; i++)
            {
                sb = surfaceBlocks[i];                
                if (sb != null) {
                    if (sb.noEmptySpace == true) continue;
                    a = sb.pos.x - origin.x; 
                    b = sb.pos.y - origin.y; 
                    c = sb.pos.z - origin.z;
                    m = a * a + b * b + c * c;
                    if (m < minSqr)
                    {
                        minSqr = m;
                        nindex = i;
                    }
                }
            }
            if (nindex > 0) return surfaceBlocks[nindex];
            else return null;
        }
    }
    /// <summary>
    /// Seek a position for structure somewhere. Returns (xpos, zpos, surface_block_index)
    /// </summary>
    public bool TryGetPlace(ref Vector3Int answer, byte size)
    {
        if (surfaceBlocks.Count == 0) return false;
        var suitable = new List<int>();
        int i = 0;
        if (size == SurfaceBlock.INNER_RESOLUTION)
        {
            for (; i < surfaceBlocks.Count; i++)
            {
                if (surfaceBlocks[i].artificialStructures == 0) suitable.Add(i);
            }
            answer = new Vector3Int(0, 0, suitable[Random.Range(0, suitable.Count - 1)]);
            return true;
        }
        else
        {
            if (size == 1)
            {
                for (; i < surfaceBlocks.Count; i++)
                {
                    if (surfaceBlocks[i].noEmptySpace != true) suitable.Add(i);
                }
                i = Random.Range(0, suitable.Count - 1);
                int realIndex = suitable[i];
                var ppos = surfaceBlocks[realIndex].GetRandomCell();
                answer = new Vector3Int(ppos.x, ppos.y, i);
                return true;
            }
            else
            {
                for (; i < surfaceBlocks.Count; i++)
                {
                    if (surfaceBlocks[i].noEmptySpace != true) suitable.Add(i);
                }
                PixelPosByte ppos = PixelPosByte.Empty;
                int realIndex = 0;
                while (suitable.Count > 0)
                {
                    i = Random.Range(0, suitable.Count - 1);
                    realIndex = suitable[i];
                    ppos = surfaceBlocks[realIndex].GetRandomPosition(size);
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

    public void DeleteBlock(ChunkPos pos)
    {
        // в сиквеле стоит пересмотреть всю иерархию классов ><
        //12.06 нет, я так не думаю
        // 24.04.2019 фига сколько времени прошло
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
                chunkRenderUpdateRequired = true;
                break;
            case BlockType.Surface:
                {
                    SurfaceBlock sb = b as SurfaceBlock;
                    if (sb.noEmptySpace != false)
                    {
                        foreach (Structure s in sb.structures)
                        {
                            if (s != null && s.indestructible)
                            {
                                if (GetBlock(x,y-1,z) == null)
                                {
                                    Block ub = AddBlock(new ChunkPos(x, y - 1, z), BlockType.Shapeless, ResourceType.METAL_S_ID, false);
                                    GameObject g = PoolMaster.GetFlyingPlatform();
                                    g.transform.position = ub.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE / 2f;
                                    if (ub != null) ub.AddDecoration(g);
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
                    chunkRenderUpdateRequired = true;
                    break;
                }
            case BlockType.Cave:
                neighboursInfluence = true;
                upperBlockInfluence = true;
                if ((b as CaveBlock).haveSurface) lowerBlockInfluence = true;
                chunkRenderUpdateRequired = true;
                break;
        }
        b.Annihilate();
        blocks.Remove(b.pos);
        RemoveBlockVisualisers(b.pos);
        ApplyVisibleInfluenceMask(x, y, z, 63);

        neighboursInfluence = false; // отложено
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
        if (PoolMaster.useIlluminationSystem) ChunkLightmapFullRecalculation();
        chunkDataUpdateRequired = true;
        shadowsUpdateRequired = true;
        // chunkRenderUpdateRequired = true; < в свитче
    }
    public void ClearChunk()
    {
        if (blocks.Count > 0)
        {
            foreach (var b in blocks) { b.Value.Annihilate(); }
            blocks.Clear();
        }
        blocks = new Dictionary<ChunkPos, Block>();
        surfaceBlocks.Clear();
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
                b = AddBlock(pos, BlockType.Shapeless, -1, false);
                b.SetMainStructure(s);
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
                b.Annihilate();                
                actions = true;
            }
        }
        if (actions) chunkDataUpdateRequired = true;
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
                    if (sb.noEmptySpace != false)
                    {
                        foreach (Structure s in sb.structures)
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
                    if (sb.noEmptySpace != false)
                    {
                        foreach (Structure s in sb.structures)
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
                    if (sb.noEmptySpace != false)
                    {
                        foreach (Structure s in sb.structures)
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
                    if (sb.noEmptySpace != false)
                    {
                        foreach (Structure s in sb.structures)
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
                    float lval = lifepowerPerBlock * ((1 - (sb.pos.ToWorldSpace() - lifeSourcePos).magnitude / CHUNK_SIZE) * 0.5f + 0.5f);
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
        foreach (var block in blocks)
        {
            Block b = block.Value;
            if (b != null && b.type != BlockType.Shapeless) saveableBlocks.Add(b);
            else continue;
        }
        int count = saveableBlocks.Count;
        fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        if (count > 0)
        {
            foreach (Block b in saveableBlocks)
            {
                b.Save(fs);
            }
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
        if (blocksCount > 1000000)
        {
            Debug.Log("chunk load error - too much blocks");
            GameMaster.LoadingFail();
            return;
        }

        if (blocksCount > 0) {
            var loadedBlocks = new Block[blocksCount];
            BlockType type;
            ChunkPos pos;
            int materialID;
            data = new byte[8];
            for (int i = 0; i < blocksCount; i++)
            {
                if (GameMaster.loadingFailed)
                {
                    Destroy(this);
                    return;
                }
                fs.Read(data, 0, data.Length);
                type = (BlockType)data[0];
                pos = new ChunkPos(data[1], data[2], data[3]);
                
                materialID = System.BitConverter.ToInt32(data, 4);
                switch (type)
                {
                    case BlockType.Cube:
                        {
                            CubeBlock cb = new CubeBlock(this, pos, materialID, false);
                            blocks.Add(pos, cb);
                            cb.LoadCubeBlockData(fs);
                            loadedBlocks[i] = cb;
                            break;
                        }
                    case BlockType.Surface:
                        {
                            SurfaceBlock sb = new SurfaceBlock(this, pos, materialID);
                            blocks.Add(pos, sb);
                            sb.LoadSurfaceBlockData(fs);
                            loadedBlocks[i] = sb;
                            surfaceBlocks.Add(sb);
                            break;
                        }
                    case BlockType.Cave:
                        {
                            var cdata = new byte[4];
                            fs.Read(cdata, 0, 4);
                            int ceilingMaterial = System.BitConverter.ToInt32(cdata, 0);
                            CaveBlock cvb = new CaveBlock(this, pos, ceilingMaterial, materialID);
                            blocks.Add(pos, cvb);                                                     
                            cvb.LoadSurfaceBlockData(fs);
                            loadedBlocks[i] = cvb;
                            surfaceBlocks.Add(cvb);
                            break;
                        }
                    case BlockType.Shapeless:
                        {
                            print("shapeless block wrote - save corrupted");
                            pos = new ChunkPos(data[1], data[2], data[3]);
                            print(pos);
                            GameMaster.loadingFailed = true;
                            return;
                        }

                    default: continue;
                }
            }
            bool corruptedData = false;
            foreach (Block b in loadedBlocks) {
                if (b == null & !corruptedData)
                {
                    Debug.Log("chunkload - corrupted data");
                    corruptedData = true;
                    return;
                }                
            } // ужасное решение
            if (PoolMaster.useIlluminationSystem) ChunkLightmapFullRecalculation();
            RenderDataFullRecalculation();
        }
        if (surfaceBlocks.Count > 0)
        {
            foreach (SurfaceBlock sb in surfaceBlocks)
            {
                if (sb.noEmptySpace != false)
                {
                    foreach (Structure s in sb.structures)
                    {
                        if (s.isBasement)
                        {
                            BlockRendererController brc = s.transform.GetChild(0).GetComponent<BlockRendererController>();
                            if (brc != null) {
                                ChunkPos cpos = s.basement.pos;
                                brc.SetVisibilityMask(GetVisibilityMask(cpos));
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
        if (roofsCount > CHUNK_SIZE * CHUNK_SIZE)
        {
            Debug.Log("chunk loading error - too much roofs");
            GameMaster.LoadingFail();
            return;
        }
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
