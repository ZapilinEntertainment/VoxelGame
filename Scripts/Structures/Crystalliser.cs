public class Crystalliser : WorkBuilding
{
    private const float CRYSTALS_PER_WORKER = 1024;
    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        if (workersCount > 0)
        {
            if (workersCount < iterations)
            {
                workersCount = 0;
                iterations = workersCount;
            }
            else workersCount -= iterations;
            colony.AddEnergyCrystals(CRYSTALS_PER_WORKER * iterations);
            if (workersCount == 0) SetActivationStatus(false, true);
        }
    }
}
