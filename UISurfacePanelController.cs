using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum CostPanelMode { Disabled, ColumnBuilding, SurfaceMaterialChanging, BlockBuilding }
public enum SurfacePanelMode {SelectAction, Build}
enum BuildingCreateInfoMode {Acceptable, Unacceptable_SideBlock, Unacceptable_Material, HeightBlocked}

public sealed class UISurfacePanelController : UIObserver {
	public Button buildButton, gatherButton, digButton, blockCreateButton, columnCreateButton, changeMaterialButton;
	SurfaceBlock surface;
	bool status_gatherEnabled = true, status_gatherOrdered = false, status_digOrdered = false;
	byte savedHqLevel = 0;
	Vector2[] showingResourcesCount;
    int selectedBuildingButton = -1;
    Vector2Int costPanel_selectedButton;
	Structure chosenBuilding = null;
	public byte constructingLevel = 1;
	SurfacePanelMode mode;
    CostPanelMode costPanelMode;
	BuildingCreateInfoMode buildingCreateMode;
	public Toggle[] buildingsLevelToggles; // fiti
	public Button[] availableBuildingsButtons; // fiti
	public Text nameField, description, gridTextField,energyTextField, housingTextField; // fiti
	public RawImage[] resourcesCostImage; // fiti
	public Button innerBuildButton, returnButton; // fiti
	public RectTransform buildZone; // fiti

	public Toggle surfaceGridToggle; // fiti
	[SerializeField] GameObject surfaceBuildingPanel, infoPanel, energyIcon, housingIcon, costPanel; // fiti
	HeadQuarters hq;



	void Start() {
		returnButton.onClick.AddListener(() => {
			this.ChangeMode(SurfacePanelMode.SelectAction);
		});
		showingResourcesCount = new Vector2[resourcesCostImage.Length];
        changeMaterialButton.onClick.AddListener(() => {
            this.SetCostPanelMode(CostPanelMode.SurfaceMaterialChanging);
        });
        columnCreateButton.onClick.AddListener(() => {
            this.SetCostPanelMode(CostPanelMode.ColumnBuilding);
        });
        blockCreateButton.onClick.AddListener(() => {
            this.SetCostPanelMode(CostPanelMode.BlockBuilding);
        });
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

    protected override void StatusUpdate()
    {
        if (surface == null)
        {
            SelfShutOff();
            return;
        }
        switch (mode)
        {
            case SurfacePanelMode.SelectAction:
                hq = GameMaster.colonyController.hq;

                bool check = false;
                if (surface.GetComponent<GatherSite>() != null)
                {
                    if (status_gatherOrdered == false)
                    {
                        status_gatherOrdered = true;
                        status_gatherEnabled = true;
                        gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.StopGather);
                        gatherButton.interactable = true;
                    }
                }
                else
                {
                    if (status_gatherOrdered == true)
                    {
                        status_gatherOrdered = false;
                        gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Gather);
                        if (surface.cellsStatus != 0)
                        {
                            status_gatherEnabled = true;
                            gatherButton.interactable = true;
                        }
                        else
                        {
                            status_gatherEnabled = false;
                            gatherButton.interactable = false;
                        }
                    }
                    else
                    {
                        check = (surface.cellsStatus != 0);
                        if (check != status_gatherEnabled)
                        {
                            status_gatherEnabled = check;
                            gatherButton.interactable = status_gatherEnabled;
                        }
                    }
                }

                CleanSite cs = surface.GetComponent<CleanSite>();
                check = (cs != null && cs.diggingMission);
                if (check != status_digOrdered)
                {
                    status_digOrdered = check;
                    digButton.transform.GetChild(0).GetComponent<Text>().text = status_digOrdered ? Localization.GetPhrase(LocalizedPhrase.StopDig) : Localization.GetWord(LocalizedWord.Dig);
                }


                if (savedHqLevel != hq.level)
                {
                    savedHqLevel = hq.level;
                    blockCreateButton.enabled = (savedHqLevel > 3);
                    columnCreateButton.enabled = (savedHqLevel > 4 && surface.pos.y < Chunk.CHUNK_SIZE - 1);
                }
                changeMaterialButton.enabled = (GameMaster.colonyController.gears_coefficient >= 2);
                break;

            case SurfacePanelMode.Build:
                if (chosenBuilding != null)
                {
                    switch (buildingCreateMode)
                    {
                        case BuildingCreateInfoMode.Acceptable:
                            float[] onStorage = GameMaster.colonyController.storage.standartResources;
                            for (int i = 0; i < resourcesCostImage.Length; i++)
                            {
                                int rid = (int)showingResourcesCount[i].x;
                                if (onStorage[rid] != showingResourcesCount[i].y)
                                {                                    
                                    resourcesCostImage[i].transform.GetChild(0).GetComponent<Text>().color = onStorage[rid] < showingResourcesCount[i].y ? Color.red : Color.white;
                                    showingResourcesCount[i].y = onStorage[rid];
                                }
                            }
                            break;
                        case BuildingCreateInfoMode.Unacceptable_Material:
                            if ((chosenBuilding as Building).requiredBasementMaterialId == surface.material_id)
                            {
                                SelectBuildingForConstruction(chosenBuilding, selectedBuildingButton);
                            }
                            break;
                        case BuildingCreateInfoMode.HeightBlocked:
                            int side = 0;
                            if (surface.pos.x == 0)
                            {
                                if (surface.pos.z == 0) side = 2;
                            }
                            else
                            {
                                if (surface.pos.x == Chunk.CHUNK_SIZE - 1) side = 1;
                                else side = 3;
                            }
                            if (surface.myChunk.sideBlockingMap[surface.pos.y, side] == false) SelectBuildingForConstruction(chosenBuilding, selectedBuildingButton);
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
                UIController.current.ShowWorksite(gs);
			}
			else gs.StopWork();
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
                UIController.current.ShowWorksite(cs);
            }
			else {
				if (cs.diggingMission) cs.StopWork();
			}
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}

    public void ChangeMode(SurfacePanelMode newMode)
    {
        switch (newMode)
        {
            case SurfacePanelMode.Build:
                int i = 0;
                hq = GameMaster.colonyController.hq;
                buildingsLevelToggles[0].transform.parent.gameObject.SetActive(true);
                while (i < buildingsLevelToggles.Length)
                {
                    if (i >= hq.level) buildingsLevelToggles[i].gameObject.SetActive(false);
                    else
                    {
                        buildingsLevelToggles[i].gameObject.SetActive(true);
                        if (i == constructingLevel - 1) buildingsLevelToggles[i].isOn = true; else buildingsLevelToggles[i].isOn = false;
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
                switch (mode)
                {
                    case SurfacePanelMode.Build: SetBuildPanelStatus(false); break;
                }
                SetActionPanelStatus(true);
                mode = SurfacePanelMode.SelectAction;
                ColonyController colony = GameMaster.colonyController;
                if (colony.gears_coefficient >= 2)
                {
                    changeMaterialButton.gameObject.SetActive(true);
                }
                else changeMaterialButton.gameObject.SetActive(false);
                columnCreateButton.gameObject.SetActive( colony.hq.level > 2 );
                blockCreateButton.gameObject.SetActive(colony.hq.level > 5);
                break;
        }
    }

    #region panels setting
    void SetCostPanelMode(CostPanelMode m)
    {        
        if (m != CostPanelMode.Disabled) costPanel.gameObject.SetActive(true);
        else
        {
            costPanel.transform.GetChild(0).gameObject.SetActive(false);
            costPanel.transform.GetChild(1).gameObject.SetActive(false);
            costPanel.transform.GetChild(2).gameObject.SetActive(false);
            changeMaterialButton.GetComponent<Image>().overrideSprite = null;
            columnCreateButton.GetComponent<Image>().overrideSprite = null;
            blockCreateButton.GetComponent<Image>().overrideSprite = null;
            costPanel.SetActive(false);
            costPanelMode = CostPanelMode.Disabled;
            return;
        }
        Transform t;
        switch (m)
        {
            case CostPanelMode.SurfaceMaterialChanging:
                if (costPanelMode == CostPanelMode.SurfaceMaterialChanging)
                {                    
                    SetCostPanelMode(CostPanelMode.Disabled);
                    return;
                }
                else
                {
                    changeMaterialButton.GetComponent<Image>().overrideSprite = UIController.current.overridingSprite;
                    columnCreateButton.GetComponent<Image>().overrideSprite = null;
                    blockCreateButton.GetComponent<Image>().overrideSprite = null;
                    int lastUsedIndex = 0;                    
                    costPanel.transform.GetChild(0).gameObject.SetActive(true);
                    costPanel.transform.GetChild(1).gameObject.SetActive(false);
                    costPanel.transform.GetChild(2).gameObject.SetActive(false);
                    foreach (ResourceType rt in ResourceType.materialsForCovering)
                    {
                        if (rt.ID == surface.material_id) continue;
                        t = costPanel.transform.GetChild(0).GetChild(lastUsedIndex);
                        t.gameObject.SetActive(true);
                        RawImage ri = t.GetChild(0).GetComponent<RawImage>();
                        ri.texture = UIController.current.resourcesTexture;
                        ri.uvRect = ResourceType.GetTextureRect(rt.ID);
                        t.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(rt.ID);
                        Button b = t.GetComponent<Button>();
                        b.onClick.RemoveAllListeners();
                        Vector2Int indxs = new Vector2Int(lastUsedIndex, rt.ID);
                        b.onClick.AddListener(() =>
                        {
                            this.CostPanel_SelectResource(indxs);
                        });
                        b.GetComponent<Image>().overrideSprite = null;
                        lastUsedIndex++;
                    }
                    if (lastUsedIndex < costPanel.transform.childCount)
                    {
                        for (; lastUsedIndex < costPanel.transform.childCount; lastUsedIndex++)
                        {
                            costPanel.transform.GetChild(lastUsedIndex).gameObject.SetActive(false);
                        }
                    }
                    costPanel.transform.GetChild(2).gameObject.SetActive(false); // build button
                }
                break;

            case CostPanelMode.ColumnBuilding:
                if (costPanelMode != CostPanelMode.ColumnBuilding)
                {
                    columnCreateButton.GetComponent<Image>().overrideSprite = UIController.current.overridingSprite;
                    changeMaterialButton.GetComponent<Image>().overrideSprite = null;
                    blockCreateButton.GetComponent<Image>().overrideSprite = null;

                    t = costPanel.transform;
                    t.GetChild(0).gameObject.SetActive(false); // buttons                
                    t.GetChild(2).gameObject.SetActive(true);// build button
                    t = t.GetChild(1);// resource cost
                    t.gameObject.SetActive(true);
                    ResourceContainer[] rc = ResourcesCost.GetCost(Structure.COLUMN_ID);
                    for (int i = 0; i < t.childCount; i++)
                    {
                        Transform r = t.GetChild(i);
                        if (i < rc.Length)
                        {
                            r.gameObject.SetActive(true);
                            int id = rc[i].type.ID;
                            r.GetComponent<RawImage>().uvRect = ResourceType.GetTextureRect(id);
                            r.GetChild(0).GetComponent<Text>().text = Localization.GetResourceName(id) + " : " + rc[i].volume.ToString();
                        }
                        else
                        {
                            r.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {                    
                    SetCostPanelMode(CostPanelMode.Disabled);
                    return;
                }
                break;

            case CostPanelMode.BlockBuilding:
                if (costPanelMode != CostPanelMode.BlockBuilding)
                {
                    blockCreateButton.GetComponent<Image>().overrideSprite = UIController.current.overridingSprite;
                    changeMaterialButton.GetComponent<Image>().overrideSprite = null;
                    columnCreateButton.GetComponent<Image>().overrideSprite = null;

                    t = costPanel.transform;
                    t.GetChild(2).gameObject.SetActive(false);
                    t.GetChild(1).gameObject.SetActive(false);
                    t = t.GetChild(0);
                    t.gameObject.SetActive(true);
                    int i = 0;
                    for (; i < ResourceType.blockMaterials.Length; i++)
                    {
                        Transform c = t.GetChild(i);
                        c.gameObject.SetActive(true);
                        int id = ResourceType.blockMaterials[i].ID;
                        c.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetTextureRect(id);
                        c.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(id);
                        Button b = c.GetComponent<Button>();
                        b.onClick.RemoveAllListeners();
                        Vector2Int indxs = new Vector2Int(i, id);
                        b.onClick.AddListener(() =>
                        {
                            this.CostPanel_SelectResource(indxs);
                        });
                        b.GetComponent<Image>().overrideSprite = null;
                    }
                    if (i < t.childCount)
                    {
                        for (; i < t.childCount; i++)
                        {
                            t.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    SetCostPanelMode(CostPanelMode.Disabled);
                    return;
                }
                break;
        }
        costPanelMode = m;
    }
    public void CostPanel_SelectResource(Vector2Int indexes)
    {        
        costPanel_selectedButton = indexes;
        costPanel.transform.GetChild(0).GetChild(indexes.x).GetComponent<Image>().overrideSprite = UIController.current.overridingSprite;
        Transform t = costPanel.transform.GetChild(2);// build button
        t.gameObject.SetActive(true);
        t.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Build) + " (" + (costPanelMode == CostPanelMode.SurfaceMaterialChanging ? GameMaster.SURFACE_MATERIAL_REPLACE_COUNT : CubeBlock.MAX_VOLUME ) + ')';
    }
    public void CostPanel_Build()
    {        
        switch (costPanelMode)
        {
            case CostPanelMode.SurfaceMaterialChanging:
                    ResourceType rt = ResourceType.GetResourceTypeById(costPanel_selectedButton.y);
                    if (GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(new ResourceContainer[] { new ResourceContainer(rt, GameMaster.SURFACE_MATERIAL_REPLACE_COUNT) }))
                    {
                        surface.ReplaceMaterial(rt.ID);
                        costPanel.transform.GetChild(0).GetChild(costPanel_selectedButton.x).GetComponent<Image>().overrideSprite = null;
                    }
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                break;
                case CostPanelMode.ColumnBuilding:
                    if (GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(ResourcesCost.GetCost(Structure.COLUMN_ID)))
                    {
                        float supportPoints = surface.myChunk.CalculateSupportPoints(surface.pos.x, surface.pos.y, surface.pos.z);
                        if (supportPoints <= 1)
                        {
                            Structure s = Structure.GetNewStructure(Structure.COLUMN_ID);
                            s.SetBasement(surface, new PixelPosByte(7, 7));
                            PoolMaster.current.BuildSplash(surface.transform.position);
                        }
                        else
                        {
                            surface.myChunk.ReplaceBlock(surface.pos, BlockType.Cave, surface.material_id, ResourceType.CONCRETE_ID, false);
                        }
                    }
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
               
                break;
            case CostPanelMode.BlockBuilding:
                BlockBuildingSite bbs = surface.gameObject.GetComponent<BlockBuildingSite>();
                if (bbs == null) bbs = surface.gameObject.AddComponent<BlockBuildingSite>();
                bbs.Set(surface, ResourceType.GetResourceTypeById(costPanel_selectedButton.y));
                SetCostPanelMode(CostPanelMode.Disabled);
                UIController.current.ShowWorksite(bbs);
                break;
        }
    }

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
        else
        {
            if (costPanelMode != CostPanelMode.Disabled) SetCostPanelMode(CostPanelMode.Disabled);
        }
	}
	void SetActionPanelStatus ( bool working ) {
		buildButton.gameObject.SetActive( working  );
		gatherButton.gameObject.SetActive( working  );
		digButton.gameObject.SetActive( working  );
		if (working) {
            if (surface.GetComponent<GatherSite>() != null)
            {
                status_gatherOrdered = true;
                status_gatherEnabled = true;
                gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.StopGather);
                gatherButton.interactable = true;
            }
            else
            {                
                status_gatherOrdered = false;
                gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Gather);
                status_gatherEnabled = (surface.cellsStatus != 0);
                gatherButton.interactable = status_gatherEnabled ;
            }

            CleanSite cs = surface.GetComponent<CleanSite>();
            if (cs != null && cs.diggingMission)
            {

                status_digOrdered = true;
                digButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.StopDig);
            }
            else
            {
                status_digOrdered = false;
                digButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dig);
            }
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
            if (costPanelMode != CostPanelMode.Disabled) SetCostPanelMode(CostPanelMode.Disabled);
        }
	}
    #endregion

    #region building construction 
    public void SelectBuildingForConstruction (Structure building, int buttonIndex) {
		chosenBuilding = building;
		availableBuildingsButtons[buttonIndex].image.overrideSprite = UIController.current.overridingSprite; 
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
        bool allConditionsMet = false;
        //side block check :
        if (chosenBuilding.borderOnlyConstruction)
        {
            if (sideBlock == false)
            {
                // construction delayed in because of not-side position
                t.text = Localization.GetRestrictionPhrase(RestrictionKey.SideConstruction);
                buildingCreateMode = BuildingCreateInfoMode.Unacceptable_SideBlock;
            }
            else
            {
                int side = 0;
                if (surface.pos.x == 0)
                {
                    if (surface.pos.z == 0) side = 2;
                }
                else
                {
                    if (surface.pos.x == Chunk.CHUNK_SIZE - 1) side = 1;
                    else side = 3;
                }

                if (surface.myChunk.sideBlockingMap[surface.pos.y, side] == false)
                {
                    allConditionsMet = true;
                }
                else
                {
                    t.text = Localization.GetRestrictionPhrase(RestrictionKey.HeightBlocked);
                    buildingCreateMode = BuildingCreateInfoMode.HeightBlocked;
                }
            }
        }
        else allConditionsMet = true;
		if (allConditionsMet) {	
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
        else
        {
            resourcesCostImage[0].gameObject.SetActive(true);            
            t.color = Color.yellow;
            resourcesCostImage[0].uvRect = ResourceType.GetTextureRect(0);
            for (int i = 1; i < resourcesCostImage.Length; i++)
            {
                resourcesCostImage[i].gameObject.SetActive(false);
            }
            innerBuildButton.gameObject.SetActive(false);
        }
	}
    void DeselectBuildingButton()
    {
        chosenBuilding = null;
        if (selectedBuildingButton >= 0) availableBuildingsButtons[selectedBuildingButton].image.overrideSprite = null;
        selectedBuildingButton = -1;
        infoPanel.SetActive(false);
    }

	public void CreateSelectedBuilding () {
        if (chosenBuilding.fullCover) CreateSelectedBuilding(0, 0);
        else CreateSelectedBuilding((byte)(SurfaceBlock.INNER_RESOLUTION / 2 - chosenBuilding.innerPosition.x_size/2), (byte)(SurfaceBlock.INNER_RESOLUTION / 2 - chosenBuilding.innerPosition.z_size/2));
    }
	public void CreateSelectedBuilding (byte x, byte z) {
		if (selectedBuildingButton == -1 | chosenBuilding == null) return;
		if (chosenBuilding.innerPosition != SurfaceRect.full) surfaceGridToggle.Select();
		ResourceContainer[] cost = ResourcesCost.GetCost(chosenBuilding.id);
		// make checks!
		if (GameMaster.colonyController.storage.CheckSpendPossibility(cost)) {
			if ( surface.IsAnyBuildingInArea(new SurfaceRect(x, z,chosenBuilding.innerPosition.x_size, chosenBuilding.innerPosition.z_size)) == false) {
                GameMaster.colonyController.storage.GetResources(cost);
                Structure s = Structure.GetNewStructure(chosenBuilding.id);
				s.gameObject.SetActive(true);
				s.SetBasement(surface, new PixelPosByte(x,z));
                PoolMaster.current.BuildSplash(surface.transform.position);
            }
			else {
				//вывести запрос на подтверждение
			}
		}
		else {
            UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
		}
	}

	public void SetConstructingLevel (int l) {
        if (constructingLevel != l)
        {
            constructingLevel = (byte)l;
            DeselectBuildingButton();
            RewriteBuildingButtons();
        }
	}

	void RewriteBuildingButtons () {
		// поправка на материал
		// поправка на side-only
		List<Building> abuildings = Structure.GetApplicableBuildingsList(constructingLevel);
		for (int n = 0; n < availableBuildingsButtons.Length; n++) {
			if (n < abuildings.Count) {
				availableBuildingsButtons[n].gameObject.SetActive(true);
				RawImage rimage = availableBuildingsButtons[n].transform.GetChild(0).GetComponent<RawImage>();
                rimage.uvRect = Structure.GetTextureRect(abuildings[n].id);
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
    #endregion
}
