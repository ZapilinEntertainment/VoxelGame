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
            case 0:
                faceVector = Vector3.forward * step;
                break;
            case 1:
                faceVector = Vector3.right * step;
                rotation = Quaternion.Euler(0, 90,0);
                break;
            case 2:
                faceVector = Vector3.back * step;
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 3:
                faceVector = Vector3.left * step;
                rotation = Quaternion.Euler(0, 270,  0);
                break;
            case 4:
                faceVector = Vector3.up * step;
                rotation = Quaternion.Euler(-90, 0, 0);
                break;
            case 5:
                faceVector = Vector3.down * step;
                rotation = Quaternion.Euler(90, 0, 0);
                break;
            case 6:
                faceVector = Vector3.down * step;
                rotation = Quaternion.Euler(-90, 0, 0);
                break;
            case 7:
                faceVector = Vector3.up * (0.5f - CaveBlock.CEILING_THICKNESS) * Block.QUAD_SIZE;
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

public sealed class Chunk : MonoBehaviour
{  
    public Dictionary<ChunkPos, Block> blocks;
    public List<SurfaceBlock> surfaceBlocks { get; private set; }
    public byte prevBitmask = 63;
    public float lifePower = 0;
    public static byte CHUNK_SIZE { get; private set; }
    //private bool allGrasslandsCreated = false;
    public byte[,,] lightMap { get; private set; }
    public int MAX_BLOCKS_COUNT = 100;
    public delegate void ChunkUpdateHandler();
    public event ChunkUpdateHandler ChunkUpdateEvent;

    private float LIGHT_DECREASE_PER_BLOCK;
    private bool chunkDataUpdateRequired = false, borderDrawn = false, shadowsUpdateRequired, chunkRenderUpdateRequired = false;
    private Dictionary<MeshVisualizeInfo, GameObject> renderers; // (face, material, illumitation) <- носители скомбинированных моделей
    private List<BlockpartVisualizeInfo> blockVisualizersList;// <- информация обо всех видимых частях блоков
    private List<MeshVisualizeInfo> redrawRequiredTypes; // <- будут перерисованы и снова скомбинированы
    private Roof[,] roofs;
    private GameObject roofObjectsHolder, combinedShadowCaster;
    private GameObject[] renderersHolders; // 6 холдеров для каждой стороны куба + 1 нестандартная

    public const float SUPPORT_POINTS_ENOUGH_FOR_HANGING = 2, CHUNK_UPDATE_TICK = 0.5f;
    public const byte MIN_CHUNK_SIZE = 3 ,UP_LIGHT = 255, BOTTOM_LIGHT = 128, NO_FACE_VALUE = 10;
    public const string BLOCK_COLLIDER_TAG = "BlockCollider";

    private static readonly byte[] powersOfTwo = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };

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
        surfaceBlocks = new List<SurfaceBlock>();
        redrawRequiredTypes = new List<MeshVisualizeInfo>();
        blockVisualizersList = new List<BlockpartVisualizeInfo>();
        roofs = new Roof[CHUNK_SIZE, CHUNK_SIZE];
        if (roofObjectsHolder == null)
        {
            roofObjectsHolder = new GameObject("roofObjectsHolder");
            roofObjectsHolder.transform.parent = transform;
            roofObjectsHolder.transform.localPosition = Vector3.zero;
            roofObjectsHolder.transform.localRotation = Quaternion.identity;
        }
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

        Grassland.ScriptReset();
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
                    if (Grassland.MaterialIsLifeSupporting(sb.material_id) && sb.grassland == null && sb.worksite == null) dirt_for_grassland.Add(sb);
                }
                if (dirt_for_grassland.Count > 0)
                {
                    int pos = Random.Range(0, dirt_for_grassland.Count);
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
        if (renderBitmask != prevBitmask)
        {
            if (renderers.Count > 0)
            {
                bool visible;
                GameObject g;
                foreach (var r in renderers)
                {
                    visible = ((renderBitmask & powersOfTwo[r.Key.faceIndex]) != 0);
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

    #region visualising
    public byte GetLightValue(int x, int y, int z)
    {
        if (!PoolMaster.useIlluminationSystem) return UP_LIGHT;
        else
        {
            if (y < 0) return BOTTOM_LIGHT;
            else
            {
                byte sz = CHUNK_SIZE;
                sz--;
                if (x < 0 || z < 0 || x > sz || y > sz || z > sz) return UP_LIGHT;
                else return lightMap[x, y, z];
            }
        }
    }
    public byte GetLightValue(ChunkPos cpos) { return GetLightValue(cpos.x, cpos.y, cpos.z); }

    public byte GetVisibilityMask(ChunkPos cpos) { return GetVisibilityMask(cpos.x, cpos.y, cpos.z); }
    public byte GetVisibilityMask(int x, int y, int z)
    {
        if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_SIZE || z < 0 || z >= CHUNK_SIZE) return 255;
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
            var blockInPosition = GetBlock(x, y, z);
            bx = GetBlock(x, y + 1, z);
            bool visionBlocked = bx != null && bx.type != BlockType.Shapeless;
            if (!visionBlocked & y != CHUNK_SIZE - 1) vmask += 16;
            else
            {
                if (GameMaster.layerCutHeight == y & (blockInPosition != null && blockInPosition.type == BlockType.Cube)) vmask += 16;
            }

            bx = GetBlock(x, y - 1, z);
            if (bx == null) vmask += 32;
            else
            {
                if (bx.type == BlockType.Surface)
                {
                    SurfaceBlock sb = bx as SurfaceBlock;
                    if (!sb.haveSupportingStructure) vmask += 32;
                }
                else
                {
                    if (bx.type == BlockType.Shapeless) vmask += 32;
                }
            }
            if (vmask != 0)
            {
                vmask += powersOfTwo[6]; // surface
                vmask += powersOfTwo[7]; // cave ceiling
            }
            return vmask;
        }
    }
    public void ApplyVisibleInfluenceMask(int x, int y, int z, byte mask)
    {
        Block b = GetBlock(x, y, z + 1); if (b != null) RefreshBlockVisualising(b);
        b = GetBlock(x + 1, y, z); if (b != null) RefreshBlockVisualising(b);
        b = GetBlock(x, y, z - 1); if (b != null) RefreshBlockVisualising(b);
        b = GetBlock(x - 1, y, z); if (b != null) RefreshBlockVisualising(b);
        b = GetBlock(x, y + 1, z); if (b != null) RefreshBlockVisualising(b);
        b = GetBlock(x, y - 1, z); if (b != null) RefreshBlockVisualising(b);
    }

    public void ChangeBlockVisualData(Block b, byte face)
    {
        if (b == null) return;
        byte visibilityMask = GetVisibilityMask(b.pos.x, b.pos.y, b.pos.z);
        BlockpartVisualizeInfo currentBlockInfo = null;
        int arrayIndex = -1;
        for (int i = 0; i < blockVisualizersList.Count; i++)
        {
            var bvi = blockVisualizersList[i];
            if (bvi.pos == b.pos && bvi.rinfo.faceIndex == face)
            {
                currentBlockInfo = bvi;
                arrayIndex = i;
                break;
            }
        }

        if ((visibilityMask & powersOfTwo[face]) != 0) // должен быть видимым
        {
            if (currentBlockInfo == null)
            {
                currentBlockInfo = b.GetFaceVisualData(face);
                if (currentBlockInfo == null) return;
                blockVisualizersList.Add(currentBlockInfo);                
                if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
            }
            else
            {
                if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
                currentBlockInfo = b.GetFaceVisualData(face);
                blockVisualizersList[arrayIndex] = currentBlockInfo;
                if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
            }
        }
        else // не должен быть виден
        {
            if (currentBlockInfo != null)
            {
                blockVisualizersList.RemoveAt(arrayIndex);
                if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
            }
        }
        chunkRenderUpdateRequired = true;
    }
    public void RefreshBlockVisualising(Block b)
    {
        byte visibilityMask = GetVisibilityMask(b.pos);
        var blockParts = new BlockpartVisualizeInfo[8];
        var indexes = new int[8];

        if (blockVisualizersList.Count > 0)
        {                        
            for (int i = 0; i < blockVisualizersList.Count; i++)
            {
                var bvi = blockVisualizersList[i];
                if (bvi.pos == b.pos)
                {
                    byte findex = bvi.rinfo.faceIndex;
                    blockParts[findex] = bvi;
                    indexes[findex] = i;
                }
            }           
        }
        BlockpartVisualizeInfo currentBlockInfo, correctBlockInfo;

        for (byte k = 0; k < 8; k++)
        {            
            currentBlockInfo = blockParts[k];
            if ((visibilityMask & powersOfTwo[k]) != 0) // должен быть видимым
            {               
                correctBlockInfo = b.GetFaceVisualData(k);
                if (currentBlockInfo != null) // данные о блоке есть..
                {
                    if (correctBlockInfo == null) // ...но их быть не должно
                    {
                        blockVisualizersList.RemoveAt(indexes[k]);
                        if (k + 1 < 8)
                        {
                            for (int j = k + 1; j < 8; j++)
                            {
                                if (indexes[j] > indexes[k]) indexes[j]--;
                            }
                        }
                        if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
                    }
                    else // ...и мы сравниваем их с правильными
                    {
                        if (correctBlockInfo != currentBlockInfo)
                        {                            
                            if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
                            blockVisualizersList[indexes[k]] = currentBlockInfo;                            
                            if (!redrawRequiredTypes.Contains(correctBlockInfo.rinfo)) redrawRequiredTypes.Add(correctBlockInfo.rinfo);
                        }
                    }
                }
                else // данных о блоке нет...
                {
                    if (correctBlockInfo == null) continue; //...и не должно быть
                    else //... но должны быть
                    {                        
                        blockVisualizersList.Add(correctBlockInfo);
                        if (!redrawRequiredTypes.Contains(correctBlockInfo.rinfo)) redrawRequiredTypes.Add(correctBlockInfo.rinfo);
                    }
                }
            }
            else // должен быть невидимым..
            {
                if (currentBlockInfo != null) //.. но есть видимая часть, которую нужно удалить
                {
                    blockVisualizersList.RemoveAt(indexes[k]);
                    if (k + 1 < 8)
                    {
                        for (int j = k + 1; j < 8; j++)
                        {
                            if (indexes[j] > indexes[k]) indexes[j]--;
                        }
                    }
                    if (!redrawRequiredTypes.Contains(currentBlockInfo.rinfo)) redrawRequiredTypes.Add(currentBlockInfo.rinfo);
                }
            }
        }
        chunkRenderUpdateRequired = true;
    }
    private void RemoveBlockVisualisers(ChunkPos cpos) // удаление всей рендер-информации для данной точки
    {
        if (blockVisualizersList.Count > 0)
        {
            BlockpartVisualizeInfo bvi;
            int i = 0;
            while (i < blockVisualizersList.Count)
            {
                bvi = blockVisualizersList[i];
                if (bvi.pos == cpos)
                {
                    var ri = bvi.rinfo;
                    if (!redrawRequiredTypes.Contains(ri))
                    {                        
                        redrawRequiredTypes.Add(ri);
                    }
                    blockVisualizersList.RemoveAt(i);
                    continue;
                }
                else i++;
            }
            chunkRenderUpdateRequired = true;
        }
    }

    private void CreateBlockpartsRenderer(MeshVisualizeInfo mvi)
    {
        if (renderers.ContainsKey(mvi)) return;
        var processingIndexes = new List<int>();
        for (int i = 0; i < blockVisualizersList.Count; i++)
        {
            if (blockVisualizersList[i].rinfo == mvi) processingIndexes.Add(i);
        }

        int pcount = processingIndexes.Count;
        if (pcount > 0)
        {
            var ci = new CombineInstance[pcount];
            Mesh m;

            for (int j = 0; j < pcount; j++)
            {
                var cdata = blockVisualizersList[processingIndexes[j]];
                m = PoolMaster.GetMesh(cdata.meshType, cdata.materialID);
                ci[j].mesh = m;                
                ci[j].transform = cdata.GetPositionMatrix();
            }

            GameObject g = new GameObject();
            m = new Mesh();
            m.CombineMeshes(ci, true); // все подмеши используют один материал
            
            //удаление копий вершин на стыках - отменено из-за uv

            var mf =g.AddComponent<MeshFilter>();
            mf.sharedMesh = m;

            var mr = g.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = PoolMaster.shadowCasting;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            if (!PoolMaster.useIlluminationSystem) mr.sharedMaterial = PoolMaster.GetMaterial(mvi.materialType);
            else mr.sharedMaterial = PoolMaster.GetMaterial(mvi.materialType, mvi.illumination);
            
            g.transform.parent = renderersHolders[mvi.faceIndex].transform;
            g.AddComponent<MeshCollider>().sharedMesh = m;
            g.tag = BLOCK_COLLIDER_TAG;

            renderers.Add(mvi, g);
        }
    }
    private void RedrawRenderer(MeshVisualizeInfo mvi)
    {
        GameObject g;
        renderers.TryGetValue(mvi, out g);
        if (g != null)
        {
            int n = blockVisualizersList.Count;
            if (n > 0)
            {
                var indexes = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    if (blockVisualizersList[i].rinfo == mvi) indexes.Add(i);
                }

                n = indexes.Count;
                if (n > 0)
                {
                    var ci = new CombineInstance[n];
                    BlockpartVisualizeInfo bvi;
                    for (int i = 0; i < n; i++)
                    {                        
                        bvi = blockVisualizersList[indexes[i]];                        
                        Mesh m = PoolMaster.GetMesh(bvi.meshType, bvi.materialID);
                        ci[i].mesh = m;
                        ci[i].transform = bvi.GetPositionMatrix();
                    }
                    Mesh cm = new Mesh();
                    cm.CombineMeshes(ci);
                    g.GetComponent<MeshFilter>().sharedMesh = cm;
                    g.GetComponent<MeshCollider>().sharedMesh = cm;
                    if (PoolMaster.useIlluminationSystem) g.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetMaterial(mvi.materialType, mvi.illumination);
                    else g.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetMaterial(mvi.materialType);
                }
                else
                {                    
                    renderers.Remove(mvi);
                    Destroy(g);
                }
            }
        }
        else CreateBlockpartsRenderer(mvi);
    }

    public void RenderDataFullRecalculation()
    {
        RemakeRenderersHolders();
        if (renderers != null) renderers.Clear();
        renderers = new Dictionary<MeshVisualizeInfo, GameObject>();

        blockVisualizersList.Clear();
        blockVisualizersList = new List<BlockpartVisualizeInfo>();
        byte visibilityMask = 0;
        foreach (var b in blocks)
        {
            Block block = b.Value;
            if (block.type != BlockType.Shapeless)
            {
                visibilityMask = GetVisibilityMask(block.pos);
                if (visibilityMask != 0)
                {
                    if (block.type == BlockType.Surface)
                    {
                        var d = block.GetFaceVisualData(Block.SURFACE_FACE_INDEX);
                        if (d != null) blockVisualizersList.Add(d);
                    }
                    else
                    {
                        var d = block.GetVisualDataList(visibilityMask);
                        if (d != null) blockVisualizersList.AddRange(d);
                    }
                }
            }
        }
        int n = blockVisualizersList.Count;
        if (n > 0)
        {
            List<MeshVisualizeInfo> processedTypes = new List<MeshVisualizeInfo>();
            BlockpartVisualizeInfo brd;
            MeshVisualizeInfo ri;           
            
            int i = 0;
            for (; i < n; i++) {
                brd = blockVisualizersList[i];
                ri = brd.rinfo;
                if (!processedTypes.Contains(ri))
                {
                    CreateBlockpartsRenderer(ri);
                    processedTypes.Add(ri);
                }
            }
        }
        redrawRequiredTypes.Clear();
        chunkRenderUpdateRequired = false;
    }
    public void ChunkLightmapFullRecalculation()
    {
        if (!PoolMaster.useIlluminationSystem) return;
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
                    b = GetBlock(x,y,z);
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
                    b = GetBlock(x,y,z);
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
                    b = GetBlock(x,y,z);
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

        if (blockVisualizersList.Count > 0)
        {
            foreach (var brd in blockVisualizersList)
            {
                int a;
                byte lightToCompare;
                switch (brd.rinfo.faceIndex)
                {
                    case 0:
                        a = brd.pos.z + 1;
                        if (a >= CHUNK_SIZE) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[brd.pos.x, brd.pos.y, a];
                        break;
                    case 1:
                        a = brd.pos.x + 1;
                        if (a >= CHUNK_SIZE) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[a, brd.pos.y, brd.pos.z];
                        break;
                    case 2:
                        a = brd.pos.z - 1;
                        if (a < 0) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[brd.pos.x, brd.pos.y, a];
                        break;
                    case 3:
                        a = brd.pos.x - 1;
                        if (a < 0 ) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[a, brd.pos.y, brd.pos.z];
                        break;
                    case 4:
                        a = brd.pos.y + 1;
                        if (a >= CHUNK_SIZE) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[brd.pos.x, a, brd.pos.z];
                        break;
                    case 5:
                        a = brd.pos.y - 1;
                        if (a < 0) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[brd.pos.x, a, brd.pos.z];
                        break;
                    default:
                        lightToCompare = lightMap[brd.pos.x, brd.pos.y, brd.pos.z];
                        break;
                }
                if (brd.rinfo.illumination != lightToCompare && !redrawRequiredTypes.Contains(brd.rinfo)) {
                    redrawRequiredTypes.Add(brd.rinfo);
                }
            }
        }
    }
    public void RecalculateIlluminationAtPoint(ChunkPos pos)
    {
        ChunkLightmapFullRecalculation(); // в разработке
    }

    private void RemakeRenderersHolders()
    {
        if (renderersHolders != null)
        {
            Destroy(renderersHolders[0]);
            Destroy(renderersHolders[1]);
            Destroy(renderersHolders[2]);
            Destroy(renderersHolders[3]);
            Destroy(renderersHolders[4]);
            Destroy(renderersHolders[5]);
            Destroy(renderersHolders[6]);
        }
        renderersHolders = new GameObject[8];
        GameObject g = new GameObject("renderersHolder_face0");
        Transform t = g.transform;
        Vector3 vzero = Vector3.zero;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[0] = g;
        g = new GameObject("renderersHolder_face1");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[1] = g;
        g = new GameObject("renderersHolder_face2");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[2] = g;
        g = new GameObject("renderersHolder_face3");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[3] = g;
        g = new GameObject("renderersHolder_face4");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[4] = g;
        g = new GameObject("renderersHolder_face5");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[5] = g;
        g = new GameObject("renderersHolder_face6");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[6] = g;
        g = new GameObject("renderersHolder_face7");
        t = g.transform;
        t.parent = transform;
        t.localPosition = vzero;
        renderersHolders[7] = g;
    }
    public void LayersCut()
    {
        int layerCutHeight = GameMaster.layerCutHeight;
        roofObjectsHolder.SetActive(layerCutHeight == CHUNK_SIZE);
        RenderDataFullRecalculation();
        foreach (var sb in surfaceBlocks)
        {
            bool viewStatus = true;
            if (sb.pos.y > layerCutHeight) viewStatus = false;
            if (sb.noEmptySpace != false)
            {
                if (sb == null) continue;
                foreach (var s in sb.structures)
                {
                    if (s == null) continue;
                    else s.SetVisibility(viewStatus);
                }
            }
        }
    }
    public void SetShadowCastingMode(bool x)
    {
        if (x)
        {
            if (combinedShadowCaster == null)
            {
                combinedShadowCaster = new GameObject("combinedShadowCaster");
                combinedShadowCaster.AddComponent<MeshFilter>();
                var mr = combinedShadowCaster.AddComponent<MeshRenderer>();
                mr.sharedMaterial = Resources.Load<Material>("Materials/ShadowsOnly");
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                combinedShadowCaster.SetActive(false);                
            }
            shadowsUpdateRequired = true;
        }
        else
        {
            if (combinedShadowCaster != null) Destroy(combinedShadowCaster);
        }
    }
    #endregion 
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
        if (surfaceBlocks.Count > 0 & GameMaster.realMaster.gameMode != GameMode.Editor)
        {
            foreach (SurfaceBlock sb in surfaceBlocks)
            {
                GameMaster.geologyModule.SpreadMinerals(sb);
            }
        }
        RenderDataFullRecalculation();
        FollowingCamera.main.WeNeedUpdate();
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

        byte influenceMask = 63; // видимость объекта, видимость стенок соседних объектов
        bool calculateUpperBlock = false;

        Block b = null;
        switch (f_type)
        {
            case BlockType.Cube:
                {
                    CubeBlock cb = new CubeBlock(this, f_pos, i_floorMaterialID, i_naturalGeneration);
                    b = cb;
                    blocks.Add(f_pos, cb);
                    influenceMask = 0; // закрывает собой все соседние стенки
                    calculateUpperBlock = true;
                    i_ceilingMaterialID = i_floorMaterialID;

                    if (PoolMaster.useIlluminationSystem)
                    {
                        lightMap[x, y, z] = 0;
                        RecalculateIlluminationAtPoint(cb.pos);
                    }
                    break;
                }
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
    public void RecalculateSurfaceBlocks()
    {
        surfaceBlocks = new List<SurfaceBlock>();
        foreach (var block in blocks)
        {
            Block b = block.Value;
            if (b == null) continue;
            if (b.type == BlockType.Surface | b.type == BlockType.Cave) surfaceBlocks.Add(b as SurfaceBlock);
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


    public bool BlockByStructure(int x, int y, int z, Structure s)
    {        
        if ((x >= CHUNK_SIZE | x < 0) || (y >= CHUNK_SIZE | y < 0) || (z >= CHUNK_SIZE | z < 0) | (s == null)) return false;
        Block b = GetBlock(x, y, z);
        if (b != null)
        {
            if (b.type == BlockType.Shapeless)
            {
                b.SetMainStructure(s);
                return true;
            }
            else return false;
        }
        else
        {
            b = AddBlock(new ChunkPos(x, y, z), BlockType.Shapeless, -1, false);
            b.SetMainStructure(s);
            return true;
        }
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
                    newModel.transform.localPosition = new Vector3((x + 0.5f) * Block.QUAD_SIZE, (CHUNK_SIZE - 0.5f) * Block.QUAD_SIZE, (z + 0.5f) * Block.QUAD_SIZE);
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
