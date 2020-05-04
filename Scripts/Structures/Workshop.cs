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
            workSpeed = colony.workspeed * workersCount * GameConstants.FACTORY_SPEED;
            if (colony.gears_coefficient < GameConstants.GEARS_UP_LIMIT)
            {
                colony.gears_coefficient += workSpeed * GEARS_UPGRADE_SPEED;
            }
        }
    }

    override protected void LabourResult() {
	}

    public override bool ShowWorkspeed()
    {
        return true;
    }
}
