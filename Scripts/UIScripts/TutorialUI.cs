using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialUI : MonoBehaviour
{
    public enum TutorialStep: byte { Intro, QuestClicked, QuestShown, Camera, Landing, Interface_People, Interface_Electricity,
    BuildWindmill, GatherLumber, BuildFarm, StoneDigging, SmelteryBuilding, RecipeExplaining, DockBuilding,
    ImmigrationExplaining, TradeExplaining_Crystals, TradeExplaining_TradePanel, TradeExplaining_FoodBuying,
    HQ_Upgrade, OreEnrichment, CitizensNeeds, SettlementCenter_Build, SettlementCenter_Explain, Finish}
    [SerializeField] private Text hugeLabel, mainText;
    [SerializeField] private GameObject adviceWindow;
    [SerializeField] private RectTransform showframe, showArrow;
    private TutorialStep currentStep = 0;
    private UIController uicontroller;
    private MainCanvasController mcc;
    private GraphicRaycaster grcaster;

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
        GameMaster.realMaster.PrepareColonyController(true);
        uicontroller = UIController.GetCurrent();
        mcc = uicontroller.GetMainCanvasController();
        //
        grcaster = mcc.GetMainCanvasTransform().GetComponent<GraphicRaycaster>();
        grcaster.enabled = false;
        var bt = mcc.SYSTEM_GetQuestButton();
        showframe.position = bt.position;
        showframe.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bt.rect.width);
        showframe.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bt.rect.height);
        showArrow.position = showframe.position + Vector3.right * showframe.rect.width;
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, showframe.rect.width);
        showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showframe.rect.height);
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
                    mcc.questUI.SYSTEM_NewTutorialQuest((byte)TutorialStep.QuestClicked);
                    mcc.SYSTEM_GetQuestButton().GetComponent<Button>().onClick.AddListener(this.ProceedButton);
                    currentStep++;
                    break;
                }
            case TutorialStep.QuestClicked:
                {
                    showArrow.gameObject.SetActive(false);
                    showframe.gameObject.SetActive(false);
                    mcc.SYSTEM_GetQuestButton().GetComponent<Button>().onClick.RemoveListener(this.ProceedButton);
                    mcc.questUI.SYSTEM_GetCloseButton().onClick.AddListener(this.ProceedButton);
                    currentStep++;
                    break;
                }
            case TutorialStep.QuestShown:
                {
                    var qui = mcc.questUI;
                    var q = qui.GetActiveQuest();
                    if (q != null && q.type == QuestType.Tutorial && q.subIndex == (byte)TutorialStep.QuestShown)
                    {
                        q.MakeQuestCompleted();
                        qui.SYSTEM_GetCloseButton().onClick.RemoveListener(this.ProceedButton);
                        //
                        currentStep++;
                    }
                    break;
                }
            case TutorialStep.Camera:
                {
                    break;
                }
        }
    }
}
