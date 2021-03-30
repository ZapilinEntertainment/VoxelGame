using UnityEngine;
public class ScenarioQuest : Quest
{
    protected Scenario myScenario;

    public ScenarioQuest(Scenario i_scenario) : base(QuestType.Scenario, 0)
    {
        myScenario = i_scenario;
        needToCheckConditions = myScenario.QuestMustCheckConditions();
        INLINE_PrepareSteps(myScenario?.GetStepsCount() ?? 1);
    }

    public override void CheckQuestConditions()
    {
        myScenario.CheckConditions();
    }
    override public void MakeQuestCompleted()
    {
        if (completed) return;
        QuestUI.current.ResetQuestCell(this);
        completed = true;
        if (subscribedToStructuresCheck) GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.StructureCheck;
    }

    public void ChangeAddInfo(int index, string s)
    {
        stepsAddInfo[index] = s;
    }
    public QuestSection DefineQuestSection()
    {
        return QuestSection.Endgame;
    }

    public void GetIconInfo(ref Texture icon, ref Rect rect)
    {
        //scenario get info
        icon = GlobalMapCanvasController.GetMapMarkersTexture();
        rect = GlobalMapCanvasController.GetMarkerRect(MapPointType.QuestMark);
    }
}
