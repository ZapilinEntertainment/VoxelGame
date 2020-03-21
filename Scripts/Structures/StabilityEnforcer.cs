using UnityEngine;
public sealed class StabilityEnforcer : WorkBuilding
{
    private int affectionID = -1;
    private const float AFFECTION_VALUE = 0.05f;
    public const float STABILITY_BORDER = 0.5f;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);             
    }
    override protected void SwitchActivityState()
    {
        base.SwitchActivityState();
        if (isEnergySupplied & isActive)
        {
            float f = workersCount; f /= maxWorkers;
            if (f != 0f)
            {
                if (!subscribedToUpdate) Subscribe();
                if (affectionID == -1) affectionID = GameMaster.realMaster.environmentMaster.AddStabilityModifier(AFFECTION_VALUE * f);
            }
        }
        else
        {
            if (subscribedToUpdate) Unsubscribe();
            if (affectionID != -1)
            {
                GameMaster.realMaster.environmentMaster.RemoveStabilityModifier(affectionID);
                affectionID = -1;
            }
        }
    }
    private void Subscribe()
    {
        GameMaster.realMaster.everydayUpdate += LabourUpdate;
        subscribedToUpdate = true;
    }
    private void Unsubscribe()
    {
        GameMaster.realMaster.everydayUpdate -= LabourUpdate;
        subscribedToUpdate = false;
    }

    override public void RecalculateWorkspeed()
    {
        if (workersCount > 0)
        {
            float f = workersCount; f /= maxWorkers;
            if (affectionID == -1) affectionID = GameMaster.realMaster.environmentMaster.AddStabilityModifier(AFFECTION_VALUE * f);
            else
            {
                GameMaster.realMaster.environmentMaster.ChangeStabilityModifierValue(affectionID, AFFECTION_VALUE * f);
            }
            if (!subscribedToUpdate) Subscribe();
            if (!isActive) SetActivationStatus(true, true);
        }
        else
        {
            if (affectionID != -1)
            {
                GameMaster.realMaster.environmentMaster.RemoveStabilityModifier(affectionID);
                affectionID = -1;
            }
            if (subscribedToUpdate) Unsubscribe();
            if (isActive) SetActivationStatus(false, true);
        }
    }
    override public void LabourUpdate()
    {
        if (!isActive | !isEnergySupplied) return;

        var gm = GameMaster.realMaster;
        if ((byte)gm.difficulty > (byte)Difficulty.Easy)
        {
            var st = GameMaster.stability;
            if (st < STABILITY_BORDER)
            {
                float stabilityDelta = STABILITY_BORDER - st;
                if (Random.value > (1f - stabilityDelta / STABILITY_BORDER))
                {
                    int vc = (byte)gm.difficulty - (byte)Difficulty.Normal + 1;
                    if (colony.citizenCount >= vc)
                    {
                        colony.RemoveCitizens(vc);
                    }
                    else
                    {
                        workersCount -= vc;
                        if (workersCount <= 0)
                        {
                            workersCount = 0;
                            SetActivationStatus(false, true);
                        }
                    }
                }
            }
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToUpdate) Unsubscribe();
        if (affectionID != -1)
        {
            GameMaster.realMaster.environmentMaster.RemoveStabilityModifier(affectionID);
            affectionID = -1;
        }
        Destroy(gameObject);
    }

    //save system is same as ancestor's
}
