using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumTransmitter : Building {
	public static List<QuantumTransmitter> transmittersList{get;private set;}
	public Expedition tracingExpedition{get;private set;}

	public static void Reset() {
		transmittersList = new List<QuantumTransmitter>();
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		AddToList(this);
		if (tracingExpedition == null) SetActivationStatus(false);
	}

	override public void SetActivationStatus(bool x) {
		if ( x == true & tracingExpedition == null & isActive == false) return; // невозможно включить вхолостую
		isActive = x;
		GameMaster.colonyController.RecalculatePowerGrid();
		transform.GetChild(0).GetComponent<Animator>().SetBool("works",x);
		ChangeRenderersView(x);
	}
	override public void SetEnergySupply(bool x) {
		if (x == energySupplied) return;
		energySupplied = x;
		if (x) {
			if (tracingExpedition != null) SetActivationStatus(true);
			else ChangeRenderersView(true);
		}
		else {
			if (tracingExpedition != null) SetActivationStatus(false);
			else ChangeRenderersView(false);
		}
	}

	public void SetExpedition(Expedition e) {
		if (e == tracingExpedition) return;
		if (tracingExpedition != null) tracingExpedition.SetTransmitter(null);
		if (e == null) {
			tracingExpedition = null;
			SetActivationStatus(false);
		}
		else {
			tracingExpedition = e;
			if (energySupplied) SetActivationStatus(true);
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

	void OnDestroy() {
		PrepareBuildingForDestruction();
		if (transmittersList != null) RemoveFromList(this);
	}
}
