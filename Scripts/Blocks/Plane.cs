﻿using UnityEngine;
using System.Collections.Generic;
public sealed class Plane
{
    public bool visible { get; private set; }
    public bool haveWorksite { get; private set; }
    public int materialID { get; private set; }
    public byte faceIndex { get; private set; }
    public MeshType meshType { get; private set; }
    private byte meshRotation;
    public Structure mainStructure { get; private set; }    
    public PlaneExtension extension { get; private set; }
    public FullfillStatus fulfillStatus
    {
        get
        {
            if (mainStructure != null) return FullfillStatus.Full;
            else
            {
                if (extension != null) return extension.fullfillStatus;
                else return FullfillStatus.Empty;
            }
        }
    }
    private bool dirty = false; // запрещает удалять плоскость для оптимизации
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

    public Chunk myChunk { get { return myBlockExtension.myBlock.myChunk; } }
    public ChunkPos pos { get { return myBlockExtension.myBlock.pos; } }

    public readonly BlockExtension myBlockExtension;
    public static readonly MeshType defaultMeshType = MeshType.Quad;
    private static UISurfacePanelController observer;

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
        return myBlockExtension.GetHashCode() + faceIndex + materialID + (int)meshType;
    }

    public bool isClean //может быть удалена и восстановлена
    {
        get
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
    }
    public bool isQuad
    {
        get { return (meshType == MeshType.Quad); }
    }
    public bool haveGrassland
    {
        get { return extension?.HaveGrassland() ?? false; }
    }

    public Plane(BlockExtension i_parent, MeshType i_meshType, int i_materialID, byte i_faceIndex, byte i_meshRotation)
    {
        myBlockExtension = i_parent;
        meshType = i_meshType;
        materialID = i_materialID;
        mainStructure = null;
        faceIndex = i_faceIndex;
        meshRotation = i_meshRotation;
        visible = true;
        if (i_meshType != defaultMeshType | meshRotation != 0) dirty = true;
    }
    public void SetMeshRotation(byte x, bool sendRedrawRequest)
    {
        if (meshRotation != x)
        {
            meshRotation = x;
            if (sendRedrawRequest) myChunk.RefreshBlockVisualising(myBlockExtension.myBlock, faceIndex);
        }
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
            GetExtension().AddStructure(s);
            mainStructure = null;
            return;
        }
        else
        {
            if (extension != null)
            {
                extension.ClearSurface(false, false, false);
                extension = null;
            }
            mainStructure?.Annihilate(false, true, false);
            mainStructure = s;
            var t = s.transform;
            t.parent = myBlockExtension.myBlock.myChunk.transform;
            t.rotation = Quaternion.Euler(GetRotation());
            t.position = GetCenterPosition();
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
    public List<Plant> GetPlantsList()
    {
        if (extension == null) return null;
        else return extension.GetPlantsList();
    }

    public void ChangeMaterial(int newId, bool redrawCall)
    {
        if (materialID == newId) return;
        materialID = newId;
        if (materialID != myBlockExtension.myBlock.GetMaterialID()) dirty = true;
        if (redrawCall & visible) myChunk.RefreshBlockVisualising(myBlockExtension.myBlock, faceIndex);
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
                        meshRotation = (byte)Random.Range(0, 3);
                        dirty = true;
                        if (visible) myChunk.RefreshBlockVisualising(myBlockExtension.myBlock, faceIndex);
                    }
                }
                else
                {
                    if (meshType != MeshType.ExcavatedPlane075)
                    {
                        meshType = MeshType.ExcavatedPlane075;
                        meshRotation = (byte)Random.Range(0, 3);
                        dirty = true;
                        if (visible) myChunk.RefreshBlockVisualising(myBlockExtension.myBlock, faceIndex);
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
                        meshRotation = (byte)Random.Range(0, 3);
                        dirty = true;
                        if (visible) myChunk.RefreshBlockVisualising(myBlockExtension.myBlock, faceIndex);
                    }
                }
                else
                {
                    if (meshType != MeshType.ExcavatedPlane05)
                    {
                        meshType = MeshType.ExcavatedPlane075;
                        meshRotation = (byte)Random.Range(0, 3);
                        dirty = true;
                        if (visible) myChunk.RefreshBlockVisualising(myBlockExtension.myBlock, faceIndex);
                    }
                }
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
        ChangeMaterial(myBlockExtension.materialID, true);
    }

    public void EnvironmentalStrike(Vector3 hitpoint, byte radius, float damage)
    {
        if (mainStructure != null) mainStructure.ApplyDamage(damage);
        else
        {
            if (extension != null) extension.EnvironmentalStrike(hitpoint, radius, damage);
            else myBlockExtension.Dig((int)damage, true, faceIndex);
        }
    }
 
    public PlaneExtension GetExtension()
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
            else return (extension.fullfillStatus != FullfillStatus.Empty);
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

    public BlockpartVisualizeInfo GetVisualInfo(Chunk chunk, ChunkPos cpos)
    {
        if ( materialID == PoolMaster.NO_MATERIAL_ID | meshType == MeshType.NoMesh) return null;
        else
        {
            return new BlockpartVisualizeInfo(cpos,
                new MeshVisualizeInfo(faceIndex, PoolMaster.GetMaterialType(materialID), GetLightValue(chunk, cpos, faceIndex)),
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

    public ChunkPos GetChunkPosition() { return myBlockExtension.myBlock.pos; }
    public Vector3 GetCenterPosition()
    {
        Vector3 centerPos = myBlockExtension.myBlock.pos.ToWorldSpace();
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
                blockCenter += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.CEILING_FACE_INDEX:
                blockCenter += new Vector3(-0.5f, +0.5f, -0.5f) * q;
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
    public Vector3 GetRotation()
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

    public void Annihilate(bool compensateStructures)
    {
        if (extension != null) extension.Annihilate(compensateStructures);
        else mainStructure?.SectionDeleted(myBlockExtension.myBlock.pos);
        if (!GameMaster.sceneClearing) {
            if (haveWorksite)
            {
                GameMaster.realMaster.colonyController.RemoveWorksite(this);
                haveWorksite = false;
            }
            if (faceIndex == Block.UP_FACE_INDEX | faceIndex == Block.SURFACE_FACE_INDEX) myBlockExtension.myBlock.myChunk.needSurfacesUpdate = true;
        }
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
        //сохранить meshrotation, если это крыша, или если grassland
    }
    public void Load(System.IO.FileStream fs)
    {

    }
    #endregion
}