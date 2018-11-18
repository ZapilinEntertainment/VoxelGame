using UnityEngine; // random

[System.Serializable]
public class RecruitingCenterSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public float backupSpeed;
	public bool finding;
}

public sealed class RecruitingCenter : WorkBuilding {
    float backupSpeed = 0.02f;
    public bool finding = false;
	ColonyController colonyController;
	const int CREW_SLOTS_FOR_BUILDING = 4, START_CREW_COST = 150;
	private static float hireCost = -1;
    public static UIRecruitingCenterObserver rcenterObserver;
    const float FIND_SPEED = 5;
    public const float FIND_WORKFLOW = 10;

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
        if (!movement) // здание не переносилось, а было построено
        {
            Crew.AddCrewSlots(CREW_SLOTS_FOR_BUILDING);
            workflow = 0;
        }
	}

	override public void LabourUpdate() {
		if ( !isActive | !energySupplied) return;
		if (workersCount > 0) {
			if (finding) {
				float candidatsCountFactor = colonyController.freeWorkers / Crew.OPTIMAL_CANDIDATS_COUNT;
				if (candidatsCountFactor > 1) candidatsCountFactor = 1;
				workflow += ( FIND_SPEED * workSpeed * 0.3f + colonyController.happiness_coefficient * 0.3f  + candidatsCountFactor * 0.3f + 0.1f * Random.value )* GameMaster.LABOUR_TICK / workflowToProcess;
				if (workflow >= workflowToProcess) {
					Crew ncrew = new Crew();
					ncrew.SetCrew(colonyController, hireCost);
					Crew.freeCrewsList.Add(ncrew);
					workflow = 0;
					finding = false;
                    UIController.current.MakeAnnouncement(Localization.AnnounceCrewReady(ncrew.name));
					hireCost = hireCost * (1 + GameConstants.HIRE_COST_INCREASE);
					hireCost = ((int)(hireCost * 100)) / 100f;
                    if (showOnGUI) rcenterObserver.SelectCrew(ncrew);
				}
			}
		}
		else {
			if (workflow > 0) {
                workflow -= backupSpeed * GameMaster.LABOUR_TICK;
				if (workflow < 0) workflow = 0;
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

    public bool StartHiring()
    {
        if (finding) return true;
        else
        {
            if (Crew.crewSlotsFree > 0)
            {
                if (GameMaster.colonyController.energyCrystalsCount >= hireCost)
                {
                    GameMaster.colonyController.GetEnergyCrystals(hireCost);
                    finding = true;
                    return true;
                }
                else
                {
                    UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughEnergyCrystals));
                    return false;
                }
            }
            else
            {
                UIController.current.MakeAnnouncement(Localization.GetRefusalReason(RefusalReason.NotEnoughSlots));
                return false;
            }
        }
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
	}

	RecruitingCenterSerializer GetRecruitingCenterSerializer() {
		RecruitingCenterSerializer rcs = new RecruitingCenterSerializer();
		rcs.workBuildingSerializer = GetWorkBuildingSerializer();
		rcs.backupSpeed = backupSpeed;
		rcs.finding = finding;
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
