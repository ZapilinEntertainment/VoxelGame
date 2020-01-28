using UnityEngine;
using System.Collections.Generic;
public sealed class Plane 
{
    public int materialID { get; private set; }
    public byte faceIndex { get; private set; }
    public MeshType meshType { get; private set; }
    public Structure mainStructure  { get; private set; }  
    public bool visible { get; private set; }
    public PlaneExtension extension { get; private set; }
    public Worksite worksite { get; private set; }
    private bool dirty = false; // запрещает удалять плоскость для оптимизации

    public Chunk myChunk { get { return myBlockExtension.myBlock.myChunk; } }
    public ChunkPos pos { get { return myBlockExtension.myBlock.pos; } }

    public readonly BlockExtension myBlockExtension;
    public static readonly MeshType defaultMeshType = MeshType.Quad;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Plane p = (Plane)obj;
        return faceIndex == p.faceIndex && myBlockExtension == p.myBlockExtension && materialID == p.materialID && meshType == p.meshType;
    }
    public override int GetHashCode()
    {
        return myBlockExtension.GetHashCode()+ faceIndex + materialID + (int)meshType;
    }

    public bool isClean() //может быть удалена и восстановлена
    {
        if (dirty) return false;
        else
        {
            if (materialID != myBlockExtension.myBlock.GetMaterialID())
            {
                dirty = true;
                return false;
            }
            else
            {
                if (extension == null && mainStructure == null) return true;
                else return false;
            }
        }
    }

    public Plane(BlockExtension i_parent, MeshType i_meshType, int i_materialID, byte i_faceIndex)
    {
        myBlockExtension = i_parent;
        meshType = i_meshType;
        materialID = i_materialID;
        mainStructure = null;
        faceIndex = i_faceIndex;
        if (i_meshType != defaultMeshType) dirty = true;
    }

    public void SetVisibility(bool x)
    {
        if (x != visible)
        {
            visible = x;
            mainStructure?.SetVisibility(visible);
        }
    }
    public void AddStructure(Structure s)
    {
        if (s.surfaceRect != SurfaceRect.full)
        {
            var e = GetExtension();
            e.AddStructure(s);
            if (mainStructure.IsDestroyed()) mainStructure = null;
            return;
        }
        else
        {
            if (extension != null)
            {
                extension.ClearSurface(false, true, false);
                extension = null;
            }
            mainStructure?.Annihilate(false, true, false);
            mainStructure = s;
            var t = mainStructure.transform;
            t.parent = myBlockExtension.myBlock.myChunk.transform;
            switch (faceIndex)
            {
                case Block.FWD_FACE_INDEX:
                    t.rotation = Quaternion.Euler(90f, s.modelRotation * 45f, 0f);
                    break;
                case Block.RIGHT_FACE_INDEX: t.rotation = Quaternion.Euler(0f, s.modelRotation * 45f, -90f); break;
                case Block.BACK_FACE_INDEX: t.rotation = Quaternion.Euler(-90f, s.modelRotation * 45f, 0f); break;
                case Block.LEFT_FACE_INDEX: t.rotation = Quaternion.Euler(0f, s.modelRotation * 45f, 90f); break;
                case Block.DOWN_FACE_INDEX:
                case Block.CEILING_FACE_INDEX:
                    t.rotation = Quaternion.Euler(180f, s.modelRotation * 45f, -90f); break;
                case Block.UP_FACE_INDEX:
                case Block.SURFACE_FACE_INDEX:
                default:
                    t.rotation = Quaternion.Euler(0f, s.modelRotation * 45f, 0f); break;
            }
            t.localPosition = GetCenterPosition();
            s.SetVisibility(visible);
        }
    }
    public void RemoveStructure(Structure s)
    {
        if (extension == null)
        {
            if (mainStructure != null && s == mainStructure) mainStructure = null;
        }
        else extension.RemoveStructure(s);
    }
    public List<Structure> GetStructuresList()
    {
        if (extension == null)
        {
            if (mainStructure == null) return null;
            else return new List<Structure>() { mainStructure };
        }
        else return extension.GetStructuresList();
    }

    public void ReplaceMaterial(int newId)
    {
        if (materialID == newId) return;
        materialID = newId;        
        if (materialID != myBlockExtension.myBlock.GetMaterialID()) dirty = true;
        myChunk.RewriteFaceVisualData(pos, faceIndex);
    }
    public void SetWorksite(Worksite w)
    {
        if (worksite != null && worksite != w) worksite.StopWork();
        worksite = w;
    }
    public void RemoveWorksiteLink(Worksite w)
    {
        if (w == worksite) worksite = null;
    }

    public void VolumeChanges(float x)
    {
        if (meshType == MeshType.Quad || meshType == MeshType.ExcavatedPlane025 || 
            meshType == MeshType.ExcavatedPlane05 || meshType == MeshType.ExcavatedPlane075)
        {
            if (x > 0.5f)
            {
                if (x > 0.75f)
                {
                    if (meshType != MeshType.Quad)
                    {
                        meshType = MeshType.Quad;
                        myChunk.RewriteFaceVisualData(pos, faceIndex);
                    }
                }
                else
                {
                    if (meshType != MeshType.ExcavatedPlane075)
                    {
                        meshType = MeshType.ExcavatedPlane075;
                        myChunk.RewriteFaceVisualData(pos, faceIndex);
                    }
                }
            }
            else
            {
                if (x < 0.25f)
                {
                    if (meshType != MeshType.ExcavatedPlane025)
                    {
                        meshType = MeshType.ExcavatedPlane025;
                        myChunk.RewriteFaceVisualData(pos, faceIndex);
                    }
                }
                else
                {
                    if (meshType != MeshType.ExcavatedPlane05)
                    {
                        meshType = MeshType.ExcavatedPlane075;
                        myChunk.RewriteFaceVisualData(pos, faceIndex);
                    }
                }
            }
        }
    }

    public ChunkPos GetChunkPosition() { return myBlockExtension.myBlock.pos; }
    public Vector3 GetCenterPosition()
    {
        Vector3 leftBottomCorner = myBlockExtension.myBlock.pos.ToWorldSpace(), xdir, zdir;
        float q = Block.QUAD_SIZE;
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                leftBottomCorner += new Vector3(0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.left * q;
                zdir = Vector3.up * q;
                break;
            case Block.RIGHT_FACE_INDEX:
                leftBottomCorner += new Vector3(0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
            case Block.BACK_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.up * q;
                break;
            case Block.LEFT_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.back * q;
                zdir = Vector3.up * q;
                break;
            case Block.DOWN_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.CEILING_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, +0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.UP_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, +0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
            case Block.SURFACE_FACE_INDEX:
            default:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
        }
        return leftBottomCorner + xdir * 0.5f + zdir * 0.5f;
    }    
    public PlaneExtension GetExtension()
    {
        if (extension == null) extension = new PlaneExtension(this, mainStructure);
        return extension;
    }
    public void NullifyExtesionLink(PlaneExtension e)
    {
        if (extension == e) extension = null;
    }

    public BlockpartVisualizeInfo GetVisualInfo(Chunk chunk, ChunkPos cpos)
    {
        if (materialID == PoolMaster.NO_MATERIAL_ID | meshType == MeshType.NoMesh) return null;
        else {           
            return new BlockpartVisualizeInfo(cpos,
                new MeshVisualizeInfo(faceIndex, PoolMaster.GetMaterialType(materialID), GetLightValue(chunk, cpos, faceIndex)),
                meshType,
                materialID
                );
        }
    }
    public static byte GetLightValue(Chunk chunk, ChunkPos cpos, byte faceIndex)
    {
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX: return chunk.GetLightValue(cpos.x, cpos.y, cpos.z + 1); 
            case Block.RIGHT_FACE_INDEX: return chunk.GetLightValue(cpos.x + 1, cpos.y, cpos.z); 
            case Block.BACK_FACE_INDEX: return chunk.GetLightValue(cpos.x, cpos.y, cpos.z - 1);
            case Block.LEFT_FACE_INDEX: return chunk.GetLightValue(cpos.x - 1, cpos.y, cpos.z);
            case Block.UP_FACE_INDEX: return chunk.GetLightValue(cpos.x, cpos.y + 1, cpos.z); 
            case Block.DOWN_FACE_INDEX: return chunk.GetLightValue(cpos.x, cpos.y - 1, cpos.z); 
            case Block.SURFACE_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
            default:
                return chunk.GetLightValue(cpos);
        }
    }

    public void Annihilate(bool compensateStructures)
    {
        if (extension != null) extension.Annihilate(compensateStructures);
        else mainStructure?.SectionDeleted(myBlockExtension.myBlock.pos);
        if (!GameMaster.sceneClearing & faceIndex == Block.SURFACE_FACE_INDEX) myBlockExtension.myBlock.myChunk.needSurfacesUpdate = true;
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {

    }
    public void Load(System.IO.FileStream fs)
    {

    }
    #endregion
}
