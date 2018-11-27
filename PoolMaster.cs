using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GreenMaterial { Leaves, Grass100, Grass80, Grass60, Grass40, Grass20}
public enum MetalMaterial { MetalK, MetalM, MetalE, MetalN, MetalP, MetalS}
public enum BasicMaterial { Concrete, Plastic, Lumber,Dirt,Stone, Farmland, MineralF, MineralL, DeadLumber, Snow}

public sealed class PoolMaster : MonoBehaviour {
    public static PoolMaster current;	
    public static List<GameObject> quadsPool;    
	public static GameObject mineElevator_pref {get;private set;}
    public static GameObject cavePref { get; private set; }
    // не убирать basic из public, так как нужен для сравнения при включении/выключении
    public static Material default_material, lr_red_material, lr_green_material, basic_material, energy_material, energy_offline_material,
        glass_material, glass_offline_material;
    public static Material billboardMaterial { get; private set; }    
    public static Material starsBillboardMaterial { get; private set; }
    public static Mesh plane_excavated_025, plane_excavated_05,plane_excavated_075;
	public static GUIStyle GUIStyle_RightOrientedLabel, GUIStyle_BorderlessButton, GUIStyle_BorderlessLabel, GUIStyle_CenterOrientedLabel, GUIStyle_SystemAlert,
	GUIStyle_RightBottomLabel, GUIStyle_COLabel_red, GUIStyle_Button_red;
    public static Sprite gui_overridingSprite;
    public static readonly Color gameOrangeColor = new Color(0.933f, 0.5686f, 0.27f);

    private static Transform zoneCube;
    private static bool useTextureRotation = false;
    private static Material[] basic_illuminated, green_illuminated, metal_illuminated;
    private static Material metal_material, green_material, darkness_material;
    private static GameObject quad_pref;    

    private GameObject lightPassengerShip_pref, lightCargoShip_pref, lightWarship_pref, privateShip_pref;
    private List<GameObject> lightPassengerShips, mediumPassengerShips, heavyPassengerShips, lightCargoShips, mediumCargoShips, heavyCargoShips,
        lightWarships, mediumWarships, heavyWarships, privateShips;// только неактивные	
    private float shipsClearTimer = 0, clearTime = 30;
    private ParticleSystem buildEmitter;
    private Sprite[] starsSprites;

    public const byte MAX_MATERIAL_LIGHT_DIVISIONS = 5;
    private const byte MEDIUM_SHIP_LVL = 4, HEAVY_SHIP_LVL = 6;    
    private const int SHIPS_BUFFER_SIZE = 5;

    public void Load() {
		if (current != null) return;
		current = this;

        buildEmitter = Instantiate(Resources.Load<ParticleSystem>("buildEmitter"));

		lightPassengerShip_pref = Resources.Load<GameObject>("Prefs/lightPassengerShip");
		lightCargoShip_pref = Resources.Load<GameObject>("Prefs/lightCargoShip");
		lightWarship_pref = Resources.Load<GameObject>("Prefs/lightWarship");
		privateShip_pref = Resources.Load<GameObject>("Prefs/privateShip");
		lightPassengerShips = new List<GameObject>();
		lightCargoShips = new List<GameObject>();
		lightWarships = new List<GameObject>();
		privateShips = new List<GameObject>();

		plane_excavated_025 = Resources.Load<Mesh>("Meshes/Plane_excavated_025");
		plane_excavated_05 = Resources.Load<Mesh>("Meshes/Plane_excavated_05");
		plane_excavated_075 = Resources.Load<Mesh>("Meshes/Plane_excavated_075");

		lr_red_material = Resources.Load<Material>("Materials/GUI_Red");
		lr_green_material = Resources.Load<Material>("Materials/GUI_Green");

        zoneCube = Instantiate(Resources.Load<Transform>("Prefs/zoneCube"), transform);zoneCube.gameObject.SetActive(false);
        cavePref = Resources.Load<GameObject>("Prefs/CaveBlock_pref");
        quadsPool = new List<GameObject>();
        quad_pref = Instantiate(Resources.Load<GameObject>("Prefs/quadPref"), transform);		// ууу, костыль! а если текстура не 4 на 4 ?
        //quad_pref.GetComponent<MeshFilter>().sharedMesh.uv = new Vector2[] { new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), new Vector2(0.98f, 0.02f), new Vector2(0.02f, 0.98f) };
        quad_pref.GetComponent<MeshFilter>().sharedMesh.uv = new Vector2[] { Vector2.zero, Vector2.one, Vector2.right, Vector2.up };
        quad_pref.transform.parent = transform;
        quad_pref.SetActive(false);
        quadsPool.Add(quad_pref);

        default_material = Resources.Load<Material>("Materials/Default");
        darkness_material = Resources.Load<Material>("Materials/Darkness");
		energy_material = Resources.Load<Material>("Materials/ChargedMaterial");
		energy_offline_material = Resources.Load<Material>("Materials/UnchargedMaterial");
        basic_material = Resources.Load<Material>("Materials/Basic");
        glass_material = Resources.Load<Material>("Materials/Glass");
        glass_offline_material = Resources.Load<Material>("Materials/GlassOffline");
        metal_material = Resources.Load<Material>("Materials/Metal");
        green_material = Resources.Load<Material>("Materials/Green");
        billboardMaterial = Resources.Load<Material>("Materials/BillboardMaterial");
        starsBillboardMaterial = Resources.Load<Material>("Materials/StarsBillboardMaterial");

        basic_illuminated = new Material[MAX_MATERIAL_LIGHT_DIVISIONS];
        green_illuminated = new Material[MAX_MATERIAL_LIGHT_DIVISIONS];
        metal_illuminated = new Material[MAX_MATERIAL_LIGHT_DIVISIONS];

        mineElevator_pref = Resources.Load<GameObject>("Structures/MineElevator");
        QuestUI.LoadTextures();
        gui_overridingSprite = Resources.Load<Sprite>("Textures/gui_overridingSprite");
        starsSprites = Resources.LoadAll<Sprite>("Textures/stars");
	}

	void Update() {
        if (GameMaster.editMode) return;
		int docksCount = GameMaster.colonyController.docks.Count;
		if (shipsClearTimer > 0) {
			shipsClearTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (shipsClearTimer <= 0) {
				if (lightPassengerShips.Count > SHIPS_BUFFER_SIZE) {
					Destroy(lightPassengerShips[0]);
					lightPassengerShips.RemoveAt(0);
				}
				if (lightCargoShips.Count > SHIPS_BUFFER_SIZE) {
					Destroy(lightCargoShips[0]);
					lightCargoShips.RemoveAt(0);
				}
				if (lightWarships.Count > SHIPS_BUFFER_SIZE) {
					Destroy(lightWarships[0]);
					lightWarships.RemoveAt(0);
				}
				if (privateShips.Count > SHIPS_BUFFER_SIZE) {
					Destroy(privateShips[0]);
					privateShips.RemoveAt(0);
				}
				shipsClearTimer = clearTime;
			}
		}       
	}

    public static GameObject GetQuad()
    {
        GameObject g = null;
        if (quadsPool.Count == 1) g = Instantiate(quadsPool[0]); // нулевой никогда не удаляется
        else
        {
            int i = quadsPool.Count - 1;
            g = quadsPool[i];
            quadsPool.RemoveAt(i);
        }
        g.transform.parent = null;
        g.SetActive(true);
        return g;
    }
    public static void ReturnQuadToPool(GameObject g)
    {
        if (g == null) return;
        else
        {
            g.SetActive(false);
            g.transform.parent = current.transform;
            quadsPool.Add(g);
        }
    }
    public static Mesh GetOriginalQuadMesh()
    {
        return quadsPool[0].GetComponent<MeshFilter>().sharedMesh;
    }

    public Sprite GetStarSprite()
    {
        return (starsSprites[(int)(Random.value * (starsSprites.Length - 1))]);
    }

    public void BuildSplash(Vector3 pos)
    {
        buildEmitter.transform.position = pos;
        buildEmitter.Emit(20);
    }
    public void DrawZone(Vector3 point, Vector3 scale, Color col)
    {
        zoneCube.position = point;
        zoneCube.localScale = scale;
        zoneCube.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_MainColor", col);
        zoneCube.gameObject.SetActive(true);
    }
    public void DisableZone() { zoneCube.gameObject.SetActive(false); }

    public Ship GetShip(byte level, ShipType type) {
		Ship s = null;
		List<GameObject> searchList = null;
		GameObject pref = null;
		switch (type) {
		case ShipType.Passenger:
			if (level < MEDIUM_SHIP_LVL) {
				searchList = lightPassengerShips;
				pref = lightPassengerShip_pref;
			}
			break;
		case ShipType.Cargo:
			if (level < MEDIUM_SHIP_LVL) {
				searchList = lightCargoShips;
				pref = lightCargoShip_pref;
			}
			break;
		case ShipType.Private:
			if (level < MEDIUM_SHIP_LVL) {
				searchList = privateShips;
				pref = privateShip_pref;
			}
			break;
		case ShipType.Military:
			if (level < MEDIUM_SHIP_LVL) {
				searchList = lightWarships;
				pref = lightWarship_pref;
			}
			break;
		}
		bool found = false;
		if (searchList != null) {
			int i = 0;
			while (i < searchList.Count && !found) {
				if (searchList[i] == null) {searchList.RemoveAt(i);continue;}
				found = true;
				s = searchList[i].GetComponent<Ship>();
				searchList.RemoveAt(i);
				i++;
				break;
			}
			if ( !found ) s = Instantiate(pref).GetComponent<Ship>();
			s.gameObject.SetActive(true);
			return s;
		}
		else {
			print ("error in ships array finding");
			return null;
		}
	}
	public void ReturnShipToPool(Ship s) {
		if (s == null || !s.gameObject.activeSelf) return;
		s.gameObject.SetActive(false);
		switch (s.type) {
		case ShipType.Cargo:
			if (s.level > MEDIUM_SHIP_LVL) {
				if (s.level > HEAVY_SHIP_LVL) heavyCargoShips.Add(s.gameObject);
				else mediumCargoShips.Add(s.gameObject);
			}
			else lightCargoShips.Add(s.gameObject);
			break;
		case ShipType.Passenger:
			if (s.level > MEDIUM_SHIP_LVL) {
				if (s.level > HEAVY_SHIP_LVL) heavyPassengerShips.Add(s.gameObject);
				else mediumPassengerShips.Add(s.gameObject);
			}
			else lightPassengerShips.Add(s.gameObject);
			break;
		case ShipType.Military:
			if (s.level > MEDIUM_SHIP_LVL) {
				if (s.level > HEAVY_SHIP_LVL) heavyWarships.Add(s.gameObject);
				else mediumWarships.Add(s.gameObject);
			}
			else lightWarships.Add(s.gameObject);
			break;
		case ShipType.Private:
			privateShips.Add(s.gameObject);
			break;
		}
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
        return g;
	}

    public static Material GetGreenMaterial(GreenMaterial mtype, MeshFilter mf, byte i_illumination)
    {
        float illumination = i_illumination / 255f;
        float p = 1f / (MAX_MATERIAL_LIGHT_DIVISIONS + 1); // цена деления на шкале освещенности
        if (illumination < p / 2f) return darkness_material;

        Mesh quad = mf.mesh;
        if (quad == null) return green_material;           
        float piece = 0.25f, add = ((Random.value > 0.5) ? piece : 0);
        Vector2[] borders;
        switch (mtype)
        {
            default:
            case GreenMaterial.Leaves:
                borders = new Vector2[] { Vector2.zero, Vector2.up * piece, Vector2.one * piece, Vector2.right * piece};
                break;
            case GreenMaterial.Grass100:
                borders = new Vector2[] { new Vector2(piece, 0), new Vector2(piece, piece), new Vector2(2 * piece, piece), new Vector2(2 * piece, 0) };
                break;
            case GreenMaterial.Grass80:
                borders = new Vector2[] { new Vector2(2 * piece + add, 0), new Vector2(2 * piece + add, piece), new Vector2(3 * piece + add, piece), new Vector2(3 * piece + add, 0) };
                break;
            case GreenMaterial.Grass60:
                borders = new Vector2[] { new Vector2(2 * piece + add, piece), new Vector2(2 * piece + add, 2 * piece), new Vector2(3 * piece + add, 2 * piece), new Vector2(3 * piece + add, piece) };
                break;
            case GreenMaterial.Grass40:
                borders = new Vector2[] { new Vector2(2 * piece + add, 2 *piece), new Vector2(2 * piece + add, 3* piece), new Vector2(3 * piece + add, 3 * piece), new Vector2(3 * piece + add, 2 * piece) };
                break;
            case GreenMaterial.Grass20:
                borders = new Vector2[] { new Vector2(2 * piece + add, 3 * piece), new Vector2(2 * piece + add, 4 * piece), new Vector2(3 * piece + add, 4 * piece), new Vector2(3 * piece + add, 3 * piece) };
                break;
        }
        // крутим развертку, если это квад, иначе просто перетаскиваем 
        bool isQuad = (quad.uv.Length == 4);
        Vector2[] uvEdited = quad.uv;
        if (isQuad)
        {
            borders = new Vector2[] { borders[0] + Vector2.one * 0.01f, borders[1] + new Vector2(0.01f, -0.01f), borders[2] - Vector2.one * 0.01f, borders[3] - new Vector2(0.01f, -0.01f) };
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
            for (int i = 0; i < uvEdited.Length; i++)
            {
                uvEdited[i] = new Vector2(uvEdited[i].x % piece, uvEdited[i].y % piece); // относительное положение в собственной текстуре
                uvEdited[i] = new Vector2(borders[0].x + uvEdited[i].x, borders[0].y + uvEdited[i].y);
            }            
        }
        quad.uv = uvEdited;

        if (illumination >= 1 - p / 2f) return green_material;
        else
        {
            // проверка на darkness в самом начале функции
            int pos = (int)(illumination / p);
            if (illumination - pos * p > p / 2f)
            {
                pos++;
            }
            if (pos >= MAX_MATERIAL_LIGHT_DIVISIONS) return green_material;
            else
            {
                if (green_illuminated[pos] == null)
                {
                    green_illuminated[pos] = new Material(green_material);
                    green_illuminated[pos].SetFloat("_Illumination", p * (pos + 1));
                }
                return green_illuminated[pos];
            }
        }
    }
    public static Material GetMetalMaterial(MetalMaterial mtype, MeshFilter mf, byte i_illumination)
    {
        float illumination = i_illumination / 255f;
        float p = 1f / (MAX_MATERIAL_LIGHT_DIVISIONS + 1); // цена деления на шкале освещенности
        if (illumination < p / 2f) return darkness_material;

        Mesh quad = mf.mesh;
        if (quad == null) return metal_material;
            float piece = 0.25f;
            Vector2[] borders;
            switch (mtype)
            {
                default:
                case MetalMaterial.MetalK:
                    borders = new Vector2[] { new Vector2(0, 3 * piece), new Vector2(0, 4 * piece), new Vector2(piece, 4 * piece), new Vector2(piece, 3 * piece) };
                    break;
                case MetalMaterial.MetalM:
                    borders = new Vector2[] { new Vector2(piece, 3 * piece), new Vector2(piece, 4 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(2 * piece, 3 * piece) };
                    break;
                case MetalMaterial.MetalE:
                    borders = new Vector2[] { new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(3 * piece, 3 * piece) };
                    break;
                case MetalMaterial.MetalN:
                    borders = new Vector2[] { new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(4 * piece, 4 * piece), new Vector2(4 * piece, 3 * piece) };
                    break;
                case MetalMaterial.MetalP:
                    borders = new Vector2[] { new Vector2(0, 2 * piece), new Vector2(0, 3 * piece), new Vector2(piece, 3 * piece), new Vector2(piece, 2 * piece) };
                    break;
                case MetalMaterial.MetalS:
                    borders = new Vector2[] { new Vector2(piece, 2 * piece), new Vector2(piece, 3 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 2 * piece) };
                    break;
            }
            bool isQuad = (quad.uv.Length == 4);
            Vector2[] uvEdited = quad.uv;
            if (isQuad)
            {
                borders = new Vector2[] { borders[0] + Vector2.one * 0.01f, borders[1] + new Vector2(0.01f, -0.01f), borders[2] - Vector2.one * 0.01f, borders[3] - new Vector2(0.01f, -0.01f) };
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
                for (int i = 0; i < uvEdited.Length; i++)
                {
                    uvEdited[i] = new Vector2(uvEdited[i].x % piece, uvEdited[i].y % piece); // относительное положение в собственной текстуре
                    uvEdited[i] = new Vector2(borders[0].x + uvEdited[i].x, borders[0].y + uvEdited[i].y);
                }
            }
            quad.uv = uvEdited;

            if (illumination >= 1 - p / 2f) return metal_material;
            else
            {
                // проверка на darkness в самом начале функции
                int pos = (int)(illumination / p);
                if (illumination - pos * p > p / 2f)
                {
                    pos++;
                }
                if (pos >= MAX_MATERIAL_LIGHT_DIVISIONS) return metal_material;
                else
                {
                    if (metal_illuminated[pos] == null)
                    {
                        metal_illuminated[pos] = new Material(metal_material);
                        metal_illuminated[pos].SetFloat("_Illumination", p * (pos + 1));
                    }
                    return metal_illuminated[pos];
                }
            }
    }
    public static Material GetBasicMaterial(BasicMaterial mtype, MeshFilter mf, byte i_illumination)
    {
        float illumination = i_illumination / 255f;
        float p = 1f / (MAX_MATERIAL_LIGHT_DIVISIONS + 1); // цена деления на шкале освещенности
        if (illumination < p / 2f) return darkness_material;

        Mesh quad = mf.mesh;
        if (quad != null)
        {
            float piece = 0.25f;
            Vector2[] borders;
            // 1 2
            // 0 3
            switch (mtype)
            {
                default:
                case BasicMaterial.Concrete:
                    borders = new Vector2[] { new Vector2(0, 3 * piece), new Vector2(0, 4 * piece), new Vector2(piece, 4 * piece), new Vector2(piece, 3 * piece) };
                    break;
                case BasicMaterial.Plastic:
                    borders = new Vector2[] { new Vector2(piece, 3 * piece), new Vector2(piece, 4 * piece), new Vector2(2 * piece, 4 * piece), new Vector2(2 * piece, 3 * piece) };
                    break;
                case BasicMaterial.Stone:
                    borders = new Vector2[] { new Vector2(3 * piece, 2 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(4 * piece, 3 * piece), new Vector2(4 * piece, 2 * piece) };
                    break;
                case BasicMaterial.MineralF:
                    borders = new Vector2[] { new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(4 * piece, 4 * piece), new Vector2(4 * piece, 3 * piece) };
                    break;
                case BasicMaterial.Lumber:
                    borders = new Vector2[] { new Vector2(0, 2 * piece), new Vector2(0, 3 * piece), new Vector2(piece, 3 * piece), new Vector2(piece, 2 * piece) };
                    break;
                case BasicMaterial.Dirt:
                    borders = new Vector2[] { new Vector2(piece, 2 * piece), new Vector2(piece, 3 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(2 * piece, 2 * piece) };
                    break;
                case BasicMaterial.Farmland:
                    borders = new Vector2[] { new Vector2(2 * piece, 2 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 2 * piece) };
                    break;
                case BasicMaterial.MineralL:
                    borders = new Vector2[] { new Vector2(0, piece), new Vector2(0, 2 * piece), new Vector2(piece, 2 * piece), new Vector2(piece, piece) };
                    break;
                case BasicMaterial.DeadLumber:
                    borders = new Vector2[] { new Vector2(2 * piece, 3 *piece), new Vector2(2 * piece, 4 * piece), new Vector2(3 * piece, 4 * piece), new Vector2(3 * piece, 3 * piece) };
                    break;
                case BasicMaterial.Snow:
                    borders = new Vector2[] { new Vector2(piece, piece), new Vector2(piece, 2 * piece), new Vector2(2 * piece, 2 * piece), new Vector2(2 * piece, piece) };
                    break;
            }
            bool isQuad = (quad.uv.Length == 4);
            Vector2[] uvEdited = quad.uv;
            if (isQuad)
            {
                borders = new Vector2[] { borders[0] + Vector2.one * 0.01f, borders[1] + new Vector2(0.01f, -0.01f), borders[2] - Vector2.one * 0.01f, borders[3] - new Vector2(0.01f, -0.01f) };
                if (useTextureRotation)
                {
                    float seed = Random.value;
                    if (seed > 0.5f)
                    {
                        if (seed > 0.75f) uvEdited = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
                        else uvEdited = new Vector2[] { borders[1], borders[3], borders[0], borders[2] };
                    }
                    else
                    {
                        if (seed > 0.25f) uvEdited = new Vector2[] { borders[2], borders[0], borders[1], borders[3] };
                        else uvEdited = new Vector2[] { borders[3], borders[1], borders[2], borders[0] };
                    }
                }
                else
                {
                    //uvEditing = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
                    uvEdited = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
                }                
            }
            else
            {                
                for (int i = 0; i < uvEdited.Length; i++)
                {
                    uvEdited[i] = new Vector2(uvEdited[i].x % piece, uvEdited[i].y % piece); // относительное положение в собственной текстуре
                    uvEdited[i] = new Vector2(borders[0].x + uvEdited[i].x, borders[0].y + uvEdited[i].y);
                }
            }
            quad.uv = uvEdited;
        }

        if (illumination >= 1 - p/2f) return basic_material;
        else
        {        
            // проверка на darkness в самом начале функции
            int pos = (int)(illumination / p);
            if (illumination - pos * p > p / 2f)
            {
                pos++;
            }
            if (pos >= MAX_MATERIAL_LIGHT_DIVISIONS) return basic_material;
            else
            {
                if (basic_illuminated[pos] == null)
                {
                    basic_illuminated[pos] = new Material(basic_material);
                    basic_illuminated[pos].SetFloat("_Illumination", p * (pos + 1));
                }
                return basic_illuminated[pos];
            }
        }
    }
}
