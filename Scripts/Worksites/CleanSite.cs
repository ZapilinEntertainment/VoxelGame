using UnityEngine;
using System.Collections.Generic;

public class CleanSite : Worksite {
	public bool diggingMission {get;protected set;}
	const int START_WORKERS_COUNT = 10;
    private const float DAMAGE_PER_TICK = 10f;
    // public const int MAX_WORKERS = 32

    public CleanSite(Plane p, bool f_diggingMission) : base (p)
    {        
        if (sign == null) sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/ClearSign")).GetComponent<WorksiteSign>();
        sign.worksite = this;
        sign.transform.position = workplace.GetCenterPosition() + workplace.GetLookVector() *Block.QUAD_SIZE * 0.5f; ;
        //FollowingCamera.main.cameraChangedEvent += SignCameraUpdate;

        diggingMission = f_diggingMission;
        if (workersCount < START_WORKERS_COUNT) colony.SendWorkers(START_WORKERS_COUNT, this);
        gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.25f;
        workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.Clearing);
    }

    override public void LabourUpdate () {
		if (workplace == null) {
            StopWork(true);
			return;
		}
        if (workplace.structuresCount == 0)
        {
            INLINE_FinishWorkSequence();
            return;
        }
        else INLINE_WorkCalculation();		
	}
    protected override void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        while (iterations > 0)
        {
            iterations--;
            Structure s = workplace.GetRandomStructure();
            if (s == null)
            {
                INLINE_FinishWorkSequence();
                return;
            }
            else
            {
                if (s.ID == Structure.PLANT_ID)
                {
                    (s as Plant).Harvest(false);
                }
                else
                {
                    HarvestableResource hr = s as HarvestableResource;
                    if (hr != null)
                    {
                        hr.Harvest();
                    }
                    else
                    {
                        s.ApplyDamage(DAMAGE_PER_TICK);
                    }
                }
            }
        }
        actionLabel = Localization.GetActionLabel(LocalizationActionLabels.CleanInProgress) + " (" + Localization.GetPhrase(LocalizedPhrase.ObjectsLeft) + " :" + workplace.structuresCount.ToString() + ")";
    }
    private void INLINE_FinishWorkSequence()
    {
        if (diggingMission)
        {
            colony.RemoveWorksite(this);
            DigSite ds = new DigSite(workplace, true, 0);
            TransferWorkers(this, ds);
            if (showOnGUI) { ds.ShowOnGUI(); showOnGUI = false; }
            StopWork(true);
        }
        else StopWork(true);
    }

    #region save-load mission
    override public void Save(System.IO.Stream fs) {
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

    public static CleanSite Load(System.IO.Stream fs, Chunk chunk )
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
