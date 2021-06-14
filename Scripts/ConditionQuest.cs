using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ConditionQuest : Quest
{    
    private bool completeQuestWhenPossible = true, subscribedToQuestUpdate = false;
    private System.Action uiRepresentationFunction;
    private QuestIcon iconType;
    private readonly SimpleCondition[] conditions;
    private readonly ColonyController colony;    

    public ConditionQuest(SimpleCondition[] i_conditions, ColonyController i_colony, bool i_completeQuestWhenPossible, QuestIcon cqi) : base(QuestType.Condition, (byte)cqi)
    {
        colony = i_colony;
        needToCheckConditions = true;
        completeQuestWhenPossible = i_completeQuestWhenPossible;
        byte count = (byte)i_conditions.Length;
        INLINE_PrepareSteps(count);
        conditions = i_conditions;
        for (var i = 0; i < count;i++)
        {
            INLINE_DefineStep(ref conditions[i], i);
        }
        iconType = cqi;
        CheckQuestConditions();
    }
    public ConditionQuest(SimpleCondition i_condition, ColonyController i_colony, bool i_completeQuestWhenPossible, QuestIcon cqi) : base(QuestType.Condition, (byte)cqi)
    {
        colony = i_colony;
        needToCheckConditions = true;
        completeQuestWhenPossible = i_completeQuestWhenPossible;
        iconType = cqi;
        INLINE_PrepareSteps(1);
        conditions = new SimpleCondition[1] { i_condition};
        INLINE_DefineStep(ref i_condition, 0);
        CheckQuestConditions();
    }
    private void INLINE_DefineStep(ref SimpleCondition sc, int i)
    {
        switch (sc.type)
        {
            case ConditionType.ResourceCountCheck: steps[i] = Localization.GetResourceName(sc.index); break;
            case ConditionType.MoneyCheck: steps[i] = Localization.GetPhrase(LocalizedPhrase.CrystalsCollected); break;
            case ConditionType.GearsCheck: steps[i] = Localization.GetWord(LocalizedWord.GearsLevel); break;
            case ConditionType.FreeWorkersCheck: steps[i] = Localization.GetWord(LocalizedWord.FreeWorkers); break;
            case ConditionType.StoredEnergyCondition: steps[i] = Localization.GetPhrase(LocalizedPhrase.EnergyStored); break;
            case ConditionType.CrewsCondition: steps[i] = Localization.ComposeCrewLevel((byte)sc.value) + ':'; break;
            case ConditionType.ShuttlesCount: steps[i] = Localization.GetWord(LocalizedWord.Shuttles) + ':'; break;
            case ConditionType.Dummy: steps[i] = string.Empty; break;
        }
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
                case ConditionType.Dummy:
                    {
                        if (sc.index != 2)
                        {
                            stepsFinished[i] =  sc.index == 1;
                        }
                        break;
                    }
            }
            if (stepsFinished[i]) completed++;
        }
        uiRepresentationFunction?.Invoke();
        if (completeQuestWhenPossible && completed == cdCount) MakeQuestCompleted();
    }
    public bool ConsumeAndFinish()
    {
        if (completed) return true;
        CheckQuestConditions();
        bool x = true;
        foreach (var s in stepsFinished) { if (!s) x = false; }
        if (!x) return false;
        else
        {
            foreach (var sc in conditions)
            {
                switch (sc.type)
                {
                    //no gears
                    //no crews
                    // no shuttles
                    case ConditionType.ResourceCountCheck:
                        {
                            colony.storage.GetResources(sc.index, sc.value);
                            break;
                        }
                    case ConditionType.MoneyCheck:
                        {
                            colony.GetEnergyCrystals(sc.value);
                            break;
                        }
                    case ConditionType.FreeWorkersCheck:
                        {
                            colony.ConsumeWorkers(sc.index);
                            break;
                        }
                    case ConditionType.StoredEnergyCondition:
                        {
                            colony.TryGetEnergy(sc.value);
                            break;
                        }
                }
            }
            MakeQuestCompleted();
            StopQuest(true);
            return true;
        }
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
        iconType.GetIconInfo(ref icon, ref rect);
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
    //condition quest check condition & condition quest consume and finish

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
    public static SimpleCondition GetDummyCondition(bool? alwaysTrue)
    {
        int x;
        if (alwaysTrue == null) x = 2;
        else
        {
            if (alwaysTrue == true) x = 1; else x = 0;
        }
        return new SimpleCondition(ConditionType.Dummy, x, 0f);
    }
}
public enum ConditionType:byte { ResourceCountCheck, MoneyCheck, GearsCheck, FreeWorkersCheck, StoredEnergyCondition,
    CrewsCondition, ShuttlesCount, Dummy}
