using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : Plant {
	[SerializeField]
	Sprite[] stages;
	byte currentStage = 0;
	[SerializeField]
	float maxLifepower_set, maxVisibleDistance = 25;
	[SerializeField]
	bool visible = true;

	void Awake() {
		PrepareStructure();
		lifepower = 1;
		maxLifepower = maxLifepower_set;
		full = false;
		growth = 0;
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}

	void Update() {
		if (full || GameMaster.gameSpeed == 0) return;
		float theoreticalGrowth = lifepower / maxLifepower;
		growth = Mathf.MoveTowards(growth, theoreticalGrowth,  growSpeed * GameMaster.lifeGrowCoefficient * Time.deltaTime);
		byte newStage = (byte)(growth * (stages.Length-1));
		if (newStage != currentStage) {
			(myRenderer as SpriteRenderer).sprite = stages[newStage];
			currentStage = newStage;
		}
		if (growth >= 1) full = true;
	}	

	override public void SetLifepower(float p) {
		lifepower = p; 
		if (lifepower < maxLifepower) full = false; else full = true;
		growth = lifepower/ maxLifepower;
		byte newStage = (byte)(growth * (stages.Length-1));
		if (newStage != currentStage) {
			(myRenderer as SpriteRenderer).sprite = stages[newStage];
			currentStage = newStage;
		}
	}

	public void CameraUpdate(Transform t) {
		if (Vector3.Distance(myRenderer.transform.position, t.position) > maxVisibleDistance) { 
			if (visible) {myRenderer.enabled = false; visible = false;}
		}
		else {
			if (!visible) {myRenderer.enabled = true;visible =true;}
			Vector3 dir = t.position - myRenderer.transform.position;
			dir.y = 0;
			myRenderer.transform.forward = dir;
		}
	}
}
