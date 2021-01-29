
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

    override protected void LabourResult(int iterations) {
        if (iterations < 1) return;
        workflow -= iterations;
		s.AddResource( new ResourceContainer (outputResource, output_value * iterations) );
	}
}
