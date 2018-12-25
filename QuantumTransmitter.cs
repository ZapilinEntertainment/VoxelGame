using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}

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
		SetActivationStatus(false, true);
	}

	override public void SetActivationStatus(bool x, bool recalculateAfter) {
		if ( x == true & isActive == false) return; // невозможно включить вхолостую
		isActive = x;
		if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
		transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("works",x);
		ChangeRenderersView(x);
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
