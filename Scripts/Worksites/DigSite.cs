using UnityEngine;
using System.Collections.Generic;

public class DigSite : Worksite {
	public bool dig = true;
	BlockExtension workObject;
    const int START_WORKERS_COUNT = 10, VOLUME_PER_ACTION = 16;


    public DigSite(Plane i_plane, bool work_is_dig) : this(i_plane, work_is_dig, START_WORKERS_COUNT) { }
    public DigSite(Plane i_plane, bool work_is_dig, int startWorkers) : base (i_plane)
    {
        workObject = workplace.GetBlock().GetExtension();
        if (workObject == null)
        {
            StopWork(true);
            return;
        }
        dig = work_is_dig;
        if (i_plane.faceIndex == Block.SURFACE_FACE_INDEX | i_plane.faceIndex == Block.UP_FACE_INDEX)
        {
            if (dig)
            {
                sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/DigSign")).GetComponent<WorksiteSign>();
            }
            else sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/PourInSign")).GetComponent<WorksiteSign>();
            sign.transform.position = workplace.GetCenterPosition() + workplace.GetLookVector() * Block.QUAD_SIZE * 0.5f;
        }
        else
        {
            sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
            sign.transform.position = workplace.GetCenterPosition();
            sign.transform.rotation = Quaternion.Euler(workplace.GetEulerRotationForQuad());
        }
		sign.worksite = this;     

        int wom = workObject.materialID;
        if (workplace.materialID != wom) workplace.ChangeMaterial(wom, true);
        maxWorkersCount = 64;
		if (startWorkers != 0) colony.SendWorkers(startWorkers, this);
        gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT;
        workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.Digging);
    }

    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        bool materialsProblem = true;
        if (dig)
        {
            float production;
            while (iterations > 0)
            {
                iterations--;
                production = workObject.Dig(VOLUME_PER_ACTION, true, workplace.faceIndex);
                if (production == 0f)
                {
                    StopWork(true);
                    return;
                }
            }
        }
        else
        {
            while (iterations > 0)
            {
                iterations--;
                materialsProblem = colony.storage.TryGetResources(ResourceType.GetResourceTypeById(workplace.materialID), VOLUME_PER_ACTION);
                if (!materialsProblem)
                {
                    if (workObject.PourIn(VOLUME_PER_ACTION, workplace.faceIndex)) StopWork(true); return;
                }
                else break;
            }
        }
        if (dig)
        {
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.DigInProgress) + " (" + ((int)((1 - workObject.GetVolumePercent()) * 100f)).ToString() + "%)";
        }
        else
        {
            if (!materialsProblem) actionLabel = Localization.GetActionLabel(LocalizationActionLabels.PouringInProgress) + " (" + ((int)(workObject.GetVolumePercent() * 100f)).ToString() + "%)";
            else actionLabel = Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources);
        }
    }

    #region save-load system
    override public void Save(System.IO.Stream fs) {
        if (workObject == null) {
            StopWork(true);
            return;
        }
        else {
            var pos = workplace.pos;
            fs.WriteByte((byte)WorksiteType.DigSite);
            fs.WriteByte(pos.x);
            fs.WriteByte(pos.y);
            fs.WriteByte(pos.z);
            fs.WriteByte(workplace.faceIndex);
            fs.WriteByte(dig ? (byte)1 : (byte)0);
            SerializeWorksite(fs);
        }
	}
	public static DigSite Load(System.IO.Stream fs, Chunk chunk)
    {
        var data = new byte[5];
        fs.Read(data, 0, data.Length);
        Plane plane = null;
        if (chunk.GetBlock(data[0], data[1], data[2])?.TryGetPlane(data[3], out plane) == true)
        {
            var cs = new DigSite(plane, data[4] == 1);
            cs.LoadWorksiteData(fs);
            return cs;
        }
        else
        {
            Debug.Log("digsite load error");
            return null;
        }
    }
	#endregion
}
