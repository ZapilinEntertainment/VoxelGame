﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaterialType : byte { Basic, Metal, Energy, Green }
//dependency

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
    public enum MaterialsPack : byte { Standart, PBR, Simplified }
    
    public static bool useIlluminationSystem { get; private set; }
    public static bool shadowCasting { get; private set; }
    public static bool useDefaultMaterials { get { return currentMaterialsPack == MaterialsPack.Standart; } }
    public static int qualityLevel { get; private set; }
    public static PoolMaster current;	 
	public static GameObject mineElevator_pref {get;private set;}    
    public static Material energyMaterial { get; private set; }
    public static Material energyMaterial_disabled{ get; private set; }
    public static Material glassMaterial { get; private set; }
    public static Material glassMaterial_disabled { get; private set; }
    public static Material darkness_material{ get; private set; }
    public static Material billboardMaterial { get; private set; }
    public static Material celestialBillboardMaterial { get; private set; }
    public static Material billboardShadedMaterial { get; private set; }
    public static Material verticalBillboardMaterial { get; private set; }
    public static Material verticalWavingBillboardMaterial { get; private set; }
    public static Material starsBillboardMaterial { get; private set; }    
	public static GUIStyle GUIStyle_RightOrientedLabel, GUIStyle_BorderlessButton, GUIStyle_BorderlessLabel, GUIStyle_CenterOrientedLabel, GUIStyle_SystemAlert,
	GUIStyle_RightBottomLabel, GUIStyle_COLabel_red, GUIStyle_Button_red;
    public static Sprite gui_overridingSprite;
    public static readonly Color gameOrangeColor = new Color(0.933f, 0.5686f, 0.27f);

    private static Transform zoneCube;
    private static Dictionary<LightPoolInfo, Material> lightPoolMaterials;
    private static Material metal_material, green_material, default_material, lr_red_material, lr_green_material, basic_material;
    private static MaterialsPack currentMaterialsPack;
    private static Sprite[] starsSprites;

    private List<Ship> inactiveShips;	
    private float shipsClearTimer = 0, clearTime = 30;
    private ParticleSystem buildEmitter, citizensLeavingEmitter, lifepowerEmitter;    

    public static byte MAX_MATERIAL_LIGHT_DIVISIONS { get; private set; }
    public const int NO_MATERIAL_ID = -1, MATERIAL_ADVANCED_COVERING_ID = -2, MATERIAL_GRASS_100_ID = -3, MATERIAL_GRASS_80_ID = -4, MATERIAL_GRASS_60_ID = -5,
        MATERIAL_GRASS_40_ID = -6, MATERIAL_GRASS_20_ID = -7, MATERIAL_LEAVES_ID = -8, MATERIAL_WHITE_METAL_ID = -9, MATERIAL_DEAD_LUMBER_ID = -10,
        MATERIAL_WHITEWALL_ID = -11, MATERIAL_MULTIMATERIAL_ID = -12, FIXED_UV_BASIC = -13, CUTTED_LAYER_TEXTURE = -14;
    // зависимость - ResourceType.GetResourceByID
    private const int SHIPS_BUFFER_SIZE = 5, MAX_QUALITY_LEVEL = 2, BASIC_MAT_INDEX = 0, METAL_MAT_INDEX = 1, GREEN_MAT_INDEX = 2, GLASS_MAT_INDEX = 3, GLASS_OFF_MAT_INDEX = 4;
  
    public static Sprite LoadOverridingSprite()
    {
        return Resources.Load<Sprite>("Textures/gui_overridingSprite");
    }

    public void Load() {
		if (current != null) return;
		current = this;        
        DefineMaterialPack();
        LoadMaterials();
        useIlluminationSystem = !shadowCasting;
        if (qualityLevel != 0) // dependency : change quality level()
        {
            buildEmitter = Instantiate(Resources.Load<ParticleSystem>("buildEmitter"));
            lifepowerEmitter = Instantiate(Resources.Load<ParticleSystem>("lifepowerEmitter"));
         }
        inactiveShips = new List<Ship>();       

		lr_red_material = Resources.Load<Material>("Materials/GUI_Red");
		lr_green_material = Resources.Load<Material>("Materials/GUI_Green");

        zoneCube = Instantiate(Resources.Load<Transform>("Prefs/zoneCube"), transform);
        zoneCube.gameObject.SetActive(false);

        default_material = Resources.Load<Material>("Materials/Default");
        darkness_material = Resources.Load<Material>("Materials/Darkness");		
        energyMaterial_disabled = Resources.Load<Material>("Materials/UnchargedMaterial");       
        verticalBillboardMaterial = Resources.Load<Material>("Materials/VerticalBillboard");
        verticalWavingBillboardMaterial = Resources.Load<Material>("Materials/VerticalWavingBillboard");

        billboardShadedMaterial = Resources.Load<Material>("Materials/Advanced/shadedBillboard");
        billboardMaterial = Resources.Load<Material>("Materials/BillboardMaterial");
        // android errors here:
       // celestialBillboardMaterial = new Material(Shader.Find("Custom/CelestialBillboard"));
       // celestialBillboardMaterial.SetColor("_MainColor", Color.white);
        starsBillboardMaterial = Resources.Load<Material>("Materials/StarsBillboardMaterial");
        celestialBillboardMaterial = new Material(starsBillboardMaterial);
        celestialBillboardMaterial.SetFloat("_Tick", 1);

        //mineElevator_pref = Resources.Load<GameObject>("Structures/MineElevator");
        gui_overridingSprite = LoadOverridingSprite();
        starsSprites = Resources.LoadAll<Sprite>("Textures/stars");

        GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
        energyMaterial = Resources.Load<Material>("Materials/ChargedMaterial");

        if (GameMaster.realMaster?.mainChunk != null) GameMaster.realMaster.mainChunk.SetShadowCastingMode(shadowCasting);
        if (useIlluminationSystem)
        {
            lightPoolMaterials = new Dictionary<LightPoolInfo, Material>();
            MAX_MATERIAL_LIGHT_DIVISIONS = QualitySettings.GetQualityLevel() == 0 ? (byte)8 : (byte)16;
        }

        var rrs = Component.FindObjectsOfType<Renderer>();
        //if (rrs != null && rrs.Length != 0) ReplaceMaterials(rrs, useAdvancedMaterials);
        if (GameMaster.realMaster?.IsInTestMode ?? false) AnnouncementCanvasController.MakeAnnouncement("Pool master loaded");

        KnowledgeTabUI.PreparePartsTexture();
    }
    private static void LoadMaterials()
    {
        switch (currentMaterialsPack)
        {
            case MaterialsPack.PBR:
                currentMaterialsPack = MaterialsPack.PBR;
                shadowCasting = true;
                break;
            case MaterialsPack.Simplified:
                currentMaterialsPack = MaterialsPack.Simplified;
                shadowCasting = false;
                break;
            default:
                currentMaterialsPack = MaterialsPack.Standart;
                shadowCasting = false;                
                break;
        }
        var mats = LoadMaterialPack(currentMaterialsPack);
        glassMaterial_disabled = mats[GLASS_OFF_MAT_INDEX];
        basic_material = mats[BASIC_MAT_INDEX];
        glassMaterial = mats[GLASS_MAT_INDEX];
        metal_material = mats[METAL_MAT_INDEX];
        green_material = mats[GREEN_MAT_INDEX];
    }
    private static Material[] LoadMaterialPack(MaterialsPack mptype)
    {
        var mats = new Material[5];
        switch (mptype)
        {
            case MaterialsPack.PBR:
                {                    
                    mats[BASIC_MAT_INDEX] = Resources.Load<Material>("Materials/Advanced/Basic_PBR");
                    mats[GLASS_MAT_INDEX] = Resources.Load<Material>("Materials/Advanced/Glass_PBR");
                    mats[METAL_MAT_INDEX] = Resources.Load<Material>("Materials/Advanced/Metal_PBR");
                    mats[GREEN_MAT_INDEX] = Resources.Load<Material>("Materials/Advanced/Green_PBR");
                    mats[GLASS_OFF_MAT_INDEX] = Resources.Load<Material>("Materials/Advanced/GlassOffline_PBR");
                    break;
                }
            case MaterialsPack.Simplified:
                {
                    mats[GLASS_OFF_MAT_INDEX] = Resources.Load<Material>("Materials/Simplified/GlassOffline");
                    mats[BASIC_MAT_INDEX] = Resources.Load<Material>("Materials/Simplified/Basic");
                    mats[GLASS_MAT_INDEX] = Resources.Load<Material>("Materials/Simplified/Glass");
                    mats[METAL_MAT_INDEX] = Resources.Load<Material>("Materials/Simplified/Metal");
                    mats[GREEN_MAT_INDEX] = Resources.Load<Material>("Materials/Simplified/Green");
                    break;
                }
            default:
                {
                    mats[GLASS_OFF_MAT_INDEX] = Resources.Load<Material>("Materials/StandartPack/GlassOffline");
                    mats[BASIC_MAT_INDEX] = Resources.Load<Material>("Materials/StandartPack/Basic");
                    mats[GLASS_MAT_INDEX] = Resources.Load<Material>("Materials/StandartPack/Glass");
                    mats[METAL_MAT_INDEX] = Resources.Load<Material>("Materials/StandartPack/Metal");
                    mats[GREEN_MAT_INDEX] = Resources.Load<Material>("Materials/StandartPack/Green");
                    break;
                }                
        }
        return mats;
    }

    public static void ChangeQualityLevel (int newLevel)
    {
        if (qualityLevel != newLevel)
        {
            QualitySettings.SetQualityLevel(newLevel);
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
            DefineMaterialPack();
            qualityLevel = newLevel;
            LoadMaterials();
            if (GameMaster.realMaster.mainChunk != null) GameMaster.realMaster.mainChunk.SetShadowCastingMode(shadowCasting);            
            var rrs = Component.FindObjectsOfType<Renderer>();
            ReplaceMaterials(rrs);

            var aoe = FollowingCamera.cam.GetComponent<AmplifyOcclusionEffect>();
            var ql = QualitySettings.GetQualityLevel();
            /*
            if ( ql == 2)
            {
                if (aoe.enabled == false) aoe.enabled = true;
            }
            else
            {
                if (aoe.enabled) aoe.enabled = false;
            }
            */
        }
    }
    private static void DefineMaterialPack()
    {
        qualityLevel = QualitySettings.GetQualityLevel();
        switch (qualityLevel)
        {
            case 0:
                currentMaterialsPack = MaterialsPack.Simplified;
                break;
            case 2:
                currentMaterialsPack = MaterialsPack.PBR;
                break;
            default: currentMaterialsPack = MaterialsPack.Standart; break;
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
                        s = Instantiate(Resources.Load<GameObject>("Prefs/Ships/passengerShip_1")).GetComponent<Ship>();
                        s.Initialize();
                        if (currentMaterialsPack != MaterialsPack.Standart) ReplaceMaterials(s.gameObject, currentMaterialsPack);
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
                            case 2: s = Instantiate(Resources.Load<GameObject>("Prefs/Ships/mediumCargoShip")).GetComponent<Ship>(); break;
                            case 3: s = Instantiate(Resources.Load<GameObject>("Prefs/Ships/heavyCargoShip")).GetComponent<Ship>(); break;
                            default:  s = Instantiate(Resources.Load<GameObject>("Prefs/Ships/lightCargoShip")).GetComponent<Ship>();
                                break;
                        }
                        s.Initialize();
                        if (currentMaterialsPack != MaterialsPack.Standart) ReplaceMaterials(s.gameObject, currentMaterialsPack);
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
                        s = Instantiate(Resources.Load<GameObject>("Prefs/Ships/privateShip")).GetComponent<Ship>();
                        s.Initialize();
                        if (currentMaterialsPack != MaterialsPack.Standart) ReplaceMaterials(s.gameObject, currentMaterialsPack);
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
                        s = Instantiate(Resources.Load<GameObject>("Prefs/Ships/lightWarship")).GetComponent<Ship>();
                        s.Initialize();
                        if (currentMaterialsPack != MaterialsPack.Standart) ReplaceMaterials(s.gameObject, currentMaterialsPack);
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

    public static Mesh SetMaterialByID(ref MeshFilter mf, ref MeshRenderer mr, int materialID, byte i_illumination)
    {
        Mesh m = mf.mesh;
        MeshMaster.SetMeshUVs(ref m, materialID);
        mf.sharedMesh = m;
        if (useIlluminationSystem) mr.sharedMaterial = GetMaterial(materialID, i_illumination);
        else mr.sharedMaterial = GetMaterial(materialID);
        return m;
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
    public static string GetColouredMaterialName() { return "Coloured"; }

    public static bool IsMaterialAGrass(int id)
    {
        return (id == MATERIAL_GRASS_100_ID | id == MATERIAL_GRASS_20_ID | id == MATERIAL_GRASS_40_ID | id == MATERIAL_GRASS_60_ID | id == MATERIAL_GRASS_80_ID);
    }

    public static void ReplaceMaterials(GameObject g)
    {
        ReplaceMaterials(g, currentMaterialsPack);
    }
    public static void ReplaceMaterials(GameObject g, MaterialsPack mpack)
    {
        Renderer[] rrs = g.GetComponentsInChildren<Renderer>();
        if (rrs.Length > 0)
        {
            ReplaceMaterials(rrs, mpack);
        }
    }
    public static void ReplaceMaterials(Renderer[] rrs)
    {
        ReplaceMaterials(rrs, currentMaterialsPack);
    }
    public static void ReplaceMaterials(Renderer[] rrs, MaterialsPack mpack)
    {
        Material[] materials;      
        if (mpack == currentMaterialsPack)
        {
            materials = new Material[5];
            materials[BASIC_MAT_INDEX] = basic_material;
            materials[GLASS_MAT_INDEX] = glassMaterial;
            materials[GLASS_OFF_MAT_INDEX] = glassMaterial_disabled;
            materials[GREEN_MAT_INDEX] = green_material;
            materials[METAL_MAT_INDEX] = metal_material;
        }
        else
        {
            materials = LoadMaterialPack(mpack);
        }
        foreach (Renderer mr in rrs)
        {
            bool castShadows = false, receiveShadows = false;

            if (mr.sharedMaterials != null)
            {
                if (mr.sharedMaterials.Length == 1)
                {
                    if (mr.sharedMaterial != null)
                    {
                        switch (mr.sharedMaterial.name)
                        {
                            case "Basic":
                            case "Basic_PBR":
                                mr.sharedMaterial = materials[BASIC_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Glass":
                            case "Glass_PBR":
                                mr.sharedMaterial = materials[GLASS_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "GlassOffline":
                            case "GlassOffline_PBR":
                                mr.sharedMaterial = materials[GLASS_OFF_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Vegetation":
                            case "Green":
                            case "Green_PBR":
                                mr.sharedMaterial = materials[GREEN_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Metal":
                            case "Metal_PBR":
                                mr.sharedMaterial = materials[METAL_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Sailcloth":
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "BillboardMaterial":
                            case "shadedBillboard":
                                if (shadowCasting) mr.sharedMaterial = billboardShadedMaterial;
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
                }
                else
                {
                    var sm = mr.sharedMaterials;
                    var sm2 = new Material[sm.Length];
                    for (int i =0; i < sm.Length; i++)
                    {
                        if (sm[i] == null)
                        {
                            sm2[i] = null;
                            continue;
                        }
                        switch (sm[i].name)
                        {
                            case "Basic":
                            case "Basic_PBR":
                                sm2[i] = materials[BASIC_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Glass":
                            case "Glass_PBR":
                                sm2[i] = materials[GLASS_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "GlassOffline":
                            case "GlassOffline_PBR":
                                sm2[i] = materials[GLASS_OFF_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Vegetation":
                            case "Green":
                            case "Green_PBR":
                                sm2[i] = materials[GREEN_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Metal":
                            case "Metal_PBR":
                                sm2[i] = materials[METAL_MAT_INDEX];
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "Sailcloth":
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "BillboardMaterial":
                            case "shadedBillboard":
                                if (shadowCasting) sm2[i] = billboardShadedMaterial;
                                else sm2[i] = billboardMaterial;
                                castShadows = true;
                                receiveShadows = true;
                                break;
                            case "ChargedMaterial":
                            case "ChargedMaterial_advanced":
                                sm2[i] = energyMaterial;
                                castShadows = true;
                                receiveShadows = false;
                                break;
                            default: sm2[i] = sm[i]; break;
                        }
                    }
                    mr.sharedMaterials = sm2;
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


    public static void SwitchMaterialsToOnline(ICollection<Renderer> renderers)
    {
        if (renderers == null || renderers.Count == 0) return;
        foreach (var r in renderers)
        {
            if (r.sharedMaterials.Length > 1)
            {
                bool replacing = false;
                Material[] newMaterials = new Material[r.sharedMaterials.Length];
                for (int j = 0; j < r.sharedMaterials.Length; j++)
                {
                    Material m = r.sharedMaterials[j];
                    if (m == glassMaterial_disabled) { m = glassMaterial; replacing = true; }
                    else
                    {
                        if (m == energyMaterial_disabled) { m = energyMaterial; replacing = true; }
                    }
                    newMaterials[j] = m;
                }
                if (replacing) r.sharedMaterials = newMaterials;
            }
            else
            {
                Material m = r.sharedMaterial;
                bool replacing = false;
                if (m == glassMaterial_disabled) { m = glassMaterial; replacing = true; }
                else
                {
                    if (m == energyMaterial_disabled) { m = energyMaterial; replacing = true; }
                }
                if (replacing) r.sharedMaterial = m;
            }
        }
    }
    public static void SwitchMaterialToOnline(Renderer r)
    {
        if (r == null) return;
        if (r.sharedMaterials.Length > 1)
        {
            bool replacing = false;
            Material[] newMaterials = new Material[r.sharedMaterials.Length];
            for (int j = 0; j < r.sharedMaterials.Length; j++)
            {
                Material m = r.sharedMaterials[j];
                if (m == glassMaterial_disabled) { m = glassMaterial; replacing = true; }
                else
                {
                    if (m == energyMaterial_disabled) { m = energyMaterial; replacing = true; }
                }
                newMaterials[j] = m;
            }
            if (replacing) r.sharedMaterials = newMaterials;
        }
        else
        {
            Material m = r.sharedMaterial;
            bool replacing = false;
            if (m == glassMaterial_disabled) { m = glassMaterial; replacing = true; }
            else
            {
                if (m == energyMaterial_disabled) { m = energyMaterial; replacing = true; }
            }
            if (replacing) r.sharedMaterial = m;
        }
    }
    public static void SwitchMaterialsToOffline(ICollection<Renderer> renderers)
    {
        if (renderers == null || renderers.Count == 0) return;
        foreach (var r in renderers)
        {
            if (r.sharedMaterials.Length > 1)
            {
                bool replacing = false;
                Material[] newMaterials = new Material[r.sharedMaterials.Length];
                for (int j = 0; j < r.sharedMaterials.Length; j++)
                {
                    Material m = r.sharedMaterials[j];
                    if (m == glassMaterial) { m = glassMaterial_disabled; replacing = true; }
                    else
                    {
                        if (m == energyMaterial) { m = energyMaterial_disabled; replacing = true; }
                    }
                    newMaterials[j] = m;
                }
                if (replacing) r.sharedMaterials = newMaterials;
            }
            else
            {
                Material m = r.sharedMaterial;
                bool replacing = false;
                if (m == glassMaterial) { m = glassMaterial_disabled; replacing = true; }
                else
                {
                    if (m == energyMaterial) { m = energyMaterial_disabled; replacing = true; }
                }
                if (replacing) r.sharedMaterial = m;
            }
        }
    }
    public static void SwitchMaterialToOffline(Renderer r)
    {
        if (r == null) return;
        if (r.sharedMaterials.Length > 1)
        {
            bool replacing = false;
            Material[] newMaterials = new Material[r.sharedMaterials.Length];
            for (int j = 0; j < r.sharedMaterials.Length; j++)
            {
                Material m = r.sharedMaterials[j];
                if (m == glassMaterial) { m = glassMaterial_disabled; replacing = true; }
                else
                {
                    if (m == energyMaterial) { m = energyMaterial_disabled; replacing = true; }
                }
                newMaterials[j] = m;
            }
            if (replacing) r.sharedMaterials = newMaterials;
        }
        else
        {
            Material m = r.sharedMaterial;
            bool replacing = false;
            if (m == glassMaterial) { m = glassMaterial_disabled; replacing = true; }
            else
            {
                if (m == energyMaterial) { m = energyMaterial_disabled; replacing = true; }
            }
            if (replacing) r.sharedMaterial = m;
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
