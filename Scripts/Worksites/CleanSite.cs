using UnityEngine;
using System.Collections.Generic;

public class CleanSite : Worksite {
	public bool diggingMission {get;protected set;}
	const int START_WORKERS_COUNT = 10;
    // public const int MAX_WORKERS = 32

    public CleanSite(Plane p, bool f_diggingMission) : base (p)
    {        
        if (sign == null) sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/ClearSign")).GetComponent<WorksiteSign>();
        sign.worksite = this;
        sign.transform.position = workplace.GetCenterPosition() + workplace.GetLookVector() *Block.QUAD_SIZE * 0.5f; ;
        //FollowingCamera.main.cameraChangedEvent += SignCameraUpdate;

        diggingMission = f_diggingMission;
        if (workersCount < START_WORKERS_COUNT) colony.SendWorkers(START_WORKERS_COUNT, this);
    }

    override public void WorkUpdate () {
		if (workplace == null) {
            StopWork(true);
			return;
		}
        var strlist = workplace.GetStructuresList();
		if (strlist == null) {
            if (diggingMission)
            {
                colony.ReplaceWorksiteFromList(workplace, this, false);
                DigSite ds = new DigSite(workplace, true, 0);
                TransferWorkers(this, ds);
                if (showOnGUI) { ds.ShowOnGUI(); showOnGUI = false; }
            }
            StopWork(true);
            return;
		}		
        else
        {
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage;
            Structure s = strlist[0];
            float workGained = 0;
            if (s.ID == Structure.PLANT_ID)
            {
                workGained = s.hp;
                (s as Plant).Harvest(false);                
            }
            else
            {
                HarvestableResource hr = s as HarvestableResource;
                if (hr != null)
                {
                    workGained = hr.resourceCount;
                    hr.Harvest();
                }
                else
                {
                    s.ApplyDamage(workflow);
                    workGained = workflow;
                }
            }
            workflow -= workGained;
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.CleanInProgress) + " (" +Localization.GetPhrase(LocalizedPhrase.ObjectsLeft) +" :" + strlist.Count.ToString()  +  ")";
        }		
	}

	protected override void RecalculateWorkspeed() {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.CLEARING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
	}

    #region save-load mission
    override public void Save(System.IO.FileStream fs) {
        if (workplace == null)
        {
            StopWork(true);
            return;
        }
        else
        {
            var pos = workplace.pos;
            fs.WriteByte((byte)WorksiteType.CleanSite);
            fs.WriteByte(pos.x);
            fs.WriteByte(pos.y);
            fs.WriteByte(pos.z);
            fs.WriteByte(workplace.faceIndex);
            fs.WriteByte(diggingMission ? (byte)1 : (byte)0);
            SerializeWorksite(fs);
        }
	}

    public static CleanSite Load(System.IO.FileStream fs, Chunk chunk )
    {
        var data = new byte[5];
        fs.Read(data, 0, data.Length);
        Plane plane = null;
        if (chunk.GetBlock(data[0], data[1], data[2])?.TryGetPlane(data[3], out plane) == true)
        {
            var cs = new CleanSite(plane, data[4] == 1);
            cs.LoadWorksiteData(fs);
            return cs;
        }
        else
        {
            Debug.Log("clean site load error");
            return null;
        }
    }
	#endregion
			
}
