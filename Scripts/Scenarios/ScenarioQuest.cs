public sealed class ScenarioQuest : Quest
{
    private Scenario myScenario;

    public ScenarioQuest(Scenario i_scenario) : base(QuestType.Scenario, 0)
    {
        myScenario = i_scenario;
    }
}
