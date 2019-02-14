using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}

    // STATIC METHODS

    static QuantumTransmitter()
    {
        transmittersList = new List<QuantumTransmitter>();
    }

	public static void ResetToDefaults_Static_QuantumTransmitter() {
		transmittersList = new List<QuantumTransmitter>();
	}
    //  PUBLIC

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (!transmittersList.Contains(this)) transmittersList.Add(this);
        SetActivationStatus(false, true);
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter) {
		if ( x == true & isActive == false) return; // невозможно включить вхолостую
		isActive = x;
		if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
		transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("works",x);
		ChangeRenderersView(x);
	}	

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PrepareBuildingForDestruction(forced);
        if (transmittersList.Contains(this)) transmittersList.Remove(this);
        Destroy(gameObject);
    }
}
