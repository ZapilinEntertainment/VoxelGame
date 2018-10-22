using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum HangarObserverMode { NoShuttle, BuildingShuttle, ShuttleInside, ShuttleOnMission }

public class UIHangarObserver : UIObserver {
    Hangar observingHangar;
    [SerializeField] RawImage mainShuttleIcon; // fiti
    [SerializeField] InputField shuttleNameTextField; // fiti
    [SerializeField] Button constructButton, disassembleButton, repairButton; // fiti
    [SerializeField] Text shuttleStatusText; // fiti
    [SerializeField] RectTransform progressBar;//fiti
    [SerializeField] Transform resourceCostContainer;
    Vector2[] showingResourcesCount;
    int savedProgressBarValue = 100;
    float fullProgressBarLength = -1, startOffset = 0;
    HangarObserverMode mode;

    public static UIHangarObserver InitializeHangarObserverScript()
    {
        UIHangarObserver uho = Instantiate(Resources.Load<GameObject>("UIPrefs/hangarObserver"), UIController.current.rightPanel.transform).GetComponent<UIHangarObserver>();
        Hangar.hangarObserver = uho;
        return uho;
    }

    private void Awake()
    {
        showingResourcesCount = new Vector2[resourceCostContainer.childCount];
    }

    public void SetObservingHangar(Hangar h)
    {
        if (fullProgressBarLength == -1)
        {
            fullProgressBarLength = progressBar.rect.width;
            startOffset = progressBar.offsetMin.x;
        }
        if (h == null)
        {
            SelfShutOff();
            return;
        }
        UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
        if (uwb == null) uwb = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else uwb.gameObject.SetActive(true);
        observingHangar = h; isObserving = true;
        uwb.SetObservingWorkBuilding(h);

        PrepareHangarWindow();

        STATUS_UPDATE_TIME = 1f; timer = STATUS_UPDATE_TIME;
    }

    override protected void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingHangar == null) SelfShutOff();
        else
        {
            if (observingHangar.shuttle == null)
            {
                if (mode != HangarObserverMode.NoShuttle) { PrepareHangarWindow(); }
                else
                {
                    float progress = observingHangar.workflow / observingHangar.workflowToProcess;
                    if (savedProgressBarValue != (int)(progress * 100))
                    {
                        savedProgressBarValue = (int)(progress * 100);
                        shuttleStatusText.text = savedProgressBarValue.ToString() + '%';
                        progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset, savedProgressBarValue / 100f * fullProgressBarLength);
                    }

                    float[] onStorage = GameMaster.colonyController.storage.standartResources;
                    for (int i = 0; i < resourceCostContainer.childCount; i++)
                    {
                        int rid = (int)showingResourcesCount[i].x;
                        if (onStorage[rid] != showingResourcesCount[i].y)
                        {
                            resourceCostContainer.GetChild(i).GetChild(0).GetComponent<Text>().color = onStorage[rid] < showingResourcesCount[i].y ? Color.red : Color.white;
                            showingResourcesCount[i].y = onStorage[rid];
                        }
                    }
                }
            }
            else
            {
                int cond = (int)(observingHangar.shuttle.condition * 100);
                if (savedProgressBarValue != cond)
                {
                    savedProgressBarValue = cond;
                    shuttleStatusText.text = savedProgressBarValue.ToString() + '%';
                    progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset, savedProgressBarValue / 100f * fullProgressBarLength);
                }
            }
        }
    }

    public void PrepareHangarWindow()
    {
        Shuttle shuttle = observingHangar.shuttle;
        bool haveShuttle = (shuttle != null);

        
        shuttleNameTextField.gameObject.SetActive(haveShuttle);
        repairButton.gameObject.SetActive(haveShuttle);        
        disassembleButton.gameObject.SetActive(haveShuttle);

        bool showProgressBar = false;
        if (haveShuttle)
        {
            shuttleNameTextField.text = shuttle.name;
            savedProgressBarValue = (int)(shuttle.condition * 100);
            mainShuttleIcon.uvRect = UIController.GetTextureUV(shuttle.condition > 0.85 ? Icons.ShuttleGoodIcon : (shuttle.condition < 0.5 ? Icons.ShuttleBadIcon : Icons.ShuttleNormalIcon));
            showProgressBar = true;
            resourceCostContainer.gameObject.SetActive(false);
            mode = HangarObserverMode.ShuttleInside;
        }
        else
        {            
            savedProgressBarValue = (int)(observingHangar.workflow / observingHangar.workflowToProcess * 100) ;
            mainShuttleIcon.uvRect = Rect.zero;
            showProgressBar = observingHangar.constructing;
            constructButton.GetComponent<Image>().overrideSprite = showProgressBar ? PoolMaster.gui_overridingSprite : null;
            resourceCostContainer.gameObject.SetActive( !showProgressBar );
            if (!showProgressBar)
            {
                mode = HangarObserverMode.NoShuttle;
                ResourceContainer[] rc = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
                int l = rc.Length;
                for (int i = 0; i < resourceCostContainer.transform.childCount; i++)
                {
                    Transform t = resourceCostContainer.GetChild(i);
                    if (i < rc.Length)
                    {
                        int rid = rc[i].type.ID;
                        t.GetComponent<RawImage>().uvRect = ResourceType.GetTextureRect(rid);
                        Text tx = t.GetChild(0).GetComponent<Text>();
                        tx.text = Localization.GetResourceName(rid) + " : " + rc[i].volume.ToString();
                        float[] storageResources = GameMaster.colonyController.storage.standartResources;
                        showingResourcesCount[i] = new Vector2(rid, rc[i].volume);
                        if (storageResources[rid] < rc[i].volume) tx.color = Color.red; else tx.color = Color.white;
                        t.gameObject.SetActive(true);
                    }
                    else
                    {
                        t.gameObject.SetActive(false);
                    }
                }
                constructButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ConstructShuttle) + " (" + Shuttle.STANDART_COST.ToString() + ')';
            }
            else mode = HangarObserverMode.BuildingShuttle;
        }
        constructButton.gameObject.SetActive(!showProgressBar);        
        progressBar.transform.parent.gameObject.SetActive(showProgressBar);
        if (showProgressBar)
        {
            shuttleStatusText.text = savedProgressBarValue.ToString() + '%';
            progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset, savedProgressBarValue / 100f * fullProgressBarLength);
            shuttleStatusText.text = savedProgressBarValue.ToString() + '%';
        }
    } 

    public void StartConstructing()
    {
        if (observingHangar.constructing)
        {
            observingHangar.StopConstruction();
            PrepareHangarWindow();
        }
        else
        {
            if (GameMaster.colonyController.energyCrystalsCount >= Shuttle.STANDART_COST)
            {
                ColonyController colony = GameMaster.colonyController;
                colony.GetEnergyCrystals(Shuttle.STANDART_COST);
                if (colony.storage.CheckBuildPossibilityAndCollectIfPossible(ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID)))
                {
                    observingHangar.StartConstruction();
                    PrepareHangarWindow();
                }
                else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
            }
            else
            {
                UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughEnergyCrystals));
                constructButton.GetComponent<Image>().overrideSprite = null;
            }
        }
    }

    public void ChangeName()
    {
        observingHangar.shuttle.name = shuttleNameTextField.text;
    }
    public void Deconstruct()
    {
        observingHangar.DeconstructShuttle();
    }
    public void Repair()
    {
        observingHangar.shuttle.RepairForCoins();
    }


    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingHangar = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        gameObject.SetActive(false);
    }

    public void LocalizeButtonTitles()
    {        
        disassembleButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Disassemble);
        repairButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Repair);
    }
}
