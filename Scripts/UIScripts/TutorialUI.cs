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
    private UIController uicontroller;
    private MainCanvasController mcc;
    private GraphicRaycaster grcaster;
    private TutorialScenario currentScenario;
    private float timer;
    private bool activateOuterProceedAfterTimer = false, nextStepReady = false;
    public static TutorialUI current { get; private set; }


    public static void Initialize()
    {
        var g = Instantiate(Resources.Load<GameObject>("UIPrefs/tutorialCanvas"));
        current = g.GetComponent<TutorialUI>();
        UIController.GetCurrent().AddToSpecialCanvas(g.transform);
    }

    private void Start()
    {
        GameMaster.realMaster.PrepareColonyController(true);
        uicontroller = UIController.GetCurrent();
        mcc = uicontroller.GetMainCanvasController();
        grcaster = mcc.GetMainCanvasTransform().GetComponent<GraphicRaycaster>();
        //
        TestMaster.CreateColony();
        GameMaster.realMaster.SYSTEM_SetNoResourcesCheat(true);
        TutorialScenario.Initialize(this, mcc);
        StartScenario(TutorialScenario.GetScenario(TutorialStep.SmelteryBuilding));
    }
    private void StartScenario(TutorialScenario s)
    {
        currentScenario = s;
        grcaster.enabled = !s.blockCanvasRaycaster;
        currentScenario.StartScenario();
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
        timer = 0f;
        currentScenario.OKButton();
        grcaster.enabled = true;
    }
    public void ProceedButton()
    {
        currentScenario?.Proceed();
        timer = 0f;
        outerProceedButton.SetActive(false);
    }

    //
    public void OpenTextWindow(string[] s)
    {
        hugeLabel.text = s[0];
        mainText.text = s[1];
        INLINE_OpenTextWindow();
    }
    public void OpenTextWindow(string label, string description)
    {
        hugeLabel.text = label;
        mainText.text = description;
        INLINE_OpenTextWindow();
    }
    private void INLINE_OpenTextWindow()
    {
        if (currentScenario.DropAnySelectionWhenWindowOpens()) mcc.ChangeChosenObject(ChosenObjectType.None);
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
    public void SetCanvasRaycasterStatus(bool active)
    {
        grcaster.enabled = active;
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
        mcc.ChangeChosenObject(ChosenObjectType.None);
        //
        if (currentScenario.step != TutorialStep.UpgradeHQ)
        {
            var nextStep = currentScenario.step + 1;
            StartScenario(TutorialScenario.GetScenario(nextStep));
        }
        Debug.Log(currentScenario.step);
    }
    //
}
