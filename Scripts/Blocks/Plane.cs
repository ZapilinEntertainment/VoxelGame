using UnityEngine;
using System.Collections.Generic;
public class Plane : MyObject
{
    public VisibilityMode visibilityMode { get; protected set; }
    public bool invisibleByOptimization { get; protected set; }
    public bool haveWorksite { get; protected set; }
    public bool destroyed { get; private set; }
    public int materialID { get; protected set; }
    public byte faceIndex { get; protected set; }
    public MeshType meshType { get; protected set; }
    protected byte meshRotation;
    public Structure mainStructure { get; protected set; }    
    public PlaneExtension extension { get; protected set; }
    public FullfillStatus fulfillStatus
    {
        get
        {
            if (mainStructure != null)  {
                if (mainStructure.surfaceRect == SurfaceRect.full) return FullfillStatus.Full;
                else return extension?.fulfillStatus ?? FullfillStatus.Unknown;
            }
            else
            {
                if (extension != null) return extension.fulfillStatus;
                else return FullfillStatus.Empty;
            }
        }
    }
    protected bool dirty = false; // запрещает удалять плоскость для оптимизации
    public int artificialStructuresCount {
        get { if (extension != null) return extension.artificialStructuresCount;
            else {
                if (mainStructure == null) return 0;
                else
                {
                    if (mainStructure.isArtificial) return 1; else return 0;
                }
            }
        }
    }
    public int structuresCount
    {
        get
        {
            if (extension != null) return extension.structuresCount;
            else
            {
                if (mainStructure == null) return 0; else return 1;
            }
        }
    }

    public IPlanable host { get; protected set; }
    public Chunk myChunk { get {
            var b = host.GetBlock();
            if (b == null)
            {
                Debug.Log(StackTraceUtility.ExtractStackTrace());
                return GameMaster.realMaster.mainChunk;
            }
            else return host.GetBlock().myChunk;
            } }
    public ChunkPos pos { get {
            var b = host.GetBlock();
            if (b == null)
            {
                Debug.Log(StackTraceUtility.ExtractStackTrace());
                return ChunkPos.zer0;
            }
            else  return host.GetBlock().pos;
        } }
    public event System.Action<VisibilityMode, bool> visibilityChangedEvent;

    public static readonly MeshType defaultMeshType = MeshType.Quad;
    private static UISurfacePanelController observer;
    protected const byte BASIC_PLANE_CODE = 0, MULTIMATERIAL_PLANE_CODE = 1;

    protected override bool IsEqualNoCheck(object obj)
    {
        var p = (Plane)obj;
        return faceIndex == p.faceIndex && materialID == p.materialID && meshType == p.meshType && destroyed == p.destroyed;
    }
    public override int GetHashCode()
    {
        return host.GetHashCode() + faceIndex + materialID + (int)meshType;
    }

    #region save-load system
    virtual public void Save(System.IO.FileStream fs)
    {        
        if (destroyed) return;
        fs.WriteByte(BASIC_PLANE_CODE);
        SaveData(fs);
    }
    protected void SaveData(System.IO.FileStream fs)
    {
        // 0 - идентификатор
        //сохранить meshrotation, если это крыша, или если grassland
        fs.WriteByte((byte)meshType); // 1        
        fs.Write(System.BitConverter.GetBytes(materialID), 0, 4); // 2 - 5
        fs.WriteByte(faceIndex); // 6
        fs.WriteByte(meshRotation); // 7

        if (extension != null)
        {
            fs.WriteByte(1); // 8
            extension.Save(fs);
        }
        else
        {
            if (mainStructure != null)
            {
                var data = mainStructure.Save().ToArray();
                fs.WriteByte(2);
                fs.Write(data, 0, data.Length);
                if (mainStructure.IsIPlanable())
                {
                    (mainStructure as IPlanable).SavePlanesData(fs);
                }
            }
            else fs.WriteByte(0);
        }
    }
    public static Plane Load(System.IO.FileStream fs, IPlanable host)
    {
        var data = new byte[9];
        fs.Read(data, 0, data.Length);
        int materialID = System.BitConverter.ToInt32(data, 2);
        Plane p;
        if (data[0] == BASIC_PLANE_CODE) p = new Plane(host, (MeshType)data[1], materialID, data[6], data[7]);
        else
        {
            p = new MultimaterialPlane(host, (MeshType)data[1], data[6], data[7]);
        }
        switch (data[8])
        {
            case 1:
                PlaneExtension.Load(fs, p); break;
            case 2:
                Structure.LoadStructure(fs, p);
                break;
        }
        return p;
    }
    #endregion

    public bool isClean //может быть удалена и восстановлена
    {
        get
        {
            if (dirty) return false;
            else
            {
                if (materialID != host.GetBlock().GetMaterialID())
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
    }
    public bool isQuad
    {
        get { return (meshType == MeshType.Quad | meshType == MeshType.FoundationSide); }
    }
    public bool isSurface
    {
        get
        {
            if (isQuad)
            {
                return (faceIndex == Block.SURFACE_FACE_INDEX | faceIndex == Block.UP_FACE_INDEX);
            }
            else return false;
        }
    }
    public bool haveGrassland
    {
        get { return extension?.HaveGrassland() ?? false; }
    }
    public bool isTerminal
    {
        get
        {
            switch (faceIndex)
            {
                case Block.FWD_FACE_INDEX: return pos.z == Chunk.chunkSize - 1;
                case Block.RIGHT_FACE_INDEX: return pos.x == Chunk.chunkSize - 1;
                case Block.BACK_FACE_INDEX: return pos.z == 0;
                case Block.LEFT_FACE_INDEX: return pos.x == 0;
                case Block.UP_FACE_INDEX: return pos.y == Chunk.chunkSize - 1;
                case Block.DOWN_FACE_INDEX: return pos.y == 0;
                default:
                    return false;
            }
        }
    } // является ли крайней плоскостью чанка
    public bool isTransparent
    {
        get { return MeshMaster.IsMeshTransparent(meshType); }
    }
    public bool isInvicible
    {
        get
        {
            if (extension == null) return mainStructure?.indestructible ?? false;
            else return extension.isInvincible;
        }
    }

    public Plane(IPlanable i_host, MeshType i_meshType, int i_materialID, byte i_faceIndex, byte i_meshRotation)
    {
        host = i_host;
        meshType = i_meshType;
        materialID = i_materialID;
        mainStructure = null;
        faceIndex = i_faceIndex;
        meshRotation = i_meshRotation;
        visibilityMode = VisibilityMode.DrawAll;
        if (i_meshType != defaultMeshType | meshRotation != 0) dirty = true;
    }

    public void SetMeshRotation(byte x, bool sendRedrawRequest)
    {
        if (meshRotation != x)
        {
            meshRotation = x;
            if (sendRedrawRequest) myChunk.RefreshBlockVisualising(host.GetBlock(), faceIndex);
        }
    }

    public void SetVisibilityMode(VisibilityMode vmode) { SetVisibilityMode(vmode, false); }
    virtual public void SetVisibilityMode(VisibilityMode vmode, bool forcedRefresh)
    {
        if (visibilityMode != vmode | forcedRefresh)
        {
            visibilityMode = vmode;
            visibilityChangedEvent?.Invoke(visibilityMode, forcedRefresh);
        }
    }
    public void SetBasisVisibility()
    {
        SetVisibilityMode(GetBlock()?.GetVisibilityMode() ?? VisibilityMode.DrawAll, true);
    }

    public Structure CreateStructure(int id)
    {
        // no checks?
        var s = Structure.GetStructureByID(id);
        s?.SetBasement(this);
        return s;
    }
    public void AddStructure(Structure s)
    {
        if (s.surfaceRect != SurfaceRect.full)
        {            
            FORCED_GetExtension().AddStructure(s);
            if (s.placeInCenter) mainStructure = s;
            return;
        }
        else
        {            
            if (extension != null)
            {
                extension.Annihilate(PlaneAnnihilationOrder.ExtensionRemovedByFullScaledStructure);
                extension = null;
            }
            mainStructure?.Annihilate(StructureAnnihilationOrder.GetReplacingOrder(mainStructure.isArtificial));
            mainStructure = s;
            var t = s.transform;
            t.parent = host.GetBlock().myChunk.transform;
            if (!(s.IsIPlanable())) t.rotation = Quaternion.Euler(GetEulerRotationForQuad());
            if (!(s is Hotel)) t.position = GetCenterPosition(); else //костыль
            {
                switch(faceIndex)
                {
                    case Block.FWD_FACE_INDEX:
                    case Block.RIGHT_FACE_INDEX:
                    case Block.LEFT_FACE_INDEX:
                    case Block.BACK_FACE_INDEX:
                        t.position = GetCenterPosition() + GetLookVector() * 0.5f * Block.QUAD_SIZE + Vector3.down * 0.5f * Block.QUAD_SIZE; break;
                    case Block.DOWN_FACE_INDEX:
                        t.position = GetCenterPosition() + GetLookVector() * Block.QUAD_SIZE; break;
                    default: t.position = GetCenterPosition();break;
                }
            }
            mainStructure.SetVisibility(visibilityMode, true);
        }
    }
    public void RemoveStructure(Structure s)
    {
        if (mainStructure != null && s == mainStructure) mainStructure = null;
        extension?.RemoveStructure(s);
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
    public Plant[] GetPlants()
    {
        if (extension == null) return null;
        else return extension.GetPlants();
    }
    public Structure GetRandomStructure()
    {
        if (extension != null) return extension.GetRandomStructure();
        else return mainStructure;
    }

    virtual public void ChangeMaterial(int newId, bool redrawCall)
    {
        if (materialID == newId) return;
        materialID = newId;
        if (materialID != host.GetBlock().GetMaterialID()) dirty = true;
        if (haveGrassland && !Nature.MaterialIsLifeSupporting(materialID))
        {
            extension?.RemoveGrassland();
        }
        if (redrawCall) myChunk.RefreshBlockVisualising(host.GetBlock(), faceIndex);
    }
    public void SetWorksitePresence(bool x)
    {
        haveWorksite = x;
    }
    public void NullifyExtensionLink(PlaneExtension e)
    {
        if (extension == e) extension = null;
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
                        meshRotation = (byte)Random.Range(0, 4);
                        dirty = true;
                        if (visibilityMode != VisibilityMode.Invisible) myChunk.RefreshBlockVisualising(host.GetBlock(), faceIndex);
                    }
                }
                else
                {
                    if (meshType != MeshType.ExcavatedPlane025)
                    {
                        meshType = MeshType.ExcavatedPlane025;
                        meshRotation = (byte)Random.Range(0, 4);
                        dirty = true;
                        if (haveGrassland) extension?.RemoveGrassland();
                        if (visibilityMode != VisibilityMode.Invisible) myChunk.RefreshBlockVisualising(host.GetBlock(), faceIndex);
                    }
                }
            }
            else
            {
                if (x < 0.25f)
                {
                    if (meshType != MeshType.ExcavatedPlane075)
                    {
                        meshType = MeshType.ExcavatedPlane075;
                        meshRotation = (byte)Random.Range(0, 4);
                        dirty = true;
                        if (haveGrassland) extension?.RemoveGrassland();
                        if (visibilityMode != VisibilityMode.Invisible) myChunk.RefreshBlockVisualising(host.GetBlock(), faceIndex);
                    }
                }
                else
                {
                    if (meshType != MeshType.ExcavatedPlane05)
                    {
                        meshType = MeshType.ExcavatedPlane05;
                        meshRotation = (byte)Random.Range(0, 4);
                        dirty = true;
                        if (haveGrassland) extension?.RemoveGrassland();
                        if (visibilityMode != VisibilityMode.Invisible) myChunk.RefreshBlockVisualising(host.GetBlock(), faceIndex);
                    }
                }
            }
        }
    }

    public Block GetBlock()
    {
        return host?.GetBlock();
    }
    public bool TryCreateGrassland(out Grassland g)
    {
        if (mainStructure != null && mainStructure.surfaceRect == SurfaceRect.full)
        {
            g = null;
            return false;
        }
        else
        {
            if (Nature.IsPlaneSuitableForGrassland(this))
            {
                if (extension == null) FORCED_GetExtension();
                return extension.TryCreateGrassland(out g);
            }
            else
            {
                g = null;
                return false;
            }
        }
    }
    public Grassland GetGrassland()
    {
        return extension?.grassland;
    }
    public void RemoveGrassland(Grassland g, bool sendAnnihilationRequest)
    {
        extension?.RemoveGrassland(g, sendAnnihilationRequest);
        ChangeMaterial(host.GetBlock().GetMaterialID(), true);
    }

    public void EnvironmentalStrike(Vector3 hitpoint, byte radius, float damage)
    {
        if (mainStructure != null) mainStructure.ApplyDamage(damage);
        else
        {
            if (extension != null) extension.EnvironmentalStrike(hitpoint, radius, damage);
            else host.Damage(damage, faceIndex);
        }
    }
 
    public PlaneExtension FORCED_GetExtension()
    {
        if (extension == null) extension = new PlaneExtension(this, mainStructure);
        return extension;
    }
    public bool ContainStructures()
    {
        if (mainStructure != null) return true;
        else
        {
            if (extension == null) return false;
            else return (extension.fulfillStatus != FullfillStatus.Empty);
        }
    }
    public bool IsAnyBuildingInArea(SurfaceRect sa)
    {
        if (extension != null) return extension.IsAnyBuildingInArea(sa);
        else
        {
            if (mainStructure != null) return true;
            else return false;
        }
    }

    virtual public BlockpartVisualizeInfo GetVisualInfo(Chunk chunk, ChunkPos cpos)
    {
        if ( materialID == PoolMaster.NO_MATERIAL_ID | meshType == MeshType.NoMesh) return null;
        else
        {
            return new BlockpartVisualizeInfo(cpos,
                new MeshVisualizeInfo(faceIndex, PoolMaster.GetMaterialType(materialID), 
                GetLightValue(chunk, cpos, faceIndex)),
                meshType,
                materialID,
                meshRotation
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
    public static byte GetFaceCounter(byte faceIndex)
    {
        switch (faceIndex)
        {
            case Block.RIGHT_FACE_INDEX: return Block.LEFT_FACE_INDEX;
            case Block.BACK_FACE_INDEX: return Block.FWD_FACE_INDEX;
            case Block.LEFT_FACE_INDEX: return Block.RIGHT_FACE_INDEX;
            case Block.UP_FACE_INDEX: return Block.DOWN_FACE_INDEX;
            case Block.DOWN_FACE_INDEX: return Block.UP_FACE_INDEX;
            case Block.SURFACE_FACE_INDEX: return Block.CEILING_FACE_INDEX;
            case Block.CEILING_FACE_INDEX: return Block.SURFACE_FACE_INDEX;
            default: return Block.BACK_FACE_INDEX;
        }
    }

    public Vector3 GetCenterPosition()
    {
        Vector3 centerPos = host.GetBlock().pos.ToWorldSpace();
        float q = Block.QUAD_SIZE * 0.5f;
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                centerPos += Vector3.forward * q;
                break;
            case Block.RIGHT_FACE_INDEX:
                centerPos += Vector3.right * q;
                break;
            case Block.BACK_FACE_INDEX:
                centerPos += Vector3.back * q;
                break;
            case Block.LEFT_FACE_INDEX:
                centerPos += Vector3.left * q;
                break;
            case Block.DOWN_FACE_INDEX:
                centerPos += Vector3.down * q;
                break;
            case Block.CEILING_FACE_INDEX:
                centerPos += Vector3.up * (q - Block.CEILING_THICKNESS);
                break;
            case Block.UP_FACE_INDEX:
                centerPos += Vector3.up * q;
                break;
            case Block.SURFACE_FACE_INDEX:
            default:
                centerPos += Vector3.down * q;
                break;
        }
        return centerPos;
    }
    public Vector3 GetLocalPosition(float x, float z)
    {
        Vector3 blockCenter = pos.ToWorldSpace(), xdir, zdir;
        float q = Block.QUAD_SIZE;
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                blockCenter += new Vector3(0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.left * q;
                zdir = Vector3.up * q;
                break;
            case Block.RIGHT_FACE_INDEX:
                blockCenter += new Vector3(0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
            case Block.BACK_FACE_INDEX:
                blockCenter += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.up * q;
                break;
            case Block.LEFT_FACE_INDEX:
                blockCenter += new Vector3(-0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.back * q;
                zdir = Vector3.up * q;
                break;
            case Block.DOWN_FACE_INDEX:
                blockCenter += new Vector3(-0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.CEILING_FACE_INDEX:
                blockCenter += new Vector3(-0.5f, +0.5f, 0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.UP_FACE_INDEX:
                blockCenter += new Vector3(-0.5f, +0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.forward * q;
                break;
            case Block.SURFACE_FACE_INDEX:
            default:
                blockCenter += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.forward * q;
                break;
        }
        float ir = PlaneExtension.INNER_RESOLUTION;
        return blockCenter + xdir * x / ir + zdir * z / ir;
    }
    public Vector3 GetLocalPosition(SurfaceRect sr)
    {
        return GetLocalPosition(sr.x + sr.size / 2f, sr.z + sr.size / 2f);
    }
    public Vector3 GetEulerRotationForQuad()
    {
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX:return Vector3.right * 90f;
            case Block.RIGHT_FACE_INDEX: return Vector3.back * 90f;
            case Block.BACK_FACE_INDEX: return Vector3.left * 90f;
            case Block.LEFT_FACE_INDEX: return Vector3.forward * 90f;
            case Block.DOWN_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
                return Vector3.right * 180f;
            case Block.UP_FACE_INDEX:
            case Block.SURFACE_FACE_INDEX:
            default:
                return Vector3.zero;
        }
    }
    public Vector3 GetEulerRotationForBlockpart()
    {
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX: return Vector3.zero;
            case Block.RIGHT_FACE_INDEX: return Vector3.up * 90f ;
            case Block.BACK_FACE_INDEX: return Vector3.up * 180f;
            case Block.LEFT_FACE_INDEX: return Vector3.down * 90f;            
            case Block.DOWN_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
                return new Vector3(90f, 90f, 0f);
            case Block.UP_FACE_INDEX:
            case Block.SURFACE_FACE_INDEX:
            default:
                return Vector3.left * 90f;
        }
    }
    public Vector3 GetLookVector()
    {
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX: return Vector3.forward;
            case Block.RIGHT_FACE_INDEX: return Vector3.right;
            case Block.BACK_FACE_INDEX: return Vector3.back;
            case Block.LEFT_FACE_INDEX: return Vector3.left;            
            case Block.DOWN_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
                return Vector3.down;
            case Block.SURFACE_FACE_INDEX:
            case Block.UP_FACE_INDEX:
            default:
                return Vector3.up;
        }
    }
    public ChunkPos GetLookingPosition()
    {
        switch(faceIndex)
        {
            case Block.CEILING_FACE_INDEX:
            case Block.SURFACE_FACE_INDEX:
                return pos;
            case Block.RIGHT_FACE_INDEX: return pos.OneBlockRight();
            case Block.BACK_FACE_INDEX: return pos.OneBlockBack();
            case Block.FWD_FACE_INDEX: return pos.OneBlockForward();
            case Block.LEFT_FACE_INDEX: return pos.OneBlockLeft();
            case Block.DOWN_FACE_INDEX: return pos.OneBlockDown();
            default: return pos.OneBlockHigher();
        }
    }
    /// <summary>
    /// returns in 0 - 1 
    /// </summary>
    public Vector2 WorldToMapPosition(Vector3 point)
    {
        Vector3 dir = point - GetLocalPosition(0, 0);
        switch (faceIndex)
        {
            case Block.FWD_FACE_INDEX: return new Vector2(dir.x, dir.y);
            case Block.RIGHT_FACE_INDEX: return new Vector2(dir.z, dir.y);
            case Block.BACK_FACE_INDEX: return new Vector2(dir.x, dir.y);
            case Block.LEFT_FACE_INDEX: return new Vector2(dir.z, dir.y);
            case Block.DOWN_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
                return new Vector2(dir.x, dir.z);
            case Block.SURFACE_FACE_INDEX:
            case Block.UP_FACE_INDEX:
            default:
                return new Vector2(dir.x, dir.z);
        }
    }

    public UISurfacePanelController ShowOnGUI()
    {
        if (observer == null)
        {
            observer = UISurfacePanelController.InitializeSurfaceObserverScript();
        }
        else observer.gameObject.SetActive(true);
        observer.SetObservingSurface(this);
        return observer;
    }

    virtual public void Annihilate(PlaneAnnihilationOrder order)
    {
        if (!destroyed)
        {
            destroyed = true;
            if (extension != null) extension.Annihilate(order);
            else
            {
                mainStructure?.Annihilate(order.GetStructuresOrder());
            }
            if (!order.chunkClearing)
            {
                if (haveWorksite)
                {
                    GameMaster.realMaster.colonyController.RemoveWorksite(this);
                    haveWorksite = false;
                }
                if (faceIndex == Block.UP_FACE_INDEX | faceIndex == Block.SURFACE_FACE_INDEX)
                {
                    var b = host?.GetBlock();
                    if (b!= null) b.myChunk.needSurfacesUpdate = true;
                }
            }
        }
    } 
}
