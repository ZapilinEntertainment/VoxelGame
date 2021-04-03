using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ConditionQuest : Quest
{
    public enum ConditionQuestIcon : byte { FoundationRouteIcon}

    private bool completeQuestWhenPossible = true, subscribedToQuestUpdate = false;
    private System.Action uiRepresentationFunction;
    private readonly SimpleCondition[] conditions;
    private readonly ColonyController colony;    

    public ConditionQuest(SimpleCondition[] i_conditions, ColonyController i_colony, bool i_completeQuestWhenPossible, ConditionQuestIcon cqi) : base(QuestType.Condition, (byte)cqi)
    {
        colony = i_colony;
        needToCheckConditions = true;
        completeQuestWhenPossible = i_completeQuestWhenPossible;
        byte count = (byte)i_conditions.Length;
        INLINE_PrepareSteps(count);
        conditions = i_conditions;
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
                case ConditionType.StoredEnergyCondition: steps[i] = Localization.GetPhrase(LocalizedPhrase.EnergyStored); break;
                case ConditionType.CrewsCondition: steps[i] = Localization.ComposeCrewLevel((byte)sc.value) + ':';break;
                case ConditionType.ShuttlesCount: steps[i] = Localization.GetWord(LocalizedWord.Shuttles) + ':'; break;
            }
        }
        CheckQuestConditions();
    }
    public ConditionQuest(SimpleCondition i_condition, ColonyController i_colony, bool i_completeQuestWhenPossible, ConditionQuestIcon cqi) : base(QuestType.Condition, (byte)cqi)
    {
        colony = i_colony;
        needToCheckConditions = true;
        completeQuestWhenPossible = i_completeQuestWhenPossible;
        INLINE_PrepareSteps(1);
        conditions = new SimpleCondition[1] { i_condition};
        switch (i_condition.type)
        {
            case ConditionType.ResourceCountCheck: steps[0] = Localization.GetResourceName(i_condition.index); break;
            case ConditionType.MoneyCheck: steps[0] = Localization.GetPhrase(LocalizedPhrase.CrystalsCollected); break;
            case ConditionType.GearsCheck: steps[0] = Localization.GetWord(LocalizedWord.GearsLevel); break;
            case ConditionType.FreeWorkersCheck: steps[0] = Localization.GetWord(LocalizedWord.FreeWorkers); break;
            case ConditionType.StoredEnergyCondition: steps[0] = Localization.GetPhrase(LocalizedPhrase.EnergyStored); break;
            case ConditionType.CrewsCondition: steps[0] = Localization.ComposeCrewLevel((byte)i_condition.value) + ':'; break;
            case ConditionType.ShuttlesCount: steps[0] = Localization.GetWord(LocalizedWord.Shuttles) + ':'; break;
        }
        CheckQuestConditions();
    }
    public override void CheckQuestConditions()
    {
        int cdCount = conditions.Length, completed = 0;
        SimpleCondition sc;
        for (int i = 0; i < cdCount; i++)
        {
            sc = conditions[i];
            switch (sc.type)
            {
                case ConditionType.ResourceCountCheck:
                    {
                        int count = (int)colony.storage.GetResourceCount(sc.index);
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
                        stepsAddInfo[i] = count.ToString() + '/' + sc.index.ToString();
                        stepsFinished[i] = count >= sc.index;
                        break;
                    }
                case ConditionType.StoredEnergyCondition:
                    {
                        int f = (int)colony.energyStored;
                        stepsAddInfo[i] = f.ToString() + '/' + ((int)sc.value).ToString();
                        stepsFinished[i] = f >= sc.value;
                        break;
                    }
                case ConditionType.CrewsCondition:
                    {
                        int count = Crew.crewsList?.Count ?? 0, suitableCount = 0;
                        byte lvl = (byte)sc.value;
                        if (count > 0)
                        {
                            foreach (var c in Crew.crewsList)
                            {
                                if (c.level >= lvl) suitableCount++;
                            }
                        }
                        stepsAddInfo[i] = suitableCount.ToString() + '/' + sc.index.ToString();
                        stepsFinished[i] = suitableCount >= sc.index;
                        break;
                    }
                case ConditionType.ShuttlesCount:
                    {
                        int count = Hangar.hangarsList?.Count ?? 0, suitableCount = 0;
                        if (count > 0)
                        {
                            foreach (var h in Hangar.hangarsList)
                            {
                                if (h.status == Hangar.HangarStatus.ShuttleInside) suitableCount++;
                            }
                        }
                        stepsAddInfo[i] = suitableCount.ToString() + '/' + sc.index.ToString();
                        stepsFinished[i] = suitableCount >= sc.index;
                        break;
                    }
            }
            if (stepsFinished[i]) completed++;
        }
        uiRepresentationFunction?.Invoke();
        if (completeQuestWhenPossible && completed == cdCount) MakeQuestCompleted();
    }

    public void BindUIUpdateFunction(System.Action f)
    {
        //для  квестов, отслеживающихся через StandartScenarioUI.conditionWindow
        uiRepresentationFunction += f;
    }
    public void SubscribeToUpdate(QuestUI qui)
    {
        //для квестов, выполняющихся вне QuestUI.activeQuests
        if (!subscribedToQuestUpdate)
        {
            qui.questUpdateEvent += this.CheckQuestConditions;
            subscribedToQuestUpdate = true;
        }
    }
    private void Unsubscribe()
    {
        if (subscribedToQuestUpdate)
        {
            UIController.GetCurrent().GetMainCanvasController().questUI.questUpdateEvent -= this.CheckQuestConditions;
            subscribedToQuestUpdate = false;
        }
    }

    public void GetIconInfo(ref Texture icon, ref Rect rect)
    {
        //use subIndex where stores construction info
        // 
        icon = UIController.iconsTexture;
        rect = UIController.GetIconUVRect(Icons.FoundationRoute);
    }

    public override void MakeQuestCompleted()
    {
        if (completed) return;
        base.MakeQuestCompleted();
        Unsubscribe();
        uiRepresentationFunction = null;
        needToCheckConditions = false;
    }
    public override void StopQuest(bool uiRedrawCall)
    {
        if (completed) return;
        base.StopQuest(uiRedrawCall);
        Unsubscribe();
        uiRepresentationFunction = null;
        needToCheckConditions = false;        
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
    public static SimpleCondition GetStoredEnergyCondition(float f)
    {
        return new SimpleCondition(ConditionType.StoredEnergyCondition, 0, f);
    }
    public static SimpleCondition GetCrewsCondition(int count, byte level)
    {
        return new SimpleCondition(ConditionType.CrewsCondition, count, level);
    }
    public static SimpleCondition GetShuttlesCondition(int count)
    {
        return new SimpleCondition(ConditionType.ShuttlesCount, count, 0f);
    }
}
public enum ConditionType:byte { ResourceCountCheck, MoneyCheck, GearsCheck, FreeWorkersCheck, StoredEnergyCondition,
    CrewsCondition, ShuttlesCount}
