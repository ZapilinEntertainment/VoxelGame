using System.Collections.Generic;
public sealed class PsychokineticGenerator : WorkBuilding
{
    public const float ENERGY_MULTIPLIER = 1f, HAPPINESS_MODIFIER = -0.05f;
    private int hmodifier_id = -1;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (hmodifier_id == -1) hmodifier_id = colony.AddHappinessModifier(HAPPINESS_MODIFIER);
    }

    private void RecalculateSurplus()
    {
        energySurplus = workersCount * ENERGY_MULTIPLIER;
        colony.powerGridRecalculationNeeded = true;
    }
    override public int AddWorkers(int x)
    {
        var w = base.AddWorkers(x);
        RecalculateSurplus();
        return w;
    }
    override public int FreeWorkers(int x)
    {
        var n = base.FreeWorkers(x);
        RecalculateSurplus();
        return n;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (hmodifier_id != -1) colony.RemoveHappinessModifier(hmodifier_id);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SaveStructureData();
        data.AddRange(SaveBuildingData());
        data.AddRange(System.BitConverter.GetBytes(workersCount));
        return data;
    }
    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        workersCount = System.BitConverter.ToInt32(data, 0);
        RecalculateSurplus();
    }
    #endregion
}
