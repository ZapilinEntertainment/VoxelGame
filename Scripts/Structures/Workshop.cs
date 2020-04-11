using System.Collections.Generic;

public sealed class Workshop : WorkBuilding {	

    private const float GEARS_UPGRADE_SPEED = 0.0001f;

	override public void Prepare() {
		PrepareWorkbuilding();
	}

    override public void LabourUpdate()
    {
        if (isActive & isEnergySupplied)
        {
            if (colony.gears_coefficient < GameConstants.GEARS_UP_LIMIT)
            {
                colony.gears_coefficient += workSpeed * GEARS_UPGRADE_SPEED;
            }
        }
    }

    override protected void LabourResult() {
	}
}
