using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
namespace TutorialScenarioNS
{
    public enum TutorialStep : byte
    {
        QuestSystem, CameraBasics, Landing, ColonyInterfaceExplaining, BuildWindmill, GatherLumber, BuildFarm,
        StoneDigging, StorageLook, SmelteryBuilding, RecipeExplaining_A, RecipeExplaining_B, CollectConcrete, BuildDock, BuildDock_Error,
        Immigration, Trade, HQUpgrade, End
    }   

    #region steps
    sealed class IntroSCN : TutorialScenario
    {
        private IntroSCN(TutorialUI i_tutorialUI, MainCanvasController i_mcc) : base (i_tutorialUI, i_mcc)
        {
            blockCanvasRaycaster = true;
            step = TutorialStep.QuestSystem;
        }
        public override void StartScenario()
        {
            tutorialUI.SetShowframe(mcc.SYSTEM_GetQuestButton());
            tutorialUI.ShowarrowToShowframe_Left();
            mcc.SYSTEM_GetQuestButton().GetComponent<Button>().onClick.AddListener(this.DisableQuestButtonSelection);
            base.StartScenario();
        }
        private void DisableQuestButtonSelection()
        {
            mcc.SYSTEM_GetQuestButton().GetComponent<Button>().onClick.RemoveListener(this.DisableQuestButtonSelection);
            tutorialUI.DisableShowArrow();
            tutorialUI.DisableShowframe();
            mcc.questUI.SYSTEM_GetCloseButton().onClick.AddListener(this.EndScenario);
        }
        public override void EndScenario()
        {
            if (completed) return;
            var qui = mcc.questUI;
            var q = qui.GetActiveQuest();
            if (q == scenarioQuest)
            {
                mcc.questUI.SYSTEM_GetCloseButton().onClick.RemoveListener(this.EndScenario);
                base.EndScenario();
            }
        }
    }
    sealed class CameraStudySCN : TutorialScenario
    {
        private byte stage = 0;
        private CameraStudySCN(TutorialUI i_tutorialUI, MainCanvasController i_mcc) : base(i_tutorialUI, i_mcc)
        {
            blockCanvasRaycaster = true;
            step = TutorialStep.CameraBasics;
        }
        public override void OKButton()
        {
            tutorialUI.ActivateProceedTimer(1f);
        }
        public override void Proceed()
        {
            if (completed) return;
            stage++;
            switch (stage)
            {
                case 2: // rotating
                    {
                        var s = localizer.GetText(step, WINDOW_INFO_1);
                        tutorialUI.OpenTextWindow(s[0], s[1]);
                        tutorialUI.ActivateProceedTimer(1f);
                        s = localizer.GetText(TutorialStep.CameraBasics, QUEST_INFO_1);
                        scenarioQuest.FillText(s);
                        tutorialUI.ActivateProceedTimer(1f);
                        break;
                    }
                case 3: // slicing
                    {
                        var s = localizer.GetText(step, WINDOW_INFO_2);
                        tutorialUI.OpenTextWindow(s[0], s[1]);
                        tutorialUI.ActivateProceedTimer(1f);
                        s = localizer.GetText(TutorialStep.CameraBasics, QUEST_INFO_2);
                        scenarioQuest.FillText(s);
                        tutorialUI.SetShowframe(mcc.SYSTEM_GetLayerCutButton());
                        tutorialUI.ShowarrowToShowframe_Left();
                        tutorialUI.ActivateProceedTimer(1f);
                        break;
                    }
                case 4: // end
                    EndScenario();
                    return;
            }
        }
    }
    sealed class LandingSCN : TutorialScenario
    {
        private LandingSCN(TutorialUI i_tutorialUI, MainCanvasController i_mcc) : base(i_tutorialUI, i_mcc)
        {
            blockCanvasRaycaster = true;
            step = TutorialStep.Landing;
        }
        public override void OKButton()
        {
            GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.BuildingCheck;
        }
        private void BuildingCheck(Structure s)
        {
            if (s is HeadQuarters)
            {
                GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.BuildingCheck;
                base.EndScenario();
            }
        }
    }
    sealed class ColonyInterfaceExplainingSCN : TutorialScenario
    {
        private byte stage = 0;
        private ColonyInterfaceExplainingSCN(TutorialUI i_tutorialUI, MainCanvasController i_mcc) : base(i_tutorialUI, i_mcc)
        {
            blockCanvasRaycaster = true;
            step = TutorialStep.ColonyInterfaceExplaining;
            useQuest = false;
        }
        public override void StartScenario()
        {
            base.StartScenario();
            tutorialUI.SetShowframe(mcc.SYSTEM_GetCitizenString());
            tutorialUI.ShowarrowToShowframe_Up();
        }
        public override void OKButton()
        {
            stage++;
            if (stage == 1)
            {
                var s = localizer.GetText(step, WINDOW_INFO_1);
                tutorialUI.OpenTextWindow(s[0], s[1]);
            }
            else EndScenario();
        }
    }
    sealed class BuildWindmillSCN : TutorialScenario
    {
        private byte stage = 0;
        private BuildWindmillSCN(TutorialUI i_tutorialUI, MainCanvasController i_mcc) : base(i_tutorialUI, i_mcc)
        {
            blockCanvasRaycaster = true;
            step = TutorialStep.BuildWindmill;
        }
        public override void OKButton()
        {
            if (stage == 0) GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.BuildingCheck;
            else EndScenario();
        }
        private void BuildingCheck(Structure s)
        {
            if (s.ID == Structure.WIND_GENERATOR_1_ID)
            {
                GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.BuildingCheck;
                stage++;
                var str = localizer.GetText(step, WINDOW_INFO_1);
                tutorialUI.OpenTextWindow(str[0], str[1]);
            }
        }
    }



    #endregion

    abstract class TutorialScenario : Scenario
    {
        protected TutorialUI tutorialUI;
        protected MainCanvasController mcc;
        public bool blockCanvasRaycaster { get; protected set; }
        protected bool useQuest = true;
        public TutorialStep step
        {
            get { return (TutorialStep)_scenarioIndex; }
            set { _scenarioIndex = (byte)value; }
        }
        protected const byte WINDOW_INFO_0 = 0, QUEST_INFO_0 = 1, WINDOW_INFO_1 = 2, QUEST_INFO_1 = 3, WINDOW_INFO_2 = 4, QUEST_INFO_2 = 5;
        protected static Localizer localizer { get; private set; }

        static TutorialScenario()
        {
            localizer = new Localizer();
        }

        public TutorialScenario(TutorialUI i_tutorialUI, MainCanvasController i_mcc)
        {
            tutorialUI = i_tutorialUI;
            mcc = i_mcc;            
            blockCanvasRaycaster = false;
        }

        override public void StartScenario()
        {            
            var s = localizer.GetText(step, WINDOW_INFO_0);
            tutorialUI.OpenTextWindow(s[0], s[1]);
            if (useQuest)
            {
                scenarioQuest = mcc.questUI.SYSTEM_NewTutorialQuest(_scenarioIndex);
                s = localizer.GetText(step, QUEST_INFO_0);
                scenarioQuest.FillText(s);
            }
        }
        virtual public void Proceed() { }
        virtual public void OKButton() { }
        public override void EndScenario()
        {
            completed = true;
            if (useQuest)
            {
                scenarioQuest.MakeQuestCompleted();
                scenarioQuest = null;
            }
            tutorialUI.NextScenario();
        }

        public override void FillQuestData(Quest q)
        {
            var str = localizer.GetText(step, QUEST_INFO_0);
            if (str != null)
            {
                q.FillText(str);
            }
        }

        #region localization
        public class Localizer
        {
            private string[] lines;
            public Localizer()
            {
                using (StreamReader sr = File.OpenText("Assets/Locales/tutorialText_ENG.txt"))
                {
                    string s = sr.ReadLine();
                    int i = 0, indexLength = (int)char.GetNumericValue(s[0]),
                        count = int.Parse(s.Substring(2, 2)), index, length;
                    lines = new string[count];
                    while (i < count && !sr.EndOfStream)
                    {
                        s = sr.ReadLine();
                        if (s[0] == '-') continue;
                        else
                        {
                            length = s.Length;
                            if (s[0] == '[' && s[indexLength] == ']')
                            {
                                index = int.Parse(s.Substring(1, indexLength));
                                if (lines[index] != null) UnityEngine.Debug.Log("string " + index.ToString() + " was rewrited");
                                lines[index] = s.Substring(indexLength + 1, length - indexLength - 1);
                                i++;
                            }
                            else UnityEngine.Debug.Log("error in line " + i.ToString());
                        }
                    }
                }
            }

            public string[] GetText(TutorialStep step, byte subIndex)
            {
                switch (step)
                {
                    case TutorialStep.QuestSystem:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[1], lines[2] };
                                case QUEST_INFO_0: return new string[3] { lines[3], lines[4], lines[5] };
                            }
                            break;
                        }
                    case TutorialStep.CameraBasics:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[6], FollowingCamera.touchscreen? lines[7] : lines[8] };
                                case QUEST_INFO_0: return new string[3] { lines[6], lines[9], lines[10] };
                                case WINDOW_INFO_1: return new string[2] { lines[11], FollowingCamera.touchscreen ? lines[12] : lines[13] };
                                case QUEST_INFO_1: return new string[3] { lines[11], lines[14], lines[15] };
                                case WINDOW_INFO_2: return new string[2] { lines[16], lines[17] };
                                case QUEST_INFO_2: return new string[3] { lines[16], lines[18], lines[19] };
                            }
                            break;
                        }
                    case TutorialStep.Landing:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[13], lines[14] };
                                case QUEST_INFO_0: return new string[3] { lines[13], lines[15], lines[16] };
                            }
                            break;
                        }
                    case TutorialStep.ColonyInterfaceExplaining:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[15], lines[16] };
                                case WINDOW_INFO_1: return new string[2] { lines[15], lines[17] };
                            }
                            break;
                        }
                    case TutorialStep.BuildWindmill:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[18], lines[19] };
                                case WINDOW_INFO_1: return new string[2] { lines[18], lines[20] };
                                case QUEST_INFO_0: return new string[3] { lines[21], lines[22], lines[23] };
                            }
                            break;
                        }
                }
                return null;
            }
        }

       
        #endregion
    }
}
}

                                    case TutorialUI.TutorialStep.BuildWindmill_0:
                                        {
                                            q.name = "
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.GatherLumber:
                                        {
                                            q.name = "Gather lumber";
                                            q.description = "Select a surface with trees and click the GATHER button to take all lumber for that cell. You can add more worker to a worksite, just pressing the " +
                        "plus buttons in the right part of the appeared worksite window.";
                                            q.steps[0] = "Lumber collected ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.BuildFarm:
                                        {
                                            q.name = "Build farm";
                                            q.description = "Build farm and assign no less than " + TutorialUI.FARM_QUEST_WORKERS_COUNT.ToString() + " workers.";
                                            q.steps[0] = "Farm built";
                                            q.steps[1] = "Workers assigned ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.StoneDigging:
                                        {
                                            q.name = "Stone digging";
                                            q.description = "Click on any side of a stone block and the press the DIG button";
                                            q.steps[0] = "Stone collected ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.SmelteryBuilding:
                                        {
                                            q.name = "Smeltery building";
                                            q.description = "Build a Smeltery for an access to crafting recipes. Also you can build second Stream generator for proper smeltery function.";
                                            q.steps[0] = "Smeltery built ";
                                            q.steps[1] = "(Additional) Stream generator built";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.RecipeExplaining_A:
                                        {
                                            q.name = "Resource producing";
                                            q.description = "Set in newfound Smeltery Stone-to-Lconcrete recipe and then power it up";
                                            q.steps[0] = "Recipe set ";
                                            q.steps[1] = "Powered up ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.CollectConcrete:
                                        {
                                            q.name = "Collect concrete";
                                            q.description = "Collect enough l-concrete for dock building. Advice: increase smeltery's workers count and sure that there is enough power to operate.";
                                            q.steps[0] = "Concrete collected ";
                                            break;
                                        }
                                    case TutorialUI.TutorialStep.BuildDock:
                                        {
                                            q.name = "Dock building";
                                            q.description = "Find a place on a island shore with " + Dock.SMALL_SHIPS_PATH_WIDTH.ToString() + 'x' + Dock.SMALL_SHIPS_PATH_WIDTH.ToString() +
                                                " space to allow trade ships to dock. Rotate dock manually if it faces wrong, using small arrows at the top of its observer panel. /n" +
                                                "If you placed dock wrong, use button in top-right corner to demolish it - in this scenario your resources will be returned.";
                                            q.steps[0] = "Dock built ";
                                            q.steps[1] = "Dock is working ";
                                            break;
                                        }
                                }
