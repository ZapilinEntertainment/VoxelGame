using System.IO;
using UnityEngine;
public sealed class FoundationRouteScenario : Scenario
{
    private enum FoundationScenarioStep { Begin, AnchorBuilding, AnchorStart, InnerRingBuilding, PierPreparing, OuterRingBuilding }
    
    private FoundationScenarioStep currentStep;
    private StandartScenarioUI scenarioUI;
    private AnchorBasement anchorBasement { get { return _anchorBasement; } }
    private AnchorBasement _anchorBasement;
    private FDR_Subscenario subscenario;
    private QuestUI questUI;
    private Localizer localizer;
    private ColonyController colony;
    private Quest scenarioQuest;
    private FoundationRoute.HexBuilder hexBuilder;

    private const byte WINDOW_INFO_0 = 1, WINDOW_INFO_1 =2, QUEST_INFO_0 = 3, QUEST_INFO_1 = 4;
    private const int ANCHOR_LAUNCH_ENERGYCOST = 20000; //70

    public FoundationRouteScenario() : base(FOUNDATION_ROUTE_ID)
    {
        localizer = new Localizer();
        colony = GameMaster.realMaster.colonyController;
        questUI = UIController.GetCurrent().GetMainCanvasController().questUI;        
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
        switch (currentStep)
        {
            case FoundationScenarioStep.Begin:
                subscenario = new FDR_Begin(this);
                break;
            case FoundationScenarioStep.AnchorBuilding:
                subscenario = new FDR_AnchorBuilding(this);
                break;
            case FoundationScenarioStep.AnchorStart:
                subscenario = new FDR_AnchorStart(this);
                break;
            case FoundationScenarioStep.InnerRingBuilding:
                subscenario = new FDR_InnerRingBuilding(this);
                break;
            case FoundationScenarioStep.PierPreparing:
                subscenario = new FDR_PierPreparing(this);
                break;
            case FoundationScenarioStep.OuterRingBuilding:
                subscenario = new FDR_OuterRingBuilding(this);
                break;
            default:
                Debug.Log("no subscenario found");
                return;
        }
        subscenario.StartScenario();
    }
    public void Next()
    {
        if (currentStep == FoundationScenarioStep.AnchorStart)
        {
            AnnouncementCanvasController.MakeAnnouncement(localizer.GetAnnounceTitle(currentStep,QUEST_INFO_1));
            scenarioQuest.MakeQuestCompleted();
        }
        currentStep++;
        StartSubscenario();
    }
    private void StartQuest(Scenario s)
    {
        var sq = new ScenarioQuest(s);
        questUI.SYSTEM_NewScenarioQuest(sq);
        scenarioQuest = sq;
        scenarioQuest.FillText(localizer.GetQuestData(currentStep));
    }
    private void StartQuest(ConditionQuest cq)
    {
        scenarioQuest = cq;
        questUI.SetNewQuest(cq, (byte)QuestSection.Endgame);
    }
    private void AssignAnchor(AnchorBasement ab)
    {
        _anchorBasement = ab;
        _anchorBasement.LinkScenario(this);
    }

    public void AnchorPoweredUp()
    {
        scenarioQuest?.MakeQuestCompleted();
        currentStep = FoundationScenarioStep.InnerRingBuilding;
        StartSubscenario();
    }
    public void AnchorBigGearReady()
    {
        if (currentStep == FoundationScenarioStep.InnerRingBuilding && (subscenario == null || subscenario.completed)) Next();
    }    

    public override void EndScenario()
    {
        GameMaster.realMaster.UnbindScenario(this);
        scenarioUI?.ScenarioEnds(this);
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
    }
    private class FDR_ConditionSubscenario : FDR_Subscenario
    {
        protected ConditionQuest conditionQuest;
        public FDR_ConditionSubscenario(FoundationRouteScenario i_scenario) : base (i_scenario) { }

        protected void StartSectorBuildingQuest(byte ring, byte index)
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
                        conditions[1] = SimpleCondition.GetShuttlesCondition(1); // 10
                        conditions[2] = SimpleCondition.GetDummyCondition(); 
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
            scenarioUI.ShowConditionPanel(conditionQuest);
            conditionQuest.BindUIUpdateFunction(scenarioUI.UpdateConditionInfo);
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
                scenarioQuest.MakeQuestCompleted();
                completed = true;
            }
        }
    }
    sealed class FDR_AnchorStart : FDR_ConditionSubscenario
    {
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
                scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Anchor_verb));
                scenarioUI.ChangeConditionIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));
                conditionQuest.BindUIUpdateFunction(scenarioUI.UpdateConditionInfo);                
                scenarioUI.ShowConditionPanel(scenarioQuest);
            }
        }
        public override void UIConditionProceedButton()
        {
            if (stage == 1)
            {
                stage++;
                scenarioQuest.MakeQuestCompleted();
                scenarioUI.DisableConditionPanel();
                anchorBasement.StartActivating();
                completed = true;
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
                scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Build));
                StartSectorBuildingQuest(0, 0);                
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
            byte ringStage = stage;
            stage++;
            ringStage--;          
            scenarioQuest.stepsAddInfo[0] = ringStage.ToString() + "/6";            
            if (ringStage < 5)
            {
                anchorBasement.AddSector(0, ringStage);
                conditionQuest.StopQuest(false);
                StartSectorBuildingQuest(0, (byte)(ringStage + 1));
            } 
            else
            {
                if (ringStage == 5)
                {
                    anchorBasement.AddSector(0, 5);
                    scenarioUI.DisableConditionPanel();
                    conditionQuest.StopQuest(false);
                    conditionQuest = null;
                    scenarioQuest.MakeQuestCompleted();                   
                    stage++;
                    scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.InnerRingBuilding, WINDOW_INFO_1));
                    scenarioUI.ShowAnnouncePanel();
                }
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
                if (nstage == 1 ) OKButton();
                else
                {
                    if (nstage == 7) {
                        stage = nstage;
                        scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.InnerRingBuilding, WINDOW_INFO_1));
                        scenarioQuest.MakeQuestCompleted();
                    }
                    else
                    {
                        stage = nstage;
                        StartSectorBuildingQuest(0, --nstage);
                        scenarioQuest.stepsAddInfo[0] = (--nstage).ToString() + "/6";
                    }
                }
            }
        }
    }
    sealed class FDR_PierPreparing : FDR_Subscenario
    {
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
        private byte stage = 0;
        public FDR_OuterRingBuilding(FoundationRouteScenario i_scn) : base(i_scn) { }

        public override void StartScenario()
        {
            scenario.StartQuest(this);
            scenarioQuest.stepsAddInfo[0] = "0/6";
            anchorBasement.StartTransportingColonists(this.RewriteColonistsCount);

            scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Build));
            StartSectorBuildingQuest(1, 0);
        }
        private void RewriteColonistsCount()
        {
            scenarioUI.ShowInfoString(
                localizer.GetAnnounceTitle(FoundationScenarioStep.OuterRingBuilding, WINDOW_INFO_0) + ' ' +
                anchorBasement.colonistsArrived.ToString()
                )
                ;
        }

        public override void UIConditionProceedButton()
        {
            scenarioQuest.stepsAddInfo[0] = (stage+1).ToString() + "/6";
            if (stage < 6)
            {
                anchorBasement.AddSector(1, stage);
                conditionQuest.StopQuest(false);
                StartSectorBuildingQuest(1, stage);
                stage++;
            }
            else
            {
                anchorBasement.AddSector(1, stage);
                scenarioQuest.MakeQuestCompleted();
                conditionQuest.StopQuest(false);
                scenarioUI.DisableConditionPanel();
                conditionQuest = null;
                scenario.Next();
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
                stage = nstage;
                StartSectorBuildingQuest(1, stage);
            }
            anchorBasement.SetColonistUIUpdateFunction(this.RewriteColonistsCount);
        }
    }
    sealed class FDR_Example : FDR_Subscenario
    {
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
                default: return null;
            }
        }
    }

    #region save-load
    public override void Save(FileStream fs)
    {
        base.Save(fs);
        fs.WriteByte((byte)currentStep);
        if (subscenario == null || subscenario.completed) fs.WriteByte(0);
        else
        {
            fs.WriteByte(1);
            subscenario.Save(fs);
        }         
    }
    public override void Load(FileStream fs)
    {
        currentStep = (FoundationScenarioStep)fs.ReadByte();
        if (currentStep > FoundationScenarioStep.AnchorBuilding)
        {
            AssignAnchor( colony.GetBuilding(Structure.ANCHOR_BASEMENT_ID) as AnchorBasement );
        }
        int x = fs.ReadByte();
        if (x != 0)
        {
            StartSubscenario();
            subscenario.Load(fs);
        }
    }
    #endregion
}
