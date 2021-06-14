using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TutorialScenarioNS
{
    public sealed class TutorialUI : MonoBehaviour
    {
        private enum FramedElementStatus : byte { NotAssigned, DrawnOver, ComponentAdded, AddedAsChild}

        [SerializeField] private Text hugeLabel, mainText;
        [SerializeField] private GameObject adviceWindow, outerProceedButton;
        [SerializeField] private RectTransform showArrow;
        [SerializeField] private Sprite frameSprite;
        private UIController uicontroller;
        private MainCanvasController mcc;
        private GraphicRaycaster grcaster;
        public TutorialScenario currentScenario { get; private set; }
        private float timer;
        private Image framedElement = null;
        private FramedElementStatus framedElementStatus;
        private bool activateOuterProceedAfterTimer = false, nextStepReady = false;
        public static TutorialUI current { get; private set; }


        public static void Initialize()
        {
            var g = Instantiate(Resources.Load<GameObject>("UIPrefs/tutorialCanvas"));
            current = g.GetComponent<TutorialUI>();
            UIController.GetCurrent().AddSpecialCanvasToHolder(g.transform);            
        }

        private void Start()
        {
            GameMaster.realMaster.PrepareColonyController(true);
            uicontroller = UIController.GetCurrent();
            mcc = uicontroller.GetMainCanvasController();
            grcaster = mcc.GetMainCanvasTransform().GetComponent<GraphicRaycaster>();
            //
            GameConstants.DisableTutorialNote();
            TutorialScenario.Initialize(this, mcc);
            outerProceedButton.GetComponentInChildren<Text>().text = Localization.GetWord(LocalizedWord.Continue);
            StartScenario(TutorialScenario.GetScenario(0));
        }
        private void StartScenario(TutorialScenario s)
        {
            currentScenario = s;
            GameMaster.realMaster.BindScenario(s);
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
            grcaster.enabled = currentScenario.blockCanvasRaycaster;
        }

        public void SetShowframe(RectTransform target)
        {
            DisableShowframe();
            var gc = target.GetComponent<Graphic>();

            void PrepareImage()
            {                
                framedElement.sprite = frameSprite;
                framedElement.pixelsPerUnitMultiplier = 20;
                framedElement.type = Image.Type.Sliced;                
            }

            if (gc != null)
            {
                if (gc is Image)
                {
                    framedElement = gc as Image;
                    framedElement.overrideSprite = frameSprite;
                    framedElementStatus = FramedElementStatus.DrawnOver;
                }
                else
                {
                    var g = new GameObject("tutorial marker");
                    g.transform.parent = target;
                    framedElement = g.AddComponent<Image>();
                    PrepareImage();
                    var rt = g.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                    framedElementStatus = FramedElementStatus.AddedAsChild;
                }
            }
            else
            {
                framedElement = target.gameObject.AddComponent<Image>();
                PrepareImage();
                framedElementStatus = FramedElementStatus.ComponentAdded;
            }
        }
        public void DisableShowframe() {
            switch (framedElementStatus) {
                case FramedElementStatus.DrawnOver:
                    if (framedElement.overrideSprite == frameSprite)  framedElement.overrideSprite = null;
                    break;
                case FramedElementStatus.ComponentAdded: Destroy(framedElement); break;
                case FramedElementStatus.AddedAsChild: Destroy(framedElement.gameObject);break;
            }
            framedElement = null;
            framedElementStatus = FramedElementStatus.NotAssigned;
        }
        public void ShowarrowToShowframe_Left()
        {
            if (showArrow.rotation != Quaternion.identity) showArrow.rotation = Quaternion.identity;

            if (framedElement != null)
            {
                var rt = framedElement.rectTransform;
                var rect = rt.rect;
                float s = Screen.height / 11f;
                showArrow.position = rt.position + Vector3.right * s * 2f;                
                showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s * 2f);
                showArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s);
            }
            if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
        }
        public void ShowarrowToShowframe_Up()
        {
            showArrow.Rotate(Vector3.up * 90f);
            if (framedElement != null)
            {
                var rt = framedElement.rectTransform;
                showArrow.position = rt.position + Vector3.down * Screen.height / 11f * 2f;
            }            
            if (!showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(true);
        }
        public void DisableShowArrow() { showArrow.gameObject.SetActive(false); }       
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

        public RectTransform GetAdviceWindow()
        {
            return adviceWindow.GetComponent<RectTransform>();
        }
        public void AdviceWindowToStartPosition()
        {
            var rt = adviceWindow.GetComponent<RectTransform>();
            rt.anchorMax = Vector2.one * 0.5f;
            rt.anchorMin = Vector2.one * 0.5f;
            rt.localPosition = Vector2.zero;
        }
        //
        public void NextScenario()
        {
            //endscenario
            if (outerProceedButton.activeSelf) outerProceedButton.SetActive(false);
            timer = 0f;
            if (framedElement != null) DisableShowframe();         
            if (showArrow.gameObject.activeSelf) showArrow.gameObject.SetActive(false);
            if (!grcaster.enabled) grcaster.enabled = true;
            mcc.ChangeChosenObject(ChosenObjectType.None);
            //
            if (currentScenario.step != TutorialStep.UpgradeHQ)
            {
                var nextStep = currentScenario.step + 1;
                StartScenario(TutorialScenario.GetScenario(nextStep));
            }
            else
            {
                GameMaster.realMaster.ChangePlayMode(GameStartSettings.GetModeChangingSettings(GameMode.Survival, Difficulty.Easy, StartFoundingType.Nothing));
                var qs = currentScenario.DefineQuestSection();
                if (qs == QuestSection.Endgame) mcc.questUI.BlockQuestPosition(qs);
                GameMaster.realMaster.UnbindScenario(currentScenario);
                currentScenario = null;
                Destroy(gameObject);
            }
        }
        //

        private void OnDestroy()
        {
            DisableShowframe();
        }
    }
}
