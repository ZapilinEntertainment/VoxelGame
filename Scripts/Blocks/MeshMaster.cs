﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType : byte
{
    NoMesh, Quad, ExcavatedPlane025, ExcavatedPlane05, ExcavatedPlane075, CaveCeilSide, CutPlane, CutEdge012, CutEdge032,
    NaturalRooftop_0, NaturalRooftop_1, NaturalRooftop_2, NaturalRooftop_3, NaturalPeak_0, ArtificialRooftop_0, ArtificialRooftop_1, ArtificialRooftop_2,
    ArtificialPeak_0, ArtificialPeak_1, StorageEntrance, StorageSide, FarmFace, FarmSide, IndustryHeater0, IndustryHeater1, FoundationSide, BigWindow, Housing_0, Housing_1, Housing_2,
    SimpleHeater_0, SmallWindows, DoubleWindows, LumbermillFace, LumbermillSide, ReactorSide_0, ReactorSide_1, SmelterySide_0, SmelterySide_1
}
//dependency: GetMesh, IsMeshTransparent, excavated meshes: Plane.VolumeChanges
public static class MeshMaster
{
    private static Mesh quadMesh, plane_excavated_025, plane_excavated_05, plane_excavated_075, cutPlane, cutEdge012, cutEdge032, caveCeil,
        natRoof_0, natRoof_1, natRoof_2, natRoof_3, natPeak_0, artRoof_0, foundationSide;
    private static GameObject storageEntrancePref, storageSidePref;
		
    public static Mesh GetMeshSourceLink(MeshType mtype)
    {
        switch (mtype)
        {            
            case MeshType.ExcavatedPlane025:
                if (plane_excavated_025 == null) plane_excavated_025 = Resources.Load<Mesh>("Meshes/Plane_excavated_025");
                return plane_excavated_025;
            case MeshType.ExcavatedPlane05:
                if (plane_excavated_05 == null) plane_excavated_05 = Resources.Load<Mesh>("Meshes/Plane_excavated_05");
                return plane_excavated_05;
            case MeshType.ExcavatedPlane075:
                if (plane_excavated_075 == null) plane_excavated_075 = Resources.Load<Mesh>("Meshes/Plane_excavated_075");
                return plane_excavated_075;
            case MeshType.CutPlane:
                {
                    if (cutPlane == null)
                    {
                        cutPlane = new Mesh();
                        cutPlane.vertices = new Vector3[4] { new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f) };
                        cutPlane.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
                        cutPlane.uv = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
                    }
                    return cutPlane;
                }
            case MeshType.CutEdge012:
                {
                    if (cutEdge012 == null)
                    {
                        cutEdge012 = new Mesh();
                        cutEdge012.vertices = new Vector3[3] { new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, -0.5f, 0) };
                        cutEdge012.triangles = new int[3] { 0, 1, 2 };
                        cutEdge012.uv = new Vector2[3] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 1f) };
                    }
                    return cutEdge012;
                }
            case MeshType.CutEdge032:
                {
                    if (cutEdge032 == null)
                    {
                        cutEdge032 = new Mesh();
                        cutEdge032.vertices = new Vector3[3] { new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(-0.5f, -0.5f, 0) };
                        cutEdge032.triangles = new int[3] { 0, 1, 2 };
                        cutEdge032.uv = new Vector2[3] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 1f) };
                    }
                    return cutEdge032;
                }
            case MeshType.CaveCeilSide:
                {
                    if (caveCeil == null)
                    {
                        caveCeil = new Mesh();
                        caveCeil.vertices = new Vector3[4] { new Vector3(0.5f, 0.4f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.4f, 0), new Vector3(-0.5f, 0.5f, 0) };
                        caveCeil.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
                        caveCeil.uv = new Vector2[4] { new Vector2(0, 0.9f), new Vector2(0, 1), new Vector2(1, 0.9f), new Vector2(1, 1) };
                    }
                    return caveCeil;
                }
            case MeshType.NaturalRooftop_0:
                if (natRoof_0 == null) natRoof_0 = Resources.Load<Mesh>("Meshes/Rooftops/natural_rooftop0");
                return natRoof_0;
            case MeshType.NaturalRooftop_1:
                if (natRoof_1 == null) natRoof_1 = Resources.Load<Mesh>("Meshes/Rooftops/natural_rooftop1");
                return natRoof_1;
            case MeshType.NaturalRooftop_2:
                if (natRoof_2 == null) natRoof_2 = Resources.Load<Mesh>("Meshes/Rooftops/natural_rooftop2");
                return natRoof_2;
            case MeshType.NaturalRooftop_3:
                if (natRoof_3 == null) natRoof_3 = Resources.Load<Mesh>("Meshes/Rooftops/natural_rooftop3");
                return natRoof_3;
            case MeshType.NaturalPeak_0:
                if (natPeak_0 == null) natPeak_0 = Resources.Load<Mesh>("Meshes/Rooftops/natural_peak0");
                return natPeak_0;
            case MeshType.ArtificialRooftop_0:
                if (artRoof_0 == null) artRoof_0 = Resources.Load<Mesh>("Meshes/Rooftops/artificial_rooftop0");
                return artRoof_0;
            case MeshType.FoundationSide:
                if (foundationSide == null) foundationSide = Resources.Load<Mesh>("Meshes/foundationBlock_side");
                return foundationSide;
            case MeshType.Quad:
            default:
                if (quadMesh == null)
                {
                    quadMesh = new Mesh();
                    quadMesh.vertices = new Vector3[4] { new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0) };
                    quadMesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
                    quadMesh.normals = new Vector3[4] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                    quadMesh.uv = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
                }
                return quadMesh;
        }
    }
    public static Mesh GetMesh(MeshType mtype, int materialID)
    {
        Mesh m = Object.Instantiate(GetMeshSourceLink(mtype));
        if (materialID != PoolMaster.FIXED_UV_BASIC) SetMeshUVs(ref m, materialID);
        return m;
    }
    public static Mesh GetMeshColliderLink(MeshType mtype)
    {
        switch (mtype)
        {
            case MeshType.NoMesh: return null;
            case MeshType.CutPlane: return GetMeshSourceLink(MeshType.CutPlane);
            case MeshType.CutEdge012: return GetMeshSourceLink(MeshType.CutEdge012);
            case MeshType.CutEdge032: return GetMeshSourceLink(MeshType.CutEdge032);
            case MeshType.CaveCeilSide: return GetMeshSourceLink(MeshType.CaveCeilSide);
            default: return GetMeshSourceLink(MeshType.Quad);
        }
    }
    public static GameObject InstantiateAdvancedMesh(MeshType mtype)
    {
        switch (mtype)
        {
            case MeshType.StorageEntrance:
                if (storageEntrancePref == null) storageEntrancePref = Resources.Load<GameObject>("Prefs/Blockparts/storageEntrance");
                return Object.Instantiate(storageEntrancePref);
            case MeshType.StorageSide:
                if (storageSidePref == null) storageSidePref = Resources.Load<GameObject>("Prefs/Blockparts/storageSide");
                return Object.Instantiate(storageSidePref);
            case MeshType.ArtificialPeak_0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Rooftops/artificialPeak_0"));
            case MeshType.ArtificialPeak_1:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Rooftops/artificialPeak_0"));
            case MeshType.ArtificialRooftop_0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Rooftops/artificialRooftop_0"));
            case MeshType.ArtificialRooftop_1:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Rooftops/artificialRooftop_1"));
            case MeshType.ArtificialRooftop_2:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Rooftops/artificialRooftop_2"));
            case MeshType.FarmFace:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/farmFace"));
            case MeshType.FarmSide:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/farmSide"));
            case MeshType.IndustryHeater0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/industryHeater0"));
            case MeshType.IndustryHeater1:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/industryHeater1"));
            case MeshType.BigWindow:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/bigWindow"));
            case MeshType.Housing_0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/housing0"));
            case MeshType.Housing_1:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/housing1"));
            case MeshType.Housing_2:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/housing2"));
            case MeshType.SimpleHeater_0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/simpleHeater0"));
            case MeshType.SmallWindows:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/smallWindows"));
            case MeshType.DoubleWindows:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/doubleWindows"));
            case MeshType.LumbermillFace:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/lumbermillFace"));
            case MeshType.LumbermillSide:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/lumbermillSide"));
            case MeshType.ReactorSide_0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/reactorSide0"));
            case MeshType.ReactorSide_1:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/reactorSide1"));
            case MeshType.SmelterySide_0:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/smelterySide0"));
            case MeshType.SmelterySide_1:
                return Object.Instantiate(Resources.Load<GameObject>("Prefs/Blockparts/smelterySide1"));
            default: return null;
        }
    }

    public static void SetMeshUVs(ref Mesh m, int materialID)
    {
        Vector2[] borders;
        float piece = 0.25f, add = 0f;// ((Random.value > 0.5) ? piece : 0);
        switch (materialID)
        {
            case ResourceType.STONE_ID:
                borders = new Vector2[] { new Vector2(3 * piece, 2 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(4 * piece, 3 * piece), new Vector2(4 * piece, 2 * piece) };
                break;
            case ResourceType.DIRT_ID:
                borders = new Vector2[] { new Vector2(piece, 2 * piece), new Vector2(piece, 3 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 2 * piece) };
                break;
            case ResourceType.LUMBER_ID:
                borders = new Vector2[] { new Vector2(0, 2 * piece), new Vector2(0, 3 * piece), new Vector2(piece, 3 * piece), new Vector2(piece, 2 * piece) };
                break;
            case ResourceType.METAL_K_ORE_ID:
            case ResourceType.METAL_K_ID:
                borders = new Vector2[] { new Vector2(0, 3 * piece), new Vector2(0, 4 * piece), new Vector2(piece, 4 * piece), new Vector2(piece, 3 * piece) };
                break;
            case ResourceType.METAL_M_ORE_ID:
            case ResourceType.METAL_M_ID:
                borders = new Vector2[] { new Vector2(piece, 3 * piece), new Vector2(piece, 4 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(2 * piece, 3 * piece) };
                break;
            case ResourceType.METAL_E_ORE_ID:
            case ResourceType.METAL_E_ID:
                borders = new Vector2[] { new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(3 * piece, 3 * piece) };
                break;
            case ResourceType.METAL_N_ORE_ID:
            case ResourceType.METAL_N_ID:
                borders = new Vector2[] { new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(4 * piece, 4 * piece), new Vector2(4 * piece, 3 * piece) };
                break;
            case ResourceType.METAL_P_ORE_ID:
            case ResourceType.METAL_P_ID:
                borders = new Vector2[] { new Vector2(0, 2 * piece), new Vector2(0, 3 * piece), new Vector2(piece, 3 * piece), new Vector2(piece, 2 * piece) };
                break;
            case ResourceType.METAL_S_ORE_ID:
            case ResourceType.METAL_S_ID:
                borders = new Vector2[] { new Vector2(piece, 2 * piece), new Vector2(piece, 3 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 2 * piece) };
                break;
            case ResourceType.MINERAL_F_ID:
                borders = new Vector2[] { new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(4 * piece, 4 * piece), new Vector2(4 * piece, 3 * piece) };
                break;
            case ResourceType.MINERAL_L_ID:
                borders = new Vector2[] { new Vector2(0, piece), new Vector2(0, 2 * piece), new Vector2(piece, 2 * piece), new Vector2(piece, piece) };
                break;
            case ResourceType.PLASTICS_ID:
                borders = new Vector2[] { new Vector2(piece, 3 * piece), new Vector2(piece, 4 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(2 * piece, 3 * piece) };
                break;
            case ResourceType.CONCRETE_ID:
                borders = new Vector2[] { new Vector2(0, 3 * piece), new Vector2(0, 4 * piece), new Vector2(piece, 4 * piece), new Vector2(piece, 3 * piece) };
                break;
            case ResourceType.SNOW_ID:
                borders = new Vector2[] { new Vector2(piece, piece), new Vector2(piece, 2 * piece), new Vector2(2 * piece, 2 * piece), new Vector2(2 * piece, piece) };
                break;
            case ResourceType.FERTILE_SOIL_ID:
                borders = new Vector2[] { new Vector2(2 * piece, 2 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 2 * piece) };
                break;
            case PoolMaster.MATERIAL_ADVANCED_COVERING_ID:
                borders = new Vector2[] { new Vector2(3 * piece, piece), new Vector2(3 * piece, 2 * piece), new Vector2(4 * piece, 2 * piece), new Vector2(4 * piece, piece) };
                break;
            case PoolMaster.MATERIAL_GRASS_100_ID:
                borders = new Vector2[] { new Vector2(piece, 0), new Vector2(piece, piece), new Vector2(2 * piece, piece), new Vector2(2 * piece, 0) };
                break;
            case PoolMaster.MATERIAL_GRASS_80_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, 0), new Vector2(2 * piece + add, piece), new Vector2(3 * piece + add, piece), new Vector2(3 * piece + add, 0) };
                break;
            case PoolMaster.MATERIAL_GRASS_60_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, piece), new Vector2(2 * piece + add, 2 * piece), new Vector2(3 * piece + add, 2 * piece), new Vector2(3 * piece + add, piece) };
                break;
            case PoolMaster.MATERIAL_GRASS_40_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, 2 * piece), new Vector2(2 * piece + add, 3 * piece), new Vector2(3 * piece + add, 3 * piece), new Vector2(3 * piece + add, 2 * piece) };
                break;
            case PoolMaster.MATERIAL_GRASS_20_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, 3 * piece), new Vector2(2 * piece + add, 4 * piece), new Vector2(3 * piece + add, 4 * piece), new Vector2(3 * piece + add, 3 * piece) };
                break;
            case PoolMaster.MATERIAL_LEAVES_ID:
                borders = new Vector2[] { Vector2.zero, Vector2.up * piece, Vector2.one * piece, Vector2.right * piece };
                break;
            case PoolMaster.MATERIAL_WHITE_METAL_ID:
                borders = new Vector2[] { new Vector2(2 * piece, 2 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 2 * piece) };
                break;
            case PoolMaster.MATERIAL_DEAD_LUMBER_ID:
                borders = new Vector2[] { new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(3 * piece, 3 * piece) };
                break;
            case PoolMaster.MATERIAL_WHITEWALL_ID:
                borders = new Vector2[] { new Vector2(2 * piece, piece), new Vector2(2 * piece, 2 * piece), new Vector2(3 * piece, 2 * piece), new Vector2(3 * piece, piece) };
                break;
            case PoolMaster.CUTTED_LAYER_TEXTURE:
                borders = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, piece), new Vector2(piece, piece), new Vector2(piece, 0f) };
                break;
            default: borders = new Vector2[] { Vector2.zero, Vector2.one, Vector2.right, Vector2.up }; break;
        }

        borders = new Vector2[4] { borders[0], borders[2], borders[1], borders[3] };

        // крутим развертку, если это квад, иначе просто перетаскиваем 
        bool isQuad = (m.uv.Length == 4);
        Vector2[] uvEdited = m.uv;
        if (isQuad)
        {
            var f = 0.00390625f;
            borders[0].x += f; // (0,0)
            borders[0].y += f;

            borders[1].x -= f; //(0,1)
            borders[1].y -= f;

            borders[2].x += f; // (1,1)
            borders[2].y -= f;

            borders[3].x -= f; // (1,0)
            borders[3].y += f;

            bool useTextureRotation = false; // иначе вращаются каждый раз при перерисовке
            if (useTextureRotation)
            {
                byte uvRotation = (byte)(Random.value / 0.25f);
                switch (uvRotation)
                {                    
                    case 1: uvEdited = new Vector2[] { borders[1], borders[3], borders[2], borders[0] }; break;
                    case 2: uvEdited = new Vector2[] { borders[2], borders[0], borders[1], borders[3] }; break;
                    case 3: uvEdited = new Vector2[] { borders[3], borders[1], borders[0], borders[2] }; break;
                    default: uvEdited = new Vector2[] { borders[0], borders[2], borders[3], borders[1] }; break;
                }
            }
            else
            {
                // Vector2[] uvs = new Vector2[] { new Vector2(0.0f,0.0f), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1)};
                uvEdited = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
            }
        }
        else
        {
            float minY = 1, maxY = 0, minX = 1, maxX = 0;
            foreach (var v in uvEdited)
            {
                if (v.x > maxX) maxX = v.x;
                if (v.x < minX) minX = v.x;
                if (v.y > maxY) maxY = v.y;
                if (v.y < minY) minY = v.y;
            }
            float xl = maxX - minX, yl = maxY - minY, k = 1;
            if (xl > yl) k = piece / xl;
            else k = piece / yl;
            float x0 = borders[0].x, y0 = borders[0].y;
            for (int i = 0; i < uvEdited.Length; i++)
            {
                uvEdited[i] = new Vector2(x0 + (uvEdited[i].x - minX) * k, y0 + (uvEdited[i].y - minY) * k);
            }
        }
        m.uv = uvEdited;
    }

    public static bool IsMeshTransparent(MeshType mt)
    {
        switch (mt)
        {
            case MeshType.Quad: 
            case MeshType.ExcavatedPlane025:
            case MeshType.ExcavatedPlane05:
            case MeshType.ExcavatedPlane075:
            case MeshType.NaturalPeak_0:
            case MeshType.NaturalRooftop_0:
            case MeshType.NaturalRooftop_1:
            case MeshType.NaturalRooftop_2:
            case MeshType.NaturalRooftop_3:
                return false;
            case MeshType.CaveCeilSide:
            case MeshType.CutPlane:
            case MeshType.CutEdge012:
            case MeshType.CutEdge032:
                return true;
            default: return true;
        }
    }

    public static Plane GetRooftop(IPlanable b, bool peak, bool artificial)
    {
        byte number = 0;
        if (!artificial)
        {
            if (!peak) number = (byte)Random.Range(0, 3);
        }
        else
        {
            if (!peak) number = Random.value > 0.75f ? (byte)0 : (byte)1;
            else number = (byte)Random.Range(0, 3);
        }
        return GetRooftop(b, peak, artificial, number);
    }
    public static Plane GetRooftop(IPlanable b, bool peak, bool artificial, byte number)
    {
        MeshType mtype = MeshType.NaturalRooftop_0;
        int materialID = PoolMaster.FIXED_UV_BASIC;
        if (!artificial) {
            if (peak) mtype = MeshType.NaturalPeak_0;
            else
            {
                switch (number)
                {
                    case 3: mtype = MeshType.NaturalRooftop_3; break;
                    case 2: mtype = MeshType.NaturalRooftop_2; break;
                    case 1: mtype = MeshType.NaturalRooftop_1; break;
                }
            }
            return new Plane(b, mtype, materialID, Block.UP_FACE_INDEX, (byte)Random.Range(0, 3));
        }
        else
        {
            if (peak)
            {
                if (number == 0) mtype = MeshType.ArtificialPeak_0; else mtype = MeshType.ArtificialPeak_1;
            }
            else
            {
                switch (number)
                {
                    case 2: mtype = MeshType.ArtificialRooftop_2; break;
                    case 1: mtype = MeshType.ArtificialRooftop_1; break;
                    default: mtype = MeshType.ArtificialRooftop_0; break;

                }
            }
            return new MultimaterialPlane(b, mtype, Block.UP_FACE_INDEX, (byte)Random.Range(0, 3));
        }
        
    }
    public static GameObject GetFlyingPlatform()
    {
        return Object.Instantiate(Resources.Load<GameObject>("Prefs/flyingPlatform_small"));
    }
}
