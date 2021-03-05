public abstract class Scenario : MyObject
{
    protected byte _scenarioIndex;
    protected Quest scenarioQuest;
    protected bool completed = false;

    protected override bool IsEqualNoCheck(object obj)
    {
        return _scenarioIndex == (obj as Scenario)._scenarioIndex;
    }

    virtual public void StartScenario() { }
    virtual public void EndScenario() { }
    virtual public void FillQuestData(Quest q) { }
}
