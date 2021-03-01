using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialUI : MonoBehaviour
{
    public enum TutorialStep: byte { Intro, QuestClicked, QuestShown, QuestsClosed, CameraMovement, CameraRotation, CameraSlicing,
        Landing, Interface_People, Interface_Electricity,  BuildWindmill_0, BuildWindmill_1, GatherLumber, BuildFarm,
        StoneDigging, StorageLook, SmelteryBuilding, RecipeExplaining_A, RecipeExplaining_B, DockBuilding}
    [SerializeField] private Text hugeLabel, mainText;
    [SerializeField] private GameObject adviceWindow, outerProceedButton;
    [SerializeField] private RectTransform showframe, showArrow;
    private TutorialStep currentStep = 0;
    private UIController uicontroller;
    private MainCanvasController mcc;
    private GraphicRaycaster grcaster;
    private Quest currentTutorialQuest;
    private Factory observingFactory;
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
        string label = string.Empty, description = string.Empty;
        Localization.GetTutorialInfo(currentStep, ref label, ref description);
        hugeLabel.text = label;
        mainText.text = description;
        if (!adviceWindow.activeSelf) adviceWindow.SetActive(true);
        if (outerProceedButton.activeSelf) outerProceedButton.SetActive(false);
        GameMaster.realMaster.PrepareColonyController(true);
        uicontroller = UIController.GetCurrent();
        mcc = uicontroller.GetMainCanvasController();
        // 
        grcaster = mcc.GetMainCanvasTransform().GetComponent<GraphicRaycaster>();
        grcaster.enabled = false;
        SetShowframe(mcc.SYSTEM_GetQuestButton());
        ShowarrowToShowframe_Left();
    }
    private void Update()
    {
        if (timer > 0f)
        {
            timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
            if (timer <= 0f)
            {
                timer = 0f;
                if (activateOuterProceedAfterTimer == true) outerProceedButton.SetActive(true);                
            }
        }
    }
    private void SetShowframe(RectTransform target)
    {
        showframe.position = target.position;
        showframe.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target.rect.width);
        showframe.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, target.rect.height);
        if (!showframe.gameObject.activeSelf) showframe.gameObject.SetActive(true);
    }
    private void ShowarrowToShowframe_Left()
    {
        if (showArrow.rotation != Quaternion.identity) showArrow.rotation = Quaternion.identity;
        showArrow.position = showframe.position + Vector3.right * showframe.rect.width;
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, showframe.rect.width);
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showframe.rect.height);
        if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
    }
    private void ShowarrowToShowframe_Up()
    {
        showArrow.Rotate(Vector3.up * 90f);
        showArrow.position = showframe.position + Vector3.down * showframe.rect.height;
        if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
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
            case TutorialStep.RecipeExplaining_A:
                {
                    observingFactory = GameMaster.realMaster.colonyController.GetBuildings<Factory>()[0];
                    mcc.Select(observingFactory);
                    var dd = Factory.factoryObserver.SYSTEM_GetRecipesDropdown();
                    SetShowframe(dd.GetComponent<RectTransform>());
                    dd.onValueChanged.AddListener(this.RecipeChanged);
                    break;
                }
               
        }
        grcaster.enabled = true;
    }

    public void ProceedButton()
    {
        //Debug.Log(currentStep);   
        void PrepareTutorialInfo(TutorialStep step)
        {
            string label = string.Empty, description = label;
            Localization.GetTutorialInfo(step, ref label, ref description);
            hugeLabel.text = label;
            mainText.text = description;
            adviceWindow.SetActive(true);
            grcaster.enabled = false;
            currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)step);
        }
        Quest StartQuest(TutorialStep ts)
        {
            return mcc.questUI.SYSTEM_NewTutorialQuest((byte)ts);
        }

        switch (currentStep)
        {           
            case TutorialStep.QuestClicked:
                {
                    showArrow.gameObject.SetActive(false);
                    showframe.gameObject.SetActive(false);
                    mcc.SYSTEM_GetQuestButton().GetComponent<Button>().onClick.RemoveListener(this.ProceedButton);
                    mcc.questUI.SYSTEM_GetTutorialQuestButton().onClick.AddListener(this.ProceedButton);
                    currentStep++;
                    break;
                }
            case TutorialStep.QuestShown:
                {
                    var qui = mcc.questUI;
                    var q = qui.GetActiveQuest();
                    if (q != null && q.type == QuestType.Tutorial && q.subIndex == (byte)TutorialStep.QuestShown)
                    {
                        qui.SYSTEM_GetTutorialQuestButton().onClick.RemoveListener(this.ProceedButton);
                        //
                        currentStep++;
                        qui.SYSTEM_GetCloseButton().onClick.AddListener(this.ProceedButton);
                    }
                    break;
                }
            case TutorialStep.QuestsClosed:
                {
                    var qui = mcc.questUI;
                    if (qui.GetActiveQuest() == Quest.NoQuest && !qui.IsEnabled())
                    {
                        currentTutorialQuest.MakeQuestCompleted();
                        qui.SYSTEM_GetCloseButton().onClick.RemoveListener(this.ProceedButton);
                        currentStep++;
                        //
                        PrepareTutorialInfo(TutorialStep.CameraMovement);
                        currentTutorialQuest = StartQuest(TutorialStep.CameraMovement) ;
                    }
                    break;
                }
            case TutorialStep.CameraMovement:
                {
                    currentStep++;
                    PrepareTutorialInfo(TutorialStep.CameraRotation);
                    currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = StartQuest(TutorialStep.CameraRotation);
                    outerProceedButton.SetActive(false);
                    break;
                }
            case TutorialStep.CameraRotation:
                {
                    currentStep++;
                    PrepareTutorialInfo(TutorialStep.CameraSlicing);
                    currentTutorialQuest.MakeQuestCompleted();

                    outerProceedButton.SetActive(false);
                    SetShowframe(mcc.SYSTEM_GetLayerCutButton());
                    ShowarrowToShowframe_Left();                       
                    break;
                }
            case TutorialStep.CameraSlicing:
                {                    
                    showframe.gameObject.SetActive(false);
                    showArrow.gameObject.SetActive(false);
                    outerProceedButton.SetActive(false);
                    currentTutorialQuest.MakeQuestCompleted();
                    timer = 0f;
                    currentStep++;
                    //
                    PrepareTutorialInfo(TutorialStep.Landing);
                    GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.StructureCheck;
                    break;
                }
            case TutorialStep.Landing:
                {
                    currentStep++;
                    currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = null;
                    //
                    PrepareTutorialInfo(TutorialStep.Interface_People);
                    SetShowframe(mcc.SYSTEM_GetCitizenString());
                    ShowarrowToShowframe_Up();                    
                    break;
                }
            case TutorialStep.Interface_People:
                {
                    currentStep++;
                    PrepareTutorialInfo(TutorialStep.Interface_Electricity);
                    SetShowframe(mcc.SYSTEM_GetEnergyString());
                    ShowarrowToShowframe_Up();
                    break;
                }
            case TutorialStep.Interface_Electricity:
                {                    
                    showframe.gameObject.SetActive(false);
                    showArrow.gameObject.SetActive(false);
                    outerProceedButton.SetActive(false);
                    currentStep++;
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
                    currentStep++;
                    //                   
                    PrepareTutorialInfo(TutorialStep.RecipeExplaining_A);
                    currentTutorialQuest = StartQuest(TutorialStep.RecipeExplaining_A);
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
                    currentTutorialQuest.CompleteStep(0);
                    break;
                }
            case TutorialStep.RecipeExplaining_B:
                {
                    Building.buildingObserver.SYSTEM_GetEnergyButton().onClick.RemoveListener(this.ProceedButton);
                    showframe.gameObject.SetActive(false);
                    currentStep++;
                    mcc.ChangeChosenObject(ChosenObjectType.None);
                    currentTutorialQuest.CompleteStep(1);
                    currentTutorialQuest.MakeQuestCompleted();
                    observingFactory = null;
                    //
                    PrepareTutorialInfo(TutorialStep.DockBuilding);
                    currentTutorialQuest = StartQuest(TutorialStep.DockBuilding);
                    break;
                }
            case TutorialStep.DockBuilding:
                {
                    currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = null;
                    currentStep++;
                    break;
                }
        }
    }

    private void StructureCheck(Structure s)
    {
        switch (currentStep)
        {
            case TutorialStep.Landing:
                if (s is HeadQuarters)
                {
                    ProceedButton();
                    return;
                }
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
            case TutorialStep.DockBuilding:
                {
                    if (s is Dock)
                    {
                        ProceedButton();
                        return;
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
                ProceedButton();
                break;
        }
    }
    private void RecipeChanged(int i)
    {
        if (currentStep == TutorialStep.RecipeExplaining_A)
        {
            if (observingFactory != null && observingFactory.GetRecipe() == Recipe.StoneToConcrete)
            {
                Factory.factoryObserver.SYSTEM_GetRecipesDropdown().onValueChanged.RemoveListener(this.RecipeChanged);
                ProceedButton();
            }
        }
    }
}
