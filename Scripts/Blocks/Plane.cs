using UnityEngine;
public sealed class Plane 
{
    public int materialID { get; private set; }
    public byte faceIndex { get; private set; }
    public MeshType meshType { get; private set; }
    public Structure mainStructure  { get; private set; }    
    public bool visible { get; private set; }
    public PlaneExtension extension { get; private set; }
    readonly public BlockExtension myBlockExtension;
    private bool dirty = false; // запрещает удалять плоскость для оптимизации

    public bool isClean() //может быть удалена и восстановлена
    {
        if (!dirty && extension == null && mainStructure == null && materialID == myBlockExtension.GetMaterialID()) return true;
        else return false;
    }

    public Plane(BlockExtension i_parent, MeshType i_meshType, int i_materialID, byte i_faceIndex)
    {
        myBlockExtension = i_parent;
        meshType = i_meshType;
        materialID = i_materialID;
        mainStructure = null;
        faceIndex = i_faceIndex;
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

    public void ReplaceMaterial(int newId)
    {
        materialID = newId;
        var b = myBlockExtension.myBlock;
        b.myChunk.ChangeBlockVisualData(b, faceIndex);
    }

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

    public void Annihilate()
    {
        

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
            byte light;
            switch (faceIndex)
            {
                case Block.FWD_FACE_INDEX: light = chunk.GetLightValue(cpos.x, cpos.y, cpos.z + 1); break;
                case Block.RIGHT_FACE_INDEX: light = chunk.GetLightValue(cpos.x + 1, cpos.y, cpos.z);break;
                case Block.BACK_FACE_INDEX: light = chunk.GetLightValue(cpos.x, cpos.y, cpos.z - 1);break;
                case Block.LEFT_FACE_INDEX: light = chunk.GetLightValue(cpos.x - 1, cpos.y, cpos.z);break;
                case Block.UP_FACE_INDEX: light = chunk.GetLightValue(cpos.x, cpos.y + 1, cpos.z);break;
                case Block.DOWN_FACE_INDEX: light = chunk.GetLightValue(cpos.x, cpos.y - 1, cpos.z);break;
                case Block.SURFACE_FACE_INDEX:
                case Block.CEILING_FACE_INDEX:
                default:
                    light = chunk.GetLightValue(cpos);break;
            }
            return new BlockpartVisualizeInfo(cpos,
                new MeshVisualizeInfo(faceIndex, PoolMaster.GetMaterialType(materialID), light),
                meshType,
                materialID
                );
        }
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
