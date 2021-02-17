﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Constructor
{
    private struct GeneratedMeshPartInfo
    {
        public readonly static GeneratedMeshPartInfo StickHead, Tube;
        public readonly bool containsData;
        public MeshType fwdFaceType, rightFaceType, backFaceType, leftFaceType, upFaceType, downFaceType, innerFaceType;
        public int fwdMaterialID, rightMaterialID, backMaterialID, leftMaterialID, upMaterialID, downMaterialID, innerMaterialID;

        public GeneratedMeshPartInfo(MeshType[] i_faceTypes, int[] i_materialsID)
        {
            fwdFaceType = i_faceTypes[0];
            rightFaceType = i_faceTypes[1];
            backFaceType = i_faceTypes[2];
            leftFaceType = i_faceTypes[3];
            upFaceType = i_faceTypes[4];
            downFaceType = i_faceTypes[5];
            innerFaceType = i_faceTypes[6];

            fwdMaterialID = i_materialsID[0];
            rightMaterialID = i_materialsID[1];
            backMaterialID = i_materialsID[2];
            leftMaterialID = i_materialsID[3];
            upMaterialID = i_materialsID[4];
            downMaterialID = i_materialsID[5];
            innerMaterialID = i_materialsID[6];

            containsData = true;
        }
        public GeneratedMeshPartInfo(MeshType[] i_faceTypes, int i_materialID)
        {
            fwdFaceType = i_faceTypes[0];
            rightFaceType = i_faceTypes[1];
            backFaceType = i_faceTypes[2];
            leftFaceType = i_faceTypes[3];
            upFaceType = i_faceTypes[4];
            downFaceType = i_faceTypes[5];
            innerFaceType = i_faceTypes[6];

            fwdMaterialID = i_materialID;
            rightMaterialID = i_materialID;
            backMaterialID = i_materialID;
            leftMaterialID = i_materialID;
            upMaterialID = i_materialID;
            downMaterialID = i_materialID;
            innerMaterialID = i_materialID;
            containsData = true;
        }

        static GeneratedMeshPartInfo()
        {
            StickHead = new GeneratedMeshPartInfo(
                new MeshType[7] { MeshType.Quad, MeshType.Quad, MeshType.Quad, MeshType.Quad, MeshType.Quad, MeshType.NoMesh, MeshType.NoMesh },
                0
                );
            Tube = new GeneratedMeshPartInfo(
                new MeshType[7] {MeshType.Quad, MeshType.Quad, MeshType.Quad, MeshType.Quad, MeshType.NoMesh, MeshType.NoMesh, MeshType.NoMesh},
                0
                );
        }

        // остриём вверх
        public static GeneratedMeshPartInfo MakeCutToUp(byte face, int material) // face - к какой стороне обращен склон
        {
            switch (face) {                
                case 0:
                    return new GeneratedMeshPartInfo(
                        new MeshType[7] { MeshType.NoMesh, MeshType.CutEdge012, MeshType.Quad, MeshType.CutEdge032,MeshType.NoMesh, MeshType.Quad, MeshType.CutPlane },
                        material
                        );
                case 1:
                    return new GeneratedMeshPartInfo(
                        new MeshType[7] { MeshType.CutEdge032, MeshType.NoMesh, MeshType.CutEdge012, MeshType.Quad, MeshType.NoMesh, MeshType.Quad, MeshType.CutPlane },
                        material
                        );
                case 2:
                    return new GeneratedMeshPartInfo(
                        new MeshType[7] { MeshType.Quad, MeshType.CutEdge032, MeshType.NoMesh, MeshType.CutEdge012, MeshType.NoMesh, MeshType.Quad, MeshType.CutPlane },
                        material
                        );
                case 3:
                    return new GeneratedMeshPartInfo(
                        new MeshType[7] {  MeshType.CutEdge012, MeshType.Quad, MeshType.CutEdge032, MeshType.NoMesh, MeshType.NoMesh, MeshType.Quad, MeshType.CutPlane },
                        material
                        );

                default: goto case 0;
            }
        }

        public void SetMaterial(int id)
        {
            fwdMaterialID = id;
            rightMaterialID = id;
            backMaterialID = id;
            leftMaterialID = id;
            upMaterialID = id;
            downMaterialID = id;
            innerMaterialID = id;
        }
    }

    public static Chunk ConstructChunk(byte chunkSize, ChunkGenerationMode mode)
    {
        int size = chunkSize;
        int[,,] dat = new int[size, size, size];
        switch (mode)
        {
            case ChunkGenerationMode.Standart: GenerateSpiralsData(size, ref dat); break;
            case ChunkGenerationMode.Cube: GeneratePyramidData(size, ref dat); break;
            case ChunkGenerationMode.Peak: dat = GeneratePeakData(size); break;
        }

        //testzone
        /*
        dat = new int[size, size, size];
        var ac = PoolMaster.MATERIAL_WHITE_METAL_ID;
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                for (int y = 0; y < 3; y++)
                {
                    dat[x, y, z] = ac;
                }
            }
        }
        */
        //eo testzone

        GameObject g = new GameObject("chunk");
        Chunk c = g.AddComponent<Chunk>();
        c.CreateNewChunk(dat);       
        //CheckForLandingPosition(c);
        c.RenderDataFullRecalculation();
        return c;
    }
    public static Chunk ConstructBlock(byte chunkSize)
    {
        int size = chunkSize;
        int[,,] dat = new int[size, size, size];
        dat[0, 0, 0] = ResourceType.STONE_ID;
        GameObject g = new GameObject("chunk");
        Chunk c = g.AddComponent<Chunk>();
        c.CreateNewChunk(dat);
        //NatureCreation(c);
        //CheckForLandingPosition(c);
        return c;
    }

    private static void GenerateSpiralsData(int size, ref int[,,] data)
    {
        int arms = 3;
        float armsLength = 1;
        if (arms > 26) arms = 26;
        int width = 1;

        int x = size / 2, y = x, z = x;
        List<Vector3Int> skeleton = new List<Vector3Int>();
        data[x, y, z] = ResourceType.DIRT_ID; skeleton.Add(new Vector3Int(x, y, z));
        data[x + 1, y, z] = ResourceType.DIRT_ID; skeleton.Add(new Vector3Int(x + 1, y, z));
        data[x - 1, y, z] = ResourceType.DIRT_ID; skeleton.Add(new Vector3Int(x - 1, y, z));
        data[x, y, z + 1] = ResourceType.DIRT_ID; skeleton.Add(new Vector3Int(x, y, z + 1));
        data[x, y, z - 1] = ResourceType.DIRT_ID; skeleton.Add(new Vector3Int(x, y, z - 1));

        int[] allVariants = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

        List<int> crossroadVariants = new List<int>();
        crossroadVariants.AddRange(allVariants);
        for (int i = 0; i < arms; i++)
        {
            int x2 = x, y2 = y, z2 = z;
            int rval = Random.Range(0, crossroadVariants.Count);
            int direction = crossroadVariants[rval];
            crossroadVariants.RemoveAt(rval);
            switch (direction)
            {
                case 0: x2--; y2++; z2++; break;
                case 1: y2++; z2++; break;
                case 2: x2++; y2++; z2++; break;
                case 3: x2--; y2++; break;
                case 4: y2++; break;
                case 5: x2++; y2++; break;
                case 6: x2--; y2++; z2--; break;
                case 7: y2++; z2--; break;
                case 8: x2++; y2++; z2--; break;
                case 9: x2--; z2++; break;
                case 10: z2++; break;
                case 11: x2++; z2++; break;
                case 12: x2--; break;
                case 13: x2++; break;
                case 14: x2--; z2--; break;
                case 15: z2--; break;
                case 16: x2++; z2--; break;
                case 17: x2--; y2--; z2++; break;
                case 18: y2--; z2++; break;
                case 19: x2++; y2--; z2++; break;
                case 20: x2--; y2--; break;
                case 21: y2--; break;
                case 22: x2++; y2--; break;
                case 23: x2--; y2--; z2--; break;
                case 24: y2--; z2--; break;
                case 25: x2++; y2--; z2--; break;
            }
            int j = (int)(size * (armsLength * 0.5f + Random.value * 0.5f) + 1);
            while (j > 0)
            {
                j--;
                rval = Random.Range(0, allVariants.Length);
                direction = allVariants[rval];
                int len = (int)((8 * armsLength) * (0.125f + Random.value * 0.875f));
                for (int k = 0; k < len; k++)
                {
                    Vector3Int left, right;
                    switch (direction)
                    {
                        case 0: x2--; y2++; z2++; left = new Vector3Int(-1, 0, -1); right = new Vector3Int(1, 0, 1); break;
                        case 1: y2++; z2++; left = new Vector3Int(-1, 0, 0); right = new Vector3Int(1, 0, 0); break;
                        case 2: x2++; y2++; z2++; left = new Vector3Int(-1, 0, 1); right = new Vector3Int(1, 0, -1); break;
                        case 3: x2--; y2++; left = new Vector3Int(0, 0, -1); right = new Vector3Int(0, 0, 1); break;
                        case 4: y2++; left = new Vector3Int(-1, 0, 0); right = new Vector3Int(1, 0, 0); break;
                        case 5: x2++; y2++; left = new Vector3Int(1, 0, 1); right = new Vector3Int(1, 0, -1); break;
                        case 6: x2--; y2++; z2--; left = new Vector3Int(1, 0, -1); right = new Vector3Int(-1, 0, 1); break;
                        case 7: y2++; z2--; left = new Vector3Int(1, 0, 0); right = new Vector3Int(-1, 0, 0); break;
                        case 8: x2++; y2++; z2--; left = new Vector3Int(1, 0, 1); right = new Vector3Int(-1, 0, -1); break;
                        case 9: x2--; z2++; left = new Vector3Int(-1, 0, -1); right = new Vector3Int(1, 0, 1); break;
                        case 10: z2++; left = new Vector3Int(-1, 0, 0); right = new Vector3Int(1, 0, 0); break;
                        case 11: x2++; z2++; left = new Vector3Int(-1, 0, 1); right = new Vector3Int(1, 0, -1); break;
                        case 12: x2--; left = new Vector3Int(0, 0, -1); right = new Vector3Int(0, 0, 1); break;
                        case 13: x2++; left = new Vector3Int(0, 0, 1); right = new Vector3Int(0, 0, -1); break;
                        case 14: x2--; z2--; left = new Vector3Int(1, 0, -1); right = new Vector3Int(-1, 0, 1); break;
                        case 15: z2--; left = new Vector3Int(1, 0, 0); right = new Vector3Int(-1, 0, 0); break;
                        case 16: x2++; z2--; left = new Vector3Int(1, 0, 1); right = new Vector3Int(-1, 0, -1); break;
                        case 17: x2--; y2--; z2++; left = new Vector3Int(-1, 0, -1); right = new Vector3Int(1, 0, 1); break;
                        case 18: y2--; z2++; left = new Vector3Int(-1, 0, 0); right = new Vector3Int(1, 0, 0); break;
                        case 19: x2++; y2--; z2++; left = new Vector3Int(-1, 0, 0); right = new Vector3Int(1, 0, -1); break;
                        case 20: x2--; y2--; left = new Vector3Int(0, 0, -1); right = new Vector3Int(0, 0, 1); break;
                        case 21: y2--; left = new Vector3Int(-1, 0, 0); right = new Vector3Int(1, 0, 0); break;
                        case 22: x2++; y2--; left = new Vector3Int(0, 0, 1); right = new Vector3Int(0, 0, -1); break;
                        case 23: x2--; y2--; z2--; left = new Vector3Int(1, 0, -1); right = new Vector3Int(-1, 0, 1); break;
                        case 24: y2--; z2--; left = new Vector3Int(1, 0, 0); right = new Vector3Int(-1, 0, 0); break;
                        case 25: x2++; y2--; z2--; left = new Vector3Int(1, 0, 1); right = new Vector3Int(-1, 0, -1); break;
                        default: right = new Vector3Int(1, 0, 0); left = new Vector3Int(-1, 0, 0); break;
                    }
                    if (x2 > 0 & y2 > 0 & z2 > 0 & x2 < size & y2 < size & z2 < size)
                    {
                        data[x2, y2, z2] = ResourceType.DIRT_ID;
                        skeleton.Add(new Vector3Int(x2, y2, z2));
                        int x3r = x2, y3r = y2, z3r = z2;
                        int x3l = x2, y3l = y2, z3l = z2;
                        bool rightAppliable = true, leftAppliable = true;
                        for (int n = 0; n < width; n++)
                        {
                            if (rightAppliable)
                            {
                                x3r += right.x; y3r += right.y; z3r += right.z;
                                if (x3r > 0 & y3r > 0 & z3r > 0 & x3r < size & y3r < size & z3r < size)
                                {
                                    data[x3r, y3r, z3r] = ResourceType.DIRT_ID;
                                    skeleton.Add(new Vector3Int(x3r, y3r, z3r));
                                }
                                else rightAppliable = false;
                            }
                            if (leftAppliable)
                            {
                                x3l += left.x; y3l += left.y; z3l += left.z;
                                if (x3l > 0 & y3l > 0 & z3l > 0 & x3l < size & y3l < size & z3l < size)
                                {
                                    data[x3l, y3l, z3l] = ResourceType.DIRT_ID;
                                    skeleton.Add(new Vector3Int(x3l, y3l, z3l));
                                }
                                else leftAppliable = false;
                            }
                        }

                    }
                    else break;
                }
            }
        }
        // обработка
        //int width = 1;   


        foreach (Vector3Int v in skeleton)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            if (y > 0 && data[x, y - 1, z] == 0) data[x, y - 1, z] = ResourceType.STONE_ID;
            if (Random.value < 0.3f & y > 1 && data[x, y - 2, z] == 0) data[x, y - 1, z] = ResourceType.STONE_ID;
            if (x < size - 1 && data[x + 1, y, z] == 0) data[x + 1, y, z] = Random.value > 0.3f ? ResourceType.STONE_ID : ResourceType.DIRT_ID;
            if (x > 0 && data[x - 1, y, z] == 0) data[x - 1, y, z] = Random.value > 0.3f ? ResourceType.STONE_ID : ResourceType.DIRT_ID;
        }
    }
    private static void GeneratePyramidData(int size, ref int[,,] data)
    {
        float radius = size * Mathf.Sqrt(2);
        float seed = 1.1f + System.DateTime.Now.Second, roughness = 0.3f;
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float cs = size;
                float perlin = Mathf.PerlinNoise(x / cs * (10 * roughness) + seed, z / cs * (10 * roughness) + seed);
                //perlin += pc; if (perlin > 1) perlin = 1;
                int height = (int)(size / 2 * perlin);
                if (height < 2) height = 2; else if (height > size / 2) height = size / 2;
                int y = 0;
                for (; y < height; y++)
                {
                    data[x, y + size / 2, z] = ResourceType.STONE_ID;
                }
                if (Random.value > 0.8f) data[x, height + size / 2 - 2, z] = ResourceType.DIRT_ID;
                if (Random.value < 0.95f) data[x, height + size / 2 - 1, z] = ResourceType.DIRT_ID;
                //down part
                float pc = (1 - Mathf.Sqrt((x - size / 2) * (x - size / 2) + (z - size / 2) * (z - size / 2)) / radius);
                pc *= pc * pc;
                height = (int)(pc * size / 2 + size / 2 * perlin);
                if (height < 0) height = 1; else if (height > size / 2) height = size / 2;
                for (y = 0; y <= height; y++)
                {
                    data[x, size / 2 - y, z] = ResourceType.STONE_ID;
                }
            }
        }
    }
    public static int[,,] GeneratePeakData(int size)
    {
        var data = new int[size, size, size];
        int quarter = size / 4, half = size / 2;
        int a = half - quarter, b = half + quarter;
        int h = 0, y = 0 ,end;
        float sqrDist = (half - a) * (half - a) ;
        for (int x = a; x < b; x++)
        {
            for (int z = a; z < b; z++)
            {
                float d = (half - x) * (half - x) + (half - z) * (half - z);
                d /= sqrDist;
                d = 1 - d;
                if (d < 0) d = 0;
                h = (int)((size - 1) * d * d + (0.5f - Random.value) * (size / 8f));
                if (h >= size) h = size - 1;
                y = 0;
                bool doubleDirt = (Random.value > 0.5f);
                if (doubleDirt) end = h - 2; else end = h - 1;
                for (;y < end; y++)
                {
                    data[x,y,z] = ResourceType.STONE_ID;
                }
                data[x, y, z] = ResourceType.DIRT_ID;
                if (doubleDirt) data[x, y + 1, z] = ResourceType.DIRT_ID;
            }
        }
        return data;
    }

    public static GameObject CreatePeakBasis(int res, int materialID)
    {
        var dataArray = new GeneratedMeshPartInfo[res,res,res];

        GeneratedMeshPartInfo tube = GeneratedMeshPartInfo.Tube, endFlat = GeneratedMeshPartInfo.StickHead;
        tube.SetMaterial(materialID);
        endFlat.SetMaterial(materialID);

        int quarter = res / 4, half = res / 2;
        int a = half - quarter, b = half + quarter;
        int h = 0, y = 0;
        float sqrDist = (half - a) * (half - a);
        for (int x = a; x < b; x++)
        {
            for (int z = a; z < b; z++)
            {
                float d = (half - x) * (half - x) + (half - z) * (half - z);
                d /= sqrDist;
                d = 1 - d;
                if (d < 0) d = 0;
                h = (int)((res - 1) * d * d + (0.5f - Random.value) * (res / 8f));
                if (h >= res) h = res;
                y = 0;
                for (; y < h - 1 ; y++)
                {
                    dataArray[x, y, z] = tube;
                }
                if (Random.value > 0.5f)
                {
                    dataArray[x, y, z] = GeneratedMeshPartInfo.MakeCutToUp((byte)Random.Range(0, 4), materialID);
               }
               else dataArray[x, y, z] = endFlat;
            }
        }
        for (y = 0; y < res - 1; y++)
        {
            dataArray[half, y, half] = tube;
        }
        dataArray[half, y, half] = endFlat;     

        // оптимизация массива внутри
        return ConstructGeneratedMesh(dataArray, materialID);
    }
    public static GameObject GetModernRuinPart(int size)
    {
        GeneratedMeshPartInfo[,,] dataArray;
        switch (size)
        {
            case 1:
                {
                    dataArray = new GeneratedMeshPartInfo[1, 1, 1];
                    dataArray[0, 0, 0] = GeneratedMeshPartInfo.StickHead;
                    return ConstructGeneratedMesh(dataArray, PoolMaster.MATERIAL_WHITEWALL_ID);
                }
            case 2:
                {
                    dataArray = new GeneratedMeshPartInfo[2, 2, 2];
                    dataArray[0, 0, 0] = GeneratedMeshPartInfo.Tube;
                    dataArray[0, 1, 0] = GeneratedMeshPartInfo.MakeCutToUp((byte)Random.Range(0, 4), ResourceType.GRAPHONIUM_ID);
                    float f = Random.value;
                    if (f > 0.5f)
                    {
                        dataArray[1, 0, 1] = f > 0.75f ? GeneratedMeshPartInfo.StickHead : GeneratedMeshPartInfo.MakeCutToUp(1, ResourceType.GRAPHONIUM_ID);
                    }
                    return ConstructGeneratedMesh(dataArray, PoolMaster.MATERIAL_WHITEWALL_ID);
                }
            case 3:
                {
                    dataArray = new GeneratedMeshPartInfo[3, 3, 3];
                    dataArray[1, 0, 1] = GeneratedMeshPartInfo.Tube;
                    dataArray[1, 1, 1] = GeneratedMeshPartInfo.Tube;
                    dataArray[1, 2, 1] = GeneratedMeshPartInfo.MakeCutToUp((byte)Random.Range(0, 4), ResourceType.GRAPHONIUM_ID);

                    return ConstructGeneratedMesh(dataArray, PoolMaster.MATERIAL_WHITEWALL_ID);                   
                }
        }
        return null;
    }

    private static int RefineGeneratedMesh(ref GeneratedMeshPartInfo[,,] dataArray)
    {
        GeneratedMeshPartInfo mi;
        MeshType type1, type2;
        int activeMeshesCount = 0, res = dataArray.GetLength(0);
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                for (int z = 0; z < res; z++)
                {
                    mi = dataArray[x, y, z];
                    if (mi.containsData)
                    {
                        if (z + 1 < res && dataArray[x, y, z + 1].containsData)
                        {
                            type1 = mi.fwdFaceType;
                            type2 = dataArray[x, y, z + 1].backFaceType;
                            if (type1 != MeshType.NoMesh & type2 != MeshType.NoMesh)
                            {
                                if (type1 == MeshType.Quad)
                                {
                                    if (type2 == MeshType.Quad)
                                    {
                                        mi.fwdFaceType = MeshType.NoMesh;
                                        dataArray[x, y, z + 1].backFaceType = MeshType.NoMesh;
                                    }
                                    else
                                    {
                                        if (type2 == MeshType.CutEdge012 | type2 == MeshType.CutEdge032)
                                        {
                                            dataArray[x, y, z + 1].backFaceType = MeshType.NoMesh;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (type1 == MeshType.NoMesh) dataArray[x, y, z + 1].backFaceType = MeshType.NoMesh;
                                else mi.fwdFaceType = MeshType.NoMesh;
                            }
                        }
                        if (x + 1 < res && dataArray[x + 1, y, z].containsData)
                        {
                            type1 = mi.rightFaceType;
                            type2 = dataArray[x + 1, y, z].leftFaceType;
                            if (type1 != MeshType.NoMesh & type2 != MeshType.NoMesh)
                            {
                                if (type1 == MeshType.Quad)
                                {
                                    if (type2 == MeshType.Quad)
                                    {
                                        mi.rightFaceType = MeshType.NoMesh;
                                        dataArray[x + 1, y, z].leftFaceType = MeshType.NoMesh;
                                    }
                                    else
                                    {
                                        if (type2 == MeshType.CutEdge012 | type2 == MeshType.CutEdge032)
                                        {
                                            dataArray[x + 1, y, z].leftFaceType = MeshType.NoMesh;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (type1 == MeshType.NoMesh) dataArray[x + 1, y, z].leftFaceType = MeshType.NoMesh;
                                else mi.rightFaceType = MeshType.NoMesh;
                            }
                        }
                        if (y + 1 < res && dataArray[x, y + 1, z].containsData)
                        {
                            type1 = mi.upFaceType;
                            type2 = dataArray[x, y + 1, z].downFaceType;
                            if (type1 != MeshType.NoMesh & type2 != MeshType.NoMesh)
                            {
                                if (type1 == MeshType.Quad)
                                {
                                    if (type2 == MeshType.Quad)
                                    {
                                        mi.upFaceType = MeshType.NoMesh;
                                        dataArray[x, y + 1, z].downFaceType = MeshType.NoMesh;
                                    }
                                    else
                                    {
                                        if (type2 == MeshType.CutEdge012 | type2 == MeshType.CutEdge032)
                                        {
                                            dataArray[x, y + 1, z].downFaceType = MeshType.NoMesh;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (type1 == MeshType.NoMesh) dataArray[x, y + 1, z].downFaceType = MeshType.NoMesh;
                                else mi.upFaceType = MeshType.NoMesh;
                            }
                        }
                        dataArray[x, y, z] = mi;
                        if (mi.fwdFaceType != MeshType.NoMesh) activeMeshesCount++;
                        if (mi.rightFaceType != MeshType.NoMesh) activeMeshesCount++;
                        if (mi.backFaceType != MeshType.NoMesh) activeMeshesCount++;
                        if (mi.leftFaceType != MeshType.NoMesh) activeMeshesCount++;
                        if (mi.upFaceType != MeshType.NoMesh) activeMeshesCount++;
                        if (mi.downFaceType != MeshType.NoMesh) activeMeshesCount++;
                        if (mi.innerFaceType != MeshType.NoMesh) activeMeshesCount++;
                    }
                }
            }
        }
        return activeMeshesCount;
    }
    private static GameObject ConstructGeneratedMesh(GeneratedMeshPartInfo[,,] dataArray, int materialID)
    {
        Vector3 scale = Vector3.one;
        var ci = new CombineInstance[RefineGeneratedMesh(ref dataArray)];
        int i = 0, xsize = dataArray.GetLength(0), ysize = dataArray.GetLength(1), zsize = dataArray.GetLength(2);
        float xhalf = xsize / 2f,  zhalf = zsize / 2f;
        Quaternion[] rotations = new Quaternion[6]
        {
            Quaternion.identity, Quaternion.Euler(0f,90f,0f), Quaternion.Euler(0f,180f,0f),
            Quaternion.Euler(0f,270f,0f), Quaternion.Euler(90f,0f,0f), Quaternion.Euler(-90f,0f,0f)
        };
        Vector3[] correctionVectors = new Vector3[6]
        {
            Vector3.forward * 0.5f * scale.z, Vector3.right * 0.5f * scale.x, Vector3.back * 0.5f * scale.z,
            Vector3.left * 0.5f * scale.x, Vector3.up * 0.5f * scale.y, Vector3.down * 0.5f * scale.y
        };
        for (int x = 0; x < xsize; x++)
        {
            for (int y = 0; y < ysize; y++)
            {
                for (int z = 0; z < zsize; z++)
                {
                    var gmi = dataArray[x, y, z];
                    if (gmi.fwdFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.fwdFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf) + correctionVectors[0]), rotations[0], scale);
                        i++;
                    }
                    if (gmi.rightFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.rightFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf) + correctionVectors[1]), rotations[1], scale);
                        i++;
                    }
                    if (gmi.backFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.backFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf) + correctionVectors[2]), rotations[2], scale);
                        i++;
                    }
                    if (gmi.leftFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.leftFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf) + correctionVectors[3]), rotations[3], scale);
                        i++;
                    }
                    if (gmi.upFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.upFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf) + correctionVectors[4]), rotations[5], scale);
                        i++;
                    }
                    if (gmi.downFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.downFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf) + correctionVectors[5]), rotations[5], scale);
                        i++;
                    }
                    if (gmi.innerFaceType != MeshType.NoMesh)
                    {
                        ci[i].mesh = MeshMaster.GetMesh(gmi.innerFaceType, materialID);
                        ci[i].transform = Matrix4x4.TRS((new Vector3(x - xhalf, y - ysize, z - zhalf)),
                            (gmi.fwdFaceType == MeshType.NoMesh) ? rotations[0] : (gmi.rightFaceType == MeshType.NoMesh ? rotations[1] : (
                            gmi.backFaceType == MeshType.NoMesh ? rotations[2] : rotations[3]
                            )),
                            scale);
                        i++;
                    }
                }
            }
        }
        GameObject g = new GameObject("peak model");
        var mf = g.AddComponent<MeshFilter>();
        var m = new Mesh();
        m.CombineMeshes(ci, true);
        mf.sharedMesh = m;
        var mr = g.AddComponent<MeshRenderer>();
        mr.sharedMaterial = PoolMaster.GetMaterial(materialID);
        if (PoolMaster.shadowCasting)
        {
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
        }
        else
        {
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        return g;
    }
}
