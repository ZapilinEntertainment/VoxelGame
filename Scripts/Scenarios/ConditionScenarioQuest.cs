using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ConditionScenarioQuest : ScenarioQuest
{
    private readonly SimpleCondition[] conditions;
    private readonly ColonyController colony;
    public ConditionScenarioQuest(Scenario i_scn, SimpleCondition[] i_conditions, ColonyController i_colony) : base(i_scn)
    {
        colony = i_colony;
        needToCheckConditions = true;
        byte count = (byte)i_conditions.Length;
        INLINE_PrepareSteps(count);
        SimpleCondition sc;
        for (var i = 0; i < count;i++)
        {
            sc = conditions[i];
            switch(sc.type)
            {
                case ConditionType.ResourceCountCheck: steps[i] = Localization.GetResourceName(sc.index); break;
                case ConditionType.MoneyCheck: steps[i] = Localization.GetPhrase(LocalizedPhrase.CrystalsCollected);break;
                case ConditionType.GearsCheck: steps[i] = Localization.GetWord(LocalizedWord.GearsLevel); break;
                case ConditionType.FreeWorkersCheck: steps[i] = Localization.GetWord(LocalizedWord.FreeWorkers); break;
            }
        }
        
    }
    public override void CheckQuestConditions()
    {
        int cdCount = conditions.Length;
        SimpleCondition sc;
        for (int i = 0; i < cdCount; i++)
        {
            sc = conditions[i];
            switch (sc.type)
            {
                case ConditionType.ResourceCountCheck:
                    {
                        int count = (int)colony.storage.standartResources[sc.index];
                        stepsAddInfo[i] = count.ToString() + '/' + ((int)sc.value).ToString();
                        stepsFinished[i] = count >= sc.value;                        
                        break;
                    }
                case ConditionType.MoneyCheck:
                    {
                        int count = (int)colony.energyCrystalsCount;
                        stepsAddInfo[i] = count.ToString() + '/' + ((int)sc.value).ToString();
                        stepsFinished[i] = count >= sc.value;
                        break;
                    }
                case ConditionType.GearsCheck:
                    {
                        float gl = colony.gears_coefficient;
                        stepsAddInfo[i] = string.Format("{0:0.###}", gl) + '/' + string.Format("{0:0.###}", sc.value);
                        stepsFinished[i] = gl >= sc.value;
                        break;
                    }
                case ConditionType.FreeWorkersCheck:
                    {
                        int count = colony.freeWorkers;
                        stepsAddInfo[i] = count.ToString() + '/' + sc.index;
                        stepsFinished[i] = count >= sc.value;
                        break;
                    }
            }
        }
    }
}
public struct SimpleCondition
{
    public ConditionType type;
    public int index;
    public float value;

    private SimpleCondition(ConditionType i_type, int i_index, float i_val)
    {
        type = i_type;
        index = i_index;
        value = i_val;
    }

    public static SimpleCondition GetResourceCondition(ResourceType rtype, float volume)
    {
        return new SimpleCondition(ConditionType.ResourceCountCheck, rtype.ID, volume);
    }
    public static SimpleCondition GetMoneyCondition(float volume)
    {
        return new SimpleCondition(ConditionType.MoneyCheck, 0, volume);
    }
    public static SimpleCondition GetGearsCondition(float val)
    {
        return new SimpleCondition(ConditionType.GearsCheck,0, val);
    }
    public static SimpleCondition GetFreeWorkersCondition(int count)
    {
        return new SimpleCondition(ConditionType.FreeWorkersCheck, count, 0f);
    }
}
public enum ConditionType:byte { ResourceCountCheck, MoneyCheck, GearsCheck, FreeWorkersCheck}
