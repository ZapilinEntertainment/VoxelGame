using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonyController : MonoBehaviour {
	Human[] citizen;
	float foodCount = 0;
	const float FOOD_CONSUMPTION = 1;
	Storage storage;

	void Awake() {
		GameMaster.realMaster.everydayUpdateList.Add(this);
	}

	void EverydayUpdate() {
		if (citizen.Length > 0) foodCount -= FOOD_CONSUMPTION * citizen.Length;
	}


	void OnDestroy() {
		GameMaster.realMaster.everydayUpdateList.Remove(this);
	}
}
