using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBlock : Block
{
    public float naturalFossils = 0;
    public byte excavatingStatus { get; private set; } // 0 is 75%+, 1 is 50%+, 2 is 25%+, 3 is less than 25%
    public int volume;
    public static readonly int MAX_VOLUME;
    public bool career { get; private set; } // изменена ли верхняя поверхность на котлован?

    public new const int SERIALIZER_LENGTH = 9;

    static CubeBlock()
    {
        MAX_VOLUME = SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION;
    }

    public int PourIn(int blocksCount)
    {
        if (volume == MAX_VOLUME) return blocksCount;
        if (blocksCount > (MAX_VOLUME - volume))
        {
            blocksCount = MAX_VOLUME - volume;
        }
        volume += blocksCount;
        CheckExcavatingStatus();
        return blocksCount;
    }

    public int Dig(int blocksCount, bool show)
    {
        if (volume == 0) return 0;
        if (blocksCount > volume) blocksCount = volume;
        volume -= blocksCount;
        if (show) career = true;
        if (career) CheckExcavatingStatus();
        else
        {
            if (volume == 0)
            {
                Block lowerBlock = myChunk.GetBlock(pos.x, pos.y - 1, pos.z);
                Block upperBlock = myChunk.GetBlock(pos.x, pos.y + 1, pos.z);
                int ceilingMaterial = material_id;
                bool convertToSurfaceType = true;
                if (upperBlock != null)
                {
                    ceilingMaterial = upperBlock.material_id;
                    if (upperBlock is SurfaceBlock) convertToSurfaceType = false;
                }

                if (lowerBlock == null)
                {
                    if (convertToSurfaceType) myChunk.DeleteBlock(pos);
                    else
                    {
                        myChunk.ReplaceBlock(pos, BlockType.Cave, -1, upperBlock.material_id, false);
                    }
                }
                else
                {
                    bool haveSupport = true;
                    int surfMaterial = material_id;
                    switch (lowerBlock.type)
                    {
                        case BlockType.Shapeless:
                            haveSupport = false;
                            break;
                        case BlockType.Cube:
                            {
                                CubeBlock cb = lowerBlock as CubeBlock;
                                if (cb.excavatingStatus != 0) haveSupport = false;
                                else {
                                    haveSupport = true;
                                    surfMaterial = lowerBlock.material_id;
                                }
                            }
                            break;
                        case BlockType.Surface:
                            {
                                SurfaceBlock sb = lowerBlock as SurfaceBlock;
                                if (sb.haveSupportingStructure)
                                {
                                    haveSupport = true;
                                    surfMaterial = sb.structureBlockRenderer != null ? PoolMaster.MATERIAL_ADVANCED_COVERING_ID : ResourceType.CONCRETE_ID;
                                }
                                else haveSupport = false;
                            }
                            break;
                        case BlockType.Cave:
                            {
                                CaveBlock cvb = lowerBlock as CaveBlock;
                                if (cvb != null) surfMaterial = cvb.ceilingMaterial;
                                else surfMaterial = lowerBlock.material_id;
                            }
                            break;
                    }
                    if (haveSupport)
                    {
                        if (convertToSurfaceType) myChunk.ReplaceBlock(pos, BlockType.Surface, lowerBlock.material_id, false);
                        else myChunk.ReplaceBlock(pos, BlockType.Cave, surfMaterial, ceilingMaterial, false);
                    }
                    else
                    {
                        if (convertToSurfaceType) myChunk.DeleteBlock(pos);
                        else myChunk.ReplaceBlock(pos, BlockType.Cave, -1, ceilingMaterial, false);
                    }
                }                
            }
        }
        return blocksCount;
    }

    public void SetFossilsVolume(int x)
    {
        naturalFossils = x;
    }

    public CubeBlock(Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id, bool naturalGeneration) : base(f_chunk, f_chunkPos)
    {
        type = BlockType.Cube;
        excavatingStatus = 0;
        naturalFossils = MAX_VOLUME;
        volume = MAX_VOLUME; career = false;     
        material_id = f_material_id;
        if (naturalGeneration) { naturalFossils = MAX_VOLUME; }
        else naturalFossils = 0;
    }

    public override void ReplaceMaterial(int newId)
    {
        if (newId == material_id) return;
        material_id = newId;
        myChunk.RefreshBlockVisualising(this);
    }

    void CheckExcavatingStatus()
    {
        if (!career) return;
        if (volume == 0)
        {
            //myChunk.DeleteBlock(pos);
            Block lowerBlock = myChunk.GetBlock(pos.x, pos.y - 1, pos.z);
            if (lowerBlock == null) {
                myChunk.DeleteBlock(pos);
            }
            else
            {
                if (lowerBlock.type == BlockType.Surface)
                {
                    if (!(lowerBlock as SurfaceBlock).haveSupportingStructure & myChunk.CalculateSupportPoints(lowerBlock.pos.x, lowerBlock.pos.y, lowerBlock.pos.z) > Chunk.SUPPORT_POINTS_ENOUGH_FOR_HANGING) myChunk.ReplaceBlock(lowerBlock.pos, BlockType.Cave, lowerBlock.material_id, material_id, false);
                }
                //проверка на верхний блок не нужна, так как добывается открыто
                myChunk.ReplaceBlock(pos, BlockType.Surface, material_id, false);
            }
            
        }
        float pc = volume / (float)MAX_VOLUME;
        if (pc > 0.5f)
        {
            if (pc > 0.75f)
            {
                if (excavatingStatus != 0)
                {
                    excavatingStatus = 0;
                    myChunk.RefreshBlockVisualising(this);
                }
            }
            else
            {
                if (excavatingStatus != 1)
                {
                    excavatingStatus = 1;
                    myChunk.RefreshBlockVisualising(this);
                }
            }
        }
        else
        { // выкопано больше половины
            if (pc > 0.25f)
            {
                if (excavatingStatus != 2)
                {
                    excavatingStatus = 2;
                    myChunk.RefreshBlockVisualising(this);
                }
            }
            else
            {
                if (excavatingStatus != 3)
                {
                    excavatingStatus = 3;
                    myChunk.RefreshBlockVisualising(this);
                }
            }

        }
    }

    override public List<BlockpartVisualizeInfo> GetVisualDataList(byte visibilityMask)
    {
        return null;
    }
    override public BlockpartVisualizeInfo GetVisualData(byte face)
    {
        if (face > 5) return null;
        else
        {
            var mvi = new MeshVisualizeInfo(face, myChunk.GetLightValue(pos), material_id);
            MeshType mtype = MeshType.Quad;
            if (face == 4 & career)
            {
                float pc = volume / (float)MAX_VOLUME;
                if (pc < 0.75f)
                {
                    if (pc > 0.5f) mtype = MeshType.ExcavatedPlane025;
                    else
                    {
                        if (pc < 0.25f) mtype = MeshType.ExcavatedPlane075;
                        else mtype = MeshType.ExcavatedPlane05;
                    }
                }
            }
            var bvi = new BlockpartVisualizeInfo(pos, mvi, mtype);
            return bvi;
        }
    }

    #region save-load system
    override public void Save( System.IO.FileStream fs)
    {
        SaveBlockData(fs);
        if (career) fs.WriteByte(1); else fs.WriteByte(0);
        fs.Write(System.BitConverter.GetBytes(naturalFossils),0,4);
        fs.Write(System.BitConverter.GetBytes(volume),0,4);
        //SERIALIZER_LENGTH = 9;
    }

    public void LoadCubeBlockData(System.IO.FileStream fs)
    {
        career = fs.ReadByte() == 1;
        var data = new byte[8];
        fs.Read(data, 0, data.Length);
        naturalFossils = System.BitConverter.ToSingle(data, 0);
        volume = System.BitConverter.ToInt32(data, 4);
        if (career) CheckExcavatingStatus();
    }
    #endregion    
}
