using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HangarObserverMode : byte { NoShuttle, BuildingShuttle, ShuttleInside, ShuttleOnMission }

public sealed class UIHangarObserver : UIObserver
{
    public Hangar observingHangar { get; private set; }
#pragma warning disable 0649
    [SerializeField] InputField shuttleNameTextField; // fiti
    [SerializeField] Button constructButton, disassembleButton, repairButton; // fiti
    [SerializeField] Transform resourceCostContainer;
#pragma warning restore 0649
    Vector2[] showingResourcesCount;
    public HangarObserverMode mode { get; private set; }

    public static UIHangarObserver InitializeHangarObserverScript()
    {
        UIHangarObserver uho = Instantiate(Resources.Load<GameObject>("UIPrefs/hangarObserver"), UIController.current.rightPanel.transform).GetComponent<UIHangarObserver>();
        return uho;
    }

    private void Awake()
    {
        showingResourcesCount = new Vector2[resourceCostContainer.childCount - 1];
    }

    public void SetObservingHangar(Hangar h)
    {
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

    public void PrepareHangarWindow()
    {
        Shuttle shuttle = observingHangar.shuttle;
        bool haveShuttle = (shuttle != null);

        shuttleNameTextField.gameObject.SetActive(haveShuttle);
        repairButton.gameObject.SetActive(haveShuttle && (shuttle.condition < 1));
        disassembleButton.gameObject.SetActive(haveShuttle);

        if (haveShuttle)
        {
            mode = shuttle.status == ShipStatus.InPort ? HangarObserverMode.ShuttleInside : HangarObserverMode.ShuttleOnMission;
            shuttleNameTextField.text = shuttle.name;
            resourceCostContainer.gameObject.SetActive(false);
            UIController.current.ActivateProgressPanel(ProgressPanelMode.Hangar);            
        }
        else
        {            
            if (observingHangar.constructing)
            {
                mode = HangarObserverMode.BuildingShuttle;
                resourceCostContainer.gameObject.SetActive(false);
                UIController.current.ActivateProgressPanel(ProgressPanelMode.Hangar);                
            }
            else
            {
                mode = HangarObserverMode.NoShuttle;
                resourceCostContainer.gameObject.SetActive(true);
                constructButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ConstructShuttle) + " (" + Shuttle.STANDART_COST.ToString() + ')';
                if (UIController.current.progressPanelMode != ProgressPanelMode.Offline) UIController.current.DeactivateProgressPanel();

                ResourceContainer[] rc = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
                for (int i = 1; i < resourceCostContainer.transform.childCount; i++)
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
            }
        }
    }

    override protected void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingHangar == null) SelfShutOff();
        else
        {
            bool haveShuttle = (observingHangar.shuttle != null);
            HangarObserverMode newMode;
            if (haveShuttle)
            {
                if (observingHangar.shuttle.status == ShipStatus.InPort)
                {
                    newMode = HangarObserverMode.ShuttleInside;
                }
                else newMode = HangarObserverMode.ShuttleOnMission;
            }
            else
            {
                if (observingHangar.constructing)
                {
                    newMode = HangarObserverMode.BuildingShuttle;
                }
                else
                {
                    newMode = HangarObserverMode.NoShuttle;
                    constructButton.interactable = observingHangar.correctLocation;
                }
            }
            if (newMode != mode) PrepareHangarWindow();            
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
        if (UIController.current.progressPanelMode == ProgressPanelMode.Hangar) UIController.current.DeactivateProgressPanel();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingHangar = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        if (UIController.current.progressPanelMode == ProgressPanelMode.Hangar) UIController.current.DeactivateProgressPanel();
        gameObject.SetActive(false);
    }

    public void LocalizeButtonTitles()
    {
        disassembleButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Disassemble);
        repairButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Repair);
    }
}
