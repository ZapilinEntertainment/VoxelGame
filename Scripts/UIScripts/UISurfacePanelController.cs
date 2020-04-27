using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CostPanelMode : byte { Disabled, ColumnBuilding, SurfaceMaterialChanging, BlockBuilding }
public enum SurfacePanelMode : byte {SelectAction, Build}


public sealed class UISurfacePanelController : UIObserver {
    private enum BuildingCreateInfoMode : byte { Acceptable, Unacceptable_SideBlock, Unacceptable_Material }

    public Button buildButton, gatherButton, digButton, blockCreateButton, columnCreateButton, changeMaterialButton;
    private Plane observingSurface;
    private bool status_digOrdered = false, firstSet = true;
    private bool? status_gather = null;
    private byte savedHqLevel = 0;
    private int[] applicableBuildingsList;
    private Vector2[] showingResourcesCount;

    private int selectedBuildingButton = -1, lastStorageStatus = -1;
    private Vector2Int costPanel_selectedButton = new Vector2Int(-1,-1);
    private int selectedStructureID = Structure.EMPTY_ID;
	private byte constructingLevel = 1;

    private SurfacePanelMode mode;
    private CostPanelMode costPanelMode;
	private BuildingCreateInfoMode buildingCreateMode;
    private Vector2Int constructingPlaneTouchPos;

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

    public static UISurfacePanelController InitializeSurfaceObserverScript()
    {
        if (current != null) return current;
        else
        {
            current = Instantiate(Resources.Load<GameObject>("UIPrefs/surfaceObserver"), UIController.current.mainCanvas).GetComponent<UISurfacePanelController>();            
            current.LocalizeTitles();
            return current;
        }
    }

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

    public void SetObservingSurface(Plane sb) {
		if (sb == null || sb.destroyed) {
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
		}
	}

    override public void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingSurface == null || observingSurface.destroyed)
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

                    bool check = false;
                    if (observingSurface.haveWorksite)
                    {
                        var w = colony.GetWorksite(observingSurface);
                        if (w != null)
                        {
                            var cs = w as CleanSite;
                            check = (cs != null && cs.diggingMission);
                        }
                    }
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
                    if (selectedStructureID != Structure.EMPTY_ID)
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
		if (observingSurface == null || observingSurface.destroyed) {
			SelfShutOff();
			return;
		}
		else {
            bool newGatherSite = true;
            if (observingSurface.haveWorksite)
            {
                var w = colony.GetWorksite(observingSurface);
                if (w != null && w is GatherSite)
                {
                    w.StopWork(true);
                    newGatherSite = false;
                }
            }
            if (newGatherSite)
            {
                var gs = new GatherSite(observingSurface);
                UIController.current.ShowWorksite(gs);
            }
			StatusUpdate();
		}
	}
	public void DigButton() {
		if (observingSurface == null || observingSurface.destroyed) {
			SelfShutOff();
			return;
		}
		else {
            bool newCleanSite = false;
            if (observingSurface.isSurface)
            {
                newCleanSite = observingSurface.fulfillStatus != FullfillStatus.Empty;
                if (observingSurface.haveWorksite)
                {
                    var w = colony.GetWorksite(observingSurface);
                    if (w != null && w is CleanSite)
                    {
                        if ((w as CleanSite).diggingMission)
                        {
                            w.StopWork(true);
                            newCleanSite = false;
                        }
                    }
                }
                
            }
            if (newCleanSite)
            {
                var cs = new CleanSite(observingSurface, true);
                UIController.current.ShowWorksite(cs);
            }
            else
            {
                var ds = new DigSite(observingSurface, true);
                UIController.current.ShowWorksite(ds);
            }
            StatusUpdate();
		}
	}

    void CheckGatherButton()
    {
        Worksite w = colony.GetWorksite(observingSurface) as GatherSite;
        if ( w is GatherSite )
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
            if (observingSurface.fulfillStatus != FullfillStatus.Empty)
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
                    //applicableBuildingsList = Building.GetApplicableBuildingsList(hq.level)
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
                changeMaterialButton.gameObject.SetActive(IsChangeSurfaceMaterialAvalable());
                columnCreateButton.gameObject.SetActive(IsColumnAvailable() & observingSurface.pos.y < Chunk.chunkSize);
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
                        if (rt.ID == observingSurface.materialID) continue;
                        t = buttonsKeeper.GetChild(lastUsedIndex);
                        t.gameObject.SetActive(true);
                        RawImage ri = t.GetChild(0).GetComponent<RawImage>();
                        ri.texture = UIController.current.resourcesIcons;
                        ri.uvRect = ResourceType.GetResourceIconRect(rt.ID);
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
                            r.GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(id);
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
                        c.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(id);
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
        t.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Build) + " (" + (costPanelMode == CostPanelMode.SurfaceMaterialChanging ? PlaneExtension.INNER_RESOLUTION * PlaneExtension.INNER_RESOLUTION : BlockExtension.MAX_VOLUME ) + ')';
    }
    public void CostPanel_Build()
    {        
        switch (costPanelMode)
        {
            case CostPanelMode.SurfaceMaterialChanging:
                    ResourceType rt = ResourceType.GetResourceTypeById(costPanel_selectedButton.y);
                if (colony.storage.CheckBuildPossibilityAndCollectIfPossible(new ResourceContainer[] { new ResourceContainer(rt, PlaneExtension.INNER_RESOLUTION * PlaneExtension.INNER_RESOLUTION) }))
                {
                    observingSurface.ChangeMaterial(rt.ID, true);
                    costPanel.transform.GetChild(0).GetChild(costPanel_selectedButton.x).GetComponent<Image>().overrideSprite = null;
                }
                else GameLogUI.NotEnoughResourcesAnnounce();
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

                        Plane p;
                        if ((s as IPlanable).TryGetPlane(observingSurface.faceIndex, out p) && !p.isTerminate)
                        {
                            UIController.current.Select(p);
                        }
                        else ReturnButton();
                        // }
                        //   else
                        //  {
                        //    observingSurface.myChunk.ReplaceBlock(observingSurface.pos, BlockType.Cave, observingSurface.material_id, ResourceType.CONCRETE_ID, false);
                        // }
                    }
                    else GameLogUI.NotEnoughResourcesAnnounce();
                }
                break;
            case CostPanelMode.BlockBuilding:
                BlockBuildingSite bbs = new BlockBuildingSite (observingSurface, ResourceType.GetResourceTypeById(costPanel_selectedButton.y));
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
                selectedStructureID = Structure.EMPTY_ID;
			}
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
            CleanSite cs = colony.GetWorksite(observingSurface) as CleanSite;
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
			columnCreateButton.gameObject.SetActive(IsColumnAvailable() && observingSurface.pos.y < Chunk.chunkSize - 1);
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
            if (mode == SurfacePanelMode.Build)
            {
                surfaceBuildingPanel.SetActive(true);
                FollowingCamera.main.CameraRotationBlock(true);
            }
        }
    }

    private void PrepareConstructionPlane()
    {
        surfaceBuildingPanel.SetActive(false);
        var t = constructionPlane.transform;
        t.position = observingSurface.GetCenterPosition();
        t.rotation = Quaternion.Euler(observingSurface.GetEulerRotationForQuad());
        t.position += observingSurface.GetLookVector() * 0.01f;
        constructingPlaneMaterial.SetTexture("_MainTex", observingSurface.FORCED_GetExtension().GetMapTexture());
        UIController.current.interceptingConstructPlaneID = constructionPlane.GetInstanceID();
        constructionPlane.SetActive(true);
        if (selectedStructureID != Structure.EMPTY_ID) FollowingCamera.main.CameraRotationBlock(false);
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

    public void SelectBuildingForConstruction (int i_structureID, int buttonIndex) {
        selectedStructureID = i_structureID;
		buildingButtonsContainer.GetChild(buttonIndex).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; 
		if (selectedBuildingButton >= 0) buildingButtonsContainer.GetChild(selectedBuildingButton).GetComponent<Image>().overrideSprite = null;
		selectedBuildingButton = buttonIndex;

        infoPanel.SetActive(true);
		nameField.text = Localization.GetStructureName(selectedStructureID);
        var sts = Structure.GetStructureSize(selectedStructureID).ToString();
        gridTextField.text = sts + " x " + sts;

        var stype = Structure.GetTypeByID(selectedStructureID);
        var btype = typeof(Building);
        if (stype == btype || stype.IsSubclassOf(btype))
        {
            var energySurplus = Building.GetEnergySurplus(selectedStructureID);
            if (energySurplus != 0)
            {
                energyIcon.SetActive(true);
                energyTextField.gameObject.SetActive(true);
                energyTextField.text = energySurplus > 0 ? '+' + energySurplus.ToString() :  energySurplus.ToString();
            }
            else
            {
                energyIcon.SetActive(false);
                energyTextField.gameObject.SetActive(false);
            }
            var htype = typeof(House);
            if (stype == htype || stype.IsSubclassOf(htype))
            {
                housingIcon.SetActive(true);
                housingTextField.gameObject.SetActive(true);
                housingTextField.text = House.GetHousingValue(selectedStructureID).ToString();
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
        description.text = Localization.GetStructureDescription(selectedStructureID);
        (description.transform.parent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, description.rectTransform.rect.height);
        
		resourcesCostImage[0].transform.parent.gameObject.SetActive(true);
		Text t = resourcesCostImage[0].transform.GetChild(0).GetComponent<Text>();
	
            string reason = "UNACCEPTABLE!";
        bool acceptable = Structure.CheckSpecialBuildingConditions(selectedStructureID, observingSurface, ref reason);
            if (!acceptable)
            {
                t.text = reason;
                t.color = Color.yellow;
                resourcesCostImage[0].uvRect = ResourceType.GetResourceIconRect(0);
                resourcesCostImage[0].gameObject.SetActive(true);
                for (int i = 1; i < resourcesCostImage.Length; i++)
                {
                    resourcesCostImage[i].gameObject.SetActive(false);
                }
                buildingCreateMode = BuildingCreateInfoMode.Unacceptable_Material;
                innerBuildButton.gameObject.SetActive(false);
            }
            else
            {
                // all conditions met
                ResourceContainer[] cost;
                if (selectedStructureID != Structure.SETTLEMENT_CENTER_ID) cost = ResourcesCost.GetCost(selectedStructureID);
                else cost = ResourcesCost.GetSettlementUpgradeCost(constructingLevel);
                //resource cost drawing
                float[] storageResources = colony.storage.standartResources;
                for (int i = 0; i < resourcesCostImage.Length; i++)
                {
                    if (i < cost.Length)
                    {
                        resourcesCostImage[i].uvRect = ResourceType.GetResourceIconRect(cost[i].type.ID);
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
    void DeselectBuildingButton()
    {
        selectedStructureID = Structure.EMPTY_ID;
        if (selectedBuildingButton >= 0) buildingButtonsContainer.GetChild(selectedBuildingButton).GetComponent<Image>().overrideSprite = null;
        selectedBuildingButton = -1;
        infoPanel.SetActive(false);
    }

    //inner build button function
	public void CreateSelectedBuilding () {
        var size = Structure.GetStructureSize(selectedStructureID);
        if (Structure.PlaceInCenter(selectedStructureID) )
        {
            CreateSelectedBuilding( (byte)(PlaneExtension.INNER_RESOLUTION /2 - size/2), (byte)(PlaneExtension.INNER_RESOLUTION/ 2 - size/2), true );
        }
        else PrepareConstructionPlane(); // включает плоскость, отключает окно выбора строений
    }
    // end build request
    public void CreateSelectedBuilding(byte x, byte z, bool checkForIntersections)
    {
        if (observingSurface == null || observingSurface.destroyed) {
            SelfShutOff();
            return;
        }
        ResourceContainer[] cost;
        if (selectedStructureID != Structure.SETTLEMENT_CENTER_ID) cost = ResourcesCost.GetCost(selectedStructureID);
        else cost = ResourcesCost.GetSettlementUpgradeCost(constructingLevel);
        if (colony.storage.CheckSpendPossibility(cost))
        {
            byte strSize = Structure.GetStructureSize(selectedStructureID),  res = PlaneExtension.INNER_RESOLUTION;
            if (x + strSize > res) x = (byte)(res - strSize);
            if (z + strSize > res) z = (byte)(res - strSize);
            if (checkForIntersections && observingSurface.IsAnyBuildingInArea(new SurfaceRect(x, z, strSize)))
            {
                constructingPlaneTouchPos = new Vector2Int(x, z);
                buildIntersectionSubmit.SetActive(true);
                return;
            }
            else
            {
                colony.storage.GetResources(cost);
                Structure s = Structure.GetStructureByID(selectedStructureID);
                byte rt = 0;
                if (s.rotate90only)
                {
                    rt = (byte)(Random.value * 3);
                    rt *= 2;
                }
                else
                {
                    rt = (byte)(Random.value * 7);
                }
                if (s.ID == Structure.SETTLEMENT_CENTER_ID)
                {
                    (s as Settlement).SetLevel(constructingLevel);
                }
                s.SetBasement(observingSurface, new PixelPosByte(x, z));
                //if (!(s is Dock) & !(s is Hangar)) s.SetModelRotation(rt);
                PoolMaster.current.BuildSplash(s.transform.position);
                GameMaster.realMaster.eventTracker?.BuildingConstructed(s);
                if (observingSurface.fulfillStatus != FullfillStatus.Empty)
                {
                    if (constructionPlane.activeSelf)
                    {
                        PrepareConstructionPlane();
                    }
                    /*
                    if (strSize == res | Structure.PlaceInCenter(selectedStructureID)) {                        
                        if (s is IPlanable)
                        {
                            var ip = s as IPlanable;
                            Plane p;
                            if (ip.TryGetPlane(observingSurface.faceIndex, out p) && !p.isTerminate)
                            {
                                var sbb = selectedBuildingButton;
                                UIController.current.Select(p);                                
                                BuildButton();
                                SelectBuildingForConstruction(selectedStructureID, sbb);
                            }
                            else ReturnButton();
                        }
                        else   ReturnButton();                        
                    }
                    */
                    ReturnButton();
                }
                else ReturnButton();
            }
        }
        else
        {
            GameLogUI.NotEnoughResourcesAnnounce();
        }
    }

    public void IntersectionSubmit_Yes()
    {
        CreateSelectedBuilding((byte)constructingPlaneTouchPos.x, (byte)constructingPlaneTouchPos.y, false);
        buildIntersectionSubmit.SetActive(false);
    }
    // public void IntersectionSubmit_No() - just deactivate the panel 
    public void ConstructingPlaneTouch(Vector3 pos)
    {
        if (buildIntersectionSubmit.activeSelf | selectedStructureID == Structure.EMPTY_ID | observingSurface == null) return;
        Vector2 mappos = observingSurface.WorldToMapPosition(pos);
        CreateSelectedBuilding((byte)(mappos.x * PlaneExtension.INNER_RESOLUTION), (byte)(mappos.y * PlaneExtension.INNER_RESOLUTION), true);
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
		var abuildings = Building.GetApplicableBuildingsList(constructingLevel, observingSurface.faceIndex);
        if (abuildings != null)
        {
            for (int n = 0; n < buildingButtonsContainer.childCount; n++)
            {
                GameObject g = buildingButtonsContainer.GetChild(n).gameObject;
                if (n < abuildings.Length)
                {
                    g.SetActive(true);
                    RawImage rimage = buildingButtonsContainer.GetChild(n).GetChild(0).GetComponent<RawImage>();
                    rimage.uvRect = Structure.GetTextureRect(abuildings[n]);
                    Button b = g.GetComponent<Button>();
                    b.onClick.RemoveAllListeners();
                    int bid = n;
                    b.onClick.AddListener(() =>
                    {
                        this.SelectBuildingForConstruction(abuildings[bid], bid);
                    });
                }
                else
                {
                    g.SetActive(false);
                }
            }
        }
	}
    #endregion

    /// <summary>
	/// Call from outside
	/// </summary>
	override public void ShutOff()
    {
        if (!GameMaster.loading)
        {
            isObserving = false;
            if (constructionPlane != null) constructionPlane.SetActive(false);
            if (gameObject == null) Object.Destroy(this);
            else gameObject.SetActive(false);
        }
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
        transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Gather);

        Transform t = buildIntersectionSubmit.transform;
        t.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.Ask_DestroyIntersectingBuildings);
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Accept);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);

        changeMaterialButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ChangeSurfaceMaterial);
        columnCreateButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.CreateColumn);
        blockCreateButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.CreateBlock);

        innerBuildButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Build);
        returnButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Return);
    }

    new private void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        if (subscribedToUpdate)
        {
            UIController uc = UIController.current;
            if (uc != null) uc.statusUpdateEvent -= StatusUpdate;
        }
        if (current == this) current = null;
    }
}
