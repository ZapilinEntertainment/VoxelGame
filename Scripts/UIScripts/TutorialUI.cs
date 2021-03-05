using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TutorialScenarioNS;

public sealed class TutorialUI : MonoBehaviour
{
    
    [SerializeField] private Text hugeLabel, mainText;
    [SerializeField] private GameObject adviceWindow, outerProceedButton;
    [SerializeField] private RectTransform showframe, showArrow;
    private TutorialStep currentStep = 0;
    private UIController uicontroller;
    private MainCanvasController mcc;
    private GraphicRaycaster grcaster;
    private Quest currentTutorialQuest;
    private Building observingBuilding;
    private TutorialScenario currentScenario;
    private float timer;
    private bool activateOuterProceedAfterTimer = false, nextStepReady = false;
    public const int LUMBER_QUEST_COUNT = 200, FARM_QUEST_WORKERS_COUNT = 20, STONE_QUEST_COUNT = 250;
    public static TutorialUI current { get; private set; }


    public static void Initialize()
    {
        var g = Instantiate(Resources.Load<GameObject>("UIPrefs/tutorialCanvas"));
        current = g.GetComponent<TutorialUI>();
        UIController.GetCurrent().AddToSpecialCanvas(g.transform);
    }

    private void Start()
    {
        string[] strs = new string[2];
        currentScenario.GetText()
        hugeLabel.text = strs[0];
        mainText.text = strs[1];
        if (!adviceWindow.activeSelf) adviceWindow.SetActive(true);
        if (outerProceedButton.activeSelf) outerProceedButton.SetActive(false);
        GameMaster.realMaster.PrepareColonyController(true);
        uicontroller = UIController.GetCurrent();
        mcc = uicontroller.GetMainCanvasController();
        grcaster = mcc.GetMainCanvasTransform().GetComponent<GraphicRaycaster>();
        //
        currentStep = TutorialStep.Intro;
        PrepareStep();
    }
    private void Update()
    {
        if (timer > 0f)
        {
            timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
            if (timer <= 0f)
            {
                timer = 0f;
                if (activateOuterProceedAfterTimer == true && currentScenario != null) outerProceedButton.SetActive(true);                
            }
        }
    }
  

    public void OKButton()
    {
        if (adviceWindow.activeSelf) adviceWindow.SetActive(false);
        switch (currentStep)
        {
            case TutorialStep.Intro:
                {
                    grcaster.enabled = true;
                    currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)TutorialStep.QuestShown);
                    mcc.SYSTEM_GetQuestButton().GetComponent<Button>().onClick.AddListener(this.ProceedButton);
                    currentStep++;
                    break;
                }
            case TutorialStep.CameraMovement:
            case TutorialStep.CameraRotation:
            case TutorialStep.CameraSlicing:            
                activateOuterProceedAfterTimer = true;
                timer = 1f;                
                break;
            case TutorialStep.Interface_People:
            case TutorialStep.Interface_Electricity:
            case TutorialStep.BuildWindmill_1:
            case TutorialStep.Immigration:
            case TutorialStep.Trade:
                ProceedButton();
                break;
            case TutorialStep.Landing:
                Zeppelin.CreateNew();
                break;
            case TutorialStep.StoneDigging:
                if (nextStepReady)
                {
                    ProceedButton();
                }
                break;
            case TutorialStep.End:
                {
                    Destroy(gameObject);
                    GameMaster.realMaster.ChangePlayMode(GameStartSettings.GetModeChangingSettings(GameMode.Survival, Difficulty.Normal, StartFoundingType.Nothing));
                    break;
                }
               
        }
        grcaster.enabled = true;
    }
    private void StartStep(TutorialStep step)
    {        
        switch (step)
        {
            case TutorialStep.Intro:
                {
                    PrepareTutorialInfo(step);
                    StartQuest(TutorialStep.QuestShown);
                    SetShowframe(mcc.SYSTEM_GetQuestButton());
                    ShowarrowToShowframe_Left();
                    break;
                }
        }
        currentStep = step;
    }
    private void EndStep(TutorialStep step)
    {        
        switch (step)
        {
            case TutorialStep.Intro:
                break;
        }
    }

    //
    public void OpenTextWindow(string label, string description)
    {
        hugeLabel.text = label;
        mainText.text = description;
        adviceWindow.SetActive(true);
        grcaster.enabled = false;
    }
    public void SetShowframe(RectTransform target)
    {
        showframe.position = target.position;
        showframe.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.rect.width);
        showframe.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);
        if (!showframe.gameObject.activeSelf) showframe.gameObject.SetActive(true);
    }
    public void ShowarrowToShowframe_Left()
    {
        if (showArrow.rotation != Quaternion.identity) showArrow.rotation = Quaternion.identity;
        showArrow.position = showframe.position + Vector3.right * showframe.rect.width;
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, showframe.rect.width);
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showframe.rect.height);
        if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
    }
    public void ShowarrowToShowframe_Up()
    {
        showArrow.Rotate(Vector3.up * 90f);
        showArrow.position = showframe.position + Vector3.down * showframe.rect.height;
        if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
    }
    public void DisableShowArrow() { showArrow.gameObject.SetActive(false); }
    public void DisableShowframe() { showframe.gameObject.SetActive(false); }
    public void ActivateProceedTimer(float t)
    {
        if (outerProceedButton.activeSelf) outerProceedButton.SetActive(false);
        timer = t;
        activateOuterProceedAfterTimer = true;
    }
    //
    public void NextScenario()
    {
        //endscenario
        if (outerProceedButton.activeSelf) outerProceedButton.SetActive(false);
        timer = 0f;
        if (showframe.gameObject.activeSelf) showframe.gameObject.SetActive(false);
        if (showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(false);
        if (!grcaster.enabled) grcaster.enabled = true;
        //
    }
    //

    private void StartQuest(TutorialStep step)
    {
        currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)step);
    }

    private void PrepareStep()
    {
        currentStep = newStep;
        //Debug.Log(currentStep);   

        switch (currentStep)
        {


 
                    //
                    PrepareTutorialInfo(TutorialStep.BuildWindmill_0);
                    currentTutorialQuest = StartQuest(TutorialStep.BuildWindmill_0);
                    break;
                }
            case TutorialStep.BuildWindmill_0:
                {
                    currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = null;
                    currentStep++;
                    mcc.SelectedObjectLost();
                    PrepareTutorialInfo(TutorialStep.BuildWindmill_1);
                    break;
                }
            case TutorialStep.BuildWindmill_1:
                {
                    currentStep++;
                    mcc.SelectedObjectLost();
                    //
                    currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)TutorialStep.GatherLumber);
                    PrepareTutorialInfo(TutorialStep.GatherLumber);
                    break;
                }
            case TutorialStep.GatherLumber:
                {
                    currentStep++;
                    currentTutorialQuest = null;
                    mcc.SelectedObjectLost();
                    //
                    PrepareTutorialInfo(TutorialStep.BuildFarm);
                    currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)TutorialStep.BuildFarm);
                    break;
                }
            case TutorialStep.BuildFarm:
                {
                    currentStep++;
                    currentTutorialQuest = null;
                    mcc.ChangeChosenObject(ChosenObjectType.None);
                    //
                    PrepareTutorialInfo(TutorialStep.StoneDigging);
                    currentTutorialQuest = StartQuest(TutorialStep.StoneDigging); ;
                    break;
                }
            case TutorialStep.StoneDigging:
                {
                    currentStep++;
                    currentTutorialQuest = null;
                    mcc.ChangeChosenObject(ChosenObjectType.None);
                    //
                    PrepareTutorialInfo(TutorialStep.StorageLook);
                    if (!mcc.IsStorageUIActive())
                    {
                        var sb = mcc.SYSTEM_GetStorageButton();
                        SetShowframe(sb);
                        ShowarrowToShowframe_Left();
                        sb.GetComponent<Button>().onClick.AddListener(this.ProceedButton);
                        nextStepReady = false;
                    }
                    else nextStepReady = true;
                    break;
                }
            case TutorialStep.StorageLook:
                {
                    showArrow.gameObject.SetActive(false);
                    showframe.gameObject.SetActive(false);
                    if (!nextStepReady)
                    {
                        mcc.SYSTEM_GetStorageButton().GetComponent<Button>().onClick.RemoveListener(this.ProceedButton);
                    }
                    else nextStepReady = false;
                    currentStep++;
                    //
                    PrepareTutorialInfo(TutorialStep.SmelteryBuilding);
                    currentTutorialQuest = StartQuest(TutorialStep.SmelteryBuilding);
                    break;
                }
            case TutorialStep.SmelteryBuilding:
                {
                    mcc.ChangeChosenObject(ChosenObjectType.None);
                    currentStep = TutorialStep.RecipeExplaining_A;
                    //                   
                    PrepareTutorialInfo(currentStep);
                    currentTutorialQuest = StartQuest(currentStep);
                    observingBuilding = GameMaster.realMaster.colonyController.GetBuildings<Factory>()[0];
                    mcc.Select(observingBuilding);
                    var dd = Factory.factoryObserver.SYSTEM_GetRecipesDropdown();
                    SetShowframe(dd.GetComponent<RectTransform>());
                    dd.onValueChanged.AddListener(this.RecipeChanged);
                    break;
                }
            case TutorialStep.RecipeExplaining_A:
                {
                    currentStep++;
                    //
                    PrepareTutorialInfo(TutorialStep.RecipeExplaining_B);
                    var eb = Building.buildingObserver.SYSTEM_GetEnergyButton();
                    SetShowframe(eb.GetComponent<RectTransform>());
                    eb.onClick.AddListener(this.ProceedButton);
                    currentTutorialQuest.SetStepCompleteness(0, true);
                    break;
                }
            case TutorialStep.RecipeExplaining_B:
                {
                    Building.buildingObserver.SYSTEM_GetEnergyButton().onClick.RemoveListener(this.ProceedButton);
                    showframe.gameObject.SetActive(false);
                    currentStep++;
                    mcc.ChangeChosenObject(ChosenObjectType.None);
                    currentTutorialQuest.SetStepCompleteness(1, true);
                    currentTutorialQuest.MakeQuestCompleted();
                    observingBuilding = null;
                    //
                    PrepareTutorialInfo(TutorialStep.CollectConcrete);
                    currentTutorialQuest = StartQuest(TutorialStep.CollectConcrete);
                    break;
                }
            case TutorialStep.CollectConcrete:
                {
                    currentTutorialQuest = null;
                    currentStep++;
                    //
                    mcc.ChangeChosenObject(ChosenObjectType.None);
                    PrepareTutorialInfo(TutorialStep.BuildDock);
                    currentTutorialQuest = StartQuest(TutorialStep.BuildDock);
                    break;
                }
            case TutorialStep.BuildDock_Error:
                {
                    PrepareTutorialInfo(TutorialStep.BuildDock_Error);
                    break;
                }
            case TutorialStep.BuildDock:
                {
                    currentStep = TutorialStep.Immigration;
                    if (!currentTutorialQuest.completed) currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = null;
                    //
                    mcc.Select(observingBuilding);
                    var obs = Dock.dockObserver;
                    obs.PrepareImmigrationPanel();
                    SetShowframe(obs.SYSTEM_GetImmigrationPanel());
                    PrepareTutorialInfo(currentStep);                   
                    break;
                }
            case TutorialStep.Immigration:
                {
                    showframe.gameObject.SetActive(false);
                    currentStep++;
                    //
                    PrepareTutorialInfo(TutorialStep.Trade);
                    break;
                }
            case TutorialStep.Trade:
                {
                    currentStep++;
                    //
                    PrepareTutorialInfo(TutorialStep.HQUpgrade);
                    currentTutorialQuest = StartQuest(TutorialStep.HQUpgrade);
                    break;
                }
            case TutorialStep.HQUpgrade:
                {
                    currentTutorialQuest = null;
                    currentStep++;
                    //
                    PrepareTutorialInfo(TutorialStep.End);
                    break;
                }
        }
    }

    private void StructureCheck(Structure s)
    {
        switch (currentStep)
        {
            case TutorialStep.Landing:
                
                break;
            case TutorialStep.BuildWindmill_0:
                {
                    if (s is WindGenerator)
                    {
                        ProceedButton();
                        return;
                    }
                    break;
                }
            case TutorialStep.BuildDock:
            case TutorialStep.BuildDock_Error:
                {
                    if (s is Dock)
                    {                        
                        var d = s as Dock;
                        if (!d.isCorrectLocated)
                        {
                            currentStep = TutorialStep.BuildDock_Error;
                            ProceedButton();
                        }
                        else
                        {
                            observingBuilding = d;
                            currentStep = TutorialStep.BuildDock;
                            ProceedButton();
                        }
                    }
                    break;
                }
        }
    }
    public void QuestCompleted(byte subIndex)
    {
        var ts = (TutorialStep)subIndex;
        switch (ts)
        {
            case TutorialStep.GatherLumber:
            case TutorialStep.BuildFarm:
            case TutorialStep.StoneDigging:
            case TutorialStep.SmelteryBuilding:
            case TutorialStep.CollectConcrete:
            case TutorialStep.BuildDock:
            case TutorialStep.HQUpgrade:
                ProceedButton();
                break;
        }
    }
    private void RecipeChanged(int i)
    {
        if (currentStep == TutorialStep.RecipeExplaining_A)
        {
            if (observingBuilding != null)
            {
                var of = observingBuilding as Factory;
                if (of != null && of.GetRecipe() == Recipe.StoneToConcrete) {
                    Factory.factoryObserver.SYSTEM_GetRecipesDropdown().onValueChanged.RemoveListener(this.RecipeChanged);
                    ProceedButton();
                }                
            }
        }
    }
}
