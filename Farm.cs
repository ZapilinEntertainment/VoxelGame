using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding {
	float lifepowerConsumption = 0.1f, farmFertility = 1;


	override protected void LabourResult() {
		int result = (int) (workflow * farmFertility * basement.fertility);
		if (result > 1) {
			int takenLifepower = GameMaster.mainChunk.TakeLifePower(result);
			if (takenLifepower == 0) return;
			if (takenLifepower < result) {
				
			}
		}
	}

}
