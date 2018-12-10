using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}
	public Expedition tracingExpedition{get;private set;}

    static QuantumTransmitter()
    {
        transmittersList = new List<QuantumTransmitter>();
    }

	public static void ResetToDefaults_Static_QuantumTransmitter() {
		transmittersList = new List<QuantumTransmitter>();
	}
    public static void PrepareList()
    {
        if (transmittersList == null) transmittersList = new List<QuantumTransmitter>();
    }

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		AddToList(this);
		if (tracingExpedition == null) SetActivationStatus(false, true);
	}

	override public void SetActivationStatus(bool x, bool recalculateAfter) {
		if ( x == true & tracingExpedition == null & isActive == false) return; // невозможно включить вхолостую
		isActive = x;
		if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
		transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("works",x);
		ChangeRenderersView(x);
	}
	override public void SetEnergySupply(bool x, bool recalculateAfter) {
		if (x == isEnergySupplied) return;
		isEnergySupplied = x;
        if (x) {
            if (tracingExpedition != null) SetActivationStatus(true, recalculateAfter);
            else
            {
                if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
                ChangeRenderersView(true);
            }
		}
		else {
            if (tracingExpedition != null) SetActivationStatus(false, recalculateAfter);
            else
            {
                if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
                ChangeRenderersView(false);
            }
		}
	}

	public void SetExpedition(Expedition e) {
		if (e == tracingExpedition) return;
		if (tracingExpedition != null) tracingExpedition.SetTransmitter(null);
		if (e == null) {
			tracingExpedition = null;
			SetActivationStatus(false, true);
		}
		else {
			tracingExpedition = e;
			if (isEnergySupplied) SetActivationStatus(true, true);
		}
	}

	public static void AddToList(QuantumTransmitter qt) {
		if (qt == null) return;
		if (transmittersList.Count == 0) transmittersList.Add(qt);
		else {
			int i = 0;
			while (i < transmittersList.Count) {
				if (transmittersList[i] == null) {
					transmittersList.RemoveAt(i);
					continue;
				}
				else {
					if (transmittersList[i] == qt) return;
				}
				i++;
			}
			transmittersList.Add(qt);
		}
	}
	public static void RemoveFromList(QuantumTransmitter qt) {
		if (qt == null | transmittersList.Count == 0) return;
		int i = 0;
		while (i < transmittersList.Count) {
			if (transmittersList[i] == null | transmittersList[i] == qt) {
				transmittersList.RemoveAt(i);
				continue;
			}
			i++;
		}
	}

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PrepareBuildingForDestruction(forced);
        if (transmittersList != null && transmittersList.Count > 0) RemoveFromList(this);
        Destroy(gameObject);
    }
}
