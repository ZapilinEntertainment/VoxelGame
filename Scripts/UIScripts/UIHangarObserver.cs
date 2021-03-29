using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIHangarObserver : UIObserver, ILocalizable // dependence : UICONTROLLER.Update - progress panel
{
    public Hangar observingHangar { get; private set; }
    public Hangar.HangarStatus showingStatus { get; private set; }
#pragma warning disable 0649
    [SerializeField] private Button constructButton; // fiti
    [SerializeField] private Transform resourceCostContainer, shuttleLabel;
    [SerializeField] private GameObject buildWindow;
#pragma warning restore 0649
    private Vector2[] showingResourcesCount;    
    private int lastStorageDrawnValue = 0;

    public static UIHangarObserver InitializeHangarObserverScript()
    {
        UIHangarObserver uho = Instantiate(Resources.Load<GameObject>("UIPrefs/hangarObserver"), mycanvas.rightPanel.transform).GetComponent<UIHangarObserver>();
        uho.LocalizeTitles();
        return uho;
    }

    private void Awake()
    {
        showingResourcesCount = new Vector2[resourceCostContainer.childCount];
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
        uwb.SetObservingPlace(h);
        PrepareHangarWindow();
    }

    public void PrepareHangarWindow()
    {
        showingStatus = observingHangar.status;
        switch (showingStatus)
        {
            case Hangar.HangarStatus.ShuttleOnMission:
                {
                    if (buildWindow.activeSelf) buildWindow.SetActive(false);
                    if (mycanvas.progressPanelMode == ProgressPanelMode.Hangar) mycanvas.DeactivateProgressPanel(ProgressPanelMode.Hangar);
                    shuttleLabel.GetChild(0).GetComponent<RawImage>().uvRect = UIController.GetIconUVRect(Icons.GuidingStar);
                    shuttleLabel.GetChild(1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ShuttleOnMission);
                    if (!shuttleLabel.gameObject.activeSelf) shuttleLabel.gameObject.SetActive(true);
                    break;
                }
            case Hangar.HangarStatus.ShuttleInside:
                {
                    if (buildWindow.activeSelf) buildWindow.SetActive(false);
                    if (mycanvas.progressPanelMode == ProgressPanelMode.Hangar) mycanvas.DeactivateProgressPanel(ProgressPanelMode.Hangar);
                    shuttleLabel.GetChild(0).GetComponent<RawImage>().uvRect = UIController.GetIconUVRect(Icons.ShuttleGoodIcon);
                    shuttleLabel.GetChild(1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ShuttleReady);
                    if (!shuttleLabel.gameObject.activeSelf) shuttleLabel.gameObject.SetActive(true);
                    break;
                }
            case Hangar.HangarStatus.ConstructingShuttle:
                {
                    if (buildWindow.activeSelf) buildWindow.SetActive(false);
                    mycanvas.ActivateProgressPanel(ProgressPanelMode.Hangar);
                    if (shuttleLabel.gameObject.activeSelf) shuttleLabel.gameObject.SetActive(false);
                    break;
                }
            case Hangar.HangarStatus.NoShuttle:
            default:
                {
                    if (!buildWindow.activeSelf) buildWindow.SetActive(true);
                    if (mycanvas.progressPanelMode == ProgressPanelMode.Hangar) mycanvas.DeactivateProgressPanel(ProgressPanelMode.Hangar);
                    if (shuttleLabel.gameObject.activeSelf) shuttleLabel.gameObject.SetActive(false);

                    ResourceContainer[] rc = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
                    var st = GameMaster.realMaster.colonyController.storage;
                    float[] storageResources = st.standartResources;
                    for (int i = 0; i < resourceCostContainer.transform.childCount; i++)
                    {
                        Transform t = resourceCostContainer.GetChild(i);
                        if (i < rc.Length)
                        {
                            int rid = rc[i].type.ID;
                            t.GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(rid);
                            Text tx = t.GetChild(0).GetComponent<Text>();
                            tx.text = Localization.GetResourceName(rid) + " : " + rc[i].volume.ToString();                            
                            showingResourcesCount[i] = new Vector2(rid, rc[i].volume);
                            if (storageResources[rid] < rc[i].volume) tx.color = Color.red; else tx.color = Color.white;
                            t.gameObject.SetActive(true);
                        }
                        else
                        {
                            t.gameObject.SetActive(false);
                        }
                    }
                    lastStorageDrawnValue = st.operationsDone;
                    break;
                }
        }
    }

    override public void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingHangar == null) SelfShutOff();
        else
        {
            if (showingStatus != observingHangar.status || 
                (showingStatus == Hangar.HangarStatus.ConstructingShuttle && lastStorageDrawnValue != GameMaster.realMaster.colonyController.storage.operationsDone))
                PrepareHangarWindow();
        }
    }    

    public void StartConstructing()
    {
        if (observingHangar.status == Hangar.HangarStatus.ConstructingShuttle)
        {
            observingHangar.StopConstruction();
            PrepareHangarWindow();
        }
        else
        {
            ColonyController colony = GameMaster.realMaster.colonyController;
            if (colony.storage.CheckBuildPossibilityAndCollectIfPossible(ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID)))
            {
                observingHangar.StartConstruction();
                PrepareHangarWindow();
            }
            else
            {
                AnnouncementCanvasController.NotEnoughResourcesAnnounce();
            }
        }
    }


    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        if (mycanvas.progressPanelMode == ProgressPanelMode.Hangar) mycanvas.DeactivateProgressPanel(ProgressPanelMode.Hangar);
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingHangar = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        if (mycanvas.progressPanelMode == ProgressPanelMode.Hangar) mycanvas.DeactivateProgressPanel(ProgressPanelMode.Hangar);
        gameObject.SetActive(false);
    }

    public void LocalizeTitles()
    {
        constructButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ConstructShuttle);
        Localization.AddToLocalizeList(this);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Localization.RemoveFromLocalizeList(this);
    }
}
