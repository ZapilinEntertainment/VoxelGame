using System.Collections.Generic;

public sealed class Workshop : WorkBuilding {	

    private const float GEARS_UPGRADE_PER_TICK = 0.01f;

	override public void Prepare() {
		PrepareWorkbuilding();
	}


    override protected void LabourResult(int iterations) {
        if (iterations < 1) return;
        workflow -= iterations;
        float val = iterations * GEARS_UPGRADE_PER_TICK, maxVal = GameConstants.GetMaxGearsCf(colony);
        if (colony.gears_coefficient + val < maxVal)
        {
            colony.gears_coefficient += val;
        }
        else colony.gears_coefficient = maxVal;
    }

    public override bool ShowUIInfo()
    {
        return true;
    }
}
