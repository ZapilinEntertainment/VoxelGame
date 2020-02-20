using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}

    private int transmissionID = NO_TRANSMISSION_VALUE;
    public const int NO_TRANSMISSION_VALUE = -1;

    static QuantumTransmitter()
    {
        transmittersList = new List<QuantumTransmitter>();
        AddToResetList(typeof(QuantumTransmitter));
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
    private static int GenerateTransmissionID()
    {
        if (transmittersList.Count == 0) return 1;
        else
        {
            int x = 1;
            foreach (var t in transmittersList)
            {
                if (t.transmissionID != NO_TRANSMISSION_VALUE && t.transmissionID > x) x = t.transmissionID; 
            }
            return x + 1;
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
        SetBuildingData(b, pos);
        if (!transmittersList.Contains(this))
        {
            transmissionID = NO_TRANSMISSION_VALUE;
            transmittersList.Add(this);
        }        
        SetActivationStatus(false, true);
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter) {
		isActive = x;
		if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
		transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("works",x);
		ChangeRenderersView(x);
	}	

    public int StartTransmission()
    {
        transmissionID = GenerateTransmissionID();
        return transmissionID;
    } 

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) basement = null;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (transmittersList.Contains(this)) transmittersList.Remove(this);
        if (transmissionID != NO_TRANSMISSION_VALUE) Expedition.ChangeTransmissionStatus(transmissionID, null);
        Destroy(gameObject);
    }

    #region save-load
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(transmissionID));
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        transmissionID = System.BitConverter.ToInt32(data,0);
    }
    #endregion
}
