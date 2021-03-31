using System.IO;
using UnityEngine;
public sealed class FoundationRouteScenario : Scenario
{
    private enum FoundationScenarioStep { Begin, AnchorBuilding, AnchorStart, InnerRingBuilding, OuterRingBuilding}

    private readonly ColonyController colony;
    private FoundationScenarioStep currentStep; private byte stage = 0;
    private StandartScenarioUI scenarioUI;
    private AnchorBasement anchorBasement;
    private readonly QuestUI questUI;
    private readonly Storage storage;
    private readonly Localizer localizer;
    private const byte WINDOW_INFO_0 = 1, WINDOW_INFO_1 =2, QUEST_INFO_0 = 3;
    private const int ANCHOR_LAUNCH_ENERGYCOST = 150000;     
    

    public FoundationRouteScenario() : base(1)
    {
        localizer = new Localizer();
        colony = GameMaster.realMaster.colonyController;
        storage = colony.storage;
        questUI = UIController.GetCurrent().GetMainCanvasController().questUI;
    }

    public override void StartScenario()
    {
        currentStep = FoundationScenarioStep.Begin;        
        //
        scenarioUI = StandartScenarioUI.GetCurrent(this);
        scenarioUI.ChangeIcon(UIController.iconsTexture, UIController.GetIconUVRect(Icons.FoundationRoute));
        PrepareStep();
    }
    private void PrepareStep() // отдельный метод нужен для возможности сохранения-загрузки
    {
        void StartQuest()
        {
            scenarioQuest = new ScenarioQuest(this);
            questUI.SYSTEM_NewScenarioQuest(scenarioQuest);
            scenarioQuest.FillText(Localization.GetPhrase(LocalizedPhrase.FoundationRoute), localizer.GetText(currentStep, QUEST_INFO_0));
        }
        switch (currentStep)
        {
            case FoundationScenarioStep.Begin:
                {
                    switch (stage)
                    {
                        case 0:
                            scenarioUI.ChangeAnnouncementText(localizer.GetText(currentStep, WINDOW_INFO_0));
                            scenarioUI.ShowAnnouncePanel();
                            break;
                        case 1:
                            scenarioUI.ChangeAnnouncementText(localizer.GetText(currentStep, WINDOW_INFO_1));
                            break;
                    }
                    break;
                }
            case FoundationScenarioStep.AnchorBuilding:
                {
                    StartQuest();
                    GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.BuildingCheck;
                    break;
                }
            case FoundationScenarioStep.AnchorStart:
                {
                    scenarioUI.ChangeAnnouncementText(localizer.GetText(currentStep, WINDOW_INFO_0));
                    scenarioUI.ShowAnnouncePanel();
                    break;
                }
            case FoundationScenarioStep.InnerRingBuilding:
                {
                    if (stage == 0)
                    {
                        scenarioUI.ChangeAnnouncementText(localizer.GetText(currentStep, WINDOW_INFO_0));
                        scenarioUI.ShowAnnouncePanel();
                    }
                    else
                    {
                        PrepareSectorBuildingQuest(0, 0);                       
                        scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Build));
                        scenarioUI.ShowConditionPanel(scenarioQuest);
                    }
                    break;
                }
        }
    }
    public override void EndScenario()
    {
        GameMaster.realMaster.UnbindScenario(this);
        scenarioUI?.ScenarioEnds(this);
    }
    //
    override public void OKButton()
    {
        switch (currentStep)
        {
            case FoundationScenarioStep.Begin:
                {
                    switch (stage)
                    {
                        case 0:
                            {
                                stage++;
                                PrepareStep();
                                break;
                            }
                        case 1:
                            {
                                scenarioUI.CloseAnnouncePanel();
                                currentStep = FoundationScenarioStep.AnchorBuilding;                                
                                PrepareStep();
                                break;
                            }
                    }
                    break;
                }
            case FoundationScenarioStep.AnchorStart:
                {
                    stage++;
                    scenarioUI.CloseAnnouncePanel();
                    scenarioUI.ChangeConditionButtonLabel(Localization.GetWord(LocalizedWord.Anchor_verb));
                    scenarioUI.ShowConditionPanel(scenarioQuest);
                    break;
                }
            case FoundationScenarioStep.InnerRingBuilding:
                {
                    scenarioUI.CloseAnnouncePanel();
                    stage++;
                    PrepareStep();
                    break;
                }
        }
    }
    //
    private void BuildingCheck(Structure s)
    {
        if (currentStep == FoundationScenarioStep.AnchorBuilding && s.ID == Structure.ANCHOR_BASEMENT_ID)
        {
            anchorBasement = s as AnchorBasement;
            GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.BuildingCheck;
            scenarioQuest.MakeQuestCompleted();
            currentStep++;
            stage = 0;
            PrepareStep();
        } 
    }
    private void PrepareSectorBuildingQuest(byte ring, byte index)
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
            scenarioQuest = new ConditionScenarioQuest(this, conditions, colony);
            scenarioQuest.FillText(
                Localization.GetPhrase(LocalizedPhrase.InnerRingConstruction) + ' ' + index.ToString(),
                localizer.lines[13]
                );
            questUI.SYSTEM_NewScenarioQuest(scenarioQuest);
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
    override public void CheckConditions() {
            if (currentStep == FoundationScenarioStep.AnchorStart && stage == 1)
            {
                bool a = anchorBasement.isEnergySupplied, b = colony.energyStored >= ANCHOR_LAUNCH_ENERGYCOST;
                scenarioQuest.SetStepCompleteness(0, a);
                scenarioQuest.SetStepCompleteness(1, b);
                scenarioUI.UpdateConditionInfo();
                scenarioUI.SetConditionButtonActivity(a & b);
            }
    }
    public override void UIConditionProceedButton()
    {
        if (currentStep == FoundationScenarioStep.InnerRingBuilding)
        {
            scenarioQuest.MakeQuestCompleted();
            if (stage < 7)
            {                
                anchorBasement.AddInnerSector();
                stage++;
                PrepareSectorBuildingQuest(0, stage);
            }
            else
            {
                stage = 0;
                currentStep++;
                PrepareSectorBuildingQuest(1, 0);
            }
        }
        else
        {
            if (currentStep == FoundationScenarioStep.AnchorStart && stage == 2)
            {
                scenarioQuest.MakeQuestCompleted();
                scenarioUI.DisableConditionPanel();
                GameMaster.audiomaster.MakeSoundEffect(SoundEffect.FD_anchorLaunch);
                // graphic effect
                currentStep++;
                stage = 0;
                anchorBasement.StartActivating(this.PrepareStep);
            }
        }
    }
    //
   
    //
    override public bool QuestMustCheckConditions() { return false; }
    override public byte GetStepsCount() {
        switch (currentStep)
        {
            case FoundationScenarioStep.InnerRingBuilding:                
            case FoundationScenarioStep.OuterRingBuilding:
                return 3;
            case FoundationScenarioStep.AnchorStart:
                return 2;
            default: return 1;
        }
    }
    //
    private sealed class Localizer
    {
        public readonly string[] lines;
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
        public string GetText(FoundationScenarioStep step, byte subIndex)
        {
            switch (step)
            {
                case FoundationScenarioStep.Begin:
                    {
                        switch (subIndex)
                        {
                            case WINDOW_INFO_0: return lines[0];
                            case WINDOW_INFO_1: return lines[1] + ' ' + Localization.GetStructureName(Structure.ANCHOR_BASEMENT_ID) + lines[2];
                        }
                        break;
                    }
                case FoundationScenarioStep.AnchorBuilding:
                    {
                        switch (subIndex)
                        {
                            case QUEST_INFO_0: return lines[4] + ' ' + Localization.GetStructureName(Structure.ANCHOR_BASEMENT_ID) + ' ' + lines[5];
                        }
                        break;
                    }
                case FoundationScenarioStep.AnchorStart:
                    {
                        switch (subIndex)
                        {
                            case WINDOW_INFO_0: return lines[6] + ' ' + lines[8] + ' ' + ANCHOR_LAUNCH_ENERGYCOST / 1000 + ' ' + lines[7];
                            case QUEST_INFO_0: return lines[8] + ' ' + ANCHOR_LAUNCH_ENERGYCOST.ToString() + lines[9] + Localization.GetWord(LocalizedWord.Anchor_verb) + lines[10];
                        }
                        break;
                    }
                case FoundationScenarioStep.InnerRingBuilding:
                    {
                        switch (subIndex)
                        {
                            case WINDOW_INFO_0: return lines[11] + ((int)AnchorBasement.MAX_EXCESS_POWER);
                            case QUEST_INFO_0: return lines[13];
                        }
                        break;
                    }
            }
            return null;
        }
    }
}
