using UnityEngine;
using System.Collections.Generic;
//RENDERING PART
public sealed partial class Chunk : MonoBehaviour {
    public byte[,,] lightMap { get; private set; }

    private float LIGHT_DECREASE_PER_BLOCK;
    private bool chunkDataUpdateRequired = false, borderDrawn = false, shadowsUpdateRequired, chunkRenderUpdateRequired = false;
    private Dictionary<MeshVisualizeInfo, GameObject> renderers; // (face, material, illumitation) <- носители скомбинированных моделей
    private List<BlockpartVisualizeInfo> blockVisualizersList;// <- информация обо всех видимых частях блоков
    private List<MeshVisualizeInfo> redrawRequiredTypes; // <- будут перерисованы и снова скомбинированы
    private GameObject combinedShadowCaster;
    private GameObject[] renderersHolders; // 6 холдеров для каждой стороны куба + 1 нестандартная

    public const byte UP_LIGHT = 255, BOTTOM_LIGHT = 128;

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
    public byte GetVisibilityMask(ChunkPos cpos) { return GetVisibilityMask(cpos.x, cpos.y, cpos.z); }
    public byte GetVisibilityMask(int x, int y, int z)
    {
        if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_SIZE || z < 0 || z >= CHUNK_SIZE) return 255;
        if (y > GameMaster.layerCutHeight) return 0;
        else
        {
            byte vmask = 255;
            var powersOfTwo = GameConstants.powersOfTwo;
            Block bx = GetBlock(x, y, z + 1);
            //sides:
            if (bx != null && !bx.IsFaceTransparent(Block.BACK_FACE_INDEX))
            {
                vmask -= powersOfTwo[Block.FWD_FACE_INDEX];
            }
            bx = GetBlock(x + 1, y, z);
            if (bx != null && !bx.IsFaceTransparent(Block.LEFT_FACE_INDEX))
            {
                vmask -= powersOfTwo[Block.RIGHT_FACE_INDEX];
            }
            bx = GetBlock(x, y, z - 1);
            if (bx != null && !bx.IsFaceTransparent(Block.FWD_FACE_INDEX))
            {
                vmask -= powersOfTwo[Block.BACK_FACE_INDEX];
            }
            bx = GetBlock(x - 1, y, z);
            if (bx != null && !bx.IsFaceTransparent(Block.RIGHT_FACE_INDEX))
            {
                vmask -= powersOfTwo[Block.LEFT_FACE_INDEX];
            }
            // up and down
            bx = GetBlock(x, y + 1, z);
            if (bx != null && !bx.IsFaceTransparent(Block.DOWN_FACE_INDEX))
            {
                vmask -= powersOfTwo[Block.UP_FACE_INDEX];
            }
            bx = GetBlock(x, y - 1, z);
            if (bx != null && !bx.IsFaceTransparent(Block.UP_FACE_INDEX))
            {
                vmask -= powersOfTwo[Block.DOWN_FACE_INDEX];
            }
            byte sidesMask = (byte)(powersOfTwo[Block.FWD_FACE_INDEX] + powersOfTwo[Block.RIGHT_FACE_INDEX] + powersOfTwo[Block.BACK_FACE_INDEX] + powersOfTwo[Block.LEFT_FACE_INDEX]);
            if ((vmask & sidesMask) == 0) //ни одна боковая сторона не рисуется
            {
                if ((vmask & powersOfTwo[Block.UP_FACE_INDEX]) == 0) vmask -= powersOfTwo[Block.SURFACE_FACE_INDEX];
                if ((vmask & powersOfTwo[Block.DOWN_FACE_INDEX]) == 0) vmask -= powersOfTwo[Block.CEILING_FACE_INDEX];
            }
            return vmask;
        }
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
                byte sz = CHUNK_SIZE;
                sz--;
                if (x < 0 || z < 0 || x > sz || y > sz || z > sz) return UP_LIGHT;
                else return lightMap[x, y, z];
            }
        }
    }
    public byte GetLightValue(ChunkPos cpos) { return GetLightValue(cpos.x, cpos.y, cpos.z); }

  
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
        byte visibilityMask = GetVisibilityMask(b.pos);
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

        if ((visibilityMask & GameConstants.powersOfTwo[face]) != 0) // должен быть видимым
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

        var pot = GameConstants.powersOfTwo;
        for (byte k = 0; k < 8; k++)
        {
            currentBlockInfo = blockParts[k];
            if ((visibilityMask & pot[k]) != 0) // должен быть видимым
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
                m = MeshMaster.GetMesh(cdata.meshType, cdata.materialID);
                ci[j].mesh = m;
                ci[j].transform = cdata.GetPositionMatrix();
            }

            GameObject g = new GameObject();
            m = new Mesh();
            m.CombineMeshes(ci, true); // все подмеши используют один материал

            //удаление копий вершин на стыках - отменено из-за uv

            var mf = g.AddComponent<MeshFilter>();
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
                        Mesh m = MeshMaster.GetMesh(bvi.meshType, bvi.materialID);
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
                    b = GetBlock(x, y, z);
                    if (b == null || b.IsFaceTransparent(Block.DOWN_FACE_INDEX)) lightMap[x, y, z] = DOWN_LIGHT;
                    else break;
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
                    b = GetBlock(x, y, z);
                    if (b == null || b.IsFaceTransparent(Block.UP_FACE_INDEX)) lightMap[x, y, z] = UP_LIGHT;
                    else break;
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
                        if (a < 0) lightToCompare = UP_LIGHT;
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
                if (brd.rinfo.illumination != lightToCompare && !redrawRequiredTypes.Contains(brd.rinfo))
                {
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
    #endregion 
}
