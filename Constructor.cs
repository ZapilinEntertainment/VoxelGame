using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour
{
    public static Constructor main;
    public float TERRAIN_ROUGHNESS = 0.3f;
    Chunk c;
    public float seed = 1.1f;

    // Use this for initialization
    void Awake()
    {
        if (main != null) { Destroy(main); main = this; } // singleton pattern
        seed += System.DateTime.Now.Second;
    }

    public void ConstructChunk(byte chunkSize, ChunkGenerationMode mode)
    {
        seed += System.DateTime.Now.Second;
        TERRAIN_ROUGHNESS = GameMaster.gameStartSettings.terrainRoughness;
        int size = chunkSize;
        int[,,] dat = new int[size, size, size];
        switch (mode)
        {
            case ChunkGenerationMode.Standart: GenerateSpirals(size, ref dat); break;
            case ChunkGenerationMode.Cube: GeneratePyramid(size, ref dat); break;
        }
        GameObject g = new GameObject("chunk");
        c = g.AddComponent<Chunk>();
        GameMaster.realMaster.SetMainChunk(c);
        c.CreateNewChunk(dat);
        NatureCreation(c);
        // + должна быть проверка для возможности посадки
    }

    private void GenerateSpirals(int size, ref int[,,] data)
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
            int rval = (int)(Random.value * (crossroadVariants.Count - 1));
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
                rval = (int)(Random.value * (allVariants.Length - 1));
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

    private void GeneratePyramid(int size, ref int[,,] data)
    {
        float radius = size * Mathf.Sqrt(2);
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float cs = size;
                float perlin = Mathf.PerlinNoise(x / cs * (10 * TERRAIN_ROUGHNESS) + seed, z / cs * (10 * TERRAIN_ROUGHNESS) + seed);
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

    void NatureCreation(Chunk chunk)
    {
        LifeSource ls = null;
        Block[,,] blocks = chunk.blocks;
        int dirtID = ResourceType.DIRT_ID, size = blocks.GetLength(0);
        int y = 0, x, z;
        SurfaceBlock chosenSurface = null;
        if (Random.value > 0.5f)
        {
            foreach (SurfaceBlock sblock in chunk.surfaceBlocks)
            {
                if (sblock != null && sblock.pos.y > y)
                {
                    if (sblock.pos.x > 0 & sblock.pos.x < size - 1 & sblock.pos.z > 0 & sblock.pos.z < size - 1)
                    {
                        chosenSurface = sblock;
                        y = sblock.pos.y;
                    }
                }
            }
            if (chosenSurface != null)
            {
                x = chosenSurface.pos.x;
                z = chosenSurface.pos.z;
                chunk.ReplaceBlock(new ChunkPos(x, y - 1, z + 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y - 1, z + 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y - 1, z), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y - 1, z - 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x, y - 1, z - 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y - 1, z - 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y - 1, z), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y - 1, z + 1), BlockType.Cube, dirtID, true);

                chunk.ReplaceBlock(new ChunkPos(x, y, z + 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y, z + 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y, z), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y, z - 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x, y, z - 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y, z - 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y, z), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y, z + 1), BlockType.Surface, dirtID, true);

                if (y + 1 < size)
                {
                    chunk.DeleteBlock(new ChunkPos(x, y + 1, z));
                    chunk.DeleteBlock(new ChunkPos(x, y + 1, z + 1));
                    chunk.DeleteBlock(new ChunkPos(x + 1, y + 1, z + 1));
                    chunk.DeleteBlock(new ChunkPos(x + 1, y + 1, z));
                    chunk.DeleteBlock(new ChunkPos(x + 1, y + 1, z - 1));
                    chunk.DeleteBlock(new ChunkPos(x, y + 1, z - 1));
                    chunk.DeleteBlock(new ChunkPos(x - 1, y + 1, z - 1));
                    chunk.DeleteBlock(new ChunkPos(x - 1, y + 1, z));
                    chunk.DeleteBlock(new ChunkPos(x - 1, y + 1, z + 1));
                    if (y + 2 < size)
                    {
                        chunk.DeleteBlock(new ChunkPos(x, y + 2, z));
                        chunk.DeleteBlock(new ChunkPos(x, y + 2, z + 1));
                        chunk.DeleteBlock(new ChunkPos(x + 1, y + 2, z + 1));
                        chunk.DeleteBlock(new ChunkPos(x + 1, y + 2, z));
                        chunk.DeleteBlock(new ChunkPos(x + 1, y + 2, z - 1));
                        chunk.DeleteBlock(new ChunkPos(x, y + 2, z - 1));
                        chunk.DeleteBlock(new ChunkPos(x - 1, y + 2, z - 1));
                        chunk.DeleteBlock(new ChunkPos(x - 1, y + 2, z));
                        chunk.DeleteBlock(new ChunkPos(x - 1, y + 2, z + 1));
                    }
                }
                chosenSurface.ReplaceMaterial(dirtID);
                ls = Structure.GetStructureByID(Structure.TREE_OF_LIFE_ID) as LifeSource;
                ls.SetBasement(chosenSurface, PixelPosByte.zero);
                chunk.GenerateNature(ls.transform.position);
            }
        }
        else
        {
            y = size;
            foreach (SurfaceBlock sblock in chunk.surfaceBlocks)
            {
                if (sblock != null && sblock.pos.y < size)
                {
                    if (sblock.pos.x > 0 & sblock.pos.x < size - 1 & sblock.pos.z > 0 & sblock.pos.z < size - 1)
                    {
                        chosenSurface = sblock;
                        y = sblock.pos.y;
                    }
                }
            }
            if (chosenSurface != null)
            {
                x = chosenSurface.pos.x;
                z = chosenSurface.pos.z;
                chunk.ReplaceBlock(new ChunkPos(x, y - 1, z + 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y - 1, z + 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y - 1, z), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y - 1, z - 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x, y - 1, z - 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y - 1, z - 1), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y - 1, z), BlockType.Cube, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y - 1, z + 1), BlockType.Cube, dirtID, true);

                chunk.ReplaceBlock(new ChunkPos(x, y, z + 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y, z + 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y, z), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x + 1, y, z - 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x, y, z - 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y, z - 1), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y, z), BlockType.Surface, dirtID, true);
                chunk.ReplaceBlock(new ChunkPos(x - 1, y, z + 1), BlockType.Surface, dirtID, true);

                if (y + 1 < size)
                {
                    chunk.DeleteBlock(new ChunkPos(x, y + 1, z + 1));
                    chunk.DeleteBlock(new ChunkPos(x + 1, y + 1, z + 1));
                    chunk.DeleteBlock(new ChunkPos(x + 1, y + 1, z));
                    chunk.DeleteBlock(new ChunkPos(x + 1, y + 1, z - 1));
                    chunk.DeleteBlock(new ChunkPos(x, y + 1, z - 1));
                    chunk.DeleteBlock(new ChunkPos(x - 1, y + 1, z - 1));
                    chunk.DeleteBlock(new ChunkPos(x - 1, y + 1, z));
                    chunk.DeleteBlock(new ChunkPos(x - 1, y + 1, z + 1));
                    if (y + 2 < size)
                    {
                        chunk.DeleteBlock(new ChunkPos(x, y + 2, z + 1));
                        chunk.DeleteBlock(new ChunkPos(x + 1, y + 2, z + 1));
                        chunk.DeleteBlock(new ChunkPos(x + 1, y + 2, z));
                        chunk.DeleteBlock(new ChunkPos(x + 1, y + 2, z - 1));
                        chunk.DeleteBlock(new ChunkPos(x, y + 2, z - 1));
                        chunk.DeleteBlock(new ChunkPos(x - 1, y + 2, z - 1));
                        chunk.DeleteBlock(new ChunkPos(x - 1, y + 2, z));
                        chunk.DeleteBlock(new ChunkPos(x - 1, y + 2, z + 1));
                    }
                }
                chosenSurface.ReplaceMaterial(dirtID);
                ls = Structure.GetStructureByID(Structure.LIFESTONE_ID) as LifeSource;
                ls.SetBasement(chosenSurface, PixelPosByte.zero);
                chunk.GenerateNature(ls.transform.position);
                return;
            }
        }
    }
}
