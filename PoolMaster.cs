using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	public static PoolMaster current;
	public Material[] grassland_ready_25, grassland_ready_50;
	public Material dryingLeaves_material, leaves_material;
	GameObject tree_pref, sapling_pref, deadTree_pref, lightPassengerShip_pref, lightCargoShip_pref, lightWarship_pref, privateShip_pref;
	public static GameObject quad_pref {get;private set;}
	public static Material	default_material, lr_red_material, lr_green_material, grass_material;
	public static Mesh plane_excavated_025, plane_excavated_05,plane_excavated_075;
	public static Texture twoButtonsDivider_tx, plusButton_tx, minusButton_tx, plusX10Button_tx, minusX10Button_tx, quadSelector_tx,  orangeSquare_tx;
	public static GUIStyle GUIStyle_RightOrientedLabel, GUIStyle_BorderlessButton, GUIStyle_BorderlessLabel, GUIStyle_CenterOrientedLabel, GUIStyle_SystemAlert;

	List<GameObject> treesPool, grassPool, lightPassengerShips, mediumPassengerShips, heavyPassengerShips, lightCargoShips, mediumCargoShips, heavyCargoShips,
		lightWarships, mediumWarships, heavyWarships, privateShips;// только неактивные
	const byte MEDIUM_SHIP_LVL = 4, HEAVY_SHIP_LVL = 6;
	const int SHIPS_BUFFER_SIZE = 5;
	public int treesPoolCount, grassPoolCount;
	public int treesPool_buffer_size =16, grassPool_buffer_size = 16;
	float treeClearTimer = 0, grassClearTimer = 0, shipsClearTimer = 0,clearTime = 30;

	void Awake() {
		if (current != null && current != this) Destroy(current); 
		current = this;

		lightPassengerShip_pref = Resources.Load<GameObject>("Prefs/lightPassengerShip");
		lightCargoShip_pref = Resources.Load<GameObject>("Prefs/lightCargoShip");
		lightWarship_pref = Resources.Load<GameObject>("Prefs/lightWarship");
		privateShip_pref = Resources.Load<GameObject>("Prefs/privateShip");
		lightPassengerShips = new List<GameObject>();
		lightCargoShips = new List<GameObject>();
		lightWarships = new List<GameObject>();
		privateShips = new List<GameObject>();

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

		dryingLeaves_material = Resources.Load<Material>("Materials/DryingLeaves");
		leaves_material = Resources.Load<Material>("Materials/Leaves");
		grass_material = Resources.Load<Material>("Materials/Grass");

		deadTree_pref = Resources.Load<GameObject>("Lifeforms/DeadTree");deadTree_pref.SetActive(false);
		tree_pref = Resources.Load<GameObject>("Lifeforms/Tree");tree_pref.SetActive(false);
		treesPool = new List<GameObject>();
		sapling_pref = Resources.Load<GameObject>("Lifeforms/TreeSapling"); sapling_pref.SetActive(false);
		grassPool = new List<GameObject>();

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
	}

	void Update() {
		if (treesPool.Count > treesPool_buffer_size) {
			treeClearTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (treeClearTimer <= 0) {
				GameObject tree = treesPool[treesPool.Count - 1];
				treesPool.RemoveAt(treesPool.Count - 1);
				Destroy(tree);
				if (treesPool.Count > treesPool_buffer_size) treeClearTimer = clearTime;
				else treeClearTimer = 0;
			}
		}
		if (grassPool.Count > grassPool_buffer_size) {
			grassClearTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (grassClearTimer <= 0) {
				GameObject grass= grassPool[grassPool.Count - 1];
				grassPool.RemoveAt(grassPool.Count - 1);
				Destroy(grass);
				if (grassPool.Count > grassPool_buffer_size) grassClearTimer = clearTime;
				else grassClearTimer = 0;
			}
		}
		treesPoolCount = treesPool.Count; grassPoolCount = grassPoolCount;

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

	public Tree GetTree() {
		GameObject tree = null;
		if (treesPool.Count == 0)	tree = Instantiate(tree_pref);
		else {
			if (treesPool[0] == null) tree = Instantiate(tree_pref);
			else {tree = treesPool[0]; treeClearTimer = clearTime;}
			treesPool.RemoveAt(0);
			if (tree.GetComponent<Tree>() == null) tree.AddComponent<Tree>();
		}
		return tree.GetComponent<Tree>();
	}
	public void ReturnTreeToPool(Tree t) {
		if (t == null) return;
		if (t.basement != null) t.UnsetBasement();
		t.hp = t.maxHp;
		t.SetLifepower(0);
		t.transform.parent = transform;
		if (t.GetComponent<FallingTree>() != null) {
			Destroy(t.GetComponent<FallingTree>());
			t.transform.localRotation = Quaternion.Euler(0,Random.value *  360,0);
		}
		t.gameObject.SetActive(false);
		treesPool.Add(t.gameObject);
	}
	public void ReturnTreeToPool(GameObject g) {
		if ( g == null) return;
		if (g.GetComponent<FallingTree>() != null) {
			Destroy(g.GetComponent<FallingTree>());
			g.transform.localRotation = Quaternion.Euler(0,Random.value *  360,0);
		}
		treeClearTimer = clearTime;
		g.transform.parent = null;
		g.SetActive(false);
		treesPool.Add(g);
	}
	public TreeSapling GetSapling() {
		GameObject grass = null;
		if (grassPool.Count == 0) grass = Instantiate(sapling_pref);
		else {
			if (grassPool[0] == null) {
				grass = Instantiate(sapling_pref);
			}
			else {
				grass = grassPool[0];			
				grassClearTimer = clearTime;
			}
			grassPool.RemoveAt(0);
		}
		return grass.GetComponent<TreeSapling>();
	}
	public void ReturnGrassToPool(TreeSapling grass) {
		if (grass == null) return;
		if (grass.basement != null) grass.UnsetBasement();
		grass.hp = grass.maxHp;
		grass.SetLifepower(0);
		grass.transform.parent = transform;
		grass.gameObject.SetActive(false);
		grassPool.Add(grass.gameObject);
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
}
