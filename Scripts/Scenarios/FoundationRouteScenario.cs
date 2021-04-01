using System.IO;
using UnityEngine;
public class FoundationRouteScenario : Scenario
{
    private enum FoundationScenarioStep { Begin, AnchorBuilding, AnchorStart, InnerRingBuilding, OuterRingBuilding }
    
    private FoundationScenarioStep currentStep;
    private StandartScenarioUI scenarioUI;
    private AnchorBasement anchorBasement;
    private FR_Subscenario subscenario;
    private QuestUI questUI;
    private Localizer localizer;
    private ColonyController colony;
    protected ScenarioQuest scenarioQuest;

    private const byte WINDOW_INFO_0 = 1, WINDOW_INFO_1 =2, QUEST_INFO_0 = 3, QUEST_INFO_1 = 4;
    private const int ANCHOR_LAUNCH_ENERGYCOST = 70000;

    public FoundationRouteScenario() : base(FOUNDATION_ROUTE_ID)
    {
        localizer = new Localizer();
        colony = GameMaster.realMaster.colonyController;
        questUI = UIController.GetCurrent().GetMainCanvasController().questUI;
    }

    public override void StartScenario()
    {        
        scenarioUI = StandartScenarioUI.GetCurrent(this);
        scenarioUI.ChangeIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));

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
        }
        subscenario.StartScenario();
    }
    public void Next()
    {
        currentStep++;
        StartSubscenario();
    }
    protected void StartQuest(Scenario s)
    {
        scenarioQuest = new ScenarioQuest(s);
        questUI.SYSTEM_NewScenarioQuest(scenarioQuest);
        scenarioQuest.FillText(localizer.GetQuestData(currentStep));
    }
    protected void AssignAnchor(AnchorBasement ab)
    {
        anchorBasement = ab;
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
    private class FR_Subscenario : Scenario
    {
        protected readonly FoundationRouteScenario scenario;
        protected StandartScenarioUI scenarioUI { get { return scenario.scenarioUI; } }
        protected AnchorBasement anchorBasement { get { return scenario.anchorBasement; } }
        protected QuestUI questUI { get { return scenario.questUI; } }
        protected Localizer localizer { get { return scenario.localizer; } }
        protected ColonyController colony { get { return scenario.colony; } }
        protected Quest scenarioQuest { get { return scenario.scenarioQuest; } }

        public FR_Subscenario(FoundationRouteScenario i_scenario) : base(FOUNDATION_ROUTE_ID)
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
    }
    private class FR_ConditionSubscenario : FR_Subscenario
    {
        protected ConditionQuest conditionQuest;
        public FR_ConditionSubscenario(FoundationRouteScenario i_scenario) : base (i_scenario) { }

        protected void PrepareSectorBuildingQuest(byte ring, byte index)
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
                conditionQuest.FillText(
                    Localization.GetPhrase(LocalizedPhrase.InnerRingConstruction) + ' ' + index.ToString(),
                    localizer.lines[13]
                    );
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
                        //conditions[0] = SimpleCondition.GetGearsCondition(4f);
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
                        //conditions[2] = SimpleCondition.GetResourceCondition(ResourceType.Plastics, 7530f); 
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
            }
        }
        public override byte GetStepsCount()
        {
            return 3;
        }
    }

    sealed class FDR_Begin : FR_Subscenario
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
    }
    sealed class FDR_AnchorBuilding : FR_Subscenario
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
                scenario.Next();
            }
        }        
    }
    sealed class FDR_AnchorStart : FR_Subscenario
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
                scenario.StartQuest(this);
                scenarioQuest.FillText(localizer.GetQuestData(FoundationScenarioStep.AnchorStart));
                scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Anchor_verb));
                scenarioUI.ShowConditionPanel(scenarioQuest);
            }
        }
        public override void CheckConditions()
        {
            int es = (int)colony.energyStored;
            bool a = anchorBasement.isEnergySupplied, b = es >= ANCHOR_LAUNCH_ENERGYCOST;
            scenarioQuest.SetStepCompleteness(0, a);
            scenarioQuest.SetStepCompleteness(1, b);
            scenarioQuest.stepsAddInfo[1] = (es / 1000).ToString() + '/' + (ANCHOR_LAUNCH_ENERGYCOST / 1000).ToString();
            scenarioUI.UpdateConditionInfo();
            scenarioUI.SetConditionButtonActivity(a & b);
        }
        public override void UIConditionProceedButton()
        {
            if (stage == 2)
            {
                stage++;
                scenarioQuest.MakeQuestCompleted();
                scenarioUI.DisableConditionPanel();
                anchorBasement.StartActivating(scenario.Next);
            }
        }
        public override byte GetStepsCount()
        {
            return 2;
        }
    }
    sealed class FDR_InnerRingBuilding : FR_ConditionSubscenario
    {
        private byte stage = 0;
        public FDR_InnerRingBuilding(FoundationRouteScenario i_scn) : base(i_scn) { }
        
        public override void StartScenario()
        {
            scenario.StartQuest(this);
            scenarioUI.ChangeAnnouncementText(localizer.GetAnnounceTitle(FoundationScenarioStep.InnerRingBuilding, WINDOW_INFO_0));
            scenarioUI.ShowAnnouncePanel();                
        }
        public override void OKButton()
        {
            if (stage == 0)
            {
                scenarioUI.CloseAnnouncePanel();
                stage++;
                PrepareSectorBuildingQuest(0, 0);
                scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Build));
                scenarioUI.ShowConditionPanel(conditionQuest);
            }
        }
        public override void UIConditionProceedButton()
        {
            byte ringStage = stage;
            stage++;
            ringStage--;
            if (ringStage < 6)
            {
                anchorBasement.AddInnerSector();                
                PrepareSectorBuildingQuest(0, ringStage);
            }
            else
            {
                scenarioQuest.MakeQuestCompleted();
                conditionQuest.StopQuest(false);
                conditionQuest = null;
                scenario.Next();
            }
        }
    }
    sealed class FDR_Example : FR_Subscenario
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
        public readonly string[] lines;
        private readonly string routeName = Localization.GetPhrase(LocalizedPhrase.FoundationRoute);
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
                         return lines[6] + ' ' + lines[8] + ' ' + ANCHOR_LAUNCH_ENERGYCOST / 1000 + ' ' + lines[7];
                    }
                case FoundationScenarioStep.InnerRingBuilding:
                    {
                        return lines[11] + ((int)AnchorBasement.MAX_EXCESS_POWER);
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
                        return new string[4]
                        {
                            routeName,
                             lines[8] + ' ' + ANCHOR_LAUNCH_ENERGYCOST.ToString() + lines[9] + Localization.GetWord(LocalizedWord.Anchor_verb) + lines[10],
                             lines[15],
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
                default: return null;
            }
        }
    }
}
