﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType {NotAssigned, Plant, HarvestableResources, Structure, MainStructure}
public class Structure : MonoBehaviour {
	public SurfaceBlock basement{get;protected set;}
	public SurfaceRect innerPosition;
	public bool borderOnlyConstruction{get;protected set;}
	public bool isArtificial {get;protected set;}
	public bool isBasement{get;protected set;}
	public bool undestructible = false;
	public StructureType type {get;protected set;}
	public float hp = 1;
	public float maxHp = 1;
	public bool randomRotation = false, rotate90only = true;
	public bool showOnGUI{get; protected set;}
	public float gui_ypos = 0;
	public int id {get; private set;}
	[SerializeField]
	protected Renderer myRenderer;
	public bool visible {get;protected set;}
	public const int TREE_SAPLING_ID = 1,  TREE_ID = 2, DEAD_TREE_ID = 3, WHEAT_CROP_ID = 4, LANDED_ZEPPELIN_ID = 5,
	TREE_OF_LIFE_ID = 6, STORAGE_0_ID = 7, CONTAINER_ID = 8, MINE_ELEVATOR_ID = 9, LIFESTONE_ID = 10, HOUSE_0_ID = 11, 
	DOCK_ID = 13, ENERGY_CAPACITOR_1_ID = 14, FARM_1_ID = 15, HQ_2_ID = 16, LUMBERMILL_1_ID = 17, MINE_ID = 18, SMELTERY_1_ID = 19, 
	WIND_GENERATOR_1_ID = 20, BIOGENERATOR_2_ID = 22, HOSPITAL_2_ID = 21, MINERAL_POWERPLANT_2_ID = 23, ORE_ENRICHER_2_ID = 24,
	ROLLING_SHOP_ID = 25, MINI_GRPH_REACTOR_ID = 26, FUEL_FACILITY_3_ID = 27, GRPH_REACTOR_4_ID = 28, PLASTICS_FACTORY_4_ID = 29,
	FOOD_FACTORY_4_ID = 30, GRPH_ENRICHER_ID = 31, XSTATION_ID = 32, QUANTUM_ENERGY_TRANSMITTER_ID = 33, CHEMICAL_FACTORY_ID = 34,
	STORAGE_1_ID = 35, STORAGE_2_ID = 36, STORAGE_3_ID = 37, STORAGE_5_ID = 38, HOUSE_1_ID = 39, HOUSE_2_ID = 40, HOUSE_3_ID = 41, 
	HOUSE_5_ID = 42, ENERGY_CAPACITOR_2_ID = 43, ENERGY_CAPACITOR_3_ID = 44, FARM_2_ID = 45, FARM_3_ID = 46, FARM_4_ID = 47, FARM_5_ID = 48,
	LUMBERMILL_2_ID = 49, LUMBERMILL_3_ID = 50, LUMBERMILL_4_ID = 51, LUMBERMILL_5_ID = 52, FOOD_FACTORY_5_ID = 53, SMELTERY_2_ID = 54, 
	SMELTERY_3_ID = 55,  SMELTERY_5_ID = 57, HQ_3_ID = 58, HQ_4_ID = 59, RESOURCE_STICK_ID = 60, COLUMN_ID = 61, SWITCH_TOWER_ID = 62, SHUTTLE_HANGAR_ID = 63,
	RECRUITING_CENTER_ID = 64, EXPEDITION_CORPUS_ID = 65, QUANTUM_TRANSMITTER_ID = 66;
	public const int TOTAL_STRUCTURES_COUNT = 67;
	static Structure[] prefs;
	static List<Building> allConstructableBuildingsList;

	public static void LoadPrefs() {
		prefs = new Structure[TOTAL_STRUCTURES_COUNT];
		prefs[TREE_SAPLING_ID] = Resources.Load<Structure>("Lifeforms/TreeSapling");
		prefs[TREE_ID] = Resources.Load<Structure>("Lifeforms/Tree");
		prefs[DEAD_TREE_ID] = Resources.Load<Structure>("Lifeforms/DeadTree");
		prefs[WHEAT_CROP_ID] = Resources.Load<Structure>("Lifeforms/wheatCrop");
		prefs[LANDED_ZEPPELIN_ID] = Resources.Load<Structure>("Structures/ZeppelinBasement");
		prefs[TREE_OF_LIFE_ID] = Resources.Load<Structure>("Structures/Tree of Life");
		prefs[STORAGE_0_ID] = Resources.Load<Structure>("Structures/Storage_level_0");
		prefs[STORAGE_1_ID] = Resources.Load<Structure>("Structures/Buildings/Storage_level_1"); 
		prefs[STORAGE_2_ID] =  Resources.Load<Structure>("Structures/Buildings/Storage_level_2");
		prefs[STORAGE_3_ID] = Resources.Load<Structure>("Structures/Buildings/Storage_level_3");
		prefs[STORAGE_5_ID] = Resources.Load<Structure>("Structures/Blocks/storageBlock_level_5");
		prefs[CONTAINER_ID] = Resources.Load<Structure>("Structures/Container");
		prefs[MINE_ELEVATOR_ID] = Resources.Load<Structure>("Structures/MineElevator");
		prefs[LIFESTONE_ID] = Resources.Load<Structure>("Structures/LifeStone");
		prefs[HOUSE_0_ID] = Resources.Load<Structure>("Structures/House_level_0");
		prefs[HOUSE_1_ID] = Resources.Load<Structure>("Structures/Buildings/House_level_1");
		prefs[HOUSE_2_ID] = Resources.Load<Structure>("Structures/Buildings/House_level_2");
		prefs[HOUSE_3_ID] = Resources.Load<Structure>("Structures/Buildings/House_level_3");
		prefs[HOUSE_5_ID] = Resources.Load<Structure>("Structures/Blocks/houseBlock_level_5");
		prefs[DOCK_ID] = Resources.Load<Structure>("Structures/Buildings/Dock_level_1");
		prefs[ENERGY_CAPACITOR_1_ID] = Resources.Load<Structure>("Structures/Buildings/EnergyCapacitor_level_1");
		prefs[ENERGY_CAPACITOR_2_ID] = Resources.Load<Structure>("Structures/Buildings/EnergyCapacitor_level_2");
		prefs[ENERGY_CAPACITOR_3_ID] = Resources.Load<Structure>("Structures/Buildings/EnergyCapacitor_level_3");
		prefs[FARM_1_ID] = Resources.Load<Structure>("Structures/Buildings/Farm_level_1");
		prefs[FARM_2_ID] = Resources.Load<Structure>("Structures/Buildings/Farm_level_2");
		prefs[FARM_3_ID] = Resources.Load<Structure>("Structures/Buildings/Farm_level_3");
		prefs[FARM_4_ID] = Resources.Load<Structure>("Structures/Buildings/Farm_level_4");
		prefs[FARM_5_ID] = Resources.Load<Structure>("Structures/Blocks/farmBlock_level_5");
		prefs[HQ_2_ID] = Resources.Load<Structure>("Structures/Buildings/HQ_level_2");
		prefs[HQ_3_ID] = Resources.Load<Structure>("Structures/Buildings/HQ_level_3");
		prefs[HQ_4_ID] = Resources.Load<Structure>("Structures/Blocks/HQ_level_4");
		prefs[LUMBERMILL_1_ID]= Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_1");
		prefs[LUMBERMILL_2_ID] = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_2");
		prefs[LUMBERMILL_3_ID] = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_3");
		prefs[LUMBERMILL_4_ID] = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_4");
		prefs[LUMBERMILL_5_ID] = Resources.Load<Structure>("Structures/Blocks/lumbermillBlock_level_5");
		prefs[MINE_ID] = Resources.Load<Structure>("Structures/Buildings/Mine_level_1");
		prefs[SMELTERY_1_ID] = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_1");
		prefs[SMELTERY_2_ID] = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_2");
		prefs[SMELTERY_3_ID] = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_3");
		prefs[SMELTERY_5_ID] = Resources.Load<Structure>("Structures/Blocks/smelteryBlock_level_5");
		prefs[WIND_GENERATOR_1_ID] = Resources.Load<Structure>("Structures/Buildings/windGenerator_level_1");
		prefs[BIOGENERATOR_2_ID] = Resources.Load<Structure>("Structures/Buildings/Biogenerator_level_2");
		prefs[HOSPITAL_2_ID] = Resources.Load<Structure>("Structures/Buildings/Hospital_level_2");
		prefs[MINERAL_POWERPLANT_2_ID] = Resources.Load<Structure>("Structures/Buildings/mineralPP_level_2");
		prefs[ORE_ENRICHER_2_ID] = Resources.Load<Structure>("Structures/Buildings/oreEnricher_level_2");
		prefs[ROLLING_SHOP_ID] = Resources.Load<Structure>("Structures/Buildings/rollingShop_level_2");
		prefs[MINI_GRPH_REACTOR_ID]  = Resources.Load<Structure>("Structures/Buildings/miniReactor_level_3");
		prefs[FUEL_FACILITY_3_ID] = Resources.Load<Structure>("Structures/Buildings/fuelFacility_level_3");
		prefs[GRPH_REACTOR_4_ID] = Resources.Load<Structure>("Structures/Buildings/graphoniumReactor_level_4");
		prefs[PLASTICS_FACTORY_4_ID] = Resources.Load<Structure>("Structures/Buildings/plasticsFactory_level_4");
		prefs[FOOD_FACTORY_4_ID] = Resources.Load<Structure>("Structures/Buildings/foodFactory_level_4");
		prefs[FOOD_FACTORY_5_ID] = Resources.Load<Structure>("Structures/Blocks/foodFactoryBlock_level_5");
		prefs[GRPH_ENRICHER_ID] = Resources.Load<Structure>("Structures/Buildings/graphoniumEnricher_level_3");
		prefs[XSTATION_ID] = Resources.Load<Structure>("Structures/Buildings/XStation_level_3");
		prefs[QUANTUM_ENERGY_TRANSMITTER_ID] = Resources.Load<Structure>("Structures/Buildings/quantumEnergyTransmitter_level_4");
		prefs[CHEMICAL_FACTORY_ID] = Resources.Load<Structure>("Structures/Buildings/chemicalFactory_level_4");
		prefs[RESOURCE_STICK_ID] = Resources.Load<Structure>("Structures/resourceStick");
		prefs[COLUMN_ID] = Resources.Load<Structure>("Structures/Column");
		prefs[SWITCH_TOWER_ID] = Resources.Load<Structure>("Structures/Buildings/switchTower");
		prefs[SHUTTLE_HANGAR_ID] = Resources.Load<Structure>("Structures/Buildings/shuttleHangar");
		prefs[RECRUITING_CENTER_ID] = Resources.Load<Structure>("Structures/Buildings/recruitingCenter");
		prefs[EXPEDITION_CORPUS_ID] = Resources.Load<Structure>("Structures/Buildings/expeditionCorpus");
		prefs[QUANTUM_TRANSMITTER_ID] = Resources.Load<Structure>("Structures/Buildings/quantumTransmitter");
	
		allConstructableBuildingsList = new List<Building>();
		allConstructableBuildingsList.Add( GetNewStructure(WIND_GENERATOR_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(STORAGE_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(HOUSE_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FARM_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(LUMBERMILL_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(SMELTERY_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(ENERGY_CAPACITOR_1_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(MINE_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(DOCK_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);

		allConstructableBuildingsList.Add( GetNewStructure(STORAGE_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(HOUSE_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FARM_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(LUMBERMILL_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(SMELTERY_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(ENERGY_CAPACITOR_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(ORE_ENRICHER_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(BIOGENERATOR_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(MINERAL_POWERPLANT_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(HOSPITAL_2_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);	
		allConstructableBuildingsList.Add( GetNewStructure(ROLLING_SHOP_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);

		allConstructableBuildingsList.Add( GetNewStructure(STORAGE_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(HOUSE_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FARM_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(LUMBERMILL_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(SMELTERY_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(ENERGY_CAPACITOR_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(MINI_GRPH_REACTOR_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FUEL_FACILITY_3_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(XSTATION_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(GRPH_ENRICHER_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);

		allConstructableBuildingsList.Add( GetNewStructure(FARM_4_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(LUMBERMILL_4_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FOOD_FACTORY_4_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(PLASTICS_FACTORY_4_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(GRPH_REACTOR_4_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add (GetNewStructure(SHUTTLE_HANGAR_ID) as Building); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add (GetNewStructure(RECRUITING_CENTER_ID) as Building); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(CHEMICAL_FACTORY_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add(GetNewStructure(EXPEDITION_CORPUS_ID) as Building); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add(GetNewStructure(QUANTUM_TRANSMITTER_ID) as Building); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);

		allConstructableBuildingsList.Add( GetNewStructure(STORAGE_5_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(HOUSE_5_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FARM_5_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(LUMBERMILL_5_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(SMELTERY_5_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(FOOD_FACTORY_5_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add( GetNewStructure(QUANTUM_ENERGY_TRANSMITTER_ID) as Building ); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		allConstructableBuildingsList.Add(GetNewStructure(SWITCH_TOWER_ID) as Building); allConstructableBuildingsList[allConstructableBuildingsList.Count - 1].gameObject.SetActive(false);
		foreach (Building b in allConstructableBuildingsList) {
			b.transform.parent = GameMaster.realMaster.transform;
		}
	}

	public static Structure GetNewStructure(int s_id) {
		Structure s = prefs[s_id];
		if (s == null) return null;
		s = Instantiate(s);
		if ( !s.gameObject.activeSelf ) s.gameObject.SetActive(true);
		s.id = s_id;
		s.SetVisibility( false );
		s.Prepare();
		return s;
	}

	virtual public void Prepare() {
		PrepareStructure();			
	}
	protected void PrepareStructure() {
		hp = maxHp;
		isBasement = false; isArtificial = true; borderOnlyConstruction = false;
		switch ( id ) {
		case WHEAT_CROP_ID:
		case TREE_SAPLING_ID:
		case TREE_ID: 
			innerPosition = SurfaceRect.one; isArtificial = false; type = StructureType.Plant;
			break;		
		case DEAD_TREE_ID: innerPosition = SurfaceRect.one;isArtificial = false; type = StructureType.Structure; 
			break;
		case LIFESTONE_ID:
		case TREE_OF_LIFE_ID:
		case LANDED_ZEPPELIN_ID: 
		case MINE_ID:
		case WIND_GENERATOR_1_ID:
		case ORE_ENRICHER_2_ID:
		case ROLLING_SHOP_ID:
		case FUEL_FACILITY_3_ID:
		case GRPH_REACTOR_4_ID:
		case GRPH_ENRICHER_ID:
		case XSTATION_ID:
		case QUANTUM_ENERGY_TRANSMITTER_ID:
		case CHEMICAL_FACTORY_ID:
		case MINI_GRPH_REACTOR_ID:
		case EXPEDITION_CORPUS_ID:
		case QUANTUM_TRANSMITTER_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			break;		
		case DOCK_ID:
		case SHUTTLE_HANGAR_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			borderOnlyConstruction = true;
			break;		
		case RECRUITING_CENTER_ID:
		case STORAGE_0_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure; break;
		case STORAGE_1_ID:
		case SWITCH_TOWER_ID:
			innerPosition = new SurfaceRect(0,0,4,4); type = StructureType.Structure; break;
		case STORAGE_2_ID:
		case STORAGE_3_ID:
			innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure; break;
		case STORAGE_5_ID: 
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
		case CONTAINER_ID :	
			innerPosition = SurfaceRect.one; isArtificial = false; type = StructureType.HarvestableResources; 
			break;
		case MINE_ELEVATOR_ID: innerPosition = new SurfaceRect(0,0,4,4); isBasement = true; type = StructureType.Structure; break;
		case HOUSE_0_ID:		innerPosition = SurfaceRect.one; type = StructureType.Structure; break;
		case HOUSE_1_ID: innerPosition = new SurfaceRect( 0, 0, 4,4); type = StructureType.Structure;break;
		case HOUSE_2_ID:
		case HOUSE_3_ID: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure; break;
		case HOUSE_5_ID: innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
		case ENERGY_CAPACITOR_1_ID: innerPosition = new SurfaceRect (0,0, 2, 4); type = StructureType.Structure; break;
		case ENERGY_CAPACITOR_2_ID:
		case ENERGY_CAPACITOR_3_ID: innerPosition = new SurfaceRect (0,0,4,8);type = StructureType.Structure; break;
		case FARM_1_ID: innerPosition = new SurfaceRect(0,0,4,4); type = StructureType.MainStructure; break;
		case FARM_2_ID: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.MainStructure; break;
		case FARM_3_ID: innerPosition = new SurfaceRect(0,0,8,8); type = StructureType.MainStructure; break;
		case FARM_4_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			break;
		case FARM_5_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true;
			break;
		case LUMBERMILL_1_ID:
		case LUMBERMILL_2_ID:
		case LUMBERMILL_3_ID:
			innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.MainStructure;
			break;
		case LUMBERMILL_4_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			break;
		case LUMBERMILL_5_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true;
			break;
		case PLASTICS_FACTORY_4_ID:	innerPosition = SurfaceRect.full; type = StructureType.MainStructure;	break;
		case FOOD_FACTORY_4_ID:		innerPosition = SurfaceRect.full; type = StructureType.MainStructure;	break;
		case FOOD_FACTORY_5_ID: innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
		case SMELTERY_1_ID:
		case SMELTERY_2_ID:
		case SMELTERY_3_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;	break;
		case SMELTERY_5_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			isBasement = true; borderOnlyConstruction = true;
		break;
		case HQ_2_ID:
		case HQ_3_ID: 
		case HQ_4_ID: 
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;	break;
		case BIOGENERATOR_2_ID:
			innerPosition = new SurfaceRect(0,0,4,10); type = StructureType.Structure;
			break;
		case HOSPITAL_2_ID:
			innerPosition = new SurfaceRect(0,0,8,8); type = StructureType.Structure;
			break;
		case MINERAL_POWERPLANT_2_ID:
			innerPosition = new SurfaceRect(0,0,10,10); type = StructureType.Structure;
			break;
		case RESOURCE_STICK_ID:
			innerPosition = new SurfaceRect(0,0,2,2); type = StructureType.Structure;
			break;		
		case COLUMN_ID:
			innerPosition = new SurfaceRect(0,0,2,2); type = StructureType.Structure;isBasement = true;
			break;		
		}
		visible = true;
	}

	virtual public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
		if ( isBasement ) basement.myChunk.chunkUpdateSubscribers.Add(this);
	}
	protected void SetStructureData(SurfaceBlock b, PixelPosByte pos) {
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.x_size, innerPosition.z_size);
		b.AddStructure(this);
		if (isBasement) {
			if (basement.pos.y + 1 < Chunk.CHUNK_SIZE) {
				ChunkPos npos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
				Block upperBlock = basement.myChunk.GetBlock(npos.x, npos.y, npos.z);
				if ( upperBlock == null ) basement.myChunk.AddBlock(npos, BlockType.Surface, ResourceType.CONCRETE_ID, false);
			}
		}
	} 

	public void UnsetBasement() {
		if ( isBasement ) {
			int i = 0;
			List <Component> clist = basement.myChunk.chunkUpdateSubscribers;
			while ( i < clist.Count ) {
				if (clist[i] == this) {
					clist.RemoveAt(i);
					continue;
				}
				i++;
			}
		}
		basement = null;
		innerPosition = new SurfaceRect(0,0,innerPosition.x_size, innerPosition.z_size);
		transform.parent = null;
	}


	public void ApplyDamage(float d) {
		hp -= d;
		if ( hp <= 0 ) Annihilate(false);
	}

	public void ChunkUpdated() {
		if ( !isBasement || basement == null) return;
		Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y+1, basement.pos.z);
		if (upperBlock == null) {
			basement.myChunk.AddBlock( new ChunkPos(basement.pos.x, basement.pos.y+1, basement.pos.z), BlockType.Surface, ResourceType.CONCRETE_ID, false);
		}
	}

	virtual public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			myRenderer.enabled = x;
			Collider c = gameObject.GetComponent<Collider>();
			if ( c != null ) c.enabled = x;
		}
	}

	//--------SAVE  SYSTEM-------------
	public virtual string Save() {
		return SaveStructureData();
	}
		
	protected string SaveStructureData() {
		string s = string.Format("{0:d2}", innerPosition.x) + string.Format("{0:d2}", innerPosition.z);
		s += string.Format("{0:d3}", id) ; 
		s +=  string.Format("{0:d1}", (int)(transform.localRotation.eulerAngles.y / 45)) ;
		s += string.Format( "{0:d3}", (int)(hp / maxHp * 100));
		return s;
	}

	public virtual void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	// ------------------------------------------

	public virtual void SetGUIVisible (bool x) {
		if (x != showOnGUI) {
			showOnGUI = x;
		}
	}

	public static List<Building> GetApplicableBuildingsList(byte s_level, SurfaceBlock sblock) {
		List<Building> buildingsList = new List<Building>();
		foreach (Building b in allConstructableBuildingsList) {
			if (b == null ) continue;
			else {
				if (b.level == s_level)	buildingsList.Add(b);
				else {if (b.level > s_level) break;}
			}
		}
		return buildingsList;
	}

	/// <summary>
	/// forced means that this object will be deleted without basement-linked actions
	/// </summary>
	/// <param name="forced">If set to <c>true</c> forced.</param>
	virtual public void Annihilate( bool forced ) { // for pooling
		SurfaceBlock lastBasement = basement;
		if (forced) UnsetBasement();
		if (isBasement) {
			Block ub = lastBasement.myChunk.GetBlock(lastBasement.pos.x , lastBasement.pos.y+1, lastBasement.pos.z);
			if ( ub != null ) {
				if ( lastBasement.myChunk.CalculateSupportPoints(lastBasement.pos.x, lastBasement.pos.y, lastBasement.pos.z) < 1 )	{
					lastBasement.myChunk.DeleteBlock(ub.pos);

				}
				else lastBasement.myChunk.ReplaceBlock(lastBasement.pos, BlockType.Cave, lastBasement.material_id, ub.material_id, false);
			}
		}
		Destroy(gameObject);
	}

	void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
	}
}
