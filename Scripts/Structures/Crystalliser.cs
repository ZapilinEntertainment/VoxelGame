public class Crystalliser : WorkBuilding
{
    private const float CRYSTALS_PER_WORKER = 1024;
    override protected void LabourResult()
    {
        if (workersCount > 0)
        {
            workersCount--;
            colony.AddEnergyCrystals(CRYSTALS_PER_WORKER);
            if (workersCount == 0) SetActivationStatus(false, true);
        }
    }
}
