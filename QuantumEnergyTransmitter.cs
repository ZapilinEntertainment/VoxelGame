using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class QuantumEnergyTransmitter : Building {
    public static QuantumEnergyTransmitter current { get; private set; }
    ColonyController colony;
    float charge = 0, chargeSpeed = 0.01f;

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetStructureData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        isActive = true;    
        if (current != null)
        {
            current.Annihilate(false);
        }
        current = this;
        colony = GameMaster.colonyController;
        colony.accumulateEnergy = false;
        connectedToPowerGrid = true;
    }

    public void LabourUpdate()
    {
        if (!isActive) return;
        charge += chargeSpeed * colony.energySurplus;
        if (charge > GameMaster.ENERGY_IN_CRYSTAL)
        {
            int count = (int)(charge / GameMaster.ENERGY_IN_CRYSTAL);
            colony.AddEnergyCrystals(count);
            charge -= count * GameMaster.ENERGY_IN_CRYSTAL;
        }
        else
        {
            if (charge < 0) charge = 0;
        }
        energySurplus = charge;
    }

    override public void SetActivationStatus(bool x)
    {
        isActive = x;
        colony.accumulateEnergy = !x;
        ChangeRenderersView(x);
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(forced);
        if (current == this)
        {
            colony.accumulateEnergy = true;
            current = null;
        }
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
    }
}
