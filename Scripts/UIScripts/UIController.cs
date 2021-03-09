using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIMode : byte { Standart, GlobalMap, ExploringMinigame, Editor, KnowledgeTab, Endgame}
// dependency: ChangeUIMode, GetCurrentCanvasTransform
public enum Icons : byte
{
    Unknown, GreenArrow, GuidingStar, OutOfPowerButton, PowerPlus, PowerMinus, PowerButton, Citizen, RedArrow, CrewBadIcon,
    CrewNormalIcon, CrewGoodIcon, ShuttleBadIcon, ShuttleNormalIcon, ShuttleGoodIcon, EnergyCrystal, TaskFrame, TaskCompleted,
    DisabledBuilding, QuestAwaitingIcon, QuestBlockedIcon, LogPanelButton, TaskFailed, TurnOn, CrewMarker, AscensionIcon,
    PersistenceIcon, SurvivalSkillsIcon, PerceptionIcon, SecretKnowledgeIcon, TechSkillsIcon, IntelligenceIcon, TreasureIcon,
    QuestMarkerIcon, StabilityIcon, SpaceAffectionIcon, LifepowerAffectionIcon, FoundationRoute, CloudWhaleRoute, EngineRoute,
    PipesRoute, PuzzlePart, CrystalRoute, MonumentRoute, BlossomRoute, PollenRoute
}

public class UIController : MonoBehaviour
{
    private static UIController _currentController;
    public static Texture iconsTexture { get; private set; }

    public System.Action updateEvent;
    private bool specialCanvasWaitingForReactivation = false;
    private float updateTimer;
    private MainCanvasController mainCanvasController;
    private ExploringMinigameUI exploringMinigameController;
    private GlobalMapCanvasController globalMapCanvasController;
    private KnowledgeTabUI knowledgeTabUI;
    private EditorUI editorCanvasController;
    private EndPanelController endPanelController;
    private GameObject specialElementsHolder;

    public UIMode currentMode { get; private set; }
    private UIMode previousMode = UIMode.Standart;
    private const float UPDATE_TIME = 1f;

    //--------------------------------
    static UIController()
    {
        iconsTexture = Resources.Load<Texture>("Textures/Icons");
    }

    public static UIController GetCurrent()
    {
        if (_currentController == null) _currentController = new GameObject("UIController").AddComponent<UIController>();
        return _currentController;
    }

    //-----------------

    public MainCanvasController GetMainCanvasController()
    {
        if (mainCanvasController == null )
        {
            mainCanvasController = Instantiate(Resources.Load<GameObject>("UIPrefs/MainCanvasController"), transform).GetComponent<MainCanvasController>();
            mainCanvasController.Initialize(this);
        }
        return mainCanvasController;
    }
    public ExploringMinigameUI GetExploringMinigameController()
    {      
        if (exploringMinigameController == null) exploringMinigameController = Instantiate(Resources.Load<GameObject>("UIPrefs/ExploringMinigameInterface"), transform).GetComponent<ExploringMinigameUI>();
        return exploringMinigameController;
    }
    public GlobalMapCanvasController GetGlobalMapCanvasController()
    {
        if (globalMapCanvasController == null)
        {
            globalMapCanvasController = Instantiate(Resources.Load<GameObject>("UIPrefs/globalMapUI"), transform).GetComponent<GlobalMapCanvasController>();
            globalMapCanvasController.SetGlobalMap(GameMaster.realMaster.globalMap);
        }
        return globalMapCanvasController;
    }
    public KnowledgeTabUI GetKnowledgeTabUI()
    {
        if (knowledgeTabUI == null)
        {
            knowledgeTabUI = Instantiate(Resources.Load<GameObject>("UIPrefs/knowledgeTab"), transform).GetComponent<KnowledgeTabUI>();
            knowledgeTabUI.Prepare(Knowledge.GetCurrent(), this);
        }
        return knowledgeTabUI;
    }
    public EditorUI GetEditorCanvasController()
    {
        if (editorCanvasController == null)
        {
            editorCanvasController = Instantiate(Resources.Load<GameObject>("UIPrefs/editorCanvas"), transform).GetComponent<EditorUI>();
        }
        return editorCanvasController;
    }
    public EndPanelController GetEndPanelController()
    {
        if (endPanelController == null)
        {
            endPanelController = Instantiate(Resources.Load<GameObject>("UIPrefs/endPanel"), transform).GetComponent<EndPanelController>();
        }
        return endPanelController;
    }

    public void AddSpecialCanvasToHolder(Transform t)
    {
        if (specialElementsHolder == null)
        {
            specialElementsHolder = new GameObject("special canvas holder");
            specialElementsHolder.transform.parent = transform;
            specialElementsHolder.transform.localPosition = Vector3.zero;
        }
        t.parent = specialElementsHolder.transform;
        specialElementsHolder.transform.SetAsLastSibling();
    }
    public void SpecialCanvasUpwards()
    {
        specialElementsHolder?.transform.SetAsLastSibling();
    }
    public void DisableSpecialCanvas()
    {
        if (specialElementsHolder != null)
        {
            specialElementsHolder.SetActive(false);
            specialCanvasWaitingForReactivation = true;
        }
    }
    public void ReactivateSpecialCanvas()
    {
        if (specialCanvasWaitingForReactivation)
        {
            if (specialElementsHolder != null)
            {
                specialElementsHolder.SetActive(true);
                SpecialCanvasUpwards();
            }
            specialCanvasWaitingForReactivation = false;
        }
    }

    private void Update()
    {
        if (GameMaster.gameSpeed != 0f) {
            updateTimer -= Time.deltaTime;
            if (updateTimer <= 0f)
            {
                updateEvent?.Invoke();
                updateTimer = UPDATE_TIME;
            }
        }
    }

    public void ChangeUIMode(UIMode newUIMode, bool disableCanvas)
    {
        if (GameMaster.sceneClearing) return;
        if (currentMode != newUIMode)
        {
            switch (currentMode)
            {
                case UIMode.Standart:
                    if (disableCanvas) mainCanvasController?.gameObject.SetActive(false);                    
                    GameMaster.realMaster.environmentMaster?.DisableDecorations();
                    break;
                case UIMode.ExploringMinigame: if (disableCanvas) exploringMinigameController?.gameObject.SetActive(false); break;
                case UIMode.GlobalMap:
                    if (disableCanvas) globalMapCanvasController.gameObject.SetActive(false);
                    break;
                case UIMode.Editor:
                    {
                        if (disableCanvas)
                        {
                            Destroy(editorCanvasController.gameObject);
                            editorCanvasController = null;
                        }
                        break;
                    }
                case UIMode.Endgame:
                    {                        
                        if (endPanelController != null) Destroy(endPanelController);
                        break;
                    }
            }
            bool haveOwnCamera = false;
            switch (newUIMode)
            {
                case UIMode.ExploringMinigame:
                    haveOwnCamera = true;
                    var mo = GetExploringMinigameController();
                    if (!mo.gameObject.activeSelf) mo.gameObject.SetActive(true);
                    break;
                case UIMode.GlobalMap:
                    haveOwnCamera = true;
                    var go = GetGlobalMapCanvasController();
                    go.RedrawMap();
                    if (!go.gameObject.activeSelf) go.gameObject.SetActive(true);
                    break;
                case UIMode.KnowledgeTab:
                    haveOwnCamera = true;
                    var kt = GetKnowledgeTabUI();
                    if (!kt.gameObject.activeSelf) kt.gameObject.SetActive(true);
                    kt.Redraw();
                    break;
                case UIMode.Endgame:
                    AnnouncementCanvasController.DeactivateLogWindow();
                    endPanelController.transform.SetAsLastSibling();
                    break;
                case UIMode.Standart:
                    var mcc = GetMainCanvasController();
                    if (!mcc.gameObject.activeSelf) mcc.gameObject.SetActive(true);
                    GameMaster.realMaster.environmentMaster?.EnableDecorations();
                    break;
                case UIMode.Editor:
                    var eui = GetEditorCanvasController();
                    break;
            }
            FollowingCamera.main.gameObject.SetActive(!haveOwnCamera);
            previousMode = currentMode;
            currentMode = newUIMode;
        }
    }

    //  REQUESTS
    public Transform GetCurrentCanvasTransform()
    {
        switch (currentMode)
        {
            case UIMode.ExploringMinigame: return exploringMinigameController.GetMainCanvasTransform();
            case UIMode.GlobalMap: return globalMapCanvasController.GetMainCanvasTransform();
            case UIMode.KnowledgeTab:return knowledgeTabUI.GetMainCanvasTransform();
            case UIMode.Editor: return editorCanvasController.GetMainCanvasTransform();
            default: return mainCanvasController?.GetMainCanvasTransform();
        }
    }
    public void ReturnToPreviousCanvas(bool disableCanvas)
    {
        ChangeUIMode(previousMode, disableCanvas);
    }
    public void ShowExpedition(Expedition e)
    {
        switch (e.stage)
        {
            case Expedition.ExpeditionStage.OnMission:
                GetExploringMinigameController().Show(e);
                ChangeUIMode(UIMode.ExploringMinigame, true);                
                break;
            case Expedition.ExpeditionStage.WayIn:
            case Expedition.ExpeditionStage.WayOut:
            case Expedition.ExpeditionStage.LeavingMission:
                if (currentMode != UIMode.GlobalMap) ChangeUIMode(UIMode.GlobalMap, true); else GetGlobalMapCanvasController().RedrawMap();
                GetGlobalMapCanvasController().SelectExpedition(e.ID);
                break;
        }
        
    }
    

    public void GameOver( GameEndingType endType, ulong score )
    {        
        GetEndPanelController().Prepare(endType, score);
        ChangeUIMode(UIMode.Endgame, true);           
    }


    // --------------
    public static Rect GetIconUVRect(Icons i)
    {
        float p = 0.125f;
        switch (i)
        {
            case Icons.GreenArrow: return new Rect(6 * p, 7 * p, p, p);
            case Icons.GuidingStar: return new Rect(7 * p, 7 * p, p, p);
            case Icons.OutOfPowerButton: return new Rect(2 * p, 7 * p, p, p);
            case Icons.PowerPlus: return new Rect(3 * p, 7 * p, p, p);
            case Icons.PowerMinus: return new Rect(4 * p, 7 * p, p, p);
            case Icons.Citizen: return new Rect(p, 6 * p, p, p);
            case Icons.RedArrow: return new Rect(2 * p, 6 * p, p, p);
            case Icons.CrewBadIcon: return new Rect(p, 5 * p, p, p);
            case Icons.CrewNormalIcon: return new Rect(2 * p, 5 * p, p, p);
            case Icons.CrewGoodIcon: return new Rect(3 * p, 5 * p, p, p);
            case Icons.ShuttleBadIcon: return new Rect(4 * p, 5 * p, p, p);
            case Icons.ShuttleNormalIcon: return new Rect(5 * p, 5 * p, p, p);
            case Icons.ShuttleGoodIcon: return new Rect(6 * p, 5 * p, p, p);
            case Icons.EnergyCrystal: return new Rect(p, 4f * p, p, p);
            case Icons.TaskFrame: return new Rect(3 * p, 4 * p, p, p);
            case Icons.TaskCompleted: return new Rect(4 * p, 4 * p, p, p);
            case Icons.DisabledBuilding: return new Rect(p, 3 * p, p, p);
            case Icons.PowerButton: return new Rect(p, 3 * p, p, p);
            case Icons.QuestAwaitingIcon: return new Rect(2 * p, 3 * p, p, p);
            case Icons.QuestBlockedIcon: return new Rect(3 * p, 3 * p, p, p);
            case Icons.LogPanelButton: return new Rect(4 * p, 3 * p, p, p);
            case Icons.TaskFailed: return new Rect(5 * p, 3 * p, p, p);
            case Icons.TurnOn: return new Rect(6 * p, 3 * p, p, p);
            case Icons.CrewMarker: return new Rect(7 * p, 3 * p, p, p);
            case Icons.AscensionIcon: return new Rect(0f, 2f * p, p, p);
            case Icons.PersistenceIcon: return new Rect(p, 2f * p, p, p);
            case Icons.SurvivalSkillsIcon: return new Rect(2f * p, 2f * p, p, p);
            case Icons.PerceptionIcon: return new Rect(3f * p, 2f * p, p, p);
            case Icons.SecretKnowledgeIcon: return new Rect(4f * p, 2f * p, p, p);
            case Icons.TechSkillsIcon: return new Rect(5f * p, 2f * p, p, p);
            case Icons.IntelligenceIcon: return new Rect(6f * p, 2f * p, p, p);
            case Icons.TreasureIcon: return new Rect(7f * p, 2f * p, p, p);
            case Icons.QuestMarkerIcon: return new Rect(0f, p, p, p);
            case Icons.StabilityIcon: return new Rect(p, p, p, p);
            case Icons.SpaceAffectionIcon: return new Rect(2f * p, p, p, p);
            case Icons.LifepowerAffectionIcon: return new Rect(3f * p, p, p, p);
            case Icons.FoundationRoute: return new Rect(4f * p, p, p, p);
            case Icons.CloudWhaleRoute: return new Rect(5f * p, p, p, p);
            case Icons.EngineRoute: return new Rect(6f * p, p, p, p);
            case Icons.PipesRoute: return new Rect(7f * p, p, p, p);
            case Icons.PuzzlePart: return new Rect(p, 0f, p, p);
            case Icons.CrystalRoute: return new Rect(4f * p, 0f, p, p);
            case Icons.MonumentRoute: return new Rect(5f * p, 0f, p, p);
            case Icons.BlossomRoute: return new Rect(6f * p, 0f, p, p);
            case Icons.PollenRoute: return new Rect(7f * p, 0f, p, p);
            case Icons.Unknown:
            default: return new Rect(0f, 0f, p, p);
        }
    }
    public static void PositionElement(RectTransform rt, RectTransform parent, SpriteAlignment alignment, Rect r)
    {
        rt.SetParent(parent);
        rt.localScale = Vector3.one;
        rt.SetAsLastSibling();
        float lx = 1f / parent.localScale.x, ly = 1f / parent.localScale.y;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, r.width * lx);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, r.height * ly);
        Vector2 anchor;
        switch (alignment)
        {
            case SpriteAlignment.BottomRight:
                anchor = Vector2.right;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.left * r.width * lx;
                break;
            case SpriteAlignment.BottomLeft:
                anchor = Vector2.zero;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.zero;
                break;
            case SpriteAlignment.RightCenter:
                anchor = new Vector2(1f, 0.5f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-1f * r.width * lx, -0.5f * r.height * ly);
                break;
            case SpriteAlignment.TopRight:
                anchor = Vector2.one;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-1f * r.width * lx, -1f * r.height * ly);
                break;
            case SpriteAlignment.Center:
                anchor = Vector2.one * 0.5f;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-0.5f * r.width * lx, -0.5f * r.height * ly);
                break;
            case SpriteAlignment.TopCenter:
                anchor = new Vector2(0.5f, 1f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-0.5f * r.width * lx, -1f * r.height * ly);
                break;
            case SpriteAlignment.BottomCenter:
                anchor = new Vector2(0.5f, 0f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-0.5f * r.width * lx, 0f);
                break;
            case SpriteAlignment.TopLeft:
                anchor = new Vector2(0f, 1f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.down * r.height * ly;
                break;
            case SpriteAlignment.LeftCenter:
                anchor = new Vector2(0f, 0.5f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.down * r.height * 0.5f * ly;
                break;
        }
        rt.anchoredPosition += r.position;
    }
}
