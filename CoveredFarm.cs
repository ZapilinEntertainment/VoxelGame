
public class CoveredFarm : WorkBuilding {
	float output_value = 1;
	ResourceType outputResource;
	const float MIN_SPEED = 0.2f;
	Storage s;

	override public void Prepare()  {
        PrepareWorkbuilding();
        switch (id)
        {
            case FARM_4_ID:
            case FARM_5_ID:
                outputResource = ResourceType.Food;
                break;
            case LUMBERMILL_4_ID:
            case LUMBERMILL_5_ID:
                outputResource = ResourceType.Lumber;
                break;
        }		
		s = colony.storage;
	}

	override protected void LabourResult() {
        int iterations = (int)(workflow / workflowToProcess);
		s.AddResource( new ResourceContainer (outputResource, output_value * iterations) );
		workflow -= iterations * workflowToProcess;
	}

	override public void RecalculateWorkspeed() {
		workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Farming);
		if (workSpeed < MIN_SPEED && workersCount > 0) workSpeed = MIN_SPEED;
        gearsDamage = workSpeed * GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT;
	}
}
