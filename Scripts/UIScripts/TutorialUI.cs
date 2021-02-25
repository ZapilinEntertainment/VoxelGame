using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialUI : MonoBehaviour
{
    public enum TutorialStep: byte { Intro, QuestClicked, QuestShown, QuestsClosed, CameraMovement, CameraRotation, CameraSlicing, Landing, Interface_People, Interface_Electricity,
    BuildWindmill, GatherLumber, BuildFarm, StoneDigging, SmelteryBuilding, RecipeExplaining, DockBuilding,
    ImmigrationExplaining, TradeExplaining_Crystals, TradeExplaining_TradePanel, TradeExplaining_FoodBuying,
    HQ_Upgrade, OreEnrichment, CitizensNeeds, SettlementCenter_Build, SettlementCenter_Explain, Finish}
    [SerializeField] private Text hugeLabel, mainText;
    [SerializeField] private GameObject adviceWindow, outerProceedButton;
    [SerializeField] private RectTransform showframe, showArrow;
    private TutorialStep currentStep = 0;
    private UIController uicontroller;
    private MainCanvasController mcc;
    private GraphicRaycaster grcaster;
    private Quest currentTutorialQuest;
    private float timer;

    public static void Initialize()
    {
        var g = Instantiate(Resources.Load<GameObject>("UIPrefs/tutorialCanvas"));
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
        ShowarrowToShowframe();
    }
    private void Update()
    {
        if (timer > 0f)
        {
            timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
            if (timer <= 0f)
            {
                timer = 0f;
                if (currentStep == TutorialStep.CameraMovement | currentStep == TutorialStep.CameraRotation | currentStep == TutorialStep.CameraSlicing) outerProceedButton.SetActive(true);                
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
    private void ShowarrowToShowframe()
    {
        showArrow.position = showframe.position + Vector3.right * showframe.rect.width;
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, showframe.rect.width);
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showframe.rect.height);
        if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
    }

    public void ProceedButton()
    {
        Debug.Log(currentStep);
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
                        string label = string.Empty, description = label;
                        Localization.GetTutorialInfo(TutorialStep.CameraMovement, ref label, ref description);
                        hugeLabel.text = label;
                        mainText.text = description;
                        currentTutorialQuest = qui.SYSTEM_NewTutorialQuest((byte)TutorialStep.CameraMovement);
                        timer = 5f;
                        adviceWindow.SetActive(true);
                    }
                    break;
                }
            case TutorialStep.CameraMovement:
                {
                    currentStep++;
                    string label = string.Empty, description = label;
                    Localization.GetTutorialInfo(TutorialStep.CameraRotation, ref label, ref description);
                    hugeLabel.text = label;
                    mainText.text = description;
                    currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)TutorialStep.CameraRotation);
                    timer = 5f;
                    adviceWindow.SetActive(true);
                    break;
                }
            case TutorialStep.CameraRotation:
                {
                    currentStep++;
                    string label = string.Empty, description = label;
                    Localization.GetTutorialInfo(TutorialStep.CameraSlicing, ref label, ref description);
                    hugeLabel.text = label;
                    mainText.text = description;
                    currentTutorialQuest.MakeQuestCompleted();
                    currentTutorialQuest = mcc.questUI.SYSTEM_NewTutorialQuest((byte)TutorialStep.CameraSlicing);
                    outerProceedButton.SetActive(false);
                    timer = 0f;
                    SetShowframe(mcc.SYSTEM_GetLayerCutButton());
                    ShowarrowToShowframe();
                    adviceWindow.SetActive(true);
                    break;
                }
            case TutorialStep.CameraSlicing:
                {
                    currentStep++;
                    showframe.gameObject.SetActive(false);
                    showArrow.gameObject.SetActive(false);
                    break;
                }
        }
    }
}
