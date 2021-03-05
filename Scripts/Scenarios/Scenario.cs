public abstract class Scenario : MyObject
{
    protected byte _scenarioIndex;
    protected ScenarioQuest scenarioQuest;
    protected bool completed = false, useSpecialQuestFilling = false;

    protected override bool IsEqualNoCheck(object obj)
    {
        return _scenarioIndex == (obj as Scenario)._scenarioIndex;
    }

    virtual public void StartScenario() { }
    virtual public void EndScenario() { }
    virtual public void CheckConditions() { }
    virtual public void SpecialQuestFilling() { } // for combined strings
}
