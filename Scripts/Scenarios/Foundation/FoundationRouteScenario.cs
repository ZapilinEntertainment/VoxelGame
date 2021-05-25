using System.IO;
using UnityEngine;
using FoundationRoute;
using UnityEngine.UI;
public sealed class FoundationRouteScenario : Scenario
{
    private enum FoundationScenarioStep { Begin, AnchorBuilding, AnchorStart, InnerRingBuilding, PierPreparing, OuterRingBuilding, Finish, Empty }
    
    private FoundationScenarioStep currentStep;
    private StandartScenarioUI scenarioUI;
    public AnchorBasement anchorBasement { get { return _anchorBasement; } }
    private AnchorBasement _anchorBasement;
    private FDR_Subscenario subscenario;
    private QuestUI questUI;
    private Localizer localizer;
    private ColonyController colony;
    private Quest scenarioQuest;
    private HexBuilder hexBuilder;
    private ConditionQuest settleQuest;
    private ConditionWindowController settleWindow;

    private const byte WINDOW_INFO_0 = 1, WINDOW_INFO_1 =2, QUEST_INFO_0 = 3, QUEST_INFO_1 = 4;
    private const int ANCHOR_LAUNCH_ENERGYCOST = 70000,
        COST_RESOURCE_ID = ResourceType.SUPPLIES_ID, COLONISTS_SEND_COST = 10000;
    public const int COLONISTS_SEND_LIMIT = 1000;    
    public static readonly string resourcesPath = "Prefs/Special/FoundationRoute/"; 

    public FoundationRouteScenario() : base(FOUNDATION_ROUTE_ID)
    {
        localizer = new Localizer();
        colony = GameMaster.realMaster.colonyController;
        questUI = UIController.GetCurrent().GetMainCanvasController().questUI;
        Knowledge.GetCurrent()?.SetExecutingScenarioIndex((int)Knowledge.ResearchRoute.Foundation);
    }
    public void PrepareUI()
    {
        scenarioUI = StandartScenarioUI.GetCurrent(this);
        scenarioUI.ChangeIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));
    }

    public override void StartScenario()
    {
        PrepareUI();
        currentStep = FoundationScenarioStep.Begin;      
        StartSubscenario();
    }
    private void StartSubscenario()
    {
        subscenario = FDR_Subscenario.GetSubscenario(currentStep, this);
        subscenario?.StartScenario();
    }
    public void Next()
    {
        switch (currentStep)
        {
            case FoundationScenarioStep.AnchorBuilding:
                scenarioQuest?.MakeQuestCompleted();
                break;
            case FoundationScenarioStep.AnchorStart:
                AnnouncementCanvasController.MakeAnnouncement(localizer.GetAnnounceTitle(currentStep, QUEST_INFO_1));
                scenarioQuest.MakeQuestCompleted();
                break;
        }
        currentStep++;
        StartSubscenario();
    }
    private void StartQuest(FDR_Subscenario s)
    {
        var sq = new ScenarioQuest(s);
        questUI.SYSTEM_NewScenarioQuest(sq);
        scenarioQuest = sq;
        scenarioQuest.FillText(localizer.GetQuestData(s.GetScenarioStep()));
    }
    private void StartQuest(ConditionQuest cq)
    {
        scenarioQuest = cq;
        questUI.SetNewQuest(cq, (byte)QuestSection.Endgame);
    }
    private void AssignAnchor(AnchorBasement ab)
    {
        _anchorBasement = ab;
        _anchorBasement?.LinkScenario(this);
    }

    public void AnchorPoweredUp()
    {
        scenarioQuest?.MakeQuestCompleted();
        currentStep = FoundationScenarioStep.InnerRingBuilding;
        StartSubscenario();
    }
    public void AnchorBigGearReady()
    {
        if (currentStep == FoundationScenarioStep.InnerRingBuilding) {
            if (subscenario == null || subscenario.completed) Next();
        }
        else
        {
            if (currentStep == FoundationScenarioStep.PierPreparing)
            {
                anchorBasement.ActivatePier();
            }
        }
    }
    public void LoadHexInfo(out string[] conditionStrings, out string[] buildingsInfo)
    {
        localizer.LoadHexBuildingData(out conditionStrings, out buildingsInfo);
    }
    public void PrepareSettling()
    {
        if (settleQuest != null) return;
        settleQuest = new ConditionQuest( new SimpleCondition[3]
        {
            SimpleCondition.GetDummyCondition(null),
            SimpleCondition.GetDummyCondition(null),
            SimpleCondition.GetResourceCondition(ResourceType.Supplies, COLONISTS_SEND_COST)
        },
        colony, false, ConditionQuest.ConditionQuestIcon.PeopleIcon  );
        settleQuest.steps[0] = localizer.colonistArrivedLabel;
        settleQuest.steps[1] = localizer.livingSpaceLabel;
        UIController.GetCurrent().updateEvent += CheckSettlingConditions;
        settleWindow = scenarioUI.ShowConditionPanel(1, settleQuest, this.SendColonists);
        settleWindow.SetMainIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.Citizen));
        settleWindow.SetButtonText(localizer.sendColonistsLabel);
    }
    private void CheckSettlingConditions()
    {
        if (completed || GameMaster.loading) return;
        settleQuest.CheckQuestConditions(); // for resource
        int x = anchorBasement.colonistsArrived;
        settleQuest.stepsAddInfo[0] = x.ToString() + " / " + COLONISTS_SEND_LIMIT.ToString();
        settleQuest.stepsFinished[0] = x >= COLONISTS_SEND_LIMIT;
        x = hexBuilder.totalHousing;
        int y = hexBuilder.colonistsCount;
        settleQuest.stepsAddInfo[1] = y.ToString() + " / " + x.ToString();
        settleQuest.stepsFinished[1] = x > y;        
        settleWindow.Refresh();
    }

    private void SendColonists()
    {
        if (completed) return;
        if (anchorBasement.colonistsArrived >= COLONISTS_SEND_LIMIT)
        {
            if (colony.storage.GetResourceCount(COST_RESOURCE_ID) >= COLONISTS_SEND_COST)
            {
                if (hexBuilder.totalHousing > hexBuilder.colonistsCount)
                {
                    anchorBasement.GetColonists(COLONISTS_SEND_LIMIT);
                    colony.storage.GetResources(COST_RESOURCE_ID, COLONISTS_SEND_COST);
                    hexBuilder.AddColonist();
                    colony.RemoveCitizens(COLONISTS_SEND_LIMIT, false);
                    settleWindow.Refresh();
                }
                else AnnouncementCanvasController.MakeImportantAnnounce(localizer.notEnoughLivingSpace);
            }
            else AnnouncementCanvasController.MakeImportantAnnounce(localizer.notEnoughSuppliesMsg);
        }
        else AnnouncementCanvasController.MakeImportantAnnounce(localizer.notEnoughColonistsMsg);
    }
    

    public override void EndScenario()
    {
        completed = true;
        GameMaster.SetPause(true);
        GameMaster.realMaster.UnbindScenario(this);

        if (scenarioUI != null)
        {
            scenarioUI.DisableConditionPanel(0);
            scenarioUI.DisableConditionPanel(1);            
            scenarioUI.ScenarioEnds(this);
        }
        settleWindow = null;
        if (settleQuest != null)
        {
            settleQuest.MakeQuestCompleted(); settleQuest = null;
            UIController.GetCurrent().updateEvent -= CheckSettlingConditions;
        }
        scenarioQuest?.MakeQuestCompleted();scenarioQuest = null;
        //hexBuilder.
        //
        FollowingCamera.main.SetObservingPosition(
            hexBuilder.GetHexWorldPosition(new HexPosition(3, Random.Range(0, 18))) + Vector3.up * 5f,
          GameMaster.sceneCenter + Vector3.down * 5f
        );
        //UIController.GetCurrent().ChangeUIMode(UIMode.Endgame, true);
        GameMaster.realMaster.GameOver(GameEndingType.FoundationRoute);
        GameMaster.SetPause(false); // minus one pause request
    }
    public override void ClearScenarioDecorations()
    {
        hexBuilder?.ClearDecorations();
        if (scenarioUI != null) Object.Destroy(scenarioUI);
    }
    //
    override public void OKButton()
    {
        subscenario.OKButton();        
    }
    //   
    
    override public void CheckConditions() {
        subscenario.CheckConditions();
    }
    public override void UIConditionProceedButton()
    {
        subscenario.UIConditionProceedButton();
    }
    override public bool QuestMustCheckConditions() {
        return subscenario.QuestMustCheckConditions();
    }
    override public byte GetStepsCount() {
        return subscenario.GetStepsCount();
    }
    private void SetHexBuilder() {
        hexBuilder = new HexBuilder(this);
    }
    //
    #region steps
    private class FDR_Subscenario : Scenario
    {
        protected readonly FoundationRouteScenario scenario;
        protected StandartScenarioUI scenarioUI { get { return scenario.scenarioUI; } }
        protected AnchorBasement anchorBasement { get { return scenario.anchorBasement; } }
        protected QuestUI questUI { get { return scenario.questUI; } }
        protected Localizer localizer { get { return scenario.localizer; } }
        protected ColonyController colony { get { return scenario.colony; } }
        protected Quest scenarioQuest { get { return scenario.scenarioQuest; } }

        public FDR_Subscenario(FoundationRouteScenario i_scenario) : base(FOUNDATION_ROUTE_ID)
        {
            scenario = i_scenario;
        }
        public override byte GetStepsCount()
        {
            return 1;
        }
        public override bool QuestMustCheckConditions()
        {
            return true;
        }

        virtual public void Save(FileStream fs) { }
        virtual public void Load(FileStream fs) { }

        public virtual FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.Empty; }
        public static FDR_Subscenario GetSubscenario(FoundationScenarioStep step, FoundationRouteScenario baseScenario)
        {
            switch (step)
            {
                case FoundationScenarioStep.Finish: return new FDR_Finish(baseScenario);
                case FoundationScenarioStep.OuterRingBuilding: return new FDR_OuterRingBuilding(baseScenario);
                case FoundationScenarioStep.PierPreparing: return new FDR_PierPreparing(baseScenario);
                case FoundationScenarioStep.InnerRingBuilding: return new FDR_InnerRingBuilding(baseScenario);
                case FoundationScenarioStep.AnchorStart: return new FDR_AnchorStart(baseScenario);
                case FoundationScenarioStep.AnchorBuilding: return new FDR_AnchorBuilding(baseScenario);
                case FoundationScenarioStep.Begin: return new FDR_Begin(baseScenario);
                default: return null;
            }
        }
    }
    private class FDR_ConditionSubscenario : FDR_Subscenario
    {
        protected ConditionQuest conditionQuest;
        protected ConditionWindowController conditionWindow;
        public FDR_ConditionSubscenario(FoundationRouteScenario i_scenario) : base (i_scenario) { }

        protected void StartSectorBuildingQuest(byte ring, byte index, System.Action clickAction)
        {
            var conditions = new SimpleCondition[3];
            if (ring == 0) // INNER RING
            {
                switch (index)
                {
                    case 0:
                        conditions[0] = SimpleCondition.GetResourceCondition(ResourceType.metal_K, 4100f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.metal_S, 1600f);
                        conditions[2] = SimpleCondition.GetMoneyCondition(2500f);
                        break;
                    case 1:
                        conditions[0] = SimpleCondition.GetGearsCondition(4f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.metal_M, 2500f);
                        conditions[2] = SimpleCondition.GetResourceCondition(ResourceType.metal_E, 1200f);
                        break;
                    case 2:
                        conditions[0] = SimpleCondition.GetMoneyCondition(2500f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.metal_P, 500f);
                        conditions[2] = SimpleCondition.GetFreeWorkersCondition(1500);
                        break;
                    case 3:
                        conditions[0] = SimpleCondition.GetFreeWorkersCondition(500);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.Concrete, 7530f);
                        conditions[2] = SimpleCondition.GetResourceCondition(ResourceType.Plastics, 7530f);
                        break;
                    case 4:
                        conditions[0] = SimpleCondition.GetFreeWorkersCondition(800);
                        conditions[1] = SimpleCondition.GetMoneyCondition(1200f);
                        conditions[2] = SimpleCondition.GetResourceCondition(ResourceType.Food, 40000f);
                        break;
                    case 5:
                        conditions[0] = SimpleCondition.GetFreeWorkersCondition(600);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.metal_K, 1200f);
                        conditions[2] = SimpleCondition.GetMoneyCondition(1500f);
                        break;
                }
                conditionQuest = new ConditionQuest(conditions, colony, false, ConditionQuest.ConditionQuestIcon.FoundationRouteIcon);
                
            }
            else // OUTER RING
            {
                switch (index)
                {
                    case 0:
                        conditions[0] = SimpleCondition.GetResourceCondition(ResourceType.Plastics, 6800f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.metal_K, 1200f);
                        conditions[2] = SimpleCondition.GetStoredEnergyCondition(6100f);
                        break;
                    case 1:
                        conditions[0] = SimpleCondition.GetResourceCondition(ResourceType.Fuel, 250f);
                        conditions[1] = SimpleCondition.GetFreeWorkersCondition(700);
                        conditions[2] = SimpleCondition.GetMoneyCondition(1700f);
                        break;
                    case 2:
                        conditions[0] = SimpleCondition.GetResourceCondition(ResourceType.Concrete, 8000f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.Plastics, 8000f);
                        conditions[2] = SimpleCondition.GetCrewsCondition(2, 3);
                        break;
                    case 3:
                        conditions[0] = SimpleCondition.GetResourceCondition(ResourceType.metal_M, 5700f);
                        conditions[1] = SimpleCondition.GetShuttlesCondition(10);
                        conditions[2] = SimpleCondition.GetDummyCondition(true); 
                        // 4 рейса с оборудованием
                        break;
                    case 4:
                        conditions[0] = SimpleCondition.GetResourceCondition(ResourceType.Dirt, 12000f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.mineral_F, 2500f);
                        conditions[2] = SimpleCondition.GetResourceCondition(ResourceType.mineral_L, 1000f);
                        break;
                    case 5:
                        conditions[0] = SimpleCondition.GetMoneyCondition(2000f);
                        conditions[1] = SimpleCondition.GetResourceCondition(ResourceType.Dirt, 1200f);
                        conditions[2] = SimpleCondition.GetGearsCondition(5f);
                        break;
                }
                // 
                conditionQuest = new ConditionQuest(conditions, colony, false, ConditionQuest.ConditionQuestIcon.FoundationRouteIcon);
                
            }
            conditionQuest.CheckQuestConditions();
            conditionWindow = scenarioUI.ShowConditionPanel(0, conditionQuest, clickAction);
            conditionQuest.BindUIUpdateFunction(conditionWindow.Refresh);
            conditionQuest.SubscribeToUpdate(questUI);                 
        }

        public override void Save(FileStream fs)
        {
            conditionQuest.Save();
        }
        public override void Load(FileStream fs)
        {
            base.Load(fs);
        }
    }

    sealed class FDR_Begin : FDR_Subscenario
    {
        private byte stage = 0;
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.Begin; }
        public FDR_Begin(FoundationRouteScenario i_scn) : base(i_scn) { }
        public override void StartScenario()
        {
            scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.Begin, WINDOW_INFO_0));
            scenarioUI.ShowAnnouncePanel();                           
        }
        public override void OKButton()
        {
            if (stage == 0)
            {
                scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.Begin, WINDOW_INFO_1));
                stage++;
            }
            else
            {
                scenarioUI.CloseAnnouncePanel();
                scenario.Next();
            }
        }
        override public void Save(FileStream fs) {
            fs.WriteByte(stage);
        }
        override public void Load(FileStream fs) {
            stage = (byte)fs.ReadByte();
            if (stage == 1) scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.Begin, WINDOW_INFO_1));
        }
    }
    sealed class FDR_AnchorBuilding : FDR_Subscenario
    {
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.AnchorBuilding; }
        public FDR_AnchorBuilding(FoundationRouteScenario i_scn) : base(i_scn) { }
        public override void StartScenario()
        {
            scenario.StartQuest(this);
            Building.AddTemporarilyAvailableBuilding(Structure.ANCHOR_BASEMENT_ID);
            GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.BuildingCheck;
        }

        private void BuildingCheck(Structure s)
        {
            if (s.ID == Structure.ANCHOR_BASEMENT_ID)
            {
                var anchorBasement = s as AnchorBasement;
                scenario.AssignAnchor(anchorBasement);
                Building.RemoveTemporarilyAvailableBuilding(Structure.ANCHOR_BASEMENT_ID);
                GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.BuildingCheck;
                scenarioQuest.stepsFinished[0] = true;
                completed = true;
            }
        }
    }
    sealed class FDR_AnchorStart : FDR_ConditionSubscenario
    {
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.AnchorStart; }
        private byte stage = 0;
        public FDR_AnchorStart(FoundationRouteScenario i_scn) : base(i_scn) { }
        public override void StartScenario()
        {            
            scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.AnchorStart, WINDOW_INFO_0));
            scenarioUI.ShowAnnouncePanel();
        }
        public override void OKButton()
        {
            if (stage == 0)
            {
                stage++;
                scenarioUI.CloseAnnouncePanel();
                conditionQuest = new ConditionQuest(
                    SimpleCondition.GetStoredEnergyCondition(ANCHOR_LAUNCH_ENERGYCOST),
                    colony,
                    false,
                    ConditionQuest.ConditionQuestIcon.FoundationRouteIcon
                    );
                scenario.StartQuest(conditionQuest);
                scenarioQuest.FillText(localizer.GetQuestData(FoundationScenarioStep.AnchorStart));

                conditionWindow = scenarioUI.ShowConditionPanel(0, conditionQuest, this.UIConditionProceedButton);
                conditionWindow.SetButtonText(Localization.GetWord(LocalizedWord.Anchor_verb));
                conditionWindow.SetMainIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.PowerPlus));
                conditionQuest.BindUIUpdateFunction(conditionWindow.Refresh);                
            }
        }
        public override void UIConditionProceedButton()
        {
            if (stage == 1)
            {
                if (colony.TryGetEnergy(ANCHOR_LAUNCH_ENERGYCOST))
                {
                    stage++;
                    scenarioQuest.MakeQuestCompleted();
                    scenarioUI.DisableConditionPanel(0);
                    anchorBasement.StartActivating();
                    completed = true;
                }
                else conditionWindow.Refresh();
            }
        }
        public override void Save(FileStream fs)
        {
            fs.WriteByte(stage);
        }
        public override void Load(FileStream fs)
        {
            var nstage = (byte)fs.ReadByte();
            if (nstage != 0)
            {
                if (nstage == 1) OKButton();
                else
                {
                    scenarioUI.CloseAnnouncePanel();
                    stage = 2;
                }                
            }
        }
    }
    sealed class FDR_InnerRingBuilding : FDR_ConditionSubscenario
    {
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.InnerRingBuilding; }
        private byte stage = 0;
        public FDR_InnerRingBuilding(FoundationRouteScenario i_scn) : base(i_scn) { }
        
        public override void StartScenario()
        {
            scenario.StartQuest(this);
            scenarioQuest.stepsAddInfo[0] = "0/6";
            scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.InnerRingBuilding, WINDOW_INFO_0));
            scenarioUI.ShowAnnouncePanel();                
        }
        public override void OKButton()
        {
            if (stage == 0)
            {
                scenarioUI.CloseAnnouncePanel();
                stage++;
                StartSectorBuildingQuest(0, 0, this.UIConditionProceedButton);
                conditionWindow.SetButtonText(Localization.GetWord(LocalizedWord.Build));
                conditionWindow.SetMainIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));
            }
            else
            {
                stage++;
                scenarioUI.CloseAnnouncePanel();
                if (anchorBasement.IsReadyToContinue()) scenario.Next();
                else completed = true;
            }
        }

        public override void UIConditionProceedButton()
        {
            if (conditionQuest.ConsumeAndFinish())
            {
                byte ringStage = stage;
                stage++;
                ringStage--;
                scenarioQuest.stepsAddInfo[0] = ringStage.ToString() + "/6";
                if (ringStage < 5)
                {
                    anchorBasement.AddInnerSector(ringStage);
                    conditionQuest.StopQuest(false);
                    StartSectorBuildingQuest(0, (byte)(ringStage + 1), this.UIConditionProceedButton);
                }
                else
                {
                    if (ringStage == 5)
                    {
                        scenario.SetHexBuilder();
                        anchorBasement.AddInnerSector(5);
                        scenarioUI.DisableConditionPanel(0);
                        conditionQuest.StopQuest(false);
                        conditionQuest = null;
                        scenarioQuest.MakeQuestCompleted();
                        stage++;
                        scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.InnerRingBuilding, WINDOW_INFO_1));
                        scenarioUI.ShowAnnouncePanel();
                    }
                }
            }
            else conditionWindow.Refresh();
        }
        public override void Save(FileStream fs)
        {
            fs.WriteByte(stage);
        }
        public override void Load(FileStream fs)
        {
            byte nstage = (byte)fs.ReadByte();
            if (nstage != 0)
            {
                OKButton();
                if (nstage == 7)
                {
                    stage = nstage;
                    scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.InnerRingBuilding, WINDOW_INFO_1));
                    scenarioQuest.MakeQuestCompleted();
                }
                else
                {
                    stage = nstage;
                    StartSectorBuildingQuest(0, --nstage, this.UIConditionProceedButton);
                    scenarioQuest.stepsAddInfo[0] = (--nstage).ToString() + "/6";
                }
            }
        }
    }
    sealed class FDR_PierPreparing : FDR_Subscenario
    {
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.PierPreparing; }
        private byte stage = 0;
        public FDR_PierPreparing(FoundationRouteScenario i_scn) : base(i_scn) { }
        public override void StartScenario()
        {            
            anchorBasement.ActivatePier();
            scenarioUI.ShowInfoString(localizer.GetAnnounceTitle(FoundationScenarioStep.PierPreparing, WINDOW_INFO_0));
        }
        public override void OKButton()
        {
            switch (stage)
            {
                case 0:
                    scenarioUI.DisableInfoString();
                    stage++;
                    scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.PierPreparing, WINDOW_INFO_1));
                    scenarioUI.ShowAnnouncePanel();
                    break;
                case 1:
                    scenarioUI.CloseAnnouncePanel();
                    stage++;
                    scenarioUI.EnableSpecialButton(Localization.GetWord(LocalizedWord.Ready) +'!', this.OKButton);
                    break;
                case 2:                    
                    scenarioUI.DisableSpecialButton();
                    stage++;                    
                    scenario.Next();
                    if (!GameMaster.loading) GameMaster.realMaster.SaveGame("FoundationRoute - autosave");
                    break;
            }
        }
        public override void Save(FileStream fs)
        {
            fs.WriteByte(stage);
        }
        public override void Load(FileStream fs)
        {
            byte nstage = (byte)fs.ReadByte();
            if (nstage != 0)
            {
                switch (nstage)
                {
                    case 1: OKButton(); break;
                    case 2: OKButton();OKButton(); break;
                }
            }
        }
    }
    sealed class FDR_OuterRingBuilding : FDR_ConditionSubscenario
    {
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.OuterRingBuilding; }
        private byte stage = 0;
        public FDR_OuterRingBuilding(FoundationRouteScenario i_scn) : base(i_scn) { }

        public override void StartScenario()
        {
            scenario.StartQuest(this);
            scenarioQuest.stepsAddInfo[0] = "0/6";
            anchorBasement.StartTransportingColonists();

            StartSectorBuildingQuest(1, 0, this.UIConditionProceedButton);
            conditionWindow.SetButtonText(Localization.GetWord(LocalizedWord.Build));
            conditionWindow.SetMainIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));
            //
        }

        public override void UIConditionProceedButton()
        {
            if (conditionQuest.ConsumeAndFinish())
            {
                scenarioQuest.stepsAddInfo[0] = (stage + 1).ToString() + "/6";
                HexType htype;
                int x = stage % 3;
                if (x == 0) htype = HexType.DummyRed;
                else
                {
                    if (x == 1) htype = HexType.DummyGreen;
                    else htype = HexType.DummyBlue;
                }
                scenario.hexBuilder.CreateHex(new HexPosition(0, stage), htype);

                if (stage < 5)
                {
                    conditionQuest.StopQuest(false);
                    StartSectorBuildingQuest(1, stage, this.UIConditionProceedButton);
                    stage++;
                }
                else
                {
                    completed = true;
                    stage++;
                    scenarioQuest.MakeQuestCompleted();
                    conditionQuest.StopQuest(false);
                    scenarioUI.DisableConditionPanel(0);
                    conditionQuest = null;
                    scenario.Next();
                }
            }
            else conditionWindow.Refresh();
        }
        public override void Save(FileStream fs)
        {
            fs.WriteByte(stage);
        }
        public override void Load(FileStream fs)
        {
            byte nstage = (byte)fs.ReadByte();
            if (nstage != 0)
            {
                stage = nstage;
                if (stage < 6) StartSectorBuildingQuest(1, (byte)(stage-1), this.UIConditionProceedButton);
            }
        }
    }
    sealed class FDR_Finish : FDR_Subscenario
    {
        public const int COLONISTS_GOAL = 100;
        private const float CITIZENS_PART = 0.2f;
        private bool windowShowed = false, ignoreCitizenUpdate = false;
        private int savedCitizensCount = -1;
        private ConditionQuest conditionQuest;
        private ConditionWindowController conditionWindow;
        private HexBuilder hbuilder;

        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.Finish; }
        public FDR_Finish(FoundationRouteScenario i_scn) : base(i_scn) { }
        public override void StartScenario()
        {
            hbuilder = scenario.hexBuilder;
            conditionQuest = new ConditionQuest(
                new SimpleCondition[3] {
                    SimpleCondition.GetDummyCondition(null), // colonists
                    SimpleCondition.GetDummyCondition(null), // energy
                    SimpleCondition.GetDummyCondition(null) // income
                },
                    colony,
                    false,
                    ConditionQuest.ConditionQuestIcon.FoundationRouteIcon
                    );
            conditionQuest.steps[0] = localizer.colonistsLabel;
            conditionQuest.steps[1] = localizer.islandStatsLabel;
            conditionQuest.steps[2] = localizer.lowerlandStatsLabel;
            
            scenario.StartQuest(conditionQuest);
            scenarioQuest.FillText(localizer.GetQuestData(FoundationScenarioStep.Finish));            
            //
            conditionWindow = scenarioUI.ShowConditionPanel(0, conditionQuest, null);
            conditionWindow.SetMainIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));
            //
            UIController.GetCurrent().updateEvent += this.Check;
            scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.Finish, WINDOW_INFO_0));
            scenarioUI.ShowAnnouncePanel();
            // 
            colony.populationUpdateEvent += this.CitizensUpdate;
            //
            Check();
        }
        private void CitizensUpdate(int c)
        {
            if (ignoreCitizenUpdate) return;
            if (savedCitizensCount == -1)
            {
                savedCitizensCount = c;
            }
            else
            {
                int delta = c - savedCitizensCount;
                if (delta > 0)
                {
                    delta = (int)((CITIZENS_PART * (0.8f + Random.value * 1.3f)) * delta);
                    if (delta > 0)
                    {
                        anchorBasement.SYSTEM_AddColonists(delta);
                        ignoreCitizenUpdate = true;
                        colony.AddCitizens(delta, false);
                        ignoreCitizenUpdate = false;
                    }
                }
                savedCitizensCount = colony.citizenCount;
            }
        }
        public override void OKButton()
        {
            if (!windowShowed)
            {
                windowShowed = true;
                scenarioUI.CloseAnnouncePanel();
            }
        }
        private void Check()
        {
            if (completed) return;
            var x = hbuilder.colonistsCount;
            conditionQuest.stepsAddInfo[0] = x.ToString().ToString()  + " / " + COLONISTS_GOAL.ToString();
            conditionQuest.stepsFinished[0] = x >= COLONISTS_GOAL;
            conditionQuest.stepsFinished[1] = (colony.energySurplus > 0f) & (colony.energyCrystalsCount > 0f);
            conditionQuest.stepsFinished[2] = hbuilder.completed;
            if (conditionQuest.stepsFinished[0] && conditionQuest.stepsFinished[1] && conditionQuest.stepsFinished[2])
            {
                //end scenario
                completed = true;
                UIController.GetCurrent().updateEvent -= this.Check;
                colony.populationUpdateEvent -= this.CitizensUpdate;
                conditionQuest.MakeQuestCompleted();
                scenarioUI.DisableConditionPanel(0);
                scenario.EndScenario();
            }
            else conditionWindow.Refresh();
        }

        public override void Save(FileStream fs)
        {
            fs.WriteByte(windowShowed ? (byte)1 : (byte)0);
        }
        public override void Load(FileStream fs)
        {
            bool ws = fs.ReadByte() == 1;
            if (ws) OKButton();
        }
    }
    // for copying
    sealed class FDR_Example : FDR_Subscenario
    {
        public override FoundationScenarioStep GetScenarioStep() { return FoundationScenarioStep.Empty; }
        private byte stage = 0;
        public FDR_Example(FoundationRouteScenario i_scn) : base(i_scn) { }
        public override void StartScenario()
        {
        }
    }
        #endregion

    private sealed class Localizer
    {
        private readonly string[] lines;
        private readonly string routeName = Localization.GetPhrase(LocalizedPhrase.FoundationRoute);
        public string conditionQuestText { get { return lines[13]; } }
        public string innerRingConstruction { get { return lines[12]; } }
        public string outerRingConstruction { get { return lines[21]; } }
        public string colonistArrivedLabel {  get { return lines[19]; } }
        public string sendColonistsLabel { get { return lines[51]; } }
        public string notEnoughColonistsMsg {  get { return lines[52] + ' ' + COLONISTS_SEND_LIMIT.ToString() + ' ' + lines[53]; } }
        public string notEnoughSuppliesMsg { get { return lines[54]; } }
        public string notEnoughLivingSpace { get { return lines[55]; } }
        public string livingSpaceLabel { get { return lines[56]; } }
        public string colonistsLabel { get { return lines[59]; } }
        public string islandStatsLabel { get { return lines[60]; } }
        public string lowerlandStatsLabel { get { return lines[61]; } }

        private string finishQuestText { get { return lines[57] + ' ' + FDR_Finish.COLONISTS_GOAL.ToString() +' '+ lines[58]; } }

        public Localizer()
        {
            using (StreamReader sr = File.OpenText("Assets/Locales/foundationRoute_ENG.txt"))
            {
                string s = sr.ReadLine();
                int i = 0, indexLength = (int)char.GetNumericValue(s[0]),
                    count = int.Parse(s.Substring(2, 2)), index, length;
                lines = new string[count];
                while (i < count && !sr.EndOfStream)
                {
                    s = sr.ReadLine();
                    length = s.Length;
                    if (length == 0 || s[0] == '-') continue;
                    else
                    {
                        if (s[0] == '[' && s[indexLength + 1] == ']')
                        {
                            index = int.Parse(s.Substring(1, indexLength));
                            if (lines[index] != null) Debug.Log("string " + index.ToString() + " was rewrited");
                            lines[index] = s.Substring(indexLength + 2, length - indexLength - 2);
                            i++;
                        }
                        else Debug.Log("error in line " + i.ToString() + ": " + s[0] + s[indexLength]);
                    }
                }
            }
        }
        public string GetAnnounceTitle(FoundationScenarioStep step, byte subIndex)
        {
            switch (step)
            {
                case FoundationScenarioStep.Begin:
                    {
                        switch (subIndex)
                        {
                            case WINDOW_INFO_0: return lines[1];
                            case WINDOW_INFO_1: return lines[2] + ' ' + Localization.GetStructureName(Structure.ANCHOR_BASEMENT_ID) + lines[3];
                        }
                        break;
                    }
                
                case FoundationScenarioStep.AnchorStart:
                    {
                        switch(subIndex)
                        {
                            case WINDOW_INFO_0: return lines[6] + ' ' + lines[8] + ' ' + ANCHOR_LAUNCH_ENERGYCOST / 1000 + ' ' + lines[7];
                            case WINDOW_INFO_1: return lines[15];
                        }
                        break;
                    }
                case FoundationScenarioStep.InnerRingBuilding:
                    {
                        switch (subIndex)
                        {
                            case WINDOW_INFO_0: return lines[11] + ' ' + ((int)AnchorBasement.MAX_EXCESS_POWER);
                            case WINDOW_INFO_1: return lines[16];
                        }
                        break;
                    }
                case FoundationScenarioStep.PierPreparing:
                    {
                        switch (subIndex) {
                            case WINDOW_INFO_0: return lines[17];
                            case WINDOW_INFO_1: return lines[18];
                        }
                        break;
                    }
                case FoundationScenarioStep.OuterRingBuilding:
                    {
                        return lines[19];
                    }
                case FoundationScenarioStep.Finish: return finishQuestText;
            }
            return null;
        }
        public string[] GetQuestData(FoundationScenarioStep step)
        {
            switch (step)
            {
                case FoundationScenarioStep.AnchorBuilding:
                    {
                        return new string[3] {
                            routeName,
                        lines[4] + ' ' + Localization.GetStructureName(Structure.ANCHOR_BASEMENT_ID) + ' ' + lines[5],
                        Localization.GetStructureName(Structure.ANCHOR_BASEMENT_ID)
                    };
                    }
                case FoundationScenarioStep.AnchorStart:
                    {
                        return new string[3]
                        {
                            routeName,
                             lines[8] + ' ' + ANCHOR_LAUNCH_ENERGYCOST.ToString() + lines[9] + Localization.GetWord(LocalizedWord.Anchor_verb) + lines[10],
                             Localization.GetPhrase(LocalizedPhrase.EnergyStored)
                        };
                    }
                case FoundationScenarioStep.InnerRingBuilding:
                    {
                        return new string[3]
                        {
                            routeName,
                            lines[13],
                            lines[14]
                        };
                    }
                case FoundationScenarioStep.OuterRingBuilding:
                    {
                        return new string[3]
                        {
                            routeName,
                            lines[20],
                            lines[14]
                        };
                    }
                case FoundationScenarioStep.Finish:
                    {
                        return new string[5]
                        {
                            routeName,
                            finishQuestText +'\n' + lines[62],
                            colonistsLabel,
                            islandStatsLabel,
                            lowerlandStatsLabel
                        };
                    }
                default: return null;
            }
        }
        public void LoadHexBuildingData(out string[] conditions, out string[] descriptions)
        {
            int count = 24;
            descriptions = new string[count];            
            for (int i = 0; i < count; i++)
            {
                descriptions[i] = lines[22 + i];
            }
            //
            count = 4;
            conditions = new string[count];
            conditions[0] = lines[47];
            conditions[1] = lines[48];
            conditions[2] = lines[49];
            conditions[3] = lines[50];
        }
    }

    #region save-load
    public override void Save(FileStream fs)
    {
        base.Save(fs);
        fs.WriteByte((byte)currentStep);
        //
        if (hexBuilder != null)
        {
            fs.WriteByte(1);
            hexBuilder.Save(fs);
        }
        else fs.WriteByte(0);
        //
        if (subscenario == null || subscenario.completed) fs.WriteByte(0);
        else
        {
            fs.WriteByte(1);
            fs.WriteByte((byte)subscenario.GetScenarioStep());
            subscenario.Save(fs);
        }
       //
        if (settleQuest != null)
        {
            fs.WriteByte(1);
        }
        else fs.WriteByte(0);
    }
    public override void Load(FileStream fs)
    {
        currentStep = (FoundationScenarioStep)fs.ReadByte();
        if (currentStep >= FoundationScenarioStep.AnchorBuilding)
        {
            AssignAnchor( colony.GetBuilding(Structure.ANCHOR_BASEMENT_ID) as AnchorBasement );
        }
        //
        if (fs.ReadByte() == 1)
        {
            if (hexBuilder == null) SetHexBuilder();
            hexBuilder.Load(fs);
        }
        //
        int x = fs.ReadByte();
        if (x != 0)
        {
            FoundationScenarioStep fss = (FoundationScenarioStep)fs.ReadByte();
            subscenario = FDR_Subscenario.GetSubscenario(fss, this);
            if (subscenario != null)
            {
                subscenario.StartScenario();
                subscenario.Load(fs);
            }
        }        
        //
        if (fs.ReadByte() == 1) PrepareSettling();
    }
    #endregion
}
