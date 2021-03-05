public sealed class ScenarioQuest : Quest
{
    private Scenario myScenario;

    public ScenarioQuest(Scenario i_scenario) : base(QuestType.Scenario, 0)
    {
        myScenario = i_scenario;
    }

    public override void CheckQuestConditions()
    {
        myScenario.CheckConditions();
    }
    public void ChangeAddInfo(int index, string s)
    {
        stepsAddInfo[index] = s;
    }
}
