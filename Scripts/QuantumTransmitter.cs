using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}
    public static int lastUsedID { get; private set; }

    public int transmitterID { get; private set; }
    public int expeditionID { get; private set; }
    // STATIC METHODS

    static QuantumTransmitter()
    {
        transmittersList = new List<QuantumTransmitter>();
        AddToResetList(typeof(QuantumTransmitter));
    }
	public static void ResetStaticData() {
		transmittersList = new List<QuantumTransmitter>();
	}
    public static QuantumTransmitter GetTransmitterByConnectionID(int s_id)
    {
        if (s_id > 0 && transmittersList.Count > 0)
        {
            foreach (QuantumTransmitter qt in transmittersList)
            {
                if (qt != null && qt.transmitterID == s_id)
                {
                    return qt;
                }
            }
        }
        return null;
    }
    public static QuantumTransmitter GetFreeTransmitter()
    {
        if (transmittersList.Count > 0)
        {
            foreach (QuantumTransmitter qt in transmittersList)
            {
                if (qt != null && qt.expeditionID == -1) return qt;
            }
        }
        return null;
    }
    //  PUBLIC
    public static void SetLastUsedID(int id)
    {
        lastUsedID = id;
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (!transmittersList.Contains(this))
        {
            transmitterID = lastUsedID++;
            transmittersList.Add(this);
        }
        expeditionID = -1;
        SetActivationStatus(false, true);
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter) {
		isActive = x;
		if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
		transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("works",x);
		ChangeRenderersView(x);
	}	

    public void AssignExpedition(Expedition e)
    {
        if (e == null) return;
        expeditionID = e.ID;
        SetActivationStatus(true, true);
    }
    public void DropExpeditionConnection()
    {
        if (expeditionID >= 0)
        {
            expeditionID = -1;
            SetActivationStatus(false, true);
        } 
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { UnsetBasement(); }
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (transmittersList.Contains(this)) transmittersList.Remove(this);
        if (expeditionID != -1)
        {
            var e = Expedition.GetExpeditionByID(expeditionID);
            if (e != null && e.stage != Expedition.ExpeditionStage.Dismissed) e.DropTransmitter(this);
            expeditionID = -1;
        }
        Destroy(gameObject);
    }

    #region save-load
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(transmitterID));
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        base.Load(fs, sblock);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        transmitterID = System.BitConverter.ToInt32(data,0);
    }
    #endregion
}
