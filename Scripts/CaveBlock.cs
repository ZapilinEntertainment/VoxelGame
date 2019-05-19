using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CaveBlock : SurfaceBlock
{
    public bool haveSurface { get; private set; }
    public int ceilingMaterial { get; private set; }
    public const float CEILING_THICKNESS = 0.1f;

    public CaveBlock(Chunk f_chunk, ChunkPos f_chunkPos, int f_up_material_id, int f_down_material_id) : base(f_chunk, f_chunkPos, f_down_material_id)
    {
        type = BlockType.Cave;
        ceilingMaterial = f_up_material_id;
        haveSurface = material_id != PoolMaster.NO_MATERIAL_ID;
    }

    public override void ReplaceMaterial(int newId)
    {
        if (newId == material_id) return;
        material_id = newId;
        ceilingMaterial = newId;
        if (grassland != null & material_id != ResourceType.DIRT_ID & material_id != ResourceType.FERTILE_SOIL_ID)
        {
            grassland.Annihilation(false, true);
        }
        myChunk.RefreshBlockVisualising(this);
    }
    public void ReplaceSurfaceMaterial(int i_id)
    {
        base.ReplaceMaterial(i_id);
    }
    public void ReplaceCeilingMaterial(int i_id)
    {
        ceilingMaterial = i_id;
        myChunk.RefreshBlockVisualising(this);
    }

    public void DestroySurface()
    {
        if (!haveSurface) return;
        var lb = myChunk.GetBlock(pos.x, pos.y - 1, pos.z);
        if (lb != null && (lb.type == BlockType.Cave | lb.type == BlockType.Cube)) return;
        haveSurface = false;
        material_id = PoolMaster.NO_MATERIAL_ID;
        if (grassland != null) grassland.Annihilation(false, true);
        if (structures.Count != 0) ClearSurface(true);
        myChunk.RefreshBlockVisualising(this);
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 47);
    }
    public void RestoreSurface(int newMaterialID)
    {
        if (haveSurface) return;
        material_id = newMaterialID;
        haveSurface = true;
        myChunk.ChangeBlockVisualData(this, 6);
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 15);

        var lb = myChunk.GetBlock(pos.x, pos.y - 1, pos.z);
        if (lb != null)
        {
            switch (lb.type)
            {
                case BlockType.Shapeless:
                    myChunk.ReplaceBlock(lb.pos, BlockType.Cave, PoolMaster.NO_MATERIAL_ID, material_id, false);
                    break;
                case BlockType.Surface:
                    myChunk.ReplaceBlock(lb.pos, BlockType.Cave, lb.material_id, material_id, false);
                    break;
            }
        }
        else
        {
            if (pos.y - 1 >= 0)
            {
                myChunk.AddBlock(new ChunkPos(pos.x, pos.y - 1, pos.z), BlockType.Cave, PoolMaster.NO_MATERIAL_ID, material_id, false);
            }
        }
    }

    override public List<BlockpartVisualizeInfo> GetVisualDataList(byte visibilityMask)
    {
        var data = new List<BlockpartVisualizeInfo>();
        MaterialType ceilingMaterialType = PoolMaster.GetMaterialType(ceilingMaterial),
            floorMaterialType = PoolMaster.GetMaterialType(material_id);
        if ((visibilityMask & 1) != 0)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(0, ceilingMaterialType, myChunk.GetLightValue(pos.x, pos.y, pos.z + 1)),
                    MeshType.CaveCeil,
                    ceilingMaterial
                    ));
        }
        if ((visibilityMask & 2) != 0)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(1, ceilingMaterialType, myChunk.GetLightValue(pos.x + 1, pos.y, pos.z)),
                    MeshType.CaveCeil,
                    ceilingMaterial
                    ));
        }
        if ((visibilityMask & 4) != 0)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(2, ceilingMaterialType, myChunk.GetLightValue(pos.x, pos.y, pos.z - 1)),
                    MeshType.CaveCeil,
                    ceilingMaterial
                    ));
        }
        if ((visibilityMask & 8) != 0)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(3, ceilingMaterialType, myChunk.GetLightValue(pos.x - 1, pos.y, pos.z)),
                    MeshType.CaveCeil,
                    ceilingMaterial
                    ));
        }
        if ((visibilityMask & 16) != 0)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(4, ceilingMaterialType, myChunk.GetLightValue(pos.x, pos.y + 1, pos.z)),
                    MeshType.Quad,
                    ceilingMaterial
                    ));
        }
        if ((visibilityMask & 32) != 0 & haveSurface)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(5, floorMaterialType, myChunk.GetLightValue(pos.x, pos.y - 1, pos.z )),
                    MeshType.Quad,
                    material_id
                    ));
        }
        byte innerLight = myChunk.GetLightValue(pos.x, pos.y, pos.z);
        if ((visibilityMask & 64) != 0 & haveSurface)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(6, floorMaterialType, innerLight),
                    MeshType.Quad,
                    material_id
                    ));
        }
        if ((visibilityMask & 128) != 0)
        {
            data.Add(new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(7, ceilingMaterialType, innerLight),
                    MeshType.Quad,
                    ceilingMaterial
                    ));
        }
        return data; 
    }
    override public BlockpartVisualizeInfo GetFaceVisualData(byte face)
    {
        switch (face)
        {
            case 0:
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x, pos.y, pos.z + 1)),
                    MeshType.CaveCeil,
                    ceilingMaterial
                    );
            case 1:
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x + 1, pos.y, pos.z)),
                    MeshType.CaveCeil
                    , ceilingMaterial
                    );
            case 2:
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x, pos.y, pos.z - 1)),
                    MeshType.CaveCeil
                    , ceilingMaterial
                    );
            case 3:
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x - 1, pos.y, pos.z)),
                    MeshType.CaveCeil
                    , ceilingMaterial
                    );
            case 4:
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x, pos.y + 1, pos.z)),
                    MeshType.Quad
                    , ceilingMaterial
                    );
            case 5:
                if (!haveSurface) return null;
                else
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x , pos.y - 1, pos.z)),
                    MeshType.Quad
                    , ceilingMaterial
                    );
            case 6:
                if (!haveSurface) return null;
                else
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(material_id), myChunk.GetLightValue(pos.x , pos.y, pos.z)),
                    MeshType.Quad
                    , material_id
                    );
            case 7:
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x , pos.y, pos.z)),
                    MeshType.Quad
                    , ceilingMaterial
                    );
            default: return null;
        }
    }

    #region save-load system
    override public void Save(System.IO.FileStream fs)
    {
        int m_id = material_id;
        if (!haveSurface) material_id = -1;
        SaveBlockData(fs);
        material_id = m_id;

        fs.Write(System.BitConverter.GetBytes(ceilingMaterial), 0, 4);        
        SaveSurfaceBlockData(fs);      
    }
    #endregion
}
