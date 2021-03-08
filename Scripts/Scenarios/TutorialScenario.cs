using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
namespace TutorialScenarioNS
{
    public enum TutorialStep : byte
    {
        QuestSystem, CameraBasics, Landing, ColonyInterfaceExplaining, BuildWindmill, GatherLumber, BuildFarm,
        StoneDigging, StorageLook, SmelteryBuilding, BuildDock, DockObserving, UpgradeHQ
    }   


    abstract class TutorialScenario : Scenario
    {
        protected static TutorialUI tutorialUI;
        protected static MainCanvasController mcc;
        public bool blockCanvasRaycaster { get; protected set; }
        protected bool useQuest = true;
        public TutorialStep step
        {
            get { return (TutorialStep)_scenarioIndex; }
            set { _scenarioIndex = (byte)value; }
        }
        protected const byte WINDOW_INFO_0 = 0, QUEST_INFO_0 = 1, WINDOW_INFO_1 = 2, QUEST_INFO_1 = 3, WINDOW_INFO_2 = 4,
            QUEST_INFO_2 = 5, SPECIAL_0 = 6, WINDOW_INFO_3 = 7;
        protected static Localizer localizer { get; private set; }
        
        static TutorialScenario()
        {
            localizer = new Localizer();
        }
        public static void Initialize(TutorialUI i_tutorialUI, MainCanvasController i_mcc)
        {
            tutorialUI = i_tutorialUI;
            mcc = i_mcc;
        }
        public static TutorialScenario GetScenario(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.QuestSystem: return new IntroSCN();
                case TutorialStep.CameraBasics: return new CameraStudySCN();
                case TutorialStep.Landing: return new LandingSCN();
                case TutorialStep.ColonyInterfaceExplaining: return new ColonyInterfaceExplainingSCN();
                case TutorialStep.BuildWindmill: return new BuildWindmillSCN();
                case TutorialStep.GatherLumber: return new GatherLumberSCN();
                case TutorialStep.BuildFarm: return new BuildFarmSCN();
                case TutorialStep.StoneDigging: return new StoneDiggingSCN();
                case TutorialStep.StorageLook: return new StorageLookingSCN();
                case TutorialStep.SmelteryBuilding: return new SmelteryBuildingSCN();
                case TutorialStep.BuildDock: return new DockObservingSCN();
                case TutorialStep.DockObserving: return new DockObservingSCN();
                case TutorialStep.UpgradeHQ: return new UpgradeHQSCN();
                default: return null;
            }
        }


        protected TutorialScenario()
        {         
            blockCanvasRaycaster = false;
        }

        override public void StartScenario()
        {
            if (!useSpecialWindowFilling) tutorialUI.OpenTextWindow(localizer.GetText(step, WINDOW_INFO_0));
            else SpecialWindowFilling();
            if (useQuest)
            {
                scenarioQuest = mcc.questUI.SYSTEM_NewScenarioQuest(this);
                if (useSpecialQuestFilling) SpecialQuestFilling();
                else
                {
                    scenarioQuest.FillText(localizer.GetText(step, QUEST_INFO_0));
                }
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

        public override bool QuestMustCheckConditions() { return false; }
        virtual public bool DropAnySelectionWhenWindowOpens() { return true; }


        #region steps
        sealed class IntroSCN : TutorialScenario
        {
            Button closeButtonHolder;
            public IntroSCN() : base()
            {
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
                closeButtonHolder = mcc.questUI.SYSTEM_GetQuestButton((int)scenarioQuest.DefineQuestSection());
                closeButtonHolder.onClick.AddListener(this.OpenedQuestCheck);
            }
            private void OpenedQuestCheck()
            {
                var q = mcc.questUI.GetActiveQuest();
                if (q == scenarioQuest)
                {
                    mcc.questUI.SYSTEM_GetCloseButton().onClick.AddListener(this.EndScenario);
                    closeButtonHolder.onClick.RemoveListener(this.OpenedQuestCheck);
                }
            }
            public override void EndScenario()
            {
                if (completed) return;
                var qui = mcc.questUI;
                if (qui.GetActiveQuest() == Quest.NoQuest && !qui.IsEnabled())
                {
                    qui.SYSTEM_GetCloseButton().onClick.RemoveListener(this.EndScenario);
                    base.EndScenario();
                }
            }
        }
        sealed class CameraStudySCN : TutorialScenario
        {
            private byte stage = 0;
            public CameraStudySCN() : base()
            {
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
                    case 1: // rotating
                        {
                            var s = localizer.GetText(step, WINDOW_INFO_1);
                            tutorialUI.OpenTextWindow(s[0], s[1]);
                            tutorialUI.ActivateProceedTimer(1f);
                            s = localizer.GetText(TutorialStep.CameraBasics, QUEST_INFO_1);
                            scenarioQuest.FillText(s);
                            break;
                        }
                    case 2: // slicing
                        {
                            var s = localizer.GetText(step, WINDOW_INFO_2);
                            tutorialUI.OpenTextWindow(s[0], s[1]);
                            tutorialUI.ActivateProceedTimer(1f);
                            s = localizer.GetText(TutorialStep.CameraBasics, QUEST_INFO_2);
                            scenarioQuest.FillText(s);
                            tutorialUI.SetShowframe(mcc.SYSTEM_GetLayerCutButton());
                            tutorialUI.ShowarrowToShowframe_Left();
                            break;
                        }
                    case 3: // end
                        EndScenario();
                        return;
                }
            }
        }
        sealed class LandingSCN : TutorialScenario
        {
            public LandingSCN() : base()
            {
                step = TutorialStep.Landing;
            }
            public override void OKButton()
            {
                GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.BuildingCheck;
                Zeppelin.CreateNew();
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
            public ColonyInterfaceExplainingSCN() : base()
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
            public BuildWindmillSCN() : base()
            {
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
        sealed class GatherLumberSCN : TutorialScenario
        {
            private Storage storage;
            private const int LUMBER_COUNT = 200;

            public GatherLumberSCN() : base()
            {
                step = TutorialStep.GatherLumber;
                storage = GameMaster.realMaster.colonyController.storage;
            }
            public override void CheckConditions()
            {
                int stCount = (int)storage.standartResources[ResourceType.LUMBER_ID];
                scenarioQuest.ChangeAddInfo(0, stCount.ToString() + " / " + LUMBER_COUNT.ToString());
                if (stCount >= LUMBER_COUNT) EndScenario();
            }
            public override bool QuestMustCheckConditions() { return true; }
        }
        sealed class BuildFarmSCN : TutorialScenario
        {
            private ColonyController colony;
            private const int WORKERS_COUNT = 20;

            public BuildFarmSCN() : base()
            {
                step = TutorialStep.BuildFarm;
                colony = GameMaster.realMaster.colonyController;
                useSpecialQuestFilling = true;
                useSpecialWindowFilling = true;
            }
            public override void CheckConditions()
            {
                var farms = colony.GetBuildings<Farm>();
                if (farms != null && farms.Count > 0)
                {
                    scenarioQuest.SetStepCompleteness(0, true);
                    int maxWorkers = 0;
                    foreach (var f in farms)
                    {
                        if (f.workersCount > maxWorkers) maxWorkers = f.workersCount;
                    }
                    scenarioQuest.ChangeAddInfo(1, maxWorkers.ToString() + " / " + WORKERS_COUNT.ToString());
                    if (maxWorkers >= WORKERS_COUNT)
                    {
                        scenarioQuest.SetStepCompleteness(1, true);
                        EndScenario();
                        return;
                    }
                    else scenarioQuest.SetStepCompleteness(1, false);
                }
                else
                {
                    scenarioQuest.SetStepCompleteness(0, false);
                    scenarioQuest.SetStepCompleteness(1, false);
                    scenarioQuest.ChangeAddInfo(1, string.Empty);
                }
            }
            public override bool QuestMustCheckConditions() { return true; }
            public override void SpecialQuestFilling()
            {
                var s = localizer.GetText(step, QUEST_INFO_0);
                var ns = new string[4];
                ns[0] = s[0];
                ns[1] = s[1] + ' ' + WORKERS_COUNT.ToString() + ' ' + s[2];
                ns[2] = s[3];
                ns[3] = s[4];
                scenarioQuest.FillText(ns);
            }
            public override void SpecialWindowFilling()
            {
                var s = localizer.GetText(step, WINDOW_INFO_0);
                tutorialUI.OpenTextWindow(s[0], s[1] + ' ' + WORKERS_COUNT.ToString() + ' ' + s[2]);
            }
            public override byte GetStepsCount() { return 2; }
        }
        sealed class StoneDiggingSCN : TutorialScenario
        {
            private Storage storage;
            private const int STONE_COUNT = 250;

            public StoneDiggingSCN() : base()
            {
                step = TutorialStep.StoneDigging;
                storage = GameMaster.realMaster.colonyController.storage;
            }
            public override void CheckConditions()
            {
                int stCount = (int)storage.standartResources[ResourceType.STONE_ID];
                scenarioQuest.ChangeAddInfo(0, ' ' + stCount.ToString() + " / " + STONE_COUNT.ToString());
                if (stCount >= STONE_COUNT) EndScenario();
            }
            public override bool QuestMustCheckConditions() { return true; }
        }
        sealed class StorageLookingSCN : TutorialScenario
        {
            private bool storageHighlightActive = false;
            private byte stage = 0;

            public StorageLookingSCN() : base()
            {
                step = TutorialStep.StorageLook;
                useQuest = false;
            }
            public override void StartScenario()
            {
                if (!mcc.IsStorageUIActive())
                {
                    var sb = mcc.SYSTEM_GetStorageButton();
                    tutorialUI.SetShowframe(sb);
                    tutorialUI.ShowarrowToShowframe_Left();
                    sb.GetComponent<Button>().onClick.AddListener(this.Proceed);
                    storageHighlightActive = true;
                    tutorialUI.OpenTextWindow(localizer.GetText(step, WINDOW_INFO_0));
                }
                else
                {
                    storageHighlightActive = false;
                    Proceed();
                }

            }
            override public void Proceed()
            {
                if (storageHighlightActive)
                {
                    mcc.SYSTEM_GetStorageButton().GetComponent<Button>().onClick.RemoveListener(this.Proceed);
                    tutorialUI.DisableShowframe();
                    tutorialUI.DisableShowArrow();
                    storageHighlightActive = false;
                }
                var s = localizer.GetText(step, WINDOW_INFO_1);
                tutorialUI.OpenTextWindow(s[0], s[1]);
                stage++;
            }
            public override void OKButton()
            {
                if (stage == 1) EndScenario();
            }
        }
        sealed class SmelteryBuildingSCN : TutorialScenario
        {
            private byte stage = 0;
            private bool subscribedToPowerButton = false;
            private Factory observingFactory;
            public override byte GetStepsCount() { return 3; }
            public override bool DropAnySelectionWhenWindowOpens() { return stage == 0; }
            public override bool QuestMustCheckConditions() { return true; }

            public SmelteryBuildingSCN() : base()
            {
                step = TutorialStep.SmelteryBuilding;
            }

            public override void OKButton()
            {
                switch (stage)
                {
                    case 0: // beginning
                        GameMaster.realMaster.eventTracker.buildingConstructionEvent += this.BuildingCheck;
                        break;
                    case 2: // recipe set
                        if (!subscribedToPowerButton)
                        {
                            Building.buildingObserver.SYSTEM_GetEnergyButton().onClick.AddListener(this.PowerSupplyCheck);
                            subscribedToPowerButton = true;
                        }
                        break;
                    case 4: // end comment
                        EndScenario();
                        break;
                }
            }
            private void BuildingCheck(Structure s)
            {
                if (s.ID == Structure.SMELTERY_1_ID)
                {
                    observingFactory = s as Factory;
                    scenarioQuest.SetStepCompleteness(0, true);
                    stage++;
                    
                    tutorialUI.OpenTextWindow(localizer.GetText(step, WINDOW_INFO_1));
                    var dd = Factory.GetFactoryObserver().SYSTEM_GetRecipesDropdown();
                    tutorialUI.SetShowframe(dd.GetComponent<RectTransform>());
                    dd.onValueChanged.AddListener(this.RecipeChanged);
                    mcc.Select(observingFactory);
                }
                else
                {
                    if (s.ID == Structure.WIND_GENERATOR_1_ID)
                    {
                        scenarioQuest.SetStepCompleteness(3, true);
                    }
                }
            }
            private void RecipeChanged(int i)
            {
                if (observingFactory != null && observingFactory.GetRecipe() == Recipe.StoneToConcrete)
                {
                    stage++;
                    Factory.GetFactoryObserver().SYSTEM_GetRecipesDropdown().onValueChanged.RemoveListener(this.RecipeChanged);
                    tutorialUI.DisableShowframe();
                    tutorialUI.OpenTextWindow(localizer.GetText(step, WINDOW_INFO_2));
                    ObservingFactoryCheck();
                    //
                }
            }
            private void PowerSupplyCheck()
            {
                if (observingFactory.isActive)
                {
                    stage++;
                    if (observingFactory.workersCount == 0)
                    {
                        tutorialUI.OpenTextWindow(localizer.GetText(step, SPECIAL_0));
                        return;
                    }
                    else INLINE_UnsubscribeFromPowerButton();
                }
            }
            private void INLINE_UnsubscribeFromPowerButton()
            {
                if (subscribedToPowerButton)
                {
                    Building.buildingObserver.SYSTEM_GetEnergyButton().onClick.RemoveListener(this.PowerSupplyCheck);
                    subscribedToPowerButton = false;
                }
            }

            public override void CheckConditions()
            {
                if (stage < 4)
                {
                    observingFactory = Factory.TryGetFactoryObserver()?.observingFactory;
                    if (observingFactory != null) ObservingFactoryCheck();
                }
            }
            private void ObservingFactoryCheck()
            {
                byte conditionsMet = 0;
                if (observingFactory.GetRecipe() == Recipe.StoneToConcrete)
                {
                    conditionsMet++;
                    scenarioQuest.SetStepCompleteness(1, true);
                }
                else scenarioQuest.SetStepCompleteness(1, false);
                if (observingFactory.workersCount != 0)
                {
                    conditionsMet++;
                    scenarioQuest.SetStepCompleteness(2, true);
                }
                else scenarioQuest.SetStepCompleteness(2, false);
                if (observingFactory.isActive )
                {
                    conditionsMet++;
                    scenarioQuest.SetStepCompleteness(3, true);
                }
                else scenarioQuest.SetStepCompleteness(3, false);
                if (conditionsMet == 3 && stage == 3) {
                    stage++;
                    INLINE_UnsubscribeFromPowerButton();
                    tutorialUI.OpenTextWindow(localizer.GetText(step, WINDOW_INFO_3));
                }
            }

            public override void EndScenario()
            {
                GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.BuildingCheck;
                INLINE_UnsubscribeFromPowerButton();
                base.EndScenario();
            }
        }
        sealed class BuildDockSCN : TutorialScenario
        {
            private ColonyController colony;
            public override byte GetStepsCount() { return 3; }

            public BuildDockSCN() : base()
            {
                step = TutorialStep.BuildDock;
                colony = GameMaster.realMaster.colonyController;
                useSpecialQuestFilling = true;
            }
            public override void SpecialQuestFilling()
            {
                var s = localizer.GetText(step, QUEST_INFO_0);
                var ns = new string[2];
                ns[0] = s[0];
                ns[1] = s[1] + ' ' + ResourcesCost.DOCK_CONCRETE_COSTVOLUME.ToString() + ' ' + s[2];
                scenarioQuest.FillText(ns);
            }
            public override void CheckConditions()
            {
                //
                bool haveDocks = false;
                if (colony.docks != null)
                {
                    int count = colony.docks.Count;
                    if (count != 0)
                    {
                        haveDocks = true;
                        bool success = false;
                        if (count == 1)
                        {
                            if (colony.docks[0].isCorrectLocated) success = true;
                        }
                        else
                        {
                            foreach (var d in colony.docks)
                            {
                                if (d.isCorrectLocated)
                                {
                                    success = true;
                                    break;
                                }
                            }
                        }
                        if (success)
                        {
                            scenarioQuest.SetStepCompleteness(2, true);
                            EndScenario();
                            return;
                        }
                        else
                        {
                            tutorialUI.OpenTextWindow(localizer.GetText(step, SPECIAL_0));
                            scenarioQuest.SetStepCompleteness(2, false);
                        }
                    }
                }
                scenarioQuest.SetStepCompleteness(1, haveDocks);
                //
                int stCount = (int)colony.storage.standartResources[ResourceType.CONCRETE_ID];
                scenarioQuest.ChangeAddInfo(0, stCount.ToString() + " / " + ResourcesCost.DOCK_CONCRETE_COSTVOLUME.ToString());
                if (stCount >= ResourcesCost.DOCK_CONCRETE_COSTVOLUME)
                {
                    var s = localizer.GetText(step, WINDOW_INFO_1);
                    tutorialUI.OpenTextWindow(
                        s[0],
                        s[1] + ' ' + Dock.SMALL_SHIPS_PATH_WIDTH.ToString() + 'x' + Dock.SMALL_SHIPS_PATH_WIDTH.ToString() + ' ' + s[2]);
                    scenarioQuest.SetStepCompleteness(0, true);
                }
                else
                {
                    if (!haveDocks) scenarioQuest.SetStepCompleteness(0, false);
                }
            }
            public override bool QuestMustCheckConditions() { return true; }
        }
        sealed class DockObservingSCN : TutorialScenario
        {
            private byte stage = 0;
            public override bool DropAnySelectionWhenWindowOpens() { return false; }

            public DockObservingSCN() : base()
            {
                blockCanvasRaycaster = true;
                step = TutorialStep.DockObserving;
                useQuest = false;
            }
            public override void StartScenario()
            {
                base.StartScenario();
                var obs = Dock.dockObserver;
                obs.PrepareImmigrationPanel();
                tutorialUI.SetShowframe(obs.SYSTEM_GetImmigrationPanel());
            }
            public override void OKButton()
            {
                stage++;
                if (stage == 1)
                {
                    Dock.dockObserver.PrepareTradingPanel();
                    var s = localizer.GetText(step, WINDOW_INFO_1);
                    tutorialUI.OpenTextWindow(s[0], s[1]);
                }
                else EndScenario();
            }
        }
        sealed class UpgradeHQSCN : TutorialScenario
        {
            private byte stage = 0;
            public UpgradeHQSCN() : base()
            {
                step = TutorialStep.UpgradeHQ;
            }
            public override void StartScenario()
            {
                base.StartScenario();
                GameMaster.realMaster.eventTracker.buildingUpgradeEvent += this.BuildingUpgrade;
            }
            private void BuildingUpgrade(Building b)
            {
                if (b.ID == Structure.HEADQUARTERS_ID)
                {
                    stage++;
                    GameMaster.realMaster.eventTracker.buildingUpgradeEvent -= this.BuildingUpgrade;
                    tutorialUI.OpenTextWindow(localizer.GetText(step, WINDOW_INFO_1));
                }
            }
            public override void OKButton()
            {
                if (stage == 1) EndScenario();
            }
        }
        #endregion

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
                        length = s.Length;
                        if (length == 0 || s[0] == '-') continue;
                        else
                        {                            
                            if (s[0] == '[' && s[indexLength + 1] == ']')
                            {
                                index = int.Parse(s.Substring(1, indexLength));
                                if (lines[index] != null) UnityEngine.Debug.Log("string " + index.ToString() + " was rewrited");
                                lines[index] = s.Substring(indexLength + 2, length - indexLength - 2);
                                i++;
                            }
                            else Debug.Log("error in line " + i.ToString() + ": " + s[0] + s[indexLength]);
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
                                case WINDOW_INFO_0: return new string[2] { lines[20], lines[21] };
                                case QUEST_INFO_0: return new string[3] { lines[20], lines[22], lines[23] };
                            }
                            break;
                        }
                    case TutorialStep.ColonyInterfaceExplaining:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[24], lines[25] };
                                case WINDOW_INFO_1: return new string[2] { lines[24], lines[26] };
                            }
                            break;
                        }
                    case TutorialStep.BuildWindmill:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[27], lines[28] };
                                case WINDOW_INFO_1: return new string[2] { lines[27], lines[29] };
                                case QUEST_INFO_0: return new string[3] { lines[30], lines[31], lines[32] };
                            }
                            break;
                        }
                    case TutorialStep.GatherLumber:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[33], lines[34] };
                                case QUEST_INFO_0: return new string[3] { lines[35], lines[36], lines[37] };
                            }
                            break;
                        }
                    case TutorialStep.BuildFarm:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[3] { lines[38], lines[39], lines[40] };
                                case QUEST_INFO_0: return new string[5] { lines[38], lines[41], lines[40], lines[42], lines[43] };
                            }
                            break;
                        }
                    case TutorialStep.StoneDigging:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[44], lines[45],  };
                                case QUEST_INFO_0: return new string[3] { lines[46], lines[47], lines[48] };
                            }
                            break;
                        }
                    case TutorialStep.StorageLook:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[49], lines[50], };
                                case WINDOW_INFO_1: return new string[2] { lines[51], lines[52] };
                            }
                            break;
                        }
                    case TutorialStep.SmelteryBuilding:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[53], lines[54], };
                                case QUEST_INFO_0: return new string[6] { lines[53], lines[55], lines[56], lines[57], lines[58], lines[59] };
                                case WINDOW_INFO_1: return new string[2] { lines[60], lines[61], };
                                case WINDOW_INFO_2: return new string[2] { lines[62], lines[63], };
                                case SPECIAL_0: return new string[2] { lines[87], lines[88] };
                                case WINDOW_INFO_3: return new string[2] { lines[90], lines[91] };
                            }
                            break;
                        }
                    case TutorialStep.BuildDock:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[3] { lines[64], lines[65], lines[66] };
                                case QUEST_INFO_0: return new string[5] { lines[64], lines[67], lines[68], lines[69], lines[70] };
                                case WINDOW_INFO_1: return new string[3] { lines[71], lines[72], lines[73] };
                                case SPECIAL_0: return new string[2] { lines[74], lines[75] };
                            }
                            break;
                        }
                    case TutorialStep.DockObserving:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[76], lines[77] };
                                case WINDOW_INFO_1: return new string[2] { lines[78], lines[79] };
                            }
                            break;
                        }
                    case TutorialStep.UpgradeHQ:
                        {
                            switch (subIndex)
                            {
                                case WINDOW_INFO_0: return new string[2] { lines[080], lines[081] };
                                case WINDOW_INFO_1: return new string[2] { lines[085], lines[086] };
                                case QUEST_INFO_0: return new string[3] { lines[082], lines[083], lines[084] };
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


