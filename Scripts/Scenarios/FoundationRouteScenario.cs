using System.IO;
using UnityEngine;
public sealed class FoundationRouteScenario : Scenario
{
    private enum FoundationScenarioStep { Begin, AnchorBuilding, AnchorStart, InnerRingBuilding, OuterRingBuilding}

    private readonly ColonyController colony;
    private FoundationScenarioStep currentStep; private byte stage = 0;
    private StandartScenarioUI scenarioUI;
    private AnchorBasement anchorBasement;
    private QuestUI questUI;
    private readonly Localizer localizer;
    private const byte WINDOW_INFO_0 = 1, WINDOW_INFO_1 =2, QUEST_INFO_0 = 3;
    private const int ANCHOR_LAUNCH_ENERGYCOST = 150000;

    public FoundationRouteScenario() : base(1)
    {
        localizer = new Localizer();
        colony = GameMaster.realMaster.colonyController;
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
            scenarioQuest = questUI.SYSTEM_NewScenarioQuest(this);
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
                        StartQuest();
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

    override public bool QuestMustCheckConditions() { return false; }
    override public byte GetStepsCount() { return 1; }
    //
    private sealed class Localizer
    {
        private string[] lines;
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
                            case QUEST_INFO_0: return lines[12];
                        }
                        break;
                    }
            }
            return null;
        }
    }
}
