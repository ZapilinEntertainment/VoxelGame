using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GreenMaterial { Leaves, Grass100, Grass80, Grass60, Grass40, Grass20}
public enum MetalMaterial { MetalK, MetalM, MetalE, MetalN, MetalP, MetalS}
public enum BasicMaterial { Concrete, Plastic, Lumber,Dirt,Stone, Farmland, MineralF, MineralL, DeadLumber}

public class PoolMaster : MonoBehaviour {
	public static PoolMaster current;
	GameObject lightPassengerShip_pref, lightCargoShip_pref, lightWarship_pref, privateShip_pref;
	public static GameObject quad_pref {get;private set;}
	public static GameObject mineElevator_pref {get;private set;}
    // не убирать basic из public, так как нужен для сравнения при включении/выключении
    public static Material default_material, lr_red_material, lr_green_material, basic_material, basic_offline_material, energy_material, energy_offline_material, glass_material, glass_offline_material;
    static Material metal_material, green_material;
    public static Mesh plane_excavated_025, plane_excavated_05,plane_excavated_075;
	public static Texture twoButtonsDivider_tx, plusButton_tx, minusButton_tx, plusX10Button_tx, minusX10Button_tx, quadSelector_tx,  orangeSquare_tx,
	greenArrow_tx, redArrow_tx, empty_tx, energyCrystal_icon_tx, shuttle_good_icon, shuttle_normal_icon, shuttle_bad_icon, 
	crew_good_icon, crew_normal_icon, crew_bad_icon, quest_defaultIcon, quest_unacceptableIcon;
	public static GUIStyle GUIStyle_RightOrientedLabel, GUIStyle_BorderlessButton, GUIStyle_BorderlessLabel, GUIStyle_CenterOrientedLabel, GUIStyle_SystemAlert,
	GUIStyle_RightBottomLabel, GUIStyle_COLabel_red, GUIStyle_Button_red;

	List<GameObject>  lightPassengerShips, mediumPassengerShips, heavyPassengerShips, lightCargoShips, mediumCargoShips, heavyCargoShips,
		lightWarships, mediumWarships, heavyWarships, privateShips;// только неактивные
	const byte MEDIUM_SHIP_LVL = 4, HEAVY_SHIP_LVL = 6;
	const int SHIPS_BUFFER_SIZE = 5;
	float  shipsClearTimer = 0,clearTime = 30;

    [SerializeField]
    Vector3 sunDirection, sunNextPosition, prevSunDirection;
    [SerializeField]
    float sunSpeed = 0.03f, lightMapUpdateTimer = 0;
    const float LIGHTMAP_UPDATE_TIME = 1, LIGHTMAP_VISUAL_CHANGE_THRESHOLD = 1f;
    const int lightmapResolution = 128;
    Color sunColor = Color.white;
    Transform sun;

	public void Load() {
		if (current != null) return;
		current = this;

		lightPassengerShip_pref = Resources.Load<GameObject>("Prefs/lightPassengerShip");
		lightCargoShip_pref = Resources.Load<GameObject>("Prefs/lightCargoShip");
		lightWarship_pref = Resources.Load<GameObject>("Prefs/lightWarship");
		privateShip_pref = Resources.Load<GameObject>("Prefs/privateShip");
		lightPassengerShips = new List<GameObject>();
		lightCargoShips = new List<GameObject>();
		lightWarships = new List<GameObject>();
		privateShips = new List<GameObject>();

		quest_unacceptableIcon = Resources.Load<Texture>("Textures/questUnacceptableIcon");
		quest_defaultIcon = Resources.Load<Texture>("Textures/questDefaultIcon");
		crew_good_icon = Resources.Load<Texture>("Textures/crew_good_icon");
		crew_normal_icon = Resources.Load<Texture>("Textures/crew_normal_icon");
		crew_bad_icon = Resources.Load<Texture>("Textures/crew_bad_icon");
		shuttle_good_icon = Resources.Load<Texture>("Textures/shuttle_good_icon");
		shuttle_normal_icon = Resources.Load<Texture>("Textures/shuttle_normal_icon");
		shuttle_bad_icon = Resources.Load<Texture>("Textures/shuttle_bad_icon");
		energyCrystal_icon_tx = Resources.Load<Texture>("Textures/energyCrystal_icon");
		empty_tx = Resources.Load<Texture>("Textures/resource_empty");
		redArrow_tx = Resources.Load<Texture>("Textures/redArrow");
		greenArrow_tx = Resources.Load<Texture>("Textures/greenArrow");
		orangeSquare_tx = Resources.Load<Texture>("Textures/orangeSquare");
		quadSelector_tx = Resources.Load<Texture>("Textures/quadSelector");
		minusX10Button_tx = Resources.Load<Texture>("Textures/minusX10Button");
		minusButton_tx = Resources.Load<Texture>("Textures/minusButton");
		plusX10Button_tx = Resources.Load<Texture>("Textures/plusX10Button");
		plusButton_tx = Resources.Load<Texture>("Textures/plusButton");
		twoButtonsDivider_tx = Resources.Load<Texture>("Textures/twoButton_divider");

		plane_excavated_025 = Resources.Load<Mesh>("Meshes/Plane_excavated_025");
		plane_excavated_05 = Resources.Load<Mesh>("Meshes/Plane_excavated_05");
		plane_excavated_075 = Resources.Load<Mesh>("Meshes/Plane_excavated_075");

		lr_red_material = Resources.Load<Material>("Materials/GUI_Red");
		lr_green_material = Resources.Load<Material>("Materials/GUI_Green");

        quad_pref = Instantiate(Resources.Load<GameObject>("Prefs/quadPref"), transform);		// ууу, костыль! а если текстура не 4 на 4 ?
        quad_pref.GetComponent<MeshFilter>().sharedMesh.uv = new Vector2[] { new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), new Vector2(0.98f, 0.02f), new Vector2(0.02f, 0.98f) };
        quad_pref.GetComponent<MeshRenderer>().enabled = false;
        default_material = Resources.Load<Material>("Materials/Default");

		energy_material = Resources.Load<Material>("Materials/ChargedMaterial");
		energy_offline_material = Resources.Load<Material>("Materials/UnchargedMaterial");
        basic_material = Resources.Load<Material>("Materials/Basic");
        basic_offline_material = Resources.Load<Material>("Materials/BasicOffline");
        glass_material = Resources.Load<Material>("Materials/Glass");
        glass_offline_material = Resources.Load<Material>("Materials/GlassOffline");
        metal_material = Resources.Load<Material>("Materials/Metal");
        green_material = Resources.Load<Material>("Materials/Green");


        mineElevator_pref = Resources.Load<GameObject>("Structures/MineElevator");
        sunDirection = Random.onUnitSphere; if (sunDirection.y < 0) sunDirection = new Vector3(sunDirection.x, sunDirection.y * (-1), sunDirection.z);
        sunNextPosition = Random.onUnitSphere; if (sunNextPosition.y < 0) sunNextPosition = new Vector3(sunNextPosition.x, sunNextPosition.y * (-1), sunNextPosition.z);
        Structure.LoadPrefs();
	}

	void Update() {

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
				if (searchList[i] == null) {Destroy(searchList[i]); searchList.RemoveAt(i);continue;}
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

	public static GameObject GetRooftop(Structure s) {
		GameObject g = null;
		g = GameObject.Instantiate(Resources.Load<GameObject>("Prefs/blockRooftop")) as GameObject;
		return g;
	}

    public static Material GetGreenMaterial(GreenMaterial mtype, MeshFilter mf)
    {
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
        Vector2[] uvEditing = quad.uv;
        if (isQuad)
        {
            borders = new Vector2[] { borders[0] + Vector2.one * 0.01f, borders[1] + new Vector2(0.01f, -0.01f), borders[2] - Vector2.one * 0.01f, borders[3] - new Vector2(0.01f, -0.01f) };
            float seed = Random.value;            
                if (seed > 0.5f)
                {
                    if (seed > 0.75f) quad.uv = new Vector2[] { borders[0] , borders[2], borders[3], borders[1] };
                    else quad.uv = new Vector2[] { borders[2], borders[3], borders[1], borders[0] };
                }
                else
                {
                    if (seed > 0.25f) quad.uv = new Vector2[] { borders[3], borders[1], borders[0], borders[2] };
                    else quad.uv = new Vector2[] { borders[1], borders[0], borders[2], borders[3] };
                }


            // Vector2[] uvs = new Vector2[] { new Vector2(0.0f,0.0f), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1)};
            uvEditing = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
        }
        else
        {            
            for (int i = 0; i < uvEditing.Length; i++)
            {
                uvEditing[i] = new Vector2(uvEditing[i].x % piece, uvEditing[i].y % piece); // относительное положение в собственной текстуре
                uvEditing[i] = new Vector2(borders[0].x + uvEditing[i].x, borders[0].y + uvEditing[i].y);
            }            
        }
        quad.uv = uvEditing;
        return green_material;
    }
    public static Material GetMetalMaterial(MetalMaterial mtype, MeshFilter mf)
    {
        Mesh quad = mf.mesh;
        if (quad != null)
        {
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
                    borders = new Vector2[] { new Vector2(piece, 2 * piece), new Vector2(2 * piece, 3 * piece), new Vector2(3 * piece, 3 * piece), new Vector2(3 * piece, 2 * piece) };
                    break;
            }
            bool isQuad = (quad.uv.Length == 4);
            Vector2[] uvEditing = quad.uv;
            if (isQuad)
            {
                float seed = Random.value;
                borders = new Vector2[] { borders[0] + Vector2.one * 0.01f, borders[1] + new Vector2(0.01f, -0.01f), borders[2] - Vector2.one * 0.01f, borders[3] - new Vector2(0.01f, -0.01f) };
                if (seed > 0.5f)
                {
                    if (seed > 0.75f) quad.uv = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
                    else quad.uv = new Vector2[] { borders[2], borders[3], borders[1], borders[0] };
                }
                else
                {
                    if (seed > 0.25f) quad.uv = new Vector2[] { borders[3], borders[1], borders[0], borders[2] };
                    else quad.uv = new Vector2[] { borders[1], borders[0], borders[2], borders[3] };
                }


                // Vector2[] uvs = new Vector2[] { new Vector2(0.0f,0.0f), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1)};
                uvEditing = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
            }
            else
            {
                for (int i = 0; i < uvEditing.Length; i++)
                {
                    uvEditing[i] = new Vector2(uvEditing[i].x % piece, uvEditing[i].y % piece); // относительное положение в собственной текстуре
                    uvEditing[i] = new Vector2(borders[0].x + uvEditing[i].x, borders[0].y + uvEditing[i].y);
                }
            }
            quad.uv = uvEditing;
        }
        return metal_material;
    }
    public static Material GetBasicMaterial(BasicMaterial mtype, MeshFilter mf, bool online)
    {
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
            }
            bool isQuad = (quad.uv.Length == 4);
            Vector2[] uvEditing = quad.uv;
            if (isQuad)
            {
                float seed = Random.value;
                borders = new Vector2[] { borders[0] + Vector2.one * 0.01f, borders[1] + new Vector2(0.01f, -0.01f), borders[2] - Vector2.one * 0.01f, borders[3] - new Vector2(0.01f, -0.01f) };
                if (seed > 0.5f)
                {
                    if (seed > 0.75f) quad.uv = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
                    else quad.uv = new Vector2[] { borders[2], borders[3], borders[1], borders[0] };
                }
                else
                {
                    if (seed > 0.25f) quad.uv = new Vector2[] { borders[3], borders[1], borders[0], borders[2] };
                    else quad.uv = new Vector2[] { borders[1], borders[0], borders[2], borders[3] };
                }               
                uvEditing = new Vector2[] { borders[0], borders[2], borders[3], borders[1] };
            }
            else
            {
                for (int i = 0; i < uvEditing.Length; i++)
                {
                    uvEditing[i] = new Vector2(uvEditing[i].x % piece, uvEditing[i].y % piece); // относительное положение в собственной текстуре
                    uvEditing[i] = new Vector2(borders[0].x + uvEditing[i].x, borders[0].y + uvEditing[i].y);
                }
            }
            quad.uv = uvEditing;
        }
        return basic_material;
    }

    public static void BuildSplashEffect(Structure s) {
		//заготовка
	}
}
