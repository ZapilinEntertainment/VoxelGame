using UnityEngine; // random
using System.Collections.Generic;

public sealed class RecruitingCenter : WorkBuilding {
    public static UIRecruitingCenterObserver rcenterObserver;
    public static List<RecruitingCenter> recruitingCentersList;
    private static float hireCost = -1;       

    float backupSpeed = 0.02f;
    public bool finding = false;
    const int CREW_SLOTS_FOR_BUILDING = 4, START_CREW_COST = 150;
    public const int REPLENISH_COST = 50;	
    const float FIND_SPEED = 5;
    public const float FIND_WORKFLOW = 10;

    static RecruitingCenter() {
        recruitingCentersList = new List<RecruitingCenter>();
        AddToResetList(typeof(RecruitingCenter));
    }
	public static void ResetStaticData() {
		hireCost = GetHireCost();
        recruitingCentersList = new List<RecruitingCenter>();
	}
    public static bool SelectAny()
    {
        int n = recruitingCentersList.Count;
        if (n > 0)
        {
            var rc = recruitingCentersList[Random.Range(0, n)];
            if (rc != null)
            {
                UIController.current.Select(rc);
                return true;
            }
        }
        return false;
    }

   public static int GetCrewsSlotsCount()
    {
        return (recruitingCentersList.Count * CREW_SLOTS_FOR_BUILDING);
    }
   public static float GetHireCost()
    {
        if (hireCost == -1) hireCost = START_CREW_COST + GameMaster.realMaster.GetDifficultyCoefficient() * 50;
        return hireCost;
    }
   public static void SetHireCost(float f)
    {
        hireCost = f;
    }

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (hireCost == -1) ResetStaticData();
		if (b == null) return;
		SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (!recruitingCentersList.Contains(this)) recruitingCentersList.Add(this);
	}

	override public void LabourUpdate() {
		if ( !isActive | !isEnergySupplied) return;
		if (workersCount > 0) {
			if (finding) {
				workflow += ( FIND_SPEED * workSpeed * 0.4f + colony.happiness_coefficient * 0.4f   + 0.2f * Random.value )* GameMaster.LABOUR_TICK / workflowToProcess;
				if (workflow >= workflowToProcess) {
                    int memCount = (int)((workersCount / (float)maxWorkers) * Crew.MAX_MEMBER_COUNT);
                    if (workersCount < memCount) memCount = workersCount;
                    if (memCount > 0)
                    {
                        Crew c = Crew.CreateNewCrew(colony, memCount);
                        workersCount -= memCount;
                        workflow = 0;
                        finding = false;
                        GameLogUI.MakeAnnouncement(Localization.AnnounceCrewReady(c.name));
                        hireCost = hireCost * (1 + GameConstants.HIRE_COST_INCREASE);
                        hireCost = ((int)(hireCost * 100)) / 100f;
                        if (showOnGUI) rcenterObserver.SelectCrew(c);
                    }
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

	override public void RecalculateWorkspeed() {
		workSpeed = (float)workersCount / (float)maxWorkers;
        gearsDamage = 0;
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
            if (Crew.crewsList.Count < GetCrewsSlotsCount())
            {
                if (colony.energyCrystalsCount >= hireCost)
                {
                    colony.GetEnergyCrystals(hireCost);
                    finding = true;
                    return true;
                }
                else
                {
                    GameLogUI.NotEnoughMoneyAnnounce();
                    return false;
                }
            }
            else
            {
                GameLogUI.MakeImportantAnnounce(Localization.GetRefusalReason(RefusalReason.NotEnoughSlots));
                if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.NotEnoughSlots);
                return false;
            }
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (recruitingCentersList.Contains(this)) recruitingCentersList.Remove(this);
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        if (recruitingCentersList.Count == 0 & rcenterObserver != null) Destroy(rcenterObserver);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(backupSpeed));
        data.Add(finding ? (byte)1 : (byte)0);
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        base.Load(fs, sblock);
        var data = new byte[5];
        fs.Read(data, 0, data.Length);
        backupSpeed = System.BitConverter.ToSingle(data, 0);
        finding = data[4] == 1;
    }   
    #endregion
}
