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
        haveSurface = false;
        if (grassland != null) grassland.Annihilation(false, true);
        if (surfaceObjects.Count != 0) ClearSurface(true);
        myChunk.RefreshBlockVisualising(this);
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 47);
    }
    public void RestoreSurface(int newMaterialID)
    {
        if (haveSurface) return;
        ceilingMaterial = newMaterialID;
        haveSurface = true;
        myChunk.RefreshBlockVisualising(this);
        myChunk.ApplyVisibleInfluenceMask(pos.x, pos.y, pos.z, 15);
    }

    override public List<BlockpartVisualizeInfo> GetVisualDataList(byte visibilityMask)
    {
        return null;
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
                return new BlockpartVisualizeInfo(
                    pos,
                    new MeshVisualizeInfo(face, PoolMaster.GetMaterialType(ceilingMaterial), myChunk.GetLightValue(pos.x , pos.y - 1, pos.z)),
                    MeshType.Quad
                    , ceilingMaterial
                    );
            case 6:
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
