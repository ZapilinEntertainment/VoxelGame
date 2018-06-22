using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GeneratorFuel {Biofuel, MineralFuel,Graphonium}
public class Powerplant : WorkBuilding {
	[SerializeField]
	GeneratorFuel fuelType;
	ResourceType fuel;
	[SerializeField]
	 float output = 100, fuelCount = 1, fuelBurnTime = 10, fuelLoadTryingTime = 2;
	float ftimer = 0;

	override public void Prepare() {
		PrepareWorkbuilding();
		switch (fuelType) {
		case GeneratorFuel.Biofuel: fuel = ResourceType.Food;break;
		case GeneratorFuel.MineralFuel: fuel = ResourceType.mineral_F;break;
		case GeneratorFuel.Graphonium : fuel = ResourceType.Graphonium;break;
		}
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 ) return;
		if (ftimer > 0) ftimer -= Time.deltaTime * GameMaster.gameSpeed;
		if (ftimer <= 0) {
			float takenFuel =  0;
			if (workersCount > 0 && isActive) takenFuel = GameMaster.colonyController.storage.GetResources(fuel, fuelCount);
			float newEnergySurplus = 0;
			if (takenFuel == 0) {
				newEnergySurplus = 0;
				ftimer = fuelLoadTryingTime;
			}
			else {
				if (workersCount > 0) {
					if (workersCount >  maxWorkers / 2) {
						if (workersCount > maxWorkers * 5f/6f) {
							newEnergySurplus = output;
						}
						else newEnergySurplus = output/2f;
					}
					else {
						if (workersCount > maxWorkers / 6) newEnergySurplus = output * 0.25f;
						else newEnergySurplus = output * 0.1f;
					}
				}
				else newEnergySurplus = 0;
				ftimer = fuelBurnTime * takenFuel / fuelCount * 2 * ((float)workersCount / (float)maxWorkers);
			}
			if (newEnergySurplus != energySurplus) {
				energySurplus = newEnergySurplus;
				GameMaster.colonyController.RecalculatePowerGrid();
			}
		}
	}

	override public int AddWorkers (int x) {
		if (workersCount == maxWorkers) return 0;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
			}
			return x;
		}
	}

	override public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, ftimer);
			ss.specificData = stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss,sblock);
		GameMaster.DeserializeByteArray(ss.specificData, ref ftimer);
	}
	#endregion

	void OnGUI() {
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), fuel.icon, ScaleMode.StretchToFill);
		if (energySurplus > 0) {
			GUI.DrawTexture(new Rect(rr.x + rr.height, rr.y, (Screen.width - rr.x - rr.height) * ftimer /fuelBurnTime, rr.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
			GUI.Label(new Rect(rr.x + rr.height, rr.y, Screen.width - rr.x - rr.height, rr.height), string.Format("{0:0.##}", ftimer / fuelBurnTime * 100) + '%' );
			rr.y += rr.height;
		}
	}
}
