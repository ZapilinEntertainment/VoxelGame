
public class CoveredFarm : WorkBuilding {
	float output_value = 1;
	ResourceType outputResource;
	const float MIN_SPEED = 0.2f;
	Storage s;

	override public void Prepare()  {
        PrepareWorkbuilding();
        switch (ID)
        {
            case COVERED_FARM:
            case FARM_BLOCK_ID:
                outputResource = ResourceType.Food;
                break;
            case COVERED_LUMBERMILL:
            case LUMBERMILL_BLOCK_ID:
                outputResource = ResourceType.Lumber;
                break;
        }		
		s = colony.storage;
	}
    override public void LabourUpdate()
    {
        if (!isActive | !isEnergySupplied) return;
        if (workersCount > 0)
        {
            workSpeed = colony.workspeed * workersCount * GameConstants.HYDROPONICS_SPEED;
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage * workSpeed;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
        else workSpeed = 0f;
    }
    override protected void LabourResult() {
        int iterations = (int)(workflow / workflowToProcess);
		s.AddResource( new ResourceContainer (outputResource, output_value * iterations) );
		workflow -= iterations * workflowToProcess;
	}
}
