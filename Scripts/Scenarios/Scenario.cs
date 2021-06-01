using System.Collections.Generic;

public abstract class Scenario : MyObject
{
    public readonly int ID; // Tutorial - 0, FD Route - 1
    protected byte _scenarioIndex;
    public bool completed { get; protected set; }
        protected bool useSpecialQuestFilling = false, useSpecialWindowFilling = false;
    public const int TUTORIAL_SCENARIO_ID = 0,FOUNDATION_ROUTE_ID = 1;
    //
    private static (int id, string filename)[] scenarioFiles;
    //
    #region save-load
    public virtual void Save(System.IO.Stream fs)
    {
        fs.Write(System.BitConverter.GetBytes(ID),0,4);
    }
    public static Scenario StaticLoad(System.IO.Stream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        var x = System.BitConverter.ToInt32(data, 0);
        Scenario s = null;
        switch (x)
        {
            case FOUNDATION_ROUTE_ID:
                var fr = new FoundationRouteScenario();
                fr.PrepareUI();
                fr.Load(fs);
                s = fr;
                
                
                break;
        }
        return s;
    }
    virtual public void Load(System.IO.Stream fs) { }
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
    virtual public void ClearScenarioDecorations() { }
    
    virtual public void OKButton() { }
    virtual public void UIConditionProceedButton() { }
    virtual public void ScenarioObjectUIDisabled() { }

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
        return TutorialScenarioNS.TutorialScenario.GetRepresentator();
        //switch (id)
       // {
        //    case TUTORIAL_SCENARIO_ID:    
       // }
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

        public ScenarioRepresentator(int i_ID, string i_name, string i_descr)
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


