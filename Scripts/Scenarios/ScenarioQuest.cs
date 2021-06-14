using UnityEngine;
using System.Collections.Generic;


public class ScenarioQuest : Quest
{
    protected Scenario myScenario;
    protected QuestIcon iconType;

    public ScenarioQuest(Scenario i_scenario, QuestIcon qi) : base(QuestType.Scenario, 0)
    {
        myScenario = i_scenario;
        iconType = qi;
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
        iconType.GetIconInfo(ref icon, ref rect);
    }

}
