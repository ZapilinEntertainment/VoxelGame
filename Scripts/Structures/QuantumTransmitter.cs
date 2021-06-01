using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}
    private static int nextTransmissionID = 0;

    private int transmissionID = NO_TRANSMISSION_VALUE;
    public const int NO_TRANSMISSION_VALUE = -1;

    static QuantumTransmitter()
    {
        transmittersList = new List<QuantumTransmitter>();
        GameMaster.staticResetFunctions += ResetStaticData;
    }
	public static void ResetStaticData() {
		transmittersList = new List<QuantumTransmitter>();
	}
    public static int GetFreeTransmittersCount()
    {
        if (transmittersList.Count == 0) return 0;
        else
        {
            int c = 0;
            foreach (var qt in transmittersList)
            {
                if (qt.transmissionID == NO_TRANSMISSION_VALUE) c++;
            }
            return c;
        }
    }
    public static QuantumTransmitter GetFreeTransmitter()
    {
        if (transmittersList.Count == 0) return null;
        else
        {
            foreach (var t in transmittersList)
            {
                if (t.transmissionID == NO_TRANSMISSION_VALUE) return t;
            }
            return null;
        }
    }
    public static void StopTransmission(int x)
    {
        if (x == NO_TRANSMISSION_VALUE) return;
        if (transmittersList.Count > 0)
        {
            foreach (var t in transmittersList)
            {
                if (t.transmissionID == x)
                {
                     t.transmissionID = NO_TRANSMISSION_VALUE;
                     t.SetActivationStatus(false, true);
                    return;
                }
            }
        }
    }


    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetStructureData(b, pos);
        if (!transmittersList.Contains(this))
        {
            transmissionID = NO_TRANSMISSION_VALUE;
            transmittersList.Add(this);
        }
        isActive = false;
        connectedToPowerGrid = false;
        SwitchActivityState();
    }

    override public void SetActivationStatus(bool x, bool sendRecalculationRequest)
    {
        if (isActive != x)
        {
            isActive = x;
            SwitchActivityState();
        }
    }
    protected override void SwitchActivityState()
    {
        ChangeRenderersView(isActive);
        transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("works", isActive);
    }

    public int StartTransmission()
    {       
        transmissionID = nextTransmissionID++;
        SetActivationStatus(true, true);
        return transmissionID;
    } 

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!order.sendMessageToBasement) basement = null;
        PrepareBuildingForDestruction(order);
        if (order.doSpecialChecks)
        {
            if (transmittersList.Contains(this)) transmittersList.Remove(this);
            if (transmissionID != NO_TRANSMISSION_VALUE) Expedition.ChangeTransmissionStatus(transmissionID, null);
        }
        Destroy(gameObject);
    }

    #region save-load
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(transmissionID));
        return data;
    }

    override public void Load(System.IO.Stream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        transmissionID = System.BitConverter.ToInt32(data,0);
        SetActivationStatus(transmissionID != NO_TRANSMISSION_VALUE, true);
        if (transmissionID >= nextTransmissionID) nextTransmissionID = transmissionID + 1;
    }
    #endregion
}
