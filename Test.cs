using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
	Chunk c;
	public int newEnergyTakeSpeed = 10;
	int energyTakeSpeed = 10, energyCount = 0;
	// Use this for initialization
	void Start () {
		c = gameObject.GetComponent<Chunk>();
	}

	void Update() {
		energyCount += c.TakeLifePower((int)(60 * Time.deltaTime * GameMaster.gameSpeed));
		if (newEnergyTakeSpeed != energyTakeSpeed) {
			energyTakeSpeed = newEnergyTakeSpeed;
			Chunk.energy_take_speed = energyTakeSpeed;
		}
	}
	
	void OnGUI() {
		GUILayout.Label(energyCount.ToString());
	}
}
