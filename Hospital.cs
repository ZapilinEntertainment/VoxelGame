using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BirthrateMode {Normal, Improved, Lowered}
public class Hospital : House {
	[SerializeField]
	int _coverage = 100;
	public int coverage {get;private set;}
	public static float hospital_birthrate_coefficient = 1;
	public static  BirthrateMode birthrateMode{get; private set;}
	public const float loweredCoefficient = 0.5f, improvedCoefficient = 1.5f;

	public static void Reset() {
		SetBirthrateMode(0);
	}

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		coverage = _coverage;
		GameMaster.colonyController.AddHospital(this);
	}

	override public void SetActivationStatus(bool x) {
		if (isActive == x) return;
		isActive = x;
		GameMaster.colonyController.RecalculateHousing();
		GameMaster.colonyController.RecalculateHospitals();
	}

	public static void SetBirthrateMode(int x) {
		switch (x) {
		case 0: birthrateMode = BirthrateMode.Normal;break;
		case 1: birthrateMode = BirthrateMode.Improved;break;
		case 2: birthrateMode = BirthrateMode.Lowered;break;
		}
	}
	public static int GetBirthrateModeIndex() {
		switch (birthrateMode) {
		case BirthrateMode.Normal: return 0;
		case BirthrateMode.Improved: return 1;
		case BirthrateMode.Lowered: return 2;
		default: return 0;
		}
	}

	void OnGUI() {
		//from building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		byte p = (byte)birthrateMode;
		GUI.DrawTexture(new Rect(rr.x, rr.y + p * rr.height, rr.width, rr.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
		if (GUI.Button(rr, Localization.lowered_birthrate + " (" + string.Format("{0:0.##}", loweredCoefficient)  + "%)")) {
			birthrateMode = BirthrateMode.Lowered; 
			hospital_birthrate_coefficient = loweredCoefficient;
		}
		rr.y += rr.height;
		if (GUI.Button(rr, Localization.normal_birthrate + " (100%)")) {
			birthrateMode = BirthrateMode.Normal; 
			hospital_birthrate_coefficient = 1;
		}
			rr.y += rr.height;
		if (GUI.Button(rr, Localization.improved_birthrate + " (" + string.Format("{0:0.##}", improvedCoefficient)  + "%)")) {
			birthrateMode = BirthrateMode.Improved; 
			hospital_birthrate_coefficient = improvedCoefficient;
		}
	}
}
