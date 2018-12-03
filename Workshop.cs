using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkshopMode : byte {NoActivity, GearsUpgrade}

[System.Serializable]
public class RollingShopSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public WorkshopMode mode;
}

public sealed class Workshop : WorkBuilding {	
    public static Workshop current;

    public WorkshopMode mode { get; private set; }
    private const float GEARS_UPGRADE_SPEED = 0.1f;

	override public void Prepare() {
		PrepareWorkbuilding();
		mode = WorkshopMode.NoActivity;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (current != null & current != this) current.Annihilate(false);
        current = this;
        colony = GameMaster.colonyController;
	}

    override public void LabourUpdate()
    {
        if (isActive & energySupplied)
        {
            if (colony.gears_coefficient < GameConstants.GEARS_UP_LIMIT)
            {
                colony.gears_coefficient += workSpeed; 
            }
        }
    }

    override protected void LabourResult() {
	}
    public void SetMode(byte x)
    {
       mode = (WorkshopMode)x;
    }
    public void SetMode (WorkshopMode rsm) { mode = rsm; }

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
