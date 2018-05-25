using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType {NotAssigned, Plant, HarvestableResources, Structure, MainStructure}
public class Structure : MonoBehaviour {
	public SurfaceBlock basement{get;protected set;}
	public SurfaceRect innerPosition {get;protected set;}
	public bool borderOnlyConstruction{get;protected set;}
	public bool isArtificial {get;protected set;}
	public bool isBasement{get;protected set;}
	public bool undestructible = false;
	public StructureType type {get;protected set;}
	public float hp = 1;
	public float maxHp = 1;
	public int nameIndex = 0;
	public bool randomRotation = false, rotate90only = true;
	public bool showOnGUI{get; protected set;}
	public float gui_ypos = 0;
	public int id {get; protected set;}
	public byte level {get; protected set;}
	[SerializeField]
	protected Renderer myRenderer;
	public bool visible {get;protected set;}
	public const int TREE_SAPLING_ID = 1,  TREE_ID = 2, DEAD_TREE_ID = 3, WHEAT_CROP_ID = 4, LANDED_ZEPPELIN_ID = 5,
	TREE_OF_LIFE_ID = 6, STORAGE_ID = 7, CONTAINER_ID = 8, MINE_ELEVATOR_ID = 9, LIFESTONE_ID = 10, HOUSE_ID = 11, 
	DOCK_ID = 13, ENERGY_CAPACITOR_ID = 14, FARM_ID = 15, HQ_ID = 16, LUMBERMILL_ID = 17, MINE_ID = 18, SMELTERY_ID = 19, 
	WIND_GENERATOR_ID = 20, BIOGENERATOR_ID = 22, HOSPITAL_ID = 21, MINERAL_POWERPLANT_ID = 23, ORE_ENRICHER_ID = 24,
	ROLLING_SHOP_ID = 25, MINI_GRPH_REACTOR_ID = 26, FUEL_FACILITY_ID = 27, GRPH_REACTOR_ID = 28, PLASTICS_FACTORY_ID = 29,
	FOOD_FACTORY_ID = 30, GRPH_ENRICHER_ID = 31, XSTATION_ID = 32, QUANTUM_ENERGY_TRANSMITTER_ID = 33, CHEMICAL_FACTORY_ID = 34;

	void Awake() {
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
		case WIND_GENERATOR_ID:
		case ORE_ENRICHER_ID:
		case ROLLING_SHOP_ID:
		case FUEL_FACILITY_ID:
		case GRPH_REACTOR_ID:
		case GRPH_ENRICHER_ID:
		case XSTATION_ID:
		case QUANTUM_ENERGY_TRANSMITTER_ID:
		case CHEMICAL_FACTORY_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			break;		
		case DOCK_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			borderOnlyConstruction = true;
			break;		
		case STORAGE_ID:
			switch ( level ) {
			case 0:
			case 5: innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
			case 1: innerPosition = new SurfaceRect(0,0,4,4); type = StructureType.Structure; break;
			case 2:
			case 3: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure; break;
			}
			break;
		case CONTAINER_ID :	
			innerPosition = SurfaceRect.one; isArtificial = false; type = StructureType.HarvestableResources; 
			break;
		case MINE_ELEVATOR_ID: innerPosition = new SurfaceRect(0,0,4,4); isBasement = true; type = StructureType.Structure; break;
		case HOUSE_ID:
			switch ( level ) {
			case 0: innerPosition = SurfaceRect.one; type = StructureType.Structure; break;
			case 1: innerPosition = new SurfaceRect( 0, 0, 4,4); type = StructureType.Structure;break;
			case 2:
			case 3: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure; break;
			case 5: innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
			}
			break;
		case ENERGY_CAPACITOR_ID:
			switch (level) {
			case 1: innerPosition = new SurfaceRect (0,0, 2, 4); type = StructureType.Structure; break;
			case 2:
			case 3: innerPosition = new SurfaceRect (0,0,4,8);type = StructureType.Structure; break;
				break;
			}
			break;
		case FARM_ID:
		case LUMBERMILL_ID:
		case PLASTICS_FACTORY_ID:
		case FOOD_FACTORY_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			if ( level > 4) isBasement = true;
			break;
		case SMELTERY_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			if ( level > 4) {isBasement = true; borderOnlyConstruction = true;}
			break;
		case HQ_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			if ( level > 3) isBasement = true;
			break;
		case BIOGENERATOR_ID:
			innerPosition = new SurfaceRect(0,0,4,10); type = StructureType.Structure;
			break;
		case HOSPITAL_ID:
		case MINI_GRPH_REACTOR_ID:
			innerPosition = new SurfaceRect(0,0,8,8); type = StructureType.Structure;
			break;
		case MINERAL_POWERPLANT_ID:
			innerPosition = new SurfaceRect(0,0,10,10); type = StructureType.Structure;
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
		b.AddCellStructure(this, pos);
		if (isBasement) {
			if (basement.pos.y + 1 < Chunk.CHUNK_SIZE) {
				ChunkPos npos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
				Block upperBlock = basement.myChunk.GetBlock(npos.x, npos.y, npos.z);
				if ( upperBlock == null ) basement.myChunk.AddBlock(npos, BlockType.Surface, ResourceType.metal_K.ID);
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

	/// <summary>
	/// forced means that this object will be deleted without basement-linked actions
	/// </summary>
	/// <param name="forced">If set to <c>true</c> forced.</param>
	virtual public void Annihilate( bool forced ) { // for pooling
		if (forced) basement = null;
		Destroy(gameObject);
	}

	public void ApplyDamage(float d) {
		hp -= d;
		if ( hp <= 0 ) Annihilate(false);
	}

	public void ChunkUpdated() {
		if ( !isBasement || basement == null) return;
		Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y+1, basement.pos.z);
		if (upperBlock == null) {
			basement.myChunk.AddBlock( new ChunkPos(basement.pos.x, basement.pos.y+1, basement.pos.z), BlockType.Surface, ResourceType.CONCRETE_ID);
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

	public virtual string Save() {
		string data = string.Empty;
		return data;
	}

	public virtual void SetGUIVisible (bool x) {
		if (x != showOnGUI) {
			showOnGUI = x;
		}
	}

	public static Structure LoadStructure (int id, byte level) {
		Structure pref = null;
		switch (id) {
			case TREE_SAPLING_ID: pref = Resources.Load<Structure>("Lifeforms/TreeSapling"); break;
			case TREE_ID: pref = Resources.Load<Structure>("Lifeforms/Tree");break;
			case DEAD_TREE_ID: pref = Resources.Load<Structure>("Lifeforms/DeadTree");break;
			case WHEAT_CROP_ID: pref = Resources.Load<Structure>("Lifeforms/wheatCrop");break;
		case LANDED_ZEPPELIN_ID: pref = Resources.Load<Structure>("Structures/ZeppelinBasement");break;
		case TREE_OF_LIFE_ID: pref = Resources.Load<Structure>("Structures/Tree of Life");break;
		case STORAGE_ID:
			switch (level) {
			case 0: pref = Resources.Load<Structure>("Structures/Storage_level_0");break;
			case 1:pref = Resources.Load<Structure>("Structures/Buildings/Storage_level_1");break;
			case 2:pref = Resources.Load<Structure>("Structures/Buildings/Storage_level_2");break;
			case 3:pref = Resources.Load<Structure>("Structures/Buildings/Storage_level_3");break;
			}
			break;
		case CONTAINER_ID: pref = Resources.Load<Structure>("Structures/Container");break;
		case MINE_ELEVATOR_ID: pref = Resources.Load<Structure>("Structures/BMineElevator");break;
		case LIFESTONE_ID: pref = Resources.Load<Structure>("Structures/LifeStone");break;
		case HOUSE_ID:
			switch ( level ) {
			case 0: pref = Resources.Load<Structure>("Structures/House_level_0");break;
			case 1: pref = Resources.Load<Structure>("Structures/Buildings/House_level_1");break;
			case 2: pref = Resources.Load<Structure>("Structures/Buildings/House_level_2");break;
			case 3: pref = Resources.Load<Structure>("Structures/Buildings/House_level_3");break;
			case 5: pref = Resources.Load<Structure>("Structures/Blocks/houseBlock_level_5");break;
			}
			break;
		case DOCK_ID: pref = Resources.Load<Structure>("Structures/Buildings/Dock_level_1");break;
		case ENERGY_CAPACITOR_ID:
			switch ( level ) {
			case 1: pref = Resources.Load<Structure>("Structures/Buildings/EnergyCapacitor_level_1");break;
			case 2: pref = Resources.Load<Structure>("Structures/Buildings/EnergyCapacitor_level_2");break;
			case 3: pref = Resources.Load<Structure>("Structures/Buildings/EnergyCapacitor_level_3");break;
			}
			break;
		case FARM_ID:
			switch ( level ) {
			case 1: pref = Resources.Load<Structure>("Structures/Buildings/Farm_level_1");break;
			case 2: pref = Resources.Load<Structure>("Structures/Buildings/Farm_level_2");break;
			case 3: pref = Resources.Load<Structure>("Structures/Buildings/Farm_level_3");break;
			case 4: pref = Resources.Load<Structure>("Structures/Buildings/Farm_level_4");break;
			case 5: pref = Resources.Load<Structure>("Structures/Blocks/farmBlock_level_5");break;
			}
			break;
		case HQ_ID:
			switch (level) {
			case 2: pref = Resources.Load<Structure>("Structures/Buildings/HQ_level_2");break;
			case 3: pref = Resources.Load<Structure>("Structures/Buildings/HQ_level_3");break;
			case 4: pref = Resources.Load<Structure>("Structures/Blocks/HQ_level_4");break;
			break;
			}
			break;
		case LUMBERMILL_ID:
			switch ( level ) {
			case 1: pref = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_1");break;
			case 2: pref = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_2");break;
			case 3: pref = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_3");break;
			case 4: pref = Resources.Load<Structure>("Structures/Buildings/Lumbermill_level_4");break;
			case 5: pref = Resources.Load<Structure>("Structures/Blocks/lumbermillBlock_level_5");break;
			}
			break;
		case MINE_ID: pref = Resources.Load<Structure>("Structures/Buildings/Mine_level_1");break;
		case SMELTERY_ID:
			switch ( level ) {
			case 1: pref = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_1");break;
			case 2: pref = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_2");break;
			case 3: pref = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_3");break;
			case 4: pref = Resources.Load<Structure>("Structures/Buildings/Smeltery_level_4");break;
			case 5: pref = Resources.Load<Structure>("Structures/Blocks/smelteryBlock_level_5");break;
			}
			break;
		case WIND_GENERATOR_ID:  pref = Resources.Load<Structure>("Structures/Buildings/windGenerator_level_1");break;
		case BIOGENERATOR_ID:  pref = Resources.Load<Structure>("Structures/Buildings/Biogenerator_level_1");break;
		case HOSPITAL_ID:  pref = Resources.Load<Structure>("Structures/Buildings/Hospital_level_2");break;
		case MINERAL_POWERPLANT_ID:  pref = Resources.Load<Structure>("Structures/Buildings/mineralPP_level_2");break;
		case ORE_ENRICHER_ID:  pref = Resources.Load<Structure>("Structures/Buildings/oreEnricher_level_2");break;
		case ROLLING_SHOP_ID:  pref = Resources.Load<Structure>("Structures/Buildings/rollingShop_level_2");break;
		case MINI_GRPH_REACTOR_ID:pref = Resources.Load<Structure>("Structures/Buildings/miniReactor_level_3");break;
		case FUEL_FACILITY_ID:  pref = Resources.Load<Structure>("Structures/Buildings/fuelFacility_level_3");break;
		case GRPH_REACTOR_ID:  pref = Resources.Load<Structure>("Structures/Buildings/graphoniumReactor_level_4");break;	
		case PLASTICS_FACTORY_ID:  pref = Resources.Load<Structure>("Structures/Buildings/plasticsFactory_level_4");break;
		case FOOD_FACTORY_ID:  
			switch (level) {
			case 4: pref = Resources.Load<Structure>("Structures/Buildings/foodFactory_level_4");break;
			case 5: pref = Resources.Load<Structure>("Structures/Blocks/foodFactory_level_5");break;
			}
			break;
		case GRPH_ENRICHER_ID:  pref = Resources.Load<Structure>("Structures/Buildings/graphoniumEnricher_level_4");break;
		case XSTATION_ID:  pref = Resources.Load<Structure>("Structures/Buildings/XStation_level_3");break;
		case QUANTUM_ENERGY_TRANSMITTER_ID:  pref = Resources.Load<Structure>("Structures/Buildings/quantumEnergyTransmitter_level_4");break;
		case CHEMICAL_FACTORY_ID:  pref = Resources.Load<Structure>("Structures/Buildings/chemicalFactory_level_5");break;
		}
		if ( pref == null) print ("error: asset not loaded, id: "+id.ToString());
		else pref.Awake();
		return pref;
	}

	void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
		}
	}
}
