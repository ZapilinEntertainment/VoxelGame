using UnityEngine;
using System.Collections.Generic;

//RENDERING PART
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
        return (materialType.ToString() + " f:" + faceIndex.ToString() + " i:" + illumination.ToString());
    }
}
public sealed class BlockpartVisualizeInfo
{
    public readonly ChunkPos pos;
    public MeshVisualizeInfo meshInfo;
    public MeshType meshType;
    public int materialID;
    public byte meshRotation;  

    public BlockpartVisualizeInfo(ChunkPos i_pos, MeshVisualizeInfo i_meshVI, MeshType i_meshType, int i_materialID, byte i_meshRotation)
    {
        pos = i_pos;
        meshInfo = i_meshVI;
        meshType = i_meshType;
        materialID = i_materialID;
        meshRotation = i_meshRotation;
    }

    public Matrix4x4 GetPositionMatrix()
    {
        var faceVector = Vector3.zero;
        var rotation = Quaternion.identity;
        float step = Block.QUAD_SIZE * 0.5f;
        switch (meshInfo.faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                faceVector = Vector3.forward * step;
                rotation = Quaternion.Euler(0f, 0f, meshRotation * 90f);
                break;
            case Block.RIGHT_FACE_INDEX:
                faceVector = Vector3.right * step;
                rotation = Quaternion.Euler(0, 90, meshRotation * 90f);
                break;
            case Block.BACK_FACE_INDEX:
                faceVector = Vector3.back * step;
                rotation = Quaternion.Euler(0, 180, meshRotation * 90f);
                break;
            case Block.LEFT_FACE_INDEX:
                faceVector = Vector3.left * step;
                rotation = Quaternion.Euler(0, 270, meshRotation * 90f);
                break;
            case Block.UP_FACE_INDEX:
                faceVector = Vector3.up * step;
                rotation = Quaternion.Euler(-90, 0, meshRotation * 90f);
                break;
            case Block.DOWN_FACE_INDEX:
                faceVector = Vector3.down * step;
                rotation = Quaternion.Euler(90, 0, meshRotation * 90f);
                break;
            case Block.SURFACE_FACE_INDEX:
                faceVector = Vector3.down * step;
                rotation = Quaternion.Euler(-90, 0, meshRotation * 90f);
                break;
            case Block.CEILING_FACE_INDEX:
                faceVector = Vector3.up * (0.5f - Block.CEILING_THICKNESS) * Block.QUAD_SIZE;
                rotation = Quaternion.Euler(90, 0, meshRotation * 90f);
                break;
        }
        return Matrix4x4.TRS(
            pos.ToWorldSpace() + faceVector,
            rotation,
            Vector3.one * Block.QUAD_SIZE
            );
    }

    public static bool operator ==(BlockpartVisualizeInfo lhs, BlockpartVisualizeInfo rhs)
    {
        if (ReferenceEquals(lhs, null))
        {
            return ReferenceEquals(rhs, null);
        }
        else return lhs.Equals(rhs);
    }
    public static bool operator !=(BlockpartVisualizeInfo lhs, BlockpartVisualizeInfo rhs)
    {
        return !(lhs == rhs);
    }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        BlockpartVisualizeInfo p = (BlockpartVisualizeInfo)obj;
        return (p.pos == pos) && (p.meshInfo == meshInfo) && (meshType == p.meshType) && (materialID == p.materialID) && (meshRotation == p.meshRotation);
    }
    public override int GetHashCode()
    {
        return pos.GetHashCode() + 1000 * meshInfo.faceIndex;
    }
}

//ПОРЯДОК ВАЖЕН! Большинство проверок строятся на числовом сравнении!
public enum VisibilityMode : byte { DrawAll,LayerCutCancel, SmallObjectsLOD, MediumObjectsLOD, HugeObjectsLOD, LayerCutHide,Invisible, }

public sealed partial class Chunk : MonoBehaviour {
    public byte[,,] lightMap { get; private set; }
    public bool needCameraUpdate = false;

    private float LIGHT_DECREASE_PER_BLOCK;
    private bool chunkDataUpdateRequired = false, borderDrawn = false, shadowsUpdateRequired, chunkRenderUpdateRequired = false;
    private Dictionary<MeshVisualizeInfo, GameObject> renderers; // (face, material, illumitation) <- носители скомбинированных моделей
    private List<BlockpartVisualizeInfo> blockVisualizersList;// <- информация обо всех видимых частях блоков
    private List<MeshVisualizeInfo> redrawRequiredTypes; // <- будут перерисованы и снова скомбинированы
    private GameObject combinedShadowCaster;
    private GameObject[] renderersHolders; // 6 холдеров для каждой стороны куба + 1 нестандартная
    public Transform markersHolder { get; private set; }
    private float[,,] distancesArray;

    public const byte UP_LIGHT = 255, BOTTOM_LIGHT = 128;
    // visibility distances
    public const float SMALL_OBJECTS_HIDE_DISTANCE_SQR = 128, MEDIUM_OBJECTS_LOD_DISTANCE_SQR = 200, HUGE_OBJECTS_LOD_DISTANCE_SQR = 600;

    private void InitializeMarkersHolder()
    {
        if (markersHolder != null) return;
        markersHolder = new GameObject("markersHolder").transform;
        markersHolder.transform.parent = transform;
        markersHolder.transform.localPosition = Vector3.zero;
        markersHolder.transform.localRotation = Quaternion.identity;
    }

    public void RenderDataFullRecalculation()
    {
        RemakeRenderersHolders();
        if (renderers != null) renderers.Clear();
        renderers = new Dictionary<MeshVisualizeInfo, GameObject>();

        blockVisualizersList.Clear();
        blockVisualizersList = new List<BlockpartVisualizeInfo>();
        byte visibilityMask;
        List<BlockpartVisualizeInfo> gdata;
        foreach (var fb in blocks)
        {
            Block b = fb.Value;
            visibilityMask = GetVisibilityMask(b.pos);
            if (visibilityMask != 0)
            {
                gdata = b.GetVisualizeInfo(visibilityMask);
                if (gdata != null) blockVisualizersList.AddRange(gdata);
            }
        }
        int n = blockVisualizersList.Count;
        if (n > 0)
        {
            List<MeshVisualizeInfo> processedTypes = new List<MeshVisualizeInfo>();
            BlockpartVisualizeInfo brd;
            MeshVisualizeInfo ri;

            int i = 0;
            for (; i < n; i++)
            {
                brd = blockVisualizersList[i];
                ri = brd.meshInfo;
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
    public byte GetVisibilityMask(ChunkPos cpos) { return GetVisibilityMask(cpos.x, cpos.y, cpos.z); }
    public byte GetVisibilityMask(int x, int y, int z)
    {
        if (y >= GameMaster.layerCutHeight) return 0;
        if (x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize) return 255;        
        else
        {
            byte vmask = 255;
            Block bx = GetBlock(x, y, z + 1);
            const byte fwd = 1 << Block.FWD_FACE_INDEX, right = 1 << Block.RIGHT_FACE_INDEX,
                back = 1 << Block.BACK_FACE_INDEX, left = 1 << Block.LEFT_FACE_INDEX,
                up = 1 << Block.UP_FACE_INDEX, down = 1 << Block.DOWN_FACE_INDEX,
                surf = 1 << Block.SURFACE_FACE_INDEX, ceil = 1 << Block.CEILING_FACE_INDEX;
            //sides:
            if (bx != null && !bx.IsFaceTransparent(Block.BACK_FACE_INDEX))
            {
                vmask -= fwd;
            }
            bx = GetBlock(x + 1, y, z);
            if (bx != null && !bx.IsFaceTransparent(Block.LEFT_FACE_INDEX))
            {
                vmask -= right;
            }
            bx = GetBlock(x, y, z - 1);
            if (bx != null && !bx.IsFaceTransparent(Block.FWD_FACE_INDEX))
            {
                vmask -= back;
            }
            bx = GetBlock(x - 1, y, z);
            if (bx != null && !bx.IsFaceTransparent(Block.RIGHT_FACE_INDEX))
            {
                vmask -= left;
            }
            // up and down
            bx = GetBlock(x, y + 1, z);
            if (GameMaster.layerCutHeight > y + 1 && bx != null && !bx.IsFaceTransparent(Block.DOWN_FACE_INDEX) )
            {
                vmask -= up;
            }
            bx = GetBlock(x, y - 1, z);
            if (bx != null && !bx.IsFaceTransparent(Block.UP_FACE_INDEX))
            {
                vmask -= down;
            }
            byte sidesMask = (byte)(fwd + right + back + left);
            if ((vmask & sidesMask) == 0) //ни одна боковая сторона не рисуется
            {
                if ((vmask & up) == 0) vmask -= surf;
                if ((vmask & down) == 0) vmask -= ceil;
            }
            return vmask;
        }
    }
    public bool IsUnderOtherBlock(Plane p)
    {
        Block b;
        ChunkPos pos = p.pos;
        byte face = p.faceIndex;
        switch (p.faceIndex)
        {
            case Block.FWD_FACE_INDEX: pos = pos.OneBlockForward(); face = Block.BACK_FACE_INDEX; break;
            case Block.RIGHT_FACE_INDEX: pos = pos.OneBlockRight(); face = Block.LEFT_FACE_INDEX; break;
            case Block.BACK_FACE_INDEX: pos = pos.OneBlockBack(); face = Block.FWD_FACE_INDEX; break;
            case Block.LEFT_FACE_INDEX: pos = pos.OneBlockLeft();face = Block.RIGHT_FACE_INDEX; break;
            case Block.UP_FACE_INDEX: pos = pos.OneBlockHigher(); face = Block.DOWN_FACE_INDEX; break;
            case Block.DOWN_FACE_INDEX: pos = pos.OneBlockDown();face = Block.UP_FACE_INDEX; break;
            default: return false;               
        }
        return (blocks.TryGetValue(pos, out b) && !b.IsFaceTransparent(face));
    }

    public void RecalculateVisibilityAtPoint(ChunkPos cpos, byte affectionMask)
    {
        var b = GetBlock(cpos); if (b != null) RefreshBlockVisualising(b);
        if (affectionMask == 0) return;
        if ((affectionMask & (1 << Block.FWD_FACE_INDEX)) != 0)
        {
            b = GetBlock(cpos.OneBlockForward());
            if (b != null) RefreshBlockVisualising(b);
        }
        if ((affectionMask & (1 << Block.RIGHT_FACE_INDEX)) != 0)
        {
            b = GetBlock(cpos.OneBlockRight());
            if (b != null) RefreshBlockVisualising(b);
        }
        if ((affectionMask & (1 << Block.BACK_FACE_INDEX)) != 0)
        {
            b = GetBlock(cpos.OneBlockBack());
            if (b != null) RefreshBlockVisualising(b);
        }
        if ((affectionMask & (1 << Block.LEFT_FACE_INDEX)) != 0)
        {
            b = GetBlock(cpos.OneBlockLeft());
            if (b != null) RefreshBlockVisualising(b);
        }
        if ((affectionMask & (1 << Block.UP_FACE_INDEX)) != 0)
        {
            b = GetBlock(cpos.OneBlockHigher());
            if (b != null) RefreshBlockVisualising(b);
        }
        if ((affectionMask & (1 << Block.DOWN_FACE_INDEX)) != 0)
        {
            b = GetBlock(cpos.OneBlockDown());
            if (b != null) RefreshBlockVisualising(b);
        }
    }
    public void RefreshBlockVisualising(Block b, byte face)
    {
        if (b == null) return;
        byte visibilityMask = GetVisibilityMask(b.pos);
        BlockpartVisualizeInfo currentBlockInfo = null;
        for (int i = 0; i < blockVisualizersList.Count; i++)
        {
            var bvi = blockVisualizersList[i];
            if (bvi.pos == b.pos && bvi.meshInfo.faceIndex == face)
            {
                currentBlockInfo = bvi;
                blockVisualizersList.RemoveAt(i);
            }
        }

        if ((visibilityMask & (1 << face)) != 0) // должен быть видимым
        {
            if (currentBlockInfo == null)
            {
                currentBlockInfo = b.GetFaceVisualData(face);
                if (currentBlockInfo == null) return;
                blockVisualizersList.Add(currentBlockInfo);
                if (!redrawRequiredTypes.Contains(currentBlockInfo.meshInfo)) redrawRequiredTypes.Add(currentBlockInfo.meshInfo);
            }
            else
            {
                if (!redrawRequiredTypes.Contains(currentBlockInfo.meshInfo)) redrawRequiredTypes.Add(currentBlockInfo.meshInfo);
                currentBlockInfo = b.GetFaceVisualData(face);
                blockVisualizersList.Add(currentBlockInfo);
                if (!redrawRequiredTypes.Contains(currentBlockInfo.meshInfo)) redrawRequiredTypes.Add(currentBlockInfo.meshInfo);
            }
        }
        else // не должен быть виден
        {
            if (currentBlockInfo != null)
            {
                if (!redrawRequiredTypes.Contains(currentBlockInfo.meshInfo)) redrawRequiredTypes.Add(currentBlockInfo.meshInfo);
            }
        }
        chunkRenderUpdateRequired = true;
    }
    public void RefreshBlockVisualising(Block b)
    {
        if (GameMaster.loading) return;
        var correctData = b.GetVisualizeInfo(GetVisibilityMask(b.pos));      
        var pos = b.pos;        

        if (correctData == null)
        { // нет видимых частей, удалить все, что относится к блоку            
            RemoveBlockVisualisers(pos);
            return;
        }
        else
        {
            int i = 0;
            bool corrections = false;
            MeshVisualizeInfo mvi;
            byte findex;
            var correctDataArray = new BlockpartVisualizeInfo[8];
            foreach (var fbvi in correctData)
            {
                findex = fbvi.meshInfo.faceIndex;
                correctDataArray[findex] = fbvi;
            }
            correctData = null;
            BlockpartVisualizeInfo bvi;
            while (i < blockVisualizersList.Count)
            {
                if (blockVisualizersList[i].pos == pos)
                {
                    bvi = blockVisualizersList[i];
                    mvi = bvi.meshInfo;
                    findex = bvi.meshInfo.faceIndex;
                    if (correctDataArray[findex] != null)
                    {
                        // замена существующих данных
                        if (bvi != correctDataArray[findex])
                        {
                            blockVisualizersList[i] = correctDataArray[findex];
                            corrections = true;
                            if (!redrawRequiredTypes.Contains(mvi)) redrawRequiredTypes.Add(mvi); // удаленный тип
                            mvi = correctDataArray[findex].meshInfo;
                            if (!redrawRequiredTypes.Contains(mvi)) redrawRequiredTypes.Add(mvi); // новый тип
                            correctDataArray[findex] = null; // все записанные стираются, незаписанные впишутся позже
                        }
                        i++;
                    }
                    else
                    {
                        blockVisualizersList.RemoveAt(i);
                        corrections = true;
                        if (!redrawRequiredTypes.Contains(mvi)) redrawRequiredTypes.Add(mvi);
                        continue;
                    }
                }
                else i++;
            }
            foreach (var fbvi in correctDataArray)
            {
                if (fbvi != null)
                {
                    blockVisualizersList.Add(fbvi);
                    corrections = true;
                    mvi = fbvi.meshInfo;
                    if (!redrawRequiredTypes.Contains(mvi)) redrawRequiredTypes.Add(mvi);
                }
            }
            if (corrections) chunkRenderUpdateRequired = true;
        }  
    }
    private void RemoveBlockVisualisers(ChunkPos cpos) // удаление всей рендер-информации для данного блока
    {
        bool corrections = false;
        MeshVisualizeInfo mvi;
        int i = 0;
        while (i < blockVisualizersList.Count)
        {
            if (blockVisualizersList[i].pos == cpos)
            {
                mvi = blockVisualizersList[i].meshInfo;
                if (!redrawRequiredTypes.Contains(mvi)) redrawRequiredTypes.Add(mvi);
                blockVisualizersList.RemoveAt(i);
                corrections = true;                
                continue;
            }
            else i++;
        }
        if (corrections) chunkRenderUpdateRequired = true;
    }

    private void CreateBlockpartsRenderer(MeshVisualizeInfo mvi)
    {
        if (renderers.ContainsKey(mvi)) return;
        var processingIndexes = new List<int>();
        for (int i = 0; i < blockVisualizersList.Count; i++)
        {
            if (blockVisualizersList[i].meshInfo == mvi) processingIndexes.Add(i);
        }

        int pcount = processingIndexes.Count;
        if (pcount > 0)
        {
            Mesh visibleMesh = new Mesh(), colliderMesh = new Mesh();
            INLINE_CombineMeshes(ref processingIndexes, ref visibleMesh, ref colliderMesh);
            GameObject g = new GameObject();
            //удаление копий вершин на стыках - отменено из-за uv

            var mf = g.AddComponent<MeshFilter>();
            mf.sharedMesh = visibleMesh;
            var mr = g.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = PoolMaster.shadowCasting;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            if (!PoolMaster.useIlluminationSystem) mr.sharedMaterial = PoolMaster.GetMaterial(mvi.materialType);
            else mr.sharedMaterial = PoolMaster.GetMaterial(mvi.materialType, mvi.illumination);

            g.transform.parent = renderersHolders[mvi.faceIndex].transform;
            g.AddComponent<MeshCollider>().sharedMesh = colliderMesh;
            g.tag = BLOCK_COLLIDER_TAG;

            renderers.Add(mvi, g);
        }
    }
    private void INLINE_CombineMeshes( ref List<int> indexes, ref Mesh visibleMesh, ref Mesh colliderMesh)
    {
        int count = indexes.Count;
        if (count > 0)
        {            
            CombineInstance[] visibleCI = new CombineInstance[count], colliderCI = new CombineInstance[count];
            Mesh m;
            BlockpartVisualizeInfo bvi;
            Matrix4x4 mtr;
            for (int i = 0; i < count; i++)
            {
                bvi = blockVisualizersList[indexes[i]];
                m = MeshMaster.GetMesh(bvi.meshType, bvi.materialID);
                visibleCI[i].mesh = m;
                visibleCI[i].transform = bvi.GetPositionMatrix();
                mtr = bvi.GetPositionMatrix();
                visibleCI[i].transform = mtr;
                colliderCI[i].mesh = bvi.meshType == MeshType.Quad ? m : MeshMaster.GetMeshColliderLink(bvi.meshType);
                colliderCI[i].transform = mtr;
            }
            
            visibleMesh.CombineMeshes(visibleCI, true); // все подмеши используют один материал            
            colliderMesh.CombineMeshes(colliderCI);
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
                    if (blockVisualizersList[i].meshInfo == mvi) indexes.Add(i);
                }                

                n = indexes.Count;
                if (n > 0)
                {
                    Mesh visibleMesh = new Mesh(), colliderMesh = new Mesh();
                    INLINE_CombineMeshes(ref indexes, ref visibleMesh, ref colliderMesh);
                    g.GetComponent<MeshFilter>().sharedMesh = visibleMesh;
                    g.GetComponent<MeshCollider>().sharedMesh = colliderMesh;
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
            Destroy(renderersHolders[7]);
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
    public Transform GetRenderersHolderTransform(byte faceIndex)
    {
        if (faceIndex < 8) return renderersHolders[faceIndex].transform;
        else return null;
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
        float s = chunkSize * Block.QUAD_SIZE - qh;
        float h = chunkSize / 2f * Block.QUAD_SIZE - qh;
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

    public void LayersCut()
    {
        int layerCutHeight = GameMaster.layerCutHeight;
        bool visible = true;
        foreach (var b in blocks.Values)
        {
            visible = b.pos.y < layerCutHeight;
            b.SetVisibilityMode(visible ? VisibilityMode.LayerCutCancel : VisibilityMode.LayerCutHide);
        }
        RenderDataFullRecalculation();
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

    private void ShadowsUpdate()
    {
        var count = renderers?.Count ?? 0;
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
    public void CameraUpdate()
    {
        if (GameMaster.loading) return;
        Vector3 cpos = transform.InverseTransformPoint(FollowingCamera.camPos);
        Vector3 v = Vector3.one * (-1);
        float size = chunkSize * Block.QUAD_SIZE;
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
            for (byte i = 0; i<8; i++)
            {
                renderersHolders[i].SetActive( (renderBitmask & (1 << i)) != 0 );
            }
            /*
            if (renderers.Count > 0)
            {
                bool visible;
                GameObject g;
                foreach (var r in renderers)
                {
                    visible = ((renderBitmask & (1 << r.Key.faceIndex)) != 0);
                    g = r.Value;
                    if (g.activeSelf != visible) g.SetActive(visible);
                }
            }
            */
            prevBitmask = renderBitmask;
        }

        float x0 = cpos.x, y0 = cpos.y, z0 = cpos.z;
        if (blocks != null && blocks.Count > 0)
        {
            float sqrdistance;
            float x, y, z, lod_cf = LODController.lodCoefficient;
            float dist1 = SMALL_OBJECTS_HIDE_DISTANCE_SQR * (0.2f + lod_cf),
                dist2 = MEDIUM_OBJECTS_LOD_DISTANCE_SQR * (0.2f + lod_cf),
                dist3 = HUGE_OBJECTS_LOD_DISTANCE_SQR * (0.2f + lod_cf);
            foreach (var b in blocks.Values) {
                //#set block visibility - copy
                x = b.pos.x * Block.QUAD_SIZE; y = b.pos.y * Block.QUAD_SIZE; z = b.pos.z * Block.QUAD_SIZE;
                sqrdistance = (x - x0) * (x - x0) + (y - y0) * (y - y0) + (z - z0) * (z - z0);
                if (sqrdistance < dist1 ) b.SetVisibilityMode(VisibilityMode.DrawAll);
                else
                {
                    if (sqrdistance > dist3) b.SetVisibilityMode(VisibilityMode.HugeObjectsLOD);
                    else
                    {
                        if (sqrdistance > dist2) b.SetVisibilityMode(VisibilityMode.MediumObjectsLOD);
                        else b.SetVisibilityMode(VisibilityMode.SmallObjectsLOD);
                    }
                }
                //end
            }
        }
        needCameraUpdate = false;
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

    #region light
    public byte GetLightValue(int x, int y, int z)
    {
        if (!PoolMaster.useIlluminationSystem) return UP_LIGHT;
        else
        {
            if (y < 0) return BOTTOM_LIGHT;
            else
            {
                byte sz = chunkSize;
                sz--;
                if (x < 0 || z < 0 || x > sz || y > sz || z > sz) return UP_LIGHT;
                else return lightMap[x, y, z];
            }
        }
    }
    public byte GetLightValue(ChunkPos cpos) { return GetLightValue(cpos.x, cpos.y, cpos.z); }  

    public void ChunkLightmapFullRecalculation()
    {
        if (!PoolMaster.useIlluminationSystem) return;
        byte UP_LIGHT = 255, DOWN_LIGHT = 128;
        int x = 0, y = 0, z = 0;
        for (x = 0; x < chunkSize; x++)
        {
            for (z = 0; z < chunkSize; z++)
            {
                for (y = 0; y < chunkSize; y++)
                {
                    lightMap[x, y, z] = 0;
                }
            }
        }
        // проход снизу
        Block b = null;
        x = 0; y = 0; z = 0;
        for (x = 0; x < chunkSize; x++)
        {
            for (z = 0; z < chunkSize; z++)
            {
                for (y = 0; y < chunkSize; y++)
                {
                    b = GetBlock(x, y, z);
                    if (b == null || b.IsFaceTransparent(Block.DOWN_FACE_INDEX)) lightMap[x, y, z] = DOWN_LIGHT;
                    else break;
                }
            }
        }
        // проход сверху
        x = 0; y = 0; z = 0;
        for (x = 0; x < chunkSize; x++)
        {
            for (z = 0; z < chunkSize; z++)
            {
                for (y = chunkSize - 1; y >= 0; y--)
                {
                    b = GetBlock(x, y, z);
                    if (b == null || b.IsFaceTransparent(Block.UP_FACE_INDEX)) lightMap[x, y, z] = UP_LIGHT;
                    else break;
                }
            }
        }
        //проход спереди
        byte decreasedVal;
        for (x = 0; x < chunkSize; x++)
        {
            for (y = 0; y < chunkSize; y++)
            {
                decreasedVal = (byte)(lightMap[x, y, chunkSize - 1] * LIGHT_DECREASE_PER_BLOCK);
                for (z = chunkSize - 2; z >= 0; z--)
                {
                    if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        //проход сзади
        for (x = 0; x < chunkSize; x++)
        {
            for (y = 0; y < chunkSize; y++)
            {
                decreasedVal = (byte)(lightMap[x, y, 0] * LIGHT_DECREASE_PER_BLOCK);
                for (z = 1; z < chunkSize; z++)
                {
                    if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        //проход справа
        for (z = 0; z < chunkSize; z++)
        {
            for (y = 0; y < chunkSize; y++)
            {
                decreasedVal = (byte)(lightMap[chunkSize - 1, y, z] * LIGHT_DECREASE_PER_BLOCK);
                for (x = chunkSize - 2; x >= 0; x--)
                {
                    b = GetBlock(x, y, z);
                    if (b == null || b.IsFaceTransparent(Block.RIGHT_FACE_INDEX))
                    {
                        if (lightMap[x, y, z] < decreasedVal) lightMap[x, y, z] = decreasedVal;
                    }
                    decreasedVal = (byte)(lightMap[x, y, z] * LIGHT_DECREASE_PER_BLOCK);
                }
            }
        }
        //проход слева
        for (z = 0; z < chunkSize; z++)
        {
            for (y = 0; y < chunkSize; y++)
            {
                decreasedVal = (byte)(lightMap[0, y, z] * LIGHT_DECREASE_PER_BLOCK);
                for (x = 1; x < chunkSize; x++)
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
                switch (brd.meshInfo.faceIndex)
                {
                    case 0:
                        a = brd.pos.z + 1;
                        if (a >= chunkSize) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[brd.pos.x, brd.pos.y, a];
                        break;
                    case 1:
                        a = brd.pos.x + 1;
                        if (a >= chunkSize) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[a, brd.pos.y, brd.pos.z];
                        break;
                    case 2:
                        a = brd.pos.z - 1;
                        if (a < 0) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[brd.pos.x, brd.pos.y, a];
                        break;
                    case 3:
                        a = brd.pos.x - 1;
                        if (a < 0) lightToCompare = UP_LIGHT;
                        else lightToCompare = lightMap[a, brd.pos.y, brd.pos.z];
                        break;
                    case 4:
                        a = brd.pos.y + 1;
                        if (a >= chunkSize) lightToCompare = UP_LIGHT;
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
                if (brd.meshInfo.illumination != lightToCompare && !redrawRequiredTypes.Contains(brd.meshInfo))
                {
                    redrawRequiredTypes.Add(brd.meshInfo);
                }
            }
        }
    }
    public void RecalculateIlluminationAtPoint(ChunkPos pos)
    {
        ChunkLightmapFullRecalculation(); // в разработке
    }
    #endregion 
}
