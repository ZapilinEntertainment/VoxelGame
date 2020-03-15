public class StabilityEnforcer : WorkBuilding
{
    private int affectionID = -1;
    private const float AFFECTION_VALUE = 0.05f;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (affectionID == -1) affectionID = GameMaster.realMaster.AddStabilityModifier(AFFECTION_VALUE);
    }


    override public void LabourUpdate()
    {
        if (!isActive | !isEnergySupplied) return;

        float stabilityDelta = 0f;

        if (workersCount > 0)
        {
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
    }
}
