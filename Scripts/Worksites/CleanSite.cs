using UnityEngine;
using System.Collections.Generic;

public class CleanSite : Worksite {
	public bool diggingMission {get;protected set;}
	const int START_WORKERS_COUNT = 10;
    // public const int MAX_WORKERS = 32

    public CleanSite(Plane p, byte i_faceIndex, bool f_diggingMission) : base (p, i_faceIndex)
    {        
        if (sign == null) sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/ClearSign")).GetComponent<WorksiteSign>();
        sign.worksite = this;
        sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * 0.5f * Block.QUAD_SIZE;
        //FollowingCamera.main.cameraChangedEvent += SignCameraUpdate;

        diggingMission = f_diggingMission;
        if (workersCount < START_WORKERS_COUNT) colony.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
        subscribedToUpdate = true;
    }

    override public void WorkUpdate () {
		if (workplace == null) {
            StopWork();
			return;
		}
        var strlist = workplace.GetStructuresList();
		if (strlist == null) {
            if (diggingMission)
            {
                workplace.RemoveWorksiteLink(this);
                DigSite ds = new DigSite();
                TransferWorkers(this, ds);
                ds.Set(workplace, true);
                if (showOnGUI) { ds.ShowOnGUI(); showOnGUI = false; }
            }
            StopWork();
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

	

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            colony.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null)
        {
          //  FollowingCamera.main.cameraChangedEvent -= SignCameraUpdate;
            MonoBehaviour.Destroy(sign.gameObject);
        }        
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (workplace != null)
        {
            workplace.RemoveWorksiteLink(this);
            workplace = null;
        }
        if (showOnGUI)
        {
            if (observer.observingWorksite == this)
            {
                observer.SelfShutOff();
                UIController.current.ChangeChosenObject(ChosenObjectType.Surface);
            }
            showOnGUI = false;
        }
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
    }

    #region save-load mission
    override protected List<byte> Save() {
		if (workplace == null) {
            StopWork();
			return null;
		}
        var pos = workplace.pos;
        var data = new List<byte>() { (byte)WorksiteType.CleanSite, pos.x, pos.y, pos.z, faceIndex,
            diggingMission ? (byte)1 : (byte)0
        };
        data.AddRange(SerializeWorksite());
		return data;
	}

    public static Worksite Load(System.IO.FileStream fs, Chunk chunk )
    {
        var data = new byte[5];
        fs.Read(data, 0, data.Length);
        Plane plane = null;
        if (chunk.GetBlock(data[0], data[1], data[2])?.TryGetPlane(data[3], out plane) == true)
        {
            var cs = new CleanSite(plane, data[3], data[4] == 1);
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
