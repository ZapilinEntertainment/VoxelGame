using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SurfacePanelMode {SelectAction, Build}
enum BuildingCreateInfoMode {Acceptable, Unacceptable_SideBlock, Unacceptable_Material}

public sealed class UISurfacePanelController : UIObserver {
	public Button buildButton, gatherButton, digButton, blockCreateButton, columnCreateButton, changeMaterialButton;
	SurfaceBlock surface;
	bool status_gatherEnabled = true, status_gatherOrdered = false, status_digOrdered = false;
	byte savedHqLevel = 0;
	Vector2[] showingResourcesCount; 
	int selectedBuildingButton = -1;
	Structure chosenBuilding = null;
	public byte constructingLevel = 1;
	SurfacePanelMode mode;
	BuildingCreateInfoMode buildingCreateMode;
	public Toggle[] buildingsLevelToggles; // fiti
	public Button[] availableBuildingsButtons; // fiti
	public Text nameField, description, gridTextField,energyTextField, housingTextField; // fiti
	public RawImage[] resourcesCostImage; // fiti
	public Button innerBuildButton, returnButton; // fiti
	public Sprite overridingSprite; // fiti
	public RectTransform buildZone; // fiti

	public Toggle surfaceGridToggle; // fiti
	[SerializeField] GameObject surfaceBuildingPanel, infoPanel, energyIcon, housingIcon; // fiti
	HeadQuarters hq;



	void Start() {
		returnButton.onClick.AddListener(() => {
			this.ChangeMode(SurfacePanelMode.SelectAction);
		});
		showingResourcesCount = new Vector2[resourcesCostImage.Length];
	}

	public void SetObservingSurface(SurfaceBlock sb) {
		if (sb == null) {
			SelfShutOff();
			return;
		}
		else {
			hq = GameMaster.colonyController.hq;
			surface = sb;
			isObserving = true;
			ChangeMode (SurfacePanelMode.SelectAction);
				
			STATUS_UPDATE_TIME = 1;
			timer = STATUS_UPDATE_TIME;
		}
	}

	protected override void StatusUpdate() {
		if ( surface == null) {
			SelfShutOff();
			return;
		}
		switch ( mode) {
		case SurfacePanelMode.SelectAction:
			hq = GameMaster.colonyController.hq;
			bool check = false;
			check = surface.cellsStatus != 1;
			if (status_gatherEnabled != check) {
				status_gatherEnabled = check;
				if (status_gatherEnabled) {
					check = surface.GetComponent<GatherSite>();
					if (check != status_gatherOrdered) {
						status_gatherOrdered = check;
						gatherButton.transform.GetChild(0).GetComponent<Text>().text = (status_gatherOrdered ? Localization.GetPhrase(LocalizedPhrase.StopGather) : Localization.GetWord(LocalizedWord.Gather));
                        }
				}
				else {
					gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord( LocalizedWord.Gather );
					gatherButton.interactable = status_gatherEnabled;
				}
			}
			else {
				check = surface.GetComponent<GatherSite>();
				if (check != status_gatherOrdered) {
					status_gatherOrdered = check;
					gatherButton.transform.GetChild(0).GetComponent<Text>().text = (status_gatherOrdered ? Localization.GetPhrase(  LocalizedPhrase.StopGather) : Localization.GetWord( LocalizedWord.Gather) );
				}
			}
			check = (surface.GetComponent<CleanSite>() != null && surface.GetComponent<CleanSite>().diggingMission);
			if (status_digOrdered != check) {
				status_digOrdered = check;
				digButton.transform.GetChild(0).GetComponent<Text>().text = (status_digOrdered == true ? Localization.GetPhrase(  LocalizedPhrase.StopDig) : Localization.GetWord(LocalizedWord.Dig));
			}
			if (savedHqLevel != hq.level) {
				savedHqLevel = hq.level;
				blockCreateButton.enabled = ( savedHqLevel> 3);
				columnCreateButton.enabled = (savedHqLevel > 4 && surface.pos.y < Chunk.CHUNK_SIZE - 1);
			}
			changeMaterialButton.enabled = (GameMaster.colonyController.gears_coefficient >=2);
			break;

		case SurfacePanelMode.Build:
			if (chosenBuilding != null) {
				switch (buildingCreateMode) {
				case BuildingCreateInfoMode.Acceptable:
					float[] onStorage = GameMaster.colonyController.storage.standartResources;
					for (int i = 0; i < resourcesCostImage.Length; i++) {
						if (onStorage[i] != showingResourcesCount[i].y) {
							int rid = (int)showingResourcesCount[i].x;
							resourcesCostImage[i].transform.GetChild(0).GetComponent<Text>().color = onStorage[rid] < showingResourcesCount[i].y ? Color.red : Color.white;
							showingResourcesCount[i].y = onStorage[rid];
						}
					}
					break;
				case BuildingCreateInfoMode.Unacceptable_Material:
					if ((chosenBuilding as Building).requiredBasementMaterialId == surface.material_id) {
						SelectBuildingForConstruction (chosenBuilding, selectedBuildingButton);
					}
					break;
				}
				//rotating window
			}
			break;
		}
	}

	public void BuildButton() {
		ChangeMode( SurfacePanelMode.Build );
	}
	public void BlockBuildingButton() {}
	public void ColumnBuildingButton() {}
	public void MaterialChangingButton() {}
	public void GatherButton() {
		if (surface == null) {
			SelfShutOff();
			return;
		}
		else {
			GatherSite gs = surface.GetComponent<GatherSite>();
			if (gs == null) {
				gs = surface.gameObject.AddComponent<GatherSite>();
				gs.Set(surface);
			}
			else Destroy(gs);
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}
	public void DigButton() {
		if (surface == null) {
			SelfShutOff();
			return;
		}
		else {
			CleanSite cs = surface.GetComponent<CleanSite>();
			if (cs == null) {
				cs = surface.gameObject.AddComponent<CleanSite>();
				cs.Set(surface, true);
			}
			else {
				if (cs.diggingMission) Destroy(cs);
			}
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}

	public void ChangeMode(SurfacePanelMode newMode) {
		switch (newMode) {
		case SurfacePanelMode.Build:
			int i = 0;
			hq = GameMaster.colonyController.hq;
			buildingsLevelToggles[0].transform.parent.gameObject.SetActive(true);
			while (i < buildingsLevelToggles.Length ) {
				if (i >= hq.level) buildingsLevelToggles[i].gameObject.SetActive(false);
				else {
					buildingsLevelToggles[i].gameObject.SetActive(true);
					if (i == constructingLevel - 1) buildingsLevelToggles[i].isOn = true; else  buildingsLevelToggles[i].isOn = false;
				}
				i++;
			}
			SetActionPanelStatus(false);
			SetBuildPanelStatus(true);

			returnButton.gameObject.SetActive(true);
			availableBuildingsButtons[0].transform.parent.gameObject.SetActive(true); // "surfaceBuildingPanel"
			buildZone.gameObject.SetActive(false);
			RewriteBuildingButtons();
			mode = SurfacePanelMode.Build;
			break;
		case SurfacePanelMode.SelectAction:
			switch (mode) {
			case SurfacePanelMode.Build: SetBuildPanelStatus(false);	break;
			}
			SetActionPanelStatus(true);
			mode = SurfacePanelMode.SelectAction;
			break;
		}
	}

	#region panels setting
	void SetBuildPanelStatus ( bool working ) {
		buildingsLevelToggles[0].transform.parent.gameObject.SetActive(working);
		returnButton.gameObject.SetActive( working );
		surfaceBuildingPanel.SetActive( working ); 
		surfaceGridToggle.gameObject.SetActive(working);        
        if (!working) {
            infoPanel.SetActive(false); // включаются по select building
            if (selectedBuildingButton != -1) {
				availableBuildingsButtons[selectedBuildingButton].image.overrideSprite = null;
				selectedBuildingButton = -1;
				chosenBuilding = null;
			}
			surfaceGridToggle.isOn = false;
		}
	}
	void SetActionPanelStatus ( bool working ) {
		buildButton.gameObject.SetActive( working  );
		gatherButton.gameObject.SetActive( working  );
		digButton.gameObject.SetActive( working  );
		if (working) {
			status_gatherEnabled = (surface.cellsStatus != 1);
			gatherButton.interactable = status_gatherEnabled;
			status_digOrdered = (surface.GetComponent<CleanSite>() != null && surface.GetComponent<CleanSite>().diggingMission);
			digButton.transform.GetChild(0).GetComponent<Text>().text = (status_digOrdered ? Localization.GetPhrase(  LocalizedPhrase.StopDig) : Localization.GetWord( LocalizedWord.Dig));
			savedHqLevel = hq.level;
			blockCreateButton.enabled = ( savedHqLevel> 3);
			columnCreateButton.enabled = (savedHqLevel > 4 && surface.pos.y < Chunk.CHUNK_SIZE - 1);
			changeMaterialButton.enabled = (GameMaster.colonyController.gears_coefficient >=2);
			UIController.current.closePanelButton.gameObject.SetActive(true);
		}
		else {
			if (changeMaterialButton.gameObject.activeSelf) changeMaterialButton.gameObject.SetActive( false ) ;
			if (blockCreateButton.gameObject.activeSelf) blockCreateButton.gameObject.SetActive( false  );
			if (columnCreateButton.gameObject.activeSelf) columnCreateButton.gameObject.SetActive( false  );
			UIController.current.closePanelButton.gameObject.SetActive(false);
		}
	}
	#endregion

	public void SelectBuildingForConstruction (Structure building, int buttonIndex) {
		chosenBuilding = building;
		availableBuildingsButtons[buttonIndex].image.overrideSprite =overridingSprite; 
		if (selectedBuildingButton >= 0) availableBuildingsButtons[selectedBuildingButton].image.overrideSprite = null;
		selectedBuildingButton = buttonIndex;

        infoPanel.SetActive(true);
		nameField.text = Localization.GetStructureName(chosenBuilding.id);
        gridTextField.text = chosenBuilding.innerPosition.x_size.ToString() + " x " + chosenBuilding.innerPosition.z_size.ToString();
        Building b = chosenBuilding as Building;
        if (b != null)
        {
            if (b.energySurplus != 0)
            {
                energyIcon.SetActive(true);
                energyTextField.gameObject.SetActive(true);
                energyTextField.text = b.energySurplus > 0 ? '+' + b.energySurplus.ToString() :  b.energySurplus.ToString();
            }
            else
            {
                energyIcon.SetActive(false);
                energyTextField.gameObject.SetActive(false);
            }
            if (b is House)
            {
                housingIcon.SetActive(true);
                housingTextField.gameObject.SetActive(true);
                housingTextField.text = (b as House).housing.ToString();
            }
            else
            {
                housingIcon.SetActive(false);
                housingTextField.gameObject.SetActive(false);
            }
        }
        else
        {
            energyIcon.SetActive(false);
            energyTextField.gameObject.SetActive(false);
            housingIcon.SetActive(false);
            housingTextField.gameObject.SetActive(false);
        }
        description.text = Localization.GetStructureDescription(chosenBuilding.id);
        

		bool sideBlock = ( surface.pos.x == 0 | surface.pos.z == 0 | surface.pos.x == Chunk.CHUNK_SIZE - 1 | surface.pos.z == Chunk.CHUNK_SIZE - 1 );
		resourcesCostImage[0].transform.parent.gameObject.SetActive(true);
		Text t = resourcesCostImage[0].transform.GetChild(0).GetComponent<Text>();
		//side block check :
		if ( chosenBuilding.borderOnlyConstruction & sideBlock != true ) {
			// construction delayed in because of not-side position
			resourcesCostImage[0].gameObject.SetActive(true);
			t.text = Localization.GetRestrictionPhrase(RestrictionKey.SideConstruction);
			t.color = Color.yellow;
			resourcesCostImage[0].uvRect = ResourceType.GetTextureRect(0);
			for (int i = 1; i < resourcesCostImage.Length; i++) {
				resourcesCostImage[i].gameObject.SetActive(false);
			}
			innerBuildButton.gameObject.SetActive(false);
			buildingCreateMode = BuildingCreateInfoMode.Unacceptable_SideBlock;
		}
		else {	
			Building bd = chosenBuilding as Building;
			// material check :
			if (bd != null & bd.requiredBasementMaterialId != -1 & bd.requiredBasementMaterialId != surface.material_id) {
				t.text = Localization.GetRestrictionPhrase(RestrictionKey.UnacceptableSurfaceMaterial);
				t.color = Color.yellow;
				resourcesCostImage[0].uvRect = ResourceType.GetTextureRect(0);
				resourcesCostImage[0].gameObject.SetActive(true);
				for (int i = 1; i < resourcesCostImage.Length - 1; i++) {
					resourcesCostImage[i].gameObject.SetActive(false);
				}
				int n = resourcesCostImage.Length - 1;
				t = resourcesCostImage[n].transform.GetChild(0).GetComponent<Text>();
				t.text = Localization.GetPhrase(LocalizedPhrase.RequiredSurface) + " : " + Localization.GetResourceName(bd.requiredBasementMaterialId);
				resourcesCostImage[n].uvRect = ResourceType.GetTextureRect(bd.requiredBasementMaterialId);
				resourcesCostImage[n].gameObject.SetActive(true);
				t.color = Color.yellow;
				buildingCreateMode = BuildingCreateInfoMode.Unacceptable_Material;
				innerBuildButton.gameObject.SetActive(false);
			} 
			else {
				// all conditions met
				ResourceContainer[] cost = ResourcesCost.GetCost(chosenBuilding.id);
				//resource cost drawing
				float[] storageResources = GameMaster.colonyController.storage.standartResources;
				for (int i = 0; i < resourcesCostImage.Length; i++) {
					if ( i < cost.Length) {						
						resourcesCostImage[i].uvRect = ResourceType.GetTextureRect(cost[i].type.ID);
						t = resourcesCostImage[i].transform.GetChild(0).GetComponent<Text>();
						t.text = Localization.GetResourceName(cost[i].type.ID) + " : " + string.Format("{0:0.##}", cost[i].volume);
						showingResourcesCount[i] = new Vector2(cost[i].type.ID, cost[i].volume);
						if (storageResources[cost[i].type.ID] < cost[i].volume ) t.color =Color.red; else t.color = Color.white;
						resourcesCostImage[i].gameObject.SetActive(true);
					}
					else {
						resourcesCostImage[i].gameObject.SetActive(false);
					}
				}
				buildingCreateMode = BuildingCreateInfoMode.Acceptable;
				buildZone.anchorMin = Vector2.zero;
				buildZone.anchorMax = new Vector2( (SurfaceBlock.INNER_RESOLUTION - chosenBuilding.innerPosition.x_size) / SurfaceBlock.INNER_RESOLUTION, (SurfaceBlock.INNER_RESOLUTION - chosenBuilding.innerPosition.z_size) / SurfaceBlock.INNER_RESOLUTION );
				innerBuildButton.gameObject.SetActive(true);
			}
		}
	}

	public void CreateSelectedBuilding () {CreateSelectedBuilding (0,0);}
	public void CreateSelectedBuilding (byte x, byte z) {
		if (selectedBuildingButton == -1 | chosenBuilding == null) return;
		if (chosenBuilding.innerPosition != SurfaceRect.full) surfaceGridToggle.Select();
		ResourceContainer[] cost = ResourcesCost.GetCost(chosenBuilding.id);
		// make checks!
		if (GameMaster.colonyController.storage.CheckSpendPossibility(cost)) {
			if ( surface.IsAnyBuildingInArea(new SurfaceRect(x, z,chosenBuilding.innerPosition.x_size, chosenBuilding.innerPosition.z_size)) == false) {
				Structure s = Structure.GetNewStructure(chosenBuilding.id);
				s.gameObject.SetActive(true);
				s.SetBasement(surface, new PixelPosByte(x,z));
			}
			else {
				//вывести запрос на подтверждение
			}
		}
		else {
			GameMaster.realMaster.AddAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
		}
	}


	public void SetConstructingLevel (int l) {
		constructingLevel = (byte)l;
		
		RewriteBuildingButtons();
	}

	void RewriteBuildingButtons () {
		// поправка на материал
		// поправка на side-only
		List<Building> abuildings = Structure.GetApplicableBuildingsList(constructingLevel);
		for (int n = 0; n < availableBuildingsButtons.Length; n++) {
			if (n < abuildings.Count) {
				availableBuildingsButtons[n].gameObject.SetActive(true);
				RawImage rimage = availableBuildingsButtons[n].transform.GetChild(0).GetComponent<RawImage>();
				Vector2 txPos = Structure.GetTexturePosition(abuildings[n].id);
				rimage.uvRect = new Rect(txPos.x, txPos.y, 0.125f, 0.125f);
				availableBuildingsButtons[n].onClick.RemoveAllListeners();
				int bid = n;
				availableBuildingsButtons[n].onClick.AddListener(() => {
					this.SelectBuildingForConstruction(abuildings[bid], bid);
				});
			}
			else {
				availableBuildingsButtons[n].gameObject.SetActive(false);
			}
		}
	}
		
}
