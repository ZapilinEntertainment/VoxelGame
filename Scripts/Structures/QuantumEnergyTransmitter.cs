public sealed class QuantumEnergyTransmitter : Building {
    public static QuantumEnergyTransmitter current { get; private set; }
    ColonyController colony;
    float charge = 0, chargeSpeed = 0.01f;

    public override bool CanBeRotated()
    {
        return false;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetStructureData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }   
        if (current != null)
        {
            current.Annihilate(StructureAnnihilationOrder.ManualDestructed);
        }
        current = this;
        colony = GameMaster.realMaster.colonyController;
        connectedToPowerGrid = true; isEnergySupplied = true;
        SetActivationStatus(false, true);
    }

    public void LabourUpdate()
    {
        if (!isActive) return;
        charge += chargeSpeed * colony.energySurplus;
        if (charge > GameConstants.ENERGY_IN_CRYSTAL)
        {
            int count = (int)(charge / GameConstants.ENERGY_IN_CRYSTAL);
            colony.AddEnergyCrystals(count);
            charge -= count * GameConstants.ENERGY_IN_CRYSTAL;
        }
        else
        {
            if (charge < 0) charge = 0;
        }
        energySurplus = charge;
    }

    protected override void SwitchActivityState()
    {
        colony.accumulateEnergy = !isActive;
        ChangeRenderersView(isActive);
        isEnergySupplied = true; connectedToPowerGrid = true;
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(order);
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
        Destroy(gameObject);
    }

    override public void Load(System.IO.Stream fs, Plane sblock)
    {
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
        charge = energySurplus; 
    }
}
