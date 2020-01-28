using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType : byte
{
    NoMesh, Quad, ExcavatedPlane025, ExcavatedPlane05, ExcavatedPlane075, CaveCeilSide, CutPlane, CutEdge012, CutEdge032,
    NaturalRooftop_0, NaturalRooftop_1, NaturalRooftop_2, NaturalRooftop_3, NaturalPeak_0,
}
//dependency: GetMesh, IsMeshTransparent, excavated meshes: Plane.VolumeChanges
public static class MeshMaster
{
    private static Mesh quadMesh, plane_excavated_025, plane_excavated_05, plane_excavated_075, cutPlane, cutEdge012, cutEdge032, caveCeil,
        natRoof_0, natRoof_1, natRoof_2, natRoof_3, natPeak_0;
		
    public static Mesh GetMesh(MeshType mtype)
    {
        switch (mtype)
        {
            default:
            case MeshType.Quad:
            if (quadMesh == null)
            {
                    quadMesh = new Mesh();
                    quadMesh.vertices = new Vector3[4] { new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0) };
                    quadMesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
                    quadMesh.normals = new Vector3[4] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                    quadMesh.uv = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
                }
            return quadMesh;
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
                if (natRoof_0 == null) natRoof_0 = Resources.Load<Mesh>("Meshes/Rooftops/naturalRooftop0");
                return natRoof_0;
            case MeshType.NaturalRooftop_1:
                if (natRoof_1 == null) natRoof_1 = Resources.Load<Mesh>("Meshes/Rooftops/naturalRooftop1");
                return natRoof_1;
            case MeshType.NaturalRooftop_2:
                if (natRoof_2 == null) natRoof_0 = Resources.Load<Mesh>("Meshes/Rooftops/naturalRooftop2");
                return natRoof_2;
            case MeshType.NaturalRooftop_3:
                if (natRoof_3 == null) natRoof_3 = Resources.Load<Mesh>("Meshes/Rooftops/naturalRooftop3");
                return natRoof_3;
            case MeshType.NaturalPeak_0:
                if (natPeak_0 == null) natPeak_0 = Resources.Load<Mesh>("Meshes/Rooftops/naturalPeak0");
                return natPeak_0;
        }
    }
    public static Mesh GetMesh(MeshType mtype, int materialID)
    {
        Mesh m = Object.Instantiate(GetMesh(mtype));
        SetMeshUVs(ref m, materialID);
        return m;
    }
    public static void SetMeshUVs(ref Mesh m, int materialID)
    {
        Vector2[] borders;
        float piece = 0.25f, add = ((Random.value > 0.5) ? piece : 0);
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
            default: borders = new Vector2[] { Vector2.zero, Vector2.one, Vector2.right, Vector2.up }; break;
        }

        borders = new Vector2[4] { borders[0], borders[2], borders[1], borders[3] };

        // крутим развертку, если это квад, иначе просто перетаскиваем 
        bool isQuad = (m.uv.Length == 4);
        Vector2[] uvEdited = m.uv;
        if (isQuad)
        {
            borders[0].x += 0.01f; // (0,0)
            borders[0].y += 0.01f;

            borders[1].x -= 0.01f; //(0,1)
            borders[1].y -= 0.01f;

            borders[2].x += 0.01f; // (1,1)
            borders[2].y -= 0.01f;

            borders[3].x -= 0.01f; // (1,0)
            borders[3].y += 0.01f;
            bool useTextureRotation = true;
            if (useTextureRotation)
            {
                float seed = Random.value;
                if (seed > 0.5f)
                {
                    if (seed > 0.75f) uvEdited = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
                    else uvEdited = new Vector2[] { borders[2], borders[3], borders[1], borders[0] };
                }
                else
                {
                    if (seed > 0.25f) uvEdited = new Vector2[] { borders[3], borders[1], borders[0], borders[2] };
                    else uvEdited = new Vector2[] { borders[1], borders[0], borders[2], borders[3] };
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

    public static Plane GetRooftop(BlockExtension b, bool peak, bool artificial)
    {
        if (artificial) return GetRooftop(b, peak, artificial, 0);
        else
        {
            if (peak)
            {
                float f = Random.value;
                byte n = 0;
                if (f >= 0.77f) n = 2;
                else if (f <= 0.33f) n = 0; else n = 1;
                return GetRooftop(b,true, false, n);
            }
            else { if (Random.value > 0.5f) return GetRooftop(b,false, false, 0); else return GetRooftop(b,false, false, 1); }
        }
    }
    public static Plane GetRooftop(BlockExtension b, bool peak, bool artificial, byte number)
    {
        MeshType mtype = MeshType.NaturalRooftop_0;
        int materialID = PoolMaster.MATERIAL_COMBINED_BASIC_ID;
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
        }
        else
        {

        }
        return new Plane(b, mtype, materialID, Block.UP_FACE_INDEX);
    }
    public static GameObject GetFlyingPlatform()
    {
        return Object.Instantiate(Resources.Load<GameObject>("Prefs/flyingPlatform_small"));
    }
}
