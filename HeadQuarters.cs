using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadQuartersSerializer {
	public BuildingSerializer buildingSerializer;
	public bool nextStageConditionMet;
	public byte level;
}

public class HeadQuarters : House {
	bool nextStageConditionMet = false;
	ColonyController colony;
	GameObject rooftop;
	
	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		if (level > 3 ) {
			if (rooftop == null) {
				rooftop = Instantiate(Resources.Load<GameObject>("Structures/HQ_rooftop"));
				rooftop.transform.parent = transform;
				rooftop.transform.localPosition = Vector3.up * (level - 3) * Block.QUAD_SIZE;
				myRenderers.Add(rooftop.transform.GetChild(0).GetComponent<MeshRenderer>());
			}
			if (level > 4) {
				int i = 5;
				while (i <= level) {
					b.myChunk.BlockByStructure( b.pos.x, (byte)(b.pos.y + i - 4), b.pos.z, this);
					GameObject addon = Instantiate(Resources.Load<GameObject>("Structures/HQ_Addon"));
					addon.transform.parent = transform;
					addon.transform.localPosition = Vector3.zero + (i - 3.5f) * Vector3.up * Block.QUAD_SIZE;
					addon.transform.localRotation = transform.localRotation;
					myRenderers.Add(addon.transform.GetChild(0).GetComponent<MeshRenderer>());
					i++;
				}
				BoxCollider bc = gameObject.GetComponent<BoxCollider>();
				bc.center = Vector3.up * (level - 3) * Block.QUAD_SIZE/2f;
				bc.size = new Vector3(Block.QUAD_SIZE, (level - 3) * Block.QUAD_SIZE, Block.QUAD_SIZE );
			}
		}
		colony = GameMaster.colonyController;
		colony.SetHQ(this);
	}

	void Update() {
		if ( showOnGUI && colony != null) {
			switch (level) {
			case 1:  
				nextStageConditionMet = (colony.docks.Count != 0); 
				break;
			case 2:  
				nextStageConditionMet = (colony.rollingShops.Count != 0);
				break;		
			case 3:
				nextStageConditionMet = (colony.graphoniumEnrichers.Count != 0);
				break;
			case 4:
				nextStageConditionMet = (colony.chemicalFactories.Count != 0);
				break;
			}
		}
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHeadQuartersSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load (StructureSerializer ss, SurfaceBlock sb) {
		HeadQuartersSerializer hqs = new HeadQuartersSerializer();
		GameMaster.DeserializeByteArray<HeadQuartersSerializer>(ss.specificData, ref hqs);
		level = hqs.level; 
		LoadStructureData(ss, sb);
		LoadBuildingData(hqs.buildingSerializer);
		nextStageConditionMet = hqs.nextStageConditionMet;
	} 
		

	protected HeadQuartersSerializer GetHeadQuartersSerializer() {
		HeadQuartersSerializer hqs = new HeadQuartersSerializer();
		hqs.level = level;
		hqs.nextStageConditionMet = nextStageConditionMet;
		hqs.buildingSerializer = GetBuildingSerializer();
		return hqs;
	}
	#endregion

	void OnGUI() {
		if ( !showOnGUI ) return;
		if (level < 7) {
				Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
				if (nextStageConditionMet) {
					GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
					if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
						if (level < 4) {
								if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
								{
									Building upgraded = Structure.GetNewStructure(upgradedIndex) as Building;
									upgraded.SetBasement(basement, PixelPosByte.zero);
								}
								else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
							}
							else { // building blocks on the top
								if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
								{
									Chunk chunk = basement.myChunk;
									ChunkPos upperPos = new ChunkPos( basement.pos.x, basement.pos.y + (level - 3), basement.pos.z);
									Block upperBlock = chunk.GetBlock(upperPos.x, upperPos.y, upperPos.z);
									if (upperBlock != null) {
									chunk.BlockByStructure(upperPos.x, upperPos.y, upperPos.z, this);
									}
									GameObject addon = Instantiate(Resources.Load<GameObject>("Structures/HQ_Addon"));
									addon.transform.parent = transform;
									addon.transform.localPosition = Vector3.zero + (level - 2.5f) * Vector3.up * Block.QUAD_SIZE;
									addon.transform.localRotation = transform.localRotation;
									myRenderers.Add(addon.transform.GetChild(0).GetComponent<MeshRenderer>());
									BoxCollider bc = gameObject.GetComponent<BoxCollider>();
									bc.size = new Vector3(Block.QUAD_SIZE, (level - 3) * Block.QUAD_SIZE, Block.QUAD_SIZE);
									bc.center = Vector3.up * (level - 3) * Block.QUAD_SIZE/2f;
									if (rooftop== null) {
										rooftop = Instantiate(Resources.Load<GameObject>("Structures/HQ_rooftop"));
										rooftop.transform.parent = transform;
									}
									rooftop.transform.localPosition = Vector3.up * (level - 2) * Block.QUAD_SIZE;
								level++; Rename();
							}
							else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
						}
				}
				if (level > 3) {
					rr.y += rr.height;
					Color c = GUI.color;
					GUI.color = Color.red;
					GUI.Label(rr, Localization.hq_upgrade_warning);
					rr.y += rr.height;
					GUI.color = c;
					GUI.Label(rr, Localization.info_level + " : " + level.ToString());
					rr.y += rr.height;
				}
			}
				else {
					Color c = GUI.color;
					GUI.color = Color.red;
					switch (level) {
					case 1: GUI.Label(rr, Localization.hq_refuse_reason_1, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 2: GUI.Label(rr, Localization.hq_refuse_reason_2, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 3: GUI.Label(rr, Localization.hq_refuse_reason_3, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 4: GUI.Label(rr, Localization.hq_refuse_reason_4, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 5: GUI.Label(rr, Localization.hq_refuse_reason_5, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 6: GUI.Label(rr, Localization.hq_refuse_reason_6, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					}
					GUI.color = c;
				}
				rr.y += rr.height;
			if ( requiredResources.Length > 0) {
				Storage storage = GameMaster.colonyController.storage;
				for (int i = 0; i < requiredResources.Length; i++) {
					if (requiredResources[i].volume > storage.standartResources[requiredResources[i].type.ID]) GUI.color = Color.red;
					GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
					GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), requiredResources[i].volume.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
						rr.y += rr.height;
					GUI.color = Color.white;
					}
				}
			}
	}
}
