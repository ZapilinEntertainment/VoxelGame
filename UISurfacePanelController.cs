using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum CostPanelMode { Disabled, ColumnBuilding, SurfaceMaterialChanging, BlockBuilding }
public enum SurfacePanelMode {SelectAction, Build}
enum BuildingCreateInfoMode {Acceptable, Unacceptable_SideBlock, Unacceptable_Material, HeightBlocked}

public sealed class UISurfacePanelController : UIObserver {
	public Button buildButton, gatherButton, digButton, blockCreateButton, columnCreateButton, changeMaterialButton;
    SurfaceBlock observingSurface;
    bool status_digOrdered = false, firstSet = true;
    bool? status_gather = null;
	byte savedHqLevel = 0;
	Vector2[] showingResourcesCount;
    int selectedBuildingButton = -1, lastStorageStatus = -1;
    Vector2Int costPanel_selectedButton;
	Structure chosenStructure = null;
	public byte constructingLevel = 1;
	SurfacePanelMode mode;
    CostPanelMode costPanelMode;
	BuildingCreateInfoMode buildingCreateMode;
    Vector2Int constructingPlaneTouchPos;

    [SerializeField] Material constructingPlaneMaterial;
    [SerializeField] GameObject buildIntersectionSubmit, constructionPlane; // window asking about deleting overlapping buildings
    [SerializeField] Transform buildingButtonsContainer; // fiti
	public Toggle[] buildingsLevelToggles; // fiti
	public Text nameField, description, gridTextField,energyTextField, housingTextField; // fiti
	public RawImage[] resourcesCostImage; // fiti
	[SerializeField] Button innerBuildButton, returnButton, constructionPlaneSwitchButton; // fiti
	[SerializeField] GameObject surfaceBuildingPanel, infoPanel, energyIcon, housingIcon, costPanel; // fiti
	HeadQuarters hq;

    public static UISurfacePanelController current { get; private set; }

    void Start() {
        if (firstSet)
        {
            showingResourcesCount = new Vector2[resourcesCostImage.Length];
            changeMaterialButton.onClick.AddListener(() =>
            {
                this.SetCostPanelMode(CostPanelMode.SurfaceMaterialChanging);
            });
            columnCreateButton.onClick.AddListener(() =>
            {
                this.SetCostPanelMode(CostPanelMode.ColumnBuilding);
            });
            blockCreateButton.onClick.AddListener(() =>
            {
                this.SetCostPanelMode(CostPanelMode.BlockBuilding);
            });
            constructionPlane.transform.parent = null;
            current = this;
            firstSet = false;
        }
       LocalizeButtonTitles();
    }

    public void SetObservingSurface(SurfaceBlock sb) {
		if (sb == null) {
			SelfShutOff();
			return;
		}
		else {
			hq = GameMaster.colonyController.hq;
			observingSurface = sb;
			isObserving = true;
			ChangeMode (SurfacePanelMode.SelectAction);
            if (constructionPlane.activeSelf) PrepareConstructionPlane();
				
			STATUS_UPDATE_TIME = 1;
			timer = STATUS_UPDATE_TIME;
		}
	}

    protected override void StatusUpdate()
    {
        if (observingSurface == null)
        {
            SelfShutOff();
            return;
        }
        switch (mode)
        {
            case SurfacePanelMode.SelectAction:
                {
                    hq = GameMaster.colonyController.hq;
                    CheckGatherButton();

                    CleanSite cs = observingSurface.GetComponent<CleanSite>();
                    bool check = (cs != null && cs.diggingMission);
                    if (check != status_digOrdered)
                    {
                        status_digOrdered = check;
                        digButton.transform.GetChild(0).GetComponent<Text>().text = status_digOrdered ? Localization.GetPhrase(LocalizedPhrase.StopDig) : Localization.GetWord(LocalizedWord.Dig);
                    }


                    if (savedHqLevel != hq.level)
                    {
                        savedHqLevel = hq.level;
                        blockCreateButton.gameObject.SetActive(IsBlockCreatingAvailable());
                        columnCreateButton.gameObject.SetActive(IsColumnAvailable());
                    }
                    changeMaterialButton.gameObject.SetActive(IsChangeSurfaceMaterialAvalable());
                    break;
                }
            case SurfacePanelMode.Build:
                {
                    if (chosenStructure != null)
                    {
                        switch (buildingCreateMode)
                        {
                            case BuildingCreateInfoMode.Acceptable:
                                Storage storage = GameMaster.colonyController.storage;
                                if (lastStorageStatus != storage.operationsDone)
                                {
                                    float[] onStorage = storage.standartResources;
                                    for (int i = 0; i < resourcesCostImage.Length; i++)
                                    {
                                        int rid = (int)showingResourcesCount[i].x;
                                        resourcesCostImage[i].transform.GetChild(0).GetComponent<Text>().color = (onStorage[rid] < showingResourcesCount[i].y) ? Color.red : Color.white;
                                    }
                                    lastStorageStatus = storage.operationsDone;
                                }
                                break;
                            case BuildingCreateInfoMode.Unacceptable_Material:
                                if ((chosenStructure as Building).requiredBasementMaterialId == observingSurface.material_id)
                                {
                                    SelectBuildingForConstruction(chosenStructure, selectedBuildingButton);
                                }
                                break;
                            case BuildingCreateInfoMode.HeightBlocked:
                                int side = 0;
                                if (observingSurface.pos.x == 0)
                                {
                                    if (observingSurface.pos.z == 0) side = 2;
                                }
                                else
                                {
                                    if (observingSurface.pos.x == Chunk.CHUNK_SIZE - 1) side = 1;
                                    else side = 3;
                                }
                                if (observingSurface.myChunk.sideBlockingMap[observingSurface.pos.y, side] == false) SelectBuildingForConstruction(chosenStructure, selectedBuildingButton);
                                break;
                        }
                        //rotating window
                    }
                }
              break;
        }
    }
       

    public void BuildButton() {
		ChangeMode( SurfacePanelMode.Build );
	}
	public void GatherButton() {
		if (observingSurface == null) {
			SelfShutOff();
			return;
		}
		else {
			GatherSite gs = observingSurface.GetComponent<GatherSite>();
			if (gs == null) {
				gs = observingSurface.gameObject.AddComponent<GatherSite>();
				gs.Set(observingSurface);
                UIController.current.ShowWorksite(gs);
			}
			else gs.StopWork();
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}
	public void DigButton() {
		if (observingSurface == null) {
			SelfShutOff();
			return;
		}
		else {
			CleanSite cs = observingSurface.GetComponent<CleanSite>();
			if (cs == null) {
				cs = observingSurface.gameObject.AddComponent<CleanSite>();
				cs.Set(observingSurface, true);
                UIController.current.ShowWorksite(cs);
            }
			else {
				if (cs.diggingMission) cs.StopWork();
			}
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}

    void CheckGatherButton()
    {
        if (observingSurface.GetComponent<GatherSite>() != null)
        {
            if (status_gather != true)
            {
                gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.StopGather);
                gatherButton.interactable = true;
                status_gather = true;
            }
        }
        else
        {
            if (observingSurface.cellsStatus != 0)
            {
                if (status_gather != false)
                {
                    gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Gather);
                    gatherButton.interactable = true;
                    status_gather = false;
                }
            }
            else
            {
                if (status_gather != null)
                {
                    gatherButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Gather);
                    gatherButton.interactable = false;
                    status_gather = null;
                }
            }
        }
    }

    public void ChangeMode(SurfacePanelMode newMode)
    {
        switch (newMode)
        {
            case SurfacePanelMode.Build:
                {
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
                    buildingButtonsContainer.gameObject.SetActive(true); // "surfaceBuildingPanel"
                    RewriteBuildingButtons();
                    mode = SurfacePanelMode.Build;
                    break;
                }
            case SurfacePanelMode.SelectAction:
                if (constructionPlane.activeSelf) SwitchConstructionPlane(false);
                buildIntersectionSubmit.SetActive(false);
                switch (mode)
                {
                    case SurfacePanelMode.Build: SetBuildPanelStatus(false); break;
                }

                SetActionPanelStatus(true);                
                mode = SurfacePanelMode.SelectAction;
                CheckGatherButton();
                ColonyController colony = GameMaster.colonyController;
                if (colony.gears_coefficient >= 2)
                {
                    changeMaterialButton.gameObject.SetActive(true);
                }
                else changeMaterialButton.gameObject.SetActive(false);
                columnCreateButton.gameObject.SetActive(IsColumnAvailable() & observingSurface.pos.y < Chunk.CHUNK_SIZE - 1);
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
                        if (rt.ID == observingSurface.material_id) continue;
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
                            Text tx = r.GetChild(0).GetComponent<Text>();
                            tx.text= Localization.GetResourceName(id) + " : " + rc[i].volume.ToString();
                            float[] storageResource = GameMaster.colonyController.storage.standartResources;
                            tx.color = (rc[i].volume > storageResource[rc[i].type.ID]) ? Color.red : Color.white;
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
                        observingSurface.ReplaceMaterial(rt.ID);
                        costPanel.transform.GetChild(0).GetChild(costPanel_selectedButton.x).GetComponent<Image>().overrideSprite = null;
                    }
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                break;
                case CostPanelMode.ColumnBuilding:
                    if (GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(ResourcesCost.GetCost(Structure.COLUMN_ID)))
                    {
                        float supportPoints = observingSurface.myChunk.CalculateSupportPoints(observingSurface.pos.x, observingSurface.pos.y, observingSurface.pos.z);
                        if (supportPoints <= 1)
                        {
                            Structure s = Structure.GetNewStructure(Structure.COLUMN_ID);
                            s.SetBasement(observingSurface, new PixelPosByte(7, 7));
                            PoolMaster.current.BuildSplash(observingSurface.transform.position);
                        }
                        else
                        {
                            observingSurface.myChunk.ReplaceBlock(observingSurface.pos, BlockType.Cave, observingSurface.material_id, ResourceType.CONCRETE_ID, false);
                        }
                    }
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
               
                break;
            case CostPanelMode.BlockBuilding:
                BlockBuildingSite bbs = observingSurface.gameObject.GetComponent<BlockBuildingSite>();
                if (bbs == null) bbs = observingSurface.gameObject.AddComponent<BlockBuildingSite>();
                bbs.Set(observingSurface, ResourceType.GetResourceTypeById(costPanel_selectedButton.y));
                SetCostPanelMode(CostPanelMode.Disabled);
                UIController.current.ShowWorksite(bbs);
                break;
        }
    }

    void SetBuildPanelStatus ( bool working ) {
		buildingsLevelToggles[0].transform.parent.gameObject.SetActive(working);
		returnButton.gameObject.SetActive( working );
		surfaceBuildingPanel.SetActive( working );       
        if (!working) {
            infoPanel.SetActive(false); // включаются по select building
            if (selectedBuildingButton != -1) {
                buildingButtonsContainer.GetChild(selectedBuildingButton).GetComponent<Image>().overrideSprite = null;
				selectedBuildingButton = -1;
				chosenStructure = null;
			}
		}
        else
        {
            if (costPanelMode != CostPanelMode.Disabled) SetCostPanelMode(CostPanelMode.Disabled);
        }
	}
	void SetActionPanelStatus ( bool working ) {
		buildButton.gameObject.SetActive( working  );
		digButton.gameObject.SetActive( working  );
        gatherButton.gameObject.SetActive(working);
		if (working) {
            CheckGatherButton();

            CleanSite cs = observingSurface.GetComponent<CleanSite>();
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
			blockCreateButton.gameObject.SetActive(IsBlockCreatingAvailable());
			columnCreateButton.gameObject.SetActive(IsColumnAvailable() && observingSurface.pos.y < Chunk.CHUNK_SIZE - 1);
            changeMaterialButton.gameObject.SetActive(IsChangeSurfaceMaterialAvalable());
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

    bool IsColumnAvailable()
    {
        return (hq.level > 2);
    }
    bool IsBlockCreatingAvailable()
    {
        return ((hq.level > 4) & (GameMaster.colonyController.gears_coefficient == 3));
    }
    bool IsChangeSurfaceMaterialAvalable()
    {
        return (GameMaster.colonyController.gears_coefficient >= 2);
    }
    #endregion

    #region building construction 


    public void SwitchConstructionPlane()
    {
        SwitchConstructionPlane(!constructionPlane.activeSelf);
    }
    public void SwitchConstructionPlane(bool x)
    {
        if (x) // enable
        {
            constructionPlaneSwitchButton.image.overrideSprite = UIController.current.overridingSprite;
            PrepareConstructionPlane();            
        }
        else
        {
            constructionPlaneSwitchButton.image.overrideSprite = null;
            constructionPlane.SetActive(false);
            UIController.current.interceptingConstructPlaneID = -1;
            if (mode == SurfacePanelMode.Build) surfaceBuildingPanel.SetActive(true);
        }
    }

    private void PrepareConstructionPlane()
    {
        surfaceBuildingPanel.SetActive(false);
        constructionPlane.transform.position = observingSurface.transform.position + Vector3.down * 0.45f;
        constructionPlane.transform.rotation = observingSurface.transform.rotation;
        constructionPlane.SetActive(true);        
        constructingPlaneMaterial.SetTexture("_MainTex", observingSurface.GetMapTexture());
        UIController.current.interceptingConstructPlaneID = constructionPlane.GetInstanceID();
        constructionPlane.SetActive(true);
    }

    public void ReturnButton()
    {
        if (surfaceBuildingPanel.activeSelf)
        {
            ChangeMode(SurfacePanelMode.SelectAction);
        }
        else
        {
            SwitchConstructionPlane(false);
        }
    }

    public void SelectBuildingForConstruction (Structure building, int buttonIndex) {
		chosenStructure = building;
		buildingButtonsContainer.GetChild(buttonIndex).GetComponent<Image>().overrideSprite = UIController.current.overridingSprite; 
		if (selectedBuildingButton >= 0) buildingButtonsContainer.GetChild(selectedBuildingButton).GetComponent<Image>().overrideSprite = null;
		selectedBuildingButton = buttonIndex;

        infoPanel.SetActive(true);
		nameField.text = Localization.GetStructureName(chosenStructure.id);
        gridTextField.text = chosenStructure.innerPosition.x_size.ToString() + " x " + chosenStructure.innerPosition.z_size.ToString();
        Building b = chosenStructure as Building;
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
        description.text = Localization.GetStructureDescription(chosenStructure.id);
        

		bool sideBlock = ( observingSurface.pos.x == 0 | observingSurface.pos.z == 0 | observingSurface.pos.x == Chunk.CHUNK_SIZE - 1 | observingSurface.pos.z == Chunk.CHUNK_SIZE - 1 );
		resourcesCostImage[0].transform.parent.gameObject.SetActive(true);
		Text t = resourcesCostImage[0].transform.GetChild(0).GetComponent<Text>();
        bool allConditionsMet = false;
        //side block check :
        if (chosenStructure.borderOnlyConstruction)
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
                if (observingSurface.pos.x == 0)
                {
                    if (observingSurface.pos.z == 0) side = 2;
                }
                else
                {
                    if (observingSurface.pos.x == Chunk.CHUNK_SIZE - 1) side = 1;
                    else side = 3;
                }

                if (observingSurface.myChunk.sideBlockingMap[observingSurface.pos.y, side] == false)
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
			Building bd = chosenStructure as Building;
			// material check :
			if (bd != null & bd.requiredBasementMaterialId != -1 & bd.requiredBasementMaterialId != observingSurface.material_id) {
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
				ResourceContainer[] cost = ResourcesCost.GetCost(chosenStructure.id);
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
                lastStorageStatus = GameMaster.colonyController.storage.operationsDone;
				buildingCreateMode = BuildingCreateInfoMode.Acceptable;
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
        chosenStructure = null;
        if (selectedBuildingButton >= 0) buildingButtonsContainer.GetChild(selectedBuildingButton).GetComponent<Image>().overrideSprite = null;
        selectedBuildingButton = -1;
        infoPanel.SetActive(false);
    }

	public void CreateSelectedBuilding () {
        if (chosenStructure.fullCover )
        {
            if (observingSurface.artificialStructures == 0) CreateSelectedBuilding(0,0);
            else buildIntersectionSubmit.SetActive(true);
        }
        else
        {
            if (chosenStructure is Farm)
            {
                int size = SurfaceBlock.INNER_RESOLUTION;
                SurfaceRect sr = chosenStructure.innerPosition;
                CreateSelectedBuilding((byte)(size/ 2 - sr.x_size/2), (byte)(size / 2 - sr.z_size/2));
            }
            else    PrepareConstructionPlane(); // включает плоскость, отключает окно выбора строений
        }
    }

    public void IntersectionSubmit_Yes()
    {
        CreateSelectedBuilding((byte)constructingPlaneTouchPos.x ,(byte)constructingPlaneTouchPos.y);
        buildIntersectionSubmit.SetActive(false);
    }
    // public void IntersectionSubmit_No() - just deactivate the panel 
    public void ConstructingPlaneTouch(Vector3 pos)
    {
        Vector2 mappos = observingSurface.WorldToMapCoordinates(pos);
        byte size = SurfaceBlock.INNER_RESOLUTION;
        byte x = (byte)(mappos.x * size), z = (byte)(mappos.y * size);
        if (chosenStructure == null) return;
        SurfaceRect sr = chosenStructure.innerPosition;
        if (x + sr.x_size >= size)
        {
            // корректировка
            x = (byte)(size - sr.x_size);
        }
        if (z + chosenStructure.innerPosition.z_size >= size)
        {
            // корректировка
            z = (byte)(size - sr.z_size);
        }
        constructingPlaneTouchPos = new Vector2Int(x, z);
        if ( observingSurface.IsAnyBuildingInArea(new SurfaceRect(x,z,sr.x_size, sr.z_size)))
        {
            buildIntersectionSubmit.SetActive(true);            
        }
        else  CreateSelectedBuilding(x, z);
    }


	public void CreateSelectedBuilding (byte x, byte z) {
		ResourceContainer[] cost = ResourcesCost.GetCost(chosenStructure.id);
		if (GameMaster.colonyController.storage.CheckSpendPossibility(cost)) {
           GameMaster.colonyController.storage.GetResources(cost);
           Structure s = Structure.GetNewStructure(chosenStructure.id);
		   s.gameObject.SetActive(true);
		    s.SetBasement(observingSurface, new PixelPosByte(x,z));
           PoolMaster.current.BuildSplash(observingSurface.transform.position);
            if (constructionPlane.activeSelf)
            {
                PrepareConstructionPlane();
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
		for (int n = 0; n < buildingButtonsContainer.childCount; n++) {
            GameObject g = buildingButtonsContainer.GetChild(n).gameObject;
            if (n < abuildings.Count) {
				g.SetActive(true);
				RawImage rimage = buildingButtonsContainer.GetChild(n).GetChild(0).GetComponent<RawImage>();
                rimage.uvRect = Structure.GetTextureRect(abuildings[n].id);
                Button b = g.GetComponent<Button>();
				b.onClick.RemoveAllListeners();
				int bid = n;
				b.onClick.AddListener(() => {
					this.SelectBuildingForConstruction(abuildings[bid], bid);
				});
			}
			else {
				g.SetActive(false);
			}
		}
	}
    #endregion

    void LocalizeButtonTitles()
    {
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Build);

        Transform t = buildIntersectionSubmit.transform;
        t.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.Ask_DestroyIntersectingBuildings);
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Accept);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);
    }
}
