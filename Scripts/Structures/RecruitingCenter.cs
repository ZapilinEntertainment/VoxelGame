using UnityEngine; // random
using System.Collections.Generic;

public sealed class RecruitingCenter : WorkBuilding {
    public static UIRecruitingCenterObserver rcenterObserver;
    public static List<RecruitingCenter> recruitingCentersList;
    private static float hireCost = -1;       

    private const float backupSpeed = 0.02f;
    public bool finding = false;
    const int CREW_SLOTS_FOR_BUILDING = 4, START_CREW_COST = 150;
    public const int REPLENISH_COST = 50;	

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
                UIController.GetCurrent()?.GetMainCanvasController()?.Select(rc);
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

	override public void SetBasement(Plane b, PixelPosByte pos) {
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
        if (!isActive || !isEnergySupplied || GameMaster.loading) return;
		if (workersCount > 0) {
			if (finding) INLINE_WorkCalculation();
        }
		else {
			if (workflow > 0) {
                workflow -= backupSpeed * GameMaster.LABOUR_TICK;
				if (workflow < 0f) workflow = 0f;
			}
		}
	}
    protected override void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        int memCount = (int)((workersCount / (float)maxWorkers) * Crew.MAX_MEMBER_COUNT);
        if (workersCount < memCount) memCount = workersCount;
        if (memCount > 0)
        {
            Crew c = Crew.CreateNewCrew(colony, memCount);
            workersCount -= memCount;
            workflow = 0;
            finding = false;
            AnnouncementCanvasController.MakeAnnouncement(Localization.GetCrewAction(LocalizedCrewAction.Ready, c));
            hireCost = hireCost * (1 + GameConstants.HIRE_COST_INCREASE);
            hireCost = ((int)(hireCost * 100)) / 100f;
            if (showOnGUI) rcenterObserver.SelectCrew(c);
        }
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
                    AnnouncementCanvasController.NotEnoughMoneyAnnounce();
                    return false;
                }
            }
            else
            {
                AnnouncementCanvasController.MakeImportantAnnounce(Localization.GetRefusalReason(RefusalReason.NotEnoughSlots));
                if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.NotEnoughSlots);
                return false;
            }
        }
    }
    public void StopHiring()
    {
        if (finding)
        {
            colony.AddEnergyCrystals(hireCost * (1f - workflow / workComplexityCoefficient));
            finding = false;
            workflow = 0f;
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
        data.Add(finding ? (byte)1 : (byte)0);
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        finding = fs.ReadByte() == 1;
    }   
    #endregion
}
