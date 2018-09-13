using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RollingShopMode {NoActivity, GearsUpgrade}

[System.Serializable]
public class RollingShopSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public RollingShopMode mode;
}

public class RollingShop : WorkBuilding {
	RollingShopMode mode;
    public static RollingShop current;
	const float GEARS_UP_LIMIT = 3, GEARS_UPGRADE_STEP = 0.1f;

	override public void Prepare() {
		PrepareWorkbuilding();
		mode = RollingShopMode.NoActivity;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (current != null & current != this) current.Annihilate(false);
        current = this;
	}

    override public void LabourUpdate()
    {
        if (isActive & energySupplied)
        {
            workflow += workSpeed;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
    }

    override protected void LabourResult() {
        int steps = (int)(workflow / workflowToProcess);
        if (steps == 0) return;
		switch (mode) {
		case RollingShopMode.GearsUpgrade:
                float total = GEARS_UPGRADE_STEP * steps;
                float ck = GameMaster.colonyController.gears_coefficient;
                if (ck < GEARS_UP_LIMIT)
                {
                    if (ck + total > GEARS_UP_LIMIT) total = GEARS_UP_LIMIT - ck;
                    GameMaster.colonyController.ImproveGearsCoefficient(total);
                }
			break;
		}
        workflow -= workflowToProcess;
	}

    public int GetModeIndex()
    {
        return (int)mode;
    }
    public void SetMode(int x)
    {
        if (RollingShopMode.IsDefined(typeof(RollingShopMode), x)) mode = (RollingShopMode)x;
    }
    public void SetMode (RollingShopMode rsm) { mode = rsm; }

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetRollingShopSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}
	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		RollingShopSerializer rss = new RollingShopSerializer();
		GameMaster.DeserializeByteArray<RollingShopSerializer>(ss.specificData, ref rss);
		LoadWorkBuildingData(rss.workBuildingSerializer);
		mode = rss.mode;
	}

	protected RollingShopSerializer GetRollingShopSerializer() {
		RollingShopSerializer rss = new RollingShopSerializer();
		rss.workBuildingSerializer = GetWorkBuildingSerializer();
		rss.mode = mode;
		return rss;
	}
	#endregion

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(forced);
        if (current == this) current = null;
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        UIController.current.ActivateRollingShopPanel();
        return workbuildingObserver;
    }
}
