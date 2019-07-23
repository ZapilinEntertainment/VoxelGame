using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaterialType : byte { Basic, Metal, Energy, Green }
public enum MeshType: byte { NoMesh, Quad, ExcavatedPlane025, ExcavatedPlane05, ExcavatedPlane075, CaveCeil, CutPlane, CutEdge012, CutEdge032}

public sealed class PoolMaster : MonoBehaviour {
    private struct LightPoolInfo
    {
        public MaterialType mtype;
        public byte illumination; 
        public LightPoolInfo (MaterialType i_mtype, byte i_light)
        {
            mtype = i_mtype;
            illumination = i_light;
        }
    }

    public static bool useAdvancedMaterials { get; private set; }
    public static bool useIlluminationSystem { get; private set; }
    public static bool shadowCasting { get; private set; }
    public static int qualityLevel { get; private set; }
    public static PoolMaster current;	 
	public static GameObject mineElevator_pref {get;private set;}    
    public static Material energyMaterial { get; private set; }
    public static Material energyMaterial_disabled{ get; private set; }
    public static Material glassMaterial { get; private set; }
    public static Material glassMaterial_disabled { get; private set; }
    public static Material darkness_material{ get; private set; }
    public static Material billboardMaterial { get; private set; }
    public static Material billboardShadedMaterial { get; private set; }
    public static Material verticalBillboardMaterial { get; private set; }
    public static Material verticalWavingBillboardMaterial { get; private set; }
    public static Material starsBillboardMaterial { get; private set; }    
	public static GUIStyle GUIStyle_RightOrientedLabel, GUIStyle_BorderlessButton, GUIStyle_BorderlessLabel, GUIStyle_CenterOrientedLabel, GUIStyle_SystemAlert,
	GUIStyle_RightBottomLabel, GUIStyle_COLabel_red, GUIStyle_Button_red;
    public static Sprite gui_overridingSprite;
    public static readonly Color gameOrangeColor = new Color(0.933f, 0.5686f, 0.27f);

    private static Transform zoneCube;
    private static bool useTextureRotation = false;
    private static Dictionary<LightPoolInfo, Material> lightPoolMaterials;
    private static Material metal_material, green_material, default_material, lr_red_material, lr_green_material, basic_material;
    private static Mesh quadMesh, plane_excavated_025, plane_excavated_05, plane_excavated_075, cutPlane, cutEdge012, cutEdge032, caveCeil;   

    private List<Ship> inactiveShips;	
    private float shipsClearTimer = 0, clearTime = 30;
    private ParticleSystem buildEmitter, citizensLeavingEmitter, lifepowerEmitter;
    private static Sprite[] starsSprites;

    public static byte MAX_MATERIAL_LIGHT_DIVISIONS { get; private set; }
    public const int NO_MATERIAL_ID = -1, MATERIAL_ADVANCED_COVERING_ID = -2, MATERIAL_GRASS_100_ID = -3, MATERIAL_GRASS_80_ID = -4, MATERIAL_GRASS_60_ID = -5,
        MATERIAL_GRASS_40_ID = -6, MATERIAL_GRASS_20_ID = -7, MATERIAL_LEAVES_ID = -8, MATERIAL_WHITE_METAL_ID = -9, MATERIAL_DEAD_LUMBER_ID = -10,
        MATERIAL_WHITEWALL_ID = -11;
    // зависимость - ResourceType.GetResourceByID
    private const int SHIPS_BUFFER_SIZE = 5, MAX_QUALITY_LEVEL = 2;

    public void Load() {
		if (current != null) return;
		current = this;

        qualityLevel = QualitySettings.GetQualityLevel();
        useAdvancedMaterials = (qualityLevel == MAX_QUALITY_LEVEL);
        shadowCasting = useAdvancedMaterials;
        useIlluminationSystem = !shadowCasting;

        if (qualityLevel != 0) // dependency : change quality level()
        {
            buildEmitter = Instantiate(Resources.Load<ParticleSystem>("buildEmitter"));
            lifepowerEmitter = Instantiate(Resources.Load<ParticleSystem>("lifepowerEmitter"));
        }

        inactiveShips = new List<Ship>();

        quadMesh = new Mesh();
        quadMesh.vertices = new Vector3[4] { new Vector3 (0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0) };
        quadMesh.triangles = new int[6] {0,1,2, 1,3,2 };
        quadMesh.normals = new Vector3[4] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        quadMesh.uv = new Vector2[4] { new Vector2(0,0) , new Vector2(0,1), new Vector2(1,0), new Vector2(1,1) };

        plane_excavated_025 = Resources.Load<Mesh>("Meshes/Plane_excavated_025");
		plane_excavated_05 = Resources.Load<Mesh>("Meshes/Plane_excavated_05");
		plane_excavated_075 = Resources.Load<Mesh>("Meshes/Plane_excavated_075");

		lr_red_material = Resources.Load<Material>("Materials/GUI_Red");
		lr_green_material = Resources.Load<Material>("Materials/GUI_Green");

        zoneCube = Instantiate(Resources.Load<Transform>("Prefs/zoneCube"), transform);zoneCube.gameObject.SetActive(false);

        default_material = Resources.Load<Material>("Materials/Default");
        darkness_material = Resources.Load<Material>("Materials/Darkness");		
        energyMaterial_disabled = Resources.Load<Material>("Materials/UnchargedMaterial");       
        verticalBillboardMaterial = Resources.Load<Material>("Materials/VerticalBillboard");
        verticalWavingBillboardMaterial = Resources.Load<Material>("Materials/VerticalWavingBillboard");

        billboardShadedMaterial = Resources.Load<Material>("Materials/Advanced/ShadedBillboard");
        billboardMaterial = Resources.Load<Material>("Materials/BillboardMaterial");           
        starsBillboardMaterial = Resources.Load<Material>("Materials/StarsBillboardMaterial");        

        mineElevator_pref = Resources.Load<GameObject>("Structures/MineElevator");
        gui_overridingSprite = Resources.Load<Sprite>("Textures/gui_overridingSprite");
        starsSprites = Resources.LoadAll<Sprite>("Textures/stars");

        GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
        if (qualityLevel == 2)
        {
            useAdvancedMaterials = true;
            shadowCasting = true;
            glassMaterial_disabled = Resources.Load<Material>("Materials/Advanced/GlassOffline_PBR");
            basic_material = Resources.Load<Material>("Materials/Advanced/Basic_PBR");
            glassMaterial = Resources.Load<Material>("Materials/Advanced/Glass_PBR");
            metal_material = Resources.Load<Material>("Materials/Advanced/Metal_PBR");
            green_material = Resources.Load<Material>("Materials/Advanced/Green_PBR");
            energyMaterial = Resources.Load<Material>("Materials/Advanced/ChargedMaterial_advanced");
        }
        else
        {
            if (qualityLevel == 2)
            {
                useAdvancedMaterials = false;
                shadowCasting = false;
                glassMaterial_disabled = Resources.Load<Material>("Materials/GlassOffline");
                basic_material = Resources.Load<Material>("Materials/Basic");
                glassMaterial = Resources.Load<Material>("Materials/Glass");
                metal_material = Resources.Load<Material>("Materials/Metal");
                green_material = Resources.Load<Material>("Materials/Green");
                energyMaterial = Resources.Load<Material>("Materials/ChargedMaterial");
            }
        }
        if (GameMaster.realMaster.mainChunk != null) GameMaster.realMaster.mainChunk.SetShadowCastingMode(shadowCasting);
        if (useIlluminationSystem)
        {
            lightPoolMaterials = new Dictionary<LightPoolInfo, Material>();
            MAX_MATERIAL_LIGHT_DIVISIONS = QualitySettings.GetQualityLevel() == 0 ? (byte)8 : (byte)16;
        }

        
        var rrs = Component.FindObjectsOfType<Renderer>();
        if (rrs != null && rrs.Length != 0) ReplaceMaterials(rrs, useAdvancedMaterials);

        //testzone
        //GameObject g = new GameObject("quad");
        //var mf = g.AddComponent<MeshFilter>();
        //mf.mesh= quadMesh;
        //var mr = g.AddComponent<MeshRenderer>();
        //mr.sharedMaterial = basic_material;
        //SetMaterialByID(ref mf, ref mr, MATERIAL_GRASS_20_ID, 255);
        //g.transform.position += Vector3.up * 2;
    }

    public static void ChangeQualityLevel (int newLevel)
    {
        if (qualityLevel != newLevel)
        {
            if (current != null)
            {
                if (qualityLevel == 0)
                {
                    current.buildEmitter = Instantiate(Resources.Load<ParticleSystem>("buildEmitter"));
                    current.lifepowerEmitter = Instantiate(Resources.Load<ParticleSystem>("lifepowerEmitter"));
                }
                else
                {
                    if (qualityLevel != 0 & newLevel == 0)
                    {
                        Destroy(current.buildEmitter);
                        Destroy(current.lifepowerEmitter);
                    }
                }                
            }
            if (newLevel == 2)
            {
                useAdvancedMaterials = true;
                shadowCasting = true;
                glassMaterial_disabled = Resources.Load<Material>("Materials/Advanced/GlassOffline_PBR");
                basic_material = Resources.Load<Material>("Materials/Advanced/Basic_PBR");
                glassMaterial = Resources.Load<Material>("Materials/Advanced/Glass_PBR");
                metal_material = Resources.Load<Material>("Materials/Advanced/Metal_PBR");
                green_material = Resources.Load<Material>("Materials/Advanced/Green_PBR");
                energyMaterial = Resources.Load<Material>("Materials/Advanced/ChargedMaterial_advanced");
            }
            else
            {
                if (qualityLevel == 2)
                {
                    useAdvancedMaterials = false;
                    shadowCasting = false;
                    glassMaterial_disabled = Resources.Load<Material>("Materials/GlassOffline");
                    basic_material = Resources.Load<Material>("Materials/Basic");
                    glassMaterial = Resources.Load<Material>("Materials/Glass");
                    metal_material = Resources.Load<Material>("Materials/Metal");
                    green_material = Resources.Load<Material>("Materials/Green");
                    energyMaterial = Resources.Load<Material>("Materials/ChargedMaterial");
                }
            }
            if (GameMaster.realMaster.mainChunk != null) GameMaster.realMaster.mainChunk.SetShadowCastingMode(shadowCasting);
            qualityLevel = newLevel;
            var rrs = Component.FindObjectsOfType<Renderer>();
            ReplaceMaterials(rrs, useAdvancedMaterials);
        }
    }

	void LabourUpdate() {
        if (GameMaster.realMaster.gameMode == GameMode.Editor ) return;
        ColonyController colony = GameMaster.realMaster.colonyController;
        if (colony == null) return;
		int docksCount = colony.docks.Count;
		if (shipsClearTimer > 0) {
			shipsClearTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (shipsClearTimer <= 0) {
				if (inactiveShips.Count > SHIPS_BUFFER_SIZE)
                {
                    Destroy(inactiveShips[0].gameObject);
                    inactiveShips.RemoveAt(0);
                }
				shipsClearTimer = clearTime;
			}
		}       
	}

    public static Mesh GetMesh(MeshType mtype)
    {
        switch (mtype)
        {
            default:
            case MeshType.Quad: return quadMesh;
            case MeshType.ExcavatedPlane025: return plane_excavated_025;
            case MeshType.ExcavatedPlane05: return plane_excavated_05;
            case MeshType.ExcavatedPlane075: return plane_excavated_075;
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
                        cutEdge012.vertices = new Vector3[3] { new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, -0.5f, 0)};
                        cutEdge012.triangles = new int[3] { 0, 1, 2};
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
            case MeshType.CaveCeil:
                {
                    if (caveCeil == null)
                    {
                        caveCeil= new Mesh();
                        caveCeil.vertices = new Vector3[4] { new Vector3(0.5f, 0.4f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.4f, 0), new Vector3(-0.5f, 0.5f, 0) };
                        caveCeil.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };                      
                        caveCeil.uv = new Vector2[4] { new Vector2(0, 0.9f), new Vector2(0, 1), new Vector2(1, 0.9f), new Vector2(1, 1) };
                    }
                    return caveCeil;
                }
        }
    }
    public static Mesh GetMesh(MeshType mtype, int materialID)
    {
        Mesh m = Instantiate(GetMesh(mtype));
        SetMeshUVs(ref m, materialID);
        return m;
    }

    public static Sprite GetStarSprite(bool random)
    {
        if (random) return (starsSprites[Random.Range(0, 14)]);
        else return starsSprites[15];
    }

    #region effects
    public void CitizenLeaveEffect(Vector3 pos)
    {
        if (citizensLeavingEmitter == null) citizensLeavingEmitter = Instantiate(Resources.Load<ParticleSystem>("Prefs/citizensLeavingEmitter")) as ParticleSystem;
        citizensLeavingEmitter.transform.position = pos;
        citizensLeavingEmitter.Emit(1);
    }
    public void BuildSplash(Vector3 pos)
    {
        if (qualityLevel == 0) return;
        buildEmitter.transform.position = pos;
        buildEmitter.Emit(12);
    }
    public void LifepowerSplash(Vector3 pos, int count)
    {
        if (qualityLevel == 0) return;
        lifepowerEmitter.transform.position = pos;
        lifepowerEmitter.Emit(count);
    }
    public void DrawZone(Vector3 point, Vector3 scale, Color col)
    {
        zoneCube.position = point;
        zoneCube.localScale = scale;
        zoneCube.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_MainColor", col);
        zoneCube.gameObject.SetActive(true);
    }
    public void DisableFlightZone() { zoneCube.gameObject.SetActive(false); }
    #endregion

    public Ship GetShip(byte level, ShipType type) {
		Ship s = null;
        bool found = false;
		switch (type) {
		case ShipType.Passenger:
                {
                    if (inactiveShips.Count > 0)
                    {
                        for (int i = 0; i < inactiveShips.Count; i++)
                        {
                            if (inactiveShips[i].type == ShipType.Passenger)
                            {
                                s = inactiveShips[i];
                                inactiveShips.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        s = Instantiate(Resources.Load<GameObject>("Prefs/passengerShip_1")).GetComponent<Ship>();
                        if (useAdvancedMaterials) ReplaceMaterials(s.gameObject, true);
                    }
                    break;
                }
		case ShipType.Cargo:
                {
                    if (inactiveShips.Count > 0)
                    {
                        for (int i = 0; i < inactiveShips.Count; i++)
                        {
                            if (inactiveShips[i].type == ShipType.Cargo & inactiveShips[i].level == level)
                            {
                                s = inactiveShips[i];
                                inactiveShips.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        switch (level) {
                            case 2: s = Instantiate(Resources.Load<GameObject>("Prefs/mediumCargoShip")).GetComponent<Ship>(); break;
                            case 3: s = Instantiate(Resources.Load<GameObject>("Prefs/heavyCargoShip")).GetComponent<Ship>(); break;
                            default:  s = Instantiate(Resources.Load<GameObject>("Prefs/lightCargoShip")).GetComponent<Ship>();
                                break;
                        }
                        if (useAdvancedMaterials) ReplaceMaterials(s.gameObject, true);
                    }
                    break;
                }
		case ShipType.Private:
                {
                    if (inactiveShips.Count > 0)
                    {
                        for (int i = 0; i < inactiveShips.Count; i++)
                        {
                            if (inactiveShips[i].type == ShipType.Private)
                            {
                                s = inactiveShips[i];
                                inactiveShips.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        s = Instantiate(Resources.Load<GameObject>("Prefs/privateShip")).GetComponent<Ship>();
                        if (useAdvancedMaterials) ReplaceMaterials(s.gameObject, true);
                    }
                    break;
                }
            case ShipType.Military:
                {
                    if (inactiveShips.Count > 0)
                    {
                        for (int i = 0; i < inactiveShips.Count; i++)
                        {
                            if (inactiveShips[i].type == ShipType.Military)
                            {
                                s = inactiveShips[i];
                                inactiveShips.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        s = Instantiate(Resources.Load<GameObject>("Prefs/lightWarship")).GetComponent<Ship>();
                        if (useAdvancedMaterials) ReplaceMaterials(s.gameObject, true);
                    }
                    break;
                }
		}
        return s;
	}
	public void ReturnShipToPool(Ship s) {
		if (s == null || !s.gameObject.activeSelf) return;
		s.gameObject.SetActive(false);
        inactiveShips.Add(s);
		if (shipsClearTimer == 0) shipsClearTimer = clearTime;
	}

    public static GameObject GetRooftop(bool peak, bool artificial)
    {
        if (artificial) return GetRooftop(peak, artificial, 0);
        else {
            if (peak) {
                float f = Random.value;
                byte n = 0;
                if (f >= 0.77f) n = 2;
                else if (f <= 0.33f) n = 0; else n = 1;
                return GetRooftop(true, false, n);
            }
            else { if (Random.value > 0.5f) return GetRooftop(false, false, 0); else return GetRooftop(false, false, 1); }
        }
    }
    public static GameObject GetRooftop(bool peak, bool artificial, byte number) {
        GameObject g = Instantiate(Resources.Load<GameObject>("Prefs/Rooftops/" + (artificial ? "artificial" : "natural") + (peak ? "Peak" : "Rooftop") + number.ToString() ) );
        g.name = (artificial ? "a" : "n") + (peak ? "p" : "r") + number.ToString();
        if (useAdvancedMaterials) ReplaceMaterials(g, true);
        return g;
	}
    public static GameObject GetFlyingPlatform()
    {
        return Instantiate(Resources.Load<GameObject>("Prefs/flyingPlatform_small"));
    }

    public static Mesh SetMaterialByID(ref MeshFilter mf, ref MeshRenderer mr, int materialID, byte i_illumination)
    {
        var m = mf.mesh;
        SetMeshUVs(ref m, materialID);
        mf.sharedMesh = m;
        if (useIlluminationSystem) mr.sharedMaterial = GetMaterial(materialID, i_illumination);
        else mr.sharedMaterial = GetMaterial(materialID);
        return m;
    }
    public static void SetMeshUVs(ref Mesh m, int materialID)
    {
        Vector2[] borders ;
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
            case MATERIAL_ADVANCED_COVERING_ID:
                borders = new Vector2[] { new Vector2(3 * piece, piece), new Vector2(3 * piece, 2 * piece), new Vector2(4 * piece, 2 * piece), new Vector2(4 * piece, piece) };
                break;
            case MATERIAL_GRASS_100_ID:
                borders = new Vector2[] { new Vector2(piece, 0), new Vector2(piece, piece), new Vector2(2 * piece, piece), new Vector2(2 * piece, 0) };
                break;
            case MATERIAL_GRASS_80_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, 0), new Vector2(2 * piece + add, piece), new Vector2(3 * piece + add, piece), new Vector2(3 * piece + add, 0) };
                break;
            case MATERIAL_GRASS_60_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, piece), new Vector2(2 * piece + add, 2 * piece), new Vector2(3 * piece + add, 2 * piece), new Vector2(3 * piece + add, piece) };
                break;
            case MATERIAL_GRASS_40_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, 2 * piece), new Vector2(2 * piece + add, 3 * piece), new Vector2(3 * piece + add, 3 * piece), new Vector2(3 * piece + add, 2 * piece) };
                break;
            case MATERIAL_GRASS_20_ID:
                borders = new Vector2[] { new Vector2(2 * piece + add, 3 * piece), new Vector2(2 * piece + add, 4 * piece), new Vector2(3 * piece + add, 4 * piece), new Vector2(3 * piece + add, 3 * piece) };
                break;
            case MATERIAL_LEAVES_ID:
                borders = new Vector2[] { Vector2.zero, Vector2.up * piece, Vector2.one * piece, Vector2.right * piece };
                break;
            case MATERIAL_WHITE_METAL_ID:
                borders = new Vector2[] { new Vector2(2 * piece, 2 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 2 * piece) };
                break;
            case MATERIAL_DEAD_LUMBER_ID:
                borders = new Vector2[] { new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(3 * piece, 3 * piece) };
                break;
            case MATERIAL_WHITEWALL_ID:
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
            if (xl > yl)   k = piece / xl;
            else  k = piece / yl;
            float x0 = borders[0].x, y0 = borders[0].y;
            for (int i = 0; i< uvEdited.Length; i++)
            {
                uvEdited[i] = new Vector2(x0 + (uvEdited[i].x - minX) * k, y0 + (uvEdited[i].y - minY) * k );
            }
        }
        m.uv = uvEdited;
    }

    public static MaterialType GetMaterialType(int materialID)
    {
        switch (materialID)
        {
            case ResourceType.PLASTICS_ID:
            case ResourceType.CONCRETE_ID:
            case ResourceType.SNOW_ID:
            case ResourceType.DIRT_ID:
            case ResourceType.STONE_ID:
            case ResourceType.FERTILE_SOIL_ID:
            case MATERIAL_ADVANCED_COVERING_ID:
            case ResourceType.LUMBER_ID:
            case MATERIAL_DEAD_LUMBER_ID:
            case MATERIAL_WHITEWALL_ID:
            case ResourceType.MINERAL_F_ID:
            case ResourceType.MINERAL_L_ID:
                return MaterialType.Basic;

            case ResourceType.METAL_K_ID:
            case ResourceType.METAL_K_ORE_ID:
            case ResourceType.METAL_M_ORE_ID:
            case ResourceType.METAL_M_ID:
            case ResourceType.METAL_E_ORE_ID:
            case ResourceType.METAL_E_ID:
            case ResourceType.METAL_N_ORE_ID:
            case ResourceType.METAL_N_ID:
            case ResourceType.METAL_P_ORE_ID:
            case ResourceType.METAL_P_ID:
            case ResourceType.METAL_S_ORE_ID:
            case ResourceType.METAL_S_ID:       
            case MATERIAL_WHITE_METAL_ID:
                return MaterialType.Metal;

            case ResourceType.GRAPHONIUM_ID: return MaterialType.Energy;
            case MATERIAL_GRASS_100_ID:
            case MATERIAL_GRASS_80_ID:
            case MATERIAL_GRASS_60_ID:
            case MATERIAL_GRASS_40_ID:
            case MATERIAL_GRASS_20_ID:
            case MATERIAL_LEAVES_ID: return MaterialType.Green;
            default: return MaterialType.Basic;
        }
    }
    public static Material GetMaterial (MaterialType mtype)
    {
        switch (mtype)
        {
            case MaterialType.Metal:
                return metal_material;
            case MaterialType.Green:
                return green_material;
            case MaterialType.Energy:
                return energyMaterial;
            case MaterialType.Basic:
            default:
                return basic_material;
        }
    }
    public static Material GetMaterial (MaterialType mtype, byte i_illumination)
    {
        if (mtype == MaterialType.Energy) return GetMaterial(mtype);
        byte p = (byte)(1f / MAX_MATERIAL_LIGHT_DIVISIONS * 127.5f);
        if (i_illumination < p) return darkness_material;
        else
        {
            if (i_illumination > 255 - p) return GetMaterial(mtype);
            else
            {
                p *= 2;
                i_illumination -= (byte)(i_illumination % p);
                Material m = null;

                var key = new LightPoolInfo(mtype, i_illumination);
                if (lightPoolMaterials.ContainsKey(key))
                {
                    lightPoolMaterials.TryGetValue(key, out m);
                    if (m == null) return GetMaterial(mtype);
                    else return m;
                }
                else
                {
                    Material m0 = GetMaterial(mtype);
                    if (m0.HasProperty("_Illumination"))
                    {
                        m = new Material(m0);
                        m.SetFloat("_Illumination", i_illumination / 255f);
                        lightPoolMaterials.Add(key, m);
                        return m;
                    }
                    else return m0;
                }
            }
        }
    }
    public static Material GetMaterial(int id)
    {
        switch (id)
        {
            case ResourceType.PLASTICS_ID:
            case ResourceType.CONCRETE_ID:
            case ResourceType.SNOW_ID:
            case ResourceType.DIRT_ID:
            case ResourceType.STONE_ID:
            case ResourceType.FERTILE_SOIL_ID:
            case MATERIAL_ADVANCED_COVERING_ID:
            case ResourceType.LUMBER_ID:
            case MATERIAL_DEAD_LUMBER_ID:
            case MATERIAL_WHITEWALL_ID:
            case ResourceType.MINERAL_F_ID:
            case ResourceType.MINERAL_L_ID:
                return basic_material;

            case ResourceType.METAL_K_ID:
            case ResourceType.METAL_K_ORE_ID:
            case ResourceType.METAL_M_ORE_ID:
            case ResourceType.METAL_M_ID: 
            case ResourceType.METAL_E_ORE_ID:
            case ResourceType.METAL_E_ID: 
            case ResourceType.METAL_N_ORE_ID:
            case ResourceType.METAL_N_ID:
            case ResourceType.METAL_P_ORE_ID:
            case ResourceType.METAL_P_ID: 
            case ResourceType.METAL_S_ORE_ID:
            case ResourceType.METAL_S_ID:            
            case MATERIAL_WHITE_METAL_ID:
                return metal_material;

            case ResourceType.GRAPHONIUM_ID:
                return energyMaterial;
            case MATERIAL_GRASS_100_ID:             
            case MATERIAL_GRASS_80_ID:                
            case MATERIAL_GRASS_60_ID:               
            case MATERIAL_GRASS_40_ID:              
            case MATERIAL_GRASS_20_ID:               
            case MATERIAL_LEAVES_ID: return green_material;
            default: return default_material;
        }
    }
    private static Material GetMaterial(int id, byte i_illumination)
    {
        var mtype = GetMaterialType(id);
        if (mtype == MaterialType.Energy) return GetMaterial(id);
        byte p = (byte)(1f / MAX_MATERIAL_LIGHT_DIVISIONS * 127.5f);
        if (i_illumination < p) return darkness_material;
        else
        {
            if (i_illumination > 255 - p) return GetMaterial(id);
            else
            {
                p *= 2;
                i_illumination -= (byte)(i_illumination % p);
                Material m = null;

                var key = new LightPoolInfo(mtype, i_illumination);
                if (lightPoolMaterials.ContainsKey(key))
                {
                    lightPoolMaterials.TryGetValue(key, out m);
                    if (m == null) return GetMaterial(id);
                    else return m;
                }
                else
                {
                    Material m0 = GetMaterial(id);
                    if (m0.HasProperty("_Illumination"))
                    {
                        m = new Material(m0);
                        m.SetFloat("_Illumination", i_illumination / 255f);
                        lightPoolMaterials.Add(key, m);
                        return m;
                    }
                    else return m0;
                }
            }
        }        
    }    

    public static void ReplaceMaterials(GameObject g, bool i_useAdvancedMaterials)
    {
        Renderer[] rrs = g.GetComponentsInChildren<Renderer>();
        if (rrs.Length > 0)
        {
            ReplaceMaterials(rrs, i_useAdvancedMaterials);
        }
    }
    public static void ReplaceMaterials(Renderer[] rrs, bool i_useAdvancedMaterials)
    {
        Material[] materials;
        if (i_useAdvancedMaterials)
        { // нужны улучшенные
            if (useAdvancedMaterials) //сейчас используются улучшенные
            {
                materials = new Material[5]
                {
                basic_material,
                glassMaterial,
                glassMaterial_disabled,
                green_material,
                metal_material
                };
            }
            else // сейчас используются обычные
            {
                materials = new Material[5]
                {
                Resources.Load<Material>("Materials/Advanced/Basic_PBR"),
                Resources.Load<Material>("Materials/Advanced/Glass_PBR"),
                Resources.Load<Material>("Materials/Advanced/GlassOffline_PBR"),
                Resources.Load<Material>("Materials/Advanced/Green_PBR"),
                Resources.Load<Material>("Materials/Advanced/Metal_PBR")
                };
            }
        }
        else // нужны обычные
        {
            if (useAdvancedMaterials) // сейчас используются улучшенные
            {
                materials = new Material[5] 
                {
                Resources.Load<Material>("Materials/Basic"),
                Resources.Load<Material>("Materials/Glass"),
                Resources.Load<Material>("Materials/GlassOffline"),
                Resources.Load<Material>("Materials/Green"),
                Resources.Load<Material>("Materials/Metal"),
                };
            }
            else // сейчас используются обычные
            {
                materials = new Material[5]
                {
                basic_material,
                glassMaterial,
                glassMaterial_disabled,
                green_material,
                metal_material
                };
            }
        }        
        foreach (Renderer mr in rrs)
        {
            bool castShadows = false, receiveShadows = false;
            if (mr.sharedMaterial != null)
            {
                switch (mr.sharedMaterial.name)
                {
                    case "Basic":
                    case "Basic_PBR":
                        mr.sharedMaterial = materials[0];
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "Glass":
                    case "Glass_PBR":
                        mr.sharedMaterial = materials[1];
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "GlassOffline":
                    case "GlassOffline_PBR":
                        mr.sharedMaterial = materials[2];
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "Vegetation":
                    case "Green":
                    case "Green_PBR":
                        mr.sharedMaterial = materials[3];
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "Metal":
                    case "Metal_PBR":
                        mr.sharedMaterial = materials[4];
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "Sailcloth":
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "BillboardMaterial":
                    case "ShadedBillboard":
                        if (i_useAdvancedMaterials) mr.sharedMaterial = billboardShadedMaterial;
                        else mr.sharedMaterial = billboardMaterial;
                        castShadows = true;
                        receiveShadows = true;
                        break;
                    case "ChargedMaterial":
                    case "ChargedMaterial_advanced":
                        mr.sharedMaterial = energyMaterial;
                        castShadows = true;
                        receiveShadows = false;
                        break;
                }
            }
            if (shadowCasting)
            {
                if (castShadows) mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                else mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = receiveShadows;
            }
            else
            {
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        else
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
        }
    }
}
