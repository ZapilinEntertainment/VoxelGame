using UnityEngine; // random

[System.Serializable]
public class RecruitingCenterSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public float backupSpeed, progress;
	public bool finding;
}

public sealed class RecruitingCenter : WorkBuilding {
    float backupSpeed = 0.02f;
    public float progress{ get; private set; }
    public bool finding = false;
	ColonyController colonyController;
	const int CREW_SLOTS_FOR_BUILDING = 4, START_CREW_COST = 150;
	static float hireCost = -1;
    public static UIRecruitingCenterObserver rcenterObserver;
    const float FIND_SPEED = 5;

	public static void ResetToDefaults_Static_RecruitingCenter() {
		hireCost = START_CREW_COST + ((int)(GameMaster.difficulty) - 2) * 50;
	}

   public static float GetHireCost()
    {
        if (hireCost == -1) hireCost = START_CREW_COST + ((int)(GameMaster.difficulty) - 2) * 50;
        return hireCost;
    }
    public static void SetHireCost(float f)
    {
        hireCost = f;
    }

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (hireCost == -1) ResetToDefaults_Static_RecruitingCenter();
		bool movement = false;
		if (basement != null) movement = true;
		if (b == null) return;
		SetBuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        colonyController = GameMaster.colonyController;
        if (!movement) // здание не создавалось, а было перенесено
        {
            Crew.AddCrewSlots(CREW_SLOTS_FOR_BUILDING);
            progress = 0;
        }
	}

	override public void LabourUpdate() {
		if ( !isActive | !energySupplied) return;
		if (workersCount > 0) {
			if (finding) {
				float candidatsCountFactor = colonyController.freeWorkers / Crew.OPTIMAL_CANDIDATS_COUNT;
				if (candidatsCountFactor > 1) candidatsCountFactor = 1;
				progress += ( FIND_SPEED * workSpeed * 0.3f + colonyController.happiness_coefficient * 0.3f  + candidatsCountFactor * 0.3f + 0.1f * Random.value )* GameMaster.LABOUR_TICK / workflowToProcess;
				if (progress >= 1) {
					Crew ncrew = new Crew();
					ncrew.SetCrew(colonyController, hireCost);
					Crew.crewsList.Add(ncrew);
					progress = 0;
					finding = false;
                    UIController.current.MakeAnnouncement(Localization.AnnounceCrewReady(ncrew.name));
					hireCost = hireCost * (1 + GameMaster.HIRE_COST_INCREASE);
					hireCost = ((int)(hireCost * 100)) / 100f;
                    if (showOnGUI) rcenterObserver.SelectCrew(ncrew);
				}
			}
		}
		else {
			if (progress > 0) {
				progress -= backupSpeed * GameMaster.LABOUR_TICK;
				if (progress < 0) progress = 0;
			}
		}
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = (float)workersCount / (float)maxWorkers;
	}

    public override UIObserver ShowOnGUI()
    {
        if (rcenterObserver == null) rcenterObserver = UIRecruitingCenterObserver.InitializeRCenterObserverScript();
        else rcenterObserver.gameObject.SetActive(true);
        rcenterObserver.SetObservingRCenter(this);
        showOnGUI = true;
        return rcenterObserver;
    }

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetRecruitingCenterSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}
	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		RecruitingCenterSerializer rcs = new RecruitingCenterSerializer();
		GameMaster.DeserializeByteArray<RecruitingCenterSerializer>(ss.specificData, ref rcs);
		LoadWorkBuildingData(rcs.workBuildingSerializer);
		backupSpeed = rcs.backupSpeed;
		finding = rcs.finding;
		progress = rcs.progress;
	}

	RecruitingCenterSerializer GetRecruitingCenterSerializer() {
		RecruitingCenterSerializer rcs = new RecruitingCenterSerializer();
		rcs.workBuildingSerializer = GetWorkBuildingSerializer();
		rcs.backupSpeed = backupSpeed;
		rcs.finding = finding;
		rcs.progress = progress;
		return rcs;
	}
	#endregion

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
        Crew.RemoveCrewSlots(CREW_SLOTS_FOR_BUILDING);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
