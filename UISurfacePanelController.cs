using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CostPanelMode : byte { Disabled, ColumnBuilding, SurfaceMaterialChanging, BlockBuilding }
public enum SurfacePanelMode : byte {SelectAction, Build}
enum BuildingCreateInfoMode : byte {Acceptable, Unacceptable_SideBlock, Unacceptable_Material}

public sealed class UISurfacePanelController : UIObserver {
	public Button buildButton, gatherButton, digButton, blockCreateButton, columnCreateButton, changeMaterialButton;
    SurfaceBlock observingSurface;
    bool status_digOrdered = false, firstSet = true;
    bool? status_gather = null;
	byte savedHqLevel = 0;
	Vector2[] showingResourcesCount;
    int selectedBuildingButton = -1, lastStorageStatus = -1;
    Vector2Int costPanel_selectedButton = new Vector2Int(-1,-1);
	Structure chosenStructure = null;
	public byte constructingLevel = 1;
	SurfacePanelMode mode;
    CostPanelMode costPanelMode;
	BuildingCreateInfoMode buildingCreateMode;
    Vector2Int constructingPlaneTouchPos;
    private Transform exampleBuildingsContainer;

#pragma warning disable 0649
    [SerializeField] Material constructingPlaneMaterial;
    [SerializeField] GameObject buildIntersectionSubmit, constructionPlane; // window asking about deleting overlapping buildings
    [SerializeField] Transform buildingButtonsContainer; // fiti
	[SerializeField] Button innerBuildButton, returnButton, constructionPlaneSwitchButton; // fiti
	[SerializeField] GameObject surfaceBuildingPanel, infoPanel, energyIcon, housingIcon, costPanel; // fiti
#pragma warning restore 0649
    public Toggle[] buildingsLevelToggles; // fiti
    public Text nameField, description, gridTextField, energyTextField, housingTextField; // fiti
    public RawImage[] resourcesCostImage; // fiti
    private HeadQuarters hq;
    private ColonyController colony;

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
            constructionPlane.transform.localScale = Vector3.one * Block.QUAD_SIZE;
            //constructionPlane.transform.GetChild(0).GetComponent<MeshFilter>().mesh.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
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
            colony = GameMaster.realMaster.colonyController;
			hq = colony.hq;
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
                    hq = colony.hq;
                    CheckGatherButton();

                    CleanSite cs = observingSurface.worksite as CleanSite;
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
                                Storage storage = colony.storage;
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
			GatherSite gs = observingSurface.worksite as GatherSite;
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
                CleanSite cs = observingSurface.worksite as CleanSite;
                if (cs == null)
                {
                    cs = observingSurface.gameObject.AddComponent<CleanSite>();
                    cs.Set(observingSurface, true);
                    UIController.current.ShowWorksite(cs);
                }
                else
                {
                    if (cs.diggingMission) cs.StopWork();
                }
			StatusUpdate(); timer = STATUS_UPDATE_TIME;
		}
	}

    void CheckGatherButton()
    {
        if ( observingSurface.worksite is GatherSite )
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
                    hq = colony.hq;
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
                if (colony.gears_coefficient >= 2)
                {
                    changeMaterialButton.gameObject.SetActive(true);
                }
                else changeMaterialButton.gameObject.SetActive(false);
                columnCreateButton.gameObject.SetActive(IsColumnAvailable() & observingSurface.pos.y < Chunk.CHUNK_SIZE);
                blockCreateButton.gameObject.SetActive(colony.hq.level > 5);                
                break;
        }
    }

    #region panels setting    

    public void SetCostPanelMode(CostPanelMode m)
    {
        if (m != CostPanelMode.Disabled)
        {
            costPanel.gameObject.SetActive(true);
            UIController.current.ChangeActiveWindow(ActiveWindowMode.SpecificBuildPanel);
        }
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
            UIController.current.DropActiveWindow(ActiveWindowMode.SpecificBuildPanel);
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
                    changeMaterialButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    columnCreateButton.GetComponent<Image>().overrideSprite = null;
                    blockCreateButton.GetComponent<Image>().overrideSprite = null;
                    Transform buttonsKeeper = costPanel.transform.GetChild(0);
                    buttonsKeeper.gameObject.SetActive(true);
                    costPanel.transform.GetChild(1).gameObject.SetActive(false);
                    costPanel.transform.GetChild(2).gameObject.SetActive(false);
                    int lastUsedIndex = 0;                    
                    foreach (ResourceType rt in ResourceType.materialsForCovering)
                    {
                        if (rt.ID == observingSurface.material_id) continue;
                        t = buttonsKeeper.GetChild(lastUsedIndex);
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
                    if (lastUsedIndex < buttonsKeeper.childCount)
                    {                        
                        for (; lastUsedIndex < buttonsKeeper.childCount; lastUsedIndex++)
                        {
                            buttonsKeeper.GetChild(lastUsedIndex).gameObject.SetActive(false);
                        }
                    }
                    costPanel.transform.GetChild(2).gameObject.SetActive(false); // build button
                }
                break;

            case CostPanelMode.ColumnBuilding:
                if (costPanelMode != CostPanelMode.ColumnBuilding)
                {
                    columnCreateButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
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
                            float[] storageResource = colony.storage.standartResources;
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
                    blockCreateButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
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
        if (costPanel_selectedButton.x != -1)
        {
            costPanel.transform.GetChild(0).GetChild(costPanel_selectedButton.x).GetComponent<Image>().overrideSprite = null;
        }
        costPanel_selectedButton = indexes;
        costPanel.transform.GetChild(0).GetChild(indexes.x).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
        Transform t = costPanel.transform.GetChild(2);// build button
        t.gameObject.SetActive(true);
        t.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Build) + " (" + (costPanelMode == CostPanelMode.SurfaceMaterialChanging ? SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION : CubeBlock.MAX_VOLUME ) + ')';
    }
    public void CostPanel_Build()
    {        
        switch (costPanelMode)
        {
            case CostPanelMode.SurfaceMaterialChanging:
                    ResourceType rt = ResourceType.GetResourceTypeById(costPanel_selectedButton.y);
                    if (colony.storage.CheckBuildPossibilityAndCollectIfPossible(new ResourceContainer[] { new ResourceContainer(rt, SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION) }))
                    {
                        observingSurface.ReplaceMaterial(rt.ID);
                        costPanel.transform.GetChild(0).GetChild(costPanel_selectedButton.x).GetComponent<Image>().overrideSprite = null;
                    }
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                break;
                case CostPanelMode.ColumnBuilding:
                {
                    if (colony.storage.CheckBuildPossibilityAndCollectIfPossible(ResourcesCost.GetCost(Structure.COLUMN_ID)))
                    {
                       // float supportPoints = observingSurface.myChunk.CalculateSupportPoints(observingSurface.pos.x, observingSurface.pos.y, observingSurface.pos.z);
                       // if (supportPoints <= 1)
                       // {
                            Structure s = Structure.GetStructureByID(Structure.COLUMN_ID);
                            s.SetBasement(observingSurface, new PixelPosByte(7, 7));
                            PoolMaster.current.BuildSplash(s.transform.position);
                        SetCostPanelMode(CostPanelMode.Disabled);
                        // }
                        //   else
                        //  {
                        //    observingSurface.myChunk.ReplaceBlock(observingSurface.pos, BlockType.Cave, observingSurface.material_id, ResourceType.CONCRETE_ID, false);
                        // }
                    }
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                }
                break;
            case CostPanelMode.BlockBuilding:
                BlockBuildingSite bbs = observingSurface.gameObject.AddComponent<BlockBuildingSite>();
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
            if (exampleBuildingsContainer != null) Destroy(exampleBuildingsContainer.gameObject);
            UIController.current.DropActiveWindow(ActiveWindowMode.BuildPanel);
		}
        else
        {
            if (costPanelMode != CostPanelMode.Disabled) SetCostPanelMode(CostPanelMode.Disabled);
            UIController.current.ChangeActiveWindow(ActiveWindowMode.BuildPanel);
        }
	}
	void SetActionPanelStatus ( bool working ) {
		buildButton.gameObject.SetActive( working  );
		digButton.gameObject.SetActive( working  );
        gatherButton.gameObject.SetActive(working);
		if (working) {
            CheckGatherButton();
            CleanSite cs = observingSurface.worksite as CleanSite;
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
        return ((hq.level > GameConstants.HQ_LEVEL_TO_CREATE_BLOCK) & (colony.gears_coefficient == GameConstants.GEARS_LEVEL_TO_CREATE_BLOCK));
    }
    bool IsChangeSurfaceMaterialAvalable()
    {
        return (colony.gears_coefficient >= GameConstants.GEARS_LEVEL_TO_CHANGE_SURFACE_MATERIAL);
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
            constructionPlaneSwitchButton.image.overrideSprite = PoolMaster.gui_overridingSprite;
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
		buildingButtonsContainer.GetChild(buttonIndex).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; 
		if (selectedBuildingButton >= 0) buildingButtonsContainer.GetChild(selectedBuildingButton).GetComponent<Image>().overrideSprite = null;
		selectedBuildingButton = buttonIndex;

        infoPanel.SetActive(true);
		nameField.text = Localization.GetStructureName(chosenStructure.id);
        gridTextField.text = chosenStructure.innerPosition.size.ToString() + " x " + chosenStructure.innerPosition.size.ToString();
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
        
		resourcesCostImage[0].transform.parent.gameObject.SetActive(true);
		Text t = resourcesCostImage[0].transform.GetChild(0).GetComponent<Text>();
	
			Building bd = chosenStructure as Building;
        if (bd != null)
        {
            bool acceptable = true;
            string reason = "UNACCEPTABLE!";
            if (bd.specialBuildingConditions)
            {                
                switch (bd.id)
                {
                    case Structure.FARM_1_ID:
                    case Structure.FARM_2_ID:
                    case Structure.FARM_3_ID:
                    case Structure.LUMBERMILL_1_ID:
                    case Structure.LUMBERMILL_2_ID:
                    case Structure.LUMBERMILL_3_ID:
                        int mid = observingSurface.material_id;
                        if (mid != ResourceType.DIRT_ID & mid != ResourceType.FERTILE_SOIL_ID) {
                            acceptable = false;
                            reason = Localization.GetRestrictionPhrase(RestrictionKey.UnacceptableSurfaceMaterial);
                        }
                        break;
                }                
            }
            if (!acceptable)
            {
                t.text = reason;
                t.color = Color.yellow;
                resourcesCostImage[0].uvRect = ResourceType.GetTextureRect(0);
                resourcesCostImage[0].gameObject.SetActive(true);
                for (int i = 1; i < resourcesCostImage.Length - 1; i++)
                {
                    resourcesCostImage[i].gameObject.SetActive(false);
                }
                //---deleted
                //int n = resourcesCostImage.Length - 1;
                //t = resourcesCostImage[n].transform.GetChild(0).GetComponent<Text>();
                //t.text = Localization.GetPhrase(LocalizedPhrase.RequiredSurface) + " : " + Localization.GetResourceName(bd.requiredBasementMaterialId);
                //resourcesCostImage[n].uvRect = ResourceType.GetTextureRect(bd.requiredBasementMaterialId);
                //resourcesCostImage[n].gameObject.SetActive(true);
                //t.color = Color.yellow;
                //---deleted
                buildingCreateMode = BuildingCreateInfoMode.Unacceptable_Material;
                innerBuildButton.gameObject.SetActive(false);
            }
            else
            {
                // all conditions met
                ResourceContainer[] cost = ResourcesCost.GetCost(chosenStructure.id);
                //resource cost drawing
                float[] storageResources = colony.storage.standartResources;
                for (int i = 0; i < resourcesCostImage.Length; i++)
                {
                    if (i < cost.Length)
                    {
                        resourcesCostImage[i].uvRect = ResourceType.GetTextureRect(cost[i].type.ID);
                        t = resourcesCostImage[i].transform.GetChild(0).GetComponent<Text>();
                        t.text = Localization.GetResourceName(cost[i].type.ID) + " : " + string.Format("{0:0.##}", cost[i].volume);
                        showingResourcesCount[i] = new Vector2(cost[i].type.ID, cost[i].volume);
                        if (storageResources[cost[i].type.ID] < cost[i].volume) t.color = Color.red; else t.color = Color.white;
                        resourcesCostImage[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        resourcesCostImage[i].gameObject.SetActive(false);
                    }
                }
                lastStorageStatus = colony.storage.operationsDone;
                buildingCreateMode = BuildingCreateInfoMode.Acceptable;
                innerBuildButton.gameObject.SetActive(true);
            }
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
        if (chosenStructure.placeInCenter )
        {
            if (observingSurface.artificialStructures == 0) CreateSelectedBuilding( (byte)(SurfaceBlock.INNER_RESOLUTION /2 - chosenStructure.innerPosition.size/2), (byte)(SurfaceBlock.INNER_RESOLUTION/ 2 - chosenStructure.innerPosition.size/2) );
            else buildIntersectionSubmit.SetActive(true);
        }
        else
        {
            if (chosenStructure is Farm)
            {
                int size = SurfaceBlock.INNER_RESOLUTION;
                SurfaceRect sr = chosenStructure.innerPosition;
                CreateSelectedBuilding((byte)(size/ 2 - sr.size/2), (byte)(size / 2 - sr.size/2));
            }
            else    PrepareConstructionPlane(); // включает плоскость, отключает окно выбора строений
        }
    }
    public void CreateSelectedBuilding(byte x, byte z)
    {
        ResourceContainer[] cost = ResourcesCost.GetCost(chosenStructure.id);
        if (colony.storage.CheckSpendPossibility(cost))
        {
            colony.storage.GetResources(cost);
            Structure s = Structure.GetStructureByID(chosenStructure.id);
            s.SetBasement(observingSurface, new PixelPosByte(x, z));
            PoolMaster.current.BuildSplash(s.transform.position);
            if (s.innerPosition.size != SurfaceBlock.INNER_RESOLUTION & observingSurface.cellsStatus != 0)
            {
                if (constructionPlane.activeSelf)
                {
                    PrepareConstructionPlane();
                }
                if (s.placeInCenter) ReturnButton();
            }
            else ReturnButton();
        }
        else
        {
            UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
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
        // корректировка :
        if (x + sr.size >= size)
        {            
            x = (byte)(size - sr.size);
        }
        if (z + chosenStructure.innerPosition.size >= size)
        {
            z = (byte)(size - sr.size);
        }
        constructingPlaneTouchPos = new Vector2Int(x, z);

        if ( observingSurface.IsAnyBuildingInArea(new SurfaceRect(x,z,sr.size)))
        {
            buildIntersectionSubmit.SetActive(true);            
        }
        else  CreateSelectedBuilding(x, z);
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
        if (exampleBuildingsContainer != null) Destroy(exampleBuildingsContainer.gameObject);
		List<Building> abuildings = Building.GetApplicableBuildingsList(constructingLevel);
        exampleBuildingsContainer = new GameObject("example buildings container").transform;
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
                abuildings[n].transform.parent = exampleBuildingsContainer;
			}
			else {
				g.SetActive(false);
			}
		}
	}
    #endregion

    /// <summary>
	/// Call from outside
	/// </summary>
	override public void ShutOff()
    {
        isObserving = false;
        if (constructionPlane.activeSelf) constructionPlane.SetActive(false);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Call from inheritors
    /// </summary>
    override public void SelfShutOff()
    {
        isObserving = false;
        if (constructionPlane.activeSelf) constructionPlane.SetActive(false);
        gameObject.SetActive(false);
    }

    void LocalizeButtonTitles()
    {
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Build);

        Transform t = buildIntersectionSubmit.transform;
        t.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.Ask_DestroyIntersectingBuildings);
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Accept);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);
    }
}
