using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	public static PoolMaster current;
	public Material[] grassland_ready_25, grassland_ready_50;
	public Material dryingLeaves_material, leaves_material, dead_tree_material;
	GameObject lightPassengerShip_pref, lightCargoShip_pref, lightWarship_pref, privateShip_pref;
	public static GameObject quad_pref {get;private set;}
	public static GameObject mineElevator_pref {get;private set;}
	public static Material	default_material, lr_red_material, lr_green_material, grass_material, 
	glass_material, glass_offline_material, energy_material, energy_offline_material, colored_material, colored_offline_material;
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

		quad_pref = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad_pref.GetComponent<MeshRenderer>().enabled =false;
		default_material = Resources.Load<Material>("Materials/Default");

		colored_material = Resources.Load<Material>("Materials/Plastic");
		colored_offline_material = new Material(colored_material); colored_offline_material.color = Color.black;
		energy_material = Resources.Load<Material>("Materials/ChargedMaterial");
		energy_offline_material = Resources.Load<Material>("Materials/UnchargedMaterial");
		glass_material = Resources.Load<Material>("Materials/Glass");
		glass_offline_material = Resources.Load<Material>("Materials/OfflineGlass");
		dead_tree_material = Resources.Load<Material>("Materials/DeadTree");
		dryingLeaves_material = Resources.Load<Material>("Materials/DryingLeaves");
		leaves_material = Resources.Load<Material>("Materials/Leaves");
		grass_material = Resources.Load<Material>("Materials/Grass");

		mineElevator_pref = Resources.Load<GameObject>("Structures/MineElevator");

		Material dirtMaterial = ResourceType.GetMaterialById(ResourceType.DIRT_ID);
		grassland_ready_25 = new Material[4]; grassland_ready_50 = new Material[4];
		grassland_ready_25[0] = new Material(dirtMaterial ); grassland_ready_25[0].name ="grassland_25_0";
		grassland_ready_25[0].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_0"));
		grassland_ready_25[1] = new Material(dirtMaterial ); grassland_ready_25[1].name ="grassland_25_1";
		grassland_ready_25[1].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_1"));
		grassland_ready_25[2] = new Material(dirtMaterial ); grassland_ready_25[2].name ="grassland_25_2";
		grassland_ready_25[2].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_2"));
		grassland_ready_25[3] = new Material(dirtMaterial ); grassland_ready_25[3].name ="grassland_25_3";
		grassland_ready_25[3].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_3"));

		grassland_ready_50[0] = new Material(dirtMaterial ); grassland_ready_50[0].name ="grassland_50_0";
		grassland_ready_50[0].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_0"));
		grassland_ready_50[1] = new Material(dirtMaterial ); grassland_ready_50[1].name ="grassland_50_1";
		grassland_ready_50[1].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_1"));
		grassland_ready_50[2] = new Material(dirtMaterial ); grassland_ready_50[2].name ="grassland_50_2";
		grassland_ready_50[2].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_2"));
		grassland_ready_50[3] = new Material(dirtMaterial ); grassland_ready_50[3].name ="grassland_50_3";
		grassland_ready_50[3].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_3"));

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

	public static void BuildSplashEffect(Structure s) {
		//заготовка
	}
}
