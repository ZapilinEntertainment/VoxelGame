using UnityEngine;

public sealed class XStation : WorkBuilding {

    public static XStation current { get; private set; }
    private static EnvironmentMaster envMaster;

    // EnvironmentMaster.environmentalConditions
    //GameMaster.lifegrowCoefficient

    public static void ResetXStationStaticData()
    {
        current = null;
        envMaster = GameMaster.realMaster.environmentMaster;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (current != null)
        {
            if (current != null) current.Annihilate(StructureAnnihilationOrder.ManualDestructed);
        }
        else GameMaster.staticResetFunctions += ResetXStationStaticData;
        current = this;
    }


    public override UIObserver ShowOnGUI()
    {
        var wo = base.ShowOnGUI();
        colony.observer.EnableTextfield(ID);
        return wo;
    }
    override public void DisabledOnGUI()
    {
        showOnGUI = false;
        colony.observer.DisableTextfield(ID);
    }

    public static string GetInfo()
    {
        return Localization.GetWord(LocalizedWord.Stability) + ": " + ((int)(GameMaster.stability * 100)).ToString() + "%\n" +
             Localization.GetPhrase(LocalizedPhrase.AscensionLevel) + ": " + ((int)((GameMaster.realMaster.globalMap?.ascension ?? 0f) * 100f)).ToString() + "%";
            ;
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!order.sendMessageToBasement) { basement = null; }
        PrepareWorkbuildingForDestruction(order);
        if (current == this) current = null;
        Destroy(gameObject);
    }
}
