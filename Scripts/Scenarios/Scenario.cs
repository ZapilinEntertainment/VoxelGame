using System.Collections.Generic;

public abstract class Scenario : MyObject
{
    public readonly int ID; // Tutorial - 0, FD Route - 1
    protected byte _scenarioIndex;
    protected bool completed = false, useSpecialQuestFilling = false, useSpecialWindowFilling = false;
    public const int FOUNDATION_ROUTE_ID = 1;
    //
    private static (int id, string filename)[] scenarioFiles;
    //
    #region save-load
    public virtual void Save(System.IO.FileStream fs)
    {

    }
    public static Scenario StaticLoad(System.IO.FileStream fs)
    {
        return null;
    }
    virtual public void Load(System.IO.FileStream fs)
    {

    }
    #endregion


    protected override bool IsEqualNoCheck(object obj)
    {
        return _scenarioIndex == (obj as Scenario)._scenarioIndex;
    }
    protected Scenario(int i_id)
    {
        ID = i_id;
    }

    virtual public void StartScenario() { }
    virtual public void EndScenario() { }
    
    virtual public void OKButton() { }
    virtual public void UIConditionProceedButton() { }

    virtual public void SpecialQuestFilling() { } // for combined strings
    virtual public void SpecialWindowFilling() { }
    //quest:
    virtual public void CheckConditions() { }
    virtual public bool QuestMustCheckConditions() { return true; }
    virtual public byte GetStepsCount() { return 1; }
    virtual public QuestSection DefineQuestSection() { return QuestSection.Endgame; }

    // SCENARIO REPRESENTATION

    public static int GetAllScenariosCount()
    {
        if (scenarioFiles == null) RefreshScenariosList();
        return scenarioFiles.Length;
    }
    private static void RefreshScenariosList()
    {
        scenarioFiles = new (int, string)[1] { (0, Localization.GetWord(LocalizedWord.Tutorial)) };
        // check directory and load names into scenario files
    }
    public static ScenarioRepresentator GetRepresentator(int id) {
        if (id == 0) return ScenarioRepresentator.GetTutorialRepresentator();
        else return null;
    }
    public static ScenarioRepresentator[] GetRepresentators(int startPosition, int length)
    {        
        var list = new List<ScenarioRepresentator>();
        int i = startPosition, end = startPosition + length;
        while (i < scenarioFiles.Length && i < end)
        {
            list.Add(GetRepresentator(scenarioFiles[i].id));
            i++;
        }
        return list.ToArray();
    }
    public static ScenarioRepresentator[] GetRepresentators()
    {
        int count = scenarioFiles.Length;
        var srrs = new ScenarioRepresentator[count];
        for (int i = 0; i < count; i++) { srrs[i] = GetRepresentator(scenarioFiles[i].id); }
        return srrs;
    }   

    public sealed class ScenarioRepresentator
    {
        public readonly int scenarioID;
        public readonly string name, description;

        public static ScenarioRepresentator GetTutorialRepresentator()
        {
            return new ScenarioRepresentator(0, "Tutorial", "Tutorial description");
        }

        private ScenarioRepresentator(int i_ID, string i_name, string i_descr)
        {
            scenarioID = i_ID;
            name = i_name;
            description = i_descr;
        }

        public GameStartSettings GetStartSettings()
        {
            //if (scenarioID == 0)
            return GameStartSettings.GetTutorialSettings();

        }
    }

    
}


